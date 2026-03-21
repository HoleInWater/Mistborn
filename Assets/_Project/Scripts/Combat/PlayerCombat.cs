using UnityEngine;

namespace Mistborn.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float m_damage = 20f;
        [SerializeField] private float m_range = 2f;
        [SerializeField] private float m_cooldown = 0.5f;
        [SerializeField] private LayerMask m_enemyLayers;

        [Header("References")]
        [SerializeField] private Transform m_attackPoint;

        private float m_lastAttackTime;
        private bool m_isAttacking;

        public bool isAttacking => m_isAttacking;

        public event System.Action OnAttack;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                TryAttack();
        }

        public void TryAttack()
        {
            if (Time.time - m_lastAttackTime < m_cooldown) return;
            
            m_lastAttackTime = Time.time;
            m_isAttacking = true;

            float damage = GetDamage();
            Vector3 origin = m_attackPoint != null ? m_attackPoint.position : transform.position;

            foreach (Collider hit in Physics.OverlapSphere(origin, m_range, m_enemyLayers))
            {
                if (hit.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(new DamageData(damage, DamageType.Standard, gameObject));
                }
            }

            m_isAttacking = false;
            OnAttack?.Invoke();
        }

        private float GetDamage()
        {
            float dmg = m_damage;
            GetComponent<PewterEnhancement>()?.TryGetComponent(out PewterEnhancement pewter);
            if (pewter != null && pewter.isEnhanced)
                dmg *= pewter.GetDamageMultiplier();
            return dmg;
        }
    }
}
