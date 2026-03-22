/// <summary>
/// Screen flash effect.
/// Usage: ScreenFlash.Flash(Color.red, 0.3f);
/// </summary>
public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }
    
    // UI IMAGE - Assign in Inspector
    public UnityEngine.UI.Image flashImage;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Create flash image if not assigned
            if (flashImage == null)
            {
                CreateFlashImage();
            }
        }
    }
    
    void CreateFlashImage()
    {
        // Create canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FlashCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create flash image
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.parent = canvas.transform;
        flashImage = flashObj.AddComponent<UnityEngine.UI.Image>();
        flashImage.color = Color.clear;
        flashImage.raycastTarget = false;
        
        // Fill screen
        flashImage.rectTransform.anchorMin = Vector2.zero;
        flashImage.rectTransform.anchorMax = Vector2.one;
        flashImage.rectTransform.sizeDelta = Vector2.zero;
    }
    
    // Flash screen with color
    public static void Flash(Color color, float duration = 0.2f)
    {
        if (Instance != null && Instance.flashImage != null)
        {
            Instance.StartCoroutine(Instance.FlashRoutine(color, duration));
        }
    }
    
    System.Collections.IEnumerator FlashRoutine(Color color, float duration)
    {
        flashImage.color = color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        flashImage.color = Color.clear;
    }
    
    // Quick helpers
    public static void DamageFlash()
    {
        Flash(new Color(1f, 0f, 0f, 0.3f), 0.15f);
    }
    
    public static void HealFlash()
    {
        Flash(new Color(0f, 1f, 0f, 0.3f), 0.2f);
    }
    
    public static void DeathFlash()
    {
        Flash(new Color(0f, 0f, 0f, 1f), 1f);
    }
}
