using UnityEngine;

namespace MistbornGameplay
{
    public class EnhancedMovement : MonoBehaviour
    {
        [Header("Movement Multipliers")]
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float jumpMultiplier = 1.3f;
        [SerializeField] private float dashMultiplier = 2f;
        
        [Header("Pewter Enhancement")]
        [SerializeField] private bool enhanceStrength = true;
        [SerializeField] private float strengthMultiplier = 2f;
        [SerializeField] private float fallDamageReduction = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem pewterAura;
        [SerializeField] private TrailRenderer movementTrail;
        [SerializeField] private Color auraColor = new Color(1f, 0.8f, 0.4f, 0.5f);
        
        private BasicPlayerMove playerMove;
        private Rigidbody playerRb;
        private bool isEnhanced = false;
        private float originalSpeed;
        private float originalJumpForce;
        private float originalDashForce;
        
        private void Awake()
        {
            playerMove = GetComponent<BasicPlayerMove>();
            playerRb = GetComponent<Rigidbody>();
        }
        
        private void Start()
        {
            if (playerMove != null)
            {
                originalSpeed = playerMove.moveSpeed;
                originalJumpForce = playerMove.jumpForce;
                originalDashForce = playerMove.dashForce;
            }
            
            if (pewterAura != null)
            {
                pewterAura.Stop();
            }
            
            if (movementTrail != null)
            {
                movementTrail.enabled = false;
            }
        }
        
        public void ActivateEnhancement()
        {
            if (isEnhanced)
            {
                return;
            }
            
            isEnhanced = true;
            
            if (playerMove != null)
            {
                playerMove.moveSpeed = originalSpeed * speedMultiplier;
                playerMove.jumpForce = originalJumpForce * jumpMultiplier;
                playerMove.dashForce = originalDashForce * dashMultiplier;
            }
            
            if (enhanceStrength && playerRb != null)
            {
                playerRb.mass *= strengthMultiplier;
            }
            
            if (pewterAura != null)
            {
                var main = pewterAura.main;
                main.startColor = auraColor;
                pewterAura.Play();
            }
            
            if (movementTrail != null)
            {
                movementTrail.enabled = true;
            }
            
            if (OnEnhancementActivated != null)
            {
                OnEnhancementActivated();
            }
        }
        
        public void DeactivateEnhancement()
        {
            if (!isEnhanced)
            {
                return;
            }
            
            isEnhanced = false;
            
            if (playerMove != null)
            {
                playerMove.moveSpeed = originalSpeed;
                playerMove.jumpForce = originalJumpForce;
                playerMove.dashForce = originalDashForce;
            }
            
            if (enhanceStrength && playerRb != null)
            {
                playerRb.mass /= strengthMultiplier;
            }
            
            if (pewterAura != null)
            {
                pewterAura.Stop();
            }
            
            if (movementTrail != null)
            {
                movementTrail.enabled = false;
            }
            
            if (OnEnhancementDeactivated != null)
            {
                OnEnhancementDeactivated();
            }
        }
        
        public bool IsEnhanced()
        {
            return isEnhanced;
        }
        
        public float GetSpeedMultiplier()
        {
            return isEnhanced ? speedMultiplier : 1f;
        }
        
        public float GetJumpMultiplier()
        {
            return isEnhanced ? jumpMultiplier : 1f;
        }
        
        public float GetDashMultiplier()
        {
            return isEnhanced ? dashMultiplier : 1f;
        }
        
        public float CalculateFallDamage(float fallDistance)
        {
            if (!isEnhanced)
            {
                return fallDistance;
            }
            
            return fallDistance * (1f - fallDamageReduction);
        }
        
        public void SetMultipliers(float speed, float jump, float dash)
        {
            speedMultiplier = speed;
            jumpMultiplier = jump;
            dashMultiplier = dash;
        }
        
        public void SetStrengthMultiplier(float strength)
        {
            strengthMultiplier = strength;
            
            if (isEnhanced && playerRb != null)
            {
                playerRb.mass = originalSpeed / strengthMultiplier;
            }
        }
        
        public void SetAuraColor(Color color)
        {
            auraColor = color;
            
            if (pewterAura != null)
            {
                var main = pewterAura.main;
                main.startColor = auraColor;
            }
        }
        
        public event System.Action OnEnhancementActivated;
        public event System.Action OnEnhancementDeactivated;
    }
}
