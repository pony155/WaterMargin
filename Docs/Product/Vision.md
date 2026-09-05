# Spelljammer product vision

## Status

This document defines the new product direction. The repository implements a
small deterministic voyage prototype; most of the broader sandbox described
below remains planned.

## Vision

Spelljammer is the working title of a 2D outer-space sandbox roguelike about
keeping a fragile ship and its crew alive beyond the edge of reliable maps. A
run begins at a free anchorage with limited fuel and provisions. The player
charts a route through a hostile void, boards wrecks, bargains or fights,
changes the ship, and decides when the promise of one more discovery is no
longer worth the risk of never returning.

The tone combines weathered maritime adventure, strange skies, precarious
community, and the freedom of a systemic sandbox. The current design and
assets remain original; adopting the working name does not import another
setting's lore, cosmology, rules, creatures, visual designs, text, or assets.

## Player fantasy

The player is the master of a small independent vessel, not a chosen savior.
They choose routes, allocate scarce space and labor, accept contracts, recruit
people with conflicting needs, and live with the physical and political trail
of each voyage. The ship gradually becomes a record of those choices.

## Pillars

- **The ship is home:** rooms, modules, cargo, scars, and crew form the player's
  persistent center of gravity.
- **Unknown routes matter:** a seeded chart reveals itself through travel;
  distance, supplies, hazards, and incomplete knowledge make navigation a
  strategic commitment.
- **Systemic voyage stories:** crew, ship, environment, factions, resources,
  and encounters interact to create outcomes rather than follow a fixed plot.
- **Classless characters:** attributes describe capability and independently
  trained skills determine what a character can do; duties never lock future
  progression.
- **Risk has an exit:** returning early with a small prize is a valid result.
  Staying out longer can transform a campaign or end it.
- **Readable causality:** the interface explains costs, blockers, hazards, and
  why autonomous crew actions occurred.
- **Original strange worlds:** locations and societies follow Spelljammer's own
  material, ecological, and cultural logic rather than importing another
  setting's canon.

## Campaign shape

A campaign is a sequence of voyages. Returning converts discoveries, cargo,
relationships, and wounds into persistent change at the anchorage. A lost ship
ends that run, while unlocked knowledge and scenario options may carry into a
broader player profile. The precise metaprogression contract is planned and
must not undermine the stakes of individual voyages.

## Product principles

- Build a small, complete voyage loop before adding a large universe.
- Let one resource or event participate in several systems.
- Keep authoritative simulation deterministic and independent from rendering,
  localized text, UI timing, and thread completion order.
- Generate from explicit seeds and preserve the commands needed to reproduce a
  failure.
- Make retreat, damage, debt, and partial success playable outcomes.
- Keep definitions data-driven and identities stable for saves and modding.
- Describe roadmap work as planned until it is implemented and verified.
