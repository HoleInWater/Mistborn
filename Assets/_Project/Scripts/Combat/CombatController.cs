using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class CombatController : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float criticalHitMultiplier = 2f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 1f;
        
        [Header("Combo System")]
        [SerializeField] private int maxComboCount = 5;
        [SerializeField] private float comboWindow = 1f;
        [SerializeField] private float comboDamageIncrease = 0.2f;
        
        [Header("Blocking")]
        [SerializeField] private float blockDamageReduction = 0.5f;
        [SerializeField] private float perfectBlockWindow = 0.2f;
        
        [Header("References")]
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private ParticleSystem attackEffect;
        
        private int currentComboCount = 0;
        private float lastAttackTime = 0f;
        private float lastComboTime = 0f;
        private bool isAttacking = false;
        private bool isBlocking = false;
        private PlayerStats playerStats;
        private BasicPlayerMove playerMove;
        
        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerMove = GetComponent<BasicPlayerMove>();
        }
        
        private void Start()
        {
            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<Animator>();
            }
        }
        
        private void Update()
        {
            UpdateCombo();
            HandleBlocking();
            HandleAttackInput();
        }
        
        private void HandleAttackInput()
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking && !isBlocking)
            {
                PerformAttack();
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                StartBlocking();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                StopBlocking();
            }
        }
        
        private void HandleBlocking()
        {
            if (playerMove != null && isBlocking)
            {
                playerMove.SetCanMove(false);
            }
        }
        
        public void PerformAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
            {
                return;
            }
            
            isAttacking = true;
            lastAttackTime = Time.time;
            
            float damage = CalculateDamage();
            
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Attack");
                playerAnimator.SetInteger("ComboCount", currentComboCount);
            }
            
            Vector3 attackDirection = transform.forward;
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, attackDirection, attackRange);
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    DealDamage(hit.collider.gameObject, damage);
                }
            }
            
            if (attackEffect != null)
            {
                attackEffect.Play();
            }
            
            Invoke(nameof(ResetAttack), 0.5f);
        }
        
        private float CalculateDamage()
        {
            float damage = baseDamage;
            
            if (playerStats != null)
            {
                damage *= playerStats.GetStatValue(StatType.AttackPower);
            }
            
            float comboBonus = 1f + (currentComboCount * comboDamageIncrease);
            damage *= comboBonus;
            
            if (playerStats != null && Random.value < playerStats.GetStatValue(StatType.CriticalHitChance))
            {
                damage *= criticalHitMultiplier;
                
                if (OnCriticalHit != null)
                {
                    OnCriticalHit();
                }
            }
            
            return damage;
        }
        
        private void DealDamage(GameObject target, float damage)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (isBlocking)
                {
                    damage *= blockDamageReduction;
                }
                
                enemy.TakeDamage(damage);
                
                if (OnDamageDealt != null)
                {
                    OnDamageDealt(target, damage);
                }
            }
        }
        
        private void UpdateCombo()
        {
            if (currentComboCount > 0 && Time.time - lastComboTime > comboWindow)
            {
                ResetCombo();
            }
        }
        
        private void IncrementCombo()
        {
            currentComboCount++;
            lastComboTime = Time.time;
            
            if (currentComboCount > maxComboCount)
            {
                currentComboCount = 1;
            }
            
            if (OnComboIncremented != null)
            {
                OnComboIncremented(currentComboCount);
            }
        }
        
        private void ResetCombo()
        {
            currentComboCount = 0;
            
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("ComboCount", 0);
            }
        }
        
        private void ResetAttack()
        {
            isAttacking = false;
            IncrementCombo();
        }
        
        public void StartBlocking()
        {
            isBlocking = true;
            
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsBlocking", true);
            }
        }
        
        public void StopBlocking()
        {
            isBlocking = false;
            
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsBlocking", false);
            }
            
            if (playerMove != null)
            {
                playerMove.SetCanMove(true);
            }
        }
        
        public bool PerfectBlockTiming(float incomingAttackTime)
        {
            float timeSinceBlockStart = Time.time - incomingAttackTime;
            return timeSinceBlockStart < perfectBlockWindow;
        }
        
        public bool IsAttacking()
        {
            return isAttacking;
        }
        
        public bool IsBlocking()
        {
            return isBlocking;
        }
        
        public int GetCurrentComboCount()
        {
            return currentComboCount;
        }
        
        public float GetCurrentDamage()
        {
            return CalculateDamage();
        }
        
        public void SetAttackCooldown(float cooldown)
        {
            attackCooldown = cooldown;
        }
        
        public void SetAttackRange(float range)
        {
            attackRange = range;
        }
        
        public void SetBaseDamage(float damage)
        {
            baseDamage = damage;
        }
        
        public event System.Action OnAttackPerformed;
        public event System.Action<GameObject, float> OnDamageDealt;
        public event System.Action OnCriticalHit;
        public event System.Action<int> OnComboIncremented;
    }
}
