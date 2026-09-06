# Spells

## Status

This document defines the planned spell system and catalog. It is not
implemented in the current ship-level expedition prototype.

## Simple magic rule

Magic creates memorable choices, not aether chemistry or ritual accounting. A
spell tooltip shows its **Focus cost**, **range**, **cast time**, and **effect**;
it may also show a short cooldown. A personal spell uses Focus, while a future
ship spell uses Aether from an Arcane module.

Casting is one action: choose a legal target, preview the effect, pay the cost,
and resolve it. A failed spell spends its stated cost and clearly says why it
failed. Only a channeled spell can be interrupted. There are no separate
reservation, preparation, refund, contamination, or detailed ritual systems.

Magic belongs to the advanced Arcane-Industrial setting. Wards and spells can
exist beside engines, reactors, sensors, armor, and energy weapons without
requiring a second layer of simulation. See [`Setting.md`](Setting.md).

## Access and learning

Characters have no mage class. To cast, a character needs:

1. `access.magic`, from `feat.access.magic` (Spellcasting Training) or a
   Racial Perk that explicitly grants it;
2. the spell's stable ID in their known-spell collection; and
3. enough Focus and a legal target.

The Elf Race Perk **Aether Sense** is the initial innate source of
`access.magic`. It grants access but no free Magic ranks or known spells. A
high Magic skill, spellbook, or item never bypasses the access requirement.

Learning a spell is a short, explicit training project: find a source, meet its
stated requirement, spend downtime, and add its spell ID. Sources can be
teachers, books, ruins, factions, or discoveries.

## Schools

There are only three magic schools. Schools organize discovery and teaching;
they are not classes and do not add separate resource systems.

| School | Purpose |
| --- | --- |
| Evocation | Direct Arcane force, fire, and lightning for clear combat effects |
| Illusion | Concealment and believable false sights that can be investigated |
| Divination | Bounded actionable information, including detecting invisible targets |

## Spell definition

Every spell definition contains a stable ID, localized name and description,
required access ID, Focus cost, range, cast time, one bounded effect, and an
optional cooldown. It may additionally declare legal target tags, damage type,
or resistance. Definitions must have bounded targets and a clear end condition.

## First playable spells

The first character-combat slice uses four Tier 1 spells:

| Spell | Stable ID | Focus | Range | Cast time | Effect |
| --- | --- | ---: | --- | --- | --- |
| Magic Missile | `spell.evocation.magic-missile` | Low | Far | Instant | Deal reliable Arcane damage to one visible target. |
| Burning Hands | `spell.evocation.burning-hands` | Low | Near | Instant | Deal fire damage in a short previewed cone. |
| Phantasmal Image | `spell.illusion.phantasmal-image` | Low | Near | Instant | Create one visual decoy that distracts or misleads. |
| Detect Invisibility | `spell.divination.detect-invisibility` | Low | Near | Instant | Reveal nearby invisible subjects or their outline briefly. |

## Catalog

The remaining entries are planned content candidates. Their final numbers are
balance data; their intended use stays short and readable.

### Evocation

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Magic Missile | `spell.evocation.magic-missile` | 1 | Deal reliable Arcane damage to one visible target. |
| Magic Missile Storm | `spell.evocation.magic-missile-storm` | 3 | Split Arcane missiles among several visible targets. |
| Burning Hands | `spell.evocation.burning-hands` | 1 | Deal fire damage in a short, previewed cone. |
| Lightning Bolt | `spell.evocation.lightning-bolt` | 2 | Deal lightning damage along a previewed line. |

Magic Missile is reliable rather than a free answer: it still needs a legal
visible target, Focus, and any defense declared by its definition. Burning
Hands and Lightning Bolt show their area before casting, including allies.

### Illusion

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Invisibility | `spell.illusion.invisibility` | 2 | Hide one subject until it attacks, casts, or the short duration ends. |
| Phantasmal Image | `spell.illusion.phantasmal-image` | 1 | Create one visual decoy that can distract or mislead. |

Illusions change perception; they do not rewrite memory, create physical cover,
or force belief. A successful inspection or detection effect reveals them.

### Divination

| Spell | Stable ID | Tier | Intended effect |
| --- | --- | ---: | --- |
| Detect Invisibility | `spell.divination.detect-invisibility` | 1 | Reveal nearby invisible subjects or their outline briefly. |

Divination reveals bounded actionable information. Detect Invisibility does not
reveal every hidden object, read minds, or bypass an unrelated ward.

## Limits, saves, and delivery

Magic cannot freely create wealth, resurrect the dead, read any mind, time
travel, or bypass the galaxy map. Saves store known spell IDs and active effects
that matter: source ID, target, remaining duration, and caster when relevant.

Content validation rejects duplicate IDs, unknown access or target tags,
negative costs, unbounded targets or durations, and effects that bypass these
limits. Implement the four first-playable spells first; add the remaining three
only when they make combat or exploration more interesting.
