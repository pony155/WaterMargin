# WaterMargin

WaterMargin is an in-development 2D colony and sandbox simulation built with
the [SpriteForge](https://github.com/pony155/SpriteForge) engine. Its direction is inspired by the
systemic storytelling and settlement-management possibilities of games such as
RimWorld, while using original rules, setting, content, code, and artwork.

> [!IMPORTANT]
> WaterMargin is in its foundation stage. The repository currently provides a
> Windows WPF rendering host and a substantial localization foundation; it is
> not yet a playable sandbox game.

## Game direction

WaterMargin aims to create stories through interacting simulation systems
rather than a fixed sequence of scripted outcomes. The intended pillars are:

- colonists with needs, skills, work priorities, relationships, and lasting
  consequences;
- a persistent settlement shaped by construction, production, resources,
  environment, threats, and recovery;
- understandable systems that let players plan, automate, specialize, and
  improvise;
- deterministic, saveable simulation with stable identities and data-driven
  content; and
- a moddable foundation that keeps gameplay rules separate from reusable
  engine services.

These are product goals, not claims about implemented features.

## Current status

Implemented foundations include:

- a .NET 10, C# 14, Windows x64 WPF application shell;
- a child Win32 viewport that renders through the native SpriteForge D3D12
  sprite renderer;
- a narrow managed/native renderer interop layer;
- a deterministic astronomical Chinese calendar for years 960 through 1644,
  including lunar/civil conversion, leap months, solar terms, and sexagenary
  year identities;
- versioned Game-owned localization catalogs, typed message formatting,
  explicit locale fallback, pinned plural/number profiles, pseudo-locales, and
  offline catalog tooling; and
- compile-time localization test coverage and deterministic catalog fixtures.

Colony simulation, world generation, colonist behavior, jobs, construction,
saves, and a complete game UI remain future work.

## Technology

- .NET 10 and C# 14
- WPF for the current Windows application host
- SpriteForge native engine and D3D12 renderer
- MIT-licensed Astronomy Engine, pinned and vendored for offline calendar builds
- CMake integration for localization tooling
- UTF-8 source catalogs compiled into bounded deterministic artifacts

SpriteForge is maintained in a separate repository. The expected sibling
checkout layout is:

```text
development/
├── SpriteForge/    Native engine and framework
└── WaterMargin/    Game, content, and game-owned tooling
```

Do not commit an absolute local engine path; provide it as a build property or
environment variable.

## Repository layout

| Path | Purpose |
| --- | --- |
| `Source/WaterMargin.App/` | Current WPF application, SpriteForge interop, and renderer presentation. |
| `Source/WaterMargin.Calendar/` | Ancient Chinese calendar rules and stable calendar-domain types. |
| `Source/WaterMargin.Localization/` | Game-owned localization runtime and message formatter. |
| `Tools/WaterMargin.Localization.Compiler/` | Source-catalog compiler and validation tools. |
| `Content/Localization/` | Authored catalogs and pinned locale-data notices. |
| `Tests/WaterMargin.Localization.Tests/` | Localization contract test project. |
| `Tests/WaterMargin.Calendar.Tests/` | Calendar invariant and conversion test project. |
| `ThirdParty/AstronomyEngine/` | Pinned MIT astronomy implementation used by the calendar. |
| `Docs/Product/` | Product vision and first playable vertical slice. |
| `Docs/Architecture/` | Current subsystem designs. |
| `Docs/Archive/` | Historical design explorations. |
| `Build/` | Focused CMake declarations included by the root project. |

## Prerequisites

For the current Windows host, install:

- the .NET 10 SDK;
- Visual Studio Build Tools with Desktop development with C++ and a Windows SDK;
- CMake 3.21 or newer, Ninja, and Python 3 for SpriteForge generation; and
- the dependencies required by the sibling SpriteForge checkout.

Follow SpriteForge's own README for its complete dependency and platform setup.

## Build and run

From `WaterMargin`, define the sibling engine root and build SpriteForge first:

```powershell
$env:SPRITEFORGE_ROOT = (Resolve-Path ..\SpriteForge)

Push-Location $env:SPRITEFORGE_ROOT
.\build\generate_projects.bat `
    --preset buildtools\presets\windows-msvc.xml `
    --generator Ninja `
    --build-type debug
cmake --build .\build\windows-msvc-debug --target install --parallel
Pop-Location
```

Build and run WaterMargin while pointing MSBuild at the installed native
engine binaries:

```powershell
$nativeDir = Join-Path $env:SPRITEFORGE_ROOT 'build\windows-msvc-debug\release\bin'
dotnet build .\WaterMargin.slnx -p:SpriteForgeNativeDir="$nativeDir"
dotnet run --project .\Source\WaterMargin.App\WaterMargin.App.csproj `
    -p:SpriteForgeNativeDir="$nativeDir"
```

The native DLLs are copied beside the managed output. The current host is
Windows-only even though reusable SpriteForge engine components target other
platforms.

## Localization

`en-US` is the current source locale. Development pseudo-locales help expose
hard-coded text, clipping, and expansion issues. Compile a source catalog with:

```powershell
dotnet run --project .\Tools\WaterMargin.Localization.Compiler\WaterMargin.Localization.Compiler.csproj -- `
    compile `
    .\Content\Localization\en-US\core.sfloc.json `
    .\out\Localization\en-US\core.sfloc
```

See
[`Source/WaterMargin.Localization/README.md`](Source/WaterMargin.Localization/README.md)
for catalog syntax, runtime limits, formatting behavior, and tooling commands.

## Ancient Chinese calendar

`WaterMargin.Calendar` provides an astronomical Chinese lunisolar calendar for
Chinese years 960 through 1644. It uses a fixed UTC+08:00 day boundary and does
not inspect the host timezone or culture. Calendar values are stable enums and
numbers; Chinese or translated display names belong in localization catalogs.

```csharp
using WaterMargin.Calendar;

AncientChineseCalendar calendar = new();
ChineseCalendarYear year = calendar.GetYear(1120);
HistoricalDate civilDate = calendar.ToCivilDate(new ChineseDate(1120, 1, 1));
ChineseDate lunarDate = calendar.FromCivilDate(civilDate);
IReadOnlyList<SolarTermOccurrence> terms = calendar.GetSolarTerms(1120);
```

The implementation projects astronomical Chinese-calendar rules backward; it
is suitable as deterministic game chronology, not as a claim that every date
matches a particular Northern Song official almanac. Its comparison civil date
uses the Julian calendar through 1582-10-04 and the Gregorian calendar from
1582-10-15 to remain compatible with the reference project's date convention.
See [`Docs/Architecture/AncientChineseCalendar.md`](Docs/Architecture/AncientChineseCalendar.md).

## Documentation

- [`Docs/Product/Vision.md`](Docs/Product/Vision.md) defines the current
  colony-sandbox direction.
- [`Docs/Product/VerticalSlice.md`](Docs/Product/VerticalSlice.md) scopes the
  first complete playable simulation loop.
- [`Docs/Architecture/Localization.md`](Docs/Architecture/Localization.md) describes the
  implemented localization phases and planned production workflow.
- [`Docs/Architecture/AncientChineseCalendar.md`](Docs/Architecture/AncientChineseCalendar.md)
  defines the implemented calendar rules, public API, and historical limits.
- [`Docs/Archive/LargeScaleRtsConcept.md`](Docs/Archive/LargeScaleRtsConcept.md) is an earlier RTS design
  exploration. It has not yet been revised for WaterMargin's colony-sandbox
  direction and should not be treated as the current gameplay specification.
- [`AGENTS.md`](AGENTS.md) defines repository ownership, architecture, and
  verification rules for contributors and coding agents.

## Contributing

Keep game rules and content in WaterMargin and reusable engine behavior in
SpriteForge. Changes that cross the managed/native boundary should document the
required interop version and remain independently buildable in both
repositories. Do not claim roadmap features as implemented until source,
integration, and appropriate verification exist.
