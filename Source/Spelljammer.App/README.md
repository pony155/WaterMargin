# Spelljammer.App source briefing

`Spelljammer.App` is the Windows x64 .NET 10 WPF executable and the current
application entry point. It owns window lifetime, presentation orchestration,
and the narrow managed/native bridge to SpriteForge. Authoritative gameplay,
settings persistence, and localization behavior remain in their respective
game-owned class libraries.

This briefing covers authored project and source files. Generated `bin/` and
`obj/` files are build output and are not part of the source inventory.

## Current startup path

```text
App.OnStartup
  -> load GameSettingsRegistry on a worker thread
  -> load embedded menu/settings catalogs through GameText
  -> create MainMenuWindow
       -> SpriteForgeMainMenuView
            -> Game Settings -> GameSettingsDialog
                                  -> SpriteForgeSettingsView
                                  -> GameSettingsRegistry.Apply
            -> Quit Game     -> Application.Shutdown
```

The application currently opens the main menu. The earlier expedition window
and renderer viewport remain in the project as prototype code, but no current
menu action constructs them.

## File inventory

### Project root

| File | Responsibility |
| --- | --- |
| `Spelljammer.App.csproj` | Declares the WPF `WinExe`, .NET 10 Windows target, x64 platform, project references, embedded localization catalogs, and the packaged main-menu background. Its pre-compile target builds the `menu` and `settings` source catalogs into bounded `.sfloc` artifacts. Setting `CopySpriteForgeNativeRuntime` activates the repository-level target that copies SpriteForge DLLs from `SpriteForgeNativeDir`. |
| `App.xaml` | Declares the WPF application type and application resource scope. It deliberately has no `StartupUri`; startup is orchestrated in code so settings can be loaded before a window is published. |
| `App.xaml.cs` | The executable startup boundary. It temporarily uses explicit shutdown mode, resolves the per-user settings path, loads the settings registry away from the UI thread, loads localized application text, creates `MainMenuWindow`, and then makes that window the normal shutdown owner. |
| `AssemblyInfo.cs` | Configures WPF theme-resource lookup. There is no theme-specific dictionary; fallback resources are resolved from the source assembly. |

### `Hosting/`

| File | Responsibility |
| --- | --- |
| `MainMenuWindow.cs` | Current top-level window and application flow coordinator. It owns the shared settings registry, settings path, and `GameText`; hosts `SpriteForgeMainMenuView`; opens the modal settings dialog; forwards save/load status to the menu; shuts down on Quit; and detaches/disposes the view when closed. |
| `GameSettingsDialog.cs` | WPF modal shell around `SpriteForgeSettingsView`. It starts settings publication on a worker thread, prevents duplicate Apply operations and closure during a write, reports failures without replacing active settings, and returns `DialogResult = true` only after successful durable publication. |
| `MainWindow.xaml` | Retained expedition-prototype layout. It defines resource/fuel/hull displays, movement and voyage commands, an `EngineViewport`, and renderer playback controls. Most of its player-visible strings predate the current localization boundary. This window is not reachable from the current main menu. |
| `MainWindow.xaml.cs` | Retained expedition-prototype coordinator. It owns an `ExpeditionSimulation` and current `ExpeditionState`, converts button clicks into typed commands, presents sector/resource results, controls the renderer animation, and can open the shared settings dialog. It is currently inactive because startup creates `MainMenuWindow` instead. |

### `Presentation/`

