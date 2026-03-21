using UnityEngine;

namespace Mistborn.Combat
{
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected float m_maxHealth = 100f;
        [SerializeField] protected float m_damage = 10f;
        [SerializeField] protected float m_speed = 3f;
        [SerializeField] protected float m_attackRange = 2f;

        protected float m_currentHealth;
        protected Transform m_player;
        protected CharacterController m_controller;

        protected virtual void Awake()
        {
            m_controller = GetComponent<CharacterController>();
            m_currentHealth = m_maxHealth;
        }

        protected virtual void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        protected virtual void Update()
        {
            if (m_player == null) return;
            if (m_currentHealth <= 0) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
            {
                Attack();
            }
            else
            {
                Chase();
            }
        }

        protected virtual void Chase()
        {
            Vector3 direction = (m_player.position - transform.position).normalized;
            direction.y = 0;
            m_controller?.Move(direction * m_speed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);
        }

        protected virtual void Attack()
        {
            PlayerHealth health = m_player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(m_damage);
            }
        }

        public void TakeDamage(float amount)
        {
            m_currentHealth -= amount;
            if (m_currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            Destroy(gameObject);
        }

        public bool isDead => m_currentHealth <= 0;
    }
}
