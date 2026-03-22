using UnityEngine;

/// <summary>
/// Collectible health potion.
/// Usage: Attach to pickup prefab.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    // SETTINGS
    public float healAmount = 25f;           // HP restored
    
    // EFFECTS
    public GameObject pickupEffect;           // VFX on pickup
    public AudioClip pickupSound;             // Sound on pickup
    
    // EVENTS
    public System.Action<float> OnPickedUp;  // (amount healed)
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other.gameObject);
        }
    }
    
    void Pickup(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(healAmount);
            OnPickedUp?.Invoke(healAmount);
        }
        
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        Destroy(gameObject);
    }
}
