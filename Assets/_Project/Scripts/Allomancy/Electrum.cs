using UnityEngine;

/// <summary>
/// Implements the Electrum Allomancy ability (Oracle).
/// Shows "Electrum Shadows" (future selves) of the user to protect against Atium.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Electrum : MonoBehaviour
{
    [Header("Settings")]
    public float ghostAlpha = 0.4f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private GameObject futureGhost;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Electrum;

        if (isBurning)
        {
            if (futureGhost == null) CreateFutureGhost();
            UpdateFutureGhost();
        }
        else if (wasBurning)
        {
            ClearGhost();
        }
    }
    
    void CreateFutureGhost()
    {
        futureGhost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        futureGhost.name = "ElectrumShadow";
        
        Renderer r = futureGhost.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1f, 1f, 1f, ghostAlpha);
        
        // Disable collider
        Destroy(futureGhost.GetComponent<Collider>());
    }

    void UpdateFutureGhost()
    {
        if (futureGhost == null) return;
        // Lore: Electrum shadows flit around you.
        Vector3 offset = new Vector3(Mathf.Sin(Time.time * 5f), 0, Mathf.Cos(Time.time * 5f)) * 1.0f;
        futureGhost.transform.position = transform.position + offset;
    }

    void ClearGhost()
    {
        if (futureGhost != null) Destroy(futureGhost);
    }

    void OnDestroy() => ClearGhost();
}
