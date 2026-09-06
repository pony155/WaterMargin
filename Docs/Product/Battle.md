# Battle and tactical encounters

## Status

This document defines the planned combat framework for ship engagements,
boarding actions, shipboard melee, space-ruin expeditions, EVA fighting,
settlement conflicts, and related tactical encounters. It is not implemented
yet. The current voyage prototype has aggregate Hull damage but no weapons,
combatants, tactical spaces, injuries, initiative, hostile AI, or battle saves.

Battle is one possible way to resolve an encounter, not a separate campaign
genre. Exploration, negotiation, stealth, rescue, sabotage, surrender, and
retreat remain valid before and during combat.

## Design goals

- Use one deterministic action and injury framework across different battle
  locations instead of creating unrelated combat minigames.
- Let ship layout, ruin structure, cover, atmosphere, gravity, visibility,
  hazards, and objectives matter as much as raw damage.
- Make crew attributes, skills, equipment, positions, learned techniques,
  access Feats, and racial Talents produce understandable tactical options.
- Connect ship-to-ship fire, module damage, boarding, internal defense, and
  disengagement without discarding consequences between scales.
- Support Arcane, Industrial, and hybrid equipment without making one path the
  mandatory combat build.
- Make injury, ammunition, Focus, Psychic Strain, damaged modules, lost cargo,
  prisoners, witnesses, and faction consequences persist after battle.
- Keep surrender, rescue, capture, escape, delay, and partial success as real
  outcomes; eliminating every opponent is not the default objective.
- Bound pathfinding, visibility, reactions, projectiles, effects, AI planning,
  and per-tick work.

## Combat contexts

All contexts share actors, actions, timing, detection, damage, conditions, and
save rules while supplying different spatial and environmental data.

| Context | Stable ID | Tactical space | Typical objectives |
| --- | --- | --- | --- |
| Ship engagement | `combat.context.ship` | Range bands, relative vectors, firing arcs, and local objects | Escape, disable, escort, blockade, capture, or destroy |
| Boarding and ship interior | `combat.context.boarding` | Compartments connected by doors, hatches, ladders, ducts, and breaches | Repel, seize a module, rescue crew, sabotage, or reach a lock |
| Space ruin | `combat.context.ruin` | Authored or generated zones, passages, seals, traps, and vertical links | Explore, recover an artifact, survive defenses, or withdraw |
| EVA and hull exterior | `combat.context.eva` | Anchors, surfaces, open gaps, debris, and tether routes | Repair, cross hulls, cut entry, rescue, or defend an exterior module |
| Station or settlement | `combat.context.settlement` | Rooms, streets, docks, crowds, restricted areas, and exits | Protect civilians, arrest, escape, hold ground, or negotiate |
| Surface expedition | `combat.context.surface` | Bounded terrain zones, elevation, weather, hazards, and extraction points | Survey, hunt, rescue, defend camp, or reach transport |

The first implementation uses graphs of tactically meaningful zones rather
than seamless three-dimensional terrain. A zone can be a ship compartment,
ruin chamber, stretch of hull, street, or terrain area. Authored links encode
doors, cover, distance, height, pressure seals, line of sight, and movement
requirements.

## Encounter structure

A tactical encounter declares:

- stable encounter definition and instance IDs;
- context, location, participants, teams, knowledge, and initial placements;
- objectives, optional objectives, failure conditions, and escape routes;
- zone graph, cover, visibility, atmosphere, gravity, lighting, and hazards;
- neutral actors, civilians, prisoners, cargo, and protected infrastructure;
- reinforcement sources, limits, arrival conditions, and warning rules;
- surrender, negotiation, retreat, pursuit, and cleanup behavior;
- rewards, salvage rights, witnesses, laws, and faction consequences; and
- owned deterministic random streams and bounded simulation budgets.

An encounter is published only after every participant has a legal placement
and at least one declared way to pursue, abandon, or fail the primary objective.
Procedural assembly selects compatible authored rooms, zones, opponents,
hazards, and objectives; it does not generate unreviewed narrative text.

## Time and command model

Authoritative combat advances on the fixed simulation tick. The player may
pause at allowed command boundaries to inspect state and issue orders, but
opening a panel or taking longer to decide never changes the result.

Actions use explicit phases:

