using UnityEngine;

/// <summary>
/// Manages push/pull physics on individual metal objects.
/// Handles player-object collision with momentum transfer.
/// Uses PHYSICS-MATH-BOOK.md Section 1 for kinetic energy and momentum.
/// </summary>
public class AllomanticObjectInteraction : MonoBehaviour
{
    [Header("Object Properties")]
    public float mass = 1f;
    public bool isMetallic = true;
    public bool canBePushed = true;
    public bool canBePulled = true;

    [Header("Impact Settings")]
    public float minImpactVelocity = 3f;
    public float impactDamageMultiplier = 5f;

    [Header("Audio")]
    public AudioClip impactSound;

    private Rigidbody rb;
    private bool wasMovingFast = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) mass = rb.mass;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minImpactVelocity) return;

        // KE = ½mv² from PHYSICS-MATH-BOOK.md Section 1
        float kineticEnergy = AllomancyPhysicsFormulas.CalculateKineticEnergy(mass, impactSpeed);
        float damage = kineticEnergy * impactDamageMultiplier * 0.01f; // Scale for gameplay

        // Damage what we hit
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null && damage > 1f)
        {
            target.TakeDamage(damage);
            CameraShakeManager.Instance?.Shake(0.15f, 0.1f);
        }

        // Audio
        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, collision.contacts[0].point);
        else
            SoundManager.Instance?.PlayImpactSound();

        // Particle effect
        ParticleEffectsManager.Instance?.PlayHitEffect(
            collision.contacts[0].point, damage);
    }

    /// <summary>
    /// Apply an Allomantic force to this object (called by SteelPush/IronPull).
    /// </summary>
    public void ApplyAllomanticForce(Vector3 force, bool isPush)
    {
        if (rb == null) return;
        if (isPush && !canBePushed) return;
        if (!isPush && !canBePulled) return;

        rb.AddForce(force, ForceMode.Force);
    }

    public float GetMass() => mass;
    public bool IsMetallic() => isMetallic;
}

/// <summary>
/// Handles player-object collision when running into Allomantic objects.
/// </summary>
public class PlayerObjectCollision : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask pushableLayer;
    public float pushThreshold = 2f;

    private Rigidbody playerRb;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        pushableLayer = LayerMask.GetMask("Metal");
    }

    void OnCollisionEnter(Collision collision)
    {
        AllomanticObjectInteraction obj = collision.gameObject.GetComponent<AllomanticObjectInteraction>();
        if (obj == null) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < pushThreshold) return;

        // Momentum transfer: p = mv
        float playerMomentum = playerRb != null ? playerRb.mass * impactSpeed : 80f * impactSpeed;
        float objectMomentum = obj.GetMass() * impactSpeed;

        if (playerMomentum > objectMomentum)
        {
            // Player pushes object
            Vector3 dir = (collision.transform.position - transform.position).normalized;
            Rigidbody objRb = collision.gameObject.GetComponent<Rigidbody>();
            if (objRb != null)
                objRb.AddForce(dir * playerMomentum * 0.5f, ForceMode.Impulse);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        AllomanticObjectInteraction obj = collision.gameObject.GetComponent<AllomanticObjectInteraction>();
        if (obj == null || playerRb == null) return;
        if (collision.contactCount == 0) return;

        if (obj.GetMass() > playerRb.mass * 3)
        {
            Vector3 pushDir = -collision.GetContact(0).normal;
            playerRb.linearVelocity += pushDir * 0.5f * Time.fixedDeltaTime;
        }
    }
}
