using UnityEngine;

/// <summary>
/// Block ability with Pewter enhancement.
/// Hold right mouse to block. Pewter burning increases damage reduction
/// and adds a short parry window at the start.
/// PHYSICS-MATH-BOOK.md Section 8: S = S_base × (1 + k × P)
/// </summary>
public class BlockAbility : MonoBehaviour
{
    [Header("Block Settings")]
    public KeyCode blockKey = KeyCode.Mouse1;
    public float baseDamageReduction = 0.4f;
    public float blockSpeedReduction = 0.5f;
    public float blockStaminaDrain = 8f;

    [Header("Parry Window")]
    public float parryWindowDuration = 0.2f;
    public float parryDamageReduction = 1f;
    public float parryCooldown = 1f;

    [Header("Pewter Enhancement")]
    [Tooltip("Pewter adds this to damage reduction (S = S_base × (1 + k×P))")]
    public float pewterBlockBonus = 0.3f;
    public float pewterParryWindowBonus = 0.15f;

    [Header("References")]
    public BasicPlayerMove playerController;
    public Allomancer allomancer;
    public PlayerStamina stamina;
    public Animator animator;

    private bool isBlocking = false;
    private bool isParrying = false;
    private float parryTimer = 0f;
    private float parryCooldownTimer = 0f;
    private float originalSpeed;

    void Start()
    {
        if (playerController == null) playerController = GetComponent<BasicPlayerMove>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (animator == null) animator = GetComponent<Animator>();

        blockKey = Keybinds.Block;

        if (playerController != null)
            originalSpeed = playerController.moveSpeed;
    }

    void Update()
    {
        parryCooldownTimer -= Time.deltaTime;
        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0f && isParrying)
            isParrying = false;

        if (Input.GetKeyDown(blockKey))
            StartBlock();

        if (Input.GetKey(blockKey) && isBlocking)
        {
            if (stamina != null)
                stamina.DrainStamina(blockStaminaDrain);
        }

        if (Input.GetKeyUp(blockKey))
            EndBlock();

        if (animator != null)
            animator.SetBool("IsBlocking", isBlocking);
    }

    void StartBlock()
    {
        isBlocking = true;

        // Parry window at start of block
        if (parryCooldownTimer <= 0f)
        {
            isParrying = true;
            float pewterBonus = GetPewterBonus();
            parryTimer = parryWindowDuration + (pewterBonus > 0 ? pewterParryWindowBonus : 0f);
            parryCooldownTimer = parryCooldown;
        }

        if (playerController != null)
            playerController.moveSpeed = originalSpeed * blockSpeedReduction;
    }

    void EndBlock()
    {
        isBlocking = false;
        isParrying = false;

        if (playerController != null)
            playerController.moveSpeed = originalSpeed;
    }

    /// <summary>
    /// Call this from damage system to get final damage after block/parry reduction.
    /// </summary>
    public float ProcessDamage(float incomingDamage)
    {
        if (!isBlocking) return incomingDamage;

        if (isParrying)
        {
            // Perfect parry — near-zero damage
            CameraShakeManager.Instance?.Shake(0.1f, 0.1f);
            SoundManager.Instance?.PlayImpactSound();
            AchievementSystem.Instance?.TryUnlock("parry_perfect");
            return incomingDamage * (1f - parryDamageReduction);
        }

        // Normal block — reduce by base + Pewter bonus
        float reduction = baseDamageReduction + GetPewterBonus();
        reduction = Mathf.Clamp01(reduction);

        SoundManager.Instance?.PlayImpactSound();
        return incomingDamage * (1f - reduction);
    }

    float GetPewterBonus()
    {
        if (allomancer == null || !allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
            return 0f;

        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float P = Mathf.Clamp01(flare / 2.5f);
        return pewterBlockBonus * P;
    }

    public bool IsBlocking() => isBlocking;
    public bool IsParrying() => isParrying;
    public float GetDamageReduction() => isBlocking ? baseDamageReduction + GetPewterBonus() : 0f;
}
