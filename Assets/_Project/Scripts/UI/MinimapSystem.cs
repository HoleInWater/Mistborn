using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Minimap system with camera, player indicator, and enemy markers.
/// Uses a top-down orthographic camera rendering to a RenderTexture.
///
/// SETUP: No manual setup required — the system creates itself at runtime.
/// Add this component to any GameObject in your scene, or let the
/// RuntimeInitializeOnLoadMethod create it automatically.
/// </summary>
public class MinimapSystem : MonoBehaviour
{
    public static MinimapSystem Instance { get; private set; }

    [Header("Camera")]
    public Camera minimapCamera;
    public float mapSize = 100f;
    public float followSpeed = 10f;
    [Tooltip("When true the map rotates so the camera's forward direction points up. " +
             "Toggle at runtime with M.")]
    public bool rotateWithCamera = false;   // default: north always up
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
    private Camera    mainCam;
    private float     currentZoom;

    // Auto-create MinimapSystem if no instance exists in the scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (Instance != null) return;
        if (FindObjectOfType<MinimapSystem>() != null) return;

        var go = new GameObject("MinimapSystem");
        go.AddComponent<MinimapSystem>();
        Debug.Log("[MinimapSystem] Auto-created via bootstrap.");
    }

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

        mainCam = Camera.main;

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
                minimapTexture = new RenderTexture(256, 256, 16,
                    RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                minimapTexture.name        = "MinimapRT";
                minimapTexture.filterMode  = FilterMode.Bilinear;
                minimapTexture.antiAliasing = 1;
                minimapTexture.Create();
            }
            minimapCamera.targetTexture = minimapTexture;
        }

        // Auto-create minimap display at top-right if not assigned in Inspector
        if (minimapDisplay == null)
            minimapDisplay = BuildMinimapDisplay();

        if (minimapDisplay != null)
            minimapDisplay.texture = minimapTexture;
    }

    RawImage BuildMinimapDisplay()
    {
        const float size   = 200f;
        const float margin = 10f;

        // Canvas
        var canvasObj = new GameObject("MinimapCanvas");
        DontDestroyOnLoad(canvasObj);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        // Dark border panel (slightly larger than the image)
        var borderObj = new GameObject("MinimapBorder");
        borderObj.transform.SetParent(canvasObj.transform, false);
        var borderImg = borderObj.AddComponent<UnityEngine.UI.Image>();
        borderImg.color = new Color(0f, 0f, 0f, 0.75f);
        var borderRT = borderObj.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = borderRT.pivot = new Vector2(1f, 1f);
        borderRT.anchoredPosition = new Vector2(-margin, -margin);
        borderRT.sizeDelta        = new Vector2(size + 6f, size + 6f);

        // RawImage for the render texture
        var imgObj = new GameObject("MinimapImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        var raw = imgObj.AddComponent<RawImage>();
        var rt = imgObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-margin - 3f, -margin - 3f);
        rt.sizeDelta        = new Vector2(size, size);

        return raw;
    }

    void Update()
    {
        // M toggles rotation mode
        if (Input.GetKeyDown(Keybinds.MinimapRotateToggle))
            ToggleRotation();

        // Re-acquire camera/player each frame in case they're spawned late
        if (mainCam == null) mainCam = Camera.main;
        if (player  == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        // Follow player from above
        Vector3 camPos = player.position + Vector3.up * 100f;
        minimapCamera.transform.position = Vector3.Lerp(
            minimapCamera.transform.position, camPos, Time.deltaTime * followSpeed);

        // Rotation: camera-based when enabled, otherwise north-up (Y=0)
        float yaw = 0f;
        if (rotateWithCamera && mainCam != null)
            yaw = mainCam.transform.eulerAngles.y;

        minimapCamera.transform.rotation = Quaternion.Euler(90f, yaw, 0f);

        // Zoom
        minimapCamera.orthographicSize = currentZoom;
    }

    /// <summary>Flip between north-up and camera-facing modes.</summary>
    public void ToggleRotation()
    {
        rotateWithCamera = !rotateWithCamera;
        Debug.Log($"[MinimapSystem] rotateWithCamera = {rotateWithCamera}");
    }

    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void ZoomIn()  => SetZoom(currentZoom - 10f);
    public void ZoomOut() => SetZoom(currentZoom + 10f);
}
