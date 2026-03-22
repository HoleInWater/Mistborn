using UnityEngine;
using System.Collections.Generic;

public class HemalurgySystem : MonoBehaviour
{
    [Header("Hemalurgy Settings")]
    public bool hasSpikes = false;
    public List<HemalurgicSpike> spikes;
    
    [Header("Stolen Attributes")]
    public Dictionary<MetalType, HemalurgicAttribute> stolenPowers;
    
    [Header("Construct Type")]
    public ConstructType constructType = ConstructType.None;
    
    private void Awake()
    {
        stolenPowers = new Dictionary<MetalType, HemalurgicAttribute>();
    }
    
    public void AddSpike(HemalurgicSpikeData spikeData)
    {
        HemalurgicSpike spike = new HemalurgicSpike();
        spike.metalType = spikeData.metalType;
        spike.stolenAttribute = spikeData.attribute;
        spike.powerLevel = spikeData.powerLevel;
        spike.bindPoint = spikeData.bindPoint;
        
        spikes.Add(spike);
        stolenPowers[spike.metalType] = new HemalurgicAttribute(spikeData.attribute, spikeData.powerLevel);
        
        hasSpikes = true;
        ApplyHemalurgicEffect(spike);
        
        Debug.Log($"Added {spike.metalType} spike - Stealing: {spike.stolenAttribute}");
    }
    
    void ApplyHemalurgicEffect(HemalurgicSpike spike)
    {
        switch (spike.metalType)
        {
            case MetalType.Iron:
                ApplyIronStrength(spike.powerLevel);
                if (constructType == ConstructType.Koloss)
                    ApplyKolossTransformation();
                break;
            case MetalType.Steel:
                ApplySteelStrength(spike.powerLevel);
                break;
            case MetalType.Tin:
                ApplyTinEyes();
                break;
            case MetalType.Pewter:
                ApplyPewterEndurance(spike.powerLevel);
                break;
            case MetalType.Chromium:
                ApplyNicrosilBoost(spike.powerLevel);
                break;
            case MetalType.Nicrosil:
                ApplyNicrosilBoost(spike.powerLevel);
                break;
        }
    }
    
    void ApplyIronStrength(float power)
    {
        Debug.Log($"Iron strength: {power}x enhanced");
    }
    
    void ApplySteelStrength(float power)
    {
        Debug.Log($"Allomantic steel strength: {power}x enhanced");
    }
    
    void ApplyTinEyes()
    {
        Debug.Log("Tin Eyes: Enhanced sensory perception");
    }
    
    void ApplyPewterEndurance(float power)
    {
        Debug.Log($"Pewter endurance: {power}x enhanced");
    }
    
    void ApplyNicrosilBoost(float power)
    {
        Debug.Log($"Nicrosil Investiture boost: {power}x");
    }
    
    void ApplyKolossTransformation()
    {
        constructType = ConstructType.Koloss;
        transform.localScale *= 1.5f;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass *= 2f;
        }
        
        Debug.Log("Koloss transformation complete");
    }
    
    public bool HasPower(MetalType metal)
    {
        return stolenPowers.ContainsKey(metal);
    }
    
    public float GetPowerLevel(MetalType metal)
    {
        if (stolenPowers.TryGetValue(metal, out var attr))
        {
            return attr.powerLevel;
        }
        return 0f;
    }
    
    public void RemoveSpike(int index)
    {
        if (index < 0 || index >= spikes.Count) return;
        
        HemalurgicSpike spike = spikes[index];
        
        if (constructType == ConstructType.Inquisitor && spike.bindPoint == BindPoint.Linchpin)
        {
            Debug.Log("Linchpin spike removed - DEATH");
            return;
        }
        
        stolenPowers.Remove(spike.metalType);
        spikes.RemoveAt(index);
        
        Debug.Log($"Removed {spike.metalType} spike");
    }
}

[System.Serializable]
public class HemalurgicSpike
{
    public MetalType metalType;
    public string stolenAttribute;
    public float powerLevel;
    public BindPoint bindPoint;
    public bool isActive = true;
}

[System.Serializable]
public class HemalurgicSpikeData
{
    public MetalType metalType;
    public string attribute;
    public float powerLevel;
    public BindPoint bindPoint;
}

public enum ConstructType
{
    None,
    Koloss,
    Kandra,
    Inquisitor,
    Custom
}

public enum BindPoint
{
    Heart,
    Eye,
    Temple,
    Spine,
    Arm,
    Leg,
    Linchpin
}

public class HemalurgicAttribute
{
    public string name;
    public float powerLevel;
    public bool isActive;
    
    public HemalurgicAttribute(string n, float power)
    {
        name = n;
        powerLevel = power;
        isActive = true;
    }
}

public class HemalurgicSpikeCreator : MonoBehaviour
{
    public static HemalurgicSpike CreateSpike(MetalType metal, string attribute, float power)
    {
        HemalurgicSpike spike = new HemalurgicSpike();
        spike.metalType = metal;
        spike.stolenAttribute = attribute;
        spike.powerLevel = power;
        spike.bindPoint = GetDefaultBindPoint(metal);
        
        return spike;
    }
    
    static BindPoint GetDefaultBindPoint(MetalType metal)
    {
        switch (metal)
        {
            case MetalType.Iron: return BindPoint.Heart;
            case MetalType.Steel: return BindPoint.Arm;
            case MetalType.Tin: return BindPoint.Eye;
            case MetalType.Pewter: return BindPoint.Leg;
            default: return BindPoint.Heart;
        }
    }
}
