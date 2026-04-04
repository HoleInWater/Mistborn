using UnityEngine;
using System.Collections;

public class MetalPickup : MonoBehaviour
{
    [Header("Metal Type")]
    public MetallurgySkill.MetalType metalType;
    
    [Header("Pickup Settings")]
    public float metalAmount = 25f;
    public float respawnTime = 30f;
    
    private bool isCollected = false;
    private Renderer objectRenderer;
    private Collider objectCollider;
    private Coroutine respawnCoroutine;
    
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
        Metallurgist metallurgist = player.GetComponent<Metallurgist>();
        
        if (metallurgist != null)
        {
            metallurgist.RefillMetal(metalType, metalAmount);
        }
        else
        {
            MetalReserve manager = player.GetComponent<MetalReserve>();
            if (manager != null)
            {
                // FIXED: Manually updating currentMetal to avoid the Refill() reference error
                manager.currentMetal = Mathf.Min(manager.maxMetal, manager.currentMetal + metalAmount);
            }
        }
        
        isCollected = true;
        if (objectRenderer != null) objectRenderer.enabled = false;
        if (objectCollider != null) objectCollider.enabled = false;

        if (respawnCoroutine != null) StopCoroutine(respawnCoroutine);
        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(respawnTime);
        Respawn();
    }

    void Respawn()
    {
        isCollected = false;
        respawnCoroutine = null;
        if (objectRenderer != null) objectRenderer.enabled = true;
        if (objectCollider != null) objectCollider.enabled = true;
    }
}
