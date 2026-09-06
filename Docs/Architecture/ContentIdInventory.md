# Design content ID inventory

## Status

This document is the reviewed inventory produced by roadmap task `M0.1.1`. It
records stable gameplay IDs explicitly present under `Docs/DesignConcept`; it
does not claim that runtime definitions or loaders exist.

The inventory is a documentation snapshot, not yet a machine-readable content
manifest. Ownership and lifecycle classifications, plus collision and spelling
decisions, are recorded in [`ContentIdReview.md`](ContentIdReview.md).

## Review method and scope

All design-concept Markdown files were scanned as UTF-8 for the requested
definition families. Candidate strings were deduplicated and then reviewed to
separate gameplay IDs from localization keys and adjacent definition kinds.

Included categories are Attributes, Skills, learned Feats, racial or Heritage
Racial Perks, Access gates, Spells, psychic techniques, combat contexts, travel
events and their explicit choices, ship modules, ship weapon configurations,
factions, and crises. Event choice, crisis phase, and crisis resolution IDs are
retained in separate tables because they are explicit content references.

Excluded strings include localized keys ending in `.name` or `.description`,
formula and effect IDs, psychic discipline IDs, psychic information-scope IDs,
range IDs, progression IDs, and illustrative third-party mod IDs. Those can be
inventoried when their definition schemas enter roadmap scope.

## Summary

| Category | Explicit unique IDs | Primary product source |
| --- | ---: | --- |
| Attributes | 7 | [`Attributes.md`](../DesignConcept/Attributes.md) |
| Skills | 29 | [`Skills.md`](../DesignConcept/Skills.md) |
| Learned Feats | 2 | [`Skills.md`](../DesignConcept/Skills.md) |
| Racial Perks | 12 | [`Races.md`](../DesignConcept/Races.md) |
| Equipment | 11 | [`Equipments.md`](../DesignConcept/Equipments.md) |
| Access gates | 2 | [`Skills.md`](../DesignConcept/Skills.md) |
| Spells | 7 | [`Spells.md`](../DesignConcept/Spells.md) |
| Psychic techniques | 4 | [`PsychicAbilities.md`](../DesignConcept/PsychicAbilities.md) |
| Combat contexts | 6 | [`Battle.md`](../DesignConcept/Battle.md) |
| Travel events | 8 | [`Events.md`](../DesignConcept/Events.md) |
| Travel-event choices | 3 | [`Events.md`](../DesignConcept/Events.md) |
| Ship modules | 35 | [`Ships.md`](../DesignConcept/Ships.md) |
| Ship weapon configurations | 3 | [`Ships.md`](../DesignConcept/Ships.md) |
| Factions | 6 | [`Factions.md`](../DesignConcept/Factions.md) |
| Crisis families | 7 | [`Endgame_Crisis.md`](../DesignConcept/Endgame_Crisis.md) |
| Crisis phases | 7 | [`Endgame_Crisis.md`](../DesignConcept/Endgame_Crisis.md) |
| Crisis resolutions | 3 | [`Endgame_Crisis.md`](../DesignConcept/Endgame_Crisis.md) |
| **Total** | **152** | |

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
| Acrobatics | `skill.acrobatics` | Skills |
| Ancient Lore | `skill.ancient-lore` | Skills |
| Archery | `skill.archery` | Skills |
| Astrogation | `skill.astrogation` | Skills |
| Athletics | `skill.athletics` | Skills |
| Command | `skill.command` | Skills |
| Cooking | `skill.cooking` | Skills |
| Crafting | `skill.crafting` | Skills |
| Deception | `skill.deception` | Skills |
| Defense | `skill.defense` | Skills |
| Enchantment | `skill.enchantment` | Skills |
| Engineering | `skill.engineering` | Skills |
| EVA | `skill.eva` | Skills |
| Gunnery | `skill.gunnery` | Skills |
| Insight | `skill.insight` | Skills |
| Language and Literacy | `skill.language-literacy` | Skills |
| Magic | `skill.magic` | Skills |
| Medicine | `skill.medicine` | Skills |
| Melee | `skill.melee` | Skills |
| Merchant | `skill.merchant` | Skills |
| Negotiation | `skill.negotiation` | Skills |
| Piloting | `skill.piloting` | Skills |
| Psionics | `skill.psionics` | Skills |
| Rigging | `skill.rigging` | Skills |
| Salvage | `skill.salvage` | Skills |
| Sensors | `skill.sensors` | Skills |
| Stealth | `skill.stealth` | Skills |
| Xenology | `skill.xenology` | Skills |

## Learned Feats

| Display label | Stable ID |
| --- | --- |
| Spellcasting Training | `feat.access.magic` |
| Psionic Training | `feat.access.psionics` |

## Racial Perks

