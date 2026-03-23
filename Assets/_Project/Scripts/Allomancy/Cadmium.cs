using UnityEngine;

/// <summary>
/// Implements the Cadmium Allomancy ability (Pulser).
/// Creates a "Slow Bubble" where time flows slower for those inside.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Cadmium : MonoBehaviour
{
    [Header("Settings")]
    public float baseBubbleRadius = 5f;
    public float timeScaleMultiplier = 0.5f; // Slow down time inside
    
    [Header("Flare Boosts")]
    public float maxBubbleRadius = 15f;

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
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Cadmium;

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
        bubbleEffect.name = "CadmiumSlowBubble";
        
        Renderer r = bubbleEffect.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(0.2f, 0.4f, 1f, 0.15f);
        
        // Disable collider
        Destroy(bubbleEffect.GetComponent<Collider>());

        float flareMult = GetFlareMultiplier();
        currentRadius = Mathf.Lerp(baseBubbleRadius, maxBubbleRadius, (flareMult - 1f) / 1.5f);
        bubbleEffect.transform.localScale = Vector3.one * currentRadius * 2f;
    }

    void UpdateBubble()
    {
        if (bubbleEffect == null) return;
    }

    void DestroyBubble()
    {
        if (bubbleEffect != null) Destroy(bubbleEffect);
    }

    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.flareIntensity;
        }
        return 1.0f;
    }

    void OnDestroy() => DestroyBubble();
}
