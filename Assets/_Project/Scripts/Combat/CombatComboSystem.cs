using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Combat
{
    public class CombatComboSystem : MonoBehaviour
    {
        [Serializable]
        public class ComboStep
        {
            public string animationName;
            public float damageMultiplier = 1f;
            public float range = 2f;
            public float angle = 180f;
            public float knockbackForce = 5f;
            public float startupTime = 0.1f;
            public float activeTime = 0.2f;
            public float recoveryTime = 0.3f;
            public int rageGain = 5;
            public string[] nextComboInputs;
        }

        [Header("Combo Settings")]
        [SerializeField] private ComboStep[] m_comboSequence;
        [SerializeField] private float m_comboWindow = 0.5f;
        [SerializeField] private int m_maxCombo = 5;
        [SerializeField] private float m_comboDecayTime = 2f;

        [Header("Input")]
        [SerializeField] private KeyCode[] m_attackKeys = new KeyCode[] { KeyCode.Mouse0 };
        [SerializeField] private bool m_bufferInputs = true;
        [SerializeField] private int m_inputBufferSize = 3;

        [Header("Rage")]
        [SerializeField] private float m_maxRage = 100f;
        [SerializeField] private float m_rageDecayRate = 2f;
        [SerializeField] private float m_ragePerHit = 10f;

        private int m_currentComboIndex;
        private float m_comboTimer;
        private float m_rage;
        private bool m_isInCombo;
        private bool m_isAttacking;
        private Animator m_animator;
        private Queue<InputCommand> m_inputBuffer = new Queue<InputCommand>();
        private List<string> m_possibleNextInputs = new List<string>();

        public event Action<int> OnComboStep;
        public event Action OnComboEnd;
        public event Action OnComboBroken;
        public event Action<int> OnRageChanged;

        public float Rage => m_rage;
        public float MaxRage => m_maxRage;
        public int CurrentCombo => m_currentComboIndex;

        private class InputCommand
        {
            public string input;
            public float timestamp;
        }

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            BuildComboGraph();
        }

        private void BuildComboGraph()
        {
            m_possibleNextInputs.Clear();
            if (m_comboSequence == null || m_comboSequence.Length == 0) return;

            m_possibleNextInputs.Add(m_comboSequence[0].animationName);

            for (int i = 0; i < m_comboSequence.Length; i++)
            {
                if (m_comboSequence[i].nextComboInputs != null)
                {
                    foreach (string input in m_comboSequence[i].nextComboInputs)
                    {
                        if (!m_possibleNextInputs.Contains(input))
                            m_possibleNextInputs.Add(input);
                    }
                }
            }
        }

        private void Update()
        {
            UpdateComboDecay();
            UpdateRageDecay();
            ProcessInputBuffer();

            foreach (KeyCode key in m_attackKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    HandleAttackInput();
                }
            }
        }

        private void HandleAttackInput()
        {
            if (m_isAttacking)
            {
                if (m_bufferInputs)
                    BufferInput("Attack");
                return;
            }

            ExecuteComboStep();
        }

        private void ExecuteComboStep()
        {
            if (m_comboSequence == null || m_comboSequence.Length == 0) return;

            int stepIndex = Mathf.Min(m_currentComboIndex, m_comboSequence.Length - 1);
            if (stepIndex >= m_comboSequence.Length)
                stepIndex = 0;

            ComboStep step = m_comboSequence[stepIndex];

            m_isInCombo = true;
            m_isAttacking = true;
            m_comboTimer = m_comboWindow;

            if (m_animator != null)
            {
                m_animator.SetTrigger(step.animationName);
            }

            OnComboStep?.Invoke(m_currentComboIndex);

            StartCoroutine(ExecuteStepRoutine(step));
        }

        private System.Collections.IEnumerator ExecuteStepRoutine(ComboStep step)
        {
            yield return new WaitForSeconds(step.startupTime);

            ActiveFrames(step);

            yield return new WaitForSeconds(step.activeTime);

            Recovery(step);

            yield return new WaitForSeconds(step.recoveryTime);

            EndStep();
        }

        private void ActiveFrames(ComboStep step)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, step.range);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Vector3 toTarget = (hit.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, toTarget);

                    if (angle <= step.angle * 0.5f)
                    {
                        DealDamage(hit.gameObject, step);
                        ApplyKnockback(hit.gameObject, step.knockbackForce);

                        m_rage = Mathf.Min(m_rage + m_ragePerHit, m_maxRage);
                        OnRageChanged?.Invoke(Mathf.RoundToInt(m_rage));
                    }
                }
            }
        }

        private void DealDamage(GameObject target, ComboStep step)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health == null) return;

            float baseDamage = health.AttackDamage;
            float finalDamage = baseDamage * step.damageMultiplier;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageData damage = new DamageData(finalDamage);
                damageable.TakeDamage(damage);
            }
        }

        private void ApplyKnockback(GameObject target, float force)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0.5f;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        private void Recovery(ComboStep step)
        {
        }

        private void EndStep()
        {
            m_isAttacking = false;
            m_currentComboIndex++;

            if (m_currentComboIndex >= m_maxCombo)
            {
                ResetCombo();
            }
        }

        private void UpdateComboDecay()
        {
            if (!m_isInCombo) return;

            m_comboTimer -= Time.deltaTime;
            if (m_comboTimer <= 0)
            {
                ResetCombo();
            }
        }

        private void ResetCombo()
        {
            if (m_currentComboIndex > 0)
            {
                OnComboEnd?.Invoke();
            }

            m_currentComboIndex = 0;
            m_isInCombo = false;
            m_isAttacking = false;
        }

        private void BreakCombo()
        {
            OnComboBroken?.Invoke();
            ResetCombo();
        }

        private void UpdateRageDecay()
        {
            if (m_rage > 0)
            {
                m_rage -= m_rageDecayRate * Time.deltaTime;
                if (m_rage < 0) m_rage = 0;
            }
        }

        private void BufferInput(string input)
        {
            m_inputBuffer.Enqueue(new InputCommand
            {
                input = input,
                timestamp = Time.time
            });

            while (m_inputBuffer.Count > m_inputBufferSize)
            {
                m_inputBuffer.Dequeue();
            }
        }

        private void ProcessInputBuffer()
        {
            while (m_inputBuffer.Count > 0)
            {
                InputCommand cmd = m_inputBuffer.Peek();

                if (Time.time - cmd.timestamp > m_comboWindow)
                {
                    m_inputBuffer.Dequeue();
                }
                else if (!m_isAttacking)
                {
                    m_inputBuffer.Dequeue();
                    ExecuteComboStep();
                }
                else
                {
                    break;
                }
            }
        }

        public void AddRage(float amount)
        {
            m_rage = Mathf.Min(m_rage + amount, m_maxRage);
            OnRageChanged?.Invoke(Mathf.RoundToInt(m_rage));
        }

        public bool UseRage(float amount)
        {
            if (m_rage < amount) return false;

            m_rage -= amount;
            OnRageChanged?.Invoke(Mathf.RoundToInt(m_rage));
            return true;
        }

        public bool IsRageFull()
        {
            return m_rage >= m_maxRage;
        }

        public float GetRagePercent()
        {
            return m_rage / m_maxRage;
        }

        public void CancelAttack()
        {
            StopAllCoroutines();
            m_isAttacking = false;
        }
    }

    public class RageAbility : MonoBehaviour
    {
        [Header("Rage Settings")]
        [SerializeField] private float m_rageCost = 50f;
        [SerializeField] private float m_rageDuration = 10f;
        [SerializeField] private float m_damageBonus = 2f;
        [SerializeField] private float m_speedBonus = 1.5f;

        [Header("Visual")]
        [SerializeField] private GameObject m_rageVFX;
        [SerializeField] private AudioClip m_rageActivationSound;

        private CombatComboSystem m_comboSystem;
        private PlayerController m_playerController;
        private bool m_isRaging;
        private GameObject m_activeVFX;

        public event Action OnRageActivated;
        public event Action OnRageEnded;

        private void Awake()
        {
            m_comboSystem = GetComponent<CombatComboSystem>();
            m_playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (m_isRaging)
            {
                m_rageTimer -= Time.deltaTime;
                if (m_rageTimer <= 0)
                {
                    EndRage();
                }
            }

            if (Input.GetKeyDown(KeyCode.F) && CanActivateRage())
            {
                ActivateRage();
            }
        }

        private float m_rageTimer;

        private bool CanActivateRage()
        {
            return !m_isRaging && m_comboSystem.UseRage(m_rageCost);
        }

        private void ActivateRage()
        {
            m_isRaging = true;
            m_rageTimer = m_rageDuration;

            if (m_rageVFX != null)
            {
                m_activeVFX = Instantiate(m_rageVFX, transform);
            }

            OnRageActivated?.Invoke();
        }

        private void EndRage()
        {
            m_isRaging = false;

            if (m_activeVFX != null)
            {
                Destroy(m_activeVFX);
                m_activeVFX = null;
            }

            OnRageEnded?.Invoke();
        }

        public float GetDamageMultiplier()
        {
            return m_isRaging ? m_damageBonus : 1f;
        }

        public float GetSpeedMultiplier()
        {
            return m_isRaging ? m_speedBonus : 1f;
        }

        public bool IsRaging()
        {
            return m_isRaging;
        }
    }
}