| Display label | Stable ID |
| --- | --- |
| Cometdelver Heritage Perk | `perk.heritage.dwarf.cometdelver` |
| Braced Stance | `perk.race.dwarf.braced-stance` |
| Soul Anchor | `perk.race.eidolon.soul-anchor` |
| Aether Sense | `perk.race.elf.aether-sense` |
| Closework | `perk.race.gnome.closework` |
| Tight Passage | `perk.race.goblin.tight-passage` |
| Blended Physiology | `perk.race.half-elf.blended-physiology` |
| Versatility | `perk.race.human.versatility` |
| Second Wind | `perk.race.orc.second-wind` |
| Mindwake | `perk.race.somnari.mindwake` |
| Dusk Sight | `perk.race.veyr.dusk-sight` |
| Trail Sense | `perk.race.tharun.trail-sense` |

Heritage tables describe additional Heritage Perks without spelling out a
stable Perk ID for each row. Only the explicit Cometdelver example is listed
above; the inventory does not synthesize IDs from Heritage IDs or display text.

## Equipment

| Display label | Stable ID |
| --- | --- |
| Casting focus | `item.arcane.casting-focus` |
| Powered armor | `item.armor.powered-armor` |
| Pressure suit | `item.armor.pressure-suit` |
| Field medkit | `item.medical.field-medkit` |
| Repair kit | `item.tool.repair-kit` |
| Survey scanner | `item.tool.survey-scanner` |
| Aether projector | `item.weapon.aether-projector` |
| Boarding blade | `item.weapon.boarding-blade` |
| Laser carbine | `item.weapon.laser-carbine` |
| Plasma rifle | `item.weapon.plasma-rifle` |
| Service pistol | `item.weapon.service-pistol` |

## Access gates

| Capability | Stable ID |
| --- | --- |
| Magical access | `access.magic` |
| Psychic access | `access.psionics` |

## Spells

### Elemental

| Display label | Stable ID |
| --- | --- |
| Burning Hands | `spell.elemental.burning-hands` |
| Lightning Bolt | `spell.elemental.lightning-bolt` |

### Spirit

| Display label | Stable ID |
| --- | --- |
| Detect Invisibility | `spell.spirit.detect-invisibility` |
| Invisibility | `spell.spirit.invisibility` |
| Magic Missile | `spell.spirit.magic-missile` |
| Magic Missile Storm | `spell.spirit.magic-missile-storm` |
| Phantasmal Image | `spell.spirit.phantasmal-image` |

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

## Travel events

| Display label | Stable ID |
| --- | --- |
| Aether Squall | `event.travel.aether-squall` |
| Coolant Leak | `event.travel.coolant-leak` |
| Crew Dispute | `event.travel.crew-dispute` |
| Derelict Signal | `event.travel.derelict-signal` |
| Distress Call | `event.travel.distress-call` |
| Pirate Shadow | `event.travel.pirate-shadow` |
| Starway Echo | `event.travel.starway-echo` |
| Stowaway | `event.travel.stowaway` |

### Explicit travel-event choices

| Display label | Stable ID |
| --- | --- |
| Isolate the loop | `event.travel.coolant-leak.choice.isolate` |
| Reduce power | `event.travel.coolant-leak.choice.reduce-power` |
| Repair the leak | `event.travel.coolant-leak.choice.repair` |

## Ship modules

| Display label | Stable ID |
| --- | --- |
| Cargo Hold | `module.cargo.hold` |
| Helm | `module.command.helm` |
| Boarding Lock | `module.contact.boarding-lock` |
| Psychic Resonator | `module.contact.psychic-resonator` |
| Signal Lantern | `module.contact.signal-lantern` |
| Energy Shield | `module.defense.energy-shield` |
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
| Ship Figurehead | `module.prow.figurehead` |
| Prow Ram | `module.prow.ram` |
| Lore Vault | `module.research.lore-vault` |
| Salvage Rig | `module.utility.salvage-rig` |
| Deck Battery | `module.weapon.deck-battery` |
| Alchemy Lab | `module.workshop.alchemy-lab` |
| Enchanting Chamber | `module.workshop.enchanting-chamber` |
| Fabricator | `module.workshop.fabricator` |

## Ship weapon configurations

| Display label | Stable ID |
| --- | --- |
| Aether Energy Cannon | `ship.weapon.arcane.aether-cannon` |
| Diesel Shell Cannon | `ship.weapon.industrial.diesel-shell-cannon` |
| Atomic Shell Cannon | `ship.weapon.industrial.atomic-shell-cannon` |

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

The requested categories contain 152 unique explicit gameplay IDs. No duplicate
ID is assigned to two different labels within these tables. Repeated references
to the same ID across design documents were consolidated under one entry.

The missing Heritage Perk IDs noted above remain future content-authoring
work. This inventory does not synthesize them from display text.
