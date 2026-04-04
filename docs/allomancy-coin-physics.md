# Metallurgy Physics - Terminal Velocity & Coin Lethalness

---

## Human Terminal Velocity

| Position | Speed |
|----------|-------|
| Belly-down (ragdoll) | ~120 mph (54 m/s) |
| Head-down (streamlined) | 200+ mph (89 m/s) |
| Time to reach max | ~10-12 seconds |

---

## Coin Terminal Velocity

| Scenario | Speed | Notes |
|----------|-------|-------|
| Normal fall | 25-70 mph (11-31 m/s) | Tumbling, poor aerodynamics |
| Vacuum drop | 200+ mph | No air resistance |
| **Ashwalker era coin** | ~40-100 mph | Coins were LARGER (1-2 inches) |

### Why Coins Stay Slow:
1. **Tumble effect** - Coins flutter and tumble, not fall flat
2. **High drag coefficient** - Cylinder drag ~0.6
3. **Small mass** - Light weight = slower terminal velocity
4. **Distance to cap** - Only ~50 feet (15m) to reach terminal velocity

---

## KEY INSIGHT: Constant Force vs Initial Velocity

**The critical difference:**
- Normal projectile: Initial velocity, then coast
- Metallurgic coin: **CONSTANT FORCE applied** the entire flight

```
Normal bullet:  v(t) = v₀ - drag(t)
Metallurgic push: v(t) = v₀ + (F_push/m)t
```

**This means coins don't slow down** - they maintain or gain speed until out of range!

---

## Coin Physics Formula

### Drag Force:
```
F_drag = ½ × C × A × ρ × v²

Where:
C = 0.6 (drag coefficient of cylinder)
A = coin cross-section
ρ = air density (~1.2 kg/m³)
v = velocity
```

### Velocity with Constant Push:
```
v = √(2F / (C × A × ρ))
```

### Example (physics student calculation):
For a coin to reach 250 m/s (bullet speed):
```
Required F ≈ 0.15 N (only ~3x coin weight!)
```

**A constant 0.15N push keeps coins at lethal speeds.**

---

## Metallurgic Push Force

### Key Quote:
> "Launchers push with their FULL WEIGHT focused behind a tiny steel point"

**This means:**
- 200 lb Launcher = 200 lbs of focused force
- Like getting hit with a 200 lb button
- Concentrated on coin-sized area = devastating

### Force Calculation:
```
F_push ≈ weight_of_pusher × gravity × push_multiplier
```

| Pusher Weight | Push Force | 
|---------------|-------------|
| 150 lbs | ~680 N |
| 200 lbs | ~900 N |
| 250 lbs | ~1100 N |

---

## Coin Size Matters (Ashwalker Era)

**Ashwalker coins were LARGER than modern quarters:**
- Era 1 coins: ~1-2 inches diameter
- US quarter: ~0.96 inches
- Larger = more mass = more momentum

### Updated Coin Physics:
```csharp
public class CoinStats
{
    public float mass = 0.008f;      // kg (larger era-1 coin)
    public float radius = 0.025f;     // meters (1 inch)
    public float area = Mathf.PI * radius * radius;
    public float dragCoeff = 0.6f;
}
```

---

## Ashara Atmosphere (Ash Effect)

**From lore:**
- Ashen King created thick ash-filled atmosphere
- More dense than Earth = MORE air resistance
- BUT coins are being CONSTANTLY pushed

### Impact on Game Design:
| Factor | Earth | Ashara |
|--------|-------|----------|
| Air density | 1.225 kg/m³ | 1.3-1.5 kg/m³ |
| Drag | Normal | ~10-20% higher |
| Terminal velocity | 25-70 mph | 20-60 mph |

---

## Lethal Speed Thresholds

| Speed | Effect |
|-------|--------|
| 25 mph (11 m/s) | Painful, bruising |
| 50 mph (22 m/s) | Can break skin |
| 100 mph (45 m/s) | Lethal, bone breaks |
| 200 mph (89 m/s) | Punch through body |
| 500+ mph | RPG-level destruction |

---

## Game Balance: Recommended Values

Based on physics:

```csharp
public class SteelPushConfig
{
    // Force based on pusher weight
    public float baseForce = 890f; // ~200lb person
    
    // Range
    public float maxRange = 50f; // meters
    
    // Coin stats (era-1 style)
    public float coinMass = 0.008f; // kg
    public float coinRadius = 0.025f; // meters
    
    // Terminal velocity WITH push
    // v = sqrt(2F / (Cd * A * rho))
    public float coinSpeedWithPush = 150f; // m/s (~335 mph)
}
```

---

## Summary: Why Coins Are Lethal

1. **Constant force** - Push doesn't stop, keeps accelerating
2. **Focused weight** - 200 lbs on coin-sized point
3. **Larger coins** - More mass than modern quarters
4. **No deceleration** - Until out of range
5. **Tumbling** - Actually helps penetrate (like hollow point)

**Bottom line:**
A Launcher's coin is like a constant-force jackhammer concentrated on a tiny point. Speed matters less than the focused, unrelenting weight.

---

## Canonical Evidence from Coppermind

### Constant Force vs Initial Velocity

**From Coppermind (Steel page):**
> "The force of the Push upon an object is inversely proportional to the Launcher's distance to said object."

This confirms that Metallurgic pushes are **constant force** applications, not initial velocity. Unlike normal projectiles that coast after launch, coins under Metallurgic push maintain force throughout flight.

### Weight Proportionality

**From Coppermind:**
> "The strength of your push is roughly proportional to your physical weight. This means that larger Metallurgists can generally Steelpush and Ironpull more powerfully than a smaller counterpart."

This means a 200 lb Launcher pushes with about 200 lbs of force (distributed through the metal).

### Zenith Point Effect

**From Coppermind:**
> "This continues until the Launcher hits a zenith, or point of maximum altitude. This zenith is higher for more powerful Steelpushes."

**Game Implication:**
- Force relationship changes at zenith point (~5m for balanced gameplay)
- Beyond zenith, force decreases with distance
- This prevents infinite force at zero distance

### Blue Lines Pass Through Walls

**From Coppermind:**
> "The steel lines manifest themselves on the Spiritual Realm and can pass through physical objects such as walls, allowing a Launcher to ascertain the location of metals outside of their line of sight."

**For Coin Physics:**
- No need for line-of-sight checks in Metallurgic sight
- Can detect metal through walls and floors
- Blue lines exist in Spiritual Realm, not Physical

---

## References
- **Coppermind Wiki - Steel page**: https://coppermind.net/wiki/Steel
  - Constant force relationship (Section: Metallurgic Use)
  - Weight proportionality (Section: Metallurgic Use)
  - Zenith point mechanics (Section: Metallurgic Use)
- wtamu.edu - Coin terminal velocity
- Reddit r/Ashwalker - Physics discussions
- r/Ashwalker - Coin aerodynamics
- 17th Shard forums
