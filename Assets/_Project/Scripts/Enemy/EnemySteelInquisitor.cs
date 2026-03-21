using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemySteelInquisitor : EnemyBase
    {
        [Header("Allomancy")]
        [SerializeField] private float m_steelPushForce = 50f;
        [SerializeField] private float m_ironPullForce = 50f;
        [SerializeField] private float m_pushRange = 30f;
        [SerializeField] private float m_allomancyCooldown = 3f;

        [Header("Combat")]
        [SerializeField] private float m_meleeDamage = 25f;
        [SerializeField] private float m_spikeAttackDamage = 15f;
        [SerializeField] private float m_healthRegenRate = 2f;

        [Header("Phasing")]
        [SerializeField] private float m_phaseDuration = 3f;
        [SerializeField] private float m_phaseCooldown = 15f;
        [SerializeField] private float m_phaseRadius = 5f;

        [Header("Boss")]
        [SerializeField] private float m_phase2Threshold = 0.5f;
        [SerializeField] private float m_phase3Threshold = 0.25f;

        private float m_allomancyTimer;
        private float m_phaseTimer;
        private float m_phaseCooldownTimer;
        private bool m_isPhasing;
        private bool m_phase2Active;
        private bool m_phase3Active;
        private int m_attackPhase;
        private Renderer[] m_renderers;

        public event System.Action<int> OnPhaseChanged;

        protected override void Awake()
        {
            base.Awake();
            m_renderers = GetComponentsInChildren<Renderer>();
            m_maxHealth = 500f;
            m_currentHealth = m_maxHealth;
            m_detectionRange = 25f;
        }

        protected override void Update()
        {
            if (isDead) return;

            RegenerateHealth();
            UpdateTimers();

            if (m_isPhasing)
            {
                ExecutePhasing();
            }
            else
            {
                base.Update();
                UseAllomancy();
            }

            CheckPhaseTransition();
        }

        private void RegenerateHealth()
        {
            if (m_currentHealth < m_maxHealth && !isDead)
            {
                m_currentHealth += m_healthRegenRate * Time.deltaTime;
                m_currentHealth = Mathf.Min(m_currentHealth, m_maxHealth);
            }
        }

        private void UpdateTimers()
        {
            m_allomancyTimer -= Time.deltaTime;

            if (m_phaseCooldownTimer > 0)
                m_phaseCooldownTimer -= Time.deltaTime;
        }

        private void UseAllomancy()
        {
            if (m_player == null) return;
            if (m_allomancyTimer > 0) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist > m_pushRange * 0.7f)
            {
                SteelPush();
            }
            else if (dist > m_attackRange)
            {
                IronPull();
            }

            m_allomancyTimer = m_allomancyCooldown * (m_phase3Active ? 0.5f : 1f);
        }

        private void SteelPush()
        {
            Vector3 direction = (m_player.position - transform.position).normalized;
            Rigidbody rb = m_player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float force = m_steelPushForce * (m_phase3Active ? 1.5f : 1f);
                rb.AddForce(direction * force, ForceMode.Impulse);
            }

            Vector3 pushDir = direction;
            pushDir.y = 0.3f;
            m_controller?.Move(pushDir * m_steelPushForce * 0.3f * Time.deltaTime);
        }

        private void IronPull()
        {
            Vector3 direction = (transform.position - m_player.position).normalized;
            Rigidbody rb = m_player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float force = m_ironPullForce * (m_phase3Active ? 1.5f : 1f);
                rb.AddForce(direction * force, ForceMode.Impulse);
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
                SetState(State.Attack);
            else if (dist <= m_detectionRange)
                SetState(State.Chase);
            else
                SetState(State.Patrol);

            if (m_phaseCooldownTimer <= 0 && !m_isPhasing)
                TryStartPhase();

            base.ExecuteState();
        }

        private void TryStartPhase()
        {
            float healthPercent = m_currentHealth / m_maxHealth;

            if (healthPercent <= m_phase3Threshold && !m_phase3Active)
            {
                StartPhase(3);
            }
            else if (healthPercent <= m_phase2Threshold && !m_phase2Active)
            {
                StartPhase(2);
            }
            else if (healthPercent > m_phase2Threshold)
            {
                StartPhase(1);
            }
        }

        private void StartPhase(int phase)
        {
            m_isPhasing = true;
            m_phaseTimer = m_phaseDuration;
            m_attackPhase = phase;

            if (phase >= 2)
            {
                m_phase2Active = true;
                OnPhaseChanged?.Invoke(2);
            }

            if (phase >= 3)
            {
                m_phase3Active = true;
                OnPhaseChanged?.Invoke(3);
            }

            SetState(State.Chase);
        }

        private void ExecutePhasing()
        {
            m_phaseTimer -= Time.deltaTime;

            foreach (Renderer r in m_renderers)
            {
                if (r != null)
                    r.enabled = Mathf.PerlinNoise(Time.time * 10f, 0f) > 0.3f;
            }

            if (m_player != null)
            {
                Vector3 dirToPlayer = (m_player.position - transform.position).normalized;
                Vector3 strafeDir = Vector3.Cross(dirToPlayer, Vector3.up);

                if (m_phase3Active)
                    strafeDir *= 1.5f;

                m_controller?.Move(strafeDir * m_speed * Time.deltaTime);
            }

            if (m_phaseTimer <= 0)
            {
                EndPhase();
            }
        }

        private void EndPhase()
        {
            m_isPhasing = false;
            m_phaseCooldownTimer = m_phaseCooldown;

            foreach (Renderer r in m_renderers)
            {
                if (r != null)
                    r.enabled = true;
            }

            if (m_phase3Active)
            {
                GroundSlamAttack();
            }
        }

        private void GroundSlamAttack()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_phaseRadius * 1.5f);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    if (hit.TryGetComponent(out PlayerHealth health))
                    {
                        health.TakeDamage(new DamageData(m_spikeAttackDamage * 2f));
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
                    float damage = m_phase3Active ? m_meleeDamage * 1.5f : m_meleeDamage;
                    health.TakeDamage(new DamageData(damage));
                }
            }
        }

        public override void TakeDamage(DamageData damage)
        {
            float reduction = m_phase2Active ? 0.7f : 1f;
            if (m_phase3Active)
                reduction = 0.5f;

            DamageData reducedDamage = new DamageData(damage.amount * reduction);
            base.TakeDamage(reducedDamage);
        }

        protected override void Die()
        {
            foreach (Renderer r in m_renderers)
            {
                if (r != null)
                    r.enabled = true;
            }

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_pushRange);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_phaseRadius);
        }
    }
}
