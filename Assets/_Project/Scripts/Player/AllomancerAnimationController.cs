using UnityEngine;

namespace Mistborn.Player
{
    public class AllomancerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private AllomancerController allomancer;
        
        [Header("Animation Parameters")]
        [SerializeField] private string steelPushBool = "isSteelPushing";
        [SerializeField] private string ironPullBool = "isIronPulling";
        [SerializeField] private string pewterBool = "isPewterBurning";
        [SerializeField] private string tinBool = "isTinBurning";
        [SerializeField] private string isMovingBool = "isMoving";
        [SerializeField] private string isAirborneBool = "isAirborne";
        [SerializeField] private string speedFloat = "speed";
        
        private CharacterController characterController;
        private Vector3 lastPosition;
        
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (allomancer == null)
                allomancer = GetComponent<AllomancerController>();
            lastPosition = transform.position;
        }
        
        private void Update()
        {
            if (animator == null) return;
            UpdateMovement();
            UpdateAllomancy();
        }
        
        private void UpdateMovement()
        {
            Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
            float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            lastPosition = transform.position;
            
            animator.SetFloat(speedFloat, speed);
            animator.SetBool(isMovingBool, speed > 0.1f);
            animator.SetBool(isAirborneBool, characterController != null && !characterController.isGrounded);
        }
        
        private void UpdateAllomancy()
        {
            if (allomancer == null) return;
            
            animator.SetBool(steelPushBool, allomancer.GetReserve(AllomanticMetal.Steel)?.IsBurning ?? false);
            animator.SetBool(ironPullBool, allomancer.GetReserve(AllomanticMetal.Iron)?.IsBurning ?? false);
            animator.SetBool(pewterBool, allomancer.GetReserve(AllomanticMetal.Pewter)?.IsBurning ?? false);
            animator.SetBool(tinBool, allomancer.GetReserve(AllomanticMetal.Tin)?.IsBurning ?? false);
        }
    }
}
