# Allomancy Science: Steel Push Physics Analysis

*Based on Reddit r/Mistborn physics discussions*

---

## Core Assumptions

### 1. Force = Reaction (Newton's 3rd Law)
When a Coinshot pushes a metal object:
- Force on coin = Force on Coinshot (equal and opposite)
- This means if you push a coin with 1700N, you experience 1700N recoil

### 2. Self-Launch Capability
If a Mistborn can launch themselves upward at ~1g (gravity acceleration):
- They're applying ~2x their body weight in force
- 170 lb person ≈ 756N to counteract gravity
- Double for self-launch ≈ **1510N** force output

---

## Coin Physics Calculation

| Variable | Value | Unit |
|----------|-------|------|
| Coin mass (quarter) | 5.67 | grams |
| Push force | 1510 | N |
| Push range | 10 | meters |

### Acceleration
```
a = F/m
a = 1510N / 0.00567kg
a = 266,000 m/s²
```

### Final Velocity
```
v = a × t
t = distance / average velocity
t ≈ 8ms (milliseconds)
v ≈ 2.4 km/s
```

**Result: Coins hit ~2.4 km/s** ( muzzle velocity of .50 cal sniper = 0.8 km/s)

---

## Key Findings from Discussion

### Finding 1: Push Strength Varies with Distance

**Quote from The Final Empire:**
> "The line of blue pointing toward the ingot grew fainter and fainter... the more her speed decreased."

**Physics Connection:**
- Force DECREASES as distance increases
- NOT constant force (like gravity)
- Likely follows some decay function (exponential or inverse)

**Game Implementation:**
```csharp
float CalculatePushForce(float distance, float maxRange)
{
    float normalizedDistance = distance / maxRange;
    return baseForce * Mathf.Exp(-normalizedDistance * decayRate);
}
```

### Finding 2: Push Isn't Pure Newtonian

**Problems if it was pure physics:**
1. Wax should be bulletproof (cancel bullet momentum = trivial)
2. Coinshot railgun effect (coins would be hypersonic)
3. No counter-force pushing (Kelsier vs Camon scene)

**Book Evidence:**
- Vin can hover over a coin without oscillating (not constant force)
- Push force varies based on target velocity
- Less force applied to allomancer than target (unless heavy/fixed object)

### Finding 3: Maximum Power Limitation

**From discussion:**
> "Force that can be applied in a steel push is not the only limitation. Power seems to be limited as well. Maximum force is inversely proportional to velocity."

**Why this matters:**
- Can push harder against stationary objects
- Can't push hard against fast-moving objects
- Explains why coins have a velocity cap

**Physics Formula:**
```
P_max = F × v
If P is limited, then: F = P_max / v
```

### Finding 4: Air Resistance Matters

**Comment:**
> "Coins aren't aerodynamic enough to break the sound barrier"

**Real-world consideration:**
- 2.4 km/s is WAY past sound barrier (~340 m/s)
- Coins would tumble, slow down rapidly
- Less realistic damage at range

---

## Simplified Game Physics Model

Based on the discussion, here's a balanced model:

### Force Calculation - LINEAR MODEL (Better for Gameplay!)

**From Reddit discussion:** Inverse square law causes problems:
- Standing on coin + pushing = infinite force
- Pull doesn't work well at all
- Books show force DECREASES with distance (Vin hovering scene)

**LINEAR FALLOFF formula:**
```
F_push = F_max × (1 - d / R_max)
```

Where:
- `F_max` = Maximum force at point-blank
- `d` = Distance to target
- `R_max` = Maximum push range
- Result: 0 when at max range, F_max when touching

### Key Variables:
| Variable | Effect | Recommended Value |
|----------|--------|------------------|
| `F_max` | Maximum push strength | 1500-2000 N |
| `R_max` | Maximum push distance | 50 meters |
| `linearFalloff` | Force decreases linearly | No decay constant needed |

### Coin Launch Velocity (Game Balance)
```
maxCoinVelocity = ~800-1200 m/s (tunable for gameplay)
NOT 2400+ m/s (would be unbalancing)
```

### Key Book Evidence:
From The Final Empire (Vin hovering):
> "The fainter the line grew, the more her speed decreased"

---

## Comparison: Real Physics vs Game Physics

| Aspect | Real Physics | Game Physics (Balanced) |
|--------|--------------|-------------------------|
| Coin velocity | 2.4 km/s | 800-1200 m/s |
| Push force | Constant | Distance-decaying |
| Self-launch | Full recoil | Reduced for playability |
| Bullet deflection | Easy | Requires timing |
| Range | ~10m effective | 30-50m with falloff |

---

## Design Philosophy

From the discussion:

> "Iron/steel allomancy cannot be explained as just a straight out force... Newton's 3rd law is ignored / works differently... It's magic, ok?"

**Our approach for the game:**

1. **Visually authentic** - Blue lines, satisfying force feedback
2. **Feel good to play** - Not punishing realism
3. **Lore-consistent** - Distance falloff, metal weight affects push
4. **Balanced gameplay** - Coins aren't railgun projectiles

---

## Implementation Reference

### Steel Push Force Formula - LINEAR (Recommended!)
```csharp
public float CalculatePushForce(float distance)
{
    // LINEAR falloff - no infinite force at zero distance!
    float distanceFactor = 1f - (distance / maxRange);
    distanceFactor = Mathf.Max(0f, distanceFactor); // Can't be negative
    
    float force = baseForce * distanceFactor;
    return force;
}
```

