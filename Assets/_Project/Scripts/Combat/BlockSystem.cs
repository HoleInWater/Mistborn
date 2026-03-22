using UnityEngine;

namespace MistbornGame.Combat
{
    public class BlockSystem : MonoBehaviour
    {
        [Header("Block Configuration")]
        [SerializeField] private float blockDamageReduction = 0.6f;
        [SerializeField] private float blockStaminaCost = 10f;
        [SerializeField] private float blockStaminaDrainRate = 5f;
        [SerializeField] private float minBlockAngle = 60f;
        [SerializeField] private float perfectBlockAngle = 30f;
        [SerializeField] private float perfectBlockBonus = 1.5f;
        
        [Header("Stamina Management")]
        [SerializeField] private float staminaRecoveryDelay = 0.5f;
        [SerializeField] private float lowStaminaThreshold = 20f;
        [SerializeField] private float exhaustedStaminaThreshold = 5f;
        
        [Header("Visual")]
        [SerializeField] private GameObject shieldModel;
        [SerializeField] private GameObject blockIndicator;
        [SerializeField] private Renderer shieldRenderer;
        [SerializeField] private Material blockActiveMaterial;
        [SerializeField] private Material blockLowStaminaMaterial;
        [SerializeField] private Material shieldNormalMaterial;
        
        [Header("Shield Weapon")]
        [SerializeField] private GameObject shieldWeapon;
        [SerializeField] private Transform shieldHand;
        
        [Header("Audio")]
        [SerializeField] private AudioClip blockStartSound;
        [SerializeField] private AudioClip blockHitSound;
        [SerializeField] private AudioClip blockBreakSound;
        [SerializeField] private AudioClip perfectBlockSound;
        
        private bool isBlocking = false;
        private float lastBlockTime = 0f;
        private float currentBlockAngle = 0f;
        private bool shieldEquipped = true;
        private bool shieldBroken = false;
        private float shieldBreakTimer = 0f;
        
        private PlayerStats playerStats;
        private Animator animator;
        
        public bool IsBlocking => isBlocking;
        public float CurrentBlockAngle => currentBlockAngle;
        public bool ShieldEquipped => shieldEquipped;
        public bool ShieldBroken => shieldBroken;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            
            SetupShield();
            SetupBlockIndicator();
        }
        
        private void SetupShield()
        {
            if (shieldWeapon != null && shieldHand != null)
            {
                shieldModel = Instantiate(shieldWeapon, shieldHand);
            }
            
            if (shieldModel != null)
            {
                shieldRenderer = shieldModel.GetComponent<Renderer>();
            }
        }
        
