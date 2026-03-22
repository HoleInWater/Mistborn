using UnityEngine;

public class ChromiumErasing : MonoBehaviour
{
    [Header("Chromium Settings")]
    public float chromiumReserve = 100f;
    public float burnRate = 3f;
    public float eraseRange = 30f;
    public float eraseStrength = 1f;
    
    [Header("Effect Settings")]
    public Color erasingColor = Color.cyan;
    public GameObject eraseEffect;
    
    [Header("State")]
    public bool isBurning = false;
    public bool isActive = false;
    
    private ParticleSystem effectParticles;
    
    void Start()
    {
        if (eraseEffect != null)
        {
            effectParticles = eraseEffect.GetComponent<ParticleSystem>();
            eraseEffect.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleErasing();
        }
        
        if (isBurning)
        {
            ProcessErasing();
        }
    }
    
    void ToggleErasing()
    {
        if (isBurning)
        {
            StopErasing();
        }
        else
        {
            StartErasing();
        }
    }
    
    void StartErasing()
    {
        if (chromiumReserve > 0)
        {
            isBurning = true;
            isActive = true;
            
            if (eraseEffect != null)
            {
                eraseEffect.SetActive(true);
            }
            
            Debug.Log("Burning Chromium - Erasing Allomantic powers!");
        }
    }
    
    void StopErasing()
    {
        isBurning = false;
        isActive = false;
        
        if (eraseEffect != null)
        {
            eraseEffect.SetActive(false);
        }
        
        Debug.Log("Stopped Chromium erasing");
    }
    
    void ProcessErasing()
    {
        chromiumReserve -= burnRate * Time.deltaTime;
        
        if (chromiumReserve <= 0)
        {
            StopErasing();
            chromiumReserve = 0f;
            return;
        }
        
        AffectNearbyAllomancers();
    }
    
    void AffectNearbyAllomancers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, eraseRange);
        
        foreach (Collider col in colliders)
        {
            Allomancer allomancer = col.GetComponent<Allomancer>();
            if (allomancer != null && allomancer != GetComponent<Allomancer>())
            {
                allomancer.DrainMetal(MetalType.Steel, eraseStrength * Time.deltaTime);
                allomancer.DrainMetal(MetalType.Iron, eraseStrength * Time.deltaTime);
                allomancer.DrainMetal(MetalType.Tin, eraseStrength * Time.deltaTime);
                allomancer.DrainMetal(MetalType.Pewter, eraseStrength * Time.deltaTime);
            }
        }
    }
    
    public void RefillChromium(float amount)
    {
        chromiumReserve = Mathf.Min(100f, chromiumReserve + amount);
    }
    
    public float GetEraseStrength()
    {
        return eraseStrength * (chromiumReserve / 100f);
    }
    
    public bool IsErasing()
    {
        return isActive;
    }
}
