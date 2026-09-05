# WaterMargin product vision

## Status

This document defines product direction. The repository currently contains
application-host, renderer-interop, and localization foundations; it does not
yet contain the colony simulation described here.

## Vision

WaterMargin is an original 2D colony sandbox in which understandable systems
combine into unexpected problems, recoveries, and stories. The player guides a
settlement rather than controlling every action directly: priorities,
available work, colonist capabilities, relationships, resources, and the
environment determine what happens next.

The project is inspired by the systemic possibility space of colony games such
as RimWorld, but must establish its own setting, terminology, mechanics,
balance, interface, content, art, and sound.

## Pillars

- **Systemic stories:** outcomes emerge from interacting rules and persistent
  state rather than a fixed script.
- **Readable decisions:** players can understand why a colonist chose an action
  and what prevents work from progressing.
- **Settlement continuity:** construction, resources, health, relationships,
  and environmental consequences persist and remain save-compatible.
- **Player expression:** priorities and layout create multiple viable ways to
  stabilize and grow a colony.
- **Data-driven growth:** stable IDs and validated definitions support content
  iteration, balancing, migration, and eventual modding.
- **Clean engine boundary:** game rules stay in WaterMargin; reusable runtime
  capabilities stay in SpriteForge.

## Product principles

- Prefer a small, complete simulation loop over many disconnected systems.
- Keep authoritative simulation deterministic and independent from rendering
  frame rate, localized text, and UI timing.
- Expose reasons and consequences through UI instead of hiding important rules.
- Treat failure and recovery as playable states, not merely game-over triggers.
- Describe roadmap work as planned until it is implemented and verified.
