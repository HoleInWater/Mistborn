// NOTE: Line 42 contains Debug.Log which should be removed for production
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    // NOTE: Consider adding [Range(0.1f, 10f)] attribute for comboWindow
    public float comboWindow = 2f;
    // NOTE: Consider adding [Range(1, 100)] attribute for maxComboCount
    public int maxComboCount = 10;
    
    [Header("Combo Rewards")]
    // NOTE: Consider adding [Range(0.01f, 1f)] attribute for damageMultiplierPerHit
    public float damageMultiplierPerHit = 0.1f;
    // NOTE: Consider adding [Range(0f, 1f)] attribute for metalCostReduction
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
