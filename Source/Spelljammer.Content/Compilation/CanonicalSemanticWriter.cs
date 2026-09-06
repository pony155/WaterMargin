using System.Security.Cryptography;
using System.Text;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Content.Compilation;

internal static class CanonicalSemanticWriter
{
    public static (byte[] Bytes, ContentFingerprint Fingerprint) Write(
        IReadOnlyList<ContentPackIdentity> packs,
        IReadOnlyList<ContentDefinition> definitions)
    {
        StringBuilder builder = new();
        builder.Append("{\"definitions\":[");
        bool first = true;
        foreach (ContentDefinition definition in definitions
                     .OrderBy(CanonicalKindOrder)
                     .ThenBy(value => value.Id))
        {
            if (!first)
            {
                builder.Append(',');
            }

            first = false;
            WriteDefinition(builder, definition);
        }

        builder.Append("],\"format\":\"spelljammer-semantic-v1\",\"packs\":[");
        for (int index = 0; index < packs.Count; index++)
        {
            if (index != 0)
            {
                builder.Append(',');
            }

            ContentPackIdentity pack = packs[index];
            builder.Append("{\"contentRevision\":").Append(pack.ContentRevision).Append(",\"id\":");
            WriteString(builder, pack.Id.ToString());
            builder.Append('}');
        }

        builder.Append("]}\n");
        byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
        string hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        return (bytes, new ContentFingerprint(hash));
    }

    private static void WriteDefinition(StringBuilder builder, ContentDefinition definition)
    {
        SortedDictionary<string, Action<StringBuilder>> properties = new(StringComparer.Ordinal)
        {
            ["id"] = value => WriteString(value, definition.Id.ToString()),
            ["kind"] = value => WriteString(value, definition.GetType().Name.Replace("Definition", string.Empty, StringComparison.Ordinal)),
            ["revision"] = value => value.Append(definition.Revision),
            ["schemaVersion"] = value => value.Append(definition.SchemaVersion),
        };

        switch (definition)
        {
            case AttributeDefinition value:
                properties["defaultValue"] = output => output.Append(value.DefaultValue);
                properties["maximum"] = output => output.Append(value.Maximum);
                properties["minimum"] = output => output.Append(value.Minimum);
                properties["tags"] = output => WriteStrings(output, value.Tags);
                break;
            case SkillDefinition value:
                properties["actionTags"] = output => WriteIds(output, value.ActionTags);
                properties["maximum"] = output => output.Append(value.Maximum);
                properties["minimum"] = output => output.Append(value.Minimum);
                properties["progressionCurveId"] = output => WriteString(output, value.ProgressionCurveId.ToString());
                break;
            case AccessDefinition value:
                properties["tags"] = output => WriteStrings(output, value.Tags);
                break;
            case BackgroundDefinition value:
                properties["attributeBonusIds"] = output => WriteIds(output, value.AttributeBonusIds.Select(id => id.Value));
                properties["compatibleRaceIds"] = output => WriteIds(output, value.CompatibleRaceIds.Select(id => id.Value));
                properties["focusSkillIds"] = output => WriteIds(output, value.FocusSkillIds.Select(id => id.Value));
                break;
            case CharacterDefinition value:
                properties["backgroundId"] = output => WriteString(output, value.BackgroundId.ToString());
                properties["equipmentIds"] = output => WriteIds(output, value.EquipmentIds);
                properties["focusSkillIds"] = output => WriteIds(output, value.FocusSkillIds.Select(id => id.Value));
                properties["heritageId"] = output => WriteString(output, value.HeritageId.ToString());
                properties["languageIds"] = output => WriteIds(output, value.LanguageIds);
                properties["positionId"] = output => WriteString(output, value.PositionId.ToString());
                properties["raceId"] = output => WriteString(output, value.RaceId.ToString());
                properties["resourceIds"] = output => WriteIds(output, value.ResourceIds.Select(id => id.Value));
                properties["scenarioIds"] = output => WriteIds(output, value.ScenarioIds.Select(id => id.Value));
                properties["scriptIds"] = output => WriteIds(output, value.ScriptIds);
                break;
            case FeatDefinition value:
                properties["grantedAccessIds"] = output => WriteIds(output, value.GrantedAccessIds.Select(id => id.Value));
                properties["trainingProjectId"] = output => WriteString(output, value.TrainingProjectId.ToString());
                break;
            case PerkDefinition value:
                properties["compatibleRaceIds"] = output => WriteIds(output, value.CompatibleRaceIds.Select(id => id.Value));
                properties["grantedAccessIds"] = output => WriteIds(output, value.GrantedAccessIds.Select(id => id.Value));
                properties["grantedTechniqueIds"] = output => WriteIds(output, value.GrantedTechniqueIds.Select(id => id.Value));
                if (!value.EffectIds.IsEmpty)
                {
                    properties["effectIds"] = output => WriteIds(output, value.EffectIds);
                }

                if (!value.GrantedPerkIds.IsEmpty)
                {
                    properties["grantedPerkIds"] = output => WriteIds(output, value.GrantedPerkIds.Select(id => id.Value));
                }

                break;
            case RaceDefinition value:
                properties["grantedPerkIds"] = output => WriteIds(output, value.GrantedPerkIds.Select(id => id.Value));
                if (!value.RequiredSupportIds.IsEmpty)
                {
                    properties["requiredSupportIds"] = output => WriteIds(output, value.RequiredSupportIds);
                }

                break;
            case HeritageDefinition value:
                properties["grantedPerkIds"] = output => WriteIds(output, value.GrantedPerkIds.Select(id => id.Value));
                properties["raceId"] = output => WriteString(output, value.RaceId.ToString());
                break;
            case TechniqueDefinition value:
                properties["grantedPerkIds"] = output => WriteIds(output, value.GrantedPerkIds.Select(id => id.Value));
                properties["requiredAccessIds"] = output => WriteIds(output, value.RequiredAccessIds.Select(id => id.Value));
                break;
            case SpellDefinition value:
                properties["castTimeTicks"] = output => output.Append(value.CastTimeTicks);
                properties["cooldownTicks"] = output => output.Append(value.CooldownTicks);
                properties["effectIds"] = output => WriteIds(output, value.EffectIds);
                properties["focusCost"] = output => output.Append(value.FocusCost);
                properties["focusResourceId"] = output => WriteString(output, value.FocusResourceId.ToString());
                properties["rangeId"] = output => WriteString(output, value.RangeId.ToString());
                properties["requiredAccessId"] = output => WriteString(output, value.RequiredAccessId.ToString());
                properties["skillId"] = output => WriteString(output, value.SkillId.ToString());
                properties["targetTags"] = output => WriteStrings(output, value.TargetTags);
                break;
            case PsychicTechniqueDefinition value:
                properties["contactModeId"] = output => WriteString(output, value.ContactModeId.ToString());
                properties["disciplineIds"] = output => WriteIds(output, value.DisciplineIds);
                properties["effectIds"] = output => WriteIds(output, value.EffectIds);
                properties["informationScopeId"] = output => WriteString(output, value.InformationScopeId.ToString());
                properties["rangeId"] = output => WriteString(output, value.RangeId.ToString());
                properties["requiredAccessId"] = output => WriteString(output, value.RequiredAccessId.ToString());
                properties["resistanceSkillId"] = output => WriteString(output, value.ResistanceSkillId.ToString());
                properties["skillId"] = output => WriteString(output, value.SkillId.ToString());
                properties["strainCost"] = output => output.Append(value.StrainCost);
                properties["strainResourceId"] = output => WriteString(output, value.StrainResourceId.ToString());
                properties["sustainCostPerTick"] = output => output.Append(value.SustainCostPerTick);
                properties["targetTags"] = output => WriteStrings(output, value.TargetTags);
                break;
            case TrainingProjectDefinition value:
                properties["facilityId"] = output => WriteString(output, value.FacilityId.ToString());
                properties["grantedFeatIds"] = output => WriteIds(output, value.GrantedFeatIds.Select(id => id.Value));
                properties["grantedTechniqueIds"] = output => WriteIds(output, value.GrantedTechniqueIds.Select(id => id.Value));
                properties["progressCap"] = output => output.Append(value.ProgressCap);
                properties["requiredSkillIds"] = output => WriteIds(output, value.RequiredSkillIds.Select(id => id.Value));
                properties["resourceCost"] = output => output.Append(value.ResourceCost);
                properties["resourceId"] = output => WriteString(output, value.ResourceId.ToString());
                properties["safetyId"] = output => WriteString(output, value.SafetyId.ToString());
                properties["workUnits"] = output => output.Append(value.WorkUnits);
                break;
        }

        builder.Append('{');
        bool first = true;
        foreach ((string name, Action<StringBuilder> write) in properties)
        {
            if (!first)
            {
                builder.Append(',');
            }

            first = false;
            WriteString(builder, name);
            builder.Append(':');
            write(builder);
        }

        builder.Append('}');
    }

