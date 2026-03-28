using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Physiologically-inspired stamina system for Mistborn.
///
/// Models three real endurance concepts:
///   1. Aerobic zone   — sustainable effort (jogging, light combat). Fast recovery.
///   2. Anaerobic zone — intense effort below the lactate threshold. Builds oxygen debt;
///                       recovery is slower until debt is paid down.
///   3. Exhaustion     — stamina depleted completely. Brief speed penalty; regen resumes
///                       after the exhaustion window clears.
///
/// Recovery is non-linear: fastest when most depleted, naturally tapers near full —
/// matching the cardiac O2-debt repayment curve.
///
/// Pewter Allomancy / Feruchemy raises aerobicRegenRate and lowers debtAccumulationRate
/// at runtime to model superhuman endurance.
///
/// FeruchemyController compatibility: regenRate is a property that aliases aerobicRegenRate.
/// </summary>
[PlayerComponent("Movement", order: 100)]
public class PlayerStamina : MonoBehaviour
{
    // ── Stamina Pool ──────────────────────────────────────────────────────────

    [Header("Stamina Pool")]
    [Tooltip("Maximum stamina. Represents total aerobic capacity.")]
    public float maxStamina = 100f;
    public float currentStamina;

    // ── Aerobic / Anaerobic Model ─────────────────────────────────────────────

    [Header("Aerobic / Anaerobic Model")]
    [Tooltip("Fraction of maxStamina below which effort is anaerobic and builds oxygen debt. " +
             "Healthy adult: ~0.50. Elite (Pewter-boosted): ~0.30.")]
    [Range(0.2f, 0.8f)]
    public float lactateThreshold = 0.50f;

    [Tooltip("Maximum oxygen debt that can accumulate. Exhaustion sets debt to this cap.")]
    [Range(10f, 80f)]
    public float maxOxygenDebt = 40f;

    [Tooltip("Rate at which oxygen debt accumulates per second of anaerobic effort, " +
             "scaled by how far below the lactate threshold stamina is.")]
    [Range(1f, 20f)]
    public float debtAccumulationRate = 8f;

    [Tooltip("Rate at which oxygen debt decays per second at full rest.")]
    [Range(1f, 15f)]
    public float debtRecoveryRate = 5f;

    // ── Recovery Rates ────────────────────────────────────────────────────────

    [Header("Recovery Rates (stamina/sec)")]
    [Tooltip("Peak regen rate in the aerobic zone — no oxygen debt, stamina low.")]
    [Range(5f, 40f)]
    public float aerobicRegenRate = 20f;

    [Tooltip("Regen rate while paying oxygen debt — body still in recovery mode.")]
    [Range(1f, 15f)]
    public float anaerobicRegenRate = 7f;

    [Tooltip("Seconds after last drain before regen begins.")]
    [Range(0.5f, 4f)]
    public float regenDelay = 1.5f;

    // ── Exhaustion ────────────────────────────────────────────────────────────

    [Header("Exhaustion")]
    [Tooltip("Duration of the exhaustion penalty window when stamina hits zero.")]
    [Range(1f, 6f)]
    public float exhaustionDuration = 3f;

    [Tooltip("Speed multiplier applied during exhaustion. 0.4 = 40% normal speed.")]
    [Range(0.1f, 1f)]
    public float exhaustionSpeedFactor = 0.4f;

    // ── UI ────────────────────────────────────────────────────────────────────

    [Header("UI")]
    public UIDocument uiDocument;

    // ── Public State ──────────────────────────────────────────────────────────

    /// <summary>True during the penalty window after stamina is fully depleted.</summary>
    public bool IsExhausted { get; private set; }

    /// <summary>
    /// Speed multiplier for movement systems to apply.
    /// Linearly recovers from exhaustionSpeedFactor → 1 over the exhaustion window.
    /// </summary>
    public float ExhaustionPenalty => IsExhausted
        ? Mathf.Lerp(exhaustionSpeedFactor, 1f, 1f - (exhaustionTimer / exhaustionDuration))
        : 1f;

    /// <summary>True when stamina is below the lactate threshold.</summary>
    public bool IsInAnaerobicZone => currentStamina < maxStamina * lactateThreshold;

    /// <summary>Current accumulated oxygen debt (0–maxOxygenDebt).</summary>
    public float OxygenDebt { get; private set; }

