using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f;
    public float gravity = 0f;
    public bool isHoming = false;
    public Transform homingTarget;
    public float homingStrength = 5f;
    
    [Header("Impact")]
    public GameObject impactEffect;
    public AudioClip impactSound;
    public float explosionRadius = 0f;
    public float explosionForce = 0f;
    
    [Header("Allomancy")]
    public MetalType metalType = MetalType.Steel;
    public float pushForce = 100f;
    public float pullForce = 100f;
    
    [Header("Collision")]
    public LayerMask hitLayers;
    public bool pierceEnemies = false;
    public int maxPierceCount = 0;
    private int currentPierceCount = 0;
    
    private Rigidbody rb;
    private Vector3 direction;
    private float lifetimeTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = gravity > 0f;
            rb.AddForce(direction * speed, ForceMode.Impulse);
        }
    }
    
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }
    
    public void Initialize(Vector3 direction, float damage, MetalType metal = MetalType.Steel)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.metalType = metal;
    }
    
    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        
        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        if (isHoming && homingTarget != null)
        {
            Vector3 targetDirection = (homingTarget.position - transform.position).normalized;
            direction = Vector3.Lerp(direction, targetDirection, homingStrength * Time.deltaTime);
            
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
        }
        
        if (gravity > 0f && rb != null)
        {
            rb.velocity += Vector3.down * gravity * Time.deltaTime;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;
        
        if (other.CompareTag("Player") || other.CompareTag("Projectile")) return;
        
        ProcessImpact(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if ((hitLayers.value & (1 << collision.gameObject.layer)) == 0) return;
        
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Projectile")) return;
        
        ProcessCollision(collision);
    }
    
    void ProcessImpact(Collider other)
    {
        DealDamage(other);
        ApplyAllomanticForce(other);
        
        if (explosionRadius > 0f)
        {
            Explode();
        }
        else
        {
            CreateImpactEffect();
        }
        
        if (!pierceEnemies || currentPierceCount >= maxPierceCount)
        {
            Destroy(gameObject);
        }
        else
        {
            currentPierceCount++;
        }
    }
    
    void ProcessCollision(Collision collision)
    {
        DealDamage(collision.collider);
        ApplyAllomanticForce(collision.collider);
        
        if (explosionRadius > 0f)
        {
            Explode();
        }
        else
        {
            CreateImpactEffect();
        }
        
        if (!pierceEnemies || currentPierceCount >= maxPierceCount)
        {
            Destroy(gameObject);
        }
        else
        {
            currentPierceCount++;
        }
    }
    
    void DealDamage(Collider other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
    
    void ApplyAllomanticForce(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        switch (metalType)
        {
            case MetalType.Steel:
                rb.AddForce(direction * pushForce, ForceMode.Impulse);
                break;
            case MetalType.Iron:
                rb.AddForce(-direction * pullForce, ForceMode.Impulse);
                break;
        }
    }
    
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f);
            }
            
            Health health = nearby.GetComponent<Health>();
            if (health != null)
            {
                float distance = Vector3.Distance(transform.position, nearby.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                health.TakeDamage(damage * damageFalloff);
            }
        }
        
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    
    void CreateImpactEffect()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }
    }
}

public class ThrowableProjectile : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwArc = 0.5f;
    public float gravity = 9.8f;
    
    [Header("Trajectory")]
    public int trajectoryPoints = 20;
    public float timeStep = 0.1f;
    public LineRenderer trajectoryLine;
    
    private Rigidbody rb;
    private Vector3 throwDirection;
    private bool isThrown = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    public void Aim(Vector3 direction)
    {
        throwDirection = direction;
        DrawTrajectory();
    }
    
    public void Throw()
    {
        if (isThrown) return;
        
        isThrown = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        
        Vector3 force = throwDirection * throwForce;
        force.y += throwArc * throwForce;
        
        rb.AddForce(force, ForceMode.Impulse);
        
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }
    
    void DrawTrajectory()
    {
        if (trajectoryLine == null) return;
        
        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;
        
        Vector3 startPos = transform.position;
        Vector3 velocity = throwDirection * throwForce;
        velocity.y += throwArc * throwForce;
        
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPos + velocity * time;
            point.y = startPos.y + velocity.y * time - 0.5f * gravity * time * time;
            
            trajectoryLine.SetPosition(i, point);
        }
    }
}

public class AllomanticProjectile : MonoBehaviour
{
    [Header("Allomancy Settings")]
    public MetalType metalType = MetalType.Steel;
    public float forceMultiplier = 1f;
    
    [Header("Visual")]
    public GameObject trailEffect;
    public Color projectileColor = Color.cyan;
    public float glowIntensity = 2f;
    
    private ProjectileController projectile;
    
    void Start()
    {
        projectile = GetComponent<ProjectileController>();
        
        if (projectile == null)
        {
            projectile = gameObject.AddComponent<ProjectileController>();
        }
        
        projectile.metalType = metalType;
        ApplyVisualEffects();
    }
    
    void ApplyVisualEffects()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", projectileColor * glowIntensity);
        }
        
        if (trailEffect != null)
        {
            ParticleSystem ps = trailEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = projectileColor;
            }
        }
    }
    
    public void SetPushForce(float force)
    {
        if (projectile != null)
        {
            projectile.pushForce = force * forceMultiplier;
            projectile.metalType = MetalType.Steel;
        }
    }
    
    public void SetPullForce(float force)
    {
        if (projectile != null)
        {
            projectile.pullForce = force * forceMultiplier;
            projectile.metalType = MetalType.Iron;
        }
    }
}
