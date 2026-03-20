// ============================================================
// FILE: ZincRioting.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Zinc Allomancy — Rioting (amplifying emotions).
//   Increases aggression, fear, passion in targets.
//
// LORE:
//   "Rioting amplifies and inflames an emotion that already exists.
//    Cannot create emotions from nothing." — Coppermind
//
// TODO:
//   - Integrate with enemy AI emotional states
//   - Add specific emotion targeting
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class ZincRioting : MonoBehaviour
    {
        [Header("Rioting Settings")]
        public KeyCode activationKey = KeyCode.X;
        public float riotRadius = 15f;
        public float riotStrength = 0.5f;
        public float burnRate = 3f;
        
        [Header("Emotions")]
        public enum RiotEmotion
        {
            Aggression,
            Fear,
            Passion,
            All
        }
        public RiotEmotion targetEmotion = RiotEmotion.Aggression;
        
        private AllomancerController allomancer;
        private bool isRioting;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                StartRioting();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopRioting();
            }
            
            if (isRioting && allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Zinc).Consume(Time.deltaTime * burnRate);
                
                if (allomancer.GetReserve(AllomanticMetal.Zinc).IsEmpty())
                {
                    StopRioting();
                }
            }
        }
        
        public void StartRioting()
        {
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Zinc))
            {
                Debug.Log("Cannot burn zinc - no reserves");
                return;
            }
            
            isRioting = true;
            allomancer.StartBurning(AllomanticMetal.Zinc);
            
            Debug.Log($"Rioting active - amplifying {targetEmotion}");
        }
        
        public void StopRioting()
        {
            isRioting = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Zinc);
            }
            
            Debug.Log("Rioting ended");
        }
        
        public void ApplyRiotEffect(EnemyBase enemy)
        {
            if (!isRioting) return;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > riotRadius) return;
            
            float distanceFactor = 1f - (distance / riotRadius);
            float effectStrength = riotStrength * distanceFactor;
            
            Debug.Log($"Rioting {enemy.name}'s {targetEmotion} with strength {effectStrength}");
            
            // TODO: Apply emotion amplification to enemy AI
            // This could make guards attack each other
            // Or make enemies flee in fear
            // Or make them fight with more passion (damage bonus)
        }
    }
}
