using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Effects
{
    public class VisualEffectsManager : MonoBehaviour
    {
        [Header("Particle Pools")]
        [SerializeField] private int hitEffectPoolSize = 20;
        [SerializeField] private int deathEffectPoolSize = 10;
        [SerializeField] private int abilityEffectPoolSize = 15;
        
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private GameObject criticalHitEffectPrefab;
        [SerializeField] private GameObject metalBurningEffectPrefab;
        
        [Header("Screen Effects")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Material damageOverlayMaterial;
        [SerializeField] private Material healOverlayMaterial;
        
        [Header("Settings")]
        [SerializeField] private float damageFlashDuration = 0.2f;
        [SerializeField] private float healFlashDuration = 0.3f;
        
        private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();
        
        public static VisualEffectsManager instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            
            InitializePools();
        }
        
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        private void InitializePools()
        {
            if (hitEffectPrefab != null)
            {
                CreatePool("HitEffect", hitEffectPrefab, hitEffectPoolSize);
            }
            
            if (deathEffectPrefab != null)
            {
                CreatePool("DeathEffect", deathEffectPrefab, deathEffectPoolSize);
            }
            
            if (criticalHitEffectPrefab != null)
            {
                CreatePool("CriticalHitEffect", criticalHitEffectPrefab, 5);
            }
            
            if (metalBurningEffectPrefab != null)
            {
                CreatePool("MetalBurningEffect", metalBurningEffectPrefab, abilityEffectPoolSize);
            }
        }
        
        private void CreatePool(string poolName, GameObject prefab, int size)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            
            effectPools[poolName] = pool;
        }
        
        public void PlayHitEffect(Vector3 position, Color color)
        {
            PlayEffect("HitEffect", position, color);
        }
        
        public void PlayDeathEffect(Vector3 position)
        {
            PlayEffect("DeathEffect", position, Color.white);
        }
        
        public void PlayCriticalHitEffect(Vector3 position)
        {
            PlayEffect("CriticalHitEffect", position, Color.red);
        }
        
        public void PlayMetalBurningEffect(Transform target, Color metalColor)
        {
            PlayEffect("MetalBurningEffect", target.position + Vector3.up, metalColor);
        }
        
        private void PlayEffect(string poolName, Vector3 position, Color color)
        {
            if (!effectPools.ContainsKey(poolName))
                return;
            
            Queue<GameObject> pool = effectPools[poolName];
            
            GameObject effect;
            
            if (pool.Count > 0)
            {
                effect = pool.Dequeue();
                effect.SetActive(true);
            }
            else
            {
                return;
            }
            
            effect.transform.position = position;
            effect.transform.rotation = Quaternion.identity;
            
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Clear();
                particles.Play();
            }
            
            Renderer renderer = effect.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = color;
            }
            
            StartCoroutine(ReturnToPool(effect, poolName, 2f));
        }
        
        private System.Collections.IEnumerator ReturnToPool(GameObject obj, string poolName, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (obj != null)
            {
                obj.SetActive(false);
                
                if (effectPools.ContainsKey(poolName))
                {
                    effectPools[poolName].Enqueue(obj);
                }
            }
        }
        
        public void PlayDamageFlash()
        {
            if (mainCamera == null || damageOverlayMaterial == null)
                return;
            
            StartCoroutine(DamageFlashCoroutine());
        }
        
        private System.Collections.IEnumerator DamageFlashCoroutine()
        {
            if (mainCamera != null && damageOverlayMaterial != null)
            {
                mainCamera.backgroundColor = Color.red;
            }
            
            yield return new WaitForSeconds(damageFlashDuration);
            
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.black;
            }
        }
        
        public void PlayHealFlash()
        {
            StartCoroutine(HealFlashCoroutine());
        }
        
        private System.Collections.IEnumerator HealFlashCoroutine()
        {
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.green;
            }
            
            yield return new WaitForSeconds(healFlashDuration);
            
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.black;
            }
        }
        
        public void PlayScreenShake(float duration, float intensity)
        {
            StartCoroutine(ScreenShakeCoroutine(duration, intensity));
        }
        
        private System.Collections.IEnumerator ScreenShakeCoroutine(float duration, float intensity)
        {
            if (mainCamera == null)
                yield break;
            
            Vector3 originalPosition = mainCamera.transform.position;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                intensity *= 0.95f;
                
                yield return null;
            }
            
            mainCamera.transform.position = originalPosition;
        }
        
        public void SpawnFloatingText(string text, Vector3 position, Color color)
        {
            GameObject floatingText = new GameObject("FloatingText");
            floatingText.transform.position = position;
            
            var textMesh = floatingText.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 24;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            
            StartCoroutine(FloatingTextCoroutine(floatingText));
        }
        
        private System.Collections.IEnumerator FloatingTextCoroutine(GameObject textObj)
        {
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;
            
            while (elapsed < duration)
            {
                textObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                
                TextMesh tm = textObj.GetComponent<TextMesh>();
                if (tm != null)
                {
                    tm.color = new Color(tm.color.r, tm.color.g, tm.color.b, 1f - (elapsed / duration));
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Destroy(textObj);
        }
        
        public void CreateTrailEffect(Transform target, Color trailColor, float duration = 1f)
        {
            StartCoroutine(TrailEffectCoroutine(target, trailColor, duration));
        }
        
        private System.Collections.IEnumerator TrailEffectCoroutine(Transform target, Color color, float duration)
        {
            float elapsed = 0f;
            Vector3 lastPosition = target.position;
            
            while (elapsed < duration)
            {
                if (target != null && target.position != lastPosition)
                {
                    GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    trail.transform.position = lastPosition;
                    trail.transform.localScale = Vector3.one * 0.2f;
                    
                    Renderer renderer = trail.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = color;
                    }
                    
                    Destroy(trail, 0.5f);
                    
                    lastPosition = target.position;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
