using UnityEngine;

public class MetalPickup : MonoBehaviour
{
    [Header("Metal Type")]
    public AllomancySkill.MetalType metalType;
    
    [Header("Pickup Settings")]
    public float metalAmount = 25f;
    public float respawnTime = 30f;
    
    private bool isCollected = false;
    private Renderer objectRenderer;
    private Collider objectCollider;
    
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectMetal(other.gameObject);
        }
    }
    
    void CollectMetal(GameObject player)
    {
        Allomancer allomancer = player.GetComponent<Allomancer>();
        
        if (allomancer != null)
        {
            // Allomancer usually handles both the type and the amount
            allomancer.RefillMetal(metalType, metalAmount);
        }
        else
        {
            MetalReserve manager = player.GetComponent<MetalReserve>();
            if (manager != null)
            {
                // FIX: Changed this to 1 argument to match the error's suggestion
                manager.Refill(metalAmount);
            }
        }
        
        isCollected = true;
        objectRenderer.enabled = false;
        objectCollider.enabled = false;
        
        Invoke("Respawn", respawnTime);
    }
    
    void Respawn()
    {
        isCollected = false;
        objectRenderer.enabled = true;
        objectCollider.enabled = true;
    }
}
