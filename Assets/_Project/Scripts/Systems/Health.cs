using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healthRegenRate = 0f;
    
    [Header("Death Settings")]
    public bool destroyOnDeath = false;
    public float deathDelay = 0f;
    
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDeath;
    
    public float NormalizedHealth => currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;
    
    void Update()
    {
        if (healthRegenRate > 0 && currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegenRate * Time.deltaTime);
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (IsDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(0, value);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Die()
    {
        OnDeath?.Invoke();
        
        if (destroyOnDeath)
        {
            if (deathDelay > 0)
            {
                Destroy(gameObject, deathDelay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
