using System.Windows;
using Spelljammer.Presentation;
using Spelljammer.Settings;

namespace Spelljammer;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        string settingsPath = GameSettingsPath.CurrentUser;
        (GameSettingsRegistry registry, GameSettingsDiagnostic diagnostic) =
            await Task.Run(() => GameSettingsRegistry.Load(settingsPath));
        GameSettingsStrings settingsStrings = GameSettingsStrings.Load();
        MainWindow window = new(registry, settingsPath, diagnostic, settingsStrings);
        MainWindow = window;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        window.Show();
    }
}
