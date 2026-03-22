using UnityEngine;
using System.Collections;

namespace MistbornGame.Enemy
{
    public enum LordRulerPhase
    {
        First,
        Second,
        Third
    }
    
    public class LordRulerBoss : Enemy
    {
        [Header("Boss Phases")]
        [SerializeField] private LordRulerPhase currentPhase = LordRulerPhase.First;
        [SerializeField] private float phaseTwoThreshold = 0.66f;
        [SerializeField] private float phaseThreeThreshold = 0.33f;
        
        [Header("Allomancy Powers")]
        [SerializeField] private bool canPushPewter = true;
        [SerializeField] private bool canPullIron = true;
        [SerializeField] private bool usesBrass = true;
        [SerializeField] private bool usesZinc = true;
        [SerializeField] private float allomancyForce = 40f;
        
        [Header("Feruchemy Powers")]
        [SerializeField] private bool usesTin = true;
        [SerializeField] private bool usesIron = true;
        [SerializeField] private float storedStrengthBonus = 1.5f;
        
        [Header("Combat Stats")]
        [SerializeField] private float baseDamage = 50f;
        [SerializeField] private float phaseTwoDamageMultiplier = 1.5f;
        [SerializeField] private float phaseThreeDamageMultiplier = 2f;
        
        [Header("Abilities")]
        [SerializeField] private float shockwaveRadius = 15f;
        [SerializeField] private float shockwaveDamage = 100f;
        [SerializeField] private float shockwaveKnockback = 30f;
        [SerializeField] private GameObject shockwaveVfx;
        
        [Header("Ultimate Attack")]
        [SerializeField] private float ultimateAttackRange = 30f;
        [SerializeField] private float ultimateDamage = 200f;
        [SerializeField] private float ultimateChargingTime = 3f;
        [SerializeField] private GameObject ultimateVfx;
        
        [Header("Environment")]
        [SerializeField] private float platformHealth = 1000f;
        [SerializeField] private bool destroyPlatformOnDeath = true;
        [SerializeField] private GameObject platform;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem powerAura;
        [SerializeField] private Material phaseOneMaterial;
        [SerializeField] private Material phaseTwoMaterial;
        [SerializeField] private Material phaseThreeMaterial;
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        
        [Header("Audio")]
        [SerializeField] private AudioClip phaseTransitionSound;
        [SerializeField] private AudioClip ultimateAttackSound;
        [SerializeField] private AudioClip shockwaveSound;
        
        private float currentPlatformHealth;
        private bool isChargingUltimate = false;
        private bool isUltimateActive = false;
        private float lastAllomancyTime = 0f;
        private float allomancyCooldown = 2f;
        private bool hasEnraged = false;
        
        protected override void Start()
        {
            maxHealth = 5000f;
            damage = baseDamage;
            attackRange = 5f;
            detectionRange = 100f;
            chaseSpeed = 6f;
            
            currentHealth = maxHealth;
            currentPlatformHealth = platformHealth;
            
            if (bodyRenderer != null)
                bodyRenderer.material = phaseOneMaterial;
        }
        
        protected override void Update()
        {
            base.Update();
            
            CheckPhaseTransition();
            
            if (!isChargingUltimate && state != EnemyState.Dead)
            {
                TryAllomancyAttack();
            }
        }
        
        private void CheckPhaseTransition()
        {
            float healthPercent = currentHealth / maxHealth;
            
            if (healthPercent <= phaseTwoThreshold && currentPhase == LordRulerPhase.First)
            {
                TransitionToPhaseTwo();
            }
            else if (healthPercent <= phaseThreeThreshold && currentPhase == LordRulerPhase.Second)
            {
                TransitionToPhaseThree();
            }
        }
        
