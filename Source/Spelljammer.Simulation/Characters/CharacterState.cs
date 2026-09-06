using System.Collections.Immutable;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Simulation.Characters;

public sealed class CharacterCapabilities
{
    public const int MaximumSetEntries = 256;
    public const int MaximumPracticeKeys = 512;

    private readonly ImmutableArray<short> attributeValues;
    private readonly ImmutableArray<byte> skillValues;
    private readonly ImmutableArray<ushort> skillPractice;
    private readonly ImmutableHashSet<ContentId> practiceKeys;

    internal CharacterCapabilities(
        ContentFingerprint fingerprint,
        ImmutableArray<short> attributeValues,
        ImmutableArray<byte> skillValues,
        ImmutableArray<ushort> skillPractice,
        ImmutableHashSet<FeatId> feats,
        ImmutableHashSet<PerkId> perks,
        ImmutableHashSet<AccessId> access,
        ImmutableHashSet<TechniqueId> techniques,
        ImmutableArray<CapabilityGrant> grantSources,
        ImmutableHashSet<ContentId>? practiceKeys = null)
    {
        if (attributeValues.Length == 0 || skillValues.Length == 0 || skillValues.Length != skillPractice.Length ||
            feats.Count > MaximumSetEntries || perks.Count > MaximumSetEntries || access.Count > MaximumSetEntries ||
            techniques.Count > MaximumSetEntries || grantSources.Length > MaximumSetEntries)
        {
            throw new ArgumentException("Character capability storage is incomplete or exceeds its bounded capacity.");
        }

        Fingerprint = fingerprint;
        this.attributeValues = attributeValues;
        this.skillValues = skillValues;
        this.skillPractice = skillPractice;
        Feats = feats;
        Perks = perks;
        Access = access;
        Techniques = techniques;
        GrantSources = grantSources;
        this.practiceKeys = practiceKeys ?? ImmutableHashSet<ContentId>.Empty;
    }

    public ContentFingerprint Fingerprint { get; }
    public ImmutableHashSet<FeatId> Feats { get; }
    public ImmutableHashSet<PerkId> Perks { get; }
    public ImmutableHashSet<AccessId> Access { get; }
    public ImmutableHashSet<TechniqueId> Techniques { get; }
    public ImmutableArray<CapabilityGrant> GrantSources { get; }

    public bool TryGetAttribute(
        AttributeId id,
        ICharacterContentCatalog catalog,
        out short value,
        out CapabilityLookupFailure failure)
    {
        if (catalog.Fingerprint != Fingerprint)
        {
            value = default;
            failure = CapabilityLookupFailure.ContentMismatch;
            return false;
        }

        if (!catalog.TryGetAttribute(id, out _, out int index) || (uint)index >= (uint)attributeValues.Length)
        {
            value = default;
            failure = CapabilityLookupFailure.DefinitionMissing;
            return false;
        }

        value = attributeValues[index];
        failure = CapabilityLookupFailure.None;
        return true;
    }

    public bool TryGetSkill(
        SkillId id,
        ICharacterContentCatalog catalog,
        out byte value,
        out CapabilityLookupFailure failure)
    {
        if (catalog.Fingerprint != Fingerprint)
        {
            value = default;
            failure = CapabilityLookupFailure.ContentMismatch;
            return false;
        }

        if (!catalog.TryGetSkill(id, out _, out int index) || (uint)index >= (uint)skillValues.Length)
        {
            value = default;
            failure = CapabilityLookupFailure.DefinitionMissing;
            return false;
        }

        value = skillValues[index];
        failure = CapabilityLookupFailure.None;
        return true;
    }

