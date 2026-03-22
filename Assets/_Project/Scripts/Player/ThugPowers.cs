using UnityEngine;

namespace MistbornGame.Player
{
    public class ThugPowers : MonoBehaviour
    {
        [Header("Pewter Enhancement")]
        [SerializeField] private float pewterStrengthBonus = 1.5f;
        [SerializeField] private float pewterSpeedBonus = 1.3f;
        [SerializeField] private float pewterDamageBonus = 1.4f;
        [SerializeField] private float pewterToughnessBonus = 1.3f;
        [SerializeField] private float pewterBurnRate = 2f;
        
        [Header("Enhanced Combat")]
        [SerializeField] private float heavyHitChance = 0.3f;
        [SerializeField] private float heavyHitDamageMultiplier = 2f;
        [SerializeField] private float groundPoundRadius = 5f;
        [SerializeField] private float groundPoundDamage = 50f;
        [SerializeField] private float groundPoundKnockback = 20f;
        
        [Header("Allomancy Detection")]
        [SerializeField] private bool canDetectAllomancers = true;
        [SerializeField] private float detectionRange = 30f;
        [SerializeField] private bool autoDetectWhenHostile = true;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem pewterBoostVfx;
        [SerializeField] private AudioClip pewterBoostSound;
        [SerializeField] private AudioClip groundPoundSound;
        [SerializeField] private Material pewterBoostMaterial;
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        
        [Header("Combat Music")]
        [SerializeField] private bool useCombatMusic = true;
        [SerializeField] private AudioClip pewterCombatMusic;
        
        private bool isPewterBoosted = false;
        private Allomancy.Allomancer allomancer;
        private PlayerStats playerStats;
        private Rigidbody rb;
        private Animator animator;
        private AudioSource audioSource;
        
        private float originalSpeed;
        private float originalDamage;
        private float originalJumpForce;
        
        private void Start()
        {
            allomancer = GetComponent<Allomancy.Allomancer>();
            playerStats = GetComponent<PlayerStats>();
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            
            if (playerStats != null)
            {
                originalSpeed = playerStats.MoveSpeed;
                originalDamage = playerStats.BaseDamage;
                originalJumpForce = playerStats.JumpForce;
            }
        }
        
        private void Update()
        {
            HandlePewterToggle();
            HandleGroundPound();
            HandleAllomancerDetection();
        }
        
        private void HandlePewterToggle()
        {
            if (Input.GetButtonDown("PewterBoost") || Input.GetKeyDown(KeyCode.V))
            {
                if (isPewterBoosted)
                {
                    DeactivatePewterBoost();
                }
                else
                {
                    ActivatePewterBoost();
                }
            }
            
            if (isPewterBoosted && allomancer != null)
            {
                allomancer.ConsumeMetal(Allomancy.Allomancer.MetalType.Pewter, pewterBurnRate * Time.deltaTime);
                
                if (!allomancer.IsBurning(Allomancy.Allomancer.MetalType.Pewter))
                {
                    DeactivatePewterBoost();
                }
            }
        }
        
        public void ActivatePewterBoost()
        {
            if (isPewterBoosted)
                return;
            
            if (allomancer != null && !allomancer.IsBurning(Allomancy.Allomancer.MetalType.Pewter))
            {
                if (!allomancer.HasMetal(Allomancy.Allomancer.MetalType.Pewter))
                    return;
                
                allomancer.BurnMetal(Allomancy.Allomancer.MetalType.Pewter);
            }
            
            isPewterBoosted = true;
            
            ApplyPewterBonuses();
            
            AudioSource.PlayClipAtPoint(pewterBoostSound, transform.position);
            
            if (pewterBoostVfx != null)
            {
                pewterBoostVfx.Play();
            }
            
            if (animator != null)
            {
                animator.SetBool("PewterBoost", true);
            }
            
            if (useCombatMusic && pewterCombatMusic != null)
            {
                AudioManager.instance?.PlayMusic(pewterCombatMusic);
            }
        }
        
        public void DeactivatePewterBoost()
        {
            if (!isPewterBoosted)
                return;
            
            isPewterBoosted = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(Allomancy.Allomancer.MetalType.Pewter);
            }
            
            RemovePewterBonuses();
            
            if (pewterBoostVfx != null)
            {
                pewterBoostVfx.Stop();
            }
            
