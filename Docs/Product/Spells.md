# Spells

## Status

This document defines the planned spellcasting system and authored spell
catalog for Spelljammer. None of these spells are implemented yet. It owns
casting, resources, disciplines, resistance, learning, persistence, spell
identity, content boundaries, and rollout scope.

Numbers such as costs, ranges, and durations remain balancing data until the
character-action prototype establishes its tick scale. A spell must still
declare those values before it can become playable.

## Catalog principles

- Spells are learned techniques, not class features. Any character can pursue
  their training, but active casting requires `access.magic` from the learned
  Spellcasting Training Feat or an explicit innate Talent.
- Complexity runs from 1 through 5 and describes execution difficulty and
  infrastructure. It is not a spell level or a character level.
- Every spell has a stable ID. Renaming its display text does not change saved
  identity.
- A spell produces one bounded, inspectable effect. Broad narrative wishes are
  not valid spell definitions.
- Costs, targets, range, duration, traces, resistance, interruption, and
  friendly-fire behavior are visible before commitment when the caster could
  reasonably know them.
- Stronger effects require proportionally visible costs, preparation,
  exposure, limited targets, scarce components, sustained attention, or ship
  infrastructure.
- Magic creates problems and options without replacing Medicine, Engineering,
  navigation, social skills, equipment, or crew positions.
- Permanent bindings belong to Enchantment. Psychic contact and influence
  follow [`PsychicAbilities.md`](PsychicAbilities.md).

## System boundary

Magic manipulates aether through learned spell patterns. It casts active spells
and sustains temporary effects. Enchantment binds persistent effects into
prepared items, ship modules, or locations; Alchemy transforms physical and
magical reagents into consumable compounds; Psionics acts through minds,
perception, focus, and psychic strain.

The systems can cooperate without replacing one another. A crafted blade may
require Crafting for its body, Alchemy for a conductive compound, Enchantment
for a persistent binding, and Magic to activate a stored spell.

Spelljammer has no mage class, spellcasting class level, or class-restricted
spell list. Any character may improve `skill.magic`, study spell theory, and
train toward magical access. Ship Mage and Warden are crew positions rather
than classes and do not themselves allow casting.

## Magical access

To cast, a character must have `access.magic`. It comes from one of two sources:

| Source | Stable ID | Rule |
| --- | --- | --- |
| Learned Feat | `feat.access.magic` | Earned by completing Spellcasting Training |
| Innate Talent | Talent-specific ID | A Race or Heritage Talent may explicitly grant `access.magic` |

The Elf Race Talent **Aether Sense** is the initial innate example. It grants
magical access and its own bounded sensing effect, so an Elf does not need the
Spellcasting Training Feat. It grants no free Magic ranks and does not teach
spells automatically.

Magical access, a known spell, and casting competence are separate. A character
must have `access.magic`, know the chosen spell ID, and meet that spell's skill,
resource, target, and contextual requirements. Without access, a character may
translate a grimoire, identify aether traces, assist a ritual in an allowed
non-casting role, or study Magic, but cannot declare a cast command.

Completing training grants access atomically. A high Magic skill, focus item,
charged artifact, spellbook, or ship station never substitutes for the access
Feat or an explicit innate Talent.

## Casting sequence

A spell action follows seven explicit phases:

1. **Declare:** choose the known spell, legal targets, approach, and bounded
   parameters.
2. **Preview:** show known costs, requirements, range, resistance,
   friendly-fire risk, environmental modifiers, and uncertainty.
3. **Reserve:** atomically reserve reagents, charge, attention, stations, and
   other required resources.
4. **Prepare:** perform required positioning, words, gestures, inscriptions,
   focus work, or ritual contributions.
5. **Resolve:** combine the chosen attribute, `skill.magic`, equipment,
   assistance, circumstances, and owned deterministic random stream.
6. **Commit:** publish effects, consumed resources, events, and observable
   traces together.
7. **Recover:** apply cooldown, fatigue, Focus loss, or continuing channel
   demands.

Failure before reservation changes no state. Interruption after reservation
uses the spell's explicit refund and backlash rules; it cannot duplicate
resources or partially publish a successful effect.

Willpower commonly controls or sustains magic, Intelligence analyzes patterns
and constructs rituals, Agility handles precise casting under motion,
Toughness endures backlash, and Charisma supports magic expressed through
performance or command. The spell and circumstances select an allowed
attribute; Magic is not permanently bound to one.

