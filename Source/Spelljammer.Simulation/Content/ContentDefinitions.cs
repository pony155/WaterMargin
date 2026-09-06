using System.Collections.Immutable;

namespace Spelljammer.Simulation.Content;

public abstract record ContentDefinition(ContentId Id, int SchemaVersion, int Revision, string NameKey, string DescriptionKey);

public sealed record AttributeDefinition(
    AttributeId AttributeId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    short Minimum,
    short Maximum,
    short DefaultValue,
    ImmutableArray<string> Tags)
    : ContentDefinition(AttributeId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record SkillDefinition(
    SkillId SkillId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    byte Minimum,
    byte Maximum,
    ContentId ProgressionCurveId,
    ImmutableArray<ContentId> ActionTags)
    : ContentDefinition(SkillId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record AccessDefinition(
    AccessId AccessId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<string> Tags)
    : ContentDefinition(AccessId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record FeatDefinition(
    FeatId FeatId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    TrainingProjectId TrainingProjectId,
    ImmutableArray<AccessId> GrantedAccessIds)
    : ContentDefinition(FeatId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record PerkDefinition(
    PerkId PerkId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<RaceId> CompatibleRaceIds,
    ImmutableArray<AccessId> GrantedAccessIds,
    ImmutableArray<TechniqueId> GrantedTechniqueIds,
    ImmutableArray<PerkId> GrantedPerkIds,
    ImmutableArray<ContentId> EffectIds)
    : ContentDefinition(PerkId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record RaceDefinition(
    RaceId RaceId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<PerkId> GrantedPerkIds,
    ImmutableArray<ContentId> RequiredSupportIds)
    : ContentDefinition(RaceId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record TrainingProjectDefinition(
    TrainingProjectId TrainingProjectId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<SkillId> RequiredSkillIds,
    int WorkUnits,
    ImmutableArray<FeatId> GrantedFeatIds)
    : ContentDefinition(TrainingProjectId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record HeritageDefinition(
    HeritageId HeritageId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    RaceId RaceId,
    ImmutableArray<PerkId> GrantedPerkIds)
    : ContentDefinition(HeritageId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record BackgroundDefinition(
    BackgroundId BackgroundId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<RaceId> CompatibleRaceIds,
    ImmutableArray<AttributeId> AttributeBonusIds,
    ImmutableArray<SkillId> FocusSkillIds)
    : ContentDefinition(BackgroundId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record TechniqueDefinition(
    TechniqueId TechniqueId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    ImmutableArray<AccessId> RequiredAccessIds,
    ImmutableArray<PerkId> GrantedPerkIds)
    : ContentDefinition(TechniqueId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);

public sealed record CharacterDefinition(
    CharacterId CharacterId,
    int SchemaVersion,
    int Revision,
    string NameKey,
    string DescriptionKey,
    RaceId RaceId,
    HeritageId HeritageId,
    BackgroundId BackgroundId,
    ImmutableArray<ScenarioId> ScenarioIds,
    ContentId PositionId,
    ImmutableArray<ContentId> LanguageIds,
    ImmutableArray<ContentId> ScriptIds,
    ImmutableArray<ContentId> EquipmentIds,
    ImmutableArray<SkillId> FocusSkillIds,
    ImmutableArray<ResourceId> ResourceIds)
    : ContentDefinition(CharacterId.Value, SchemaVersion, Revision, NameKey, DescriptionKey);
