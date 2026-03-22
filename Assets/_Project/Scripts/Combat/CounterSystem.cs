using UnityEngine;
using System.Collections;

namespace MistbornGame.Combat
{
    public class CounterSystem : MonoBehaviour
    {
        [Header("Counter Configuration")]
        [SerializeField] private float counterWindow = 0.4f;
        [SerializeField] private float counterCooldown = 1.5f;
        [SerializeField] private float counterDamageBonus = 2f;
        [SerializeField] private float counterStaminaCost = 25f;
        [SerializeField] private float counterKnockback = 15f;
        
        [Header("Successive Counters")]
        [SerializeField] private int maxConsecutiveCounters = 3;
        [SerializeField] private float counterComboMultiplier = 0.5f;
        
        [Header("Counter Types")]
        [SerializeField] private bool allowLightCounter = true;
        [SerializeField] private bool allowHeavyCounter = true;
        [SerializeField] private bool allowPerfectCounter = true;
        
        [Header("Perfect Counter")]
        [SerializeField] private float perfectCounterWindow = 0.15f;
        [SerializeField] private float perfectCounterDamage = 4f;
        [SerializeField] private float perfectCounterStunDuration = 1f;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem counterVfx;
        [SerializeField] private ParticleSystem perfectCounterVfx;
        [SerializeField] private GameObject counterIndicator;
        [SerializeField] private LineRenderer trajectoryLine;
        
        [Header("Audio")]
        [SerializeField] private AudioClip counterSuccessSound;
        [SerializeField] private AudioClip perfectCounterSound;
        [SerializeField] private AudioClip counterFailSound;
        
        private bool isCounterReady = false;
        private float lastCounterTime = -100f;
        private int consecutiveCounters = 0;
        private float lastEnemyAttackTime = 0f;
        private Enemy.Enemy counterTarget;
        
        private PlayerStats playerStats;
        private Animator animator;
        private Rigidbody playerRb;
        
        public bool IsCounterReady => isCounterReady;
        public int ConsecutiveCounters => consecutiveCounters;
        public float CounterDamageBonus => counterDamageBonus * (1f + consecutiveCounters * counterComboMultiplier);
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            playerRb = GetComponent<Rigidbody>();
            
            if (counterIndicator != null)
                counterIndicator.SetActive(false);
        }
        
        private void Update()
        {
            if (Time.time - lastCounterTime < counterCooldown)
            {
                isCounterReady = false;
            }
            else
            {
                isCounterReady = true;
            }
            
            if (Input.GetButtonDown("Counter") || Input.GetKeyDown(KeyCode.C))
            {
                TryInitiateCounter();
            }
            
            UpdateTrajectoryLine();
        }
        
        private void TryInitiateCounter()
        {
            if (!isCounterReady)
                return;
            
            if (playerStats != null && playerStats.CurrentStamina < counterStaminaCost)
                return;
            
            if (animator != null && animator.GetBool("IsAttacking"))
                return;
            
            if (Time.time - lastEnemyAttackTime > counterWindow)
                return;
            
            InitiateCounter();
        }
        
        private void InitiateCounter()
        {
            lastCounterTime = Time.time;
            consecutiveCounters = 0;
            
            if (playerStats != null)
            {
                playerStats.UseStamina(counterStaminaCost);
            }
            
            animator?.SetTrigger("CounterAttack");
            
            counterIndicator?.SetActive(true);
            
            StartCoroutine(ProcessCounterSequence());
        }
        
        private IEnumerator ProcessCounterSequence()
        {
            yield return new WaitForSeconds(counterWindow * 0.3f);
            
            DetectAndCounterEnemy();
            
            yield return new WaitForSeconds(counterWindow * 0.7f);
            
            counterIndicator?.SetActive(false);
        }
        
        private void DetectAndCounterEnemy()
        {
            if (counterTarget == null)
            {
                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 5f);
                
                foreach (var col in nearbyEnemies)
                {
                    Enemy.Enemy enemy = col.GetComponent<Enemy.Enemy>();
                    if (enemy != null && enemy.IsAttacking)
                    {
                        counterTarget = enemy;
                        break;
                    }
                }
            }
            
