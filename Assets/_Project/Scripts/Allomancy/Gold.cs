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
    private GameObject pastGhost;
    
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
            if (pastGhost == null) CreatePastGhost();
            UpdatePastGhost();
        }
        else if (wasBurning)
        {
            ClearGhost();
        }
    }
    
    void CreatePastGhost()
    {
        pastGhost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pastGhost.name = "GoldShadow";
        
        Renderer r = pastGhost.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1f, 0.8f, 0.2f, ghostAlpha);
        
        // Disable collider
        Destroy(pastGhost.GetComponent<Collider>());
    }

    void UpdatePastGhost()
    {
        if (pastGhost == null) return;
        // Lore: Gold shadow stands next to you, looking like you used to be.
        // For simplicity, it follows just behind.
        pastGhost.transform.position = Vector3.Lerp(pastGhost.transform.position, transform.position - transform.forward * 1.5f, Time.deltaTime * 2f);
        pastGhost.transform.rotation = Quaternion.Slerp(pastGhost.transform.rotation, transform.rotation, Time.deltaTime * 2f);
    }

    void ClearGhost()
    {
        if (pastGhost != null) Destroy(pastGhost);
    }

    void OnDestroy() => ClearGhost();
}