## Magical resources

| Resource | Meaning |
| --- | --- |
| Focus | Short-term mental attention used to shape or sustain a spell |
| Stamina | Physical exertion, breath, pain, or fatigue caused by casting |
| Aether charge | Energy drawn from the environment, a crystal, an item, or an Arcane ship network |
| Reagent | A tagged substance consumed, damaged, or transformed by the spell |
| Item charge | Bounded energy stored in a wand, charm, weapon, tool, or module |
| Time | Preparation, channeling, ritual, recovery, or exposure measured in simulation ticks |

Aether is not a free universal mana pool. Each source declares capacity,
recharge, ownership, contamination, and connection. Aether-rich regions can
increase availability and instability; aether-dead regions can raise costs or
block tagged spells. Known restrictions are exposed before commitment.

## General limits and counterplay

- Magic cannot create permanent matter, fuel, food, blood, or wealth from
  nothing.
- Vitality supports Medicine and recovery; it does not casually restore the
  dead or erase lasting consequences.
- Seeking returns bounded observations with source and confidence. It cannot
  reveal arbitrary hidden state, guarantee prophecy, or read minds.
- Passage requires valid destinations, range, capacity, and usually prepared
  anchors. It cannot bypass the galaxy graph without an explicit route.
- Veiling changes available evidence; it cannot rewrite memories.
- Magic cannot override another mind. Psychic influence uses the consent and
  resistance rules in [`PsychicAbilities.md`](PsychicAbilities.md).
- Permanent effects require Enchantment and a compatible prepared target.
- Time travel and alteration of committed history are outside the system.

Counterplay can include interruption, distance, cover, resistance, dispelling,
aether starvation, grounding materials, wards, environmental interference, or
a competing spell. Immunity and resistance come from explicit tags and effects
rather than hidden narrative exceptions.

## Spell record

Every catalog entry expands into a validated data definition containing:

| Field | Purpose |
| --- | --- |
| Stable ID and localized keys | Persistent identity separated from display text |
| Required access ID | Capability gate checked before a cast can be declared |
| Discipline tags | Discovery, teaching, equipment, resistance, and counterspell grouping |
| Complexity | Bounded difficulty from 1 through 5 |
| Form | Immediate, sustained, channeled, or ritual |
| Approach | Allowed attributes and the recommended attribute-skill pairing |
| Timing | Preparation, action, recovery, duration, and sustain intervals in simulation ticks |
| Delivery | Self, touch, projectile, line, area, anchor, link, or connected ship network |
| Target rules | Legal tags, capacity, line of effect, visibility, consent, and friendly fire |
| Costs | Focus, Stamina, aether charge, reagents, item charge, time, stations, and assistants |
| Effect | Bounded magnitude, stacking group, duration, and termination rule |
| Counterplay | Resistance, interruption, cover, wards, dispelling, grounding, or environmental limits |
| Evidence | Light, sound, heat, aether signature, residue, witnesses, and identification difficulty |
| Failure | Refund, fizzle, deviation, weaker result, backlash, and contamination rules |
| Learning | Mentor, script, lore, facility, practice, and prerequisite requirements |

A definition is incomplete if it relies on prose such as "normal range" or
"reasonable damage." Authored data uses shared range, damage, condition, and
resource IDs whose meanings are defined once.

Cast-command validation checks `requiredAccessId` before reserving resources.
The rejection is explicit and cannot be bypassed by calling a spell effect
directly from equipment, UI, or event content unless that source is itself
authored to cast and pay for the spell.

## Complexity and availability

| Complexity | Typical content | Expected access |
| ---: | --- | --- |
| 1 | Utility, signaling, diagnosis, minor movement, and brief protection | Personal casting with basic training |
| 2 | Strong utility, controlled hazards, multiple targets, or sustained effects | Practiced caster, focus item, or modest reagent |
| 3 | Encounter-defining control, compartment-scale effects, and difficult counters | Specialist training, rare reagent, assistant, or prepared site |
| 4 | Ship-scale effects, dangerous transit, and major rituals | Connected modules, several crew, and significant preparation |
| 5 | Rare strategic workings tied to unique discoveries or locations | Campaign project with faction, artifact, and world consequences |

