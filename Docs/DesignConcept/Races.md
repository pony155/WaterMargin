# Races and heritage design

## Status

This document defines planned race, heritage, physiology, and character-creation
rules. These systems are not implemented yet. The current prototype simulates
only ship-level expedition resources.

## Design goals

- Make every crew member mechanically understandable and narratively distinct.
- Support humans, elves, half-elves, dwarves, orcs, gnomes, goblins, Somnari,
  vampires, Eidolons, Kharuun, and future races without hard-coding content
  into simulation code.
- Use classless progression: attributes describe capability, skills improve
  through use and training, and no class limits what a character may learn.
- Give each race recognizable strengths, needs, and complications without
  prescribing personality, morality, intelligence, profession, or faction.
- Use heritage to combine a character's subrace, formative tradition,
  languages, customs, and starting knowledge.
- Let race matter aboard a ship: atmosphere, gravity, food, sleep, medical
  care, room design, and travel hazards should all interact.
- Preserve stable character identities and deterministic generation across
  saves and replays.

## Terminology

The simulation uses **race** for a broad people and their baseline physiology.
It uses **heritage** for the subrace and formative tradition that provide a
focused adaptation, language and script knowledge, customs, and other starting
knowledge. Save data and content definitions use `raceId` and
`heritageId`.

A heritage is not required to match the community stereotypically associated
with a race. Race-specific heritages describe physiological variants, while
shared or adopted heritages represent characters raised in another community.
For example, a dwarf raised in a human port retains the dwarf race while using
a port heritage compatible with dwarves.

## Character composition

A persistent character is composed from independent, stable layers:

| Layer | Examples | Simulation responsibility |
| --- | --- | --- |
| Identity | character ID, name seed, pronouns, portrait seed | Persistence and presentation lookup |
| Race | human, elf, half-elf, dwarf, orc, gnome, goblin, Somnari, vampire, Eidolon, Kharuun | Body plan and baseline physiological rules |
| Heritage | voidborn, deepforge, free-anchorage | Subrace, formative tradition, languages, customs, and starting knowledge |
| Background | academy graduate, dockhand, caravan guard | Starting skill package and history, never a class |
| Attributes | Strength, Agility, Willpower, Intelligence, Luck, Charisma, Toughness | Broad capability shared by many actions |
| Skills | piloting, engineering, merchant, negotiation, language and literacy | Learned competence that improves independently |
| Racial Perks | Versatility, Aether Sense, Braced Stance | Racial feats granted by Race and Heritage |

No translated name or description becomes gameplay identity. Definitions use
canonical lowercase ASCII IDs; localized keys provide player-facing names.

## Shared physiological model

Races modify common systems rather than owning isolated minigames:

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
different planning opportunity rather than making one race universally
better.

## Race roster

### Humans

Humans are widespread generalists whose settlements survive through social and
technical adaptation. Their Racial Perk is **Versatility**: one starting
aptitude may be reassigned during recruitment. Their bodies have no extreme
environmental adaptation and depend on ordinary atmosphere, food, sleep, and
medical support.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Hearthworld | `heritage.human.hearthworld` | Faster recovery in comfortable gravity and atmosphere | Suffers acclimation penalties sooner in extreme environments |
| Voidborn | `heritage.human.voidborn` | Lower food use and better low-gravity movement | Reduced tolerance for high gravity and heavy recoil |
| Emberworld | `heritage.human.emberworld` | Better heat and dehydration tolerance | Cold exposure accumulates faster |

### Elves

Elves are long-lived beings whose nervous systems sense patterns in light and
the void's aetheric currents. Their Racial Perk, **Aether Sense**, can reveal
weak anomalies or unstable routes before ordinary instruments, but intense
interference causes sensory strain. Aether Sense grants innate `access.magic`,
allowing an Elf to learn and cast spells without the Spellcasting Training
Feat; it provides no free Magic ranks or spells. Elves enter a short trance
instead of normal sleep; they still need safe downtime.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Dawnweave | `heritage.elf.dawnweave` | Gains focus in strong natural or stellar light | Darkness increases fatigue unless quarters provide tuned lighting |
| Gloamroot | `heritage.elf.gloamroot` | Excellent low-light sight and reduced sensory signature | Bright or rapidly changing light causes strain |
| Glassleaf | `heritage.elf.glassleaf` | Reads crystalline and aetheric material with unusual precision | Resonant machinery and storms can overwhelm that sense |

