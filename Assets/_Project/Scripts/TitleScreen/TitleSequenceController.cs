/* TitleSequenceController.cs
 *
 * PURPOSE:
 * Drives the cinematic title sequence synced to the main theme audio track.
 * All timings are in seconds from audio start, set in the Inspector so designers
 * can tune to the exact beat without recompiling.
 *
 * SEQUENCE OVERVIEW (default timings, adjust in Inspector):
 *   0 – 9s     Fade from black → misty field, ash falls in distance
 *   ~9s        Percussion enters → company logo animation
 *   ~28s       Drums pick up → Luthadel street scenes + rolling credits
 *   ~45s       Build → Kredik Shaw panoramic pan + "proudly presents"
 *   Drop       Rock drop → MISTBORN title in glowing blue Allomantic lines
 *
 * SETUP:
 *   1. Create a "TitleSequence" scene with this on a manager GameObject.
 *   2. Assign the main theme AudioClip.
 *   3. Assign CanvasGroup references for each visual layer (overlay, logos, credits, title).
 *   4. Assign camera Animators or Cinemachine brains for each camera cut.
 *   5. Populate the creditLines list with text + timing pairs.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TitleSequenceController : MonoBehaviour
{
    // ── Audio ────────────────────────────────────────────────────────────────
    [Header("Audio")]
    [Tooltip("Main theme track. Sequence timings are relative to this clip.")]
    public AudioClip mainThemeClip;
    public AudioSource musicSource;
    [Range(0f, 1f)] public float musicVolume = 1f;

    // ── Timing Cues (seconds from audio start) ──────────────────────────────
    [Header("Timing Cues — Sync to Main Theme Beats")]
    [Tooltip("Seconds: black screen fades to misty field")]
    public float fadeInDuration = 9f;

    [Tooltip("Seconds: percussion enters, company logo starts")]
    public float logoStartTime = 9f;
    public float logoDuration = 5f;

    [Tooltip("Seconds: drums pick up, Luthadel street scenes + credit lines")]
    public float streetsStartTime = 28f;

    [Tooltip("Seconds: Kredik Shaw panoramic pan begins")]
    public float panoramicStartTime = 45f;

    [Tooltip("Seconds: rock drop — MISTBORN title appears")]
    public float titleDropTime = 60f;
    public float titleAnimDuration = 3f;

    [Tooltip("Seconds after title fully visible: transition to main menu")]
    public float postTitleHold = 4f;

    // ── Visual Layers ────────────────────────────────────────────────────────
    [Header("Visual Layers")]
    [Tooltip("Full-screen black overlay for fade-in")]
    public CanvasGroup blackOverlay;

    [Tooltip("Misty field background (enabled at start, fades in behind black)")]
    public GameObject mistyFieldScene;

    [Tooltip("Company logo UI group (Crimson Blade Interactive + optional Sanderson)")]
    public CanvasGroup companyLogoGroup;
    public Animator companyLogoAnimator;

    [Tooltip("Luthadel streets camera/scene group")]
    public GameObject luthadel​StreetsGroup;

    [Tooltip("Kredik Shaw panoramic camera/scene group")]
    public GameObject kredikShawGroup;

    [Tooltip("CanvasGroup for the MISTBORN title (AllomanticTitleRenderer lives here)")]
    public CanvasGroup titleGroup;

    // ── Credits ──────────────────────────────────────────────────────────────
    [Header("Rolling Credits")]
    [Tooltip("TMP text element used to display credit lines one at a time")]
    public TextMeshProUGUI creditText;
    public CanvasGroup creditTextGroup;
    public float creditFadeDuration = 0.8f;
    public float creditHoldDuration = 3f;

    [Tooltip("Credit lines displayed in order during the streets + panoramic phases")]
    public List<CreditLine> creditLines = new List<CreditLine>();

    [Serializable]
    public class CreditLine
    {
        [Tooltip("Time (seconds from audio start) when this credit appears")]
        public float time;
        [TextArea] public string text;
    }

    // ── Scene Transition ─────────────────────────────────────────────────────
    [Header("Scene Transition")]
    [Tooltip("Scene to load after title sequence (main menu or gameplay)")]
    public string nextSceneName = "MainMenu";
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Escape;
    public KeyCode skipKeyAlt = KeyCode.Space;

    // ── Private State ────────────────────────────────────────────────────────
    private float sequenceTime;
    private bool sequenceComplete;
    private bool isSkipping;
    private Coroutine creditCoroutine;

    // Track which phases have been triggered
    private bool logoTriggered;
    private bool streetsTriggered;
    private bool panoramicTriggered;
    private bool titleTriggered;

    void Start()
    {
        // Initial state: everything hidden
        SetAlpha(blackOverlay, 1f);
        SetAlpha(companyLogoGroup, 0f);
        SetAlpha(titleGroup, 0f);
        SetAlpha(creditTextGroup, 0f);

        if (mistyFieldScene != null) mistyFieldScene.SetActive(true);
        if (luthadel​StreetsGroup != null) luthadel​StreetsGroup.SetActive(false);
        if (kredikShawGroup != null) kredikShawGroup.SetActive(false);

        // Start music
        if (musicSource != null && mainThemeClip != null)
        {
            musicSource.clip = mainThemeClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        sequenceTime = 0f;
    }

    void Update()
    {
        if (sequenceComplete) return;

        // Skip support
        if (allowSkip && (Input.GetKeyDown(skipKey) || Input.GetKeyDown(skipKeyAlt)))
        {
            SkipToMenu();
            return;
        }

        sequenceTime += Time.deltaTime;

        // ── Phase 1: Fade from black (0 → fadeInDuration) ────────────────
        if (sequenceTime <= fadeInDuration && blackOverlay != null)
        {
            blackOverlay.alpha = Mathf.Lerp(1f, 0f, sequenceTime / fadeInDuration);
        }
        else if (blackOverlay != null && blackOverlay.alpha > 0f)
        {
            blackOverlay.alpha = 0f;
        }

        // ── Phase 2: Company logo (logoStartTime) ────────────────────────
        if (!logoTriggered && sequenceTime >= logoStartTime)
        {
            logoTriggered = true;
            StartCoroutine(ShowCompanyLogo());
        }

        // ── Phase 3: Luthadel streets + credits (streetsStartTime) ───────
        if (!streetsTriggered && sequenceTime >= streetsStartTime)
        {
            streetsTriggered = true;
            CutToStreetsPhase();
        }

        // ── Phase 4: Kredik Shaw panoramic (panoramicStartTime) ──────────
        if (!panoramicTriggered && sequenceTime >= panoramicStartTime)
        {
            panoramicTriggered = true;
            CutToPanoramicPhase();
        }

        // ── Phase 5: Title drop (titleDropTime) ─────────────────────────
        if (!titleTriggered && sequenceTime >= titleDropTime)
        {
            titleTriggered = true;
            StartCoroutine(ShowTitle());
        }

        // Drive credit lines based on time
        UpdateCredits();
    }

    // ── Phase Implementations ────────────────────────────────────────────────

    IEnumerator ShowCompanyLogo()
    {
        if (companyLogoAnimator != null)
            companyLogoAnimator.SetTrigger("Play");

        // Fade in logo
        yield return FadeCanvasGroup(companyLogoGroup, 0f, 1f, 1f);

        // Hold for logoDuration minus fade time
        yield return new WaitForSeconds(Mathf.Max(0f, logoDuration - 2f));

        // Fade out logo
        yield return FadeCanvasGroup(companyLogoGroup, 1f, 0f, 1f);
    }

    void CutToStreetsPhase()
    {
        // Swap camera/scene — misty field out, Luthadel streets in
        if (mistyFieldScene != null) mistyFieldScene.SetActive(false);
        if (luthadel​StreetsGroup != null) luthadel​StreetsGroup.SetActive(true);

        // Start credit line coroutine
        creditCoroutine = StartCoroutine(PlayCreditLines());
    }

    void CutToPanoramicPhase()
    {
        // Swap to Kredik Shaw panoramic
        if (luthadel​StreetsGroup != null) luthadel​StreetsGroup.SetActive(false);
        if (kredikShawGroup != null) kredikShawGroup.SetActive(true);
    }

    IEnumerator ShowTitle()
    {
        // Trigger the AllomanticTitleRenderer to draw the blue lines
        AllomanticTitleRenderer titleRenderer = titleGroup?.GetComponentInChildren<AllomanticTitleRenderer>();
        if (titleRenderer != null)
            titleRenderer.StartDrawing(titleAnimDuration);

        // Fade in the title group
        yield return FadeCanvasGroup(titleGroup, 0f, 1f, titleAnimDuration);

        // Hold
        yield return new WaitForSeconds(postTitleHold);

        // Sequence complete — transition
        TransitionToNextScene();
    }

    // ── Credit Line System ───────────────────────────────────────────────────

    private int currentCreditIndex = 0;

    void UpdateCredits()
    {
        if (creditLines == null || creditLines.Count == 0) return;

        // Check if any new credit line should appear based on sequence time
        while (currentCreditIndex < creditLines.Count
            && sequenceTime >= creditLines[currentCreditIndex].time)
        {
            ShowCreditLine(creditLines[currentCreditIndex]);
            currentCreditIndex++;
        }
    }

    void ShowCreditLine(CreditLine line)
    {
        if (creditCoroutine != null)
            StopCoroutine(creditCoroutine);
        creditCoroutine = StartCoroutine(AnimateCreditLine(line.text));
    }

    IEnumerator AnimateCreditLine(string text)
    {
        if (creditText == null || creditTextGroup == null) yield break;

        // Fade out current
        if (creditTextGroup.alpha > 0f)
            yield return FadeCanvasGroup(creditTextGroup, creditTextGroup.alpha, 0f, creditFadeDuration * 0.5f);

        // Set new text
        creditText.text = text;

        // Fade in
        yield return FadeCanvasGroup(creditTextGroup, 0f, 1f, creditFadeDuration);

        // Hold
        yield return new WaitForSeconds(creditHoldDuration);

        // Fade out
        yield return FadeCanvasGroup(creditTextGroup, 1f, 0f, creditFadeDuration);
    }

    IEnumerator PlayCreditLines()
    {
        // Fallback: if credits aren't time-based, play them sequentially
        // (UpdateCredits handles time-based playback, this is the backup)
        yield break;
    }

    // ── Transition ───────────────────────────────────────────────────────────

    void TransitionToNextScene()
    {
        if (sequenceComplete) return;
        sequenceComplete = true;
        StartCoroutine(FadeAndLoadScene());
    }

    void SkipToMenu()
    {
        if (isSkipping) return;
        isSkipping = true;
        sequenceComplete = true;

        if (musicSource != null)
            StartCoroutine(FadeAudio(musicSource, 0f, 1f));

        StartCoroutine(FadeAndLoadScene());
    }

    IEnumerator FadeAndLoadScene()
    {
        // Fade to black
        if (blackOverlay != null)
            yield return FadeCanvasGroup(blackOverlay, blackOverlay.alpha, 1f, 1.5f);

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // ── Utility ──────────────────────────────────────────────────────────────

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }
}
