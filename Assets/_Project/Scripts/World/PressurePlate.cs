using UnityEngine;

/// <summary>
/// Pressure plate that activates linked objects.
/// Usage: Attach to plate prefab.
/// </summary>
public class PressurePlate : MonoBehaviour
{
    // SETTINGS
    public float requiredWeight = 0f;        // 0 = any player
    public bool oneTimeActivation = false;
    
    // TARGETS - Objects to activate
    public GameObject[] targetObjects;
    public string[] targetTags;
    
    // VISUALS
    public Material activatedMaterial;
    private Material originalMaterial;
    private Renderer plateRenderer;
    
    // STATE
    private bool isActivated = false;
    
    // EVENTS
    public System.Action OnActivated;
    public System.Action OnDeactivated;
    
    void Start()
    {
        plateRenderer = GetComponent<Renderer>();
        if (plateRenderer != null)
        {
            originalMaterial = plateRenderer.material;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isActivated && oneTimeActivation) return;
        
        if (other.CompareTag("Player"))
        {
            Activate();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (oneTimeActivation) return;
        
        if (other.CompareTag("Player"))
        {
            Deactivate();
        }
    }
    
    void Activate()
    {
        isActivated = true;
        
        // Visual feedback
        if (plateRenderer != null && activatedMaterial != null)
        {
            plateRenderer.material = activatedMaterial;
        }
        
        // Activate targets
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                
                // Try to call OnActivate if method exists
                obj.SendMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
            }
        }
        
        // Activate by tag
        foreach (string tag in targetTags)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in taggedObjects)
            {
                obj.SetActive(true);
            }
        }
        
        OnActivated?.Invoke();
    }
    
    void Deactivate()
    {
        isActivated = false;
        
        if (plateRenderer != null)
        {
            plateRenderer.material = originalMaterial;
        }
        
        // Deactivate targets
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        OnDeactivated?.Invoke();
    }
}
