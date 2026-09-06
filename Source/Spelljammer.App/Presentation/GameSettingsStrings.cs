using System.IO;
using System.Reflection;
using Spelljammer.Localization;

namespace Spelljammer.Presentation;

internal sealed class GameSettingsStrings
{
    private const string ResourceName = "Spelljammer.Localization.en-US.settings.sfloc";
    private readonly LocalizationService localization;

    private GameSettingsStrings(LocalizationService localization)
    {
        this.localization = localization;
    }

    internal static GameSettingsStrings Load()
    {
        Assembly assembly = typeof(GameSettingsStrings).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded localization artifact '{ResourceName}' is unavailable.");
        if (stream.Length > LocalizationLimits.MaximumArtifactBytes)
        {
            throw new InvalidOperationException("The embedded settings localization artifact exceeds its supported bound.");
        }

        byte[] artifact = new byte[stream.Length];
        stream.ReadExactly(artifact);
        Require(LocalizationArtifact.Decode(artifact, out LocalizationCatalog? catalog, out string error), error);

        LocalizationService service = new();
        Require(service.Initialize(new LocalizationConfig("en-US", RequiredNamespaces: ["settings"])),
            "Could not initialize the settings localization service.");
        LocaleId locale = LocaleId.Create("en-US");
        Require(service.StageLocale(locale, [catalog!], out LocaleGeneration? generation),
            "Could not stage the settings localization catalog.");
        Require(service.PublishLocale(generation!), "Could not publish the settings localization catalog.");
        return new GameSettingsStrings(service);
    }

    internal void BeginFrame() => Require(
        localization.BeginFormattingFrame(),
        "Could not begin the settings localization frame.");

    internal string Get(string name)
    {
        LocalizationStatus status = localization.GetStatic(LocalizationKey.Create(name), out LocalizedMessage? message);
        Require(status, $"Could not resolve localization key '{name}'.");
        return message!.Text;
    }

    internal string Format(string name, params LocalizationArgument[] arguments)
    {
        LocalizationStatus status = localization.Format(LocalizationKey.Create(name), arguments, out LocalizedMessage? message);
        Require(status, $"Could not format localization key '{name}'.");
        return message!.Text;
    }

    internal string Percent(int value) =>
        Format("settings.value.percent", LocalizationArgument.Integer("value", value));

    internal string Diagnostic(string name, string code) =>
        Format(name, LocalizationArgument.Text("code", code));

    private static void Require(LocalizationStatus status, string detail)
    {
        if (status != LocalizationStatus.Success)
        {
            throw new InvalidOperationException($"{detail} ({status}).");
        }
    }
}
