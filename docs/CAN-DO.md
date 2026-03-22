# What We've Built — Project Status

*Last updated: March 21, 2026*

---

## Current State: IN DEVELOPMENT

**The team has implemented Allomancy mechanics, core game systems, movement abilities, and UI systems.**

---

## Scripts Created (By Folder)

### Systems/ - Core Game Logic
| Script | Status | What It Does |
|--------|--------|--------------|
| `Health.cs` | ✅ Done | Health with regeneration, damage, death, revive |
| `Stamina.cs` | ✅ Done | Stamina with regen, costs for abilities |
| `SaveSystem.cs` | ✅ Done | Save/load game data |
| `Inventory.cs` | ✅ Done | Player inventory with keys, items |
| `Checkpoint.cs` | ✅ Done | Save progress points |
| `RespawnSystem.cs` | ✅ Done | Handle death and respawn |
| `DamageDealer.cs` | ✅ Done | Deal damage to entities |
| `InputManager.cs` | ✅ Done | Centralized input handling |
| `AIController.cs` | ✅ Done | Basic enemy AI |
| `MetalReserveManager.cs` | ✅ Done | Manage allomantic metal reserves |

### Allomancy/ - 16 Metal Powers
| Script | Status | What It Does |
|--------|--------|--------------|
| `SteelPush.cs` | ✅ Done | Push metals away |
| `IronPull.cs` | ✅ Done | Pull metals toward |
| `PewterBurn.cs` | ✅ Done | Enhanced strength, speed, healing |
| `TinEnhance.cs` | ✅ Done | Enhanced senses, sight |
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
| `MetalType.cs` | ✅ Done | Enum for all 20 metals |

### Player/ - Movement Abilities
| Script | Status | What It Does |
|--------|--------|--------------|
| `DashAbility.cs` | ✅ Done | Pewter dash with stamina |
| `WallRun.cs` | ✅ Done | Wall running |
| `DodgeRoll.cs` | ✅ Done | Invincible dodge roll |

### UI/ - User Interface
| Script | Status | What It Does |
|--------|--------|--------------|
| `MainMenu.cs` | ✅ Done | Main menu with play/settings/quit |
| `GameManager.cs` | ✅ Done | Pause, game over, scene transitions |
| `SettingsManager.cs` | ✅ Done | Volume, graphics settings |
| `HUDController.cs` | ✅ Done | Health/stamina bar updates |

### World/ - Environment Objects
| Script | Status | What It Does |
|--------|--------|--------------|
| `Door.cs` | ✅ Done | Lockable doors with keys |
| `MetalPickup.cs` | ✅ Done | Pickup metal for reserves |
| `Checkpoint.cs` | ✅ Done | Save progress |
| `MovingPlatform.cs` | ✅ Done | Moving platforms |
| `TriggerZone.cs` | ✅ Done | Trigger events |
| `LootDrop.cs` | ✅ Done | Drop loot on death |

### Effects/ - Visual Effects
| Script | Status | What It Does |
|--------|--------|--------------|
| `ParticleEffect.cs` | ✅ Done | VFX for abilities |
| `SoundManager.cs` | ✅ Done | Audio management |

---

## Total Scripts: 38+

---

## Documentation

| Doc | Purpose |
|-----|---------|
| `allomancy-physics-analysis.md` | Steel/Iron force calculations |
| `allomancy-coin-physics.md` | Coin flight physics |
| `allomancy-desmos-functions.md` | Math for graphs |

---

## What's Left

**Gameplay:**
- Tune Allomancy physics
- Balance damage numbers
- Create enemy prefabs
- Add particle effects
- Add audio clips

**UI:**
- Main Menu polish
- Pause menu
- Skill tree (ThenBuzzard100)
- Inventory UI

**Art/Assets:**
- Real character model
- Environment art
- Weapon models
- More VFX
