using UnityEngine;

namespace MistbornGame.Enemy
{
    public enum ThugType
    {
        StreetThug,
        HouseGuard,
        NobleGuard,
        Bouncer
    }
    
    public class Thug : Enemy
    {
        [Header("Thug Specific")]
        [SerializeField] private ThugType thugType = ThugType.StreetThug;
        [SerializeField] private float pewterBoostMultiplier = 1.3f;
        [SerializeField] private float pewterSpeedBoost = 1.5f;
        [SerializeField] private float pewterDamageBoost = 1.4f;
        [SerializeField] private float pewterToughnessBoost = 1.5f;
        
        [Header("Combat")]
        [SerializeField] private float comboChance = 0.4f;
        [SerializeField] private int maxComboHits = 3;
        [SerializeField] private float comboDelay = 0.3f;
        [SerializeField] private float heavyAttackChance = 0.2f;
        
        [Header("Weapon")]
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Transform weaponHand;
        [SerializeField] private float throwRange = 15f;
        [SerializeField] private float throwDamage = 20f;
        
        [Header("Tactics")]
        [SerializeField] private bool usesFlanking = true;
        [SerializeField] private float flankingDistance = 5f;
        [SerializeField] private bool canCallBackup = true;
        [SerializeField] private float backupCallRange = 20f;
        [SerializeField] private float backupCooldown = 30f;
        
        private bool isPewterBoosted = false;
        private int currentComboCount = 0;
        private float lastComboTime = 0f;
        private float lastBackupCall = -100f;
        private bool hasWeapon = true;
        private Vector3 flankingPosition;
        
        protected override void Start()
        {
            base.Start();
            
            switch (thugType)
            {
                case ThugType.StreetThug:
                    maxHealth = 60f;
                    damage = 12f;
                    attackRange = 2.5f;
                    detectionRange = 15f;
                    chaseSpeed = 3.5f;
                    break;
                case ThugType.HouseGuard:
                    maxHealth = 120f;
                    damage = 18f;
                    attackRange = 3f;
                    detectionRange = 20f;
                    chaseSpeed = 3.5f;
                    usesFlanking = true;
                    break;
                case ThugType.NobleGuard:
                    maxHealth = 180f;
                    damage = 25f;
                    attackRange = 3.5f;
                    detectionRange = 25f;
                    chaseSpeed = 4f;
                    usesFlanking = true;
                    canCallBackup = true;
                    break;
                case ThugType.Bouncer:
                    maxHealth = 250f;
                    damage = 35f;
                    attackRange = 4f;
                    detectionRange = 15f;
                    chaseSpeed = 2.5f;
                    heavyAttackChance = 0.5f;
                    pewterBoostMultiplier = 1.5f;
                    break;
            }
            
            currentHealth = maxHealth;
            
            if (weaponPrefab != null && weaponHand != null)
            {
                EquipWeapon();
            }
        }
        
        protected override void Update()
        {
            if (!isPewterBoosted && ShouldActivatePewter())
            {
                ActivatePewter();
            }
            
            base.Update();
            
            if (state == EnemyState.Chase && usesFlanking)
            {
                UpdateFlanking();
            }
            
            if (canCallBackup && Time.time - lastBackupCall > backupCooldown)
            {
                TryCallBackup();
            }
        }
        
        private bool ShouldActivatePewter()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float healthPercent = currentHealth / maxHealth;
            
            return distanceToPlayer < attackRange * 2 || healthPercent < 0.5f;
        }
        
