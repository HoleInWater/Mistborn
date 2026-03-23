/* FlareManager.cs
 *
 * PURPOSE:
 * Centralized manager for Allomantic flaring. Tracks flare state and intensity,
 * controlled via the scroll wheel. Intensity affects burn rate and ability force
 * in SteelPush and IronPull.
 *
 * SCROLL WHEEL CONTROL:
 * =====================
 * - Scroll UP   → Increase flare intensity (toward max flare / "burning bright")
 * - Scroll DOWN → Decrease flare intensity (toward normal burn)
 * - Intensity 0 = not flaring at all (normal burn if burning is active)
 * - Intensity 1–10 = flaring, scaling force and drain accordingly
 *
 * LEFT CTRL KEY:
 * - Instantly toggles flare ON (max intensity) or OFF (intensity 0)
 *
 * USAGE FROM OTHER SCRIPTS:
 * =========================
 *   FlareManager.Instance.IsFlaring           // true if intensity > 0
 *   FlareManager.Instance.IsIronFlaring       // same (unified flare)
 *   FlareManager.Instance.IsSteelFlaring      // same (unified flare)
 *   FlareManager.Instance.FlareIntensity      // 0–10 int
 *   FlareManager.Instance.FlareMultiplier     // 1.0–maxFlareMultiplier float, for force scaling
 *   FlareManager.Instance.flareBurnRate       // current drain rate per second
 */

using UnityEngine;

public class FlareManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
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

    [Header("Flare Intensity Settings")]
    [Tooltip("How many scroll steps to go from 0 → max intensity.")]
    public int maxIntensitySteps = 10;

    [Tooltip("How much the scroll wheel changes intensity per tick.")]
    public int scrollStepSize = 1;

    [Tooltip("Base burn rate at intensity 0 (normal burn, not flaring).")]
    public float baseBurnRate = 1f;

    [Tooltip("Additional burn rate added per intensity step on top of baseBurnRate.")]
    public float burnRatePerStep = 1.5f;

    [Tooltip("Force multiplier at max intensity (intensity 10 → this value).")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;

    [Header("Debug")]
    public bool showDebugGUI = true;

    // ── Public Read-Only State ────────────────────────────────────────────────

    /// <summary>Current flare intensity step (0 = off, maxIntensitySteps = full flare).</summary>
    public int FlareIntensity { get; private set; } = 0;

    /// <summary>True when the player is actively flaring (intensity > 0).</summary>
    public bool IsFlaring => FlareIntensity > 0;

    /// <summary>Compatibility aliases used by IronPull / SteelPush.</summary>
    public bool IsIronFlaring  => IsFlaring;
    public bool IsSteelFlaring => IsFlaring;

    /// <summary>
    /// Normalised force multiplier in range [1.0, maxFlareMultiplier].
    /// At intensity 0 → 1.0x  |  At max intensity → maxFlareMultiplier.
    /// </summary>
    public float FlareMultiplier =>
        Mathf.Lerp(1f, maxFlareMultiplier, (float)FlareIntensity / maxIntensitySteps);

    /// <summary>
    /// Current total burn rate per second (base + intensity contribution).
    /// Allomancer reads this to know how fast to drain metal.
    /// </summary>
    public float flareBurnRate =>
        baseBurnRate + burnRatePerStep * FlareIntensity;

    // ── Private ───────────────────────────────────────────────────────────────
    private bool wasCtrlToggleOn = false; // tracks toggle state for Ctrl key

    // ── Unity Loop ────────────────────────────────────────────────────────────
    void Update()
    {
        HandleScrollWheel();
        HandleCtrlToggle();
        HandleMetalDrain();
    }

    // ── Input Handlers ────────────────────────────────────────────────────────

    /// <summary>Scroll wheel adjusts flare intensity up or down.</summary>
    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            // Scroll up → more flare
            FlareIntensity = Mathf.Min(FlareIntensity + scrollStepSize, maxIntensitySteps);
        }
        else if (scroll < 0f)
        {
            // Scroll down → less flare
            FlareIntensity = Mathf.Max(FlareIntensity - scrollStepSize, 0);
        }
    }

    /// <summary>Left Ctrl instantly toggles between max flare and no flare.</summary>
    void HandleCtrlToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (IsFlaring)
                FlareIntensity = 0;
            else
                FlareIntensity = maxIntensitySteps;
        }
    }

    /// <summary>Drain the metal reserve while flaring; auto-stop if empty.</summary>
    void HandleMetalDrain()
    {
        if (!IsFlaring) return;
        if (metalReserve == null) return;

        metalReserve.Drain(flareBurnRate * Time.deltaTime);

        if (metalReserve.currentMetal <= 0)
        {
            FlareIntensity = 0;
            Debug.Log("[FLARE] Metal exhausted – flare extinguished.");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Force flare intensity to a specific step (0–max).</summary>
    public void SetFlareIntensity(int step)
    {
        FlareIntensity = Mathf.Clamp(step, 0, maxIntensitySteps);
    }

    /// <summary>Kill the flare immediately.</summary>
    public void StopFlaring()
    {
        FlareIntensity = 0;
    }

    // ── Debug GUI ─────────────────────────────────────────────────────────────
    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = IsFlaring ? new Color(1f, 0.6f, 0f) : Color.white;
        style.fontSize = 13;

        float y = 10f;
        float x = Screen.width - 220f;

        GUI.Label(new Rect(x, y, 210, 20), "═══ FLARE MANAGER ═══", style); y += 20;
        GUI.Label(new Rect(x, y, 210, 20),
            $"Intensity : {FlareIntensity} / {maxIntensitySteps}", style); y += 18;
        GUI.Label(new Rect(x, y, 210, 20),
            $"Multiplier: {FlareMultiplier:F2}x", style); y += 18;
        GUI.Label(new Rect(x, y, 210, 20),
            $"Burn Rate : {flareBurnRate:F1} /s", style); y += 18;
        GUI.Label(new Rect(x, y, 210, 20),
            $"State     : {(IsFlaring ? "FLARING 🔥" : "Normal")}", style); y += 18;
        GUI.Label(new Rect(x, y, 210, 20), "Scroll ↑↓ to adjust", style); y += 18;
        GUI.Label(new Rect(x, y, 210, 20), "L-Ctrl = toggle max/off", style);
    }
}
