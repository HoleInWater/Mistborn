using UnityEngine;

public class ThrowableProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    
    [Header("Physics")]
    public Rigidbody rb;
    
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            
            SpawnImpactEffect();
            Destroy(gameObject);
        }
    }
    
    void SpawnImpactEffect()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }
}
