using UnityEngine;
using System.Collections.Generic;

public class CognitiveAllomancy : MonoBehaviour
{
    [Header("Copper - Cloud (Hide Pulses)")]
    public float cloudRadius = 15f;
    public float cloudMetalCostPerSecond = 1.5f;
    public bool hideOwnPulse = true;

    [Header("Bronze - Seek (Detect Pulses)")]
    public float seekRange = 50f;
    public float seekMetalCostPerSecond = 1.5f;
    public bool showPulseDirection = true;

    [Header("References")]
    public Allomancer allomancer;
    public LayerMask targetLayer;
    public Transform pulseIndicator;

    private bool isCopperBurning = false;
    private bool isBronzeBurning = false;
    private List<Allomancer> detectedPulses = new List<Allomancer>();

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
        targetLayer = LayerMask.GetMask("Character");
    }

    void Update()
    {
        bool wasCopper = isCopperBurning;
        bool wasBronze = isBronzeBurning;

        isCopperBurning = allomancer != null && allomancer.IsBurning() && 
                          allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Copper;
        isBronzeBurning = allomancer != null && allomancer.IsBurning() && 
                           allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Bronze;

        if (isCopperBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            ActivateCloud(flareMult);
            DrainMetal(AllomancySkill.MetalType.Copper, cloudMetalCostPerSecond * flareMult * Time.deltaTime);
        }

        if (isBronzeBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SeekPulses(flareMult);
            DrainMetal(AllomancySkill.MetalType.Bronze, seekMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasBronze)
        {
            ClearPulseDetection();
        }
    }

    void ActivateCloud(float flareMult)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, cloudRadius * flareMult, targetLayer);

        foreach (Collider c in targets)
        {
            Allomancer target = c.GetComponentInParent<Allomancer>();
            if (target != null && target != allomancer)
            {
                target.isHiddenByCloud = true;
            }
        }

        if (allomancer != null)
        {
            allomancer.isHiddenByCloud = hideOwnPulse;
        }
    }

    void SeekPulses(float flareMult)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, seekRange * flareMult, targetLayer);
        detectedPulses.Clear();

        foreach (Collider c in targets)
        {
            Allomancer target = c.GetComponentInParent<Allomancer>();
            if (target != null && target != allomancer && !target.isHiddenByCloud)
            {
                detectedPulses.Add(target);
                ShowPulseIndicator(target.transform.position);
            }
        }

        if (GetComponent<AllomanticSight>())
        {
            GetComponent<AllomanticSight>().HighlightAllomancers(detectedPulses);
        }
    }

    void ShowPulseIndicator(Vector3 position)
    {
        if (pulseIndicator == null) return;
        
        pulseIndicator.gameObject.SetActive(true);
        pulseIndicator.position = position;
        pulseIndicator.LookAt(transform);
    }

    void ClearPulseDetection()
    {
        detectedPulses.Clear();
        if (pulseIndicator != null)
        {
            pulseIndicator.gameObject.SetActive(false);
        }
    }

    void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(metal, amount);
    }

    public List<Allomancer> GetDetectedPulses() => detectedPulses;
    public bool IsCloudActive() => isCopperBurning;
    public bool IsSeekingActive() => isBronzeBurning;
}