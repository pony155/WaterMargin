using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spelljammer.Settings;

public static class GameSettingsCodec
{
    private static readonly string[] RequiredProperties =
    [
        "schemaVersion",
        "masterVolume",
        "musicVolume",
        "effectsVolume",
        "subtitles",
        "reducedMotion",
        "screenShake",
        "uiScalePercent",
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 8,
        WriteIndented = true,
    };

    public static byte[] Encode(GameSettingsProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!profile.IsValid)
        {
            throw new ArgumentException("Game settings contain an unsupported schema or invalid value.", nameof(profile));
        }

        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(profile, JsonOptions);
        if (bytes.Length > GameSettingsProfile.MaximumSerializedBytes)
        {
            throw new InvalidOperationException("Game settings exceed the serialized byte limit.");
        }

        return bytes;
    }

    public static GameSettingsReadResult Decode(ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length > GameSettingsProfile.MaximumSerializedBytes)
        {
            return Failed(GameSettingsDiagnostic.Oversized);
        }

        if (bytes.IsEmpty)
        {
            return Failed(GameSettingsDiagnostic.Corrupt);
        }

        try
        {
            RejectDuplicateProperties(bytes.Span);
            GameSettingsProfile? profile = JsonSerializer.Deserialize<GameSettingsProfile>(bytes.Span, JsonOptions);
            if (profile is null)
            {
                return Failed(GameSettingsDiagnostic.Corrupt);
            }

            if (profile.SchemaVersion != GameSettingsProfile.CurrentSchemaVersion)
            {
                return Failed(GameSettingsDiagnostic.Unsupported);
            }

            return profile.IsValid
                ? new GameSettingsReadResult(profile, true, GameSettingsDiagnostic.None)
                : Failed(GameSettingsDiagnostic.InvalidValue);
        }
        catch (JsonException)
        {
            return Failed(GameSettingsDiagnostic.Corrupt);
        }
        catch (InvalidOperationException)
        {
            return Failed(GameSettingsDiagnostic.Corrupt);
        }
    }

    private static void RejectDuplicateProperties(ReadOnlySpan<byte> json)
    {
        Utf8JsonReader reader = new(json, new JsonReaderOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 8,
        });
        Stack<HashSet<string>> objects = new();
        HashSet<string>? rootProperties = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                HashSet<string> properties = new(StringComparer.Ordinal);
                if (objects.Count == 0)
                {
                    rootProperties = properties;
                }

                objects.Push(properties);
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                objects.Pop();
            }
            else if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string property = reader.GetString() ?? throw new JsonException();
                if (objects.Count == 0 || !objects.Peek().Add(property))
                {
                    throw new JsonException("Duplicate settings property.");
                }
            }
        }

        if (rootProperties is null || !rootProperties.SetEquals(RequiredProperties))
        {
            throw new JsonException("Settings document does not contain the complete schema.");
        }
    }

    private static GameSettingsReadResult Failed(GameSettingsDiagnostic diagnostic) =>
        new(GameSettingsProfile.Default, false, diagnostic);
}
