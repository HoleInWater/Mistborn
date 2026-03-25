using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class AllomanticObjectInteraction : MonoBehaviour
{
    [Header("Allomantic Properties")]
    public float mass = 1f;
    public bool isAnchored = false;
    public bool canBePushed = true;
    public bool canBePulled = true;
    public float metalPurity = 1f; // 0-1, affects push/pull force

    [Header("Metal Type")]
    public MetalType metalType = MetalType.Steel;
    public enum MetalType { Steel, Iron, Pewter, Tin, Zinc, Brass, Copper, Bronze, Atium, Gold, Other }

    [Header("Physics")]
    public float friction = 0.8f;
    public float bounciness = 0.3f;
    public float angularFriction = 0.5f;

    [Header("Effects")]
    public GameObject onPushEffect;
    public GameObject onPullEffect;
    public AudioClip pushSound;
    public AudioClip pullSound;

    [Header("References")]
    private Rigidbody rb;
    private AudioSource audioSource;

    private Vector3 lastExternalForce;
    private float lastPushPullTime;
    private bool wasRecentlyPushed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = mass;
        rb.friction = friction;
        rb.bounciness = bounciness;
        rb.angularDrag = angularFriction;
        rb.isKinematic = isAnchored;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (wasRecentlyPushed && Time.time - lastPushPullTime > 0.1f)
        {
            wasRecentlyPushed = false;
        }
    }

    public void ApplyAllomanticPush(Vector3 direction, float force, float playerMass)
    {
        if (!canBePushed || isAnchored) return;

        float forceMultiplier = metalPurity;
        float playerRatio = playerMass / (playerMass + mass);

        if (mass > playerMass * 3)
        {
            // Anchored - player gets pushed instead
            Debug.Log($"[ALLOMANTIC] {gameObject.name} is anchored, pushing player instead");
            return;
        }

        Vector3 appliedForce = direction.normalized * force * forceMultiplier;
        rb.AddForce(appliedForce, ForceMode.Impulse);

        lastExternalForce = appliedForce;
        lastPushPullTime = Time.time;
        wasRecentlyPushed = true;

        PlayEffect(pushSound, onPushEffect);

        Debug.Log($"[ALLOMANTIC] Pushed {gameObject.name} with force {force:F1}");
    }

    public void ApplyAllomanticPull(Vector3 direction, float force, float playerMass)
    {
        if (!canBePulled || isAnchored) return;

        float forceMultiplier = metalPurity;
        float targetRatio = mass / (playerMass + mass);

        if (mass > playerMass * 3)
        {
            Debug.Log($"[ALLOMANTIC] {gameObject.name} is anchored, pulling player instead");
            return;
        }

        Vector3 appliedForce = direction.normalized * force * forceMultiplier;
        rb.AddForce(appliedForce, ForceMode.Impulse);

        lastExternalForce = appliedForce;
        lastPushPullTime = Time.time;
        wasRecentlyPushed = true;

        PlayEffect(pullSound, onPullEffect);

        Debug.Log($"[ALLOMANTIC] Pulled {gameObject.name} with force {force:F1}");
    }

    void PlayEffect(AudioClip clip, GameObject effectPrefab)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public Vector3 GetLastExternalForce() => lastExternalForce;
    public bool WasRecentlyPushed() => wasRecentlyPushed;
    public float GetVelocityMagnitude() => rb.linearVelocity.magnitude;
}

public class PlayerObjectCollision : MonoBehaviour
{
    [Header("Settings")]
    public float collisionPushForce = 10f;
    public float pushRecoveryTime = 0.3f;
    public LayerMask pushableLayer;

    [Header("References")]
    public Rigidbody playerRb;
    public AllomanticObjectInteraction allomanticObject;

    private float pushRecoveryTimer;

