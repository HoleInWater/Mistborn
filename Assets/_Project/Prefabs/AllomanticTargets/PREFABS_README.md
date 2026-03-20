# Allomantic Target Prefabs

Every metal object prefab needs these components:

1. **Rigidbody** — with appropriate mass for the metal type
2. **AllomanticTarget** — set metalType and isAnchored
3. **Collider** — for physics and overlap detection
4. **Material** — from Assets/_Project/Materials/

## Naming Convention

```
Metal_Coin_01.prefab
Metal_Coin_02.prefab
Metal_Bracket_Wall_01.prefab
Metal_Bracket_Wall_02.prefab
Metal_Railing_Floor_01.prefab
Metal_Door_01.prefab
Metal_Barrel_01.prefab
```

## Prefab Examples

### Metal Coin (Light, Pushable)
- Primitive: Sphere, radius 0.05
- Mass: 0.1
- isAnchored: false
- Use: Steelpush launches this at enemies

### Metal Bracket (Heavy, Anchored)
- Primitive: Cube, 0.3m
- Mass: 50
- isAnchored: true
- Use: Push off this to fly upward

### Metal Railing (Floor)
- Primitive: Cube, 2m x 0.1m x 0.1m
- Mass: 100
- isAnchored: true
- Use: Push down to launch upward
