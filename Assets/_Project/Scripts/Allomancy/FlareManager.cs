/* FlareManager.cs
 *
 * PURPOSE:
 * Manages burn state and flare intensity (1–10). Does NOT touch metal reserves
 * directly — Allomancer.cs owns the single reserve pool and handles all draining.
 * Switching intensity just changes how fast Allomancer drains that one pool.
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
 *   FlareManager.Instance.flareBurnRate    // drain/sec passed to Allomancer
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

    [Header("Force Scaling")]
    [Tooltip("Force multiplier at intensity 10.")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;

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
            float mult = IsBurning
                ? Mathf.Lerp(1f, maxFlareMultiplier, (float)(Intensity - 1) / (maxIntensitySteps - 1))
                : 1f;

            // Apply Nicroburst and Duralumin boosts from the cached Allomancer
            if (_cachedAllomancer != null && IsBurning)
            {
                if (_cachedAllomancer.isDuraluminPrimed)
                    mult *= 10f; // Duralumin: 10x burst
                if (_cachedAllomancer.isNicrobursting)
                    mult *= 3f;  // Nicroburst: 3x boost
            }
            return mult;
        }
    }


    /// <summary>
    /// Drain rate per second at current intensity.
    /// Read by Allomancer.Update() — it drains the actual reserve pool.
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
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            IsBurning = !IsBurning;
            Debug.Log($"[FLARE] Burning {(IsBurning ? "ON" : "OFF")} – intensity {Intensity}");
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

    public void SetIntensity(int v) =>
        Intensity = Mathf.Clamp(v, 1, maxIntensitySteps);

    // No OnGUI — FlareIntensityHUD handles all display.
    // No metal draining — Allomancer.cs owns the reserve pool.
}
