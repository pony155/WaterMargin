# Content ID ownership and collision review

## Status

This document completes roadmap tasks `M0.1.2` and `M0.1.3`. It classifies the
152 explicit IDs in [`ContentIdInventory.md`](ContentIdInventory.md) and records
the spelling and collision review performed before version 1 loader work.

No gameplay content loader or public save format exists yet. Classification is
a design commitment, not an implementation claim.

## Classification model

Ownership and delivery are separate:

- **Base-owned** means the ID is reserved for the Spelljammer base pack.
- **Example-only** means the string illustrates a future definition but is not
  committed as base content yet.
- **Deferred** means the ID is base-owned but its runtime definition is not
  required in the first implementation milestone that introduces its registry.
- **Prototype-serialized** means an existing save or public artifact already
  persists that exact string ID.

An ID may therefore be both base-owned and deferred. Example-only takes
precedence over base ownership until content authoring promotes it explicitly.

## Complete classification

Every ID in an inventory section inherits the classification below unless an
exception is listed. This grouping marks every inventoried ID without copying a
second 152-row table that could drift from the canonical inventory.

| Inventory group | Count | Ownership | Earliest registry | Delivery note |
| --- | ---: | --- | --- | --- |
| Attributes | 7 | Base-owned | M2 | All seven are required base definitions |
| Skills | 29 | Base-owned | M2 | All 29 are required base definitions |
| Racial Perks | 12 | Base-owned | M3/M4 | Registry begins in M3; supernatural grants become executable in M4 |
| Equipment | 11 | Base-owned | M5 | Initial weapons, powered armor, tools, medical gear, and Arcane focus |
| Learned Feats | 2 | Base-owned | M4 | Both are required access paths |
| Access gates | 2 | Base-owned | M4 | Both are required access definitions |
| Spells | 7 | Base-owned | M4 | Four are first-slice; three are deferred catalog content |
| Psychic techniques | 4 | Base-owned | M4 | All four are first-slice content |
| Combat contexts | 6 | Base-owned | M5 | Three initial contexts and three deferred contexts |
| Travel events | 8 | Base-owned | M8 | Four first-voyage events and four deferred events |
| Travel-event choices | 3 | Base-owned | M8 | All three belong to the first Coolant Leak event |
| Ship modules | 35 | Base-owned | M5 | Registry begins in M5; individual catalog delivery is phased |
| Ship weapon configurations | 3 | Base-owned | M5 | Aether and Diesel are first-slice; Atomic is deferred atompunk content |
| Factions | 6 | Base-owned | M8 | Initial voyage selects three; remaining roster is deferred |
| Crisis families | 7 | Base-owned | M9 | All endgame content is deferred beyond the first voyage |
| Crisis phases | 7 | Base-owned | M9 | Shared M9 phase vocabulary |
| Crisis resolutions | 3 | Example-only | M9 | IDs appear only in the Shattered Meridian JSON example |

No inventoried ID is prototype-serialized. Current prototype enums and numeric
properties exist only in memory; they neither use these strings nor establish a
public save contract.

## First-slice and deferred exceptions

The first M4 Spell slice is:

```text
spell.divination.detect-invisibility
spell.evocation.burning-hands
spell.evocation.magic-missile
spell.illusion.phantasmal-image
```

The other three Spell IDs remain base-owned deferred catalog content.

The initial M5 combat contexts are:

```text
combat.context.ship
combat.context.boarding
combat.context.ruin
```

The following are deferred beyond the first Battle slice:

```text
combat.context.eva
combat.context.settlement
combat.context.surface
```

The first M8 travel-event slice is:

```text
event.travel.aether-squall
event.travel.coolant-leak
event.travel.derelict-signal
event.travel.distress-call
```

The other four Event IDs remain base-owned deferred catalog content.

The initial M5 ship slice requires these 23 module definitions:

