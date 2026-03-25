using UnityEngine;

public class PewterEnhancement : MonoBehaviour
{
    [Header("Settings")]
    public float strengthMultiplier = 1.5f;
    public float speedMultiplier = 1.8f;
    public float jumpMultiplier = 2f;
    public float damageMultiplier = 1.5f;
    public float healRateMultiplier = 2f;
    public float fallDamageReduction = 0.7f;
    public float metalCostPerSecond = 3f;

    [Header("References")]
    public Allomancer allomancer;
    public BasicPlayerMove playerMove;
    public PlayerCombat combat;

    private bool isBurning = false;
    private float originalMoveSpeed;
    private float originalJumpForce;
    private Rigidbody rb;

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
        playerMove = GetComponentInParent<BasicPlayerMove>();
        combat = GetComponentInParent<PlayerCombat>();
        rb = GetComponentInParent<Rigidbody>();

        if (playerMove != null)
        {
            originalMoveSpeed = playerMove.speed;
            originalJumpForce = playerMove.jumpForce;
        }
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && 
                    allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Pewter;

        if (isBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            ApplyPewterEffects(flareMult);
            DrainMetal(flareMult);
        }
        else if (wasBurning)
        {
            ResetPewterEffects();
        }
    }

    void ApplyPewterEffects(float flareMult)
    {
        if (playerMove != null)
        {
            playerMove.speed = originalMoveSpeed * speedMultiplier * flareMult;
            playerMove.jumpForce = originalJumpForce * jumpMultiplier * flareMult;
        }

        if (combat != null)
        {
            combat.damageMultiplier = damageMultiplier * flareMult;
        }
    }

    void ResetPewterEffects()
    {
        if (playerMove != null)
        {
            playerMove.speed = originalMoveSpeed;
            playerMove.jumpForce = originalJumpForce;
        }

        if (combat != null)
        {
            combat.damageMultiplier = 1f;
        }
    }

    void DrainMetal(float flareMult)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCostPerSecond * flareMult * Time.deltaTime);
    }

    public float GetFallDamageReduction() => fallDamageReduction;
}