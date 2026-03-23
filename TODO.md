# 🌑 MISTBORN ERA ONE — MASTER DEVELOPMENT TODO

> **Total Tasks:** 400+ | **Estimated Duration:** 1,198 days | **Current Phase:** 0 - Foundation Stabilization

## 📊 PROGRESS SUMMARY
| Phase | Status | Tasks | Completed | Remaining |
|-------|--------|-------|-----------|-----------|
| 0 - Foundation Stabilization | ⏳ IN PROGRESS | 125+ | 12 | 113+ |
| 1 - Steel & Iron Core Loop | 📋 PENDING | 40+ | 0 | 40+ |
| 2 - Player Feel & Movement | 📋 PENDING | 25+ | 0 | 25+ |
| 3 - Combat & Enemies | 📋 PENDING | 35+ | 0 | 35+ |
| 4 - All 16 Metals | 📋 PENDING | 30+ | 0 | 30+ |
| 5 - World & Stealth | 📋 PENDING | 20+ | 0 | 20+ |
| 6 - Story & Quest Systems | 📋 PENDING | 15+ | 0 | 15+ |
| 7 - UI, Audio & Polish | 📋 PENDING | 25+ | 0 | 25+ |
| 8 - Save, Achievements & QA | 📋 PENDING | 20+ | 0 | 20+ |

**✅ Completed (43/400+):**
1. PlayerSetup.cs - Runtime script to add missing components
2. AllomanticTarget.cs - Metal object component
3. Allomancer.cs updated - Wired MetalHUD references
4. PlayerCamera.cs script - Created but not fully integrated
5. Out-of-metal state - Implemented disable push/pull when reserve hits 0
6. Out-of-metal UI feedback - Added warning display when metal reserves hit 0
7. Repository clean-up - Removed duplicate utility scripts, reorganized folders
8. Added comprehensive comments to all new code
9. Weight-proportional force for Steel Push - F = playerMass / referenceMass × baseForce
10. Weight-proportional force for Iron Pull - Same physics as Steel Push
11. Anchor detection for Steel Push - pushes player when target is heavy/kinematic
12. Anchor detection for Iron Pull - pulls player when target is heavy/kinematic
13. Physics documentation updated with canonical evidence from Coppermind wiki
14. Cleaned up Debug.Log statements with #if UNITY_EDITOR for production builds
15. Integrated SteelPush with Allomancer metal reserves (uses DrainMetal)
16. Fixed flaring to drain metal 3x faster
17. Improved force calculation: simple inverse distance with minDistance limit
18. Integrated IronPull with Allomancer metal reserves (uses DrainMetal)
19. Improved anchor detection using AllomanticTarget.isAnchored flag
20. Changed detection to OverlapSphere for metal-through-walls (lore accurate)
21. Added tooltips to force settings for calibration clarity
22. Added optional visual effect when pushing metal
23. Added flight mechanics: extra upward boost when pushing off anchored objects below
24. Added impulse mode for light objects (coins) to achieve realistic velocities
25. Added camera shake and placeholder audio system for push feedback
26. Added flaring visual vignette effect (screen tint when flaring)
27. Added debug logging for impulse calibration
28. Added crosshair color change when metal is in range
29. Added push cooldown after releasing button to prevent spam
30. Added physics with gravity and drag for metal objects (realistic projectile motion)
31. Added static metal registry and tagging system for efficient metal detection
32. Researched blue lines (Spiritual Realm) from Reddit and Coppermind, updated docs
33. Added aluminum filtering: aluminum alloys cannot be pushed/pulled (lore accurate)
34. Added real-time debug display for calibration in editor
35. Fixed weight-proportional force to use referenceMass instead of targetMass (lore accurate)
36. Added steel bubble defensive ability (pushes all nearby metal away)
37. Improved blue lines with pulsing/shimmering effect and chest-origin as per lore
38. Implemented object pooling for blue lines to improve performance
39. Added targeted metal detection with enhanced debug display showing specific target info
40. Added visual differentiation in blue lines for non-pushable and anchored metals
41. Added push prediction line showing trajectory when targeting metal objects
42. Added push force visual feedback with color-coded screen tint and prediction line velocity coloring
43. Fixed allomancy physics to match book lore: inverse distance (1/r) with zenith cap (max 2x), fixed syntax errors, improved debug display

---

## 🔧 PHASE 0: FOUNDATION STABILIZATION
*Get what exists actually working together*

### ✅ Already Completed
- [x] Create PlayerSetup.cs (runtime script to add missing components)
- [x] Create AllomanticTarget.cs script for metal objects
- [x] Update Allomancer.cs to wire MetalHUD references
- [x] Create PlayerCamera.cs script (not yet integrated)
- [x] Implement out-of-metal state (disable push/pull when reserve hits 0)

