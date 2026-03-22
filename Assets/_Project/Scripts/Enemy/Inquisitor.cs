using UnityEngine;

namespace MistbornGame.Enemy
{
    public class Inquisitor : Enemy
    {
        [Header("Inquisitor Specific")]
        [SerializeField] private float spikeDamageBonus = 0.5f;
        [SerializeField] private float healingPerHit = 5f;
        [SerializeField] private float[] spikePenetrationDamage = new float[] { 10f, 15f, 20f };
        
        [Header("Allomancy")]
        [SerializeField] private bool canUsePewter = true;
        [SerializeField] private bool canUseTin = true;
        [SerializeField] private float pewterSpeedBonus = 1.5f;
        [SerializeField] private float tinSenseRange = 50f;
        
        [Header("Spike Effects")]
        [SerializeField] private GameObject spikePrefab;
        [SerializeField] private Transform spikeHandL;
        [SerializeField] private Transform spikeHandR;
        [SerializeField] private GameObject spikeProjectile;
        
        [Header("Powers")]
        [SerializeField] private float steelPushForce = 30f;
        [SerializeField] private float ironPullForce = 25f;
        [SerializeField] private float metalSenseRange = 30f;
        [SerializeField] private float pushCooldown = 3f;
        [SerializeField] private float pullCooldown = 4f;
        
        [Header("Combat")]
        [SerializeField] private float heavyAttackDamage = 40f;
        [SerializeField] private float spikeThrowDamage = 25f;
        [SerializeField] private float spikeThrowRange = 20f;
        
        private bool isPewterBoosted = false;
        private bool isTinBoosted = false;
        private float lastPushTime = -100f;
        private float lastPullTime = -100f;
        private bool hasSpikes = true;
        
        protected override void Start()
        {
            base.Start();
            
            maxHealth = 600f;
            damage = 45f;
            attackRange = 4f;
            detectionRange = 30f;
            chaseSpeed = 7f;
            
            currentHealth = maxHealth;
            
            if (canUseTin)
            {
                StartCoroutine(ActivateTinSenses());
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (canUsePewter && ShouldBoostPewter())
            {
                ActivatePewterBoost();
            }
            else if (isPewterBoosted)
            {
                DeactivatePewterBoost();
            }
            
            if (state == EnemyState.Chase || state == EnemyState.Attack)
            {
                TryMetalBurning();
            }
        }
        
        private bool ShouldBoostPewter()
        {
            if (!canUsePewter) return false;
            
            float healthPercent = currentHealth / maxHealth;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            return healthPercent < 0.5f || distanceToPlayer > attackRange * 2;
        }
        
        private void ActivatePewterBoost()
        {
            if (isPewterBoosted) return;
            
            isPewterBoosted = true;
            agent.speed = chaseSpeed * pewterSpeedBonus;
            damage *= 1.3f;
            animator.SetBool("PewterBoost", true);
        }
        
        private void DeactivatePewterBoost()
        {
            if (!isPewterBoosted) return;
            
            isPewterBoosted = false;
            agent.speed = chaseSpeed;
            damage /= 1.3f;
            animator.SetBool("PewterBoost", false);
        }
        
        private System.Collections.IEnumerator ActivateTinSenses()
        {
            while (true)
            {
                if (isTinBoosted || state != EnemyState.Idle)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                
                isTinBoosted = true;
                detectionRange = tinSenseRange;
                yield return new WaitForSeconds(5f);
                isTinBoosted = false;
                detectionRange = 30f;
            }
        }
        
        private void TryMetalBurning()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer < metalSenseRange && CanDetectMetalPlayer())
            {
                if (Time.time - lastPushTime > pushCooldown && distanceToPlayer > attackRange)
                {
                    TrySteelPush();
                }
                else if (Time.time - lastPullTime > pullCooldown && distanceToPlayer > attackRange * 1.5f)
                {
                    TryIronPull();
                }
            }
        }
        
