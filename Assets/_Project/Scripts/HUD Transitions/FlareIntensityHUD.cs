/* FlareIntensityHUD.cs
 *
 * PURPOSE:
 * Draws a single 10-segment flare intensity bar in the top-right corner.
 * Reflects the one shared intensity used by both Iron and Steel.
 *
 * SETUP:
 * Attach to any active GameObject. Reads FlareManager.Instance automatically.
 *
 * DISPLAY STATES:
 * - Not burning  → bar dimmed, label reads "FLARE"
 * - Burning      → segments fill 1–10 (orange → red), label reads "FLARE  4 / 10"
 */

using UnityEngine;

public class FlareIntensityHUD : MonoBehaviour
{
    [Header("Layout")]
    public float marginRight   = 20f;
    public float marginTop     = 20f;
    public float segmentWidth  = 18f;
    public float segmentHeight = 10f;
    public float segmentGap    = 3f;
    public float labelHeight   = 18f;

    [Header("Colors")]
    public Color lowColor  = new Color(1f, 0.55f, 0f, 1f);           // orange
    public Color midColor  = new Color(1f, 0.25f, 0f, 1f);           // deep orange
    public Color highColor = new Color(1f, 0.05f, 0f, 1f);           // red
    public Color offColor  = new Color(0.15f, 0.15f, 0.15f, 0.8f);  // unlit segment
    public Color labelOnColor  = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color labelOffColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Tooltip("Alpha of the whole HUD when not burning.")]
    [Range(0f, 1f)]
    public float idleAlpha = 0.3f;

    [Header("Options")]
    public bool showNumber = true;

    // ── Private ───────────────────────────────────────────────────────────────
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

        bool isBurning = FlareManager.Instance.IsBurning;
        int  intensity = FlareManager.Instance.Intensity;
        int  maxSteps  = FlareManager.Instance.maxIntensitySteps;
        float alpha    = isBurning ? 1f : idleAlpha;

        float barWidth = maxSteps * segmentWidth + (maxSteps - 1) * segmentGap;
        float x        = Screen.width - marginRight - barWidth;
        float y        = marginTop;

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

        Color lc = isBurning ? labelOnColor : labelOffColor;
        lc.a *= alpha;
        _labelStyle.normal.textColor = lc;

        string labelText = isBurning && showNumber
            ? $"FLARE  {intensity} / {maxSteps}"
            : "FLARE";

        GUI.Label(new Rect(x, y, barWidth, labelHeight), labelText, _labelStyle);

        // ── Segments ──────────────────────────────────────────────────────────
        float segY = y + labelHeight + 2f;

        for (int i = 0; i < maxSteps; i++)
        {
            float segX = x + i * (segmentWidth + segmentGap);
            bool  lit  = isBurning && (i + 1) <= intensity;

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
