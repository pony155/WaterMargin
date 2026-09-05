# Character and ancestry design

## Status

This document defines planned character content and simulation rules. Character
entities, ancestry definitions, lineages, crew needs, relationships, and
character creation are not implemented yet. The current prototype simulates
only ship-level expedition resources.

## Design goals

- Make every crew member mechanically understandable and narratively distinct.
- Support humans, elves, half-elves, dwarves, orcs, vampires, and future
  ancestries without hard-coding content into simulation code.
- Give each ancestry recognizable strengths, needs, and complications without
  prescribing personality, morality, intelligence, profession, or faction.
- Separate inherited physiology from culture, upbringing, vocation, belief,
  and personal traits.
- Let ancestry matter aboard a ship: atmosphere, gravity, food, sleep, medical
  care, room design, travel hazards, and relationships should all interact.
- Preserve stable character identities and deterministic generation across
  saves and replays.

## Terminology

The simulation uses **ancestry** for a broad people or physiological family and
**lineage** for the narrower variants often called subraces. UI copy may use
more setting-appropriate terms later, but save data and content IDs use
`ancestryId` and `lineageId`.

Culture is never a subrace. A dwarf raised in a human port and a human raised
aboard an orc vessel retain their ancestry while receiving culture, language,
relationships, and learned abilities from their lived history.

Vampirism is presented as a character origin alongside the other choices, but
is modeled as a transformation layered over an ancestry. This permits a human,
elf, dwarf, or orc vampire without duplicating every base definition. A
vampire's bloodline fills the same character-creation role as a lineage.

## Character composition

A persistent character is composed from independent, stable layers:

| Layer | Examples | Simulation responsibility |
| --- | --- | --- |
| Identity | character ID, name seed, pronouns, portrait seed | Persistence and presentation lookup |
| Ancestry | human, elf, half-elf, dwarf, orc | Body plan and baseline physiological rules |
| Lineage | voidborn human, deepforge dwarf | One focused adaptation and one meaningful cost |
| Culture | free anchorage, convoy clan, court exile | Languages, customs, starting relationships, learned knowledge |
| Upbringing | dockhand, monastery ward, officer's child | Starting memories, contacts, and aptitudes |
| Vocation | pilot, navigator, gunner, surgeon, artificer | Trained skills, not an immutable character class |
| Traits | patient, reckless, curious, claustrophobic | Individual behavior and preferences |
| Conditions | injured, irradiated, vampiric, exhausted | Mutable health and supernatural state |
| Relationships | trust, fear, affection, debt, rivalry | Character-specific social state |

No translated name or description becomes gameplay identity. Definitions use
canonical lowercase ASCII IDs; localized keys provide player-facing names.

## Shared physiological model

Ancestries modify common systems rather than owning isolated minigames:

- body size and mass;
- movement and work stamina;
- breathable atmosphere and vacuum tolerance;
- comfortable gravity, temperature, and light;
- sleep or trance requirements;
- nutrition type and consumption rate;
- healing, toxin, disease, and radiation response;
- lifespan and age stage;
- senses and environmental perception; and
- compatibility with equipment, quarters, and medical treatment.

Modifiers should usually change a cost, threshold, recovery rate, or available
option. Flat combat bonuses are used sparingly. Every advantage must create a
different planning opportunity rather than making one ancestry universally
better.

## Ancestry roster

### Humans

Humans are widespread generalists whose settlements survive through social and
technical adaptation. Their baseline feature is **Versatility**: one starting
aptitude may be reassigned during recruitment. Their bodies have no extreme
environmental adaptation and depend on ordinary atmosphere, food, sleep, and
medical support.

| Lineage | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Hearthworld | `lineage.human.hearthworld` | Faster recovery in comfortable gravity and atmosphere | Suffers acclimation penalties sooner in extreme environments |
| Voidborn | `lineage.human.voidborn` | Lower food use and better low-gravity movement | Reduced tolerance for high gravity and heavy recoil |
| Emberworld | `lineage.human.emberworld` | Better heat and dehydration tolerance | Cold exposure accumulates faster |

