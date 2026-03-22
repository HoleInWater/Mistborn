using UnityEngine;

public class LerasiumController : MonoBehaviour
{
    [Header("Lerasium Settings")]
    public float lerasiumReserve = 100f;
    public float burnRate = 2f;
    public bool canUseAllomancy = true;
    public bool canUseFeruchemy = true;
    public bool canUseHemalurgy = true;
    
    [Header("Transformation State")]
    public bool isTransformed = false;
    public float transformationProgress = 0f;
    
    [Header("Mistborn Enhancement")]
    public float allomanticStrengthMultiplier = 2f;
    public float feruchemicStorageMultiplier = 2f;
    
    [Header("Passive Effects")]
    public float enhancedHealingRate = 2f;
    public float enhancedSensesRange = 2f;
    public float enhancedStrength = 1.5f;
    public float enhancedSpeed = 1.5f;
    public float enhancedDurability = 1.5f;
    
    private Health health;
    private Stamina stamina;
    
    void Start()
    {
        health = GetComponent<Health>();
        stamina = GetComponent<Stamina>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && lerasiumReserve > 0)
        {
            StartTransformation();
        }
        
        if (isTransformed)
        {
            ProcessTransformation();
        }
    }
    
    void ProcessTransformation()
    {
        if (transformationProgress < 1f)
        {
            transformationProgress += Time.deltaTime * 0.1f;
            
            ApplyLerasiumEffects();
            
            if (transformationProgress >= 1f)
            {
                CompleteTransformation();
            }
        }
    }
    
    void ApplyLerasiumEffects()
    {
        float effectStrength = transformationProgress;
        
        if (health != null && effectStrength > 0.5f)
        {
            health.Heal(enhancedHealingRate * Time.deltaTime * effectStrength);
        }
    }
    
    void StartTransformation()
    {
        if (!isTransformed && lerasiumReserve > 0)
        {
            isTransformed = true;
            transformationProgress = 0f;
            Debug.Log("Lerasium transformation beginning...");
        }
    }
    
    void CompleteTransformation()
    {
        isTransformed = true;
        canUseAllomancy = true;
        canUseFeruchemy = true;
        canUseHemalurgy = true;
        
        Debug.Log("Lerasium transformation complete! You are now a full Mistborn!");
    }
    
    public void GrantAllomancy()
    {
        AllomancyInputController allomancy = GetComponent<AllomancyInputController>();
        if (allomancy != null)
        {
            Debug.Log("Allomancy granted!");
        }
    }
    
    public void GrantFeruchemy()
    {
        FeruchemySystem feruchemy = GetComponent<FeruchemySystem>();
        if (feruchemy != null)
        {
            feruchemy.isFeruchemist = true;
            Debug.Log("Feruchemy granted!");
        }
    }
    
    public void DrainLerasium(float amount)
    {
        lerasiumReserve = Mathf.Max(0f, lerasiumReserve - amount);
        
        if (lerasiumReserve <= 0)
        {
            Debug.LogWarning("Lerasium depleted!");
        }
    }
    
    public bool HasFullTransformation()
    {
        return isTransformed && transformationProgress >= 1f;
    }
    
    public float GetTransformationProgress()
    {
        return transformationProgress;
    }
    
    public float GetAllomanticMultiplier()
    {
        if (!isTransformed) return 1f;
        return allomanticStrengthMultiplier * transformationProgress;
    }
    
    public float GetFeruchemicMultiplier()
    {
        if (!isTransformed) return 1f;
        return feruchemicStorageMultiplier * transformationProgress;
    }
}
