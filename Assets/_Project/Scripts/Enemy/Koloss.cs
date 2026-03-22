using UnityEngine;

namespace MistbornGame.Enemy
{
    public enum KolossType
    {
        Normal,
        Brute,
        Heavy,
        Berserker
    }
    
    public class Koloss : Enemy
    {
        [Header("Koloss Specific")]
        [SerializeField] private KolossType kolossType = KolossType.Normal;
        [SerializeField] private float baseJumpForce = 10f;
        [SerializeField] private float groundSlamRadius = 5f;
        [SerializeField] private float groundSlamDamage = 30f;
        [SerializeField] private float groundSlamKnockback = 20f;
        [SerializeField] private GameObject groundSlamVfx;
        
        [Header("Berserker State")]
        [SerializeField] private float berserkerThreshold = 0.3f;
        [SerializeField] private float berserkerSpeedMultiplier = 2f;
        [SerializeField] private float berserkerDamageMultiplier = 1.5f;
        [SerializeField] private float berserkerRageDuration = 10f;
        
        [Header("Visual")]
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material enragedMaterial;
        [SerializeField] private ParticleSystem rageParticles;
        
        private bool isBerserker = false;
        private bool isGroundedSlamming = false;
        private bool hasJumped = false;
        private float rageEndTime = 0f;
        
        protected override void Start()
        {
            base.Start();
            
            switch (kolossType)
            {
                case KolossType.Normal:
                    maxHealth = 150f;
                    damage = 20f;
                    attackRange = 3f;
                    agent.speed = 4f;
                    break;
                case KolossType.Brute:
                    maxHealth = 300f;
                    damage = 35f;
                    attackRange = 4f;
                    agent.speed = 3.5f;
                    break;
                case KolossType.Heavy:
                    maxHealth = 500f;
                    damage = 50f;
                    attackRange = 5f;
                    agent.speed = 2.5f;
                    groundSlamDamage *= 1.5f;
                    groundSlamRadius *= 1.5f;
                    break;
                case KolossType.Berserker:
                    maxHealth = 200f;
                    damage = 30f;
                    attackRange = 3.5f;
                    agent.speed = 5f;
                    berserkerThreshold = 0.5f;
                    break;
            }
            
            currentHealth = maxHealth;
        }
        
        protected override void Update()
        {
            if (isBerserker && Time.time > rageEndTime)
            {
                EndBerserkerMode();
            }
            
            base.Update();
            
            if (!isGroundedSlamming && state == EnemyState.Chase)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer > attackRange * 1.5f && !hasJumped && CanJump())
                {
                    JumpTowardsPlayer();
                }
            }
        }
        
        private bool CanJump()
        {
            if (!agent.isOnNavMesh) return false;
            
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(player.position, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
            return false;
        }
        
        private void JumpTowardsPlayer()
        {
            hasJumped = true;
            
            Vector3 jumpDirection = (player.position - transform.position).normalized;
            jumpDirection.y = 0.5f;
            
            agent.velocity = Vector3.zero;
            agent.enabled = false;
            
            GetComponent<Rigidbody>().AddForce(jumpDirection * baseJumpForce * 100f, ForceMode.Impulse);
            
            animator.SetTrigger("JumpAttack");
            
            Invoke(nameof(StartGroundedSlam), 0.5f);
        }
        
        private void StartGroundedSlam()
        {
            isGroundedSlamming = true;
            
            GetComponent<Rigidbody>().velocity = Vector3.down * 20f;
            
            Invoke(nameof(ExecuteGroundedSlam), 0.3f);
        }
        
        private void ExecuteGroundedSlam()
        {
            if (groundSlamVfx != null)
            {
                Instantiate(groundSlamVfx, transform.position, Quaternion.identity);
            }
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundSlamRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0.5f;
                    knockbackDirection.Normalize();
                    
                    hitCollider.GetComponent<Rigidbody>()?.AddForce(knockbackDirection * groundSlamKnockback * 100f, ForceMode.Impulse);
                    
                    DealDamage(groundSlamDamage, DamageType.Heavy, hitCollider.gameObject);
                }
            }
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.3f, 0.5f);
            
            animator.SetTrigger("GroundSlamEnd");
            isGroundedSlamming = false;
            
            Invoke(nameof(ReEnableAgent), 0.5f);
        }
        
        private void ReEnableAgent()
        {
            hasJumped = false;
            agent.enabled = true;
        }
        
        protected override void CheckState()
        {
            if (isGroundedSlamming) return;
            
            base.CheckState();
            
            if (!isBerserker && currentHealth / maxHealth <= berserkerThreshold)
            {
                EnterBerserkerMode();
            }
        }
        
        private void EnterBerserkerMode()
        {
            isBerserker = true;
            rageEndTime = Time.time + berserkerRageDuration;
            
            agent.speed *= berserkerSpeedMultiplier;
            damage *= berserkerDamageMultiplier;
            
            animator.SetBool("Berserker", true);
            
            if (bodyRenderer != null && enragedMaterial != null)
            {
                bodyRenderer.material = enragedMaterial;
            }
            
            if (rageParticles != null)
            {
                rageParticles.Play();
            }
            
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/KolossRage"), transform.position);
        }
        
        private void EndBerserkerMode()
        {
            isBerserker = false;
            
            switch (kolossType)
            {
                case KolossType.Normal:
                    agent.speed = 4f;
                    break;
                case KolossType.Brute:
                    agent.speed = 3.5f;
                    break;
                case KolossType.Heavy:
                    agent.speed = 2.5f;
                    break;
                case KolossType.Berserker:
                    agent.speed = 5f;
                    break;
            }
            
            damage /= berserkerDamageMultiplier;
            
            animator.SetBool("Berserker", false);
            
            if (bodyRenderer != null && normalMaterial != null)
            {
                bodyRenderer.material = normalMaterial;
            }
            
            if (rageParticles != null)
            {
                rageParticles.Stop();
            }
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            float finalDamage = base.TakeDamage(damage, type);
            
            if (!isBerserker && currentHealth / maxHealth <= berserkerThreshold)
            {
                EnterBerserkerMode();
            }
            
            return finalDamage;
        }
        
        protected override void Death()
        {
            if (rageParticles != null)
            {
                rageParticles.Stop();
            }
            
            base.Death();
            
            DropSpike();
        }
        
        private void DropSpike()
        {
            if (Random.value < 0.2f)
            {
                GameObject spike = Resources.Load<GameObject>("Pickups/KolossSpike");
                if (spike != null)
                {
                    Instantiate(spike, transform.position + Vector3.up, Quaternion.identity);
                }
            }
        }
    }
}