### Half-elves

Half-elves are a first-class race in the initial content model, representing
characters with both human and elven parentage. Their Racial Perk, **Blended
Physiology**, lets a character select one minor human adaptation and one minor
elven sense, each at reduced strength. It does not determine heritage or
guarantee social acceptance.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Concord | `heritage.half-elf.concord` | Switches between normal sleep and trance when quarters permit | Neither rest mode is as efficient as its specialist form |
| Starling | `heritage.half-elf.starling` | Adapts quickly to changing gravity and duty schedules | Needs more recovery after repeated schedule changes |
| Threshold | `heritage.half-elf.threshold` | Learns unfamiliar languages and customs faster | Starts with fewer specialist skill ranks |

Future mixed-race support may generalize parentage, but released save IDs
for half-elves must remain valid through an explicit migration.

### Dwarves

Dwarves are compact, dense-bodied people adapted to confined habitats and
demanding physical environments. Their Racial Perk, **Braced Stance**, reduces
forced movement and work interruption. Their mass raises acceleration costs,
and cramped does not mean weightless: ship layout and rescue equipment must
support them.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Deepforge | `heritage.dwarf.deepforge` | Exceptional high-gravity and heat tolerance | Low gravity reduces precision until acclimated |
| Cometdelver | `heritage.dwarf.cometdelver` | Detects structural weakness and valuable mineral seams | Requires more calories during heavy work |
| Brasswake | `heritage.dwarf.brasswake` | Maintains concentration amid vibration and machinery noise | Quiet natural environments provide less effective recreation |

### Orcs

Orcs are powerfully built and recover well from exertion. Their Racial Perk,
**Second Wind**, allows
a controlled burst of work or combat stamina followed by a visible recovery
debt. Orcs are not inherently violent, unintelligent, or hostile; individuals
and communities determine their values and behavior.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Redwake | `heritage.orc.redwake` | Sustains strenuous labor and emergency damage control | Higher food and oxygen consumption while exerting |
| Stormborn | `heritage.orc.stormborn` | Better resistance to electrical and aether-storm injury | Medical recovery uses more conductive supplies |
| Greenmoon | `heritage.orc.greenmoon` | Heals minor wounds quickly with adequate food and rest | Starvation and sleep loss suppress regeneration sharply |

### Gnomes

Gnomes are small-bodied people with extremely sensitive touch and fine spatial
judgment. Their Racial Perk, **Closework**, reduces penalties when manipulating
compact mechanisms or working in confined stations. Their reach and unaided
carrying capacity are limited, and shared equipment must be adjustable rather
than assuming one body scale. Gnomes are not inherently inventive or
scholarly; those qualities come from individual attributes, skills, and
heritage.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Coilwhisper | `heritage.gnome.coilwhisper` | Feels subtle vibration, current, and mechanical imbalance through contact | Heavy vibration and electrical surges cause sensory strain |
| Sporegarden | `heritage.gnome.sporegarden` | Symbiotic microbiome improves food variety and resistance to organic toxins | Sterile quarters and broad-spectrum medicine can disrupt recovery |
| Cloudstep | `heritage.gnome.cloudstep` | Excellent orientation and fine movement in low gravity | High-gravity labor and heavy recoil accumulate fatigue quickly |

### Goblins

Goblins are compact, quick-moving people adapted to crowded habitats and
improvised routes. Their Racial Perk, **Tight Passage**, reduces movement and
work penalties in ducts, wreckage, and congested decks. Their lighter frames
are easier to throw off balance, and standard armor, furniture, and controls
may require refitting. Goblins are not inherently dishonest, reckless, or
crude; behavior comes from the individual and their society.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Cindervein | `heritage.goblin.cindervein` | Tolerates smoke, heat, and poor industrial air better | Cold exposure and sudden cooling cause fatigue sooner |
| Nightglass | `heritage.goblin.nightglass` | Tracks motion and detail in very low light | Glare and rapid flashes impair vision without protection |
| Hullrunner | `heritage.goblin.hullrunner` | Moves efficiently across cluttered hulls and during brief decompression alarms | Heavy suits and high gravity consume stamina faster |

