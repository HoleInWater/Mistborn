using UnityEngine;

/// <summary>
/// Advanced combat: combo chains, parry/block, and Allomancy-enhanced finishers.
/// Pewter strength from PHYSICS-MATH-BOOK.md Section 8: S = S_base × (1 + k × P)
/// </summary>
public class CombatComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    public int maxComboHits = 5;
    public float comboWindow = 0.8f;
    public float baseDamage = 20f;
    public float comboMultiplierPerHit = 1.15f;
    public float finisherDamageMultiplier = 3f;

    [Header("References")]
    public Animator animator;
    public Allomancer allomancer;

    private int currentCombo = 0;
    private float comboTimer = 0f;

    void Update()
    {
        if (comboTimer > 0f)
            comboTimer -= Time.deltaTime;
        else if (currentCombo > 0)
            ResetCombo();
    }

    public void RegisterHit()
    {
        currentCombo++;
        comboTimer = comboWindow;

        if (currentCombo >= maxComboHits)
        {
            PerformFinisher();
            ResetCombo();
        }
        else
        {
            animator?.SetInteger("ComboCount", currentCombo);
            animator?.SetTrigger("Attack");
        }
    }

    void PerformFinisher()
    {
        animator?.SetTrigger("Finisher");
        CameraShakeManager.Instance?.Shake(0.3f, 0.2f);
    }

    void ResetCombo()
    {
        currentCombo = 0;
        animator?.SetInteger("ComboCount", 0);
    }

    public float GetCurrentDamage()
    {
        float damage = baseDamage * Mathf.Pow(comboMultiplierPerHit, currentCombo);

        if (currentCombo >= maxComboHits)
            damage *= finisherDamageMultiplier;

        // Pewter enhancement
        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
        {
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float P = Mathf.Clamp01(flare / 2.5f);
            damage *= AllomancyPhysicsFormulas.CalculatePewterStrength(1f, 2f, P);
        }

        return damage;
    }

    public int GetComboCount() => currentCombo;
}

/// <summary>
/// Parry/block system with perfect parry window.
/// Pewter extends block damage reduction.
/// </summary>
public class ParrySystem : MonoBehaviour
{
    [Header("Block")]
    public float blockDamageReduction = 0.7f;
    public float blockStaminaCost = 5f;

    [Header("Parry")]
    public float parryWindow = 0.2f;
    public float parryStaminaCost = 10f;
    public float parryCooldown = 0.5f;

    [Header("References")]
    public Animator animator;
    public PlayerStamina stamina;
    public Allomancer allomancer;

    private bool isBlocking = false;
    private bool isParrying = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0f && isParrying)
            isParrying = false;

        // Mouse1 to block/parry (Keybinds.Block = Mouse1)
        if (Input.GetMouseButtonDown(1) && cooldownTimer <= 0f)
        {
            StartParry();
        }

        isBlocking = Input.GetMouseButton(1);
        animator?.SetBool("IsBlocking", isBlocking);
    }

    void StartParry()
    {
        isParrying = true;
        parryTimer = parryWindow;
        cooldownTimer = parryCooldown;
        animator?.SetTrigger("Parry");
        stamina?.UseStamina(parryStaminaCost);
    }

    /// <summary>
    /// Called by enemy attack to check if damage is mitigated.
    /// Returns the actual damage after block/parry reduction.
    /// </summary>
    public float ProcessIncomingDamage(float damage)
    {
        if (isParrying)
        {
            // Perfect parry — zero damage + counter opportunity
            CameraShakeManager.Instance?.Shake(0.1f, 0.1f);
            SoundManager.Instance?.PlayParrySound();
            AchievementSystem.Instance?.TryUnlock("parry_perfect");
            return 0f;
        }

        if (isBlocking)
        {
            float reduction = blockDamageReduction;
            // Pewter enhances block
            if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
                reduction *= 1.3f;

            stamina?.DrainStamina(blockStaminaCost);
            SoundManager.Instance?.PlayBlockSound();
            return damage * (1f - Mathf.Clamp01(reduction));
        }

        return damage;
    }

    public bool IsBlocking() => isBlocking;
    public bool IsParrying() => isParrying;
}