### 🔧 Scene & Project Cleanup
- [ ] Open Scene 1.unity in Unity Editor
- [ ] Locate Player GameObject (tagged 'Player')
- [ ] Select Player GameObject in Hierarchy
- [ ] Inspect Components in Inspector window
- [ ] Verify BasicPlayerMove script component exists
- [ ] Verify PlayerCamera script component exists (or will be added by PlayerSetup)
- [ ] Verify AllomanticSight script component exists (or will be added by PlayerSetup)
- [ ] Verify PlayerStamina script component exists
- [ ] Verify Sprint script component exists
- [ ] Verify HealthBarTransitions script component exists
- [ ] Verify Animator component exists
- [ ] Verify Rigidbody component exists
- [ ] Verify CapsuleCollider component exists
- [ ] Verify PlayerSetup script component exists (or add it)
- [ ] Verify PlayerController (BasicPlayerMove) has all required fields
- [ ] Verify PlayerStamina script is attached and configured
- [ ] Verify Sprint script is attached and configured
- [ ] Verify HealthBarTransitions script is attached
- [ ] Verify Animator component has valid controller
- [ ] Verify Rigidbody component has correct settings
- [ ] Verify CapsuleCollider component has correct size/shape

### 🔧 Physics Layer Matrix Setup
- [ ] Open Edit → Project Settings → Tags and Layers
- [ ] In 'Layers' section, find empty User Layer slot
- [ ] Type 'Player' in User Layer 6
- [ ] Type 'Metal' in User Layer 7
- [ ] Type 'Enemy' in User Layer 8
- [ ] Type 'World' in User Layer 9
- [ ] Click 'Apply' to save layer names
- [ ] Open Edit → Project Settings → Physics
- [ ] Scroll to 'Layer Collision Matrix'
- [ ] Configure collision matrix:
  - Player collides with: Metal, Enemy, World
  - Metal collides with: Player, Metal, World (NOT Enemy)
  - Enemy collides with: Player, World
  - World collides with: Everything

### 🔧 Metal Object Prefabs
#### Coin Prefab
- [ ] Create empty GameObject named 'Coin'
- [ ] Add Mesh Filter (Sphere)
- [ ] Add Mesh Renderer with material (gold/yellow, metallic)
- [ ] Scale to (0.02, 0.02, 0.02)
- [ ] Add Sphere Collider
- [ ] Add Rigidbody (mass: 0.003, drag: 0, angular drag: 0.05)
- [ ] Add AllomanticTarget (metalType: Steel, isAnchored: false)
- [ ] Set Tag: 'Metal' (create if missing)
- [ ] Set Layer: 'Metal' (7)
- [ ] Save as prefab in Assets/_Project/Prefabs/

#### MetalBracket Prefab (Anchored)
- [ ] Create empty GameObject named 'MetalBracket'
- [ ] Add Mesh Filter (Cube)
- [ ] Add Mesh Renderer with material (dark gray, metallic)
- [ ] Scale to (0.1, 0.1, 0.1)
- [ ] Add Box Collider
- [ ] Add Rigidbody (mass: 5, isKinematic: true)
- [ ] Add AllomanticTarget (metalType: Steel, isAnchored: true)
- [ ] Set Tag: 'Metal'
- [ ] Set Layer: 'Metal'
- [ ] Save as prefab

#### MetalRailing Prefab
- [ ] Create empty GameObject named 'MetalRailing'
- [ ] Add Mesh Filter (Cube)
- [ ] Add Mesh Renderer with material (silver, metallic)
- [ ] Scale to (2, 0.05, 0.05)
- [ ] Add Box Collider (size: 2, 0.05, 0.05)
- [ ] Add Rigidbody (mass: 10)
- [ ] Add AllomanticTarget (metalType: Steel, isAnchored: false)
- [ ] Set Tag: 'Metal'
- [ ] Set Layer: 'Metal'
- [ ] Save as prefab

### 🔧 Scene Population
- [ ] Place 15-20 Coin instances in scene
- [ ] Place 15-20 MetalBracket instances (attached to walls/floors)
- [ ] Place 15-20 MetalRailing instances
- [ ] Verify all instances have correct layer and tag

### 🔧 HUD Hookup
- [ ] Create UI Canvas ('GameCanvas')
- [ ] Create HUD Panel with:
  - CurrentMetalText (Text - TextMeshPro)
  - MetalIcon (Image)
  - MetalReserveSlider (Slider)
- [ ] Add MetalHUD script to Canvas
- [ ] Assign UI references in MetalHUD inspector
- [ ] Add Allomancer script to Player GameObject
- [ ] Assign MetalHUD reference to Allomancer.metalHUD field

### 🔧 Out of Metal State
- [x] Add `public bool canBurnMetal = true;` to Allomancer.cs
- [x] Modify DrainMetal to set canBurnMetal false when reserve ≤ 0
- [x] Add check in SteelPush/IronPull to disable when canBurnMetal false
- [x] Add UI feedback for out-of-metal state

### 🔧 Testing
- [ ] Save Scene 1.unity
- [ ] Enter Play Mode
- [ ] Verify no console errors
- [ ] Test WASD movement
- [ ] Test Space bar jump
- [ ] Test Left Shift sprint
- [ ] Test Tab for Allomantic Sight (blue lines should appear)
- [ ] Test Steel Push/Right Mouse on metal objects
- [ ] Test Iron Pull/Left Mouse on metal objects
- [ ] Verify anchored brackets pull player, not themselves
- [ ] Exit Play Mode

