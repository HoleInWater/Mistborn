using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements the Malatium Allomancy ability.
/// Allows seeing a person’s past or what they could have been.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Malatium : MonoBehaviour
{
    [Header("Settings")]
    public float baseRevealRange = 15f;
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    [Header("Flare Boosts")]
    public float maxRevealRange = 35f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        // Check if we are currently burning Malatium according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Malatium;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();
            RevealTrueNature(flareMult);
        }
        else if (wasBurning)
        {
            ResetReveals();
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

    void RevealTrueNature(float flareMult)
    {
        float currentRange = Mathf.Lerp(baseRevealRange, maxRevealRange, (flareMult - 1f) / 1.5f);
        Collider[] nearby = Physics.OverlapSphere(transform.position, currentRange);
        
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            Renderer targetRenderer = col.GetComponent<Renderer>();
            if (targetRenderer != null && !originalMaterials.ContainsKey(targetRenderer))
            {
                // Store original and apply spectral shift
                originalMaterials.Add(targetRenderer, targetRenderer.material);
                
                Material malatMat = new Material(targetRenderer.material);
                malatMat.color = malatiumColor;
                malatMat.name += " (Malatium)";
                targetRenderer.material = malatMat;
                
                // Start a decay coroutine to restore if they leave range or we stop
                StartCoroutine(TimedRestore(targetRenderer, 2.0f));
            }
        }
    }

    IEnumerator TimedRestore(Renderer r, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (r != null && originalMaterials.ContainsKey(r))
        {
            if (!isBurning) // Only restore if we stopped burning or pulse finished
            {
                r.material = originalMaterials[r];
                originalMaterials.Remove(r);
            }
        }
    }

    void ResetReveals()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null) kvp.Key.material = kvp.Value;
        }
        originalMaterials.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.8f, 0.4f, 0.2f, 0.3f);
            float flareMult = GetFlareMultiplier();
            float currentRange = Mathf.Lerp(baseRevealRange, maxRevealRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}