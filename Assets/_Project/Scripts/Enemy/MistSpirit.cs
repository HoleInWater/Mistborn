using UnityEngine;

namespace MistbornGame.Enemy
{
    public enum MistSpiritType
    {
        Normal,
        Phantom,
        Shade
    }
    
    public class MistSpirit : Enemy
    {
        [Header("Mist Spirit Specific")]
        [SerializeField] private MistSpiritType spiritType = MistSpiritType.Normal;
        [SerializeField] private float phaseSpeed = 8f;
        [SerializeField] private float normalSpeed = 3f;
        [SerializeField] private float phaseCooldown = 3f;
        [SerializeField] private float phaseDuration = 1.5f;
        
        [Header("Mist Abilities")]
        [SerializeField] private bool canUseAllomancy = true;
        [SerializeField] private bool usesPewter = true;
        [SerializeField] private bool usesTin = true;
        [SerializeField] private float mistFormRadius = 3f;
        
        [Header("Phasing")]
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material phaseMaterial;
        [SerializeField] private ParticleSystem mistParticles;
        
        [Header("Fear Effect")]
        [SerializeField] private float fearRadius = 10f;
        [SerializeField] private float fearDuration = 3f;
        [SerializeField] private float fearIntensity = 0.5f;
        
        private bool isPhasing = false;
        private bool isInMistForm = false;
        private float lastPhaseTime = -100f;
        private Renderer[] renderers;
        
        protected override void Start()
        {
            base.Start();
            
            switch (spiritType)
            {
                case MistSpiritType.Normal:
                    maxHealth = 80f;
                    damage = 20f;
                    attackRange = 3f;
                    detectionRange = 40f;
                    chaseSpeed = normalSpeed;
                    break;
                case MistSpiritType.Phantom:
                    maxHealth = 120f;
                    damage = 30f;
                    attackRange = 2.5f;
                    detectionRange = 50f;
                    chaseSpeed = phaseSpeed;
                    phaseCooldown = 2f;
                    break;
                case MistSpiritType.Shade:
                    maxHealth = 200f;
                    damage = 45f;
                    attackRange = 4f;
                    detectionRange = 60f;
                    chaseSpeed = phaseSpeed;
                    canCauseFear = true;
                    break;
            }
            
            currentHealth = maxHealth;
            
            renderers = GetComponentsInChildren<Renderer>();
            
            StartMistForm();
        }
        
        private void StartMistForm()
        {
            if (mistParticles != null)
            {
                mistParticles.Play();
            }
            
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.material = phaseMaterial;
                }
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (!isPhasing && Time.time - lastPhaseTime > phaseCooldown && state == EnemyState.Chase)
            {
                TryPhase();
            }
            
            UpdateFearEffect();
        }
        
        private void TryPhase()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer < detectionRange * 0.7f)
            {
                StartPhase();
            }
        }
        
        private void StartPhase()
        {
            isPhasing = true;
            lastPhaseTime = Time.time;
            
            agent.speed = phaseSpeed;
            
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.material = phaseMaterial;
                }
            }
            
            if (mistParticles != null)
            {
                mistParticles.Play();
            }
            
            animator.SetBool("Phasing", true);
            
            Invoke(nameof(EndPhase), phaseDuration);
        }
        
        private void EndPhase()
        {
            isPhasing = false;
            agent.speed = normalSpeed;
            
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.material = normalMaterial;
                }
            }
            
            animator.SetBool("Phasing", false);
        }
        
        private void UpdateFearEffect()
        {
            if (!canCauseFear) return;
            if (spiritType != MistSpiritType.Shade) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer < fearRadius && state == EnemyState.Chase)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.AddStatusEffect(StatusEffectType.Fear, fearDuration, fearIntensity);
                }
            }
        }
        
        protected override void Attack()
        {
            if (isPhasing)
            {
                PhaseAttack();
            }
            else
            {
                base.Attack();
            }
        }
        
        private void PhaseAttack()
        {
            Vector3 behindPlayer = player.position - player.forward * 2f;
            behindPlayer.y = transform.position.y;
            
            agent.SetDestination(behindPlayer);
            
            if (Vector3.Distance(transform.position, behindPlayer) < 1f)
            {
                DealDamage(damage * 1.5f);
                EndPhase();
            }
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            if (isPhasing)
            {
                damage *= 0.3f;
            }
            
            return base.TakeDamage(damage, type);
        }
        
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStats playerStats = other.GetComponent<PlayerStats>();
                if (playerStats != null && playerStats.IsInMist)
                {
                    if (usesPewter)
                    {
                        agent.speed = chaseSpeed * 0.7f;
                        Invoke(nameof(ResetSpeed), 2f);
                    }
                }
            }
        }
        
        private void ResetSpeed()
        {
            agent.speed = chaseSpeed;
        }
        
        protected override void Death()
        {
            if (mistParticles != null)
            {
                mistParticles.Stop();
            }
            
            base.Death();
            
            SpawnMistEssence();
        }
        
        private void SpawnMistEssence()
        {
            if (Random.value < 0.5f)
            {
                GameObject essence = Resources.Load<GameObject>("Pickups/MistEssence");
                if (essence != null)
                {
                    Instantiate(essence, transform.position, Quaternion.identity);
                }
            }
        }
    }
}
