using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Allomancy
{
    public class PewterHealing : MonoBehaviour
    {
        [Header("Healing Settings")]
        [SerializeField] private float m_healRate = 5f;
        [SerializeField] private float m_maxMendHealth = 50f;
        [SerializeField] private float m_mendCooldown = 30f;

        [Header("Wound Types")]
        [SerializeField] private bool m_healBleeding = true;
        [SerializeField] private bool m_healBurning = true;
        [SerializeField] private bool m_healPoison = true;

        [Header("Visual")]
        [SerializeField] private Color m_healColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private GameObject m_healParticles;
        [SerializeField] private AudioClip m_healSound;

        private AllomancerController m_allomancer;
        private PlayerHealth m_playerHealth;
        private float m_mendCooldownTimer;
        private bool m_isHealing;
        private ParticleSystem m_particleSystem;
        private AudioSource m_audioSource;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_playerHealth = GetComponent<PlayerHealth>();
            m_audioSource = GetComponent<AudioSource>();

            if (m_audioSource == null)
                m_audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            if (m_mendCooldownTimer > 0)
            {
                m_mendCooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                TryStartHealing();
            }
            else
            {
                StopHealing();
            }

            if (Input.GetKeyDown(KeyCode.F) && CanUseMend())
            {
                ActivateMend();
            }
        }

        private void TryStartHealing()
        {
            if (m_allomancer == null || !m_allomancer.IsBurning(AllomanticMetal.Pewter))
                return;

            if (!m_isHealing && CanHeal())
            {
                StartHealing();
            }
        }

        private bool CanHeal()
        {
            if (m_playerHealth == null) return false;
            if (m_playerHealth.CurrentHealth >= m_playerHealth.MaxHealth) return false;
            if (HasActiveWounds()) return true;
            return false;
        }

        private bool HasActiveWounds()
        {
            return true;
        }

        private void StartHealing()
        {
            m_isHealing = true;

            if (m_healParticles != null && m_particleSystem == null)
            {
                GameObject healVFX = Instantiate(m_healParticles, transform.position, Quaternion.identity);
                healVFX.transform.SetParent(transform);
                m_particleSystem = healVFX.GetComponent<ParticleSystem>();
            }

            if (m_healSound != null)
            {
                m_audioSource.clip = m_healSound;
                m_audioSource.loop = true;
                m_audioSource.Play();
            }
        }

        private void StopHealing()
        {
            if (!m_isHealing) return;

            m_isHealing = false;

            if (m_particleSystem != null)
            {
                m_particleSystem.Stop();
                Destroy(m_particleSystem.gameObject, 1f);
                m_particleSystem = null;
            }

            if (m_audioSource != null && m_audioSource.clip == m_healSound)
            {
                m_audioSource.Stop();
            }
        }

        private void FixedUpdate()
        {
            if (!m_isHealing) return;

            if (m_healBleeding)
            {
                HealStatusEffects();
            }

            if (m_playerHealth != null)
            {
                m_playerHealth.Heal(m_healRate * Time.fixedDeltaTime);
            }
        }

        private void HealStatusEffects()
        {
        }

        private bool CanUseMend()
        {
            return m_mendCooldownTimer <= 0 &&
                   m_allomancer != null &&
                   m_allomancer.IsBurning(AllomanticMetal.Pewter);
        }

        private void ActivateMend()
        {
            if (m_playerHealth == null) return;

            float currentHealth = m_playerHealth.CurrentHealth;
            float healAmount = m_maxMendHealth;
            float maxHealth = m_playerHealth.MaxHealth;

            float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
            m_playerHealth.Heal(actualHeal);

            m_mendCooldownTimer = m_mendCooldown;

            PlayMendEffect();

            Debug.Log($"Mend activated! Healed {actualHeal} health");
        }

        private void PlayMendEffect()
        {
            if (m_particleSystem != null)
            {
                var main = m_particleSystem.main;
                main.startColor = m_healColor;
                m_particleSystem.Emit(50);
            }

            if (m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_healSound);
            }
        }

        public float MendCooldownPercent()
        {
            if (m_mendCooldown <= 0) return 1f;
            return 1f - (m_mendCooldownTimer / m_mendCooldown);
        }

        public bool IsMendReady()
        {
            return m_mendCooldownTimer <= 0;
        }
    }

    public class PewterFighting : MonoBehaviour
    {
        [Header("Combat Bonuses")]
        [SerializeField] private float m_damageBonus = 1.5f;
        [SerializeField] private float m_armorBonus = 0.3f;
        [SerializeField] private float m_staggerReduction = 0.5f;
        [SerializeField] private float m_attackSpeedBonus = 1.3f;

        [Header("Feelings")]
        [SerializeField] private bool m_reducePain = true;
        [SerializeField] private float m_painReduction = 0.75f;

        private AllomancerController m_allomancer;
        private PlayerCombat m_playerCombat;
        private PlayerHealth m_playerHealth;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_playerCombat = GetComponent<PlayerCombat>();
            m_playerHealth = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Pewter);

            if (m_playerCombat != null)
            {
                m_playerCombat.SetDamageMultiplier(isBurning ? m_damageBonus : 1f);
                m_playerCombat.SetAttackSpeed(isBurning ? m_attackSpeedBonus : 1f);
            }
        }

        public float GetDamageMultiplier()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Pewter);
            return isBurning ? m_damageBonus : 1f;
        }

        public float GetArmorBonus()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Pewter);
            return isBurning ? m_armorBonus : 0f;
        }

        public float GetPainReduction()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Pewter);
            return isBurning && m_reducePain ? m_painReduction : 0f;
        }
    }

    public class PewterStacking : MonoBehaviour
    {
        [Header("Stacking")]
        [SerializeField] private int m_maxStacks = 5;
        [SerializeField] private float m_stackDecayTime = 3f;
        [SerializeField] private float m_stackDurationBonus = 0.5f;
        [SerializeField] private AnimationCurve m_stackCurve = AnimationCurve.Linear(0, 1f, 1, 3f);

        private int m_currentStacks;
        private float m_stackTimer;

        public int CurrentStacks => m_currentStacks;
        public float StackMultiplier => m_stackCurve.Evaluate((float)m_currentStacks / m_maxStacks);

        private void Update()
        {
            if (m_currentStacks <= 0) return;

            m_stackTimer -= Time.deltaTime;
            if (m_stackTimer <= 0)
            {
                DecayStack();
            }
        }

        public void AddStack()
        {
            if (m_currentStacks >= m_maxStacks) return;

            m_currentStacks++;
            m_stackTimer = m_stackDecayTime;
        }

        private void DecayStack()
        {
            m_currentStacks = Mathf.Max(0, m_currentStacks - 1);
            m_stackTimer = m_stackDecayTime;
        }

        public float GetEffectiveDuration(float baseDuration)
        {
            return baseDuration + (m_currentStacks * m_stackDurationBonus);
        }

        public float GetEffectiveDamage(float baseDamage)
        {
            return baseDamage * StackMultiplier;
        }
    }
}