1. **Declare:** select actor, action, target, route, equipment, and parameters.
2. **Validate:** check knowledge, range, access, resources, friendly fire, and
   current command authority.
3. **Reserve:** reserve ammunition, charge, Focus, Stamina, Psychic Strain
   capacity, stations, and required items.
4. **Prepare:** spend wind-up time, aim, move, speak, cast, reload, or operate a
   station.
5. **Commit:** resolve the action and publish its effects atomically.
6. **Recover:** apply recovery time, sustain costs, exposure, and cooldown.

Movement, weapon fire, spells, psychic techniques, repairs, medical actions,
and environmental changes can overlap. Equal-tick conflicts use documented
priority categories and stable actor IDs as final tie-breakers, never UI order
or thread completion timing.

A reaction reserves attention or equipment before its trigger occurs. Overwatch,
parry, intercept, counterspell, emergency seal, and protective movement are
examples. Actors cannot receive unlimited free reactions from one event.

## Action resolution

Combat uses the same contextual capability model as ordinary crew work:

```text
relevant attribute + relevant skill + equipment + assistance
+ technique and access + circumstances
```

Examples include:

- Strength plus Melee to force an armored opponent away from a hatch;
- Agility plus Archery to fire a bow across a low-gravity ruin chamber;
- Intelligence plus Gunnery to calculate a shot through moving debris;
- Willpower plus Magic to sustain Brace Ward under fire, requiring
  `access.magic` and the known spell;
- Charisma plus Command to coordinate a fighting withdrawal;
- Toughness plus EVA to remain functional after suit damage; and
- Intelligence plus Engineering to disable an ancient defense without
  destroying it.

Attack resolution separates contact, protection, and harm. First determine
whether the action reaches the target, then apply cover, armor, wards, shields,
and resistance, and finally commit bounded harm and secondary effects. This
keeps a missed shot distinct from a shot stopped by armor.

The exact formula and numeric scale remain deferred until prototype balancing.
The action record must preserve inputs, modifiers, random result, mitigation,
harm, resource use, and rejection or failure reason.

## Spatial rules

Zones contain bounded capacity and tagged positions such as exposed, covered,
elevated, sealed, unstable, anchored, occupied, or hazardous. Links define
movement time, direction, access, traversal skill, visibility, and whether a
door or barrier can close across them.

Distance uses contextual range bands rather than one universal meter scale:

- personal encounters use engaged, near, far, and beyond;
- ship encounters use grappled, close, exchange, long, and contact-only; and
- sensors can detect beyond weapon range with confidence and identity limits.

Content definitions map range bands to authored distances when exact units are
needed. A personal bow cannot target a ship merely because both definitions say
"far."

Movement does not provide cost-free attacks. Crossing a watched link, leaving
engagement, opening a pressure door, climbing, drifting without an anchor, or
carrying an injured character can expose explicit reactions and extra time.

## Detection and surprise

Combat begins from encounter knowledge rather than omniscient team vision.
Each actor can have Unknown, Detected, Located, Identified, and Assessed
knowledge states with source, confidence, and last observation tick.

Sensors, sight, sound, aether traces, psychic signals, tracks, damage, and
faction reports reveal different facts. Stealth changes evidence and detection;
it does not make an actor absent from authoritative collision or hazard rules.

Surprise provides a bounded preparation or position advantage. It never grants
an entire team unlimited unanswered turns. An ambush must have a valid concealment
source, observation path, and trigger.

## Ship-to-ship engagements

Ship combat combines navigation, crew stations, weapons, defenses, damage
control, and objectives. It is not a duel between two aggregate health bars.

The tactical state tracks:

- relative range band, approach vector, velocity commitment, and escape route;
- sensor contact and identification confidence;
- available firing arcs, weapon readiness, ammunition, heat, and charge;
- power and aether allocation across propulsion, defense, weapons, sensors, and
  damaged networks;
- exposed or protected modules and compartments;
- nearby terrain such as debris, ruins, stations, storms, and gravity hazards;
  and
- boarding alignment, docking locks, tethers, breaches, and separation risk.

Crew act through stations. A Pilot maneuvers, Gunner aims and fires, Engineer
reroutes power or performs damage control, Ship Mage operates Arcane effects,
Mindwarden handles psychic threats, and Captain sets objectives and priorities.
Positions grant responsibility and authority, not automatic skill ranks.

