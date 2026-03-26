using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic overlay for the minimap system. Shows metal sources when burning
/// Bronze (Seeker), enemy positions when burning Tin, and quest markers.
/// Includes fog of war that clears as the player explores.
/// </summary>
public class MinimapAllomanticOverlay : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Allomancer allomancer;
    public Camera minimapCamera;

    [Header("Bronze Detection (Seeker)")]
    public float bronzeDetectionRange = 50f;
    public float bronzePulseInterval = 1f;
    public GameObject metalMarkerPrefab;

    [Header("Tin Detection (Enhanced Hearing)")]
    public float tinEnemyDetectionRange = 40f;
    public GameObject enemyMarkerPrefab;

    [Header("Quest Markers")]
    public GameObject questMarkerPrefab;
    public GameObject checkpointMarkerPrefab;
    public Color activeQuestColor = Color.yellow;
    public Color completedQuestColor = Color.green;

    [Header("Fog of War")]
    public bool enableFogOfWar = true;
    public float fogClearRadius = 20f;
    public float fogResolution = 2f;
    public Texture2D fogTexture;
    public UnityEngine.UI.RawImage fogOverlayImage;

    [Header("Minimap Zoom")]
    public float zoomSpeed = 0.5f;
    public float minZoom = 30f;
    public float maxZoom = 100f;
    private float currentZoom = 60f;

    // Marker tracking
    private Dictionary<int, GameObject> metalMarkers = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> enemyMarkers = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> questMarkers = new Dictionary<string, GameObject>();
    private float bronzePulseTimer;

    // Fog of war data
    private bool[,] fogCleared;
    private int fogWidth;
    private int fogHeight;
    private Vector2 fogOrigin;
    private Color32[] fogPixels;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                allomancer = playerObj.GetComponent<Allomancer>();
            }
        }

        if (minimapCamera == null)
            minimapCamera = Camera.main; // Fallback — assign minimap camera in Inspector

        if (enableFogOfWar)
            InitializeFogOfWar();

        currentZoom = minimapCamera != null ? minimapCamera.orthographicSize : 60f;
    }

    void Update()
    {
        if (player == null || allomancer == null) return;

        // Bronze detection — reveal metal sources
        if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Bronze))
        {
            bronzePulseTimer -= Time.deltaTime;
            if (bronzePulseTimer <= 0f)
            {
                bronzePulseTimer = bronzePulseInterval;
                ScanForMetalSources();
            }
        }
        else
        {
            ClearMetalMarkers();
        }

        // Tin detection — reveal enemies (throttled to match bronze pulse)
        if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Tin))
        {
            // Reuse bronze timer to avoid a second expensive scan per frame
            if (bronzePulseTimer <= 0.05f)
                ScanForEnemies();
        }
        else
        {
            ClearEnemyMarkers();
        }

        // Quest markers (always visible)
        UpdateQuestMarkers();

        // Fog of war
        if (enableFogOfWar)
            UpdateFogOfWar();

        // Zoom control
        HandleZoom();
    }

    // ── Bronze Metal Detection ───────────────────────────────────────────

    void ScanForMetalSources()
    {
        LayerMask metalMask = LayerMask.GetMask("Metal");
        float flareMultiplier = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = bronzeDetectionRange * flareMultiplier;

        Collider[] metals = Physics.OverlapSphere(player.position, effectiveRange, metalMask);
        HashSet<int> foundIds = new HashSet<int>();

        foreach (var col in metals)
        {
            int id = col.gameObject.GetInstanceID();
            foundIds.Add(id);

            if (!metalMarkers.ContainsKey(id) && metalMarkerPrefab != null)
            {
                GameObject marker = Instantiate(metalMarkerPrefab, transform);
                metalMarkers[id] = marker;

                // Color based on metal type
                AllomanticTarget target = col.GetComponent<AllomanticTarget>();
                UnityEngine.UI.Image img = marker.GetComponent<UnityEngine.UI.Image>();
                if (img != null && target != null)
                    img.color = GetMetalMarkerColor(target.metalType);
            }

            // Update position
            if (metalMarkers.ContainsKey(id))
            {
                metalMarkers[id].transform.position = WorldToMinimapPosition(col.transform.position);
            }
        }

        // Remove markers for metals no longer in range
        List<int> toRemove = new List<int>();
        foreach (var kvp in metalMarkers)
        {
            if (!foundIds.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }
        foreach (int id in toRemove)
        {
            Destroy(metalMarkers[id]);
            metalMarkers.Remove(id);
        }
    }

    void ClearMetalMarkers()
    {
        foreach (var kvp in metalMarkers)
            if (kvp.Value != null) Destroy(kvp.Value);
        metalMarkers.Clear();
    }

    // ── Tin Enemy Detection ──────────────────────────────────────────────

    void ScanForEnemies()
    {
        float flareMultiplier = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = tinEnemyDetectionRange * flareMultiplier;

        Collider[] entities = Physics.OverlapSphere(player.position, effectiveRange);
        HashSet<int> foundIds = new HashSet<int>();

        foreach (var col in entities)
        {
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy == null) continue;

            int id = col.gameObject.GetInstanceID();
            foundIds.Add(id);

            if (!enemyMarkers.ContainsKey(id) && enemyMarkerPrefab != null)
            {
                GameObject marker = Instantiate(enemyMarkerPrefab, transform);
                enemyMarkers[id] = marker;

                UnityEngine.UI.Image img = marker.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = GetEnemyMarkerColor(enemy.enemyType);
            }

            if (enemyMarkers.ContainsKey(id))
                enemyMarkers[id].transform.position = WorldToMinimapPosition(col.transform.position);
        }

        List<int> toRemove = new List<int>();
        foreach (var kvp in enemyMarkers)
        {
            if (!foundIds.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }
        foreach (int id in toRemove)
        {
            Destroy(enemyMarkers[id]);
            enemyMarkers.Remove(id);
        }
    }

    void ClearEnemyMarkers()
    {
        foreach (var kvp in enemyMarkers)
            if (kvp.Value != null) Destroy(kvp.Value);
        enemyMarkers.Clear();
    }

    // ── Quest Markers ────────────────────────────────────────────────────

    void UpdateQuestMarkers()
    {
        if (QuestManager.Instance == null || questMarkerPrefab == null) return;

        // This is a simplified version — in production, quest objectives would have
        // world positions. For now, we show markers for active quest NPCs/locations.
    }

    // ── Fog of War ───────────────────────────────────────────────────────

    void InitializeFogOfWar()
    {
        fogWidth = 256;
        fogHeight = 256;
        fogOrigin = Vector2.zero;

        fogCleared = new bool[fogWidth, fogHeight];
        fogTexture = new Texture2D(fogWidth, fogHeight, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Bilinear;

        fogPixels = new Color32[fogWidth * fogHeight];
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = new Color32(0, 0, 0, 200); // Dark fog

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();

        if (fogOverlayImage != null)
            fogOverlayImage.texture = fogTexture;
    }

    void UpdateFogOfWar()
    {
        if (fogTexture == null || player == null) return;

        // Convert player world position to fog pixel coords
        Vector2 playerFogPos = WorldToFogCoord(player.position);
        int clearRadiusPixels = Mathf.RoundToInt(fogClearRadius / fogResolution);

        bool changed = false;
        int px = Mathf.RoundToInt(playerFogPos.x);
        int py = Mathf.RoundToInt(playerFogPos.y);

        for (int x = -clearRadiusPixels; x <= clearRadiusPixels; x++)
        {
            for (int y = -clearRadiusPixels; y <= clearRadiusPixels; y++)
            {
                if (x * x + y * y > clearRadiusPixels * clearRadiusPixels) continue;

                int fx = px + x;
                int fy = py + y;
                if (fx < 0 || fx >= fogWidth || fy < 0 || fy >= fogHeight) continue;

                if (!fogCleared[fx, fy])
                {
                    fogCleared[fx, fy] = true;
                    fogPixels[fy * fogWidth + fx] = new Color32(0, 0, 0, 0); // Clear
                    changed = true;
                }
            }
        }

        if (changed)
        {
            fogTexture.SetPixels32(fogPixels);
            fogTexture.Apply();
        }
    }

    Vector2 WorldToFogCoord(Vector3 worldPos)
    {
        float mapWorldSize = fogWidth * fogResolution;
        float x = (worldPos.x - fogOrigin.x + mapWorldSize * 0.5f) / mapWorldSize * fogWidth;
        float y = (worldPos.z - fogOrigin.y + mapWorldSize * 0.5f) / mapWorldSize * fogHeight;
        return new Vector2(x, y);
    }

    // ── Zoom ─────────────────────────────────────────────────────────────

    void HandleZoom()
    {
        if (minimapCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
<<<<<<< HEAD
        if (Input.GetKey(KeyCode.Tab) && Mathf.Abs(scroll) > 0.01f)
=======
        if (Input.GetKey(Keybinds.MetalWheel) && Mathf.Abs(scroll) > 0.01f)
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        {
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed * 100f, minZoom, maxZoom);
            minimapCamera.orthographicSize = currentZoom;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    Vector3 WorldToMinimapPosition(Vector3 worldPos)
    {
        // Convert world position to minimap UI position (simplified)
        if (player == null) return Vector3.zero;
        Vector3 offset = worldPos - player.position;
        return transform.position + new Vector3(offset.x, offset.z, 0) * 0.1f;
    }

    Color GetMetalMarkerColor(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:    return new Color(0.3f, 0.5f, 1f);
            case AllomancySkill.MetalType.Iron:     return new Color(0.2f, 0.8f, 1f);
            case AllomancySkill.MetalType.Pewter:   return new Color(0.8f, 0.2f, 0.2f);
            case AllomancySkill.MetalType.Tin:      return new Color(1f, 1f, 0.5f);
            case AllomancySkill.MetalType.Atium:    return new Color(0.9f, 0.9f, 1f);
            default:                                 return Color.white;
        }
    }

    Color GetEnemyMarkerColor(EnemyAI.EnemyType type)
    {
        switch (type)
        {
            case EnemyAI.EnemyType.Guard:           return Color.red;
            case EnemyAI.EnemyType.Koloss:          return new Color(0.5f, 0, 0);
            case EnemyAI.EnemyType.SteelInquisitor: return new Color(0.8f, 0, 0.8f);
            default:                                 return new Color(1f, 0.3f, 0.3f);
        }
    }

    public float GetCurrentZoom() => currentZoom;
}
