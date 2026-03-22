using UnityEngine;

namespace MistbornGame.Allomancy
{
    public class TinEyes : MonoBehaviour
    {
        [Header("Tin Enhancement")]
        [SerializeField] private float enhancedVisionRange = 200f;
        [SerializeField] private float enhancedHearingRange = 50f;
        [SerializeField] private float normalVisionRange = 100f;
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem tinGlow;
        [SerializeField] private Material tinEnhancedMaterial;
        [SerializeField] private Renderer playerRenderer;
        
        [Header("Settings")]
        [SerializeField] private float senseBoostMultiplier = 1.5f;
        
        private Allomancer allomancer;
        private Camera playerCamera;
        private AudioListener playerAudio;
        
        private void Start()
        {
            allomancer = GetComponent<Allomancer>();
            playerCamera = GetComponentInChildren<Camera>();
            playerAudio = GetComponentInChildren<AudioListener>();
        }
        
        private void Update()
        {
            if (allomancer != null && allomancer.IsBurning(MetalType.Tin))
            {
                EnableTinSenses();
            }
            else
            {
                DisableTinSenses();
            }
        }
        
        private void EnableTinSenses()
        {
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = enhancedVisionRange;
            }
            
            if (tinGlow != null && !tinGlow.isPlaying)
            {
                tinGlow.Play();
            }
            
            if (playerRenderer != null && tinEnhancedMaterial != null)
            {
                playerRenderer.material = tinEnhancedMaterial;
            }
        }
        
        private void DisableTinSenses()
        {
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = normalVisionRange;
            }
            
            if (tinGlow != null && tinGlow.isPlaying)
            {
                tinGlow.Stop();
            }
        }
        
        public float GetEnhancedVisionRange()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Tin) 
                ? enhancedVisionRange * senseBoostMultiplier 
                : normalVisionRange;
        }
        
        public float GetEnhancedHearingRange()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Tin) 
                ? enhancedHearingRange * senseBoostMultiplier 
                : 10f;
        }
        
        public bool IsEnhanced()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Tin);
        }
    }
}
