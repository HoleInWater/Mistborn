/// <summary>
/// Core stamina system for player abilities.
/// Usage: Stamina stamina = GetComponent<Stamina>();
/// 
/// EVENTS:
///   stamina.OnStaminaChanged += (current, max) => { };
///   stamina.OnStaminaDepleted += () => { };
///   stamina.OnStaminaFull += () => { };
/// 
/// METHODS:
///   stamina.UseStamina(25f);     // Returns true if successful
///   stamina.HasStamina(25f);      // Check if can afford cost
///   stamina.RefillStamina(50f);
///   stamina.RestoreFullStamina();
/// </summary>
public class Stamina : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float maxStamina = 100f;           // Maximum stamina points
    public float currentStamina = 100f;       // Current stamina points
    public float staminaRegenRate = 15f;       // Stamina per tick when regenerating
    public float regenDelay = 1f;             // Seconds before regen starts
    public float regenTickRate = 0.1f;        // How often regen ticks
    
    // ABILITY COSTS - Adjust in Inspector
    public float dashStaminaCost = 25f;       // Cost for dash ability
    public float wallRunStaminaCost = 5f;    // Cost per second for wall run
    public float sprintStaminaCost = 3f;     // Cost per second for sprint
    
    // EVENTS - Subscribe for callbacks
    public System.Action<float, float> OnStaminaChanged; // (current, max)
    public System.Action OnStaminaDepleted;  // Fired when stamina reaches 0
    public System.Action OnStaminaFull;      // Fired when stamina fully restored
    
    // INTERNAL STATE
    private float timeSinceUse = 0f;         // Time since last stamina use
    private float lastRegenTick = 0f;        // When last regen occurred
    private bool wasExhausted = false;     // Was stamina depleted
    
    // PUBLIC API
    public float NormalizedStamina => currentStamina / maxStamina;
    public bool HasStamina(float cost) => currentStamina >= cost;
    
    void Update()
    {
        if (currentStamina < maxStamina)
        {
            timeSinceUse += Time.deltaTime;
            
            if (timeSinceUse >= regenDelay)
            {
                if (Time.time - lastRegenTick >= regenTickRate)
                {
                    lastRegenTick = Time.time;
                    float prevStamina = currentStamina;
                    currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * regenTickRate);
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                    
                    if (currentStamina >= maxStamina && prevStamina < maxStamina)
                    {
                        OnStaminaFull?.Invoke();
                    }
                }
            }
        }
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            timeSinceUse = 0f;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                wasExhausted = true;
                OnStaminaDepleted?.Invoke();
            }
            return true;
        }
        return false;
    }
    
    public void RefillStamina(float amount)
    {
        float prevStamina = currentStamina;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        timeSinceUse = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        
        if (currentStamina >= maxStamina && prevStamina < maxStamina)
        {
            OnStaminaFull?.Invoke();
        }
    }
    
    public void RestoreFullStamina()
    {
        currentStamina = maxStamina;
        timeSinceUse = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    public void ResetRegenTimer()
    {
        timeSinceUse = 0f;
    }
}
