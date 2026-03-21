using UnityEngine;

namespace Mistborn.Combat
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float m_maxHealth = 100f;
        [SerializeField] private float m_currentHealth;

        private void Awake()
        {
            m_currentHealth = m_maxHealth;
        }

        public void TakeDamage(float amount)
        {
            m_currentHealth -= amount;
            m_currentHealth = Mathf.Max(0, m_currentHealth);

            if (m_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            m_currentHealth += amount;
            m_currentHealth = Mathf.Min(m_currentHealth, m_maxHealth);
        }

        private void Die()
        {
            Debug.Log("Player died");
        }

        public float CurrentHealth => m_currentHealth;
        public float MaxHealth => m_maxHealth;
    }
}
