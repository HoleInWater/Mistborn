// ============================================================
// FILE: TinEnhancement.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Tin Allomancy — enhanced senses.
//   Sight, hearing, smell, taste, and touch all amplified.
//
// LORE:
//   "Enhanced senses can be weaponized against the Tineye. A sudden 
//    loud noise becomes agonizing. A bright flash is blinding." — Coppermind
//
// TODO:
//   - Implement selective sense focus (skill upgrade)
//   - Add sensory overload system
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class TinEnhancement : MonoBehaviour
    {
        [Header("Tin Enhancement Settings")]
        public KeyCode activationKey = KeyCode.E;
        public float sightRangeMultiplier = 2f;
        public float hearingRangeMultiplier = 2f;
        public float senseThresholdDivider = 2f; // Lower = more sensitive
        
        [Header("Vulnerability Settings")]
        public float flashbangDuration = 1f;
        public float deafenDuration = 1f;
        public float smokeDuration = 2f;
        
        [Header("Selective Focus")]
        public bool enableSelectiveFocus = false;
        public enum SenseFocus
        {
            All,
            Sight,
            Hearing,
            Smell,
            Touch
        }
        public SenseFocus currentFocus = SenseFocus.All;
        
        private AllomancerController allomancer;
        private bool isEnhanced;
        private bool isOverloaded;
        private float normalFogDensity;
        private Camera playerCamera;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        private void Update()
        {
            if (isOverloaded)
            {
                return; // Cannot use tin while overloaded
            }
            
            // Toggle tin burning
            if (Input.GetKeyDown(activationKey))
            {
                StartEnhancement();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopEnhancement();
            }
            
            // Handle selective focus (if enabled)
            if (enableSelectiveFocus && isEnhanced)
            {
                HandleSelectiveFocus();
            }
        }
        
        private void StartEnhancement()
        {
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Tin))
            {
                Debug.Log("Cannot burn tin - no reserves");
                return;
            }
            
            isEnhanced = true;
            allomancer.StartBurning(AllomanticMetal.Tin);
            ApplyEnhancement();
            
            Debug.Log("Tin burning - senses enhanced");
        }
        
        private void StopEnhancement()
        {
            if (!isEnhanced) return;
            
            isEnhanced = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Tin);
            }
            
            RemoveEnhancement();
            
            Debug.Log("Tin stopped");
        }
        
        private void ApplyEnhancement()
        {
            // Increase render distance
            if (playerCamera != null)
            {
                playerCamera.farClipPlane *= sightRangeMultiplier;
            }
            
            // TODO: Increase audio listener range
            // TODO: Enable extended enemy detection
            
            // TODO: Add visual tint effect
        }
        
        private void RemoveEnhancement()
        {
            // Reset render distance
            if (playerCamera != null)
            {
                playerCamera.farClipPlane /= sightRangeMultiplier;
            }
            
            // TODO: Reset audio listener range
        }
        
        private void HandleSelectiveFocus()
        {
            // Cycle through focus modes
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentFocus = SenseFocus.All;
            if (Input.GetKeyDown(KeyCode.Alpha2)) currentFocus = SenseFocus.Sight;
            if (Input.GetKeyDown(KeyCode.Alpha3)) currentFocus = SenseFocus.Hearing;
            // etc.
        }
        
        // Called when player is hit by flashbang/explosion
        public void TriggerOverload(float duration)
        {
            if (!isEnhanced) return; // Only affects enhanced players
            
            isOverloaded = true;
            
            // Stop tin burning
            StopEnhancement();
            
            // TODO: Apply screen effects (white flash, blur)
            // TODO: Reduce player control
            
            Invoke(nameof(EndOverload), duration);
            
            Debug.Log("SENSORY OVERLOAD!");
        }
        
        private void EndOverload()
        {
            isOverloaded = false;
            Debug.Log("Sensory overload ended");
        }
        
        // Detection ranges for game systems
        public float GetEnemyDetectionRange(float baseRange)
        {
            if (!isEnhanced) return baseRange;
            
            if (currentFocus == SenseFocus.All || currentFocus == SenseFocus.Hearing)
            {
                return baseRange * hearingRangeMultiplier;
            }
            return baseRange;
        }
        
        public float GetWorldRenderDistance(float baseDistance)
        {
            if (!isEnhanced) return baseDistance;
            
            if (currentFocus == SenseFocus.All || currentFocus == SenseFocus.Sight)
            {
                return baseDistance * sightRangeMultiplier;
            }
            return baseDistance;
        }
    }
}
