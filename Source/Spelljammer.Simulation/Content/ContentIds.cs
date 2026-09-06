using System.Diagnostics.CodeAnalysis;

namespace Spelljammer.Simulation.Content;

public readonly record struct ContentId : IComparable<ContentId>
{
    public const int MaximumLength = 127;

    public ContentId(string value)
    {
        if (!IsCanonical(value))
        {
            throw new ArgumentException("Content ID does not use the canonical lowercase ASCII grammar.", nameof(value));
        }

        Value = value;
    }

    public string? Value { get; }

    public bool IsValid => Value is not null;

    public int CompareTo(ContentId other) => StringComparer.Ordinal.Compare(Value, other.Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool TryParse(string? value, out ContentId id)
    {
        if (IsCanonical(value))
        {
            id = new ContentId(value);
            return true;
        }

        id = default;
        return false;
    }

    public static bool IsCanonical([NotNullWhen(true)] string? value)
    {
        if (value is null || value.Length is < 3 or > MaximumLength)
        {
            return false;
        }

        int segments = 1;
        bool segmentStart = true;
        bool afterHyphen = false;
        foreach (char character in value)
        {
            if (character == '.')
            {
                if (segmentStart || afterHyphen || ++segments > 8)
                {
                    return false;
                }

                segmentStart = true;
                afterHyphen = false;
                continue;
            }

            if (segmentStart)
            {
                if (character is < 'a' or > 'z')
                {
                    return false;
                }

                segmentStart = false;
                continue;
            }

            if (character == '-')
            {
                if (afterHyphen)
                {
                    return false;
                }

                afterHyphen = true;
                continue;
            }

            if (character is not (>= 'a' and <= 'z' or >= '0' and <= '9'))
            {
                return false;
            }

            afterHyphen = false;
        }

        return segments >= 2 && !segmentStart && !afterHyphen;
    }
}

public readonly record struct AttributeId : IComparable<AttributeId>
{
    public AttributeId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "attribute.");
    public AttributeId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(AttributeId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out AttributeId id) => TypedContentId.TryParse(value, "attribute.", out id);
}

public readonly record struct SkillId : IComparable<SkillId>
{
    public SkillId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "skill.");
    public SkillId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(SkillId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out SkillId id) => TypedContentId.TryParse(value, "skill.", out id);
}

public readonly record struct AccessId : IComparable<AccessId>
{
    public AccessId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "access.");
    public AccessId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(AccessId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out AccessId id) => TypedContentId.TryParse(value, "access.", out id);
}

public readonly record struct FeatId : IComparable<FeatId>
{
    public FeatId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "feat.");
    public FeatId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(FeatId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out FeatId id) => TypedContentId.TryParse(value, "feat.", out id);
}

public readonly record struct PerkId : IComparable<PerkId>
{
    public PerkId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "perk.");
    public PerkId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(PerkId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out PerkId id) => TypedContentId.TryParse(value, "perk.", out id);
}

public readonly record struct RaceId : IComparable<RaceId>
{
    public RaceId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "race.");
    public RaceId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(RaceId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out RaceId id) => TypedContentId.TryParse(value, "race.", out id);
}

public readonly record struct CharacterId : IComparable<CharacterId>
{
    public CharacterId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "character.");
    public CharacterId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(CharacterId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out CharacterId id) => TypedContentId.TryParse(value, "character.", out id);
}

public readonly record struct HeritageId : IComparable<HeritageId>
{
    public HeritageId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "heritage.");
    public HeritageId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(HeritageId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out HeritageId id) => TypedContentId.TryParse(value, "heritage.", out id);
}

public readonly record struct BackgroundId : IComparable<BackgroundId>
{
    public BackgroundId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "background.");
    public BackgroundId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(BackgroundId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out BackgroundId id) => TypedContentId.TryParse(value, "background.", out id);
}

public readonly record struct TrainingProjectId : IComparable<TrainingProjectId>
{
    public TrainingProjectId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "training.");
    public TrainingProjectId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(TrainingProjectId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out TrainingProjectId id) => TypedContentId.TryParse(value, "training.", out id);
}

public readonly record struct TechniqueId : IComparable<TechniqueId>
{
    public TechniqueId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "technique.");
    public TechniqueId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(TechniqueId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static bool TryParse(string? value, out TechniqueId id)
    {
        return TypedContentId.TryParse(value, "technique.", out id);
    }
}

public readonly record struct ScenarioId : IComparable<ScenarioId>
{
    public ScenarioId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "scenario.");
    public ScenarioId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(ScenarioId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

public readonly record struct ActionId : IComparable<ActionId>
{
    public ActionId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "action.");
    public ActionId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(ActionId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

public readonly record struct ResourceId : IComparable<ResourceId>
{
    public ResourceId(ContentId value) => Value = TypedContentId.RequirePrefix(value, "resource.");
    public ResourceId(string value) : this(new ContentId(value)) { }
    public ContentId Value { get; }
    public bool IsValid => Value.IsValid;
    public int CompareTo(ResourceId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

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

file static class TypedContentId
{
    public static ContentId RequirePrefix(ContentId value, string prefix) =>
        value.IsValid && value.ToString().StartsWith(prefix, StringComparison.Ordinal)
            ? value
            : throw new ArgumentException($"Content ID must begin with '{prefix}'.", nameof(value));

    public static bool TryParse<T>(string? value, string prefix, out T id)
        where T : struct
    {
        if (ContentId.TryParse(value, out ContentId parsed) && parsed.ToString().StartsWith(prefix, StringComparison.Ordinal))
        {
            id = (T)Activator.CreateInstance(typeof(T), parsed)!;
            return true;
        }

        id = default;
        return false;
    }
}
