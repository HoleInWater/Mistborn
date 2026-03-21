using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyKoloss : EnemyBase
    {
        [Header("Koloss Stats")]
        [SerializeField] private float m_chargeSpeed = 8f;
        [SerializeField] private float m_chargeRange = 10f;
        [SerializeField] private float m_chargeCooldown = 5f;
        [SerializeField] private float m_chargeDamage = 40f;
        [SerializeField] private float m_chargeWindup = 1f;
        [SerializeField] private float m_chargeKnockback = 15f;
        [SerializeField] private float m_groundSlamRadius = 5f;
        [SerializeField] private float m_groundSlamDamage = 25f;

        [Header("Animation")]
        [SerializeField] private float m_animatorSpeedMultiplier = 0.8f;

        private float m_chargeTimer;
        private bool m_isCharging;
        private bool m_isWindup;
        private Vector3 m_chargeDirection;
        private CharacterController m_charController;

        protected override void Awake()
        {
            base.Awake();
            m_charController = GetComponent<CharacterController>();
            m_speed *= m_animatorSpeedMultiplier;
            m_detectionRange *= 1.5f;
        }

        protected override void Start()
        {
            base.Start();
            m_chargeTimer = m_chargeCooldown;
        }

        protected override void Update()
        {
            if (isDead) return;

            if (m_isCharging)
            {
                ExecuteCharge();
            }
            else if (m_isWindup)
            {
                m_chargeTimer -= Time.deltaTime;
                transform.Rotate(Vector3.up, 180f * Time.deltaTime);
                if (m_chargeTimer <= 0)
                {
                    m_isWindup = false;
                    m_isCharging = true;
                    m_chargeDirection = (m_player.position - transform.position).normalized;
                    m_chargeDirection.y = 0;
                    m_chargeDirection.Normalize();
                    m_chargeTimer = 1.5f;
                }
            }
            else
            {
                m_chargeTimer -= Time.deltaTime;
                base.Update();
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
                SetState(State.Attack);
            else if (dist <= m_chargeRange && m_chargeTimer <= 0 && !m_isCharging && !m_isWindup)
                SetState(State.Charge);
            else if (dist <= m_detectionRange)
                SetState(State.Chase);
            else
                SetState(State.Patrol);

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
                    TryAttack();
                    break;
                case State.Charge:
                    StartCharge();
                    break;
                case State.Patrol:
                    Patrol();
                    break;
            }
        }

        private void StartCharge()
        {
            if (m_isCharging || m_isWindup) return;

            m_isWindup = true;
            m_chargeTimer = m_chargeWindup;
            SetState(State.Chase);
        }

        private void ExecuteCharge()
        {
            m_chargeTimer -= Time.deltaTime;

            Vector3 targetPos = transform.position + m_chargeDirection * m_chargeSpeed * Time.deltaTime;
            m_charController?.Move(m_chargeDirection * m_chargeSpeed * Time.deltaTime);

            if (m_chargeTimer <= 0 || ReachedTarget())
            {
                m_isCharging = false;
                m_chargeTimer = m_chargeCooldown;
                GroundSlam();
            }
        }

        private bool ReachedTarget()
        {
            float dist = Vector3.Distance(transform.position, m_player.position);
            return dist < 2f;
        }

        private void GroundSlam()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_groundSlamRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    if (hit.TryGetComponent(out PlayerHealth health))
                    {
                        health.TakeDamage(new DamageData(m_groundSlamDamage));
                        Rigidbody rb = hit.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            Vector3 knockbackDir = (hit.transform.position - transform.position).normalized;
                            knockbackDir.y = 0.5f;
                            rb.AddForce(knockbackDir * m_chargeKnockback, ForceMode.Impulse);
                        }
                    }
                }
            }
        }

        protected override void Attack()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist <= m_attackRange)
            {
                if (m_player.TryGetComponent(out PlayerHealth health))
                {
                    health.TakeDamage(new DamageData(m_damage));
                }
            }
        }

        public override void TakeDamage(DamageData damage)
        {
            base.TakeDamage(damage);

            if (!isDead && m_chargeTimer > 0)
            {
                m_chargeTimer -= 0.5f;
            }
        }

        protected override void Die()
        {
            m_isCharging = false;
            m_isWindup = false;
            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_chargeRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_groundSlamRadius);
        }
    }
}
