/* FlareManager.cs
 *
 * PURPOSE:
 * Manages burn state and flare intensity (1–10). Does NOT touch metal reserves
 * directly — Allomancer.cs owns the single reserve pool and handles all draining.
 * Switching intensity just changes how fast Allomancer drains that one pool.
 *
 * LORE-ACCURATE FLARING:
 * ======================
 * Flaring = burning metal at accelerated rate for temporary power boost.
 * - Physical metals (Pewter, Tin, Steel, Iron) can be flared.
 * - Mental metals (Zinc, Brass, Copper, Bronze) CANNOT be flared.
 * - Temporal metals (Atium, Electrum, Gold, Malatium) CANNOT be flared.
 * - Enhancement metals (Aluminum, Duralumin) have special rules.
 * - Time metals (Bendalloy, Cadmium) CANNOT be flared.
 * - God metals (Chromium, Nicrosil) CANNOT be flared.
 *
 * RISKS:
 * - Over-flaring can damage the Allomancer's body (Tin Savant risk).
 * - Flaring drains reserves much faster (3x-10x base rate).
 * - Metal runs out entirely if flared too long.
 *
 * CONTROL SCHEME:
 * ===============
 * - Left Ctrl        → Toggle burning ON / OFF
 * - Scroll UP        → (while burning) Increase intensity toward 10
 * - Scroll DOWN      → (while burning) Decrease intensity toward 1
 *
 * Intensity is never 0 — burning always starts at 1 and scrolls 1–10.
 * Turning burning off preserves intensity for next time.
 *
 * USAGE FROM OTHER SCRIPTS:
 * =========================
 *   FlareManager.Instance.IsBurning        // true when Left Ctrl is toggled on
 *   FlareManager.Instance.IsFlaring        // true only when burning AND intensity > 1
 *   FlareManager.Instance.IsIronFlaring    // true when burning Iron and flaring
 *   FlareManager.Instance.IsSteelFlaring   // true when burning Steel and flaring
 *   FlareManager.Instance.IsPewterFlaring  // true when burning Pewter and flaring
 *   FlareManager.Instance.IsTinFlaring     // true when burning Tin and flaring
 *   FlareManager.Instance.Intensity        // 1–10
 *   FlareManager.Instance.FlareMultiplier  // 1.0–maxFlareMultiplier (for force scaling)
 *   FlareManager.Instance.flareBurnRate    // drain/sec passed to Allomancer
 *   FlareManager.Instance.IsFlaringAllowed // whether current metal can be flared
 */

using UnityEngine;