        private void SetupBlockIndicator()
        {
            if (blockIndicator != null)
            {
                blockIndicator.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (shieldBroken)
            {
                shieldBreakTimer -= Time.deltaTime;
                if (shieldBreakTimer <= 0f)
                {
                    RepairShield();
                }
                return;
            }
            
            HandleBlockInput();
            UpdateBlockVisuals();
        }
        
        private void HandleBlockInput()
        {
            bool wantsToBlock = Input.GetButton("Block") || Input.GetKey(KeyCode.LeftShift);
            
            if (wantsToBlock && CanBlock())
            {
                if (!isBlocking)
                {
                    StartBlocking();
                }
                else
                {
                    MaintainBlocking();
                }
            }
            else if (isBlocking)
            {
                StopBlocking();
            }
        }
        
        private bool CanBlock()
        {
            if (shieldBroken)
                return false;
            
            if (playerStats != null && playerStats.CurrentStamina < blockStaminaCost)
                return false;
            
            if (animator != null && animator.GetBool("IsDodging"))
                return false;
            
            return true;
        }
        
        private void StartBlocking()
        {
            isBlocking = true;
            lastBlockTime = Time.time;
            
            if (playerStats != null)
            {
                playerStats.UseStamina(blockStaminaCost);
            }
            
            if (animator != null)
            {
                animator.SetBool("IsBlocking", true);
            }
            
            AudioSource.PlayClipAtPoint(blockStartSound, transform.position);
            
            if (blockIndicator != null)
            {
                blockIndicator.SetActive(true);
            }
        }
        
        private void MaintainBlocking()
        {
            if (playerStats != null)
            {
                float currentStamina = playerStats.CurrentStamina;
                
                if (currentStamina < blockStaminaCost)
                {
                    StopBlocking();
                    return;
                }
                
                playerStats.UseStamina(blockStaminaDrainRate * Time.deltaTime);
            }
            
            CalculateBlockAngle();
        }
        
        private void StopBlocking()
        {
            isBlocking = false;
            currentBlockAngle = 0f;
            
            if (animator != null)
            {
                animator.SetBool("IsBlocking", false);
            }
            
            if (blockIndicator != null)
            {
                blockIndicator.SetActive(false);
            }
            
            if (shieldRenderer != null && shieldNormalMaterial != null)
            {
                shieldRenderer.material = shieldNormalMaterial;
            }
        }
        
        private void CalculateBlockAngle()
        {
            Transform cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
                return;
            
            Vector3 forward = cameraTransform.forward;
            Vector3 toDamage = Vector3.forward;
            
            currentBlockAngle = Vector3.Angle(forward, toDamage);
        }
        
        public float ProcessIncomingDamage(float damage, Vector3 attackDirection)
        {
            if (!isBlocking || shieldBroken)
                return damage;
            
            Transform cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
                return damage;
            
            Vector3 blockDirection = cameraTransform.forward;
            Vector3 attackDir = attackDirection;
            attackDir.y = 0;
            blockDirection.y = 0;
            
            float angle = Vector3.Angle(blockDirection, attackDir);
            
            if (angle > minBlockAngle)
            {
                return damage;
            }
            
            AudioSource.PlayClipAtPoint(blockHitSound, transform.position);
            
            if (playerStats != null)
            {
                playerStats.UseStamina(blockStaminaCost * 0.5f);
            }
            
            if (angle <= perfectBlockAngle)
            {
                return ProcessPerfectBlock(damage);
            }
            
            return damage * (1f - blockDamageReduction);
        }
        
        private float ProcessPerfectBlock(float damage)
        {
            AudioSource.PlayClipAtPoint(perfectBlockSound, transform.position);
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.05f, 0.1f);
            
            if (shieldRenderer != null && blockActiveMaterial != null)
            {
                shieldRenderer.material = blockActiveMaterial;
                Invoke(nameof(ResetShieldMaterial), 0.2f);
            }
            
            float reflectedDamage = damage * (1f - blockDamageReduction) * perfectBlockBonus;
            
            return reflectedDamage * 0.2f;
        }
        
        private void ResetShieldMaterial()
        {
            if (shieldRenderer != null && shieldNormalMaterial != null)
            {
                shieldRenderer.material = shieldNormalMaterial;
            }
        }
        
        private void UpdateBlockVisuals()
        {
            if (shieldRenderer == null)
                return;
            
            if (!isBlocking)
                return;
            
            if (playerStats != null && playerStats.CurrentStamina < lowStaminaThreshold)
            {
                shieldRenderer.material = blockLowStaminaMaterial;
            }
            else
            {
                shieldRenderer.material = shieldNormalMaterial;
            }
        }
        
        public void BreakShield(float duration)
        {
            if (shieldBroken)
                return;
            
            shieldBroken = true;
            shieldBreakTimer = duration;
            
            if (shieldModel != null)
            {
                shieldModel.SetActive(false);
            }
            
            AudioSource.PlayClipAtPoint(blockBreakSound, transform.position);
            
            animator?.SetTrigger("ShieldBroken");
        }
        
        private void RepairShield()
        {
            shieldBroken = false;
            
            if (shieldModel != null)
            {
                shieldModel.SetActive(true);
            }
            
            animator?.SetTrigger("ShieldRepaired");
        }
        
        public void EquipShield()
        {
            shieldEquipped = true;
            
            if (shieldModel != null)
            {
                shieldModel.SetActive(true);
            }
        }
        
        public void UnequipShield()
        {
            shieldEquipped = false;
            shieldModel?.SetActive(false);
        }
        
        public void UpgradeBlock(string upgradeType)
        {
            switch (upgradeType)
            {
                case "DamageReduction":
                    blockDamageReduction = Mathf.Min(blockDamageReduction + 0.1f, 0.9f);
                    break;
                case "StaminaCost":
                    blockStaminaCost = Mathf.Max(blockStaminaCost - 2f, 5f);
                    break;
                case "Angle":
                    minBlockAngle += 10f;
                    perfectBlockAngle += 5f;
                    break;
            }
        }
    }
}
