using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public int baseDamage = 10;
    
    [Header("References")]
    public ComboSystem comboSystem;
    public LayerMask enemyLayer;
    
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Attack();
        }
    }
    
    void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        
        lastAttackTime = Time.time;
        isAttacking = true;
        
        if (comboSystem != null)
        {
            comboSystem.RegisterHit();
        }
        
        float damage = baseDamage * (comboSystem != null ? comboSystem.DamageMultiplier : 1f);
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Hit {enemy.name} for {damage} damage!");
            }
        }
        
        isAttacking = false;
    }
}
