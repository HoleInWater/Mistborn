using UnityEngine;
using System.Collections.Generic;

public class MinimapSystem : MonoBehaviour
{
    public static MinimapSystem Instance { get; private set; }

    [Header("Minimap Settings")]
    public Camera minimapCamera;
    public RenderTexture minimapTexture;
    public float mapSize = 200f;
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;
    public bool rotateWithPlayer = true;
    public float zoomLevel = 1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;

    [Header("References")]
    public Transform player;
    public UnityEngine.UI.RawImage minimapDisplay;

    [Header("Markers")]
    public GameObject markerPrefab;
    public float markerUpdateRate = 0.1f;

    private Dictionary<string, MinimapMarker> markers = new Dictionary<string, MinimapMarker>();
    private float lastMarkerUpdate;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        if (minimapCamera == null) minimapCamera = GetComponentInChildren<Camera>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        SetupMinimapCamera();
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null) return;

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = mapSize / 2f / zoomLevel;
        minimapCamera.nearClipPlane = 1f;
        minimapCamera.farClipPlane = 500f;
        minimapCamera.depth = 1;

        if (minimapTexture != null)
        {
            minimapCamera.targetTexture = minimapTexture;
        }
    }

    void LateUpdate()
    {
        if (minimapCamera == null || player == null) return;

        UpdateCameraPosition();
        UpdateMarkers();
    }

    void UpdateCameraPosition()
    {
        Vector3 targetPos = player.position;
        targetPos.y = player.position.y + 100f;

        minimapCamera.transform.position = Vector3.Lerp(
            minimapCamera.transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );

        if (rotateWithPlayer)
        {
            Quaternion targetRot = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            minimapCamera.transform.rotation = Quaternion.Slerp(
                minimapCamera.transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        minimapCamera.orthographicSize = mapSize / 2f / zoomLevel;
    }

    void UpdateMarkers()
    {
        if (Time.time - lastMarkerUpdate < markerUpdateRate) return;
        lastMarkerUpdate = Time.time;

        foreach (var marker in markers.Values)
        {
            if (marker != null && marker.target != null)
            {
                marker.UpdatePosition();
            }
        }
    }

    public void AddMarker(string markerId, Transform target, Color color, string label = "")
    {
        if (markers.ContainsKey(markerId)) return;

        if (markerPrefab == null) return;

        GameObject markerObj = Instantiate(markerPrefab, minimapCamera.transform);
        MinimapMarker marker = markerObj.GetComponent<MinimapMarker>();

        if (marker != null)
        {
            marker.Initialize(target, color, label);
            markers[markerId] = marker;
        }
    }

    public void RemoveMarker(string markerId)
    {
        if (!markers.ContainsKey(markerId)) return;

        MinimapMarker marker = markers[markerId];
        markers.Remove(markerId);

        if (marker != null) Destroy(marker.gameObject);
    }

    public void UpdateMarkerColor(string markerId, Color color)
    {
        if (!markers.ContainsKey(markerId)) return;
        markers[markerId].SetColor(color);
    }

    public void SetZoom(float zoom)
    {
        zoomLevel = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void ZoomIn()
    {
        SetZoom(zoomLevel + 0.25f);
    }

    public void ZoomOut()
    {
        SetZoom(zoomLevel - 0.25f);
    }

    public void ToggleRotation()
    {
        rotateWithPlayer = !rotateWithPlayer;
    }

    public void AddQuestMarker(Transform target, string questName)
    {
        AddMarker($"Quest_{questName}", target, Color.yellow, questName);
    }

    public void AddEnemyMarker(Transform target)
    {
        AddMarker($"Enemy_{target.GetInstanceID()}", target, Color.red);
    }

    public void AddObjectiveMarker(Transform target, string objectiveName)
    {
        AddMarker($"Objective_{objectiveName}", target, Color.green, objectiveName);
    }

    public void ClearAllMarkers()
    {
        foreach (var marker in markers.Values)
        {
            if (marker != null) Destroy(marker.gameObject);
        }
        markers.Clear();
    }
}

public class MinimapMarker : MonoBehaviour
{
    [Header("Settings")]
    public SpriteRenderer icon;
    public TMPro.TextMeshProUGUI label;
    public bool showLabel = false;
    public float labelOffset = 0.5f;

    public Transform target { get; private set; }
    private Color markerColor = Color.white;

    public void Initialize(Transform targetTransform, Color color, string markerLabel = "")
    {
        target = targetTransform;
        markerColor = color;

        if (icon != null) icon.color = color;

        if (label != null)
        {
            label.text = markerLabel;
            label.gameObject.SetActive(!string.IsNullOrEmpty(markerLabel));
        }
    }

    public void UpdatePosition()
    {
        if (target == null) return;

        transform.position = new Vector3(target.position.x, 200f, target.position.z);
    }

    public void SetColor(Color color)
    {
        markerColor = color;
        if (icon != null) icon.color = color;
    }

    public void SetLabel(string text)
    {
        if (label != null)
        {
            label.text = text;
            label.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}

public class MinimapMarkerPoint : MonoBehaviour
{
    [Header("Marker Settings")]
    public MarkerType markerType = MarkerType.Location;
    public string markerId = "";
    public Color markerColor = Color.white;
    public bool isDiscovered = false;
    public float discoveryRadius = 10f;

    public enum MarkerType { Location, Quest, Enemy, Objective, NPC, Item }

    private bool hasBeenDiscovered = false;

    void Start()
    {
        if (string.IsNullOrEmpty(markerId))
        {
            markerId = $"{gameObject.name}_{GetInstanceID()}";
        }

        if (isDiscovered)
        {
            DiscoverMarker();
        }
    }

    void Update()
    {
        if (!hasBeenDiscovered)
        {
            CheckDiscovery();
        }
    }

    void CheckDiscovery()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= discoveryRadius)
        {
            DiscoverMarker();
        }
    }

    public void DiscoverMarker()
    {
        if (hasBeenDiscovered) return;

        hasBeenDiscovered = true;
        isDiscovered = true;

        if (MinimapSystem.Instance != null)
        {
            MinimapSystem.Instance.AddMarker(markerId, transform, markerColor, markerType.ToString());
        }

        Debug.Log($"[MINIMAP] Discovered marker: {markerId}");
    }

    public void RemoveMarker()
    {
        if (MinimapSystem.Instance != null)
        {
            MinimapSystem.Instance.RemoveMarker(markerId);
        }
    }

    public void UpdateColor(Color newColor)
    {
        markerColor = newColor;
        if (MinimapSystem.Instance != null)
        {
            MinimapSystem.Instance.UpdateMarkerColor(markerId, newColor);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = markerColor;
        Gizmos.DrawWireSphere(transform.position, discoveryRadius);
    }
}

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject waypointPrefab;
    public float waypointShowDistance = 100f;
    public float waypointHideDistance = 10f;

    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    private Dictionary<string, Waypoint> activeWaypoints = new Dictionary<string, Waypoint>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        UpdateWaypoints();
    }

    void UpdateWaypoints()
    {
        foreach (var waypoint in activeWaypoints.Values)
        {
            if (waypoint == null) continue;

            float distance = Vector3.Distance(player.position, waypoint.target.position);
            waypoint.UpdateDisplay(distance, waypointShowDistance, waypointHideDistance);
        }
    }

    public void SetWaypoint(string waypointId, Transform target, Color color, string label = "")
    {
        if (activeWaypoints.ContainsKey(waypointId))
        {
            activeWaypoints[waypointId].target = target;
            activeWaypoints[waypointId].label = label;
            return;
        }

        if (waypointPrefab == null) return;

        GameObject waypointObj = Instantiate(waypointPrefab, transform);
        Waypoint waypoint = waypointObj.GetComponent<Waypoint>();

        if (waypoint != null)
        {
            waypoint.Initialize(target, color, label);
            activeWaypoints[waypointId] = waypoint;
        }
    }

    public void RemoveWaypoint(string waypointId)
    {
        if (!activeWaypoints.ContainsKey(waypointId)) return;

        Waypoint waypoint = activeWaypoints[waypointId];
        activeWaypoints.Remove(waypointId);

        if (waypoint != null) Destroy(waypoint.gameObject);
    }

    public void ClearAllWaypoints()
    {
        foreach (var waypoint in activeWaypoints.Values)
        {
            if (waypoint != null) Destroy(waypoint.gameObject);
        }
        activeWaypoints.Clear();
    }

    public void SetQuestWaypoint(Transform target, string questName)
    {
        SetWaypoint($"Quest_{questName}", target, Color.yellow, questName);
    }
}

public class Waypoint : MonoBehaviour
{
    [Header("UI")]
    public UnityEngine.UI.Image icon;
    public TMPro.TextMeshProUGUI distanceLabel;
    public TMPro.TextMeshProUGUI nameLabel;
    public UnityEngine.UI.Image arrow;

    public Transform target { get; set; }
    public string label { get; set; }

    private Color waypointColor;

    public void Initialize(Transform targetTransform, Color color, string waypointLabel)
    {
        target = targetTransform;
        label = waypointLabel;
        waypointColor = color;

        if (icon != null) icon.color = color;
        if (nameLabel != null) nameLabel.text = waypointLabel;
    }

    public void UpdateDisplay(float distance, float showDistance, float hideDistance)
    {
        bool shouldShow = distance <= showDistance && distance >= hideDistance;
        gameObject.SetActive(shouldShow);

        if (distanceLabel != null)
        {
            distanceLabel.text = $"{distance:F0}m";
        }

        if (arrow != null)
        {
            Vector3 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, -angle);
        }
    }
}