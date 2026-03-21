using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyThug : EnemyBase
    {
        [Header("Combat Style")]
        [SerializeField] private bool m_useFlanking = true;
        [SerializeField] private float m_flankingDistance = 3f;
        [SerializeField] private float m_tacticalRetreatHealth = 0.3f;

        [Header("Weapons")]
        [SerializeField] private bool m_usesRanged = false;
        [SerializeField] private float m_rangedRange = 15f;
        [SerializeField] private float m_rangedCooldown = 2f;
        [SerializeField] private float m_rangedDamage = 8f;

        [Header("Formation")]
        [SerializeField] private bool m_usesFormation = false;
        [SerializeField] private Transform m_formationAnchor;
        [SerializeField] private float m_formationSpacing = 2f;
        [SerializeField] private int m_formationPosition = 0;

        [Header("Combat Arts")]
        [SerializeField] private float m_battleCryRadius = 10f;
        [SerializeField] private float m_battleCryDuration = 5f;

        private float m_rangedTimer;
        private bool m_isFlanking;
        private Vector3 m_flankPosition;
        private bool m_isRetreating;
        private CharacterController m_charController;
        private Animator m_animator;

        protected override void Awake()
        {
            base.Awake();
            m_charController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();

            m_maxHealth = 80f;
            m_currentHealth = m_maxHealth;
            m_damage = 12f;
            m_speed = 4f;
            m_detectionRange = 18f;
            m_attackRange = 2f;
        }

        protected override void Start()
        {
            base.Start();
            CalculateFlankPosition();
        }

        protected override void Update()
        {
            if (isDead) return;

            UpdateRangedCooldown();
            UpdateRetreatState();

            if (m_rangedTimer > 0)
                m_rangedTimer -= Time.deltaTime;

            base.Update();
        }

        private void UpdateRangedCooldown()
        {
        }

        private void UpdateRetreatState()
        {
            float healthPercent = m_currentHealth / m_maxHealth;

            if (healthPercent <= m_tacticalRetreatHealth && !m_isRetreating)
            {
                StartRetreat();
            }
            else if (healthPercent > m_tacticalRetreatHealth + 0.1f && m_isRetreating)
            {
                EndRetreat();
            }
        }

        private void CalculateFlankPosition()
        {
            if (m_player == null) return;

            Vector3 toPlayer = (m_player.position - transform.position).normalized;
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;

            int flankSide = m_formationPosition % 2 == 0 ? 1 : -1;
            m_flankPosition = m_player.position + perpendicular * flankSide * m_flankingDistance;
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (m_isRetreating)
            {
                SetState(State.Patrol);
            }
            else if (m_usesRanged && dist <= m_rangedRange && dist > m_attackRange && m_rangedTimer <= 0)
            {
                SetState(State.Attack);
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
                    if (m_useFlanking && !m_isFlanking)
                    {
                        StartFlanking();
                    }
                    MoveTo(GetMovementTarget());
                    break;
                case State.Attack:
                    if (m_usesRanged && Vector3.Distance(transform.position, m_player.position) > m_attackRange)
                    {
                        RangedAttack();
                    }
                    else
                    {
                        TryAttack();
                    }
                    break;
                case State.Patrol:
                    if (m_isRetreating)
                    {
                        Retreat();
                    }
                    else
                    {
                        Patrol();
                    }
                    break;
            }
        }

        private Vector3 GetMovementTarget()
        {
            if (m_useFlanking && m_isFlanking)
            {
                return m_flankPosition;
            }

            if (m_usesFormation && m_formationAnchor != null)
            {
                Vector3 formationPos = m_formationAnchor.position + Vector3.right * m_formationPosition * m_formationSpacing;
                return formationPos;
            }

            return m_player.position;
        }

        private void StartFlanking()
        {
            m_isFlanking = true;
            CalculateFlankPosition();
        }

        private void RangedAttack()
        {
            if (m_rangedTimer > 0) return;

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(new DamageData(m_rangedDamage));
            }

            m_rangedTimer = m_rangedCooldown;

            if (m_animator != null)
            {
                m_animator.SetTrigger("RangedAttack");
            }

            CalculateNextFlankPosition();
        }

        private void CalculateNextFlankPosition()
        {
            if (m_player == null) return;

            float angle = Random.Range(-60f, 60f);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * transform.forward * m_flankingDistance;
            m_flankPosition = m_player.position + offset;
        }

        private void StartRetreat()
        {
            m_isRetreating = true;

            if (m_animator != null)
            {
                m_animator.SetTrigger("Retreat");
            }
        }

        private void EndRetreat()
        {
            m_isRetreating = false;
        }

        private void Retreat()
        {
            Vector3 retreatDir = (transform.position - m_player.position).normalized;
            MoveTo(transform.position + retreatDir * 5f);
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

                m_isFlanking = false;
            }
        }

        public void CallForHelp()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, m_battleCryRadius);
            foreach (Collider col in nearby)
            {
                if (col.CompareTag("Enemy"))
                {
                    EnemyThug ally = col.GetComponent<EnemyThug>();
                    if (ally != null && ally != this)
                    {
                        ally.ReceiveCallForHelp(transform.position);
                    }
                }
            }
        }

        public void ReceiveCallForHelp(Vector3 callerPosition)
        {
            if (m_state == State.Patrol || m_state == State.Idle)
            {
                m_isFlanking = false;
            }
        }

        public override void TakeDamage(DamageData damage)
        {
            base.TakeDamage(damage);

            if (Random.value < 0.1f)
            {
                CallForHelp();
            }
        }
    }
}