### Somnari

Somnari are a psychic race whose nervous systems resonate with nearby thought.
Their Racial Perk, **Mindwake**, lets them sense active psychic effects and
initiate a consensual short-range mindlink without a spell or device. Mindwake
grants innate `access.psionics`, not mastery: the Psionics skill governs
control, clarity, range, and learned techniques. Psychic storms, crowded minds,
and repeated use cause strain, and Mindwake never reveals private thoughts
without an explicit effect and a consent or resistance check.

See [`PsychicAbilities.md`](PsychicAbilities.md) for contact, consent,
resistance, strain, and information rules.

Somnari breathe, eat, and rest normally, but their sleep includes a vivid dream
phase needed to recover psychic strain. They are not inherently wiser, calmer,
more truthful, or more intelligent than other races.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Chorusborn | `heritage.somnari.chorusborn` | Maintains a consensual mindlink among several nearby participants | Crowds and overlapping psychic signals accumulate strain faster |
| Veilward | `heritage.somnari.veilward` | Shields themself or an assisted ally against psychic intrusion | Dropping the shield to receive beneficial contact takes focus and time |
| Farwhisper | `heritage.somnari.farwhisper` | Detects directed psychic signals and emotional distress at extended range | Aether storms and ancient sites can create misleading psychic echoes |

### Vampires

Vampires are an undead race sustained by blood or a setting-specific vital
essence. Their histories may describe inherited or transformative origins, but
character creation stores `race.vampire` and one compatible heritage.

All vampires receive the Racial Perk **Unliving Physiology**: they do not
breathe, resist vacuum and common disease, and can remain active without
ordinary food. They instead track thirst. Starvation does not silently force
harmful behavior; it unlocks
explicit risks, consent policies, restraint options, and command decisions.
Radiant exposure impairs healing and may cause injury depending on protection.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Crimson | `heritage.vampire.crimson` | Converts fresh blood into rapid healing and strength | Thirst rises faster and stored blood spoils without refrigeration |
| Umbral | `heritage.vampire.umbral` | Conceals presence and sees clearly in near darkness | Direct stellar light is especially dangerous |
| Ashen | `heritage.vampire.ashen` | Tolerates filtered daylight and long dormant passages | Healing and physical bursts consume more essence |

Ship policy must record how blood stores are acquired and consumed and how
shortages are handled. Vampirism creates a resource constraint; it does not
assign morality.

### Eidolons

Eidolons are an undead spirit race whose identity persists through a crafted
Soul Anchor and an embodied vessel. Character creation stores `race.eidolon`
and one compatible Heritage. They are distinct from vampires: a vampire
maintains an undead body with vital essence, while an Eidolon's spirit operates
a replaceable or reconstructable form from an external anchor.

Their Racial Perk, **Soul Anchor**, removes the need to breathe, eat ordinary
food, or sleep and provides resistance to vacuum and common disease. Activity
and recovery instead consume resonance held by the anchor, and serious bodily
disruption requires a safe anchor, suitable material, and time to rebuild a
usable vessel. This does not prevent injury, incapacitation, or removal from an
encounter. Loss, capture, or depletion of the anchor prevents recovery.
Conventional Medicine can treat a compatible organic vessel but not the spirit
itself; Eidolon care usually combines Enchantment, Crafting, and purpose-built
ship facilities.

Soul Anchor is supernatural physiology, not spellcasting or psionics. It does
not grant `access.magic`, `access.psionics`, a Skill rank, or a technique.
Eidolons may learn either access Feat through the same training available to
other characters. They are not inherently ancient, emotionless, truthful, or
knowledgeable; memories and personality belong to the individual.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Reliquary-Bound | `heritage.eidolon.reliquary-bound` | Reconstitutes more efficiently beside a portable, prepared reliquary | Concentrating anchor and reserve in one object makes its capture or damage especially dangerous |
| Huskbound | `heritage.eidolon.huskbound` | A preserved organic vessel fits ordinary armor, tools, and some medical procedures | Decay control consumes preservative supplies and contaminated environments can damage the vessel |
| Starwisp | `heritage.eidolon.starwisp` | A light semi-material vessel moves precisely in low gravity and through narrow openings | It carries little unaided mass and magical or electrical interference disrupts coordination |

