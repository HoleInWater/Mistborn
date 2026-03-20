# Scene Setup Guide

## TestArena.unity

The main testing scene for Allomancy mechanics.

### Required Objects

1. **Player**
   - Position: (0, 1.8, 0)
   - Components:
     - CharacterController
     - Rigidbody (use gravity)
     - AllomancerController
     - SteelPushAbility
     - IronPullAbility
     - AllomanticSight
     - PlayerController
     - SteelpushAssistedJump

2. **PlayerCamera**
   - Child of Player or separate
   - Position offset: (0, 2, -5)
   - Components: PlayerCamera

3. **MetalHUD**
   - Canvas in scene
   - Components: MetalReserveUI
   - Connect to AllomancerController

### Metal Objects for Testing

#### Anchored (Push Off To Fly)
- **Floor Plate**: 2x2m metal plate on ground, mass 100
- **Wall Brackets**: Metal pieces on walls, mass 50+
- **Ceiling Anchors**: Metal pieces above, mass 75+

#### Pushable (Coins)
- Small spheres (0.1m radius)
- Mass: 0.1
- isAnchored: false
- Put 10-20 around the arena

#### Environmental Metal
- Walls should be detectable (can see blue lines through them)
- Metal doors, pipes, grates

### Lighting

- Directional Light: "Ash Sky" — grey/blue tint
- Ambient: Low, dark feel
- Consider point lights for "moody" feel

### Skybox

Use a dark, ashy grey skybox to evoke the ash-covered world.

## Controls to Test

| Action | Key |
|--------|-----|
| Move | WASD |
| Look | Mouse |
| Jump | Space |
| Steel Push | Right Click |
| Iron Pull | Left Click |
| Allomantic Sight | Tab |
| Steelpush Jump | Space while burning Steel + anchor below |
