// NOTE: Line 33 contains Debug.Log which should be removed for production
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
    // NOTE: Consider adding [Tooltip("Layer mask for enemies")] attribute for better inspector documentation
    public LayerMask enemyLayer;
    
    private float lastAttackTime = 0f;
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) Attack();
    }
    
    void Attack() {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        
        if (comboSystem != null) comboSystem.RegisterHit();
        float damage = baseDamage * (comboSystem != null ? comboSystem.DamageMultiplier : 1f);
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider enemy in enemies) {
            // Updated to find HealthBarTransitions
            HealthBarTransitions health = enemy.GetComponent<HealthBarTransitions>();
            if (health != null) {
                health.TakeDamage(damage);
                Debug.Log($"Hit {enemy.name} for {damage} damage!");
            }
        }
    }
}
