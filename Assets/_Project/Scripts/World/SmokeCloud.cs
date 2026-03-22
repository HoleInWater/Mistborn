using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class SmokeCloud : MonoBehaviour
    {
        [Header("Cloud Settings")]
        [SerializeField] private float cloudRadius = 10f;
        [SerializeField] private float cloudHeight = 8f;
        [SerializeField] private float fadeInDuration = 2f;
        [SerializeField] private float fadeOutDuration = 3f;
        [SerializeField] private float cloudDensity = 0.5f;
        
        [Header("Smoke Effects")]
        [SerializeField] private bool causesBlindness = true;
        [SerializeField] private float blindnessRadius = 5f;
        [SerializeField] private float visionReduction = 0.8f;
        [SerializeField] private bool causesSuffocation = false;
        [SerializeField] private float suffocationDamage = 5f;
        [SerializeField] private float suffocationInterval = 1f;
        
        [Header("Visual Settings")]
        [SerializeField] private Material smokeMaterial;
        [SerializeField] private ParticleSystem smokeParticles;
        [SerializeField] private Color smokeColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private bool useVolumetricEffect = true;
        
        [Header("Movement")]
        [SerializeField] private bool movesWithWind = true;
        [SerializeField] private Vector3 windDirection = Vector3.right;
        [SerializeField] private float windSpeed = 1f;
        [SerializeField] private float turbulence = 0.5f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip smokeAmbientSound;
        [SerializeField] private AudioClip smokeSpreadSound;
        
        private MeshRenderer cloudRenderer;
        private SphereCollider detectionCollider;
        private AudioSource audioSource;
        private bool isActive = false;
        private bool isFading = false;
        private float fadeStartTime;
        private float fadeStartAlpha;
        private Vector3 currentPosition;
        private float lastSuffocationTime;
        
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
        }
        
        private void Start()
        {
            SetupVisual();
            
            if (smokeParticles != null)
            {
                smokeParticles.Stop();
            }
            
            SetCloudActive(false);
            currentPosition = transform.position;
        }
        
        private void SetupVisual()
        {
            if (cloudRenderer != null && smokeMaterial != null)
            {
                cloudRenderer.material = smokeMaterial;
                cloudRenderer.material.color = smokeColor;
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
            
            if (smokeParticles != null)
            {
                smokeParticles.Play();
            }
            
            if (audioSource != null && smokeSpreadSound != null)
            {
                audioSource.PlayOneShot(smokeSpreadSound);
            }
            
            Invoke(nameof(StartAmbientSound), smokeSpreadSound != null ? smokeSpreadSound.length : 0f);
            
            if (OnSmokeCloudActivated != null)
            {
                OnSmokeCloudActivated();
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
            
            if (smokeParticles != null)
            {
                smokeParticles.Stop();
            }
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            if (OnSmokeCloudDeactivated != null)
            {
                OnSmokeCloudDeactivated();
            }
        }
        
        private void StartAmbientSound()
        {
            if (smokeAmbientSound != null && audioSource != null && isActive)
            {
                audioSource.clip = smokeAmbientSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
        private void Update()
        {
            if (isFading)
            {
                UpdateFade();
            }
            
            if (isActive)
            {
                UpdateAOEEffects();
                
                if (movesWithWind)
                {
                    UpdateWindMovement();
                }
            }
        }
        
        private void UpdateFade()
        {
            float duration = isActive ? fadeInDuration : fadeOutDuration;
            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            float targetAlpha = isActive ? smokeColor.a * cloudDensity : 0f;
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
        
        private void UpdateAOEEffects()
        {
            Collider[] affectedColliders = Physics.OverlapSphere(transform.position, cloudRadius);
            
            foreach (Collider collider in affectedColliders)
            {
                ApplySmokeEffect(collider);
            }
        }
        
        private void ApplySmokeEffect(Collider target)
        {
            if (causesBlindness)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                
                if (distance <= blindnessRadius)
                {
                    MistVision mistVision = target.GetComponent<MistVision>();
                    if (mistVision != null)
                    {
                        float reduction = visionReduction * (1f - (distance / blindnessRadius));
                        mistVision.SetMistOpacity(reduction);
                    }
                }
            }
            
            if (causesSuffocation && Time.time - lastSuffocationTime >= suffocationInterval)
            {
                lastSuffocationTime = Time.time;
                
                PlayerStats stats = target.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(suffocationDamage);
                }
                
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(suffocationDamage);
                }
            }
        }
        
        private void UpdateWindMovement()
        {
            Vector3 windMovement = windDirection.normalized * windSpeed * Time.deltaTime;
            
            Vector3 turbulenceOffset = new Vector3(
                Mathf.PerlinNoise(Time.time * turbulence, 0f) - 0.5f,
                0f,
                Mathf.PerlinNoise(0f, Time.time * turbulence) - 0.5f
            );
            
            windMovement += turbulenceOffset * turbulence * Time.deltaTime;
            
            currentPosition += windMovement;
            transform.position = currentPosition;
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!isActive)
            {
                return;
            }
            
            ApplySmokeEffect(other);
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
            smokeColor = color;
            if (cloudRenderer != null && cloudRenderer.material != null)
            {
                cloudRenderer.material.color = smokeColor;
            }
        }
        
        public void SetDensity(float density)
        {
            cloudDensity = Mathf.Clamp01(density);
        }
        
        public void SetWindDirection(Vector3 direction)
        {
            windDirection = direction;
        }
        
        public void SetWindSpeed(float speed)
        {
            windSpeed = speed;
        }
        
        public event System.Action OnSmokeCloudActivated;
        public event System.Action OnSmokeCloudDeactivated;
    }
}
