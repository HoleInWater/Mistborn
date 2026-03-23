/* FlareManager.cs
 *
 * PURPOSE:
 * Centralized manager for Allomantic flaring. Controls burn state and flare intensity.
 *
 * CONTROL SCHEME:
 * ===============
 * - Left Ctrl          → Toggle burning ON / OFF
 * - Scroll wheel UP    → (while burning) Increase flare intensity
 * - Scroll wheel DOWN  → (while burning) Decrease flare intensity
 *
 * Flare intensity only changes while actively burning.
 * Stopping burn via Ctrl preserves the last intensity so it resumes where you left off.
 *
 * USAGE FROM OTHER SCRIPTS:
 * =========================
 *   FlareManager.Instance.IsFlaring          // true if burning AND intensity > 0
 *   FlareManager.Instance.IsIronFlaring      // alias
 *   FlareManager.Instance.IsSteelFlaring     // alias
 *   FlareManager.Instance.IsBurning          // true if Ctrl is toggled on
 *   FlareManager.Instance.FlareIntensity     // 0–10
 *   FlareManager.Instance.FlareMultiplier    // 1.0 – maxFlareMultiplier (for force scaling)
 *   FlareManager.Instance.flareBurnRate      // drain per second at current intensity
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
    [Tooltip("Reference to the MetalReserve UI/data component.")]
    public MetalReserve metalReserve;

    [Header("Flare Settings")]
    [Tooltip("How many scroll steps between 0 and max intensity.")]
    public int maxIntensitySteps = 10;

    [Tooltip("How much each scroll tick changes intensity.")]
    public int scrollStepSize = 1;

    [Tooltip("Base drain per second while burning at intensity 0.")]
    public float baseBurnRate = 1f;

    [Tooltip("Extra drain per second added per intensity step.")]
    public float burnRatePerStep = 1.5f;

    [Tooltip("Force multiplier at max intensity.")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;

    // ── Public State ──────────────────────────────────────────────────────────

    /// <summary>Whether the player has burning toggled on (Ctrl key).</summary>
    public bool IsBurning { get; private set; } = false;

    /// <summary>Current flare intensity (0 = burning but not flaring, max = full flare).</summary>
    public int FlareIntensity { get; private set; } = 0;

    /// <summary>True when burning AND intensity > 0.</summary>
    public bool IsFlaring      => IsBurning && FlareIntensity > 0;
    public bool IsIronFlaring  => IsFlaring;
    public bool IsSteelFlaring => IsFlaring;

    /// <summary>Smooth force multiplier: 1.0 at intensity 0, maxFlareMultiplier at max.</summary>
    public float FlareMultiplier =>
        IsBurning
            ? Mathf.Lerp(1f, maxFlareMultiplier, (float)FlareIntensity / maxIntensitySteps)
            : 1f;

    /// <summary>Total drain per second at current state (used by Allomancer).</summary>
    public float flareBurnRate =>
        IsBurning ? baseBurnRate + burnRatePerStep * FlareIntensity : 0f;

    // ── Unity Loop ────────────────────────────────────────────────────────────

    void Update()
    {
        HandleCtrlToggle();
        HandleScrollWheel();
        HandleMetalDrain();
    }

    void HandleCtrlToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            IsBurning = !IsBurning;
            Debug.Log($"[FLARE] Burning {(IsBurning ? "ON" : "OFF")} – intensity {FlareIntensity}");
        }
    }

    void HandleScrollWheel()
    {
        // Scroll only adjusts intensity while actively burning
        if (!IsBurning) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
            FlareIntensity = Mathf.Min(FlareIntensity + scrollStepSize, maxIntensitySteps);
        else if (scroll < 0f)
            FlareIntensity = Mathf.Max(FlareIntensity - scrollStepSize, 0);
    }

    void HandleMetalDrain()
    {
        if (!IsBurning) return;
        if (metalReserve == null) return;

        metalReserve.Drain(flareBurnRate * Time.deltaTime);

        if (metalReserve.currentMetal <= 0)
        {
            IsBurning      = false;
            FlareIntensity = 0;
            Debug.Log("[FLARE] Metal exhausted – burning stopped.");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void SetFlareIntensity(int step) =>
        FlareIntensity = Mathf.Clamp(step, 0, maxIntensitySteps);

    public void StopFlaring()
    {
        IsBurning      = false;
        FlareIntensity = 0;
    }

    // No OnGUI here — FlareIntensityHUD.cs handles all display.
}
