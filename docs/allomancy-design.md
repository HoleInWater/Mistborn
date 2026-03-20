# Allomancy Design Document

## What is Allomancy?

Allomancy is the magic system from Brandon Sanderson's Mistborn series. Here's the short version:

1. You swallow metal (flakes, dust, filings)
2. You "burn" it — the metal starts giving you power
3. You get an effect based on which metal you're burning
4. When the metal runs out, the power stops

Simple rules, but the combinations and physics make it interesting.

## The 16 Metals

| Metal | Push/Pull | Effect | Status |
|-------|-----------|--------|--------|
| Steel | Push | Propels metal objects away from you | **ACTIVE** |
| Iron | Pull | Attracts metal objects toward you | **ACTIVE** |
| Pewter | — | Enhanced strength and durability | Future |
| Tin | — | Enhanced senses | Future |
| Brass | Soothe | Calms emotional states | Future |
| Zinc | Riot | Provokes emotional states | Future |
| Copper | — | Masks Allomancy detection | Future |
| Bronze | — | Detects burning Allomancers | Future |
| Gold | — | See your own possible pasts | Future |
| Electrum | — | See your own possible futures | Future |
| Atium | — | See others' futures | Future |
| Malatium | — | See true nature of things | Future |
| Aluminum | — | Drains your own metals | Future |
| Duralumin | — | Drains others' metals | Future |
| Chromium | — | Stores your metals externally | Future |
| Nicrosil | — | Stores others' metals | Future |
| Bendalloy | — | Time dilation bubble | Future |
| Cadmium | — | Time compression bubble | Future |

## Steel & Iron: The Physical Pair

These are our focus for Sprint 1. They work on external metal objects and follow Newton's Third Law.

### The Core Rule

**Whatever you push or pull on, pushes/pulls back.**

- Light object (coin): Coin flies away, you stay put
- Heavy/anchored object (metal bracket in wall): You fly toward it

### Steel Push

Pushes metal objects **away** from you.

Key uses:
- Launch coins as projectiles at enemies
- Push metal enemies backward
- Push off metal ground to fly upward

### Iron Pull

Pulls metal objects **toward** you.

Key uses:
- Disarm enemies with metal weapons
- Pull coins back to you
- Pull yourself toward metal anchors to swing/climb

## Allomantic Sight

When burning Steel or Iron, you see faint blue lines extending from your chest to every metal object in range.

- Line brightness = object mass (brighter = heavier)
- Line length = distance (longer = farther)

Currently implemented with Debug.DrawLine. Will upgrade to proper VFX in Sprint 2.

## Metal Reserves

Each metal has its own reserve (0-100). Burning consumes the reserve at a rate measured in units per second.

- Burning more metals at once = faster total drain
- A Mistborn burns all 16; a Misting burns only one

## Open Questions

> ❓ TEAM DECISION NEEDED: What should the starting reserve amount be for Steel/Iron? (50? 100?)

> ❓ TEAM DECISION NEEDED: Should burn rate increase while flaring (burning at max power)?

> ❓ TEAM DECISION NEEDED: Should we add a "metal reserves regenerate over time" mechanic or keep it as fixed amounts?
