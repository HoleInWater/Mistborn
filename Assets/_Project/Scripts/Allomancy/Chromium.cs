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
        // Lore: Leecher requires physical contact. Short range raycast + sphere fallback handles this.
        RaycastHit hit;
        Allomancer target = null;

        if (Physics.Raycast(transform.position, transform.forward, out hit, leechRange, targetLayer))
        {
            target = hit.collider.GetComponentInParent<Allomancer>();
        }
        else
        {
            // Sphere check for "messy" contact
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, leechRange * 0.5f, targetLayer);
            if (hitColliders.Length > 0) target = hitColliders[0].GetComponentInParent<Allomancer>();
        }

        if (target != null)
        {
            target.ClearAllReserves();
            Debug.Log($"[CHROMIUM] Leeched all metals from {target.name}");
            // Reset burning to avoid multiple wipes per frame
            allomancer.StopBurning();
        }
    }
}