### Kharuun

Kharuun are a spacefaring jackal-like race with long directional ears, keen
chemical senses in atmosphere, and strong spatial memory. Character creation
stores `race.kharuun` and one compatible Heritage. Their Racial Perk,
**Trail Sense**, reduces uncertainty when following an already observed
physical, chemical, thermal, acoustic, or signal trail. It improves how
Sensors, Xenology, Ancient Lore, and relevant field work use available
evidence; it never reveals unobserved information and cannot smell through a
vacuum without a collected sample or instrument.

Kharuun breathe, eat, and sleep normally. Their hearing and scent can be
overloaded by machinery, fumes, weapons, or unfamiliar atmospheres, so fitted
helmets and sensory filters matter. Suits, seats, beds, and armor must also
accommodate muzzle, ears, digitigrade legs, and tail. Kharuun are not
inherently loyal, predatory, nomadic, or suited to a particular profession;
those qualities come from the character and their communities.

| Heritage | Stable ID | Heritage Perk | Cost |
| --- | --- | --- | --- |
| Sunwake | `heritage.kharuun.sunwake` | Tolerates heat, dry air, and long periods of measured water rationing | Cold exposure and sudden temperature drops accumulate fatigue faster |
| Hull-Listener | `heritage.kharuun.hull-listener` | Reads faint vibration and movement through direct contact with a hull or deck | Heavy machinery, impacts, and sustained alarm noise cause sensory strain |
| Startrail | `heritage.kharuun.startrail` | Retains bearings and correlates weak route traces after partial sensor loss | Featureless space and contradictory interference require more time and can produce false leads |

## Racial perks

A **Racial Perk** is a racial feat: a discrete rule granted by a character's Race or
compatible Heritage. It represents inherited physiology, supernatural nature,
or a heritage-specific adaptation. It does not represent personality, beliefs,
professional training, or general learned competence.

A Racial Perk may explicitly grant an innate supernatural access ID. Innate access
substitutes for the corresponding learned access Feat, but never grants skill
ranks or unnamed abilities. Learned Feats come from training and are defined in
[`Skills.md`](Skills.md); they are not Racial Perks and are not inherited.
See [`Perks.md`](Perks.md) for the wider Perk terminology and identity rules.
The technical content namespace is `perk.*`, and Race and Heritage definitions
grant their Perks through `grantedPerkIds`.

Every character begins with two Racial Perks:

1. one Race Perk granted by their `raceId`; and
2. one Heritage Perk granted by their `heritageId`.

| Race | Race Perk | Stable technical ID | Innate access |
| --- | --- | --- | --- |
| Human | Versatility | `perk.race.human.versatility` | None |
| Elf | Aether Sense | `perk.race.elf.aether-sense` | `access.magic` |
| Half-elf | Blended Physiology | `perk.race.half-elf.blended-physiology` | None by default |
| Dwarf | Braced Stance | `perk.race.dwarf.braced-stance` | None |
| Orc | Second Wind | `perk.race.orc.second-wind` | None |
| Gnome | Closework | `perk.race.gnome.closework` | None |
| Goblin | Tight Passage | `perk.race.goblin.tight-passage` | None |
| Somnari | Mindwake | `perk.race.somnari.mindwake` | `access.psionics` |
| Vampire | Unliving Physiology | `perk.race.vampire.unliving-physiology` | None by default |
| Eidolon | Soul Anchor | `perk.race.eidolon.soul-anchor` | None by default |
| Kharuun | Trail Sense | `perk.race.kharuun.trail-sense` | None |

Heritage Perks use IDs such as `perk.heritage.dwarf.cometdelver`. A Racial Perk
definition owns its explicit effects, costs, requirements, and
incompatibilities. Race and Heritage definitions grant Racial Perk IDs rather than
duplicating those rules.

