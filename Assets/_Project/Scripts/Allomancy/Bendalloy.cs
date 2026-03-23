using UnityEngine;

/// <summary>
/// Implements the Bendalloy Allomancy ability (Slider).
/// Creates a "Speed Bubble" where time flows faster for those inside.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Bendalloy : MonoBehaviour
{
    [Header("Settings")]
    public float baseBubbleRadius = 5f;
    public float timeScaleMultiplier = 2.0f; // Speed up time inside
    
    [Header("Flare Boosts")]
    public float maxBubbleRadius = 12f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private GameObject bubbleEffect;
    private float currentRadius;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Bendalloy;

        if (isBurning)
        {
            if (bubbleEffect == null) CreateBubble();
            UpdateBubble();
        }
        else if (wasBurning)
        {
            DestroyBubble();
        }
    }
    
    void CreateBubble()
    {
        bubbleEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubbleEffect.name = "BendalloySpeedBubble";
        
        Renderer r = bubbleEffect.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1f, 0.9f, 0.4f, 0.15f);
        
        // Disable collider
        Destroy(bubbleEffect.GetComponent<Collider>());
        
        float flareMult = GetFlareMultiplier();
        currentRadius = Mathf.Lerp(baseBubbleRadius, maxBubbleRadius, (flareMult - 1f) / 1.5f);
        bubbleEffect.transform.localScale = Vector3.one * currentRadius * 2f;
    }

    void UpdateBubble()
    {
        if (bubbleEffect == null) return;
        // Bubble is stationary relative to the location where it was created, 
        // OR it follows the user? Lore: Usually stationary.
        // But for game feel, let's keep it stationary for now.
    }

    void DestroyBubble()
    {
        if (bubbleEffect != null) Destroy(bubbleEffect);
    }

    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.FlareIntensity;
        }
        return 1.0f;
    }

    void OnDestroy() => DestroyBubble();
}
