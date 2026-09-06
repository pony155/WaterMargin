# Game settings architecture

## Implemented boundary

`Spelljammer.Settings` owns the first versioned local player-preference
profile. It is headless and has no WPF, SpriteForge, localization, native
pointer, or authoritative campaign dependency. The active profile contains
bounded master, music, and effects volumes, subtitles, reduced motion, screen
shake, and interface scale.

The WPF host owns startup loading and the modal lifecycle. The dialog delegates
its retained document, fixed logical layout, focus, modal trapping,
pointer/keyboard interaction, sliders, toggles, buttons, and typed action queue
to SpriteForge UI. The game maps copied stable actions to a draft profile and
publishes that draft only after durable persistence succeeds.

The integration requires SpriteForge UI interop version 1 from
`Engine/Public/SpriteForgeUIInterop.h`. The ABI is deliberately generic; no
Spelljammer setting name or rule is defined in the engine. Native calls remain
on the WPF owner thread. UTF-8 accessibility names are copied during the call,
and actions, element snapshots, and presentation records are copied into
caller-owned bounded arrays.

## Profile and file contract

The current profile schema is 1. Volumes are integers from 0 through 100 and
interface scale is an integer percentage from 75 through 150. The strict JSON
codec rejects duplicate or unknown properties, malformed input, unsupported
schema versions, out-of-range values, trailing data, and documents larger than
64 KiB. Invalid input resolves to safe defaults with a stable diagnostic; it
is never partially published.

The current-user path is:

```text
%LOCALAPPDATA%\Spelljammer\settings.v1.json
```

Apply encodes the complete candidate, writes a uniquely named temporary file
in the same directory, flushes it durably, decodes and compares the staged
artifact, and then replaces only the exact settings target. Replacing an
existing target records `settings.v1.json.recovery`. A failed stage or replace
keeps the previous active profile and target. Recovery validates the exact
recovery artifact before staging another durable exact-target replacement;
cleanup deletes only that recovery path.

## Presentation and localization

The settings source strings live in
`Content/Packs/base/Localization/en-US/settings.sfloc.json`. The offline
compiler produces a validated artifact before WPF compilation and embeds it in
the application. At startup the game-owned localization runtime decodes,
stages, and publishes the `settings` namespace on the UI thread. SpriteForge
receives copied resolved accessible names and stable numeric element/action
keys; it never receives localization catalog storage or translated identity.

SpriteForge currently returns neutral solid rectangles plus element snapshots.
The WPF host realizes those rectangles and resolved text. Direct realization
through SpriteForge's renderer and a native UI Automation bridge for this
managed child surface remain planned; keyboard-only operation is implemented,
but full screen-reader integration must not yet be claimed.

## Determinism and deferred work

The local profile is not stored in a campaign, content lock, semantic
fingerprint, replay, or simulation state. Reduced motion changes only the
presentation timer. Audio, subtitle, screen-shake, and interface-scale values
are retained now, but their live consumers await those presentation systems.

Campaign-affecting settings remain authoritative save work. They require an
explicit campaign schema revision, stable IDs and resolved values, validation
before publication, and migration coverage. They must not be added to this
local preference document.

## Verification ownership

`Tests/Spelljammer.Settings.Tests` is a compile-only contract executable for
codec determinism, invalid-input fallback, transactional publication,
replacement failure, recovery, and exact cleanup. SpriteForge's
`UIInteropTests.cpp` compiles the versioned ABI, modal control actions, copied
layout/presentation, and failed-batch atomicity. CI or the user owns executable
test runs under repository policy.
