using UnityEngine;

/// <summary>
/// Static utility class containing ALL core physics formulas from docs/PHYSICS-MATH-BOOK.md.
/// Every formula is referenced by its handbook section number.
/// </summary>
public static class AllomancyPhysicsFormulas
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 2: STEEL & IRON — PUSH/PULL FORCE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lore-accurate Allomantic force (inverse-square, mass-product).
    /// F(a) = A × m₁ × m₂ / r²
    /// Handbook Section 2: A_vin ≈ 35,316 (without flaring), A_conservative ≈ 1,500
    /// </summary>
    public static float CalculateAllomanticForce(float A, float allomancerMass, float metalMass, float distance)
    {
        float r = Mathf.Max(0.5f, distance); // Prevent division by zero
        return A * allomancerMass * metalMass / (r * r);
    }

    /// <summary>
    /// Allomantic strength constant A, scaled by flare intensity.
    /// Base A = 1500 (conservative), flaring can multiply up to 2.5x.
    /// </summary>
    public static float GetAllomanticStrength(float baseA, float flareMultiplier)
    {
        return baseA * flareMultiplier;
    }

    /// <summary>
    /// Linear force model for better game feel (Handbook Section 2 alternate).
    /// F(a) = F_max × (r_max - r) / r_max, for 0 ≤ r ≤ r_max
    /// </summary>
    public static float CalculateLinearForce(float maxForce, float distance, float maxRange)
    {
        if (distance >= maxRange) return 0f;
        return maxForce * (maxRange - distance) / maxRange;
    }

    /// <summary>
    /// Newton's 3rd Law mass ratio — determines how much each party moves.
    /// The lighter object moves more. Returns (playerRatio, objectRatio).
    /// </summary>
    public static void CalculateMassRatios(float playerMass, float objectMass, bool isAnchored,
        out float playerRatio, out float objectRatio)
    {
        if (isAnchored)
        {
            playerRatio = 1f;
            objectRatio = 0f;
        }
        else
        {
            float totalMass = playerMass + objectMass;
            playerRatio = objectMass / totalMass; // Lighter player = more movement
            objectRatio = playerMass / totalMass;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 3: COIN VELOCITY FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Coin velocity after being pushed a given distance, assuming constant force.
    /// v(d) = √(2 × F(a) × d / m₂)
    /// Handbook Section 3: Conservative v_max ≈ 490 m/s
    /// </summary>
    public static float CalculateCoinVelocity(float force, float distance, float coinMass)
    {
        if (coinMass <= 0f) return 0f;
        float val = 2f * force * distance / coinMass;
        return val > 0f ? Mathf.Sqrt(val) : 0f;
    }

    /// <summary>
    /// Coin acceleration from Allomantic push.
    /// a = F(a) / m₂
    /// </summary>
    public static float CalculateCoinAcceleration(float force, float coinMass)
    {
        if (coinMass <= 0f) return 0f;
        return force / coinMass;
    }

    /// <summary>
    /// Air drag correction for coin velocity (advanced, Handbook Section 3).
    /// v(d) = v_terminal × (1 - e^(-d/τ))
    /// For a coin: v_terminal ≈ 77.8 m/s with drag
    /// </summary>
    public static float CalculateCoinVelocityWithDrag(float distance, float coinMass,
        float dragCoefficient = 0.47f, float crossSectionArea = 0.00045f, float airDensity = 1.225f)
    {
        float vTerminal = coinMass * 9.81f / (0.5f * airDensity * dragCoefficient * crossSectionArea);
        float tau = vTerminal / 9.81f; // Drag time constant
        return vTerminal * (1f - Mathf.Exp(-distance / tau));
    }

    /// <summary>
    /// Power-limited Allomantic force (community theory, Handbook Section 3).
    /// F(v) = min(F_max, P_max / v)
    /// At low velocity the force is capped at F_max; as the target speeds up,
    /// the available force drops (constant power output). This prevents coins
    /// from reaching hypersonic speeds and explains why fast bullets are hard
    /// to deflect — the allomancer's force is inversely proportional to the
    /// object's velocity.
    /// </summary>
    public static float CalculatePowerLimitedForce(float maxForce, float maxPower, float velocity)
    {
        if (velocity <= 0.01f) return maxForce;
        return Mathf.Min(maxForce, maxPower / velocity);
    }

    /// <summary>
    /// Terminal push velocity under the power-limited + drag model.
    /// v_terminal = (2 × P_max / (ρ × C_d × A))^(1/3)
    /// This is the speed at which push force = drag force, so the coin stops accelerating.
    /// </summary>
    public static float CalculatePowerLimitedTerminalVelocity(float maxPower,
        float dragCoefficient = 0.47f, float crossSectionArea = 0.00045f, float airDensity = 1.225f)
    {
        float denominator = airDensity * dragCoefficient * crossSectionArea;
        if (denominator <= 0f) return 0f;
        return Mathf.Pow(2f * maxPower / denominator, 1f / 3f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 4: FERUCHEMY STORAGE FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Constant-rate Feruchemical storage.
    /// S(t) = r₀ × t
    /// </summary>
    public static float CalculateConstantStorage(float rate, float time)
    {
        return rate * time;
    }

    /// <summary>
    /// Variable-rate storage with diminishing returns (Handbook Section 4).
    /// S(t) = (k/λ) × (1 - e^(-λt))
    /// Where k = initial storage rate, λ = diminishing returns factor
    /// </summary>
    public static float CalculateVariableStorage(float initialRate, float diminishingFactor, float time)
    {
        if (diminishingFactor <= 0f) return initialRate * time;
        return (initialRate / diminishingFactor) * (1f - Mathf.Exp(-diminishingFactor * time));
    }

    /// <summary>
    /// Asymptotic storage approaching metalmind capacity (Handbook Section 4).
    /// S = C_max × r / (C_max + r)
    /// Approaches C_max but never reaches it.
    /// </summary>
    public static float CalculateAsymptoticStorage(float maxCapacity, float storedAmount)
    {
        return maxCapacity * storedAmount / (maxCapacity + storedAmount);
    }

    /// <summary>
    /// Metalmind capacity based on volume and metal density (Handbook Section 4).
    /// C_max = K × V × ρ_metal
    /// Iron K≈1.0, Steel K≈1.1, Pewter K≈0.95
    /// </summary>
    public static float CalculateMetalmindCapacity(float capacityConstant, float volume, float metalDensity)
    {
        return capacityConstant * volume * metalDensity;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 5: COMPOUNDING EXPONENTIAL FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Basic compounding exponential (Handbook Section 5).
    /// P(n) = P₀ × 10^n
    /// Each cycle produces 10x the previous output.
    /// </summary>
    public static float CalculateCompoundingPower(float initialPower, int cycles)
    {
        return initialPower * Mathf.Pow(10f, cycles);
    }

    /// <summary>
    /// Compounding with diminishing returns (Handbook Section 5).
    /// P(n) = P₀ × 10^n × e^(-δn)
    /// Where δ = diminishing returns constant (0 < δ < 1)
    /// </summary>
    public static float CalculateCompoundingWithDiminishingReturns(float initialPower, int cycles, float diminishingConstant)
    {
        return initialPower * Mathf.Pow(10f, cycles) * Mathf.Exp(-diminishingConstant * cycles);
    }

    /// <summary>
    /// Net gain per compounding cycle (Handbook Section 5).
    /// G(n) = P(n) - P(n-1) - C_cost
    /// </summary>
    public static float CalculateNetCompoundingGain(float power, float previousPower, float cost)
    {
        return power - previousPower - cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 6: SPEED COMPOUNDING (STEEL FERUCHEMY)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Compound speed function (Handbook Section 6).
    /// v_compound(n) = v_base × 10^n × e^(-εn)
    /// Maximum theoretical: ~50 km/s. Game-capped much lower.
    /// </summary>
    public static float CalculateCompoundSpeed(float baseSpeed, int cycles, float efficiencyDecay)
    {
        return baseSpeed * Mathf.Pow(10f, cycles) * Mathf.Exp(-efficiencyDecay * cycles);
    }

    /// <summary>
    /// Heat generation from air resistance at high speed (Handbook Section 6).
    /// P_heat = ½ × ρ × C_d × A × v³
    /// Used to calculate damage at extreme compounded speeds.
    /// </summary>
    public static float CalculateSpeedHeatGeneration(float velocity, float dragCoefficient = 0.47f,
        float crossSection = 0.7f, float airDensity = 1.225f)
    {
        return 0.5f * airDensity * dragCoefficient * crossSection * velocity * velocity * velocity;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 7: IRON COMPOUNDING MASS (IRON FERUCHEMY)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Compound mass function (Handbook Section 7).
    /// m_compound(n) = m_base + m_stored × 10^n
    /// </summary>
    public static float CalculateCompoundMass(float baseMass, float storedMass, int cycles)
    {
        return baseMass + storedMass * Mathf.Pow(10f, cycles);
    }

    /// <summary>
    /// Feruchemy weight factor (Handbook Section 7).
    /// W = m × g × f, where f is the Feruchemy weight factor
    /// f < 1 when storing (lighter), f > 1 when tapping (heavier)
    /// </summary>
    public static float CalculateFeruchemicalWeight(float mass, float gravityFactor)
    {
        return mass * 9.81f * gravityFactor;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 8: PEWTER STRENGTH FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pewter strength multiplier (Handbook Section 8).
    /// S_pewter = S_base × (1 + k × P)
    /// Where k = efficiency constant, P = power level 0-1
    /// </summary>
    public static float CalculatePewterStrength(float baseStrength, float efficiencyK, float powerLevel)
    {
        return baseStrength * (1f + efficiencyK * powerLevel);
    }

    /// <summary>
    /// Pewter muscle mass increase (Handbook Section 8).
    /// m_muscle = m_base × (1 + α × P), α ≈ 0.5
    /// </summary>
    public static float CalculatePewterMuscleMass(float baseMass, float growthAlpha, float powerLevel)
    {
        return baseMass * (1f + growthAlpha * powerLevel);
    }

    /// <summary>
    /// Maximum force from Pewter enhancement (Handbook Section 8).
    /// F_max = m_total × a_max / η
    /// </summary>
    public static float CalculatePewterMaxForce(float totalMass, float maxAcceleration, float efficiency)
    {
        if (efficiency <= 0f) return 0f;
        return totalMass * maxAcceleration / efficiency;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 9: TIME BUBBLE FUNCTIONS (BENDALLOY & CADMIUM)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cadmium slow bubble: T_inside = T_outside × τ_slow (Handbook Section 9).
    /// τ_slow ≈ 0.1 (10x slower inside). Game default: 0.15
    /// </summary>
    public static float CalculateCadmiumTimeInside(float outsideTime, float slowFactor)
    {
        return outsideTime * slowFactor;
    }

    /// <summary>
    /// Bendalloy fast bubble: T_inside = T_outside × τ_fast (Handbook Section 9).
    /// τ_fast ≈ 10 (10x faster inside). Game default: 8
    /// </summary>
    public static float CalculateBendalloyTimeInside(float outsideTime, float fastFactor)
    {
        return outsideTime * fastFactor;
    }

    /// <summary>
    /// Combined bubble interaction (Handbook Section 9).
    /// T_effective = T_outside × (τ_cadmium / τ_bendalloy)
    /// </summary>
    public static float CalculateCombinedBubbleTime(float outsideTime, float cadmiumFactor, float bendalloyFactor)
    {
        if (bendalloyFactor <= 0f) return outsideTime;
        return outsideTime * (cadmiumFactor / bendalloyFactor);
    }

    /// <summary>
    /// Bubble duration limit based on metal reserve (Handbook Section 9).
    /// D_max = D_metal × E_efficiency
    /// </summary>
    public static float CalculateBubbleDuration(float metalReserve, float efficiency)
    {
        return metalReserve * efficiency;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 11: DIMINISHING RETURNS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// General diminishing returns curve (Handbook Section 11).
    /// effectiveness = 1 - e^(-k × reserve)
    /// At low reserve: near-linear. At high reserve: diminishing.
    /// </summary>
    public static float CalculateDiminishingReturns(float reserve, float diminishingK)
    {
        return 1f - Mathf.Exp(-diminishingK * reserve);
    }

    /// <summary>
    /// Flaring multiplier with diminishing returns (Handbook Section 11).
    /// Used by FlareManager to scale force from intensity.
    /// </summary>
    public static float CalculateFlareMultiplier(int intensity, int maxIntensity, float maxMultiplier)
    {
        if (maxIntensity <= 0) return 1f;
        float normalized = (float)intensity / maxIntensity;
        float diminished = 1f - Mathf.Exp(-3f * normalized); // Steep initial, plateaus
        return 1f + diminished * (maxMultiplier - 1f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 12: PRACTICAL APPLICATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Anchor quality based on mass (Handbook Section 10 graph).
    /// Higher mass = better anchor. Buildings/walls are quality 1.0.
    /// Q = log10(mass) / log10(1000), clamped 0-1
    /// </summary>
    public static float CalculateAnchorQuality(float metalMass)
    {
        if (metalMass <= 0f) return 0f;
        return Mathf.Clamp01(Mathf.Log10(metalMass) / 3f); // log10(1000) = 3
    }

    /// <summary>
    /// Predict projectile position accounting for gravity (Section 1).
    /// pos(t) = start + v×t + ½g×t²
    /// </summary>
    public static Vector3 PredictPosition(Vector3 startPos, Vector3 velocity, float time)
    {
        return startPos + (velocity * time) + (0.5f * Physics.gravity * time * time);
    }

    /// <summary>
    /// Predict position under a sustained Allomantic push/pull + gravity.
    /// The push direction is from origin toward target (or vice versa for pull).
    /// Combined acceleration = pushAccel × pushDirection + gravity
    /// pos(t) = start + v₀×t + ½×a_combined×t²
    /// Works for any angle — vertical, diagonal, horizontal — producing correct arcs.
    /// </summary>
    public static Vector3 PredictArcPosition(Vector3 startPos, Vector3 velocity,
        Vector3 pushAcceleration, float time)
    {
        Vector3 totalAccel = pushAcceleration + Physics.gravity;
        return startPos + (velocity * time) + (0.5f * totalAccel * time * time);
    }

    /// <summary>
    /// Velocity at time t under sustained push + gravity.
    /// v(t) = v₀ + a_combined × t
    /// </summary>
    public static Vector3 PredictArcVelocity(Vector3 initialVelocity,
        Vector3 pushAcceleration, float time)
    {
        return initialVelocity + (pushAcceleration + Physics.gravity) * time;
    }

    /// <summary>
    /// Decompose a push/pull force vector into vertical and horizontal components.
    /// Useful for determining if a diagonal push can sustain flight (vertical > gravity)
    /// or if a horizontal push will produce a useful arc vs just scraping the ground.
    /// </summary>
    public static void DecomposeForce(Vector3 forceDirection, float magnitude,
        out float verticalComponent, out float horizontalComponent)
    {
        verticalComponent   = forceDirection.y * magnitude;
        horizontalComponent = new Vector2(forceDirection.x, forceDirection.z).magnitude * magnitude;
    }

    /// <summary>
    /// Check if a push at a given angle can sustain flight (vertical push component > weight).
    /// angle = angle between push direction and straight down (0° = directly below = best levitation).
    /// Returns the net vertical acceleration (positive = ascending, negative = falling).
    /// </summary>
    public static float NetVerticalAcceleration(float pushAccelMagnitude, Vector3 pushDirection)
    {
        float verticalPush = -pushDirection.y * pushAccelMagnitude; // negative pushDir.y = pushing upward
        return verticalPush + Physics.gravity.y; // gravity.y is negative
    }

    /// <summary>
    /// Kinetic energy of a moving object (Section 1).
    /// KE = ½mv²
    /// </summary>
    public static float CalculateKineticEnergy(float mass, float velocity)
    {
        return 0.5f * mass * velocity * velocity;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WORLD SCALE — 2 Unity units = 5 feet (0.762 m/unit)
    // Canonical source: standard Unity humanoid is 2 units tall ≈ 5 ft / 1.524 m
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>1 Unity unit = 2.5 feet = 0.762 meters</summary>
    public const float METERS_PER_UNIT = 0.762f;
    public const float UNITS_PER_METER = 1.312f;   // = 1 / 0.762
    public const float FEET_PER_UNIT   = 2.5f;
    public const float UNITS_PER_FOOT  = 0.4f;     // = 1 / 2.5

    // Common distances:
    // maxRange = 60 units ≈ 150 ft ≈ 45.7 m  (lore: "a few hundred feet")

    // ═══════════════════════════════════════════════════════════════════════════
    // IN-GAME TIME SCALE — 20 real minutes per in-game day (DayNightCycle default)
    // 1 real second = 72 in-game seconds
    // Metal burn durations below are in REAL seconds (what the player experiences).
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Real seconds per in-game day (DayNightCycle.dayLengthMinutes × 60)</summary>
    public const float REAL_SECONDS_PER_INGAME_DAY = 20f * 60f;  // 1200 s
    /// <summary>In-game seconds that pass for every real second (72× compression)</summary>
    public const float INGAME_SECONDS_PER_REAL_SECOND = (24f * 3600f) / REAL_SECONDS_PER_INGAME_DAY; // 72

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS (handbook values, game-tuned for 2u=5ft scale)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Vin's calculated Allomantic strength constant (unflared)</summary>
    public const float A_VIN = 35316f;

    /// <summary>Conservative A for game balance (book-consistent)</summary>
    public const float A_CONSERVATIVE = 1500f;

    // ── Official coin specs (Shire Post Mint, Sanderson-licensed) ────────────
    /// <summary>Clip mass: 3 g copper coin, 2 cm diameter (standard Coinshot ammo)</summary>
    public const float CLIP_MASS = 0.003f;
    /// <summary>Boxing mass: 15.5 g brass coin, 3 cm diameter</summary>
    public const float BOXING_MASS = 0.0155f;
    /// <summary>Default coin mass for physics — Clip (lighter = faster projectile)</summary>
    public const float COIN_MASS = CLIP_MASS;

    /// <summary>Clip cross-section area: π × (0.01)² = 0.000314 m² (radius 1 cm)</summary>
    public const float CLIP_CROSS_SECTION = 0.000314f;
    /// <summary>Boxing cross-section area: π × (0.015)² = 0.000707 m² (radius 1.5 cm)</summary>
    public const float BOXING_CROSS_SECTION = 0.000707f;
    /// <summary>Default coin cross-section for physics — Clip</summary>
    public const float COIN_CROSS_SECTION = CLIP_CROSS_SECTION;

    /// <summary>Cadmium slow factor (10x slower, handbook τ≈0.1)</summary>
    public const float CADMIUM_TAU = 0.1f;

    /// <summary>Bendalloy fast factor (10x faster, handbook τ≈10)</summary>
    public const float BENDALLOY_TAU = 10f;

    /// <summary>Pewter muscle growth constant α (handbook ≈0.5)</summary>
    public const float PEWTER_ALPHA = 0.5f;

    /// <summary>Standard air density kg/m³</summary>
    public const float AIR_DENSITY = 1.225f;

    /// <summary>Coin drag coefficient (sphere approximation)</summary>
    public const float COIN_DRAG_COEFFICIENT = 0.47f;

    /// <summary>Gravity in Unity units/s² (9.81 m/s² × 1.312 units/m)</summary>
    public const float GRAVITY_UNITS = 12.87f;
}
