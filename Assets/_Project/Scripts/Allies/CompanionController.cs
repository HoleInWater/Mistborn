using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Allies
{
    public class CompanionController : MonoBehaviour
    {
        [Header("Companion Settings")]
        [SerializeField] private string m_companionName = "Companion";
        [SerializeField] private float m_followDistance = 3f;
        [SerializeField] private float m_separationDistance = 1.5f;

        [Header("Movement")]
        [SerializeField] private float m_followSpeed = 5f;
        [SerializeField] private float m_combatSpeed = 6f;
        [SerializeField] private float m_rotationSpeed = 10f;

        [Header("Combat")]
        [SerializeField] private float m_attackRange = 2f;
        [SerializeField] private float m_aggroRange = 15f;
        [SerializeField] private float m_attackCooldown = 1f;
        [SerializeField] private float m_attackDamage = 10f;
        [SerializeField] private bool m_autoAttack = true;

        [Header("Abilities")]
        [SerializeField] private bool m_canHeal = true;
        [SerializeField] private float m_healAmount = 20f;
        [SerializeField] private float m_healCooldown = 30f;
        [SerializeField] private float m_healRange = 10f;

        [Header("Behavior")]
        [SerializeField] private CompanionBehavior m_behavior = CompanionBehavior.Follow;
        [SerializeField] private float m_commandDelay = 0.5f;

        [Header("Stats")]
        [SerializeField] private int m_maxHealth = 100;
        [SerializeField] private float m_revivalTime = 10f;

        private Transform m_player;
        private CharacterController m_charController;
        private Animator m_animator;
        private int m_currentHealth;
        private float m_attackTimer;
        private float m_healTimer;
        private bool m_isFollowing;
        private bool m_isInCombat;
        private bool m_isDead;
        private Vector3 m_targetPosition;
        private List<Transform> m_enemiesInRange = new List<Transform>();
        private CompanionState m_state = CompanionState.Idle;

        public event Action<Transform> OnTargetAcquired;
        public event Action OnCombatEnter;
        public event Action OnCombatExit;
        public event Action<int> OnHealthChanged;
        public event Action OnCompanionDeath;
        public event Action OnRevived;

        public int Health => m_currentHealth;
        public int MaxHealth => m_maxHealth;
        public bool IsDead => m_isDead;
        public CompanionBehavior Behavior => m_behavior;

        public enum CompanionBehavior { Follow, Defend, Patrol, Idle }
        public enum CompanionState { Idle, Following, Combat, Healing, Dead }

        private void Awake()
        {
            m_charController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            m_currentHealth = m_maxHealth;
        }

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (m_player != null)
            {
                SetState(CompanionState.Following);
            }
        }

        private void Update()
        {
            if (m_isDead)
            {
                HandleDeath();
                return;
            }

            UpdateTimers();
            UpdateEnemiesInRange();

            switch (m_state)
            {
                case CompanionState.Idle:
                    HandleIdle();
                    break;
                case CompanionState.Following:
                    HandleFollowing();
                    break;
                case CompanionState.Combat:
                    HandleCombat();
                    break;
                case CompanionState.Healing:
                    HandleHealing();
                    break;
            }

            UpdateAnimation();
        }

        private void UpdateTimers()
        {
            if (m_attackTimer > 0)
                m_attackTimer -= Time.deltaTime;

            if (m_healTimer > 0)
                m_healTimer -= Time.deltaTime;
        }

        private void UpdateEnemiesInRange()
        {
            m_enemiesInRange.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, m_aggroRange);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    m_enemiesInRange.Add(hit.transform);
                }
            }
        }

        private void HandleIdle()
        {
        }

        private void HandleFollowing()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist > m_followDistance)
            {
                Vector3 targetPos = m_player.position - m_player.forward * m_followDistance;
                targetPos.y = transform.position.y;

                MoveTo(targetPos, m_followSpeed);
            }
            else
            {
                m_charController?.Move(Vector3.zero);
            }

            if (m_autoAttack && m_enemiesInRange.Count > 0)
            {
                SetState(CompanionState.Combat);
            }
        }

        private void HandleCombat()
        {
            if (m_enemiesInRange.Count == 0)
            {
                SetState(CompanionState.Following);
                return;
            }

            Transform nearest = GetNearestEnemy();
            if (nearest == null) return;

            float dist = Vector3.Distance(transform.position, nearest.position);

            if (dist > m_attackRange)
            {
                MoveTo(nearest.position, m_combatSpeed);
            }
            else
            {
                AttackTarget(nearest);
            }
        }

        private void HandleHealing()
        {
            if (m_player == null)
            {
                SetState(CompanionState.Following);
                return;
            }

            PlayerHealth playerHealth = m_player.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                if (m_healTimer <= 0)
                {
                    HealPlayer();
                }
            }
            else
            {
                SetState(CompanionState.Following);
            }
        }

        private void HandleDeath()
        {
        }

        private Transform GetNearestEnemy()
        {
            if (m_enemiesInRange.Count == 0) return null;

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Transform enemy in m_enemiesInRange)
            {
                if (enemy == null) continue;

                float dist = Vector3.Distance(transform.position, enemy.position);
                if (dist < nearestDist)
                {
                    nearest = enemy;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        private void MoveTo(Vector3 target, float speed)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            m_charController?.Move(direction * speed * Time.deltaTime);

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
            }
        }

        private void AttackTarget(Transform target)
        {
            if (m_attackTimer > 0) return;

            m_attackTimer = m_attackCooldown;

            transform.LookAt(target);

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(new DamageData(m_attackDamage));
            }

            if (m_animator != null)
            {
                m_animator.SetTrigger("Attack");
            }
        }

        private void HealPlayer()
        {
            if (!m_canHeal) return;
            if (m_healTimer > 0) return;

            PlayerHealth playerHealth = m_player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(m_healAmount);
            }

            m_healTimer = m_healCooldown;

            if (m_animator != null)
            {
                m_animator.SetTrigger("Heal");
            }
        }

        private void SetState(CompanionState newState)
        {
            if (m_state == newState) return;

            m_state = newState;

            switch (m_state)
            {
                case CompanionState.Combat:
                    OnCombatEnter?.Invoke();
                    break;
                case CompanionState.Following:
                    OnCombatExit?.Invoke();
                    break;
            }
        }

        private void UpdateAnimation()
        {
            if (m_animator == null) return;

            float speed = m_charController != null ? m_charController.velocity.magnitude : 0f;
            m_animator.SetFloat("Speed", speed);
        }

        public void TakeDamage(int amount)
        {
            if (m_isDead) return;

            m_currentHealth -= amount;
            OnHealthChanged?.Invoke(m_currentHealth);

            if (m_currentHealth <= 0)
            {
                m_currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            m_isDead = true;
            SetState(CompanionState.Dead);

            if (m_animator != null)
            {
                m_animator.SetTrigger("Death");
            }

            OnCompanionDeath?.Invoke();
        }

        public void Revive(int healthAmount = 0)
        {
            m_isDead = false;
            m_currentHealth = healthAmount > 0 ? healthAmount : m_maxHealth;

            if (m_animator != null)
            {
                m_animator.SetTrigger("Revive");
            }

            SetState(CompanionState.Following);
            OnRevived?.Invoke();
        }

        public void SetBehavior(CompanionBehavior behavior)
        {
            m_behavior = behavior;
        }

        public void Follow()
        {
            SetState(CompanionState.Following);
        }

        public void HoldPosition()
        {
            SetState(CompanionState.Idle);
        }

        public void AttackTargetCommand(Transform target)
        {
            if (target != null)
            {
                m_enemiesInRange.Add(target);
                SetState(CompanionState.Combat);
            }
        }

        public void HealCommand()
        {
            SetState(CompanionState.Healing);
        }

        public void StayHere()
        {
            SetState(CompanionState.Idle);
            m_targetPosition = transform.position;
        }
    }

    public class CompanionCommands : MonoBehaviour
    {
        [Header("Commands")]
        [SerializeField] private KeyCode m_followCommand = KeyCode.F;
        [SerializeField] private KeyCode m_holdCommand = KeyCode.H;
        [SerializeField] private KeyCode m_healCommand = KeyCode.J;
        [SerializeField] private KeyCode m_attackCommand = KeyCode.K;

        private CompanionController m_companion;

        private void Start()
        {
            m_companion = FindObjectOfType<CompanionController>();
        }

        private void Update()
        {
            if (m_companion == null) return;

            if (Input.GetKeyDown(m_followCommand))
            {
                m_companion.Follow();
            }

            if (Input.GetKeyDown(m_holdCommand))
            {
                m_companion.HoldPosition();
            }

            if (Input.GetKeyDown(m_healCommand))
            {
                m_companion.HealCommand();
            }

            if (Input.GetKeyDown(m_attackCommand))
            {
                LockOnTargeting targeting = FindObjectOfType<LockOnTargeting>();
                if (targeting != null)
                {
                    m_companion.AttackTargetCommand(targeting.CurrentTarget);
                }
            }
        }
    }
}
