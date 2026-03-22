using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Combat
{
    public class RangedCombat : MonoBehaviour
    {
        [Header("Ranged Configuration")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseDamage = 20f;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private float projectileSpeed = 30f;
        [SerializeField] private float range = 50f;
        
        [Header("Aiming")]
        [SerializeField] private bool requiresAim = true;
        [SerializeField] private float aimTime = 0.5f;
        [SerializeField] private float aimSpread = 0.05f;
        [SerializeField] private float maxAimSpread = 0.2f;
        
        [Header("Metal Projectiles")]
        [SerializeField] private bool useMetalProjectiles = true;
        [SerializeField] private MetalType[] availableMetalProjectiles;
        [SerializeField] private float metalProjectileCost = 10f;
        
        [Header("Charged Shot")]
        [SerializeField] private bool hasChargedShot = true;
        [SerializeField] private float maxChargeTime = 2f;
        [SerializeField] private float minChargeDamage = 1f;
        [SerializeField] private float maxChargeDamage = 3f;
        [SerializeField] private float chargeStaminaCost = 20f;
        
        [Header("Special")]
        [SerializeField] private bool hasSpecialAttack = true;
        [SerializeField] private float specialAttackCooldown = 10f;
        [SerializeField] private float specialDamageMultiplier = 5f;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private LineRenderer aimLine;
        
        [Header("Allomancy Enhancement")]
        [SerializeField] private bool enhancedBySteel = true;
        [SerializeField] private float steelSpeedBonus = 1.5f;
        [SerializeField] private float steelRangeBonus = 1.3f;
        
        private bool isAiming = false;
        private bool isCharging = false;
        private float currentAimTime = 0f;
        private float currentChargeTime = 0f;
        private float lastFireTime = -100f;
        private float lastSpecialTime = -100f;
        private float currentSpread = 0f;
        private bool hasSpecialReady = true;
        
        private PlayerStats playerStats;
        private Animator animator;
        private Camera playerCamera;
        
        public bool IsAiming => isAiming;
        public bool HasSpecialReady => hasSpecialReady;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            playerCamera = Camera.main;
            
            if (aimLine != null)
                aimLine.enabled = false;
        }
        
        private void Update()
        {
            if (GameManager.IsPaused)
                return;
            
            HandleAiming();
            HandleCharging();
            HandleFiring();
            UpdateAimLine();
        }
        
        private void HandleAiming()
        {
            if (!requiresAim)
                return;
            
            bool wantsToAim = Input.GetButton("Fire2") || Input.GetKey(KeyCode.Mouse1);
            
            if (wantsToAim && !isAiming && CanAim())
            {
                StartAiming();
            }
            else if (!wantsToAim && isAiming)
            {
                EndAiming();
            }
            
            if (isAiming)
            {
                UpdateAiming();
            }
        }
        
        private bool CanAim()
        {
            return Time.time - lastFireTime >= 1f / fireRate;
        }
        
        private void StartAiming()
        {
            isAiming = true;
            currentAimTime = 0f;
            currentSpread = aimSpread;
            
            if (animator != null)
            {
                animator.SetBool("IsAiming", true);
            }
            
            if (aimLine != null)
            {
                aimLine.enabled = true;
            }
        }
        
        private void UpdateAiming()
        {
            currentAimTime += Time.deltaTime;
            currentSpread = Mathf.Lerp(aimSpread, maxAimSpread, currentAimTime / aimTime);
            
            if (animator != null)
            {
                animator.SetFloat("AimProgress", Mathf.Clamp01(currentAimTime / aimTime));
            }
        }
        
        private void EndAiming()
        {
            isAiming = false;
            
            if (animator != null)
            {
                animator.SetBool("IsAiming", false);
                animator.SetFloat("AimProgress", 0f);
            }
            
            if (aimLine != null)
            {
                aimLine.enabled = false;
            }
        }
        
        private void HandleCharging()
        {
            if (!hasChargedShot)
                return;
            
            bool wantsToCharge = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Mouse0);
            
            if (wantsToCharge && isAiming && !isCharging)
            {
                StartCharging();
            }
            else if (!wantsToCharge && isCharging)
            {
                ReleaseCharge();
            }
            
            if (isCharging)
            {
                UpdateCharging();
            }
        }
        
        private void StartCharging()
        {
            isCharging = true;
            currentChargeTime = 0f;
            
            if (animator != null)
            {
                animator.SetBool("IsCharging", true);
            }
        }
        
        private void UpdateCharging()
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);
            
            if (animator != null)
            {
                animator.SetFloat("ChargeProgress", currentChargeTime / maxChargeTime);
            }
        }
        
        private void ReleaseCharge()
        {
            isCharging = false;
            
            if (animator != null)
            {
                animator.SetBool("IsCharging", false);
                animator.SetFloat("ChargeProgress", 0f);
            }
            
            if (currentChargeTime >= maxChargeTime * 0.5f)
            {
                FireChargedShot();
            }
            else
            {
                FireNormalShot();
            }
            
            currentChargeTime = 0f;
        }
        
        private void HandleFiring()
        {
            if (!requiresAim)
            {
                if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    FireNormalShot();
                }
            }
        }
        
        private void FireNormalShot()
        {
            if (Time.time - lastFireTime < 1f / fireRate)
                return;
            
            lastFireTime = Time.time;
            
            Vector3 direction = GetFireDirection();
            float damage = CalculateDamage();
            
            SpawnProjectile(direction, damage);
            
            PlayFireEffects();
            
            EndAiming();
        }
        
        private void FireChargedShot()
        {
            if (playerStats != null)
            {
                playerStats.UseStamina(chargeStaminaCost);
            }
            
            Vector3 direction = GetFireDirection();
            float chargeMultiplier = Mathf.Lerp(minChargeDamage, maxChargeDamage, currentChargeTime / maxChargeTime);
            float damage = CalculateDamage() * chargeMultiplier;
            
            SpawnProjectile(direction, damage, true);
            
            PlayFireEffects(true);
            
            EndAiming();
        }
        
        private Vector3 GetFireDirection()
        {
            Vector3 direction;
            
            if (playerCamera != null)
            {
                direction = playerCamera.transform.forward;
            }
            else
            {
                direction = transform.forward;
            }
            
            direction += Random.insideUnitSphere * currentSpread;
            direction.Normalize();
            
            return direction;
        }
        
        private float CalculateDamage()
        {
            float damage = baseDamage;
            
            if (playerStats != null)
            {
                damage *= playerStats.BaseDamage;
            }
            
            if (isAiming && currentAimTime >= aimTime)
            {
                damage *= 1.5f;
            }
            
            return damage;
        }
        
        private void SpawnProjectile(Vector3 direction, float damage, bool isCharged = false)
        {
            if (projectilePrefab == null)
                return;
            
            Transform spawnPoint = firePoint != null ? firePoint : transform;
            
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.LookRotation(direction));
            
            ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                float speed = projectileSpeed;
                float actualRange = range;
                
                if (enhancedBySteel && playerStats != null)
                {
                    Allomancy.Allomancer allomancer = GetComponent<Allomancy.Allomancer>();
                    if (allomancer != null && allomancer.IsBurning(Allomancy.Allomancer.MetalType.Steel))
                    {
                        speed *= steelSpeedBonus;
                        actualRange *= steelRangeBonus;
                    }
                }
                
                projectileController.Initialize(damage, speed, actualRange);
                projectileController.SetOwner(gameObject);
            }
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
            
            Destroy(projectile, 5f);
        }
        
        private void PlayFireEffects(bool isCharged = false)
        {
            if (muzzleFlash != null)
            {
                ParticleSystem flash = Instantiate(muzzleFlash, firePoint.position, Quaternion.identity);
                flash.Play();
                Destroy(flash.gameObject, 1f);
            }
            
            AudioSource.PlayClipAtPoint(fireSound, firePoint.position);
            
            if (animator != null)
            {
                animator.SetTrigger("Fire");
            }
        }
        
        private void UpdateAimLine()
        {
            if (aimLine == null || !isAiming)
                return;
            
            Vector3 startPoint = firePoint != null ? firePoint.position : transform.position + Vector3.up;
            Vector3 endPoint = startPoint + GetFireDirection() * range;
            
            RaycastHit hit;
            if (Physics.Raycast(startPoint, GetFireDirection(), out hit, range))
            {
                endPoint = hit.point;
            }
            
            aimLine.SetPosition(0, startPoint);
            aimLine.SetPosition(1, endPoint);
        }
        
        public void FireSpecialAttack()
        {
            if (!hasSpecialAttack)
                return;
            
            if (Time.time - lastSpecialTime < specialAttackCooldown)
                return;
            
            lastSpecialTime = Time.time;
            
            Vector3 direction = GetFireDirection();
            float damage = CalculateDamage() * specialDamageMultiplier;
            
            SpawnProjectile(direction, damage, true);
            
            if (muzzleFlash != null)
            {
                ParticleSystem flash = Instantiate(muzzleFlash, firePoint.position, Quaternion.identity);
                flash.Emit(50);
                Destroy(flash.gameObject, 1f);
            }
            
            AudioSource.PlayClipAtPoint(fireSound, firePoint.position);
            
            StartCoroutine(SpecialCooldownCoroutine());
        }
        
        private System.Collections.IEnumerator SpecialCooldownCoroutine()
        {
            hasSpecialReady = false;
            yield return new WaitForSeconds(specialAttackCooldown);
            hasSpecialReady = true;
        }
        
        public void UseMetalProjectile(MetalType metal)
        {
            if (!useMetalProjectiles)
                return;
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null)
            {
                if (inventory.HasMetal(metal, metalProjectileCost))
                {
                    inventory.UseMetal(metal, (int)metalProjectileCost);
                }
            }
        }
        
        public float GetCurrentAimSpread()
        {
            return currentSpread;
        }
        
        public float GetChargeProgress()
        {
            return currentChargeTime / maxChargeTime;
        }
    }
}
