using System.IO;
using System.Reflection;
using Spelljammer.Localization;

namespace Spelljammer.Presentation;

internal sealed class GameText
{
    private static readonly string[] ResourceNames =
    [
        "Spelljammer.Localization.en-US.menu.sfloc",
        "Spelljammer.Localization.en-US.settings.sfloc",
    ];

    private readonly LocalizationService localization;

    private GameText(LocalizationService localization)
    {
        this.localization = localization;
    }

    internal static GameText Load()
    {
        Assembly assembly = typeof(GameText).Assembly;
        List<LocalizationCatalog> catalogs = new(ResourceNames.Length);
        foreach (string resourceName in ResourceNames)
        {
            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded localization artifact '{resourceName}' is unavailable.");
            if (stream.Length > LocalizationLimits.MaximumArtifactBytes)
            {
                throw new InvalidOperationException(
                    $"Embedded localization artifact '{resourceName}' exceeds its supported bound.");
            }

            byte[] artifact = new byte[stream.Length];
            stream.ReadExactly(artifact);
            Require(LocalizationArtifact.Decode(
                artifact,
                out LocalizationCatalog? catalog,
                out string error), error);
            catalogs.Add(catalog!);
        }

        LocalizationService service = new();
        Require(service.Initialize(new LocalizationConfig(
            "en-US",
            RequiredNamespaces: ["menu", "settings"])),
            "Could not initialize the application localization service.");
        LocaleId locale = LocaleId.Create("en-US");
        Require(service.StageLocale(locale, catalogs, out LocaleGeneration? generation),
            "Could not stage the application localization catalogs.");
        Require(service.PublishLocale(generation!), "Could not publish the application localization catalogs.");
        return new GameText(service);
    }

    internal void BeginFrame() => Require(
        localization.BeginFormattingFrame(),
        "Could not begin the application localization frame.");

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
