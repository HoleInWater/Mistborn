// NOTE: Line 42 contains Debug.Log which should be removed for production
// NOTE: Consider adding [RequireComponent(typeof(PlayerCombat))] attribute for dependency
using UnityEngine;

[PlayerComponent("Combat", order: 20)]
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
    
    [Header("Rage Settings")]
    public float ragePerHit = AllomancyConstants.RagePerHit;
    public float rageDecayRate = AllomancyConstants.RageDecayRate;

    
    private int currentCombo = 0;
    private float lastHitTime = 0f;
    private float currentDamageMultiplier = 1f;
    private float rageMeter = 0f;
    private bool isRaging = false;
    
    public int CurrentCombo => currentCombo;
    public float DamageMultiplier => currentDamageMultiplier;
    public float Rage => rageMeter;
    
    void Update()
    {
        if (Time.time - lastHitTime > comboWindow)
        {
            if (currentCombo > 0) ResetCombo();
            
            // Decay rage when out of combat
            if (rageMeter > 0)
            {
                rageMeter = Mathf.Max(0, rageMeter - rageDecayRate * Time.deltaTime);
                if (isRaging && rageMeter < 0.2f) EndRage();
            }
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

        // Achievement: 10-hit combo
        if (currentCombo >= 10)
            AchievementSystem.Instance?.TryUnlock("combo_10");
        
        // Build rage
        if (!isRaging)
        {
            rageMeter = Mathf.Min(1f, rageMeter + ragePerHit);
            if (rageMeter >= 1f) StartRage();
        }
        
    }

    private void StartRage()
    {
        isRaging = true;
        // Lore: Rage causes an automatic Flare boost
        if (FlareManager.Instance != null)
        {
            FlareManager.Instance.SetIntensity(10);
            // We don't force IsBurning = true here to respect player control, 
            // but it's an option if we want "Auto-Burn".
        }
    }

    private void EndRage()
    {
        isRaging = false;
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