---

## 📋 PHASE 1: STEEL & IRON CORE LOOP
*Make Push/Pull feel like the books*

### Physics Polish
- [x] Implement weight-proportional force: `F = playerMass / targetMass × basePushForce`
- [x] Add distance falloff: `F = F_base × (1 / distance)`, capped at 100m
- [x] Implement anchor detection: if target mass > playerMass × 3 → push player
- [x] Add Zenith Point: maximum force at 5m, falls off beyond
- [x] Calibrate coin velocity: 10m push → 80 km/h (22.22 m/s)
- [x] Implement flight mechanics: pushing off floor brackets launches player upward

### Allomantic Sight Polish
- [ ] Line thickness reflects mass (thicker = heavier)
- [ ] Set max detection range: 80-100m
- [ ] Add pulse/shimmer animation on line width
- [x] Add slow-motion effect on Tab press (0.5x time for 0.3s)

### Metal Flaring
- [x] Add flaring input: Shift + Push/Pull = burn faster
- [x] Implement flaring: force × 2, reserve drain × 3
- [x] Add screen vignette/pulse VFX when flaring

### Metal Wheel UI
- [ ] Create radial menu with 16 metal slots
- [ ] Add scroll wheel/right thumbstick input to open
- [ ] Slow time to 0.3× when wheel is open
- [ ] Show lore tooltips on hover
- [ ] Controller support: thumbstick navigation, A/X confirms

### Balancing
- [ ] Create BalancingTestScene with distance markers
- [ ] Test and tune push/pull forces
- [ ] Document constants in AllomancyConstants.cs

---

## 📋 PHASE 2: PLAYER FEEL & MOVEMENT
*Make traversal satisfying*

- [ ] Implement camera collision
- [ ] Add camera lag (cinematic feel)
- [ ] Add coyote time (0.15s after edge)
- [ ] Add jump buffering (0.2s before landing)
- [ ] Set sprint max: ~8 m/s
- [ ] Add air control while flying
- [ ] Create Pewter metal ability (speed ×1.8, jump ×2, damage ×0.5)
- [ ] Create Tin metal ability (enhanced senses)
- [ ] Implement lock-on targeting system

---

## 📋 PHASE 3: COMBAT & ENEMIES
*Give players something to fight*

- [ ] Create Guard AI enemy (patrol, detection, combat)
- [ ] Create Coinshot enemy (ranged allomancer)
- [ ] Create Koloss enemy (heavy brute)
- [ ] Create Steel Inquisitor boss (3 phases)
- [ ] Implement combat system (combos, parry, hit stop)
- [ ] Create Seeker enemy (detects allomancy)

---

## 📋 PHASE 4: ALL 16 METALS
*Complete the Allomantic system*

- [ ] Implement mental metals: Zinc, Brass, Copper, Bronze
- [ ] Implement enhancement metals: Aluminum, Duralumin
- [ ] Implement temporal metals: Bendalloy, Cadmium (time bubbles)
- [ ] Implement god metals: Atium, Electrum, Gold, Malatium
- [ ] Integrate all metals with Metal Wheel UI

---

## 📋 PHASE 5: WORLD & STEALTH
*Build Luthadel — verticality is everything*

- [ ] Create city block environment with vertical traversal
- [ ] Implement stealth system
- [ ] Create Kandra enemy type

---

## 📋 PHASE 6: STORY & QUEST SYSTEMS
*Give the world meaning*

- [ ] Implement dialogue system
- [ ] Create quest system
- [ ] Implement faction system

---

## 📋 PHASE 7: UI, AUDIO & POLISH
*The game is complete — now make it feel complete*

- [ ] Create full HUD (health, metal reserves, minimap)
- [ ] Create menus (main, pause, options)
- [ ] Implement audio system
- [ ] Add VFX for all abilities
- [ ] Create skill tree UI

---

## 📋 PHASE 8: SAVE, ACHIEVEMENTS & QA
*Ship it right*

- [ ] Implement save/load system
- [ ] Create tutorial system
- [ ] Add achievements
- [ ] Comprehensive QA and bug fixing

---

## 📚 REFERENCE MATERIALS

- **Phase 0 Instructions:** `Phase0_Instructions.md` - Detailed manual steps for Foundation phase
- **Lore Reference:** Coppermind Wiki, 17th Shard, Arcanum (WoB)

## 🔗 TEAM RESOURCES
- GitHub Repository: https://github.com/HoleInWater/Mistborn
- Unity Version: 2022/6000 LTS
- Main Scene: `Assets/_Project/Scenes/Scene 1.unity`
- Player Prefab: Located in scene (tagged 'Player')
- Scripts Folder: `Assets/_Project/Scripts/`

---

*This TODO list is automatically updated as tasks are completed. Last updated: 2026-03-22 17:22:28*