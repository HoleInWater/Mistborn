using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyMistSpirit : EnemyBase
    {
        [Header("Mist Form")]
        [SerializeField] private float m_phaseSpeed = 6f;
        [SerializeField] private float m_attackSpeed = 12f;
        [SerializeField] private float m_attackRange = 4f;
        [SerializeField] private float m_vanishDuration = 2f;

        [Header("Combat")]
        [SerializeField] private float m_mistDamage = 8f;
        [SerializeField] private float m_telegraphTime = 0.5f;

        [Header("Effects")]
        [SerializeField] private GameObject m_mistParticles;
        [SerializeField] private AudioClip m_phaseSound;
        [SerializeField] private float m_particleOpacity = 0.6f;

        private bool m_isPhasing;
        private bool m_isAttacking;
        private float m_vanishTimer;
        private Vector3 m_attackTarget;
        private Renderer[] m_renderers;
        private Material[] m_materials;
        private float m_normalOpacity;

        protected override void Awake()
        {
            base.Awake();
            m_renderers = GetComponentsInChildren<Renderer>();
            m_materials = new Material[m_renderers.Length];

            for (int i = 0; i < m_renderers.Length; i++)
            {
                m_materials[i] = m_renderers[i].material;
            }

            m_maxHealth = 60f;
            m_currentHealth = m_maxHealth;
            m_detectionRange = 20f;
            m_attackRange = 3f;
        }

        protected override void Start()
        {
            base.Start();
            SetOpacity(m_particleOpacity);
        }

        protected override void Update()
        {
            if (isDead) return;

            if (m_isPhasing)
            {
                ExecutePhase();
            }
            else if (m_isAttacking)
            {
                ExecuteAttack();
            }
            else
            {
                base.Update();
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (m_vanishTimer > 0)
            {
                SetState(State.Attack);
            }
            else if (dist <= m_attackRange)
            {
                TryVanish();
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
                    PhaseToPlayer();
                    break;
                case State.Attack:
                    if (m_vanishTimer <= 0)
                        TryVanish();
                    break;
                case State.Patrol:
                    float phaseSpeed = m_phaseSpeed * 0.3f;
                    Vector3 dir = (m_startPos - transform.position).normalized;
                    m_controller?.Move(dir * phaseSpeed * Time.deltaTime);
                    break;
            }
        }

        private void PhaseToPlayer()
        {
            if (m_player == null) return;

            Vector3 targetPos = m_player.position + (transform.position - m_player.position).normalized * 3f;
            targetPos.y = transform.position.y;

            Vector3 dir = (targetPos - transform.position).normalized;
            m_controller?.Move(dir * m_phaseSpeed * Time.deltaTime);

            transform.rotation = Quaternion.LookRotation(dir);
        }

        private void TryVanish()
        {
            if (m_vanishTimer > 0) return;

            m_isPhasing = true;
            m_vanishTimer = m_vanishDuration;
            SetOpacity(0.1f);

            if (m_mistParticles != null)
                m_mistParticles.SetActive(true);

            SetState(State.Attack);
        }

        private void ExecutePhase()
        {
            m_vanishTimer -= Time.deltaTime;

            float halfDuration = m_vanishDuration * 0.5f;

            if (m_vanishTimer > halfDuration)
            {
                if (m_player != null)
                {
                    m_attackTarget = m_player.position;
                    transform.position = Vector3.Lerp(transform.position, m_attackTarget + Random.insideUnitSphere, Time.deltaTime * 3f);
                }
            }
            else if (m_vanishTimer <= 0)
            {
                AppearAndAttack();
            }
        }

        private void AppearAndAttack()
        {
            m_isPhasing = false;
            SetOpacity(m_particleOpacity);

            if (m_mistParticles != null)
                m_mistParticles.SetActive(false);

            if (m_player != null)
            {
                transform.position = m_player.position + (transform.position - m_player.position).normalized * 2f;
                transform.LookAt(m_player);

                m_isAttacking = true;
            }
        }

        private void ExecuteAttack()
        {
            if (m_player == null)
            {
                m_isAttacking = false;
                return;
            }

            Vector3 dir = (m_player.position - transform.position).normalized;
            m_controller?.Move(dir * m_attackSpeed * Time.deltaTime);

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist < 1f)
            {
                MeleeAttack();
                m_isAttacking = false;
                m_vanishTimer = 2f;
            }
        }

        private void MeleeAttack()
        {
            if (m_player == null) return;

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(new DamageData(m_mistDamage));
            }
        }

        private void SetOpacity(float alpha)
        {
            foreach (Material mat in m_materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;

                    if (mat.HasProperty("_Transparent"))
                    {
                        mat.SetFloat("_Transparent", alpha < 1f ? 1f : 0f);
                    }
                }
            }
        }

        public override void TakeDamage(DamageData damage)
        {
            if (m_isPhasing)
            {
                damage.amount *= 0.3f;
            }

            base.TakeDamage(damage);

            if (!m_isPhasing && m_vanishTimer <= 0 && !isDead)
            {
                TryVanish();
            }
        }

        protected override void Die()
        {
            SetOpacity(1f);
            if (m_mistParticles != null)
                m_mistParticles.SetActive(false);

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_detectionRange);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, m_attackRange);
        }
    }
}
