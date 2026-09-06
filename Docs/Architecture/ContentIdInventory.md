# Product content ID inventory

## Status

This document is the reviewed inventory produced by roadmap task `M0.1.1`. It
records stable gameplay IDs explicitly present under `Docs/Product`; it does
not claim that runtime definitions or loaders exist.

The inventory is a documentation snapshot, not yet a machine-readable content
manifest. Ownership and lifecycle labels required by `M0.1.2`, and collision or
spelling decisions required by `M0.1.3`, remain deliberately unassigned.

## Review method and scope

All Product Markdown files were scanned as UTF-8 for the requested definition
families. Candidate strings were deduplicated and then reviewed to separate
gameplay IDs from localization keys and adjacent definition kinds.

Included categories are Attributes, Skills, learned Feats, racial or Heritage
Talents, Access gates, Spells, psychic techniques, combat contexts, ship
modules, factions, and crises. Crisis phase and resolution IDs are retained in
separate tables because they are explicit crisis content references.

Excluded strings include localized keys ending in `.name` or `.description`,
formula and effect IDs, psychic discipline IDs, psychic information-scope IDs,
range IDs, progression IDs, and illustrative third-party mod IDs. Those can be
inventoried when their definition schemas enter roadmap scope.

## Summary

| Category | Explicit unique IDs | Primary product source |
| --- | ---: | --- |
| Attributes | 7 | [`Attributes.md`](../Product/Attributes.md) |
| Skills | 14 | [`Skills.md`](../Product/Skills.md), plus one explicit Medicine reference in [`Ships.md`](../Product/Ships.md) |
| Learned Feats | 2 | [`Skills.md`](../Product/Skills.md) |
| Talents | 10 | [`Races.md`](../Product/Races.md) |
| Access gates | 2 | [`Skills.md`](../Product/Skills.md) |
| Spells | 32 | [`Spells.md`](../Product/Spells.md) |
| Psychic techniques | 4 | [`PsychicAbilities.md`](../Product/PsychicAbilities.md) |
| Combat contexts | 6 | [`Battle.md`](../Product/Battle.md) |
| Ship modules | 32 | [`Ships.md`](../Product/Ships.md) |
| Factions | 6 | [`Factions.md`](../Product/Factions.md) |
| Crisis families | 7 | [`Endgame_Crisis.md`](../Product/Endgame_Crisis.md) |
| Crisis phases | 7 | [`Endgame_Crisis.md`](../Product/Endgame_Crisis.md) |
| Crisis resolutions | 3 | [`Endgame_Crisis.md`](../Product/Endgame_Crisis.md) |
| **Total** | **132** | |

## Attributes

| Display label | Stable ID |
| --- | --- |
| Agility | `attribute.agility` |
| Charisma | `attribute.charisma` |
| Intelligence | `attribute.intelligence` |
| Luck | `attribute.luck` |
| Strength | `attribute.strength` |
| Toughness | `attribute.toughness` |
| Willpower | `attribute.willpower` |

## Skills

| Display label | Stable ID | Explicit source |
| --- | --- | --- |
| Alchemy | `skill.alchemy` | Skills |
| Ancient Lore | `skill.ancient-lore` | Skills |
| Archery | `skill.archery` | Skills |
| Cooking | `skill.cooking` | Skills |
| Crafting | `skill.crafting` | Skills |
| Enchantment | `skill.enchantment` | Skills |
| Engineering | `skill.engineering` | Skills |
| Language and Literacy | `skill.language-literacy` | Skills |
| Magic | `skill.magic` | Skills |
| Medicine | `skill.medicine` | Ships sickbay example |
| Melee | `skill.melee` | Skills |
| Merchant | `skill.merchant` | Skills |
| Negotiation | `skill.negotiation` | Skills |
| Psionics | `skill.psionics` | Skills |

The Skill catalog also names the following capabilities without assigning an
explicit Stable ID: Athletics, Acrobatics, EVA, Stealth, Piloting, Astrogation,
Sensors, Rigging, Salvage, Gunnery, Defense, Xenology, Command, Insight, and
Deception. They are not silently converted into inferred IDs by this inventory.

## Learned Feats

| Display label | Stable ID |
| --- | --- |
| Spellcasting Training | `feat.access.magic` |
| Psionic Training | `feat.access.psionics` |

## Talents

