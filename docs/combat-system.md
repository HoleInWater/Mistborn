# Combat System Design

## Overview

Combat in Mistborn Era One is built around Allomancy. The player doesn't swing swords — they push, pull, and fly. This document defines how combat works.

---

## Core Combat Loop

### The Flow
1. **Engage** — Use Allomantic Sight to locate metal on enemies
2. **Attack** — Push enemies' metal weapons away (disarm) or push enemies into obstacles
3. **Defend** — Create steel bubble to deflect incoming metal, or use pewter for strength
4. **Traverse** — Push off metal anchors to reposition, pull to swing
5. **Finish** — Coins as projectiles, or let physics do the work

### No Traditional Weapons (At Start)
Early game: All combat is through Allomancy. No melee weapons, no guns. Everything is steel/iron manipulation.

---

## Combat Abilities

### Steel Push
- **Primary Use:** Push metal weapons out of enemy hands
- **Tactical Use:** Push enemies off ledges, into walls, into each other
- **Traversal Use:** Push off anchored metal to fly
- **Damage:** Coins as projectiles (5-15 damage per coin)

### Iron Pull
- **Primary Use:** Pull weapons toward player (catch and use)
- **Tactical Use:** Pull enemies closer for positioning
- **Traversal Use:** Pull toward anchors for swings
- **Damage:** Direct pull does no damage, but positions enemies

### Pewter (Future)
- **Combat Mode:** Activating pewter makes you a melee threat
- **Benefits:** Double damage, faster attacks, pain resistance
- **Risk:** Pewter drag when depleted

### Defense
- **Steel Bubble:** Deflect incoming metal projectiles
- **Pewter:** Ignore stagger, keep fighting through damage
- **Movement:** Outmaneuver with push/pull traversal

---

## Enemy Types

### Skaa Soldier
- Basic melee attack
- Metal armor (can be pushed)
- No Allomancy
- HP: 50

### Noble Guard
- Better equipment, more HP
- May have metal weapons
- HP: 100

### Steel Inquisitor (Boss)
- Multiple Allomantic powers
- Metal-detection sight (darkness doesn't work)
- HP: 500
- Attacks:
  - Steelpush knockback
  - Ironpull disorientation
  - Pewter rage mode

### Koloss (Future)
- Hemalurgically enhanced
- Metal spikes visible
- HP: 200
- Strong but slow

---

## Damage System

### Player Damage
| Attack | Damage |
|--------|--------|
| Coin hit | 10 |
| Pushed into wall | 20-40 |
| Pushed off ledge | 30+ (fall damage) |
| Coin volley (3 coins) | 30 |

### Enemy Damage
| Enemy | Damage |
|-------|--------|
| Skaa Soldier | 15 |
| Noble Guard | 25 |
| Inquisitor push | 20 |
| Koloss | 40 |

### Player Health
- Base HP: 100
- Pewter: Ignore damage for duration
- Goldmind: Regenerate 50 HP over 10 seconds

---

## Status Effects

### Player
- **Bleeding:** Slow HP drain, slows movement (from sharp weapons)
- **Pewter Drag:** 50% speed/strength for 3 seconds after pewter depletes
- **Tin Overload:** Blinded by flashes, deafened by explosions

### Enemies
- **Pushed:** Brief stun when pushed
- **Disarmed:** Must recover weapon or fight barehanded
- **Panicked:** Guards flee if ally killed quickly

---

## Combat UI

- Health bar (top left or center bottom)
- Metal reserves (bottom center)
- Enemy health bars (above enemy when targeted)
- Damage numbers (float up from hit location)
- Status effect icons (near health bar)

---

## Questions for Team

1. **Melee weapons?** — Should late-game give player metal weapons?
2. **Health scaling?** — How many enemies per encounter?
3. **Death penalty?** — Save checkpoint or full restart?
4. **Difficulty modes?** — Enemy aggression, damage, health scaling?

---

## Reference

- Movement feel: Mirror's Edge, Assassin's Creed
- Combat feel: Warframe, DOOM
- Allomancy feel: Jet Set Radio (fluid, momentum-based)
