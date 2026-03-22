using UnityEngine;

namespace MistbornGameplay
{
    public class AllomanticFlare : MonoBehaviour
    {
        [Header("Flare Settings")]
        [SerializeField] private float flareMultiplier = 3f;
        [SerializeField] private float flareDuration = 2f;
        [SerializeField] private float metalCostMultiplier = 5f;
        [SerializeField] private float fadeOutSpeed = 2f;
        
        [Header("Effect Settings")]
        [SerializeField] private ParticleSystem flareEffect;
        [SerializeField] private Light flareLight;
        [SerializeField] private float maxLightIntensity = 3f;
        [SerializeField] private float lightFlickerSpeed = 10f;
        
        [Header("Push/Pull Effects")]
        [SerializeField] private float pushForceMultiplier = 3f;
        [SerializeField] private float pullForceMultiplier = 3f;
        [SerializeField] private float rangeMultiplier = 2f;
        
        private Allomancer allomancer;
        private IronPull ironPull;
        private SteelPush steelPush;
        private bool isFlaring = false;
        private float flareEndTime = 0f;
        private float originalPushForce;
        private float originalPullForce;
        private float originalRange;
        
        private void Awake()
        {
            allomancer = GetComponent<Allomancer>();
            ironPull = GetComponent<IronPull>();
            steelPush = GetComponent<SteelPush>();
        }
        
        private void Start()
        {
            if (ironPull != null)
            {
                originalPushForce = ironPull.pushForce;
                originalPullForce = ironPull.pullForce;
                originalRange = ironPull.maxRange;
            }
            
            if (flareLight != null)
            {
                flareLight.intensity = 0f;
            }
            
            if (flareEffect != null)
            {
                flareEffect.Stop();
            }
        }
        
        public void StartFlare()
        {
            if (isFlaring)
            {
                return;
            }
            
            if (allomancer != null && !allomancer.HasEnoughMetal(MetalType.Iron, metalCostMultiplier * Time.deltaTime))
            {
                return;
            }
            
            isFlaring = true;
            flareEndTime = Time.time + flareDuration;
            
            ApplyFlareEffects();
            
            if (flareEffect != null)
            {
                flareEffect.Play();
            }
            
            if (flareLight != null)
            {
                flareLight.enabled = true;
            }
            
            if (OnFlareStarted != null)
            {
                OnFlareStarted();
            }
        }
        
        public void StopFlare()
        {
            if (!isFlaring)
            {
                return;
            }
            
            isFlaring = false;
            
            RemoveFlareEffects();
            
            if (flareEffect != null)
            {
                flareEffect.Stop();
            }
            
            if (OnFlareEnded != null)
            {
                OnFlareEnded();
            }
        }
        
        private void ApplyFlareEffects()
        {
            if (allomancer != null)
            {
                allomancer.SetMetalBurnRate(metalCostMultiplier);
            }
            
            if (ironPull != null)
            {
                ironPull.pushForce = originalPushForce * pushForceMultiplier;
                ironPull.pullForce = originalPullForce * pullForceMultiplier;
                ironPull.maxRange = originalRange * rangeMultiplier;
            }
            
            if (steelPush != null)
            {
                steelPush.pushForce = originalPushForce * pushForceMultiplier;
                steelPush.pullForce = originalPullForce * pullForceMultiplier;
                steelPush.maxRange = originalRange * rangeMultiplier;
            }
        }
        
        private void RemoveFlareEffects()
        {
            if (allomancer != null)
            {
                allomancer.ResetMetalBurnRate();
            }
            
            if (ironPull != null)
            {
                ironPull.pushForce = originalPushForce;
                ironPull.pullForce = originalPullForce;
                ironPull.maxRange = originalRange;
            }
            
            if (steelPush != null)
            {
                steelPush.pushForce = originalPushForce;
                steelPush.pullForce = originalPullForce;
                steelPush.maxRange = originalRange;
            }
        }
        
        private void Update()
        {
            if (isFlaring)
            {
                UpdateFlare();
                
                if (Time.time >= flareEndTime)
                {
                    StopFlare();
                }
            }
            else if (flareLight != null && flareLight.enabled)
            {
                UpdateLightFadeOut();
            }
        }
        
        private void UpdateFlare()
        {
            if (flareLight != null)
            {
                float flicker = Mathf.PerlinNoise(Time.time * lightFlickerSpeed, 0f);
                flareLight.intensity = Mathf.Lerp(maxLightIntensity * 0.7f, maxLightIntensity, flicker);
            }
            
            if (allomancer != null && !allomancer.HasEnoughMetal(MetalType.Iron, metalCostMultiplier * Time.deltaTime))
            {
                StopFlare();
            }
        }
        
        private void UpdateLightFadeOut()
        {
            if (flareLight != null)
            {
                flareLight.intensity = Mathf.Lerp(flareLight.intensity, 0f, fadeOutSpeed * Time.deltaTime);
                
                if (flareLight.intensity < 0.01f)
                {
                    flareLight.enabled = false;
                }
            }
        }
        
        public bool IsFlaring()
        {
            return isFlaring;
        }
        
        public float GetRemainingFlareTime()
        {
            return Mathf.Max(0f, flareEndTime - Time.time);
        }
        
        public float GetFlareMultiplier()
        {
            return flareMultiplier;
        }
        
        public void SetFlareDuration(float duration)
        {
            flareDuration = duration;
        }
        
        public void SetFlareMultiplier(float multiplier)
        {
            flareMultiplier = multiplier;
        }
        
        public void SetMetalCostMultiplier(float cost)
        {
            metalCostMultiplier = cost;
        }
        
        public event System.Action OnFlareStarted;
        public event System.Action OnFlareEnded;
    }
}
