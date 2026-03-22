# Phase 0 - Foundation Stabilization Instructions

## Scene & Project Cleanup

### 1. Open Scene 1.unity and confirm all scripts are attached
- Open the scene in Unity Editor
- Select the Player GameObject (tagged "Player")
- In the Inspector, verify the following components are present:
  - **BasicPlayerMove** (or PlayerMove) - PlayerController
  - **PlayerCamera** - Third-person camera controller (added by PlayerSetup script at runtime)
  - **AllomanticSight** - Metal sight ability (added by PlayerSetup script at runtime)
  - **PlayerStamina** - Stamina system
  - **Sprint** - Sprint ability
  - **HealthBarTransitions** - Health UI
  - **Animator** - Animation controller
  - **Rigidbody** - Physics
  - **CapsuleCollider** - Collision
- If any are missing, they will be added automatically by the `PlayerSetup` script when the game starts.

### 2. Verify AllomanticTarget is on every metal object in scene
- Currently there are no metal objects in the scene. We will create them in the next steps.

### 3. Confirm PlayerController, PlayerCamera, and AllomanticSight all exist on the Player prefab
- PlayerController: `BasicPlayerMove` script exists.
- PlayerCamera: Created via `PlayerSetup` script at runtime.
- AllomanticSight: Created via `PlayerSetup` script at runtime.
- **Note:** The `PlayerSetup` script automatically adds missing components when the scene loads.

### 4. Add Rigidbody + Colliders to all metal objects — verify physics layers are set correctly
- We will create metal prefabs with Rigidbody and appropriate colliders (see below).

### 5. Set up Physics Layer matrix in Project Settings → Physics
1. Open **Edit → Project Settings → Physics**
2. In the **Layer Collision Matrix**, ensure the following layers exist (add them if missing):
   - **Player** (layer 6)
   - **Metal** (layer 7)
   - **Enemy** (layer 8)
   - **World** (layer 9)
3. Configure collisions:
   - **Player** collides with **Metal**, **Enemy**, **World**
   - **Metal** collides with **Player**, **Metal**, **World** (but not with Enemy)
   - **Enemy** collides with **Player**, **World**
   - **World** collides with everything

### 6. Test that Play Mode launches without console errors
- Press Play and check the Console window for errors.
- If errors appear, fix them before proceeding.

---

## Metal Object Prefabs

### 1. Create Coin prefab
1. Create a new Sphere GameObject (`GameObject → 3D Object → Sphere`)
2. Rename it to `Coin`
3. Scale: (0.02, 0.02, 0.02)
4. Add components:
   - **Rigidbody**: Mass = 0.003 kg, Drag = 0, Angular Drag = 0.05
   - **Sphere Collider** (already present)
   - **AllomanticTarget** (script created):
     - Metal Type: Steel
     - Is Anchored: false
     - Mass: 0.003
5. Set the tag to `Metal` (create tag if needed)
6. Set the layer to `Metal` (layer 7)
7. Create a prefab by dragging from Hierarchy to `Assets/_Project/Prefabs/` (create folder if missing).

### 2. Create MetalBracket prefab
1. Create a new Cube GameObject
2. Rename it to `MetalBracket`
3. Scale: (0.1, 0.1, 0.1)
4. Add components:
   - **Rigidbody**: Is Kinematic = true (so it doesn't move), Mass = 5
   - **Box Collider** (already present)
   - **AllomanticTarget**:
     - Metal Type: Steel
     - Is Anchored: true (because it's fixed)
     - Mass: 5
5. Set tag to `Metal`, layer to `Metal`
6. Create prefab.

### 3. Create MetalRailing prefab
1. Create a new Cube GameObject
2. Rename it to `MetalRailing`
3. Scale: (2, 0.05, 0.05)  (long horizontal bar)
4. Add components:
   - **Rigidbody**: Is Kinematic = false, Mass = 10
   - **Box Collider** (adjust size if needed)
   - **AllomanticTarget**:
     - Metal Type: Steel
     - Is Anchored: false
     - Mass: 10
5. Set tag to `Metal`, layer to `Metal`
6. Create prefab.

### 4. Place 15-20 of each prefab type in Scene 1.unity
- Drag the prefabs into the scene, spread them around the test area.
- Ensure they are on the `Metal` layer and have the `Metal` tag.

### 5. Verify Coin can be pushed/pulled freely; verify Bracket pulls the PLAYER not the bracket
- Enter Play Mode.
- Press Tab to activate Allomantic Sight – blue lines should appear connecting to all metal objects.
- Use Left Mouse (Iron Pull) and Right Mouse (Steel Push) on coins and railings.
- For anchored brackets, the player should move toward the bracket (not the bracket moving).

---

## HUD Hookup

### 1. Wire MetalHUD to AllomanticController — each metal's reserve depletes as it burns
- The `MetalHUD` script is in `Assets/_Project/Scripts/GUI/MetalHUD.cs`.
- The `Allomancer` script (AllomanticController) is in `Assets/_Project/Scripts/Allomancy/Allomancer.cs`.
- **Manual steps:**
  1. Create a UI Canvas (if not exists) and add a `MetalHUD` component.
  2. Assign the UI elements (text, slider, icons) in the Inspector.
  3. In the Player's `Allomancer` component, reference the `MetalHUD` instance.
  4. Add code to `Allomancer` to call `MetalHUD.UpdateReserve()` when metal drains.

### 2. Confirm HUD bars visually drain when Steel/Iron is used in Play Mode
- After wiring, test using Steel Push/Iron Pull and watch the metal reserve slider decrease.

### 3. Add 'Out of metal' state — disable push/pull when reserve hits 0
- In `Allomancer.DrainMetal()`, when reserve reaches 0, set a flag `canBurnMetal = false`.
- In `SteelPush` and `IronPull` abilities, check this flag before allowing push/pull.

---

## Next Steps
After completing the above, proceed to **Phase 1 — Steel & Iron Core Loop**.