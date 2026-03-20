# Steel & Iron Range — Technical Specs

## Official Range

Based on the **Mistborn Adventure Game** and book references:
- **~100 paces** (~200-250 feet / 60-75 meters) maximum range
- Range affected by:
  - Allomancer strength
  - Flaring (burning faster = extended range)
  - Metal mass (heavier = visible from further)

## Physics Model

From the books and community analysis:

### Force Formula (simplified)
```
Force = BaseForce × (PlayerWeight / TargetMass) × DistanceFactor × FlareBonus
```

### Distance Factor
- Close range: Full force (constant)
- Past certain point: Rapid falloff
- Past zenith: No effect

The exact falloff curve is debated — inverse square, or a "step function" where force is constant until a cutoff point.

### Visual Representation
```
Force
  ↑
  │███████
  │██████████
  │███████████████
  │█████████████████████████
  └────────────────────────────→ Distance
       Full     Cutoff   Max
```

## What This Means for Gameplay

| Scenario | Can you affect it? |
|----------|-------------------|
| Coin 10m away | Yes, full force |
| Coin 50m away | Yes, reduced |
| Coin 100m away | Maybe, very weak |
| Coin 200m away | No |
| Metal car 100m away | Yes |
| Airship 500m up | No (and probably aluminum) |
| Satellite | No |

## Zenith Point

There's a maximum height you can reach with a single push:
- Depends on your weight and the metal's mass
- Drop more metal = push again = go higher
- Vin hovered at her zenith until she dropped more metal

## Aluminum Note

**Planes are aluminum.** Aluminum is Allomantically inert — you can't push or pull it. The Malwish airship in TLM was specifically aluminum, which is why Wax couldn't reach it from the ground.

## Game Implementation

For our game, we use:
- `maxRange = 30f` (slightly under max for balance)
- `useDistanceFalloff = true` (past 15m)
- Heavier objects extend effective range by 1.5x

## References
- Mistborn Adventure Game (Crafty Games)
- Coppermind Wiki: Steel/Iron articles
- 17th Shard physics discussion threads
- Brandon Sanderson's Arcanum (WoB)
