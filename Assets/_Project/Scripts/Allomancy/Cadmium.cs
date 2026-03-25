using UnityEngine;

/// <summary>
/// Implements the Cadmium Allomancy ability (Pulser).
/// Creates a "Slow Bubble" where time flows slower for those inside.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Cadmium : MonoBehaviour
{
    [Header("Settings — PHYSICS-MATH-BOOK.md Section 9")]
    public float baseBubbleRadius = 5f;
    [Tooltip("Handbook: τ_slow ≈ 0.1 (10x slower inside). Game-tuned higher for playability.")]
    public float timeScaleMultiplier = 0.15f; // Lore: ~10x slower inside bubble
    
    [Header("Flare Boosts")]
    public float maxBubbleRadius = 15f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float currentRadius;
    private TimeBubble currentBubble;
    
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
            if (currentBubble == null) CreateBubble();
        }
        else if (wasBurning)
        {
            DestroyBubble();
        }
    }
    
    void CreateBubble()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "CadmiumSlowBubble";
        go.transform.position = transform.position;
        
        float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
        currentRadius = Mathf.Lerp(baseBubbleRadius, maxBubbleRadius, (flareMult - 1f) / 1.5f);
        go.transform.localScale = Vector3.one * currentRadius * 2f;

        Renderer r = go.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard")); 
        r.material.color = new Color(0.2f, 0.4f, 1f, 0f); // Start invisible
        SetupTransparentMaterial(r.material);

        currentBubble = go.AddComponent<TimeBubble>();
        // Lore: Duralumin-Cadmium is extremely slow.
        currentBubble.timeScaleMultiplier = Mathf.Clamp(timeScaleMultiplier / flareMult, 0.01f, 1f);
    }

    private void SetupTransparentMaterial(Material m)
    {
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.renderQueue = 3000;
    }

    void DestroyBubble()
    {
        if (currentBubble != null)
        {
            currentBubble.Shutdown();
            currentBubble = null;
        }
    }


    // Removed local GetFlareMultiplier

    void OnDestroy() => DestroyBubble();
}
