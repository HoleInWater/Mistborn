using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages in-game cutscenes — camera movement, dialogue, character animation,
/// player control disable/enable, and letterbox bars.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [System.Serializable]
    public class CutsceneBeat
    {
        public string description;
        public Transform cameraTarget;
        public float cameraFOV = 40f;
        public float duration = 3f;
        public string dialogueSpeaker;
        [TextArea] public string dialogueText;
        public string animTrigger;
        public string eventToFire;
    }

    [Header("UI")]
    public GameObject letterboxTop;
    public GameObject letterboxBottom;
    public UnityEngine.UI.Text subtitleText;
    public CanvasGroup fadeOverlay;

    [Header("Settings")]
    public float letterboxAnimSpeed = 3f;
    public float cameraTransitionSpeed = 2f;

    private bool isPlaying = false;
    private Camera mainCamera;
    private BasicPlayerMove playerMove;
    private float originalFOV;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null) originalFOV = mainCamera.fieldOfView;

        HideLetterbox();
        if (subtitleText != null) subtitleText.text = "";
    }

    /// <summary>
    /// Play a sequence of cutscene beats.
    /// </summary>
    public void PlayCutscene(List<CutsceneBeat> beats)
    {
        if (isPlaying || beats == null || beats.Count == 0) return;
        StartCoroutine(CutsceneSequence(beats));
    }

    IEnumerator CutsceneSequence(List<CutsceneBeat> beats)
    {
        isPlaying = true;

        // Disable player
        playerMove = FindObjectOfType<BasicPlayerMove>();
        if (playerMove != null) playerMove.enabled = false;

        // Letterbox in
        yield return StartCoroutine(AnimateLetterbox(true));

        GameFlowManager.Instance?.SetState(GameFlowManager.GameState.Cutscene);

        foreach (var beat in beats)
        {
            // Camera
            if (beat.cameraTarget != null && mainCamera != null)
            {
                float elapsed = 0f;
                Vector3 startPos = mainCamera.transform.position;
                Quaternion startRot = mainCamera.transform.rotation;
                float startFOV = mainCamera.fieldOfView;

                while (elapsed < beat.duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0, 1, elapsed / beat.duration);

                    mainCamera.transform.position = Vector3.Lerp(startPos, beat.cameraTarget.position, t);
                    mainCamera.transform.rotation = Quaternion.Slerp(startRot, beat.cameraTarget.rotation, t);
                    mainCamera.fieldOfView = Mathf.Lerp(startFOV, beat.cameraFOV, t);

                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(beat.duration);
            }

            // Dialogue subtitle
            if (!string.IsNullOrEmpty(beat.dialogueText) && subtitleText != null)
            {
                string prefix = !string.IsNullOrEmpty(beat.dialogueSpeaker) ? $"{beat.dialogueSpeaker}: " : "";
                subtitleText.text = prefix + beat.dialogueText;
                yield return new WaitForSecondsRealtime(Mathf.Max(beat.duration * 0.5f, 2f));
                subtitleText.text = "";
            }

            // Fire event
            if (!string.IsNullOrEmpty(beat.eventToFire))
                EventManager.TriggerEvent(beat.eventToFire);
        }

        // Letterbox out
        yield return StartCoroutine(AnimateLetterbox(false));

        // Restore
        if (mainCamera != null) mainCamera.fieldOfView = originalFOV;
        if (playerMove != null) playerMove.enabled = true;
        GameFlowManager.Instance?.SetState(GameFlowManager.GameState.Playing);

        isPlaying = false;
    }

    /// <summary>
    /// Quick fade to black and back (for scene transitions).
    /// </summary>
    public void FadeToBlack(float duration, System.Action onBlack = null)
    {
        StartCoroutine(FadeSequence(duration, onBlack));
    }

    IEnumerator FadeSequence(float duration, System.Action onBlack)
    {
        if (fadeOverlay == null) yield break;

        // Fade in
        float half = duration * 0.5f;
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.alpha = elapsed / half;
            yield return null;
        }

        onBlack?.Invoke();

        // Fade out
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.alpha = 1f - (elapsed / half);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
    }

    IEnumerator AnimateLetterbox(bool show)
    {
        float target = show ? 1f : 0f;
        float current = show ? 0f : 1f;

        while (Mathf.Abs(current - target) > 0.01f)
        {
            current = Mathf.Lerp(current, target, Time.unscaledDeltaTime * letterboxAnimSpeed);

            if (letterboxTop != null)
            {
                RectTransform rt = letterboxTop.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(0, Mathf.Lerp(80, 0, current));
            }
            if (letterboxBottom != null)
            {
                RectTransform rt = letterboxBottom.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(0, Mathf.Lerp(-80, 0, current));
            }

            yield return null;
        }
    }

    void HideLetterbox()
    {
        if (letterboxTop != null) letterboxTop.SetActive(false);
        if (letterboxBottom != null) letterboxBottom.SetActive(false);
    }

    public bool IsPlaying() => isPlaying;
}