    private static int CanonicalKindOrder(ContentDefinition definition) => definition switch
    {
        AccessDefinition => 0,
        AttributeDefinition => 1,
        BackgroundDefinition => 2,
        CharacterDefinition => 3,
        FeatDefinition => 4,
        HeritageDefinition => 5,
        RaceDefinition => 6,
        SkillDefinition => 7,
        PerkDefinition => 8,
        PsychicTechniqueDefinition => 9,
        SpellDefinition => 10,
        TechniqueDefinition => 11,
        TrainingProjectDefinition => 12,
        _ => throw new ArgumentOutOfRangeException(nameof(definition)),
    };

    private static void WriteStrings(StringBuilder builder, IEnumerable<string> values)
    {
        builder.Append('[');
        bool first = true;
        foreach (string value in values.Order(StringComparer.Ordinal))
        {
            if (!first)
            {
                builder.Append(',');
            }

            first = false;
            WriteString(builder, value);
        }

        builder.Append(']');
    }

    private static void WriteIds(StringBuilder builder, IEnumerable<ContentId> ids) =>
        WriteStrings(builder, ids.Select(id => id.ToString()));

    private static void WriteString(StringBuilder builder, string value)
    {
        builder.Append('"');
        foreach (Rune rune in value.EnumerateRunes())
        {
            switch (rune.Value)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case <= 0x1f:
                    builder.Append("\\u00").Append(rune.Value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                default:
                    builder.Append(rune);
                    break;
            }
        }

        builder.Append('"');
    }
}