```text
module.cargo.hold
module.command.helm
module.contact.signal-lantern
module.defense.energy-shield
module.defense.reinforced-plating
module.defense.ward-projector
module.habitat.atmosphere-recycler
module.habitat.crew-quarters
module.habitat.galley
module.habitat.provision-locker
module.habitat.sickbay
module.navigation.star-compass
module.power.aether-dynamo
module.power.crystal-accumulator
module.power.diesel-generator
module.power.flywheel-bank
module.propulsion.flux-sail
module.propulsion.propellant-drive
module.prow.figurehead
module.prow.ram
module.utility.salvage-rig
module.weapon.deck-battery
module.workshop.fabricator
```

The other 12 inventoried ship modules remain base-owned deferred catalog
content. This first set supports energy-package, armor, Energy Shield, prow,
weapon, habitat, care, cargo, and utility customization without requiring the
full catalog.

The first M5 weapon slice includes `ship.weapon.arcane.aether-cannon` and
`ship.weapon.industrial.diesel-shell-cannon`. The Atomic Shell Cannon remains
base-owned deferred atompunk content.

The initial M8 faction candidate set is the Free Anchorage Compact, Horizon
Salvagers' Union, Meridian Foundry League, and Lumenwake Covenant; a campaign
uses the first two plus one of the latter pair. Pilgrim Garden Fleet and Quiet
Chorus Assembly are deferred roster content.

## Missing IDs resolved during review

The compact Skill table named 15 capabilities without Stable IDs. Version 1
cannot compile a dynamic Skill registry while those names remain implicit, so
the review added the following IDs to `Skills.md` and the inventory:

```text
skill.acrobatics
skill.astrogation
skill.athletics
skill.command
skill.deception
skill.defense
skill.eva
skill.gunnery
skill.insight
skill.piloting
skill.rigging
skill.salvage
skill.sensors
skill.stealth
skill.xenology
```

These are new explicit IDs derived directly from unique existing Skill labels;
no prior ID was renamed. `skill.medicine` already existed in the Ships sickbay
example and was moved into the authoritative Skill table without changing it.

Heritage rows still describe Perks whose individual Perk IDs are not
authored. They are not required by the M0 fixture or M2 Attribute/Skill loader;
M3 content authoring must assign and review them before the base character pack
is complete.

## Collision review

The 152 inventoried IDs are unique. No one ID maps to two different display
labels, and ordinal comparison produces no case-only collision.

The following similar forms are intentional:

| Forms | Decision |
| --- | --- |
| `skill.magic`, `feat.access.magic`, `access.magic` | Separate competence, learned grant source, and effective capability gate |
| `skill.psionics`, `feat.access.psionics`, `access.psionics` | Same three-layer distinction for psychic use |
| `spell.*` and `psychic.*` techniques | Preserve established domains; document kind validates the more specific prefix |
| `perk.race.*` and `perk.heritage.*` | Preserve grant provenance in identity |
| `crisis.<family>`, `crisis.phase.*`, and nested resolution IDs | Preserve separate family, phase, and resolution definition kinds |
| Hyphenated names such as `half-elf` and `nuclear-thermal-drive` | Hyphens are canonical within one segment; underscores are forbidden |

The design directory is `Docs/DesignConcept`. Architecture links,
README references, and repository guidance now use that path. Filename
`Endgame_Crisis.md` retains its underscore, but filenames do not participate in
gameplay identity.

Illustrative third-party IDs previously used a pack ID that looked more
specific than their shared namespace. Version 1 resolves this by using a pack
ID of the form `mod.<namespace>`, for example `mod.starwrights`, with definitions
such as `skill.mod.starwrights.gravimetry`. These examples are not base IDs and
are excluded from the 152 count.

## Review conclusion

No inventoried base ID requires a rename before loader work. The only
example-only gameplay IDs are the three Shattered Meridian resolutions, and no
current ID is constrained by prototype serialization.

Future ID additions must pass the grammar and ownership rules in
[`ContentContractsV1.md`](ContentContractsV1.md) and be added to the inventory
before their definitions are treated as base content.
