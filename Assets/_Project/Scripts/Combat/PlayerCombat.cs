// ============================================================
// FILE: PewterarmController.cs
// SYSTEM: Combat / Player
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Base class for player combat abilities.
//   Handles melee attacks, damage, and combat state.
//
// TODO:
//   - Add animation hooks
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        public float baseDamage = 20f;
        public float attackRange = 2f;
        public float attackCooldown = 0.5f;
        public float attackAngle = 60f;
        
        [Header("References")]
        public PewterEnhancement pewterEnhancement;
        public Transform attackPoint;
        
        [Header("Layers")]
        public LayerMask enemyLayers;
        
        private float lastAttackTime;
        private bool isAttacking;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
            }
        }
        
        public void Attack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            
            lastAttackTime = Time.time;
            isAttacking = true;
            
            // Apply pewter damage bonus
            float damage = baseDamage;
            if (pewterEnhancement != null && pewterEnhancement.IsEnhanced())
            {
                damage *= pewterEnhancement.GetDamageMultiplier();
            }
            
            // Detect enemies in range
            Collider[] hitEnemies = Physics.OverlapSphere(
                attackPoint != null ? attackPoint.position : transform.position,
                attackRange,
                enemyLayers
            );
            
            foreach (Collider enemy in hitEnemies)
            {
                // Check if in front of player
                Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);
                
                if (angle <= attackAngle)
                {
                    DamageEnemy(enemy.gameObject, damage);
                }
            }
            
            isAttacking = false;
            
            // TODO: Play attack animation
        }
        
        private void DamageEnemy(GameObject enemy, float damage)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.TakeDamage(damage);
            }
            else
            {
                Debug.Log($"Hit {enemy.name} for {damage} damage");
            }
            
            // TODO: Play hit effect
            // TODO: Knockback
        }
        
        public bool IsAttacking()
        {
            return isAttacking;
        }
    }
}
