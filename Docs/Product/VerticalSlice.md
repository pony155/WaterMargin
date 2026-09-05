# First playable vertical slice

## Feature status

- [x] Windows application host and SpriteForge renderer prototype
- [x] Game-owned localization runtime and catalog compiler
- [ ] Deterministic colony simulation project and fixed-tick host integration
- [ ] Small authored map with persistent stable identities
- [ ] Colonist needs, capabilities, and inspectable work priorities
- [ ] Gather, haul, store, construct, eat, and rest job loop
- [ ] Minimal resource stockpile and construction flow
- [ ] Save/load round trip with a versioned format
- [ ] Localized status, inspection, and command UI
- [ ] Headless deterministic scenario coverage

## Goal

The first slice should prove one complete colony loop rather than the breadth of
the final game. A few colonists begin on a small authored map, gather and haul a
basic resource, construct a useful object, satisfy hunger and rest needs, and
continue correctly after save/load.

## Required boundaries

- `WaterMargin.Simulation` owns authoritative state and does not depend on WPF,
  renderer handles, or localized strings.
- The WPF application translates player intent into typed commands and renders
  committed simulation snapshots through SpriteForge.
- Every work decision exposes a stable reason suitable for debugging and
  localized presentation.
- Random choices use owned seeded streams; replaying the same command sequence
  from the same initial state produces the same authoritative result.
- Save data uses versioned stable IDs and validates completely before replacing
  the active world.

## Explicitly deferred

Procedural world generation, combat, factions, diplomacy, multiplayer, broad
mod support, complex health simulation, and production-scale content are not
required for the first playable slice.
