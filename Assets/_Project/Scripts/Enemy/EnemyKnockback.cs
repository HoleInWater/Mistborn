using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Makes enemies react to being pushed/pulled by Metallurgy.
/// Temporarily disables NavMeshAgent, applies ragdoll-like force,
/// then recovers and re-engages. Lighter enemies get pushed further.
/// </summary>
public class EnemyKnockback : MonoBehaviour
{
    [Header("Settings")]
    public float knockbackResistance = 1f;
    public float recoveryTime = 0.5f;
    public float stunThreshold = 15f;
    public float stunDuration = 1.5f;

    [Header("References")]
    public NavMeshAgent navAgent;
    public Animator animator;
    public EnemyAI enemyAI;

    private Rigidbody rb;
    private bool isKnockedBack = false;
    private bool isStunned = false;

    void Start()
    {
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody>();

        // Set resistance based on enemy type
        if (enemyAI != null)
        {
            switch (enemyAI.enemyType)
            {
                case EnemyAI.EnemyType.Bloodbrute: knockbackResistance = 3f; break;
                case EnemyAI.EnemyType.IronSentinel: knockbackResistance = 2f; break;
                case EnemyAI.EnemyType.Thug: knockbackResistance = 1.5f; break;
                default: knockbackResistance = 1f; break;
            }
        }
    }

    /// <summary>
    /// Apply knockback from an Metallurgic push or pull.
    /// Called when a Steel Push or Iron Pull hits this enemy.
    /// </summary>
    public void ApplyMetallurgicKnockback(Vector3 direction, float force)
    {
        if (isKnockedBack) return;

        float effectiveForce = force / knockbackResistance;

        if (effectiveForce > stunThreshold)
            StartCoroutine(StunSequence(direction, effectiveForce));
        else
            StartCoroutine(KnockbackSequence(direction, effectiveForce));
    }

    IEnumerator KnockbackSequence(Vector3 direction, float force)
    {
        isKnockedBack = true;

        // Disable nav
        if (navAgent != null) navAgent.enabled = false;

        // Ensure rigidbody
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.VelocityChange);

        animator?.SetTrigger("Hit");

        yield return new WaitForSeconds(recoveryTime);

        // Restore kinematic so NavMeshAgent can resume control
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.Warp(transform.position);
        }

        isKnockedBack = false;
    }

    IEnumerator StunSequence(Vector3 direction, float force)
    {
        isKnockedBack = true;
        isStunned = true;

        if (navAgent != null) navAgent.enabled = false;

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.VelocityChange);

        animator?.SetBool("IsStunned", true);

        yield return new WaitForSeconds(stunDuration);

        animator?.SetBool("IsStunned", false);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.Warp(transform.position);
        }

        isStunned = false;
        isKnockedBack = false;
    }

    /// <summary>
    /// Called by collision detection when a pushed object hits this enemy.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit by a high-speed metal object (coin, pushed metal)
        if (collision.relativeVelocity.magnitude > 10f)
        {
            CoinPhysics coin = collision.gameObject.GetComponent<CoinPhysics>();
            MetallurgicTarget metal = collision.gameObject.GetComponent<MetallurgicTarget>();

            if (coin != null || metal != null)
            {
                float impactSpeed = collision.relativeVelocity.magnitude;
                Vector3 impactDir = collision.relativeVelocity.normalized;

                // KE = ½mv²  (PHYSICS-MATH-BOOK.md Section 1)
                // Use the impacting object's mass so a heavy pushed door hurts more than a coin.
                float impactMass = collision.rigidbody != null ? collision.rigidbody.mass : 0.01f;
                float damage = 0.5f * impactMass * impactSpeed * impactSpeed;
                IDamageable hp = GetComponent<IDamageable>();
                hp?.TakeDamage(damage);

                // Knockback from impact
                ApplyMetallurgicKnockback(impactDir, impactSpeed * 0.5f);
            }
        }
    }

    public bool IsKnockedBack() => isKnockedBack;
    public bool IsStunned() => isStunned;
}
