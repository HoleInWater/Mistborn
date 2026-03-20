# Architecture Overview

How the systems fit together.

## Script Dependency Diagram

```
                    ┌─────────────────────┐
                    │  AllomancerController │ (owns all reserves)
                    └──────────┬──────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  SteelPushAbility│  │  IronPullAbility │  │  AllomanticSight│ (reads targets)
└────────┬────────┘  └────────┬────────┘  └─────────────────┘
         │                    │
         │         AllomanticTarget (many)
         │              ▲
         │              │
         └──────────────┘
```

## Core Systems

### AllomancerController
- Sits on the Player GameObject
- Manages all 16 MetalReserve instances
- Handles consumption over time
- Fires events when metals deplete

### SteelPushAbility / IronPullAbility
- Also on the Player GameObject
- Detect metal targets in range
- Apply physics forces to targets
- Call StartBurning/StopBurning on AllomancerController

### AllomanticTarget
- Sits on any metal object in the world
- Reports its metal type, mass, and anchored status
- Required component: Rigidbody

### AllomanticSight
- Reads all AllomanticTargets in range
- Draws lines for the player to see
- Purely visual — doesn't affect gameplay

## Player Setup Checklist

When setting up a new scene, the Player GameObject needs:

1. `CharacterController` (for movement)
2. `AllomancerController`
3. `SteelPushAbility`
4. `IronPullAbility`
5. `AllomanticSight`
6. `PlayerController`
7. `PlayerCamera` (as child or separate object)

## Scene Objects

Metal objects in the scene need:

1. `Rigidbody`
2. `AllomanticTarget` (set metalType, mass, isAnchored)
3. `Collider` (for overlap detection)
4. Visual mesh

Naming convention: `Metal_Coin_01`, `Metal_Bracket_Wall_01`
