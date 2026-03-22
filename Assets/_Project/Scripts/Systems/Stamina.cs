using UnityEngine;

public class Stamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 10f;
    public float staminaDrainRate = 20f;
    public float dashStaminaCost = 25f;
    public float wallRunStaminaCost = 5f;
    
    public System.Action<float, float> OnStaminaChanged;
    public System.Action OnStaminaDepleted;
    
    public float NormalizedStamina => currentStamina / maxStamina;
    public bool HasStamina(float cost) => currentStamina >= cost;
    
    void Update()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            if (currentStamina <= 0)
            {
                OnStaminaDepleted?.Invoke();
            }
            return true;
        }
        return false;
    }
    
    public void RefillStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    public void RestoreFullStamina()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
