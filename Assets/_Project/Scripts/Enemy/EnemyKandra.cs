using UnityEngine;
using System.Collections.Generic;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyKandra : EnemyBase
    {
        [Header("Shapeshifter")]
        [SerializeField] private float m_disguiseRadius = 10f;
        [SerializeField] private float m_transformDuration = 1.5f;
        [SerializeField] private float m_disguiseCooldown = 10f;

        [Header("Combat")]
        [SerializeField] private float m_backstabMultiplier = 3f;
        [SerializeField] private float m_ambushRange = 5f;
        [SerializeField] private float m_poisonDamage = 2f;
        [SerializeField] private float m_poisonDuration = 4f;

        [Header("Impersonation")]
        [SerializeField] private Renderer[] m_bodyRenderers;
        [SerializeField] private GameObject[] m_disguisePrefabs;
        [SerializeField] private Material m_kandraMaterial;

        private bool m_isDisguised;
        private float m_disguiseTimer;
        private float m_transformTimer;
        private bool m_isTransforming;
        private Vector3 m_ambushPosition;
        private bool m_isAmbushing;
        private List<Material> m_originalMaterials = new List<Material>();
        private CharacterController m_charController;

        protected override void Awake()
        {
            base.Awake();
            m_charController = GetComponent<CharacterController>();
            m_maxHealth = 80f;
            m_currentHealth = m_maxHealth;
            m_detectionRange = 25f;
            m_speed = 5f;

            StoreOriginalMaterials();
        }

        protected override void Start()
        {
            base.Start();
            m_disguiseTimer = m_disguiseCooldown * 0.5f;
        }

        private void StoreOriginalMaterials()
        {
            m_originalMaterials.Clear();
            foreach (Renderer r in m_bodyRenderers)
            {
                if (r != null)
                    m_originalMaterials.Add(r.material);
            }
        }

        protected override void Update()
        {
            if (isDead) return;

            if (m_isTransforming)
            {
                ExecuteTransform();
            }
            else
            {
                base.Update();

                if (m_isAmbushing)
                {
                    CheckAmbushTrigger();
                }

                if (!m_isDisguised && m_player != null)
                {
                    float dist = Vector3.Distance(transform.position, m_player.position);
                    if (dist > m_detectionRange * 1.5f)
                    {
                        m_disguiseTimer -= Time.deltaTime;
                        if (m_disguiseTimer <= 0 && !m_isDisguised)
                        {
                            StartDisguise();
                        }
                    }
                }
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_ambushRange && !m_isAmbushing && !m_isDisguised)
            {
                StartAmbush();
            }
            else if (dist <= m_attackRange)
            {
                SetState(State.Attack);
            }
            else if (dist <= m_detectionRange)
            {
                if (m_isDisguised)
                    SetState(State.Idle);
                else
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
                    if (m_isAmbushing)
                        ExecuteAmbushAttack();
                    else
                        TryAttack();
                    break;
                case State.Idle:
                    break;
                case State.Patrol:
                    Patrol();
                    break;
            }
        }

        private void StartDisguise()
        {
            m_isTransforming = true;
            m_transformTimer = m_transformDuration;
        }

        private void ExecuteTransform()
        {
            m_transformTimer -= Time.deltaTime;

            float t = 1f - (m_transformTimer / m_transformDuration);

            foreach (Renderer r in m_bodyRenderers)
            {
                if (r != null)
                {
                    r.material.color = Color.Lerp(m_originalMaterials[0].color, Color.gray, t);
                }
            }

            if (m_transformTimer <= 0)
            {
                CompleteDisguise();
            }
        }

        private void CompleteDisguise()
        {
            m_isTransforming = false;
            m_isDisguised = true;

            foreach (Renderer r in m_bodyRenderers)
            {
                if (r != null && m_disguisePrefabs.Length > 0)
                {
                    int prefabIndex = Random.Range(0, m_disguisePrefabs.Length);
                    if (m_disguisePrefabs[prefabIndex] != null)
                    {
                        GameObject disguise = Instantiate(m_disguisePrefabs[prefabIndex], transform.position, transform.rotation);
                        disguise.transform.SetParent(transform);
                    }
                    r.enabled = false;
                }
            }

            m_detectionRange = 8f;
            m_disguiseTimer = m_disguiseCooldown;
        }

        private void StartAmbush()
        {
            if (m_player == null) return;

            m_isAmbushing = true;
            m_ambushPosition = m_player.position + (transform.position - m_player.position).normalized * 2f;
            m_ambushPosition.y = transform.position.y;

            m_isDisguised = false;
            SetState(State.Idle);
        }

        private void CheckAmbushTrigger()
        {
            if (m_player == null)
            {
                m_isAmbushing = false;
                return;
            }

            float distToAmbush = Vector3.Distance(transform.position, m_ambushPosition);
            if (distToAmbush > 0.5f)
            {
                m_charController?.Move((m_ambushPosition - transform.position).normalized * m_speed * 2f * Time.deltaTime);
            }
            else
            {
                SetState(State.Attack);
            }
        }

        private void ExecuteAmbushAttack()
        {
            if (m_player == null)
            {
                m_isAmbushing = false;
                return;
            }

            Vector3 dir = (m_player.position - transform.position).normalized;
            m_charController?.Move(dir * m_speed * 3f * Time.deltaTime);

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist < 1.5f)
            {
                AmbushHit();
                m_isAmbushing = false;
            }
        }

        private void AmbushHit()
        {
            if (m_player == null) return;

            if (m_player.TryGetComponent(out PlayerHealth health))
            {
                float damage = m_damage * m_backstabMultiplier;
                health.TakeDamage(new DamageData(damage));
                ApplyPoison();
            }

            m_isAmbushed = false;
        }

        private void ApplyPoison()
        {
            if (m_player == null) return;

            PoisonEffect poison = m_player.GetComponent<PoisonEffect>();
            if (poison == null)
            {
                poison = m_player.gameObject.AddComponent<PoisonEffect>();
            }

            poison.ApplyPoison(m_poisonDamage, m_poisonDuration);
        }

        protected override void Attack()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist <= m_attackRange)
            {
                if (m_player.TryGetComponent(out PlayerHealth health))
                {
                    bool isBehind = IsBehindTarget();
                    float damage = isBehind ? m_damage * m_backstabMultiplier : m_damage;
                    health.TakeDamage(new DamageData(damage));

                    if (isBehind)
                        ApplyPoison();
                }
            }
        }

        private bool IsBehindTarget()
        {
            if (m_player == null) return false;

            Vector3 toEnemy = (transform.position - m_player.position).normalized;
            Vector3 playerForward = m_player.forward;
            playerForward.y = 0;
            toEnemy.y = 0;

            return Vector3.Dot(toEnemy, playerForward) < -0.5f;
        }

        public override void TakeDamage(DamageData damage)
        {
            if (m_isDisguised)
            {
                damage.amount *= 0.5f;
            }

            base.TakeDamage(damage);

            if (m_isDisguised && damage.amount > 10f)
            {
                BreakDisguise();
            }
        }

        private void BreakDisguise()
        {
            m_isDisguised = false;
            m_detectionRange = 25f;

            foreach (Transform child in transform)
            {
                if (child.name.Contains("Disguise"))
                {
                    Destroy(child.gameObject);
                }
            }

            for (int i = 0; i < m_bodyRenderers.Length; i++)
            {
                if (m_bodyRenderers[i] != null)
                {
                    m_bodyRenderers[i].enabled = true;
                    if (i < m_originalMaterials.Count)
                        m_bodyRenderers[i].material = m_originalMaterials[i];
                }
            }

            m_disguiseTimer = m_disguiseCooldown;
        }

        private bool m_isAmbushed;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_disguiseRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_ambushRange);
        }
    }

    public class PoisonEffect : MonoBehaviour
    {
        private float m_damagePerTick = 1f;
        private float m_tickInterval = 0.5f;
        private float m_duration;
        private float m_timer;

        public void ApplyPoison(float damagePerTick, float duration)
        {
            m_damagePerTick = damagePerTick;
            m_duration = duration;
            m_timer = duration;
        }

        private void Update()
        {
            if (m_timer <= 0) return;

            m_timer -= Time.deltaTime;

            float tickTimer = m_tickInterval;
            while (tickTimer <= 0)
            {
                if (TryGetComponent(out PlayerHealth health))
                {
                    health.TakeDamage(new DamageData(m_damagePerTick));
                }
                tickTimer += m_tickInterval;
            }
        }
    }
}
