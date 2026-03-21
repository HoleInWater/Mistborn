using UnityEngine;
using Mistborn.Allomancy;

namespace Mistborn.Enemy
{
    public class EnemySeeker : EnemyBase
    {
        [Header("Detection")]
        [SerializeField] private float m_detectionRange = 30f;
        [SerializeField] private float m_detectionInterval = 0.5f;
        [SerializeField] private bool m_alertOnDetection = true;

        [Header("Seeker Specific")]
        [SerializeField] private float m_bronzeDrainRate = 2f;
        [SerializeField] private bool m_isAlwaysDetecting = true;

        private float m_bronzeReserve = 100f;
        private float m_detectionTimer;
        private bool m_isDetectingAllomancy;
        private bool m_playerDetected;

        protected override void Start()
        {
            base.Start();
            m_isDetectingAllomancy = m_isAlwaysDetecting;
        }

        protected override void Update()
        {
            base.Update();

            if (m_isAlwaysDetecting)
            {
                DetectAllomancy();
            }
        }

        protected override void UpdateAI()
        {
            if (m_playerDetected)
            {
                base.UpdateAI();
            }
            else
            {
                SetState(State.Patrol);
                base.UpdateAI();
            }
        }

        private void DetectAllomancy()
        {
            if (m_bronzeReserve <= 0) return;

            m_detectionTimer -= Time.deltaTime;
            if (m_detectionTimer > 0) return;

            m_detectionTimer = m_detectionInterval;

            m_bronzeReserve -= m_bronzeDrainRate * m_detectionInterval;
            if (m_bronzeReserve < 0) m_bronzeReserve = 0;

            if (m_player == null) return;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist > m_detectionRange) return;

            AllomancerController playerAllomancy = m_player.GetComponent<AllomancerController>();
            if (playerAllomancy != null && IsPlayerBurningAnyMetal(playerAllomancy))
            {
                TriggerDetection();
            }
        }

        private bool IsPlayerBurningAnyMetal(AllomancerController allomancy)
        {
            foreach (MetalReserve reserve in allomancy.Reserves)
            {
                if (reserve.IsBurning)
                {
                    return true;
                }
            }
            return false;
        }

        private void TriggerDetection()
        {
            if (m_playerDetected) return;

            m_playerDetected = true;

            if (m_alertOnDetection)
            {
                Debug.Log("SEEKER DETECTED PLAYER!");
            }
        }

        protected override void Attack()
        {
        }

        public bool HasDetectedPlayer()
        {
            return m_playerDetected;
        }

        public void ResetDetection()
        {
            m_playerDetected = false;
        }
    }
}