    void Update()
    {
        if (pushRecoveryTimer > 0)
            pushRecoveryTimer -= Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (pushRecoveryTimer > 0) return;

        AllomanticObjectInteraction obj = collision.gameObject.GetComponent<AllomanticObjectInteraction>();
        if (obj == null) return;

        if (((1 << collision.gameObject.layer) & pushableLayer) == 0) return;

        float playerMass = playerRb != null ? playerRb.mass : 80f;
        float objMass = obj.mass;

        Vector3 impactDir = collision.contacts[0].normal;
        float impactForce = collision.relativeVelocity.magnitude * collisionPushForce;

        if (objMass > playerMass * 3)
        {
            // Player gets pushed back
            if (playerRb != null)
            {
                Vector3 pushBack = -impactDir * impactForce * (objMass / playerMass);
                playerRb.linearVelocity += pushBack;
                pushRecoveryTimer = pushRecoveryTime;
            }
        }
        else
        {
            // Object gets pushed
            obj.rb.AddForce(impactDir * impactForce, ForceMode.Impulse);
        }

        Debug.Log($"[COLLISION] Player collided with {obj.name}, force: {impactForce:F1}");
    }

    void OnCollisionStay(Collision collision)
    {
        AllomanticObjectInteraction obj = collision.gameObject.GetComponent<AllomanticObjectInteraction>();
        if (obj == null) return;

        if (((1 << collision.gameObject.layer) & pushableLayer) == 0) return;

        float playerMass = playerRb != null ? playerRb.mass : 80f;

        if (obj.mass > playerMass * 3 && playerRb != null && collision.contactCount > 0)
        {
            // Push player slowly when leaning against heavy object
            Vector3 pushDir = -collision.GetContact(0).normal;
            playerRb.linearVelocity += pushDir * 0.5f * Time.fixedDeltaTime;
        }
    }
}

public class PushPullImpactHandler : MonoBehaviour
{
    [Header("Impact Settings")]
    public float minImpactVelocity = 2f;
    public float impactDamageThreshold = 5f;
    public float maxImpactDamage = 20f;

    [Header("Visual")]
    public GameObject impactParticlePrefab;
    public AudioClip impactSound;

    [Header("References")]
    private Rigidbody rb;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float velocity = rb.linearVelocity.magnitude;

        if (velocity > minImpactVelocity)
        {
            HandleImpact(collision, velocity);
        }
    }

    void HandleImpact(Collision collision, float velocity)
    {
        // Spawn impact particles
        if (impactParticlePrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject particles = Instantiate(impactParticlePrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(particles, 2f);
        }

        // Play impact sound
        if (impactSound != null && audioSource != null)
        {
            float volume = Mathf.Clamp01(velocity / 10f);
            audioSource.PlayOneShot(impactSound, volume);
        }

        // Apply damage if applicable
        if (velocity > impactDamageThreshold)
        {
            float damage = Mathf.Lerp(0, maxImpactDamage, (velocity - impactDamageThreshold) / 10f);
            
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        Debug.Log($"[IMPACT] {gameObject.name} hit {collision.gameObject.name} at {velocity:F1} m/s");
    }

    public void OnPushedByAllomancy(float force)
    {
        Debug.Log($"[ALLOMANTIC] {gameObject.name} was pushed with force {force:F1}");
    }
}

public class MomentumTransfer : MonoBehaviour
{
    [Header("Settings")]
    public float transferEfficiency = 0.8f;
    public float minVelocityForTransfer = 3f;

    [Header("References")]
    public Transform playerTransform;
    public Rigidbody playerRb;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TransferMomentumToPlayer(collision);
        }
    }

    void TransferMomentumToPlayer(Collision collision)
    {
        if (playerRb == null || rb == null) return;

        float velocity = rb.linearVelocity.magnitude;
        if (velocity < minVelocityForTransfer) return;

        Vector3 transferDir = (playerTransform.position - transform.position).normalized;
        
        float transferAmount = velocity * transferEfficiency;
        
        playerRb.linearVelocity += transferDir * transferAmount;
        
        rb.linearVelocity *= (1f - transferEfficiency);
        
        Debug.Log($"[MOMENTUM] Transferred {transferAmount:F1} velocity to player");
    }
}