// NOTE: Line 33 contains Debug.Log which should be removed for production
// NOTE: Consider adding [RequireComponent(typeof(Rigidbody))] attribute for physics
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    // NOTE: Consider adding [Range(0.1f, 10f)] attribute for attackRange
    public float attackRange = 2f;
    // NOTE: Consider adding [Range(0.1f, 5f)] attribute for attackCooldown
    public float attackCooldown = 0.5f;
    // NOTE: Consider adding [Range(1f, 100f)] attribute for baseDamage
    public float baseDamage = 10f;
    
    [Header("References")]
    public ComboSystem comboSystem;
    public LockOnSystem lockOnSystem;
    // NOTE: Consider adding [Tooltip("Layer mask for enemies")] attribute for better inspector documentation
    public LayerMask enemyLayer;
    
    private float lastAttackTime = 0f;
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) Attack();
    }
    
    public void Attack() {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // Orient toward lock-on target if active
        if (lockOnSystem != null && lockOnSystem.CurrentTarget != null)
        {
            Vector3 dir = (lockOnSystem.CurrentTarget.position - transform.position).normalized;
            dir.y = 0;
            transform.forward = dir;
        }
        
        if (comboSystem != null) comboSystem.RegisterHit();

        float damage = baseDamage * (comboSystem != null ? comboSystem.DamageMultiplier : 1f);
        
        // Position and direction for attack
        Vector3 attackPos = transform.position + transform.forward * 1f;
        Collider[] enemies = Physics.OverlapSphere(attackPos, attackRange, enemyLayer);
        
        foreach (Collider enemy in enemies) {
            // Standardized to IDamageable
            IDamageable damageable = enemy.GetComponentInParent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(damage);
                Debug.Log($"Hit {enemy.name} for {damage} damage!");
                
                // Visual feedback could be added here
            }
        }
    }

}
