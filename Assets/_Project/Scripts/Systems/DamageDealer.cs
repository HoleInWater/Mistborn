/// <summary>
/// Handles dealing damage to entities with Health component.
/// Usage: DamageDealer damage = GetComponent<DamageDealer>();
/// 
/// METHODS:
///   damage.DealDamage(target, amount);
///   damage.DealDamageAtPosition(position, radius, damage);
/// </summary>
public class DamageDealer : MonoBehaviour
{
    // SETTINGS
    public float baseDamage = 10f;             // Base damage amount
    public float damageRadius = 0f;            // Area damage radius (0 = single target)
    public bool destroyOnHit = false;           // Destroy this object on damage deal
    public float destroyDelay = 0f;            // Delay before destruction
    
    // DAMAGE TYPES
    public enum DamageType
    {
        Physical,
        Allomantic,
        Burn,
        Impact
    }
    
    public DamageType damageType = DamageType.Physical;
    
    // EVENTS
    public System.Action<GameObject, float> OnDamageDealt; // (target, amount)
    
    // PUBLIC API
    
    /// <summary>Deal damage to a single target with Health component</summary>
    public void DealDamage(GameObject target, float damageAmount)
    {
        if (target == null) return;
        
        Health health = target.GetComponent<Health>();
        
        if (health != null)
        {
            health.TakeDamage(damageAmount);
            OnDamageDealt?.Invoke(target, damageAmount);
            Debug.Log($"Dealt {damageAmount} damage to {target.name}");
            
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
    
    /// <summary>Deal damage in an area (requires damageRadius > 0)</summary>
    public void DealAreaDamage(Vector3 center, float radius, float damageAmount)
    {
        if (radius <= 0) return;
        
        Collider[] hits = Physics.OverlapSphere(center, radius);
        
        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponent<Health>();
            
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                OnDamageDealt?.Invoke(hit.gameObject, damageAmount);
            }
        }
        
        if (destroyOnHit)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>Set custom damage value at runtime</summary>
    public void SetDamage(float damage)
    {
        baseDamage = damage;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (damageRadius > 0) return;
        DealDamage(other.gameObject, baseDamage);
    }
}
