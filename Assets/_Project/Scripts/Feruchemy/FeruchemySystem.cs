using UnityEngine;
using System.Collections.Generic;

public class FeruchemySystem : MonoBehaviour
{
    [Header("Feruchemy Settings")]
    public bool isFeruchemist = false;
    public FeruchemyMetal[] storedMetals;
    
    [Header("Current Storage State")]
    public bool isStoring = false;
    public bool isTapping = false;
    public FeruchemyMetal currentStorage;
    
    [Header("Compounding")]
    public bool canCompound = false;
    public bool isCompounding = false;
    
    private Dictionary<MetalType, FeruchemyMetal> metalStorage;
    
    void Start()
    {
        InitializeStorage();
    }
    
    void InitializeStorage()
    {
        metalStorage = new Dictionary<MetalType, FeruchemyMetal>();
        
        MetalType[] types = new MetalType[] 
        {
            MetalType.Iron,
            MetalType.Steel,
            MetalType.Tin,
            MetalType.Pewter,
            MetalType.Chromium,
            MetalType.Nicrosil,
            MetalType.Cadmium,
            MetalType.Bendalloy,
            MetalType.Gold,
            MetalType.Electrum,
            MetalType.Aluminum,
            MetalType.Duralumin,
            MetalType.Zinc,
            MetalType.Brass,
            MetalType.Copper,
            MetalType.Bronze
        };
        
        foreach (MetalType type in types)
        {
            metalStorage[type] = new FeruchemyMetal(type, 0f, 0f);
        }
    }
    
    void Update()
    {
        HandleStorageInput();
        ProcessCompounding();
    }
    
    void HandleStorageInput()
    {
        if (!isFeruchemist) return;
        
        if (Input.GetKey(KeyCode.K))
        {
            StartStoring(currentStorage);
        }
        else if (Input.GetKey(KeyCode.L))
        {
            StartTapping(currentStorage);
        }
        else
        {
            StopStorage();
        }
    }
    
    public void StartStoring(FeruchemyMetal metal)
    {
        if (!isFeruchemist || metal.currentStored <= 0) return;
        
        isStoring = true;
        isTapping = false;
        
        float rate = metal.storageRate * Time.deltaTime;
        metal.currentStored = Mathf.Min(metal.maxStorage, metal.currentStored + rate);
    }
    
    public void StartTapping(FeruchemyMetal metal)
    {
        if (!isFeruchemist || metal.currentStored <= 0) return;
        
        isTapping = true;
        isStoring = false;
        
        float rate = metal.tapRate * Time.deltaTime;
        metal.currentStored = Mathf.Max(0, metal.currentStored - rate);
        
        ApplyFeruchemyEffect(metal.type, rate);
    }
    
    public void StopStorage()
    {
        isStoring = false;
        isTapping = false;
    }
    
    void ApplyFeruchemyEffect(MetalType type, float amount)
    {
        switch (type)
        {
            case MetalType.Iron:
                ApplyIronEffect(amount);
                break;
            case MetalType.Steel:
                ApplySteelEffect(amount);
                break;
            case MetalType.Tin:
                ApplyTinEffect(amount);
                break;
            case MetalType.Pewter:
                ApplyPewterEffect(amount);
                break;
            case MetalType.Chromium:
                ApplyChromiumEffect(amount);
                break;
            case MetalType.Nicrosil:
                ApplyNicrosilEffect(amount);
                break;
        }
    }
    
    void ApplyIronEffect(float amount)
    {
        FeruchemyMetal iron = metalStorage[MetalType.Iron];
        float multiplier = 1f + (iron.currentStored / iron.maxStorage);
        
        gameObject.GetComponent<Rigidbody>()?.mass *= multiplier;
    }
    
    void ApplySteelEffect(float amount)
    {
        FeruchemyMetal steel = metalStorage[MetalType.Steel];
        float speedMultiplier = 1f + (steel.currentStored / steel.maxStorage);
        
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            float originalSpeed = 5f;
            controller.Move(Vector3.forward * originalSpeed * speedMultiplier * Time.deltaTime);
        }
    }
    
    void ApplyTinEffect(float amount)
    {
        FeruchemyMetal tin = metalStorage[MetalType.Tin];
        float senseMultiplier = 1f + (tin.currentStored / tin.maxStorage);
        
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f * senseMultiplier, Time.deltaTime);
    }
    
    void ApplyPewterEffect(float amount)
    {
        FeruchemyMetal pewter = metalStorage[MetalType.Pewter];
        float strengthMultiplier = 1f + (pewter.currentStored / pewter.maxStorage);
        
        Debug.Log($"Feruchemy Pewter: {strengthMultiplier}x strength");
    }
    
    void ApplyChromiumEffect(float amount)
    {
        FeruchemyMetal chromium = metalStorage[MetalType.Chromium];
        float recoveryMultiplier = 1f + (chromium.currentStored / chromium.maxStorage);
        
        Debug.Log($"Feruchemy Chromium: {recoveryMultiplier}x recovery");
    }
    
    void ApplyNicrosilEffect(float amount)
    {
        if (!canCompound) return;
        
        FeruchemyMetal nicrosil = metalStorage[MetalType.Nicrosil];
        float compoundingPower = nicrosil.currentStored / nicrosil.maxStorage;
        
        Debug.Log($"Nicrosil Compounding active: {compoundingPower}x Investiture");
    }
    
    void ProcessCompounding()
    {
        if (!canCompound || !isCompounding) return;
        
        if (Input.GetKey(KeyCode.J))
        {
            CompoundFeruchemy();
        }
    }
    
    public void CompoundFeruchemy()
    {
        if (!canCompound) return;
        
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals == null) return;
        
        foreach (var kvp in metalStorage)
        {
            if (kvp.Value.currentStored > 0)
            {
                float compounded = kvp.Value.currentStored * 10f;
                kvp.Value.currentStored = compounded;
                Debug.Log($"Compounded {kvp.Key}: {compounded}x");
            }
        }
        
        metals.PurgeAll();
    }
    
    public FeruchemyMetal GetMetalStorage(MetalType type)
    {
        return metalStorage.TryGetValue(type, out var metal) ? metal : null;
    }
    
    public float GetStoragePercentage(MetalType type)
    {
        if (metalStorage.TryGetValue(type, out var metal))
        {
            return metal.currentStored / metal.maxStorage;
        }
        return 0f;
    }
}

[System.Serializable]
public class FeruchemyMetal
{
    public MetalType type;
    public float currentStored;
    public float maxStorage = 100f;
    public float storageRate = 1f;
    public float tapRate = 2f;
    public float[] tapMultipliers = new float[5];
    
    public FeruchemyMetal(MetalType t, float stored, float max)
    {
        type = t;
        currentStored = stored;
        maxStorage = max > 0 ? max : 100f;
        
        tapMultipliers = new float[] { 1f, 2f, 5f, 10f, 20f };
    }
    
    public float GetTappedValue(float tapSpeed)
    {
        int index = Mathf.Clamp(Mathf.FloorToInt(tapSpeed), 0, tapMultipliers.Length - 1);
        return currentStored * tapMultipliers[index];
    }
    
    public bool IsEmpty() => currentStored <= 0;
    public bool IsFull() => currentStored >= maxStorage;
}
