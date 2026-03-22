using UnityEngine;

namespace MistbornGameplay
{
    public class MistParticle : MonoBehaviour
    {
        [Header("Particle Settings")]
        [SerializeField] private float particleLifetime = 10f;
        [SerializeField] private float fadeOutDuration = 2f;
        [SerializeField] private float particleSize = 1f;
        [SerializeField] private float driftSpeed = 1f;
        
        [Header("Movement")]
        [SerializeField] private Vector3 driftDirection = Vector3.up;
        [SerializeField] private bool useRandomDrift = true;
        [SerializeField] private float turbulence = 0.5f;
        
        [Header("Visual")]
        [SerializeField] private Material particleMaterial;
        [SerializeField] private Color particleColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
        [SerializeField] private bool useGlow = true;
        [SerializeField] private float glowIntensity = 0.5f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip ambientMistSound;
        [SerializeField] private float audioVolume = 0.3f;
        
        private Renderer particleRenderer;
        private AudioSource audioSource;
        private float spawnTime;
        private float opacity = 1f;
        private Vector3 originalPosition;
        private Vector3 currentDriftDirection;
        
        private void Awake()
        {
            particleRenderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            spawnTime = Time.time;
            originalPosition = transform.position;
            
            if (useRandomDrift)
            {
                currentDriftDirection = Random.insideUnitSphere.normalized;
            }
            else
            {
                currentDriftDirection = driftDirection.normalized;
            }
            
            SetupVisuals();
            SetupAudio();
        }
        
        private void SetupVisuals()
        {
            if (particleRenderer != null && particleMaterial != null)
            {
                particleRenderer.material = particleMaterial;
                particleRenderer.material.color = particleColor;
            }
            
            transform.localScale = Vector3.one * particleSize;
        }
        
        private void SetupAudio()
        {
            if (ambientMistSound != null)
            {
                audioSource.clip = ambientMistSound;
                audioSource.volume = audioVolume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
        private void Update()
        {
            UpdateParticle();
            UpdateVisuals();
            CheckLifetime();
        }
        
        private void UpdateParticle()
        {
            Vector3 drift = currentDriftDirection * driftSpeed * Time.deltaTime;
            transform.position += drift;
            
            if (turbulence > 0f)
            {
                Vector3 turbulenceOffset = new Vector3(
                    Mathf.PerlinNoise(Time.time * turbulence, 0f) - 0.5f,
                    Mathf.PerlinNoise(0f, Time.time * turbulence) - 0.5f,
                    Mathf.PerlinNoise(Time.time * turbulence, Time.time * turbulence) - 0.5f
                );
                
                transform.position += turbulenceOffset * turbulence * Time.deltaTime;
            }
            
            if (useRandomDrift && Random.value < 0.01f)
            {
                currentDriftDirection = Random.insideUnitSphere.normalized;
            }
        }
        
        private void UpdateVisuals()
        {
            float age = Time.time - spawnTime;
            float fadeStart = particleLifetime - fadeOutDuration;
            
            if (age > fadeStart)
            {
                float fadeProgress = (age - fadeStart) / fadeOutDuration;
                opacity = 1f - fadeProgress;
                opacity = Mathf.Clamp01(opacity);
                
                if (particleRenderer != null && particleRenderer.material != null)
                {
                    Color color = particleRenderer.material.color;
                    color.a = opacity * particleColor.a;
                    particleRenderer.material.color = color;
                }
            }
            
            if (useGlow && particleRenderer != null)
            {
                particleRenderer.material.SetFloat("_GlowIntensity", glowIntensity * opacity);
            }
        }
        
        private void CheckLifetime()
        {
            float age = Time.time - spawnTime;
            
            if (age >= particleLifetime)
            {
                DestroyParticle();
            }
        }
        
        private void DestroyParticle()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            Destroy(gameObject);
        }
        
        public void SetDriftDirection(Vector3 direction)
        {
            currentDriftDirection = direction.normalized;
            useRandomDrift = false;
        }
        
        public void SetParticleColor(Color color)
        {
            particleColor = color;
            
            if (particleRenderer != null && particleRenderer.material != null)
            {
                particleRenderer.material.color = particleColor;
            }
        }
        
        public void SetParticleSize(float size)
        {
            particleSize = size;
            transform.localScale = Vector3.one * particleSize;
        }
        
        public void SetOpacity(float newOpacity)
        {
            opacity = newOpacity;
            
            if (particleRenderer != null && particleRenderer.material != null)
            {
                Color color = particleRenderer.material.color;
                color.a = opacity * particleColor.a;
                particleRenderer.material.color = color;
            }
        }
        
        public float GetOpacity()
        {
            return opacity;
        }
        
        public float GetAge()
        {
            return Time.time - spawnTime;
        }
        
        public float GetLifetimePercentage()
        {
            return GetAge() / particleLifetime;
        }
    }
}
