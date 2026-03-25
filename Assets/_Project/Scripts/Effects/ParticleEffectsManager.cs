using UnityEngine;
using System.Collections.Generic;

public class ParticleEffectsManager : MonoBehaviour
{
    public static ParticleEffectsManager Instance { get; private set; }

    [Header("Allomancy Effects")]
    public GameObject metalBurnParticles;
    public GameObject steelPushParticles;
    public GameObject ironPullParticles;
    public GameObject pewterBurstParticles;
    public GameObject tinSensoryParticles;
    public GameObject allomanticSightParticles;

    [Header("Metal Specific")]
    public GameObject zincRiotParticles;
    public GameObject brassSootheParticles;
    public GameObject copperCloudParticles;
    public GameObject bronzeDetectParticles;
    public GameObject atiumGhostParticles;
    public GameObject electrumPathParticles;
    public GameObject duraluminBurstParticles;
    public GameObject nicrosilBurstParticles;

    [Header("Combat Effects")]
    public GameObject hitEffectParticles;
    public GameObject criticalHitParticles;
    public GameObject deathEffectParticles;
    public GameObject explosionParticles;

    [Header("Environmental")]
    public GameObject ashParticles;
    public GameObject mistParticles;
    public GameObject smokeParticles;
    public GameObject dustParticles;
    public GameObject sparkParticles;

    [Header("Pool Settings")]
    public int poolSize = 20;

    private Dictionary<string, Queue<GameObject>> particlePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> activeParticles = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        InitializePools();
    }

    void InitializePools()
    {
        CreatePool("MetalBurn", metalBurnParticles);
        CreatePool("SteelPush", steelPushParticles);
        CreatePool("IronPull", ironPullParticles);
        CreatePool("PewterBurst", pewterBurstParticles);
        CreatePool("TinSensory", tinSensoryParticles);
        CreatePool("AllomanticSight", allomanticSightParticles);
        CreatePool("HitEffect", hitEffectParticles);
        CreatePool("CriticalHit", criticalHitParticles);
        CreatePool("DeathEffect", deathEffectParticles);
        CreatePool("Explosion", explosionParticles);
    }

    void CreatePool(string poolName, GameObject prefab)
    {
        if (prefab == null) return;

        Queue<GameObject> pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        particlePools[poolName] = pool;
    }

    GameObject GetFromPool(string poolName)
    {
        if (!particlePools.ContainsKey(poolName)) return null;

        Queue<GameObject> pool = particlePools[poolName];

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return null;
    }

    void ReturnToPool(string poolName, GameObject obj)
    {
        if (!particlePools.ContainsKey(poolName)) return;

        obj.SetActive(false);
        particlePools[poolName].Enqueue(obj);
    }

    public void PlayMetalBurnEffect(Vector3 position, AllomancySkill.MetalType metal, float intensity = 1f)
    {
        GameObject effect = GetFromPool("MetalBurn");
        if (effect == null) return;

        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * intensity;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = GetMetalColor(metal);
            main.startSize = 0.2f * intensity;
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("MetalBurn", effect, 2f));
    }

    Color GetMetalColor(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel: return Color.blue;
            case AllomancySkill.MetalType.Iron: return Color.cyan;
            case AllomancySkill.MetalType.Pewter: return Color.red;
            case AllomancySkill.MetalType.Tin: return Color.yellow;
            case AllomancySkill.MetalType.Zinc: return new Color(1, 0.5f, 0);
            case AllomancySkill.MetalType.Brass: return new Color(0, 1, 0.5f);
            case AllomancySkill.MetalType.Copper: return Color.green;
            case AllomancySkill.MetalType.Bronze: return Color.magenta;
            case AllomancySkill.MetalType.Atium: return Color.white;
            case AllomancySkill.MetalType.Gold: return Color.yellow;
            case AllomancySkill.MetalType.Electrum: return new Color(0.8f, 0.8f, 0.2f);
            case AllomancySkill.MetalType.Aluminum: return Color.gray;
            case AllomancySkill.MetalType.Duralumin: return new Color(0.5f, 0, 1);
            case AllomancySkill.MetalType.Bendalloy: return new Color(0, 0.5f, 1);
            case AllomancySkill.MetalType.Cadmium: return new Color(1, 0.5f, 0);
            default: return Color.white;
        }
    }

    public void PlaySteelPushEffect(Vector3 position, Vector3 direction, float intensity = 1f)
    {
        GameObject effect = GetFromPool("SteelPush");
        if (effect == null) return;

        effect.transform.position = position;
        effect.transform.forward = direction;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = 0.3f * intensity;
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("SteelPush", effect, 1f));
    }

    public void PlayIronPullEffect(Vector3 position, Vector3 direction, float intensity = 1f)
    {
        GameObject effect = GetFromPool("IronPull");
        if (effect == null) return;

        effect.transform.position = position;
        effect.transform.forward = direction;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = 0.3f * intensity;
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("IronPull", effect, 1f));
    }

    public void PlayHitEffect(Vector3 position, float damage = 1f)
    {
        string poolName = damage > 50 ? "CriticalHit" : "HitEffect";
        GameObject effect = GetFromPool(poolName);
        if (effect == null) return;

        effect.transform.position = position;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = 0.2f * (damage / 25f);
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay(poolName, effect, 0.5f));
    }

    public void PlayDeathEffect(Vector3 position)
    {
        GameObject effect = GetFromPool("DeathEffect");
        if (effect == null) return;

        effect.transform.position = position;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("DeathEffect", effect, 3f));
    }

    public void PlayExplosion(Vector3 position, float intensity = 1f)
    {
        GameObject effect = GetFromPool("Explosion");
        if (effect == null) return;

        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * intensity;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = 1f * intensity;
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("Explosion", effect, 2f));
    }

    public void PlayAllomanticSightEffect(Vector3 position, Vector3 targetPosition)
    {
        GameObject effect = GetFromPool("AllomanticSight");
        if (effect == null) return;

        effect.transform.position = position;
        effect.transform.LookAt(targetPosition);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = Color.blue;
            ps.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay("AllomanticSight", effect, 1f));
    }

    public void PlayCustomEffect(GameObject prefab, Vector3 position, float duration = 2f)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        Destroy(effect, duration);
    }

    System.Collections.IEnumerator ReturnToPoolAfterDelay(string poolName, GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(poolName, effect);
    }
}

