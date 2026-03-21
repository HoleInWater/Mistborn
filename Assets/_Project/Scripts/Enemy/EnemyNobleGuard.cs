using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyNobleGuard : EnemyBase
    {
        [Header("Armor")]
        [SerializeField] private float m_armorRating = 0.4f;
        [SerializeField] private float m_blockChance = 0.3f;
        [SerializeField] private float m_blockCooldown = 2f;
        [SerializeField] private float m_parryWindow = 0.5f;

        [Header("Shield")]
        [SerializeField] private bool m_hasShield = true;
        [SerializeField] private float m_shieldBlockAngle = 90f;
        [SerializeField] private float m_shieldUpDuration = 3f;
        [SerializeField] private float m_shieldCooldown = 8f;

        [Header("Combat")]
        [SerializeField] private float m_counterDamage = 20f;
        [SerializeField] private float m_counterWindow = 1f;
        [SerializeField] private float m_shieldBashDamage = 15f;

        private float m_blockTimer;
        private float m_lastBlockTime;
        private bool m_isBlocking;
        private bool m_shieldUp;
        private float m_shieldTimer;
        private Vector3 m_blockDirection;
        private Animator m_animator;

        protected override void Awake()
        {
            base.Awake();
            m_animator = GetComponent<Animator>();
            m_maxHealth = 120f;
            m_currentHealth = m_maxHealth;
            m_attackRange = 3f;
            m_detectionRange = 18f;
            m_speed = 3.5f;
        }

        protected override void Update()
        {
            if (isDead) return;

            UpdateTimers();
            UpdateShieldState();

            if (m_isBlocking)
            {
                CheckBlockSuccess();
            }

            base.Update();
        }

        private void UpdateTimers()
        {
            m_blockTimer -= Time.deltaTime;

            if (m_blockTimer <= 0 && m_isBlocking)
            {
                EndBlock();
            }
        }

        private void UpdateShieldState()
        {
            if (!m_hasShield) return;

            m_shieldTimer -= Time.deltaTime;

            if (m_shieldTimer <= 0 && m_shieldUp)
            {
                m_shieldUp = false;
                m_shieldTimer = m_shieldCooldown;
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (m_shieldTimer <= 0 && !m_shieldUp && dist < 5f)
            {
                RaiseShield();
            }
            else if (dist <= m_attackRange)
            {
                SetState(State.Attack);
            }
            else if (dist <= m_detectionRange)
            {
                SetState(State.Chase);
            }
            else
            {
                SetState(State.Patrol);
            }

            ExecuteStateInternal();
        }

        private void ExecuteStateInternal()
        {
            switch (m_state)
            {
                case State.Chase:
                    MoveTo(m_player.position);
                    break;
                case State.Attack:
                    if (m_shieldUp)
                        TryShieldBash();
                    else
                        TryAttack();
                    break;
                case State.Patrol:
                    Patrol();
                    break;
            }
        }

        private void RaiseShield()
        {
            m_shieldUp = true;
            m_shieldTimer = m_shieldUpDuration;
            m_isBlocking = true;
            m_blockTimer = m_shieldUpDuration;
            m_blockDirection = transform.forward;

            if (m_animator != null)
                m_animator.SetTrigger("ShieldUp");
        }

        private void CheckBlockSuccess()
        {
            if (m_player == null) return;

            Vector3 toPlayer = (m_player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toPlayer);

            if (angle < m_shieldBlockAngle)
            {
                Vector3 attackDir = m_player.forward;
                float dot = Vector3.Dot(attackDir, transform.forward);

                if (dot < -0.3f)
                {
                    m_lastBlockTime = Time.time;
                }
            }
        }

        private void TryShieldBash()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist < 2f)
            {
                ShieldBash();
            }
        }

        private void ShieldBash()
        {
            if (m_player == null) return;

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(new DamageData(m_shieldBashDamage));

                Rigidbody rb = m_player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockback = (m_player.position - transform.position).normalized;
                    knockback.y = 0.3f;
                    rb.AddForce(knockback * 8f, ForceMode.Impulse);
                }
            }

            m_shieldUp = false;
            m_shieldTimer = m_shieldCooldown;
        }

        private void EndBlock()
        {
            m_isBlocking = false;
            m_blockTimer = m_blockCooldown;
        }

        protected override void Attack()
        {
            if (m_player == null) return;

            if (Time.time - m_lastBlockTime < m_counterWindow)
            {
                CounterAttack();
                return;
            }

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(new DamageData(m_damage));
            }
        }

        private void CounterAttack()
        {
            if (m_player == null) return;

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(new DamageData(m_counterDamage));
            }

            if (m_animator != null)
                m_animator.SetTrigger("Counter");
        }

        public override void TakeDamage(DamageData damage)
        {
            if (m_isBlocking)
            {
                Vector3 damageDir = Vector3.zero;
                if (damage.source != null)
                    damageDir = (damage.source.transform.position - transform.position).normalized;

                float angle = Vector3.Angle(transform.forward, damageDir);

                if (angle < m_shieldBlockAngle)
                {
                    if (Random.value < m_blockChance)
                    {
                        damage.amount = 0;
                        PlayBlockEffect();
                        return;
                    }

                    damage.amount *= (1f - m_armorRating);
                }
            }
            else
            {
                damage.amount *= (1f - m_armorRating * 0.5f);
            }

            base.TakeDamage(damage);
        }

        private void PlayBlockEffect()
        {
            if (m_animator != null)
                m_animator.SetTrigger("BlockHit");

            m_lastBlockTime = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_hasShield)
            {
                Gizmos.color = Color.blue;
                Vector3 dir = transform.forward * 3f;
                Vector3 right = Quaternion.Euler(0, m_shieldBlockAngle * 0.5f, 0) * dir;
                Vector3 left = Quaternion.Euler(0, -m_shieldBlockAngle * 0.5f, 0) * dir;

                Gizmos.DrawLine(transform.position, transform.position + right);
                Gizmos.DrawLine(transform.position, transform.position + left);
            }
        }
    }
}
