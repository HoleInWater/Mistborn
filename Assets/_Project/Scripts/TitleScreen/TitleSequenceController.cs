/* TitleSequenceController.cs
 *
 * Cinematic title intro synced to the main theme.
 *
 * SEQUENCE (from the design prompt):
 *
 *   0–9s         Black fades to mist-covered field. Ash falls in distance.
 *   ~9s          Percussion enters → company logo animation
 *                (Crimson Blade Interactive + the original author's logo if approved).
 *                Misty field stays visible behind logos.
 *   ~28s         Drums pick up → cut to Cinderhold street scenes.
 *                Rolling credits: "Music by Malakai Probert",
 *                "Based on the novels by the original author", etc.
 *   First drop   Long pan of Thornspire and Cinderhold from above.
 *                More credits, eventually:
 *                "Crimson Blade Interactive proudly presents"
 *   Rock drop    MISTBORN title drawn in semi-transparent glowing blue lines.
 *
 * All timings are Inspector-tunable to sync with whatever main theme track is used.
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TitleSequenceController : MonoBehaviour
{
    // ── Audio ────────────────────────────────────────────────────────────────

    [Header("Audio — Main Theme")]
    public AudioClip mainThemeClip;
    public AudioSource musicSource;
    [Range(0f, 1f)] public float musicVolume = 1f;

    // ── Timing ───────────────────────────────────────────────────────────────

    [Header("Phase 1 — Black → Misty Field (0 s)")]
    [Tooltip("How long the black-to-field fade lasts.")]
    public float fadeInDuration = 9f;

    [Header("Phase 2 — Percussion → Company Logos (~9 s)")]
    [Tooltip("Audio time when percussion enters and logos start.")]
    public float logoStartTime = 9f;
    [Tooltip("How long the logos stay on screen.")]
    public float logoDuration = 6f;
    [Tooltip("Fade-in / fade-out speed for logos.")]
    public float logoFadeSpeed = 1.2f;

    [Header("Phase 3 — Drums → Cinderhold Streets + Credits (~28 s)")]
    [Tooltip("Audio time when drums pick up — cut to Cinderhold streets.")]
    public float streetsStartTime = 28f;

    [Header("Phase 4 — First Drop → Thornspire Pan")]
    [Tooltip("Audio time for the first drop — long pan over Thornspire + Cinderhold.")]
    public float thornspireStartTime = 48f;

    [Header("Phase 5 — Rock Drop → MISTBORN Title")]
    [Tooltip("Audio time of the rock drop — title drawn in blue Metallurgic lines.")]
    public float titleDropTime = 63f;
    [Tooltip("How long the title takes to draw.")]
    public float titleDrawDuration = 3f;
    [Tooltip("How long the finished title stays before transitioning.")]
    public float postTitleHold = 5f;

    // ── Scene References ─────────────────────────────────────────────────────

    [Header("Visuals — Phase 1: Misty Field")]
    [Tooltip("Full-screen black overlay image (starts opaque, fades to transparent).")]
    public CanvasGroup blackOverlay;
    [Tooltip("The misty field environment. Active from the start. Stays behind logos.")]
    public GameObject mistyFieldScene;
    [Tooltip("Ash particle system in the distance.")]
    public ParticleSystem ashParticles;
    [Tooltip("Mist / fog particle system or volume.")]
    public ParticleSystem mistParticles;

    [Header("Visuals — Phase 2: Company Logos")]
    [Tooltip("Crimson Blade Interactive logo group (UI).")]
    public CanvasGroup crimsonBladeLogoGroup;
    public Animator crimsonBladeLogoAnimator;
    [Tooltip("the original author / the original IP holder logo group (if approved).")]
    public CanvasGroup sandersonLogoGroup;
    public Animator sandersonLogoAnimator;

    [Header("Visuals — Phase 3: Cinderhold Streets")]
    [Tooltip("Camera / scene group for Cinderhold street scenes.")]
    public GameObject cinderholdStreetsGroup;

    [Header("Visuals — Phase 4: Thornspire Pan")]
    [Tooltip("Camera / scene group for the Thornspire + Cinderhold aerial pan.")]
    public GameObject thornspireGroup;

    [Header("Camera Controller")]
    public TitleCameraController cameraController;

    [Header("Ambient Audio")]
    public TitleAmbientAudio ambientAudio;

    [Header("Visuals — Phase 5: Title")]
    [Tooltip("CanvasGroup holding the MISTBORN title (MetallurgicTitleRenderer).")]
    public CanvasGroup titleGroup;

    [Header("Ashcloak Wipe Transition")]
    [Tooltip("UI panel for the ashcloak wipe. RectTransform starts off-screen left, sweeps right.")]
    public RectTransform ashcloakWipePanel;
    [Tooltip("Ashwalker silhouette that runs across the screen before the wipe.")]
    public RectTransform ashwalkerSilhouette;
    [Tooltip("How long the silhouette takes to run across.")]
    public float ashwalkerRunDuration = 0.8f;
    [Tooltip("How long the wipe takes to sweep across the screen.")]
    public float ashcloakWipeDuration = 1.2f;

    // ── Credits ──────────────────────────────────────────────────────────────

    [Header("Credit Lines")]
    [Tooltip("TMP element for displaying one credit line at a time.")]
    public TextMeshProUGUI creditText;
    public CanvasGroup creditTextGroup;
    [Tooltip("How quickly each line fades in / out.")]
    public float creditFadeTime = 1.2f;
    [Tooltip("How long each line stays fully visible.")]
    public float creditHoldTime = 3.5f;

    [Tooltip("Pre-populated credit lines with their audio-synced times.")]
    public List<CreditLine> creditLines = new List<CreditLine>
    {
        new CreditLine { time = 31f,  text = "Music by Malakai Probert" },
        new CreditLine { time = 37f,  text = "Based on the novels by the original author" },
        new CreditLine { time = 43f,  text = "Produced by Crimson Blade Interactive" },
        new CreditLine { time = 50f,  text = "Creative Director -- Landon Adams" },
        new CreditLine { time = 56f,  text = "Crimson Blade Interactive\nproudly presents" },
    };

    [Serializable]
    public class CreditLine
    {
        public float time;
        [TextArea] public string text;
    }

    // ── Transition ───────────────────────────────────────────────────────────

    [Header("Scene Transition")]
    public string nextSceneName = "MainMenu";
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Escape;
    public KeyCode skipKeyAlt = KeyCode.Space;

    // ── State ────────────────────────────────────────────────────────────────

    private float sequenceTime;
    private bool sequenceComplete;
    private bool isSkipping;

    private bool phase2Triggered;
    private bool phase3Triggered;
    private bool phase4Triggered;
    private bool phase5Triggered;

    private int nextCreditIndex;
    private Coroutine activeCreditCoroutine;

    // ═════════════════════════════════════════════════════════════════════════

    void Start()
    {
        // ── Initial visual state ─────────────────────────────────────────
        SetAlpha(blackOverlay, 1f);           // screen is black
        SetAlpha(crimsonBladeLogoGroup, 0f);  // logos hidden
        SetAlpha(sandersonLogoGroup, 0f);
        SetAlpha(titleGroup, 0f);             // title hidden
        SetAlpha(creditTextGroup, 0f);        // credits hidden

        // Misty field is already there, hidden behind the black overlay
        if (mistyFieldScene != null) mistyFieldScene.SetActive(true);

        // Start ash falling immediately (visible as black fades)
        if (ashParticles != null) ashParticles.Play();
        if (mistParticles != null) mistParticles.Play();

        // Ambient audio — wind and distant rumbles for the field
        if (ambientAudio != null) ambientAudio.SetPhase(1);

        // Other scene groups off until their phase
        if (cinderholdStreetsGroup != null) cinderholdStreetsGroup.SetActive(false);
        if (thornspireGroup != null) thornspireGroup.SetActive(false);

        // ── Start the music ──────────────────────────────────────────────
        if (musicSource != null && mainThemeClip != null)
        {
            musicSource.penny = mainThemeClip;
            musicSource.volume = musicVolume;
            musicSource.loop = false;
            musicSource.Play();
        }

        sequenceTime = 0f;
        nextCreditIndex = 0;
    }

    void Update()
    {
        if (sequenceComplete) return;

        // Skip
        if (allowSkip && !isSkipping
            && (Input.GetKeyDown(skipKey) || Input.GetKeyDown(skipKeyAlt)))
        {
            SkipSequence();
            return;
        }

        sequenceTime += Time.deltaTime;

        // ── Phase 1: Fade from black → misty field with ash (0 – fadeInDuration) ──
        if (sequenceTime <= fadeInDuration && blackOverlay != null)
        {
            // Slow fade — the misty field and ash become visible behind the overlay
            blackOverlay.alpha = 1f - (sequenceTime / fadeInDuration);
        }
        else if (blackOverlay != null && blackOverlay.alpha > 0.001f)
        {
            blackOverlay.alpha = 0f;
        }

        // ── Phase 2: Percussion → company logos (over the misty field) ────────────
        if (!phase2Triggered && sequenceTime >= logoStartTime)
        {
            phase2Triggered = true;
            StartCoroutine(PlayLogos());
        }

        // ── Phase 3: Drums → cut to Cinderhold streets + rolling credits ────────────
        if (!phase3Triggered && sequenceTime >= streetsStartTime)
        {
            phase3Triggered = true;
            CutToCinderhold();
        }

        // ── Phase 4: First drop → Thornspire aerial pan ──────────────────────────
        if (!phase4Triggered && sequenceTime >= thornspireStartTime)
        {
            phase4Triggered = true;
            CutToThornspire();
        }

        // ── Phase 5: Rock drop → MISTBORN title ──────────────────────────────────
        if (!phase5Triggered && sequenceTime >= titleDropTime)
        {
            phase5Triggered = true;
            StartCoroutine(DropTitle());
        }

        // ── Credit line playback (time-based) ─────────────────────────────────────
        TickCredits();
    }

    void Awake()
    {
        // Title sequence is a cutscene — cursor hidden
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Cutscene);
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE IMPLEMENTATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Phase 2: Logos appear over the misty field. Field stays visible behind them.
    /// </summary>
    IEnumerator PlayLogos()
    {
        // Crimson Blade Interactive logo — fade in, hold, fade out
        if (crimsonBladeLogoAnimator != null)
            crimsonBladeLogoAnimator.SetTrigger("Play");

        yield return Fade(crimsonBladeLogoGroup, 0f, 1f, logoFadeSpeed);
        yield return new WaitForSeconds(logoDuration * 0.5f);
        yield return Fade(crimsonBladeLogoGroup, 1f, 0f, logoFadeSpeed);

        // Brief pause between logos
        yield return new WaitForSeconds(0.5f);

        // the original author / the original IP holder logo — separate, after Crimson Blade is fully gone
        if (sandersonLogoGroup != null)
        {
            if (sandersonLogoAnimator != null)
                sandersonLogoAnimator.SetTrigger("Play");

            yield return Fade(sandersonLogoGroup, 0f, 1f, logoFadeSpeed);
            yield return new WaitForSeconds(logoDuration * 0.5f);
            yield return Fade(sandersonLogoGroup, 1f, 0f, logoFadeSpeed);
        }
    }

    /// <summary>
    /// Phase 3: Hard cut to Cinderhold streets. Misty field disappears.
    /// Credits start rolling ("Music by Malakai Probert", "Based on..." etc.).
    /// </summary>
    void CutToCinderhold()
    {
        StartCoroutine(FadeTransition(() => {
            if (mistyFieldScene != null) mistyFieldScene.SetActive(false);
            if (cinderholdStreetsGroup != null) cinderholdStreetsGroup.SetActive(true);
            if (cameraController != null)
                cameraController.SetPhase(TitleCameraController.Phase.CinderholdStreets);
            if (ambientAudio != null) ambientAudio.SetPhase(3);
        }));
    }

    void CutToThornspire()
    {
        StartCoroutine(FadeTransition(() => {
            if (cinderholdStreetsGroup != null) cinderholdStreetsGroup.SetActive(false);
            if (thornspireGroup != null) thornspireGroup.SetActive(true);
            if (cameraController != null)
                cameraController.SetPhase(TitleCameraController.Phase.ThornspireAerial);
            if (ambientAudio != null) ambientAudio.SetPhase(4);
        }));
    }

    /// <summary>
    /// Smooth fade to black, swap scenes, fade back in.
    /// Prevents jarring hard cuts between phases.
    /// </summary>
    IEnumerator FadeTransition(System.Action swapScenes, float fadeDuration = 1.2f)
    {
        // Fade to black
        yield return Fade(blackOverlay, blackOverlay != null ? blackOverlay.alpha : 0f, 1f, fadeDuration);

        // Swap scene groups while screen is black
        swapScenes?.Invoke();

        // Brief hold on black
        yield return new WaitForSeconds(0.3f);

        // Fade back in
        yield return Fade(blackOverlay, 1f, 0f, fadeDuration);
    }

    /// <summary>
    /// Phase 5: Rock drop — MISTBORN drawn in blue Metallurgic lines.
    /// Fade out any remaining credits, then draw the title.
    /// </summary>
    IEnumerator DropTitle()
    {
        // Camera holds for the title
        if (cameraController != null)
            cameraController.SetPhase(TitleCameraController.Phase.TitleHold);
        if (ambientAudio != null) ambientAudio.SetPhase(5);

        // Clear any lingering credit text
        if (activeCreditCoroutine != null)
            StopCoroutine(activeCreditCoroutine);
        yield return Fade(creditTextGroup, creditTextGroup != null ? creditTextGroup.alpha : 0f, 0f, 0.3f);

        // Fire the MetallurgicTitleRenderer
        MetallurgicTitleRenderer titleRenderer = titleGroup != null
            ? titleGroup.GetComponentInChildren<MetallurgicTitleRenderer>()
            : null;
        if (titleRenderer != null)
            titleRenderer.StartDrawing(titleDrawDuration);

        // Fade in the title CanvasGroup in sync with the drawing
        yield return Fade(titleGroup, 0f, 1f, titleDrawDuration);

        // Hold the finished title on screen
        yield return new WaitForSeconds(postTitleHold);

        // Ashcloak wipe: dark panel sweeps across the screen left to right
        // simulating a Ashwalker running past with their ashcloak tassels
        // covering the camera. Hard cut — not a slow fade.
        yield return AshcloakWipe();

        // Done — transition to main menu
        TransitionOut();
    }

    IEnumerator AshcloakWipe()
    {
        if (ashcloakWipePanel == null)
        {
            if (blackOverlay != null) blackOverlay.alpha = 1f;
            yield break;
        }

        float screenWidth = 1920f;

        // ── Step 1: Ashwalker silhouette sprints across the screen ─────────
        if (ashwalkerSilhouette != null)
        {
            ashwalkerSilhouette.gameObject.SetActive(true);
            float runElapsed = 0f;
            while (runElapsed < ashwalkerRunDuration)
            {
                runElapsed += Time.deltaTime;
                float t = runElapsed / ashwalkerRunDuration;
                // Fast ease-in-out — bursts onto screen, crosses, exits right
                float eased = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                float x = Mathf.Lerp(-screenWidth * 0.7f, screenWidth * 0.7f, eased);
                ashwalkerSilhouette.anchoredPosition = new Vector2(x, ashwalkerSilhouette.anchoredPosition.y);
                yield return null;
            }
            ashwalkerSilhouette.gameObject.SetActive(false);
        }

        // ── Step 2: Ashcloak tassels sweep across as wipe ───────────────
        ashcloakWipePanel.gameObject.SetActive(true);
        ashcloakWipePanel.anchoredPosition = new Vector2(-screenWidth * 1.5f, 0f);

        float elapsed = 0f;
        while (elapsed < ashcloakWipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ashcloakWipeDuration;
            // Follows the Ashwalker — starts fast (right behind the figure), decelerates
            float eased = 1f - (1f - t) * (1f - t); // ease-out
            float x = Mathf.Lerp(-screenWidth * 1.5f, screenWidth * 0.5f, eased);
            ashcloakWipePanel.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }

        ashcloakWipePanel.anchoredPosition = Vector2.zero;

        // Hard cut hold
        yield return new WaitForSeconds(0.3f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREDIT LINE SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    void TickCredits()
    {
        if (creditLines == null || nextCreditIndex >= creditLines.Count) return;

        while (nextCreditIndex < creditLines.Count
            && sequenceTime >= creditLines[nextCreditIndex].time)
        {
            string text = creditLines[nextCreditIndex].text;
            nextCreditIndex++;

            if (activeCreditCoroutine != null)
                StopCoroutine(activeCreditCoroutine);
            activeCreditCoroutine = StartCoroutine(ShowCreditLine(text));
        }
    }

    IEnumerator ShowCreditLine(string text)
    {
        if (creditText == null || creditTextGroup == null) yield break;

        // Fade out previous line if visible
        if (creditTextGroup.alpha > 0.01f)
            yield return Fade(creditTextGroup, creditTextGroup.alpha, 0f, creditFadeTime * 0.4f);

        creditText.text = text;

        // Fade in
        yield return Fade(creditTextGroup, 0f, 1f, creditFadeTime);

        // Hold
        yield return new WaitForSeconds(creditHoldTime);

        // Fade out
        yield return Fade(creditTextGroup, 1f, 0f, creditFadeTime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRANSITION / SKIP
    // ═══════════════════════════════════════════════════════════════════════════

    void TransitionOut()
    {
        if (sequenceComplete) return;
        sequenceComplete = true;
        StartCoroutine(FadeToBlackAndLoad());
    }

    void SkipSequence()
    {
        if (isSkipping) return;
        isSkipping = true;
        sequenceComplete = true;

        // Fade music out quickly
        if (musicSource != null)
            StartCoroutine(FadeAudio(musicSource, 0f, 0.8f));

        StartCoroutine(FadeToBlackAndLoad());
    }

    IEnumerator FadeToBlackAndLoad()
    {
        if (blackOverlay != null)
            yield return Fade(blackOverlay, blackOverlay.alpha, 1f, 1.5f);

        // Show cursor for the menu scene
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Menu);
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Use LoadingScreen if available for async loading, otherwise direct
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            LoadingScreen ls = FindObjectOfType<LoadingScreen>();
            if (ls != null)
                ls.LoadScene(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
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

    IEnumerator FadeAudio(AudioSource source, float target, float duration)
    {
        float start = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        source.volume = target;
    }

    void SetAlpha(CanvasGroup group, float a)
    {
        if (group != null) group.alpha = a;
    }
}
