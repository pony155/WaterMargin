using System.Collections.Immutable;
using Spelljammer.Content.Diagnostics;
using Spelljammer.Content.Manifests;
using Spelljammer.Content.Parsing;
using Spelljammer.Content.Sources;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Content.Compilation;

public sealed class GameContentCompiler
{
    private static readonly HashSet<string> RuntimePrimitives = new(StringComparer.Ordinal)
    {
        "progression.skill.standard",
        "action.spell.cast",
        "action.spell.identify",
        "effect.recovery.soul-anchor",
        "effect.tracking.observed-trail",
        "resource.focus",
        "resource.psychic-strain",
        "resource.training-supplies",
        "facility.arcane-study",
        "facility.quiet-sanctum",
        "safety.supervised",
        "range.far",
        "range.near",
        "contact.invited",
        "psychic.discipline.contact",
        "psychic.scope.deliberate-message",
        "effect.spirit.magic-missile-impact",
        "effect.psychic.shared-channel",
    };

    private readonly ContentLimits limits;

    public GameContentCompiler(ContentLimits? limits = null)
    {
        this.limits = limits ?? ContentLimits.Version1;
        this.limits.Validate();
    }

    public ContentCompilationResult Compile(
        IReadOnlyList<IContentPackSource> packSources,
        SemanticVersion gameVersion)
    {
        ArgumentNullException.ThrowIfNull(packSources);
        DiagnosticSink diagnostics = new(limits);
        if (packSources.Count > limits.EnabledPacks)
        {
            diagnostics.Limit("enabled-packs");
            return Failed(diagnostics);
        }

        List<CandidatePack> discovered = [];
        foreach (IContentPackSource source in packSources)
        {
            ImmutableArray<string> entries;
            try
            {
                IReadOnlyList<string> sourceEntries = source.EnumerateFiles();
                entries = [.. sourceEntries.Order(StringComparer.Ordinal)];
            }
            catch (ContentSourceException exception)
            {
                return IoFailed(diagnostics, exception);
            }

            if (entries.Any(entry => !SourceValidation.IsRelativePath(entry)))
            {
                diagnostics.Add(ContentDiagnosticCodes.PathInvalid);
                continue;
            }

            int manifestCount = entries.Count(entry => entry == "manifest.json");
            if (manifestCount == 0)
            {
                diagnostics.Add(ContentDiagnosticCodes.ManifestMissing, relativePath: "manifest.json");
                continue;
            }

            if (manifestCount != 1)
            {
                diagnostics.Add(ContentDiagnosticCodes.ManifestMultiple, relativePath: "manifest.json");
                continue;
            }

            byte[] manifestBytes;
            try
            {
                manifestBytes = source.ReadFile("manifest.json", limits.ManifestBytes);
            }
            catch (ContentSourceLimitException)
            {
                diagnostics.Limit("manifest-bytes", relativePath: "manifest.json");
                continue;
            }
            catch (ContentSourceException exception)
            {
                return IoFailed(diagnostics, exception);
            }

            if (manifestBytes.Length > limits.ManifestBytes)
            {
                diagnostics.Limit("manifest-bytes", relativePath: "manifest.json");
                continue;
            }

            PackManifest? manifest = ManifestParser.Parse(manifestBytes, limits, diagnostics);
            if (manifest is not null)
            {
                discovered.Add(new CandidatePack(source, manifest, entries));
            }
        }

        if (diagnostics.HasErrors)
        {
            return Failed(diagnostics);
        }

        ImmutableArray<CandidatePack> orderedPacks = OrderPacks(discovered, gameVersion, diagnostics);
        if (diagnostics.HasErrors)
        {
            return Failed(diagnostics);
        }

        List<SourceDefinition> definitions = ParseDefinitions(orderedPacks, diagnostics, out ContentIoFailure? ioFailure);
        if (ioFailure is not null)
        {
            return new ContentCompilationResult(null, diagnostics.ToImmutable(), ioFailure);
        }

        if (diagnostics.HasErrors || !Claim(definitions, diagnostics) || !Link(definitions, diagnostics) || !Validate(definitions, diagnostics))
        {
            return Failed(diagnostics);
        }

        if (!ValidateDefaultLocalization(orderedPacks, definitions, diagnostics, out ioFailure))
        {
            return ioFailure is null
                ? Failed(diagnostics)
                : new ContentCompilationResult(null, diagnostics.ToImmutable(), ioFailure);
        }

        return CompileSnapshot(orderedPacks, definitions, diagnostics);
    }

    private bool ValidateDefaultLocalization(
        ImmutableArray<CandidatePack> packs,
        IReadOnlyList<SourceDefinition> definitions,
        DiagnosticSink diagnostics,
        out ContentIoFailure? ioFailure)
    {
        ioFailure = null;
        foreach (CandidatePack pack in packs)
        {
            string packId = pack.Manifest.Id.ToString();
            HashSet<string> keys = new(StringComparer.Ordinal);
            SortedSet<string> localeFiles = new(StringComparer.Ordinal);
            foreach (string localizationRoot in pack.Manifest.LocalizationRoots)
            {
                string prefix = localizationRoot + "/" + DefaultLocaleCatalogParser.DefaultLocale + "/";
                foreach (string entry in pack.Entries)
                {
                    if (entry.StartsWith(prefix, StringComparison.Ordinal) &&
                        entry.EndsWith(".sfloc.json", StringComparison.Ordinal))
                    {
                        localeFiles.Add(entry);
                    }
                }
            }

            foreach (string path in localeFiles)
            {
                byte[] bytes;
                try
                {
                    bytes = pack.Source.ReadFile(path, limits.DefinitionFileBytes);
                }
                catch (ContentSourceLimitException)
                {
                    diagnostics.Limit("localization-file-bytes", packId, path);
                    continue;
                }
                catch (ContentSourceException exception)
                {
                    ioFailure = CreateIoFailure(exception);
                    return false;
                }

                if (!DefaultLocaleCatalogParser.TryReadKeys(bytes, packId, path, limits, diagnostics, out IReadOnlyList<string> sourceKeys))
                {
                    continue;
                }

                foreach (string key in sourceKeys)
                {
                    if (!keys.Add(key))
                    {
                        diagnostics.Add(ContentDiagnosticCodes.CollectionDuplicate, packId, path, propertyPath: "/messages/" + key);
                    }
                }
            }

            if (!keys.Contains(pack.Manifest.DisplayNameKey))
            {
                diagnostics.Add(
                    ContentDiagnosticCodes.LocalizationKeyMissing,
                    packId,
                    "manifest.json",
                    propertyPath: "/displayNameKey",
                    arguments: ContentDiagnosticArgument.SafeId(pack.Manifest.DisplayNameKey));
            }

            foreach (SourceDefinition definition in definitions.Where(value => value.PackId == packId).OrderBy(value => value.Id))
            {
                CheckLocalizationKey(definition, definition.NameKey, "/nameKey", keys, diagnostics);
                CheckLocalizationKey(definition, definition.DescriptionKey, "/descriptionKey", keys, diagnostics);
            }
        }

        return !diagnostics.HasErrors;
    }