        private void TransitionToPhaseTwo()
        {
            currentPhase = LordRulerPhase.Second;
            damage = baseDamage * phaseTwoDamageMultiplier;
            chaseSpeed = 8f;
            
            if (powerAura != null)
                powerAura.Play();
            
            if (bodyRenderer != null && phaseTwoMaterial != null)
                bodyRenderer.material = phaseTwoMaterial;
            
            animator.SetTrigger("PhaseTwo");
            AudioSource.PlayClipAtPoint(phaseTransitionSound, transform.position);
            
            UseStoredFeruchemyPower();
            
            RestorePlatformHealth(0.3f);
        }
        
        private void TransitionToPhaseThree()
        {
            currentPhase = LordRulerPhase.Third;
            damage = baseDamage * phaseThreeDamageMultiplier;
            chaseSpeed = 10f;
            agent.speed = chaseSpeed;
            
            if (powerAura != null)
            {
                var main = powerAura.main;
                main.startSpeed = 5f;
                main.startLifetime = 2f;
            }
            
            if (bodyRenderer != null && phaseThreeMaterial != null)
                bodyRenderer.material = phaseThreeMaterial;
            
            animator.SetTrigger("PhaseThree");
            AudioSource.PlayClipAtPoint(phaseTransitionSound, transform.position);
            
            if (!hasEnraged)
            {
                hasEnraged = true;
                StartCoroutine(FrenzyModeCoroutine());
            }
        }
        
        private void UseStoredFeruchemyPower()
        {
            if (usesTin)
            {
                StartCoroutine(EnhancedSensesCoroutine());
            }
            
            if (usesIron)
            {
                StartCoroutine(EnhancedStrengthCoroutine());
            }
        }
        
        private IEnumerator EnhancedSensesCoroutine()
        {
            detectionRange *= 2f;
            yield return new WaitForSeconds(10f);
            detectionRange /= 2f;
        }
        
        private IEnumerator EnhancedStrengthCoroutine()
        {
            damage *= storedStrengthBonus;
            yield return new WaitForSeconds(10f);
            damage /= storedStrengthBonus;
        }
        
        private IEnumerator FrenzyModeCoroutine()
        {
            while (currentPhase == LordRulerPhase.Third)
            {
                if (state == EnemyState.Chase && !isChargingUltimate)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                    
                    if (distanceToPlayer < ultimateAttackRange && Random.value < 0.02f)
                    {
                        StartUltimateAttack();
                    }
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void TryAllomancyAttack()
        {
            if (Time.time - lastAllomancyTime < allomancyCooldown) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > attackRange * 2)
            {
                if (canPushPewter && Random.value < 0.4f)
                {
                    ExecuteSteelPush();
                }
                else if (canPullIron && Random.value < 0.3f)
                {
                    ExecuteIronPull();
                }
            }
            
            if (usesBrass && Random.value < 0.2f)
            {
                ExecuteBrassSoothe();
            }
            
            if (usesZinc && Random.value < 0.15f)
            {
                ExecuteZincRage();
            }
        }
        
        private void ExecuteSteelPush()
        {
            lastAllomancyTime = Time.time;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * allomancyForce * 100f, ForceMode.Impulse);
            }
            
            player.GetComponent<PlayerStats>()?.TakeDamage(20f, DamageType.Allomantic);
            
            animator.SetTrigger("SteelPush");
            SpawnAllomancyVfx("SteelPushVFX", direction);
        }
        