Complexity never grants automatic access to lower-complexity spells. A
character knows a spell only when its stable ID is present in their known-spell
collection. Complexity 4 and 5 content is exceptional and should not become a
routine combat rotation.

## First playable spells

The first crew-encounter milestone uses four Complexity 1 spells. Their final
numeric values will be data-authored, but their behavioral contracts are fixed
below.

### Lantern Spark

| Property | Definition |
| --- | --- |
| Stable ID | `spell.radiance.lantern-spark` |
| Discipline | Radiance |
| Form and delivery | Immediate; self, touch, or near point |
| Approach | Charisma or Willpower plus Magic |
| Costs | Low Focus; optional aether charge to extend brightness or duration |
| Effect | Creates a bounded light or an unmistakable visible signal with chosen permitted color and pulse pattern |
| Counterplay | Cover, distance, bright ambient light, Radiance suppression, or dispelling |
| Evidence | Visible light, slight heat, and a short-lived aether trace |
| Failure | No light or a shorter, dimmer pulse; never an uncontrolled fireball |

Lantern Spark illuminates; it does not blind by default, ignite arbitrary
materials, encode unlimited information, or impersonate an authenticated
faction signal. Signal protocols can require Language and Literacy or faction
knowledge.

### Vector Tether

| Property | Definition |
| --- | --- |
| Stable ID | `spell.vectoring.vector-tether` |
| Discipline | Vectoring |
| Form and delivery | Sustained; near visible object |
| Approach | Agility or Willpower plus Magic |
| Costs | Low Focus plus a sustain cost |
| Effect | Pulls, pushes, slows, or holds one small unattended object within a declared force limit |
| Counterplay | Mass, anchoring, cover, distance, competing force, interruption, or dispelling |
| Evidence | Visible distortion, object movement, and an aether signature along the tether |
| Failure | The tether fails to attach or releases without imparting extra momentum |

Held, living, anchored, hazardous, or ship-scale targets require a different
spell with explicit resistance and safety rules. Vector Tether cannot provide
free continuous propulsion.

### Brace Ward

| Property | Definition |
| --- | --- |
| Stable ID | `spell.warding.brace-ward` |
| Discipline | Warding |
| Form and delivery | Sustained; self, touch, or connected Arcane ship network |
| Approach | Willpower or Intelligence plus Magic |
| Costs | Focus and aether charge; sustain or fixed bounded duration |
| Effect | Grants one character, module, or compartment bounded resistance to an identified impact or pressure hazard |
| Counterplay | Damage beyond capacity, an uncovered hazard tag, interruption, aether starvation, or dispelling |
| Evidence | A visible ward pattern under stress and detectable aether residue |
| Failure | Protection does not commit; reserved resources follow the declared interruption rule |

Brace Ward mitigates tagged harm rather than granting universal immunity. It
does not repair existing damage and cannot replace a hull, pressure seal, or
protective equipment.

### Aether Trace

| Property | Definition |
| --- | --- |
| Stable ID | `spell.seeking.aether-trace` |
| Discipline | Seeking |
| Form and delivery | Channeled; self-centered near survey |
| Approach | Intelligence plus Magic |
| Costs | Focus and survey time |
| Effect | Reveals bounded evidence of recent nearby magical activity, including age band, direction, and broad discipline when supported by evidence |
| Counterplay | Elapsed time, distance, contamination, veiling, false traces, shielding, and environmental noise |
| Evidence | The caster visibly concentrates and emits a detectable searching pulse |
| Failure | Returns no conclusion or an explicitly uncertain impression, never fabricated certainty |

Aether Trace reports evidence with source and confidence. Ancient Lore,
Enchantment, or Language and Literacy may be needed to interpret a discovered
signature, artifact, or inscription.

## Planned discipline catalog

The following entries establish the intended breadth of the catalog. They are
content candidates, not promises for the first playable slice, and each still
requires a complete validated spell record.

### Warding

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Brace Ward | `spell.warding.brace-ward` | 1 | Mitigate one identified impact or pressure hazard |
| Threshold Seal | `spell.warding.threshold-seal` | 2 | Mark a doorway or hatch and resist one declared crossing condition |
| Spellbreak Lattice | `spell.warding.spellbreak-lattice` | 3 | Contest active magic inside a bounded prepared area |
| Haven Circuit | `spell.warding.haven-circuit` | 4 | Sustain compartment-scale protection through a connected Ward Projector |

