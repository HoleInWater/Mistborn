/* Compounding.cs
 *
 * PURPOSE:
 * Bridges the Allomancer and Feruchemist systems to implement Compounding —
 * the act of burning a metalmind Allomantically to produce a massive burst
 * of the Feruchemical attribute stored within.
 *
 * LORE-ACCURATE COMPOUNDING:
 * ==========================
 * Compounding exploits a loophole in the Metallic Arts. Normally:
 *   - Allomancy draws power from Preservation (end-positive)
 *   - Feruchemy stores and retrieves the user's own power (end-neutral)
 *
 * When a Compounder burns a charged metalmind Allomantically, Preservation's
 * power is filtered through the Feruchemical charge, producing ~10x the stored
 * attribute instead of the normal Allomantic effect.
 *
 * This is how the Lord Ruler achieved immortality: he stored health in gold
 * metalminds, then Compounded them for effectively infinite healing.
 *
 * COMPOUNDING CYCLE:
 *   Cycle 0: Store 1 unit in metalmind (Feruchemy)
 *   Cycle 1: Burn metalmind (Allomancy) → ~10 units back
 *   Cycle 2: Store 10 → Burn → ~100 units
 *   After n cycles: P(n) = P₀ × 10^n × e^(-δn)
 *
 * GAMEPLAY:
 * - Compounding activates automatically when burning a metal you also have
 *   Feruchemical access to, AND the corresponding metalmind has charge.
 * - Produces 10x the normal Feruchemical tap output.
 * - Drains both Allomantic reserves and metalmind charge simultaneously.
 * - Sustained compounding increases cycle count for even greater output,
 *   but with diminishing returns to prevent infinite scaling.
 */

using UnityEngine;
using System;

