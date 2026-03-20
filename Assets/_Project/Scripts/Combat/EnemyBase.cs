// ============================================================
// FILE: EnemyBase.cs
// SYSTEM: Combat
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Base class for all enemies. Handles health, damage, and basic AI state.
//
// TODO:
//   - Connect to actual combat system
//   - Add animation hooks
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class EnemyBase : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float currentHealth;
        
        [Header("Combat")]
        public float attackDamage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        
        [Header("AI State")]
        public float detectionRange = 15f;
        public float chaseSpeed = 4f;
        public float patrolSpeed = 2f;
        
        protected Transform player;
        protected Vector3 startPosition;
        protected AIState currentState = AIState.Patrol;
        protected float lastAttackTime;
        
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }
        
        protected virtual void Start()
        {
            currentHealth = maxHealth;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            startPosition = transform.position;
        }
        
        protected virtual void Update()
        {
            if (currentHealth <= 0)
            {
                currentState = AIState.Dead;
                OnDeath();
                return;
            }
            
            UpdateAI();
        }
        
        protected virtual void UpdateAI()
        {
            float distanceToPlayer = player != null 
                ? Vector3.Distance(transform.position, player.position) 
                : Mathf.Infinity;
            
            if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attack;
                Attack();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chase;
                ChasePlayer();
            }
            else
            {
                currentState = AIState.Patrol;
                Patrol();
            }
        }
        
        protected virtual void ChasePlayer()
        {
            if (player == null) return;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * chaseSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        
        protected virtual void Patrol()
        {
            // Simple patrol: walk to start, walk back
            Vector3 target = startPosition;
            float distance = Vector3.Distance(transform.position, target);
            
            if (distance < 1f)
            {
                startPosition = transform.position + Vector3.forward * 5f;
            }
            
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * patrolSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
        }
        
        protected virtual void Attack()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                OnAttack();
            }
        }
        
        protected virtual void OnAttack()
        {
            // Deal damage to player if in range
            if (player == null) return;
            
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                // TODO: Call player.TakeDamage(attackDamage)
                Debug.Log($"Enemy attacking for {attackDamage} damage");
            }
        }
        
        protected virtual void OnDeath()
        {
            // TODO: Play death animation, drop loot, etc.
            Debug.Log("Enemy died");
            Destroy(gameObject, 2f);
        }
        
        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            Debug.Log($"Enemy took {damage} damage, {currentHealth}/{maxHealth} HP remaining");
            
            if (currentHealth <= 0)
            {
                currentState = AIState.Dead;
                OnDeath();
            }
        }
    }
}
