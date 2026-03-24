using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Atium Allomancy ability (Seer).
/// Shows "Atium Shadows" (future ghosts) of nearby entities.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Atium : MonoBehaviour
{
    [Header("Settings")]
    public float baseVisionRange = 25f;
    public float ghostAlpha = 0.3f;
    
    [Header("Flare Boosts")]
    public float maxVisionRange = 60f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private List<GameObject> futureGhosts = new List<GameObject>();
    private float lastGhostUpdate = 0f;
    private float ghostUpdateInterval = 0.05f; // Update ghost positions frequently
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        // Check if we are currently burning Atium according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Atium;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();
            UpdateFutureVision(flareMult);
        }
        else if (wasBurning)
        {
            ClearFutures();
        }
    }
    
    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.FlareIntensity;
        }
        return 1.0f;
    }

    void UpdateFutureVision(float flareMult)
    {
        if (Time.time - lastGhostUpdate < ghostUpdateInterval) return;
        lastGhostUpdate = Time.time;

        ClearFutures();

        float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
        
        // Find potential targets (AI and other Allomancers)
        AIController[] enemies = FindObjectsOfType<AIController>();
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= currentRange)
            {
                CreateFutureGhost(enemy.gameObject);
            }
        }
    }

    void CreateFutureGhost(GameObject target)
    {
        // Lore: Atium shadows are slightly ahead in time. 
        // For game logic, we'll just instantiate a semi-transparent copy and offset it based on velocity.
        GameObject ghost = Instantiate(target, target.transform.position, target.transform.rotation);
        ghost.name = $"AtiumShadow_{target.name}";
        
        // Offset logically based on current velocity to represent "future"
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            ghost.transform.position += rb.linearVelocity * 0.5f; // 0.5s into future
        }

        // Make transparent
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
                
                Color c = m.color;
                m.color = new Color(c.r, c.g, c.b, ghostAlpha);
            }
        }

        // Disable logic/colliders on ghost
        MonoBehaviour[] scripts = ghost.GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts) s.enabled = false;
        
        Collider[] colliders = ghost.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        futureGhosts.Add(ghost);
    }

    void ClearFutures()
    {
        foreach (var ghost in futureGhosts)
        {
            if (ghost != null) Destroy(ghost);
        }
        futureGhosts.Clear();
    }

    void OnDestroy() => ClearFutures();

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            float flareMult = GetFlareMultiplier();
            float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}