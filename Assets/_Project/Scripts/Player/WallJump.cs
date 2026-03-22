using UnityEngine;
using System.Collections;

namespace MistbornGame.Player
{
    public class WallJump : MonoBehaviour
    {
        [Header("Wall Jump Configuration")]
        [SerializeField] private float wallJumpForce = 12f;
        [SerializeField] private float wallJumpHeight = 8f;
        [SerializeField] private float wallJumpForwardMultiplier = 1.5f;
        [SerializeField] private float wallSlideSpeed = 2f;
        [SerializeField] private float maxWallSlideTime = 2f;
        
        [Header("Detection")]
        [SerializeField] private float wallCheckDistance = 1f;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private bool checkBothSides = true;
        
        [Header("Cooldown")]
        [SerializeField] private float wallJumpCooldown = 0.3f;
        [SerializeField] private int maxConsecutiveWallJumps = 3;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem wallJumpParticles;
        [SerializeField] private AudioClip wallJumpSound;
        [SerializeField] private GameObject wallSlideIndicator;
        
        [Header("Allomancy Integration")]
        [SerializeField] private bool enhancedByPewter = true;
        [SerializeField] private float pewterBoostMultiplier = 1.3f;
        
        private bool isWallSliding = false;
        private bool isWallJumping = false;
        private bool canWallJump = true;
        private int consecutiveWallJumps = 0;
        private float lastWallJumpTime = -100f;
        private float wallSlideTime = 0f;
        private Vector3 wallNormal;
        private Vector3 lastWallNormal;
        
        private Rigidbody rb;
        private PlayerStats playerStats;
        private CharacterController characterController;
        private Animator animator;
        private Stamina stamina;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            playerStats = GetComponent<PlayerStats>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            stamina = GetComponent<Stamina>();
        }
        
        private void Update()
        {
            if (GameManager.IsPaused)
                return;
            
            CheckForWall();
            
            if (isWallSliding)
            {
                HandleWallSlide();
            }
            
            if (Input.GetButtonDown("Jump") && canWallJump)
            {
                TryWallJump();
            }
        }
        
        private void FixedUpdate()
        {
            if (isWallSliding)
            {
                ApplyWallSlidePhysics();
            }
        }
        
        private void CheckForWall()
        {
            bool hitLeftWall = false;
            bool hitRightWall = false;
            
            RaycastHit leftHit;
            RaycastHit rightHit;
            
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            
            if (Physics.Raycast(origin, -transform.right, out leftHit, wallCheckDistance, wallLayer))
            {
                hitLeftWall = true;
                wallNormal = leftHit.normal;
            }
            
            if (Physics.Raycast(origin, transform.right, out rightHit, wallCheckDistance, wallLayer))
            {
                hitRightWall = true;
                wallNormal = rightHit.normal;
            }
            
            bool isGrounded = characterController != null ? characterController.isGrounded : Physics.Raycast(transform.position, Vector3.down, 0.2f);
            
            bool touchingWall = (hitLeftWall || hitRightWall) && !isGrounded && rb.velocity.y < 0;
            
            if (touchingWall && !isWallSliding && !isWallJumping)
            {
                StartWallSlide();
            }
            else if (!touchingWall && isWallSliding)
            {
                EndWallSlide();
            }
        }
        
        private void StartWallSlide()
        {
            isWallSliding = true;
            wallSlideTime = 0f;
            consecutiveWallJumps = 0;
            
            if (wallSlideIndicator != null)
            {
                wallSlideIndicator.SetActive(true);
            }
            
            animator?.SetBool("IsWallSliding", true);
            
            lastWallNormal = wallNormal;
        }
        
        private void EndWallSlide()
        {
            isWallSliding = false;
            
            if (wallSlideIndicator != null)
            {
                wallSlideIndicator.SetActive(false);
            }
            
            animator?.SetBool("IsWallSliding", false);
        }
        
        private void HandleWallSlide()
        {
            wallSlideTime += Time.deltaTime;
            
            if (wallSlideTime > maxWallSlideTime)
            {
                EndWallSlide();
                StartCoroutine(WallJumpCooldown());
                return;
            }
            
            if (characterController != null && characterController.isGrounded)
            {
                EndWallSlide();
            }
        }
        
        private void ApplyWallSlidePhysics()
        {
            if (rb == null)
                return;
            
            Vector3 velocity = rb.velocity;
            velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
            rb.velocity = velocity;
        }
        
        private void TryWallJump()
        {
            if (!canWallJump || !isWallSliding && !CanGroundJump())
                return;
            
            if (stamina != null && stamina.CurrentStamina < 10f)
                return;
            
            if (consecutiveWallJumps >= maxConsecutiveWallJumps)
            {
                StartCoroutine(WallJumpCooldown());
                return;
            }
            
            if (isWallSliding)
            {
                WallJumpAction();
            }
            else
            {
                GroundJumpAction();
            }
        }
        
        private void WallJumpAction()
        {
            isWallJumping = true;
            isWallSliding = false;
            consecutiveWallJumps++;
            lastWallJumpTime = Time.time;
            
            if (stamina != null)
            {
                stamina.UseStamina(10f);
            }
            
            Vector3 jumpDirection = lastWallNormal;
            jumpDirection.y = 0;
            jumpDirection = jumpDirection.normalized * wallJumpForwardMultiplier + Vector3.up;
            
            float force = wallJumpForce;
            
            if (enhancedByPewter && playerStats != null)
            {
                Allomancy.Allomancer allomancer = GetComponent<Allomancy.Allomancer>();
                if (allomancer != null && allomancer.IsBurning(Allomancy.Allomancer.MetalType.Pewter))
                {
                    force *= pewterBoostMultiplier;
                }
            }
            
            rb.velocity = Vector3.zero;
            rb.AddForce(jumpDirection * force * 100f, ForceMode.Impulse);
            
            StartCoroutine(EndWallJump());
            
            SpawnWallJumpEffects();
            
            animator?.SetTrigger("WallJump");
        }
        
        private void GroundJumpAction()
        {
            if (characterController == null)
                return;
            
            rb.AddForce(Vector3.up * wallJumpHeight * 100f, ForceMode.Impulse);
            
            animator?.SetTrigger("Jump");
        }
        
        private bool CanGroundJump()
        {
            return characterController != null && characterController.isGrounded;
        }
        
        private IEnumerator EndWallJump()
        {
            yield return new WaitForSeconds(0.2f);
            isWallJumping = false;
        }
        
        private IEnumerator WallJumpCooldown()
        {
            canWallJump = false;
            consecutiveWallJumps = 0;
            
            yield return new WaitForSeconds(wallJumpCooldown);
            
            canWallJump = true;
        }
        
        private void SpawnWallJumpEffects()
        {
            AudioSource.PlayClipAtPoint(wallJumpSound, transform.position);
            
            if (wallJumpParticles != null)
            {
                ParticleSystem particles = Instantiate(wallJumpParticles, transform.position, Quaternion.LookRotation(lastWallNormal));
                particles.Play();
                Destroy(particles.gameObject, 2f);
            }
        }
        
        public bool IsWallSliding => isWallSliding;
        public bool CanWallJump => canWallJump;
        public int ConsecutiveWallJumps => consecutiveWallJumps;
    }
}
