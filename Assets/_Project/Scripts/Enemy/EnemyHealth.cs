using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    public float currentHealth = 50f;

    [Header("Death")]
    public bool dropLoot = true;
    public GameObject lootPrefab;
    [Range(0f, 1f)] public float lootDropChance = 0.5f;
    public GameObject deathEffect;
    
    public bool isDead { get; set; } = false;

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // If EnemyAI is present it owns health — route damage through it so
        // it can react (transition to Chase, trigger death sequence, grant XP).
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) { ai.TakeDamage(amount); return; }

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }
    
    void Die()
    {
        isDead = true;
        currentHealth = 0;
        
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (dropLoot && lootPrefab != null)
        {
            if (Random.value <= lootDropChance)
            {
                Instantiate(lootPrefab, transform.position + Vector3.up, Quaternion.identity);
            }
        }
        
        Destroy(gameObject, 0.1f);
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