### Vectoring

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Vector Tether | `spell.vectoring.vector-tether` | 1 | Move or secure one small unattended object |
| Driftstep | `spell.vectoring.driftstep` | 2 | Redirect the caster's bounded movement in low gravity |
| Gravity Knot | `spell.vectoring.gravity-knot` | 3 | Create a short-lived area of increased or redirected pull |
| Keel Turn | `spell.vectoring.keel-turn` | 4 | Assist one declared ship maneuver through a connected Arcane network |

### Radiance

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Lantern Spark | `spell.radiance.lantern-spark` | 1 | Create light or an obvious visual signal |
| Heat Draw | `spell.radiance.heat-draw` | 2 | Move bounded heat from one target into a prepared sink |
| Starflare Beacon | `spell.radiance.starflare-beacon` | 3 | Emit a powerful detectable signal with a chosen public pattern |
| Dawn Array | `spell.radiance.dawn-array` | 4 | Illuminate or heat a ship-scale area through connected projectors |

### Veiling

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Quiet Silhouette | `spell.veiling.quiet-silhouette` | 1 | Reduce one subject's obvious visual outline while it moves slowly |
| False Wake | `spell.veiling.false-wake` | 2 | Create a bounded misleading aether trail with discoverable inconsistencies |
| Masked Hold | `spell.veiling.masked-hold` | 3 | Obscure declared properties of cargo from specified magical senses |
| Ghost Rig | `spell.veiling.ghost-rig` | 4 | Alter a ship's apparent profile without changing its physical collision body |

### Shaping

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Seam Press | `spell.shaping.seam-press` | 1 | Temporarily close a small clean split until repaired |
| Cutline | `spell.shaping.cutline` | 2 | Weaken a marked line in compatible unattended material |
| Hullskin | `spell.shaping.hullskin` | 3 | Temporarily reinforce a bounded prepared surface |
| Formwright Chorus | `spell.shaping.formwright-chorus` | 4 | Reshape compatible ship material through a staffed ritual station |

Shaping changes existing matter temporarily unless an Enchantment or Crafting
project makes an explicitly supported result permanent. It cannot generate
valuable material from nothing.

### Seeking

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Aether Trace | `spell.seeking.aether-trace` | 1 | Find evidence of recent nearby magic with uncertainty |
| Fault Echo | `spell.seeking.fault-echo` | 2 | Identify stress or disruption in a visible object or connected module |
| Waymark Compass | `spell.seeking.waymark-compass` | 3 | Determine direction and confidence toward a prepared known anchor |
| Starway Sounding | `spell.seeking.starway-sounding` | 4 | Survey a nearby Starway entrance with ship instruments and crew assistance |

### Vitality

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Steady Pulse | `spell.vitality.steady-pulse` | 1 | Stabilize one living target while treatment is prepared |
| Draw Taint | `spell.vitality.draw-taint` | 2 | Assist treatment of one identified poison or contamination into a prepared vessel |
| Borrowed Breath | `spell.vitality.borrowed-breath` | 3 | Sustain limited respiration briefly while a known hazard is addressed |
| Sanctuary Vigil | `spell.vitality.sanctuary-vigil` | 4 | Support several patients during a staffed medical ritual |

Vitality exposes diagnosis requirements, consent, side effects, and medical
follow-up. It stabilizes and supports natural recovery; it does not resurrect
the dead, regrow anything without a dedicated system, or erase injury history.

### Passage

| Spell | Stable ID | Complexity | Intended effect |
| --- | --- | ---: | --- |
| Anchor Step | `spell.passage.anchor-step` | 2 | Move the caster a short distance to a visible prepared mark |
| Paired Threshold | `spell.passage.paired-threshold` | 3 | Open brief transit between two nearby authenticated anchors |
| Cargo Aperture | `spell.passage.cargo-aperture` | 4 | Transfer bounded tagged cargo between staffed prepared sites |
| Starway Accord | `spell.passage.starway-accord` | 5 | Alter access to one discovered Starway under unique authored conditions |

Passage validates destination, capacity, occupancy, obstruction, and failure
placement before reserving resources. It cannot select an unknown destination,
bypass the galaxy graph, duplicate cargo, or strand half an entity across a
commit boundary.

## Variants and scaling

