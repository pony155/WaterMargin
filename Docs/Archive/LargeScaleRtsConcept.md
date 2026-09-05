# Large-scale 2D RTS architecture

> [!NOTE]
> This document is archived historical exploration from before Spelljammer
> adopted its colony-sandbox direction. It is not a current product plan. See
> the [product vision](../Product/Vision.md) and
> [first playable vertical slice](../Product/VerticalSlice.md).

## Status and scope

This document defines the target workload and application architecture for
SpriteForge's Total War-inspired tactical battles. It is a product design, not
a statement that a playable battle or every optimization described here is
implemented. Current runtime status is recorded in the engine design documents
and delivery plans.

The design target is a deterministic real-time battle containing up to 10,000
active soldiers and, when the camera and content require it, 10,000 visible
sprite instances. The number is a capacity and profiling target, not a promise
that every system performs full work for every soldier on every tick.

The design builds on the SpriteForge engine contracts for
[assets](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/AssetSystem.md),
[animation](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/AnimationSystem.md),
[world/ECS](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/ECS.md),
[commands](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/CommandSystem.md),
[tilemaps](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/TilemapSystem.md),
[framework lifecycle](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/Frameworkd.md),
[rendering](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/Renderer.md), and the planned
[UI system](https://github.com/pony155/SpriteForge/blob/main/Engine/Docs/UISystem.md). Game localization ownership and its
data/tooling boundary are specified in the planned
[localization system](../Architecture/Localization.md). Game-specific regiment,
formation, combat, morale, and AI rules remain under `Game/`; `Engine/` must not
depend on them.

## Design principles

- A regiment is the unit of player intent, path planning, and high-level AI.
  Soldiers retain individual position, animation, collision, combat, and
  presentation state only where the game needs it.
- Simulation advances on a fixed tick with stable command, system, partition,
  and reduction order. Presentation frame rate never changes battle results.
- Hot loops are native, data-oriented, bounded, and partitionable. C# gameplay
  code orchestrates high-level state and never performs a 10,000-soldier loop.
- Simulation publishes committed snapshots. Rendering and audio consume those
  snapshots and never become authorities for combat or movement.
- Capacity limits, overflow behavior, degradation policy, and telemetry are
  explicit. Large-battle behavior must be measured on documented reference
  hardware before it becomes a release claim.

## Unit art and asset profile

### Logical frame layout

Each unit type uses one logical, top-down frame canvas, commonly `64 x 64` or
`128 x 128` texels. Every frame for that type records the same untrimmed source
size and a stable ground-contact pivot. Offline tools may trim transparent
borders and pack the result tightly; trim offsets and the original source size
must preserve the logical canvas and prevent animation wobble.

The initial content profile uses north, south, east, and west facings. A unit
may store one lateral facing and derive the opposite facing with the renderer's
`flipX` flag. Content that is not visually symmetric may author east and west
separately. Diagonal movement selects an authored facing using a deterministic
direction policy. Pixel-perfect presentation does not blend sprite frames or
apply incidental small rotations; smooth rotation is an explicit material and
content choice.

Recommended baseline action lengths are:

| Action | Frames | Playback |
| --- | ---: | --- |
| Idle | 4 | Loop |
| Walk | 8 | Loop |
| Attack | 8 | Once or authored combo loop |
| Death | 12 | Once, holding the final frame |

These counts are game-content conventions, not engine animation limits.
Durations remain explicit per frame so an artist can vary timing without
changing simulation tick rate. Gameplay hit timing comes from deterministic
animation events or combat state, never from renderer frame interpolation.

### Runtime packaging

The portable baseline compiles texture, sprite-sheet metadata, and animation
clips as separate assets. Deterministically packed atlases use padding and edge
extrusion to prevent bleeding. The renderer resolves an opaque sprite-frame ID;
game systems do not depend on UV rectangles, descriptor indices, or backend
texture objects.

An optional texture-array artifact/profile may place same-sized, compatible
unit frames in array layers when profiling shows a benefit. Array dimensions,
format, mip policy, color space, and sampler state must match across all layers.
Backends without that capability use the atlas or bounded texture-table path
and must produce equivalent pixels. A texture array is therefore a packaging
optimization, not the public animation or asset model.

## Simulation model

### Regiment and soldier data

The EnTT-backed world stores game-defined components in dense pools. A likely
split is:

| Scope | Representative state |
| --- | --- |
| Regiment | stable identity, current order, path corridor, formation shape, facing, target, morale, fatigue |
| Soldier | transform/velocity, regiment identity, formation-slot index, local steering state, combat state, animation state |
| Shared asset | unit definition, movement/combat statistics, sprite sheet, animation clips, formation template |

The table defines ownership, not exact C++ layouts. Game code may use
structure-of-arrays scratch data or specialized packed components after
profiling, but it must preserve ECS identity, deterministic query semantics,
snapshot rules, and structural barriers. Large immutable definitions are
referenced by stable asset IDs instead of copied per soldier.

### Movement and formation flow

1. A player, AI, replay, or C# gameplay system emits one typed order for a
   regiment.
2. Regiment planning validates the order and computes or updates one path
   corridor for the group. It does not request an independent global path for
   every member.
3. Formation allocation deterministically maps active soldiers to stable slots
   relative to the regiment anchor.
4. A native spatial partition is rebuilt or incrementally updated in stable
   cell/entity order.
5. Native movement jobs steer soldiers toward their slots and apply bounded
   local separation, alignment, cohesion, terrain, and collision constraints.
6. Reductions and cross-partition effects merge in ascending assigned partition
   order before committed state is published.

Path requests may be asynchronous, but their results are observed only at a
defined tick boundary and include enough identity/version data to reject stale
results. Repath frequency, neighbor count, spatial-cell occupancy, and steering
iterations are bounded. A capacity failure follows an explicit game policy and
is visible in telemetry; it never silently changes ordering or allocates without
limit.

Navigation consumes immutable or versioned walkability/cost snapshots derived
from the tilemap and other game-owned obstacles. The engine tilemap does not own
regiment tactics, formation rules, or soldier steering.

### Combat and determinism

Broad-phase combat queries use the same or another explicitly owned spatial
partition. Candidate pairs are canonicalized and resolved in stable order.
Damage, morale, death, and formation membership changes use commands/events and
commit at documented barriers. Random decisions draw from seeded streams whose
ownership and sampling order do not depend on worker completion order.

Floating-point steering is deterministic only within the documented platform
profile unless a constrained numeric path is selected. Network lockstep or
cross-architecture replay must define a stronger fixed/floating-point contract
before claiming bit-identical results.

## C++ and C# boundary

C# owns gameplay composition: mission flow, scenario triggers, campaign rules,
high-level regiment AI state, campaign/battle orchestration, and UI-driven
intent. C++ owns the engine runtime and bounded native work such as pathfinding,
formation-slot assignment, spatial queries, movement/steering, combat hot loops,
animation evaluation, and presentation extraction.

C# gameplay code may inspect immutable, bounded regiment summaries and submit
typed commands through a versioned interop boundary. It must not enumerate the
complete soldier population each tick, retain ECS/native pointers, call
renderer/backend APIs, or receive native service objects. An illustrative
gameplay operation is:

```csharp
BattleCommands.MoveRegiment(regimentId, targetX, targetY, facing);
```

The concrete binding encodes a versioned `battle.regiment.move` command for a
future fixed-tick boundary. Its payload uses stable regiment identity and
integer- or fixed-defined target coordinates appropriate to replay/save needs.
C# callers receive a bounded status or transient ticket; they do not
synchronously invoke the movement system. Interop transfers owned values or
explicitly scoped borrowed views, never managed references embedded in native
serialized state.

The managed host, runtime version, ahead-of-time/JIT policy, assembly loading,
hot reload, and packaging model are not yet implemented. Those choices must be
profile-gated and validated on every supported platform before the first
buildable game target claims C# support. Managed allocation and garbage
collection may affect wall-clock performance but must never alter fixed-tick
ordering or simulation results; steady-state gameplay callbacks should avoid
unbounded allocation.

## Rendering profile

Presentation extraction reads the committed soldier state, resolves animation
frames and materials, performs conservative high-level culling, and emits
backend-neutral sprite descriptions. The renderer then uses its ordinary
packed, fence-safe instance path. The native GPU record is private to the
renderer and may contain transform, frame UV/array layer, untrimmed size, pivot,
trim offset, tint, flip flags, material, and texture/descriptor index.

One static unit quad is expanded by the vertex shader or backend equivalent.
Compatible consecutive sprites are emitted with instanced draws; descriptor
indexing, indirect drawing, texture arrays, and GPU culling are capability-
gated optimizations. D3D12 is the implemented backend and Metal is planned, but
the game and neutral renderer never encode D3D12/Metal command objects.

### Ordering and alpha

Battlefield presentation derives an integer semantic order from the unit's
quantized ground-contact Y coordinate, with explicit layer and stable entity or
submission tie-breakers. The renderer receives the resolved order; it does not
infer gameplay position from backend depth. This keeps translucent composition
stable for equal/near-equal Y values and across batching strategies.

Premultiplied-alpha sprites retain stable semantic ordering and normally do not
write depth. A deliberately authored alpha-cutout/opaque material may use alpha
cutoff and depth testing as a separate pipeline variant. The cutoff is material
data, not a global `alpha < 0.1` rule. Texture or pipeline sorting must not alter
the visible order of translucent units.

The game culls regiments or spatial cells before individual extraction; the
renderer performs conservative per-sprite culling as the final guard. Offscreen
simulation uses an explicit lower-frequency or aggregate policy only when that
policy cannot affect deterministic outcomes observable at full fidelity.

## Fixed-tick and host-frame lifecycle

The framework owns host-frame timing and invokes the game through registered
systems. A tactical fixed tick is scheduled conceptually as:

```text
map input / AI / C# gameplay intent to typed regiment commands
    -> validate and dispatch commands for this tick
    -> update regiment plans and consume ready path results
    -> update the spatial partition in stable order
    -> move formation members in deterministic partitions
    -> resolve combat, morale, death, and membership changes
    -> advance authoritative animation state and events
    -> commit structural commands and publish the world snapshot
```

After the final tick for a host frame:

```text
committed snapshot
    -> interpolate presentation transforms where allowed
    -> resolve sprite frames and semantic Y order
    -> cull and pack bounded sprite submissions
    -> execute renderer work and present
```

The exact system-to-phase mapping belongs to the game scheduler configuration.
The framework provides fixed phases and commit/publication boundaries; it does
not hard-code an RTS loop or run C# gameplay as an unconstrained variable-delta
update.

## Capacity and validation

The renderer's initial design gate is 10,000 visible sprites at 60 Hz, with
sprite extraction/cull/sort/batch below 2 ms CPU and sprite GPU work below 4 ms
on documented reference hardware. These remain provisional until the benchmark
scene and reference machine are checked into the repository.

The game must add battle benchmarks that report at least:

- active, visible, culled, moving, engaged, and dead soldier counts;
- regiments, path requests, expanded nodes, repaths, and stale results;
- spatial cells, occupancy high-water mark, neighbor candidates, and truncation;
- fixed-tick time per system, job count, partition imbalance, and merge time;
- C# callback time, managed allocations/collections, and regiment commands per
  tick;
- extracted sprites, instances, batches, draw/indirect calls, upload bytes, and
  capacity rejections.

Correctness fixtures cover stable formation allocation, command replay, path
result publication, partition-count invariance, combat pair ordering, animation
events, Y-order tie-breaking, alpha composition, atlas/array pixel equivalence,
and deterministic overflow policies. Optimization work follows profiles from
these fixtures rather than replacing public contracts speculatively.

## Open decisions

- Reference hardware, fixed-tick rate, and the exact large-battle pass/fail
  budgets.
- Navigation algorithm and corridor representation for dynamic battlefields.
- Formation reassignment and casualty-compaction policy.
- Numeric profile required for replay and any future multiplayer model.
- Which unit sets benefit from texture arrays compared with atlas/bindless
  rendering on each supported backend.
- Simulation level-of-detail rules that preserve authoritative outcomes.