    private static void CheckLocalizationKey(
        SourceDefinition definition,
        string key,
        string propertyPath,
        IReadOnlySet<string> availableKeys,
        DiagnosticSink diagnostics)
    {
        if (!availableKeys.Contains(key))
        {
            diagnostics.Add(
                ContentDiagnosticCodes.LocalizationKeyMissing,
                definition.PackId,
                definition.RelativePath,
                definition.Id.ToString(),
                propertyPath,
                ContentDiagnosticArgument.SafeId(key));
        }
    }

    private ImmutableArray<CandidatePack> OrderPacks(
        IReadOnlyList<CandidatePack> packs,
        SemanticVersion gameVersion,
        DiagnosticSink diagnostics)
    {
        Dictionary<ContentId, CandidatePack> byId = [];
        if (packs.Count > limits.GraphNodes)
        {
            diagnostics.Limit("graph-nodes");
            return [];
        }

        foreach (CandidatePack pack in packs)
        {
            if (!byId.TryAdd(pack.Manifest.Id, pack))
            {
                diagnostics.Add(ContentDiagnosticCodes.PackIdDuplicate, pack.Manifest.Id.ToString(), "manifest.json");
            }
            else if (!pack.Manifest.GameVersionRange.Contains(gameVersion))
            {
                diagnostics.Add(
                    ContentDiagnosticCodes.GameVersionIncompatible,
                    pack.Manifest.Id.ToString(),
                    "manifest.json",
                    arguments: ContentDiagnosticArgument.Version(gameVersion.ToString()));
            }
        }

        if (diagnostics.HasErrors)
        {
            return [];
        }

        Dictionary<ContentId, HashSet<ContentId>> successors = byId.Keys.ToDictionary(id => id, _ => new HashSet<ContentId>());
        Dictionary<ContentId, int> indegrees = byId.Keys.ToDictionary(id => id, _ => 0);
        int edgeCount = 0;
        foreach (CandidatePack pack in packs)
        {
            foreach (PackDependency dependency in pack.Manifest.Dependencies)
            {
                if (!byId.TryGetValue(dependency.Id, out CandidatePack? predecessor))
                {
                    diagnostics.Add(ContentDiagnosticCodes.DependencyMissing, pack.Manifest.Id.ToString(), "manifest.json",
                        arguments: ContentDiagnosticArgument.SafeId(dependency.Id.ToString()));
                    continue;
                }

                if (!dependency.VersionRange.Contains(predecessor.Manifest.Version))
                {
                    diagnostics.Add(ContentDiagnosticCodes.DependencyVersionMismatch, pack.Manifest.Id.ToString(), "manifest.json",
                        arguments: ContentDiagnosticArgument.SafeId(dependency.Id.ToString()));
                    continue;
                }

                AddEdge(dependency.Id, pack.Manifest.Id, successors, indegrees, ref edgeCount);
            }

            foreach (ContentId predecessor in pack.Manifest.LoadAfter)
            {
                if (byId.ContainsKey(predecessor))
                {
                    AddEdge(predecessor, pack.Manifest.Id, successors, indegrees, ref edgeCount);
                }
            }
        }

        if (edgeCount > limits.PackEdges)
        {
            diagnostics.Limit("dependency-and-load-after-edges");
        }

        if (diagnostics.HasErrors)
        {
            return [];
        }

        SortedSet<ContentId> ready = new(new PackOrderComparer());
        foreach ((ContentId id, int degree) in indegrees)
        {
            if (degree == 0)
            {
                ready.Add(id);
            }
        }

        List<CandidatePack> result = [];
        Dictionary<ContentId, int> depth = byId.Keys.ToDictionary(id => id, _ => 1);
        while (ready.Count != 0)
        {
            ContentId id = ready.Min;
            ready.Remove(id);
            result.Add(byId[id]);
            foreach (ContentId successor in successors[id].Order())
            {
                depth[successor] = Math.Max(depth[successor], depth[id] + 1);
                if (--indegrees[successor] == 0)
                {
                    ready.Add(successor);
                }
            }
        }

        if (result.Count != packs.Count)
        {
            diagnostics.Add(ContentDiagnosticCodes.DependencyCycle);
            return [];
        }

        if (depth.Values.Any(value => value > limits.DependencyDepth))
        {
            diagnostics.Limit("dependency-depth");
            return [];
        }

        return [.. result];
    }

    private static void AddEdge(
        ContentId predecessor,
        ContentId successor,
        Dictionary<ContentId, HashSet<ContentId>> successors,
        Dictionary<ContentId, int> indegrees,
        ref int edgeCount)
    {
        if (successors[predecessor].Add(successor))
        {
            indegrees[successor]++;
            edgeCount++;
        }
    }