Typical ship actions include scan, identify, close, evade, hold vector, fire,
intercept, brace, vent heat, reroute power, repair, jam, signal, negotiate,
launch boarding, repel boarding, rescue, disengage, and surrender.

Weapons target a ship, visible module, exposed compartment, projectile, or
declared area according to their tags. Precision targeting requires sufficient
knowledge and firing solution. Damage can breach hull, disable modules, ignite
cargo, injure crew, cut networks, change maneuver capability, or force
evacuation.

Disengagement is a contest of position, propulsion, detection, terrain, and
pursuit commitment. A faster ship does not automatically escape if trapped at
a dock, attached by boarding gear, or unable to navigate an unknown route.

## Boarding and shipboard melee

Boarding begins only through a valid transfer path: agreed docking, captured
Boarding Lock, matched-vector grapple, shuttle, EVA crossing, breached hull, or
an explicit Passage spell. The transition preserves ship motion, exterior
hazards, damage, crew positions, and reinforcement travel time.

The ship interior is a compartment graph. Doors, pressure seals, gravity,
lighting, smoke, fire, coolant, radiation, noise, narrow passages, and fragile
systems shape close combat. Defenders can lock routes, evacuate compartments,
cut gravity, vent an empty zone, move cargo as cover, isolate networks, or
counter-board through another path.

Melee weapons are valuable in cramped compartments because they are controllable
and do not require ammunition. Ranged weapons offer reach but must declare
penetration, recoil, noise, ammunition, firing clearance, and risk to hull,
crew, cargo, or machinery. A missed shot still follows its weapon's explicit
stray-shot rule.

Boarding objectives include capturing the bridge, disabling propulsion,
releasing prisoners, taking cargo, planting a device, rescuing survivors,
opening a lock, or holding until separation. Killing the entire opposing crew
is never an assumed requirement.

## Space-ruin expeditions

Ruin battles combine exploration state with tactical state. Entering combat
does not reveal the whole map or discard previous discoveries, opened seals,
translated scripts, disabled traps, moved objects, contamination, or consumed
supplies.

A ruin zone may contain:

- ancient guardians, scavengers, rival expeditions, predators, or trapped
  survivors;
- dormant defenses triggered by movement, sound, aether, heat, identity, or an
  incorrectly translated command;
- vacuum, unstable gravity, radiation, spores, psychic echoes, shifting
  passages, collapsing floors, or failing life support;
- control stations that can be understood through Engineering, Magic,
  Psionics, Language and Literacy, or Ancient Lore; and
- artifacts, records, living systems, or infrastructure that combat can
  permanently damage.

Players can scout, mark safe routes, translate warnings, disable systems,
negotiate with rivals, lure threats, seal chambers, or withdraw before combat.
During battle, completing the expedition objective may be more valuable than
holding every zone.

Ruin generation guarantees an entrance, a retreat or declared one-way rule,
bounded zone count, valid objective placement, and compatible environmental
requirements. It cannot place the only required tool beyond the obstacle that
requires it.

## EVA and exterior combat

EVA movement requires suit capability, propellant or anchors, and an explicit
route. Momentum, tether state, handholds, visibility, debris, radiation,
temperature, and decompression affect action timing and risk.

Forced movement is especially dangerous because leaving an anchor can turn an
ordinary hit into separation from the ship. A character drifting away enters a
rescue state with position and remaining support; they do not disappear merely
because they left the local battle graph.

Projectile recoil, hull penetration, suit puncture, and loose equipment use
explicit tags. Magic and Psionics still require their access Feats or innate
Talents and do not ignore vacuum, range, line of effect, or Psychic Strain.

## Settlement and surface conflicts

Ports, habitats, and settlements include civilians, laws, witnesses, property,
security responses, restricted weapons, and routes that remain important after
combat. Starting violence can raise Alarm, void agreements, close markets, or
create arrest and restitution demands even when the tactical action succeeds.

Surface expeditions add terrain, weather, local ecology, gravity, atmosphere,
vehicle access, and extraction timing. The player's crew remains a small unit;
armies and galaxy-scale battles are represented by strategic fronts while the
ship handles one bounded objective within them.

