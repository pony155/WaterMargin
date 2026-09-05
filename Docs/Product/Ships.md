# Ships and modules

## Status

This document defines the planned ship-frame, compartment, module, network,
damage, and refit systems. They are not implemented yet. The current voyage
prototype stores only aggregate Hull, Fuel, Supplies, and Cargo values; those
values must not be described as a finished modular-ship simulation.

## Design goals

- Make the ship a persistent home whose layout, capabilities, damage, and scars
  record the campaign.
- Make every installed module create useful choices about space, mass, power,
  heat, crew time, maintenance, and risk.
- Support two viable upgrade paths—Arcane and Industrial—without making either
  a universal best choice.
- Let crew attributes, skills, positions, and duties affect module operation
  without creating classes or class-locked stations.
- Make failures local, inspectable, and capable of bounded cascades through
  explicit connections.
- Keep definitions data-driven and use stable IDs for saves, scenarios,
  balancing, and eventual modding.

## Ship composition

A ship consists of distinct persistent layers:

| Layer | Responsibility |
| --- | --- |
| Frame | Exterior shape, structural limits, mount zones, baseline hull, and maximum mass |
| Compartments | Interior spaces, access paths, atmosphere zones, hazards, and room capacity |
| Modules | Installed capabilities such as propulsion, life support, workshops, weapons, and wards |
| Networks | Explicit power, atmosphere, heat, fuel, aether, control, and logistics connections |
| Cargo | Bounded stored goods that are not permanently installed |
| Crew stations | Places where assigned duties operate or assist modules |
| Ship state | Damage, faults, modifications, contents, history, and active policies |

A room is navigable ship space. A module is installed equipment that occupies
one or more mounting cells inside a compartment or on an exterior hardpoint.
Not every room is a module, and portable cargo does not become a module until a
validated installation command commits it.

## Module contract

Every module definition declares:

- a stable definition ID and localized presentation keys;
- footprint, mass, mount tags, orientation rules, and access requirements;
- network ports and bounded input, output, storage, and throughput values;
- supported operating modes and transition costs;
- crew stations and recommended skills;
- produced capabilities, resource conversions, heat, noise, and signature;
- integrity, armor, fault, breach, and repair rules;
- compatible upgrades, enchantments, ammunition, and cargo tags; and
- explicit incompatibilities and any unique-installation limit.

Module definitions use IDs in the form `module.<category>.<name>`. Installed
instances receive separate stable IDs so two cargo holds can share one
definition while retaining different damage, contents, and histories.

## Ship upgrade paths

Every ship frame supports two primary upgrade paths. The selected path defines
the ship's main energy backbone, core propulsion choices, upgrade tree,
maintenance economy, and characteristic failures. It does not determine the
crew's culture, morality, faction, or available character skills.

| Upgrade path | Stable ID | Energy and progression | Strengths | Costs and risks |
| --- | --- | --- | --- | --- |
| Arcane | `ship.path.arcane` | Aether crystals, ambient currents, runic conduits, and increasingly powerful enchantments | Low mass, flexible routing, quiet operation, strong wards, and unusual utility effects | Rare reagents, skilled magical maintenance, aether interference, psychic feedback, and dispelling |
| Industrial | `ship.path.industrial` | Dieselpunk compression engines and propellant machinery that advance into atompunk reactors and nuclear-thermal drives | Common early parts, field repair, high sustained output, heavy armor, and reliable physical weapons | Greater mass, heat, noise, vibration, fuel use, coolant demand, exhaust at early tiers, radiation at advanced tiers, and frequent maintenance |

### Arcane progression

The Arcane path treats aether as the ship's primary energy source. Early
upgrades stabilize an Aether Dynamo and Flux Sail. Later upgrades improve
crystal storage, runic distribution, remote control, ward efficiency, and
enchantment capacity. Magic and Enchantment provide the main specialist work,
while Engineering remains important for the physical housings, controls, and
connections.

Arcane failures include crystal fractures, unstable bindings, aether leaks,
spell interference, psychic echoes, and effects that reveal the ship to
supernatural detection. Arcane energy is not limitless: crystals, reagents,
stored charge, heat capacity, and crew attention remain bounded resources.

### Industrial progression

The Industrial path begins with dieselpunk compression engines, generators,
pumps, flywheels, analog controls, and high-pressure propellant systems. Later
upgrades introduce atompunk reactors, shielded turbine halls, radiothermal
generators, and nuclear-thermal propulsion. Engineering, Crafting, and Rigging
provide its main specialist work.

