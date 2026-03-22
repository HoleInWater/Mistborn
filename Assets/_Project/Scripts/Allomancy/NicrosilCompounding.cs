using UnityEngine;

public class NicrosilCompounding : MonoBehaviour
{
    [Header("Nicrosil Settings")]
    public float nicrosilReserve = 100f;
    public float burnRate = 4f;
    public float amplificationMultiplier = 10f;
    
    [Header("Target Metal")]
    public MetalType targetMetal = MetalType.Steel;
    
    [Header("State")]
    public bool isBurning = false;
    public bool isAmplifying = false;
    
    [Header("Visual")]
    public Color amplifyingColor = Color.magenta;
    public GameObject amplifyEffect;
    
    private ParticleSystem effectParticles;
    
    void Start()
    {
        if (amplifyEffect != null)
        {
            effectParticles = amplifyEffect.GetComponent<ParticleSystem>();
            amplifyEffect.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleAmplification();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) targetMetal = MetalType.Steel;
        if (Input.GetKeyDown(KeyCode.Alpha2)) targetMetal = MetalType.Iron;
        if (Input.GetKeyDown(KeyCode.Alpha3)) targetMetal = MetalType.Tin;
        if (Input.GetKeyDown(KeyCode.Alpha4)) targetMetal = MetalType.Pewter;
        
        if (isBurning)
        {
            ProcessCompounding();
        }
    }
    
    void ToggleAmplification()
    {
        if (isBurning)
        {
            StopCompounding();
        }
        else
        {
            StartCompounding();
        }
    }
    
    void StartCompounding()
    {
        if (nicrosilReserve > 0)
        {
            isBurning = true;
            isAmplifying = true;
            
            if (amplifyEffect != null)
            {
                amplifyEffect.SetActive(true);
            }
            
            Debug.Log($"Nicrosil Compounding active - Amplifying {targetMetal}!");
        }
    }
    
    void StopCompounding()
    {
        isBurning = false;
        isAmplifying = false;
        
        if (amplifyEffect != null)
        {
            amplifyEffect.SetActive(false);
        }
        
        Debug.Log("Nicrosil compounding stopped");
    }
    
    void ProcessCompounding()
    {
        nicrosilReserve -= burnRate * Time.deltaTime;
        
        if (nicrosilReserve <= 0)
        {
            StopCompounding();
            nicrosilReserve = 0f;
            return;
        }
        
        AmplifyAllomancy();
    }
    
    void AmplifyAllomancy()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals == null) return;
        
        float currentReserve = metals.GetReserve(targetMetal);
        
        if (currentReserve > 0)
        {
            float drainAmount = burnRate * Time.deltaTime;
            float amplifiedAmount = drainAmount * amplificationMultiplier;
            
            metals.Drain(targetMetal, drainAmount);
            
            metals.Refill(targetMetal, amplifiedAmount - drainAmount);
        }
    }
    
    public void RefillNicrosil(float amount)
    {
        nicrosilReserve = Mathf.Min(100f, nicrosilReserve + amount);
    }
    
    public void SetTargetMetal(MetalType metal)
    {
        targetMetal = metal;
        Debug.Log($"Nicrosil target set to: {metal}");
    }
    
    public float GetAmplificationMultiplier()
    {
        return amplificationMultiplier * (nicrosilReserve / 100f);
    }
    
    public bool IsAmplifying()
    {
        return isAmplifying;
    }
}
