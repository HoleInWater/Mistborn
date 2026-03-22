using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Image progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;
    public Image fadeImage;
    
    [Header("Loading Tips")]
    public string[] loadingTips;
    
    [Header("Settings")]
    public float fadeSpeed = 1f;
    public bool showTips = true;
    
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    private bool isLoading = false;
    
    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.black;
        }
        
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
        }
        
        ShowRandomTip();
    }
    
    void Update()
    {
        if (currentProgress < targetProgress)
        {
            currentProgress += Time.deltaTime * fadeSpeed;
            currentProgress = Mathf.Min(currentProgress, targetProgress);
            UpdateProgressUI();
        }
        
        if (isLoading && fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Max(0, color.a);
            fadeImage.color = color;
        }
    }
    
    public void StartLoading(float targetProgressValue)
    {
        isLoading = true;
        targetProgress = targetProgressValue;
    }
    
    public void SetProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }
    
    void UpdateProgressUI()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = currentProgress;
        }
        
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
        }
    }
    
    void ShowRandomTip()
    {
        if (!showTips || loadingTips == null || loadingTips.Length == 0)
            return;
        
        string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
        
        if (tipText != null)
        {
            tipText.text = $"Tip: {randomTip}";
        }
    }
    
    public void CompleteLoading()
    {
        targetProgress = 1f;
        currentProgress = 1f;
        UpdateProgressUI();
        
        StartCoroutine(FadeOutAndHide());
    }
    
    System.Collections.IEnumerator FadeOutAndHide()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            
            float elapsed = 0f;
            while (elapsed < fadeSpeed)
            {
                elapsed += Time.deltaTime;
                color.a = elapsed / fadeSpeed;
                fadeImage.color = color;
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        isLoading = false;
        currentProgress = 0f;
        targetProgress = 0f;
        
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }
        
        ShowRandomTip();
    }
}

public class TipRotator : MonoBehaviour
{
    public TextMeshProUGUI tipDisplay;
    public string[] tips;
    public float tipChangeInterval = 5f;
    
    private int currentTipIndex = 0;
    private float timeSinceLastChange = 0f;
    
    void Update()
    {
        if (tips == null || tips.Length == 0) return;
        
        timeSinceLastChange += Time.deltaTime;
        
        if (timeSinceLastChange >= tipChangeInterval)
        {
            NextTip();
            timeSinceLastChange = 0f;
        }
    }
    
    void NextTip()
    {
        currentTipIndex = (currentTipIndex + 1) % tips.Length;
        
        if (tipDisplay != null)
        {
            StartCoroutine(FadeInNewTip());
        }
    }
    
    System.Collections.IEnumerator FadeInNewTip()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        
        Color originalColor = tipDisplay.color;
        originalColor.a = 0f;
        tipDisplay.color = originalColor;
        tipDisplay.text = tips[currentTipIndex];
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            originalColor.a = elapsed / duration;
            tipDisplay.color = originalColor;
            yield return null;
        }
        
        originalColor.a = 1f;
        tipDisplay.color = originalColor;
    }
}