Dieselpunk failures include leaks, seized bearings, fuel fires, coolant loss,
smoke, vibration, and exhaust or oxidizer problems. Combustion systems must
carry both fuel and an explicit oxidizer where the surrounding environment
cannot support combustion.

Atompunk systems replace much of that fuel demand with long-lived reactor fuel
and high sustained output. They introduce reactor trips, radiation exposure,
shielding mass, coolant loops, contaminated parts, and containment breaches.
Atomic propulsion still consumes reaction mass, and shutdown reactors retain
decay heat that must be removed.

### Commitment and hybrid ships

A ship has one primary path at a time. Core generators, backbone upgrades, and
top-tier propulsion from the two paths are mutually exclusive while installed.
Changing paths is possible, but it is a major shipyard refit rather than a free
respec.

Common habitat, cargo, command, and work modules can be built for either
backbone. An off-path module requires a compatible converter or isolated local
generator, adding mass, footprint, heat, maintenance, and a bounded conversion
loss. Hybrid ships are therefore supported as deliberate specialist builds,
but cannot collect both paths' strongest benefits without paying visible costs.

## Planned module catalog

### Command and navigation

| Module | Stable ID | Function | Typical skill or position |
| --- | --- | --- | --- |
| Helm | `module.command.helm` | Commits maneuver and travel commands and exposes current ship-control status | Piloting; Pilot or Captain |
| Star Compass | `module.navigation.star-compass` | Supports route plotting, position fixes, and anomaly-aware astrogation | Astrogation; Navigator |
| Sensor Mast | `module.navigation.sensor-mast` | Detects contacts, hazards, emissions, and survey targets | Sensors; Navigator or Scout |
| Chart Archive | `module.navigation.chart-archive` | Stores verified routes, survey records, scripts, and discovered lore | Astrogation, Language and Literacy, or Ancient Lore; Cartographer |

### Propulsion and power

| Module | Stable ID | Path | Function | Typical skill or position |
| --- | --- | --- | --- | --- |
| Flux Sail | `module.propulsion.flux-sail` | Arcane | Converts aether charge and stellar currents into voyage movement | Piloting or Rigging; Pilot or Rigger |
| Aether Dynamo | `module.power.aether-dynamo` | Arcane | Converts crystals or gathered currents into ship power and aether charge | Magic, Enchantment, or Engineering; Ship Mage or Engineer |
| Crystal Accumulator | `module.power.crystal-accumulator` | Arcane | Stores bounded aether charge for peak demand and emergency operation | Enchantment; Artificer |
| Runic Distributor | `module.power.runic-distributor` | Arcane | Routes and prioritizes aether while isolating unstable branches | Enchantment or Engineering; Artificer |
| Diesel Generator | `module.power.diesel-generator` | Industrial—Dieselpunk | Produces mechanical and electrical power from fuel and oxidizer with cooling and exhaust demands | Engineering; Engineer |
| Propellant Drive | `module.propulsion.propellant-drive` | Industrial—Dieselpunk | Produces sustained thrust by consuming power and stored propellant | Piloting or Engineering; Pilot or Engineer |
| Flywheel Bank | `module.power.flywheel-bank` | Industrial—Dieselpunk | Stores bounded mechanical energy for peak demand and emergency operation | Engineering or Crafting; Engineer |
| Atomic Reactor | `module.power.atomic-reactor` | Industrial—Atompunk | Provides high sustained power from reactor fuel while requiring shielding, control rods, and cooling | Engineering; Chief Engineer |
| Nuclear-Thermal Drive | `module.propulsion.nuclear-thermal-drive` | Industrial—Atompunk | Heats stored reaction mass for efficient high-output thrust | Piloting or Engineering; Pilot or Chief Engineer |
| Vector Vanes | `module.propulsion.vector-vanes` | Either | Provides docking, evasion, and close maneuver control through a path-compatible drive | Piloting or Engineering; Pilot |

### Habitat and care

| Module | Stable ID | Function | Typical skill or position |
| --- | --- | --- | --- |
| Atmosphere Recycler | `module.habitat.atmosphere-recycler` | Maintains breathable atmosphere and filters smoke, spores, and toxins | Engineering or Xenology; Engineer |
| Crew Quarters | `module.habitat.crew-quarters` | Provides bounded bunks, privacy, rest quality, and environmental tuning | Steward |
| Galley | `module.habitat.galley` | Turns provisions into safe meals for compatible diets | Cooking; Chef |
| Sickbay | `module.habitat.sickbay` | Supports diagnosis, surgery, quarantine, recovery, and medical storage | Medicine; Doctor or Medic |
| Common Room | `module.habitat.common-room` | Supports recreation, meetings, belonging, and conflict mediation | Insight or Negotiation; Steward or First Mate |
| Provision Locker | `module.habitat.provision-locker` | Preserves food, blood, medicine, and other tagged consumables | Cooking, Medicine, or Alchemy |

