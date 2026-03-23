/* FlareIntensityHUD.cs
 *
 * PURPOSE:
 * Draws the flare intensity bar in the top-right corner of the screen.
 * Uses Unity's OnGUI — no Canvas setup required.
 *
 * SETUP:
 * Attach to any active GameObject. Reads FlareManager.Instance automatically.
 *
 * DISPLAY STATES:
 * - Not burning  → bar is dimmed, label reads "BURNING OFF"
 * - Burning, intensity 0  → bar is active but all segments unlit, label reads "BURNING"
 * - Burning, intensity >0 → segments fill left→right, orange→red, label reads "FLARING  N / 10"
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

    [Tooltip("Height of the label row above the bar (pixels).")]
    public float labelHeight = 18f;

    [Header("Colors")]
    [Tooltip("Segment color at low intensity (steps 1–4).")]
    public Color lowColor  = new Color(1f, 0.55f, 0f, 1f);   // orange

    [Tooltip("Segment color at mid intensity (steps 5–7).")]
    public Color midColor  = new Color(1f, 0.25f, 0f, 1f);   // deep orange

    [Tooltip("Segment color at high intensity (steps 8–10).")]
    public Color highColor = new Color(1f, 0.05f, 0f, 1f);   // red

    [Tooltip("Color of unlit segments.")]
    public Color offColor  = new Color(0.15f, 0.15f, 0.15f, 0.8f);

    [Tooltip("Label color while burning.")]
    public Color labelColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    [Tooltip("Label color while NOT burning (dimmed).")]
    public Color labelOffColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Tooltip("Alpha applied to the whole HUD when not burning.")]
    [Range(0f, 1f)]
    public float idleAlpha = 0.3f;

    [Header("Options")]
    [Tooltip("Show N / 10 count next to label.")]
    public bool showNumber = true;

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

        bool isBurning  = FlareManager.Instance.IsBurning;
        bool isFlaring  = FlareManager.Instance.IsFlaring;
        int  intensity  = FlareManager.Instance.FlareIntensity;
        int  maxSteps   = FlareManager.Instance.maxIntensitySteps;

        float alpha = isBurning ? 1f : idleAlpha;

        // ── Dimensions ────────────────────────────────────────────────────────
        float barWidth = maxSteps * segmentWidth + (maxSteps - 1) * segmentGap;
        float x = Screen.width - marginRight - barWidth;
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

        string labelText;
        Color  lc;

        if (!isBurning)
        {
            labelText = "BURNING OFF";
            lc        = labelOffColor;
        }
        else if (intensity == 0)
        {
            labelText = "BURNING";
            lc        = labelColor;
        }
        else
        {
            labelText = showNumber ? $"FLARING  {intensity} / {maxSteps}" : "FLARING";
            lc        = labelColor;
        }

        lc.a *= alpha;
        _labelStyle.normal.textColor = lc;
        GUI.Label(new Rect(x, y, barWidth, labelHeight), labelText, _labelStyle);

        // ── Segments ──────────────────────────────────────────────────────────
        float segY = y + labelHeight + 2f;

        for (int i = 0; i < maxSteps; i++)
        {
            float segX = x + i * (segmentWidth + segmentGap);
            bool  lit  = isBurning && i < intensity;

            Color c;
            if (!lit)
            {
                c   = offColor;
                c.a *= alpha;
            }
            else
            {
                float t = (float)(i + 1) / maxSteps;
                c = t <= 0.4f ? lowColor : t <= 0.7f ? midColor : highColor;
                // no alpha reduction — lit segments are always full brightness
            }

            GUI.color = c;
            GUI.DrawTexture(new Rect(segX, segY, segmentWidth, segmentHeight), _pixel);
            GUI.color = Color.white;
        }
    }

    void OnDestroy()
    {
        if (_pixel != null) Destroy(_pixel);
    }
}