| Display label | Stable ID |
| --- | --- |
| Cometdelver Heritage Talent | `talent.heritage.dwarf.cometdelver` |
| Braced Stance | `talent.race.dwarf.braced-stance` |
| Aether Sense | `talent.race.elf.aether-sense` |
| Closework | `talent.race.gnome.closework` |
| Tight Passage | `talent.race.goblin.tight-passage` |
| Blended Physiology | `talent.race.half-elf.blended-physiology` |
| Versatility | `talent.race.human.versatility` |
| Second Wind | `talent.race.orc.second-wind` |
| Mindwake | `talent.race.somnari.mindwake` |
| Unliving Physiology | `talent.race.vampire.unliving-physiology` |

Heritage tables describe additional Heritage Talents without spelling out a
Stable Talent ID for each row. Only the explicit Cometdelver example is listed
above; the inventory does not synthesize IDs from Heritage IDs or display text.

## Access gates

| Capability | Stable ID |
| --- | --- |
| Magical access | `access.magic` |
| Psychic access | `access.psionics` |

## Spells

### Passage

| Display label | Stable ID |
| --- | --- |
| Anchor Step | `spell.passage.anchor-step` |
| Cargo Aperture | `spell.passage.cargo-aperture` |
| Paired Threshold | `spell.passage.paired-threshold` |
| Starway Accord | `spell.passage.starway-accord` |

### Radiance

| Display label | Stable ID |
| --- | --- |
| Dawn Array | `spell.radiance.dawn-array` |
| Heat Draw | `spell.radiance.heat-draw` |
| Lantern Spark | `spell.radiance.lantern-spark` |
| Starflare Beacon | `spell.radiance.starflare-beacon` |

### Seeking

| Display label | Stable ID |
| --- | --- |
| Aether Trace | `spell.seeking.aether-trace` |
| Fault Echo | `spell.seeking.fault-echo` |
| Starway Sounding | `spell.seeking.starway-sounding` |
| Waymark Compass | `spell.seeking.waymark-compass` |

### Shaping

| Display label | Stable ID |
| --- | --- |
| Cutline | `spell.shaping.cutline` |
| Formwright Chorus | `spell.shaping.formwright-chorus` |
| Hullskin | `spell.shaping.hullskin` |
| Seam Press | `spell.shaping.seam-press` |

### Vectoring

| Display label | Stable ID |
| --- | --- |
| Driftstep | `spell.vectoring.driftstep` |
| Gravity Knot | `spell.vectoring.gravity-knot` |
| Keel Turn | `spell.vectoring.keel-turn` |
| Vector Tether | `spell.vectoring.vector-tether` |

### Veiling

| Display label | Stable ID |
| --- | --- |
| False Wake | `spell.veiling.false-wake` |
| Ghost Rig | `spell.veiling.ghost-rig` |
| Masked Hold | `spell.veiling.masked-hold` |
| Quiet Silhouette | `spell.veiling.quiet-silhouette` |

### Vitality

| Display label | Stable ID |
| --- | --- |
| Borrowed Breath | `spell.vitality.borrowed-breath` |
| Draw Taint | `spell.vitality.draw-taint` |
| Sanctuary Vigil | `spell.vitality.sanctuary-vigil` |
| Steady Pulse | `spell.vitality.steady-pulse` |

### Warding

| Display label | Stable ID |
| --- | --- |
| Brace Ward | `spell.warding.brace-ward` |
| Haven Circuit | `spell.warding.haven-circuit` |
| Spellbreak Lattice | `spell.warding.spellbreak-lattice` |
| Threshold Seal | `spell.warding.threshold-seal` |

## Psychic techniques

| Display label | Stable ID |
| --- | --- |
| Mindlink | `psychic.contact.mindlink` |
| Echo Sense | `psychic.empathy.echo-sense` |
| Kinetic Nudge | `psychic.psychokinesis.kinetic-nudge` |
| Quiet Mind | `psychic.shielding.quiet-mind` |

`psychic.discipline.*` strings organize techniques and
`psychic.scope.deliberate-message` limits information. They are adjacent
definition kinds and are not counted as psychic technique IDs.

## Combat contexts

| Display label | Stable ID |
| --- | --- |
| Boarding and ship interior | `combat.context.boarding` |
| EVA and hull exterior | `combat.context.eva` |
| Space ruin | `combat.context.ruin` |
| Station or settlement | `combat.context.settlement` |
| Ship engagement | `combat.context.ship` |
| Surface expedition | `combat.context.surface` |

