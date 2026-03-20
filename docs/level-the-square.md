# Level Design — The广场 (The Square)

*The central plaza of Luthadel. Great for tutorial and early game.*

---

## Overview

The Square is the heart of Luthadel. It's a large open plaza where citizens gather, nobles parade, and the Steel Ministry holds ceremonies. For gameplay, it's perfect for learning Allomancy and early combat.

---

## Layout

```
                    [KREDIK SHAW]
                       (distant)

    ═══════════════════════════════════════
    ║                                      ║
    ║         THE PLAZA                    ║
    ║                                      ║
    ║    [Statue]                          ║
    ║         ◇                           ║
    ║                                      ║
    ║  [Tower]              [Tower]         ║
    ║     ◦                   ◦            ║
    ║                                      ║
    ═══════════════════════════════════════
    ║                                      ║
    ║   [Street]        [Street]            ║
    ║      ↓              ↓               ║
    ║                                      ║
```

---

## Key Features

### The Statue (Center)
- Large metal statue of the Lord Ruler
- Acts as a major Allomantic anchor
- **Gameplay:** Players can fly by pushing off it

### Metal Towers (Edges)
- Tall lamp posts / decorative towers
- Scattered around plaza edges
- **Gameplay:** Chain-push between them for long-distance traversal

### Plaza Ground
- Cobblestone texture
- Some metal grates
- **Gameplay:** Coins dropped here can be pushed off

### Surrounding Streets
- Lead to other districts
- Alleyways for stealth
- Wagons (pushable)
- **Gameplay:** Combat zone, mix of open and cover

---

## Allomantic Opportunities

### Traversal
| Anchor | Height | Use |
|--------|--------|-----|
| Statue | 15m | Main flight anchor |
| Tower 1 | 8m | Starting anchor |
| Tower 2 | 8m | Chain anchor |
| Ground Grates | 0m | Floor anchors |

### Objects
| Object | Mass | Anchored | Pushable |
|--------|------|----------|----------|
| Statue | 500 | Yes | No |
| Towers | 100 | Yes | No |
| Grates | 50 | Yes | No |
| Wagons | 30 | No | Yes |
| Coins | 0.1 | No | Yes |
| Guards' Weapons | 2 | No | Yes |

---

## Enemy Placement

### Tutorial Phase
- 2-3 Skaa Guards (patrol)
- Stationary (no threat, just atmosphere)

### Combat Phase
- 4-6 Noble Guards
- 1 Coinshot (rare, uses push)
- Patrol routes that overlap

### Challenge Phase
- Mix of Guards + Coinshot
- Inquisitor patrol (rare)

---

## Player Routes

### Route A: Ground Level
```
Start → Street → Through alley → Plaza
```
- Stealth route
- Avoid patrols
- Takes longer

### Route B: Rooftops
```
Start → Climb → Rooftop → Jump to plaza
```
- Fast route
- Uses wall-push
- Exposed

### Route C: Direct Flight
```
Start → Drop coin → Push off ground → Fly to statue → Push to target
```
- Allomancer route
- Shows off abilities
- Requires metal reserves

---

## Environmental Details

### Props
- Market stalls (some metal)
- Wagons
- Benches (metal frames)
- Fountain (central, dry)
- Lanterns (metal, lit)

### Atmosphere
- Ash falling (particle effect)
- Dim lighting
- Distant sounds of city

---

## Spawn Points

| Point | Position | Use |
|-------|----------|-----|
| Street Start | South edge | Player spawn |
| Plaza Center | Statue base | Fast travel |
| Rooftop | East building |Alternate spawn |

---

## Difficulty Scaling

| Difficulty | Enemies | Types |
|------------|---------|-------|
| Easy | 3 | Skaa only |
| Normal | 5 | Mix Skaa + Noble |
| Hard | 8 | All types + Coinshot |
| Nightmare | 10+ | All + Inquisitor |

---

## Questions for Team

1. How big should the plaza be? (50m x 50m? 100m x 100m?)
2. Should there be civilians walking around?
3. Time of day — dusk or night?
