using UnityEngine;
/// <summary>
/// Floating damage/healing numbers.
/// Usage: FloatingText.Show(worldPosition, "10", Color.red);
/// </summary>
public class FloatingText : MonoBehaviour
{
    // SETTINGS
    public float floatSpeed = 2f;            // How fast it rises
    public float fadeSpeed = 1f;             // How fast it fades
    public float lifetime = 1f;              // Total lifetime
    
    // VISUALS
    public TextMesh textMesh;
    public Color textColor = Color.white;
    
    private float currentLifetime;
    
    void Start()
    {
        currentLifetime = lifetime;
    }
    
    void Update()
    {
        // Rise up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        // Fade out
        currentLifetime -= Time.deltaTime;
        float alpha = currentLifetime / lifetime;
        
        if (textMesh != null)
        {
            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
        }
        
        // Destroy when faded
        if (currentLifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    // Static method to create floating text
    public static void Show(Vector3 position, string text, Color color)
    {
        GameObject obj = new GameObject("FloatingText");
        obj.transform.position = position;
        
        TextMesh tm = obj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 24;
        tm.color = color;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        
        obj.AddComponent<FloatingText>();
    }
    
    public static void Damage(Vector3 position, float amount)
    {
        Show(position, $"-{amount:F0}", Color.red);
    }
    
    public static void Heal(Vector3 position, float amount)
    {
        Show(position, $"+{amount:F0}", Color.green);
    }
}
