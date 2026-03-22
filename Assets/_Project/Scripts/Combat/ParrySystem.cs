using UnityEngine;

namespace MistbornGame.Combat
{
    public enum ParryType
    {
        Light,
        Medium,
        Heavy,
        Perfect
    }
    
    public class ParrySystem : MonoBehaviour
    {
        [Header("Parry Configuration")]
        [SerializeField] private ParryType parryType = ParryType.Light;
        [SerializeField] private float parryWindow = 0.2f;
        [SerializeField] private float parryCooldown = 0.5f;
        [SerializeField] private float perfectParryWindow = 0.1f;
        [SerializeField] private float perfectParryBonus = 1.5f;
        
        [Header("Parry Effects")]
        [SerializeField] private float parryDamageReduction = 0.8f;
        [SerializeField] private float parryKnockback = 10f;
        [SerializeField] private float parryStaminaCost = 15f;
        [SerializeField] private float perfectParryStaminaGain = 20f;
        
        [Header("Timing Window Visual")]
        [SerializeField] private bool showParryIndicator = true;
        [SerializeField] private GameObject parryIndicatorPrefab;
        [SerializeField] private Transform parryIndicatorPoint;
        
        [Header("Audio/Visual")]
        [SerializeField] private AudioClip parrySound;
        [SerializeField] private AudioClip perfectParrySound;
        [SerializeField] private AudioClip parryFailSound;
        [SerializeField] private GameObject parryVfxPrefab;
        [SerializeField] private GameObject perfectParryVfxPrefab;
        [SerializeField] private Material parryActiveMaterial;
        [SerializeField] private Renderer weaponRenderer;
        
        private bool isParryActive = false;
        private float lastParryTime = -100f;
        private float parryStartTime = 0f;
        private bool hasPerfectParrySetup = false;
        private GameObject parryIndicator;
        
        private PlayerStats playerStats;
        private Animator animator;
        private Rigidbody playerRb;
        
        public bool IsParryActive => isParryActive;
        public float CurrentParryWindow => isParryActive ? parryWindow : 0f;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            playerRb = GetComponent<Rigidbody>();
            
            SetupParryIndicator();
        }
        
        private void SetupParryIndicator()
        {
            if (!showParryIndicator || parryIndicatorPrefab == null)
                return;
            
            Transform spawnPoint = parryIndicatorPoint != null ? parryIndicatorPoint : transform;
            parryIndicator = Instantiate(parryIndicatorPrefab, spawnPoint);
            parryIndicator.SetActive(false);
        }
        
        private void Update()
        {
            if (Input.GetButtonDown("Parry") || Input.GetKeyDown(KeyCode.P))
            {
                StartParry();
            }
            
            if (Input.GetButtonUp("Parry") || Input.GetKeyUp(KeyCode.P))
            {
                EndParry();
            }
            
            if (isParryActive && Time.time - parryStartTime > parryWindow)
            {
                EndParry();
            }
        }
        
        public void StartParry()
        {
            if (Time.time - lastParryTime < parryCooldown)
                return;
            
            if (playerStats != null && playerStats.CurrentStamina < parryStaminaCost)
                return;
            
            if (animator != null)
            {
                animator.SetTrigger("Parry");
            }
            
            isParryActive = true;
            parryStartTime = Time.time;
            lastParryTime = Time.time;
            
            if (playerStats != null)
            {
                playerStats.UseStamina(parryStaminaCost);
            }
            
            ShowParryIndicator(true);
            
            hasPerfectParrySetup = false;
        }
        
        public void EndParry()
        {
            isParryActive = false;
            ShowParryIndicator(false);
            
            if (animator != null)
            {
                animator.ResetTrigger("Parry");
            }
        }
        
        public bool CheckParrySuccess(float incomingDamage, Vector3 attackDirection)
        {
            if (!isParryActive)
                return false;
            
            float timeSinceParryStart = Time.time - parryStartTime;
            
            if (timeSinceParryStart <= perfectParryWindow)
            {
                ExecutePerfectParry(incomingDamage, attackDirection);
                return true;
            }
            else if (timeSinceParryStart <= parryWindow)
            {
                ExecuteParry(incomingDamage, attackDirection);
                return true;
            }
            
            return false;
        }
        