public class FlareManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static FlareManager Instance { get; private set; }

    private Allomancer _cachedAllomancer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _cachedAllomancer = GetComponentInParent<Allomancer>();
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Intensity Settings")]
    [Tooltip("Maximum intensity level (scroll ceiling).")]
    public int maxIntensitySteps = 10;

    [Tooltip("Intensity change per scroll tick.")]
    public int scrollStepSize = 1;

    [Header("Burn Rates")]
    [Tooltip("Drain per second passed to Allomancer at intensity 1.")]
    public float baseBurnRate = 1f;

    [Tooltip("Additional drain per second per intensity step above 1.")]
    public float burnRatePerStep = 1.5f;

    [Tooltip("Flaring drain multiplier - how much faster flaring drains metal.")]
    public float flareDrainMultiplier = 3f;

    [Tooltip("Over-flaring drain multiplier at high intensities.")]
    public float overFlareDrainMultiplier = 5f;

    [Header("Force Scaling")]
    [Tooltip("Force multiplier at intensity 10.")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;

    [Header("Over-Flare Risks")]
    [Tooltip("Intensity threshold where over-flaring risks begin.")]
    public int overFlareThreshold = 7;

    [Tooltip("Damage per second when over-flaring.")]
    public float overFlareDamagePerSecond = 5f;

    [Tooltip("Maximum body strain from over-flaring.")]
    public float maxBodyStrain = 100f;

    [Tooltip("Body strain recovery rate per second when not flaring.")]
    public float bodyStrainRecoveryRate = 10f;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Whether burning is toggled on (Left Ctrl).</summary>
    public bool IsBurning { get; private set; } = false;

    /// <summary>Shared intensity 1–10. Persists when burning is toggled off.</summary>
    public int Intensity { get; private set; } = 1;

    /// <summary>Current body strain from over-flaring (0-maxBodyStrain).</summary>
    public float BodyStrain { get; private set; } = 0f;

    // ── Derived Properties ────────────────────────────────────────────────────

    /// <summary>True only when burning AND intensity > 1 (actual flaring).</summary>
    public bool IsFlaring => IsBurning && Intensity > 1;

    /// <summary>True when burning Iron and intensity > 1.</summary>
    public bool IsIronFlaring => IsBurning && IsCurrentMetalFlarable() && GetCurrentMetal() == AllomancySkill.MetalType.Iron && Intensity > 1;

    /// <summary>True when burning Steel and intensity > 1.</summary>
    public bool IsSteelFlaring => IsBurning && IsCurrentMetalFlarable() && GetCurrentMetal() == AllomancySkill.MetalType.Steel && Intensity > 1;

    /// <summary>True when burning Pewter and intensity > 1.</summary>
    public bool IsPewterFlaring => IsBurning && IsCurrentMetalFlarable() && GetCurrentMetal() == AllomancySkill.MetalType.Pewter && Intensity > 1;

    /// <summary>True when burning Tin and intensity > 1.</summary>
    public bool IsTinFlaring => IsBurning && IsCurrentMetalFlarable() && GetCurrentMetal() == AllomancySkill.MetalType.Tin && Intensity > 1;

    /// <summary>True if the current metal can be flared (not all metals can).</summary>
    public bool IsFlaringAllowed => IsCurrentMetalFlarable();

    /// <summary>True if currently over-flaring (intensity above threshold).</summary>
    public bool IsOverFlaring => IsBurning && Intensity >= overFlareThreshold;

    /// <summary>Backward-compat aliases.</summary>
    public int FlareIntensity => Intensity;
    public int flareIntensity => Intensity;

    /// <summary>
    /// Force multiplier: 1.0 at intensity 1, maxFlareMultiplier at intensity 10.
    /// Returns 1.0 when not burning.
    /// Lore: Flaring provides temporary power boost.
    /// </summary>
    public float FlareMultiplier
    {
        get
        {
            if (!IsBurning) return 1f;

            // Check if current metal can be flared
            if (!IsCurrentMetalFlarable()) return 1f;

            float mult = Mathf.Lerp(1f, maxFlareMultiplier, (float)(Intensity - 1) / (maxIntensitySteps - 1));

            // Apply Nicroburst and Duralumin boosts from the cached Allomancer
            if (_cachedAllomancer != null)
            {
                if (_cachedAllomancer.isDuraluminPrimed)
                    mult *= 10f; // Duralumin: 10x burst (lore-accurate)
                if (_cachedAllomancer.isNicrobursting)
                    mult *= 3f;  // Nicroburst: 3x boost
            }

            return mult;
        }
    }

    /// <summary>
    /// Drain rate per second at current intensity.
    /// Lore: Flaring drains reserves much faster (3x-10x base rate).
    /// </summary>
    public float flareBurnRate
    {
        get
        {
            if (!IsBurning) return 0f;

            // Check if current metal can be flared
            if (!IsCurrentMetalFlarable()) return baseBurnRate;

            // Base drain at intensity 1
            float baseRate = baseBurnRate;

            // Flaring adds exponential drain
            if (Intensity > 1)
            {
                // Calculate flare drain based on intensity
                float flareMultiplier = flareDrainMultiplier + (Intensity - 1) * 0.5f;

                // Over-flaring increases drain dramatically
                if (Intensity >= overFlareThreshold)
                {
                    flareMultiplier *= overFlareDrainMultiplier;
                }

                baseRate *= flareMultiplier;
            }

            return baseRate;
        }
    }

    /// <summary>
    /// Gets the current metal being burned.
    /// </summary>
    private AllomancySkill.MetalType GetCurrentMetal()
    {
        if (_cachedAllomancer == null) return AllomancySkill.MetalType.Steel;
        return _cachedAllomancer.GetCurrentMetal();
    }

    /// <summary>
    /// Checks if the current metal can be flared.
    /// Lore: Not all metals can be flared.
    /// Physical metals (Pewter, Tin, Steel, Iron) can be flared.
    /// Mental metals, Temporal metals, and others cannot.
    /// </summary>
    private bool IsCurrentMetalFlarable()
    {
        AllomancySkill.MetalType metal = GetCurrentMetal();

        switch (metal)
        {
            // Physical metals - CAN be flared
            case AllomancySkill.MetalType.Steel:
            case AllomancySkill.MetalType.Iron:
            case AllomancySkill.MetalType.Pewter:
            case AllomancySkill.MetalType.Tin:
                return true;

            // Mental metals - CANNOT be flared
            case AllomancySkill.MetalType.Zinc:
            case AllomancySkill.MetalType.Brass:
            case AllomancySkill.MetalType.Copper:
            case AllomancySkill.MetalType.Bronze:
                return false;

            // Temporal metals - CANNOT be flared
            case AllomancySkill.MetalType.Atium:
            case AllomancySkill.MetalType.Electrum:
            case AllomancySkill.MetalType.Gold:
            case AllomancySkill.MetalType.Malatium:
                return false;

            // Time metals - CANNOT be flared
            case AllomancySkill.MetalType.Bendalloy:
            case AllomancySkill.MetalType.Cadmium:
                return false;

            // Enhancement metals - SPECIAL RULES
            case AllomancySkill.MetalType.Aluminum:
                return false; // Aluminum cannot be flared (instantly drains all)
            case AllomancySkill.MetalType.Duralumin:
                return false; // Duralumin has its own burst mechanic

            // God metals - CANNOT be flared
            case AllomancySkill.MetalType.Chromium:
            case AllomancySkill.MetalType.Nicrosil:
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the specific effect multiplier for the current metal when flaring.
    /// Lore: Different metals have different effects when flared.
    /// </summary>
    public float GetMetalFlareMultiplier()
    {
        if (!IsFlaring || !IsCurrentMetalFlarable()) return 1f;

        AllomancySkill.MetalType metal = GetCurrentMetal();

        switch (metal)
        {
            case AllomancySkill.MetalType.Pewter:
                // Pewter: immense physical strength and healing
                return FlareMultiplier * 1.5f;
            case AllomancySkill.MetalType.Tin:
                // Tin: drastically boosts all five senses
                return FlareMultiplier * 1.3f;
            case AllomancySkill.MetalType.Steel:
            case AllomancySkill.MetalType.Iron:
                // Steel/Iron: stronger pushes and pulls
                return FlareMultiplier;
            default:
                return FlareMultiplier;
        }
    }

    // ── Unity Loop ────────────────────────────────────────────────────────────

    void Update()
    {
        HandleBurnToggle();
        HandleScrollWheel();
        HandleOverFlareRisks();
        RecoverBodyStrain();
    }

    void HandleBurnToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            IsBurning = !IsBurning;
            Debug.Log($"[FLARE] Burning {(IsBurning ? "ON" : "OFF")} – intensity {Intensity}");
        }
    }

    void HandleScrollWheel()
    {
        if (!IsBurning) return;

        // Check if current metal can be flared
        if (!IsCurrentMetalFlarable() && Intensity > 1)
        {
            // If metal can't be flared, force intensity to 1
            Intensity = 1;
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        int delta = scroll > 0f ? scrollStepSize : -scrollStepSize;
        Intensity = Mathf.Clamp(Intensity + delta, 1, maxIntensitySteps);

        // Warn if over-flaring
        if (Intensity >= overFlareThreshold && IsCurrentMetalFlarable())
        {
            Debug.LogWarning($"[FLARE] WARNING: Over-flaring at intensity {Intensity}! Risk of body damage.");
        }
    }

    /// <summary>
    /// Lore: Over-flaring can strain or "break" the user's body.
    /// </summary>
    void HandleOverFlareRisks()
    {
        if (!IsBurning || !IsOverFlaring || !IsCurrentMetalFlarable()) return;

        // Increase body strain when over-flaring
        BodyStrain += overFlareDamagePerSecond * Time.deltaTime;
        BodyStrain = Mathf.Min(BodyStrain, maxBodyStrain);

        // Apply damage to player if body strain is high
        if (BodyStrain > maxBodyStrain * 0.5f)
        {
            PlayerHealth health = GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                float damage = overFlareDamagePerSecond * (BodyStrain / maxBodyStrain) * Time.deltaTime;
                health.TakeDamage(damage);
            }
        }

        // Apply Tin Savant risk when flaring Tin
        if (GetCurrentMetal() == AllomancySkill.MetalType.Tin)
        {
            // Tin Savant: long-term intense use leads to savantism
            // For now, just increase body strain faster
            BodyStrain += overFlareDamagePerSecond * 0.5f * Time.deltaTime;
        }
    }

    /// <summary>
    /// Recover body strain when not over-flaring.
    /// </summary>
    void RecoverBodyStrain()
    {
        if (BodyStrain <= 0) return;

        // Recover faster when not burning or not over-flaring
        float recoveryRate = bodyStrainRecoveryRate;

        if (!IsBurning)
        {
            recoveryRate *= 2f; // Recover faster when not burning
        }
        else if (!IsOverFlaring)
        {
            recoveryRate *= 0.5f; // Recover slower when burning but not over-flaring
        }
        else
        {
            return; // No recovery when over-flaring
        }

        BodyStrain -= recoveryRate * Time.deltaTime;
        BodyStrain = Mathf.Max(BodyStrain, 0f);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StopBurning() => IsBurning = false;

    public void SetIntensity(int v) =>
        Intensity = Mathf.Clamp(v, 1, maxIntensitySteps);

    /// <summary>
    /// Forces a flare burst (like Duralumin).
    /// </summary>
    public void ForceBurst()
    {
        Intensity = maxIntensitySteps;
        Debug.Log("[FLARE] Forced burst at maximum intensity!");
    }

    /// <summary>
    /// Gets the body strain percentage (0-1).
    /// </summary>
    public float GetBodyStrainPercent() => BodyStrain / maxBodyStrain;

    /// <summary>
    /// Checks if the player is at risk of body damage from over-flaring.
    /// </summary>
    public bool IsAtBodyDamageRisk() => BodyStrain > maxBodyStrain * 0.3f;

    // No OnGUI — FlareIntensityHUD handles all display.
    // No metal draining — Allomancer.cs owns the reserve pool.
}