## Weapons, armor, and techniques

Weapons are data-authored items with stable IDs and tags for skill, grip, range,
delivery, damage type, penetration, recoil, ammunition or charge, preparation,
action time, recovery, noise, trace, and valid targets.

- Melee covers unarmed attacks and handheld close-combat weapons.
- Archery covers bows, crossbows, and unusual string-launched weapons.
- Gunnery covers personal firearms, mounted weapons, and ship weapons; each
  definition declares its supported scale.
- Magic and Psionics use their own skills, learned content, resources, and
  access requirements.
- Defense covers active protection, positioning, shields, and learned defensive
  techniques without becoming universal armor expertise.

Techniques provide bounded maneuvers such as disarm, suppress, pin, guard,
aimed shot, controlled burst, shield another, or fighting withdrawal. They are
learned separately from skills when their definition requires it. A high skill
improves execution but does not automatically grant every technique.

Armor declares coverage, protection, seals, mass, movement effects, power,
damage, and environmental compatibility. Protection trades mobility, noise,
heat, fatigue, perception, cost, or maintenance instead of being a simple
always-better number.

## Magic, Psionics, and enchantment

A character cannot cast because combat started. Spellcasting requires
`access.magic`, the known spell, sufficient resources, a legal target, and the
normal phases in [`Spells.md`](Spells.md). Casting can be interrupted, traced,
countered, or sustained across ticks.

Psychic techniques require `access.psionics` and follow consent, resistance,
privacy, information, and Psychic Strain rules in
[`PsychicAbilities.md`](PsychicAbilities.md). A failed intrusion cannot reveal
the information it failed to reach. Mental influence never silently replaces a
player command.

Enchantments modify compatible equipment, modules, or locations through
explicit effects. They do not grant knowledge of a spell or bypass its access
Feat unless the item definition casts independently and owns its costs,
targeting, and failure behavior.

## Harm, injury, and incapacitation

Characters do not rely on a large undifferentiated health pool. Harm resolves
through bounded severity bands such as superficial, wounded, critical, and
incapacitated, with authored injury tags for bleeding, fracture, burn, poison,
vacuum exposure, psychic shock, and similar consequences.

Armor, cover, wards, resistance, and Toughness can reduce or redirect harm when
their tags apply. Toughness improves endurance and survival; it does not make a
character immune to a blade, decompression, or reactor radiation.

Injuries can affect movement, senses, action time, equipment use, consciousness,
and recovery. Medicine can stabilize and treat them; Vitality magic can support
stabilization and natural recovery without erasing medical gameplay. Serious
injuries persist after the encounter.

An incapacitated actor can be rescued, captured, abandoned, or killed only
under explicit rules. Death-causing effects, bleed-out deadlines, stabilization,
and coup-de-grace actions are visible and do not arise from hidden narrative
fiat.

Ships resolve harm through defenses, armor, hull structure, compartments,
modules, networks, fires, breaches, and crew exposure as defined in
[`Ships.md`](Ships.md). Character and ship damage commit together when one
attack affects both.

## Morale, surrender, and prisoners

Combatants have authored goals and risk tolerances. They may retreat, surrender,
negotiate, protect another actor, abandon an objective, or refuse a suicidal
order when their state and authority rules allow it.

Surrender declares who yields, what equipment or ship control is transferred,
which actors are protected, and what happens if terms are broken. Prisoners
remain characters with needs, injuries, affiliation, knowledge, and legal
status. Detention consumes guarded space, food, air, medical care, and crew
attention.

Command, Negotiation, faction standing, visible losses, escape routes, and
credible threats can affect morale without becoming magical mind control.
Race never supplies a universal courage, aggression, or surrender rule.

## Tactical AI

AI actors use the same authoritative observations and legal actions as player
crew. Their definitions provide bounded priorities such as protect, capture,
delay, escape, feed, patrol, investigate, or negotiate, plus risk tolerance,
coordination, and known doctrine.

Planning searches have fixed depth, candidate, path, and time budgets. If no
preferred plan fits the budget, the actor chooses a documented safe fallback
such as hold, take cover, assist, withdraw, or wait. Stable IDs break equal
choices before a deterministic random stream is consulted.

Difficulty changes authored resources, coordination, preparation, and error
margins. It does not give AI secret player knowledge, unlimited reactions,
free ammunition, or damage immunity.

