# Character skills

## Status

This document defines the planned classless skill, language, script, and lore
systems. They are not implemented yet. Broad capability is defined in
[`Attributes.md`](Attributes.md), while race, heritage, identity, and crew
positions are defined in [`Races.md`](Races.md).

## Classless skill system

Characters have no class, global character level, class skill, or class-locked
ability. A background answers “what has this person done before?” but does not
decide what they can become. Every character may improve every skill, equip any
compatible item, perform any ship duty, and learn any technique whose explicit
requirements they meet.

Skills express learned competence and advance independently. Their planned
player-facing range is 0–100. A character can be an excellent pilot and novice
navigator, or a skilled doctor who later learns swordplay, without selecting or
changing a class.

Talents are not skills. They are racial feats granted by Race and Heritage,
have no 0–100 rating, and do not improve through practice. See
[`Races.md`](Races.md) for their rules.

## Skill catalog

| Area | Initial skills |
| --- | --- |
| Movement | athletics, acrobatics, EVA, stealth |
| Voyage | piloting, astrogation, sensors, rigging |
| Ship work | engineering, salvage, crafting |
| Combat | melee, archery, gunnery, defense |
| Mystic | magic, psionics |
| Care and knowledge | medicine, alchemy, cooking, xenology, ancient lore, language and literacy |
| Social | command, insight, persuasion, deception, trade |

The following skills form the core of personal combat, item creation, crew
care, and discovery:

| Skill | Stable ID | Scope and boundaries |
| --- | --- | --- |
| Magic | `skill.magic` | Casts, controls, identifies, and counters learned spells. Individual spells are techniques or discoveries, not separate classes. |
| Psionics | `skill.psionics` | Detects, projects, shields, and shapes thought through learned psychic techniques. Every effect defines its target, range, consent or resistance rule, and strain cost. |
| Melee | `skill.melee` | Covers unarmed combat and hand-held weapons. Weapon tags and learned techniques create differences without separate weapon classes. |
| Archery | `skill.archery` | Covers bows, crossbows, unusual string weapons, ammunition choice, and aimed physical projectiles. Ship guns and personal firearms use Gunnery. |
| Alchemy | `skill.alchemy` | Identifies reagents and produces medicines, stimulants, toxins, blood substitutes, volatile mixtures, and magical compounds. |
| Crafting | `skill.crafting` | Fabricates and improves personal equipment, ammunition, tools, fittings, and replacement parts from known designs. |
| Cooking | `skill.cooking` | Plans meals, preserves provisions, safely handles unusual diets, and turns limited ingredients into nutrition and morale. |
| Ancient Lore | `skill.ancient-lore` | Recognizes lost civilizations, obsolete customs, relic functions, old routes, and the historical context of discoveries. |
| Language and Literacy | `skill.language-literacy` | Governs speaking, understanding, reading, writing, translation, and script decipherment for learned languages and scripts. |

Engineering diagnoses, operates, and repairs ship systems; Crafting manufactures
their components. Salvage extracts usable material; Alchemy transforms suitable
reagents. These skills can cooperate in one job without collapsing into a
single universally optimal skill.

Magic is available to every character who learns a spell and can meet its
requirements. Casting may consume stamina, focus, reagents, stored charge, or a
setting-specific resource defined by the spell. Armor, race, heritage, or
background may alter those costs but never imposes a hidden class restriction.

Psionics is likewise learnable by any character. Training, psychic implements,
or an explicit Talent can provide access to its techniques. The Somnari
Mindwake Talent supplies innate access to basic psychic contact, not free skill
ranks or unrestricted mind reading. Failed or resisted psychic actions may
create strain, distorted impressions, or detectable psychic feedback.

## Action resolution

Skills do not have one permanently governing attribute. Each action selects the
attribute appropriate to its method and context. The interface must show the
selected attribute, skill, equipment, help, conditions, and difficulty before
or after resolution as appropriate.

An action's capability is derived from:

```text
relevant attribute + relevant skill + equipment + assistance
    + situational modifiers + deterministic random result
```

The exact formula is deferred until implementation and balancing. Random
results must come from an explicitly owned seeded stream, and the action record
must preserve enough information to reproduce the outcome.

## Advancement

- Using a skill against a meaningful challenge grants bounded practice for that
  skill; trivial repeated actions do not provide unlimited advancement.
- Instruction, manuals, simulators, and downtime projects can grant practice at
  a resource and time cost.
- High skill ranks require progressively more practice and may require a
  teacher, facility, discovery, or dangerous field experience.
- Techniques unlock from skill thresholds, attributes, relationships,
  equipment, discoveries, or conditions—not from class levels.
- Practice awards are deterministic, capped per committed action, and included
  in the authoritative command result.
- Enemies and encounters do not automatically scale to a global character
  level. Location, faction, world state, and authored threat determine danger.

## Languages, scripts, and ancient lore

Language knowledge is important expedition equipment. An unread warning can be
as dangerous as a damaged hull, while a correctly interpreted ledger, epitaph,
or star chart can reveal routes, recipes, claims, allies, and sealed locations.

The model separates three concepts:

- a **language** is a spoken, signed, or otherwise communicated system;
- a **script** is a writing system that can encode one or more languages; and
- **lore** is contextual knowledge about an era, people, event, place, object,
  ritual, or belief.

Learning a language or script adds it to a character's known knowledge. The
single Language and Literacy skill determines how effectively the character
speaks, understands, reads, writes, translates, and deciphers what they know.
Knowing a script does not guarantee knowledge of every language written with
it, and translating words does not supply their historical meaning.

### Script traditions

