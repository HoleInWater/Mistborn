// ============================================================
// FILE: SteelpushAssistedJump.cs
// SYSTEM: Player / Movement
// STATUS: PLANNED — Sprint 2
// AUTHOR: 
//
// PURPOSE:
//   Allows the player to use anchored Steelpush for super-jumps.
//   Push off metal below to launch upward.
//
// LORE:
//   "This functions through essentially attempting to push the 
//    planet itself, which... causes one to be pushed away." — Coppermind
//
//   "Coinshots commonly crouch down before they push, to give 
//    the push a little more lift." — Coppermind
//
// TODO (AI Agent):
//   - Detect anchored metal below player
//   - Calculate launch force based on distance and metal mass
//   - Add visual feedback (crouch, launch, trail)
//
// TODO (Team):
//   - Launch force balance
//   - Air control during ascent/descent
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Player
{
    public class SteelpushAssistedJump : MonoBehaviour
    {
        [Header("Assisted Jump Settings")]
        public float baseLaunchForce = 15f;
        public float maxLaunchForce = 40f;
        public float minMetalMass = 50f; // Minimum mass to push off
        public float detectionRange = 10f;
        public float maxDetectionAngle = 45f; // Below player only
        
        [Header("Timing")]
        public float holdDuration = 0.5f; // Hold to charge
        public float maxChargeTime = 2f;
        public float releaseBoost = 1.5f;
        
        [Header("References")]
        public KeyCode activateKey = KeyCode.Space;
        public LayerMask metalLayer;
        
        private AllomancerController allomancer;
        private Rigidbody playerRb;
        private bool isCharging;
        private float chargeStartTime;
        private AllomanticTarget currentAnchor;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            playerRb = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            // Only work if burning steel
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Steel))
                return;
            
            // Find anchor below
            currentAnchor = FindAnchorBelow();
            
            // Charge and release
            if (Input.GetKeyDown(activateKey) && currentAnchor != null && IsGrounded())
            {
                StartCharge();
            }
            
            if (Input.GetKeyUp(activateKey) && isCharging)
            {
                ExecuteLaunch();
            }
            
            if (isCharging && Time.time - chargeStartTime >= maxChargeTime)
            {
                ExecuteLaunch(); // Auto-release at max
            }
        }
        
        private AllomanticTarget FindAnchorBelow()
        {
            RaycastHit[] hits = Physics.RaycastAll(
                transform.position, 
                Vector3.down, 
                detectionRange, 
                metalLayer
            );
            
            foreach (RaycastHit hit in hits)
            {
                AllomanticTarget target = hit.collider.GetComponent<AllomanticTarget>();
                if (target != null && target.isAnchored && target.metalMass >= minMetalMass)
                {
                    // Check angle (must be somewhat below)
                    float angle = Vector3.Angle(Vector3.down, hit.normal);
                    if (angle <= maxDetectionAngle)
                    {
                        return target;
                    }
                }
            }
            
            return null;
        }
        
        private void StartCharge()
        {
            isCharging = true;
            chargeStartTime = Time.time;
            allomancer.StartBurning(AllomanticMetal.Steel);
            
            // TODO: Play charge effect (blue glow, crouch animation)
        }
        
        private void ExecuteLaunch()
        {
            if (!isCharging || currentAnchor == null) return;
            
            isCharging = false;
            
            // Calculate charge multiplier
            float chargeTime = Time.time - chargeStartTime;
            float chargeMultiplier = Mathf.Clamp(chargeTime / holdDuration, 1f, releaseBoost);
            
            // Calculate launch force based on anchor mass
            float massMultiplier = Mathf.Clamp(currentAnchor.metalMass / 100f, 0.5f, 2f);
            float force = baseLaunchForce * chargeMultiplier * massMultiplier;
            force = Mathf.Min(force, maxLaunchForce);
            
            // Launch upward
            playerRb.AddForce(Vector3.up * force, ForceMode.Impulse);
            
            // Stop burning steel (or keep going for sustained flight?)
            allomancer.StopBurning(AllomanticMetal.Steel);
            
            // TODO: Play launch effect
        }
        
        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }
    }
}
