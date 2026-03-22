// NOTE: Line 46 contains Debug.Log which should be removed for production
using UnityEngine;

public class MetalPickup : MonoBehaviour
{
    [Header("Metal Type")]
    public AllomancySkill.MetalType metalType;
    
    [Header("Pickup Settings")]
    // NOTE: Consider adding [Range(1f, 100f)] attribute for metalAmount
    public float metalAmount = 25f;
    // NOTE: Consider adding [Range(1f, 300f)] attribute for respawnTime
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
        if (allomancer == null)
        {
            MetalReserveManager manager = player.GetComponent<MetalReserveManager>();
            if (manager != null)
            {
                manager.Refill(metalType, metalAmount);
            }
        }
        else
        {
            allomancer.RefillMetal(metalType, metalAmount);
        }
        
        Debug.Log($"Collected {metalType} (+{metalAmount})");
        
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
