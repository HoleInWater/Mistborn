using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class VisualEffects : MonoBehaviour
{
    [Header("Particle Effects")]
    public ParticleSystem dustParticles;
    public ParticleSystem impactParticles;
    public ParticleSystem trailParticles;
    
    [Header("Screen Effects")]
    public GameObject damageVignette;
    public GameObject healEffect;
    public GameObject metalBurnEffect;
    
    [Header("Lighting")]
    public Light playerLight;
    public float normalLightIntensity = 1f;
    public float metalBurnLightIntensity = 2f;
    
    [Header("Post Processing")]
    public Volume postProcessingVolume;
    public Color dodgeVignetteColor = Color.red;
    public float normalSaturation = 1f;
    
    private Dictionary<string, GameObject> effectPool = new Dictionary<string, GameObject>();
    
    void Start()
    {
        InitializeEffects();
    }
    
    void InitializeEffects()
    {
        if (damageVignette != null)
        {
            damageVignette.SetActive(false);
        }
        
        if (healEffect != null)
        {
            healEffect.SetActive(false);
        }
        
        if (playerLight != null)
        {
            playerLight.intensity = normalLightIntensity;
        }
    }
    
    public void PlayDustEffect(Vector3 position)
    {
        if (dustParticles != null)
        {
            dustParticles.transform.position = position;
            dustParticles.Play();
        }
    }
    
    public void PlayImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactParticles != null)
        {
            ParticleSystem particles = Instantiate(impactParticles, position, Quaternion.LookRotation(normal));
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constant);
        }
    }
    
    public void PlayTrailEffect(Vector3 start, Vector3 end)
    {
        if (trailParticles != null)
        {
            ParticleSystem particles = Instantiate(trailParticles, start, Quaternion.identity);
            particles.transform.LookAt(end);
            particles.Play();
        }
    }
    
    public void ShowDamageVignette(float duration)
    {
        if (damageVignette != null)
        {
            damageVignette.SetActive(true);
            Invoke(nameof(HideDamageVignette), duration);
        }
    }
    
    void HideDamageVignette()
    {
        if (damageVignette != null)
        {
            damageVignette.SetActive(false);
        }
    }
    
    public void ShowHealEffect()
    {
        if (healEffect != null)
        {
            healEffect.SetActive(true);
            Invoke(nameof(HideHealEffect), 2f);
        }
    }
    
    void HideHealEffect()
    {
        if (healEffect != null)
        {
            healEffect.SetActive(false);
        }
    }
    
    public void SetMetalBurnEffect(bool active)
    {
        if (metalBurnEffect != null)
        {
            metalBurnEffect.SetActive(active);
        }
        
        if (playerLight != null)
        {
            playerLight.intensity = active ? metalBurnLightIntensity : normalLightIntensity;
        }
    }
    
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }
    
    System.Collections.IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPosition = Camera.main.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            Camera.main.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            intensity *= 0.95f;
            
            yield return null;
        }
        
        Camera.main.transform.localPosition = originalPosition;
    }
    
    public void FlashScreen(Color color, float duration)
    {
        StartCoroutine(ScreenFlash(color, duration));
    }
    
    System.Collections.IEnumerator ScreenFlash(Color color, float duration)
    {
        GameObject flash = new GameObject("ScreenFlash");
        flash.transform.SetParent(Camera.main.transform);
        flash.transform.localPosition = Vector3.forward;
        
        SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
        renderer.color = new Color(color.r, color.g, color.b, 0.5f);
        renderer.sprite = Sprite.Create(
            Texture2D.whiteTexture, 
            new Rect(0, 0, 1, 1), 
            new Vector2(0.5f, 0.5f)
        );
        flash.transform.localScale = new Vector3(10, 10, 1);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            renderer.color = new Color(color.r, color.g, color.b, alpha * 0.5f);
            yield return null;
        }
        
        Destroy(flash);
    }
    
    public GameObject SpawnVFX(string vfxName, Vector3 position, Quaternion rotation, float lifetime = 5f)
    {
        GameObject vfx = null;
        
        if (effectPool.ContainsKey(vfxName))
        {
            vfx = Instantiate(effectPool[vfxName], position, rotation);
        }
        else
        {
            vfx = new GameObject(vfxName);
            vfx.transform.position = position;
            vfx.transform.rotation = rotation;
        }
        
        if (lifetime > 0)
        {
            Destroy(vfx, lifetime);
        }
        
        return vfx;
    }
    
    public void RegisterEffect(string name, GameObject prefab)
    {
        effectPool[name] = prefab;
    }
}

