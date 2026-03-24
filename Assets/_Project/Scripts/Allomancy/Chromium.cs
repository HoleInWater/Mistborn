using UnityEngine;

/// <summary>
/// Implements the Chromium Allomancy ability (Leecher).
/// Wipes the metal reserves of another Allomancer upon touch/close range.
/// </summary>
public class Chromium : MonoBehaviour
{
    [Header("Settings")]
    public float leechRange = 3f;
    public LayerMask targetLayer;

    [Header("References")]
    public Allomancer allomancer;

    private bool isBurning = false;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Chromium;

        if (isBurning && !wasBurning)
        {
            Leech();
        }
    }

    void Leech()
    {
        // Lore: Leecher requires physical contact. Short range raycast/sphere handles this.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, leechRange, targetLayer))
        {
            Allomancer target = hit.collider.GetComponentInParent<Allomancer>();
            if (target != null)
            {
                target.ClearAllReserves();
                Debug.Log($"[CHROMIUM] Leeched all metals from {target.name}");
                
                // Visual feedback (e.g. spark or flash) could be added here
            }
        }
    }
}
