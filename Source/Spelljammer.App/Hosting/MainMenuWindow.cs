using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Spelljammer.Presentation;
using Spelljammer.Settings;

namespace Spelljammer;

internal sealed class MainMenuWindow : Window
{
    private readonly GameSettingsRegistry settings;
    private readonly string settingsPath;
    private readonly GameText strings;
    private readonly SpriteForgeMainMenuView menuView;

    internal MainMenuWindow(
        GameSettingsRegistry settings,
        string settingsPath,
        GameSettingsDiagnostic startupDiagnostic,
        GameText strings)
    {
        this.settings = settings;
        this.settingsPath = settingsPath;
        this.strings = strings;
        Title = strings.Get("menu.window-title");
        Width = 1280;
        Height = 720;
        MinWidth = 960;
        MinHeight = 540;
        WindowState = WindowState.Maximized;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Black;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        menuView = new SpriteForgeMainMenuView(strings, GetApplicationVersion());
        menuView.SettingsRequested += MenuView_SettingsRequested;
        menuView.QuitRequested += MenuView_QuitRequested;
        Content = menuView;
        Closed += Window_Closed;
        ApplyResolution(settings.Active.Resolution);

        if (startupDiagnostic is not GameSettingsDiagnostic.None and not GameSettingsDiagnostic.Missing)
        {
            menuView.SetStatus(strings.Diagnostic(
                "settings.status.load-failed",
                GameSettingsDiagnostics.Stable(startupDiagnostic)), isError: true);
        }
    }

    private void MenuView_SettingsRequested(object? sender, EventArgs e)
    {
        GameSettingsDialog dialog = new(settings, settingsPath, strings) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            strings.SetLanguage(settings.Active.Language);
            Title = strings.Get("menu.window-title");
            menuView.RefreshLanguage();
            ApplyResolution(settings.Active.Resolution);
            menuView.SetStatus(strings.Get("settings.status.saved"), isError: false);
            menuView.Focus();
        }
    }

    private static void MenuView_QuitRequested(object? sender, EventArgs e) =>
        Application.Current.Shutdown();

    private static string GetApplicationVersion()
    {
        string version = typeof(App).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? throw new InvalidOperationException("The application version metadata is unavailable.");
        int buildMetadata = version.IndexOf('+', StringComparison.Ordinal);
        return buildMetadata >= 0 ? version[..buildMetadata] : version;
    }

    private void ApplyResolution(string resolutionId)
    {
        if (!GameSettingsChoices.TryGetResolution(resolutionId, out GameResolutionChoice resolution))
        {
            throw new InvalidOperationException($"Unsupported active display resolution '{resolutionId}'.");
        }

        if (resolution.IsDesktop)
        {
            WindowState = WindowState.Maximized;
            return;
        }

        WindowState = WindowState.Normal;
        Rect workArea = SystemParameters.WorkArea;
        Width = Math.Min(resolution.Width, workArea.Width);
        Height = Math.Min(resolution.Height, workArea.Height);
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Top + (workArea.Height - Height) / 2;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        menuView.SettingsRequested -= MenuView_SettingsRequested;
        menuView.QuitRequested -= MenuView_QuitRequested;
        menuView.Dispose();
        Closed -= Window_Closed;
    }
}