public class ParticleEffects : MonoBehaviour
{
    [Header("Metal Particles")]
    public GameObject steelParticlePrefab;
    public GameObject ironParticlePrefab;
    public GameObject pewterParticlePrefab;
    public GameObject atiumParticlePrefab;
    
    [Header("Settings")]
    public float particleLifetime = 2f;
    public int particlesPerSecond = 100;
    
    public void EmitMetalParticles(MetalType metal, Vector3 position, Vector3 direction)
    {
        GameObject prefab = GetParticlePrefab(metal);
        if (prefab == null) return;
        
        ParticleSystem particles = Instantiate(prefab, position, Quaternion.LookRotation(direction)).GetComponent<ParticleSystem>();
        particles.Emit(particlesPerSecond);
        Destroy(particles.gameObject, particleLifetime);
    }
    
    GameObject GetParticlePrefab(MetalType metal)
    {
        switch (metal)
        {
            case MetalType.Steel: return steelParticlePrefab;
            case MetalType.Iron: return ironParticlePrefab;
            case MetalType.Pewter: return pewterParticlePrefab;
            case MetalType.Atium: return atiumParticlePrefab;
            default: return null;
        }
    }
    
    public void CreatePewterEnhancementEffect(Transform target)
    {
        GameObject effect = new GameObject("PewterEnhancement");
        effect.transform.SetParent(target);
        effect.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.8f, 0.7f, 0.6f);
        main.startSize = 0.1f;
        main.startSpeed = 1f;
        main.startLifetime = 1f;
        
        var emission = ps.emission;
        emission.rateOverTime = 50f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        
        ps.Play();
        
        Destroy(effect, 3f);
    }
    
    public void CreateAtiumFutureEffect(Transform target)
    {
        GameObject effect = new GameObject("AtiumEffect");
        effect.transform.SetParent(target);
        effect.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.8f, 0.2f);
        main.startSize = 0.05f;
        main.startSpeed = 0.5f;
        main.startLifetime = 2f;
        
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        
        ps.Play();
        
        Destroy(effect, 4f);
    }
}

public class LightingEffects : MonoBehaviour
{
    [Header("Light Sources")]
    public Light directionalLight;
    public Light ambientLight;
    
    [Header("Metal Burning Lights")]
    public Color steelColor = Color.cyan;
    public Color ironColor = Color.blue;
    public Color pewterColor = Color.yellow;
    public Color atiumColor = new Color(1f, 0.8f, 0.2f);
    
    [Header("Intensity")]
    public float normalIntensity = 1f;
    public float enhancedIntensity = 2f;
    
    public void SetMetalLight(MetalType metal, bool active)
    {
        if (playerLight == null) return;
        
        if (active)
        {
            playerLight.color = GetMetalColor(metal);
            playerLight.intensity = enhancedIntensity;
        }
        else
        {
            playerLight.intensity = normalIntensity;
        }
    }
    
    Color GetMetalColor(MetalType metal)
    {
        switch (metal)
        {
            case MetalType.Steel: return steelColor;
            case MetalType.Iron: return ironColor;
            case MetalType.Pewter: return pewterColor;
            case MetalType.Atium: return atiumColor;
            default: return Color.white;
        }
    }
    
    private Light playerLight;
    
    void Start()
    {
        playerLight = GetComponent<Light>();
    }
}
