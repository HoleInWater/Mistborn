# TODO — Mistborn Era One Unity Game

## ✅ COMPLETED

### Core Systems
- [x] Project structure setup
- [x] GitHub repository created and public
- [x] AllomancerController - manages metal reserves
- [x] MetalReserve - tracks single metal state
- [x] AllomanticMetal enum - all 16 metals
- [x] AllomanticTarget - marks pushable objects

### Allomancy Abilities
- [x] SteelPushAbility - push metals (Right Click)
- [x] IronPullAbility - pull metals (Left Click)
- [x] PewterEnhancement - strength/speed (Q)
- [x] TinEnhancement - enhanced senses (E)
- [x] CopperCloud - hide Allomancy (C)
- [x] BronzeDetection - detect others burning
- [x] BrassSoothing - calm emotions (Z)
- [x] ZincRioting - provoke emotions (X)
- [x] AluminumProtection - burn all metals (F)
- [x] DuraluminBurst - super burst (R)
- [x] BendalloyBubble - time dilation (G)
- [x] BlueLineRenderer - visual lines to metals
- [x] AllomanticSight - toggle metal sight (Tab)
- [x] SteelpushAssistedJump - super jump
- [x] CoinPouch - throw coins

### Combat
- [x] IDamageable interface
- [x] PlayerHealth - player health system
- [x] PlayerCombat - melee attacks
- [x] EnemyBase - enemy AI base class
- [x] EnemyGuard - melee patrol enemy
- [x] EnemyCoinshot - enemy Allomancer
- [x] EnemySeeker - detects player burning metals

### Player
- [x] PlayerController - movement
- [x] CameraController - third-person camera
- [x] AllomancerAnimationController - animation states
- [x] SteelpushAssistedJump - super jump

### UI
- [x] MetalHUD - metal reserve display
- [x] MetalReserveUI - slider-based display
- [x] PauseMenu - pause functionality
- [x] CombatUI - damage numbers, enemy health bars

### Utilities
- [x] GameManager - game state, checkpoints
- [x] SoundManager - audio system
- [x] CheckpointSystem - respawn points
- [x] SaveLoadSystem - save/load game state
- [x] TestArenaSetup - editor tool for testing
- [x] AllomancyConstants - game constants
- [x] AllomancyPhysicsFormulas - physics calculations

### Documentation
- [x] README.md - project overview
- [x] CONTRIBUTING.md - how to contribute
- [x] onboarding.md - setup guide
- [x] architecture.md - system design
- [x] combat-system.md - combat design
- [x] allomancy-design.md - abilities design
- [x] hud-design.md - UI design
- [x] environment-design.md - world design
- [x] animation-spec.md - animation specs
- [x] lore-notes.md - Mistborn lore
- [x] story-outline.md - game story
- [x] character-vin.md - Vin character
- [x] tutorial-design.md - tutorial
- [x] progression-system.md - leveling
- [x] metal-economy.md - metal pickups
- [x] audio-needs.md - sound requirements
- [x] team-playbook.md - workflow
- [x] git-guide.md - Git commands
- [x] github-guide.md - GitHub Desktop
- [x] unity-ui-guide.md - How to set up UI in Unity
- [x] enemy-koloss.md - Koloss enemy
- [x] enemy-steel-inquisitor.md - Inquisitor enemy
- [x] enemy-noble-guard.md - Noble guard
- [x] enemy-kandra.md - Kandra enemy
- [x] level-the-square.md - level design
- [x] level-canal-district.md - level design
- [x] PROJECT-REQUEST-SceneSetup-Kaderator.md
- [x] CAN-DO.md - capabilities doc
- [x] TEAM-TASKS.md - team task list

---

## 🔴 NEEDS UNITY EDITOR

These cannot be done by AI - need Unity installed:

### Scene Setup
- [ ] Create TestArena scene (.unity file)
- [ ] Place player character
- [ ] Add test metal objects
- [ ] Set up lighting

