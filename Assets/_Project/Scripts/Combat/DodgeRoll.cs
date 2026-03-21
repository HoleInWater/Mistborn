using UnityEngine;

namespace Mistborn.Combat
{
    public class DodgeRoll : MonoBehaviour
    {
        [Header("Dodge Settings")]
        [SerializeField] private float m_dodgeDistance = 5f;
        [SerializeField] private float m_dodgeDuration = 0.3f;
        [SerializeField] private float m_dodgeCooldown = 0.8f;
        [SerializeField] private float m_invincibilityDuration = 0.2f;

        [Header("Movement")]
        [SerializeField] private float m_dodgeSpeed = 20f;
        [SerializeField] private AnimationCurve m_dodgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Animation")]
        [SerializeField] private string m_dodgeAnimTrigger = "Dodge";

        [Header("Costs")]
        [SerializeField] private bool m_costStamina = true;
        [SerializeField] private float m_staminaCost = 15f;

        [Header("Audio")]
        [SerializeField] private AudioClip m_dodgeSound;

        private CharacterController m_charController;
        private Animator m_animator;
        private AudioSource m_audioSource;
        private Vector3 m_dodgeDirection;
        private float m_dodgeTimer;
        private float m_cooldownTimer;
        private bool m_isDodging;
        private bool m_isInvincible;
        private Vector3 m_originalPosition;

        public bool IsDodging => m_isDodging;
        public bool CanDodge => m_cooldownTimer <= 0 && !m_isDodging;

        private void Awake()
        {
            m_charController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            m_audioSource = GetComponent<AudioSource>();

            if (m_audioSource == null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (m_cooldownTimer > 0)
            {
                m_cooldownTimer -= Time.deltaTime;
            }

            if (m_isDodging)
            {
                ExecuteDodge();
            }
        }

        public bool TryDodge(Vector3 direction)
        {
            if (!CanDodge) return false;

            if (m_costStamina && !HasStamina()) return false;

            StartDodge(direction);
            return true;
        }

        public bool TryDodge()
        {
            return TryDodge(GetDodgeDirection());
        }

        private Vector3 GetDodgeDirection()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(horizontal, 0, vertical);

            if (inputDir.magnitude < 0.1f)
            {
                inputDir = transform.forward;
            }

            return inputDir.normalized;
        }

        private void StartDodge(Vector3 direction)
        {
            m_isDodging = true;
            m_isInvincible = true;
            m_dodgeTimer = m_dodgeDuration;
            m_cooldownTimer = m_dodgeCooldown;
            m_dodgeDirection = direction.normalized;
            m_originalPosition = transform.position;

            if (m_costStamina)
            {
                ConsumeStamina();
            }

            if (m_audioSource != null && m_dodgeSound != null)
            {
                m_audioSource.PlayOneShot(m_dodgeSound);
            }

            if (m_animator != null)
            {
                m_animator.SetTrigger(m_dodgeAnimTrigger);
            }

            Invoke(nameof(EndInvincibility), m_invincibilityDuration);
        }

        private void ExecuteDodge()
        {
            m_dodgeTimer -= Time.deltaTime;

            float t = 1f - (m_dodgeTimer / m_dodgeDuration);
            float curveValue = m_dodgeCurve.Evaluate(t);

            Vector3 dodgeOffset = m_dodgeDirection * m_dodgeDistance * curveValue;
            Vector3 targetPos = m_originalPosition + dodgeOffset;

            targetPos.y = transform.position.y;

            Vector3 moveDir = (targetPos - transform.position).normalized;
            float speed = m_dodgeSpeed * (1f - t);

            m_charController?.Move(moveDir * speed * Time.deltaTime);

            if (m_dodgeTimer <= 0)
            {
                EndDodge();
            }
        }

        private void EndDodge()
        {
            m_isDodging = false;
            transform.position = new Vector3(transform.position.x, m_originalPosition.y, transform.position.z);
        }

        private void EndInvincibility()
        {
            m_isInvincible = false;
        }

        private bool HasStamina()
        {
            PlayerStamina stamina = GetComponent<PlayerStamina>();
            if (stamina == null) return true;
            return stamina.CurrentStamina >= m_staminaCost;
        }

        private void ConsumeStamina()
        {
            PlayerStamina stamina = GetComponent<PlayerStamina>();
            stamina?.UseStamina(m_staminaCost);
        }

        public void OnDamaged(DamageData damage)
        {
            if (m_isInvincible)
            {
                damage.amount = 0;
            }
        }
    }

    public class PlayerStamina : MonoBehaviour
    {
        [Header("Stamina Settings")]
        [SerializeField] private float m_maxStamina = 100f;
        [SerializeField] private float m_staminaRegen = 20f;
        [SerializeField] private float m_regenDelay = 2f;

        [Header("Costs")]
        [SerializeField] private float m_sprintCost = 5f;
        [SerializeField] private float m_dodgeCost = 15f;

        private float m_currentStamina;
        private float m_regenTimer;

        public float CurrentStamina => m_currentStamina;
        public float MaxStamina => m_maxStamina;
        public float StaminaPercent => m_currentStamina / m_maxStamina;

        private void Awake()
        {
            m_currentStamina = m_maxStamina;
        }

        private void Update()
        {
            if (m_currentStamina < m_maxStamina)
            {
                m_regenTimer += Time.deltaTime;

                if (m_regenTimer >= m_regenDelay)
                {
                    m_currentStamina += m_staminaRegen * Time.deltaTime;
                    m_currentStamina = Mathf.Min(m_currentStamina, m_maxStamina);
                }
            }
        }

        public bool UseStamina(float amount)
        {
            if (m_currentStamina < amount) return false;

            m_currentStamina -= amount;
            m_regenTimer = 0f;
            return true;
        }

        public bool HasStamina(float amount)
        {
            return m_currentStamina >= amount;
        }

        public void RestoreStamina(float amount)
        {
            m_currentStamina = Mathf.Min(m_currentStamina + amount, m_maxStamina);
        }

        public void ResetRegenTimer()
        {
            m_regenTimer = 0f;
        }
    }
}
