# Allomancy Design Document

## What is Allomancy?

Allomancy is the magic system from Brandon Sanderson's Mistborn series. Here's the gist:

1. You swallow metal (flakes, dust, filings)
2. You "burn" it — the metal starts giving you power
3. You get an effect based on which metal you're burning
4. When the metal runs out, the power stops

The power comes from Preservation, one of the two Shards that created Scadrial. Allomancy is end-positive — you gain more power than you put in.

## The 16 Metals

| Metal | Push/Pull | Effect | Misting Name | Status |
|-------|-----------|--------|--------------|--------|
| **Steel** | Push | Propels metal objects away | Coinshot | ACTIVE |
| **Iron** | Pull | Attracts metal objects toward | Lurcher | ACTIVE |
| Pewter | — | Enhanced strength/durability | Thug | Future |
| Tin | — | Enhanced senses | Tineye | Future |
| Brass | Soothe | Calms emotions | Soother | Future |
| Zinc | Riot | Provokes emotions | Rioter | Future |
| Copper | — | Masks Allomancy detection | Smoker | Future |
| Bronze | — | Detects burning Allomancers | Seeker | Future |
| Gold | — | See possible pasts | Augur | Future |
| Electrum | — | See possible futures | Oracle | Future |
| Atium | — | See others' futures | — | Future |
| Malatium | — | See true nature | — | Future |
| Aluminum | — | Drains own metals | Aluminum Gnat | Future |
| Duralumin | — | Drains others' metals | Duralumin Gnat | Future |
| Chromium | — | Stores metals externally | Leecher | Future |
| Nicrosil | — | Stores others' metals | Nicroburst | Future |
| Bendalloy | — | Time dilation bubble | Slider | Future |
| Cadmium | — | Time compression bubble | Pulser | Future |

## Steel & Iron: The Physical Pair

These are our focus for Sprint 1. They work on external metal objects and follow Newton's Third Law (plus some mistborn-specific rules).

### The Core Rules (from the books)

**1. Push strength is proportional to your physical weight**
- Bigger Allomancer = stronger push
- A 200lb Mistborn pushes harder than a 120lb one

**2. Push strength is inversely proportional to distance**
- Closer = stronger push
- There's a "zenith point" where force maxes out

**3. Whatever you push, pushes back (Newton's 3rd Law)**
- Light object (coin): Coin flies, you stay
- Heavy/anchored object: You fly away from it

**4. Metal inside a body cannot be pushed**
- Can't push metal weapons while enemy's holding them (though strong pushers can overcome this)
- Can't push metalminds embedded in bodies

**5. The push comes from your "center of self"**
- Not your physical center — your spiritual/cognitive center
- This is why Steel lines can pass through walls (they exist on the Spiritual Realm)

### Steel Push

Pushes metal objects **away** from your center of self.

Key uses:
- Launch coins as projectiles
- Push metal enemies backward
- Push off metal ground to fly upward
- Create steel bubble to deflect incoming metal

### Iron Pull

Pulls metal objects **toward** your center of self.

Key uses:
- Disarm enemies with metal weapons
- Pull coins back to you
- Pull yourself toward metal anchors

## Allomantic Sight

When burning Steel or Iron, you see faint blue lines extending from your center of self to every metal object in range.

From the Coppermind:
> "When burning steel, blue lines emerge from the Coinshot and connect themselves to pieces of nearby metal, with the size of the steel line indicating how big the metal is."

**Key facts:**
- Line brightness = object mass (brighter = heavier)
- Lines exist on the Spiritual Realm — they pass through physical objects
- You can see metal *inside* walls
- You can identify specific metals and even colors with the lines

Currently implemented with Debug.DrawLine. Will upgrade to proper VFX in Sprint 2.

## Metal Reserves

Each metal has its own reserve (0-100). Burning consumes the reserve.

### Burn Rate
- Burning faster = more power but drains faster
- "Flaring" = burning at max rate for extra strength

### Key Question for Team
Should burn rate be:
- A) Constant (simple, predictable)
- B) Variable based on intensity (hold for more power)
- C) Proportional to user's weight (lore accurate)

## Open Questions

> ❓ **TEAM**: What should the starting reserve amount be for Steel/Iron? (50? 100?)

> ❓ **TEAM**: Should we implement weight-proportional push strength? (Lore accurate but harder to balance)

> ❓ **TEAM**: Should flaring be a mechanic? (Hold shift for extra power, drains faster)

> ❓ **TEAM**: Should we implement the "metal in bodies" rule? (Realistic but complex)

## Reference: Invested Project

The game "Invested" (github.com/austin-j-taylor/Invested) has working Allomancy physics we can reference.

What they've solved:
- Force calculation based on distance and mass
- Momentum conservation for flight
- Blue line rendering

We should check their implementation when refining our physics.
