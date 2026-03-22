using UnityEngine;

/// <summary>
/// Projectile launched by Allomantic push/pull.
/// Usage: Attach to coin or metal projectile prefab.
/// </summary>
public class AllomanticProjectile : MonoBehaviour
{
    // SETTINGS
    public float speed = 50f;                // Projectile speed
    public float damage = 10f;                // Damage on hit
    public float lifetime = 5f;               // Auto-destroy time
    public float metalCost = 5f;              // Metal used
    
    // TRAJECTORY
    public Vector3 launchDirection;           // Direction to fly
    public bool isPushed = false;             // True = pushed, False = pulled
    
    // EVENTS
    public System.Action OnHit;
    public System.Action OnExpire;
    
    private Rigidbody rb;
    private bool hasHit = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        // Launch projectile
        rb.velocity = launchDirection.normalized * speed;
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Hit something - deal damage
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            hasHit = true;
            OnHit?.Invoke();
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (!hasHit)
        {
            OnExpire?.Invoke();
        }
    }
}
