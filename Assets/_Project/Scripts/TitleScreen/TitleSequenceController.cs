/* TitleSequenceController.cs
 *
 * Cinematic title intro synced to the main theme.
 *
 * SEQUENCE (from the design prompt):
 *
 *   0–9s         Black fades to mist-covered field. Ash falls in distance.
 *   ~9s          Percussion enters → company logo animation
 *                (Crimson Blade Interactive + Sanderson's logo if approved).
 *                Misty field stays visible behind logos.
 *   ~28s         Drums pick up → cut to Luthadel street scenes.
 *                Rolling credits: "Music by Malakei",
 *                "Based on the novels by Brandon Sanderson", etc.
 *   First drop   Long pan of Kredik Shaw and Luthadel from above.
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

    [Header("Phase 3 — Drums → Luthadel Streets + Credits (~28 s)")]
    [Tooltip("Audio time when drums pick up — cut to Luthadel streets.")]
    public float streetsStartTime = 28f;

    [Header("Phase 4 — First Drop → Kredik Shaw Pan")]
    [Tooltip("Audio time for the first drop — long pan over Kredik Shaw + Luthadel.")]
    public float kredikShawStartTime = 45f;

    [Header("Phase 5 — Rock Drop → MISTBORN Title")]
    [Tooltip("Audio time of the rock drop — title drawn in blue Allomantic lines.")]
    public float titleDropTime = 60f;
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
    [Tooltip("Brandon Sanderson / Dragonsteel logo group (if approved).")]
    public CanvasGroup sandersonLogoGroup;
    public Animator sandersonLogoAnimator;

    [Header("Visuals — Phase 3: Luthadel Streets")]
    [Tooltip("Camera / scene group for Luthadel street scenes.")]
    public GameObject luthadelStreetsGroup;

    [Header("Visuals — Phase 4: Kredik Shaw Pan")]
    [Tooltip("Camera / scene group for the Kredik Shaw + Luthadel aerial pan.")]
    public GameObject kredikShawGroup;

    [Header("Camera Controller")]
    public TitleCameraController cameraController;

    [Header("Visuals — Phase 5: Title")]
    [Tooltip("CanvasGroup holding the MISTBORN title (AllomanticTitleRenderer).")]
    public CanvasGroup titleGroup;

    [Header("Mistcloak Wipe Transition")]
    [Tooltip("UI panel for the mistcloak wipe. RectTransform starts off-screen left, sweeps right.")]
    public RectTransform mistcloakWipePanel;
    [Tooltip("How long the wipe takes to sweep across the screen.")]
    public float mistcloakWipeDuration = 1.2f;

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
        new CreditLine { time = 28f,  text = "Music by Malakei" },
        new CreditLine { time = 35f,  text = "Based on the novels by Brandon Sanderson" },
        new CreditLine { time = 42f,  text = "Produced by Crimson Blade Interactive" },
        new CreditLine { time = 49f,  text = "Creative Director — Landon Adams" },
        new CreditLine { time = 55f,  text = "Crimson Blade Interactive\nproudly presents" },
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

        // Other scene groups off until their phase
        if (luthadelStreetsGroup != null) luthadelStreetsGroup.SetActive(false);
        if (kredikShawGroup != null) kredikShawGroup.SetActive(false);

        // ── Start the music ──────────────────────────────────────────────
        if (musicSource != null && mainThemeClip != null)
        {
            musicSource.clip = mainThemeClip;
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

        // ── Phase 3: Drums → cut to Luthadel streets + rolling credits ────────────
        if (!phase3Triggered && sequenceTime >= streetsStartTime)
        {
            phase3Triggered = true;
            CutToLuthadel();
        }

        // ── Phase 4: First drop → Kredik Shaw aerial pan ──────────────────────────
        if (!phase4Triggered && sequenceTime >= kredikShawStartTime)
        {
            phase4Triggered = true;
            CutToKredikShaw();
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
        // Crimson Blade Interactive logo
        if (crimsonBladeLogoAnimator != null)
            crimsonBladeLogoAnimator.SetTrigger("Play");

        yield return Fade(crimsonBladeLogoGroup, 0f, 1f, logoFadeSpeed);
        yield return new WaitForSeconds(logoDuration * 0.4f);

        // Sanderson / Dragonsteel logo (if approved — just fade in alongside or after)
        if (sandersonLogoGroup != null)
        {
            if (sandersonLogoAnimator != null)
                sandersonLogoAnimator.SetTrigger("Play");

            yield return Fade(sandersonLogoGroup, 0f, 1f, logoFadeSpeed);
            yield return new WaitForSeconds(logoDuration * 0.4f);
            yield return Fade(sandersonLogoGroup, 1f, 0f, logoFadeSpeed);
        }

        // Fade out Crimson Blade logo before streets phase
        yield return Fade(crimsonBladeLogoGroup, 1f, 0f, logoFadeSpeed);
    }

    /// <summary>
    /// Phase 3: Hard cut to Luthadel streets. Misty field disappears.
    /// Credits start rolling ("Music by Malakei", "Based on..." etc.).
    /// </summary>
    void CutToLuthadel()
    {
        if (mistyFieldScene != null) mistyFieldScene.SetActive(false);
        if (luthadelStreetsGroup != null) luthadelStreetsGroup.SetActive(true);
        if (cameraController != null)
            cameraController.SetPhase(TitleCameraController.Phase.LuthadelStreets);
    }

    /// <summary>
    /// Phase 4: First drop — cut to long aerial pan of Kredik Shaw + Luthadel.
    /// More credits continue, building toward "proudly presents".
    /// </summary>
    void CutToKredikShaw()
    {
        if (luthadelStreetsGroup != null) luthadelStreetsGroup.SetActive(false);
        if (kredikShawGroup != null) kredikShawGroup.SetActive(true);
        if (cameraController != null)
            cameraController.SetPhase(TitleCameraController.Phase.KredikShawAerial);
    }

    /// <summary>
    /// Phase 5: Rock drop — MISTBORN drawn in blue Allomantic lines.
    /// Fade out any remaining credits, then draw the title.
    /// </summary>
    IEnumerator DropTitle()
    {
        // Camera holds for the title
        if (cameraController != null)
            cameraController.SetPhase(TitleCameraController.Phase.TitleHold);

        // Clear any lingering credit text
        if (activeCreditCoroutine != null)
            StopCoroutine(activeCreditCoroutine);
        yield return Fade(creditTextGroup, creditTextGroup != null ? creditTextGroup.alpha : 0f, 0f, 0.3f);

        // Fire the AllomanticTitleRenderer
        AllomanticTitleRenderer titleRenderer = titleGroup != null
            ? titleGroup.GetComponentInChildren<AllomanticTitleRenderer>()
            : null;
        if (titleRenderer != null)
            titleRenderer.StartDrawing(titleDrawDuration);

        // Fade in the title CanvasGroup in sync with the drawing
        yield return Fade(titleGroup, 0f, 1f, titleDrawDuration);

        // Hold the finished title on screen
        yield return new WaitForSeconds(postTitleHold);

        // Mistcloak wipe: dark panel sweeps across the screen left to right
        // simulating a Mistborn running past with their mistcloak tassels
        // covering the camera. Hard cut — not a slow fade.
        yield return MistcloakWipe();

        // Done — transition to main menu
        TransitionOut();
    }

    IEnumerator MistcloakWipe()
    {
        if (mistcloakWipePanel == null)
        {
            // Fallback: instant black if no wipe panel assigned
            if (blackOverlay != null) blackOverlay.alpha = 1f;
            yield break;
        }

        // Start off-screen to the left (full screen width to the left)
        mistcloakWipePanel.gameObject.SetActive(true);
        float screenWidth = 1920f; // reference resolution
        mistcloakWipePanel.anchoredPosition = new Vector2(-screenWidth * 1.5f, 0f);

        float elapsed = 0f;
        while (elapsed < mistcloakWipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / mistcloakWipeDuration;
            // Ease-in: starts slow, accelerates (like a running figure)
            float eased = t * t;
            float x = Mathf.Lerp(-screenWidth * 1.5f, screenWidth * 0.5f, eased);
            mistcloakWipePanel.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }

        // Snap to cover entire screen
        mistcloakWipePanel.anchoredPosition = Vector2.zero;

        // Brief hold — hard cut feel
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
