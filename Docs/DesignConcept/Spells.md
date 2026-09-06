# Spells

## Status

This document defines the planned spell system and catalog. It is not
implemented in the current ship-level expedition prototype.

## Design rule: magic should be quick to understand

Magic creates memorable choices, not aether chemistry or ritual accounting. A
player should understand a spell from its tooltip and use it in one decision.
Each spell shows only its **Focus cost**, **range**, **cast time**, and
**effect**. A spell may also show a short cooldown when it needs one.

- A spell has one strong, bounded job.
- Personal spells use Focus. Ship spells use Aether from an Arcane module
  instead; they do not add a second personal resource puzzle.
- An attempted spell either works, is resisted, or fails clearly. It does not
  create a long list of partial refunds, contaminants, traces, or backlash
  subtypes.
- Effects use familiar game language: damage, protection, movement, light,
  concealment, healing support, or information. They do not need detailed
  physical simulation.
- Magic supports skills, gear, and planning; it cannot freely create wealth,
  resurrect the dead, read any mind, time travel, or bypass the galaxy map.

Magic belongs to an advanced Arcane-Industrial civilization, not a separate
medieval layer. Wards and enchantments can sit beside engines, reactors,
sensors, and shell cannon, while still following their own access and resource
rules. See [`Setting.md`](Setting.md) for the shared setting baseline.

## Access and learning

Characters have no mage class. A character can cast when they have all three:

1. `access.magic`, granted by `feat.access.magic` (Spellcasting Training) or a
   Racial Perk that explicitly grants it;
2. the spell's stable ID in their known-spell collection; and
3. enough Focus or, for a ship spell, enough Aether plus its required module.

The Elf Race Perk **Aether Sense** is the initial innate source of
`access.magic`. It grants access and a small sensing effect, but no free Magic
skill ranks or known spells. A high Magic skill, spellbook, or item never
bypasses the access requirement.

Learning a spell is a short, explicit training project: find a legitimate
source, meet its stated skill requirement, spend the listed downtime, then add
the spell ID. Sources can be teachers, books, ruins, factions, or discoveries;
the game only tracks requirements that create an interesting decision.

## Casting

Casting is one action:

1. choose a known spell and legal target;
2. preview its four visible values and any resistance; and
3. pay the cost and resolve the effect.

The Magic skill and a suitable attribute improve the chance or strength when a
spell calls for a check. A failed check spends the action and its stated cost;
the interface says why it failed. Channeled and ritual spells are exceptions:
they show a simple progress bar and can be interrupted. There are no separate
reservation, preparation, recovery, refund, trace, or contamination systems.

## Spell definition

Every spell definition contains a stable ID, localized name and description,
required access ID, and these player-facing values:

| Value | Meaning |
| --- | --- |
| Focus cost | Personal casting cost; `0` is allowed for a deliberately minor spell |
| Range | Self, touch, near, far, or ship |
| Cast time | Instant, channel, or ritual |
| Effect | One bounded result and, when needed, its duration or target limit |
| Cooldown | Optional short reuse delay |

Definitions may also declare target tags, damage type, resistance, and a
required ship module. These are rules checks, not extra player-facing resource
systems. Each definition must have a clear end condition and bounded targets.

## Spell tiers

Tiers describe availability, not character levels:

| Tier | Use |
| ---: | --- |
| 1 | Reliable personal utility and encounter tools |
| 2 | Strong movement, control, healing support, and clever exploration |
| 3 | Major encounter or compartment effects |
| 4 | Rare ship-scale workings requiring an Arcane module |
| 5 | Unique story or endgame workings only |

Tiers 1 and 2 should supply most routine play. Tiers 4 and 5 are special
moments, not a combat rotation.

## First playable spells

The first character-combat slice needs only these four Tier 1 spells:

| Spell | Stable ID | Focus | Range | Cast time | Effect |
| --- | --- | ---: | --- | --- | --- |
| Lantern Spark | `spell.radiance.lantern-spark` | Low | Near | Instant | Create light or an obvious visual signal. |
| Vector Tether | `spell.vectoring.vector-tether` | Low | Near | Instant | Move, pull, or secure one small unattended object. |
| Brace Ward | `spell.warding.brace-ward` | Low | Touch | Instant | Briefly reduce one declared hit or hazard. |
| Aether Trace | `spell.seeking.aether-trace` | Low | Near | Channel | Reveal a clue about recent nearby magic; the result states its confidence. |

These spells do not ignite arbitrary objects, grant universal immunity, move
ships for free, or reveal hidden truth without limits.

## Catalog

The remaining entries are planned content candidates. Their final numbers are
balance data; their intended use is deliberately short and readable.

