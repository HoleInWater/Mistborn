using UnityEngine;

/// <summary>
/// Fall damage system with Pewter negation.
/// Lore: Pewter lets a Mistborn survive falls that would kill a normal person.
/// Burning Pewter at the moment of impact negates most fall damage.
/// Uses KE = ½mv² from PHYSICS-MATH-BOOK.md Section 1.
/// </summary>
[PlayerComponent("Movement", order: 110)]
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
    private Pewter _pewter;

    private bool wasAirborne = false;
    private float maxFallVelocity = 0f;

    void Start()
    {
        if (playerRb  == null) playerRb  = GetComponent<Rigidbody>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator   == null) animator   = GetComponent<Animator>();
        _pewter = GetComponentInChildren<Pewter>();
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

        bool pewterActive = _pewter != null && _pewter.IsBurningPewter();
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        if (pewterActive && fallSpeed < pewterSafeFallSpeed * flare)
        {
            CameraShakeManager.Instance?.Shake(landingShakeDuration, landingShakeMagnitude * 0.3f);
            animator?.SetTrigger("Land");
            return;
        }

        float t = Mathf.InverseLerp(safeFallSpeed, lethalFallSpeed, fallSpeed);
        float damage = Mathf.Lerp(0f, maxFallDamage, t);

        // Pewter toughness reduces fall damage — scales with flare
        if (pewterActive)
        {
            float toughness = Mathf.Clamp01((flare - 1f) / 9f);
            damage *= (1f - pewterDamageReduction * toughness);
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
