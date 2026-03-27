using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Minimap system with camera, player indicator, and enemy markers.
/// Uses a top-down orthographic camera rendering to a RenderTexture.
/// </summary>
public class MinimapSystem : MonoBehaviour
{
    public static MinimapSystem Instance { get; private set; }

    [Header("Camera")]
    public Camera minimapCamera;
    public float mapSize = 100f;
    public float followSpeed = 10f;
    public bool rotateWithPlayer = true;
    public float minZoom = 30f;
    public float maxZoom = 150f;

    [Header("UI")]
    public RawImage minimapDisplay;
    public RenderTexture minimapTexture;

    [Header("Markers")]
    public Image playerMarker;
    public Color enemyMarkerColor = Color.red;
    public Color questMarkerColor = Color.yellow;

    private Transform player;
    private float currentZoom;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        currentZoom = mapSize;

        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform);
            minimapCamera = camObj.AddComponent<Camera>();
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = currentZoom;
            minimapCamera.nearClipPlane = 1f;
            minimapCamera.farClipPlane = 500f;
            minimapCamera.depth = -1;
            minimapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));

            if (minimapTexture == null)
            {
                minimapTexture = new RenderTexture(256, 256, 16);
                minimapTexture.filterMode = FilterMode.Bilinear;
            }
            minimapCamera.targetTexture = minimapTexture;

            if (minimapDisplay != null)
                minimapDisplay.texture = minimapTexture;
        }
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        // Follow player from above
        Vector3 camPos = player.position + Vector3.up * 100f;
        minimapCamera.transform.position = Vector3.Lerp(
            minimapCamera.transform.position, camPos, Time.deltaTime * followSpeed);

        // Look down
        minimapCamera.transform.rotation = Quaternion.Euler(90f,
            rotateWithPlayer ? player.eulerAngles.y : 0f, 0f);

        // Zoom
        minimapCamera.orthographicSize = currentZoom;
    }

    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void ZoomIn() => SetZoom(currentZoom - 10f);
    public void ZoomOut() => SetZoom(currentZoom + 10f);
}
