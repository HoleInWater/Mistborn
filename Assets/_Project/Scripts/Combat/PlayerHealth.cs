using UnityEngine;
using Mistborn.Allomancy;

namespace Mistborn.Combat
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float m_maxHealth = 100f;
        [SerializeField] private float m_invincibilityDuration = 0.5f;
        [SerializeField] private GameObject m_deathScreen;

        private float m_currentHealth;
        private float m_invincibilityTimer;
        private bool m_isInvincible;
        private bool m_isDead;

        public float health => m_currentHealth;
        public float maxHealth => m_maxHealth;
        public bool isDead => m_isDead;
        public bool isInvincible => m_isInvincible;

        public event System.Action OnDeath;
        public event System.Action<float> OnDamaged;
        public event System.Action<float> OnHealed;

        private void Awake()
        {
            m_currentHealth = m_maxHealth;
        }

        private void Update()
        {
            if (m_isInvincible)
            {
                m_invincibilityTimer -= Time.deltaTime;
                if (m_invincibilityTimer <= 0)
                    m_isInvincible = false;
            }
        }

        public void TakeDamage(DamageData damage)
        {
            if (m_isDead || m_isInvincible) return;

            float actual = damage.amount;
            
            PewterEnhancement pewter = GetComponent<PewterEnhancement>();
            if (pewter != null && pewter.isEnhanced)
                actual *= (1f - pewter.GetPainResistance());

            m_currentHealth -= actual;
            OnDamaged?.Invoke(actual);

            if (m_currentHealth <= 0)
            {
                m_currentHealth = 0;
                Die();
            }
            else
            {
                m_isInvincible = true;
                m_invincibilityTimer = m_invincibilityDuration;
            }
        }

        public void Heal(float amount)
        {
            if (m_isDead) return;
            m_currentHealth = Mathf.Min(m_currentHealth + amount, m_maxHealth);
            OnHealed?.Invoke(amount);
        }

        private void Die()
        {
            if (m_isDead) return;
            m_isDead = true;
            OnDeath?.Invoke();
            m_deathScreen?.SetActive(true);
        }

        public void Respawn()
        {
            m_isDead = false;
            m_currentHealth = m_maxHealth;
            m_isInvincible = true;
            m_invincibilityTimer = m_invincibilityDuration;
            m_deathScreen?.SetActive(false);
            OnRespawn?.Invoke();
        }

        public event System.Action OnRespawn;
    }
}
