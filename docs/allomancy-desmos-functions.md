# Allomancy Steel Push - Desmos Math Functions

Use these in Desmos (desmos.com/calculator)

---

## VARIABLES (set these as sliders)

| Variable | Description | Suggested Range |
|----------|-------------|----------------|
| `F_0` | Base push force (Newtons) | 500-2000 |
| `R_max` | Maximum push range (meters) | 10-50 |
| `d` | Distance to target | 0 to R_max |
| `v_0` | Initial velocity of target | -50 to 50 |
| `m_c` | Mass of coin (kg) | 0.00567 (quarter) |
| `m_p` | Mass of pusher (kg) | 70-90 |
| `t` | Time (seconds) | 0-2 |
| `k` | Distance decay constant | 0.5-2 |
| `P_max` | Maximum power limit | 5000-20000 |
| `v_cap` | Velocity cap (m/s) | 800-1200 |
| `C_d` | Drag coefficient | 0.1-1.0 |
| `ρ` | Air density (kg/m³) | 1.225 |

---

## 1. DISTANCE FALLOFF FUNCTION
```
F(d) = F_0 * e^(-k * d / R_max)
```

**This models:** Push force decreases exponentially with distance

---

## 2. VELOCITY DAMPING FUNCTION
```
D(v) = 1 - (|v| / v_cap)^2  when |v| < v_cap
D(v) = 0.1                    when |v| >= v_cap
```

---

## 3. COMBINED PUSH FORCE
```
F_push(d, v) = F(d) * max(0.3, D(v)) * 1000
```

*(multiply by 1000 to scale for visibility in Desmos)*

---

## 4. ACCELERATION
```
a(t) = F_push / m_c
```

---

## 5. VELOCITY OVER TIME (with drag)
```
v(t) = v_final * e^(-C_d * ρ * v(t)^2 * t) + v_terminal

v_terminal = sqrt(2 * m_c * g / (C_d * ρ * A))
```

---

## 6. POSITION OVER TIME
```
x(t) = x_0 + v_0 * t + 0.5 * a * t^2
```

---

## 7. POWER OUTPUT
```
P(t) = F_push * |v(t)|
```

---

## 8. COIN VELOCITY CAP
```
v_final = min(v_cap, (2 * F_0 * R_max / m_c)^0.5)
```

---

## DESMOS EXPRESSIONS TO COPY

```
F_0 = 1500
R_max = 50
k = 1
v_cap = 1000
m_c = 0.00567
m_p = 77
C_d = 0.47
ρ = 1.225

F(d) = F_0 e^{-k d / R_max}

v_final(d) = \sqrt{2 F(d) R_max / m_c}

a(d) = F(d) / m_c

P(d) = F(d) v_final(d)
```

---

## 9. REACTION FORCE ON PUSHER
```
F_reaction = F_push * (m_p / (m_p + m_c))
```

---

## 10. DISTANCE AFTER TIME T
```
x(t) = \int_{0}^{t} v(τ) dτ
```

---

## GRAPH SUGGESTIONS

**Graph 1: Force vs Distance**
- x-axis: d (0 to R_max)
- y-axis: F(d)
- Type: Line

**Graph 2: Velocity over Distance**
- x-axis: d
- y-axis: v_final(d)
- Type: Line

**Graph 3: 3D Surface (Force vs Distance AND Velocity)**
- F_push(d, v) = F_0 e^{-k d / R_max} * (1 - (v/v_cap)^2)
- Use 3D graphing mode

---

## SIMPLIFIED GAME BALANCE VERSION

For gameplay tuning (not real physics):

```
F_push = baseDamage * e^(-distance/maxRange) * metalQuality
```

Where:
- `baseDamage` = 100 (tunable)
- `maxRange` = 50
- `metalQuality` = 0.5-1.0 (coins vs ingots)

---

## DESMOS LINK (pre-made)

https://www.desmos.com/calculator/your-function-here

*(Note: Create account at desmos.com, paste the expressions above)*

---

## QUICK REFERENCE

| Symbol | Meaning | Unit |
|--------|---------|------|
| F | Force | Newtons (N) |
| d | Distance | Meters (m) |
| v | Velocity | m/s |
| a | Acceleration | m/s² |
| m | Mass | kg |
| t | Time | seconds |
| e | Euler's number | ≈2.718 |
| g | Gravity | 9.8 m/s² |
| P | Power | Watts (W) |
| ρ | Air density | kg/m³ |