/// <summary>
/// Compounding component that bridges Allomancer and Feruchemist on the same GameObject.
/// Automatically detects when both systems are active on the same metal and amplifies output.
/// </summary>
public class Compounding : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Base multiplier for Compounding output vs normal tapping.</summary>
    public const float BaseCompoundingMultiplier = 10f;

    /// <summary>Maximum compounding cycles before hard cap.</summary>
    public const int MaxCycles = 4;

    /// <summary>Diminishing returns constant (δ) per cycle.</summary>
    public const float DiminishingConstant = 0.3f;

    /// <summary>How fast metalmind charge drains during compounding (units/sec).</summary>
    public const float MetalmindDrainRate = 5f;

    /// <summary>Additional Allomantic reserve drain multiplier during compounding.</summary>
    public const float AllomanticDrainMultiplier = 2.5f;

    /// <summary>Time in seconds of sustained compounding to advance one cycle.</summary>
    public const float CycleAdvanceTime = 8f;

    /// <summary>Time without compounding before cycle count decays.</summary>
    public const float CycleDecayDelay = 3f;

    /// <summary>Rate at which cycles decay when not compounding (cycles/sec).</summary>
    public const float CycleDecayRate = 0.5f;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════════════════

    [Header("Compounding State")]
    [SerializeField] private bool[] isCompounding = new bool[Feruchemist.MetalmindCount];

    [Header("Cycle Tracking")]
    [SerializeField] private float[] compoundingTime = new float[Feruchemist.MetalmindCount];
    [SerializeField] private float[] currentCycles = new float[Feruchemist.MetalmindCount];
    [SerializeField] private float[] timeSinceLastCompound = new float[Feruchemist.MetalmindCount];

    [Header("Output Multipliers (read-only debug)")]
    [SerializeField] private float[] outputMultipliers = new float[Feruchemist.MetalmindCount];

    [Header("Tuning")]
    [Tooltip("Override base multiplier per metal. 0 = use default.")]
    public float[] metalMultiplierOverrides = new float[Feruchemist.MetalmindCount];

    // Cached references
    private Allomancer allomancer;
    private Feruchemist feruchemist;

    // Events
    public event Action<int, float> OnCompoundingStarted;   // metalIndex, multiplier
    public event Action<int> OnCompoundingStopped;           // metalIndex
    public event Action<int, int> OnCycleAdvanced;           // metalIndex, newCycleCount

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════

    void Awake()
    {
        allomancer = GetComponent<Allomancer>();
        feruchemist = GetComponent<Feruchemist>();

        for (int i = 0; i < Feruchemist.MetalmindCount; i++)
        {
            outputMultipliers[i] = 1f;
            timeSinceLastCompound[i] = CycleDecayDelay + 1f;
        }

        MistbornRegistry.RegisterCompounder(this);
    }

    void OnDestroy()
    {
        MistbornRegistry.UnregisterCompounder(this);
    }

    void Update()
    {
        if (allomancer == null || feruchemist == null) return;

        float dt = Time.deltaTime;

        for (int i = 0; i < Feruchemist.MetalmindCount; i++)
        {
            bool wasCompounding = isCompounding[i];
            bool shouldCompound = EvaluateCompoundingState(i);

            if (shouldCompound)
            {
                ProcessActiveCompounding(i, dt);

                if (!wasCompounding)
                {
                    OnCompoundingStarted?.Invoke(i, outputMultipliers[i]);
                }
            }
            else
            {
                if (wasCompounding)
                {
                    StopCompounding(i);
                    OnCompoundingStopped?.Invoke(i);
                }

                ProcessCycleDecay(i, dt);
            }

            isCompounding[i] = shouldCompound;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CORE LOGIC
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Evaluate whether compounding should be active for a given metal.
    /// Requires: Allomantic burn active + Feruchemical metalmind has charge + both unlocked.
    /// </summary>
    private bool EvaluateCompoundingState(int metalIndex)
    {
        if (!feruchemist.CanCompound(metalIndex)) return false;

        Metalmind mind = feruchemist.GetMetalmind(metalIndex);
        if (mind == null || !mind.HasCharge) return false;

        AllomancySkill.MetalType metalType = Feruchemist.MetalIndexToAllomanticType(metalIndex);
        if (!allomancer.IsMetalBurning(metalType)) return false;

        // Must have Allomantic reserves to sustain compounding
        if (allomancer.GetMetalReserve(metalType) <= 0f) return false;

        return true;
    }

    /// <summary>
    /// Process one frame of active compounding for a metal.
    /// Drains both systems and applies amplified Feruchemical output.
    /// </summary>
    private void ProcessActiveCompounding(int metalIndex, float dt)
    {
        Metalmind mind = feruchemist.GetMetalmind(metalIndex);
        AllomancySkill.MetalType metalType = Feruchemist.MetalIndexToAllomanticType(metalIndex);

        // --- Advance compounding time and cycles ---
        compoundingTime[metalIndex] += dt;
        timeSinceLastCompound[metalIndex] = 0f;

        int previousCycleInt = Mathf.FloorToInt(currentCycles[metalIndex]);
        float cycleProgress = compoundingTime[metalIndex] / CycleAdvanceTime;
        currentCycles[metalIndex] = Mathf.Min(cycleProgress, MaxCycles);

        int newCycleInt = Mathf.FloorToInt(currentCycles[metalIndex]);
        if (newCycleInt > previousCycleInt && newCycleInt > 0)
        {
            OnCycleAdvanced?.Invoke(metalIndex, newCycleInt);
        }

        // --- Calculate output multiplier using physics formulas ---
        float baseMultiplier = metalMultiplierOverrides[metalIndex] > 0f
            ? metalMultiplierOverrides[metalIndex]
            : BaseCompoundingMultiplier;

        int effectiveCycles = Mathf.Max(1, Mathf.FloorToInt(currentCycles[metalIndex]));
        float rawPower = AllomancyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, effectiveCycles, DiminishingConstant
        );

        // Normalize: at cycle 1, P = 10 × e^(-0.3) ≈ 7.4x. We want cycle 1 ≈ 10x.
        // Scale so that cycle 1 gives exactly baseMultiplier.
        float cycle1Power = AllomancyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, 1, DiminishingConstant
        );
        float normalizedMultiplier = rawPower * (baseMultiplier / cycle1Power);

        outputMultipliers[metalIndex] = normalizedMultiplier;

        // --- Drain metalmind charge ---
        float metalmindDrain = MetalmindDrainRate * dt;
        feruchemist.DrainCharge(metalIndex, metalmindDrain);

        // --- Additional Allomantic reserve drain ---
        // Compounding is costly — burns through reserves faster
        float additionalDrain = allomancer.baseBurnRate * (AllomanticDrainMultiplier - 1f) * dt;
        allomancer.DrainMetal(metalType, additionalDrain);

        // --- Apply amplified attribute modifier to Feruchemist ---
        // Override the Feruchemist's attribute modifier with compounded value
        // Normal tap gives ~1.8x; compounding gives normalizedMultiplier × that
        float compoundedModifier = 1f + (normalizedMultiplier * 0.8f);
        feruchemist.attributeModifiers[metalIndex] = compoundedModifier;
    }

    /// <summary>
    /// Clean up when compounding stops for a metal.
    /// </summary>
    private void StopCompounding(int metalIndex)
    {
        // Reset attribute modifier to neutral (Feruchemist will recalculate if still tapping)
        feruchemist.attributeModifiers[metalIndex] = 1f;
        outputMultipliers[metalIndex] = 1f;
    }

    /// <summary>
    /// Decay cycle count when not actively compounding.
    /// Lore: compounding momentum dissipates when you stop the loop.
    /// </summary>
    private void ProcessCycleDecay(int metalIndex, float dt)
    {
        timeSinceLastCompound[metalIndex] += dt;

        if (timeSinceLastCompound[metalIndex] > CycleDecayDelay && currentCycles[metalIndex] > 0f)
        {
            currentCycles[metalIndex] -= CycleDecayRate * dt;
            if (currentCycles[metalIndex] < 0f)
            {
                currentCycles[metalIndex] = 0f;
                compoundingTime[metalIndex] = 0f;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API — Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether a specific metal is currently being Compounded.
    /// </summary>
    public bool IsCompounding(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Feruchemist.MetalmindCount) return false;
        return isCompounding[metalIndex];
    }

    /// <summary>
    /// Whether any metal is currently being Compounded.
    /// </summary>
    public bool IsCompoundingAny()
    {
        for (int i = 0; i < Feruchemist.MetalmindCount; i++)
            if (isCompounding[i]) return true;
        return false;
    }

    /// <summary>
    /// Get the current output multiplier for a compounding metal.
    /// Returns 1.0 if not compounding.
    /// </summary>
    public float GetOutputMultiplier(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Feruchemist.MetalmindCount) return 1f;
        return outputMultipliers[metalIndex];
    }

    /// <summary>
    /// Get the current compounding cycle count (fractional) for a metal.
    /// </summary>
    public float GetCurrentCycles(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Feruchemist.MetalmindCount) return 0f;
        return currentCycles[metalIndex];
    }

    /// <summary>
    /// Get all metals currently being Compounded.
    /// </summary>
    public bool[] GetCompoundingMetals()
    {
        bool[] result = new bool[Feruchemist.MetalmindCount];
        Array.Copy(isCompounding, result, Feruchemist.MetalmindCount);
        return result;
    }

    /// <summary>
    /// Get the Feruchemical attribute modifier produced by compounding.
    /// This is the amplified modifier applied to gameplay systems.
    /// </summary>
    public float GetCompoundedAttributeModifier(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Feruchemist.MetalmindCount) return 1f;
        if (!isCompounding[metalIndex]) return feruchemist.GetAttributeModifier(metalIndex);
        return feruchemist.attributeModifiers[metalIndex];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API — Manual Control (for AI/Boss use)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Force-start compounding on a specific metal (for boss AI).
    /// Requires the character to have both Allomantic reserves and metalmind charge.
    /// Will start burning the metal Allomantically if not already burning.
    /// </summary>
    public bool ForceStartCompounding(int metalIndex)
    {
        if (!feruchemist.CanCompound(metalIndex)) return false;

        Metalmind mind = feruchemist.GetMetalmind(metalIndex);
        if (mind == null || !mind.HasCharge) return false;

        AllomancySkill.MetalType metalType = Feruchemist.MetalIndexToAllomanticType(metalIndex);

        // Ensure Allomantic reserves exist
        if (allomancer.GetMetalReserve(metalType) <= 0f) return false;

        // Start burning if not already
        if (!allomancer.IsMetalBurning(metalType))
        {
            allomancer.StartBurning(metalType);
        }

        return true;
    }

    /// <summary>
    /// Force-stop compounding on a specific metal (for boss AI).
    /// Stops the Allomantic burn for that metal.
    /// </summary>
    public void ForceStopCompounding(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Feruchemist.MetalmindCount) return;

        AllomancySkill.MetalType metalType = Feruchemist.MetalIndexToAllomanticType(metalIndex);
        allomancer.StopBurning();
    }

    /// <summary>
    /// Pre-charge a metalmind for compounding (for boss setup).
    /// Adds charge directly without requiring store time.
    /// </summary>
    public void PreChargeMetalmind(int metalIndex, float amount)
    {
        feruchemist.AddCharge(metalIndex, amount);
    }

    /// <summary>
    /// Get the total compounding power output for display/damage calculations.
    /// Returns the raw multiplied power from AllomancyPhysicsFormulas.
    /// </summary>
    public float GetCompoundingPower(int metalIndex)
    {
        if (!isCompounding[metalIndex]) return 0f;

        float baseMultiplier = metalMultiplierOverrides[metalIndex] > 0f
            ? metalMultiplierOverrides[metalIndex]
            : BaseCompoundingMultiplier;

        int effectiveCycles = Mathf.Max(1, Mathf.FloorToInt(currentCycles[metalIndex]));
        return AllomancyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, effectiveCycles, DiminishingConstant
        );
    }
}
