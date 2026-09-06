using System.Collections.Frozen;
using System.Collections.Immutable;
using Spelljammer.Content.Manifests;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Content.Compilation;

public readonly record struct ContentFingerprint
{
    public ContentFingerprint(string hexadecimal)
    {
        ArgumentNullException.ThrowIfNull(hexadecimal);
        if (hexadecimal.Length != 64 || hexadecimal.Any(character =>
                character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new ArgumentException("A content fingerprint must be a lowercase SHA-256 value.", nameof(hexadecimal));
        }

        Hexadecimal = hexadecimal;
    }

    public string? Hexadecimal { get; }
    public bool IsValid => Hexadecimal is not null;
    public override string ToString() => Hexadecimal ?? string.Empty;
}

public sealed record ContentPackIdentity(ContentId Id, SemanticVersion Version, int ContentRevision);

public sealed class GameContentSnapshot
{
    private readonly FrozenDictionary<ContentId, ContentDefinition> definitionsById;

    internal GameContentSnapshot(
        ContentFingerprint fingerprint,
        ImmutableArray<ContentPackIdentity> packs,
        ImmutableArray<AttributeDefinition> attributes,
        ImmutableArray<SkillDefinition> skills,
        ImmutableArray<AccessDefinition> access,
        ImmutableArray<FeatDefinition> feats,
        ImmutableArray<PerkDefinition> perks,
        ImmutableArray<RaceDefinition> races,
        ImmutableArray<TrainingProjectDefinition> trainingProjects,
        ImmutableArray<byte> canonicalSemanticContent)
    {
        Fingerprint = fingerprint;
        Packs = packs;
        Attributes = attributes;
        Skills = skills;
        Access = access;
        Feats = feats;
        Perks = perks;
        Races = races;
        TrainingProjects = trainingProjects;
        CanonicalSemanticContent = canonicalSemanticContent;

        definitionsById = attributes.Cast<ContentDefinition>()
            .Concat(skills)
            .Concat(access)
            .Concat(feats)
            .Concat(perks)
            .Concat(races)
            .Concat(trainingProjects)
            .ToFrozenDictionary(definition => definition.Id);
    }

    public ContentFingerprint Fingerprint { get; }
    public ImmutableArray<ContentPackIdentity> Packs { get; }
    public ImmutableArray<AttributeDefinition> Attributes { get; }
    public ImmutableArray<SkillDefinition> Skills { get; }
    public ImmutableArray<AccessDefinition> Access { get; }
    public ImmutableArray<FeatDefinition> Feats { get; }
    public ImmutableArray<PerkDefinition> Perks { get; }
    public ImmutableArray<RaceDefinition> Races { get; }
    public ImmutableArray<TrainingProjectDefinition> TrainingProjects { get; }
    public ImmutableArray<byte> CanonicalSemanticContent { get; }

    public bool TryGetDefinition(ContentId id, out ContentDefinition? definition) =>
        definitionsById.TryGetValue(id, out definition);
}
