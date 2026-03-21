// ============================================================
// FILE: EnemySeeker.cs
// SYSTEM: Combat
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Bronze-burning enemy that can detect Allomancers.
//   Alert system when player uses metals.
//
// LORE:
//   "An unskilled Seeker only knows someone is burning something.
//    A skilled Seeker can identify which specific metal." — Coppermind
//
// TODO:
//   - Add detection alert system
//   - Add call for backup behavior
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class EnemySeeker : EnemyBase
    {
        [Header("Detection")]
        public float detectionRange = 30f;
        public float detectionInterval = 0.5f;
        public bool alertOnDetection = true;
        
        [Header("Seeker Specific")]
        public float bronzeDrainRate = 2f;
        public bool isAlwaysDetecting = true;
        
        private float bronzeReserve = 100f;
        private float detectionTimer;
        private bool isDetectingAllomancy;
        private bool playerDetected;
        
        protected override void Start()
        {
            base.Start();
            isDetectingAllomancy = isAlwaysDetecting;
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isAlwaysDetecting)
            {
                DetectAllomancy();
            }
        }
        
        protected override void UpdateAI()
        {
            if (playerDetected)
            {
                // Once detected, chase and attack normally
                base.UpdateAI();
            }
            else
            {
                // Patrol or idle until detection
                currentState = AIState.Patrol;
                base.UpdateAI();
            }
        }
        
        private void DetectAllomancy()
        {
            if (bronzeReserve <= 0) return;
            
            detectionTimer -= Time.deltaTime;
            if (detectionTimer > 0) return;
            
            detectionTimer = detectionInterval;
            
            // Drain bronze while detecting
            bronzeReserve -= bronzeDrainRate * detectionInterval;
            if (bronzeReserve < 0) bronzeReserve = 0;
            
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > detectionRange) return;
            
            // Check if player is burning metals
            AllomancerController playerAllomancy = player.GetComponent<AllomancerController>();
            if (playerAllomancy != null && IsPlayerBurningAnyMetal(playerAllomancy))
            {
                TriggerDetection();
            }
            
            // Check for coppercloud (blocks detection)
            // TODO: Check if player is in a coppercloud
        }
        
        private bool IsPlayerBurningAnyMetal(AllomancerController allomancy)
        {
            // Check if any metal is currently burning
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
            if (playerDetected) return;
            
            playerDetected = true;
            
            if (alertOnDetection)
            {
                Debug.Log("SEEKER DETECTED PLAYER!");
                // TODO: Alert nearby enemies
                // TODO: Play alert sound/animation
                // TODO: Trigger alert state in enemy AI
            }
        }
        
        protected override void ChasePlayer()
        {
            // Seekers might have unique chase behavior
            base.ChasePlayer();
        }
        
        protected override void Attack()
        {
            // Seekers don't attack directly, they detect
            // But they might call for backup or flee
            base.Attack();
        }
        
        public bool HasDetectedPlayer()
        {
            return playerDetected;
        }
        
        public void ResetDetection()
        {
            playerDetected = false;
        }
    }
}
