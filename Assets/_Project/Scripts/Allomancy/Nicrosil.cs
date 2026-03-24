using UnityEngine;

/// <summary>
/// Implements the Nicrosil Allomancy ability (Nicroburst).
/// Supercharges the next metal burn of target Allomancers at close range.
/// </summary>
public class Nicrosil : MonoBehaviour
{
    [Header("Settings")]
    public float burstRange = 3f;
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
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Nicrosil;

        if (isBurning && !wasBurning)
        {
            Nicroburst();
        }
    }

    void Nicroburst()
    {
        // Lore: Nicroburst requires physical contact.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, burstRange, targetLayer))
        {
            Allomancer target = hit.collider.GetComponentInParent<Allomancer>();
            if (target != null)
            {
                target.isNicrobursting = true;
                Debug.Log($"[NICROSIL] Supercharged {target.name}");
                
                // Visual feedback (e.g. blue flash) could be added here
            }
        }
    }
}
