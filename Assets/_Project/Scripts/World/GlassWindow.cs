using UnityEngine;
using System.Collections;

namespace MistbornGame.Environment
{
    public class GlassWindow : MonoBehaviour
    {
        [Header("Window Configuration")]
        [SerializeField] private float glassHealth = 100f;
        [SerializeField] private float shatterThreshold = 0.3f;
        [SerializeField] private GameObject glassPane;
        [SerializeField] private Material intactGlassMaterial;
        [SerializeField] private Material crackedGlassMaterial;
        [SerializeField] private Material shatteredGlassMaterial;
        
        [Header("Shatter Effect")]
        [SerializeField] private GameObject shatterParticlePrefab;
        [SerializeField] private float particleLifetime = 3f;
        [SerializeField] private AudioClip shatterSound;
        [SerializeField] private AudioClip crackSound;
        
        [Header("Allomancy")]
        [SerializeField] private bool canBePushed = true;
        [SerializeField] private bool canBePulled = true;
        [SerializeField] private float pushForceThreshold = 15f;
        [SerializeField] private float pullForceThreshold = 12f;
        
        [Header("Physics")]
        [SerializeField] private Rigidbody glassRigidbody;
        [SerializeField] private bool enablePhysics = true;
        [SerializeField] private float glassMass = 10f;
        
        [Header("Debris")]
        [SerializeField] private int shardCount = 20;
        [SerializeField] private float shardSpreadRadius = 3f;
        [SerializeField] private float shardSpreadForce = 10f;
        
        private float currentHealth;
        private float maxHealth;
        private bool isShattered = false;
        private bool isCracked = false;
        private Renderer windowRenderer;
        private Collider windowCollider;
        
        public bool IsShattered => isShattered;
        public bool IsCracked => isCracked;
        
        private void Start()
        {
            maxHealth = glassHealth;
            currentHealth = glassHealth;
            
            windowRenderer = GetComponent<Renderer>();
            windowCollider = GetComponent<Collider>();
            
            if (glassPane != null && intactGlassMaterial != null)
            {
                glassPane.GetComponent<Renderer>().material = intactGlassMaterial;
            }
            
            if (glassRigidbody == null && enablePhysics)
            {
                glassRigidbody = gameObject.AddComponent<Rigidbody>();
                glassRigidbody.mass = glassMass;
                glassRigidbody.isKinematic = true;
            }
        }
        
        private void Update()
        {
            if (isShattered)
                return;
            
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (windowRenderer == null)
                return;
            
            float healthPercent = currentHealth / maxHealth;
            
            if (healthPercent <= shatterThreshold)
            {
                if (!isShattered)
                    Shatter();
            }
            else if (healthPercent <= 0.5f && !isCracked)
            {
                ApplyCracks();
            }
        }
        
        private void ApplyCracks()
        {
            isCracked = true;
            
            if (crackedGlassMaterial != null && glassPane != null)
            {
                glassPane.GetComponent<Renderer>().material = crackedGlassMaterial;
            }
            
            AudioSource.PlayClipAtPoint(crackSound, transform.position);
            
            if (shatterParticlePrefab != null)
            {
                GameObject crackVfx = Instantiate(shatterParticlePrefab, transform.position, Quaternion.identity);
                crackVfx.transform.localScale = Vector3.one * 0.5f;
                Destroy(crackVfx, 1f);
            }
        }
        
        public void Shatter()
        {
            if (isShattered)
                return;
            
            isShattered = true;
            
            AudioSource.PlayClipAtPoint(shatterSound, transform.position);
            
            if (glassPane != null)
            {
                glassPane.SetActive(false);
            }
            
            if (windowCollider != null)
            {
                windowCollider.enabled = false;
            }
            
            SpawnGlassShards();
            SpawnShatterParticles();
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.1f, 0.2f);
        }
        