## Ship modules

| Display label | Stable ID |
| --- | --- |
| Cargo Hold | `module.cargo.hold` |
| Helm | `module.command.helm` |
| Boarding Lock | `module.contact.boarding-lock` |
| Psychic Resonator | `module.contact.psychic-resonator` |
| Signal Lantern | `module.contact.signal-lantern` |
| Reinforced Plating | `module.defense.reinforced-plating` |
| Ward Projector | `module.defense.ward-projector` |
| Atmosphere Recycler | `module.habitat.atmosphere-recycler` |
| Common Room | `module.habitat.common-room` |
| Crew Quarters | `module.habitat.crew-quarters` |
| Galley | `module.habitat.galley` |
| Provision Locker | `module.habitat.provision-locker` |
| Sickbay | `module.habitat.sickbay` |
| Chart Archive | `module.navigation.chart-archive` |
| Sensor Mast | `module.navigation.sensor-mast` |
| Star Compass | `module.navigation.star-compass` |
| Aether Dynamo | `module.power.aether-dynamo` |
| Atomic Reactor | `module.power.atomic-reactor` |
| Crystal Accumulator | `module.power.crystal-accumulator` |
| Diesel Generator | `module.power.diesel-generator` |
| Flywheel Bank | `module.power.flywheel-bank` |
| Runic Distributor | `module.power.runic-distributor` |
| Flux Sail | `module.propulsion.flux-sail` |
| Nuclear-Thermal Drive | `module.propulsion.nuclear-thermal-drive` |
| Propellant Drive | `module.propulsion.propellant-drive` |
| Vector Vanes | `module.propulsion.vector-vanes` |
| Lore Vault | `module.research.lore-vault` |
| Salvage Rig | `module.utility.salvage-rig` |
| Deck Battery | `module.weapon.deck-battery` |
| Alchemy Lab | `module.workshop.alchemy-lab` |
| Enchanting Chamber | `module.workshop.enchanting-chamber` |
| Fabricator | `module.workshop.fabricator` |

## Factions

| Display label | Stable ID |
| --- | --- |
| Free Anchorage Compact | `faction.free-anchorage-compact` |
| Horizon Salvagers' Union | `faction.horizon-salvagers-union` |
| Lumenwake Covenant | `faction.lumenwake-covenant` |
| Meridian Foundry League | `faction.meridian-foundry-league` |
| Pilgrim Garden Fleet | `faction.pilgrim-garden-fleet` |
| Quiet Chorus Assembly | `faction.quiet-chorus-assembly` |

## Crises

### Crisis families

| Display label | Stable ID |
| --- | --- |
| The Cinder Crown | `crisis.cinder-crown` |
| The Distant Throne | `crisis.distant-throne` |
| The Glasswake Bloom | `crisis.glasswake-bloom` |
| The Mawflight | `crisis.mawflight` |
| War of the Severed Banners | `crisis.severed-banners-war` |
| The Shattered Meridian | `crisis.shattered-meridian` |
| The Unbidden Chorus | `crisis.unbidden-chorus` |

### Crisis phases

| Display label | Stable ID |
| --- | --- |
| Aftermath | `crisis.phase.aftermath` |
| Confirmed | `crisis.phase.confirmed` |
| Confrontation | `crisis.phase.confrontation` |
| Dormant | `crisis.phase.dormant` |
| Escalation | `crisis.phase.escalation` |
| Omens | `crisis.phase.omens` |
| Outbreak | `crisis.phase.outbreak` |

### Explicit resolution examples

| Display label | Stable ID |
| --- | --- |
| Migrate | `crisis.shattered-meridian.resolution.migrate` |
| Sever | `crisis.shattered-meridian.resolution.sever` |
| Stabilize | `crisis.shattered-meridian.resolution.stabilize` |

Only the Shattered Meridian JSON example currently assigns explicit resolution
IDs. Other described resolution choices remain prose and are not assigned IDs
by this inventory.

## Review result

The requested categories contain 132 unique explicit gameplay IDs. No duplicate
ID is assigned to two different labels within these tables. Repeated references
to the same ID across Product documents were consolidated under one entry.

The missing explicit Skill and Heritage Talent IDs noted above are inputs to
the later `M0.1.3` spelling and collision review. This task does not resolve or
mint them.
