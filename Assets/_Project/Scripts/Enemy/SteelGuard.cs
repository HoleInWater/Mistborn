using UnityEngine;
using UnityEngine.AI;

namespace MistbornGame.Enemy
{
    public enum SteelGuardType
    {
        Basic,
        Elite,
        Commander,
        Inquisitor
    }
    
    public class SteelGuard : Enemy
    {
        [Header("Steel Guard Specific")]
        [SerializeField] private SteelGuardType guardType = SteelGuardType.Basic;
        [SerializeField] private float pushForce = 15f;
        [SerializeField] private float pushRange = 10f;
        [SerializeField] private float coinDamage = 10f;
        [SerializeField] private GameObject coinProjectile;
        [SerializeField] private Transform coinSpawnPoint;
        
        [Header("Guard Abilities")]
        [SerializeField] private float shieldBlockChance = 0.3f;
        [SerializeField] private float shieldBlockReduction = 0.5f;
        [SerializeField] private float chargeAttackCooldown = 5f;
        [SerializeField] private float chargeDamage = 25f;
        [SerializeField] private float chargeSpeed = 15f;
        
        [Header("Formation")]
        [SerializeField] private bool useFormation = true;
        [SerializeField] private float formationSpacing = 2f;
        [SerializeField] private Transform formationLeader;
        
        private bool isCharging = false;
        private bool isBlocking = false;
        private float lastChargeTime = -100f;
        private Vector3 chargeTarget;
        
        protected override void Start()
        {
            base.Start();
            
            switch (guardType)
            {
                case SteelGuardType.Basic:
                    maxHealth = 100f;
                    damage = 15f;
                    attackRange = 3f;
                    break;
                case SteelGuardType.Elite:
                    maxHealth = 200f;
                    damage = 25f;
                    attackRange = 3.5f;
                    shieldBlockChance = 0.5f;
                    break;
                case SteelGuardType.Commander:
                    maxHealth = 350f;
                    damage = 35f;
                    attackRange = 4f;
                    shieldBlockChance = 0.6f;
                    chargeAttackCooldown = 4f;
                    break;
                case SteelGuardType.Inquisitor:
                    maxHealth = 500f;
                    damage = 50f;
                    attackRange = 5f;
                    shieldBlockChance = 0.7f;
                    pushForce = 25f;
                    coinDamage = 25f;
                    break;
            }
            
            currentHealth = maxHealth;
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isCharging)
            {
                UpdateChargeAttack();
            }
            else if (state == EnemyState.Chase)
            {
                MaintainFormation();
                TryChargeAttack();
            }
        }
        
        protected override void Attack()
        {
            if (isCharging) return;
            
            base.Attack();
            
            if (CanThrowCoins())
            {
                ThrowCoin();
            }
            
            MeleeAttack();
        }
        
        private bool CanThrowCoins()
        {
            if (guardType == SteelGuardType.Basic) return false;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            return distanceToPlayer > attackRange && distanceToPlayer < pushRange && !isCharging;
        }
        
        private void ThrowCoin()
        {
            if (coinProjectile == null || coinSpawnPoint == null) return;
            
            Vector3 direction = (player.position - coinSpawnPoint.position).normalized;
            GameObject coin = Instantiate(coinProjectile, coinSpawnPoint.position, Quaternion.LookRotation(direction));
            
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                coinRb.velocity = direction * pushForce;
            }
            
            coin.GetComponent<Projectile>()?.Initialize(damage: coinDamage, knockback: pushForce * 0.5f, targetTags: new[] { "Player" });
            
            Destroy(coin, 3f);
        }
        
        private void TryChargeAttack()
        {
            if (guardType == SteelGuardType.Basic) return;
            if (Time.time - lastChargeTime < chargeAttackCooldown) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange * 2) return;
            
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, attackRange * 3, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != player) return;
            }
            
            StartChargeAttack();
        }
        
        private void StartChargeAttack()
        {
            isCharging = true;
            chargeTarget = player.position;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            
            animator.SetTrigger("ChargeStart");
        }
        
        private void UpdateChargeAttack()
        {
            Vector3 direction = (chargeTarget - transform.position).normalized;
            direction.y = 0;
            
            transform.position += direction * chargeSpeed * Time.deltaTime;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);
            
            if (Vector3.Distance(transform.position, chargeTarget) < 1f)
            {
                EndChargeAttack();
            }
        }
        
        private void EndChargeAttack()
        {
            isCharging = false;
            agent.isStopped = false;
            
            animator.SetTrigger("ChargeEnd");
            
            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                DealDamage(chargeDamage);
            }
            
            lastChargeTime = Time.time;
        }
        
        private void MaintainFormation()
        {
            if (!useFormation || formationLeader == null) return;
            
            Vector3 targetPosition = formationLeader.position - transform.right * formationSpacing;
            
            if (Vector3.Distance(transform.position, targetPosition) > formationSpacing * 0.5f)
            {
                agent.SetDestination(targetPosition);
            }
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            if (isBlocking)
            {
                damage *= (1f - shieldBlockReduction);
                animator.SetTrigger("Blocked");
                return damage;
            }
            
            float finalDamage = base.TakeDamage(damage, type);
            
            if (Random.value < shieldBlockChance && !isCharging)
            {
                StartCoroutine(ShieldBlockCoroutine());
            }
            
            return finalDamage;
        }
        
        private System.Collections.IEnumerator ShieldBlockCoroutine()
        {
            isBlocking = true;
            animator.SetBool("ShieldBlock", true);
            yield return new WaitForSeconds(0.5f);
            isBlocking = false;
            animator.SetBool("ShieldBlock", false);
        }
        
        protected override void Death()
        {
            base.Death();
            
            if (guardType == SteelGuardType.Inquisitor)
            {
                SpawnLoot();
            }
        }
        
        private void SpawnLoot()
        {
            float randomValue = Random.value;
            if (randomValue < 0.3f)
            {
                Instantiate(Resources.Load("Pickups/MetalVial"), transform.position, Quaternion.identity);
            }
        }
    }
}
