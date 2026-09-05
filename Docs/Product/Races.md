# Races and heritage design

## Status

This document defines planned race, heritage, physiology, and character-creation
rules. These systems are not implemented yet. The current prototype simulates
only ship-level expedition resources.

## Design goals

- Make every crew member mechanically understandable and narratively distinct.
- Support humans, elves, half-elves, dwarves, orcs, gnomes, goblins, Somnari,
  vampires, and future races without hard-coding content into simulation code.
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
| Race | human, elf, half-elf, dwarf, orc, gnome, goblin, Somnari, vampire | Body plan and baseline physiological rules |
| Heritage | voidborn, deepforge, free-anchorage | Subrace, formative tradition, languages, customs, and starting knowledge |
| Attributes | Strength, Agility, Willpower, Intelligence, Luck, Charisma, Toughness | Broad capability shared by many actions |
| Skills | piloting, engineering, language and literacy | Learned competence that improves independently |
| Background | academy graduate, dockhand, caravan guard | Starting skill package and history, never a class |
| Position | doctor, chef, navigator, engineer | Ongoing responsibility, authority, pay, and expected duties |
| Talents | Versatility, Aether Sense, Braced Stance | Racial feats granted by Race and Heritage |

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
technical adaptation. Their Race Talent is **Versatility**: one starting
aptitude may be reassigned during recruitment. Their bodies have no extreme
environmental adaptation and depend on ordinary atmosphere, food, sleep, and
medical support.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Hearthworld | `heritage.human.hearthworld` | Faster recovery in comfortable gravity and atmosphere | Suffers acclimation penalties sooner in extreme environments |
| Voidborn | `heritage.human.voidborn` | Lower food use and better low-gravity movement | Reduced tolerance for high gravity and heavy recoil |
| Emberworld | `heritage.human.emberworld` | Better heat and dehydration tolerance | Cold exposure accumulates faster |

### Elves

Elves are long-lived beings whose nervous systems sense patterns in light and
the void's aetheric currents. Their Race Talent, **Aether Sense**, can reveal
weak anomalies or unstable routes before ordinary instruments, but intense
interference causes sensory strain. Elves enter a short trance instead of
normal sleep; they still need safe downtime.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Dawnweave | `heritage.elf.dawnweave` | Gains focus in strong natural or stellar light | Darkness increases fatigue unless quarters provide tuned lighting |
| Gloamroot | `heritage.elf.gloamroot` | Excellent low-light sight and reduced sensory signature | Bright or rapidly changing light causes strain |
| Glassleaf | `heritage.elf.glassleaf` | Reads crystalline and aetheric material with unusual precision | Resonant machinery and storms can overwhelm that sense |

### Half-elves

Half-elves are a first-class race in the initial content model, representing
characters with both human and elven parentage. Their Race Talent, **Blended
Physiology**, lets a character select one minor human adaptation and one minor
elven sense, each at reduced strength. It does not determine heritage or
guarantee social acceptance.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Concord | `heritage.half-elf.concord` | Switches between normal sleep and trance when quarters permit | Neither rest mode is as efficient as its specialist form |
| Starling | `heritage.half-elf.starling` | Adapts quickly to changing gravity and duty schedules | Needs more recovery after repeated schedule changes |
| Threshold | `heritage.half-elf.threshold` | Learns unfamiliar languages and customs faster | Starts with fewer specialist skill ranks |

Future mixed-race support may generalize parentage, but released save IDs
for half-elves must remain valid through an explicit migration.

### Dwarves

Dwarves are compact, dense-bodied people adapted to confined habitats and
demanding physical environments. Their Race Talent, **Braced Stance**, reduces
forced movement and work interruption. Their mass raises acceleration costs,
and cramped does not mean weightless: ship layout and rescue equipment must
support them.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Deepforge | `heritage.dwarf.deepforge` | Exceptional high-gravity and heat tolerance | Low gravity reduces precision until acclimated |
| Cometdelver | `heritage.dwarf.cometdelver` | Detects structural weakness and valuable mineral seams | Requires more calories during heavy work |
| Brasswake | `heritage.dwarf.brasswake` | Maintains concentration amid vibration and machinery noise | Quiet natural environments provide less effective recreation |

### Orcs

