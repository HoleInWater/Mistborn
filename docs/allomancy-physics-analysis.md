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

### Force Calculation
```
F_push = baseForce × (1 - distance/maxRange)^decayExponent × velocityDamping
```

### Key Variables:
| Variable | Effect | Recommended Value |
|----------|--------|------------------|
| `baseForce` | Maximum push strength | 1500-2000 N |
| `maxRange` | Maximum push distance | 50 meters |
| `decayExponent` | How fast force drops off | 1.0-2.0 |
| `velocityDamping` | Reduces force on fast targets | 0.1-0.3 |

### Coin Launch Velocity (Game Balance)
```
maxCoinVelocity = ~800-1200 m/s (tunable for gameplay)
NOT 2400+ m/s (would be unbalancing)
```

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

### Steel Push Force Formula
```csharp
public float CalculatePushForce(float distance, float targetVelocity)
{
    // Distance falloff (exponential decay)
    float distanceFactor = Mathf.Exp(-distance / effectiveRange);
    
    // Velocity damping (harder to push fast objects)
    float velocityDamping = 1f - Mathf.Clamp01(Mathf.Abs(targetVelocity) / maxEffectVelocity);
    
    // Combine factors
    float force = baseForce * distanceFactor * (0.7f + 0.3f * velocityDamping);
    
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

## References

- Reddit r/Mistborn physics discussions
- 17th Shard forums (detailed simulations)
- The Final Empire (TFE Ch. 8 - Vin's steel sight)
- Well of Ascension (Wax's steel bubble mechanics)