    // ── FeruchemyController compatibility shim ────────────────────────────────
    // FeruchemyController reads and writes regenRate directly.
    // Map it to aerobicRegenRate so the Feruchemy energy effect still works.

    public float regenRate
    {
        get => aerobicRegenRate;
        set => aerobicRegenRate = value;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private float regenTimer;
    private float exhaustionTimer;
    private ProgressBar staminaBar;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        currentStamina = maxStamina;

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            staminaBar = root?.Q<ProgressBar>("Stamina");
            if (staminaBar != null) staminaBar.highValue = maxStamina;
        }
    }

    void Update()
    {
        if (staminaBar != null) staminaBar.value = currentStamina;

        TickExhaustion();
        TickRecovery();
    }

    // ── Internal Ticks ────────────────────────────────────────────────────────

    private void TickExhaustion()
    {
        if (!IsExhausted) return;

        exhaustionTimer -= Time.deltaTime;
        if (exhaustionTimer <= 0f)
        {
            IsExhausted    = false;
            exhaustionTimer = 0f;
        }
    }

    private void TickRecovery()
    {
        // Debt still decays slowly even mid-exhaustion — heart rate is coming down
        if (OxygenDebt > 0f)
        {
            float debtDecayScale = IsExhausted ? 0.25f : 1f;
            OxygenDebt = Mathf.Max(0f, OxygenDebt - debtRecoveryRate * debtDecayScale * Time.deltaTime);
        }

        // No stamina regen during exhaustion or while the delay timer is running
        if (IsExhausted || regenTimer > 0f)
        {
            regenTimer = Mathf.Max(0f, regenTimer - Time.deltaTime);
            return;
        }

        if (currentStamina >= maxStamina) return;

        // Non-linear regen: strongest when most depleted (cardiac recovery curve).
        // Stays above 40% speed even when nearly full so it doesn't feel stuck.
        float depletion   = 1f - (currentStamina / maxStamina);
        float regenScale  = Mathf.Lerp(0.4f, 1f, depletion);
        float rate        = OxygenDebt > 1f ? anaerobicRegenRate : aerobicRegenRate;

        currentStamina = Mathf.Clamp(currentStamina + rate * regenScale * Time.deltaTime, 0f, maxStamina);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Continuous stamina cost per second (sprinting, blocking).
    /// Time.deltaTime is applied internally — pass rate, not amount*dt.
    /// </summary>
    public void DrainStamina(float amountPerSecond)
    {
        if (IsExhausted) return;

        currentStamina -= amountPerSecond * Time.deltaTime;
        currentStamina  = Mathf.Clamp(currentStamina, 0f, maxStamina);
        regenTimer      = regenDelay;

        // Accumulate oxygen debt proportional to depth below lactate threshold
        if (currentStamina < maxStamina * lactateThreshold)
        {
            float depthRatio = 1f - (currentStamina / (maxStamina * lactateThreshold));
            OxygenDebt = Mathf.Min(maxOxygenDebt,
                OxygenDebt + debtAccumulationRate * depthRatio * Time.deltaTime);
        }

        if (currentStamina <= 0f) TriggerExhaustion();
    }

    /// <summary>
    /// Instant stamina cost (jump, dodge, heavy attack).
    /// </summary>
    public void UseStamina(float amount)
    {
        if (IsExhausted) return;

        currentStamina -= amount;
        currentStamina  = Mathf.Clamp(currentStamina, 0f, maxStamina);
        regenTimer      = regenDelay;

        // Burst efforts below threshold spike oxygen debt
        if (currentStamina < maxStamina * lactateThreshold)
            OxygenDebt = Mathf.Min(maxOxygenDebt, OxygenDebt + amount * 0.4f);

        if (currentStamina <= 0f) TriggerExhaustion();
    }

    public float GetCurrentStamina() => currentStamina;

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TriggerExhaustion()
    {
        if (IsExhausted) return;

        IsExhausted    = true;
        exhaustionTimer = exhaustionDuration;
        currentStamina  = 0f;
        OxygenDebt      = maxOxygenDebt;                    // fully in debt at collapse
        regenTimer      = exhaustionDuration + regenDelay;  // don't regen until penalty clears
        EventManager.TriggerEvent("PlayerExhausted");
    }
}