            if (animator != null)
            {
                animator.SetBool("PewterBoost", false);
            }
        }
        
        private void ApplyPewterBonuses()
        {
            if (playerStats != null)
            {
                playerStats.AddDamageMultiplier(pewterDamageBonus - 1f);
                playerStats.AddSpeedBonus(pewterSpeedBonus - 1f);
                playerStats.AddDefenseBonus(pewterToughnessBonus - 1f);
                
                playerStats.AddStrengthBonus(pewterStrengthBonus - 1f);
            }
            
            if (rb != null)
            {
                rb.mass *= pewterToughnessBonus;
            }
            
            if (bodyRenderer != null && pewterBoostMaterial != null)
            {
                bodyRenderer.material = pewterBoostMaterial;
            }
        }
        
        private void RemovePewterBonuses()
        {
            if (playerStats != null)
            {
                playerStats.RemoveDamageMultiplier(pewterDamageBonus - 1f);
                playerStats.RemoveSpeedBonus(pewterSpeedBonus - 1f);
                playerStats.RemoveDefenseBonus(pewterToughnessBonus - 1f);
                
                playerStats.RemoveStrengthBonus(pewterStrengthBonus - 1f);
            }
            
            if (rb != null)
            {
                rb.mass /= pewterToughnessBonus;
            }
        }
        
        private void HandleGroundPound()
        {
            if (!isPewterBoosted)
                return;
            
            if (Input.GetButtonDown("GroundPound") || Input.GetKeyDown(KeyCode.G))
            {
                ExecuteGroundPound();
            }
        }
        
        private void ExecuteGroundPound()
        {
            if (animator != null)
            {
                animator.SetTrigger("GroundPound");
            }
            
            AudioSource.PlayClipAtPoint(groundPoundSound, transform.position);
            
            StartCoroutine(GroundPoundCoroutine());
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.3f, 0.5f);
        }
        
        private System.Collections.IEnumerator GroundPoundCoroutine()
        {
            float airTime = 0.3f;
            float groundPoundTime = 0.1f;
            float recoveryTime = 0.3f;
            
            if (rb != null)
            {
                Vector3 originalVelocity = rb.velocity;
                rb.velocity = Vector3.zero;
                rb.AddForce(Vector3.up * 15f, ForceMode.Impulse);
            }
            
            yield return new WaitForSeconds(airTime);
            
            if (rb != null)
            {
                rb.velocity = Vector3.down * 50f;
            }
            
            yield return new WaitForSeconds(groundPoundTime);
            
            ApplyGroundPoundDamage();
            
            yield return new WaitForSeconds(recoveryTime);
            
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
        }
        
        private void ApplyGroundPoundDamage()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundPoundRadius);
            
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    direction.y = 0.5f;
                    direction.Normalize();
                    
                    hit.GetComponent<Rigidbody>()?.AddForce(direction * groundPoundKnockback * 100f, ForceMode.Impulse);
                    
                    Enemy.Enemy enemy = hit.GetComponent<Enemy.Enemy>();
                    if (enemy != null)
                    {
                        float damage = groundPoundDamage * (isPewterBoosted ? pewterDamageBonus : 1f);
                        enemy.TakeDamage(damage, Enemy.DamageType.Heavy);
                    }
                }
            }
        }
        
        private void HandleAllomancerDetection()
        {
            if (!canDetectAllomancers)
                return;
            
            if (autoDetectWhenHostile)
            {
                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, detectionRange);
                
                foreach (var enemy in nearbyEnemies)
                {
                    AI.AllomancerAI aiAllomancer = enemy.GetComponent<AI.AllomancerAI>();
                    if (aiAllomancer != null && aiAllomancer.IsBurningMetal)
                    {
                        OnAllomancerDetected(enemy.transform);
                        break;
                    }
                }
            }
        }
        
        private void OnAllomancerDetected(Transform allomancerTransform)
        {
            if (isPewterBoosted)
                return;
            
            ActivatePewterBoost();
        }
        
        public void OnCombatStarted()
        {
            if (!isPewterBoosted && canDetectAllomancers)
            {
                ActivatePewterBoost();
            }
        }
        
        public bool IsPewterBoosted => isPewterBoosted;
        
        private void OnDestroy()
        {
            if (isPewterBoosted)
            {
                RemovePewterBonuses();
            }
        }
    }
}
