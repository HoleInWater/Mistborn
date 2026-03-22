using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class AshfallSystem : MonoBehaviour
    {
        [Header("Ashfall Settings")]
        [SerializeField] private float ashfallDensity = 0.5f;
        [SerializeField] private float ashfallRadius = 50f;
        [SerializeField] private float ashfallHeight = 30f;
        [SerializeField] private float ashParticleSize = 0.1f;
        
        [Header("Particle Settings")]
        [SerializeField] private ParticleSystem ashParticles;
        [SerializeField] private Material ashMaterial;
        [SerializeField] private Color ashColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        [Header("Accumulation")]
        [SerializeField] private bool accumulateOnSurfaces = true;
        [SerializeField] private float accumulationRate = 0.1f;
        [SerializeField] private float maxAccumulation = 10f;
        [SerializeField] private LayerMask accumulationLayers;
        
        [Header("Environmental Effects")]
        [SerializeField] private bool reduceVisibility = true;
        [SerializeField] private float visibilityReduction = 0.3f;
        [SerializeField] private bool affectStealth = true;
        [SerializeField] private float stealthBonus = 0.2f;
        
        [Header("Player Effects")]
        [SerializeField] private bool causesDiscomfort = true;
        [SerializeField] private float discomfortInterval = 5f;
        [SerializeField] private float discomfortAmount = 1f;
        
        [Header("Visual Settings")]
        [SerializeField] private bool tintLighting = true;
        [SerializeField] private Color lightingTint = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private float ambientLightReduction = 0.2f;
        
        private Dictionary<GameObject, float> surfaceAccumulations = new Dictionary<GameObject, float>();
        private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
        private Transform playerTransform;
        private float lastDiscomfortTime = 0f;
        private Color originalAmbientLight;
        
        private void Awake()
        {
            if (ashParticles != null)
            {
                SetupAshParticles();
            }
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            originalAmbientLight = RenderSettings.ambientLight;
            
            StartAshfall();
        }
        
        private void SetupAshParticles()
        {
            var main = ashParticles.main;
            main.startColor = ashColor;
            main.startSize = ashParticleSize;
            
            var shape = ashParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.boxThickness = new Vector3(ashfallRadius * 2f, ashfallHeight, ashfallRadius * 2f);
            
            var emission = ashParticles.emission;
            emission.rateOverTime = ashfallDensity * 100f;
        }
        
        public void StartAshfall()
        {
            if (ashParticles != null)
            {
                ashParticles.Play();
            }
            
            if (tintLighting)
            {
                ApplyLightingTint();
            }
            
            if (OnAshfallStarted != null)
            {
                OnAshfallStarted();
            }
        }
        
        public void StopAshfall()
        {
            if (ashParticles != null)
            {
                ashParticles.Stop();
            }
            
            if (tintLighting)
            {
                RemoveLightingTint();
            }
            
            ClearAccumulations();
            
            if (OnAshfallStopped != null)
            {
                OnAshfallStopped();
            }
        }
        
        private void Update()
        {
            UpdatePlayerEffects();
            UpdateAccumulation();
        }
        
        private void UpdatePlayerEffects()
        {
            if (playerTransform == null)
            {
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer > ashfallRadius)
            {
                return;
            }
            
            if (causesDiscomfort && Time.time - lastDiscomfortTime >= discomfortInterval)
            {
                lastDiscomfortTime = Time.time;
                ApplyDiscomfort();
            }
            
            if (reduceVisibility)
            {
                ApplyVisibilityEffect(distanceToPlayer);
            }
            
            if (affectStealth)
            {
                ApplyStealthEffect();
            }
        }
        
        private void ApplyDiscomfort()
        {
            PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.MaxHealth, -discomfortAmount);
            }
        }
        
        private void ApplyVisibilityEffect(float distance)
        {
            MistVision mistVision = playerTransform.GetComponent<MistVision>();
            if (mistVision != null)
            {
                float distanceFactor = 1f - (distance / ashfallRadius);
                float reduction = visibilityReduction * distanceFactor;
                mistVision.SetMistOpacity(reduction);
            }
        }
        
        private void ApplyStealthEffect()
        {
            StealthSystem stealth = playerTransform.GetComponent<StealthSystem>();
            if (stealth != null)
            {
                stealth.ModifyStealth(stealthBonus * Time.deltaTime);
            }
        }
        
        private void UpdateAccumulation()
        {
            if (!accumulateOnSurfaces)
            {
                return;
            }
            
            Collider[] surfaces = Physics.OverlapSphere(transform.position, ashfallRadius, accumulationLayers);
            
            foreach (Collider surface in surfaces)
            {
                if (!surfaceAccumulations.ContainsKey(surface.gameObject))
                {
                    surfaceAccumulations[surface.gameObject] = 0f;
                    Renderer renderer = surface.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        originalMaterials[surface.gameObject] = renderer.material;
                    }
                }
                
                surfaceAccumulations[surface.gameObject] += accumulationRate * Time.deltaTime;
                surfaceAccumulations[surface.gameObject] = Mathf.Min(surfaceAccumulations[surface.gameObject], maxAccumulation);
                
                UpdateSurfaceAppearance(surface.gameObject);
            }
            
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var kvp in surfaceAccumulations)
            {
                if (!surfaces.Any(s => s.gameObject == kvp.Key))
                {
                    RemoveAccumulation(kvp.Key);
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (GameObject obj in toRemove)
            {
                surfaceAccumulations.Remove(obj);
                originalMaterials.Remove(obj);
            }
        }
        
        private void UpdateSurfaceAppearance(GameObject surface)
        {
            Renderer renderer = surface.GetComponent<Renderer>();
            if (renderer != null)
            {
                float accumulation = surfaceAccumulations[surface];
                float t = accumulation / maxAccumulation;
                
                if (ashMaterial != null)
                {
                    renderer.material = ashMaterial;
                    Color color = Color.Lerp(originalMaterials[surface].color, ashColor, t);
                    renderer.material.color = color;
                }
            }
        }
        
        private void RemoveAccumulation(GameObject surface)
        {
            Renderer renderer = surface.GetComponent<Renderer>();
            if (renderer != null && originalMaterials.ContainsKey(surface))
            {
                renderer.material = originalMaterials[surface];
            }
        }
        
        private void ClearAccumulations()
        {
            foreach (var kvp in surfaceAccumulations)
            {
                RemoveAccumulation(kvp.Key);
            }
            
            surfaceAccumulations.Clear();
            originalMaterials.Clear();
        }
        
        private void ApplyLightingTint()
        {
            RenderSettings.ambientLight = Color.Lerp(originalAmbientLight, lightingTint * originalAmbientLight, ambientLightReduction);
        }
        
        private void RemoveLightingTint()
        {
            RenderSettings.ambientLight = originalAmbientLight;
        }
        
        public float GetAshfallDensity()
        {
            return ashfallDensity;
        }
        
        public void SetAshfallDensity(float density)
        {
            ashfallDensity = density;
            
            if (ashParticles != null)
            {
                var emission = ashParticles.emission;
                emission.rateOverTime = ashfallDensity * 100f;
            }
        }
        
        public float GetAccumulationAmount(GameObject surface)
        {
            if (surfaceAccumulations.ContainsKey(surface))
            {
                return surfaceAccumulations[surface];
            }
            
            return 0f;
        }
        
        public Dictionary<GameObject, float> GetAllAccumulations()
        {
            return new Dictionary<GameObject, float>(surfaceAccumulations);
        }
        
        public void ClearPlayerEffects()
        {
            if (playerTransform != null)
            {
                MistVision mistVision = playerTransform.GetComponent<MistVision>();
                if (mistVision != null)
                {
                    mistVision.SetMistOpacity(0f);
                }
            }
        }
        
        public event System.Action OnAshfallStarted;
        public event System.Action OnAshfallStopped;
    }
}
