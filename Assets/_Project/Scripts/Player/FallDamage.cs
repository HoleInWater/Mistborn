using UnityEngine;

/// <summary>
/// Fall damage system with Pewter negation.
/// Lore: Pewter lets a Mistborn survive falls that would kill a normal person.
/// Burning Pewter at the moment of impact negates most fall damage.
/// Uses KE = ½mv² from PHYSICS-MATH-BOOK.md Section 1.
/// </summary>
public class FallDamage : MonoBehaviour
{
    [Header("Settings")]
    public float safeFallSpeed = 8f;
    public float lethalFallSpeed = 30f;
    public float maxFallDamage = 80f;

    [Header("Pewter Negation")]
    [Tooltip("Pewter reduces fall damage by this percentage")]
    public float pewterDamageReduction = 0.8f;
    [Tooltip("Flaring Pewter negates fall damage entirely below this speed")]
    public float pewterSafeFallSpeed = 20f;

    [Header("Effects")]
    public float landingShakeMagnitude = 0.3f;
    public float landingShakeDuration = 0.2f;

    [Header("References")]
    public Rigidbody playerRb;
    public Allomancer allomancer;
    public Animator animator;

    private bool wasAirborne = false;
    private float maxFallVelocity = 0f;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerRb == null) return;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);
        float verticalSpeed = -playerRb.linearVelocity.y; // Positive when falling

        // Track max fall speed while airborne
        if (!isGrounded)
        {
            wasAirborne = true;
            maxFallVelocity = Mathf.Max(maxFallVelocity, verticalSpeed);
        }

        // Landing detection
        if (isGrounded && wasAirborne)
        {
            OnLanding(maxFallVelocity);
            wasAirborne = false;
            maxFallVelocity = 0f;
        }
    }

    void OnLanding(float fallSpeed)
    {
        if (fallSpeed < safeFallSpeed) return;

        // Check Pewter
        bool pewterActive = allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter);
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        if (pewterActive && fallSpeed < pewterSafeFallSpeed * flare)
        {
            // Pewter absorbs the impact — just camera shake
            CameraShakeManager.Instance?.Shake(landingShakeDuration, landingShakeMagnitude * 0.3f);
            animator?.SetTrigger("Land");
            return;
        }

        // Calculate damage: KE = ½mv², scaled to gameplay
        // Damage scales linearly from 0 at safeFallSpeed to maxFallDamage at lethalFallSpeed
        float t = Mathf.InverseLerp(safeFallSpeed, lethalFallSpeed, fallSpeed);
        float damage = Mathf.Lerp(0f, maxFallDamage, t);

        // Pewter reduces remaining damage
        if (pewterActive)
        {
            float P = Mathf.Clamp01(flare / 2.5f);
            damage *= (1f - pewterDamageReduction * P);
        }

        // Skill tree reduction
        if (AllomanticSkillTree.Instance != null)
        {
            float fallReduction = AllomanticSkillTree.Instance.GetSkillValue("Move_FallDamage");
            damage *= (1f - fallReduction);
        }

        if (damage > 1f)
        {
            IDamageable health = GetComponent<IDamageable>();
            health?.TakeDamage(damage);

            CameraShakeManager.Instance?.Shake(landingShakeDuration * 2f, landingShakeMagnitude);
            SoundManager.Instance?.PlayImpactSound();

            // Ragdoll on very hard landing
            if (damage > maxFallDamage * 0.7f)
            {
                PlayerRagdoll ragdoll = GetComponent<PlayerRagdoll>();
                ragdoll?.OnImpact(Vector3.down, damage);
            }
        }

        animator?.SetTrigger("Land");
    }
}
