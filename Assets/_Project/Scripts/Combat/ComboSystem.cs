// NOTE: Line 42 contains Debug.Log which should be removed for production
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboWindow = 2f;
    public int maxComboCount = 10;
    
    [Header("Combo Rewards")]
    public float damageMultiplierPerHit = 0.1f;
    public float metalCostReduction = 0.05f;
    
    private int currentCombo = 0;
    private float lastHitTime = 0f;
    private float currentDamageMultiplier = 1f;
    
    public int CurrentCombo => currentCombo;
    public float DamageMultiplier => currentDamageMultiplier;
    
    void Update()
    {
        if (currentCombo > 0 && Time.time - lastHitTime > comboWindow)
        {
            ResetCombo();
        }
    }
    
    public void RegisterHit()
    {
        if (Time.time - lastHitTime <= comboWindow)
        {
            currentCombo = Mathf.Min(currentCombo + 1, maxComboCount);
        }
        else
        {
            currentCombo = 1;
        }
        
        lastHitTime = Time.time;
        currentDamageMultiplier = 1f + (currentCombo * damageMultiplierPerHit);
        
        Debug.Log($"Combo: {currentCombo}x - Damage: {currentDamageMultiplier:F1}x");
    }
    
    public void ResetCombo()
    {
        currentCombo = 0;
        currentDamageMultiplier = 1f;
    }
    
    public float GetMetalCostReduction()
    {
        return currentCombo * metalCostReduction;
    }
}
