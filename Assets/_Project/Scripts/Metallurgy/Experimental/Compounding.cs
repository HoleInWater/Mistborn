/* Compounding.cs
 *
 * PURPOSE:
 * Bridges the Metallurgist and Storecrafter systems to implement Compounding —
 * the act of burning a metalmind Metallurgically to produce a massive burst
 * of the Storecrafted attribute stored within.
 *
 * LORE-ACCURATE COMPOUNDING:
 * ==========================
 * Compounding exploits a loophole in the Metallic Arts. Normally:
 *   - Metallurgy draws power from The Warden (end-positive)
 *   - Storecraft stores and retrieves the user's own power (end-neutral)
 *
 * When a Compounder burns a charged metalmind Metallurgically, The Warden's
 * power is filtered through the Storecrafted charge, producing ~10x the stored
 * attribute instead of the normal Metallurgic effect.
 *
 * This is how the Ashen King achieved immortality: he stored health in gold
 * metalminds, then Compounded them for effectively infinite healing.
 *
 * COMPOUNDING CYCLE:
 *   Cycle 0: Store 1 unit in metalmind (Storecraft)
 *   Cycle 1: Burn metalmind (Metallurgy) → ~10 units back
 *   Cycle 2: Store 10 → Burn → ~100 units
 *   After n cycles: P(n) = P₀ × 10^n × e^(-δn)
 *
 * GAMEPLAY:
 * - Compounding activates automatically when burning a metal you also have
 *   Storecrafted access to, AND the corresponding metalmind has charge.
 * - Produces 10x the normal Storecrafted tap output.
 * - Drains both Metallurgic reserves and metalmind charge simultaneously.
 * - Sustained compounding increases cycle count for even greater output,
 *   but with diminishing returns to prevent infinite scaling.
 */

using UnityEngine;
using System;

