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

        // Offset origin above the foot pivot so we don't start inside the capsule boundary.
        // Short distance (0.25 m) catches ground within a tight margin.
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.15f, Vector3.down, 0.3f);
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

        // KE = ½mv² — damage scales with v², not v (PHYSICS-MATH-BOOK.md Section 1).
        // InverseLerp on v² gives the correct quadratic ramp: small falls do little
        // damage while fast falls spike sharply, matching real kinetic energy growth.
        float t = Mathf.InverseLerp(safeFallSpeed * safeFallSpeed,
                                    lethalFallSpeed * lethalFallSpeed,
                                    fallSpeed * fallSpeed);
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

        // Pewter superhero landing — slam creates AOE shockwave
        if (pewterActive && fallSpeed > safeFallSpeed * 1.5f)
            PewterSlam(fallSpeed);
    }

    /// <summary>
    /// Lore: a Mistborn burning Pewter can survive falls that would kill a normal
    /// person, and their landing impact can crack stone and stagger nearby enemies.
    /// This is the "superhero landing" — knee down, fist to the ground, shockwave.
    /// </summary>
    void PewterSlam(float fallSpeed)
    {
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        // KE = ½mv² — the slam damage scales with how fast you were falling
        float playerMass = playerRb != null ? playerRb.mass : 70f;
        float impactKE = AllomancyPhysicsFormulas.CalculateKineticEnergy(playerMass, fallSpeed);

        // Shockwave radius scales with impact energy (capped)
        float radius = Mathf.Clamp(Mathf.Sqrt(impactKE) * 0.1f * flare, 3f, 12f);
        float damage = Mathf.Clamp(impactKE * 0.01f * flare, 5f, 40f);

        // Find and damage/stagger nearby enemies
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.gameObject == this.gameObject) continue;

            // Damage
            IDamageable target = col.GetComponentInParent<IDamageable>();
            target?.TakeDamage(damage);

            // Knockback — push enemies away from landing point
            Rigidbody enemyRb = col.attachedRigidbody;
            if (enemyRb != null && !enemyRb.isKinematic)
            {
                Vector3 awayDir = (col.transform.position - transform.position).normalized;
                awayDir.y = 0.3f; // slight upward angle
                float knockForce = damage * 0.5f;
                enemyRb.AddForce(awayDir * knockForce, ForceMode.Impulse);
            }

            // Stagger animation
            var enemyAI = col.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.animator?.SetTrigger("Hit");
        }

        // Effects — dramatic impact
        CameraShakeManager.Instance?.Shake(0.4f * flare, 0.3f * flare);
        SoundManager.Instance?.PlayImpactSound();
        HitstopManager.Instance?.PewterSlam();
        GroundSlamEffect.Spawn(transform.position, radius, flare);

        // Drain extra pewter for the slam
        if (allomancer != null)
            allomancer.DrainMetal(AllomancySkill.MetalType.Pewter,
                AllomancyConstants.PewterDrainRate * 2f);
    }
}
