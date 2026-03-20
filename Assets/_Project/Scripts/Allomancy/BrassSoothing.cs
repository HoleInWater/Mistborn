// ============================================================
// FILE: BrassSoothing.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Brass Allomancy — Soothing (calming emotions).
//   Reduces aggression, fear, and hostility in targets.
//
// LORE:
//   "Soothing dampens and suppresses emotions. An unskilled Soother 
//    makes people feel vaguely calm. A skilled Soother can suppress 
//    all emotions except one." — Coppermind
//
// TODO:
//   - Integrate with enemy AI emotional states
//   - Add visual effect for targeting
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    public class BrassSoothing : MonoBehaviour
    {
        [Header("Soothing Settings")]
        public KeyCode activationKey = KeyCode.Z;
        public float sootheRadius = 15f;
        public float sootheStrength = 0.5f; // 0-1, how much to calm
        public float burnRate = 3f;
        
        [Header("Targeting")]
        public bool targetEveryone = true; // False = focused on one target
        public float focusedRange = 20f;
        
        [Header("Effects")]
        public float aggressionReduction = 0.3f;
        public float fearReduction = 0.3f;
        
        private AllomancerController allomancer;
        private bool isSoothing;
        private GameObject currentTarget;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                StartSoothing();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopSoothing();
            }
            
            if (isSoothing && allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Brass).Consume(Time.deltaTime * burnRate);
                
                if (allomancer.GetReserve(AllomanticMetal.Brass).IsEmpty())
                {
                    StopSoothing();
                }
            }
        }
        
        public void StartSoothing()
        {
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Brass))
            {
                Debug.Log("Cannot burn brass - no reserves");
                return;
            }
            
            isSoothing = true;
            allomancer.StartBurning(AllomanticMetal.Brass);
            
            Debug.Log("Soothing active - calming emotions");
        }
        
        public void StopSoothing()
        {
            isSoothing = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Brass);
            }
            
            Debug.Log("Soothing ended");
        }
        
        public void ApplySoothingEffect(EnemyBase enemy)
        {
            if (!isSoothing) return;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > sootheRadius) return;
            
            // Reduce enemy aggression
            // TODO: Integrate with enemy emotional state
            float distanceFactor = 1f - (distance / sootheRadius);
            float effectStrength = sootheStrength * distanceFactor;
            
            Debug.Log($"Soothing {enemy.name} with strength {effectStrength}");
        }
        
        public void SetTarget(GameObject target)
        {
            currentTarget = target;
            targetEveryone = false;
        }
    }
}
