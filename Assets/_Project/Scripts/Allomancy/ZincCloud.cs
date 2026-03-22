using UnityEngine;

namespace MistbornGameplay
{
    public class ZincCloud : MonoBehaviour
    {
        [Header("Cloud Settings")]
        [SerializeField] private float cloudRadius = 10f;
        [SerializeField] private float cloudHeight = 5f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private LayerMask affectLayer;
        
        [Header("Rage Effect Settings")]
        [SerializeField] private float rageDamageMultiplier = 1.5f;
        [SerializeField] private float rageSpeedMultiplier = 1.25f;
        [SerializeField] private float rageAttackSpeedMultiplier = 1.3f;
        
        [Header("Visual Settings")]
        [SerializeField] private Material cloudMaterial;
        [SerializeField] private ParticleSystem cloudParticles;
        [SerializeField] private Color cloudColor = new Color(1f, 0.5f, 0f, 0.5f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private AudioClip rageLoopSound;
        
        private MeshRenderer cloudRenderer;
        private SphereCollider detectionCollider;
        private AudioSource audioSource;
        private bool isActive = false;
        private bool isFading = false;
        private float fadeStartTime;
        private float fadeStartAlpha;
        
        private void Awake()
        {
            cloudRenderer = GetComponent<MeshRenderer>();
            if (cloudRenderer == null)
            {
                cloudRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            
            detectionCollider = GetComponent<SphereCollider>();
            if (detectionCollider == null)
            {
                detectionCollider = gameObject.AddComponent<SphereCollider>();
            }
            detectionCollider.radius = cloudRadius;
            detectionCollider.isTrigger = true;
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
        }
        
        private void Start()
        {
            SetupVisual();
            
            if (cloudParticles != null)
            {
                cloudParticles.Stop();
            }
            
            SetCloudActive(false);
        }
        
        private void SetupVisual()
        {
            if (cloudRenderer != null && cloudMaterial != null)
            {
                cloudRenderer.material = cloudMaterial;
                cloudRenderer.material.color = cloudColor;
                cloudRenderer.enabled = false;
            }
            
            transform.localScale = new Vector3(cloudRadius * 2f, cloudHeight, cloudRadius * 2f);
        }
        
        public void Activate()
        {
            if (isActive && !isFading)
            {
                return;
            }
            
            isActive = true;
            isFading = true;
            fadeStartTime = Time.time;
            fadeStartAlpha = cloudRenderer != null ? cloudRenderer.material.color.a : 0f;
            
            if (cloudRenderer != null)
            {
                cloudRenderer.enabled = true;
            }
            
            if (cloudParticles != null)
            {
                cloudParticles.Play();
            }
            
            if (audioSource != null && activationSound != null)
            {
                audioSource.clip = activationSound;
                audioSource.Play();
                
                Invoke(nameof(StartLoopSound), activationSound.length);
            }
            
            if (OnCloudActivated != null)
            {
                OnCloudActivated();
            }
        }
        
        public void Deactivate()
        {
            if (!isActive && !isFading)
            {
                return;
            }
            
            isActive = false;
            isFading = true;
            fadeStartTime = Time.time;
            fadeStartAlpha = cloudRenderer != null ? cloudRenderer.material.color.a : 1f;
            
            if (cloudParticles != null)
            {
                cloudParticles.Stop();
            }
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            RemoveEffectsFromAll();
            
            if (OnCloudDeactivated != null)
            {
                OnCloudDeactivated();
            }
        }
        
        private void StartLoopSound()
        {
            if (rageLoopSound != null && audioSource != null && isActive)
            {
                audioSource.clip = rageLoopSound;
                audioSource.Play();
            }
        }
        
        private void Update()
        {
            if (isFading)
            {
                UpdateFade();
            }
            
            UpdateAOEEffect();
        }
        
        private void UpdateFade()
        {
            float duration = isActive ? fadeInDuration : fadeOutDuration;
            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            float targetAlpha = isActive ? cloudColor.a : 0f;
            float currentAlpha = Mathf.Lerp(fadeStartAlpha, targetAlpha, t);
            
            if (cloudRenderer != null)
            {
                Color currentColor = cloudRenderer.material.color;
                cloudRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
            }
            
            if (t >= 1f)
            {
                isFading = false;
                
                if (!isActive && cloudRenderer != null)
                {
                    cloudRenderer.enabled = false;
                }
            }
        }
        
        private void UpdateAOEEffect()
        {
            if (!isActive)
            {
                return;
            }
            
            Collider[] affectedColliders = Physics.OverlapSphere(transform.position, cloudRadius, affectLayer);
            
            foreach (Collider collider in affectedColliders)
            {
                ApplyRageEffect(collider);
            }
        }
        
        private void ApplyRageEffect(Collider target)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ModifyDamageMultiplier(rageDamageMultiplier);
                enemy.ModifySpeed(rageSpeedMultiplier);
                enemy.ModifyAttackSpeed(rageAttackSpeedMultiplier);
                
                if (enemy is AIController ai)
                {
                    ai.SetAggressionLevel(1f);
                }
            }
            
            PlayerStats stats = target.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.AttackPower, rageDamageMultiplier - 1f);
                stats.ModifyStat(StatType.MovementSpeed, rageSpeedMultiplier - 1f);
            }
        }
        
        private void RemoveEffectsFromAll()
        {
            Collider[] affectedColliders = Physics.OverlapSphere(transform.position, cloudRadius, affectLayer);
            
            foreach (Collider collider in affectedColliders)
            {
                RemoveRageEffect(collider);
            }
        }
        
        private void RemoveRageEffect(Collider target)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ModifyDamageMultiplier(1f / rageDamageMultiplier);
                enemy.ModifySpeed(1f / rageSpeedMultiplier);
                enemy.ModifyAttackSpeed(1f / rageAttackSpeedMultiplier);
                
                if (enemy is AIController ai)
                {
                    ai.SetAggressionLevel(0.5f);
                }
            }
            
            PlayerStats stats = target.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.AttackPower, 1f - rageDamageMultiplier);
                stats.ModifyStat(StatType.MovementSpeed, 1f - rageSpeedMultiplier);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!isActive)
            {
                return;
            }
            
            ApplyRageEffect(other);
        }
        
        public bool IsActive()
        {
            return isActive;
        }
        
        public float GetRadius()
        {
            return cloudRadius;
        }
        
        public void SetCloudColor(Color color)
        {
            cloudColor = color;
            if (cloudRenderer != null && cloudRenderer.material != null)
            {
                cloudRenderer.material.color = cloudColor;
            }
        }
        
        public void SetRadius(float radius)
        {
            cloudRadius = radius;
            transform.localScale = new Vector3(cloudRadius * 2f, cloudHeight, cloudRadius * 2f);
            if (detectionCollider != null)
            {
                detectionCollider.radius = cloudRadius;
            }
        }
        
        public void SetRageMultipliers(float damage, float speed, float attackSpeed)
        {
            rageDamageMultiplier = damage;
            rageSpeedMultiplier = speed;
            rageAttackSpeedMultiplier = attackSpeed;
        }
        
        public event System.Action OnCloudActivated;
        public event System.Action OnCloudDeactivated;
    }
}
