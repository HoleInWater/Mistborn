# The Mistborn Physics & Mathematics Handbook

*A comprehensive analysis of the physics and mathematics of Brandon Sanderson's Mistborn universe*

---

## Table of Contents

1. [Core Equations](#1-core-equations)
2. [Steel & Iron: Push/Pull Force Functions](#2-steel--iron-pushpull-force-functions)
3. [Coin Velocity Functions](#3-coin-velocity-functions)
4. [Feruchemy Storage Functions](#4-feruchemy-storage-functions)
5. [Compounding Exponential Functions](#5-compounding-exponential-functions)
6. [Speed Compounding Functions](#6-speed-compounding-functions)
7. [Iron Compounding Mass Functions](#7-iron-compounding-mass-functions)
8. [Pewter Strength Functions](#8-pewter-strength-functions)
9. [Time Bubble Functions](#9-time-bubble-functions)
10. [Graphical Analysis](#10-graphical-analysis)
11. [Diminishing Returns Functions](#11-diminishing-returns-functions)
12. [Practical Applications](#12-practical-applications)

---

## 1. Core Equations

### Newton's Second Law
```
F = m × a

Where:
  F = Force (Newtons)
  m = Mass (kg)
  a = Acceleration (m/s²)
```

### Kinetic Energy
```
KE = ½mv²

Where:
  KE = Kinetic Energy (Joules)
  m = Mass (kg)
  v = Velocity (m/s)
```

### Gravitational Force
```
Fg = G × (m₁ × m₂) / r²

Where:
  G = Gravitational constant ≈ 6.674 × 10⁻¹¹ N⋅m²/kg²
  m₁, m₂ = Masses of objects
  r = Distance between centers
```

---

## 2. Steel & Iron: Push/Pull Force Functions

### Primary Force Equation

The allomantic force follows an inverse-square law:

```
         A × m₁ × m₂
F(a) = ─────────────────
              r²

Where:
  F(a) = Allomantic force (Newtons)
  A     = Allomantic strength constant (varies by user/flaring)
  m₁    = Mass of allomancer (kg)
  m₂    = Mass of metal being pushed/pulled (kg)
  r     = Distance between allomancer and metal (m)

Domain:  r ∈ [0, max_range]
Range:    F(a) ∈ [0, F_max]
```

### Allomantic Strength Constant (A)

Based on Vin's hover calculation:
```
Given:
  Vin mass (m₁) = 40 kg
  Coin mass (m₂) = 0.01 kg
  Equilibrium distance (r) = 6 m
  g = 9.81 m/s²

At equilibrium: F(a) = m₁ × g

Solving for A:
  A = (m₁ × g × r²) / (m₁ × m₂)
  A = (40 × 9.81 × 36) / (40 × 0.01)
  A = 14,126.4 × 36 / 0.4
  A = 35,316
```

**A_vin ≈ 35,316** (without flaring metals)

### Linear Force Model (Game Design)

For better game feel, a linear model was proposed:
```
              F_max × (r_max - r)
F(a) = ─────────────────────────────
                   r_max

For: 0 ≤ r ≤ r_max

Graph Shape: Linear decrease from F_max to 0
```

---

## 3. Coin Velocity Functions

### Maximum Velocity at Distance

Assuming constant force and no air resistance:
```
v(d) = √(2 × F(a) × d / m₂)

Where:
  v(d) = Velocity at distance d (m/s)
  F(a) = Allomantic force (constant, calculated at initial distance)
  d    = Distance traveled (m)
  m₂   = Mass of coin (kg)
```

### Vin's Maximum Coin Push

```
Given:
  A = 35,316
  m₁ = 40 kg (Vin)
  m₂ = 0.01 kg (coin)
  r  = 5 m (push distance)
  
Step 1: Calculate Force
  F(a) = A × m₁ × m₂ / r²
  F(a) = 35,316 × 40 × 0.01 / 25
  F(a) = 14,126.4 / 25
  F(a) = 565 N

Step 2: Calculate Acceleration
  a = F(a) / m₂
  a = 565 / 0.01
  a = 56,500 m/s²

Step 3: Calculate Velocity
  v = √(2 × a × d)
  v = √(2 × 56,500 × 5)
  v = √565,000
  v ≈ 751.7 m/s

Step 4: Top Speed (extended push)
  v_max = √(2 × F(a) × d_max / m₂)
  v_max = √(2 × 565 × 50 / 0.01)
  v_max ≈ 2,377 m/s
```

### Conservative Estimate (Book-Consistent)

```
Using: A_conservative = 1,500

F(a) = 1,500 × 40 × 0.01 / 25 = 24 N
v_max = √(2 × 24 × 50 / 0.01) = √240,000 ≈ 490 m/s

This is more consistent with book portrayals.
```

### Air Drag Correction (Advanced)

```
v(d) = v_terminal × (1 - e^(-d/τ))

Where:
  v_terminal = Terminal velocity = m₂ × g / (½ × ρ × C_d × A)
  τ = Drag time constant
  ρ = Air density ≈ 1.225 kg/m³
  C_d = Drag coefficient ≈ 0.47 (sphere)
  A = Cross-sectional area (m²)

For a quarter (r ≈ 0.012m):
  A = π × 0.012² ≈ 0.00045 m²
  v_terminal = 0.01 × 9.81 / (0.5 × 1.225 × 0.47 × 0.00045)
  v_terminal ≈ 77.8 m/s
```

---

## 4. Feruchemy Storage Functions

### Basic Storage Function

Storage is proportional to time stored at a given rate:

```
S(t) = ∫[0 to t] r(τ) dτ

Where:
  S(t) = Total stored at time t
  r(τ) = Storage rate as function of time τ
  t     = Total storage time
```

### Constant Rate Storage

If storing at constant rate r₀:
```
S(t) = r₀ × t
```

### Variable Rate Storage

```
S(t) = ∫[0 to t] k × e^(-λτ) dτ
     = (k/λ) × (1 - e^(-λt))

Where:
  k = Initial storage rate
  λ = Diminishing returns factor
```

### Metal Capacity Function

```
C_max = K × V × ρ_metal

Where:
  C_max = Maximum capacity of metalmind
  K     = Capacity constant (varies by metal)
  V     = Volume of metal (m³)
  ρ     = Density of metal (kg/m³)

Empirical constants:
  Iron:   K ≈ 1.0 (baseline)
  Steel:  K ≈ 1.1
  Pewter: K ≈ 0.95
```

### Storage with Diminishing Returns

```
        C_max × r
S = ─────────────────
       C_max + r

Asymptotic function approaching C_max
```

---

## 5. Compounding Exponential Functions

### Basic Compounding Loop

```
Cycle 0: Store 1 unit
Cycle 1: Burn → Get 10 units
Cycle 2: Store 10 → Burn → Get 100 units
Cycle n: Get 10^n units

P(n) = P₀ × 10^n

Where:
  P(n) = Power after n cycles
  P₀   = Initial stored power
  n    = Number of compounding cycles
```

### Compounding with Diminishing Returns

```
P(n) = P₀ × 10^n × e^(-δn)

Where:
  δ = Diminishing returns constant (0 < δ < 1)
```

### Net Gain Per Cycle

```
G(n) = P(n) - P(n-1) - C_cost

Where:
  G(n) = Net gain at cycle n
  C_cost = Investiture cost per cycle
```

---

## 6. Speed Compounding Functions

### Basic Feruchemy Steel

```
v_stored(t) = ∫[0 to t] s(τ) dτ

Where:
  v_stored = Speed stored in metalmind
  s(τ)     = Speed at time τ
```

### Compound Speed Function

```
v_compound(n) = v_base × 10^n × e^(-εn)

Where:
  v_base = Base stored speed
  n      = Number of compound cycles
  ε      = Efficiency decay constant

Maximum theoretical: ~50 km/s (0.0167% c)
```

### Speed vs. Time Perception

```
T_perceived = T_actual × (1 - v/c)

As v approaches c, perceived time slows
At v = 50 km/s:
  T_perceived ≈ T_actual × 0.99983
```

### Heat Generation from Air Resistance

```
P_heat = ½ × ρ × C_d × A × v³

For a human at v = 50 km/s:
  P_heat = 0.5 × 1.225 × 0.47 × 0.7 × (50,000)³
  P_heat ≈ 3.0 × 10¹¹ W (300 gigawatts!)

This would vaporize the runner instantly.
```

---

## 7. Iron Compounding Mass Functions

### Basic Iron Feruchemy

```
m(t) = m_base + ∫[0 to t] w(τ) dτ

Where:
  m(t)  = Mass at time t
  m_base = Base mass
  w(τ)   = Weight storage rate
```

### Compound Mass Function

```
m_compound(n) = m_base + m_stored × 10^n

Where:
  m_stored = Mass stored before compounding
  n        = Number of compound cycles
```

### Schwarzschild Mass Limit

```
r_s = (2 × G × m) / c²

For m = 1 kg:  r_s ≈ 1.48 × 10⁻²⁷ m
For m = 70 kg: r_s ≈ 1.04 × 10⁻²⁵ m
For m = 1000 kg: r_s ≈ 1.48 × 10⁻²⁴ m

Conclusion: Human cannot become black hole through Feruchemy alone
```

### Weight vs. Mass Distinction

```
Weight formula (Feruchemy):
  W = m × g × f

Where:
  W = Perceived weight
  m = Actual mass
  g = Gravitational acceleration
  f = Feruchemy weight factor (0 < f < ∞)

Conservation of momentum:
  p_total = m₁v₁ + m₂v₂ (conserved in Feruchemy)
```

---

## 8. Pewter Strength Functions

### Strength Multiplier

```
S_pewter = S_base × (1 + k × P)

Where:
  S_pewter = Pewter-enhanced strength
  S_base   = Base strength
  k        = Pewter efficiency constant
  P        = Pewter power level (0 to 1)
```

### Muscle Mass Relationship

```
m_muscle = m_base × (1 + α × P)

Where:
  α = Muscle growth constant ≈ 0.5 per power level
```

### Combined Strength-Weight Effect

```
F_max = m_total × a_max / η

Where:
  m_total = m_base + Δm_muscle
  a_max   = Maximum sustainable acceleration
  η       = Efficiency factor
```

### Stacking Pewter + Iron

```
Tapping Pewter:  +m_muscle → +weight
Storing Iron:    -m_weight

Net effect: +m_muscle - m_weight

At equilibrium: These approximately cancel, preventing infinite power
```

---

## 9. Time Bubble Functions

### Cadmium (Slow) Bubble

```
T_inside = T_outside × τ_slow

Where:
  τ_slow = Time dilation factor (0 < τ_slow < 1)
  Typical: τ_slow ≈ 0.1 (10x slower)

Duration limit: D_max = D_metal × E_efficiency
```

### Bendalloy (Fast) Bubble

```
T_inside = T_outside × τ_fast

Where:
  τ_fast = Time acceleration factor (τ_fast > 1)
  Typical: τ_fast ≈ 10 (10x faster)
```

### Combined Bubble Effects

```
For cadmium + bendalloy interaction:
  T_effective = T_outside × (τ_cadmium / τ_bendalloy)
```

### Light Frequency Shift (Blocked by Investiture)

```
f_inside = f_outside × τ
f_exit   = f_inside / τ = f_outside (No net shift!)

Brandon Sanderson explicitly blocks this physical effect.
```

---

## 10. Graphical Analysis

### Steel Push Force vs Distance

```
Force (N)
    │
400 │                                    ╭────────── Maximum
    │                               ╭────╯
300 │                          ╭────╯     Force Limit
    │                     ╭────╯
200 │                ╭────╯            Speed Limit
    │           ╭────╯
100 │      ╭────╯
    │ ╭────╯
    └────────────────────────────────────────
      0   1   2   3   4   5   6   7   8   Distance (m)

    ═══════════════════════════════════════════════
    Cross-over point: Force limitation → Speed limitation
```

### Steel Push Velocity vs Distance

```
Velocity (m/s)
    │
200 │╭───────────────────╮
    ││                   │
150 ││    ╭──────────────╯
    ││    │ Speed limit
100 ││    │
    ││    │
 50 │╯    │
    │     ╰──────────────────────────────
    └────────────────────────────────────────
      0   1   2   3   4   5   6   7   8   Distance (m)

    ═══════════════════════════════════════════════
    Velocity decreases as distance increases (1/r²)
```

### Compounding Growth

```
Power Level (log scale)
    │
    │                                           ╱ Exponential
1M │                                        ╱
    │                                     ╱
100K│                                  ╱
    │                               ╱
10K │                            ╱
    │                         ╱
 1K │                      ╱
    │                   ╱
100 │                ╱
    │             ╱
 10 │          ╱  ╱
    │       ╱╱╱╱
  1 │════╱═══════════════════════════════════════
    0   1   2   3   4   5   Cycles
       Compounding cycle (×10 per cycle)
```

### Diminishing Returns Curve

```
Effectiveness
    │
100%│╭─────────────────────────────────────────
    ││  Full power
 80%││ ╭──
    ││ │
 60%││ │        Diminishing
 40%││ │        returns
    ││ │        kick in
 20%││ │        here
    │╯ │        │
    │  ╰────────╯
    └─────────────────────────────────────────
      0   25  50  75  100 Metal Reserve (%)
```

### Time Bubble Time Dilation

```
Outside Bubble          Inside Bubble
    │                        │
    │ Timeline ──────────────│──────────────
    │    │                   │    │
    │    │  Time flows       │    │ SLOW
    │    │  normally         │    │ TIME
    │    │                   │    │
    │    │                   │    │
    │════│═══════════════════│════│═════════
         │                   Bubble Boundary
```

### Pewter Enhancement Curve

```
Enhancement
    │
  4x │                              ╱
     │                            ╱
  3x │                          ╱ (Flared)
     │                        ╱
  2x │╭──────────────────────╯
     ││ Normal burn
  1x │╯
    └──────────────────────────────────────
      0   1   2   3   4   Time (hours)
         Pewter Drag begins
```

### Iron Feruchemy Mass Effect

```
Inertial Mass
    │
200%│                               ╭──── Mass
    │                              ╱      (mi)
150%│                             ╱
    │                            ╱
100%│━━━━━━━━━━━━━━━━━━━━━━━━━━━╯ Normal
    │ Gravitational Mass
 50% │                            (mg)
    │ Normal
    └──────────────────────────────────────
      -50% -25%  0%  +25% +50% +100%
         Stored ←──│──→ Tapped
```

### Anchor Quality vs Mass

```
Anchor Quality (Q)
    │
1.0 │━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ Building/Wall
    │
0.9 │              ╭──────────────────── Heavy Ingot
    │              │
0.7 │       ╭──────╯
    │       │
0.5 │  ╭───╯          Medium Ingot
    │  │
0.3 │ │
    │ │
0.1 │╯                   Small Ingot
    │  ╭
0.01│  │  ╭────────────── Coin (in flight)
    │  │  │
    └─────────────────────────────────────────
      2g  100g  1kg   10kg   100kg  1000kg
              Metal Mass (log scale)
```

### Two-Regime Model (Pagerunner)

```
Force on Allomancer
    │
    │  ╱╱╱╱╱╱╱╱╱╱
    │╱ REGIME 2:       High force
    │  (Coin hits wall)
    │
    │  (Empty - no force)
    │  REGIME 1:        Low force
    │  (Coin in flight)
    │  ╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱
    └─────────────────────────────────────────
         ←── Time ──→
         
    Discontinuity explains "soft landing" puzzle
```

### Conservation of Momentum (Coin Push)

```
BEFORE:                    AFTER:
    │                          │
    │  Coin →→→               │← Coin
 M₁ │      (m)       M₂       │   (m×v₂)
    │                         │
    │  Allomancer              │ Allomancer ←←
    │                         │   (m×v₁)
    
Conservation: m×v₂ = M₂×v₂ = M₁×v₁ = m×v₁

If m << M₁:  v₁ ≈ 0  (Allomancer barely moves)
If m ≈ M₁:  v₁ ≈ v₂ (Both move equally)
```
F(a)
  ^
  |╲
  | ╲
  |  ╲
  |   ╲
  |    ╲
  |     ╲
  |______╲________> r
  0     r_max

Inverse Square Law: F(a) ∝ 1/r²
```

### Graph 2: Linear Force Model

```
F(a)
  ^
  |╲
  |  ╲
  |    ╲
  |      ╲
  |        ╲
  |          ╲
  |____________╲________> r
  0           r_max

Linear Model: F(a) = F_max × (1 - r/r_max)
Better for game feel (no infinite force at r=0)
```

### Graph 3: Compounding Exponential Growth

```
Power
  ^
  |╲
  | ╲
  |  ╲
  |   ╲
  |    ╲
  |     ╲
  |      ╲_______10^n curve
  |       ╲
  |        ╲
  |________╲___________> n (cycles)
  0  1  2  3  4  5
```

### Graph 4: Feruchemy Storage with Diminishing Returns

```
Stored
  ^
  |╲___________C_max (asymptote)
  | ╲
  |  ╲
  |   ╲
  |    ╲
  |     ╲
  |______╲________________> time
  0      t
```

### Graph 5: Speed Compounding with Efficiency Loss

```
Speed
  ^
  |    ╲
  |     ╲_____
  |      ╲   ╲
  |       ╲    ╲___     10^n (no decay)
  |        ╲      ╲
  |         ╲       ╲__
  |_________╲_________╲___> n
  0         5          10

With decay factor e^(-εn): curve bends downward
```

### Graph 6: Coin Velocity with Air Drag

```
Velocity
  ^
  |╲
  | ╲____
  |  ╲    ╲
  |   ╲     ╲___
  |    ╲        ╲
  |     ╲__________╲___________> distance
  |               v_terminal
  |______________________________>
  0
```

### Graph 7: Metal Capacity Function

```
Capacity
  ^
  |    ╲
  |     ╲___
  |      ╲   ╲
  |       ╲    ╲____
  |        ╲         ╲___
  |_________╲_______________> metal volume
  0
```

### Graph 8: Pewter + Iron Cancellation

```
Strength/
Weight
  ^
  |   Pewter (+muscle = +weight)
  |  ╱
  | ╱  Iron (-weight)
  |╱
  |╲
  | ╲
  |  ╲_______
  |_______________> time
  
Net: Approximately flat (canceling effect)
```

---

## 11. Diminishing Returns Functions

### Exponential Decay Model

```
Effectiveness(E) = E_max × (1 - e^(-λt))

Where:
  λ = Diminishing returns rate constant
```

### Power Law Model

```
Effectiveness(E) = E_max / (1 + (t/τ)ᵏ)

Where:
  τ = Characteristic time constant
  k = Exponent (typically 1-3)
```

### Compounding Efficiency Decay

```
η(n) = η₀ × e^(-δn) × (1 - α/n)

Where:
  η(n) = Efficiency at cycle n
  η₀   = Base efficiency
  δ    = Exponential decay rate
  α    = Minimum efficiency floor
```

### Steel Feruchemy Special Case

```
v_tap(t) = v_stored / (1 + β × t)

Where:
  β = Rate of mental processing limit
```

---

## 12. Practical Applications

### Coin Highway Travel

```
Distance per coin: d_coin ≈ 50-150 ft (15-45 m)
Coins needed for 100 miles: N = 160,934 / d_coin
N ≈ 1,000-10,000 coins

Conclusion: Impractical without pre-placed anchors
```

### Allomantic Jump Height

```
H = v² / (2g) = (F(a) × t_push / m)² / (2g)

For maximum push with A = 35,316, t_push = 1s:
  H_max ≈ 10,000 m (unrealistic - shows book limits force)
```

### Sound Barrier Breaking

```
v_sound ≈ 343 m/s at sea level

Breaking sound barrier requires:
  F(a) / m_coin > 343 × 2g / v_initial

With A = 35,316, Vin can break sound barrier easily
In-book: Force is deliberately limited for narrative
```

### Steel Compounder Speed Limit

```
From WoB and book references:
  v_max_steel_runner ≈ 200 km/h (base)
  v_compounded ≈ 50 km/s (TLR maximum)

Time to cross USA (4,000 km):
  t ≈ 80 seconds at max compounded speed
```

---

## Appendix A: Reference Constants

```
Physical Constants:
  g = 9.81 m/s²              (Scadrial gravity)
  c = 299,792,458 m/s        (Speed of light)
  G = 6.674 × 10⁻¹¹ N⋅m²/kg² (Gravitational constant)
  
Allomantic Constants:
  A_vin_normal ≈ 35,316       (Vin's base allomantic strength)
  A_vin_flared ≈ 10 × A_vin  (10x with flaring)
  max_range ≈ 75-150 m        (Push/Pull maximum distance)
  compound_multiplier ≈ 10     (Standard compounding gain)
  
Human Reference Values:
  m_human ≈ 70 kg (average)
  m_coin ≈ 0.01 kg (quarter)
  v_sound ≈ 343 m/s
```

---

## Appendix B: Glossary

| Symbol | Meaning |
|--------|---------|
| F(a) | Allomantic force |
| A | Allomantic strength constant |
| m₁ | Mass of allomancer |
| m₂ | Mass of metal target |
| r | Distance between allomancer and metal |
| v | Velocity |
| S(t) | Feruchemy storage function |
| P(n) | Power after n compounding cycles |
| τ | Time dilation factor |
| η | Efficiency |

---

## 13. Anchor Quality Theory

### The Problem with Coin Pushes

From the 17th Shard discussion by 8bitBob (2017):

**Key Insight**: Coins are surprisingly poor anchors for allomancy. The mass of the target metal significantly affects push effectiveness.

### Coin Mass Comparison

```
Metal Mass Comparison (from text):
├── American Penny:     2.5g
├── Mexican 100 Peso:   34g
├── Small Ingot:        ~2,270g (5 lbs)
└── Large Ingot:        ~4,500g+ (10+ lbs)

Mass Ratio: Ingot / Penny = ~900-1,800x
```

### The Anchor Quality Equation

Pagerunner's Model 7 introduces "anchor quality" as an efficiency term:

```
F = S × Q

Where:
  F = Total force exerted
  S = Push strength (chosen by Allomancer)
  Q = Anchor quality (0 to 1, based on mass/connection)
```

### Anchor Quality Factors

```
Q = f(mass, connection, visibility)

Coin in flight:       Q ≈ 0.001-0.01 (very poor anchor)
Coin vs wall:         Q ≈ 0.1-0.5 (moderate anchor)
Metal ingot:          Q ≈ 0.5-0.9 (good anchor)
Building/bridge:      Q ≈ 0.95-1.0 (excellent anchor)
```

### Why Coins Seem Powerful

The confusion comes from comparing small pushes:
- Small force on coin → coin flies fast
- Same small force on wall → barely notices
- Both situations have the SAME force, different anchor quality

From Kelsier: *"Either you'll be pulled toward the object, or it will be pulled toward you. If your weights are similar, then you'll both move."*

This confirms **conservation of momentum** and Newton's Third Law apply to allomancy.

---

## 14. Inertial vs Gravitational Mass

### The Feruchemical Iron Paradox

From Satsuoni's analysis (17th Shard, 2013):

**Problem**: If Feruchemy increased BOTH inertial and gravitational mass equally, tapping iron would:
1. Still fall at normal rate (a = F/m unchanged)
2. Take MORE force to jump (F = ma)
3. Result in: falling normally but can't jump = dead

**Solution**: Feruchemy changes ONLY inertial mass, not gravitational mass.

### The Three Types of Mass

```
1. Inertial Mass (mi)
   - Resistance to changes in motion
   - "How hard is it to push this?"
   
2. Passive Gravitational Mass (mg_passive)
   - How gravity pulls ON you
   - "How strong is gravity's pull on me?"
   
3. Active Gravitational Mass (mg_active)
   - How YOU pull on other things
   - "Do I have a gravitational field?"
```

### Feruchemy's Effect

```
Normal Person:  mi = mg_passive = mg_active

Feruchemist Tapping Iron (2x density):
   mi = 2x normal
   mg_passive = 1x normal (gravity unaffected)
   mg_active = 1x normal
   
Result: Fall at normal rate, but more resistant to air drag
```

### Why This Works

```
Normal falling:
   F_gravity = mg (downward)
   F_drag = ½ρv²CdA (upward, increases with v²)
   Terminal velocity: v_t = √(2mg/(ρCdA))
   For m=70kg: v_t ≈ 60 m/s

Tapping iron (2x mi, same mg):
   Same gravity force (mg unchanged)
   Same drag formula (depends on v², not m)
   SAME terminal velocity
   
But: Takes longer to reach v_t (higher inertia)
And: Landing has more force (same a, higher m → higher F)
```

### Brandon Sanderson Confirmed (WoB)

*"We're trying to conserve momentum. We're trying to follow physics as best we can."*

And on weight/mass distinction:
*"Weight is the force exerted on an object by the planet equal to the mass of the object multiplied by the acceleration due to gravity."*

---

## 15. Allomantic Charge Theory

### Gagylpus' Model (17th Shard, 2013)

**Hypothesis**: Allomantic force is distributed like a "body force" proportional to mass (similar to gravity).

### The Four Postulates

```
1. Allomantic force follows inverse-square law
   - Necessary: Otherwise could pull Kredik Shaw from Fadrex
   
2. Force is a "body force" distributed by mass
   - Each bit of mass pushes/pulls every other bit
   - Reduces to center-of-mass for distant objects
   
3. Metals have unique "Allomantic charge"
   - Similar to electric charge
   - Only interacts with charge from same metal piece
   
4. Charge is somewhat mobile in Allomancer's body
   - Equilibrium: distributed proportional to mass
   - Can shift toward/away from contact point
```

### Mathematical Formulation

```
For two mass elements dm₁ and dm₂:
   dF = k × (dm₁ × dm₂) / r²

Integrating over all mass elements:
   F = k × ∫∫ (dm₁ × dm₂) / r²
   
For distant objects (r >> object size):
   F ≈ k × (m₁ × m₂) / d²
   
Where d = distance between centers of mass
```

### Why This Explains Coin Flattening

When Vin and Kelsier push on opposite sides of a coin:
- Force on each coin element differs by distance
- Horizontal forces mostly cancel
- Vertical forces add (coin stretches/flattens)
- Accounts for the "squished coin" phenomenon

---

## 16. Pewter Enhancement Physics

### What Pewter Actually Enhances

From the books and WoB:

```
Pewter Enhancement Profile:
├── Strength:          +100% (most pronounced)
├── Durability:        +50-80% (second most)
├── Speed:             +10-20% (minor)
├── Balance:           +Enhanced (proprioception)
├── Pain Tolerance:    +Immune (combat effectiveness)
├── Fatigue:           +Delayed (stamina extension)
├── Healing:           +Minor boost (cellular rate)
└── Mass:              NO CHANGE
```

### The "Fight or Flight" Analogy

From Lee Falin's analysis (Reactor Magazine, 2012):

```
Pewter (91% tin, 9% lead) acts like:
   - Adrenaline response (epinephrine)
   - Calcium channel activation
   - Muscle fiber recruitment
   - Pain signal suppression
   
This is BIOCHEMICAL, not mechanical
Allomancy amplifies body's OWN responses
```

### Pewter Drag Mechanics

```
Normal Pewter Burn: 2x enhancement
Flared Pewter:      3x enhancement

Pewter Drag (after extended use):
   - Enhancement decreases
   - Eventually: DECREASED below normal
   - Recovery: Stop burning, sleep
   - Duration: Proportional to burn intensity
```

### Pewter + Feruchemy Synergies

**F-Pewter + A-Pewter**: Store magical strength in metalmind
- Tapping later gives enhanced strength
- WITHOUT the other pewter effects
- Can stack with active pewter burning

**F-Steel + A-Pewter**: Ultimate combat combo
```
Tapping speed → Faster burn rate → More pewter power
Math:
   F-Steel 2x → Pewter burn 2x faster
   Normal pewter 2x → 2x effect
   With 2x burn rate: 2x × 2x = 4x effective power
   With flaring: 3x × 2x = 6x power
```

### Brandon on Pewter Storage

*"Allomantic pewter strength can be stored in a metalmind, but it's probably easier to just Compound."* — WoB

This means:
1. A-Pewter can be stored (unlike normal strength)
2. It's a MAGICAL attribute, not physical
3. Compounding is more efficient than storage

---

## 17. Compounding Math - Detailed

### The Compounding Cycle

```
Basic Feruchemy:     1 unit stored → 1 unit retrieved
Compounding Cycle:
   1. Store 1 unit in metalmind
   2. Burn metalmind (Allomancy)
   3. Get ~10 units back (magical amplification)
   4. Store 10 units in new metalmind
   5. Repeat
   
After n cycles: 10^n units equivalent
```

### Compounding Table

```
Cycles   Multiplier   Practical Limit (Era 1)
─────────────────────────────────────────────
0        1x           Baseline
1        10x          Normal Mistborn
2        100x         Powerful Compounder
3        1,000x       TLR's advantage
4        10,000x      Theoretical max
5        100,000x     Universe-breaking (see Dead ers)
```

### Iron Compounding - The "Deaders"

```
Why Iron Compounders Die:

Step 1: Store 1x mass in metalmind
Step 2: Burn → Get 10x mass equivalent
Step 3: Tap → Now has 10x inertial mass
Step 4: Repeat...

After 3 cycles: 1,000x normal mass
   - Normal human (70kg) becomes 70,000 kg
   - Volume unchanged → Neutron star density
   - Bones collapse under own weight
   - Brain crushes itself

Brandon confirmed: "Iron compounders... die from their own weight."
```

### Speed Compounding - TLR's Speed

```
If TLR used F-Steel Compounding:

Store 1 second of speed
Burn metalmind: Get ~10 seconds equivalent
Tapping: Experience 10 seconds in 1 second
   = 10x speed boost (subjective)
   = Move 10x faster (objective)

After 2 cycles: 100x speed
   = 100 seconds of experience per real second
   = Effectively frozen time perception
   
After 3 cycles: 1,000x speed
   = Subjective 1,000 seconds per real second
   = Objective 1,000x faster movement
```

---

## 18. Advanced Force Models

### Pagerunner's Model 6 (Complete)

From the "Impossible Physics of Allomancy" (17th Shard, 2017):

**Core Principles**:
```
1. Allomancer defines "strength" S (mental intention)
2. S sets BOTH max force AND max speed
3. The relationship between F_max and v_max is NON-PHYSICAL
4. Whichever limit is reached first determines behavior
```

**The Two-Regime Model**:

```
Regime 1: Coin in flight
   - Anchor quality Q ≈ small
   - F_actual = S × Q << F_max
   - v_limited by air resistance
   - Allomancer feels negligible force
   
Regime 2: Coin against wall
   - Anchor quality Q ≈ large
   - F_actual = S × Q ≈ F_max
   - v = 0 (object stopped)
   - Allomancer feels full reaction force
```

### The "Equilibrium Distance" Problem

**Problem**: Vin pushes off coin to rise to Kelsier's height. But:
- Full push → Should rocket past equilibrium
- Yet she stops at exactly the right height

**Solution**: Push strength varies with distance

```
S_actual = S_max × f(d)

Where f(d) decreases as d decreases

At ground (d = far):  S = 100% → Full force
At equilibrium (d = eq): S = 50% → Balanced
Below equilibrium:     S = <50% → Gravity wins

Result: Natural "soft landing" at equilibrium point
```

### Why This Is "Magic"

Pagerunner notes:
*"The relationship between maximum speed and maximum force at a given strength is not based in physics whatsoever; it is an entirely arbitrary connection, made so the math matches how we've seen Allomancy function."*

This is the "intent-based" nature of allomancy Brandon built in.

---

## Appendix C: Sources

### Primary Sources (17th Shard Forums)

1. **Pagerunner (2017)** - "The Impossible Physics of Allomancy"
   - 6 models of allomancy
   - Mathematical addendum
   - PDF attachments
   
2. **8bitBob (2017)** - Comments on Pagerunner's thread
   - Inertial mass analysis
   - Conservation of momentum defense
   - Anchor quality theory
   
3. **Scriptorian (2013)** - "Theory on Physics of Allomancy"
   - Inverse-square force formula
   - Feruchemy mass distinction
   
4. **Gagylpus (2013)** - Body force theory
   - Allomantic charge hypothesis
   - Center of mass mechanics
   
5. **Satsuoni (2013)** - Mass type analysis
   - Three mass types (inertial, passive/active gravitational)
   - Feruchemy effects on each
   
6. **Longshot97 (2023)** - Pewter interactions
   - A-Pewter + Feruchemy
   - Compounding hacks

### Secondary Sources

7. **Lee Falin (2012)** - "Science of Allomancy: Pewter"
   - Reactor Magazine analysis
   - Biochemical mechanisms
   
8. **Jack Lonstein (2018)** - "Physics of the Cosmere"
   - Medium article
   - Comprehensive overview

### Brandon Sanderson WoB Sources

- Arcanum.coppermind.net database
- Twitter Q&A sessions
- 17th Shard forum Q&As
- alloy of Law Q&A (2011)

### Key Quotes Used

1. *"We're trying to conserve momentum. We're trying to follow physics as best we can."*
2. *"Allomantic pewter strength can be stored in a metalmind, but it's probably easier to just Compound."*
3. *"Iron compounders... die from their own weight."*
4. *"The mind is such a big part of what makes us who we are."*

---

## Appendix D: Glossary

```
A-Pewter:       Allomantic Pewter (the metal, burning it)
Anchor:         Metal object being pushed/pulled
Atium:          God metal, grants future sight
Cadmium:        Metal, creates time dilation bubbles
Compounding:    Feruchemy + Allomancy loop for amplification
Duralumin:      Enhancement metal, amplifies other powers
Electrum:       Temporal metal, grants self-future-sight
Feruchemy:      Storage magic, 1:1 ratio
Flaring:        Pushing Allomantic power to 150%
Hemalurgy:      Stealing powers via metal spikes
Investiture:    Magical energy (Cosmere-wide concept)
Lurcher:        Iron Puller (moves toward metal)
Mistborn:       Can burn ALL 16 metals
Pewter:         Physical enhancement metal
Push Strength:  Mental intention multiplied by metal burning
Savant:         Extreme user, permanent side effects
Steel Push:     Push metal away (Coinshot)
Tin:            Sensory enhancement metal
Twinborn:       Has one Allomancy + one Feruchemy power
```

---

---

## 19. The Atium Retcon (Era 1)

### What Changed

**Original (Era 1 canon)**:
- Atium was the 11th metal
- Grantied future sight
- Appeared as pure metal
- Atium Mistings existed

**Retconned (Post-Lost Metal)**:
- Pure Atium is a GOD METAL
- Era 1 "atium" was actually **Nalatium** (Atium + Electrum alloy)
- Everyone should be able to burn god metals (retcon explains why they couldn't)
- Atium-electrum alloys called "Nalatium" grant future sight with enhanced mental processing

### Brandon Sanderson's Explanation

From WoB (2021 YouTube Spoiler Stream 3):

> *"This is accurate, yes. You could, by the way, just continue to call it atium. That's what they think atium is in-world. It's very slightly alloyed with electrum, and we call that nalatium."*

### Why the Retcon?

```
1. Universal God Metal Burning
   - God metals (Lerasium, Atium) should be burnable by ALL
   - Era 1 atium couldn't be burned by non-Mistborn
   - This was an "oversight" Brandon wanted to fix
   
2. Pattern Consistency  
   - All other metals have alloys with known effects
   - Atium seemed "out of place"
   - Now follows the pattern: God metals have alloys

3. Future Trilogy Setup
   - Era 4 planned with 16 base metals
   - Pure Atium needed for specific future plot
   - Marsh uses it in Lost Metal to survive
```

### Nalatium Properties

```
Alloy:         Atium + Electrum
In-World Name: "Atium"
Effect:        Future sight + Enhanced mental processing
               (ability to comprehend all the futures seen)

Pure Atium:    True God metal
Effect:        Enhanced vision of future + Mind enhancement
               Grants ability to process vast futures
```

### Era 1 vs Era 2 Metals

```
Era 1 Known:                    Era 2 Known:
├── Steel                    ├── Steel
├── Iron                     ├── Iron
├── Tin                      ├── Tin
├── Pewter                   ├── Pewter
├── Zinc                     ├── Zinc
├── Brass                    ├── Brass
├── Copper                   ├── Copper
├── Bronze                   ├── Bronze
├── Gold                     ├── Gold
├── Electrum                 ├── Electrum
├── Aluminum                 ├── Aluminum
├── Duralumin                ├── Duralumin
├── Chromium                 ├── Chromium
├── Nicrosil                 ├── Nicrosil
├── CADMIUM ← NEW           ├── Cadmium
├── BENDALLOY ← NEW         ├── Bendalloy
├── Atium (NALATIUM)        ├── Atium (P GOD METAL)
└── Malatium (Au+At)        └── Malatium
```

### Implications for Physics Book

**Era 1 "Atium" physics still valid**:
- Future shadow calculations
- Duralumin+Atium bursts
- Atium burning against enemies

**Pure Atium (Era 2+) unknown**:
- Original calculations may not apply
- Mind enhancement likely stronger
- May have different force interactions

---

## 20. Hemalurgy & Koloss Physics

### How Hemalurgy Works

```
Spike Creation:
1. Drive metal through someone's heart
2. Power/attribute is SPIKED into the metal
3. Remove spike (person usually dies)
4. Insert spike into recipient
5. Power/attribute is STAPLED to their Spiritweb

Key Point: Some Investiture is LOST in transfer
           Hemalurgy is "destructive" art of Ruin
```

### Metal Affinities (Hemalurgy)

```
Metal:     Steals:
─────────────────────────────
Iron       Human strength
Steel      Allomantic strength
Tin        Sensory enhancement (tin eyes)
Pewter     Pewter strength/durability
Gold       Gold healing/nicrosil memory
Electrum   Emotional allomancy
Zinc       Rioting (emotions)
Brass      Soothing (emotions)
Copper     Copper mind (memories)
Bronze     Bronze awareness (Seeking)
Aluminum   Feruchemy
Duralumin  Feruchemy + Allomancy
Cadmium    Breath (from Warbreaker connection)
Nicrosil   Investiture stores (compounding hack!)
Atium      Spiritual attributes
Malatium   Gold-like (see alternate self)
```

### Koloss Creation & Physics

```
Creation: 4 Iron spikes through human body
           Each spike steals "human strength"
           
Result:   Koloss gain massive physical power
          Lose intelligence
          Continue growing throughout life
          
Physics Problem: Why do they grow?
```

### Koloss Growth Model

```
Hypothesis 1: Hemalurgic Power Leakage
   - Spikes leak power when not in use
   - Power absorption → physical growth
   - Older koloss = more power absorbed = larger
   
Hypothesis 2: Muscle Mass Compensation
   - More strength = more muscle needed
   - Muscle growth = body growth
   - Skin doesn't grow proportionally (tight/ripped)
   
Brandon's Answer:
   - Koloss are hemalurgic constructs
   - Growth is SPIRITUAL in nature
   - Body adapts to increasing power
   - Harmony "fixed" them in Era 2
```

### Koloss Strength Scaling

```
Normal Human:   70kg body, 100% strength
Koloss (small): 150kg body, ~3-5x human strength
Koloss (large): 500kg+ body, ~10-20x human strength
Koloss (titan): Growth continues until death

Energy Source: Hemalurgic spike leakage + food
```

### Inquisitor Spike Count

```
Standard Inquisitor: 9-11 spikes
Eye Spikes:          Iron/Steel (Allomantic sight)
Major Spikes:        Various powers
Linchpin Spike:      Upper back, holds spiritweb together

Physics Note:
- Heart relocated to accommodate spikes
- Brain shaped around eye-spikes
- Vital areas can be damaged without death
- Linchpin removal = death (spiritweb collapse)
```

### Hemalurgic Constructs Summary

```
Construct:      Spikes:    Result:
─────────────────────────────────────────────
Koloss          4 Iron     Brute strength, growing
Kandra          2 Same     Sapient, flexible bodies
Inquisitor      9-11 Mix   Allomantic powers, near-immortal
```

---

## 21. Aluminum & Duralumin (The True Enhancement Metals)

### Chromium & Nicrosil (Era 2 Discovery)

```
Chromium:    "Erasing" metal
             - Can wipe Allomantic reserves
             - Works like Leecher ability
             - Named after chrome (shiny/smooth)
             
Nicrosil:    "Investiture Store" metal
             - Amplifies other metals burned simultaneously
             - Key to Compounding hacks
             - Named after nickel (hard/industrial)
```

### Enhancement Metal Properties

```
Metal:       Effect:
──────────────────────────────────────────────────
Aluminum:    Drains ALL Allomantic reserves
             - Purer metals for storage
             - Universal counter
             
Duralumin:   Amplifies CURRENTLY BURNING metals
             - 10x effect on active metals
             - Drains reserves faster
             - Used for Duralumin+Atium "nuke"
             
Chromium:    Wipes metal reserves (external)
             - Leecher ability
             - Can target others
             
Nicrosil:    Amplifies ALL metals being burned
             - More efficient than Duralumin
             - Used for Investiture compounding
```

### Enhancement Physics

```
Duralumin Burst:
   Base power × Duration = Normal
   Duralumin boost × Duration = Burst
   
   Example:
   Normal Atium burn: 10 seconds of futures
   With Duralumin:   1 second of SUPER futures
   Total invested:    Same (Investiture conserved)

Nicrosil Compounding:
   Nicrosil + Steel + Speed → SPEED COMPOUNDING
   - Burns speed metalmind with Nicrosil
   - Gets amplified steel experience
   - Tap to get amplified speed
```

---

## 22. Allomantic Conservation Laws

### What Allomancy Conserves

```
CONFIRMED BY BRANDON (WoB):
✓ Conservation of Momentum
✓ Conservation of Energy  
✓ Inverse Square Law (like gravity/EM)

NOT Conservative (Magic Override):
✗ Conservation of Mass (Iron Feruchemy)
✗ Time flow (Speed Bubbles)
✗ Information (Atium futuresight)
```

### Momentum Conservation in Action

```
Scenario: Coinshot pushes coin into wall

Step 1: Force applied to coin (F)
Step 2: Equal force applied to Allomancer (-F)
Step 3: Coin hits wall (massive effective mass)
Step 4: Force redirects Allomancer backward

If Conservation TRUE:
   m_coin × v_coin = m_allo × v_allo
   Small coin gets high v, large Allo gets small v
```

### Energy Conservation

```
Pushing coin (m=0.01kg) to 1000 m/s:
   KE = ½ × 0.01 × 1000² = 5,000 J
   
Allomancer "spends" this energy from metal burn
   Metal energy → Kinetic energy
   (Investiture conversion)

Therefore: Metal reserves ARE finite
            Burn rate = Energy output rate
```

### Why Conservation Matters for Game Design

```
UNITY IMPLEMENTATION NOTES:

1. Coin Push:
   - Apply force to coin: F = PushStrength × AnchorQuality
   - Apply equal opposite force to player
   - Use rigidbody.AddForce()
   
2. Heavy Metal Anchor:
   - Anchor quality based on mass
   - Q = min(1, anchorMass / threshold)
   - Higher Q = more effective push
   
3. Range Limiting:
   - F = S × A / r² (inverse square)
   - Clamp to visibility range
   - Beyond range: Force = 0
```

---

## Appendix E: WoB Quick Reference

### Steel/Iron Allomancy

| Question | Answer |
|----------|--------|
| Force formula? | Inverse square, like gravity |
| Range limit? | ~100 paces, up to 300 with upgrades |
| Conservation? | Yes, momentum AND energy |
| Anchor mass matters? | Yes, heavier = better anchor |
| Can push Shardblades? | Very hard, requires immense power |

### Feruchemy

| Question | Answer |
|----------|--------|
| Storage ratio? | 1:1 (one out, one in) |
| Compounding? | ~10x amplification |
| Can store pewter strength? | Yes, but easier to compound |
| Iron Feruchemy effect? | Inertial mass only (not gravitational) |
| Health storage? | Via multiple metals |

### Compounding

| Question | Answer |
|----------|--------|
| Basic mechanism? | Feruchemy → Allomancy → Feruchemy |
| Amplification? | ~10x per cycle |
| Diminishing returns? | Yes, eventual limits |
| Dangers? | Pewter-drag-like effects |
| Iron compounder? | Called "Deaders" - die from weight |

### Atium (Retconned)

| Question | Answer |
|----------|--------|
| Era 1 atium? | Actually Nalatium (At+El alloy) |
| Pure Atium burnable? | Yes, by anyone (WoB) |
| Pure Atium effect? | Enhanced futuresight + mind |
| Malatium? | Gold + Atium alloy |
| Nalatium effect? | Same as Era 1 atium |

### Temporal Metals

| Question | Answer |
|----------|--------|
| Time bubbles real? | Yes (Cadmium/Bendalloy) |
| Bubble size? | ~10 feet diameter |
| Time ratio? | ~10:1 outside:inside |
| Blue shift? | Investiture blocks it |
| Uses? | Combat, healing, siege |

---

## 23. Time Bubble Physics (Detailed)

### Bubble Properties

```
Bendalloy (Slider) Bubble:
├── Effect: Time speeds UP inside
├── Compression Factor: ~8:1 (2 min = 15 sec real time)
├── Size: 5-15 feet diameter
├── Cost: Very expensive metal
└── Duralumin Combo: Extreme speed-up, crystal-like bubble

Cadmium (Pulser) Bubble:
├── Effect: Time slows DOWN inside
├── Size: ~Room-sized (larger than Bendalloy)
├── Anchoring: Stays with moving objects
└── Use: Extended survival, FTL possibility
```

### Time Dilation Math

```
Let:
  t_inside = time experienced inside bubble
  t_outside = time experienced outside
  CF = Compression Factor (dilation ratio)

Bendalloy (Speed Up):
  t_inside = t_outside × CF
  If CF = 8: 1 second outside = 8 seconds inside

Cadmium (Slow Down):
  t_inside = t_outside / CF
  If CF = 8: 1 second outside = 0.125 seconds inside
```

### Bubble Nesting

```
Multiple bubbles CAN stack:
├── Bendalloy × Bendalloy = Multiplied speed
├── Cadmium × Cadmium = Multiplied slowness
├── Bendalloy + Cadmium = Cancel out (net 1:1)
└── Duralumin/Nicrosil = Extreme amplification

Example (Wayne's mechanics):
  Normal bubble: 8x speed inside
  With Duralumin: 80x+ speed inside
```

### Jostling Effect

```
Objects crossing bubble boundaries get deflected:
├── Light: Refracted/shifted (but not dangerous)
├── Physical objects: Wildly off-course
├── Reason: Different parts of object at different time rates
└── Combat use: Pop bubbles with thrown objects

Brandon on Jostling:
"Objects entering or exiting the bubble are thrown 
wildly off-course, most likely because different parts 
of the object are moving at different speeds during the transition."
```

### Energy Conservation Workaround

```
Problem: Kinetic energy changes when time dilation applies
Solution: Spiritual Realm energy transfer

Without Spiritual transfer:
  Light would cause dangerous radiation (redshift/blueshift)
  Objects would gain/lose energy crossing bubbles
  
With Spiritual transfer:
  Energy is absorbed/released by Spiritual Realm
  No dangerous radiation
  Conservation appears maintained through Investiture
```

### FTL Possibilities

```
Theoretical FTL method using time bubbles:
1. Create Cadmium bubble (slows time inside)
2. Bubble anchors to planet's movement
3. Planet moves through space
4. Inside: Very little time passes
5. Outside: Light-years of distance covered
6. Exit bubble: You aged less, traveled farther

This requires massive Investiture and is theoretically possible
```

### Time Bubble Combat Applications

```
Inside Bubble Advantages:
├── Perceive outside as frozen (slow-mo)
├── Prepare attacks while opponent waits
├── Escape from overwhelming situations
├── Extended healing time
└── Dramatic dialogue time

Dangers:
├── Vulnerability when exiting
├── Bubble pop = sudden time shift
├── Cannot use Allomancy while exiting (interference)
└── Strategic planning required
```

### Bubble Physics Summary Table

| Property | Bendalloy | Cadmium |
|----------|-----------|---------|
| Time Effect | Faster | Slower |
| Typical Size | 5-15 ft | Room-sized |
| Cost | Very High | High |
| Nesting | Multiplies | Multiplies |
| Cancel | With Cadmium | With Bendalloy |

---

## 24. Atium Physics (Era 1 vs Era 2)

### The Atium Electrum Retcon

```
Era 1 Canon:
├── Atium = Pure metal (11th metal)
├── Grants future sight
├── Atium Mistings exist
└── Used by TLR, Vin, Kelsier

Retconned Canon (WoB 2021):
├── Era 1 "atium" = Nalatium (Atium-Electrum alloy)
├── Pure Atium = God metal, burnable by ANYONE
├── Electrum Mistings = What atium mistings actually were
└── Alloys of atium have various temporal effects
```

### Pure Atium Properties (Era 2+)

```
Pure Atium (God Metal):
├── Burnable by: Everyone (not just Mistborn)
├── Effect: Enhanced futuresight + mind enhancement
├── Hemalurgy: Steals spiritual attributes
├── FTL: Allows glimpses of spiritual realm
└── Brandon: "It's very slightly alloyed with electrum"

Key Quote:
"This is accurate, yes. You could, by the way, just continue 
to call it atium. That's what they think atium is in-world."
```

### Nalatium vs Pure Atium

```
Nalatium (Era 1 atium):
├── Composition: Atium + Electrum alloy
├── In-world name: "Atium"
├── Effect: Future sight (like Era 1 atium)
├── Discovery: Found in Pits of Hathsin
└── After HoA: No longer available

Pure Atium:
├── Composition: 100% Atium (god metal)
├── Effect: Enhanced futuresight + mental processing
├── Discovery: Created from ettmetal experiments
├── Use: Marsh uses to stay alive (Lost Metal)
└── Special: Can see farther/faster than nalatium
```

### Atium-Feruchemy Interactions

```
Compounding Atium:
├── Store: Future sight in goldmind
├── Burn: Get amplified futuresight
├── Tap: Enhanced prediction abilities
└── Risk: Too much atium = temporal confusion

Atium Feruchemy (in Feruchemy table):
├── Stores: Youth/age
├── Tap: Become younger
├── Use: TLR's extended lifespan
└── Note: Used gold compounders for this
```

### Atium + Duralumin Burst

```
Calculation:
  Normal Atium: See ~10 seconds ahead
  Duralumin Atium: See 10 seconds of EXTREME detail
  Cost: 10x burn rate for 1 second burst

Combat Application:
  TLR vs Vin (Hero of Ages)
  TLR uses Duralumin+Atium = Complete future sight
  Vin counters with Electrum = Duralumin counters this
  Result: Stalemate until external factors
```

### Atium Physics Model

```
Future Sight Mechanics:
  1. Burn Atium → Connect to Spiritual Realm
  2. Spiritual Realm = Where futures exist
  3. Atium grants "Fortune" - ability to see futures
  4. Mental enhancement = Process all futures simultaneously

Mathematical Model:
  Let F = Number of possible futures
  Let P = Processing capacity (mental enhancement)
  let T = Time ahead visible
  
  F = f(T, Investiture_level, Mental_capacity)
  Atium increases P by factor of mental_enhancement_bonus
```

---

## 25. Investiture Conservation Laws

### What Cosmere Conserves

```
CONFIRMED by Brandon Sanderson:
┌─────────────────────────────────────────────────────────┐
│  PHYSICS LAW          │  COSMERE STATUS                │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Momentum conserved       │
│  Momentum             │  (Allomancy follows this)      │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Metal burn = energy     │
│  Energy               │  (But Investiture adds)        │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Time bubbles transfer   │
│  Information          │  energy through Spiritual      │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  PARTIAL - Feruchemy can       │
│  Mass                 │  change inertial mass only    │
├─────────────────────────────────────────────────────────┤
│  Thermodynamics       │  NO - Investiture is new      │
│  (Traditional)        │  form of energy/matter        │
├─────────────────────────────────────────────────────────┤
│  Causality           │  NO - Time manipulation        │
│                      │  exists (speed bubbles)        │
└─────────────────────────────────────────────────────────┘
```

### Investiture as Third Category

```
Traditional Physics:
  ├── Matter (mass)
  ├── Energy (kinetic, potential, etc.)
  └── Information

Cosmere Addition:
  └── Investiture (magical energy)
       ├── Allomancy = Investiture → Energy
       ├── Feruchemy = 1:1 Investiture storage
       ├── Hemalurgy = Investiture transfer (with loss)
       └── Compounding = Investiture multiplication

Result: Thermodynamics "bent" not broken
```

### Allomancy Energy Budget

```
Pushing coin to velocity v:
  Required Energy = ½ × m_coin × v²
  
Source of Energy:
  ├── Metal burning releases Investiture
  ├── Investiture → Kinetic Energy conversion
  └── Conversion rate ∝ metal burn rate
  
Example:
  Coin (0.01 kg) to 1000 m/s
  KE = ½ × 0.01 × 1000000 = 5,000 J
  
If burn rate = 5 J/s:
  Duration = 1000 seconds of push
  
This matches text: Metal burning depletes during sustained pushes
```

### Hemalurgy Energy Loss

```
Spike Creation:
  1. Kill person, extract spiritweb fragment
  2. Transfer to metal spike (SIGNIFICANT LOSS)
  3. Insert spike into recipient
  4. Staple fragment to recipient's spiritweb (MORE LOSS)

Energy Budget:
  Original power: 100%
  After spike creation: ~70-80%
  After transfer: ~50-60%
  
Result: Hemalurgic powers are WEAKER than original
        But permanent (doesn't require metal burning)
```

### Feruchemy Conservation

```
Storage (1:1 ratio):
  ├── Store attribute: 10 units → metalmind
  ├── Tap attribute: 10 units → person
  └── Net: Conservation maintained

Compounding (10x):
  ├── Store: 10 units
  ├── Burn metalmind: Get 100 units back
  ├── Re-store: 100 units in new metalmind
  ├── Burn again: Get 1000 units
  └── Violation: NOT conserving - AMPLIFYING
  
How is this possible?
  └── External Investiture source (Spiritual Realm)
```

### Time Bubble Energy Math

```
Bendalloy Bubble (~8x speed):

Outside observer sees:
  Object enters bubble (slow time)
  Light from bubble is blueshifted
  Energy appears to increase

Inside observer sees:
  Object exits bubble (fast time)
  Light from outside is redshifted
  Energy appears to decrease

Solution: Energy transfers to/from Spiritual Realm

Net Result: No dangerous radiation, conservation maintained
```

---

## 26. Practical Game Physics Implementation

### Unity Force Model

```
Allomantic Force = Mental_Choice × Metal_Burn_Rate × Anchor_Quality / Distance²

Implementation:
```csharp
public float CalculatePushForce(
    float allomancerStrength,  // Player's mental push
    float metalBurnRate,        // Current burn rate
    float anchorMass,            // Target metal mass
    float distance)              // Distance to target
{
    float anchorQuality = Mathf.Clamp01(anchorMass / MIN_MASS);
    float baseForce = allomancerStrength * metalBurnRate;
    float distanceFactor = 1f / (distance * distance);
    
    return baseForce * anchorQuality * distanceFactor;
}
```
```

### Anchor Quality Table

```
Anchor Type          │ Mass    │ Quality │ Notes
─────────────────────┼─────────┼─────────┼────────────────────
Coin (in flight)     │ 2-5g    │ 0.01    │ Very poor anchor
Coin (vs wall)       │ 2-5g    │ 0.5     │ Better when braced
Ingot (5 lb)         │ 2.3 kg  │ 0.7     │ Good anchor
Heavy Object (50 lb) │ 23 kg   │ 0.9     │ Excellent anchor
Building/Ground      │ 1000+kg │ 1.0     │ Perfect anchor
```

### Pewter Enhancement in Code

```
```csharp
public class PewterEffects
{
    public float GetStrengthBonus(float pewterLevel, bool isFlared)
    {
        float baseMultiplier = pewterLevel; // 1.0 - 2.0+
        float flareBonus = isFlared ? 1.5f : 1.0f;
        
        return baseMultiplier * flareBonus;
    }
    
    public float GetSpeedBonus(float pewterLevel)
    {
        // Pewter adds ~20% speed per level above 1
        return 1f + (pewterLevel - 1f) * 0.2f;
    }
    
    public float GetDamageReduction(float pewterLevel, bool isFlared)
    {
        float baseReduction = pewterLevel; // More durable
        float flareBonus = isFlared ? 1.5f : 1.0f;
        
        return baseReduction * flareBonus;
    }
}
```
```

### Time Bubble Implementation

```
```csharp
public class SpeedBubble : MonoBehaviour
{
    public float compressionFactor = 8f; // 8:1 time ratio
    public float radius = 3f;
    
    void OnTriggerEnter(Collider other)
    {
        SpeedAffected entity = other.GetComponent<SpeedAffected>();
        if (entity != null)
        {
            entity.EnterBubble(compressionFactor);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        SpeedAffected entity = other.GetComponent<SpeedAffected>();
        if (entity != null)
        {
            entity.ExitBubble();
        }
    }
}

public class SpeedAffected : MonoBehaviour
{
    private float originalTimeScale = 1f;
    
    public void EnterBubble(float dilationRatio)
    {
        Time.timeScale *= dilationRatio;
        Debug.Log($"Inside bubble: Time × {dilationRatio}");
    }
    
    public void ExitBubble()
    {
        Time.timeScale = 1f;
        Debug.Log("Exited bubble: Normal time");
    }
}
```
```

### Feruchemy Storage Model

```
```csharp
public class FeruchemyStorage
{
    public float Store(float value, float duration, float rate)
    {
        // 1:1 conservation model
        float stored = value * duration * rate;
        return stored;
    }
    
    public float Tap(float stored, float rate)
    {
        // Can tap faster than stored (amplification)
        float tapped = stored * rate;
        return tapped;
    }
    
    public float Compound(float stored, int cycles, float factor = 10f)
    {
        // Feruchemy + Allomancy loop
        for (int i = 0; i < cycles; i++)
        {
            stored *= factor; // 10x per cycle
        }
        return stored;
    }
}
```
```

### Complete Allomancy System Architecture

```
Player GameObject
├── Allomancer (base metal management)
├── MetalReserveManager (metal storage)
├── AllomancyInputController (input handling)
├── AllomanticCalculators (physics math)
│
├── Physical Allomancy
│   ├── SteelPush
│   ├── IronPull
│   ├── TinEnhance
│   └── PewterBurn / PewterManager
│
├── Mental Allomancy
│   ├── ZincRiot
│   ├── BrassSoothe
│   ├── CopperCloud
│   └── BronzeDetect
│
├── Temporal Allomancy
│   ├── GoldPast
│   ├── ElectrumFuture
│   ├── CadmiumBubble
│   └── BendalloyBubble
│
├── Enhancement Allomancy
│   ├── AluminumPurge
│   ├── DuraluminBurst
│   ├── ChromiumErasing
│   └── NicrosilCompounding
│
├── God Metals
│   ├── AtiumController
│   ├── LerasiumController
│   ├── HarmoniumController
│   └── MalatiumReveal
│
└── Integration
    ├── MistbornAbilityManager
    ├── FeruchemySystem
    └── HemalurgySystem
```

---

## Appendix F: Brandon Sanderson WoB Compilation

### Steel/Iron (Physical)

| Question | Answer | Source |
|----------|--------|--------|
| Force formula? | Inverse square, conservation of momentum | 2015 WoT Q&A |
| Mass matter? | Yes, anchor mass affects power | 2015 17th Shard |
| Push Shardblades? | Very hard, requires immense power | In-book |
| Range? | ~100 paces, extends with upgrades | AoL |
| Momentum conserved? | YES, intentional | Shadows of Self |

### Pewter (Physical Enhancement)

| Question | Answer | Source |
|----------|--------|--------|
| Enhancement factors? | Strength, speed, healing, balance | In-book |
| Speed increase? | Minor (~10-20%) | In-book |
| Strength increase? | Major (~100%) | In-book |
| Healing increase? | Moderate (~50%) | In-book |
| Can store in Feruchemy? | Yes, but compounding easier | 2011 AoL Q&A |

### Feruchemy (Storage)

| Question | Answer | Source |
|----------|--------|--------|
| Storage ratio? | 1:1 for most attributes | In-book |
| Compounding? | ~10x amplification | In-book |
| Iron stores what? | Inertial mass | 2013 17th Shard |
| Steel stores what? | Speed | In-book |
| Can store Pewter strength? | Yes, magical attribute | 2011 WoB |

### Time Bubbles (Temporal)

| Question | Answer | Source |
|----------|--------|--------|
| Effect on time? | Speed up (Bendalloy) or slow down (Cadmium) | In-book |
| Size? | 5-15 ft (Bendalloy), room-sized (Cadmium) | In-book |
| Conservation? | Energy transfers via Spiritual Realm | 2015 WoT Q&A |
| Stack? | Yes, effects multiply | In-book |
| Cancel? | Bendalloy + Cadmium = neutral | In-book |

### Atium (Retconned)

| Question | Answer | Source |
|----------|--------|--------|
| Era 1 atium? | Actually Nalatium (At+El alloy) | 2021 WoB |
| Pure Atium burnable? | Yes, by anyone | 2021 WoB |
| Pure Atium effect? | Enhanced futuresight + mind | 2021 WoB |
| Malatium? | Gold + Atium alloy | In-book |
| Nalatium discovery? | Pits of Hathsin | HoA |

### God Metals (Era 2)

| Metal | Effect | Notes |
|-------|--------|-------|
| Lerasium | Grants Mistborn | Anyone who burns it |
| Atium | Enhanced futuresight | Pure form, not alloy |
| Harmonium | Steam explosion | Highly unstable |
| Ettmetal | Reacts with water/air | Era 2 technology |

---

## Appendix G: Physics Quick Reference Card

```
╔═══════════════════════════════════════════════════════════════════╗
║                    MISTBORN PHYSICS QUICK REFERENCE                ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  STEEL/Iron Push/Pull                                              ║
║  ├─ F = S × A / r² (Force, Strength, Anchor, Distance)           ║
║  ├─ Momentum CONSERVED ✓                                          ║
║  └─ Anchor mass affects effectiveness                              ║
║                                                                   ║
║  FERUCHEMY                                                         ║
║  ├─ 1:1 storage ratio                                              ║
║  ├─ Compounding: ~10x per cycle                                    ║
║  ├─ Iron: Inertial mass only (not gravitational)                   ║
║  └─ Steel: Physical speed                                          ║
║                                                                   ║
║  PEWTER                                                            ║
║  ├─ Strength: 2x base                                             ║
║  ├─ Speed: 1.2x base                                              ║
║  ├─ Flaring: 1.5x normal                                           ║
║  └─ Drag: Extended use reduces effectiveness                       ║
║                                                                   ║
║  TIME BUBBLES                                                      ║
║  ├─ Bendalloy: Time speeds up (8:1 typical)                       ║
║  ├─ Cadmium: Time slows down                                      ║
║  ├─ Stack: Effects multiply                                       ║
║  └─ Energy: Transfers via Spiritual Realm                          ║
║                                                                   ║
║  ATIUM (RETCONNED)                                                 ║
║  ├─ Era 1: Actually Nalatium (At+El alloy)                        ║
║  ├─ Pure Atium: God metal, burnable by anyone                     ║
║  └─ Effect: Enhanced futuresight + mind enhancement              ║
║                                                                   ║
║  HEMALURGY                                                         ║
║  ├─ Power loss in transfer (~50-60% efficiency)                    ║
║  ├─ Spikes: Permanent but weaker than original                    ║
║  └─ Koloss: 4 iron spikes = strength + growth                      ║
║                                                                   ║
║  INVESTITURE                                                       ║
║  ├─ Third category: Matter + Energy + Investiture                 ║
║  ├─ Can convert to/from Energy                                     ║
║  └─ Allows "bending" thermodynamics                                ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## Appendix H: Mathematical Formulas Summary

### Core Allomancy Equations

```
1. Push/Pull Force:
   F = A × S × Q / r²
   
   Where:
   - F = Force applied
   - A = Allomantic strength coefficient
   - S = Mental push strength (chosen by allomancer)
   - Q = Anchor quality (0-1, based on mass/connection)
   - r = Distance to anchor

2. Coin Velocity:
   v = √(2 × F × d / m)
   
   Where:
   - v = Final velocity
   - F = Applied force
   - d = Push distance
   - m = Coin mass

3. Allomancer Recoil:
   a_recoil = F / m_allomancer
   
   Conservation of momentum: m_coin × v_coin = m_allo × v_allo
```

### Feruchemy Equations

```
1. Storage (1:1):
   Stored = BaseValue × Duration × Rate

2. Tapping:
   Tapped = Stored × TapMultiplier
   
3. Compounding (×10):
   After n cycles: Original × 10^n

4. Iron Mass Effect:
   mi_new = mi_original × FeruchemyMultiplier
   Terminal velocity unchanged (mg unchanged)
```

### Time Bubble Equations

```
1. Time Dilation:
   t_inside = t_outside × CompressionFactor

2. Bendalloy:
   t_inside = t_outside × 8 (typical)

3. Cadmium:
   t_inside = t_outside / 8 (typical)

4. Nested Bubbles:
   CF_total = CF_1 × CF_2 × ... × CF_n
```

### Pewter Enhancement

```
1. Strength:
   S_pewter = S_base × PewterLevel × (isFlared ? 1.5 : 1)

2. Speed:
   v_pewter = v_base × (1 + 0.2 × (PewterLevel - 1))

3. Drag Penalty:
   If burnTime > dragThreshold:
       Penalty = 1 - ((burnTime - threshold) / maxDuration)
```

### Compounding Growth

```
Cycles │ Multiplier │ Practical Application
───────┼────────────┼────────────────────────────────
0      │ 1×         │ Normal Feruchemy
1      │ 10×        │ Powerful Feruchemist
2      │ 100×       │ Twinborn Compounder
3      │ 1,000×     │ Lord Ruler tier
4      │ 10,000×    │ Dangerous territory
5      │ 100,000×   │ Iron compounder "Deaders"
```

---

*Document compiled from r/Mistborn, r/Cosmere, and 17th Shard community analysis*
*Brandon Sanderson's official WoB from Arcanum.coppermind.net*
*Cosmere Era: 1022-1025 FE*
*Last Updated: March 2026*
*Version: 2.0 - Includes Atium Retcon and Time Bubble Physics*
