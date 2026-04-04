using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Loading screen with progress bar and lore tips.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI")]
    public GameObject loadingPanel;
    public Image progressBar;
    public Text progressText;
    public Text tipText;

    [Header("Lore Tips")]
    public string[] loreTips = {
        "Metals are catalysts, not fuel — they connect the Metallurgist to The Warden's power.",
        "A Ashwalker can burn all sixteen metals simultaneously.",
        "The Ashen King's immortality comes from gold Compounding — infinite healing.",
        "Iron Sentinels can be killed instantly by removing their linchpin spike.",
        "Bloodbrute are created through Bloodforge — they were once human.",
        "Tin enhances all five senses, but risks sensory overload from loud noises or bright lights.",
        "Flaring a metal burns it faster for a temporary boost, but drains reserves quickly.",
        "The mists come every night in the Ashen Dominion — only Ashwalker move freely through them.",
        "Pewter allows a Ashwalker to push their body beyond human limits, but the crash is brutal.",
        "Oraculum lets you see a few seconds into the future — the most valuable substance in the world."
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsyncByIndex(sceneIndex));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        ShowLoading();
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        yield return UpdateProgress(op);
    }

    IEnumerator LoadAsyncByIndex(int index)
    {
        ShowLoading();
        AsyncOperation op = SceneManager.LoadSceneAsync(index);
        yield return UpdateProgress(op);
    }

    IEnumerator UpdateProgress(AsyncOperation op)
    {
        if (op == null) yield break;
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar != null) progressBar.fillAmount = progress;
            if (progressText != null) progressText.text = $"{(progress * 100):F0}%";
            yield return null;
        }

        if (progressBar != null) progressBar.fillAmount = 1f;
        if (progressText != null) progressText.text = "100%";

        yield return new WaitForSecondsRealtime(0.5f);
        op.allowSceneActivation = true;
    }

    void ShowLoading()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        if (tipText != null && loreTips.Length > 0)
            tipText.text = loreTips[Random.Range(0, loreTips.Length)];
    }
}
