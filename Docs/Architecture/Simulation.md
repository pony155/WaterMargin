# Space-expedition simulation

## Implemented boundary

`Source/Spelljammer.Simulation` is a headless .NET project that owns the first
authoritative gameplay state. It does not reference WPF, SpriteForge,
localization, wall-clock APIs, or mutable presentation objects.

`ExpeditionSimulation.Create(seed)` creates a run. `Apply(state, command)` is
the only state-transition boundary. Accepted commands return a new immutable
state; rejected commands return the original state and a stable
`CommandRejection` value. The current 4 × 4 chart is bounded by a 16-bit visit
mask and a 16-bit salvage mask, so growth cannot create unbounded work.

Sector properties are derived from the run seed and stable row-major sector ID
with an integer mixing function. The same seed and command stream therefore
produce the same hazards, rewards, and state on every host. Presentation maps
the resulting enums and numbers to player-facing text.

## Prototype rules

- Travel costs one fuel and exposes the hull to the destination's danger.
- Every third accepted action consumes one supply.
- Each non-anchorage sector can be salvaged once.
- Salvage produces cargo and may contain a fuel cache.
- Repair consumes two cargo and restores up to three hull.
- A run succeeds by returning to the anchorage with at least eight cargo.
- Zero hull or zero supplies ends the run.

These constants are compiled prototype rules. Moving them into validated,
versioned content definitions is planned before saves become a public contract.

## Next architecture step

Introduce a fixed-tick `VoyageWorld` with stable ship, crew, module, and
encounter identities. Commands should enter a bounded queue and publish a
read-only snapshot only after a complete tick. The current expedition state can
then become the strategic navigation layer around the more detailed world.