        private void ExecuteParry(float incomingDamage, Vector3 attackDirection)
        {
            float reducedDamage = incomingDamage * (1f - parryDamageReduction);
            
            playerStats?.TakeDamage(reducedDamage);
            
            Vector3 knockbackDirection = -attackDirection;
            knockbackDirection.y = 0.3f;
            knockbackDirection.Normalize();
            
            if (playerRb != null)
            {
                playerRb.AddForce(knockbackDirection * parryKnockback * 100f, ForceMode.Impulse);
            }
            
            PlayParryEffects(false);
            
            PerfectParryWindow(false);
        }
        
        private void ExecutePerfectParry(float incomingDamage, Vector3 attackDirection)
        {
            if (playerStats != null)
            {
                playerStats.AddStamina(perfectParryStaminaGain);
            }
            
            Enemy.Enemy enemy = FindParryTarget(attackDirection);
            if (enemy != null)
            {
                float perfectDamage = incomingDamage * perfectParryBonus;
                enemy.TakeDamage(perfectDamage, DamageType.Perfect);
            }
            
            PlayParryEffects(true);
            
            PerfectParryWindow(true);
        }
        
        private Enemy.Enemy FindParryTarget(Vector3 attackDirection)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, attackDirection, 5f);
            
            foreach (var hit in hits)
            {
                Enemy.Enemy enemy = hit.transform.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    return enemy;
                }
            }
            
            return null;
        }
        
        private void PerfectParryWindow(bool active)
        {
            hasPerfectParrySetup = active;
        }
        
        private void PlayParryEffects(bool isPerfect)
        {
            if (isPerfect)
            {
                AudioSource.PlayClipAtPoint(perfectParrySound, transform.position);
                
                if (perfectParryVfxPrefab != null)
                {
                    GameObject vfx = Instantiate(perfectParryVfxPrefab, transform.position + Vector3.forward, Quaternion.identity);
                    Destroy(vfx, 2f);
                }
                
                Camera.main?.GetComponent<CameraShake>()?.Shake(0.1f, 0.2f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(parrySound, transform.position);
                
                if (parryVfxPrefab != null)
                {
                    GameObject vfx = Instantiate(parryVfxPrefab, transform.position + Vector3.forward, Quaternion.identity);
                    Destroy(vfx, 1f);
                }
            }
            
            if (weaponRenderer != null && parryActiveMaterial != null)
            {
                Material[] mats = weaponRenderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = parryActiveMaterial;
                }
                weaponRenderer.materials = mats;
                
                Invoke(nameof(ResetWeaponMaterial), 0.2f);
            }
        }
        
        private void ResetWeaponMaterial()
        {
            if (weaponRenderer != null)
            {
                weaponRenderer.material = null;
            }
        }
        
        private void ShowParryIndicator(bool show)
        {
            if (parryIndicator != null)
            {
                parryIndicator.SetActive(show);
            }
        }
        
        public float GetParryDamageMultiplier()
        {
            if (!isParryActive)
                return 1f;
            
            float timeSinceParryStart = Time.time - parryStartTime;
            
            if (timeSinceParryStart <= perfectParryWindow)
                return perfectParryBonus;
            
            return parryDamageReduction;
        }
        
        public void UpgradeParryType(ParryType newType)
        {
            parryType = newType;
            
            switch (newType)
            {
                case ParryType.Light:
                    parryWindow = 0.15f;
                    parryCooldown = 0.6f;
                    parryStaminaCost = 10f;
                    break;
                case ParryType.Medium:
                    parryWindow = 0.2f;
                    parryCooldown = 0.5f;
                    parryStaminaCost = 15f;
                    break;
                case ParryType.Heavy:
                    parryWindow = 0.25f;
                    parryCooldown = 0.4f;
                    parryStaminaCost = 20f;
                    parryDamageReduction = 0.9f;
                    break;
                case ParryType.Perfect:
                    parryWindow = 0.3f;
                    parryCooldown = 0.3f;
                    parryStaminaCost = 25f;
                    parryDamageReduction = 0.95f;
                    perfectParryBonus = 2f;
                    break;
            }
        }
    }
}
