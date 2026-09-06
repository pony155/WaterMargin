# Spelljammer

Spelljammer is an in-development 2D outer-space sandbox roguelike built with
the [SpriteForge](https://github.com/pony155/SpriteForge) engine. The player
commands a small voidfaring ship, explores a seeded star chart, takes risks for
salvage, and tries to bring enough of the expedition home to finance the next
voyage.

Spelljammer is the requested working project name. The current implementation
draws inspiration from the broad fantasy of age-of-sail adventure among the
stars and from systemic roguelikes, while its universe, terminology,
characters, rules, content, code, artwork, and sound remain original.

> [!IMPORTANT]
> Spelljammer is at a playable-prototype stage, not a content-complete game.
> The current shell exposes a small deterministic expedition loop and a native
> renderer demonstration. Crew simulation, encounters, combat, trading,
> procedural ship interiors, saves, and a full game UI remain planned.

## Current prototype

Each new chart creates a deterministic 4 × 4 region from an explicit seed.
Travel consumes fuel and can damage the hull; time consumes supplies. A sector
can be salvaged only once, recovered cargo can patch the hull, and a successful
run requires returning to the free anchorage with at least eight cargo.

Implemented foundations include:

- a headless `Spelljammer.Simulation` project with immutable expedition state,
  typed commands, stable sector identities, explicit rejection reasons, bounded
  maps, and seed-derived hazards and rewards;
- a .NET 10, C# 14, Windows x64 WPF host that presents the expedition loop;
- a child Win32 viewport rendered through SpriteForge's native D3D12 sprite
  renderer and a narrow managed/native interop layer;
- versioned game-owned localization catalogs, typed message formatting,
  explicit fallback, pinned plural/number profiles, pseudo-locales, and offline
  catalog tooling; and
- compile-only simulation and localization contract targets for CI execution.

## Product direction

- **A ship is a home:** its hull, cargo space, modules, crew, and damage persist
  through a voyage and force meaningful tradeoffs.
- **The chart is a gamble:** routes reveal hazards, opportunities, factions,
  strange environments, and shortcuts one decision at a time.
- **Systems tell the story:** crew needs, ship failures, weather, pursuit,
  resources, and encounters combine without a prescribed plot.
- **Characters remain classless:** attributes shape broad capability while
  skills improve independently through use, instruction, and experience.
- **Retreat is a decision:** a modest return keeps a campaign alive; greed can
  strand a run in the void.
- **Runs are reproducible:** explicit seeds and command streams make simulation
  outcomes testable and debuggable.

These are product goals, not claims that every system is implemented. See
[`Docs/DesignConcept/Vision.md`](Docs/DesignConcept/Vision.md) and
[`Docs/DesignConcept/VerticalSlice.md`](Docs/DesignConcept/VerticalSlice.md). The planned
Arcane-Industrial setting, including dieselpunk and atompunk technology, is
defined in [`Docs/DesignConcept/Setting.md`](Docs/DesignConcept/Setting.md). The planned
crew races, heritages, physiology, and character-generation boundaries are
defined in [`Docs/DesignConcept/Races.md`](Docs/DesignConcept/Races.md). The
classless capability model is split into
[`Docs/DesignConcept/Attributes.md`](Docs/DesignConcept/Attributes.md) and
[`Docs/DesignConcept/Skills.md`](Docs/DesignConcept/Skills.md); Perk and Racial
Perk rules are defined in [`Docs/DesignConcept/Perks.md`](Docs/DesignConcept/Perks.md).
Planned personal weapons, armor, tools, and relics are defined in
[`Docs/DesignConcept/Equipments.md`](Docs/DesignConcept/Equipments.md).
Planned spellcasting rules,
the authored spell catalog, and psychic systems are defined in
[`Docs/DesignConcept/Spells.md`](Docs/DesignConcept/Spells.md), and
[`Docs/DesignConcept/PsychicAbilities.md`](Docs/DesignConcept/PsychicAbilities.md).
Ship engagements, boarding, ruin expeditions, EVA fighting, injuries, and
tactical resolution are defined in
[`Docs/DesignConcept/Battle.md`](Docs/DesignConcept/Battle.md). Planned ship frames,
modules, networks, damage, and refits are defined in
[`Docs/DesignConcept/Ships.md`](Docs/DesignConcept/Ships.md). The versioned procedural galaxy
graph, Starways, system generation, and discovery model are defined in
[`Docs/DesignConcept/GalaxyMap.md`](Docs/DesignConcept/GalaxyMap.md). Seeded random
events during interstellar travel are defined in
[`Docs/DesignConcept/Events.md`](Docs/DesignConcept/Events.md). Planned faction
membership, standing, diplomacy, territory, laws, markets, and conflict are
defined in [`Docs/DesignConcept/Factions.md`](Docs/DesignConcept/Factions.md). Optional
late-campaign threats, escalation, alternative resolutions, and aftermath are
defined in
[`Docs/DesignConcept/Endgame_Crisis.md`](Docs/DesignConcept/Endgame_Crisis.md).

## Repository layout

| Path | Purpose |
| --- | --- |
| `Source/Spelljammer.App/` | WPF host, expedition presentation, SpriteForge interop, and renderer viewport. |
| `Source/Spelljammer.Simulation/` | Headless authoritative space-expedition state and commands. |
| `Source/Spelljammer.Localization/` | Game-owned localization runtime and message formatter. |
| `Tools/Spelljammer.Localization.Compiler/` | Source-catalog compiler and validation tools. |
| `Content/Localization/` | Authored catalogs and pinned locale-data notices. |
| `Tests/Spelljammer.Simulation.Tests/` | Compile-only deterministic simulation contracts. |
| `Tests/Spelljammer.Localization.Tests/` | Compile-only localization contracts. |
| `Tests/Spelljammer.Content.Tests/` | Frozen fixtures for the planned gameplay content loader; no test project yet. |
| `Docs/DesignConcept/` | Current vision and playable-slice scope. |
| `Docs/Architecture/` | Implemented and planned subsystem boundaries. |
| `Docs/Archive/` | Historical explorations; not current product authority. |
| `Build/` | Focused CMake declarations included by the root project. |

Planned implementation contracts are documented in
[`Docs/Architecture/ContentPacks.md`](Docs/Architecture/ContentPacks.md),
[`Docs/Architecture/CharacterCapabilities.md`](Docs/Architecture/CharacterCapabilities.md),
[`Docs/Architecture/Modding.md`](Docs/Architecture/Modding.md), and
[`Docs/Architecture/ImplementationRoadmap.md`](Docs/Architecture/ImplementationRoadmap.md).
The frozen version 1 identity, serialization, bounds, and diagnostic contracts
are in
[`Docs/Architecture/ContentContractsV1.md`](Docs/Architecture/ContentContractsV1.md)
and
[`Docs/Architecture/ContentLimitsAndDiagnostics.md`](Docs/Architecture/ContentLimitsAndDiagnostics.md).

## Build and run

Install the .NET 10 SDK and build the managed solution:

```powershell
dotnet build .\Spelljammer.slnx
```

The WPF host also needs a built sibling SpriteForge checkout. Supply its native
output without committing a developer-specific path:

```powershell
$env:SPRITEFORGE_ROOT = (Resolve-Path ..\SpriteForge)
$nativeDir = Join-Path $env:SPRITEFORGE_ROOT 'build\windows-msvc-debug\release\bin'

dotnet build .\Spelljammer.slnx -p:SpriteForgeNativeDir="$nativeDir"
dotnet run --project .\Source\Spelljammer.App\Spelljammer.App.csproj `
    -p:SpriteForgeNativeDir="$nativeDir"
```

Follow SpriteForge's README to configure and install the native engine. The
current host is Windows-only even though reusable SpriteForge components may
target other platforms.

## Localization

`en-US` is the current source locale. Compile a source catalog with:

```powershell
dotnet run --project .\Tools\Spelljammer.Localization.Compiler\Spelljammer.Localization.Compiler.csproj -- `
    compile `
    .\Content\Localization\en-US\core.sfloc.json `
    .\out\Localization\en-US\core.sfloc
```

See [`Source/Spelljammer.Localization/README.md`](Source/Spelljammer.Localization/README.md)
for catalog syntax and runtime behavior.

## Contributing

Keep game rules and content in Spelljammer and reusable engine behavior in
SpriteForge. Authoritative state must remain independent of WPF, renderer
handles, localized strings, frame rate, and wall-clock timing. Describe roadmap
work as planned until source and verification exist.
