// ============================================================
// FILE: EnemyGuard.cs
// SYSTEM: Combat
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Basic soldier enemy. Metal armor can be pushed.
//   No Allomancy, just melee combat.
//
// TODO:
//   - Add patrol waypoints
//   - Add animation hooks
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class EnemyGuard : EnemyBase
    {
        [Header("Guard Specific")]
        public float meleeDamage = 15f;
        public float meleeRange = 2.5f;
        public float detectionAngle = 90f;
        public float hearingRange = 10f;
        
        [Header("Patrol")]
        public bool shouldPatrol = true;
        public float patrolWaitTime = 2f;
        public Vector3[] patrolPoints;
        
        private int currentPatrolIndex;
        private float patrolWaitTimer;
        private bool isWaitingAtPoint;
        
        protected override void Start()
        {
            base.Start();
            currentPatrolIndex = 0;
        }
        
        protected override void UpdateAI()
        {
            if (player == null)
            {
                currentState = AIState.Idle;
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            bool canSeePlayer = CanSeePlayer(distanceToPlayer);
            
            if (distanceToPlayer <= attackRange && canSeePlayer)
            {
                currentState = AIState.Attack;
                Attack();
            }
            else if (canSeePlayer || distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chase;
                ChasePlayer();
            }
            else if (shouldPatrol)
            {
                currentState = AIState.Patrol;
                Patrol();
            }
            else
            {
                currentState = AIState.Idle;
            }
        }
        
        private bool CanSeePlayer(float distance)
        {
            if (player == null) return false;
            
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Vector3 forward = transform.forward;
            
            float angle = Vector3.Angle(forward, directionToPlayer);
            if (angle > detectionAngle) return false;
            
            // Check line of sight
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distance))
            {
                if (hit.transform != player)
                {
                    return false; // Something blocking view
                }
            }
            
            return true;
        }
        
        protected override void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            
            if (isWaitingAtPoint)
            {
                patrolWaitTimer -= Time.deltaTime;
                if (patrolWaitTimer <= 0)
                {
                    isWaitingAtPoint = false;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                return;
            }
            
            Vector3 target = patrolPoints[currentPatrolIndex];
            float distance = Vector3.Distance(transform.position, target);
            
            if (distance < 1f)
            {
                isWaitingAtPoint = true;
                patrolWaitTimer = patrolWaitTime;
                return;
            }
            
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * patrolSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
        }
        
        protected override void OnAttack()
        {
            if (player == null) return;
            
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= meleeRange)
            {
                // TODO: Call player.TakeDamage(meleeDamage)
                Debug.Log($"Guard attacking for {meleeDamage} damage");
            }
        }
        
        protected override void OnDeath()
        {
            // Drop loot? Increase score?
            base.OnDeath();
        }
    }
}
