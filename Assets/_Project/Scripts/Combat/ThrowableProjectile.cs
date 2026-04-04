using UnityEngine;

/// <summary>
/// Projectile behavior for thrown/pushed coins and metal objects.
/// Deals damage on impact based on velocity (KE = ½mv²).
/// </summary>
public class ThrowableProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float baseDamage = 15f;
    public float velocityDamageScale = 0.5f;
    public float lifetime = 10f;
    public float minDamageVelocity = 5f;
    public bool hasHitTarget = false;

    private Rigidbody rb;
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    void Update()
    {
        if (Time.time - spawnTime > lifetime)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHitTarget) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minDamageVelocity) return;
        if (collision.gameObject.CompareTag("Player")) return;

        float damage = baseDamage + (speed * velocityDamageScale);

        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            hasHitTarget = true;

            EnemyKnockback kb = collision.gameObject.GetComponent<EnemyKnockback>();
            if (kb != null)
                kb.ApplyMetallurgicKnockback(collision.relativeVelocity.normalized, speed * 0.3f);

            if (collision.contactCount > 0)
                ParticleEffectsManager.Instance?.PlayHitEffect(collision.GetContact(0).point, damage);

            SoundManager.Instance?.PlayImpactSound();
        }
    }
}