            if (counterTarget != null)
            {
                ExecuteCounter(counterTarget);
            }
            else
            {
                AudioSource.PlayClipAtPoint(counterFailSound, transform.position);
            }
        }
        
        private void ExecuteCounter(Enemy.Enemy enemy)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance > 5f)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                direction.y = 0;
                transform.position += direction * (distance - 3f);
            }
            
            Vector3 lookDirection = (enemy.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.forward = lookDirection;
            
            float timeSinceAttack = Time.time - lastEnemyAttackTime;
            
            if (timeSinceAttack <= perfectCounterWindow && allowPerfectCounter)
            {
                ExecutePerfectCounter(enemy);
            }
            else if (timeSinceAttack <= counterWindow * 0.5f && allowHeavyCounter)
            {
                ExecuteHeavyCounter(enemy);
            }
            else if (allowLightCounter)
            {
                ExecuteLightCounter(enemy);
            }
        }
        
        private void ExecuteLightCounter(Enemy.Enemy enemy)
        {
            float damage = enemy.Damage * counterDamageBonus;
            enemy.TakeDamage(damage);
            
            consecutiveCounters++;
            
            Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
            knockbackDirection.y = 0.3f;
            enemy.GetComponent<Rigidbody>()?.AddForce(knockbackDirection * counterKnockback * 50f, ForceMode.Impulse);
            
            AudioSource.PlayClipAtPoint(counterSuccessSound, transform.position);
            
            if (counterVfx != null)
            {
                ParticleSystem vfx = Instantiate(counterVfx, enemy.transform.position, Quaternion.identity);
                Destroy(vfx.gameObject, 2f);
            }
            
            CameraShake(0.1f, 0.2f);
        }
        
        private void ExecuteHeavyCounter(Enemy.Enemy enemy)
        {
            float damage = enemy.Damage * counterDamageBonus * 1.5f;
            enemy.TakeDamage(damage, DamageType.Heavy);
            
            consecutiveCounters++;
            
            enemy.GetComponent<UnityEngine.AI.NavMeshAgent>()?.Stop();
            
            StartCoroutine(StunEnemy(enemy, 0.5f));
            
            AudioSource.PlayClipAtPoint(counterSuccessSound, transform.position);
            
            Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
            enemy.GetComponent<Rigidbody>()?.AddForce(knockbackDirection * counterKnockback * 100f, ForceMode.Impulse);
            
            if (counterVfx != null)
            {
                ParticleSystem vfx = Instantiate(counterVfx, enemy.transform.position, Quaternion.identity);
                Destroy(vfx.gameObject, 2f);
            }
            
            CameraShake(0.2f, 0.3f);
        }
        
        private void ExecutePerfectCounter(Enemy.Enemy enemy)
        {
            float damage = enemy.Damage * perfectCounterDamage;
            enemy.TakeDamage(damage, DamageType.Perfect);
            
            consecutiveCounters = maxConsecutiveCounters;
            
            enemy.GetComponent<UnityEngine.AI.NavMeshAgent>()?.Stop();
            
            StartCoroutine(StunEnemy(enemy, perfectCounterStunDuration));
            
            AudioSource.PlayClipAtPoint(perfectCounterSound, transform.position);
            
            Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
            knockbackDirection.y = 0.5f;
            enemy.GetComponent<Rigidbody>()?.AddForce(knockbackDirection * counterKnockback * 150f, ForceMode.Impulse);
            
            if (perfectCounterVfx != null)
            {
                ParticleSystem vfx = Instantiate(perfectCounterVfx, enemy.transform.position, Quaternion.identity);
                Destroy(vfx.gameObject, 3f);
            }
            
            CameraShake(0.3f, 0.5f);
            
            if (playerStats != null)
            {
                playerStats.AddStamina(counterStaminaCost * 0.5f);
            }
        }
        
        private IEnumerator StunEnemy(Enemy.Enemy enemy, float duration)
        {
            enemy.SetStunned(duration);
            yield return new WaitForSeconds(duration);
        }
        
        public void RegisterEnemyAttack(Enemy.Enemy enemy)
        {
            lastEnemyAttackTime = Time.time;
            counterTarget = enemy;
        }
        
        private void UpdateTrajectoryLine()
        {
            if (trajectoryLine == null || !isCounterReady)
            {
                if (trajectoryLine != null)
                    trajectoryLine.enabled = false;
                return;
            }
            
            trajectoryLine.enabled = true;
            
            Vector3[] positions = new Vector3[20];
            
            for (int i = 0; i < positions.Length; i++)
            {
                float t = i * 0.1f;
                Vector3 startPos = transform.position + Vector3.up;
                Vector3 targetPos = counterTarget != null ? counterTarget.transform.position : transform.position + transform.forward * 5f;
                
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t / (positions.Length * 0.1f));
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 2f;
                
                positions[i] = currentPos;
            }
            
            trajectoryLine.positionCount = positions.Length;
            trajectoryLine.SetPositions(positions);
        }
        
        private void CameraShake(float duration, float intensity)
        {
            Camera.main?.GetComponent<CameraShake>()?.Shake(duration, intensity);
        }
        
        public void ResetCombo()
        {
            consecutiveCounters = 0;
        }
        
        public void UpgradeCounter(string upgradeType)
        {
            switch (upgradeType)
            {
                case "Window":
                    counterWindow += 0.1f;
                    perfectCounterWindow += 0.05f;
                    break;
                case "Damage":
                    counterDamageBonus += 0.5f;
                    break;
                case "Cooldown":
                    counterCooldown = Mathf.Max(counterCooldown - 0.2f, 0.5f);
                    break;
                case "StaminaCost":
                    counterStaminaCost = Mathf.Max(counterStaminaCost - 3f, 10f);
                    break;
            }
        }
    }
}