/// <summary>
/// Compounding component that bridges Metallurgist and Storecrafter on the same GameObject.
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

    /// <summary>Additional Metallurgic reserve drain multiplier during compounding.</summary>
    public const float MetallurgicDrainMultiplier = 2.5f;

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
    [SerializeField] private bool[] isCompounding = new bool[Storecrafter.MetalmindCount];

    [Header("Cycle Tracking")]
    [SerializeField] private float[] compoundingTime = new float[Storecrafter.MetalmindCount];
    [SerializeField] private float[] currentCycles = new float[Storecrafter.MetalmindCount];
    [SerializeField] private float[] timeSinceLastCompound = new float[Storecrafter.MetalmindCount];

    [Header("Output Multipliers (read-only debug)")]
    [SerializeField] private float[] outputMultipliers = new float[Storecrafter.MetalmindCount];

    [Header("Tuning")]
    [Tooltip("Override base multiplier per metal. 0 = use default.")]
    public float[] metalMultiplierOverrides = new float[Storecrafter.MetalmindCount];

    // Cached references
    private Metallurgist metallurgist;
    private Storecrafter storecrafter;

    // Events
    public event Action<int, float> OnCompoundingStarted;   // metalIndex, multiplier
    public event Action<int> OnCompoundingStopped;           // metalIndex
    public event Action<int, int> OnCycleAdvanced;           // metalIndex, newCycleCount

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════

    void Awake()
    {
        metallurgist = GetComponent<Metallurgist>();
        storecrafter = GetComponent<Storecrafter>();

        for (int i = 0; i < Storecrafter.MetalmindCount; i++)
        {
            outputMultipliers[i] = 1f;
            timeSinceLastCompound[i] = CycleDecayDelay + 1f;
        }

        AshwalkerRegistry.RegisterCompounder(this);
    }

    void OnDestroy()
    {
        AshwalkerRegistry.UnregisterCompounder(this);
    }

    void Update()
    {
        if (metallurgist == null || storecrafter == null) return;

        float dt = Time.deltaTime;

        for (int i = 0; i < Storecrafter.MetalmindCount; i++)
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
    /// Requires: Metallurgic burn active + Storecrafted metalmind has charge + both unlocked.
    /// </summary>
    private bool EvaluateCompoundingState(int metalIndex)
    {
        if (!storecrafter.CanCompound(metalIndex)) return false;

        Metalmind mind = storecrafter.GetMetalmind(metalIndex);
        if (mind == null || !mind.HasCharge) return false;

        MetallurgySkill.MetalType metalType = Storecrafter.MetalIndexToMetallurgicType(metalIndex);
        if (!metallurgist.IsMetalBurning(metalType)) return false;

        // Must have Metallurgic reserves to sustain compounding
        if (metallurgist.GetMetalReserve(metalType) <= 0f) return false;

        return true;
    }

    /// <summary>
    /// Process one frame of active compounding for a metal.
    /// Drains both systems and applies amplified Storecrafted output.
    /// </summary>
    private void ProcessActiveCompounding(int metalIndex, float dt)
    {
        Metalmind mind = storecrafter.GetMetalmind(metalIndex);
        MetallurgySkill.MetalType metalType = Storecrafter.MetalIndexToMetallurgicType(metalIndex);

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
        float rawPower = MetallurgyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, effectiveCycles, DiminishingConstant
        );

        // Normalize: at cycle 1, P = 10 × e^(-0.3) ≈ 7.4x. We want cycle 1 ≈ 10x.
        // Scale so that cycle 1 gives exactly baseMultiplier.
        float cycle1Power = MetallurgyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, 1, DiminishingConstant
        );
        float normalizedMultiplier = rawPower * (baseMultiplier / cycle1Power);

        outputMultipliers[metalIndex] = normalizedMultiplier;

        // --- Drain metalmind charge ---
        float metalmindDrain = MetalmindDrainRate * dt;
        storecrafter.DrainCharge(metalIndex, metalmindDrain);

        // --- Additional Metallurgic reserve drain ---
        // Compounding is costly — burns through reserves faster
        float additionalDrain = metallurgist.baseBurnRate * (MetallurgicDrainMultiplier - 1f) * dt;
        metallurgist.DrainMetal(metalType, additionalDrain);

        // --- Apply amplified attribute modifier to Storecrafter ---
        // Override the Storecrafter's attribute modifier with compounded value
        // Normal tap gives ~1.8x; compounding gives normalizedMultiplier × that
        float compoundedModifier = 1f + (normalizedMultiplier * 0.8f);
        storecrafter.attributeModifiers[metalIndex] = compoundedModifier;
    }

    /// <summary>
    /// Clean up when compounding stops for a metal.
    /// </summary>
    private void StopCompounding(int metalIndex)
    {
        // Reset attribute modifier to neutral (Storecrafter will recalculate if still tapping)
        storecrafter.attributeModifiers[metalIndex] = 1f;
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
        if (metalIndex < 0 || metalIndex >= Storecrafter.MetalmindCount) return false;
        return isCompounding[metalIndex];
    }

    /// <summary>
    /// Whether any metal is currently being Compounded.
    /// </summary>
    public bool IsCompoundingAny()
    {
        for (int i = 0; i < Storecrafter.MetalmindCount; i++)
            if (isCompounding[i]) return true;
        return false;
    }

    /// <summary>
    /// Get the current output multiplier for a compounding metal.
    /// Returns 1.0 if not compounding.
    /// </summary>
    public float GetOutputMultiplier(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Storecrafter.MetalmindCount) return 1f;
        return outputMultipliers[metalIndex];
    }

    /// <summary>
    /// Get the current compounding cycle count (fractional) for a metal.
    /// </summary>
    public float GetCurrentCycles(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Storecrafter.MetalmindCount) return 0f;
        return currentCycles[metalIndex];
    }

    /// <summary>
    /// Get all metals currently being Compounded.
    /// </summary>
    public bool[] GetCompoundingMetals()
    {
        bool[] result = new bool[Storecrafter.MetalmindCount];
        Array.Copy(isCompounding, result, Storecrafter.MetalmindCount);
        return result;
    }

    /// <summary>
    /// Get the Storecrafted attribute modifier produced by compounding.
    /// This is the amplified modifier applied to gameplay systems.
    /// </summary>
    public float GetCompoundedAttributeModifier(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Storecrafter.MetalmindCount) return 1f;
        if (!isCompounding[metalIndex]) return storecrafter.GetAttributeModifier(metalIndex);
        return storecrafter.attributeModifiers[metalIndex];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API — Manual Control (for AI/Boss use)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Force-start compounding on a specific metal (for boss AI).
    /// Requires the character to have both Metallurgic reserves and metalmind charge.
    /// Will start burning the metal Metallurgically if not already burning.
    /// </summary>
    public bool ForceStartCompounding(int metalIndex)
    {
        if (!storecrafter.CanCompound(metalIndex)) return false;

        Metalmind mind = storecrafter.GetMetalmind(metalIndex);
        if (mind == null || !mind.HasCharge) return false;

        MetallurgySkill.MetalType metalType = Storecrafter.MetalIndexToMetallurgicType(metalIndex);

        // Ensure Metallurgic reserves exist
        if (metallurgist.GetMetalReserve(metalType) <= 0f) return false;

        // Start burning if not already
        if (!metallurgist.IsMetalBurning(metalType))
        {
            metallurgist.StartBurning(metalType);
        }

        return true;
    }

    /// <summary>
    /// Force-stop compounding on a specific metal (for boss AI).
    /// Stops the Metallurgic burn for that metal.
    /// </summary>
    public void ForceStopCompounding(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= Storecrafter.MetalmindCount) return;

        MetallurgySkill.MetalType metalType = Storecrafter.MetalIndexToMetallurgicType(metalIndex);
        metallurgist.StopBurning();
    }

    /// <summary>
    /// Pre-charge a metalmind for compounding (for boss setup).
    /// Adds charge directly without requiring store time.
    /// </summary>
    public void PreChargeMetalmind(int metalIndex, float amount)
    {
        storecrafter.AddCharge(metalIndex, amount);
    }

    /// <summary>
    /// Get the total compounding power output for display/damage calculations.
    /// Returns the raw multiplied power from MetallurgyPhysicsFormulas.
    /// </summary>
    public float GetCompoundingPower(int metalIndex)
    {
        if (!isCompounding[metalIndex]) return 0f;

        float baseMultiplier = metalMultiplierOverrides[metalIndex] > 0f
            ? metalMultiplierOverrides[metalIndex]
            : BaseCompoundingMultiplier;

        int effectiveCycles = Mathf.Max(1, Mathf.FloorToInt(currentCycles[metalIndex]));
        return MetallurgyPhysicsFormulas.CalculateCompoundingWithDiminishingReturns(
            baseMultiplier, effectiveCycles, DiminishingConstant
        );
    }
}