        private void ActivatePewter()
        {
            isPewterBoosted = true;
            
            damage *= pewterDamageBoost;
            agent.speed *= pewterSpeedBoost;
            attackRange *= pewterToughnessBoost;
            
            animator.SetBool("PewterBoost", true);
            
            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().mass *= pewterToughnessBoost;
            }
        }
        
        private void UpdateFlanking()
        {
            if (Vector3.Distance(transform.position, flankingPosition) < 1f)
            {
                CalculateFlankingPosition();
            }
        }
        
        private void CalculateFlankingPosition()
        {
            Vector3 toPlayer = (player.position - transform.position).normalized;
            Vector3 perpendicular = Vector3.Cross(Vector3.up, toPlayer);
            
            float direction = Random.value < 0.5f ? 1f : -1f;
            flankingPosition = player.position + perpendicular * flankingDistance * direction;
            flankingPosition.y = transform.position.y;
            
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(flankingPosition, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(flankingPosition);
            }
        }
        
        protected override void Attack()
        {
            if (Time.time - lastComboTime > comboDelay)
            {
                currentComboCount = 0;
            }
            
            if (Random.value < heavyAttackChance && !isPewterBoosted)
            {
                HeavyAttack();
                return;
            }
            
            if (Random.value < comboChance && currentComboCount < maxComboHits)
            {
                ComboAttack();
            }
            else
            {
                BasicAttack();
            }
            
            if (!hasWeapon && Random.value < 0.1f)
            {
                TryThrowWeapon();
            }
        }
        
        private void BasicAttack()
        {
            animator.SetTrigger("Attack");
            DealDamage(damage);
        }
        
        private void ComboAttack()
        {
            currentComboCount++;
            lastComboTime = Time.time;
            
            animator.SetInteger("ComboCount", currentComboCount);
            animator.SetTrigger("ComboAttack");
            
            float comboDamage = damage * (1f + currentComboCount * 0.2f);
            DealDamage(comboDamage);
        }
        
        private void HeavyAttack()
        {
            animator.SetTrigger("HeavyAttack");
            DealDamage(damage * 2f);
            
            Vector3 knockbackDirection = (player.position - transform.position).normalized;
            knockbackDirection.y = 0.3f;
            
            player.GetComponent<Rigidbody>()?.AddForce(knockbackDirection * 15f * 100f, ForceMode.Impulse);
        }
        
        private void EquipWeapon()
        {
            if (weaponPrefab != null && weaponHand != null)
            {
                GameObject weapon = Instantiate(weaponPrefab, weaponHand);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
            }
        }
        
        private void TryThrowWeapon()
        {
            if (!hasWeapon) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > throwRange) return;
            
            hasWeapon = false;
            
            Vector3 direction = (player.position - transform.position).normalized;
            
            GameObject thrownWeapon = Instantiate(weaponPrefab, transform.position + Vector3.up, Quaternion.LookRotation(direction));
            
            Rigidbody rb = thrownWeapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = direction * 20f;
                rb.angularVelocity = Random.insideUnitSphere * 10f;
            }
            
            thrownWeapon.GetComponent<Projectile>()?.Initialize(throwDamage, 5f, new[] { "Player" });
            
            Destroy(thrownWeapon, 3f);
            
            animator.SetTrigger("ThrowWeapon");
            
            Invoke(nameof(ResetWeapon), 10f);
        }
        
        private void ResetWeapon()
        {
            hasWeapon = true;
            EquipWeapon();
        }
        
        private void TryCallBackup()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > backupCallRange) return;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, backupCallRange);
            int allyCount = 0;
            
            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.GetComponent<Thug>() != null && enemy.gameObject != gameObject)
                {
                    allyCount++;
                }
            }
            
            if (allyCount < 2)
            {
                CallForBackup();
            }
        }
        
        private void CallForBackup()
        {
            lastBackupCall = Time.time;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, backupCallRange);
            
            foreach (var enemy in nearbyEnemies)
            {
                Thug ally = enemy.GetComponent<Thug>();
                if (ally != null && ally != this)
                {
                    ally.GetComponent<UnityEngine.AI.NavMeshAgent>()?.SetDestination(player.position);
                }
            }
            
            animator.SetTrigger("CallBackup");
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            float finalDamage = base.TakeDamage(damage, type);
            
            if (isPewterBoosted)
            {
                finalDamage *= 0.7f;
            }
            
            return finalDamage;
        }
        
        protected override void Death()
        {
            base.Death();
            
            if (!hasWeapon)
            {
                GameObject droppedWeapon = Instantiate(weaponPrefab, transform.position + Vector3.up, Quaternion.identity);
                droppedWeapon.GetComponent<Rigidbody>()?.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }
        }
    }
}