    private List<SourceDefinition> ParseDefinitions(
        ImmutableArray<CandidatePack> packs,
        DiagnosticSink diagnostics,
        out ContentIoFailure? ioFailure)
    {
        ioFailure = null;
        List<SourceDefinition> definitions = [];
        int totalFiles = 0;
        long totalBytes = 0;
        foreach (CandidatePack pack in packs)
        {
            List<(string Path, DefinitionKind Kind)> files = [];
            foreach (string definitionRoot in pack.Manifest.DefinitionRoots)
            {
                string prefix = definitionRoot + "/";
                foreach (string entry in pack.Entries)
                {
                    if (!entry.StartsWith(prefix, StringComparison.Ordinal) || !entry.EndsWith(".json", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string underRoot = entry[prefix.Length..];
                    if (!DefinitionParser.TryGetKind(underRoot, out DefinitionKind kind))
                    {
                        diagnostics.Add(ContentDiagnosticCodes.KindMismatch, pack.Manifest.Id.ToString(), entry);
                        continue;
                    }

                    files.Add((entry, kind));
                }
            }

            files.Sort((left, right) => StringComparer.Ordinal.Compare(left.Path, right.Path));

            if (files.Count > limits.DefinitionFilesPerPack)
            {
                diagnostics.Limit("definition-files-per-pack", pack.Manifest.Id.ToString());
                continue;
            }

            totalFiles = checked(totalFiles + files.Count);
            if (totalFiles > limits.DefinitionFilesPerSet)
            {
                diagnostics.Limit("definition-files-per-content-set");
                return definitions;
            }

            long packBytes = 0;
            foreach ((string path, DefinitionKind kind) in files)
            {
                byte[] bytes;
                try
                {
                    bytes = pack.Source.ReadFile(path, limits.DefinitionFileBytes);
                }
                catch (ContentSourceLimitException)
                {
                    diagnostics.Limit("definition-file-bytes", pack.Manifest.Id.ToString(), path);
                    continue;
                }
                catch (ContentSourceException exception)
                {
                    ioFailure = CreateIoFailure(exception);
                    return definitions;
                }

                packBytes = checked(packBytes + bytes.Length);
                totalBytes = checked(totalBytes + bytes.Length);
                if (packBytes > limits.DefinitionBytesPerPack)
                {
                    diagnostics.Limit("definition-bytes-per-pack", pack.Manifest.Id.ToString());
                    break;
                }

                if (totalBytes > limits.DefinitionBytesPerSet)
                {
                    diagnostics.Limit("definition-bytes-per-content-set");
                    return definitions;
                }

                SourceDefinition? definition = DefinitionParser.Parse(bytes, kind, pack.Manifest.Id.ToString(), path, limits, diagnostics);
                if (definition is not null)
                {
                    definitions.Add(definition);
                }
            }

        }

        return definitions;
    }

    private bool Claim(IReadOnlyList<SourceDefinition> definitions, DiagnosticSink diagnostics)
    {
        Dictionary<ContentId, SourceDefinition> byId = [];
        Dictionary<DefinitionKind, int> kindCounts = [];
        foreach (SourceDefinition definition in definitions.OrderBy(value => value.Id))
        {
            string expectedPrefix = Prefix(definition.Kind);
            if (!definition.Id.ToString().StartsWith(expectedPrefix, StringComparison.Ordinal))
            {
                diagnostics.Add(ContentDiagnosticCodes.KindMismatch, definition.PackId, definition.RelativePath, definition.Id.ToString());
                continue;
            }

            if (!Owns(definition.PackId, definition.Id.ToString()))
            {
                diagnostics.Add(ContentDiagnosticCodes.NamespaceViolation, definition.PackId, definition.RelativePath, definition.Id.ToString());
                continue;
            }

            if (!byId.TryAdd(definition.Id, definition))
            {
                diagnostics.Add(ContentDiagnosticCodes.DefinitionIdDuplicate, definition.PackId, definition.RelativePath, definition.Id.ToString());
            }

            kindCounts.TryGetValue(definition.Kind, out int count);
            if (++count > limits.DefinitionsPerKind)
            {
                diagnostics.Limit("definitions-per-kind", definition.PackId, definition.RelativePath);
            }

            kindCounts[definition.Kind] = count;
        }

        if (definitions.Count > limits.DefinitionsPerSet)
        {
            diagnostics.Limit("definitions-per-content-set");
        }

        if (definitions.Count > limits.GraphNodes)
        {
            diagnostics.Limit("graph-nodes");
        }

        return !diagnostics.HasErrors;
    }

    private bool Link(IReadOnlyList<SourceDefinition> definitions, DiagnosticSink diagnostics)
    {
        Dictionary<string, SourceDefinition> byId = definitions.ToDictionary(value => value.Id.ToString(), StringComparer.Ordinal);
        foreach (SourceDefinition definition in definitions.OrderBy(value => value.Id))
        {
            switch (definition.Kind)
            {
                case DefinitionKind.Skill:
                    CheckPrimitive(definition, definition.Strings["progressionCurveId"], "/progressionCurveId", diagnostics);
                    CheckPrimitives(definition, definition.Arrays["actionTags"], "/actionTags", diagnostics);
                    break;
                case DefinitionKind.Feat:
                    CheckReference(definition, definition.Strings["trainingProjectId"], DefinitionKind.TrainingProject, byId, "/trainingProjectId", diagnostics);
                    CheckReferences(definition, definition.Arrays["grantedAccessIds"], DefinitionKind.Access, byId, "/grantedAccessIds", diagnostics);
                    break;
                case DefinitionKind.Background:
                    CheckReferences(definition, definition.Arrays["compatibleRaceIds"], DefinitionKind.Race, byId, "/compatibleRaceIds", diagnostics);
                    CheckReferences(definition, definition.Arrays["attributeBonusIds"], DefinitionKind.Attribute, byId, "/attributeBonusIds", diagnostics);
                    CheckReferences(definition, definition.Arrays["focusSkillIds"], DefinitionKind.Skill, byId, "/focusSkillIds", diagnostics);
                    break;
                case DefinitionKind.Character:
                    CheckReference(definition, definition.Strings["raceId"], DefinitionKind.Race, byId, "/raceId", diagnostics);
                    CheckReference(definition, definition.Strings["heritageId"], DefinitionKind.Heritage, byId, "/heritageId", diagnostics);
                    CheckReference(definition, definition.Strings["backgroundId"], DefinitionKind.Background, byId, "/backgroundId", diagnostics);
                    CheckReferences(definition, definition.Arrays["focusSkillIds"], DefinitionKind.Skill, byId, "/focusSkillIds", diagnostics);
                    break;
                case DefinitionKind.Heritage:
                    CheckReference(definition, definition.Strings["raceId"], DefinitionKind.Race, byId, "/raceId", diagnostics);
                    CheckReferences(definition, definition.Arrays["grantedPerkIds"], DefinitionKind.Perk, byId, "/grantedPerkIds", diagnostics);
                    break;
                case DefinitionKind.Perk:
                    CheckReferences(definition, definition.Arrays["compatibleRaceIds"], DefinitionKind.Race, byId, "/compatibleRaceIds", diagnostics);
                    CheckReferences(definition, definition.Arrays["grantedAccessIds"], DefinitionKind.Access, byId, "/grantedAccessIds", diagnostics);
                    foreach (string technique in definition.Arrays["grantedTechniqueIds"])
                    {
                        CheckTechniqueReference(definition, technique, byId, "/grantedTechniqueIds", diagnostics);
                    }

                    CheckReferences(definition, definition.Arrays["grantedPerkIds"], DefinitionKind.Perk, byId, "/grantedPerkIds", diagnostics);
                    CheckPrimitives(definition, definition.Arrays["effectIds"], "/effectIds", diagnostics);

                    break;
                case DefinitionKind.Race:
                    CheckReferences(definition, definition.Arrays["grantedPerkIds"], DefinitionKind.Perk, byId, "/grantedPerkIds", diagnostics);
                    break;
                case DefinitionKind.Spell:
                    CheckReference(definition, definition.Strings["requiredAccessId"], DefinitionKind.Access, byId, "/requiredAccessId", diagnostics);
                    CheckReference(definition, definition.Strings["skillId"], DefinitionKind.Skill, byId, "/skillId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["focusResourceId"], "/focusResourceId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["rangeId"], "/rangeId", diagnostics);
                    CheckPrimitives(definition, definition.Arrays["effectIds"], "/effectIds", diagnostics);
                    break;
                case DefinitionKind.PsychicTechnique:
                    CheckReference(definition, definition.Strings["requiredAccessId"], DefinitionKind.Access, byId, "/requiredAccessId", diagnostics);
                    CheckReference(definition, definition.Strings["skillId"], DefinitionKind.Skill, byId, "/skillId", diagnostics);
                    CheckReference(definition, definition.Strings["resistanceSkillId"], DefinitionKind.Skill, byId, "/resistanceSkillId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["strainResourceId"], "/strainResourceId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["contactModeId"], "/contactModeId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["rangeId"], "/rangeId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["informationScopeId"], "/informationScopeId", diagnostics);
                    CheckPrimitives(definition, definition.Arrays["disciplineIds"], "/disciplineIds", diagnostics);
                    CheckPrimitives(definition, definition.Arrays["effectIds"], "/effectIds", diagnostics);
                    break;
                case DefinitionKind.Technique:
                    CheckReferences(definition, definition.Arrays["requiredAccessIds"], DefinitionKind.Access, byId, "/requiredAccessIds", diagnostics);
                    CheckReferences(definition, definition.Arrays["grantedPerkIds"], DefinitionKind.Perk, byId, "/grantedPerkIds", diagnostics);
                    break;
                case DefinitionKind.TrainingProject:
                    CheckReferences(definition, definition.Arrays["requiredSkillIds"], DefinitionKind.Skill, byId, "/requiredSkillIds", diagnostics);
                    CheckReferences(definition, definition.Arrays["grantedFeatIds"], DefinitionKind.Feat, byId, "/grantedFeatIds", diagnostics);
                    foreach (string technique in definition.Arrays["grantedTechniqueIds"])
                    {
                        CheckTechniqueReference(definition, technique, byId, "/grantedTechniqueIds", diagnostics);
                    }

                    CheckPrimitive(definition, definition.Strings["facilityId"], "/facilityId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["resourceId"], "/resourceId", diagnostics);
                    CheckPrimitive(definition, definition.Strings["safetyId"], "/safetyId", diagnostics);
                    break;
            }
        }


        ValidateGrantCycles(definitions, diagnostics);

        return !diagnostics.HasErrors;
    }

    private bool Validate(IReadOnlyList<SourceDefinition> definitions, DiagnosticSink diagnostics)
    {
        IOrderedEnumerable<SourceDefinition> ordered = definitions.OrderBy(value => value.Id);
        Dictionary<string, SourceDefinition> byId = definitions.ToDictionary(value => value.Id.ToString(), StringComparer.Ordinal);
        foreach (SourceDefinition definition in ordered)
        {
            if (definition.Revision < 1)
            {
                OutOfRange(definition, "/revision", diagnostics);
            }

            switch (definition.Kind)
            {
                case DefinitionKind.Attribute:
                    ValidateAttribute(definition, diagnostics);
                    break;
                case DefinitionKind.Skill:
                    ValidateSkill(definition, diagnostics);
                    break;
                case DefinitionKind.TrainingProject when definition.Integers["workUnits"] is < 1 or > 1_000_000:
                    OutOfRange(definition, "/workUnits", diagnostics);
                    break;
                case DefinitionKind.TrainingProject when definition.Integers["progressCap"] < definition.Integers["workUnits"] ||
                    definition.Integers["progressCap"] > 1_000_000:
                    OutOfRange(definition, "/progressCap", diagnostics);
                    break;
                case DefinitionKind.TrainingProject when definition.Integers["resourceCost"] is < 0 or > 1_000_000:
                    OutOfRange(definition, "/resourceCost", diagnostics);
                    break;
                case DefinitionKind.Spell when definition.Integers["focusCost"] is < 1 or > 1_000_000 ||
                    definition.Integers["castTimeTicks"] is < 0 or > 10_000 || definition.Integers["cooldownTicks"] is < 0 or > 1_000_000:
                    OutOfRange(definition, "/focusCost", diagnostics);
                    break;
                case DefinitionKind.PsychicTechnique when definition.Integers["strainCost"] is < 1 or > 100 ||
                    definition.Integers["sustainCostPerTick"] is < 0 or > 100:
                    OutOfRange(definition, "/strainCost", diagnostics);
                    break;
            }
        }

        int totalReferences = 0;
        foreach (SourceDefinition definition in ordered)
        {
            int definitionReferences = 0;
            foreach ((string field, ImmutableArray<string> values) in definition.Arrays)
            {
                if (values.Length != values.Distinct(StringComparer.Ordinal).Count())
                {
                    diagnostics.Add(ContentDiagnosticCodes.CollectionDuplicate, definition.PackId, definition.RelativePath,
                        definition.Id.ToString(), "/" + field);
                }

                if (field is not ("tags" or "targetTags"))
                {
                    totalReferences = checked(totalReferences + values.Length);
                    definitionReferences = checked(definitionReferences + values.Length);
                }
            }

            totalReferences = checked(totalReferences + definition.Strings.Count);
            definitionReferences = checked(definitionReferences + definition.Strings.Count);

            switch (definition.Kind)
            {
                case DefinitionKind.Feat:
                    RequireNonempty(definition, "grantedAccessIds", diagnostics);
                    break;
                case DefinitionKind.Background:
                    RequireNonempty(definition, "compatibleRaceIds", diagnostics);
                    RequireNonempty(definition, "focusSkillIds", diagnostics);
                    break;
                case DefinitionKind.Character:
                    ValidateCharacter(definition, byId, diagnostics);
                    RequireNonempty(definition, "scenarioIds", diagnostics);
                    RequireNonempty(definition, "languageIds", diagnostics);
                    RequireNonempty(definition, "scriptIds", diagnostics);
                    RequireNonempty(definition, "equipmentIds", diagnostics);
                    RequireNonempty(definition, "focusSkillIds", diagnostics);
                    RequireNonempty(definition, "resourceIds", diagnostics);
                    break;
                case DefinitionKind.Heritage:
                    ValidateHeritage(definition, byId, diagnostics);
                    RequireNonempty(definition, "grantedPerkIds", diagnostics);
                    break;
                case DefinitionKind.Perk:
                    RequireNonempty(definition, "compatibleRaceIds", diagnostics);
                    break;
                case DefinitionKind.Race:
                    ValidateRace(definition, byId, diagnostics);
                    RequireNonempty(definition, "grantedPerkIds", diagnostics);
                    break;
                case DefinitionKind.TrainingProject:
                    RequireNonempty(definition, "requiredSkillIds", diagnostics);
                    if (definition.Arrays["grantedFeatIds"].IsEmpty && definition.Arrays["grantedTechniqueIds"].IsEmpty)
                    {
                        diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, definition.PackId, definition.RelativePath,
                            definition.Id.ToString(), "/grantedFeatIds");
                    }
                    break;
                case DefinitionKind.Spell:
                    RequireNonempty(definition, "targetTags", diagnostics);
                    RequireNonempty(definition, "effectIds", diagnostics);
                    break;
                case DefinitionKind.PsychicTechnique:
                    RequireNonempty(definition, "disciplineIds", diagnostics);
                    RequireNonempty(definition, "targetTags", diagnostics);
                    RequireNonempty(definition, "effectIds", diagnostics);
                    break;
            }
        }

        foreach (SourceDefinition definition in ordered)
        {
            foreach ((string field, ImmutableArray<string> values) in definition.Arrays)
            {
                bool isTagField = field is "tags" or "targetTags";
                int maximum = isTagField ? limits.TagsPerDefinition : limits.ReferencesPerDefinition;
                if (values.Length > maximum)
                {
                    diagnostics.Limit(isTagField ? "tags-per-definition" : "references-per-definition",
                        definition.PackId, definition.RelativePath);
                }
            }

            int definitionReferences = definition.Arrays
                .Where(pair => pair.Key is not ("tags" or "targetTags"))
                .Sum(pair => pair.Value.Length);
            definitionReferences += definition.Strings.Count;

            if (definitionReferences > limits.ReferencesPerDefinition)
            {
                diagnostics.Limit("references-per-definition", definition.PackId, definition.RelativePath);
            }
        }

        if (totalReferences > limits.ReferencesPerSet)
        {
            diagnostics.Limit("references-per-content-set");
        }

        if (totalReferences > limits.GraphEdges)
        {
            diagnostics.Limit("graph-edges");
        }

        return !diagnostics.HasErrors;
    }

    private ContentCompilationResult CompileSnapshot(
        ImmutableArray<CandidatePack> packs,
        IReadOnlyList<SourceDefinition> sources,
        DiagnosticSink diagnostics)
    {
        ImmutableArray<AttributeDefinition> attributes = [.. sources.Where(value => value.Kind == DefinitionKind.Attribute).OrderBy(value => value.Id).Select(CompileAttribute)];
        ImmutableArray<SkillDefinition> skills = [.. sources.Where(value => value.Kind == DefinitionKind.Skill).OrderBy(value => value.Id).Select(CompileSkill)];
        ImmutableArray<AccessDefinition> access = [.. sources.Where(value => value.Kind == DefinitionKind.Access).OrderBy(value => value.Id).Select(CompileAccess)];
        ImmutableArray<BackgroundDefinition> backgrounds = [.. sources.Where(value => value.Kind == DefinitionKind.Background).OrderBy(value => value.Id).Select(CompileBackground)];
        ImmutableArray<CharacterDefinition> characters = [.. sources.Where(value => value.Kind == DefinitionKind.Character).OrderBy(value => value.Id).Select(CompileCharacter)];
        ImmutableArray<FeatDefinition> feats = [.. sources.Where(value => value.Kind == DefinitionKind.Feat).OrderBy(value => value.Id).Select(CompileFeat)];
        ImmutableArray<HeritageDefinition> heritages = [.. sources.Where(value => value.Kind == DefinitionKind.Heritage).OrderBy(value => value.Id).Select(CompileHeritage)];
        ImmutableArray<PerkDefinition> perks = [.. sources.Where(value => value.Kind == DefinitionKind.Perk).OrderBy(value => value.Id).Select(CompilePerk)];
        ImmutableArray<RaceDefinition> races = [.. sources.Where(value => value.Kind == DefinitionKind.Race).OrderBy(value => value.Id).Select(CompileRace)];
        ImmutableArray<SpellDefinition> spells = [.. sources.Where(value => value.Kind == DefinitionKind.Spell).OrderBy(value => value.Id).Select(CompileSpell)];
        ImmutableArray<PsychicTechniqueDefinition> psychicTechniques = [.. sources.Where(value => value.Kind == DefinitionKind.PsychicTechnique).OrderBy(value => value.Id).Select(CompilePsychicTechnique)];
        ImmutableArray<TechniqueDefinition> techniques = [.. sources.Where(value => value.Kind == DefinitionKind.Technique).OrderBy(value => value.Id).Select(CompileTechnique)];
        ImmutableArray<TrainingProjectDefinition> training = [.. sources.Where(value => value.Kind == DefinitionKind.TrainingProject).OrderBy(value => value.Id).Select(CompileTraining)];
        ImmutableArray<ContentPackIdentity> identities = [.. packs.Select(pack => new ContentPackIdentity(
            pack.Manifest.Id, pack.Manifest.Version, pack.Manifest.ContentRevision))];
        ContentDefinition[] all = [.. attributes, .. skills, .. access, .. backgrounds, .. characters, .. feats, .. heritages, .. perks, .. races, .. spells, .. psychicTechniques, .. techniques, .. training];
        (byte[] canonicalBytes, ContentFingerprint fingerprint) = CanonicalSemanticWriter.Write(identities, all);
        Dictionary<ContentId, ContentId> provenance = sources.ToDictionary(
            source => source.Id,
            source => new ContentId(source.PackId));
        GameContentSnapshot snapshot = new(fingerprint, identities, attributes, skills, access, backgrounds, characters, feats, heritages, perks, races, spells, psychicTechniques, techniques, training,
            [.. canonicalBytes], provenance);
        return new ContentCompilationResult(snapshot, diagnostics.ToImmutable(), null);
    }

    private static AttributeDefinition CompileAttribute(SourceDefinition value) => new(
        new AttributeId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        (short)value.Attribute!.Minimum, (short)value.Attribute.Maximum, (short)value.Attribute.DefaultValue,
        Sort(value.Attribute.Tags));

    private static SkillDefinition CompileSkill(SourceDefinition value) => new(
        new SkillId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        (byte)value.Skill!.Minimum, (byte)value.Skill.Maximum, value.Skill.ProgressionCurveId,
        [.. value.Skill.ActionTags.Order()]);

    private static AccessDefinition CompileAccess(SourceDefinition value) => new(
        new AccessId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey, Sort(value.Arrays["tags"]));

    private static BackgroundDefinition CompileBackground(SourceDefinition value) => new(
        new BackgroundId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        Sort(value.Arrays["compatibleRaceIds"]).Select(item => new RaceId(item)).ToImmutableArray(),
        Sort(value.Arrays["attributeBonusIds"]).Select(item => new AttributeId(item)).ToImmutableArray(),
        Sort(value.Arrays["focusSkillIds"]).Select(item => new SkillId(item)).ToImmutableArray());

    private static CharacterDefinition CompileCharacter(SourceDefinition value) => new(
        new CharacterId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        new RaceId(value.Strings["raceId"]), new HeritageId(value.Strings["heritageId"]),
        new BackgroundId(value.Strings["backgroundId"]),
        Sort(value.Arrays["scenarioIds"]).Select(item => new ScenarioId(item)).ToImmutableArray(),
        new ContentId(value.Strings["positionId"]),
        Sort(value.Arrays["languageIds"]).Select(item => new ContentId(item)).ToImmutableArray(),
        Sort(value.Arrays["scriptIds"]).Select(item => new ContentId(item)).ToImmutableArray(),
        Sort(value.Arrays["equipmentIds"]).Select(item => new ContentId(item)).ToImmutableArray(),
        Sort(value.Arrays["focusSkillIds"]).Select(item => new SkillId(item)).ToImmutableArray(),
        Sort(value.Arrays["resourceIds"]).Select(item => new ResourceId(item)).ToImmutableArray());

    private static FeatDefinition CompileFeat(SourceDefinition value) => new(
        new FeatId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        new TrainingProjectId(value.Strings["trainingProjectId"]),
        Sort(value.Arrays["grantedAccessIds"]).Select(item => new AccessId(item)).ToImmutableArray());

    private static PerkDefinition CompilePerk(SourceDefinition value) => new(
        new PerkId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        Sort(value.Arrays["compatibleRaceIds"]).Select(item => new RaceId(item)).ToImmutableArray(),
        Sort(value.Arrays["grantedAccessIds"]).Select(item => new AccessId(item)).ToImmutableArray(),
        Sort(value.Arrays["grantedTechniqueIds"]).Select(item => new TechniqueId(item)).ToImmutableArray(),
        Sort(value.Arrays["grantedPerkIds"]).Select(item => new PerkId(item)).ToImmutableArray(),
        Sort(value.Arrays["effectIds"]).Select(item => new ContentId(item)).ToImmutableArray());

    private static RaceDefinition CompileRace(SourceDefinition value) => new(
        new RaceId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        Sort(value.Arrays["grantedPerkIds"]).Select(item => new PerkId(item)).ToImmutableArray(),
        Sort(value.Arrays["requiredSupportIds"]).Select(item => new ContentId(item)).ToImmutableArray());

    private static HeritageDefinition CompileHeritage(SourceDefinition value) => new(
        new HeritageId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        new RaceId(value.Strings["raceId"]),
        Sort(value.Arrays["grantedPerkIds"]).Select(item => new PerkId(item)).ToImmutableArray());

    private static TechniqueDefinition CompileTechnique(SourceDefinition value) => new(
        new TechniqueId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        Sort(value.Arrays["requiredAccessIds"]).Select(item => new AccessId(item)).ToImmutableArray(),
        Sort(value.Arrays["grantedPerkIds"]).Select(item => new PerkId(item)).ToImmutableArray());

    private static SpellDefinition CompileSpell(SourceDefinition value) => new(
        new SpellId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        new AccessId(value.Strings["requiredAccessId"]), new SkillId(value.Strings["skillId"]),
        new ResourceId(value.Strings["focusResourceId"]), value.Integers["focusCost"],
        new ContentId(value.Strings["rangeId"]), value.Integers["castTimeTicks"], value.Integers["cooldownTicks"],
        Sort(value.Arrays["targetTags"]), Sort(value.Arrays["effectIds"]).Select(item => new ContentId(item)).ToImmutableArray());

    private static PsychicTechniqueDefinition CompilePsychicTechnique(SourceDefinition value) => new(
        new PsychicTechniqueId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        new AccessId(value.Strings["requiredAccessId"]), new SkillId(value.Strings["skillId"]),
        new SkillId(value.Strings["resistanceSkillId"]), new ResourceId(value.Strings["strainResourceId"]),
        value.Integers["strainCost"], value.Integers["sustainCostPerTick"],
        new ContentId(value.Strings["contactModeId"]), new ContentId(value.Strings["rangeId"]),
        new ContentId(value.Strings["informationScopeId"]),
        Sort(value.Arrays["disciplineIds"]).Select(item => new ContentId(item)).ToImmutableArray(),
        Sort(value.Arrays["targetTags"]), Sort(value.Arrays["effectIds"]).Select(item => new ContentId(item)).ToImmutableArray());

    private static TrainingProjectDefinition CompileTraining(SourceDefinition value) => new(
        new TrainingProjectId(value.Id), 1, value.Revision, value.NameKey, value.DescriptionKey,
        Sort(value.Arrays["requiredSkillIds"]).Select(item => new SkillId(item)).ToImmutableArray(),
        value.Integers["workUnits"],
        value.Integers["progressCap"], new ContentId(value.Strings["facilityId"]),
        new ResourceId(value.Strings["resourceId"]), value.Integers["resourceCost"],
        new ContentId(value.Strings["safetyId"]),
        Sort(value.Arrays["grantedFeatIds"]).Select(item => new FeatId(item)).ToImmutableArray(),
        Sort(value.Arrays["grantedTechniqueIds"]).Select(item => new TechniqueId(item)).ToImmutableArray());

    private static ImmutableArray<string> Sort(ImmutableArray<string> values) => [.. values.Order(StringComparer.Ordinal)];

    private static void ValidateAttribute(SourceDefinition definition, DiagnosticSink diagnostics)
    {
        AttributeSourceDto source = definition.Attribute!;
        int minimum = source.Minimum;
        int maximum = source.Maximum;
        int defaultValue = source.DefaultValue;
        bool storageValid = minimum >= short.MinValue && maximum <= short.MaxValue && minimum <= maximum &&
            defaultValue >= minimum && defaultValue <= maximum;
        bool baseRangeValid = definition.PackId != "spelljammer.base" || minimum == 1 && maximum == 10;
        if (!storageValid || !baseRangeValid)
        {
            OutOfRange(definition, "/defaultValue", diagnostics);
        }
    }

    private static void ValidateSkill(SourceDefinition definition, DiagnosticSink diagnostics)
    {
        SkillSourceDto source = definition.Skill!;
        int minimum = source.Minimum;
        int maximum = source.Maximum;
        bool storageValid = minimum >= byte.MinValue && maximum <= byte.MaxValue && minimum <= maximum;
        bool baseRangeValid = definition.PackId != "spelljammer.base" || minimum == 0 && maximum == 100;
        if (!storageValid || !baseRangeValid)
        {
            OutOfRange(definition, "/minimum", diagnostics);
        }
    }

    private static void ValidateRace(SourceDefinition race, IReadOnlyDictionary<string, SourceDefinition> byId, DiagnosticSink diagnostics)
    {
        foreach (string perkId in race.Arrays["grantedPerkIds"])
        {
            SourceDefinition perk = byId[perkId];
            if (!perk.Arrays["compatibleRaceIds"].Contains(race.Id.ToString(), StringComparer.Ordinal))
            {
                diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, race.PackId, race.RelativePath, race.Id.ToString(), "/grantedPerkIds");
            }
        }
    }

    private static void ValidateHeritage(SourceDefinition heritage, IReadOnlyDictionary<string, SourceDefinition> byId, DiagnosticSink diagnostics)
    {
        string raceId = heritage.Strings["raceId"];
        foreach (string perkId in heritage.Arrays["grantedPerkIds"])
        {
            SourceDefinition perk = byId[perkId];
            if (!perk.Arrays["compatibleRaceIds"].Contains(raceId, StringComparer.Ordinal))
            {
                diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, heritage.PackId, heritage.RelativePath,
                    heritage.Id.ToString(), "/grantedPerkIds");
            }
        }
    }