Orcs are powerfully built and recover well from exertion. Their Race Talent,
**Second Wind**, allows
a controlled burst of work or combat stamina followed by a visible recovery
debt. Orcs are not inherently violent, unintelligent, or hostile; individuals
and communities determine their values and behavior.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Redwake | `heritage.orc.redwake` | Sustains strenuous labor and emergency damage control | Higher food and oxygen consumption while exerting |
| Stormborn | `heritage.orc.stormborn` | Better resistance to electrical and aether-storm injury | Medical recovery uses more conductive supplies |
| Greenmoon | `heritage.orc.greenmoon` | Heals minor wounds quickly with adequate food and rest | Starvation and sleep loss suppress regeneration sharply |

### Gnomes

Gnomes are small-bodied people with extremely sensitive touch and fine spatial
judgment. Their Race Talent, **Closework**, reduces penalties when manipulating
compact mechanisms or working in confined stations. Their reach and unaided
carrying capacity are limited, and shared equipment must be adjustable rather
than assuming one body scale. Gnomes are not inherently inventive or
scholarly; those qualities come from individual attributes, skills, and
heritage.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Coilwhisper | `heritage.gnome.coilwhisper` | Feels subtle vibration, current, and mechanical imbalance through contact | Heavy vibration and electrical surges cause sensory strain |
| Sporegarden | `heritage.gnome.sporegarden` | Symbiotic microbiome improves food variety and resistance to organic toxins | Sterile quarters and broad-spectrum medicine can disrupt recovery |
| Cloudstep | `heritage.gnome.cloudstep` | Excellent orientation and fine movement in low gravity | High-gravity labor and heavy recoil accumulate fatigue quickly |

### Goblins

Goblins are compact, quick-moving people adapted to crowded habitats and
improvised routes. Their Race Talent, **Tight Passage**, reduces movement and
work penalties in ducts, wreckage, and congested decks. Their lighter frames
are easier to throw off balance, and standard armor, furniture, and controls
may require refitting. Goblins are not inherently dishonest, reckless, or
crude; behavior comes from the individual and their society.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Cindervein | `heritage.goblin.cindervein` | Tolerates smoke, heat, and poor industrial air better | Cold exposure and sudden cooling cause fatigue sooner |
| Nightglass | `heritage.goblin.nightglass` | Tracks motion and detail in very low light | Glare and rapid flashes impair vision without protection |
| Hullrunner | `heritage.goblin.hullrunner` | Moves efficiently across cluttered hulls and during brief decompression alarms | Heavy suits and high gravity consume stamina faster |

### Somnari

Somnari are a psychic race whose nervous systems resonate with nearby thought.
Their Race Talent, **Mindwake**, lets them sense active psychic effects and
initiate a consensual short-range mindlink without a spell or device. Mindwake
provides access, not mastery: the Psionics skill governs control, clarity,
range, and learned techniques. Psychic storms, crowded minds, and repeated use
cause strain, and Mindwake never reveals private thoughts without an explicit
effect and a consent or resistance check.

Somnari breathe, eat, and rest normally, but their sleep includes a vivid dream
phase needed to recover psychic strain. They are not inherently wiser, calmer,
more truthful, or more intelligent than other races.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Chorusborn | `heritage.somnari.chorusborn` | Maintains a consensual mindlink among several nearby participants | Crowds and overlapping psychic signals accumulate strain faster |
| Veilward | `heritage.somnari.veilward` | Shields themself or an assisted ally against psychic intrusion | Dropping the shield to receive beneficial contact takes focus and time |
| Farwhisper | `heritage.somnari.farwhisper` | Detects directed psychic signals and emotional distress at extended range | Aether storms and ancient sites can create misleading psychic echoes |

### Vampires

Vampires are an undead race sustained by blood or a setting-specific vital
essence. Their histories may describe inherited or transformative origins, but
character creation stores `race.vampire` and one compatible heritage.

All vampires receive the Race Talent **Unliving Physiology**: they do not
breathe, resist vacuum and common disease, and can remain active without
ordinary food. They instead track thirst. Starvation does not silently force
harmful behavior; it unlocks
explicit risks, consent policies, restraint options, and command decisions.
Radiant exposure impairs healing and may cause injury depending on protection.