## End of battle and consequences

An encounter ends when its objective, withdrawal, surrender, separation, or
terminal conditions commit. The cleanup phase records:

- surviving, injured, incapacitated, dead, missing, captured, and rescued
  characters;
- ship, module, equipment, cargo, ruin, and environmental damage;
- ammunition, charge, Focus, Stamina, Psychic Strain, medicine, fuel, and time
  spent;
- recovered items, registered salvage, contraband, prisoners, and evidence;
- witnesses, reports, law violations, agreements, standing, and Alarm changes;
- incomplete objectives, escaping opponents, pursuit opportunities, and
  altered return routes; and
- knowledge gained about actors, weapons, locations, factions, and hazards.

Victory does not restore resources or injuries automatically. Retreat preserves
survivors and information at the cost of abandoned objectives, cargo, position,
or reputation. A battle result becomes normal persistent campaign state rather
than a temporary reward screen.

## Data and persistence

An authored encounter definition resembles:

```json
{
  "schemaVersion": 1,
  "id": "encounter.ruin.sealed-observatory",
  "nameKey": "encounter.ruin.sealed-observatory.name",
  "contextId": "combat.context.ruin",
  "zoneGraphId": "zone-graph.ruin.observatory-small",
  "objectiveIds": [
    "objective.ruin.recover-chart",
    "objective.common.withdraw"
  ],
  "participantTableId": "participants.ruin.guardians-low",
  "hazardTableId": "hazards.ruin.aether-vacuum",
  "actorLimit": 12,
  "reinforcementLimit": 4,
  "durationLimitTicks": 1800
}
```

Persistent battle state stores definition revisions, encounter seed and random
streams, tick, participants, teams, knowledge, zone state, objectives, reserved
resources, scheduled actions, reactions, projectiles, active effects, hazards,
injuries, ship damage, reinforcement state, retreat paths, and committed event
history.

Saving is allowed only at documented commit boundaries. Loading restores the
same next authoritative tick and never rerolls an attack, AI choice,
reinforcement, injury, or ruin layout. Presentation animation and selected UI
panels are not save state.

## Bounds and validation

Runtime limits include actors, zones, links, carried items, active effects,
scheduled actions, reactions per trigger, projectiles, reinforcements, path
expansions, visibility checks, AI candidates, event history, and maximum
encounter duration.

Content loading rejects:

- duplicate or missing encounter, actor, zone, objective, item, effect, and
  localization IDs;
- disconnected required objectives, illegal placements, missing retreat rules,
  or a zone graph beyond capacity;
- attacks without skill, range, target, delivery, protection, harm, evidence,
  ammunition or charge, and miss behavior;
- supernatural actions without access, learned-content, cost, targeting, and
  resistance requirements;
- reinforcements without source, limit, route, warning, and arrival condition;
- hazards, triggers, reactions, or AI plans capable of unbounded recursion;
- boarding without a valid transfer path or ship separation rule;
- ship attacks whose compartment and character consequences cannot commit
  atomically; and
- encounter completion that discards injuries, damage, spent resources,
  witnesses, faction consequences, or changed exploration state.

New content is validated completely before replacing the active catalog. A
failed reload leaves the previous definitions available.

## First playable battle scope

The first battle milestone should contain two connected encounters:

1. a small ship engagement with scanning, maneuver, one weapon, module damage,
   damage control, signaling, disengagement, and an optional boarding route;
2. a six-zone derelict or ruin expedition with four active crew, limited
   visibility, one environmental hazard, one hostile group, one ancient
   defense, one non-combat solution, and an extraction objective.

The same crew, equipment, ammunition, injuries, spell resources, Psychic
Strain, ship damage, knowledge, and faction consequences persist between the
two. The slice includes one melee action, one ranged action, one defensive
reaction, one spell or psychic technique with valid access, one medical
stabilization, one surrender or retreat, and one damaged object the player
wanted to preserve.

The milestone succeeds when the same seed and command sequence reproduces the
same battle, the player can explain every major modifier and injury, and
eliminating every opponent is not required. Large fleets, armies, seamless
three-dimensional movement, procedural tactical prose, destructible planets,
competitive multiplayer, and real-time physics simulation remain deferred.
