using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healthRegenRate = 0f;
    public float regenDelay = 3f;
    public float regenTickRate = 0.5f;
    
    public bool destroyOnDeath = false;
    public float deathDelay = 0f;
    
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnRevive;
    
    private float timeSinceDamage = 0f;
    private float lastRegenTick = 0f;
    
    public float NormalizedHealth => currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;
    
    void Update()
    {
        if (IsDead) return;
        
        if (currentHealth < maxHealth)
        {
            timeSinceDamage += Time.deltaTime;
            
            if (timeSinceDamage >= regenDelay && healthRegenRate > 0)
            {
                if (Time.time - lastRegenTick >= regenTickRate)
                {
                    lastRegenTick = Time.time;
                    currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegenRate * regenTickRate);
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                }
            }
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        
        currentHealth = Mathf.Max(0, currentHealth - amount);
        timeSinceDamage = 0f;
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
            Destroy(gameObject, deathDelay);
        }
    }
    
    public void Revive(float healthAmount)
    {
        if (!IsDead) return;
        
        currentHealth = healthAmount;
        OnRevive?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
