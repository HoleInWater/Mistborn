# What We've Built — Project Status

*Last updated: March 21, 2026*

---

## Current State: IN DEVELOPMENT

**The team has implemented Allomancy mechanics, combat, movement abilities, and UI systems.**

---

## Scripts Created

### Allomancy (16 Metals)
| Script | Status | What It Does |
|--------|--------|--------------|
| `SteelPush.cs` | ✅ Done | Push metals away from player |
| `IronPull.cs` | ✅ Done | Pull metals toward player |
| `PewterBurn.cs` | ✅ Done | Enhanced strength, speed, healing |
| `TinEnhance.cs` | ✅ Done | Enhanced senses, zoom vision |
| `ZincRiot.cs` | ✅ Done | Enrage enemies |
| `BrassSoothe.cs` | ✅ Done | Calm enemies |
| `CopperCloud.cs` | ✅ Done | Hide Allomantic pulses |
| `BronzeDetect.cs` | ✅ Done | Detect burning Allomancers |
| `AtiumBurn.cs` | ✅ Done | See enemy futures |
| `GoldPast.cs` | ✅ Done | See past selves |
| `ElectrumFuture.cs` | ✅ Done | See own futures |
| `AluminumPurge.cs` | ✅ Done | Purge all metal reserves |
| `DuraluminBurst.cs` | ✅ Done | Mega Allomancy burst |
| `MalatiumReveal.cs` | ✅ Done | Reveal true nature |
| `TimeBubble.cs` | ✅ Done | Speed/slow time bubbles |
| `Allomancer.cs` | ✅ Done | Base Allomancer component |
| `AllomanticSight.cs` | ✅ Done | Blue lines to metals |
| `MetalReserveManager.cs` | ✅ Done | Manage all metal reserves |

### Player Systems
| Script | Status | What It Does |
|--------|--------|--------------|
| `PlayerMove.cs` | ✅ Done | WASD movement, third-person camera |
| `AnimationStateController.cs` | ✅ Done | Player animations |
| `PlayerCombat.cs` | ✅ Done | Basic attack system |
| `Health.cs` | ✅ Done | Player health |
| `PlayerStamina.cs` | ✅ Done | Sprint stamina |
| `BlockAbility.cs` | ✅ Done | Block with pewter |
| `DashAbility.cs` | ✅ Done | Pewter dash |
| `DodgeRoll.cs` | ✅ Done | Invincible dodge roll |
| `WallRun.cs` | ✅ Done | Wall running |
| `MetalMagnet.cs` | ✅ Done | Attract nearby metals |
| `StealthSystem.cs` | ✅ Done | Crouch for stealth |
| `VaultJump.cs` | ✅ Done | Vault over obstacles |

### Combat
| Script | Status | What It Does |
|--------|--------|--------------|
| `ComboSystem.cs` | ✅ Done | Combo attacks with multipliers |
| `EnemyHealth.cs` | ✅ Done | Enemy health system |
| `AIController.cs` | ✅ Done | Enemy AI |

### UI/HUD
| Script | Status | What It Does |
|--------|--------|--------------|
| `MetalHUD.cs` | ✅ Done | Display metal reserves |
| `StaminaHUD.cs` | ✅ Done | Stamina bar |

### Effects & Audio
| Script | Status | What It Does |
|--------|--------|--------------|
| `ParticleEffect.cs` | ✅ Done | VFX for abilities |
| `SoundManager.cs` | ✅ Done | Audio management |

### World
| Script | Status | What It Does |
|--------|--------|--------------|
| `MetalPickup.cs` | ✅ Done | Pickup metal for reserves |

### Skill Tree
| Script | Status | What It Does |
|--------|--------|--------------|
| `AllomancySkill.cs` | ✅ Done | Skill definition |
| `AllomancySkillTreeController.cs` | ✅ Done | Skill tree UI |
| `AllomancySkillUI.cs` | ✅ Done | Skill UI logic |

---

## Total Scripts: 32

---

## Assets Created

| Asset | Status |
|-------|--------|
| `Scene 1.unity` | ✅ Done - Playable test scene |
| `PlayerController.controller` | ✅ Done - Animations |
| Mixamo X Bot models | ✅ Done |
| `Ground(Temp).mat` | ✅ Done |
| `Player(Temp).mat` | ✅ Done |
| HUD (HUD.uss, HUD.uxml) | ✅ Done |
| Health Bar sprites (14) | ✅ Done |
| Metal Progress Bar sprites (14) | ✅ Done |
| Skill Tree UI (AllomancySkillVisualTree.uss/uxml) | ✅ Done |
| Allomancy sprites | ✅ Done |

---

## Controls Implemented

| Action | Key |
|--------|-----|
| Move | WASD |
| Look | Mouse |
| Sprint | Left Shift |
| Dodge Roll | Space |
| Steel Push | Right Mouse |
| Iron Pull | Left Mouse |
| Pewter Enhancement | Q |
| Tin Senses | E |
| Zinc Riot | Z |
| Brass Soothe | X |
| Copper Cloud | C |
| Bronze Detect | V |
| Atium Future | T |
| Gold Past | G |
| Electrum Future | 5 |
| Aluminum Purge | F |
| Duralumin Burst | R |
| Bendalloy Bubble | 8 |
| Cadmium Bubble | 9 |
| Skill Tree | Tab |

---

## What's Left

**In Unity:**
- Tune metal physics
- Add particle effects
- Balance damage numbers
- Create enemy prefabs
- Add audio clips

**Art/Assets:**
- Real character model
- Environment art
- Weapon models
- More VFX

---

## Key Docs

| Doc | Purpose |
|-----|---------|
| `onboarding.md` | Setup guide |
| `allomancy-design.md` | How Allomancy works |
| `magic-qa-reference.md` | All 16 metals |
| `mistborn-era-one-lore.md` | Lore reference |
