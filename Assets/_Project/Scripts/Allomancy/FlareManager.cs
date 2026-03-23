/* FlareManager.cs
 *
 * PURPOSE:
 * Centralized manager for Allomantic flaring. One shared intensity (1–10)
 * that applies equally to both Iron and Steel. Burning is toggled with
 * Left Ctrl; the scroll wheel adjusts intensity while burning.
 *
 * CONTROL SCHEME:
 * ===============
 * - Left Ctrl        → Toggle burning ON / OFF
 * - Scroll UP        → (while burning) Increase intensity toward 10
 * - Scroll DOWN      → (while burning) Decrease intensity toward 1
 *
 * Intensity is never 0 — burning always starts at 1 and scrolls between 1–10.
 * Turning burning off preserves the current intensity for next time.
 *
 * USAGE FROM OTHER SCRIPTS:
 * =========================
 *   FlareManager.Instance.IsBurning        // true when Left Ctrl is toggled on
 *   FlareManager.Instance.IsFlaring        // same — burning always means flaring (intensity >= 1)
 *   FlareManager.Instance.IsIronFlaring    // alias for IsBurning
 *   FlareManager.Instance.IsSteelFlaring   // alias for IsBurning
 *   FlareManager.Instance.Intensity        // 1–10
 *   FlareManager.Instance.FlareMultiplier  // 1.0 – maxFlareMultiplier (for force scaling)
 *   FlareManager.Instance.flareBurnRate    // drain per second at current intensity
 */

using UnityEngine;

public class FlareManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static FlareManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dependencies")]
    public MetalReserve metalReserve;

    [Header("Intensity Settings")]
    [Tooltip("Maximum intensity level (scroll ceiling).")]
    public int maxIntensitySteps = 10;

    [Tooltip("Intensity change per scroll tick.")]
    public int scrollStepSize = 1;

    [Header("Burn Rates")]
    [Tooltip("Drain per second at intensity 1.")]
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

    /// <summary>True when burning is active. Intensity is always >= 1 so burning = flaring.</summary>
    public bool IsFlaring      => IsBurning;
    public bool IsIronFlaring  => IsBurning;
    public bool IsSteelFlaring => IsBurning;

    /// <summary>Backward-compat alias.</summary>
    public int FlareIntensity => Intensity;

    /// <summary>
    /// Smooth force multiplier: 1.0 at intensity 1, maxFlareMultiplier at intensity 10.
    /// Returns 1.0 when not burning.
    /// </summary>
    public float FlareMultiplier =>
        IsBurning
            ? Mathf.Lerp(1f, maxFlareMultiplier, (float)(Intensity - 1) / (maxIntensitySteps - 1))
            : 1f;

    /// <summary>Metal drain per second at current intensity.</summary>
    public float flareBurnRate =>
        IsBurning ? baseBurnRate + burnRatePerStep * (Intensity - 1) : 0f;

    // ── Unity Loop ────────────────────────────────────────────────────────────

    void Update()
    {
        HandleBurnToggle();
        HandleScrollWheel();
        HandleMetalDrain();
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

    void HandleMetalDrain()
    {
        if (!IsBurning || metalReserve == null) return;

        metalReserve.Drain(flareBurnRate * Time.deltaTime);

        if (metalReserve.currentMetal <= 0)
        {
            IsBurning = false;
            Debug.Log("[FLARE] Metal exhausted – burning stopped.");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StopBurning() => IsBurning = false;

    public void SetIntensity(int v) =>
        Intensity = Mathf.Clamp(v, 1, maxIntensitySteps);

    // No OnGUI — FlareIntensityHUD handles all display.
}
