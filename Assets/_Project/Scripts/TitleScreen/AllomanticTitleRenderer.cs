/* AllomanticTitleRenderer.cs
 *
 * PURPOSE:
 * Draws the "MISTBORN" title using animated, semi-transparent glowing blue lines
 * that mimic Allomantic sight (steel lines). Each letter is "drawn" stroke by stroke
 * as if traced by invisible Allomantic forces.
 *
 * SETUP:
 *   1. Attach to a UI GameObject under the title CanvasGroup.
 *   2. Assign the line material (additive/glow shader recommended).
 *   3. The component uses UI.Graphic for rendering — works with Canvas scaling.
 *   4. Call StartDrawing(duration) from TitleSequenceController at the rock drop.
 *
 * VISUAL STYLE:
 *   - Lines are semi-transparent blue (#4488FF at ~60% opacity)
 *   - Outer glow pulses subtly (Allomantic energy)
 *   - Lines "draw in" from left to right, stroke by stroke
 *   - After fully drawn, a brief flare brightens the whole title
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class AllomanticTitleRenderer : MonoBehaviour
{
    [Header("Title Text")]
    [Tooltip("TMP text with the title. Font should support the desired style.")]
    public TextMeshProUGUI titleText;
    [Tooltip("The title string to display")]
    public string titleString = "MISTBORN";

    [Header("Allomantic Line Colors")]
    public Color lineColor = new Color(0.27f, 0.53f, 1f, 0.6f);       // Blue, semi-transparent
    public Color glowColor = new Color(0.4f, 0.65f, 1f, 0.3f);        // Outer glow
    public Color flareColor = new Color(0.6f, 0.8f, 1f, 0.9f);        // Flare peak

    [Header("Animation")]
    [Tooltip("Total time to draw all letters (seconds)")]
    public float drawDuration = 3f;
    [Tooltip("Duration of the post-draw flare pulse")]
    public float flareDuration = 0.8f;
    [Tooltip("Glow pulse frequency (Hz) — subtle energy shimmer")]
    public float glowPulseFrequency = 1.5f;
    [Tooltip("Glow pulse amplitude (0–1)")]
    [Range(0f, 0.5f)] public float glowPulseAmplitude = 0.15f;

    [Header("Line Renderers (Optional)")]
    [Tooltip("If assigned, draws decorative blue lines radiating from the title")]
    public List<LineRenderer> decorativeLines;
    public float decorativeLineLength = 2f;

    [Header("Subtitle")]
    public TextMeshProUGUI subtitleText;
    public string subtitleString = "";
    public float subtitleDelay = 1.5f;
    public float subtitleFadeDuration = 1f;

    private bool isDrawing;
    private bool drawComplete;
    private float drawProgress;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start invisible
        if (titleText != null)
        {
            titleText.text = titleString;
            titleText.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            titleText.ForceMeshUpdate();
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitleString;
            subtitleText.color = new Color(1f, 1f, 1f, 0f);
        }

        HideDecorativeLines();
    }

    public void StartDrawing(float duration)
    {
        if (isDrawing) return;
        drawDuration = duration;
        StartCoroutine(DrawSequence());
    }

    IEnumerator DrawSequence()
    {
        isDrawing = true;
        drawProgress = 0f;

        if (titleText == null) yield break;

        titleText.text = titleString;
        titleText.ForceMeshUpdate();

        int totalChars = titleString.Length;
        TMP_TextInfo textInfo = titleText.textInfo;

        // Phase 1: Draw each character stroke by stroke
        float perCharDuration = drawDuration / Mathf.Max(1, totalChars);

        for (int i = 0; i < totalChars; i++)
        {
            float charStart = Time.time;

            // Reveal this character with a line-draw effect
            while (Time.time - charStart < perCharDuration)
            {
                float t = (Time.time - charStart) / perCharDuration;
                drawProgress = (i + t) / totalChars;

                // Set character alpha based on draw progress
                UpdateCharacterVisibility(i, t, textInfo);

                // Pulse glow on already-drawn characters
                UpdateGlowPulse(i, textInfo);

                yield return null;
            }

            // Finalize this character at full line color
            SetCharacterColor(i, lineColor, textInfo);
            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        drawComplete = true;

        // Phase 2: Flare — brief brightness pulse across entire title
        yield return StartCoroutine(FlareTitle());

        // Phase 3: Show decorative radiating lines
        yield return StartCoroutine(RevealDecorativeLines());

        // Phase 4: Subtitle fade-in
        if (subtitleText != null && !string.IsNullOrEmpty(subtitleString))
        {
            yield return new WaitForSeconds(subtitleDelay);
            yield return StartCoroutine(FadeInSubtitle());
        }

        isDrawing = false;
    }

    void UpdateCharacterVisibility(int charIndex, float progress, TMP_TextInfo textInfo)
    {
        // Current character fades in with the blue line color
        Color currentColor = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * progress);
        SetCharacterColor(charIndex, currentColor, textInfo);
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    void UpdateGlowPulse(int upToChar, TMP_TextInfo textInfo)
    {
        // Subtle energy shimmer on already-drawn characters
        float pulse = 1f + Mathf.Sin(Time.time * glowPulseFrequency * Mathf.PI * 2f) * glowPulseAmplitude;

        for (int i = 0; i < upToChar; i++)
        {
            Color pulsed = new Color(
                lineColor.r * pulse,
                lineColor.g * pulse,
                lineColor.b * pulse,
                lineColor.a
            );
            SetCharacterColor(i, pulsed, textInfo);
        }
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    void SetCharacterColor(int charIndex, Color color, TMP_TextInfo textInfo)
    {
        if (charIndex >= textInfo.characterCount) return;
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int meshIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] colors = textInfo.meshInfo[meshIndex].colors32;

        Color32 c32 = color;
        colors[vertexIndex + 0] = c32;
        colors[vertexIndex + 1] = c32;
        colors[vertexIndex + 2] = c32;
        colors[vertexIndex + 3] = c32;
    }

    IEnumerator FlareTitle()
    {
        float elapsed = 0f;
        TMP_TextInfo textInfo = titleText.textInfo;

        while (elapsed < flareDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flareDuration;

            // Sharp rise, slow decay
            float flareIntensity = t < 0.3f
                ? Mathf.Lerp(0f, 1f, t / 0.3f)
                : Mathf.Lerp(1f, 0f, (t - 0.3f) / 0.7f);

            Color current = Color.Lerp(lineColor, flareColor, flareIntensity);

            for (int i = 0; i < titleString.Length; i++)
                SetCharacterColor(i, current, textInfo);

            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        // Settle to final line color
        for (int i = 0; i < titleString.Length; i++)
            SetCharacterColor(i, lineColor, titleText.textInfo);
        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    IEnumerator RevealDecorativeLines()
    {
        if (decorativeLines == null || decorativeLines.Count == 0) yield break;

        float duration = 0.5f;
        float elapsed = 0f;

        foreach (var lr in decorativeLines)
        {
            if (lr == null) continue;
            lr.gameObject.SetActive(true);
            lr.startColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            lr.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (var lr in decorativeLines)
            {
                if (lr == null) continue;
                lr.startColor = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * t);
                lr.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * t);
            }
            yield return null;
        }
    }

    IEnumerator FadeInSubtitle()
    {
        if (subtitleText == null) yield break;

        subtitleText.text = subtitleString;
        float elapsed = 0f;

        while (elapsed < subtitleFadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, elapsed / subtitleFadeDuration);
            subtitleText.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
    }

    void HideDecorativeLines()
    {
        if (decorativeLines == null) return;
        foreach (var lr in decorativeLines)
        {
            if (lr != null) lr.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Continuous glow pulse after drawing is complete
        if (drawComplete && titleText != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * glowPulseFrequency * Mathf.PI * 2f) * glowPulseAmplitude;
            TMP_TextInfo textInfo = titleText.textInfo;

            for (int i = 0; i < titleString.Length; i++)
            {
                Color pulsed = new Color(
                    lineColor.r * pulse,
                    lineColor.g * pulse,
                    lineColor.b * pulse,
                    lineColor.a
                );
                SetCharacterColor(i, pulsed, textInfo);
            }
            titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
