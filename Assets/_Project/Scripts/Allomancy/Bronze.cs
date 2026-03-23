using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Bronze Allomancy ability (Seeker).
/// Detects nearby Allomantic pulses unless hidden by a Copper cloud.
/// </summary>
public class Bronze : MonoBehaviour
{
    [Header("Settings")]
    public float baseDetectionRadius = 30f;
    public float pulseInterval = 0.4f;
    
    [Header("Flare Boosts")]
    public float maxDetectionRadius = 80f;

    [Header("References")]
    public Allomancer allomancer;
    
    [Header("Pulse Parameters")]
    public float minPulseInterval = 0.2f; // Rapid (Physical)
    public float maxPulseInterval = 2.0f; // Slow (Temporal)
    public Color pulseColor = new Color(0.8f, 0.5f, 0.1f, 0.5f);
    public AudioClip pulseAudioClip;
    
    public enum MetalCategory { Physical, Mental, Temporal, Enhancement }

    private bool isBurning = false;
    private Dictionary<Allomancer, float> targetPulseTimers = new Dictionary<Allomancer, float>();
    private List<Allomancer> detectedAllomancers = new List<Allomancer>();
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        // Check if we are currently burning Bronze according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Bronze;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();
            DetectPulses(flareMult);
        }
    }
    
    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.flareIntensity;
        }
        return 1.0f;
    }

    void DetectPulses(float flareMult)
    {
        float currentRadius = Mathf.Lerp(baseDetectionRadius, maxDetectionRadius, (flareMult - 1f) / 1.5f);
        
        // Optimize: Use a static list of allomancers if possible, or OverlapSphere and check components
        Allomancer[] others = FindObjectsOfType<Allomancer>();
        detectedAllomancers.Clear();

        foreach (var other in others)
        {
            if (other == allomancer || !other.IsBurning()) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist <= currentRadius && !Copper.IsPulseHidden(other.transform.position))
            {
                detectedAllomancers.Add(other);
                HandleTargetPulse(other, flareMult);
            }
        }
    }

    void HandleTargetPulse(Allomancer target, float flareMult)
    {
        if (!targetPulseTimers.ContainsKey(target))
            targetPulseTimers[target] = 0f;

        targetPulseTimers[target] -= Time.deltaTime;

        if (targetPulseTimers[target] <= 0f)
        {
            AllomancySkill.MetalType metal = target.GetCurrentMetal();
            MetalCategory category = GetMetalCategory(metal);
            float interval = GetPulseInterval(category);
            
            // Scaled by flare and distance? Lore: flaring makes them sharper, distance makes them harder to hear/feel.
            targetPulseTimers[target] = interval;
            
            PlayPulseFeedback(target, metal, category);
        }
    }

    MetalCategory GetMetalCategory(AllomancySkill.MetalType metal)
    {
        int index = (int)metal;
        if (index < 4) return MetalCategory.Physical;
        if (index < 8) return MetalCategory.Mental;
        if (index < 12) return MetalCategory.Temporal;
        return MetalCategory.Enhancement;
    }

    float GetPulseInterval(MetalCategory category)
    {
        switch (category)
        {
            case MetalCategory.Physical: return 0.4f;
            case MetalCategory.Mental: return 0.8f;
            case MetalCategory.Temporal: return 1.5f;
            case MetalCategory.Enhancement: return 1.2f;
            default: return 1.0f;
        }
    }

    void PlayPulseFeedback(Allomancer target, AllomancySkill.MetalType metal, MetalCategory category)
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);
        float volume = Mathf.Clamp01(1f - (dist / baseDetectionRadius));
        
        // LORE: Seeker "hears" or "feels" the pulse.
        Debug.Log($"[BRONZE] Pulse: {metal} ({category}) from {target.name} - Volume: {volume:F2}");
        
        // Spawn Visual Pulse
        CreateVisualPulse(target.transform.position, metal, category);
        
        // Play Audio Thump with appropriate pitch
        if (pulseAudioClip != null)
        {
            float pitch = GetPitchForCategory(category);
            AudioSource.PlayClipAtPoint(pulseAudioClip, target.transform.position, volume);
            // Note: PlayClipAtPoint doesn't support pitch easily; ideally use a pooled AudioSource
        }
    }

    float GetPitchForCategory(MetalCategory category)
    {
        switch (category)
        {
            case MetalCategory.Physical: return 1.5f;
            case MetalCategory.Mental: return 1.0f;
            case MetalCategory.Temporal: return 0.5f;
            case MetalCategory.Enhancement: return 0.8f;
            default: return 1.0f;
        }
    }

    void CreateVisualPulse(Vector3 position, AllomancySkill.MetalType metal, MetalCategory category)
    {
        GameObject pulse = new GameObject("BronzePulseRing");
        pulse.transform.position = position;
        
        LineRenderer lr = pulse.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 32;
        lr.startWidth = 0.15f;
        lr.endWidth = 0.05f;
        
        Color c = GetColorForMetal(metal, category);
        lr.startColor = c;
        lr.endColor = new Color(c.r, c.g, c.b, 0);
        
        lr.material = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < 32; i++)
        {
            float angle = i * Mathf.PI * 2 / 32;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
        }

        StartCoroutine(AnimatePulse(pulse, lr, category));
    }

    Color GetColorForMetal(AllomancySkill.MetalType metal, MetalCategory category)
    {
        // Category bases
        switch (category)
        {
            case MetalCategory.Physical: return new Color(0.7f, 0.7f, 0.9f, 0.6f); // Steel-blue
            case MetalCategory.Mental: return new Color(1.0f, 0.9f, 0.4f, 0.6f);   // Brass-yellow
            case MetalCategory.Temporal: return new Color(1.0f, 1.0f, 1.0f, 0.8f); // Ethereal White
            case MetalCategory.Enhancement: return new Color(0.8f, 0.4f, 1.0f, 0.6f); // Vibrant Purple
            default: return pulseColor;
        }
    }

    System.Collections.IEnumerator AnimatePulse(GameObject obj, LineRenderer lr, MetalCategory category)
    {
        float elapsed = 0f;
        float duration = category == MetalCategory.Temporal ? 1.2f : 0.6f;
        float targetScale = category == MetalCategory.Enhancement ? 8f : 5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, targetScale, t);
            
            Color c = lr.startColor;
            lr.startColor = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }
        Destroy(obj);
    }

    public List<Allomancer> GetDetectedAllomancers() => detectedAllomancers;

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.8f, 0.4f, 0f, 0.2f);
            float flareMult = GetFlareMultiplier();
            float currentRadius = Mathf.Lerp(baseDetectionRadius, maxDetectionRadius, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}