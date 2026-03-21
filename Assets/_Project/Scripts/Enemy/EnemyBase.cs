using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected float m_maxHealth = 100f;
        [SerializeField] protected float m_damage = 10f;
        [SerializeField] protected float m_speed = 4f;

        [Header("Combat")]
        [SerializeField] protected float m_attackRange = 2f;
        [SerializeField] protected float m_attackCooldown = 1f;

        [Header("AI")]
        [SerializeField] protected float m_detectionRange = 15f;

        protected float m_currentHealth;
        protected float m_lastAttack;
        protected State m_state = State.Idle;
        protected Transform m_player;
        protected Vector3 m_startPos;
        protected CharacterController m_controller;

        public float health => m_currentHealth;
        public bool isDead => m_currentHealth <= 0;

        public event System.Action OnDeath;
        public event System.Action<float> OnDamaged;
        public event System.Action<State> OnStateChanged;

        protected enum State { Idle, Patrol, Chase, Attack, Dead }

        protected virtual void Awake()
        {
            m_controller = GetComponent<CharacterController>();
            m_currentHealth = m_maxHealth;
        }

        protected virtual void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
            m_startPos = transform.position;
        }

        protected virtual void Update()
        {
            if (isDead) return;
            UpdateAI();
        }

        protected virtual void UpdateAI()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
                SetState(State.Attack);
            else if (dist <= m_detectionRange)
                SetState(State.Chase);
            else
                SetState(State.Patrol);

            ExecuteState();
        }

        protected virtual void ExecuteState()
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

        protected void MoveTo(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0;
            m_controller?.Move(dir * m_speed * Time.deltaTime);
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        protected virtual void Patrol()
        {
            MoveTo(m_startPos);
        }

        protected virtual void TryAttack()
        {
            if (Time.time - m_lastAttack < m_attackCooldown) return;
            m_lastAttack = Time.time;
            Attack();
        }

        protected virtual void Attack()
        {
            if (m_player == null) return;
            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist <= m_attackRange)
            {
                if (m_player.TryGetComponent(out PlayerHealth health))
                    health.TakeDamage(new DamageData(m_damage));
            }
        }

        public virtual void TakeDamage(DamageData damage)
        {
            m_currentHealth -= damage.amount;
            OnDamaged?.Invoke(damage.amount);
            if (m_currentHealth <= 0)
            {
                m_currentHealth = 0;
                Die();
            }
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject, 2f);
        }

        protected void SetState(State newState)
        {
            if (m_state != newState)
            {
                m_state = newState;
                OnStateChanged?.Invoke(newState);
            }
        }
    }
}
