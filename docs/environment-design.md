# Environment Design — Luthadel

## Overview

Luthadel is the capital city of the Final Empire. This document describes the environment design for the game.

---

## City Atmosphere

### The Ash
- **Constant ash falling** from the grey sky
- Everything has a layer of grey/reddish ash
- Ash accumulates on horizontal surfaces
- **Implications for art:**
  - Grey/ash color palette
  - Particles falling
  - Ash piles in corners

### Lighting
- **Perpetual twilight** — ash blocks sunlight
- Overcast, diffuse lighting
- Warm tones from distant fires
- Cool tones from shadows
- **Implications:**
  - Not bright and cheerful
  - Opportunities for dramatic lighting
  - Night sections are VERY dark

### The Mists
- Come every night
- Thicker than normal fog
- Glowing faintly
- Make visibility difficult
- **Implications:**
  - Night = misty
  - Visibility reduced
  - Allomantic sight more important at night

---

## Architectural Style

### Buildings
- **Thick walls** — stone and metal
- **High windows** — let in minimal light
- **Flat roofs** — for Allomancer traversal
- **Metal gutters and pipes** — Allomancy playground
- **Dark interiors** — contrast with ash-covered exteriors

### Streets
- **Narrow alleyways** — good for stealth
- **Wide noble streets** — formal, dangerous
- **Paved with cobblestone** — some metal grates
- **Ash drains** — channels for ash runoff

### Metal Content
Everything metal should be marked as AllomanticTarget:

| Object | Mass | Anchored |
|--------|------|----------|
| Gutters | 20 | Yes |
| Lamp posts | 50 | Yes |
| Street grates | 100 | Yes |
| Doors (metal) | 40 | Yes |
| Wagons | 30 | No |
| Weapons | 2 | No |
| Armor | 5 | No |

---

## District Design

### The Canton of Finance
- Noble houses
- Tall buildings
- Lots of metal (wealth display)
- Dangerous for Skaa

### The Pits
- Industrial area
- Factories
- Low buildings, open spaces
- Metal everywhere (great for traversal)

### The Central Dominance
- Lord Ruler's palace area
- Kredik Shaw
- Heavily guarded
- Lots of Steel Inquisitors

### The Caves
- Below the city
- Underground
- Natural rock
- Hidden passages

---

## Traversal Opportunities

### Rooftops
- Most buildings flat enough to run on
- Gaps between buildings = push opportunity
- Chimneys, water towers = anchors
- **Challenge:** Height + ash = limited visibility

### Alleyways
- Narrow, vertical spaces
- Push off walls to climb
- Good for stealth
- Pull enemies into walls

### Streets
- Open, exposed
- Push off lamp posts
- Pull from building to building
- Dangerous (enemies can see you)

### Underground
- Cave system below city
- Limited metal (natural rock)
- Stealth-focused gameplay
- Different feel from rooftop traversal

---

## Key Locations

### The广场 (The Square)
- Central gathering space
- Large metal statue in center (anchor!)
- Open, exposed
- **Use:** Tutorial area, combat arena

### Kredik Shaw
- Lord Ruler's palace
- Spires and towers
- Extremely metal-rich
- Flight heaven
- **Use:** End-game area

### Canal Systems
- Water channels through city
- Metal bridges
- Some covered sections
- **Use:** Stealth routes

---

## Environmental Hazards

| Hazard | Effect | Counter |
|--------|--------|---------|
| Ash Storms | Reduced visibility | Tin enhanced sight |
| Deep Gorges | Fall damage | Pewter, careful flying |
| Steel Inquisitors | Detection | Coppercloud |
| Noble Guards | Combat | Stealth, Allomancy |
| Ashborn | Damage over time | Stay indoors/under shelter |

---

## Art Direction

### Color Palette
- **Primary:** Grey (#606060) — ash, stone
- **Secondary:** Dark Red (#4A2020) — rust, blood
- **Accent:** Blue (#4080FF) — Allomancy, mist
- **Highlight:** Orange (#FF8040) — fires, warmth

### Materials
- **Stone:** Dark grey, weathered
- **Metal:** Dark iron, some brass (noble areas)
- **Wood:** Rare, valuable-looking
- **Fabric:** Dark, hooded cloaks

### Particle Effects
- Ash falling (constant, subtle)
- Smoke from fires
- Mist (at night)
- Sparks when metal hit

---

## Questions for Team

1. **Outdoor vs Indoor ratio?** Mostly outdoor traversal?
2. **Day/night cycle?** Affects gameplay significantly
3. **Vertical scale?** How tall should buildings be?
4. **LOD strategy?** Optimize for lower-end hardware

---

## Reference
- Assassin's Creed (rooftop traversal)
- Dishonored (vertical level design)
- Dark Souls (atmosphere, oppressive setting)