### Work, research, and cargo

| Module | Stable ID | Function | Typical skill or position |
| --- | --- | --- | --- |
| Cargo Hold | `module.cargo.hold` | Stores bounded cargo by mass, volume, hazard, and environment tags | Merchant; Quartermaster or Trader |
| Workshop | `module.workshop.fabricator` | Repairs and fabricates tools, fittings, ammunition, and replacement parts | Crafting or Engineering; Artificer |
| Alchemy Laboratory | `module.workshop.alchemy-lab` | Identifies reagents and produces medicines, compounds, and volatile mixtures | Alchemy; Alchemist |
| Enchanting Chamber | `module.workshop.enchanting-chamber` | Creates and maintains persistent magical bindings under controlled conditions | Enchantment; Artificer or Ship Mage |
| Lore Vault | `module.research.lore-vault` | Preserves artifacts, inscriptions, translations, and authenticated discoveries | Ancient Lore or Language and Literacy; Antiquarian |
| Salvage Rig | `module.utility.salvage-rig` | Recovers cargo and wreck material without bringing every hazard aboard | Salvage or Rigging; Salvager |

### Defense and contact

| Module | Stable ID | Function | Typical skill or position |
| --- | --- | --- | --- |
| Reinforced Plating | `module.defense.reinforced-plating` | Adds localized protection at the cost of mass and maneuverability | Engineering |
| Ward Projector | `module.defense.ward-projector` | Sustains a bounded defense against magical, psychic, or environmental threats | Magic, Psionics, or Enchantment; Warden |
| Deck Battery | `module.weapon.deck-battery` | Mounts a tagged ship weapon and stores its ready ammunition | Gunnery; Gunner |
| Boarding Lock | `module.contact.boarding-lock` | Controls docking, boarding, quarantine seals, and ship-to-ship access | Engineering or Defense; Master-at-Arms |
| Signal Lantern | `module.contact.signal-lantern` | Sends identification, negotiation, warning, and distress signals | Language and Literacy or Negotiation; Envoy |

The catalog is a design vocabulary, not a promise that every module belongs in
the first playable voyage. New definitions should create new system
interactions rather than duplicate an existing module with larger numbers.

## Networks and allocation

Modules exchange resources only through declared network ports:

| Network | Carries | Example failure |
| --- | --- | --- |
| Power | Industrial mechanical or electrical energy | An overloaded branch sheds lower-priority modules |
| Atmosphere | Air, pressure, filtration, and environmental mixture | A breach isolates a compartment and reduces habitable capacity |
| Heat | Produced heat and cooling capacity | Excess heat degrades output and raises fire risk |
| Fuel | Tagged crystals, combustible or reactor fuel, propellant, oxidizer, coolant, and bounded delivery rates | A damaged line limits drive or generator output |
| Aether | Arcane energy, magical charge, wards, and supernatural interference | Feedback destabilizes an enchantment or reveals the ship |
| Control | Commands, sensors, and automation | A severed route forces local manual operation |
| Logistics | Physical access for crew, parts, ammunition, and cargo | A blocked passage delays work and evacuation |

Arcane backbones primarily distribute Aether; Industrial backbones distribute
Power. A module declares which energy media it accepts. Converters bridge the
two networks only at their declared capacity and efficiency, so an Industrial
ship cannot power a ward for free and an Arcane ship cannot operate heavy
machinery without suitable conversion or an enchanted variant.

The player assigns priorities rather than directly distributing every unit each
tick. The authoritative simulation resolves supply in a stable order, records
every unmet request, and exposes which producer, connection, capacity, policy,
or priority blocked a module. Allocation and module updates occur on the fixed
simulation tick and never depend on UI or rendering cadence.

## Operation and crew

Each installed module has a requested mode and a committed operating state.
Typical modes are Offline, Standby, and Active; condition is tracked separately
as Intact, Damaged, Disabled, or Breached. A module becomes operational only
when its dependencies, connections, crew access, and resource requests are
satisfied.

