// NOTE: Lines 22, 35, 47 contain Debug.Log which should be removed for production
// NOTE: Consider adding [RequireComponent(typeof(Collider))] attribute for trigger detection
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    // NOTE: Consider adding [Range(1f, 1000f)] attribute for maxHealth
    public float maxHealth = 50f;
    // NOTE: Consider adding [SerializeField] for currentHealth (private field)
    public float currentHealth = 50f;
    
    [Header("Death")]
    public bool dropLoot = true;
    public GameObject lootPrefab;
    // NOTE: Consider adding [Range(0f, 1f)] attribute for lootDropChance
    public float lootDropChance = 0.5f;
    public GameObject deathEffect;
    
    public bool isDead { get; private set; } = false;
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
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
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
