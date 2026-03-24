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
    private GhostRenderer ghostRenderer;
    
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
            if (ghostRenderer == null) CreateFutureGhost();
            UpdateFutureGhost();
        }
        else if (wasBurning)
        {
            ClearGhost();
        }
    }
    
    void CreateFutureGhost()
    {
        ghostRenderer = gameObject.AddComponent<GhostRenderer>();
        // Lore: Electrum shadows flit around you.
        ghostRenderer.SetupGhost(gameObject, new Color(1f, 1f, 1f), ghostAlpha);
    }

    void UpdateFutureGhost()
    {
        if (ghostRenderer == null) return;
        // Lore: Electrum shadows flit around you.
        Vector3 offset = new Vector3(Mathf.Sin(Time.time * 5f), 0, Mathf.Cos(Time.time * 5f)) * 1.5f;
        ghostRenderer.UpdateTransform(transform.position + offset, transform.rotation);
    }

    void ClearGhost()
    {
        if (ghostRenderer != null) Destroy(ghostRenderer);
    }


    void OnDestroy() => ClearGhost();
}