        private void ExecuteIronPull()
        {
            lastAllomancyTime = Time.time;
            
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * allomancyForce * 80f, ForceMode.Impulse);
            }
            
            animator.SetTrigger("IronPull");
            SpawnAllomancyVfx("IronPullVFX", direction);
        }
        
        private void ExecuteBrassSoothe()
        {
            lastAllomancyTime = Time.time;
            
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.ReduceRage(0.3f);
            }
            
            animator.SetTrigger("BrassSoothe");
        }
        
        private void ExecuteZincRage()
        {
            lastAllomancyTime = Time.time;
            
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.ReduceStamina(20f);
            }
            
            animator.SetTrigger("ZincRage");
        }
        
        private void SpawnAllomancyVfx(string vfxName, Vector3 direction)
        {
            GameObject vfx = Resources.Load<GameObject>("Effects/" + vfxName);
            if (vfx != null)
            {
                GameObject instance = Instantiate(vfx, transform.position + Vector3.up, Quaternion.LookRotation(direction));
                Destroy(instance, 2f);
            }
        }
        
        private void StartUltimateAttack()
        {
            isChargingUltimate = true;
            agent.isStopped = true;
            
            animator.SetTrigger("UltimateCharge");
            AudioSource.PlayClipAtPoint(ultimateAttackSound, transform.position);
            
            StartCoroutine(UltimateAttackCoroutine());
        }
        
        private IEnumerator UltimateAttackCoroutine()
        {
            yield return new WaitForSeconds(ultimateChargingTime);
            
            if (currentPhase == LordRulerPhase.Third)
            {
                ExecuteUltimateAttack();
            }
            
            isChargingUltimate = false;
            agent.isStopped = false;
        }
        
        private void ExecuteUltimateAttack()
        {
            isUltimateActive = true;
            
            if (ultimateVfx != null)
            {
                GameObject vfx = Instantiate(ultimateVfx, transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateAttackRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
                    direction.y = 0.5f;
                    direction.Normalize();
                    
                    hitCollider.GetComponent<Rigidbody>()?.AddForce(direction * shockwaveKnockback * 100f, ForceMode.Impulse);
                    
                    hitCollider.GetComponent<PlayerStats>()?.TakeDamage(ultimateDamage, DamageType.Heavy);
                }
            }
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.5f, 1f);
            
            StartCoroutine(ShockwaveCoroutine());
        }
        
        private IEnumerator ShockwaveCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            isUltimateActive = false;
        }
        
        public void ExecuteShockwave()
        {
            AudioSource.PlayClipAtPoint(shockwaveSound, transform.position);
            
            if (shockwaveVfx != null)
            {
                GameObject vfx = Instantiate(shockwaveVfx, transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shockwaveRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
                    direction.y = 0.3f;
                    
                    hitCollider.GetComponent<Rigidbody>()?.AddForce(direction * shockwaveKnockback * 50f, ForceMode.Impulse);
                    
                    hitCollider.GetComponent<PlayerStats>()?.TakeDamage(shockwaveDamage, DamageType.Heavy);
                }
            }
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.3f, 0.5f);
        }
        
        public override float TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            float finalDamage = base.TakeDamage(damage, type);
            
            currentPlatformHealth -= finalDamage * 0.1f;
            
            if (currentPlatformHealth <= 0 && platform != null)
            {
                platform.GetComponent<Collider>().enabled = false;
            }
            
            return finalDamage;
        }
        
        private void RestorePlatformHealth(float percentage)
        {
            currentPlatformHealth = Mathf.Min(currentPlatformHealth + platformHealth * percentage, platformHealth);
        }
        
        protected override void Death()
        {
            StartCoroutine(BossDeathSequence());
        }
        
        private IEnumerator BossDeathSequence()
        {
            animator.SetTrigger("Death");
            
            if (powerAura != null)
                powerAura.Stop();
            
            if (ultimateVfx != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 20f;
                    offset.y = 5f;
                    GameObject vfx = Instantiate(ultimateVfx, transform.position + offset, Quaternion.identity);
                    Destroy(vfx, 2f);
                    yield return new WaitForSeconds(0.3f);
                }
            }
            
            if (destroyPlatformOnDeath && platform != null)
            {
                Rigidbody[] rigidbodies = platform.GetComponentsInChildren<Rigidbody>();
                foreach (var rb in rigidbodies)
                {
                    rb.isKinematic = false;
                    rb.AddExplosionForce(50f, transform.position, 30f);
                }
            }
            
            yield return new WaitForSeconds(2f);
            
            base.Death();
        }
    }
}
