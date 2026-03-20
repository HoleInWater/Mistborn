# Placeholder Models

Since we don't have real models yet, we're using Unity primitives:

## Player Character
- Use Unity Capsule primitive
- Height: 2 meters
- Placeholder gray material
- Will be replaced with rigged character model later

## Metal Objects

### Coins (Pushable)
- Use Unity Sphere primitive
- Radius: 0.05m
- Material: Silver metallic
- Add Rigidbody with mass ~0.1
- Add AllomanticTarget with isAnchored = false

### Wall Brackets (Anchored)
- Use Unity Cube primitive
- Size: 0.3m x 0.3m x 0.3m
- Material: Dark metal
- Add Rigidbody with mass ~50
- Add AllomanticTarget with isAnchored = true
- Parent to wall geometry

### Railings (Floor Anchors)
- Use Unity Cube primitive (thin, wide)
- Size: 2m x 0.1m x 0.1m
- Material: Iron/steel look
- Add Rigidbody with mass ~100
- Add AllomanticTarget with isAnchored = true

## Environment
- Buildings: Large cube primitives at various scales
- Ground: Standard Unity plane
- Keep it simple — we just need something to test Allomancy mechanics
