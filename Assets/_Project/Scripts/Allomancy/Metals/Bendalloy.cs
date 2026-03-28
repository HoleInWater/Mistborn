using UnityEngine;

/// <summary>
/// Implements the Bendalloy Allomancy ability (Slider).
/// Creates a "Speed Bubble" where time flows faster for those inside.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 170)]
public class Bendalloy : MonoBehaviour
{
    [Header("Settings — PHYSICS-MATH-BOOK.md Section 9")]
    public float baseBubbleRadius = 5f;
    [Tooltip("Handbook: τ_fast ≈ 10 (10x faster inside). Game-tuned lower for playability.")]
    public float timeScaleMultiplier = 8.0f; // Lore: ~10x faster inside bubble
    
    [Header("Flare Boosts")]
    public float maxBubbleRadius = 12f;

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
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Bendalloy;

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
        go.name = "BendalloySpeedBubble";
        go.transform.position = transform.position;
        
        float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
        currentRadius = Mathf.Lerp(baseBubbleRadius, maxBubbleRadius, (flareMult - 1f) / 1.5f);
        go.transform.localScale = Vector3.one * currentRadius * 2f;

        Renderer r = go.GetComponent<Renderer>();
        Shader hdrpLit = Shader.Find("HDRP/Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        r.material = new Material(hdrpLit);
        SetupTransparentMaterial(r.material);
        r.material.color = new Color(1f, 0.9f, 0.4f, 0.15f);

        currentBubble = go.AddComponent<TimeBubble>();
        // Lore: Duralumin-Bendalloy is extremely fast.
        // Set fields before Start() runs (next frame) so registration uses correct values.
        currentBubble.timeScaleMultiplier = timeScaleMultiplier * flareMult;
        currentBubble.creator = transform;
    }

    private void SetupTransparentMaterial(Material m)
    {
        // HDRP transparent surface setup
        if (m.HasProperty("_SurfaceType"))
        {
            m.SetFloat("_SurfaceType", 1f);         // 1 = Transparent
            m.SetFloat("_BlendMode", 0f);           // 0 = Alpha
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_TransparentZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else
        {
            // Fallback for non-HDRP
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_ALPHABLEND_ON");
        }
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
