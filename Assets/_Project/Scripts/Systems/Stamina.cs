using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    
    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 5f;
    public float recoveryDelay = 2f;
    private float timeSinceLastUse = 0f;
    
    [Header("Exhaustion")]
    public float exhaustionThreshold = 0f;
    public float exhaustionRecoveryMultiplier = 0.25f;
    
    [Header("Events")]
    public UnityEvent<float, float> OnStaminaChanged;
    
    public float NormalizedStamina => currentStamina / maxStamina;
    public bool IsExhausted => currentStamina <= exhaustionThreshold;
    
    void Start()
    {
        currentStamina = maxStamina;
    }
    
    void Update()
    {
        if (currentStamina < maxStamina)
        {
            timeSinceLastUse += Time.deltaTime;
            
            if (timeSinceLastUse >= recoveryDelay)
            {
                float recoveryRate = passiveRecoveryRate;
                
                if (IsExhausted)
                {
                    recoveryRate *= exhaustionRecoveryMultiplier;
                }
                
                currentStamina = Mathf.Min(maxStamina, currentStamina + recoveryRate * Time.deltaTime);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            timeSinceLastUse = 0f;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }
    
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    public void SetMaxStamina(float newMax)
    {
        float ratio = currentStamina / maxStamina;
        maxStamina = newMax;
        currentStamina = ratio * maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    public void ResetRecoveryTimer()
    {
        timeSinceLastUse = 0f;
    }
}
