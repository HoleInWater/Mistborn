using UnityEngine;

/// <summary>
/// Implements the Gold Allomancy ability (Augur).
/// Shows a "Gold Shadow" (past self) of the user.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Gold : MonoBehaviour
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
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Gold;

        if (isBurning)
        {
            if (ghostRenderer == null) CreatePastGhost();
            UpdatePastGhost();
        }
        else if (wasBurning)
        {
            ClearGhost();
        }
    }
    
    void CreatePastGhost()
    {
        ghostRenderer = gameObject.AddComponent<GhostRenderer>();
        // Lore: Gold shadow looks like you.
        ghostRenderer.SetupGhost(gameObject, new Color(1f, 0.8f, 0.2f), ghostAlpha);
    }

    void UpdatePastGhost()
    {
        if (ghostRenderer == null) return;
        // Follows slightly behind at the "past" position
        Vector3 pastPos = transform.position - transform.forward * 1.5f + Vector3.right * 0.5f;
        ghostRenderer.UpdateTransform(pastPos, transform.rotation);
    }

    void ClearGhost()
    {
        if (ghostRenderer != null) Destroy(ghostRenderer);
    }


    void OnDestroy() => ClearGhost();
}
