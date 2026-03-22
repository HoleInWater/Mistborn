/// <summary>
/// Color flash on materials (for hit effects, etc.)
/// Usage: MaterialFlash.Flash(renderer, Color.red, 0.2f);
/// </summary>
public class MaterialFlash : MonoBehaviour
{
    // Flash a specific renderer
    public static void Flash(Renderer renderer, Color flashColor, float duration)
    {
        if (renderer == null) return;
        
        Instance.StartCoroutine(Instance.FlashRoutine(renderer, flashColor, duration));
    }
    
    private static MaterialFlash Instance
    {
        get
        {
            MaterialFlash inst = FindObjectOfType<MaterialFlash>();
            if (inst == null)
            {
                GameObject obj = new GameObject("MaterialFlash");
                inst = obj.AddComponent<MaterialFlash>();
            }
            return inst;
        }
    }
    
    System.Collections.IEnumerator FlashRoutine(Renderer renderer, Color flashColor, float duration)
    {
        Material mat = renderer.material;
        Color originalColor = mat.color;
        
        mat.color = flashColor;
        yield return new WaitForSeconds(duration);
        mat.color = originalColor;
    }
}
