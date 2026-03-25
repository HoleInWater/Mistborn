using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central performance manager handling object pooling, distance culling,
/// LOD management, and update frequency throttling for the Mistborn game.
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance { get; private set; }

    // ── Object Pool ──────────────────────────────────────────────────────
    [Header("Object Pooling")]
    public int defaultPoolSize = 20;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();

    // ── Distance Culling ─────────────────────────────────────────────────
    [Header("Distance Culling")]
    public float nearDistance = 30f;
    public float mediumDistance = 60f;
    public float farDistance = 100f;
    public float cullDistance = 150f;
    public float cullingUpdateInterval = 0.5f;

    private Transform playerTransform;
    private float cullingTimer;
    private List<CullableObject> cullableObjects = new List<CullableObject>();

    // ── LOD Management ───────────────────────────────────────────────────
    [Header("LOD")]
    public float lodBias = 1f;
    public int maxActiveParticles = 200;

    // ── Update Throttling ────────────────────────────────────────────────
    [Header("Throttling")]
    public int maxPhysicsQueries = 50;
    public int maxAIUpdatesPerFrame = 10;

    private int aiUpdateIndex = 0;
    private List<EnemyAI> trackedEnemies = new List<EnemyAI>();

    // ── Stats ────────────────────────────────────────────────────────────
    [Header("Debug")]
    public bool showPerformanceStats = false;
    private int poolHits = 0;
    private int poolMisses = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        QualitySettings.lodBias = lodBias;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // Periodic distance culling
        cullingTimer += Time.deltaTime;
        if (cullingTimer >= cullingUpdateInterval)
        {
            cullingTimer = 0f;
            UpdateDistanceCulling();
        }

        // Throttled AI updates
        UpdateThrottledAI();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // OBJECT POOLING
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pre-warm a pool with a specific number of instances.
    /// </summary>
    public void PrewarmPool(string poolId, GameObject prefab, int count = -1)
    {
        if (prefab == null) return;
        if (count < 0) count = defaultPoolSize;

        if (!pools.ContainsKey(poolId))
        {
            pools[poolId] = new Queue<GameObject>();

            GameObject parent = new GameObject($"Pool_{poolId}");
            parent.transform.SetParent(transform);
            poolParents[poolId] = parent.transform;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, poolParents[poolId]);
            obj.SetActive(false);
            pools[poolId].Enqueue(obj);
        }
    }

    /// <summary>
    /// Get an object from a pool. Returns null if pool is empty and no prefab is available.
    /// </summary>
    public GameObject GetFromPool(string poolId, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolId) || pools[poolId].Count == 0)
        {
            poolMisses++;
            return null;
        }

        poolHits++;
        GameObject obj = pools[poolId].Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Return an object to its pool.
    /// </summary>
    public void ReturnToPool(string poolId, GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);

        if (!pools.ContainsKey(poolId))
        {
            pools[poolId] = new Queue<GameObject>();
            GameObject parent = new GameObject($"Pool_{poolId}");
            parent.transform.SetParent(transform);
            poolParents[poolId] = parent.transform;
        }

        obj.transform.SetParent(poolParents.ContainsKey(poolId) ? poolParents[poolId] : transform);
        pools[poolId].Enqueue(obj);
    }

    /// <summary>
    /// Return to pool after a delay.
    /// </summary>
    public void ReturnToPoolDelayed(string poolId, GameObject obj, float delay)
    {
        StartCoroutine(ReturnAfterDelay(poolId, obj, delay));
    }

    System.Collections.IEnumerator ReturnAfterDelay(string poolId, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(poolId, obj);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DISTANCE CULLING
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Register an object for distance-based culling.
    /// </summary>
    public void RegisterCullable(CullableObject cullable)
    {
        if (!cullableObjects.Contains(cullable))
            cullableObjects.Add(cullable);
    }

    /// <summary>
    /// Unregister a cullable object.
    /// </summary>
    public void UnregisterCullable(CullableObject cullable)
    {
        cullableObjects.Remove(cullable);
    }

    void UpdateDistanceCulling()
    {
        if (playerTransform == null) return;

        // Clean up destroyed objects
        cullableObjects.RemoveAll(c => c == null);

        Vector3 playerPos = playerTransform.position;

        foreach (var cullable in cullableObjects)
        {
            float dist = Vector3.Distance(playerPos, cullable.transform.position);

            if (dist > cullDistance)
            {
                cullable.SetLODLevel(CullableObject.LODLevel.Culled);
            }
            else if (dist > farDistance)
            {
                cullable.SetLODLevel(CullableObject.LODLevel.Far);
            }
            else if (dist > mediumDistance)
            {
                cullable.SetLODLevel(CullableObject.LODLevel.Medium);
            }
            else
            {
                cullable.SetLODLevel(CullableObject.LODLevel.Near);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AI THROTTLING
    // ═══════════════════════════════════════════════════════════════════════

    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!trackedEnemies.Contains(enemy))
            trackedEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyAI enemy)
    {
        trackedEnemies.Remove(enemy);
    }

    void UpdateThrottledAI()
    {
        trackedEnemies.RemoveAll(e => e == null);
        if (trackedEnemies.Count == 0) return;

        int updatesThisFrame = Mathf.Min(maxAIUpdatesPerFrame, trackedEnemies.Count);

        for (int i = 0; i < updatesThisFrame; i++)
        {
            int idx = (aiUpdateIndex + i) % trackedEnemies.Count;
            // Enemies far from player get fewer updates
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(playerTransform.position, trackedEnemies[idx].transform.position);
                if (dist > farDistance && i > updatesThisFrame / 2)
                    continue;
            }
        }

        aiUpdateIndex = (aiUpdateIndex + updatesThisFrame) % Mathf.Max(1, trackedEnemies.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // STATS
    // ═══════════════════════════════════════════════════════════════════════

    void OnGUI()
    {
        if (!showPerformanceStats) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 12;

        int y = 10;
        GUI.Label(new Rect(Screen.width - 220, y, 210, 20),
            $"FPS: {(1f / Time.unscaledDeltaTime):F0}", style);
        y += 18;
        GUI.Label(new Rect(Screen.width - 220, y, 210, 20),
            $"Pool Hits/Misses: {poolHits}/{poolMisses}", style);
        y += 18;
        GUI.Label(new Rect(Screen.width - 220, y, 210, 20),
            $"Cullable Objects: {cullableObjects.Count}", style);
        y += 18;
        GUI.Label(new Rect(Screen.width - 220, y, 210, 20),
            $"Tracked Enemies: {trackedEnemies.Count}", style);
    }
}

/// <summary>
/// Attach to objects that should be distance-culled by PerformanceManager.
/// </summary>
public class CullableObject : MonoBehaviour
{
    public enum LODLevel { Near, Medium, Far, Culled }

    [Header("Culling")]
    public bool disableRendererOnCull = true;
    public bool disableColliderOnCull = true;
    public bool disableAIOnCull = true;

    private LODLevel currentLevel = LODLevel.Near;
    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        PerformanceManager.Instance?.RegisterCullable(this);
    }

    void OnDestroy()
    {
        PerformanceManager.Instance?.UnregisterCullable(this);
    }

    public void SetLODLevel(LODLevel level)
    {
        if (level == currentLevel) return;
        currentLevel = level;

        switch (level)
        {
            case LODLevel.Near:
                SetRenderersEnabled(true);
                SetCollidersEnabled(true);
                break;

            case LODLevel.Medium:
                SetRenderersEnabled(true);
                SetCollidersEnabled(true);
                // Could reduce shadow quality here
                break;

            case LODLevel.Far:
                SetRenderersEnabled(true);
                SetCollidersEnabled(false);
                break;

            case LODLevel.Culled:
                if (disableRendererOnCull) SetRenderersEnabled(false);
                if (disableColliderOnCull) SetCollidersEnabled(false);
                break;
        }
    }

    void SetRenderersEnabled(bool enabled)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = enabled;
        }
    }

    void SetCollidersEnabled(bool enabled)
    {
        foreach (var c in colliders)
        {
            if (c != null) c.enabled = enabled;
        }
    }

    public LODLevel GetCurrentLevel() => currentLevel;
}