### Prefabs
- [ ] Create Player prefab with all scripts
- [ ] Create EnemyGuard prefab
- [ ] Create EnemyCoinshot prefab
- [ ] Create metal prop prefabs (coins, brackets)

### Materials & Visuals
- [ ] Ash/soot skybox
- [ ] Metal materials
- [ ] Character placeholder (capsule works for now)

---

## 🔴 NEEDS GAME TESTING

### Physics
- [ ] Test push force feels right
- [ ] Test pull force feels right
- [ ] Test flying off anchored metal
- [ ] Tune metal reserve drain rates

### Gameplay
- [ ] Test blue lines visibility
- [ ] Test camera following player
- [ ] Balance pewter enhancement strength
- [ ] Balance enemy damage/health

---

## 🟡 COULD BE IMPROVED

### Visuals
- [ ] Add pewter glow VFX
- [ ] Add metal burn effect
- [ ] Upgrade LineRenderer to Volumetric Lines asset
- [ ] Add particle effects for abilities

### Audio
- [ ] Add actual sound files
- [ ] Set up audio mixer
- [ ] Add 3D spatial audio

### Polish
- [ ] Add screen shake on hit
- [ ] Add damage numbers
- [ ] Add enemy health bars
- [ ] Add death/respawn screen

---

## 🟢 NICE TO HAVE (Sprint 2+)

### Enemies
- [x] Koloss - big melee enemy
- [x] Steel Inquisitor - boss enemy
- [x] Kandra - shapeshifter enemy
- [x] Noble Guard - armored enemy
- [x] Mist Spirit - phasing ghost enemy
- [x] Pewter Armsmaster - combat arts
- [x] Lurcher - Allomancer enemy
- [x] Thug - tactical fighter

### Levels
- [ ] The Square level
- [ ] Canal District level
- [ ] Keep interior level

### Features
- [x] Save/Load system
- [x] Metal pickup pickups
- [x] Skill tree system
- [x] Dialogue system
- [x] Objective/tracking system
- [x] Particle effect manager
- [x] Enemy spawner
- [x] Audio manager
- [x] World interactions (doors, levers, chests)
- [x] Atmosphere/weather system
- [x] Tutorial system
- [x] Achievement system
- [x] Inventory system
- [x] Dodge roll
- [x] Screen effects
- [x] Loading screen manager
- [x] Settings manager
- [x] Camera effects (shake, recoil)
- [x] Game state machine
- [x] Quick use hotbar
- [x] Compass UI
- [x] VFX library
- [x] Pewter Mend healing ability
- [x] Tin enhanced senses
- [x] Electrum/Gold temporal abilities
- [x] Smoke cloud effect
- [x] Metal immunity/status effects
- [x] Trail effects
- [x] Slow motion effect
- [x] Atium/Malatium temporal abilities
- [x] Lock-on targeting system
- [x] Combat combo system with rage
- [x] Ally companion AI
- [x] Combat log with world text
- [x] Hitbox debugger

### Documentation
- [x] Era 1 comprehensive lore guide
- [x] Game design quick reference

---

## 📁 Folder Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Allomancy/       ✅ All abilities (16 metals)
│   │   ├── Combat/          ✅ Player combat, dodge
│   │   ├── Enemy/          ✅ Enemy AI (12 types)
│   │   ├── Allies/          ✅ Companion AI
│   │   ├── Player/          ✅ Movement/Camera/Skills
│   │   ├── UI/              ✅ HUD/Menus/Dialogue/Tutorial
│   │   ├── Utilities/       ✅ Managers
│   │   ├── World/          ✅ Checkpoints/Spawner/Interactions
│   │   ├── Effects/        ✅ Particle systems/Screen effects
│   │   ├── Audio/          ✅ Sound management
│   │   └── Managers/       ✅ Game state, transitions
│   ├── Scenes/
│   │   └── (needs Unity)  🔴
│   ├── Prefabs/
│   │   └── (needs Unity)   🔴
│   └── Materials/
│       └── (needs Unity)   🔴
├── docs/                   ✅ All docs
└── README.md               ✅
```