A spell definition may expose bounded parameters such as duration band, area
shape, signal pattern, warded hazard, or number of targets. Increasing a
parameter raises declared costs and complexity according to authored rules; it
does not invite free-form effect construction.

Materially different behavior receives a new spell ID. Balance-only data can
be revised in a schema-compatible definition, while a change that would make a
saved effect mean something different requires a new ID or save migration.

Equipment, Talents, assistance, environment, and ship modules may modify a
spell through explicit tags. Modifiers are bounded and their order is stable.
No source may silently turn a personal spell into an unlimited ship-scale
effect.

## Failure, evidence, and persistence

A failed spell may consume bounded resources, miss its target, create a weaker
effect, leave a visible trace, damage a focus, or cause authored backlash. It
does not select from an unlimited catastrophe table. Environmental changes and
continuing effects resolve on the fixed simulation tick.

Every cast declares what observers can detect, such as light, sound, heat,
aether signature, altered matter, or residue. Magic, Enchantment, Ancient Lore,
sensors, and ordinary witnesses may identify different parts of that evidence.
Concealment changes observations without removing authoritative causality.

Persistent character state stores known spell IDs and active learning projects.
Active effects store their source spell, caster, targets, magnitude, start tick,
bounded duration, sustaining source, and termination rule. Saves also preserve
reserved resources and interrupted ritual state when allowed by the versioned
save contract.

## Learning sources and scripts

Spell records appear in living instruction, field notes, academy manuals,
faction archives, enchanted objects, ruin inscriptions, and ancient devices.
Each source declares language, script, completeness, authenticity, and required
Ancient Lore domain.

Translation can reveal a spell's existence before it makes the pattern safe to
practice. Characters record discovery, translation, verification, practice,
and mastery as separate project state. Different cultures may preserve the
same spell ID through different terminology or notation without making one
script universally magical.

Teaching access can depend on faction membership, standing, payment, duty,
theft, salvage rights, or negotiation. Those gates control availability, not
who is metaphysically permitted to learn Magic.

Spellcasting Training and spell learning are separate projects. A character
may complete them in either order, but cannot cast until both `access.magic`
and the specific known-spell ID are present. A racial Talent that grants innate
access replaces only the access project, not spell learning.

## Ship spells and rituals

Personal spells operate on ship targets only when their target and capacity
rules allow it. Ship-scale spells require explicit stations and connected
modules such as an Aether Dynamo, Runic Distributor, Ward Projector, or Flux
Sail. Operators reserve module capacity and aether charge through the same
transaction as the spell.

Ritual entries additionally declare leader, assistants, work slots, required
skills, materials, interruption behavior, and commit point. Assistants may
contribute Magic, Enchantment, Engineering, Ancient Lore, Language and
Literacy, or another listed skill, but assistance never stacks without a cap.

Industrial ships can cast supported ship spells through explicit converters,
isolated Arcane modules, charged devices, or visiting specialists. The catalog
does not impose an invisible technology-path prohibition.

## Content validation

Before a spell definition is published, tooling must reject:

- missing or reused stable IDs and localization keys;
- unknown discipline, target, effect, resource, range, or failure references;
- negative costs, unlimited collections, durations without termination, or
  effects whose work grows without a cap;
- non-consensual character effects without resistance and hostility rules;
- information effects that reveal unrestricted simulation state;
- Passage effects without destination, capacity, obstruction, and rollback
  rules;
- Vitality effects that bypass the injury and Medicine contracts;
- ship effects without compatible network, station, ownership, and allocation
  checks;
- recursive triggers, unsafe stacking, partial publication, and resource
  duplication paths; and
- player-visible text embedded in authoritative simulation data.

Validation occurs before replacing a working catalog. Failed content reloads
leave the previous validated definitions active and report actionable errors.

## Delivery order

1. Implement the four first-playable spells and their shared target, cost,
   evidence, interruption, and save contracts.
2. Add one counterspell interaction, one translated spell-learning project,
   and one Arcane ship-assisted cast.
3. Expand each discipline only when it creates a tested voyage, crew,
   exploration, or encounter decision.
4. Introduce Complexity 4 and 5 rituals after module networks, multi-crew work,
   faction consequences, and save migrations are reliable.

The catalog grows through distinct systemic uses, not by adding near-duplicate
damage spells. Procedural spell generation, unrestricted wishes, routine
resurrection, time travel, and permanent creation of matter remain outside the
planned scope.
