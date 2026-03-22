using UnityEngine;
using System.Collections;

namespace MistbornGame.Environment
{
    public class CandleHolder : MonoBehaviour
    {
        [Header("Candle Configuration")]
        [SerializeField] private bool isLit = false;
        [SerializeField] private float lightRadius = 5f;
        [SerializeField] private float lightIntensity = 1f;
        [SerializeField] private float detectionRange = 15f;
        
        [Header("Light Source")]
        [SerializeField] private Light candleLight;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private GameObject flameObject;
        
        [Header("Flicker Effect")]
        [SerializeField] private bool enableFlicker = true;
        [SerializeField] private float flickerSpeed = 10f;
        [SerializeField] private float flickerIntensity = 0.1f;
        
        [Header("Allomancy Interaction")]
        [SerializeField] private bool canBePushed = false;
        [SerializeField] private float pushForce = 10f;
        [SerializeField] private bool canBePulled = false;
        [SerializeField] private float pullForce = 8f;
        
        [Header("Combat")]
        [SerializeField] private bool causesSteelPush = false;
        [SerializeField] private bool causesIronPull = false;
        
        private bool hasFlame = false;
        private float baseIntensity;
        private Rigidbody rb;
        
        public bool IsLit => isLit;
        public float LightRadius => lightRadius;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            
            if (candleLight != null)
            {
                baseIntensity = candleLight.intensity;
                candleLight.enabled = false;
            }
            
            if (flameObject != null)
                flameObject.SetActive(false);
            
            if (flameParticles != null)
                flameParticles.Stop();
            
            LightCandle();
        }
        
        private void Update()
        {
            if (enableFlicker && isLit)
            {
                ApplyFlicker();
            }
            
            UpdateLightPosition();
        }
        
        private void ApplyFlicker()
        {
            if (candleLight == null)
                return;
            
            float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * flickerIntensity;
            candleLight.intensity = baseIntensity * lightIntensity * (1f + flicker);
        }
        
        private void UpdateLightPosition()
        {
            if (candleLight != null && flameObject != null)
            {
                candleLight.transform.position = flameObject.transform.position;
            }
        }
        
        public void LightCandle()
        {
            if (isLit)
                return;
            
            isLit = true;
            
            if (candleLight != null)
            {
                candleLight.enabled = true;
                candleLight.intensity = baseIntensity * lightIntensity;
            }
            
            if (flameObject != null)
                flameObject.SetActive(true);
            
            if (flameParticles != null)
                flameParticles.Play();
            
            hasFlame = true;
            
            StartCoroutine(FlickerIgnition());
        }
        
        private IEnumerator FlickerIgnition()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float scale = Mathf.Lerp(0f, 1f, elapsed / duration);
                flameObject.transform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            flameObject.transform.localScale = Vector3.one;
        }
        
        public void ExtinguishCandle()
        {
            if (!isLit)
                return;
            
            isLit = false;
            hasFlame = false;
            
            if (candleLight != null)
                candleLight.enabled = false;
            
            if (flameObject != null)
                flameObject.SetActive(false);
            
            if (flameParticles != null)
                flameParticles.Stop();
        }
        
        public void ToggleCandle()
        {
            if (isLit)
                ExtinguishCandle();
            else
                LightCandle();
        }
        
        public void ApplyForceFromDirection(Vector3 direction, float force)
        {
            if (causesSteelPush && rb != null)
            {
                direction.y = 0.3f;
                rb.AddForce(direction * force, ForceMode.Impulse);
            }
        }
        
        public void PullFromDirection(Vector3 sourcePosition, float force)
        {
            if (causesIronPull && rb != null)
            {
                Vector3 direction = (transform.position - sourcePosition).normalized;
                direction.y = 0;
                rb.AddForce(direction * force, ForceMode.Impulse);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.relativeVelocity.magnitude > 5f)
                {
                    ExtinguishCandle();
                }
            }
        }
        
        public void SetLightRadius(float radius)
        {
            lightRadius = radius;
            if (candleLight != null)
            {
                candleLight.range = radius;
            }
        }
        
        public void SetLightIntensity(float intensity)
        {
            lightIntensity = intensity;
            if (candleLight != null)
            {
                candleLight.intensity = baseIntensity * intensity;
            }
        }
    }
}