| Heritage | Stable ID | Heritage Talent | Cost |
| --- | --- | --- | --- |
| Crimson | `heritage.vampire.crimson` | Converts fresh blood into rapid healing and strength | Thirst rises faster and stored blood spoils without refrigeration |
| Umbral | `heritage.vampire.umbral` | Conceals presence and sees clearly in near darkness | Direct stellar light is especially dangerous |
| Ashen | `heritage.vampire.ashen` | Tolerates filtered daylight and long dormant passages | Healing and physical bursts consume more essence |

Ship policy must record how blood stores are acquired and consumed and how
shortages are handled. Vampirism creates a resource constraint; it does not
assign morality.

## Racial talents

A **Talent** is a racial feat: a discrete rule granted by a character's Race or
compatible Heritage. It represents inherited physiology, supernatural nature,
or a heritage-specific adaptation. It does not represent personality, beliefs,
professional training, or general learned competence.

Every character begins with two Talents:

1. one Race Talent granted by their `raceId`; and
2. one Heritage Talent granted by their `heritageId`.

| Race | Race Talent | Stable Talent ID |
| --- | --- | --- |
| Human | Versatility | `talent.race.human.versatility` |
| Elf | Aether Sense | `talent.race.elf.aether-sense` |
| Half-elf | Blended Physiology | `talent.race.half-elf.blended-physiology` |
| Dwarf | Braced Stance | `talent.race.dwarf.braced-stance` |
| Orc | Second Wind | `talent.race.orc.second-wind` |
| Gnome | Closework | `talent.race.gnome.closework` |
| Goblin | Tight Passage | `talent.race.goblin.tight-passage` |
| Somnari | Mindwake | `talent.race.somnari.mindwake` |
| Vampire | Unliving Physiology | `talent.race.vampire.unliving-physiology` |

Heritage Talents use IDs such as `talent.heritage.dwarf.cometdelver`. A Talent
definition owns its explicit effects, costs, requirements, and
incompatibilities. Race and Heritage definitions grant Talent IDs rather than
duplicating those rules.

Talents do not have a 0–100 value, improve through use, or unlock because of a
class. Learned techniques and professional expertise belong to Skills instead.
Content validation rejects missing Talent IDs, duplicate grants, and Talents
that are incompatible with the granting Race or Heritage.

## Classless capabilities

Characters have no class, global character level, class skill, or class-locked
ability. Attributes describe broad capability, while independently advancing
skills describe learned competence. Backgrounds and crew positions provide
history and responsibility without restricting future progression.

See [Attributes.md](Attributes.md) for the attribute roster, contextual use,
and modifier rules. See [Skills.md](Skills.md) for the skill catalog, action
resolution, advancement, languages, scripts, and ancient lore.

## Crew positions and shipboard duties

A **position** is a continuing post in the crew organization. A **duty** is a
specific task or watch assignment. Doctor and Chef are positions; treating an
injury and preparing supper are duties. Keeping these concepts separate lets a
character help outside their normal job without changing class or identity.

| Concept | Duration | Provides | Example |
| --- | --- | --- | --- |
| Background | Historical | Starting skills, contacts, and memories | Former dockhand |
| Position | Until reassigned | Responsibility, authority, pay share, schedule priority | Ship's doctor |
| Duty | One job or watch | A concrete action and success criteria | Treat a burned deckhand |

### Positions

The planned position catalog includes:

| Department | Positions | Typical responsibilities |
| --- | --- | --- |
| Command | Captain, First Mate, Quartermaster | Policy, discipline, watches, cargo allocation, and pay shares |
| Navigation | Pilot, Navigator, Cartographer | Helm control, route planning, charts, sensors, and docking |
| Care | Doctor, Medic, Chef, Steward | Treatment, surgery, quarantine, meals, sanitation, and crew comfort |
| Technical | Chief Engineer, Engineer, Artificer, Rigger | Drive operation, repairs, fabrication, rigging, and power allocation |
| Exploration | Scout, Xenologist, Salvager, Alchemist | Surveys, field samples, ruins, reagents, and recovery operations |
| Security | Master-at-Arms, Marine, Archer, Gunner | Watches, boarding defense, weapons, prisoners, and drills |
| Mystic | Ship Mage, Warden | Magical navigation, wards, enchantments, curses, and anomalies |
| Civil | Envoy, Trader, Chronicler, Antiquarian | Negotiation, commerce, languages, records, lore, and faction relations |

