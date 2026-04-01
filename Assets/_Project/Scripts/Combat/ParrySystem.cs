using UnityEngine;

/// <summary>
/// Parry/block system with perfect parry window.
/// Input is driven externally by PlayerCombat (right-click tap = parry, hold = block).
/// Pewter extends block damage reduction.
/// </summary>
[PlayerComponent("Combat", order: 15)]
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
    private Pewter _pewter;

    private bool isBlocking = false;
    private bool isParrying = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (stamina == null)  stamina  = GetComponent<PlayerStamina>();
        if (_pewter == null)  _pewter  = GetComponentInParent<Pewter>();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        parryTimer    -= Time.deltaTime;

        if (parryTimer <= 0f && isParrying)
            isParrying = false;
    }

    /// <summary>Called by PlayerCombat on a right-click tap to attempt a parry.</summary>
    public void TryParry()
    {
        if (cooldownTimer > 0f) return;
        isParrying    = true;
        parryTimer    = parryWindow;
        cooldownTimer = parryCooldown;
        animator?.SetTrigger("Parry");
        stamina?.UseStamina(parryStaminaCost);
    }

    /// <summary>Called by PlayerCombat each frame to sync block state.</summary>
    public void SetBlocking(bool blocking)
    {
        isBlocking = blocking;
        animator?.SetBool("IsBlocking", blocking);
        if (blocking && stamina != null)
            stamina.DrainStamina(blockStaminaCost * Time.deltaTime);
    }

    /// <summary>
    /// Called by enemy attacks — returns actual damage after block/parry.
    /// </summary>
    public float ProcessIncomingDamage(float damage)
    {
        if (isParrying)
        {
            CameraShakeManager.Instance?.Shake(0.1f, 0.1f);
            SoundManager.Instance?.PlayParrySound();
            AchievementSystem.Instance?.TryUnlock("parry_perfect");
            return 0f;
        }

        if (isBlocking)
        {
            float reduction = blockDamageReduction;
            if (_pewter != null && _pewter.IsBurningPewter())
                reduction *= 1.3f;

            SoundManager.Instance?.PlayBlockSound();
            return damage * (1f - Mathf.Clamp01(reduction));
        }

        return damage;
    }

    public bool IsBlocking() => isBlocking;
    public bool IsParrying() => isParrying;
}
