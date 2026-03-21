using UnityEngine;
using Mistborn.Allomancy;

namespace Mistborn.Enemy
{
    public class EnemyCoinshot : EnemyBase
    {
        [Header("Allomancy")]
        [SerializeField] private float m_pushForce = 400f;
        [SerializeField] private float m_pullForce = 400f;
        [SerializeField] private float m_metalRange = 20f;
        [SerializeField] private float m_steelReserve = 50f;
        [SerializeField] private float m_ironReserve = 50f;

        private AllomancerController m_allomancer;

        protected override void Start()
        {
            base.Start();
            m_allomancer = gameObject.AddComponent<AllomancerController>();
            m_allomancer.GetReserve(AllomanticMetal.Steel).SetAmount(m_steelReserve);
            m_allomancer.GetReserve(AllomanticMetal.Iron).SetAmount(m_ironReserve);
        }

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
                SetState(State.Attack);
            else if (dist <= m_detectionRange)
                SetState(State.Chase);
            else
                SetState(State.Patrol);

            base.ExecuteState();
        }

        protected override void Attack()
        {
            if (Time.time - m_lastAttack < m_attackCooldown) return;
            m_lastAttack = Time.time;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist < m_metalRange && CanUseMetal(AllomanticMetal.Steel))
                PushPlayer();
            else if (dist > m_attackRange && dist < m_metalRange && CanUseMetal(AllomanticMetal.Iron))
                PullPlayer();
        }

        private bool CanUseMetal(AllomanticMetal metal) =>
            m_allomancer != null && m_allomancer.CanBurn(metal);

        private void PushPlayer()
        {
            if (!CanUseMetal(AllomanticMetal.Steel)) return;
            Vector3 dir = (m_player.position - transform.position).normalized;
            m_player.GetComponent<Rigidbody>()?.AddForce(dir * m_pushForce * 0.3f, ForceMode.Impulse);
            m_allomancer.StartBurning(AllomanticMetal.Steel);
        }

        private void PullPlayer()
        {
            if (!CanUseMetal(AllomanticMetal.Iron)) return;
            Vector3 dir = (transform.position - m_player.position).normalized;
            m_player.GetComponent<Rigidbody>()?.AddForce(dir * m_pullForce * 0.3f, ForceMode.Impulse);
            m_allomancer.StartBurning(AllomanticMetal.Iron);
        }

        protected override void Die()
        {
            m_allomancer?.StopBurning(AllomanticMetal.Steel);
            m_allomancer?.StopBurning(AllomanticMetal.Iron);
            base.Die();
        }
    }
}
