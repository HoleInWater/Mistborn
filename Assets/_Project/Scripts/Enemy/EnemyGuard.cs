using UnityEngine;

namespace Mistborn.Enemy
{
    public class EnemyGuard : EnemyBase
    {
        [Header("Guard")]
        [SerializeField] private float m_meleeRange = 2.5f;
        [SerializeField] private Vector3[] m_patrolPoints;
        [SerializeField] private float m_patrolWait = 2f;

        private int m_patrolIndex;
        private float m_waitTimer;
        private bool m_waiting;

        protected override void ExecuteState()
        {
            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);

            if (dist <= m_attackRange)
                SetState(State.Attack);
            else if (dist <= m_detectionRange)
                SetState(State.Chase);
            else if (m_patrolPoints != null && m_patrolPoints.Length > 0)
                SetState(State.Patrol);
            else
                SetState(State.Idle);

            base.ExecuteState();
        }

        protected override void Patrol()
        {
            if (m_waiting)
            {
                m_waitTimer -= Time.deltaTime;
                if (m_waitTimer <= 0)
                {
                    m_waiting = false;
                    m_patrolIndex = (m_patrolIndex + 1) % m_patrolPoints.Length;
                }
                return;
            }

            Vector3 target = m_patrolPoints[m_patrolIndex];
            float dist = Vector3.Distance(transform.position, target);

            if (dist < 1f)
            {
                m_waiting = true;
                m_waitTimer = m_patrolWait;
                return;
            }

            MoveTo(target);
        }
    }
}
