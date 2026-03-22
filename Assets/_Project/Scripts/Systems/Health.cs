/// <summary>
/// Core health system for player and enemies.
/// Usage: Health health = GetComponent<Health>();
/// 
/// EVENTS:
///   health.OnHealthChanged += (current, max) => { };
///   health.OnDeath += () => { };
///   health.OnRevive += () => { };
/// 
/// METHODS:
///   health.TakeDamage(10f);
///   health.Heal(25f);
///   health.SetHealth(50f);
///   health.SetMaxHealth(150f);
///   health.Revive(50f);
/// </summary>
public class Health : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float maxHealth = 100f;             // Maximum health points
    public float currentHealth = 100f;         // Current health points
    public float healthRegenRate = 0f;         // Health per tick when regenerating
    public float regenDelay = 3f;              // Seconds before regen starts
    public float regenTickRate = 0.5f;        // How often regen ticks
    
    public bool destroyOnDeath = false;        // Destroy gameobject on death
    public float deathDelay = 0f;              // Delay before destruction
    
    // EVENTS - Subscribe for callbacks
    public System.Action<float, float> OnHealthChanged; // (current, max)
    public System.Action OnDeath;             // Fired when health reaches 0
    public System.Action OnRevive;            // Fired when revived
    
    // INTERNAL STATE
    private float timeSinceDamage = 0f;       // Time since last damage
    private float lastRegenTick = 0f;          // When last regen occurred
    
    // PUBLIC API
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
