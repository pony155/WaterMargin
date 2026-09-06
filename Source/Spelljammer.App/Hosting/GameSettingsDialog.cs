using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Spelljammer.Presentation;
using Spelljammer.Settings;

namespace Spelljammer;

internal sealed class GameSettingsDialog : Window
{
    private readonly GameSettingsRegistry registry;
    private readonly string settingsPath;
    private readonly SpriteForgeSettingsView settingsView;
    private bool applyInProgress;

    internal GameSettingsDialog(
        GameSettingsRegistry registry,
        string settingsPath,
        GameSettingsStrings strings)
    {
        this.registry = registry;
        this.settingsPath = settingsPath;
        Title = strings.Get("settings.title.window");
        Width = 820;
        Height = 680;
        MinWidth = 660;
        MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.CanResize;
        Background = new SolidColorBrush(Color.FromRgb(9, 13, 24));
        ShowInTaskbar = false;
        settingsView = new SpriteForgeSettingsView(registry.Active, strings);
        settingsView.ApplyRequested += SettingsView_ApplyRequested;
        settingsView.CancelRequested += SettingsView_CancelRequested;
        Content = settingsView;
        Closing += Dialog_Closing;
        Closed += Dialog_Closed;
    }

    private async void SettingsView_ApplyRequested(object? sender, GameSettingsApplyRequestedEventArgs e)
    {
        if (applyInProgress)
        {
            return;
        }

        applyInProgress = true;
        settingsView.SetBusy();
        GameSettingsApplyResult result = await Task.Run(() => registry.Apply(settingsPath, e.Profile));
        applyInProgress = false;
        if (!result.Applied)
        {
            settingsView.SetApplyFailure(result.Diagnostic);
            return;
        }

        DialogResult = true;
    }

    private void SettingsView_CancelRequested(object? sender, EventArgs e) => DialogResult = false;

    private void Dialog_Closing(object? sender, CancelEventArgs e)
    {
        if (applyInProgress)
        {
            e.Cancel = true;
        }
    }

    private void Dialog_Closed(object? sender, EventArgs e)
    {
        settingsView.ApplyRequested -= SettingsView_ApplyRequested;
        settingsView.CancelRequested -= SettingsView_CancelRequested;
        settingsView.Dispose();
        Closing -= Dialog_Closing;
        Closed -= Dialog_Closed;
    }
}
