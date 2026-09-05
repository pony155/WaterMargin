# Character attributes

## Status

This document defines the planned attribute model. Character attributes are not
implemented yet. Races, heritages, and complete character composition are
defined in [`Races.md`](Races.md); learned capabilities and action resolution
are defined in [`Skills.md`](Skills.md).

## Purpose

Attributes express broad capability. They answer how a character approaches a
task, while skills answer what the character has learned to do. Spelljammer has
no character classes or global character level.

The normal player-facing attribute range is 1–10. Permanent values change
rarely. Injuries, needs, equipment, supernatural conditions, assistance, and
the environment apply temporary modifiers without rewriting those permanent
values.

## Attribute roster

| Attribute | Stable ID | Governs |
| --- | --- | --- |
| Might | `attribute.might` | Force, lifting, heavy weapons, and resisting forced movement |
| Finesse | `attribute.finesse` | Coordination, balance, delicate manipulation, and reaction |
| Vigor | `attribute.vigor` | Stamina, exertion, physical recovery, and environmental endurance |
| Reason | `attribute.reason` | Analysis, memory, diagnosis, planning, and technical learning |
| Awareness | `attribute.awareness` | Perception, attention, aim, danger detection, and spatial judgment |
| Resolve | `attribute.resolve` | Concentration, fear resistance, pain tolerance, and self-control |
| Presence | `attribute.presence` | Leadership, empathy, intimidation, performance, and negotiation |
| Resonance | `attribute.resonance` | Aetheric and psychic sensitivity, magical or psychic control, and supernatural resistance |

## Contextual use

Attributes are not permanently bound to skills. An action declares the
attribute that matches its method and circumstances:

- Might plus Engineering can force a warped drive housing into position.
- Reason plus Engineering can diagnose why the housing failed.
- Resolve plus Engineering can complete the repair during an aether storm.
- Might plus Melee can deliver a forceful strike.
- Finesse plus Melee can place a precise strike.
- Awareness plus Archery can make a difficult shot.
- Resonance plus Archery can guide an enchanted arrow.
- Awareness plus Psionics can detect an attempted mental contact.
- Resolve plus Psionics can maintain a shield against psychic intrusion.
- Presence plus Psionics can send a clear emotion through a mindlink.
- Presence plus Ancient Lore can explain a discovery to a suspicious faction.

The interface must identify the chosen attribute and explain why it applies.
Alternative approaches should be available when fiction, equipment, and known
techniques support them.

## Race, heritage, and attributes

Race may change physiological rules, while heritage may make an attribute
cheaper to use in a particular environment. Neither assigns intelligence,
morality, or a hard attribute ceiling. For example, a heritage may reduce the
Vigor cost of high-gravity work without receiving a universal Vigor bonus.

This distinction keeps race and heritage relevant to ship design and survival
while allowing any character to become an expert in any field through training
and experience.

## Permanent and temporary change

Permanent attributes may change through:

- prolonged focused training;
- a major campaign milestone;
- aging or recovery from a lasting condition;
- a permanent injury, prosthetic, or bodily modification;
- magical transformation; or
- a rare authored story consequence.

Temporary modifiers come from current state such as fatigue, hunger, thirst,
fear, morale, wounds, gravity, atmosphere, lighting, medication, equipment, and
crew assistance. Sources stack only through explicit bounded rules, and the UI
must show every applied source.

## Data and persistence

Attribute definitions use stable canonical IDs and localized presentation keys.
Persistent character state stores permanent values separately from active
modifier records. Each modifier records a stable source ID, affected attribute,
amount, stacking rule, start tick, and bounded duration or removal condition.

Character creation validates the allowed range and point budget before
publication. Runtime changes produce an event describing the old value, new
value, cause, and authoritative tick. Save migrations are required before
changing a released attribute ID, scale, or meaning.

## First playable scope

The first crew-enabled slice should include all eight attributes because they
form a small stable foundation. It only needs to exercise the attributes used
by navigation, salvage, repair, cooking, medicine, and the first ancient-lore
encounter. Attribute advancement can remain deferred; temporary modifiers and
contextual attribute-plus-skill selection must be visible from the start.