Crew positions provide responsibility and authority, while duties describe the
actual work performed at a station. Skills and contextual attributes determine
performance. A module may be operated by an unconventional crew member with
visible penalties unless a physical, safety, or policy requirement prevents
it. Automation can replace some labor but has explicit capacity and failure
rules.

Examples include:

- Intelligence plus Engineering to diagnose a dynamo fault;
- Strength plus Rigging to deploy a jammed Flux Sail;
- Toughness plus Engineering to seal an atomic coolant leak;
- Intelligence plus Crafting to calibrate a diesel injection system;
- Toughness plus Salvage to continue an exterior recovery under strain;
- Willpower plus Enchantment to stabilize a failing Ward Projector;
- Intelligence plus Merchant to arrange cargo for inspection and sale; and
- Charisma plus Negotiation to transmit acceptable docking terms through the
  Signal Lantern.

## Damage, repair, and refit

Damage targets explicit compartments, modules, connections, or the frame. It
reduces integrity and may add a named fault such as jammed, leaking, shorted,
contaminated, irradiated, misaligned, containment-breached, fuel-starved, or
aether-unstable.
Faults declare their effects and possible propagation; damage does not produce
an unbounded hidden cascade.

Emergency work can isolate a network, suppress a fault, patch integrity, or
restore limited output. Complete repair may require Engineering diagnosis,
Crafting parts, Enchantment work, a suitable facility, and downtime. The UI
shows the required resources, expected time, risk, and resulting limitations
before the repair command is committed.

Installation, removal, and replacement are transactional. The simulation
validates frame capacity, footprint, mass, access, network compatibility,
cargo displacement, crew safety, and unique limits before changing the working
configuration. Failed validation leaves the previous ship intact. Major refits
normally require an anchorage or shipyard; explicitly tagged field modules may
be swapped during a voyage.

## Data and persistence

An authored module definition resembles:

```json
{
  "schemaVersion": 1,
  "id": "module.habitat.sickbay",
  "nameKey": "ship.module.habitat.sickbay.name",
  "footprint": 2,
  "mass": 8,
  "mountTags": ["interior", "pressurized"],
  "compatiblePathIds": ["ship.path.arcane", "ship.path.industrial"],
  "recommendedSkillIds": ["skill.medicine"],
  "ports": [
    { "network": "power", "direction": "input", "capacity": 2 },
    { "network": "atmosphere", "direction": "input", "capacity": 1 }
  ],
  "capabilityTags": ["treatment", "surgery", "quarantine"]
}
```

Persistent ship state stores its primary path ID and backbone revision.

Persistent module state stores instance ID, definition ID and revision,
location, orientation, integrity, requested mode, faults, contents, installed
upgrades, enchantments, and bounded history references. Saves never store a
localized path or module name as identity.

Definition loading rejects duplicate or missing IDs, unknown path references,
invalid footprints, negative or excessive capacities, incompatible ports,
impossible dependency cycles, unknown skills or tags, and unbounded fault
propagation before publishing a replacement catalog.

## First playable ship scope

The first complete crew-enabled voyage uses one authored frame, a shared set of
modules, and a choice between two fixed energy packages:

| Module | Slice purpose |
| --- | --- |
| Helm and Star Compass | Travel commands and inspectable route checks |
| Atmosphere Recycler and Crew Quarters | Crew survival and rest |
| Galley and Provision Locker | Meals, supplies, and varied diets |
| Sickbay | Injury treatment and the Doctor position |
| Cargo Hold and Salvage Rig | Bounded recovery and cargo capacity |
| Workshop | Engineering repair and replacement parts |
| Signal Lantern | Contact, negotiation, warnings, and distress calls |

| Energy package | Starting modules | Slice tradeoff |
| --- | --- | --- |
| Arcane | Flux Sail, Aether Dynamo, Crystal Accumulator, and Ward Projector | Lower mass and supernatural defense against scarce reagents and aether instability |
| Industrial | Propellant Drive, Diesel Generator, Flywheel Bank, and Reinforced Plating | Durable repairable dieselpunk output against greater mass, fuel, coolant, heat, and noise |

The player selects one package before departure. The slice needs only one
energy-allocation decision, one path-specific module fault, one crew-operated
repair, and one choice where cargo, safety, or capability competes for limited
capacity. The fault must expose the affected module, network, and repair path.
Full hybrid construction, changing paths during a voyage, atompunk Industrial
upgrades, multiple frames, boarding layouts, broad weapon customization, module
manufacturing, and unrestricted shipyards remain deferred until that loop is
deterministic and readable.
