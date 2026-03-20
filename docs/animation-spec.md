# Animation Controller Specification

*This document describes what animations are needed and how they should feel.*

---

## Player Animations

### Movement

| Animation | Trigger | Duration | Notes |
|-----------|---------|----------|-------|
| Idle | Standing still | Loop | Subtle breathing, coat movement |
| Walk | WASD | Loop | ~1 second cycle |
| Run | Shift + WASD | Loop | Faster, coat trails |
| Jump Rise | Space | 0.5s | Anticipation crouch, launch |
| Jump Fall | Airborne | Loop | Arms out for balance |
| Land | Ground contact | 0.3s | Quick recovery |

### Allomancy

| Animation | Trigger | Duration | Notes |
|-----------|---------|----------|-------|
| Steel Push | Right Click | 0.4s | Arm thrust forward, palm out |
| Iron Pull | Left Click | 0.4s | Arm extend toward target |
| Blue Lines Active | Tab (toggle) | Loop | Blue glow, hands raised |
| Pewter Burn | Q | Loop | Stance widens, muscles tense |
| Pewter Drag | Pewter depleted | 2s | Collapse, gasping |
| Pewter Attack | Q + Attack | Variable | Heavy swings |

### Combat

| Animation | Trigger | Duration | Notes |
|-----------|---------|----------|-------|
| Coin Throw | Throw key | 0.3s | Arm flick, coin flies |
| Coin Volley | Rapid throws | Loop | Continuous throwing |
| Hit React | Taking damage | 0.2s | Knocked back, pain |
| Death | HP = 0 | 2s | Collapse, fade out |

---

## Enemy Animations

### Skaa Soldier

| Animation | Trigger | Duration |
|-----------|---------|----------|
| Patrol | Idle AI | Loop |
| Chase | Player detected | Run |
| Attack | Close to player | Sword swing |
| Hit | Taking damage | Stagger back |
| Death | HP = 0 | Collapse |

### Steel Inquisitor

| Animation | Trigger | Duration | Notes |
|-----------|---------|----------|-------|
| Steel Push | Combat | 0.5s | Dramatic arm thrust |
| Iron Pull | Combat | 0.5s | Grabbing motion |
| Pewter Rage | HP < 50% | Loop | Faster, red glow |
| Normal stance | Default | Loop | Hands clasped behind |

---

## Animation Priorities

### Phase 1 (Sprint 1)
- [ ] Idle
- [ ] Walk
- [ ] Run
- [ ] Jump/Fall/Land
- [ ] Steel Push
- [ ] Iron Pull

### Phase 2 (Sprint 2)
- [ ] Coin Throw
- [ ] Hit React
- [ ] Death
- [ ] Blue Lines Active

### Phase 3 (Sprint 3)
- [ ] Pewter Burn
- [ ] Pewter Drag
- [ ] Pewter Attack
- [ ] All enemy animations

---

## Animation Style Guide

### Movement Feel
- **Fast and fluid** — Mistborn characters are acrobatic
- **Coat/cloak physics** — Important for visual identity
- **Grounded** — Even with flying, feet/stance visible

### Combat Feel
- **Explosive** — Quick attacks, heavy impacts
- **Weight** — Pewter attacks have heft
- **Recovery frames** — Brief pauses between actions

### Allomancy Feel
- **Centered** — Power flows from chest/heart
- **Blue glow** — Visible during metal burning
- **Effortless** — Pushing feels natural, not like casting

---

## Technical Notes

### Blend Trees
- Use 2D Blend Tree for movement (forward + strafe)
- Blend trees for combat states

### Animation Events
- Steel Push: Trigger sound at 0.2s, apply force at 0.25s
- Iron Pull: Trigger sound at 0.2s, apply force at 0.3s
- Land: Trigger footstep at 0s

### Avatar Mask
- Allomancy animations only affect upper body
- Lower body continues movement
- Allows running while pushing/pulling

---

## Reference Games
- Assassin's Creed (movement)
- Warframe (combat fluidity)
- Dying Light (parkour feel)
