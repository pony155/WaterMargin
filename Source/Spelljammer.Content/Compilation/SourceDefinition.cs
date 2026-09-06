using System.Collections.Immutable;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Content.Compilation;

internal enum DefinitionKind : byte
{
    Attribute,
    Skill,
    Access,
    Feat,
    Perk,
    Race,
    TrainingProject
}

internal sealed record SourceDefinition(
    DefinitionKind Kind,
    ContentId Id,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    string PackId,
    string RelativePath,
    IReadOnlyDictionary<string, int> Integers,
    IReadOnlyDictionary<string, string> Strings,
    IReadOnlyDictionary<string, ImmutableArray<string>> Arrays);

internal sealed record CandidatePack(
    Sources.IContentPackSource Source,
    Manifests.PackManifest Manifest,
    ImmutableArray<string> Entries);
