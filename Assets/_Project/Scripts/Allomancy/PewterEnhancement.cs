// ============================================================
// FILE: PewterEnhancement.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Pewter Allomancy — enhanced physical abilities.
//   Grants strength, speed, endurance, and pain tolerance.
//
// LORE:
//   "Pewter enhances strength, speed, endurance, balance, and pain 
//    tolerance simultaneously." — Coppermind
//
// TODO:
//   - Tune enhancement multipliers
//   - Add visual effects (pewter glow)
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class PewterEnhancement : MonoBehaviour
    {
        [Header("Pewter Enhancement Settings")]
        public KeyCode activationKey = KeyCode.Q;
        public float strengthMultiplier = 2f;
        public float speedMultiplier = 1.5f;
        public float enduranceMultiplier = 3f;
        public float painResistance = 0.5f; // 50% pain reduction
        public float burnRate = 1.5f;
        
        [Header("Pewter Drag")]
        public float dragDuration = 3f;
        public float dragSpeedPenalty = 0.5f;
        public float dragStrengthPenalty = 0.5f;
        
        private AllomancerController allomancer;
        private CharacterController characterController;
        private bool isEnhanced;
        private bool isDragging;
        private float normalSpeed;
        private float normalStrength;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            characterController = GetComponent<CharacterController>();
            
            if (characterController != null)
            {
                normalSpeed = characterController.slopeLimit / 10f; // Approximate base speed
            }
        }
        
        private void Update()
        {
            if (isDragging)
            {
                UpdateDrag();
                return;
            }
            
            // Toggle pewter burning
            if (Input.GetKeyDown(activationKey))
            {
                StartEnhancement();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopEnhancement();
            }
            
            // Continue burning if active
            if (isEnhanced && allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Pewter).Consume(Time.deltaTime * burnRate);
                
                if (allomancer.GetReserve(AllomanticMetal.Pewter).IsEmpty())
                {
                    TriggerPewterDrag();
                }
            }
        }
        
        private void StartEnhancement()
        {
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Pewter))
            {
                Debug.Log("Cannot burn pewter - no reserves");
                return;
            }
            
            isEnhanced = true;
            allomancer.StartBurning(AllomanticMetal.Pewter);
            ApplyEnhancement();
            
            Debug.Log("Pewter burning - enhanced");
        }
        
        private void StopEnhancement()
        {
            if (!isEnhanced) return;
            
            isEnhanced = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Pewter);
            }
            
            RemoveEnhancement();
            
            Debug.Log("Pewter stopped");
        }
        
        private void ApplyEnhancement()
        {
            // Apply visual effect
            // TODO: Add pewter glow VFX
            
            // Enhance will be applied in movement/combat systems
        }
        
        private void RemoveEnhancement()
        {
            // Remove visual effect
            // TODO: Remove pewter glow VFX
        }
        
        private void TriggerPewterDrag()
        {
            isEnhanced = false;
            isDragging = true;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Pewter);
            }
            
            RemoveEnhancement();
            Invoke(nameof(EndDrag), dragDuration);
            
            Debug.Log("PEWTER DRAG - depleted!");
        }
        
        private void UpdateDrag()
        {
            // During drag, player is slowed and weakened
            // This is handled by checking isDragging in combat/movement
        }
        
        private void EndDrag()
        {
            isDragging = false;
            Debug.Log("Drag ended");
        }
        
        // Called by combat system to check enhancement state
        public bool IsEnhanced()
        {
            return isEnhanced;
        }
        
        public bool IsDragging()
        {
            return isDragging;
        }
        
        public float GetDamageMultiplier()
        {
            if (isDragging) return dragStrengthPenalty;
            if (isEnhanced) return strengthMultiplier;
            return 1f;
        }
        
        public float GetSpeedMultiplier()
        {
            if (isDragging) return dragSpeedPenalty;
            if (isEnhanced) return speedMultiplier;
            return 1f;
        }
        
        public float GetPainResistance()
        {
            if (isEnhanced) return painResistance;
            return 0f;
        }
    }
}