        private bool CanDetectMetalPlayer()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, metalSenseRange);
            foreach (var col in nearbyColliders)
            {
                if (col.CompareTag("Player"))
                {
                    Player.PlayerStats stats = col.GetComponent<Player.PlayerStats>();
                    if (stats != null && stats.HasMetalEquipped())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        private void TrySteelPush()
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, metalSenseRange))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    ExecuteSteelPush(direction);
                }
            }
        }
        
        private void ExecuteSteelPush(Vector3 direction)
        {
            lastPushTime = Time.time;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * steelPushForce * 100f, ForceMode.Impulse);
            }
            
            player.GetComponent<PlayerStats>()?.TakeDamage(10f, DamageType.Allomantic);
            
            animator.SetTrigger("SteelPush");
            
            SpawnPushVisual(direction);
        }
        
        private void TryIronPull()
        {
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0;
            
            RaycastHit hit;
            if (Physics.Raycast(player.position + Vector3.up, direction, out hit, metalSenseRange))
            {
                if (hit.transform == transform)
                {
                    ExecuteIronPull(direction);
                }
            }
        }
        
        private void ExecuteIronPull(Vector3 direction)
        {
            lastPullTime = Time.time;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * ironPullForce * 100f, ForceMode.Impulse);
            }
            
            animator.SetTrigger("IronPull");
            
            SpawnPullVisual(direction);
        }
        
        private void SpawnPushVisual(Vector3 direction)
        {
            GameObject vfx = Resources.Load<GameObject>("Effects/SteelPushVFX");
            if (vfx != null)
            {
                GameObject instance = Instantiate(vfx, transform.position + Vector3.up, Quaternion.LookRotation(direction));
                Destroy(instance, 2f);
            }
        }
        
        private void SpawnPullVisual(Vector3 direction)
        {
            GameObject vfx = Resources.Load<GameObject>("Effects/IronPullVFX");
            if (vfx != null)
            {
                GameObject instance = Instantiate(vfx, transform.position + Vector3.up, Quaternion.LookRotation(direction));
                Destroy(instance, 2f);
            }
        }
        
        protected override void Attack()
        {
            base.Attack();
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer < spikeThrowRange && hasSpikes && Random.value < 0.3f)
            {
                ThrowSpike();
            }
            else
            {
                HeavyMeleeAttack();
            }
        }
        
        private void ThrowSpike()
        {
            hasSpikes = false;
            
            Vector3 direction = (player.position - transform.position).normalized;
            
            GameObject spike = Instantiate(spikeProjectile, transform.position + Vector3.up, Quaternion.LookRotation(direction));
            
            spike.GetComponent<Rigidbody>().velocity = direction * 25f;
            spike.GetComponent<Projectile>()?.Initialize(spikeThrowDamage, 15f, new[] { "Player" });
            
            Destroy(spike, 3f);
            
            animator.SetTrigger("SpikeThrow");
            
            Invoke(nameof(ResetSpikes), 5f);
        }
        
        private void ResetSpikes()
        {
            hasSpikes = true;
        }
        
        private void HeavyMeleeAttack()
        {
            animator.SetTrigger("HeavyAttack");
            DealDamage(heavyAttackDamage);
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            float finalDamage = damage;
            
            if (type == DamageType.Allomantic)
            {
                finalDamage *= (1f + spikeDamageBonus);
            }
            
            float healedAmount = finalDamage * (healingPerHit / 100f);
            currentHealth = Mathf.Min(currentHealth + healedAmount, maxHealth);
            
            return base.TakeDamage(finalDamage, type);
        }
        
        protected override void Death()
        {
            animator.SetTrigger("Death");
            
            DropSpikes();
            
            base.Death();
        }
        
        private void DropSpikes()
        {
            GameObject spike = Resources.Load<GameObject>("Pickups/InquisitorSpike");
            if (spike != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 2f;
                    offset.y = 1f;
                    Instantiate(spike, transform.position + offset, Quaternion.identity);
                }
            }
        }
    }
}
