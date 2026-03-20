# HUD Design Document

## Overview

How the player sees information about their character, metal reserves, and the world.

---

## Screen Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [Health Bar]                              [Enemy Health]  │
│  ████████░░░░░░░ 50/100                    ████░░░ 75/100  │
│                                                             │
│                                                             │
│                      [GAME WORLD]                           │
│                                                             │
│                                                             │
│                                                             │
│                                                             │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  [STEEL]  ████████████░░░░  80/100     [IRON]  █████████  │
│             ● (burning)                         90/100     │
│                                                             │
│  [Controls Hint]                    [Minimap/Compass]       │
└─────────────────────────────────────────────────────────────┘
```

---

## HUD Elements

### 1. Player Health Bar
- **Position:** Top-left
- **Style:** Horizontal bar with fill
- **Shows:** Current HP / Max HP
- **States:**
  - Normal: Green fill
  - Low (< 30%): Red fill, pulsing
  - Pewter active: No damage taken animation

### 2. Metal Reserve Bars
- **Position:** Bottom of screen, center
- **Style:** Horizontal bars, side by side
- **Shows:** 
  - Metal name (STEEL, IRON)
  - Current amount / Max amount
  - Fill bar
  - Burning indicator (●) when active
- **States:**
  - Normal: Metal-specific color (Steel=blue, Iron=grey)
  - Burning: Bright cyan glow
  - Low (< 20%): Red, flashing
  - Empty: Greyed out

### 3. Enemy Health Bars
- **Position:** Above targeted enemy
- **Style:** Small bar
- **Shows:** Enemy HP when damaged or targeted
- **States:**
  - Hidden when full and not targeted
  - Shown when damaged
  - Red when enemy is attacking

### 4. Allomantic Sight Indicator
- **Position:** Center of screen
- **Style:** Subtle blue glow when active
- **Shows:** Blue lines rendering to metal objects

### 5. Compass/Minimap
- **Position:** Bottom-right corner
- **Style:** Circular compass or top-down minimap
- **Shows:** 
  - Direction facing
  - Nearby metal objects (dots)
  - Objectives (if any)

### 6. Controls Hint
- **Position:** Bottom-left
- **Style:** Small text
- **Shows:** Key hints for new players
- **Fades out** after first few minutes

---

## Metal Reserve UI Details

### Layout
```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│    ┌─────────────────┐         ┌─────────────────┐            │
│    │ STEEL           │         │ IRON            │            │
│    │ ██████████░░░░░ │         │ ██████████████ │            │
│    │ 75 / 100    ●   │         │ 100 / 100       │            │
│    └─────────────────┘         └─────────────────┘            │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### Colors
| State | Steel Color | Iron Color |
|-------|-------------|------------|
| Normal | Blue (#4A90D9) | Grey (#808080) |
| Burning | Cyan (#00FFFF) | Light Cyan |
| Low | Red (#FF4444) | Red |
| Empty | Dark Grey | Dark Grey |

### Burning Indicator
- **●** (filled circle) = Currently burning
- **○** (empty circle) = Not burning
- Shown next to reserve amount

---

## Future HUD Elements

### Pewter Enhancement
```
┌──────────────────┐
│ PEWTER           │
│ ████████░░░░░░░ │
│ 60 / 100    ●   │
└──────────────────┘
```

### Tin Enhancement
```
┌──────────────────┐
│ TIN              │
│ ████████████░░░ │
│ 85 / 100    ●   │
│ [Enhanced]      │
└──────────────────┘
```

### Cooldown Indicators
```
┌──────────────────┐
│ STEEL            │
│ ████████████████ │
│ READY            │
│ (F) Throw Coin  │
└──────────────────┘
```

---

## Interaction Prompts

When near interactable objects:

```
       ┌─────────────┐
       │   [E] Open   │
       │   [F] Push   │
       └─────────────┘
```

Position: Above the interactable object

---

## Damage Numbers

Float up from damage location:
- **Player damage taken:** Red numbers
- **Enemy damage dealt:** White numbers  
- **Critical hits:** Larger, yellow
- **Metal effect:** Blue numbers

---

## Status Effect Icons

Position: Near health bar or top-right

| Effect | Icon | Color |
|--------|------|-------|
| Pewter Drag | ⏳ | Orange |
| Tin Overload | ⚡ | Yellow |
| Slowed | 🐌 | Blue |
| Burning Metal | 🔥 | Cyan |

---

## Questions for Team

1. **HUD Style:** Dark/transparent or solid background?
2. **Minimap:** Do we want one? Only compass?
3. **Damage numbers:** Too cluttered or helpful feedback?
4. **Mobile support:** If ever needed, touch controls?

---

## Reference
- Dark Souls (minimal, unobtrusive)
- DOOM (clean, readable at a glance)
- Assassin's Creed (overlapping elements)
