using UnityEngine;

public class Stamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 15f;
    public float staminaDrainRate = 20f;
    public float regenDelay = 1f;
    public float regenTickRate = 0.1f;
    
    public float dashStaminaCost = 25f;
    public float wallRunStaminaCost = 5f;
    public float sprintStaminaCost = 3f;
    
    public System.Action<float, float> OnStaminaChanged;
    public System.Action OnStaminaDepleted;
    public System.Action OnStaminaFull;
    
    private float timeSinceUse = 0f;
    private float lastRegenTick = 0f;
    private bool wasExhausted = false;
    
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
