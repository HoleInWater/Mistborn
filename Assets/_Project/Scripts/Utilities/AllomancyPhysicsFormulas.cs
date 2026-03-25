using UnityEngine;

/// <summary>
/// Comprehensive physics formulas for Allomancy based on PHYSICS-MATH-BOOK.md
/// Implements lore-accurate force calculations, coin trajectories, and time bubble physics
/// </summary>
public static class AllomancyPhysicsFormulas
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - Derived from physics handbook calculations
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Allomantic strength constant (Vin baseline) ≈ 35,316</summary>
    public const float AllomanticStrengthConstant = 35316f;

    /// <summary>Conservative estimate for book consistency ≈ 1,500</summary>
    public const float ConservativeStrengthConstant = 1500f;

    /// <summary>Default coin mass in kg (quarter)</summary>
    public const float CoinMass = 0.01f;

    /// <summary>Default player mass in kg (Vin)</summary>
    public const float PlayerMass = 40f;

    /// <summary>Standard max range for push/pull in meters</summary>
    public const float MaxRange = 80f;

    /// <summary>Zenith point - distance where force peaks</summary>
    public const float ZenithPoint = 5f;

    /// <summary>Maximum force cap</summary>
    public const float MaxForce = 2000f;

    // ═══════════════════════════════════════════════════════════════════════════
    // CORE FORCE EQUATIONS - Steel & Iron Push/Pull
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary inverse-square force equation for Push/Pull
    /// F(a) = (A × m₁ × m₂) / r²
    /// </summary>
    public static float CalculateInverseSquareForce(float strengthConstant, float playerMass, float targetMass, float distance)
    {
        float r = Mathf.Max(0.5f, distance);
        float force = (strengthConstant * playerMass * targetMass) / (r * r);
        return Mathf.Clamp(force, 0, MaxForce);
    }

    /// <summary>
    /// Linear force model for better game feel
    /// F(a) = F_max × (r_max - r) / r_max
    /// </summary>
    public static float CalculateLinearForce(float maxForce, float maxRange, float distance)
    {
        float r = Mathf.Clamp(distance, 0, maxRange);
        return maxForce * (maxRange - r) / maxRange;
    }

    /// <summary>
    /// Lore-accurate force with zenith cap
    /// Force increases until zenith point, then decreases
    /// </summary>
    public static float CalculateLoreAccurateForce(float strengthConstant, float playerMass, float targetMass, float distance, float flareMultiplier = 1f)
    {
        float r = Mathf.Max(0.5f, distance);
        
        float baseForce = (strengthConstant * playerMass * targetMass) / (r * r);
        
        // Zenith cap - max force at 5m, then diminishes
        float zenithCap = 2f;
        if (r < ZenithPoint)
        {
            baseForce *= zenithCap;
        }
        else
        {
            baseForce *= Mathf.Clamp01(ZenithPoint / r);
        }
        
        // Apply flaring
        baseForce *= flareMultiplier;
        
        return Mathf.Clamp(baseForce, 0, MaxForce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WEIGHT-PROPORTIONAL FORCE - Physics-based push/pull
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Weight-proportional force based on physics handbook
    /// F = (playerMass / referenceMass) × baseForce
    /// </summary>
    public static float CalculateWeightProportionalForce(float playerMass, float referenceMass, float baseForce, float distance)
    {
        float distanceFactor = 1f / Mathf.Max(1f, distance);
        float weightRatio = playerMass / referenceMass;
        return baseForce * weightRatio * distanceFactor;
    }

    /// <summary>
    /// Determines who moves more based on mass ratio (Newton's 3rd Law)
    /// Heavier object moves less, lighter object moves more
    /// </summary>
    public static void CalculateMovementDistribution(float playerMass, float targetMass, float totalForce, out float playerMovement, out float targetMovement)
    {
        float totalMass = playerMass + targetMass;
        
        // Mass ratio determines velocity distribution
        playerMovement = totalForce * (targetMass / totalMass);
        targetMovement = totalForce * (playerMass / totalMass);
    }

    /// <summary>
    /// Check if target is "anchored" (too heavy to move effectively)
    /// If target mass > playerMass × 3, player gets pulled instead
    /// </summary>
    public static bool IsTargetAnchored(float playerMass, float targetMass)
    {
        return targetMass > playerMass * 3f;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COIN VELOCITY FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate velocity at distance given constant force
    /// v(d) = √(2 × F(a) × d / m₂)
    /// </summary>
    public static float CalculateCoinVelocity(float force, float distance, float coinMass)
    {
        float velocity = Mathf.Sqrt(2f * force * distance / coinMass);
        return velocity;
    }

    /// <summary>
    /// Calculate max velocity with extended push
    /// </summary>
    public static float CalculateMaxCoinVelocity(float strengthConstant, float playerMass, float coinMass, float maxDistance)
    {
        float forceAt5m = (strengthConstant * playerMass * coinMass) / 25f; // at 5m (r² = 25)
        float velocity = Mathf.Sqrt(2f * forceAt5m * maxDistance / coinMass);
        return velocity;
    }

    /// <summary>
    /// Velocity with air drag correction
    /// v(d) = v_terminal × (1 - e^(-d/τ))
    /// </summary>
    public static float CalculateVelocityWithDrag(float initialVelocity, float distance, float dragCoefficient = 0.47f, float coinRadius = 0.012f)
    {
        float airDensity = 1.225f; // kg/m³
        float crossSection = Mathf.PI * coinRadius * coinRadius;
        float terminalVelocity = (CoinMass * Physics.gravity.y) / (0.5f * airDensity * dragCoefficient * crossSection);
        
        float dragTimeConstant = CoinMass / (0.5f * airDensity * dragCoefficient * crossSection);
        
        return terminalVelocity * (1f - Mathf.Exp(-distance / dragTimeConstant));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PREDICTION & TRAJECTORY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Predict position at time t
    /// pos = start + (v × t) + (0.5 × g × t²)
    /// </summary>
    public static Vector3 PredictPosition(Vector3 startPos, Vector3 velocity, float time)
    {
        return startPos + (velocity * time) + (0.5f * Physics.gravity * time * time);
    }

    /// <summary>
    /// Predict position with allomantic force
    /// </summary>
    public static Vector3 PredictAllomanticTrajectory(Vector3 startPos, Vector3 initialDirection, float force, float coinMass, float maxTime, int steps = 20)
    {
        Vector3[] points = new Vector3[steps + 1];
        Vector3 velocity = initialDirection * Mathf.Sqrt(2f * force * maxTime / coinMass);
        
        float dt = maxTime / steps;
        
        for (int i = 0; i <= steps; i++)
        {
            float t = i * dt;
            points[i] = PredictPosition(startPos, velocity, t);
            
            // Update velocity based on distance to player (inverse square force)
            float dist = Vector3.Distance(startPos, points[i]);
            if (dist > 0.5f)
            {
                float newForce = CalculateInverseSquareForce(AllomanticStrengthConstant, PlayerMass, coinMass, dist);
                float acceleration = newForce / coinMass;
                velocity += Vector3.down * acceleration * dt;
            }
        }
        
        return points[steps];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FERUCHEMY STORAGE FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Basic storage function - constant rate
    /// S(t) = r₀ × t
    /// </summary>
    public static float CalculateConstantStorage(float rate, float time)
    {
        return rate * time;
    }

    /// <summary>
    /// Variable rate storage with diminishing returns
    /// S(t) = (k/λ) × (1 - e^(-λt))
    /// </summary>
    public static float CalculateVariableStorage(float initialRate, float diminishingFactor, float time)
    {
        if (diminishingFactor <= 0) return initialRate * time;
        return (initialRate / diminishingFactor) * (1f - Mathf.Exp(-diminishingFactor * time));
    }

    /// <summary>
    /// Metal capacity function
    /// C_max = K × V × ρ_metal
    /// </summary>
    public static float CalculateMetalCapacity(float capacityConstant, float volume, float metalDensity)
    {
        return capacityConstant * volume * metalDensity;
    }

    /// <summary>
    /// Storage with diminishing returns (asymptotic)
    /// S = (C_max × r) / (C_max + r)
    /// </summary>
    public static float CalculateStorageWithDiminishingReturns(float maxCapacity, float rate)
    {
        if (maxCapacity <= 0) return rate;
        return (maxCapacity * rate) / (maxCapacity + rate);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPOUNDING FUNCTIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Basic compounding exponential
    /// P(n) = P₀ × 10^n
    /// </summary>
    public static float CalculateCompoundingPower(float initialPower, int cycles)
    {
        return initialPower * Mathf.Pow(10f, cycles);
    }

    /// <summary>
    /// Compounding with diminishing returns
    /// P(n) = P₀ × 10^n × e^(-δn)
    /// </summary>
    public static float CalculateCompoundingWithDiminishingReturns(float initialPower, int cycles, float diminishingConstant)
    {
        return initialPower * Mathf.Pow(10f, cycles) * Mathf.Exp(-diminishingConstant * cycles);
    }

    /// <summary>
    /// Net gain per cycle
    /// G(n) = P(n) - P(n-1) - C_cost
    /// </summary>
    public static float CalculateNetCompoundingGain(float power, float previousPower, float cost)
    {
        return power - previousPower - cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIME BUBBLE PHYSICS - Bendalloy & Cadmium
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Time scale inside bubble
    /// </summary>
    public static float CalculateTimeScale(float bubbleMultiplier)
    {
        return 1f / bubbleMultiplier; // If outside moves 2x, inside moves at 0.5x
    }

    /// <summary>
    /// Velocity adjustment entering/leaving bubble
    /// </summary>
    public static Vector3 AdjustVelocityForTimeBubble(Vector3 velocity, float timeScale)
    {
        return velocity * timeScale;
    }

    /// <summary>
    /// Force modification inside time bubble
    /// </summary>
    public static float ModifyForceInTimeBubble(float force, float timeScale)
    {
        return force * timeScale; // Forces also scale with time
    }

    /// <summary>
    /// Bubble radius for given metal amount
    /// </summary>
    public static float CalculateBubbleRadius(float metalAmount, float baseRadius = 10f)
    {
        return baseRadius * Mathf.Sqrt(metalAmount / 100f); // Area scales with metal
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PEWTER ENHANCEMENT - Strength & Speed
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pewter strength multiplier based on reserve percentage
    /// </summary>
    public static float CalculatePewterStrengthMultiplier(float metalReservePercent, float baseMultiplier = 1.5f)
    {
        return 1f + (baseMultiplier - 1f) * (metalReservePercent / 100f);
    }

    /// <summary>
    /// Pewter speed boost
    /// </summary>
    public static float CalculatePewterSpeedMultiplier(float metalReservePercent)
    {
        return 1f + 0.8f * (metalReservePercent / 100f);
    }

    /// <summary>
    /// Pewter fall damage reduction
    /// </summary>
    public static float CalculatePewterFallReduction(float fallDistance, float metalReservePercent)
    {
        float baseReduction = 0.7f;
        float reduction = baseReduction * (metalReservePercent / 100f);
        return Mathf.Clamp01(reduction);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FLARING MECHANICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Flare multiplier based on intensity level (1-10)
    /// </summary>
    public static float CalculateFlareMultiplier(int intensityLevel, float maxMultiplier = 2.5f)
    {
        return Mathf.Lerp(1f, maxMultiplier, (float)(intensityLevel - 1) / 9f);
    }

    /// <summary>
    /// Flare burn rate
    /// </summary>
    public static float CalculateFlareBurnRate(float baseRate, float additionalRatePerLevel, int intensityLevel)
    {
        return baseRate + (additionalRatePerLevel * (intensityLevel - 1));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REACTION FORCE CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate reaction force on sender (Newton's 3rd Law)
    /// </summary>
    public static Vector3 CalculateReactionForce(Vector3 forceVector, float senderMass, float targetMass)
    {
        float ratio = targetMass / (senderMass + targetMass);
        return -forceVector * ratio;
    }

    /// <summary>
    /// Calculate force for anchored objects (player moves instead)
    /// </summary>
    public static Vector3 CalculateAnchoredPushForce(Vector3 direction, float force, float playerMass, float targetMass)
    {
        float playerForce = force * (targetMass / (playerMass + targetMass));
        return direction * playerForce;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DURALUMIN BURST
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Duralumin burst force multiplier
    /// </summary>
    public static float CalculateDuraluminBurst(float metalReserve)
    {
        return 10f * (metalReserve / 100f); // 10x multiplier when full
    }

    /// <summary>
    /// Nicroburst amplification
    /// </summary>
    public static float CalculateNicroburstAmplification(float currentMultiplier)
    {
        return currentMultiplier * 2f;
    }
}