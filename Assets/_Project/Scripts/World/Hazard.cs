/// <summary>
/// Kill player on trigger enter.
/// Usage: Attach to hazard objects (spikes, lava, etc.)
/// </summary>
public class Hazard : MonoBehaviour
{
    // SETTINGS
    public float damage = 100f;              // Instant kill damage
    public bool destroyOnContact = false;     // Destroy this object after hit
    public float destroyDelay = 0f;
    
    // EFFECTS
    public GameObject deathEffect;
    public AudioClip deathSound;
    
    // EVENTS
    public System.Action OnPlayerHit;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                OnPlayerHit?.Invoke();
                
                if (deathEffect != null)
                {
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                }
                
                if (deathSound != null)
                {
                    AudioSource.PlayClipAtPoint(deathSound, transform.position);
                }
                
                if (destroyOnContact)
                {
                    Destroy(gameObject, destroyDelay);
                }
            }
        }
    }
}
