using UnityEngine;

public class HarmoniumController : MonoBehaviour
{
    [Header("Harmonium ( Ettmetal ) Settings")]
    public float harmoniumReserve = 100f;
    public float burnRate = 3f;
    public float explosionThreshold = 90f;
    public float stabilityTemperature = 25f;
    
    [Header("Current State")]
    public bool isUnstable = false;
    public bool isExploding = false;
    public float instabilityLevel = 0f;
    
    [Header("Steam Generation")]
    public float steamGenerationRate = 10f;
    public GameObject steamEffect;
    
    [Header("Explosion Settings")]
    public float explosionRadius = 15f;
    public float explosionForce = 5000f;
    public float explosionDamage = 100f;
    
    private ParticleSystem steamParticles;
    
    void Start()
    {
        if (steamEffect != null)
        {
            steamParticles = steamEffect.GetComponent<ParticleSystem>();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            IgniteHarmonium();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ExtinguishHarmonium();
        }
        
        ProcessInstability();
    }
    
    void IgniteHarmonium()
    {
        if (harmoniumReserve > 0 && !isExploding)
        {
            isUnstable = true;
            Debug.Log("Harmonium ignited!");
            
            if (steamEffect != null)
            {
                steamEffect.SetActive(true);
            }
        }
    }
    
    void ExtinguishHarmonium()
    {
        isUnstable = false;
        instabilityLevel = 0f;
        isExploding = false;
        
        if (steamEffect != null)
        {
            steamEffect.SetActive(false);
        }
        
        Debug.Log("Harmonium extinguished!");
    }
    
    void ProcessInstability()
    {
        if (!isUnstable || isExploding) return;
        
        harmoniumReserve -= burnRate * Time.deltaTime;
        
        float environmentTemperature = GetEnvironmentTemperature();
        
        if (environmentTemperature > stabilityTemperature)
        {
            instabilityLevel += (environmentTemperature - stabilityTemperature) * Time.deltaTime * 0.1f;
        }
        else
        {
            instabilityLevel -= Time.deltaTime * 0.5f;
        }
        
        instabilityLevel = Mathf.Clamp01(instabilityLevel);
        
        if (instabilityLevel >= explosionThreshold / 100f)
        {
            TriggerExplosion();
        }
    }
    
    float GetEnvironmentTemperature()
    {
        return stabilityTemperature;
    }
    
    void TriggerExplosion()
    {
        isExploding = true;
        Debug.Log("HARMONIUM EXPLOSION!");
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
            
            Health health = nearby.GetComponent<Health>();
            if (health != null)
            {
                float distance = Vector3.Distance(transform.position, nearby.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                health.TakeDamage(explosionDamage * damageFalloff);
            }
        }
        
        harmoniumReserve = 0f;
        isUnstable = false;
        
        if (steamEffect != null)
        {
            steamEffect.SetActive(false);
        }
    }
    
    public void AddHarmonium(float amount)
    {
        harmoniumReserve = Mathf.Min(100f, harmoniumReserve + amount);
        
        if (harmoniumReserve > explosionThreshold)
        {
            Debug.LogWarning("Harmonium levels critical!");
        }
    }
    
    public float GetInstabilityPercentage()
    {
        return instabilityLevel * 100f;
    }
    
    public bool IsDangerous()
    {
        return instabilityLevel > 0.5f;
    }
    
    public bool IsExploding()
    {
        return isExploding;
    }
    
    void OnDestroy()
    {
        if (steamEffect != null)
        {
            Destroy(steamEffect);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