Positions use stable IDs such as `position.doctor` and `position.chef`. A
position definition contains localized name keys, responsibility tags,
authority permissions, preferred skills, schedule defaults, and any required
facility. Preferred skills generate recommendations; they do not prevent an
unconventional appointment.

A character normally holds one primary position and may hold one secondary
position on a small ship. Posts can be vacant, shared, or temporarily filled by
an acting crew member. Some permissions—opening the medicine locker, spending
ship funds, changing course, or sentencing a prisoner—come from ship policy and
position authority rather than personal skill.

### Duties

The schedule and command systems assign bounded duties such as:

- helm the ship or plot a course;
- stand lookout, security, or engineering watch;
- diagnose a patient, perform treatment, or manage quarantine;
- plan meals, prepare food, preserve provisions, or clean the galley;
- inspect the drive, repair a module, fabricate a part, or work the rigging;
- survey a location, gather reagents, salvage cargo, or identify an artifact;
- copy an inscription, translate a text, authenticate a relic, or update lore;
- maintain a ward, cast a voyage ritual, or contain a magical hazard;
- train the crew, guard a prisoner, repel boarders, or operate a weapon; and
- inventory cargo, negotiate a trade, update charts, or record discoveries.

Duty performance emerges from attributes, skills, equipment, position
authority, health, needs, environment, and cooperation. A Chef with high
Medicine may treat an emergency; a Doctor with Alchemy may prepare medicine; a
Captain with Cooking may take a galley watch. The interface should recommend
qualified crew and explain penalties without forbidding those assignments.

## Character generation

Given a content revision, scenario, and explicit character seed, generation
must be deterministic:

1. Select an allowed race and compatible heritage.
2. Grant and validate the Race Talent and Heritage Talent.
3. Generate age stage, body parameters, name seed, pronouns, and appearance.
4. Allocate attributes, background skills, and one or more personal aptitudes.
5. Assign known language, script, and lore IDs from heritage and personal history.
6. Assign an initial crew position and bounded duty schedule.
7. Add bounded beliefs and memories.
8. Validate equipment, atmosphere, quarters, diet, and medical compatibility.
9. Publish the character only after the complete definition passes validation.

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
  "grantedTalentIds": [
    "talent.heritage.dwarf.cometdelver"
  ]
}
```

Persistent race state stores race and heritage definition IDs, definition
revisions, and race-specific rolled parameters. It never stores localized names
as identity. Definition loading must reject duplicate IDs, missing races,
incompatible heritages, missing or incompatible Talents, incompatible body
rules, unknown modifiers, and unbounded generation tables before publication.

Ship organization stores position assignments separately as stable character
ID and position ID pairs. Reassigning a position updates authority and schedule
only after the complete proposed roster passes staffing, capacity, and policy
validation.

## Balance rules

- No race receives a universal intelligence, morality, or social-value
  modifier.
- Every heritage begins with one legible strength and one relevant cost.
- Environmental advantages must matter often enough to be worth choosing but
  must not make a heritage mandatory for a position or duty.
- Equipment, training, medicine, and ship design provide alternate solutions
  to most physiological disadvantages.
- Mixed crews should create logistical and operational choices, not a hidden
  optimal mono-race strategy.
- Automated crew decisions expose the need, duty, rule, and blocker that caused
  the choice.

## First playable character scope

The first crew-enabled vertical slice should use nine authored characters: one
human, elf, half-elf, dwarf, orc, gnome, goblin, Somnari, and vampire. Each needs
a race, heritage, background, attributes, skills, two Talents, crew position,
and compatible quarters. The two Talents are racial feats: one granted by Race
and one by Heritage. The initial roster must include a Doctor and Chef so
medical care, meals, position authority, and cross-duty assignment are
exercised. The slice needs only four shared needs—rest, nutrition or thirst,
safety, and belonging—and five duties: navigate, salvage, repair, prepare a
meal, and treat an injury. Attribute, skill, language, script, ancient-lore, and
encounter scope is defined in the linked capability documents.

The slice succeeds when race and heritage change real voyage decisions, every
crew action has an inspectable reason, and replaying the same seed and commands
produces the same character and ship state. Romance, children, aging campaigns,
full genetic inheritance, unrestricted hybrid races, and dozens of content
variants are explicitly deferred.
