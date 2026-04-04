# Iron Sentinel — Boss Enemy

*One of the most dangerous enemies in the game.*

---

## Overview

Iron Sentinels are the Ashen King's elite soldiers. They've been pierced with Bloodforged spikes that give them Metallurgic powers. Their eyes have been replaced with metal spikes — they see through Metallurgy.

---

## Abilities

| Ability | Type | Description |
|---------|------|-------------|
| Steel Push | Metallurgy | Pushes metal objects and the player |
| Iron Pull | Metallurgy | Pulls metal objects and the player |
| Pewter Rage | Mode | HP below 50% triggers enhanced state |
| Metal Sight | Passive | Sees through walls, darkness doesn't work |

---

## Stats

| Stat | Value |
|------|-------|
| HP | 500 |
| Damage (Push) | 25 |
| Damage (Melee) | 35 |
| Speed | Normal → Fast (pewter rage) |
| Detection Range | 30m |
| Attack Range | Melee + 20m (Metallurgy) |

---

## Behavior

### Phase 1: Normal

- Patrols area
- Detects player through metal sight
- Uses steel push when player is in range
- Uses iron pull to bring player closer

### Phase 2: Combat

- Aggressive chasing
- Alternates push/pull to disorient player
- Will create distance if player gets too close

### Phase 3: Pewter Rage (HP < 50%)

- Eyes glow red
- Movement speed increases
- Attack speed increases
- Damage increases
- Uses pewter-enhanced melee attacks

---

## Combat Strategy (For Player)

1. **Stay mobile** — Sentinels track through metal
2. **Use cover** — Hide behind non-metal objects
3. **Counter push** — Push against their push for knockback
4. **Duralumin burst** — Emergency nuke their metals (future)
5. **Coppercloud** — Block their detection (future)

---

## Implementation Notes

See `EnemyBase.cs` and `EnemyLauncher.cs` for the code base.

### Needed Hooks
- [ ] Pewter rage mode trigger
- [ ] Metal-detection sight (doesn't use normal vision)
- [ ] Multiple spike powers (pull from multiple metals)

---

## Visual Design

- Dark robes
- Metal spikes through eyes
- Red glow when in pewter rage
- Intimidating presence

---

## Audio

- Deep, ominous footsteps
- Metal scraping sounds
- Pewter rage: heavy breathing, increased heartbeat
