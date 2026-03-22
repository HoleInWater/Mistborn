using UnityEngine;

namespace MistbornGame.Utilities
{
    public class DamageDealer : MonoBehaviour
    {
        [Header("Damage Configuration")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Normal;
        [SerializeField] private float knockbackForce = 0f;
        
        [Header("Combat")]
        [SerializeField] private string[] targetTags = { "Enemy" };
        [SerializeField] private string[] ignoreTags = { "Player" };
        
        [Header("Effects")]
        [SerializeField] private bool spawnHitEffect = true;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip hitSound;
        
        [Header("Owner")]
        [SerializeField] private GameObject owner;
        [SerializeField] private bool destroyOnHit = true;
        
        private bool hasDealtDamage = false;
        
        public float Damage
        {
            get => damage;
            set => damage = value;
        }
        
        public DamageType DamageType
        {
            get => damageType;
            set => damageType = value;
        }
        
        public GameObject Owner
        {
            get => owner;
            set => owner = value;
        }
        
        public void Initialize(float damage, DamageType type = DamageType.Normal)
        {
            this.damage = damage;
            this.damageType = type;
        }
        
        public void Initialize(float damage, DamageType type, GameObject owner)
        {
            this.damage = damage;
            this.damageType = type;
            this.owner = owner;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            ProcessCollision(other);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            ProcessCollision(collision.collider);
        }
        
        private void ProcessCollision(Collider other)
        {
            if (hasDealtDamage)
                return;
            
            if (ShouldIgnore(other.gameObject))
                return;
            
            DealDamage(other.gameObject);
            
            if (destroyOnHit)
            {
                hasDealtDamage = true;
                Destroy(gameObject);
            }
        }
        
        private bool ShouldIgnore(GameObject obj)
        {
            foreach (string tag in ignoreTags)
            {
                if (obj.CompareTag(tag))
                    return true;
            }
            
            if (owner != null && obj == owner)
                return true;
            
            return false;
        }
        
        public void DealDamage(GameObject target)
        {
            if (target == null)
                return;
            
            Health health = target.GetComponent<Health>();
            
            if (health != null)
            {
                health.TakeDamage(damage, damageType);
            }
            
            Enemy.Enemy enemy = target.GetComponent<Enemy.Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageType);
            }
            
            PlayerStats player = target.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(damage, damageType);
            }
            
            if (knockbackForce > 0f)
            {
                ApplyKnockback(target);
            }
            
            if (spawnHitEffect)
            {
                SpawnHitEffect(target.transform.position);
            }
            
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, target.transform.position);
            }
        }
        
        private void ApplyKnockback(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                direction.y = 0.5f;
                rb.AddForce(direction * knockbackForce * 100f, ForceMode.Impulse);
            }
        }
        
        private void SpawnHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        public void ResetDamageState()
        {
            hasDealtDamage = false;
        }
    }
    
    public enum DamageType
    {
        Normal,
        Heavy,
        Light,
        Perfect,
        Critical,
        Allomantic,
        Hemalurgic,
        Piercing,
        Fire,
        Ice
    }
}
