/* AllomanticTitleRenderer.cs
 *
 * Draws "MISTBORN" in semi-transparent glowing blue lines at the rock drop.
 *
 * The letters are "traced" left to right as if invisible Allomantic steel lines
 * are drawing them into existence. Each character reveals progressively — the
 * leading edge is bright white-blue, the trail settles to translucent blue.
 *
 * After all letters are drawn, a single flare pulse ripples across the title,
 * then it settles into a gentle pulsing glow (Allomantic energy).
 */

using UnityEngine;
using System.Collections;
using TMPro;

public class AllomanticTitleRenderer : MonoBehaviour
{
    [Header("Title")]
    public TextMeshProUGUI titleText;
    public string titleString = "MISTBORN";

    [Header("Allomantic Line Colors")]
    [Tooltip("Settled color — semi-transparent blue, like steel lines.")]
    public Color blueLineColor = new Color(0.27f, 0.53f, 1f, 0.6f);
    [Tooltip("Leading-edge color — bright white-blue as the line is being drawn.")]
    public Color traceColor = new Color(0.7f, 0.85f, 1f, 0.95f);
    [Tooltip("Flare color — bright pulse after all letters are drawn.")]
    public Color flareColor = new Color(0.6f, 0.82f, 1f, 0.95f);

    [Header("Draw Animation")]
    public float drawDuration = 3f;
    [Tooltip("How long the leading edge glow lingers before settling to blue.")]
    public float traceSettleTime = 0.4f;

    [Header("Post-Draw Glow")]
    [Tooltip("Subtle energy pulse frequency after the title is fully drawn.")]
    public float pulseHz = 1.2f;
    [Range(0f, 0.3f)]
    public float pulseStrength = 0.1f;
    [Tooltip("Duration of the single flare that fires right after drawing completes.")]
    public float flareDuration = 0.6f;

    [Header("Subtitle")]
    public TextMeshProUGUI subtitleText;
    public string subtitleString = "";
    public float subtitleDelay = 1.5f;
    public float subtitleFadeDuration = 1f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool isDrawing;
    private bool drawComplete;
    private float[] charRevealTime;   // Time.time when each char started revealing

    void Awake()
    {
        if (titleText != null)
        {
            titleText.text = titleString;
            titleText.ForceMeshUpdate();
            SetAllCharsAlpha(0f);
        }
    }

    /// <summary>Called by TitleSequenceController at the rock drop.</summary>
    public void StartDrawing(float duration)
    {
        if (isDrawing) return;
        drawDuration = duration;
        StartCoroutine(DrawSequence());
    }

    IEnumerator DrawSequence()
    {
        isDrawing = true;

        if (titleText == null) yield break;

        titleText.text = titleString;
        titleText.ForceMeshUpdate();

        TMP_TextInfo info = titleText.textInfo;
        int count = titleString.Length;
        charRevealTime = new float[count];

        // All characters start invisible
        SetAllCharsAlpha(0f);

        float perChar = drawDuration / Mathf.Max(1, count);
        float startTime = Time.time;

        // ── Trace phase: reveal characters left to right ─────────────────
        for (int i = 0; i < count; i++)
        {
            charRevealTime[i] = Time.time;
            float charEnd = Time.time + perChar;

            while (Time.time < charEnd)
            {
                float t = (Time.time - charRevealTime[i]) / perChar; // 0→1

                // Current character: traces in from transparent to bright white-blue
                Color leading = Color.Lerp(
                    new Color(traceColor.r, traceColor.g, traceColor.b, 0f),
                    traceColor,
                    t
                );
                SetCharColor(i, leading, info);

                // Already-drawn characters: settle from trace white-blue → blue line
                for (int j = 0; j < i; j++)
                {
                    float age = Time.time - charRevealTime[j];
                    float settle = Mathf.Clamp01(age / traceSettleTime);
                    Color c = Color.Lerp(traceColor, blueLineColor, settle);
                    SetCharColor(j, c, info);
                }

                titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return null;
            }

            // Finalize current char at trace color (will settle next frame)
            SetCharColor(i, traceColor, info);
            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        // Let the last few characters settle
        float settleEnd = Time.time + traceSettleTime;
        while (Time.time < settleEnd)
        {
            for (int i = 0; i < count; i++)
            {
                float age = Time.time - charRevealTime[i];
                float settle = Mathf.Clamp01(age / traceSettleTime);
                SetCharColor(i, Color.Lerp(traceColor, blueLineColor, settle), info);
            }
            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        // All at settled blue
        for (int i = 0; i < count; i++)
            SetCharColor(i, blueLineColor, info);
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // ── Flare pulse ──────────────────────────────────────────────────
        yield return FlareAllChars(info, count);

        drawComplete = true;

        // Subtitle fade-in
        if (subtitleText != null && !string.IsNullOrEmpty(subtitleString))
        {
            yield return new WaitForSeconds(subtitleDelay);
            subtitleText.text = subtitleString;
            float elapsed = 0f;
            while (elapsed < subtitleFadeDuration)
            {
                elapsed += Time.deltaTime;
                subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g,
                    subtitleText.color.b, Mathf.Lerp(0f, 1f, elapsed / subtitleFadeDuration));
                yield return null;
            }
        }

        isDrawing = false;
    }

    /// <summary>
    /// Single bright pulse that ripples across the title then settles back.
    /// </summary>
    IEnumerator FlareAllChars(TMP_TextInfo info, int count)
    {
        float elapsed = 0f;
        while (elapsed < flareDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flareDuration;

            // Fast rise (30%), slow decay (70%)
            float intensity = t < 0.3f
                ? t / 0.3f
                : 1f - ((t - 0.3f) / 0.7f);

            Color c = Color.Lerp(blueLineColor, flareColor, intensity);
            for (int i = 0; i < count; i++)
                SetCharColor(i, c, info);
            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        // Settle back to blue
        for (int i = 0; i < count; i++)
            SetCharColor(i, blueLineColor, info);
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    void Update()
    {
        // Gentle Allomantic energy pulse after drawing is done
        if (!drawComplete || titleText == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseHz * Mathf.PI * 2f) * pulseStrength;
        TMP_TextInfo info = titleText.textInfo;

        Color pulsed = new Color(
            blueLineColor.r * pulse,
            blueLineColor.g * pulse,
            blueLineColor.b * pulse,
            blueLineColor.a
        );

        for (int i = 0; i < titleString.Length; i++)
            SetCharColor(i, pulsed, info);

        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    void SetCharColor(int index, Color color, TMP_TextInfo info)
    {
        if (index >= info.characterCount) return;
        TMP_CharacterInfo ch = info.characterInfo[index];
        if (!ch.isVisible) return;

        Color32 c32 = color;
        Color32[] colors = info.meshInfo[ch.materialReferenceIndex].colors32;
        int v = ch.vertexIndex;
        colors[v] = c32;
        colors[v + 1] = c32;
        colors[v + 2] = c32;
        colors[v + 3] = c32;
    }

    void SetAllCharsAlpha(float alpha)
    {
        if (titleText == null) return;
        TMP_TextInfo info = titleText.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            Color c = new Color(blueLineColor.r, blueLineColor.g, blueLineColor.b, alpha);
            SetCharColor(i, c, info);
        }
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
