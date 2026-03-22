using UnityEngine;

/// <summary>
/// Copper Cloud - Hide Allomantic pulses from Bronze detectors.
/// Usage: CopperCloud cloud = GetComponent<CopperCloud>();
/// </summary>
public class CopperCloud : MonoBehaviour
{
    // SETTINGS
    public float cloudRadius = 15f;           // Size of cloud
    public float metalCostPerSecond = 4f;    // Drain rate
    
    // VISUALS
    public Color cloudColor = new Color(0.3f, 0.2f, 0.5f, 0.2f);
    
    // STATE
    private bool isActive = false;
    private GameObject cloudEffect;
    
    // EVENTS
    public System.Action OnCloudStart;
    public System.Action OnCloudEnd;
    
    // PUBLIC API
    public bool IsActive => isActive;
    
    void Update()
    {
        // Press C to toggle
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isActive)
                StopCloud();
            else
                StartCloud();
        }
        
        if (isActive)
        {
            DrainMetal();
            MaintainCloud();
        }
    }
    
    void StartCloud()
    {
        isActive = true;
        CreateCloud();
        Debug.Log("Copper Cloud Active!");
        OnCloudStart?.Invoke();
    }
    
    void StopCloud()
    {
        isActive = false;
        DestroyCloud();
        Debug.Log("Copper Cloud Ended");
        OnCloudEnd?.Invoke();
    }
    
    void CreateCloud()
    {
        if (cloudEffect != null)
            Destroy(cloudEffect);
        
        cloudEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cloudEffect.name = "CopperCloud";
        cloudEffect.transform.localScale = Vector3.one * cloudRadius * 2;
        cloudEffect.transform.parent = transform;
        
        Renderer renderer = cloudEffect.GetComponent<Renderer>();
        renderer.material.color = cloudColor;
        
        Destroy(cloudEffect.GetComponent<Collider>());
    }
    
    void MaintainCloud()
    {
        if (cloudEffect != null)
        {
            cloudEffect.transform.position = transform.position;
        }
    }
    
    void DestroyCloud()
    {
        if (cloudEffect != null)
        {
            Destroy(cloudEffect);
            cloudEffect = null;
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Copper, metalCostPerSecond * Time.deltaTime))
            {
                StopCloud();
            }
        }
    }
    
    void OnDestroy()
    {
        DestroyCloud();
    }
}
