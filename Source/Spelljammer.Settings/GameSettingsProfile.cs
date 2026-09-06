using System.Text.Json.Serialization;

namespace Spelljammer.Settings;

public sealed record GameSettingsProfile(
    int SchemaVersion,
    int MasterVolume,
    int MusicVolume,
    int EffectsVolume,
    bool Subtitles,
    bool ReducedMotion,
    bool ScreenShake,
    int UiScalePercent)
{
    public const int CurrentSchemaVersion = 1;
    public const int MinimumVolume = 0;
    public const int MaximumVolume = 100;
    public const int MinimumUiScalePercent = 75;
    public const int MaximumUiScalePercent = 150;
    public const int MaximumSerializedBytes = 64 * 1024;

    public static GameSettingsProfile Default { get; } = new(
        CurrentSchemaVersion,
        80,
        65,
        80,
        true,
        false,
        true,
        100);

    [JsonIgnore]
    public bool IsValid =>
        SchemaVersion == CurrentSchemaVersion &&
        MasterVolume is >= MinimumVolume and <= MaximumVolume &&
        MusicVolume is >= MinimumVolume and <= MaximumVolume &&
        EffectsVolume is >= MinimumVolume and <= MaximumVolume &&
        UiScalePercent is >= MinimumUiScalePercent and <= MaximumUiScalePercent;
}

public enum GameSettingsDiagnostic : byte
{
    None,
    Missing,
    Corrupt,
    Oversized,
    Unsupported,
    InvalidValue,
    IoFailure,
}

public static class GameSettingsDiagnostics
{
    public static string Stable(GameSettingsDiagnostic diagnostic) => diagnostic switch
    {
        GameSettingsDiagnostic.None => string.Empty,
        GameSettingsDiagnostic.Missing => "settings.missing",
        GameSettingsDiagnostic.Corrupt => "settings.corrupt",
        GameSettingsDiagnostic.Oversized => "settings.oversized",
        GameSettingsDiagnostic.Unsupported => "settings.unsupported",
        GameSettingsDiagnostic.InvalidValue => "settings.value-invalid",
        GameSettingsDiagnostic.IoFailure => "settings.io-failure",
        _ => throw new ArgumentOutOfRangeException(nameof(diagnostic)),
    };
}

public sealed record GameSettingsReadResult(
    GameSettingsProfile Profile,
    bool Loaded,
    GameSettingsDiagnostic Diagnostic);

public sealed record GameSettingsApplyResult(
    GameSettingsProfile ActiveProfile,
    bool Applied,
    GameSettingsDiagnostic Diagnostic);
