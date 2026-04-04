/* FlareManager.cs
 *
 * PURPOSE:
 * Manages burn state and flare intensity (1–10). Does NOT touch metal reserves
 * directly — Metallurgist.cs owns the single reserve pool and handles all draining.
 * Switching intensity just changes how fast Metallurgist drains that one pool.
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
 *   FlareManager.Instance.IsFlaring        // alias for IsBurning
 *   FlareManager.Instance.IsIronFlaring    // alias for IsBurning
 *   FlareManager.Instance.IsSteelFlaring   // alias for IsBurning
 *   FlareManager.Instance.Intensity        // 1–10
 *   FlareManager.Instance.FlareMultiplier  // 1.0–maxFlareMultiplier (for force scaling)
 *   FlareManager.Instance.flareBurnRate    // drain/sec passed to Metallurgist
 */

using UnityEngine;

[PlayerComponent("Metallurgy", order: 20)]
public class FlareManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static FlareManager Instance { get; private set; }

    private Metallurgist _cachedMetallurgist;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _cachedMetallurgist = GetComponentInParent<Metallurgist>();
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Intensity Settings")]
    [Tooltip("Maximum intensity level (scroll ceiling).")]
    public int maxIntensitySteps = 10;

    [Tooltip("Intensity change per scroll tick.")]
    public int scrollStepSize = 1;

    [Header("Burn Rates")]
    // Both zeroed — individual metal scripts handle their own MAG-calibrated drain.
    // flareBurnRate is still computed and readable but evaluates to 0.
    [Tooltip("Drain per second passed to Metallurgist. Set to 0 — metal scripts own their drain.")]
    public float baseBurnRate = 0f;

    [Tooltip("Additional drain per intensity step. Set to 0 — metal scripts own flare scaling.")]
    public float burnRatePerStep = 0f;

    [Header("Force Scaling")]
    [Tooltip("Force multiplier at intensity 10.")]
    [Range(1f, 10f)]
    public float maxFlareMultiplier = 3.2f;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Whether burning is toggled on (Left Ctrl).</summary>
    public bool IsBurning { get; private set; } = false;

    /// <summary>Shared intensity 1–10. Persists when burning is toggled off.</summary>
    public int Intensity { get; private set; } = 1;

    // ── Derived Properties ────────────────────────────────────────────────────

    public bool IsFlaring      => IsBurning;
    public bool IsIronFlaring  => IsBurning;
    public bool IsSteelFlaring => IsBurning;

    /// <summary>Backward-compat aliases.</summary>
    public int FlareIntensity => Intensity;
    public int flareIntensity => Intensity;

    /// <summary>
    /// Force multiplier: 1.0 at intensity 1, maxFlareMultiplier at intensity 10.
    /// Returns 1.0 when not burning.
    /// </summary>
    public float FlareMultiplier
    {
        get
        {
            float mult = IsBurning && maxIntensitySteps > 1
                ? Mathf.Lerp(1f, maxFlareMultiplier, (float)(Intensity - 1) / (maxIntensitySteps - 1))
                : 1f;

            // Apply Nicroburst and Duralumin boosts from the cached Metallurgist
            if (_cachedMetallurgist != null && IsBurning)
            {
                if (_cachedMetallurgist.isDuraluminPrimed)
                    mult *= 10f; // Duralumin: 10x burst
                if (_cachedMetallurgist.isNicrobursting)
                    mult *= 3f;  // Nicroburst: 3x boost
            }
            return mult;
        }
    }


    /// <summary>
    /// Drain rate per second at current intensity.
    /// Read by Metallurgist.Update() — it drains the actual reserve pool.
    /// </summary>
    public float flareBurnRate =>
        IsBurning ? baseBurnRate + burnRatePerStep * (Intensity - 1) : 0f;

    // ── Unity Loop ────────────────────────────────────────────────────────────

    void Update()
    {
        HandleBurnToggle();
        HandleScrollWheel();
    }

    void HandleBurnToggle()
    {
        if (Input.GetKeyDown(Keybinds.Crouch))
        {
            IsBurning = !IsBurning;
            // Keep Metallurgist burn state in sync so B-key and Left-Ctrl agree
            if (_cachedMetallurgist == null) _cachedMetallurgist = GetComponentInParent<Metallurgist>();
            if (_cachedMetallurgist != null)
            {
                if (IsBurning) _cachedMetallurgist.StartBurning(_cachedMetallurgist.GetCurrentMetal());
                else           _cachedMetallurgist.StopBurning();
            }
        }
    }

    void HandleScrollWheel()
    {
        if (!IsBurning) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        int delta = scroll > 0f ? scrollStepSize : -scrollStepSize;
        Intensity = Mathf.Clamp(Intensity + delta, 1, maxIntensitySteps);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StopBurning() => IsBurning = false;
    public void SetBurning(bool state) => IsBurning = state;

    public void SetIntensity(int v) =>
        Intensity = Mathf.Clamp(v, 1, maxIntensitySteps);

    // No OnGUI — FlareIntensityHUD handles all display.
    // No metal draining — Metallurgist.cs owns the reserve pool.
}