    public CharacterCapabilitySnapshot Snapshot(ICharacterContentCatalog catalog)
    {
        if (catalog.Fingerprint != Fingerprint || catalog.Attributes.Length != attributeValues.Length ||
            catalog.Skills.Length != skillValues.Length)
        {
            throw new InvalidOperationException("The capability state does not belong to this content catalog.");
        }

        ImmutableArray<AttributeValueSnapshot>.Builder attributes = ImmutableArray.CreateBuilder<AttributeValueSnapshot>(attributeValues.Length);
        for (int index = 0; index < attributeValues.Length; index++)
        {
            attributes.Add(new AttributeValueSnapshot(catalog.Attributes[index].AttributeId, attributeValues[index]));
        }

        ImmutableArray<SkillValueSnapshot>.Builder skills = ImmutableArray.CreateBuilder<SkillValueSnapshot>(skillValues.Length);
        for (int index = 0; index < skillValues.Length; index++)
        {
            skills.Add(new SkillValueSnapshot(catalog.Skills[index].SkillId, skillValues[index], skillPractice[index]));
        }

        return new CharacterCapabilitySnapshot(
            Fingerprint,
            attributes.MoveToImmutable(),
            skills.MoveToImmutable(),
            [.. Feats.Order()],
            [.. Perks.Order()],
            [.. Access.Order()],
            [.. Techniques.Order()],
            [.. GrantSources.OrderBy(value => value.CapabilityId).ThenBy(value => value.SourceId)]);
    }

    internal CharacterCapabilities AwardPractice(
        ICharacterContentCatalog catalog,
        SkillId skillId,
        ushort amount,
        ContentId practiceKey,
        out SkillAdvancementEvent? advancement)
    {
        advancement = null;
        if (amount == 0 || practiceKeys.Contains(practiceKey) || practiceKeys.Count >= MaximumPracticeKeys ||
            catalog.Fingerprint != Fingerprint || !catalog.TryGetSkill(skillId, out SkillDefinition? definition, out int index))
        {
            return this;
        }

        int practice = Math.Min(ushort.MaxValue, skillPractice[index] + amount);
        int value = skillValues[index];
        int threshold = checked((value + 1) * 10);
        if (practice >= threshold && value < definition!.Maximum)
        {
            practice -= threshold;
            value++;
            advancement = new SkillAdvancementEvent(skillId, skillValues[index], (byte)value, amount, practiceKey);
        }

        return new CharacterCapabilities(
            Fingerprint,
            attributeValues,
            skillValues.SetItem(index, (byte)value),
            skillPractice.SetItem(index, (ushort)practice),
            Feats,
            Perks,
            Access,
            Techniques,
            GrantSources,
            practiceKeys.Add(practiceKey));
    }

    internal CharacterCapabilities WithTrainingGrants(
        FeatDefinition feat,
        TrainingProjectId projectId)
    {
        if (Feats.Contains(feat.FeatId))
        {
            return this;
        }

        ImmutableHashSet<FeatId> feats = Feats.Add(feat.FeatId);
        ImmutableHashSet<AccessId> access = Access.Union(feat.GrantedAccessIds);
        ImmutableArray<CapabilityGrant>.Builder grants = GrantSources.ToBuilder();
        grants.Add(new CapabilityGrant(feat.FeatId.Value, projectId.Value, GrantSourceKind.TrainingProject));
        foreach (AccessId accessId in feat.GrantedAccessIds)
        {
            grants.Add(new CapabilityGrant(accessId.Value, feat.FeatId.Value, GrantSourceKind.Feat));
        }

        if (grants.Count > MaximumSetEntries)
        {
            throw new InvalidOperationException("Training grants exceed the character capability capacity.");
        }

        return new CharacterCapabilities(Fingerprint, attributeValues, skillValues, skillPractice, feats, Perks,
            access, Techniques, grants.MoveToImmutable(), practiceKeys);
    }
}

public sealed record CharacterState(
    CharacterId Id,
    ContentFingerprint ContentFingerprint,
    ScenarioId ScenarioId,
    RaceId RaceId,
    HeritageId HeritageId,
    BackgroundId BackgroundId,
    ContentId PositionId,
    CharacterCapabilities Capabilities,
    ImmutableArray<ContentId> LanguageIds,
    ImmutableArray<ContentId> ScriptIds,
    ImmutableHashSet<ContentId> EquipmentIds,
    ImmutableDictionary<ResourceId, int> Resources,
    ImmutableDictionary<TrainingProjectId, int> TrainingProgress,
    bool CanAct = true);

public sealed record SkillAdvancementEvent(
    SkillId SkillId,
    byte PreviousValue,
    byte NewValue,
    ushort PracticeAward,
    ContentId PracticeKey);