| File | Responsibility |
| --- | --- |
| `GameText.cs` | Application localization facade. It reads the embedded `en-US` menu and settings artifacts with size bounds, decodes and stages both catalogs transactionally, publishes the locale, begins formatting frames, and exposes small helpers for static text, formatted values, percentages, and stable diagnostics. |
| `SpriteForgeMainMenuView.cs` | Current main-menu surface. It loads the packaged `Background.png`, draws it with aspect-preserving cover scaling, and uses a fixed 1280x720 logical UI canvas. SpriteForge owns retained elements, hit testing, focus, pointer/keyboard processing, and action generation; WPF realizes copied solid presentation commands and localized text. The view emits only `SettingsRequested` and `QuitRequested`, keeps bounded element/action buffers, and releases its native UI context deterministically. |
| `SpriteForgeSettingsView.cs` | Interactive settings surface and draft editor on an 800x640 logical canvas. It defines SpriteForge sliders, toggles, buttons, modal focus behavior, and accessibility names for audio and interface preferences. Native actions update an immutable draft profile or emit Apply/Cancel events; WPF draws the copied presentation, value labels, focus outlines, and status messages. Reset reconstructs the native document from defaults. |
| `EngineViewport.cs` | Retained expedition renderer host. It derives from `HwndHost`, creates a child Win32 window, checks SpriteForge rendering ABI v1, creates a renderer and an in-memory RGBA sprite sheet, submits bounded sprite draws, and advances a four-frame animation with a WPF render-priority timer. It owns and destroys the native texture, renderer, and child window. It is only used by the inactive `MainWindow`. |

### `Interop/`

| File | Responsibility |
| --- | --- |
| `SpriteForgeNative.cs` | Sole low-level SpriteForge P/Invoke boundary for this application. It mirrors renderer and UI status values, blittable camera/draw/UI structures, input/action/snapshot/presentation types, and the C ABI entry points used by the views. Its static initializer verifies managed UI structure sizes before the first native call. Ownership stays with the calling view: renderer and UI handles must be destroyed on their owner thread, copied arrays are bounded, and no native pointer enters game or save state. |

## Runtime and ownership boundaries

| Concern | Owner in the current application |
| --- | --- |
| WPF startup and window lifetime | `App`, `MainMenuWindow`, `GameSettingsDialog` |
| Main-menu and settings interaction state | SpriteForge UI documents, accessed through `SpriteForgeNative` |
| Raster/text realization | WPF drawing in the two SpriteForge view adapters |
| Localized menu/settings text | `GameText` over `Spelljammer.Localization` |
| Active settings and durable publication | `Spelljammer.Settings` through `GameSettingsRegistry` |
| Authoritative expedition state | `Spelljammer.Simulation`, currently hosted only by inactive prototype code |
| Native sprite rendering | SpriteForge through `EngineViewport`, currently inactive |

The WPF views translate physical pointer coordinates into their logical canvas,
send copied input records to SpriteForge, consume bounded action arrays, and
then invalidate their WPF rendering. They never use translated labels as
command identity: stable numeric keys identify controls, while localized text
is presentation and accessibility data.

## Build-time inputs outside this directory

`Spelljammer.App.csproj` deliberately links rather than duplicates these files:

- `Content/Packs/base/Assets/UI/MainMenu/Background.png` becomes the WPF pack
  resource `Assets/UI/MainMenu/Background.png`.
- `Content/Packs/base/Localization/en-US/menu.sfloc.json` is compiled and
  embedded as `Spelljammer.Localization.en-US.menu.sfloc`.
- `Content/Packs/base/Localization/en-US/settings.sfloc.json` is compiled and
  embedded as `Spelljammer.Localization.en-US.settings.sfloc`.
- SpriteForge native DLLs come from the configurable `SpriteForgeNativeDir`;
  no developer-specific absolute engine path belongs in the project.

## Where changes belong

- Add or change top-level window flow under `Hosting/`.
- Add WPF realization or game-specific UI adapters under `Presentation/`.
- Extend `SpriteForgeNative.cs` only for a deliberate, versioned public
  SpriteForge C ABI addition.
- Put reusable rendering, platform, input, or UI capabilities in SpriteForge,
  not in this application project.
- Put settings validation/storage, localization runtime behavior, and gameplay
  simulation in their existing class libraries rather than in a window.
- Add player-visible menu or settings wording to the authored localization
  catalogs, not as presentation literals.

## Build and launch

From the repository root:

```powershell
$env:SPRITEFORGE_ROOT = (Resolve-Path ..\SpriteForge)
$nativeDir = Join-Path $env:SPRITEFORGE_ROOT 'build\windows-msvc-debug\release\bin'

dotnet build .\Spelljammer.slnx -p:SpriteForgeNativeDir="$nativeDir"
dotnet run --project .\Source\Spelljammer.App\Spelljammer.App.csproj `
    -p:SpriteForgeNativeDir="$nativeDir"
```

