# The Ashwalker Physics & Mathematics Handbook

*A comprehensive analysis of the physics and mathematics of the original author's Ashwalker universe*

---

## Table of Contents

1. [Physics Fundamentals](#1-physics-fundamentals)
   - Newton's Three Laws, Gravity, Momentum & Impulse, Friction,
     Projectile Motion, Circular Motion, Work & Energy,
     Metallurgy-Specific Rules, Quick Reference, Constants
2. [Steel & Iron: Push/Pull Force Functions](#2-steel--iron-pushpull-force-functions)
3. [Coin Velocity Functions](#3-coin-velocity-functions)
4. [Storecraft Storage Functions](#4-storecraft-storage-functions)
5. [Compounding Exponential Functions](#5-compounding-exponential-functions)
6. [Speed Compounding Functions](#6-speed-compounding-functions)
7. [Iron Compounding Mass Functions](#7-iron-compounding-mass-functions)
8. [Pewter Strength Functions](#8-pewter-strength-functions)
9. [Time Bubble Functions](#9-time-bubble-functions)
10. [Graphical Analysis](#10-graphical-analysis)
11. [Diminishing Returns Functions](#11-diminishing-returns-functions)
12. [Practical Applications](#12-practical-applications)
13. [Unity World Scale & Unit Conversions](#13-unity-world-scale--unit-conversions)
14. [Metal Ingestion, Capacity & Toxicology](#14-metal-ingestion-capacity--toxicology)
15. [Arc Trajectories — Diagonal Push/Pull Physics](#15-arc-trajectories--diagonal-pushpull-physics)

---

## 1. Physics Fundamentals

*Based on Newton's Laws of Motion and their Metallurgic applications.*

---

### Newton's First Law — Law of Inertia

> "An object at rest stays at rest. An object in motion stays in motion at
> constant velocity — unless acted on by a net force."

```
If F_net = 0, then v = constant (including v = 0)
```

**Ashwalker Application:**
A Launcher's coin keeps flying in a straight line at constant speed unless
gravity, air resistance, or another Metallurgic force acts on it. A Ashwalker
flying through the mists continues moving until they Push or Pull again.

---

### Newton's Second Law — Law of Acceleration

> "The acceleration of an object equals the net force on it divided by its mass."

```
F = m × a
a = F / m
F_net = sum of all forces acting on the object

Units:
  F = Newtons (N)
  m = kilograms (kg)
  a = meters per second squared (m/s²)
```

**Ashwalker Application:**
When a Launcher Pushes a coin (mass ≈ 5 g = 0.005 kg) with 100 N of
Metallurgic force:
```
  a = F / m = 100 / 0.005 = 20,000 m/s²   — extremely fast!
```
The same Push applied to a steel door produces far less acceleration.

---

### Newton's Third Law — Law of Action & Reaction

> "For every action, there is an equal and opposite reaction."

```
F_on_B = -F_on_A
(equal in magnitude, opposite in direction)
```

**Ashwalker Application — *this is the core of Metallurgy*:**
When a Launcher Pushes a coin away, the Push also propels the Ashwalker in
the opposite direction with equal force. Anchoring: if the metal object is
too heavy to move, all the force returns to launch the Ashwalker instead.
If both are free to move, each accelerates inversely proportional to its mass.

---

### Gravity

```
F_gravity = m × g        (g = 9.8 m/s² on Ashara / Earth)

Weight vs. Mass:
  Mass (kg)   — amount of matter; never changes
  Weight (N)  — gravitational pull; F_gravity = m × g

Free-fall (no air resistance):
  d = ½ × g × t²
  v = g × t
```

**Ashwalker Application:**
A Ashwalker Pushing downward on a coin anchored to the ground effectively
reduces their felt gravity — they can hover or slow a fall. A Hauler
(Iron Sparkblood) Pulling upward on a metal anchor above them can fly.

```
Example — Ashwalker after 2 s of free fall:
  v = 9.8 × 2       = 19.6 m/s downward
  d = ½ × 9.8 × 4   = 19.6 m fallen
```

---

### Momentum & Impulse

```
Momentum:
  p = m × v          (units: kg·m/s)

Conservation of Momentum (no external forces):
  m₁v₁ + m₂v₂ = m₁v₁' + m₂v₂'

Impulse (change in momentum):
  J = F × t = Δp = m × Δv
```

**Ashwalker Application:**
A coin fired by a Launcher carries momentum p = m × v. When it hits a
target that momentum transfers. A heavier coin at lower speed carries the
same momentum as a lighter coin at higher speed — a tactical trade-off.

```
Short powerful Push  = high force, short time   (burst move)
Long sustained Push  = lower force, longer time  (same total impulse)
```

---

### Normal Force & Weight

```
Flat ground (no vertical acceleration):
  N = m × g

Incline at angle θ from horizontal:
  N             = m × g × cos(θ)   (perpendicular to surface)
  F_along_slope = m × g × sin(θ)   (parallel to surface)
```

**Ashwalker Application:**
When a Ashwalker Pushes off a wall at an angle, only the component
perpendicular to the wall actually propels them away. The angle matters.
```
  Pushes away (perpendicular):  F × cos(θ)
  Slides along wall (parallel): F × sin(θ)
```

---

### Friction

```
Static friction (object not yet moving):
  F_static_max = μ_s × N

Kinetic friction (object sliding):
  F_kinetic = μ_k × N

μ (coefficient of friction, unitless):
  Ice:              ~0.03
  Wood on wood:     ~0.3
  Rubber on asphalt: ~0.7
```

**Ashwalker Application:**
When a Ashwalker slides coins across a stone floor before Pushing them,
friction slows the coin before launch. A coin without enough speed may
not generate the expected trajectory force.

---

### Projectile Motion — Coins & Launched Objects

```
Horizontal (no drag):
  x   = v_x × t
  v_x = v × cos(θ)

Vertical:
  y         = v_y × t − ½ × g × t²
  v_y       = v × sin(θ)
  v_y_final = v_y − g × t

Time of flight (same launch and landing height):
  t = (2 × v × sin(θ)) / g

Maximum range:
  R = (v² × sin(2θ)) / g
  Best angle for max range = 45°
```

**Ashwalker Application:**
A coin launched at an upward angle follows a parabolic arc. A Launcher
must account for gravity drop over distance. A horizontal Push is fine
for close range but loses accuracy at distance without aiming upward.

---

### Circular / Rotational Motion — Swinging & Arcs

```
Centripetal acceleration:
  a_c = v² / r

Centripetal force (to maintain circular path):
  F_c = m × v² / r
```

**Ashwalker Application:**
A Ashwalker swinging around a metal anchor (lamppost, steel cable) in an
arc experiences centripetal force pulling toward the anchor. The faster
the swing, the more Pull force is needed. If the Pull is too weak, they
fly off in a straight line tangent to the circle.

---

### Work & Energy

```
Work:
  W = F × d × cos(θ)     (θ = angle between force and motion)
  Units: Joules (J)

Kinetic Energy:
  KE = ½ × m × v²

Gravitational Potential Energy:
  PE = m × g × h

Conservation of Energy (no friction):
  KE + PE = constant
  ½mv₁² + mgh₁ = ½mv₂² + mgh₂

Speed after falling height h (from rest):
  v = √(2 × g × h)
```

**Ashwalker Application:**
```
A Push doing 500 J of work on a 0.005 kg coin:
  v = √(2 × KE / m) = √(2 × 500 / 0.005) ≈ 447 m/s
  (faster than a bullet — Metallurgy is terrifying)
```

---

### Center of Mass — 3D Objects

The center of mass determines where an Metallurgic push/pull targets on an
object, and how force distributes when pushing composite objects (armor, doors,
vehicles with metal components).

**Continuous mass distribution (triple integral):**
```
x_cm = (1/M) × ∫∫∫ x × ρ(x,y,z) dV
y_cm = (1/M) × ∫∫∫ y × ρ(x,y,z) dV
z_cm = (1/M) × ∫∫∫ z × ρ(x,y,z) dV

M = ∫∫∫ ρ(x,y,z) dV      (total mass)

Where ρ(x,y,z) = density at point (x,y,z)
```

**Volume element by coordinate system (choose by shape):**
```
Cartesian:    dV = dx dy dz             (boxes, walls, armor plates)
Cylindrical:  dV = r dr dθ dz           (coins, metal pipes, columns)
Spherical:    dV = r² sin(φ) dr dφ dθ   (metal spheres, oraculum beads)
```

**Uniform density shortcut:** If ρ is constant, center of mass = geometric
centroid. `ρ` cancels from numerator and denominator.

**Discrete / composite objects (no calculus needed):**
```
r_cm = Σ(m_i × r_i) / Σ(m_i)

Example — Sentinel with steel spike:
  Body:  m₁ = 80 kg, center at (0, 1.0, 0)
  Spike: m₂ = 0.5 kg, center at (0, 1.7, 0.1)
  r_cm = (80×(0,1.0,0) + 0.5×(0,1.7,0.1)) / 80.5
       ≈ (0, 1.004, 0.0006)
  The spike barely shifts the center — a push targets the body center.

Example — Door with iron hinges:
  Wood: m₁ = 25 kg, center at (0.5, 1.0, 0)
  Hinge: m₂ = 2 kg, center at (0, 1.0, 0)
  r_cm of metal only = hinge position → push/pull targets the hinge edge,
  which torques the door open (this is how Darius opens locked doors).
```

**Moments** (the numerator of each coordinate):
```
M_yz = ∫∫∫ x × ρ dV    → moment about YZ-plane → gives x_cm
M_xz = ∫∫∫ y × ρ dV    → moment about XZ-plane → gives y_cm
M_xy = ∫∫∫ z × ρ dV    → moment about XY-plane → gives z_cm
```

**Center of mass can be outside the physical body:**
For hollow or concave shapes (rings, hooks, horseshoes, bent metal bars), the
center of mass lies in empty space. Metallurgic rules still apply:

```
1. Blue line targets the mathematical center of mass, even if it's in air.
   A hollow ring's blue line points to the center of the hole.

2. Force acts at that point. Pushing the center of a hollow hoop pushes
   the entire hoop away — the force "grips" the mass distribution, not
   a physical surface.

3. Off-center pushes create torque:
   τ = r × F  (cross product of lever arm × force)

   Skilled metallurgists exploit this:
   - Push one end of a bar + pull the other → spin
   - Push off-center on a door → torque it open around hinges
   - Target a specific rivet instead of the whole plate

4. Subconscious correction: metallurgists instinctively adjust their
   push/pull origin to achieve desired motion — adapting to the
   object's mass distribution without consciously computing integrals.
```

**Unity note:** `Rigidbody.centerOfMass` handles this automatically for physics
objects. For push/pull targeting, the force acts toward `targetRb.worldCenterOfMass`,
which is the physics engine's computed center of mass. Composite colliders with
different densities shift this point automatically. For hollow objects, Unity
correctly places the center of mass in empty space if the collider geometry
dictates it.

---

### Metallurgy-Specific Rules

**Push / Pull Force Balance:**
```
Net force on Ashwalker = −(net force on metal object)

If m_object >> m_Ashwalker:  almost all force goes to Ashwalker
If m_Ashwalker >> m_object:  almost all force goes to the object
If m_Ashwalker ≈ m_object:   both accelerate equally in opposite directions

  a_ashwalker = F / m_ashwalker
  a_object   = F / m_object
```

**Line-of-Sight Rule:**
Metallurgic Pushes and Pulls act along the straight line between the
Ashwalker and the metal object. The angle of Push equals the angle of
that line.

**Anchor Types:**
```
Fixed anchor (bolted to ground):
  All force returns to Ashwalker → maximum self-propulsion

Free object (coin, bead):
  Force splits by mass ratio → launches object + Ashwalker recoils

Another person / creature:
  Force splits — both move (useful for combat throws)
```

**Stacking Pushes (multiple metals simultaneously):**
```
F_total = F₁ + F₂ + F₃ + ...   (vector sum)

Vector components:
  F_x     = F × cos(θ)
  F_y     = F × sin(θ)
  |F_total| = √(F_x² + F_y²)
  angle   = arctan(F_y / F_x)
```

---

### Quick Reference — Common Formulas

```
F = m × a                        Newton's 2nd Law
F_gravity = m × g  (g = 9.8)    Weight
p = m × v                        Momentum
J = F × t = Δp                   Impulse
KE = ½ × m × v²                 Kinetic Energy
PE = m × g × h                   Potential Energy
v = √(2×g×h)                     Speed after falling height h
F_friction = μ × N               Friction
F_centripetal = m×v²/r           Circular motion
R = v²×sin(2θ)/g                 Projectile range
F_A = −F_B                       Newton's 3rd Law
```

---

### Useful Constants

```
g  = 9.8 m/s²                        Gravity on Ashara / Earth
G  = 6.674 × 10⁻¹¹ N·m²/kg²         Universal gravitational constant
1 N = 1 kg·m/s²
1 J = 1 N·m = 1 kg·m²/s²

Typical coin mass (crown/half-crown): ~5–10 g  (0.005–0.01 kg)
Average human mass:                      ~70 kg
Iron Sentinel spike:                  ~0.5–1 kg (estimate)
```

---

## 1b. Lore-Canon Steel/Iron Rules (Confirmed Mechanics)

*Sourced from the novels, the original author's author notes, and
community physics analysis (reddit.com/r/Ashwalker, 17thshard.com).*

---

### Center of Self — Where the Push Originates

> "The push comes from the metallurgist's *center of self* — not their center of mass."
> — Coppermind / confirmed community consensus

```
Default behavior:
  Origin  = metallurgist's CHEST  (where you'd point saying "who, me?")
  Target  = metal object's CENTER OF MASS

NOT:
  Origin  = metallurgist's hips / belly button (center of mass)
```

The blue lines visible to Metallurgists always originate from the chest, not the
hips. This is a subtle but real distinction — a push aimed straight ahead pulls
the metallurgist's chest forward, which can torque the body if the legs are in a
different position.

**Advanced skill — shifting the center of self:**
Very skilled (or Savant-level) Metallurgists can move the origin point across
their body — e.g. originating a pull from the hands to catch a coin mid-air.
This is rare and difficult; most Metallurgists never achieve it.

**Advanced skill — targeting specific parts of an object:**
A skilled Metallurgist can push or pull on sub-sections of a metal object rather
than its center of mass:
- Darius spins cage bars by pushing one end and pulling the other simultaneously
- Wax can identify a bullet is multi-piece and push on only one piece
- This requires intense focus and Metallurgic practice — default is always the
  object's center of mass

**Defensive note — Hauler breastplates:**
Haulers sometimes wear heavy wooden breastplates. An Iron Pull draws all
incoming coins toward the chest origin — so a wooden chest plate absorbs the
impact that would otherwise hit flesh.

---

### Force Is Proportional to User Mass

The harder a Ashwalker pushes, the more force is generated — but the maximum
force is bounded by their own body weight. A heavier user produces stronger
pushes and pulls. This is why Storecrafted Iron (Crashers who store/tap weight)
dramatically amplifies Metallurgic force.

```
F_max ≈ proportional to m_metallurgist

Crasher (tapping Iron weight):
  m_effective = m_base × tapping_multiplier
  F_effective = F_base × tapping_multiplier
```

This is already captured by `F = A × m₁ × m₂ / r²` — m₁ (metallurgist mass)
scales the force directly. Burning Pewter increases physical mass (m₁ grows),
making pushes stronger as a side effect.

---

### Weight vs. Anchor Rule

```
If  m_object < m_metallurgist:   object moves  (coin, small metal)
If  m_object > m_metallurgist:   metallurgist moves  (steel beam, anchored floor)
If  m_object ≈ m_metallurgist:   both move proportionally

Anchored object (bolted, embedded, structural):
  treated as m_object → ∞
  all force returns to metallurgist → maximum self-propulsion
```

---

### Confirmed Coin Velocities

A standard Sparkblood Launcher can accelerate a coin past the speed of sound:

```
Speed of sound on Ashara ≈ 343 m/s

Book-consistent estimate (A = 1,500):
  F  = 1500 × 70 × 0.01 / 25 = 42 N
  v  = √(2 × 42 × 50 / 0.01) ≈ 648 m/s   (mach ~1.9)

Conservative lower bound (A = 1,000):
  v  ≈ 490 m/s   (mach ~1.4)

Upper bound (Ember, A = 35,316):
  v  ≈ 2,377 m/s  (mach ~6.9)

All estimates exceed mach 1 — coins easily pierce wood and soft flesh.
A coin at 490 m/s carries:
  KE = ½ × 0.01 × 490² ≈ 1,200 J   (comparable to a rifle round)
```

---

### Distance Scaling — Linear, Not Inverse-Square

Despite the metallurgic force formula using `1/r²` in a gravitational analogy,
community physics analysis (Phantine, 17th Shard) concludes that **linear**
distance falloff matches the books better than inverse-square:

```
Linear model (used in game):
  F(r) = F_max × max(0,  1 − r / R)

  At r = 0  (point blank): F = F_max        (full force)
  At r = R/2:              F = F_max × 0.5  (half force)
  At r = R  (max range):   F = 0            (no force)
```

Why not inverse-square?
- `F ∝ 1/r²` gives near-infinite force at r → 0, which causes instability
- Ember's hover equilibrium at ~50 ft is only consistent with linear scaling
- The books describe force "diminishing heavily" but not spiking at close range
- Game developers who've tried inverse-square all report it "works terribly"

**Ember's hover point (calibration data):**
```
F_max × (1 − 50ft / 100ft) = Ember's weight
F_max × 0.5 = m × g
F_max = 2 × m × g ≈ 2 × 75 kg × 9.8 ≈ 1,470 N  (~1.5 kN)

Community consensus: peak force ≈ 1.5 kN at point blank for a typical Sparkblood.
```

### Effective Range

```
Ashwalker Adventure Game base limit:  100 paces ≈ 60 m  (used in game)
With training/upgrade:               300 paces ≈ 180 m

Larger anchors extend effective range — a coin's line is thin and faint at 10 m;
a steel beam's line is bright and visible at 60 m. This is implemented as:
  effectiveRange = maxRange × (1 + log10(anchorMass) × 0.5)

  Coin  (0.01 kg, log10 = -2): effectiveRange ≈ maxRange × 1.0 (no bonus — clamped)
  Door  (5 kg,   log10 =  0.7): effectiveRange ≈ maxRange × 1.35
  Beam  (100 kg, log10 =  2.0): effectiveRange ≈ maxRange × 2.0 (capped at 1.5×)
```

### Anchor Quality — Why Coins Against Walls Feel Different

When a coin is free in the air, it is a **weak anchor** — it moves easily,
so most of the force goes into accelerating the coin, very little returns to
the metallurgist. When the coin hits a wall, it effectively gains the mass of
the wall + ground → near-infinite effective mass → almost all force returns
to the metallurgist. This is why Ember gets thrown back hard when the coin stops.

```
Free coin (m_coin << m_metallurgist):
  metallurgist recoil ≈ F × m_coin / m_metallurgist  (tiny)
  coin acceleration = F / m_coin                  (huge)

Coin against wall (m_effective → ∞):
  metallurgist recoil = F / m_metallurgist            (full force)
  wall acceleration ≈ 0
```

This is just Newton's 3rd Law — the force was always equal and opposite.
The anchor's mass determines which body actually moves.

---

### Steel vs. Iron — Tactical Differences

| | Steel (Push) | Iron (Pull) |
|---|---|---|
| Primary use | Offense — launch coins, deflect metal | Mobility — reel self toward anchors |
| Anchor behavior | Pushes metallurgist away from anchor | Pulls metallurgist toward anchor |
| Ideal for | Fast projectiles, area denial | High-speed flight, disarming |
| Steel Bubble | Deflects incoming metal projectiles | — |

**Steel Bubble note:** Deflects most metal projectiles. Extremely fast objects
(bullet velocity and above) can sometimes pierce it due to insufficient reaction
time — the bubble is a sustained Push field, not a rigid barrier.

---

### Flaring & Duralumin

```
Flaring Steel/Iron:
  A_flared = A_base × flare_multiplier   (up to ~3× for normal flaring)
  Drains metal reserve faster

Duralumin burst (single massive pulse):
  A_duralumin = A_base × entire_reserve_multiplier
  Instantly exhausts all steel/iron reserve
  Produces a single, extremely powerful push/pull
  Used for emergency propulsion or coin-rifle shots
```

---

## 2. Steel & Iron: Push/Pull Force Functions

### Primary Force Equation

The metallurgic force follows an inverse-square law:

```
         A × m₁ × m₂
F(a) = ─────────────────
              r²

Where:
  F(a) = Metallurgic force (Newtons)
  A     = Metallurgic strength constant (varies by user/flaring)
  m₁    = Mass of metallurgist (kg)
  m₂    = Mass of metal being pushed/pulled (kg)
  r     = Distance between metallurgist and metal (m)

Domain:  r ∈ [0, max_range]
Range:    F(a) ∈ [0, F_max]
```

### Metallurgic Strength Constant (A)

Based on Ember's hover calculation:
```
Given:
  Ember mass (m₁) = 40 kg
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
  F(a) = Metallurgic force (constant, calculated at initial distance)
  d    = Distance traveled (m)
  m₂   = Mass of coin (kg)
```

### Official Coin Specifications (Shire Post Mint, the original author-licensed)

```
Ashen Dominion Clip (standard Launcher ammo):
  Material:  Copper
  Diameter:  2 cm   (radius 1 cm)
  Mass:      3 g    (0.003 kg)
  Cross-section: π × 0.01² ≈ 0.000314 m²

Ashen Dominion Crown (large denomination):
  Material:  Brass
  Diameter:  3 cm   (radius 1.5 cm)
  Mass:      15.5 g (0.0155 kg)
  Cross-section: π × 0.015² ≈ 0.000707 m²

Pennies are the standard projectile — lighter = faster for the same force.
Crowns are ~5× heavier: slower but hit much harder (higher momentum).
```

### Ember's Maximum Coin Push (Clip)

```
Given:
  A = 35,316
  m₁ = 40 kg (Ember)
  m₂ = 0.003 kg (penny)
  r  = 5 m (push distance)
  
Step 1: Calculate Force
  F(a) = A × m₁ × m₂ / r²
  F(a) = 35,316 × 40 × 0.003 / 25
  F(a) = 4,237.9 / 25
  F(a) = 169.5 N

Step 2: Calculate Acceleration
  a = F(a) / m₂
  a = 169.5 / 0.003
  a = 56,500 m/s²

Step 3: Calculate Velocity at 5 m
  v = √(2 × a × d)
  v = √(2 × 56,500 × 5)
  v = √565,000
  v ≈ 751.7 m/s

Step 4: Top Speed (extended push, 50 m)
  v_max = √(2 × 169.5 × 50 / 0.003)
  v_max ≈ 2,377 m/s
```

### Crown Push (heavier coin)

```
Given same metallurgist, same A:
  m₂ = 0.0155 kg (crown)
  F(a) = 35,316 × 40 × 0.0155 / 25 = 876.6 N
  a = 876.6 / 0.0155 = 56,555 m/s²  (similar acceleration — force scales with mass)

  v at 5 m ≈ 752 m/s  (same — force and mass cancel in v = √(2Fd/m))
  BUT: momentum = m × v = 0.0155 × 752 = 11.7 kg·m/s
       (vs Clip: 0.003 × 752 = 2.3 kg·m/s — Crown hits 5× harder)
```

Note: With the inverse-square force model (F ∝ m₂), velocity is independent
of coin mass. Heavier coins have proportionally more force applied, so they
reach similar speeds but carry far more momentum and kinetic energy.

### Conservative Estimate (Book-Consistent)

```
Using: A_conservative = 1,500 (Clip, m₂ = 0.003 kg)

F(a) = 1,500 × 40 × 0.003 / 25 = 7.2 N
v_max = √(2 × 7.2 × 50 / 0.003) = √240,000 ≈ 490 m/s

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

For a Clip (r = 0.01 m, m = 0.003 kg):
  A = π × 0.01² ≈ 0.000314 m²
  v_terminal = 0.003 × 9.81 / (0.5 × 1.225 × 0.47 × 0.000314)
  v_terminal ≈ 32.4 m/s

For a Crown (r = 0.015 m, m = 0.0155 kg):
  A = π × 0.015² ≈ 0.000707 m²
  v_terminal = 0.0155 × 9.81 / (0.5 × 1.225 × 0.47 × 0.000707)
  v_terminal ≈ 74.6 m/s

Note: These are *gravity-only* terminal velocities (free fall). Under a
sustained Metallurgic push, the effective terminal velocity is much higher
because the push force far exceeds gravity. Use CalculatePowerLimitedTerminalVelocity
for the push-limited speed.
```

---

### Community Analysis: Realistic Coin Velocities

*Source: r/Ashwalker, u/Phantine, 17th Shard community research*

**Vacuum estimate (no drag, constant force, using official Clip specs):**
```
Given:
  Ashwalker mass = 77 kg (170 lb)
  Push force = 2 × m × g = 2 × 77 × 9.81 ≈ 1,510 N
    (conservative: enough to launch self upward at 1g)
  Coin = Clip, mass = 3 g (0.003 kg), copper, 2 cm diameter
  Push range = 10 m

  Acceleration = 1,510 / 0.003 = 503,333 m/s²
  Push duration = √(2 × 10 / 503,333) = 0.0063 s  (6.3 ms)
  Final velocity = 503,333 × 0.0063 ≈ 3,171 m/s  (3.2 km/s)

  For reference: .50 cal BMG bullet = 890 m/s, M16 = 960 m/s
  A Clip in vacuum would be 3.6× faster than a sniper rifle round.
  (Crowns at 15.5 g reach similar speed but carry 5× more momentum.)
```

**Why coins aren't hypersonic in practice:**
1. **Air drag** — coins are flat, not aerodynamic. Drag coefficient for a tumbling disk ≈ 1.1 (vs 0.3 for a bullet). Terminal velocity for a coin under push ≈ 77–120 m/s depending on orientation.
2. **Power limit** — community theory: Metallurgic power (P = F × v) may be capped, so force drops as velocity increases. This prevents infinite acceleration.
3. **Distance falloff** — force decreases with distance (linear model), so the push weakens as the coin moves away.

---

### Power-Limited Force Model (Community Theory)

```
If Metallurgic power is capped at P_max:
  F(v) = P_max / v       (force drops as velocity rises)
  F(v) = min(F_max, P_max / v)   (capped at F_max at low velocity)

Acceleration under power limit:
  a(v) = F(v) / m = P_max / (m × v)

Terminal push velocity (where F(v) = drag):
  v_push_terminal: solve P_max / v = ½ρ C_d A v²
  → v_push_terminal = (2 P_max / (ρ C_d A))^(1/3)

For gameplay we use: v_max = min(v_push_terminal, v_drag_terminal)
```

This model explains why:
- Coins can't be accelerated to supersonic speed (power limit + drag)
- Heavy objects experience more force at low speed (F = P/v, v is low → F is high)
- Ember can hover (low velocity → high force available)
- Bullets are hard to deflect (high velocity → low push force available)

---

### Velocity Comparison Table

```
Projectile          │ Velocity (m/s) │ KE (J)    │ Notes
────────────────────┼────────────────┼───────────┼──────────────────────
Thrown coin          │ 15–25          │ 1–2       │ No Metallurgy
Pushed coin (game)   │ 77–120         │ 17–41     │ Drag-limited, game feel
Pushed coin (lore)   │ 200–400        │ 113–454   │ Power-limited + some drag
Crossbow bolt        │ 60–100         │ 54–150    │ Reference
Pistol bullet        │ 370            │ 515       │ 9mm Luger
Rifle bullet         │ 960            │ 1,800     │ 5.56 NATO
.50 cal BMG          │ 890            │ 18,000    │ Anti-materiel
Duralumin coin burst │ 400–800+       │ 450–1800  │ Full reserve, one shot
Sound (Mach 1)       │ 343            │ —         │ Speed of sound reference
```

**Game implementation:** Use drag-limited model (77–120 m/s) for normal pushes,
power-limited model (200–400 m/s) for flared pushes, and the full vacuum estimate
only for Duralumin bursts where the entire reserve fires in one pulse.

---

### MAG Travel Velocities (Steeljumping / Ironpulling Flight)

*Source: Ashwalker Adventure Game (canon-compliant)*

```
Mode                  │ Speed (mph) │ Speed (m/s) │ Unity (units/s) │ Notes
──────────────────────┼─────────────┼─────────────┼─────────────────┼────────────────
Standard travel       │ ~40         │ 17.9        │ 23.5            │ Galloping horse
Base steeljump        │ ~45         │ 20.1        │ 26.4            │ Normal Launcher
Max (Increased Vel.)  │ ~250        │ 111.8       │ 146.7           │ MAG stunt
Duralumin burst       │ 500+        │ 223+        │ 293+            │ Uncontrollable
Steel Storecraft tap   │ 300+        │ 134+        │ 176+            │ Twinborn (Wax)
```

**Factors affecting travel velocity:**
- Lighter metallurgists accelerate faster (Ember vs Darius)
- Fixed/heavy anchors allow greater push-off speed
- Height and angle of push determine arc vs ballistic trajectory
- Pewter assists with G-force tolerance at high speed
- Air resistance at 250+ mph is significant (~1.5 kN on a human body)

**Game cap:** `maxRecoilSpeed` limits the per-impulse velocity change.
Sustained pushing creates acceleration, so effective speed depends on
how long and how many anchors the player chains together.

---

## 4. Storecraft Storage Functions

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

### Basic Storecraft Steel

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

### Basic Iron Storecraft

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

Conclusion: Human cannot become black hole through Storecraft alone
```

### Weight vs. Mass Distinction

```
Weight formula (Storecraft):
  W = m × g × f

Where:
  W = Perceived weight
  m = Actual mass
  g = Gravitational acceleration
  f = Storecraft weight factor (0 < f < ∞)

Conservation of momentum:
  p_total = m₁v₁ + m₂v₂ (conserved in Storecraft)
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

the original author explicitly blocks this physical effect.
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

### Iron Storecraft Mass Effect

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
Force on Metallurgist
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
    │  Metallurgist              │ Metallurgist ←←
    │                         │   (m×v₁)
    
Conservation: m×v₂ = M₂×v₂ = M₁×v₁ = m×v₁

If m << M₁:  v₁ ≈ 0  (Metallurgist barely moves)
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

### Graph 4: Storecraft Storage with Diminishing Returns

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

### Steel Storecraft Special Case

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

### Metallurgic Jump Height

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

With A = 35,316, Ember can break sound barrier easily
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
  g = 9.81 m/s²              (Ashara gravity)
  c = 299,792,458 m/s        (Speed of light)
  G = 6.674 × 10⁻¹¹ N⋅m²/kg² (Gravitational constant)
  
Metallurgic Constants:
  A_vin_normal ≈ 35,316       (Ember's base metallurgic strength)
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
| F(a) | Metallurgic force |
| A | Metallurgic strength constant |
| m₁ | Mass of metallurgist |
| m₂ | Mass of metal target |
| r | Distance between metallurgist and metal |
| v | Velocity |
| S(t) | Storecraft storage function |
| P(n) | Power after n compounding cycles |
| τ | Time dilation factor |
| η | Efficiency |

---

## 13. Anchor Quality Theory

### The Problem with Coin Pushes

From the 17th Shard discussion by 8bitBob (2017):

**Key Insight**: Coins are surprisingly poor anchors for metallurgy. The mass of the target metal significantly affects push effectiveness.

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
  S = Push strength (chosen by Metallurgist)
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

From Darius: *"Either you'll be pulled toward the object, or it will be pulled toward you. If your weights are similar, then you'll both move."*

This confirms **conservation of momentum** and Newton's Third Law apply to metallurgy.

---

## 14. Inertial vs Gravitational Mass

### The Storecrafted Iron Paradox

From Satsuoni's analysis (17th Shard, 2013):

**Problem**: If Storecraft increased BOTH inertial and gravitational mass equally, tapping iron would:
1. Still fall at normal rate (a = F/m unchanged)
2. Take MORE force to jump (F = ma)
3. Result in: falling normally but can't jump = dead

**Solution**: Storecraft changes ONLY inertial mass, not gravitational mass.

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

### Storecraft's Effect

```
Normal Person:  mi = mg_passive = mg_active

Storecrafter Tapping Iron (2x density):
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

### the original author Confirmed (WoB)

*"We're trying to conserve momentum. We're trying to follow physics as best we can."*

And on weight/mass distinction:
*"Weight is the force exerted on an object by the planet equal to the mass of the object multiplied by the acceleration due to gravity."*

---

## 15. Metallurgic Charge Theory

### Gagylpus' Model (17th Shard, 2013)

**Hypothesis**: Metallurgic force is distributed like a "body force" proportional to mass (similar to gravity).

### The Four Postulates

```
1. Metallurgic force follows inverse-square law
   - Necessary: Otherwise could pull Thornspire from Ironvale
   
2. Force is a "body force" distributed by mass
   - Each bit of mass pushes/pulls every other bit
   - Reduces to center-of-mass for distant objects
   
3. Metals have unique "Metallurgic charge"
   - Similar to electric charge
   - Only interacts with charge from same metal piece
   
4. Charge is somewhat mobile in Metallurgist's body
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

When Ember and Darius push on opposite sides of a coin:
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
Metallurgy amplifies body's OWN responses
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

### Pewter + Storecraft Synergies

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

### Author on Pewter Storage

*"Metallurgic pewter strength can be stored in a metalmind, but it's probably easier to just Compound."* — WoB

This means:
1. A-Pewter can be stored (unlike normal strength)
2. It's a MAGICAL attribute, not physical
3. Compounding is more efficient than storage

---

## 17. Compounding Math - Detailed

### The Compounding Cycle

```
Basic Storecraft:     1 unit stored → 1 unit retrieved
Compounding Cycle:
   1. Store 1 unit in metalmind
   2. Burn metalmind (Metallurgy)
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
1        10x          Normal Ashwalker
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

The author confirmed: "Iron compounders... die from their own weight."
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

From the "Impossible Physics of Metallurgy" (17th Shard, 2017):

**Core Principles**:
```
1. Metallurgist defines "strength" S (mental intention)
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
   - Metallurgist feels negligible force
   
Regime 2: Coin against wall
   - Anchor quality Q ≈ large
   - F_actual = S × Q ≈ F_max
   - v = 0 (object stopped)
   - Metallurgist feels full reaction force
```

### The "Equilibrium Distance" Problem

**Problem**: Ember pushes off coin to rise to Darius's height. But:
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
*"The relationship between maximum speed and maximum force at a given strength is not based in physics whatsoever; it is an entirely arbitrary connection, made so the math matches how we've seen Metallurgy function."*

This is the "intent-based" nature of metallurgy the author built in.

---

## Appendix C: Sources

### Primary Sources (17th Shard Forums)

1. **Pagerunner (2017)** - "The Impossible Physics of Metallurgy"
   - 6 models of metallurgy
   - Mathematical addendum
   - PDF attachments
   
2. **8bitBob (2017)** - Comments on Pagerunner's thread
   - Inertial mass analysis
   - Conservation of momentum defense
   - Anchor quality theory
   
3. **Scriptorian (2013)** - "Theory on Physics of Metallurgy"
   - Inverse-square force formula
   - Storecraft mass distinction
   
4. **Gagylpus (2013)** - Body force theory
   - Metallurgic charge hypothesis
   - Center of mass mechanics
   
5. **Satsuoni (2013)** - Mass type analysis
   - Three mass types (inertial, passive/active gravitational)
   - Storecraft effects on each
   
6. **Longshot97 (2023)** - Pewter interactions
   - A-Pewter + Storecraft
   - Compounding hacks

### Secondary Sources

7. **Lee Falin (2012)** - "Science of Metallurgy: Pewter"
   - Reactor Magazine analysis
   - Biochemical mechanisms
   
8. **Jack Lonstein (2018)** - "Physics of the the greater universe"
   - Medium article
   - Comprehensive overview

### the original author WoB Sources

- Arcanum.coppermind.net database
- Twitter Q&A sessions
- 17th Shard forum Q&As
- alloy of Law Q&A (2011)

### Key Quotes Used

1. *"We're trying to conserve momentum. We're trying to follow physics as best we can."*
2. *"Metallurgic pewter strength can be stored in a metalmind, but it's probably easier to just Compound."*
3. *"Iron compounders... die from their own weight."*
4. *"The mind is such a big part of what makes us who we are."*

---

## Appendix D: Glossary

```
A-Pewter:       Metallurgic Pewter (the metal, burning it)
Anchor:         Metal object being pushed/pulled
Oraculum:          God metal, grants future sight
Cadmium:        Metal, creates time dilation bubbles
Compounding:    Storecraft + Metallurgy loop for amplification
Duralumin:      Enhancement metal, amplifies other powers
Electrum:       Temporal metal, grants self-future-sight
Storecraft:      Storage magic, 1:1 ratio
Flaring:        Pushing Metallurgic power to 150%
Bloodforge:      Stealing powers via metal spikes
Investiture:    Magical energy (the greater universe-wide concept)
Hauler:        Iron Puller (moves toward metal)
Ashwalker:       Can burn ALL 16 metals
Pewter:         Physical enhancement metal
Push Strength:  Mental intention multiplied by metal burning
Savant:         Extreme user, permanent side effects
Steel Push:     Push metal away (Launcher)
Tin:            Sensory enhancement metal
Twinborn:       Has one Metallurgy + one Storecraft power
```

---

---

## 19. The Oraculum Retcon (Era 1)

### What Changed

**Original (Era 1 canon)**:
- Oraculum was the 11th metal
- Grantied future sight
- Appeared as pure metal
- Oraculum Sparkbloods existed

**Retconned (Post-Lost Metal)**:
- Pure Oraculum is a GOD METAL
- Era 1 "oraculum" was actually **Naloraculum** (Oraculum + Electrum alloy)
- Everyone should be able to burn god metals (retcon explains why they couldn't)
- Oraculum-electrum alloys called "Naloraculum" grant future sight with enhanced mental processing

### the original author's Explanation

From WoB (2021 YouTube Spoiler Stream 3):

> *"This is accurate, yes. You could, by the way, just continue to call it oraculum. That's what they think oraculum is in-world. It's very slightly alloyed with electrum, and we call that naloraculum."*

### Why the Retcon?

```
1. Universal God Metal Burning
   - God metals (Primodium, Oraculum) should be burnable by ALL
   - Era 1 oraculum couldn't be burned by non-Ashwalker
   - This was an "oversight" the author wanted to fix
   
2. Pattern Consistency  
   - All other metals have alloys with known effects
   - Oraculum seemed "out of place"
   - Now follows the pattern: God metals have alloys

3. Future Trilogy Setup
   - Era 4 planned with 16 base metals
   - Pure Oraculum needed for specific future plot
   - Marsh uses it in Lost Metal to survive
```

### Naloraculum Properties

```
Alloy:         Oraculum + Electrum
In-World Name: "Oraculum"
Effect:        Future sight + Enhanced mental processing
               (ability to comprehend all the futures seen)

Pure Oraculum:    True God metal
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
├── Oraculum (NALATIUM)        ├── Oraculum (P GOD METAL)
└── Revelum (Au+At)        └── Revelum
```

### Implications for Physics Book

**Era 1 "Oraculum" physics still valid**:
- Future shadow calculations
- Duralumin+Oraculum bursts
- Oraculum burning against enemies

**Pure Oraculum (Era 2+) unknown**:
- Original calculations may not apply
- Mind enhancement likely stronger
- May have different force interactions

---

## 20. Bloodforge & Bloodbrute Physics

### How Bloodforge Works

```
Spike Creation:
1. Drive metal through someone's heart
2. Power/attribute is SPIKED into the metal
3. Remove spike (person usually dies)
4. Insert spike into recipient
5. Power/attribute is STAPLED to their Spiritweb

Key Point: Some Investiture is LOST in transfer
           Bloodforge is "destructive" art of The Unmaker
```

### Metal Affinities (Bloodforge)

```
Metal:     Steals:
─────────────────────────────
Iron       Human strength
Steel      Metallurgic strength
Tin        Sensory enhancement (tin eyes)
Pewter     Pewter strength/durability
Gold       Gold healing/nicrosil memory
Electrum   Emotional metallurgy
Zinc       Rioting (emotions)
Brass      Soothing (emotions)
Copper     Copper mind (memories)
Bronze     Bronze awareness (Seeking)
Aluminum   Storecraft
Duralumin  Storecraft + Metallurgy
Cadmium    Breath (from Warbreaker connection)
Nicrosil   Investiture stores (compounding hack!)
Oraculum      Spiritual attributes
Revelum   Gold-like (see alternate self)
```

### Bloodbrute Creation & Physics

```
Creation: 4 Iron spikes through human body
           Each spike steals "human strength"
           
Result:   Bloodbrute gain massive physical power
          Lose intelligence
          Continue growing throughout life
          
Physics Problem: Why do they grow?
```

### Bloodbrute Growth Model

```
Hypothesis 1: Bloodforged Power Leakage
   - Spikes leak power when not in use
   - Power absorption → physical growth
   - Older bloodbrute = more power absorbed = larger
   
Hypothesis 2: Muscle Mass Compensation
   - More strength = more muscle needed
   - Muscle growth = body growth
   - Skin doesn't grow proportionally (tight/ripped)
   
The Author's Answer:
   - Bloodbrute are bloodforged constructs
   - Growth is SPIRITUAL in nature
   - Body adapts to increasing power
   - Harmony "fixed" them in Era 2
```

### Bloodbrute Strength Scaling

```
Normal Human:   70kg body, 100% strength
Bloodbrute (small): 150kg body, ~3-5x human strength
Bloodbrute (large): 500kg+ body, ~10-20x human strength
Bloodbrute (titan): Growth continues until death

Energy Source: Bloodforged spike leakage + food
```

### Sentinel Spike Count

```
Standard Sentinel: 9-11 spikes
Eye Spikes:          Iron/Steel (Metallurgic sight)
Major Spikes:        Various powers
Linchpin Spike:      Upper back, holds spiritweb together

Physics Note:
- Heart relocated to accommodate spikes
- Brain shaped around eye-spikes
- Vital areas can be damaged without death
- Linchpin removal = death (spiritweb collapse)
```

### Bloodforged Constructs Summary

```
Construct:      Spikes:    Result:
─────────────────────────────────────────────
Bloodbrute          4 Iron     Brute strength, growing
Fleshkin          2 Same     Sapient, flexible bodies
Sentinel      9-11 Mix   Metallurgic powers, near-immortal
```

---

## 21. Aluminum & Duralumin (The True Enhancement Metals)

### Chromium & Nicrosil (Era 2 Discovery)

```
Chromium:    "Erasing" metal
             - Can wipe Metallurgic reserves
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
Aluminum:    Drains ALL Metallurgic reserves
             - Purer metals for storage
             - Universal counter
             
Duralumin:   Amplifies CURRENTLY BURNING metals
             - 10x effect on active metals
             - Drains reserves faster
             - Used for Duralumin+Oraculum "nuke"
             
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
   Normal Oraculum burn: 10 seconds of futures
   With Duralumin:   1 second of SUPER futures
   Total invested:    Same (Investiture conserved)

Nicrosil Compounding:
   Nicrosil + Steel + Speed → SPEED COMPOUNDING
   - Burns speed metalmind with Nicrosil
   - Gets amplified steel experience
   - Tap to get amplified speed
```

---

## 22. Metallurgic Conservation Laws

### What Metallurgy Conserves

```
CONFIRMED BY BRANDON (WoB):
✓ Conservation of Momentum
✓ Conservation of Energy  
✓ Inverse Square Law (like gravity/EM)

NOT Conservative (Magic Override):
✗ Conservation of Mass (Iron Storecraft)
✗ Time flow (Speed Bubbles)
✗ Information (Oraculum futuresight)
```

### Momentum Conservation in Action

```
Scenario: Launcher pushes coin into wall

Step 1: Force applied to coin (F)
Step 2: Equal force applied to Metallurgist (-F)
Step 3: Coin hits wall (massive effective mass)
Step 4: Force redirects Metallurgist backward

If Conservation TRUE:
   m_coin × v_coin = m_allo × v_allo
   Small coin gets high v, large Allo gets small v
```

### Energy Conservation

```
Pushing coin (m=0.01kg) to 1000 m/s:
   KE = ½ × 0.01 × 1000² = 5,000 J
   
Metallurgist "spends" this energy from metal burn
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

### Steel/Iron Metallurgy

| Question | Answer |
|----------|--------|
| Force formula? | Inverse square, like gravity |
| Range limit? | ~100 paces, up to 300 with upgrades |
| Conservation? | Yes, momentum AND energy |
| Anchor mass matters? | Yes, heavier = better anchor |
| Can push Shardblades? | Very hard, requires immense power |

### Storecraft

| Question | Answer |
|----------|--------|
| Storage ratio? | 1:1 (one out, one in) |
| Compounding? | ~10x amplification |
| Can store pewter strength? | Yes, but easier to compound |
| Iron Storecraft effect? | Inertial mass only (not gravitational) |
| Health storage? | Via multiple metals |

### Compounding

| Question | Answer |
|----------|--------|
| Basic mechanism? | Storecraft → Metallurgy → Storecraft |
| Amplification? | ~10x per cycle |
| Diminishing returns? | Yes, eventual limits |
| Dangers? | Pewter-drag-like effects |
| Iron compounder? | Called "Deaders" - die from weight |

### Oraculum (Retconned)

| Question | Answer |
|----------|--------|
| Era 1 oraculum? | Actually Naloraculum (At+El alloy) |
| Pure Oraculum burnable? | Yes, by anyone (WoB) |
| Pure Oraculum effect? | Enhanced futuresight + mind |
| Revelum? | Gold + Oraculum alloy |
| Naloraculum effect? | Same as Era 1 oraculum |

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

Author on Jostling:
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
├── Cannot use Metallurgy while exiting (interference)
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

## 24. Oraculum Physics (Era 1 vs Era 2)

### The Oraculum Electrum Retcon

```
Era 1 Canon:
├── Oraculum = Pure metal (11th metal)
├── Grants future sight
├── Oraculum Sparkbloods exist
└── Used by TLR, Ember, Darius

Retconned Canon (WoB 2021):
├── Era 1 "oraculum" = Naloraculum (Oraculum-Electrum alloy)
├── Pure Oraculum = God metal, burnable by ANYONE
├── Electrum Sparkbloods = What oraculum sparkbloods actually were
└── Alloys of oraculum have various temporal effects
```

### Pure Oraculum Properties (Era 2+)

```
Pure Oraculum (God Metal):
├── Burnable by: Everyone (not just Ashwalker)
├── Effect: Enhanced futuresight + mind enhancement
├── Bloodforge: Steals spiritual attributes
├── FTL: Allows glimpses of spiritual realm
└── The author: "It's very slightly alloyed with electrum"

Key Quote:
"This is accurate, yes. You could, by the way, just continue 
to call it oraculum. That's what they think oraculum is in-world."
```

### Naloraculum vs Pure Oraculum

```
Naloraculum (Era 1 oraculum):
├── Composition: Oraculum + Electrum alloy
├── In-world name: "Oraculum"
├── Effect: Future sight (like Era 1 oraculum)
├── Discovery: Found in Ember Pits
└── After HoA: No longer available

Pure Oraculum:
├── Composition: 100% Oraculum (god metal)
├── Effect: Enhanced futuresight + mental processing
├── Discovery: Created from ettmetal experiments
├── Use: Marsh uses to stay alive (Lost Metal)
└── Special: Can see farther/faster than naloraculum
```

### Oraculum-Storecraft Interactions

```
Compounding Oraculum:
├── Store: Future sight in goldmind
├── Burn: Get amplified futuresight
├── Tap: Enhanced prediction abilities
└── Risk: Too much oraculum = temporal confusion

Oraculum Storecraft (in Storecraft table):
├── Stores: Youth/age
├── Tap: Become younger
├── Use: TLR's extended lifespan
└── Note: Used gold compounders for this
```

### Oraculum + Duralumin Burst

```
Calculation:
  Normal Oraculum: See ~10 seconds ahead
  Duralumin Oraculum: See 10 seconds of EXTREME detail
  Cost: 10x burn rate for 1 second burst

Combat Application:
  TLR vs Ember (The Prophesied One)
  TLR uses Duralumin+Oraculum = Complete future sight
  Ember counters with Electrum = Duralumin counters this
  Result: Stalemate until external factors
```

### Oraculum Physics Model

```
Future Sight Mechanics:
  1. Burn Oraculum → Connect to Spiritual Realm
  2. Spiritual Realm = Where futures exist
  3. Oraculum grants "Fortune" - ability to see futures
  4. Mental enhancement = Process all futures simultaneously

Mathematical Model:
  Let F = Number of possible futures
  Let P = Processing capacity (mental enhancement)
  let T = Time ahead visible
  
  F = f(T, Investiture_level, Mental_capacity)
  Oraculum increases P by factor of mental_enhancement_bonus
```

---

## 25. Investiture Conservation Laws

### What the greater universe Conserves

```
CONFIRMED by the original author:
┌─────────────────────────────────────────────────────────┐
│  PHYSICS LAW          │  COSMERE STATUS                │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Momentum conserved       │
│  Momentum             │  (Metallurgy follows this)      │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Metal burn = energy     │
│  Energy               │  (But Investiture adds)        │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  YES - Time bubbles transfer   │
│  Information          │  energy through Spiritual      │
├─────────────────────────────────────────────────────────┤
│  Conservation of      │  PARTIAL - Storecraft can       │
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

the greater universe Addition:
  └── Investiture (magical energy)
       ├── Metallurgy = Investiture → Energy
       ├── Storecraft = 1:1 Investiture storage
       ├── Bloodforge = Investiture transfer (with loss)
       └── Compounding = Investiture multiplication

Result: Thermodynamics "bent" not broken
```

### Metallurgy Energy Budget

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

### Bloodforge Energy Loss

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
  
Result: Bloodforged powers are WEAKER than original
        But permanent (doesn't require metal burning)
```

### Storecraft Conservation

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
Metallurgic Force = Mental_Choice × Metal_Burn_Rate × Anchor_Quality / Distance²

Implementation:
```csharp
public float CalculatePushForce(
    float metallurgistStrength,  // Player's mental push
    float metalBurnRate,        // Current burn rate
    float anchorMass,            // Target metal mass
    float distance)              // Distance to target
{
    float anchorQuality = Mathf.Clamp01(anchorMass / MIN_MASS);
    float baseForce = metallurgistStrength * metalBurnRate;
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

### Storecraft Storage Model

```
```csharp
public class StorecraftStorage
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
        // Storecraft + Metallurgy loop
        for (int i = 0; i < cycles; i++)
        {
            stored *= factor; // 10x per cycle
        }
        return stored;
    }
}
```
```

### Complete Metallurgy System Architecture

```
Player GameObject
├── Metallurgist (base metal management)
├── MetalReserveManager (metal storage)
├── MetallurgyInputController (input handling)
├── MetallurgicCalculators (physics math)
│
├── Physical Metallurgy
│   ├── SteelPush
│   ├── IronPull
│   ├── TinEnhance
│   └── PewterBurn / PewterManager
│
├── Mental Metallurgy
│   ├── ZincRiot
│   ├── BrassSoothe
│   ├── CopperCloud
│   └── BronzeDetect
│
├── Temporal Metallurgy
│   ├── GoldPast
│   ├── ElectrumFuture
│   ├── CadmiumBubble
│   └── BendalloyBubble
│
├── Enhancement Metallurgy
│   ├── AluminumPurge
│   ├── DuraluminBurst
│   ├── ChromiumErasing
│   └── NicrosilCompounding
│
├── God Metals
│   ├── OraculumController
│   ├── PrimodiumController
│   ├── HarmoniumController
│   └── RevelumReveal
│
└── Integration
    ├── AshwalkerAbilityManager
    ├── StorecraftSystem
    └── BloodforgeSystem
```

---

## Appendix F: the original author WoB Compilation

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
| Can store in Storecraft? | Yes, but compounding easier | 2011 AoL Q&A |

### Storecraft (Storage)

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

### Oraculum (Retconned)

| Question | Answer | Source |
|----------|--------|--------|
| Era 1 oraculum? | Actually Naloraculum (At+El alloy) | 2021 WoB |
| Pure Oraculum burnable? | Yes, by anyone | 2021 WoB |
| Pure Oraculum effect? | Enhanced futuresight + mind | 2021 WoB |
| Revelum? | Gold + Oraculum alloy | In-book |
| Naloraculum discovery? | Ember Pits | HoA |

### God Metals (Era 2)

| Metal | Effect | Notes |
|-------|--------|-------|
| Primodium | Grants Ashwalker | Anyone who burns it |
| Oraculum | Enhanced futuresight | Pure form, not alloy |
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
║  ├─ Era 1: Actually Naloraculum (At+El alloy)                        ║
║  ├─ Pure Oraculum: God metal, burnable by anyone                     ║
║  └─ Effect: Enhanced futuresight + mind enhancement              ║
║                                                                   ║
║  HEMALURGY                                                         ║
║  ├─ Power loss in transfer (~50-60% efficiency)                    ║
║  ├─ Spikes: Permanent but weaker than original                    ║
║  └─ Bloodbrute: 4 iron spikes = strength + growth                      ║
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

### Core Metallurgy Equations

```
1. Push/Pull Force:
   F = A × S × Q / r²
   
   Where:
   - F = Force applied
   - A = Metallurgic strength coefficient
   - S = Mental push strength (chosen by metallurgist)
   - Q = Anchor quality (0-1, based on mass/connection)
   - r = Distance to anchor

2. Coin Velocity:
   v = √(2 × F × d / m)
   
   Where:
   - v = Final velocity
   - F = Applied force
   - d = Push distance
   - m = Coin mass

3. Metallurgist Recoil:
   a_recoil = F / m_metallurgist
   
   Conservation of momentum: m_coin × v_coin = m_allo × v_allo
```

### Storecraft Equations

```
1. Storage (1:1):
   Stored = BaseValue × Duration × Rate

2. Tapping:
   Tapped = Stored × TapMultiplier
   
3. Compounding (×10):
   After n cycles: Original × 10^n

4. Iron Mass Effect:
   mi_new = mi_original × StorecraftMultiplier
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
0      │ 1×         │ Normal Storecraft
1      │ 10×        │ Powerful Storecrafter
2      │ 100×       │ Twinborn Compounder
3      │ 1,000×     │ Ashen King tier
4      │ 10,000×    │ Dangerous territory
5      │ 100,000×   │ Iron compounder "Deaders"
```

---

## 13. Unity World Scale & Unit Conversions

*All physics code uses these conventions. Mismatch here causes wrong force magnitudes.*

---

### Space Scale

```
2 Unity units = 5 feet = 1.524 meters

Derived:
  1 Unity unit  = 2.5 ft   = 0.762 m
  1 meter       = 1.312 units
  1 foot        = 0.4 units

Quick reference:
  maxRange = 60 units  ≈ 150 ft  ≈ 45.7 m  (lore: "a few hundred feet")
  Player height ≈ 2 units = 5 ft = 1.524 m  (standard Unity capsule)
  Coin diameter ≈ 0.016 units ≈ 1.2 cm  (US quarter / Ashwalker penny)

To convert:
  meters → units : × 1.312
  feet   → units : × 0.4
  units  → meters: × 0.762
  units  → feet  : × 2.5
```

**Gravity in Unity units:**
```
g = 9.81 m/s²  ×  1.312 units/m  =  12.87 units/s²

Physics.gravity in Unity should be set to (0, -12.87, 0)
(or use the default (0, -9.81, 0) with METERS_PER_UNIT scale-correction
in force calculations)
```

---

### Time Scale — Day/Night Cycle

```
DayNightCycle.dayLengthMinutes = 20 (default)

  1 in-game day    = 20 real minutes  = 1,200 real seconds
  1 in-game hour   = 50 real seconds
  1 in-game minute = 0.833 real seconds
  1 in-game second = 0.01389 real seconds

  Compression ratio: 72× (72 in-game seconds per real second)
```

Metal burn durations from the MAG are **in-universe** durations (in-game hours/minutes).
The code stores them as **real-world seconds** (what the player experiences), mapping
1 MAG minute ≈ 0.833 real seconds. See `MetallurgyConstants` for derived drain rates.

---

### MAG Metal Burn Rates

*Source: Ashwalker Adventure Game (Crafty Games, official the original author-licensed)*
*Canonical burn durations at normal intensity, full reserve (100 units)*

```
Metal        │ MAG Duration │ Real-s (×0.833/min) │ Drain/s at reserve=100
─────────────┼──────────────┼─────────────────────┼───────────────────────
Aluminum     │ instant      │ 0 s (purge reserve) │ instant
Oraculum        │ 30 s         │ 30 s                │ ≈ 3.333
Bendalloy    │ 5 min        │ 300 s               │ ≈ 0.333
Brass        │ 20 min       │ 1,200 s             │ ≈ 0.0833
Bronze       │ 30 min       │ 1,800 s             │ ≈ 0.0556
Cadmium      │ 30 min       │ 1,800 s             │ ≈ 0.0556
Chromium     │ instant      │ 0 s (strip target)  │ instant
Copper       │ 40 min       │ 2,400 s             │ ≈ 0.0417
Duralumin    │ instant      │ 0 s (burst reserve) │ instant
Electrum     │ 10 min       │ 600 s               │ ≈ 0.1667
Gold         │ 10 min       │ 600 s               │ ≈ 0.1667
Iron         │ 20 min       │ 1,200 s             │ ≈ 0.0833
Nicrosil     │ instant      │ 0 s (burst target)  │ instant
Pewter       │ 5 min        │ 300 s               │ ≈ 0.333
Steel        │ 20 min       │ 1,200 s             │ ≈ 0.0833
Tin          │ 1 hour       │ 3,600 s             │ ≈ 0.0278
Zinc         │ 20 min       │ 1,200 s             │ ≈ 0.0833
```

*Notes:*
- *Flaring roughly doubles burn rate (halves duration)*
- *Duralumin + any metal expends the full reserve in one burst*
- *MAG durations represent "active use" — passive burning (Copper/Tin while exploring) matches these*
- *Drain rates in code live in `MetallurgyConstants` as `XxxDrainRate` constants*

---

---

## 14. Metal Ingestion, Capacity & Toxicology

*Lore-accurate constraints on Ashwalker metal storage and the real chemistry of ingestion.*

---

### Lore: Physical Capacity

```
Ashwalker physiology is adapted to handle metal ingestion better than normal humans.
No strict upper limit — constrained only by stomach size and discomfort.

Standard vial  ≈ shot glass (≈ 30–45 mL) — sufficient for a full day of burning
Handful of shavings — comfortable maximum for casual storage
Larger solid pieces (> 1 tsp) — unpredictable, advised against
```

**Safety rule (canonical):** Burn off all reserves before sleeping to prevent
heavy metal poisoning. Ashwalker are *resistant* to their metals' toxicity, not immune.

**Pewter Dragging reference:** Ember and Darius consumed large amounts of pewter beads
for sustained physical endurance — demonstrates reserve can exceed the standard vial
in emergencies.

**Savantism:** Constant heavy burning of large amounts (especially Pewter or Tin)
causes physiological alteration — the Metallurgist is permanently changed by the metal.

---

### Stomach Capacity Formulas

Used for simulating a Ashwalker's reserve pool or vial sizing.

**Adult stomach (fasting → full):**
```
Fasting capacity  ≈ 140 mL
Maximum capacity  ≈ 1,500–2,000 mL

Regression (anatomical study, age > 18):
  Volume (mL) = 27 + 14 × MAD – 1.28 × Age
  (MAD = Mean Axial Diameter from imaging, in mm)

Greater Curvature length (cm):
  GC = 17.47 + 0.02 × age + 0.06 × body_weight_kg
```

**Pediatric reference (not for Ashwalker, background context):**
```
Day 1:   5–7 mL    (cherry)
Day 3:   22–27 mL  (walnut)
Week 1:  45–60 mL  (apricot)
Month 1: 80–150 mL (large egg)
```

**Game implication:** A standard vial (≈ 40 mL) → reserve = 100 units.
Pewter-drag emergency vial (≈ 150 mL) → reserve = 375 units (3.75×).

---

### Metal Densities (kg/m³)

Real-world densities for all 16 Metallurgic metals. Used for metalmind capacity
calculations, coin mass verification, and computing ingestion volume from mass.

```
Category     │ Metal       │ Composition      │ Density (kg/m³)
─────────────┼─────────────┼──────────────────┼────────────────
Physical     │ Iron        │ Fe               │ 7,874
             │ Steel       │ Fe + C           │ 7,850
             │ Tin         │ Sn               │ 7,265
             │ Pewter      │ Sn + Pb alloy    │ ~7,300
─────────────┼─────────────┼──────────────────┼────────────────
Mental       │ Zinc        │ Zn               │ 7,134
             │ Brass       │ Cu + Zn alloy    │ ~8,500
             │ Copper      │ Cu               │ 8,960
             │ Bronze      │ Cu + Sn alloy    │ ~8,800
─────────────┼─────────────┼──────────────────┼────────────────
Temporal     │ Gold        │ Au               │ 19,320
             │ Electrum    │ Au + Ag alloy    │ ~15,000
             │ Cadmium     │ Cd               │ 8,650
             │ Bendalloy   │ Cd+Pb+Sn+Bi     │ ~9,500
─────────────┼─────────────┼──────────────────┼────────────────
Enhancement  │ Aluminum    │ Al               │ 2,700
             │ Duralumin   │ Al + Cu alloy    │ ~2,800
             │ Chromium    │ Cr               │ 7,190
             │ Nicrosil    │ Cr+Ni+Si alloy   │ ~8,200

Range: Aluminum (2,700) to Gold (19,320) — a 7× spread.
```

**Volume from mass:** `V = m / ρ`
```
Example — 5 g of each metal in a vial:
  Aluminum:  5 / 2700 = 1.85 mL   (lightest → largest volume per gram)
  Gold:      5 / 19320 = 0.26 mL  (densest → smallest volume per gram)
  Iron:      5 / 7874 = 0.64 mL

  16 metals × 5 g each = 80 g total
  Total volume ≈ 10–12 mL (less than a tablespoon)
```

This confirms lore: a full day's supply of all 16 metals fits easily in a
small flask or pouch. The metals are consumed as fine shavings or beads
suspended in alcohol solution.

---

### Heavy Metal Toxicity — Risk Formulas

For modeling poisoning if a Ashwalker fails to burn reserves.

**1. Average Daily Intake (Chronic Daily Intake)**
```
CDI = (C × IR × EF × ED) / (BW × AT)

  C   = metal concentration in sample  (mg/kg or mg/mL)
  IR  = ingestion rate                 (mL/day or g/day)
  EF  = exposure frequency             (days/year)
  ED  = exposure duration              (years)
  BW  = body weight                    (kg)
  AT  = averaging time                 (days; 365 × ED for non-cancer)
```

**2. Hazard Quotient (non-cancer risk)**
```
HQ = CDI / RfD

  RfD = Reference Dose (maximum safe daily intake, mg/kg/day)

  HQ < 1  → considered safe
  HQ > 1  → unsafe (poisoning risk)
```

**3. Hazard Index (multiple metals — e.g., Cadmium + Pewter + Lead)**
```
HI = Σ HQ_i = HQ_Cd + HQ_Pb + HQ_Sn + ...

HI > 1 → cumulative toxicity risk exceeds threshold
```

**4. Ecological Risk Index (water/sediment — relevant for environmental Metallurgy lore)**
```
RI = Σ (E_r^i)  where  E_r^i = T_r^i × C_f^i

  T_r^i = toxic response coefficient  (Cd=20, As=10, Pb=5)
  C_f^i = contamination factor        (sample / background concentration)
```

**Noted toxic metals (lore-confirmed):** Cadmium, Pewter (tin/lead alloy), Lead.

---

### Metal Dissolution in Stomach Acid

Stomach acid: pH 1.5–3.5, primarily HCl. Metal shavings dissolve via acid reaction.

**General equation:**
```
M  +  2 HCl  →  MCl₂  +  H₂↑

Specific examples:
  Iron (pewter base):   Fe + 2HCl → FeCl₂ + H₂
  Zinc:                 Zn + 2HCl → ZnCl₂ + H₂
  Aluminum:             2Al + 6HCl → 2AlCl₃ + 3H₂
```

**Maximum dissolvable mass (stoichiometric limit):**
```
m_max = (C_acid × V_stomach × M_metal) / (n × 1000)

  C_acid    = HCl concentration  (mol/L, typically 0.1–0.15 in stomach)
  V_stomach = gastric volume     (mL)
  M_metal   = molar mass of metal (g/mol; Fe=55.85, Zn=65.38, Al=26.98)
  n         = moles of HCl per mole of metal (Fe→2, Zn→2, Al→3)
```

**Example — iron shavings in a 40 mL vial at pH 2 (C_acid ≈ 0.01 mol/L):**
```
m_max = (0.01 × 40 × 55.85) / (2 × 1000) = 0.0112 g ≈ 11 mg

Conclusion: at stomach-acid concentrations, very little iron dissolves.
Metal shavings pass through mostly intact unless the stomach is empty
and acid is concentrated — which is why vials use fine particles in
alcohol solution to maximize surface area and absorption speed.
```

**Corrosion rate reference:** Razor blade (Fe) loses ~35% mass in 24 h in
concentrated acid, far less in stomach conditions (30–120 min transit time).

**Game application:** Metal reserve depletion from poisoning (if `burnedReserve = false`
at the `SleepEvent`): `damage = HQ × maxHealth × metalConcentrationFactor`

---

## 15. Arc Trajectories — Diagonal Push/Pull Physics

*Metallurgic pushes are rarely straight up or straight down. A Ashwalker pushing
off a coin on a rooftop while running launches at an angle — the resulting path
is a parabolic arc under the combined influence of the push vector and gravity.*

---

### Why Arcs Matter

```
A push/pull force acts along the blue line — the straight line from chest to metal.
As the Ashwalker and the metal source change positions, the blue line's angle changes.
Gravity always pulls straight down.

Combined effect: the Ashwalker traces a smooth parabolic arc, not a straight line.

Examples:
  - Push off a coin below-left → arc curves right and up
  - Pull toward a wall anchor at 45° → smooth Tarzan-swing arc
  - Push a coin at a target to the right → coin arcs down from gravity
```

---

### Core Arc Equation

Position at time t under constant push acceleration + gravity:

```
pos(t) = pos₀ + v₀ × t + ½ × a_combined × t²

where:
  a_combined = a_push + g
  a_push     = pushDirection × pushAccelMagnitude  (3D vector, from target to self for recoil)
  g          = (0, -9.81, 0) m/s²   or (0, -12.87, 0) in Unity units
```

Velocity at time t:
```
v(t) = v₀ + a_combined × t
```

---

### Force Decomposition

Any push at angle θ from vertical decomposes into:

```
F_vertical   = F_push × cos(θ)     (fights gravity — needed for levitation)
F_horizontal = F_push × sin(θ)     (lateral movement)

For levitation at angle θ:
  F_push × cos(θ) > m × g   →   push must exceed weight ÷ cos(θ)

At θ = 0° (straight down):   F_push > weight         (easiest levitation)
At θ = 30°:                  F_push > weight / 0.866  (15% more force needed)
At θ = 45°:                  F_push > weight / 0.707  (41% more force needed)
At θ = 60°:                  F_push > weight / 0.500  (2× force needed)
At θ = 90° (horizontal):     no vertical lift possible from this metal alone
```

---

### Continuous vs Pulsed Force Application

```
OLD (pulsed):
  Every 0.2s: instant velocity change of Δv along push direction.
  Between pulses: only gravity acts.
  Result: saw-tooth trajectory — jerky, especially at angles.

NEW (continuous):
  Every frame: velocity change of Δv × (dt / cooldown) along current push direction.
  Gravity always acts simultaneously.
  Result: smooth parabolic arc — the push direction updates as positions change,
  naturally curving the trajectory.

  Net impulse per second is identical: Δv / cooldown in both systems.
```

---

### Practical Scenarios

**Coin Push at angle α below horizontal (coin launched upward at α):**
```
Phase 1 — Push active (0 to t_push):
  x(t) = v₀ cos(α) × t + ½ a_push_x × t²
  y(t) = v₀ sin(α) × t + ½ (a_push_y − g) × t²

Phase 2 — Free flight (t_push to landing):
  Standard projectile: x continues at v_x, y decelerates under g only.

Horizontal range = Phase1_x + Phase2_x
Max height: solve v_y(t) = 0
```

**Hauler swing (pull toward elevated anchor):**
```
The Hauler arcs toward the anchor like a pendulum.
The pull force direction tracks the anchor, creating a smooth curve:
  - Start: mostly horizontal pull
  - Midpoint: pull becomes more vertical (passing below anchor)
  - End: pull decelerates as Hauler approaches anchor

Radial acceleration ≈ pullAccel − g×cos(θ_line)
Tangential acceleration = −g×sin(θ_line)
```

**Hover at equilibrium (distance r where push = weight):**
```
At angle θ from vertical:
  pushAccel × (1 − r/R) × cos(θ) = g
  r_hover = R × (1 − g / (pushAccel × cos(θ)))

Closer to vertical (smaller θ) → can hover at greater range.
More angled (larger θ) → must be closer to metal for hover.
```

---

## 16. Volcanic Ash & Aerosol Particle Physics

*Ashara's Ashmounts spew constant volcanic ash. These formulas govern how
ash falls, accumulates, and interacts — used by the ash particle systems,
DayNightCycle ash attenuation, and weather gameplay.*

---

### Terminal Settling Velocity

How fast an ash particle falls through still air:

```
v_t = √( (4 × g × d_p × (ρ_p − ρ_f)) / (3 × C_d × ρ_f) )

Where:
  v_t  = terminal velocity (m/s)
  g    = gravity (9.81 m/s²)
  d_p  = particle diameter (m)
  ρ_p  = particle density (kg/m³)  — ash: 700–2600 depending on vesicle content
  ρ_f  = fluid (air) density (1.225 kg/m³)
  C_d  = drag coefficient (depends on Reynolds number and shape)

Example — fine ash (d_p = 0.1 mm, ρ_p = 1500 kg/m³):
  v_t ≈ √((4 × 9.81 × 0.0001 × (1500 − 1.225)) / (3 × 24 × 1.225))
  v_t ≈ 0.26 m/s  (very slow — stays airborne for hours)

Example — coarse ash (d_p = 2 mm, ρ_p = 2000 kg/m³):
  v_t ≈ 3.6 m/s  (falls visibly, like heavy snow)
```

**Game application:** The `AshParticles` system uses `gravityModifier = 0.12`
which gives effective settling ≈ 1.2 m/s — between fine and coarse ash, which
looks right for the persistent Ashara ashfall.

---

### Drag Coefficient for Irregular Particles

Ash particles aren't spheres — they're jagged, vesicular fragments.
The drag coefficient depends on sphericity (ψ):

```
C_d = (24 / Re) × (1 + 0.15 × Re^0.687)    for Re < 1000  (Schiller-Naumann)

For irregular ash (sphericity correction):
  C_d_irregular = C_d_sphere / ψ

Where:
  ψ = sphericity (ratio of sphere surface area to actual surface area)
      ψ = 1.0 for perfect sphere
      ψ ≈ 0.6–0.8 for volcanic ash (jagged fragments)
      ψ ≈ 0.4–0.5 for very vesicular pumice

  Re = Reynolds number = (ρ_f × v × d_p) / μ
      μ = dynamic viscosity of air ≈ 1.81 × 10⁻⁵ Pa·s
```

Lower sphericity → higher drag → slower settling → ash stays airborne longer.
Ashara's constant ash haze is partially explained by low-sphericity fragments
from explosive Ashmount eruptions.

---

### Sticking Probability

When ash particles collide (with each other or surfaces), do they stick?

```
P_stick = exp(−St)

Where:
  St = Stokes number = (ρ_p × d_p² × v) / (18 × μ × L)

  v = relative velocity at collision
  L = characteristic length of collector (building wall, leaf, ground)

Interpretation:
  St << 1  →  P_stick ≈ 1.0  (particle follows air flow, deposits gently)
  St >> 1  →  P_stick ≈ 0    (particle has too much inertia, bounces off)
  St ≈ 1   →  P_stick ≈ 0.37 (transition — some stick, some don't)
```

**Game application:** This is why ash accumulates on flat surfaces (roofs,
ground, window ledges) but not so much on vertical walls — the sticking
probability is higher when the air flow curves around a surface and the
particle's inertia carries it into contact.

---

### Fractal Aggregation

Ash particles clump together into aggregates (clusters):

```
N = k_f × (d_agg / d_0)^D_f

Where:
  N     = number of primary particles in aggregate
  k_f   = fractal prefactor (typically 1.0–1.5)
  d_agg = diameter of aggregate
  d_0   = diameter of primary (single) particle
  D_f   = fractal dimension (1.8–2.5 for volcanic ash, typically ~2.0)

Example — 100 primary particles (d_0 = 50 μm, D_f = 2.0):
  d_agg = d_0 × (N / k_f)^(1/D_f)
  d_agg = 50 × (100)^0.5 = 500 μm = 0.5 mm

  The aggregate is 10× the diameter of a single particle but much lighter
  per volume (fractal = lots of void space inside).
```

**Game application:** Aggregation explains why ash sometimes falls as visible
clumps rather than individual invisible particles. The particle system's
`startSize` range (0.02–0.05) represents mixed singles and small aggregates.

---

### Key Physical Parameters

```
Reynolds Number:
  Re = (ρ_f × v × d_p) / μ

  Re < 1     → Stokes regime (viscous, slow settling)
  1 < Re < 1000 → Intermediate (most volcanic ash)
  Re > 1000  → Newton regime (large fragments, fast)

Stokes Number:
  St = (ρ_p × d_p² × v) / (18 × μ × L)

  Measures how well a particle follows air streamlines vs its own inertia.

Ash Particle Density Ranges:
  Dense basaltic ash:     2000–2600 kg/m³
  Andesitic ash:          1500–2000 kg/m³
  Pumiceous/vesicular:     700–1200 kg/m³
  Ashara ash (estimated): ~1500 kg/m³  (mix of dense + vesicular)
```

---

### Ash Accumulation Rate

```
Rate (kg/m²/s) = C_ash × v_t × ρ_p × (π/6) × d_p³ × N_particles/m³

For Ashara's constant ashfall (estimated):
  ~0.1–1.0 mm/day accumulation on flat surfaces
  Lowborn sweep roofs and streets regularly (lore-confirmed)
  Without sweeping, ash would bury Cinderhold in weeks

Game: DayNightCycle.ashAttenuation = 0.45 represents ~45% of sunlight
blocked by the ash column — consistent with persistent light ashfall.
```

---

---

## 17. Pendulum Physics — Hanging Objects & Swinging Metal

*Gibbets, hanging signs, lanterns, chains, and cage traps all swing as
physical pendulums. Metallurgic Pushes on hanging metal objects create
dramatic pendulum motion — a Launcher pushing a metal sign makes it
swing with predictable physics.*

---

### Physical Pendulum Period

```
T = 2π × √(I / (m × g × d))

Where:
  T = period of one full swing (seconds)
  I = moment of inertia about the pivot (kg·m²)
  m = mass of the object (kg)
  g = gravitational acceleration (9.81 m/s²)
  d = distance from pivot to center of mass (m)
```

### Angular Frequency

```
ω = √(m × g × d / I)

For small oscillations (θ < 15°):
  θ(t) = θ_max × cos(ω × t)
```

### Common Hanging Objects

```
Object          │ I (kg·m²)  │ m (kg)  │ d (m) │ T (s) │ Notes
────────────────┼────────────┼─────────┼───────┼───────┼──────────────
Hanging sign    │ 0.08       │ 2       │ 0.15  │ 1.47  │ Creaks in wind
Metal lantern   │ 0.02       │ 1.5     │ 0.2   │ 0.82  │ Quick gentle swing
Gibbet cage     │ 0.5        │ 5       │ 0.3   │ 1.16  │ Heavy, slow
Iron chain (1m) │ 0.3        │ 3       │ 0.5   │ 0.90  │ Catenary motion
Banner pole     │ 0.15       │ 1       │ 0.4   │ 1.23  │ Cloth dampens
Cage trap       │ 1.0        │ 8       │ 0.4   │ 1.13  │ Swings when triggered
```

### Simple Pendulum (Uniform Rod Pivoted at End)

```
T = 2π × √(2L / (3g))

Where L = total length of the rod.
For a 1m rod: T = 2π × √(2/29.43) ≈ 1.64s
```

### Metallurgic Interaction

```
When a Launcher pushes a hanging metal object:

1. Push force → impulse → initial angular velocity
   ω₀ = J / I  (where J = impulse = F × Δt)

2. Object swings as a pendulum with period T
   Maximum angle: θ_max = ω₀ / ω

3. Damping from air resistance and pivot friction:
   θ(t) = θ_max × e^(−γt) × cos(ωt)
   γ = damping coefficient (typically 0.1–0.5 for metal objects)

4. A Hauler pulling on the same object while it swings
   can amplify the oscillation (resonance) if timed to the period.
```

**Game application:** `TitleObjectSway` uses the period formula to set
realistic swing speeds. The gibbet cage at `swaySpeed = 0.85 Hz` matches
`T ≈ 1.16s` from the physical pendulum calculation.

---

*Document compiled from r/Ashwalker, r/the greater universe, and 17th Shard community analysis*
*the original author's official WoB from Arcanum.coppermind.net*
*the greater universe Era: 1022-1025 FE*
*Last Updated: April 2026*
*Version: 5.0 - Includes Pendulum Physics and Ash Aerosols*
