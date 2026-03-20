// ============================================================
// FILE: EnemyCoinshot.cs
// SYSTEM: Combat
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Enemy that can use Steel and Iron Allomancy.
//   Uses the same physics system as the player.
//
// LORE:
//   Steel Inquisitors and some soldiers can be Allomancers.
//   Enemy Coinshots use push/pull against the player.
//
// TODO:
//   - Balance force values vs player
//   - Add metal reserves management
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class EnemyCoinshot : EnemyBase
    {
        [Header("Allomancy")]
        public float pushForce = 400f;
        public float pullForce = 400f;
        public float metalRange = 20f;
        public float metalDrainRate = 5f;
        
        [Header("Metal Reserves")]
        public float steelReserve = 50f;
        public float ironReserve = 50f;
        
        private AllomancerController allomancer;
        
        protected override void Start()
        {
            base.Start();
            
            // Give this enemy Allomancer abilities
            allomancer = gameObject.AddComponent<AllomancerController>();
            if (allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Steel).currentAmount = steelReserve;
                allomancer.GetReserve(AllomanticMetal.Iron).currentAmount = ironReserve;
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            // Drain metal while using abilities
            DrainMetal();
        }
        
        protected override void Attack()
        {
            if (player == null) return;
            
            float distance = Vector3.Distance(transform.position, player.position);
            
            // Try to push player away if close
            if (distance < metalRange && CanUseMetal(AllomanticMetal.Steel))
            {
                PushPlayer();
            }
            // Try to pull player if far
            else if (distance > attackRange && distance < metalRange && CanUseMetal(AllomanticMetal.Iron))
            {
                PullPlayer();
            }
        }
        
        private bool CanUseMetal(AllomanticMetal metal)
        {
            return allomancer != null && allomancer.CanBurn(metal);
        }
        
        private void PushPlayer()
        {
            if (!CanUseMetal(AllomanticMetal.Steel)) return;
            
            Vector3 pushDirection = (player.position - transform.position).normalized;
            
            // Check if player has metal (weapons, armor)
            // For now, just apply knockback force
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(pushDirection * pushForce * 0.3f, ForceMode.Impulse);
            }
            
            allomancer.StartBurning(AllomanticMetal.Steel);
        }
        
        private void PullPlayer()
        {
            if (!CanUseMetal(AllomanticMetal.Iron)) return;
            
            Vector3 pullDirection = (transform.position - player.position).normalized;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(pullDirection * pullForce * 0.3f, ForceMode.Impulse);
            }
            
            allomancer.StartBurning(AllomanticMetal.Iron);
        }
        
        private void DrainMetal()
        {
            if (allomancer == null) return;
            
            // Stop burning if not in combat
            if (currentState != AIState.Chase && currentState != AIState.Attack)
            {
                allomancer.StopBurning(AllomanticMetal.Steel);
                allomancer.StopBurning(AllomanticMetal.Iron);
            }
        }
        
        protected override void OnDeath()
        {
            // Stop all metal burning
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Steel);
                allomancer.StopBurning(AllomanticMetal.Iron);
            }
            
            base.OnDeath();
        }
    }
}
