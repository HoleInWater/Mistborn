# Metal Economy Design

*How players get and use metals.*

---

## Overview

Metals are the ammunition for Allomancy. Managing them is key to survival. This document covers how metals are acquired, conserved, and upgraded.

---

## Starting Inventory

New players start with:
| Metal | Starting Reserve | Burn Rate |
|-------|-----------------|-----------|
| Steel | 100 | 2/sec |
| Iron | 100 | 2/sec |

**Rationale:** Enough to learn with, not enough to spam.

---

## Acquisition Methods

### 1. Metal Pouches

Purchase metal pouches from suppliers.

| Pouch | Contents | Cost |
|--------|----------|------|
| Small | 50 units | 10 bronze |
| Medium | 100 units | 20 bronze |
| Large | 200 units | 35 bronze |

**Locations:** Black market, some shops, friendly contacts

---

### 2. Enemy Drops

Defeating enemies may drop metal.

| Enemy | Drop | Chance |
|-------|------|--------|
| Skaa Guard | 10-20 Steel | 50% |
| Noble Guard | 20-40 Steel | 75% |
| Koloss | 50 Steel | 100% |
| Inquisitor | 100 Steel + Iron | 100% |

---

### 3. Environmental Collection

Pick up metal objects in the world.

- Coins on ground
- Metal debris
- Scrap metal
- Weapons from enemies

**Collection:** Walk over metal, auto-collected
**Conversion:** Metal converts to reserve (1 metal = 1 reserve unit)

---

### 4. Metal Caches

Hidden throughout the world.

| Cache Type | Contents | Rarity |
|-----------|----------|--------|
| Skaa Stash | 50 Steel | Common |
| Noble Drop | 100 mixed | Uncommon |
| Inquisitor Supply | 200 all | Rare |

---

### 5. Ashes (Currency)

Earned from:
- Completing missions
- Defeating enemies
- Finding secrets
- Selling items

**Spend on:** Metal pouches, upgrades, information

---

## Conservation Strategies

### 1. Precision Over Spam

Instead of constant pushing:
- Wait for right moment
- One precise push > multiple weak pushes
- Save metal for when needed

### 2. Environmental Metal

Don't waste coins on:
- Moving boxes
- Opening doors (push shoulder)
- Attacking weak enemies (physical attack)

### 3. Resource Management

Know your reserve:
- HUD shows current amount
- Blue = healthy, Yellow = low, Red = critical
- Plan around critical reserves

---

## Upgrade Paths

### Reserve Capacity

Increase max reserve per metal.

| Upgrade | Effect | Cost | Level |
|---------|--------|------|-------|
| Standard Reserves | +25 max | Free | Start |
| Improved Reserves | +50 max | 100 ashes | 2 |
| Elite Reserves | +100 max | 250 ashes | 4 |
| Master Reserves | +200 max | 500 ashes | 6 |

### Burn Efficiency

Decrease burn rate.

| Upgrade | Effect | Cost | Level |
|---------|--------|------|-------|
| Controlled Burn | -20% burn | 150 ashes | 3 |
| Efficient Burn | -35% burn | 300 ashes | 5 |
| Minimal Burn | -50% burn | 600 ashes | 7 |

### Recovery Rate

Passive regeneration.

| Upgrade | Effect | Cost | Level |
|---------|--------|------|-------|
| None | No regen | - | Start |
| Slow Recovery | +1/sec | 200 ashes | 3 |
| Normal Recovery | +2/sec | 400 ashes | 5 |
| Fast Recovery | +4/sec | 800 ashes | 7 |

---

## Late Game

Once upgraded:
- 200 reserve + 50% efficiency + 4/sec regen
- ~40 seconds of constant use
- Regenerates 160 units in downtime

**Feels:** Never running out
**Balance:** Strong but not unlimited

---

## Questions

1. Metal scarcity or abundance?
2. Trading system between players?
3. Special metals (Atium, etc.)?
