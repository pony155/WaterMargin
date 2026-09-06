using System.Collections.Immutable;
using System.Text.Json;
using Spelljammer.Content.Compilation;
using Spelljammer.Content.Diagnostics;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Content.Parsing;

internal static class DefinitionParser
{
    private static readonly string[] CommonRequired = ["schemaVersion", "revision", "id", "nameKey", "descriptionKey"];

    private static readonly IReadOnlyDictionary<DefinitionKind, string[]> KindFields =
        new Dictionary<DefinitionKind, string[]>
        {
            [DefinitionKind.Attribute] = ["minimum", "maximum", "defaultValue", "tags"],
            [DefinitionKind.Skill] = ["minimum", "maximum", "progressionCurveId", "actionTags"],
            [DefinitionKind.Access] = ["tags"],
            [DefinitionKind.Feat] = ["trainingProjectId", "grantedAccessIds"],
            [DefinitionKind.Perk] = ["compatibleRaceIds", "grantedAccessIds", "grantedTechniqueIds"],
            [DefinitionKind.Race] = ["grantedPerkIds"],
            [DefinitionKind.TrainingProject] = ["requiredSkillIds", "workUnits", "grantedFeatIds"],
        };

    private static readonly IReadOnlyDictionary<string, DefinitionKind> Directories =
        new Dictionary<string, DefinitionKind>(StringComparer.Ordinal)
        {
            ["Attributes"] = DefinitionKind.Attribute,
            ["Skills"] = DefinitionKind.Skill,
            ["Access"] = DefinitionKind.Access,
            ["Feats"] = DefinitionKind.Feat,
            ["Perks"] = DefinitionKind.Perk,
            ["Races"] = DefinitionKind.Race,
            ["TrainingProjects"] = DefinitionKind.TrainingProject,
        };

    public static bool TryGetKind(string pathUnderRoot, out DefinitionKind kind)
    {
        int separator = pathUnderRoot.IndexOf('/');
        if (separator <= 0 || separator == pathUnderRoot.Length - 1)
        {
            kind = default;
            return false;
        }

        return Directories.TryGetValue(pathUnderRoot[..separator], out kind);
    }

    public static SourceDefinition? Parse(
        byte[] bytes,
        DefinitionKind kind,
        string packId,
        string relativePath,
        ContentLimits limits,
        DiagnosticSink diagnostics)
    {
        using JsonDocument? document = StrictJson.Parse(bytes, packId, relativePath, limits, diagnostics);
        if (document is null)
        {
            return null;
        }

        string[] kindFields = KindFields[kind];
        string[] required = [.. CommonRequired, .. kindFields];
        HashSet<string> allowed = new(required, StringComparer.Ordinal);
        JsonElement root = document.RootElement;
        if (!SourceValidation.ValidateProperties(root, allowed, required, diagnostics, packId, relativePath) ||
            !SourceValidation.TrySchemaVersion(root, diagnostics, packId, relativePath))
        {
            return null;
        }

        if (!SourceValidation.TryString(root, "id", out string idText) || !ContentId.TryParse(idText, out ContentId id) ||
            !SourceValidation.TryString(root, "nameKey", out string nameKey) || !SourceValidation.IsLocalizationKey(nameKey, limits) ||
            !SourceValidation.TryString(root, "descriptionKey", out string descriptionKey) || !SourceValidation.IsLocalizationKey(descriptionKey, limits))
        {
            diagnostics.Add(ContentDiagnosticCodes.IdInvalid, packId, relativePath);
            return null;
        }

        JsonElement revisionElement = root.GetProperty("revision");
        int revision = revisionElement.ValueKind == JsonValueKind.Number && revisionElement.TryGetInt32(out int parsedRevision)
            ? parsedRevision
            : 0;

        Dictionary<string, int> integers = new(StringComparer.Ordinal);
        Dictionary<string, string> strings = new(StringComparer.Ordinal);
        Dictionary<string, ImmutableArray<string>> arrays = new(StringComparer.Ordinal);
        foreach (string field in kindFields)
        {
            JsonElement value = root.GetProperty(field);
            if (value.ValueKind == JsonValueKind.Number)
            {
                if (!value.TryGetInt32(out int number))
                {
                    diagnostics.Add(ContentDiagnosticCodes.ValueOutOfRange, packId, relativePath, idText, "/" + field);
                    return null;
                }

                integers.Add(field, number);
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                strings.Add(field, value.GetString()!);
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                ImmutableArray<string>.Builder values = ImmutableArray.CreateBuilder<string>();
                foreach (JsonElement item in value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        diagnostics.Add(ContentDiagnosticCodes.JsonInvalid, packId, relativePath, idText, "/" + field);
                        return null;
                    }

                    values.Add(item.GetString()!);
                }

                arrays.Add(field, values.ToImmutable());
            }
            else
            {
                diagnostics.Add(ContentDiagnosticCodes.JsonInvalid, packId, relativePath, idText, "/" + field);
                return null;
            }
        }

        foreach ((string field, string value) in strings)
        {
            if (!ContentId.IsCanonical(value))
            {
                diagnostics.Add(ContentDiagnosticCodes.IdInvalid, packId, relativePath, idText, "/" + field);
                return null;
            }
        }

        foreach ((string field, ImmutableArray<string> values) in arrays)
        {
            if (field == "tags")
            {
                if (values.Any(value => !SourceValidation.IsIdSegment(value)))
                {
                    diagnostics.Add(ContentDiagnosticCodes.IdInvalid, packId, relativePath, idText, "/tags");
                    return null;
                }

                continue;
            }

            for (int index = 0; index < values.Length; index++)
            {
                if (!ContentId.IsCanonical(values[index]))
                {
                    diagnostics.Add(ContentDiagnosticCodes.IdInvalid, packId, relativePath, idText, $"/{field}/{index}");
                    return null;
                }
            }
        }

        return new SourceDefinition(kind, id, 1, revision, nameKey, descriptionKey, packId, relativePath,
            integers, strings, arrays);
    }
}