### Elves

Elves are long-lived beings whose nervous systems sense patterns in light and
the void's aetheric currents. **Aether Sense** can reveal weak anomalies or
unstable routes before ordinary instruments, but intense interference causes
sensory strain. Elves enter a short trance instead of normal sleep; they still
need safe downtime.

| Lineage | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Dawnweave | `lineage.elf.dawnweave` | Gains focus in strong natural or stellar light | Darkness increases fatigue unless quarters provide tuned lighting |
| Gloamroot | `lineage.elf.gloamroot` | Excellent low-light sight and reduced sensory signature | Bright or rapidly changing light causes strain |
| Glassleaf | `lineage.elf.glassleaf` | Reads crystalline and aetheric material with unusual precision | Resonant machinery and storms can overwhelm that sense |

### Half-elves

Half-elves are a first-class ancestry in the initial content model, representing
characters with both human and elven heritage. **Dual Heritage** lets a
character select one minor human adaptation and one minor elven sense, each at
reduced strength. It does not determine culture or guarantee social acceptance.

| Lineage | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Concord | `lineage.half-elf.concord` | Switches between normal sleep and trance when quarters permit | Neither rest mode is as efficient as its specialist form |
| Starling | `lineage.half-elf.starling` | Adapts quickly to changing gravity and duty schedules | Needs more recovery after repeated schedule changes |
| Threshold | `lineage.half-elf.threshold` | Learns unfamiliar languages and customs faster | Starts with fewer specialist skill ranks |

Future mixed-heritage support may generalize parentage, but released save IDs
for half-elves must remain valid through an explicit migration.

### Dwarves

Dwarves are compact, dense-bodied people adapted to confined habitats and
demanding physical environments. **Braced Stance** reduces forced movement and
work interruption. Their mass raises acceleration costs, and cramped does not
mean weightless: ship layout and rescue equipment must support them.

| Lineage | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Deepforge | `lineage.dwarf.deepforge` | Exceptional high-gravity and heat tolerance | Low gravity reduces precision until acclimated |
| Cometdelver | `lineage.dwarf.cometdelver` | Detects structural weakness and valuable mineral seams | Requires more calories during heavy work |
| Brasswake | `lineage.dwarf.brasswake` | Maintains concentration amid vibration and machinery noise | Quiet natural environments provide less effective recreation |

### Orcs

Orcs are powerfully built and recover well from exertion. **Second Wind** allows
a controlled burst of work or combat stamina followed by a visible recovery
debt. Orcs are not inherently violent, unintelligent, or hostile; individuals
and cultures determine their values and behavior.

| Lineage | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Redwake | `lineage.orc.redwake` | Sustains strenuous labor and emergency damage control | Higher food and oxygen consumption while exerting |
| Stormborn | `lineage.orc.stormborn` | Better resistance to electrical and aether-storm injury | Medical recovery uses more conductive supplies |
| Greenmoon | `lineage.orc.greenmoon` | Heals minor wounds quickly with adequate food and rest | Starvation and sleep loss suppress regeneration sharply |

### Vampires

Vampires are transformed people sustained by blood or a setting-specific vital
essence. In character creation they form a major origin group; in simulation
they retain a base ancestry and add `condition.vampiric` plus a bloodline.

All vampires share **Unliving Physiology**: they do not breathe, resist vacuum
and common disease, and can remain active without ordinary food. They instead
track thirst. Starvation does not silently force harmful behavior; it unlocks
explicit risks, consent policies, restraint options, and command decisions.
Radiant exposure impairs healing and may cause injury depending on protection.

