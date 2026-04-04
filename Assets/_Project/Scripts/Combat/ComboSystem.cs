using UnityEngine;

[PlayerComponent("Combat", order: 20)]
public class ComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboWindow = 2f;
    public int maxComboCount = 10;

    [Header("Combo Rewards")]
    public float damageMultiplierPerHit = 0.1f;
    public float metalCostReduction = 0.05f;

    [Header("Rage Settings")]
    public float ragePerHit = MetallurgyConstants.RagePerHit;
    public float rageDecayRate = MetallurgyConstants.RageDecayRate;

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
            currentCombo = Mathf.Min(currentCombo + 1, maxComboCount);
        else
            currentCombo = 1;

        lastHitTime = Time.time;
        currentDamageMultiplier = 1f + (currentCombo * damageMultiplierPerHit);

        if (currentCombo >= 10)
            AchievementSystem.Instance?.TryUnlock("combo_10");

        if (!isRaging)
        {
            rageMeter = Mathf.Min(1f, rageMeter + ragePerHit);
            if (rageMeter >= 1f) StartRage();
        }
    }

    private void StartRage()
    {
        isRaging = true;
        if (FlareManager.Instance != null)
            FlareManager.Instance.SetIntensity(10);
    }

    private void EndRage()
    {
        isRaging = false;
        FlareManager.Instance?.SetIntensity(1);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        currentDamageMultiplier = 1f;
    }

    public float GetMetalCostReduction() => currentCombo * metalCostReduction;
}
