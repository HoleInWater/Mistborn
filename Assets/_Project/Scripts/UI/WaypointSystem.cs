using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// World-space waypoint markers for quest objectives, shops, checkpoints, and custom markers.
/// Markers appear as icons on screen edges when off-screen.
/// </summary>
public class WaypointSystem : MonoBehaviour
{
    public static WaypointSystem Instance { get; private set; }

    [System.Serializable]
    public class Waypoint
    {
        public string id;
        public string label;
        public Vector3 worldPosition;
        public Transform trackTarget; // If set, follows this transform
        public WaypointType type;
        public Color color = Color.white;
        public bool active = true;
        public GameObject uiInstance;
    }

    public enum WaypointType { Quest, Shop, Checkpoint, Ally, Custom, Enemy }

    [Header("UI")]
    public Canvas hudCanvas;
    public GameObject waypointPrefab;

    [Header("Settings")]
    public float maxRenderDistance = 200f;
    public float minScreenEdgeMargin = 30f;
    public float distanceFadeStart = 150f;
    public bool showDistance = true;

    [Header("Colors")]
    public Color questColor = Color.yellow;
    public Color shopColor = Color.green;
    public Color checkpointColor = Color.cyan;
    public Color allyColor = new Color(0.3f, 0.5f, 1f);
    public Color enemyColor = Color.red;

    private List<Waypoint> waypoints = new List<Waypoint>();
    private Camera mainCam;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) { mainCam = Camera.main; return; }

        foreach (var wp in waypoints)
        {
            if (!wp.active || wp.uiInstance == null) continue;

            Vector3 worldPos = wp.trackTarget != null ? wp.trackTarget.position : wp.worldPosition;
            float dist = Vector3.Distance(mainCam.transform.position, worldPos);

            if (dist > maxRenderDistance)
            {
                wp.uiInstance.SetActive(false);
                continue;
            }

            wp.uiInstance.SetActive(true);

            // Screen position
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            bool isBehind = screenPos.z < 0;

            if (isBehind)
            {
                screenPos *= -1;
            }

            // Clamp to screen edges
            screenPos.x = Mathf.Clamp(screenPos.x, minScreenEdgeMargin, Screen.width - minScreenEdgeMargin);
            screenPos.y = Mathf.Clamp(screenPos.y, minScreenEdgeMargin, Screen.height - minScreenEdgeMargin);

            wp.uiInstance.transform.position = screenPos;

            // Distance text
            if (showDistance)
            {
                Text text = wp.uiInstance.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = dist < 10f ? wp.label : $"{wp.label}\n{dist:F0}m";
            }

            // Fade with distance
            CanvasGroup cg = wp.uiInstance.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float fadeRange = Mathf.Max(maxRenderDistance - distanceFadeStart, 0.01f);
                float fade = dist > distanceFadeStart ? 1f - ((dist - distanceFadeStart) / fadeRange) : 1f;
                cg.alpha = fade;
            }
        }
    }

    // ── Public API ───────────────────────────────────────────────────────

    public Waypoint AddWaypoint(string id, string label, Vector3 position, WaypointType type)
    {
        RemoveWaypoint(id); // Prevent duplicates

        Waypoint wp = new Waypoint
        {
            id = id,
            label = label,
            worldPosition = position,
            type = type,
            color = GetColorForType(type),
            active = true
        };

        if (waypointPrefab != null && hudCanvas != null)
        {
            wp.uiInstance = Instantiate(waypointPrefab, hudCanvas.transform);
            Image img = wp.uiInstance.GetComponent<Image>();
            if (img != null) img.color = wp.color;

            if (wp.uiInstance.GetComponent<CanvasGroup>() == null)
                wp.uiInstance.AddComponent<CanvasGroup>();
        }

        waypoints.Add(wp);
        return wp;
    }

    public Waypoint AddWaypoint(string id, string label, Transform target, WaypointType type)
    {
        Waypoint wp = AddWaypoint(id, label, target.position, type);
        wp.trackTarget = target;
        return wp;
    }

    public void RemoveWaypoint(string id)
    {
        Waypoint wp = waypoints.Find(w => w.id == id);
        if (wp != null)
        {
            if (wp.uiInstance != null) Destroy(wp.uiInstance);
            waypoints.Remove(wp);
        }
    }

    public void SetWaypointActive(string id, bool active)
    {
        Waypoint wp = waypoints.Find(w => w.id == id);
        if (wp != null) wp.active = active;
    }

    public void ClearAllWaypoints()
    {
        foreach (var wp in waypoints)
            if (wp.uiInstance != null) Destroy(wp.uiInstance);
        waypoints.Clear();
    }

    Color GetColorForType(WaypointType type)
    {
        switch (type)
        {
            case WaypointType.Quest: return questColor;
            case WaypointType.Shop: return shopColor;
            case WaypointType.Checkpoint: return checkpointColor;
            case WaypointType.Ally: return allyColor;
            case WaypointType.Enemy: return enemyColor;
            default: return Color.white;
        }
    }

    public int GetActiveWaypointCount() => waypoints.FindAll(w => w.active).Count;
}