| Bloodline | Stable ID | Feature | Cost |
| --- | --- | --- | --- |
| Crimson | `bloodline.vampire.crimson` | Converts fresh blood into rapid healing and strength | Thirst rises faster and stored blood spoils without refrigeration |
| Umbral | `bloodline.vampire.umbral` | Conceals presence and sees clearly in near darkness | Direct stellar light is especially dangerous |
| Ashen | `bloodline.vampire.ashen` | Tolerates filtered daylight and long dormant passages | Healing and physical bursts consume more essence |

Ship policy must record who may donate blood, when stores may be consumed, and
how emergencies are handled. Consent, trust, faction law, and scarcity create
the conflict; vampirism does not assign morality.

## Skills and shipboard roles

Ancestry never locks a role. Any character can train any vocation when their
body can use the required equipment. The first role set is planned as:

- captain: command, negotiation, morale, and voyage policy;
- pilot: maneuvering, docking, and evasive travel;
- navigator: chart interpretation, anomaly sensing, and route prediction;
- deckhand: cargo, rigging, EVA, rescue, and general maintenance;
- artificer: drive, module, weapon, and hull repair;
- surgeon: medicine, prosthetics, blood storage, and quarantine;
- gunner: ship weapons, point defense, and boarding support; and
- envoy: trade, language, customs, intelligence, and faction relations.

Roles describe current duty. Skills improve through practice and instruction;
injury, fatigue, equipment, relationships, and environment affect performance.

## Character generation

Given a content revision, scenario, and explicit character seed, generation
must be deterministic:

1. Select an allowed ancestry and compatible lineage.
2. Select culture and upbringing independently where scenario rules permit.
3. Generate age stage, body parameters, name seed, pronouns, and appearance.
4. Allocate vocation skills and one or more personal aptitudes.
5. Add bounded traits, beliefs, relationships, memories, and conditions.
6. Validate equipment, atmosphere, quarters, diet, and medical compatibility.
7. Publish the character only after the complete definition passes validation.

Scenarios may provide authored characters or weighted generation tables.
Weights affect frequency, never capability ceilings or moral alignment.

## Data and persistence contract

Planned authored definitions live under `Content/Characters/` and compile into
validated bounded artifacts. A lineage definition references its parent rather
than copying the ancestry:

```json
{
  "schemaVersion": 1,
  "id": "lineage.dwarf.cometdelver",
  "ancestryId": "ancestry.dwarf",
  "nameKey": "character.lineage.dwarf.cometdelver.name",
  "descriptionKey": "character.lineage.dwarf.cometdelver.description",
  "grantedRules": [
    "rule.sense.structural-weakness",
    "rule.need.heavy-work-calories"
  ]
}
```

Persistent character state stores definition IDs, definition revision, rolled
parameters, learned state, and mutable conditions. It never stores localized
names as identity. Definition loading must reject duplicate IDs, missing
parents, ancestry cycles, incompatible body rules, unknown modifiers, and
unbounded generation tables before publication.

## Balance rules

- No ancestry receives a universal intelligence, morality, or social-value
  modifier.
- Every lineage begins with one legible strength and one relevant cost.
- Environmental advantages must matter often enough to be worth choosing but
  must not make a lineage mandatory for a role.
- Equipment, training, medicine, and ship design provide alternate solutions
  to most physiological disadvantages.
- Mixed crews should create logistical and social choices, not a hidden optimal
  mono-ancestry strategy.
- Automated crew decisions expose the need, duty, rule, and blocker that caused
  the choice.

## First playable character scope

The first crew-enabled vertical slice should use six authored characters: one
human, elf, half-elf, dwarf, orc, and vampire. Each needs an ancestry, lineage
or bloodline, culture, vocation, two traits, one relationship, and compatible
quarters. The slice needs only four shared needs—rest, nutrition or thirst,
safety, and belonging—and three duties: navigate, salvage, and repair.

The slice succeeds when ancestry changes real voyage decisions, every crew
action has an inspectable reason, and replaying the same seed and commands
produces the same character and ship state. Romance, children, aging campaigns,
full genetic inheritance, unrestricted hybrid ancestry, and dozens of content
variants are explicitly deferred.