Racial Perks do not have a 0–100 value, improve through use, or unlock because of a
class. Learned techniques and professional expertise belong to Skills, while
trained supernatural access belongs to learned Feats. Content validation
rejects missing Racial Perk IDs, unknown access IDs, duplicate grants, and Racial Perks
that are incompatible with the granting Race or Heritage.

## Classless capabilities

Characters have no class, global character level, class skill, or class-locked
ability. Attributes describe broad capability, while independently advancing
skills describe learned competence. Backgrounds provide history without
restricting future progression.

See [Attributes.md](Attributes.md) for the attribute roster, contextual use,
and modifier rules. See [Skills.md](Skills.md) for the skill catalog, action
resolution, advancement, languages, scripts, and ancient lore.

## Character generation

Given a content revision, scenario, and explicit character seed, generation
must be deterministic:

1. Select an allowed race and compatible heritage.
2. Grant and validate the Race Perk and Heritage Perk.
3. Generate age stage, body parameters, name seed, pronouns, and appearance.
4. Allocate attributes, background skills, any documented pre-campaign
   training Feats, and one or more personal aptitudes.
5. Assign known language, script, and lore IDs from heritage and personal history.
6. Add bounded beliefs and memories.
7. Validate equipment, atmosphere, quarters, diet, and medical compatibility.
8. Publish the character only after the complete definition passes validation.

Scenarios may provide authored characters or weighted generation tables.
Weights affect frequency, never capability ceilings or moral alignment.

## Data and persistence contract

Planned authored definitions live under `Content/Characters/` and compile into
validated bounded artifacts. A heritage definition references its compatible
race rather than copying the race definition:

```json
{
  "schemaVersion": 1,
  "id": "heritage.dwarf.cometdelver",
  "raceId": "race.dwarf",
  "nameKey": "character.heritage.dwarf.cometdelver.name",
  "descriptionKey": "character.heritage.dwarf.cometdelver.description",
  "grantedPerkIds": [
    "perk.heritage.dwarf.cometdelver"
  ]
}
```

Persistent race state stores race and heritage definition IDs, definition
revisions, and race-specific rolled parameters. It never stores localized names
as identity. Definition loading must reject duplicate IDs, missing races,
incompatible heritages, missing or incompatible Racial Perks, incompatible body
rules, unknown modifiers, and unbounded generation tables before publication.

## Balance rules

- No race receives a universal intelligence, morality, or social-value
  modifier.
- Every heritage begins with one legible strength and one relevant cost.
- Environmental advantages must matter often enough to be worth choosing but
  must not make a heritage mandatory for a particular activity.
- Equipment, training, medicine, and ship design provide alternate solutions
  to most physiological disadvantages.
- Mixed crews should create logistical and operational choices, not a hidden
  optimal mono-race strategy.
- Automated crew decisions expose the need, task, rule, and blocker that caused
  the choice.

## First playable character scope

The first crew-enabled vertical slice should use eleven authored characters:
one human, elf, half-elf, dwarf, orc, gnome, goblin, Somnari, vampire, Eidolon,
and Kharuun. Each needs a race, heritage, background, attributes, skills, two
Racial Perks, and compatible quarters. The two Racial Perks are racial feats:
one granted by Race and one by Heritage. The Elf must exercise innate
magical access and the Somnari must exercise innate psychic access. The
Eidolon's anchor logistics and the Kharuun's evidence-bounded tracking must
each change at least one voyage decision. At least one character without
either access Racial Perk must demonstrate earning an access Feat through
documented training. Ship jobs and schedules are a separate future crew-system
concern, not a component of race or heritage. The slice needs only four shared
needs—rest, nutrition or equivalent reserve, safety, and belonging.
Attribute, skill, language, script, ancient-lore, and encounter scope is defined
in the linked capability documents. See [`Battle.md`](Battle.md) for combat,
injury, surrender, and tactical-layout rules.

The slice succeeds when race and heritage change real voyage decisions, every
crew action has an inspectable reason, and replaying the same seed and commands
produces the same character and ship state. Romance, children, aging campaigns,
full genetic inheritance, unrestricted hybrid races, and dozens of content
variants are explicitly deferred.