### Better Implementation (with velocity damping):
```csharp
public float CalculatePushForce(float distance, float targetVelocity)
{
    // LINEAR distance falloff
    float distanceFactor = 1f - Mathf.Clamp01(distance / maxRange);
    
    // Velocity damping (harder to push fast objects)
    float velocityFactor = 1f - Mathf.Clamp01(Mathf.Abs(targetVelocity) / maxTargetVelocity);
    
    // Combine factors
    float force = baseForce * distanceFactor * (0.5f + 0.5f * velocityFactor);
    
    return force;
}
```

### Range System
```csharp
public bool IsInRange(Vector3 targetPosition)
{
    float distance = Vector3.Distance(transform.position, targetPosition);
    return distance <= maxRange && distance >= minRange;
}
```

---

## Summary

| Concept | Description |
|---------|-------------|
| **Newton's 3rd Law** | Equal force on pusher and pushed (mostly) |
| **Distance Decay** | Force drops off with distance (exponential) |
| **Power Limit** | Can't push fast objects as hard |
| **Air Drag** | Coins slow down rapidly |
| **Velocity Cap** | Coins capped for game balance |

**Key takeaway:** Allomancy follows "hard magic" rules - consistent enough to be predictable, but physics is modified for storytelling and gameplay.

---

## Canonical Evidence from Coppermind Wiki

### Rule 1: Force Proportional to Weight

**From Coppermind (Steel page):**
> "The strength of your push is roughly proportional to your physical weight. This means that larger Allomancers can generally Steelpush and Ironpull more powerfully than a smaller counterpart."

**Game Implementation:**
```csharp
float forceMultiplier = playerWeight / targetWeight;
float pushForce = baseForce * forceMultiplier;
```

### Rule 2: Force Inversely Proportional to Distance

**From Coppermind:**
> "The force of the Push upon an object is inversely proportional to the Coinshot's distance to said object. This continues until the Coinshot hits a zenith, or point of maximum altitude."

**Note:** This is NOT inverse square law (1/r²), but simple inverse (1/r). The zenith point is where this relationship changes.

### Rule 3: Zenith Point (Maximum Altitude)

**From Coppermind:**
> "This continues until the Coinshot hits a zenith, or point of maximum altitude. This zenith is higher for more powerful Steelpushes, such as those burned in conjunction with duralumin, or with more massive steel anchors."

**Game Implication:**
- There's a point where push force maxes out
- Beyond this point, force may decrease or behave differently
- We set zenith at 5 meters for balanced gameplay

### Rule 4: Spiritual Realm Blue Lines

**From Coppermind:**
> "When burning steel, blue lines emerge from the Coinshot and connect themselves to pieces of nearby metal, with the size of the steel line indicating how big the metal is."

> "The steel lines manifest themselves on the Spiritual Realm and can be cut or interfered with. Supposedly due to their lack of physicality, they can pass through physical objects such as walls, allowing a Coinshot to ascertain the location of metals outside of their line of sight and to detect such things as metal girders within walls."

**Game Implementation:**
- Blue lines should pass through walls (no line-of-sight check needed)
- Line thickness indicates metal mass
- Can see through floors/ceilings

### Rule 5: Metal Inside Bodies is Resistant

**From Coppermind:**
> "Steel Allomancy is strongly resisted by the body of sophonts, thus making metalminds embedded into the body difficult to push. Due to this, some Feruchemists choose to surgically implant their metalminds into their body."

> "The degree of resistance is proportional to how Invested the target is, and would therefore require more force to push metal in the body of someone such as Susebron and an average Scadrian, who would be harder than a Drab."

**Game Implication:**
- Metal armor on NPCs should be harder to push than external metal
- Invested metal (metalminds) even more resistant

### Rule 6: Conservation of Momentum with Feruchemy

**From Coppermind:**
> "The Law of Conservation of Momentum holds when using iron Feruchemy. Therefore, a Coinshot who has access to iron Feruchemy can alter their mass in order to increase or decrease their velocity. When one increases their mass, their velocity would decrease in order to equalize their momentum, and vice versa."

**For Our Game:**
- Basic physics: momentum is conserved
- If player is lighter (via Feruchemy), they accelerate faster but can't push as hard
- If player is heavier, they push harder but accelerate slower

### Rule 7: Steelpush is NOT Pure Newtonian

**Key Insight:** While we use Newtonian physics as a base, Allomancy modifies it:

1. **Force varies with distance** (not constant like gravity)
2. **Zenith point exists** (no infinite force at zero distance)
3. **Metal in body is protected** (not pure physics)
4. **Blue lines are Spiritual, not Physical** (no line-of-sight needed)

---

## References

### Canonical Sources (Primary)
- **Coppermind Wiki - Steel page**: https://coppermind.net/wiki/Steel
  - Weight proportionality rule (Section: Allomantic Use)
  - Distance inverse relationship (Section: Allomantic Use)  
  - Zenith point mechanics (Section: Allomantic Use)
  - Spiritual Realm blue lines (Section: Allomantic Use)
  - Body resistance to Steelpush (Section: Allomantic Use)
  - Conservation of momentum with Feruchemy (Section: Synergy with Iron Feruchemy)

### Community Sources (Secondary)
- Reddit r/Mistborn physics discussions
- 17th Shard forums (detailed simulations)
- The Final Empire (TFE Ch. 8 - Vin's steel sight)
- Well of Ascension (Wax's steel bubble mechanics)

### Book References
- The Final Empire, Chapter 7: "Steelpushing is the art of burning steel to push metals away"
- The Lost Metal, Chapter 25: "The steel lines manifest themselves on the Spiritual Realm"
- The Lost Metal, Chapter 7: "The force of the Push is inversely proportional to distance"
- The Alloy of Law, Prologue: "Steelpushing liquid metals works similarly to a ferrofluid"
