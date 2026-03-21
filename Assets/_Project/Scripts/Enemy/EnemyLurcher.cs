using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyLurcher : EnemyBase
    {
        [Header("Allomancy")]
        [SerializeField] private float m_pushForce = 40f;
        [SerializeField] private float m_pullForce = 40f;
        [SerializeField] private float m_allomancyRange = 25f;
        [SerializeField] private float m_allomancyCooldown = 2f;

        [Header("Combat")]
        [SerializeField] private float m_meleeDamage = 8f;
        [SerializeField] private float m_surgeDamage = 25f;
        [SerializeField] private float m_knockbackForce = 15f;

        [Header("Metal Detection")]
        [SerializeField] private float m_detectionRadius = 30f;
        [SerializeField] private LayerMask m_metalLayers = ~0;

        private float m_allomancyTimer;
        private bool m_isPulling;
        private Vector3 m_pullDirection;
        private Transform m_currentTargetMetal;
        private Rigidbody m_playerRb;

        protected override void Awake()
        {
            base.Awake();
            m_maxHealth = 70f;
            m_currentHealth = m_maxHealth;
            m_detectionRange = 20f;
            m_speed = 4.5f;
            m_attackRange = 15f;
        }

        protected override void Start()
        {
            base.Start();
            m_playerRb = m_player?.GetComponent<Rigidbody>();
        }

        protected override void Update()
        {
            if (isDead) return;

            UpdateAllomancyCooldown();

            if (m_isPulling)
            {
                ExecutePull();
            }
            else
            {
                base.Update();
            }
        }

        private void UpdateAllomancyCooldown()
        {
            if (m_allomancyTimer > 0)
            {
                m_allomancyTimer -= Time.deltaTime;
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (m_allomancyTimer <= 0 && dist <= m_allomancyRange)
            {
                UseAllomancy(dist);
            }
            else if (dist <= m_attackRange * 0.3f)
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
                    TryAttack();
                    break;
                case State.Patrol:
                    Patrol();
                    break;
            }
        }

        private void UseAllomancy(float dist)
        {
            if (m_allomancyTimer > 0) return;

            m_currentTargetMetal = FindNearestMetal();

            if (m_currentTargetMetal != null)
            {
                StartPull(m_currentTargetMetal);
            }
            else if (dist > 10f)
            {
                SteelPush();
            }
            else
            {
                IronPull();
            }

            m_allomancyTimer = m_allomancyCooldown;
        }

        private Transform FindNearestMetal()
        {
            Collider[] metals = Physics.OverlapSphere(transform.position, m_detectionRadius, m_metalLayers);

            Transform closest = null;
            float closestDist = m_detectionRadius;

            foreach (Collider metal in metals)
            {
                if (metal.CompareTag("Metal") || metal.GetComponent<AllomanticTarget>() != null)
                {
                    float dist = Vector3.Distance(transform.position, metal.transform.position);
                    if (dist < closestDist)
                    {
                        closest = metal.transform;
                        closestDist = dist;
                    }
                }
            }

            return closest;
        }

        private void StartPull(Transform metal)
        {
            if (metal == null) return;

            m_isPulling = true;
            m_pullDirection = (transform.position - metal.position).normalized;
            m_pullDirection.y = 0;
        }

        private void ExecutePull()
        {
            if (m_currentTargetMetal == null)
            {
                m_isPulling = false;
                return;
            }

            float dist = Vector3.Distance(transform.position, m_currentTargetMetal.position);

            if (dist < 3f)
            {
                m_isPulling = false;
                IronPull();
                return;
            }

            Rigidbody metalRb = m_currentTargetMetal.GetComponent<Rigidbody>();
            if (metalRb != null)
            {
                metalRb.AddForce(m_pullDirection * m_pullForce, ForceMode.Impulse);
            }

            Vector3 pullPos = m_currentTargetMetal.position + m_pullDirection * m_pullForce * Time.deltaTime;
            m_currentTargetMetal.position = pullPos;
        }

        private void SteelPush()
        {
            if (m_player == null) return;

            Vector3 direction = (m_player.position - transform.position).normalized;
            direction.y = 0.3f;

            if (m_playerRb != null)
            {
                m_playerRb.AddForce(direction * m_pushForce, ForceMode.Impulse);
            }

            m_isPulling = false;

            PushNearbyMetals(transform.position, direction);
        }

        private void IronPull()
        {
            if (m_player == null) return;

            Vector3 direction = (transform.position - m_player.position).normalized;
            direction.y = 0.3f;

            if (m_playerRb != null)
            {
                m_playerRb.AddForce(direction * m_pullForce, ForceMode.Impulse);
            }

            PullNearbyMetals(transform.position);
        }

        private void PushNearbyMetals(Vector3 origin, Vector3 direction)
        {
            Collider[] metals = Physics.OverlapSphere(origin, m_allomancyRange, m_metalLayers);

            foreach (Collider metal in metals)
            {
                if (metal.CompareTag("Metal") || metal.GetComponent<AllomanticTarget>() != null)
                {
                    Vector3 toMetal = (metal.transform.position - origin).normalized;
                    float dot = Vector3.Dot(toMetal, direction);

                    if (dot > 0.5f)
                    {
                        Rigidbody rb = metal.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddForce(toMetal * m_pushForce * dot, ForceMode.Impulse);
                        }
                    }
                }
            }
        }

        private void PullNearbyMetals(Vector3 origin)
        {
            Collider[] metals = Physics.OverlapSphere(origin, m_allomancyRange, m_metalLayers);

            foreach (Collider metal in metals)
            {
                if (metal.CompareTag("Metal") || metal.GetComponent<AllomanticTarget>() != null)
                {
                    Vector3 toMetal = (origin - metal.transform.position).normalized;

                    Rigidbody rb = metal.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(toMetal * m_pullForce, ForceMode.Impulse);
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
                    health.TakeDamage(new DamageData(m_meleeDamage));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, m_allomancyRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_detectionRadius);
        }
    }
}
