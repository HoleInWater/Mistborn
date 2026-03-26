using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Cinematic boss introduction sequence. Shows boss name, title, and health bar.
/// Triggered when player enters a boss arena.
/// </summary>
public class BossIntroSequence : MonoBehaviour
{
    [Header("UI")]
    public GameObject introPanel;
    public Text bossNameText;
    public Text bossTitleText;
    public Image bossHealthBar;
    public CanvasGroup introCanvasGroup;

    [Header("Timing")]
    public float introDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.8f;
    public float cameraHoldDuration = 2f;

    [Header("Camera")]
    public float introCameraFOV = 40f;
    public float cameraLookSpeed = 2f;

    private Camera mainCamera;
    private float originalFOV;
    private bool isPlaying = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (introPanel != null) introPanel.SetActive(false);
    }

    /// <summary>
    /// Play boss introduction. Call when player enters boss arena.
    /// </summary>
    public void PlayIntro(string bossName, string title, Transform bossTransform)
    {
        if (isPlaying) return;
        StartCoroutine(IntroSequence(bossName, title, bossTransform));
    }

    IEnumerator IntroSequence(string bossName, string title, Transform boss)
    {
        isPlaying = true;

        // Setup UI
        if (introPanel != null) introPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = bossName;
        if (bossTitleText != null) bossTitleText.text = title;
        if (introCanvasGroup != null) introCanvasGroup.alpha = 0f;

<<<<<<< HEAD
        // Slow time for cinematic feel
=======
        // Slow time for cinematic feel — captured before any yield so it's always restored
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.3f;

        // Camera zoom
        if (mainCamera != null)
<<<<<<< HEAD
        {
            originalFOV = mainCamera.fieldOfView;
        }

        // Disable player input
        BasicPlayerMove player = FindObjectOfType<BasicPlayerMove>();
        if (player != null) player.enabled = false;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (introCanvasGroup != null)
                introCanvasGroup.alpha = elapsed / fadeInDuration;
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(originalFOV, introCameraFOV,
                    elapsed / fadeInDuration);
            yield return null;
        }

        // Hold — camera looks at boss
        elapsed = 0f;
        while (elapsed < cameraHoldDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (mainCamera != null && boss != null)
            {
                Vector3 dir = (boss.position - mainCamera.transform.position).normalized;
                Quaternion look = Quaternion.LookRotation(dir);
                mainCamera.transform.rotation = Quaternion.Slerp(
                    mainCamera.transform.rotation, look, Time.unscaledDeltaTime * cameraLookSpeed);
            }
            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (introCanvasGroup != null)
                introCanvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(introCameraFOV, originalFOV,
                    elapsed / fadeOutDuration);
            yield return null;
        }

        // Cleanup
        if (introPanel != null) introPanel.SetActive(false);
        Time.timeScale = originalTimeScale;

        // Re-enable player
        if (player != null) player.enabled = true;

        // Show boss health bar on CombatUI
        CombatUI.Instance?.ShowBossHealth(bossName, 1f);

        isPlaying = false;
        SoundManager.Instance?.TransitionToBoss();
=======
            originalFOV = mainCamera.fieldOfView;

        // Cache player reference once — FindObjectOfType is expensive and can return null after a scene change
        BasicPlayerMove player = FindObjectOfType<BasicPlayerMove>();
        if (player != null) player.enabled = false;

        bool completed = false;
        try
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (introCanvasGroup != null)
                    introCanvasGroup.alpha = elapsed / fadeInDuration;
                if (mainCamera != null)
                    mainCamera.fieldOfView = Mathf.Lerp(originalFOV, introCameraFOV,
                        elapsed / fadeInDuration);
                yield return null;
            }

            // Hold — camera looks at boss
            elapsed = 0f;
            while (elapsed < cameraHoldDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (mainCamera != null && boss != null)
                {
                    Vector3 dir = (boss.position - mainCamera.transform.position).normalized;
                    Quaternion look = Quaternion.LookRotation(dir);
                    mainCamera.transform.rotation = Quaternion.Slerp(
                        mainCamera.transform.rotation, look, Time.unscaledDeltaTime * cameraLookSpeed);
                }
                yield return null;
            }

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (introCanvasGroup != null)
                    introCanvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
                if (mainCamera != null)
                    mainCamera.fieldOfView = Mathf.Lerp(introCameraFOV, originalFOV,
                        elapsed / fadeOutDuration);
                yield return null;
            }

            completed = true;
        }
        finally
        {
            // Always restore timeScale and player control, even if coroutine is stopped mid-way
            Time.timeScale = originalTimeScale;
            if (introPanel != null) introPanel.SetActive(false);
            if (player != null) player.enabled = true;
            isPlaying = false;
        }

        if (completed)
        {
            CombatUI.Instance?.ShowBossHealth(bossName, 1f);
            SoundManager.Instance?.TransitionToBoss();
        }
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    /// <summary>
    /// Quick accessor for CombatUI singleton (may not exist).
    /// </summary>
    private static class CombatUI
    {
        public static CombatUIProxy Instance
        {
            get
            {
                var ui = Object.FindObjectOfType<global::CombatUI>();
                return ui != null ? new CombatUIProxy(ui) : null;
            }
        }
    }

    private class CombatUIProxy
    {
        private global::CombatUI ui;
        public CombatUIProxy(global::CombatUI ui) { this.ui = ui; }
        public void ShowBossHealth(string name, float hp) { ui.ShowBossHealth(name, hp); }
    }

    public bool IsPlaying() => isPlaying;
}
