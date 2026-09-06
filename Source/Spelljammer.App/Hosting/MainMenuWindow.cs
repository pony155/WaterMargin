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

        menuView = new SpriteForgeMainMenuView(strings);
        menuView.SettingsRequested += MenuView_SettingsRequested;
        menuView.QuitRequested += MenuView_QuitRequested;
        Content = menuView;
        Closed += Window_Closed;

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
            menuView.SetStatus(strings.Get("settings.status.saved"), isError: false);
            menuView.Focus();
        }
    }

    private static void MenuView_QuitRequested(object? sender, EventArgs e) =>
        Application.Current.Shutdown();

    private void Window_Closed(object? sender, EventArgs e)
    {
        menuView.SettingsRequested -= MenuView_SettingsRequested;
        menuView.QuitRequested -= MenuView_QuitRequested;
        menuView.Dispose();
        Closed -= Window_Closed;
    }
}
