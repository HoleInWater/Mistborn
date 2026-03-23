/* FlareIntensityHUD.cs
 *
 * PURPOSE:
 * Draws a 10-segment flare intensity bar in the top-right corner of the screen
 * using Unity's built-in OnGUI system — no Canvas setup required.
 *
 * SETUP:
 * 1. Attach this script to any active GameObject (e.g. the Player or a HUD object).
 * 2. Make sure FlareManager is in the scene (Allomancer.cs adds it automatically).
 * 3. That's it — the bar appears immediately at runtime.
 *
 * APPEARANCE:
 * ┌─────────────────────────────┐  ← top-right corner
 * │  FLARE                      │
 * │  [■][■][■][■][■][■][□][□][□][□] │
 * └─────────────────────────────┘
 *
 * Segments glow orange → red as intensity climbs.
 * At intensity 0 the bar fades to near-invisible (not burning = not flaring).
 *
 * CUSTOMISATION:
 * All sizing, colors, and position are exposed in the Inspector.
 */
 
using UnityEngine;
 
public class FlareIntensityHUD : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Distance from the right edge of the screen (pixels).")]
    public float marginRight = 20f;
 
    [Tooltip("Distance from the top edge of the screen (pixels).")]
    public float marginTop = 20f;
 
    [Tooltip("Width of each segment block (pixels).")]
    public float segmentWidth = 18f;
 
    [Tooltip("Height of each segment block (pixels).")]
    public float segmentHeight = 10f;
 
    [Tooltip("Gap between segments (pixels).")]
    public float segmentGap = 3f;
 
    [Tooltip("Height of the label text row above the bar (pixels).")]
    public float labelHeight = 18f;
 
    [Header("Colors")]
    [Tooltip("Segment color at low intensity (steps 1–4).")]
    public Color lowColor = new Color(1f, 0.55f, 0f, 1f);       // orange
 
    [Tooltip("Segment color at mid intensity (steps 5–7).")]
    public Color midColor = new Color(1f, 0.25f, 0f, 1f);       // deep orange
 
    [Tooltip("Segment color at high intensity (steps 8–10).")]
    public Color highColor = new Color(1f, 0.05f, 0f, 1f);      // red
 
    [Tooltip("Color of unlit segments.")]
    public Color offColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
 
    [Tooltip("Label text color.")]
    public Color labelColor = new Color(0.8f, 0.8f, 0.8f, 1f);
 
    [Tooltip("Alpha multiplier when flare intensity is 0 (bar dims but stays visible).")]
    [Range(0f, 1f)]
    public float idleAlpha = 0.35f;
 
    [Header("Options")]
    [Tooltip("Show numeric intensity next to the label.")]
    public bool showNumber = true;
 
    [Tooltip("Hide the HUD entirely when intensity is 0.")]
    public bool hideWhenIdle = false;
 
    // Cached texture for drawing solid-color rectangles
    private Texture2D _pixel;
    private GUIStyle  _labelStyle;
 
    void Start()
    {
        _pixel = new Texture2D(1, 1);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }
 
    void OnGUI()
    {
        if (FlareManager.Instance == null) return;
 
        int   intensity = FlareManager.Instance.FlareIntensity;
        int   maxSteps  = FlareManager.Instance.maxIntensitySteps;
        bool  isIdle    = intensity == 0;
 
        if (hideWhenIdle && isIdle) return;
 
        float alpha = isIdle ? idleAlpha : 1f;
 
        // ── Dimensions ────────────────────────────────────────────────────────
        int   totalSegs  = maxSteps;
        float barWidth   = totalSegs * segmentWidth + (totalSegs - 1) * segmentGap;
        float barHeight  = segmentHeight;
        float blockH     = labelHeight + barHeight + 4f;  // total HUD height
 
        float x = Screen.width  - marginRight - barWidth;
        float y = marginTop;
 
        // ── Label ─────────────────────────────────────────────────────────────
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
        }
 
        Color labelC = labelColor;
        labelC.a *= alpha;
        _labelStyle.normal.textColor = labelC;
 
        string labelText = showNumber
            ? $"FLARE  {intensity} / {maxSteps}"
            : "FLARE";
 
        GUI.Label(new Rect(x, y, barWidth, labelHeight), labelText, _labelStyle);
 
        // ── Segments ──────────────────────────────────────────────────────────
        float segY = y + labelHeight + 2f;
 
        for (int i = 0; i < totalSegs; i++)
        {
            float segX = x + i * (segmentWidth + segmentGap);
            bool  lit  = i < intensity;
 
            Color c;
            if (!lit)
            {
                c   = offColor;
                c.a *= alpha;
            }
            else
            {
                // Pick color ramp based on which segment this is
                float t = (float)(i + 1) / totalSegs; // 0.1 → 1.0
                if      (t <= 0.4f) c = lowColor;
                else if (t <= 0.7f) c = midColor;
                else                c = highColor;
 
                c.a *= alpha;
            }
 
            DrawRect(new Rect(segX, segY, segmentWidth, segmentHeight), c, 2f);
        }
    }
 
    /// <summary>
    /// Draws a solid rectangle with an optional rounded-corner illusion via
    /// a slightly inset highlight line (keeps it readable at small sizes).
    /// </summary>
    void DrawRect(Rect r, Color color, float radius = 0f)
    {
        GUI.color = color;
        GUI.DrawTexture(r, _pixel);
        GUI.color = Color.white;
    }
 
    void OnDestroy()
    {
        if (_pixel != null) Destroy(_pixel);
    }
}
 