    private static void ValidateCharacter(SourceDefinition character, IReadOnlyDictionary<string, SourceDefinition> byId, DiagnosticSink diagnostics)
    {
        string raceId = character.Strings["raceId"];
        SourceDefinition race = byId[raceId];
        SourceDefinition heritage = byId[character.Strings["heritageId"]];
        SourceDefinition background = byId[character.Strings["backgroundId"]];
        if (heritage.Strings["raceId"] != raceId)
        {
            diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, character.PackId, character.RelativePath,
                character.Id.ToString(), "/heritageId");
        }

        if (!background.Arrays["compatibleRaceIds"].Contains(raceId, StringComparer.Ordinal))
        {
            diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, character.PackId, character.RelativePath,
                character.Id.ToString(), "/backgroundId");
        }

        if (race.Arrays["requiredSupportIds"].IsEmpty)
        {
            diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, character.PackId, character.RelativePath,
                character.Id.ToString(), "/raceId");
        }
    }

    private void ValidateGrantCycles(IReadOnlyList<SourceDefinition> definitions, DiagnosticSink diagnostics)
    {
        Dictionary<string, SourceDefinition> grantNodes = definitions
            .Where(value => value.Kind is DefinitionKind.Perk or DefinitionKind.Technique)
            .ToDictionary(value => value.Id.ToString(), StringComparer.Ordinal);
        Dictionary<string, byte> marks = new(StringComparer.Ordinal);
        foreach (SourceDefinition node in grantNodes.Values.OrderBy(value => value.Id))
        {
            Visit(node, 0);
        }

        void Visit(SourceDefinition node, int depth)
        {
            if (depth >= limits.ValidationTraversalDepth)
            {
                diagnostics.Limit("grant-depth", node.PackId, node.RelativePath);
                return;
            }

            string id = node.Id.ToString();
            if (marks.TryGetValue(id, out byte mark))
            {
                if (mark == 1)
                {
                    diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, node.PackId, node.RelativePath, id, "/grants");
                }

                return;
            }

            marks[id] = 1;
            IEnumerable<string> successors = node.Kind == DefinitionKind.Perk
                ? node.Arrays["grantedPerkIds"].Concat(node.Arrays["grantedTechniqueIds"])
                : node.Arrays["grantedPerkIds"];
            foreach (string successor in successors.Order(StringComparer.Ordinal))
            {
                if (grantNodes.TryGetValue(successor, out SourceDefinition? target))
                {
                    Visit(target, depth + 1);
                }
            }

            marks[id] = 2;
        }
    }

    private static void RequireNonempty(SourceDefinition definition, string field, DiagnosticSink diagnostics)
    {
        if (definition.Arrays[field].IsEmpty)
        {
            diagnostics.Add(ContentDiagnosticCodes.SemanticInvalid, definition.PackId, definition.RelativePath,
                definition.Id.ToString(), "/" + field);
        }
    }

    private static void OutOfRange(SourceDefinition definition, string property, DiagnosticSink diagnostics) =>
        diagnostics.Add(ContentDiagnosticCodes.ValueOutOfRange, definition.PackId, definition.RelativePath, definition.Id.ToString(), property);

    private static void CheckPrimitive(SourceDefinition definition, string id, string property, DiagnosticSink diagnostics)
    {
        if (!RuntimePrimitives.Contains(id))
        {
            Unknown(definition, id, property, diagnostics);
        }
    }

    private static void CheckPrimitives(SourceDefinition definition, IEnumerable<string> ids, string property, DiagnosticSink diagnostics)
    {
        foreach (string id in ids)
        {
            CheckPrimitive(definition, id, property, diagnostics);
        }
    }

    private static void CheckReferences(
        SourceDefinition definition,
        IEnumerable<string> ids,
        DefinitionKind kind,
        IReadOnlyDictionary<string, SourceDefinition> byId,
        string property,
        DiagnosticSink diagnostics)
    {
        foreach (string id in ids)
        {
            CheckReference(definition, id, kind, byId, property, diagnostics);
        }
    }

    private static void CheckReference(
        SourceDefinition definition,
        string id,
        DefinitionKind kind,
        IReadOnlyDictionary<string, SourceDefinition> byId,
        string property,
        DiagnosticSink diagnostics)
    {
        if (!byId.TryGetValue(id, out SourceDefinition? target) || target.Kind != kind)
        {
            Unknown(definition, id, property, diagnostics);
        }
    }

    private static void CheckTechniqueReference(
        SourceDefinition definition,
        string id,
        IReadOnlyDictionary<string, SourceDefinition> byId,
        string property,
        DiagnosticSink diagnostics)
    {
        if (!byId.TryGetValue(id, out SourceDefinition? target) ||
            target.Kind is not (DefinitionKind.Technique or DefinitionKind.Spell or DefinitionKind.PsychicTechnique))
        {
            Unknown(definition, id, property, diagnostics);
        }
    }

    private static void Unknown(SourceDefinition definition, string id, string property, DiagnosticSink diagnostics) =>
        diagnostics.Add(ContentDiagnosticCodes.ReferenceUnknown, definition.PackId, definition.RelativePath,
            definition.Id.ToString(), property, ContentDiagnosticArgument.SafeId(id));

    private static string Prefix(DefinitionKind kind) => kind switch
    {
        DefinitionKind.Attribute => "attribute.",
        DefinitionKind.Skill => "skill.",
        DefinitionKind.Access => "access.",
        DefinitionKind.Background => "background.",
        DefinitionKind.Character => "character.",
        DefinitionKind.Feat => "feat.",
        DefinitionKind.Heritage => "heritage.",
        DefinitionKind.Perk => "perk.",
        DefinitionKind.Race => "race.",
        DefinitionKind.Spell => "spell.",
        DefinitionKind.PsychicTechnique => "psychic.",
        DefinitionKind.Technique => "technique.",
        DefinitionKind.TrainingProject => "training.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static bool Owns(string packId, string definitionId)
    {
        string[] segments = definitionId.Split('.');
        if (packId == "spelljammer.base")
        {
            return !segments.Contains("mod", StringComparer.Ordinal);
        }

        string namespaceName = packId["mod.".Length..];
        return segments.Length >= 4 && segments[1] == "mod" && segments[2] == namespaceName;
    }

    private static ContentCompilationResult Failed(DiagnosticSink diagnostics) =>
        new(null, diagnostics.ToImmutable(), null);

    private ContentCompilationResult IoFailed(DiagnosticSink diagnostics, ContentSourceException exception) =>
        new(null, diagnostics.ToImmutable(), CreateIoFailure(exception));

    private ContentIoFailure CreateIoFailure(ContentSourceException exception)
    {
        string? relativePath = exception.RelativePath;
        if (relativePath is null || !SourceValidation.IsRelativePath(relativePath) ||
            System.Text.Encoding.UTF8.GetByteCount(relativePath) > limits.DiagnosticArgumentBytes)
        {
            relativePath = null;
        }

        return new ContentIoFailure(exception.Kind, relativePath);
    }

    private sealed class PackOrderComparer : IComparer<ContentId>
    {
        public int Compare(ContentId left, ContentId right)
        {
            bool leftBase = left.ToString() == "spelljammer.base";
            bool rightBase = right.ToString() == "spelljammer.base";
            if (leftBase != rightBase)
            {
                return leftBase ? -1 : 1;
            }

            return left.CompareTo(right);
        }
    }
}
