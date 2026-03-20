// ============================================================
// FILE: AllomancerAnimationController.cs
// SYSTEM: Animation
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Bridges Allomancy system with animation states.
//   Triggers animations based on metal burning and abilities.
//
// TODO:
//   - Hook up to actual animator
//   - Add animation events
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Player
{
    public class AllomancerAnimationController : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;
        public AllomancerController allomancer;
        
        [Header("States")]
        public string steelPushBool = "isSteelPushing";
        public string ironPullBool = "isIronPulling";
        public string pewterBool = "isPewterBurning";
        public string tinBool = "isTinBurning";
        
        [Header("Parameters")]
        public string isMovingBool = "isMoving";
        public string isAirborneBool = "isAirborne";
        public string speedFloat = "speed";
        
        private CharacterController characterController;
        private Vector3 lastPosition;
        
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (allomancer == null)
            {
                allomancer = GetComponent<AllomancerController>();
            }
            
            lastPosition = transform.position;
        }
        
        private void Update()
        {
            if (animator == null) return;
            
            UpdateMovementAnimation();
            UpdateAllomancyAnimation();
        }
        
        private void UpdateMovementAnimation()
        {
            Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
            float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            
            lastPosition = transform.position;
            
            // Set speed float
            animator.SetFloat(speedFloat, speed);
            
            // Set moving bool
            bool isMoving = speed > 0.1f;
            animator.SetBool(isMovingBool, isMoving);
            
            // Set airborne bool
            bool isAirborne = characterController != null && !characterController.isGrounded;
            animator.SetBool(isAirborneBool, isAirborne);
        }
        
        private void UpdateAllomancyAnimation()
        {
            if (allomancer == null) return;
            
            // Steel push
            bool isSteelBurning = allomancer.GetReserve(AllomanticMetal.Steel)?.isBurning ?? false;
            animator.SetBool(steelPushBool, isSteelBurning);
            
            // Iron pull
            bool isIronBurning = allomancer.GetReserve(AllomanticMetal.Iron)?.isBurning ?? false;
            animator.SetBool(ironPullBool, isIronBurning);
            
            // Pewter
            bool isPewterBurning = allomancer.GetReserve(AllomanticMetal.Pewter)?.isBurning ?? false;
            animator.SetBool(pewterBool, isPewterBurning);
            
            // Tin
            bool isTinBurning = allomancer.GetReserve(AllomanticMetal.Tin)?.isBurning ?? false;
            animator.SetBool(tinBool, isTinBurning);
        }
        
        // Called by animation events
        public void OnSteelPushHit()
        {
            Debug.Log("Steel push animation hit frame");
            // TODO: Apply steel push force
        }
        
        public void OnIronPullHit()
        {
            Debug.Log("Iron pull animation hit frame");
            // TODO: Apply iron pull force
        }
        
        public void OnPewterAttack()
        {
            Debug.Log("Pewter attack animation frame");
            // TODO: Apply pewter damage bonus
        }
    }
}
