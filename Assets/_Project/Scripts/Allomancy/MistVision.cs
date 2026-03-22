using UnityEngine;

namespace MistbornGameplay
{
    public class MistVision : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float normalViewDistance = 100f;
        [SerializeField] private float mistViewDistance = 200f;
        [SerializeField] private float nightVisionIntensity = 1.5f;
        
        [Header("Metal Detection")]
        [SerializeField] private bool enableMetalDetection = true;
        [SerializeField] private float metalDetectionRange = 50f;
        [SerializeField] private Color metalDetectionColor = new Color(1f, 0.8f, 0f, 1f);
        
        [Header("Visual Effects")]
        [SerializeField] private Material mistOverlayMaterial;
        [SerializeField] private ParticleSystem mistParticles;
        [SerializeField] private float mistOpacity = 0.3f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip mistVisionStartSound;
        [SerializeField] private AudioClip mistVisionLoopSound;
        
        private Camera playerCamera;
        private AudioSource audioSource;
        private bool isMistVisionActive = false;
        private float originalViewDistance;
        private float originalAmbientIntensity;
        
        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (playerCamera != null)
            {
                originalViewDistance = playerCamera.farClipPlane;
            }
            
            if (mistParticles != null)
            {
                mistParticles.Stop();
            }
            
            RenderSettings.ambientLight = Color.white;
        }
        
        public void ActivateMistVision()
        {
            if (isMistVisionActive)
            {
                return;
            }
            
            isMistVisionActive = true;
            
            ApplyMistVisionEffects();
            
            if (audioSource != null && mistVisionStartSound != null)
            {
                audioSource.clip = mistVisionStartSound;
                audioSource.Play();
                
                Invoke(nameof(StartLoopSound), mistVisionStartSound.length);
            }
            
            if (mistParticles != null)
            {
                mistParticles.Play();
            }
            
            if (OnMistVisionActivated != null)
            {
                OnMistVisionActivated();
            }
        }
        
        public void DeactivateMistVision()
        {
            if (!isMistVisionActive)
            {
                return;
            }
            
            isMistVisionActive = false;
            
            RemoveMistVisionEffects();
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            if (mistParticles != null)
            {
                mistParticles.Stop();
            }
            
            if (OnMistVisionDeactivated != null)
            {
                OnMistVisionDeactivated();
            }
        }
        
        private void StartLoopSound()
        {
            if (mistVisionLoopSound != null && audioSource != null && isMistVisionActive)
            {
                audioSource.clip = mistVisionLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
        private void ApplyMistVisionEffects()
        {
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = mistViewDistance;
            }
            
            RenderSettings.ambientLight = Color.gray * nightVisionIntensity;
            
            if (mistOverlayMaterial != null)
            {
                mistOverlayMaterial.color = new Color(
                    mistOverlayMaterial.color.r,
                    mistOverlayMaterial.color.g,
                    mistOverlayMaterial.color.b,
                    mistOpacity
                );
            }
        }
        
        private void RemoveMistVisionEffects()
        {
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = originalViewDistance;
            }
            
            RenderSettings.ambientLight = Color.white;
            
            if (mistOverlayMaterial != null)
            {
                mistOverlayMaterial.color = new Color(
                    mistOverlayMaterial.color.r,
                    mistOverlayMaterial.color.g,
                    mistOverlayMaterial.color.b,
                    0f
                );
            }
        }
        
        private void Update()
        {
            if (isMistVisionActive && enableMetalDetection)
            {
                DetectMetals();
            }
        }
        
        private void DetectMetals()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, metalDetectionRange);
            
            foreach (Collider collider in nearbyColliders)
            {
                MetalVial vial = collider.GetComponent<MetalVial>();
                if (vial != null)
                {
                    HighlightMetalObject(vial.gameObject);
                }
                
                PushableObject pushable = collider.GetComponent<PushableObject>();
                if (pushable != null)
                {
                    HighlightMetalObject(pushable.gameObject);
                }
                
                PullableObject pullable = collider.GetComponent<PullableObject>();
                if (pullable != null)
                {
                    HighlightMetalObject(pullable.gameObject);
                }
            }
        }
        
        private void HighlightMetalObject(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    mat.SetColor("_OutlineColor", metalDetectionColor);
                    mat.SetFloat("_OutlineWidth", 0.05f);
                }
            }
        }
        
        public bool IsMistVisionActive()
        {
            return isMistVisionActive;
        }
        
        public void SetMistOpacity(float opacity)
        {
            mistOpacity = Mathf.Clamp01(opacity);
            
            if (mistOverlayMaterial != null)
            {
                mistOverlayMaterial.color = new Color(
                    mistOverlayMaterial.color.r,
                    mistOverlayMaterial.color.g,
                    mistOverlayMaterial.color.b,
                    mistOpacity
                );
            }
        }
        
        public void SetMetalDetectionEnabled(bool enabled)
        {
            enableMetalDetection = enabled;
        }
        
        public event System.Action OnMistVisionActivated;
        public event System.Action OnMistVisionDeactivated;
    }
}