public class ParticleEventReceiver : MonoBehaviour
{
    [Header("Settings")]
    public bool autoAttachParticles = true;
    public float particleScale = 1f;

    private ParticleSystem attachedParticles;
    private Allomancer allomancer;

    void Start()
    {
        if (autoAttachParticles)
        {
            attachedParticles = GetComponentInChildren<ParticleSystem>();
        }

        allomancer = GetComponentInParent<Allomancer>();
    }

    public void PlayParticlesForMetal(AllomancySkill.MetalType metal)
    {
        if (attachedParticles == null) return;

        var main = attachedParticles.main;
        main.startColor = GetMetalColor(metal);
        main.startSize = 0.2f * particleScale;
        attachedParticles.Play();
    }

    public void StopParticles()
    {
        if (attachedParticles != null)
        {
            attachedParticles.Stop();
        }
    }

    Color GetMetalColor(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel: return Color.blue;
            case AllomancySkill.MetalType.Iron: return Color.cyan;
            case AllomancySkill.MetalType.Pewter: return Color.red;
            case AllomancySkill.MetalType.Tin: return Color.yellow;
            case AllomancySkill.MetalType.Zinc: return new Color(1, 0.5f, 0);
            case AllomancySkill.MetalType.Brass: return new Color(0, 1, 0.5f);
            case AllomancySkill.MetalType.Copper: return Color.green;
            case AllomancySkill.MetalType.Bronze: return Color.magenta;
            case AllomancySkill.MetalType.Atium: return Color.white;
            case AllomancySkill.MetalType.Gold: return Color.yellow;
            case AllomancySkill.MetalType.Electrum: return new Color(0.8f, 0.8f, 0.2f);
            case AllomancySkill.MetalType.Aluminum: return Color.gray;
            case AllomancySkill.MetalType.Duralumin: return new Color(0.5f, 0, 1);
            case AllomancySkill.MetalType.Bendalloy: return new Color(0, 0.5f, 1);
            case AllomancySkill.MetalType.Cadmium: return new Color(1, 0.5f, 0);
            default: return Color.white;
        }
    }
}