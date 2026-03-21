using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Enemy
{
    public class EnemyPewterArmsmaster : EnemyBase
    {
        [Header("Pewter Enhancement")]
        [SerializeField] private float m_strengthMultiplier = 2f;
        [SerializeField] private float m_speedMultiplier = 1.5f;
        [SerializeField] private float m_burnDuration = 8f;
        [SerializeField] private float m_recoveryDuration = 5f;

        [Header("Combat")]
        [SerializeField] private float m_powerAttackDamage = 35f;
        [SerializeField] private float m_powerAttackRange = 4f;
        [SerializeField] private float m_slamRadius = 3f;
        [SerializeField] private float m_slamDamage = 20f;

        [Header("Combat Arts")]
        [SerializeField] private string[] m_combatArts = { "PowerStrike", "GroundSlam", "Whirlwind" };
        [SerializeField] private float m_artCooldown = 10f;

        private bool m_isBurningPewter;
        private float m_pewterTimer;
        private float m_artTimer;
        private string m_currentArt;
        private int m_artIndex;
        private Animator m_animator;
        private CharacterController m_charController;

        protected override void Awake()
        {
            base.Awake();
            m_animator = GetComponent<Animator>();
            m_charController = GetComponent<CharacterController>();

            m_maxHealth = 150f;
            m_currentHealth = m_maxHealth;
            m_damage = 15f;
            m_detectionRange = 20f;
            m_speed = 4f;
            m_attackRange = 2.5f;
        }

        protected override void Start()
        {
            base.Start();
            StartPewterBurn();
        }

        protected override void Update()
        {
            if (isDead) return;

            UpdatePewterState();
            UpdateArtCooldown();

            if (!m_isBurningPewter)
            {
                m_pewterTimer -= Time.deltaTime;
                if (m_pewterTimer <= 0)
                {
                    StartPewterBurn();
                }
            }

            base.Update();
        }

        private void UpdatePewterState()
        {
            if (!m_isBurningPewter) return;

            m_pewterTimer -= Time.deltaTime;

            if (m_pewterTimer <= 0)
            {
                EndPewterBurn();
            }
        }

        private void UpdateArtCooldown()
        {
            if (m_artTimer > 0)
            {
                m_artTimer -= Time.deltaTime;
            }
        }

        private void StartPewterBurn()
        {
            m_isBurningPewter = true;
            m_pewterTimer = m_burnDuration;

            m_damage = 15f * m_strengthMultiplier;
            m_speed = 4f * m_speedMultiplier;
            m_attackRange = 2.5f * 1.2f;

            PlayPewterEffect();
        }

        private void EndPewterBurn()
        {
            m_isBurningPewter = false;
            m_pewterTimer = m_recoveryDuration;

            m_damage = 15f;
            m_speed = 4f;
            m_attackRange = 2.5f;
        }

        private void PlayPewterEffect()
        {
            if (m_animator != null)
            {
                m_animator.SetFloat("SpeedMultiplier", m_isBurningPewter ? m_speedMultiplier : 1f);
            }
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (m_artTimer <= 0 && dist <= m_powerAttackRange * 1.5f)
            {
                TryUseCombatArt();
            }
            else if (dist <= m_attackRange)
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

        private void TryUseCombatArt()
        {
            if (m_artTimer > 0) return;

            m_currentArt = m_combatArts[m_artIndex];
            m_artIndex = (m_artIndex + 1) % m_combatArts.Length;
            m_artTimer = m_artCooldown;

            ExecuteCombatArt(m_currentArt);
        }

        private void ExecuteCombatArt(string art)
        {
            switch (art)
            {
                case "PowerStrike":
                    StartCoroutine(PowerStrikeRoutine());
                    break;
                case "GroundSlam":
                    StartCoroutine(GroundSlamRoutine());
                    break;
                case "Whirlwind":
                    StartCoroutine(WhirlwindRoutine());
                    break;
            }
        }

        private System.Collections.IEnumerator PowerStrikeRoutine()
        {
            SetState(State.Chase);

            Vector3 startPos = transform.position;
            Vector3 targetPos = m_player.position;
            Vector3 direction = (targetPos - startPos).normalized;

            transform.rotation = Quaternion.LookRotation(direction);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, targetPos + direction * 2f, t);
                yield return null;
            }

            DealDamageInRadius(transform.position, m_powerAttackRange, m_powerAttackDamage);
        }

        private System.Collections.IEnumerator GroundSlamRoutine()
        {
            if (m_animator != null)
                m_animator.SetTrigger("GroundSlam");

            yield return new WaitForSeconds(0.5f);

            DealDamageInRadius(transform.position, m_slamRadius, m_slamDamage);

            Rigidbody rb = m_player?.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockback = Vector3.up * 10f;
                knockback.y = 8f;
                rb.AddForce(knockback, ForceMode.Impulse);
            }
        }

        private System.Collections.IEnumerator WhirlwindRoutine()
        {
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                transform.Rotate(Vector3.up, 360f * Time.deltaTime);

                Vector3 circlePos = transform.position + transform.right * Mathf.Sin(elapsed * 5f) * 2f;
                m_charController?.Move((circlePos - transform.position).normalized * m_speed * Time.deltaTime);

                DealDamageInRadius(transform.position, 2f, m_damage * 0.5f);

                yield return null;
            }
        }

        private void DealDamageInRadius(Vector3 center, float radius, float damage)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    if (hit.TryGetComponent(out PlayerHealth health))
                    {
                        health.TakeDamage(new DamageData(damage));
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
                    health.TakeDamage(new DamageData(m_damage));
                }
            }
        }

        public override void TakeDamage(DamageData damage)
        {
            if (m_isBurningPewter)
            {
                damage.amount *= 0.5f;
            }

            base.TakeDamage(damage);
        }
    }
}