### Warding

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Brace Ward | `spell.warding.brace-ward` | 1 | Briefly reduce one declared hit or hazard. |
| Threshold Seal | `spell.warding.threshold-seal` | 2 | Protect a marked door or hatch against one intrusion. |
| Spellbreak Lattice | `spell.warding.spellbreak-lattice` | 3 | Disrupt active magic in a small area. |
| Haven Circuit | `spell.warding.haven-circuit` | 4 | Protect one ship compartment through a Ward Projector. |

### Vectoring

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Vector Tether | `spell.vectoring.vector-tether` | 1 | Move or secure a small loose object. |
| Driftstep | `spell.vectoring.driftstep` | 2 | Make one controlled move in low gravity. |
| Gravity Knot | `spell.vectoring.gravity-knot` | 3 | Create a brief small area that pulls targets. |
| Keel Turn | `spell.vectoring.keel-turn` | 4 | Help one ship maneuver through an Arcane network. |

### Radiance

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Lantern Spark | `spell.radiance.lantern-spark` | 1 | Create light or a clear signal. |
| Heat Draw | `spell.radiance.heat-draw` | 2 | Move dangerous heat into a prepared sink. |
| Starflare Beacon | `spell.radiance.starflare-beacon` | 3 | Send a powerful visible signal. |
| Dawn Array | `spell.radiance.dawn-array` | 4 | Light or warm a ship area through Arcane projectors. |

### Veiling

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Quiet Silhouette | `spell.veiling.quiet-silhouette` | 1 | Make one slowly moving subject harder to see. |
| False Wake | `spell.veiling.false-wake` | 2 | Leave a convincing but discoverable false trail. |
| Masked Hold | `spell.veiling.masked-hold` | 3 | Hide selected cargo details from magical scans. |
| Ghost Rig | `spell.veiling.ghost-rig` | 4 | Change a ship's apparent profile, not its collision body. |

### Shaping

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Seam Press | `spell.shaping.seam-press` | 1 | Temporarily close a small clean split. |
| Cutline | `spell.shaping.cutline` | 2 | Weaken a marked line in unattended material. |
| Hullskin | `spell.shaping.hullskin` | 3 | Briefly reinforce a prepared surface. |
| Formwright Chorus | `spell.shaping.formwright-chorus` | 4 | Reshape compatible ship material at a ritual station. |

### Seeking

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Aether Trace | `spell.seeking.aether-trace` | 1 | Find a clue about nearby recent magic. |
| Fault Echo | `spell.seeking.fault-echo` | 2 | Identify a visible item's or module's weak point. |
| Waymark Compass | `spell.seeking.waymark-compass` | 3 | Point toward a prepared known anchor. |
| Starway Sounding | `spell.seeking.starway-sounding` | 4 | Survey a nearby Starway with ship instruments. |

### Vitality

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Steady Pulse | `spell.vitality.steady-pulse` | 1 | Stabilize one living target until treatment starts. |
| Draw Taint | `spell.vitality.draw-taint` | 2 | Help treat one known poison or contamination. |
| Borrowed Breath | `spell.vitality.borrowed-breath` | 3 | Give brief breathing support during a rescue. |
| Sanctuary Vigil | `spell.vitality.sanctuary-vigil` | 4 | Support several patients through a medical ritual. |

### Passage

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Anchor Step | `spell.passage.anchor-step` | 2 | Move to a visible prepared mark. |
| Paired Threshold | `spell.passage.paired-threshold` | 3 | Open a brief link between nearby prepared anchors. |
| Cargo Aperture | `spell.passage.cargo-aperture` | 4 | Transfer limited cargo between staffed prepared sites. |
| Starway Accord | `spell.passage.starway-accord` | 5 | Change access to one discovered Starway in a unique story event. |

## Ship magic

Ship spells use the same readable pattern: choose the spell, spend Aether from
a connected Arcane module, and see the effect. They require the stated module
and enough Aether, but do not simulate network topology, assistants, work
slots, component quality, or detailed ritual bookkeeping. Industrial ships can
use isolated Arcane modules, charged devices, or visiting casters when their
loadout supports it.

## Saves and validation

Saves store known spell IDs and only active effects that matter: source ID,
target, remaining duration, and caster when relevant. Content validation
rejects duplicate IDs, unknown access or target tags, negative costs,
unbounded targets or durations, invalid ship-module references, and effects
that bypass the explicit limits above. A failed catalog reload leaves the last
valid catalog active.

## Delivery order

1. Implement the four first-playable spells with one simple Focus pool.
2. Add one resistance rule, one spell-learning project, and one ship spell.
3. Add other spells only when they make exploration, combat, or voyage choices
   more interesting.

Do not add procedural spell generation, unrestricted wishes, routine
resurrection, time travel, or complex material creation.