Each race has an associated script tradition, but scripts are learned
inventions rather than biological abilities. Heritage determines starting
language and script knowledge. Anyone may learn another script, and communities
may adopt, adapt, or reject the script associated with their race.

| Tradition | Stable ID | Common association | Visual and practical character |
| --- | --- | --- | --- |
| Tidemark | `script.tidemark` | Human ports and merchant fleets | Compact strokes suited to ledgers, cargo seals, and rapidly amended charts |
| Lumenbranch | `script.lumenbranch` | Elven observatories and memory houses | Branching marks whose spacing can encode emphasis and aetheric relationships |
| Bridgehand | `script.bridgehand` | Half-elven enclaves and mixed crews | A phonetic hand designed to represent borrowed sounds and switch languages cleanly |
| Deepcut | `script.deepcut` | Dwarven holds, foundries, and hullwrights | Angular marks readable when carved, stamped, or felt through smoke and darkness |
| Knotstroke | `script.knotstroke` | Orc convoy clans and oath circles | Connected strokes derived from route cords, banners, and witnessed agreements |
| Coilscript | `script.coilscript` | Gnome workshops, schools, and compact habitats | Nested curves combine ordinary prose with measurements and mechanical relationships |
| Patchsign | `script.patchsign` | Goblin flotillas, markets, and maintenance crews | Modular marks remain legible when stenciled, rearranged, or repaired on reused material |
| Dreamtrace | `script.dreamtrace` | Somnari sanctuaries, navigators, and memory circles | Layered curves annotate ordinary language with emotion, certainty, and the source of a remembered impression |
| Sable Cipher | `script.sable-cipher` | Vampire courts, refuges, and blood archives | Layered notation separating public text from heritage, debt, and feeding records |

Sable Cipher is associated with vampire communities but is not automatic race
knowledge. Half-elves likewise do not automatically know Bridgehand or the
scripts used by either parent's community.

Living scripts may have regional hands and historical forms. These are
definition variants under one stable script identity unless they require a
genuinely different decipherment rule. Ancient or unknown scripts use their own
IDs and are never mislabeled as a modern race's writing merely because
their glyphs look similar.

### Language and Literacy skill

Each character stores one bounded `skill.language-literacy` value rather than
separate speaking, reading, and writing skills. The character also stores
bounded sets of known language and script IDs. Heritage, background, authored
personal history, study, and discoveries add knowledge; race alone does not.

Reading an unfamiliar discovery proceeds through explicit stages:

1. **Identify:** recognize the script family, age, medium, and signs of forgery.
2. **Transliterate:** convert visible marks into known symbols or sounds.
3. **Translate:** determine literal meaning using known languages and the skill.
4. **Contextualize:** use Ancient Lore to understand obsolete names, metaphors,
   laws, rituals, dates, and technical assumptions.
5. **Authenticate:** compare material, authorship, provenance, and conflicting
   sources before acting on the result.

Language and Literacy drives identification, transliteration, translation,
speaking, reading, and writing. Ancient Lore drives context and authentication.
Intelligence can notice patterns in damaged or hidden marks and reconstruct
missing text, while Willpower can read magical or psychic impressions. A single
check may combine several characters, tools, or ship facilities while
preserving who contributed each result.

Failure creates uncertainty, partial readings, wasted time, or a dangerous
interpretation; it does not silently replace the original inscription. The UI
must distinguish observed glyphs, transliteration, proposed translation,
confidence, known alternatives, and confirmed lore.

### Lore knowledge

`skill.ancient-lore` measures a character's ability to reason about the past;
specific facts are persistent discoveries with stable IDs. A high skill does
not conjure facts that the crew has never encountered. Lore can be acquired
from inscriptions, artifacts, oral histories, archives, mentors, visions, and
comparison between sources.

Lore discoveries may:

- reveal a safe course or hidden sector;
- identify an artifact's purpose, command phrase, or hazard;
- unlock a spell, alchemical recipe, crafting design, or ship upgrade;
- expose an old treaty, ownership claim, blood debt, or faction grievance;
- improve negotiation with someone who values the source;
- distinguish a genuine relic from a forgery; or
- revise an earlier interpretation when stronger evidence appears.

The knowledge model records subject ID, source ID, discoverer, confidence,
translation state, and whether the crew has shared the fact. Rumor, hypothesis,
translation, and confirmed knowledge remain distinct states.

In-world scripts are authored glyph, font, or image assets, not substitutes for
the player's selected UI language. Every inscription needs accessible
transliteration, translation, scale, contrast, and non-visual presentation when
the character has earned that information. Simulation and saves store script,
language, and lore IDs rather than rendered glyphs or translated strings.

## Data and persistence

Skills, languages, scripts, techniques, and lore subjects use stable canonical
IDs and localized presentation keys. Persistent character state stores one
Language and Literacy skill value alongside the other skill values, plus
bounded sets of known language and script IDs. Lore progress uses records keyed
by stable character, knowledge, and source IDs.

Discovery publication validates every reference and replaces the previous
crew-knowledge snapshot atomically. Failed training, translation, or content
loading cannot partially alter known facts or skill progress.

## First playable scope

The first crew slice initially exercises Piloting, Astrogation, Engineering,
Salvage, Medicine, Cooking, Command, and Language and Literacy. The first
encounter milestone adds Magic, Psionics, Melee, Archery, Alchemy, and Crafting
with at least one usable spell, psychic technique, melee technique, ranged
technique, alchemical recipe, and crafted item.

That milestone also includes one ancient ruin inscription. At least two crew
members with different script or lore knowledge must be able to collaborate on
its reading, and the resulting interpretation must change a real voyage choice
without requiring the player to read the fictional glyphs personally.
