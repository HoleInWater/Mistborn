using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Object-pooled particle effect manager for all Allomancy, combat, and environmental effects.
/// </summary>
public class ParticleEffectsManager : MonoBehaviour
{
    public static ParticleEffectsManager Instance { get; private set; }

    [Header("Allomancy Effects")]
    public GameObject metalBurnParticles;
    public GameObject steelPushParticles;
    public GameObject ironPullParticles;
    public GameObject pewterBurstParticles;
    public GameObject tinSensoryParticles;
    public GameObject atiumGhostParticles;
    public GameObject duraluminBurstParticles;

    [Header("Combat Effects")]
    public GameObject hitEffectParticles;
    public GameObject criticalHitParticles;
    public GameObject deathEffectParticles;
    public GameObject explosionParticles;

    [Header("Environmental")]
    public GameObject ashParticles;
    public GameObject mistParticles;
    public GameObject sparkParticles;

    [Header("Pool Settings")]
    public int poolSize = 20;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CreatePool("MetalBurn", metalBurnParticles);
        CreatePool("SteelPush", steelPushParticles);
        CreatePool("IronPull", ironPullParticles);
        CreatePool("PewterBurst", pewterBurstParticles);
        CreatePool("HitEffect", hitEffectParticles);
        CreatePool("CriticalHit", criticalHitParticles);
        CreatePool("DeathEffect", deathEffectParticles);
        CreatePool("Explosion", explosionParticles);
    }

    void CreatePool(string name, GameObject prefab)
    {
        if (prefab == null) return;
        Queue<GameObject> pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        pools[name] = pool;
    }

    GameObject GetFromPool(string name)
    {
        if (!pools.ContainsKey(name) || pools[name].Count == 0) return null;
        GameObject obj = pools[name].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    void ReturnToPool(string name, GameObject obj)
    {
        if (!pools.ContainsKey(name)) return;
        obj.SetActive(false);
        pools[name].Enqueue(obj);
    }

    public void PlayMetalBurnEffect(Vector3 position, AllomancySkill.MetalType metal, float intensity = 1f)
    {
        GameObject effect = GetFromPool("MetalBurn");
        if (effect == null) return;
        effect.transform.position = position;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = GetMetalColor(metal);
            main.startSize = 0.2f * intensity;
            ps.Play();
        }
        StartCoroutine(ReturnAfterDelay("MetalBurn", effect, 2f));
    }

    public void PlaySteelPushEffect(Vector3 position, Vector3 direction, float intensity = 1f)
    {
        GameObject effect = GetFromPool("SteelPush");
        if (effect == null) return;
        effect.transform.position = position;
        effect.transform.forward = direction;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) { var m = ps.main; m.startSize = 0.3f * intensity; ps.Play(); }
        StartCoroutine(ReturnAfterDelay("SteelPush", effect, 1f));
    }

    public void PlayIronPullEffect(Vector3 position, Vector3 direction, float intensity = 1f)
    {
        GameObject effect = GetFromPool("IronPull");
        if (effect == null) return;
        effect.transform.position = position;
        effect.transform.forward = direction;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) { var m = ps.main; m.startSize = 0.3f * intensity; ps.Play(); }
        StartCoroutine(ReturnAfterDelay("IronPull", effect, 1f));
    }

    public void PlayHitEffect(Vector3 position, float damage = 25f)
    {
        string pool = damage > 50 ? "CriticalHit" : "HitEffect";
        GameObject effect = GetFromPool(pool);
        if (effect == null) return;
        effect.transform.position = position;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
        StartCoroutine(ReturnAfterDelay(pool, effect, 0.5f));
    }

    public void PlayDeathEffect(Vector3 position)
    {
        GameObject effect = GetFromPool("DeathEffect");
        if (effect == null) return;
        effect.transform.position = position;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
        StartCoroutine(ReturnAfterDelay("DeathEffect", effect, 3f));
    }

    public void PlayExplosion(Vector3 position, float intensity = 1f)
    {
        GameObject effect = GetFromPool("Explosion");
        if (effect == null) return;
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * intensity;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
        StartCoroutine(ReturnAfterDelay("Explosion", effect, 2f));
    }

    IEnumerator ReturnAfterDelay(string pool, GameObject effect, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ReturnToPool(pool, effect);
    }

    Color GetMetalColor(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:     return new Color(0.3f,  0.5f,  1f);
            case AllomancySkill.MetalType.Iron:      return new Color(0.2f,  0.8f,  1f);
            case AllomancySkill.MetalType.Pewter:    return new Color(0.8f,  0.2f,  0.2f);
            case AllomancySkill.MetalType.Tin:       return new Color(1f,    1f,    0.5f);
            case AllomancySkill.MetalType.Zinc:      return new Color(1f,    0.5f,  0f);
            case AllomancySkill.MetalType.Brass:     return new Color(0.2f,  0.9f,  0.5f);
            case AllomancySkill.MetalType.Copper:    return new Color(0.2f,  0.8f,  0.2f);
            case AllomancySkill.MetalType.Bronze:    return new Color(0.8f,  0.3f,  0.8f);
            case AllomancySkill.MetalType.Atium:     return new Color(0.9f,  0.9f,  1f);
            case AllomancySkill.MetalType.Malatium:  return new Color(0.50f, 0.28f, 0.62f);
            case AllomancySkill.MetalType.Gold:      return new Color(1f,    0.85f, 0.2f);
            case AllomancySkill.MetalType.Electrum:  return new Color(0.90f, 0.90f, 0.40f);
            case AllomancySkill.MetalType.Aluminum:  return new Color(0.90f, 0.92f, 0.96f);
            case AllomancySkill.MetalType.Duralumin: return new Color(0.45f, 0.75f, 1.00f);
            case AllomancySkill.MetalType.Bendalloy: return new Color(0.90f, 0.58f, 0.18f);
            case AllomancySkill.MetalType.Cadmium:   return new Color(0.20f, 0.72f, 0.70f);
            default:                                  return Color.white;
        }
    }
}
