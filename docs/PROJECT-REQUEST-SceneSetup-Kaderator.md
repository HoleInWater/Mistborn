# PROJECT REQUEST: Unity Scene Setup
## For Kaderator

*Hey, need your help setting up the actual Unity scene. Here's what I've got ready and what needs to happen.*

---

## What's Ready to Go

I've written all the scripts. They're waiting to be hooked up in Unity.

**Scripts to add to scene:**
- AllomancerController
- SteelPushAbility
- IronPullAbility
- AllomanticSight
- PlayerController
- PlayerCamera
- SteelpushAssistedJump
- MetalReserveUI

**Scripts that auto-generate:**
- TestArenaSetup.cs — run from Unity menu "Mistborn > Setup Test Arena"

---

## What You Need to Do

### 1. Create the TestArena Scene

File: `Assets/_Project/Scenes/TestArena.unity`

In Unity:
1. File > New Scene
2. Save as TestArena.unity in that folder
3. Don't delete it, just save

### 2. Set Up the Player

Create an empty GameObject called "Player" and add:

**Components:**
- CharacterController
- Rigidbody (use gravity)
- All the scripts from above

**Hierarchy should look like:**
```
Player
├── (scripts go here)
└── PlayerCamera (child object with camera)
```

**Position:** (0, 1.8, 0)

### 3. Set Up Metal Objects

Using TestArenaSetup (Mistborn > Setup Test Arena):
1. Click "Generate Basic Arena"
2. Click "Add Metal Coins"  
3. Click "Add Anchored Metal"

Or do it manually:

**Coins (pushable):**
- Sphere, radius 0.1
- Rigidbody, mass 0.1
- AllomanticTarget: isAnchored = false, metalMass = 0.1
- Put 10-20 around the arena

**Floor Plate (for flying):**
- Cube, 2x0.1x2
- Rigidbody, mass 100
- AllomanticTarget: isAnchored = true, metalMass = 100
- Position: ground level, center

**Wall Brackets (anchored):**
- Cube, 0.5x0.5x0.5
- Rigidbody, mass 50
- AllomanticTarget: isAnchored = true, metalMass = 50
- Place 4 around walls at different heights

### 4. Set Up Camera

Create Main Camera:
- Child of Player
- Position: (0, 2, -5)
- Add PlayerCamera script
- Set Target = Player

### 5. Set Up HUD

Create Canvas:
- Add MetalReserveUI script
- Create two sliders (Steel, Iron bars)
- Connect to AllomancerController on Player

### 6. Lighting

- Directional Light with grey/blue tint (ash sky feel)
- Low ambient light
- Or use a dark skybox

---

## Quick Checklist

- [ ] Created TestArena.unity
- [ ] Set up Player with all scripts
- [ ] Added coins (10-20)
- [ ] Added floor plate (anchored)
- [ ] Added wall brackets (4, different heights)
- [ ] Camera follows player
- [ ] HUD shows metal reserves
- [ ] Hit Play and test push/pull

---

## Questions?

If something's unclear or you hit issues, let me know and I'll write more specific instructions.

Once it's working, push the scene file and we can start building from there.