        private void SpawnGlassShards()
        {
            if (glassRigidbody != null)
            {
                glassRigidbody.isKinematic = false;
                glassRigidbody.AddExplosionForce(20f, transform.position, 5f);
            }
            
            for (int i = 0; i < shardCount; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject shard = CreateShardMesh();
                
                if (shard != null)
                {
                    shard.transform.position = spawnPos;
                    shard.transform.rotation = Random.rotation;
                    
                    Rigidbody shardRb = shard.AddComponent<Rigidbody>();
                    shardRb.mass = 0.1f;
                    
                    Vector3 randomDir = (spawnPos - transform.position).normalized + Random.insideUnitSphere;
                    randomDir.y = Mathf.Abs(randomDir.y) + 0.5f;
                    shardRb.AddForce(randomDir * shardSpreadForce * 50f, ForceMode.Impulse);
                    shardRb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
                    
                    StartCoroutine(DestroyShardAfterTime(shard, 5f));
                }
            }
        }
        
        private GameObject CreateShardMesh()
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shard.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            
            if (shatteredGlassMaterial != null)
            {
                shard.GetComponent<Renderer>().material = shatteredGlassMaterial;
            }
            
            Destroy(shard.GetComponent<Collider>());
            
            return shard;
        }
        
        private void SpawnShatterParticles()
        {
            if (shatterParticlePrefab != null)
            {
                GameObject particles = Instantiate(shatterParticlePrefab, transform.position, Quaternion.identity);
                particles.transform.localScale = Vector3.one * 2f;
                Destroy(particles, particleLifetime);
            }
        }
        
        private IEnumerator DestroyShardAfterTime(GameObject shard, float time)
        {
            yield return new WaitForSeconds(time);
            
            if (shard != null)
            {
                Destroy(shard);
            }
        }
        
        public void TakeDamage(float damage, Vector3 impactPoint)
        {
            if (isShattered)
                return;
            
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Shatter();
            }
            else if (currentHealth / maxHealth <= shatterThreshold)
            {
                Shatter();
            }
            else if (!isCracked)
            {
                ApplyCracks();
            }
        }
        
        public void ApplyForceFromDirection(Vector3 direction, float force)
        {
            if (!canBePushed || isShattered)
                return;
            
            if (force >= pushForceThreshold)
            {
                Vector3 impactPoint = transform.position + direction * 0.5f;
                TakeDamage(force * 5f, impactPoint);
                
                if (glassRigidbody != null && !glassRigidbody.isKinematic)
                {
                    glassRigidbody.AddForceAtPosition(direction * force * 100f, impactPoint, ForceMode.Impulse);
                }
            }
        }
        
        public void ApplyPullForce(Vector3 sourcePosition, float force)
        {
            if (!canBePulled || isShattered)
                return;
            
            Vector3 direction = (transform.position - sourcePosition).normalized;
            
            if (force >= pullForceThreshold)
            {
                Vector3 impactPoint = transform.position;
                TakeDamage(force * 3f, impactPoint);
                
                if (glassRigidbody != null)
                {
                    glassRigidbody.AddForce(direction * force * 100f, ForceMode.Impulse);
                }
            }
        }
        
        public void RepairWindow()
        {
            currentHealth = maxHealth;
            isShattered = false;
            isCracked = false;
            
            if (glassPane != null)
            {
                glassPane.SetActive(true);
                if (intactGlassMaterial != null)
                {
                    glassPane.GetComponent<Renderer>().material = intactGlassMaterial;
                }
            }
            
            if (windowCollider != null)
            {
                windowCollider.enabled = true;
            }
            
            if (glassRigidbody != null)
            {
                glassRigidbody.isKinematic = true;
                glassRigidbody.velocity = Vector3.zero;
                glassRigidbody.angularVelocity = Vector3.zero;
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 5f)
            {
                TakeDamage(collision.relativeVelocity.magnitude * 10f, collision.contacts[0].point);
            }
        }
    }
}
