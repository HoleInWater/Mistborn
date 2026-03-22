using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap UI")]
    public RawImage minimapImage;
    public Camera minimapCamera;
    public RectTransform playerIcon;
    public RectTransform[] questMarkerIcons;
    
    [Header("Settings")]
    public float mapSize = 200f;
    public float mapZoom = 1f;
    public bool rotateWithPlayer = true;
    public bool showQuestMarkers = true;
    public bool showEnemyMarkers = true;
    public bool showNPCMarkers = true;
    
    [Header("Colors")]
    public Color playerColor = Color.green;
    public Color questMarkerColor = Color.yellow;
    public Color enemyMarkerColor = Color.red;
    public Color npcMarkerColor = Color.blue;
    public Color itemMarkerColor = Color.cyan;
    
    [Header("Icons")]
    public Sprite playerIconSprite;
    public Sprite questIconSprite;
    public Sprite enemyIconSprite;
    public Sprite npcIconSprite;
    public Sprite itemIconSprite;
    
    private Transform playerTransform;
    private Vector3 mapCenter;
    private float worldToMapScale;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        SetupMinimapCamera();
        InitializeMarkers();
    }
    
    void SetupMinimapCamera()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = mapSize / 2f * mapZoom;
        }
    }
    
    void InitializeMarkers()
    {
    }
    
    void LateUpdate()
    {
        UpdatePlayerIcon();
        UpdateQuestMarkers();
        UpdateEnemyMarkers();
        UpdateNPCMarkers();
    }
    
    void UpdatePlayerIcon()
    {
        if (playerIcon == null || playerTransform == null) return;
        
        if (rotateWithPlayer)
        {
            Vector3 euler = playerIcon.eulerAngles;
            euler.z = -playerTransform.eulerAngles.y;
            playerIcon.eulerAngles = euler;
        }
    }
    
    void UpdateQuestMarkers()
    {
        if (!showQuestMarkers) return;
        
        QuestManager questManager = FindObjectOfType<QuestManager>();
        if (questManager == null) return;
        
        foreach (Transform marker in questMarkerIcons)
        {
            marker.gameObject.SetActive(false);
        }
        
        int markerIndex = 0;
        foreach (Quest quest in questManager.GetActiveQuests())
        {
            if (markerIndex >= questMarkerIcons.Length) break;
            
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.isCompleted) continue;
                
                GameObject target = GameObject.Find(objective.targetName);
                if (target == null) continue;
                
                Vector2 mapPosition = WorldToMapPosition(target.transform.position);
                RectTransform marker = questMarkerIcons[markerIndex];
                
                marker.anchoredPosition = mapPosition;
                marker.gameObject.SetActive(true);
                markerIndex++;
            }
        }
    }
    
    void UpdateEnemyMarkers()
    {
        if (!showEnemyMarkers) return;
        
    }
    
    void UpdateNPCMarkers()
    {
        if (!showNPCMarkers) return;
        
    }
    
    Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        Vector3 offset = worldPosition - mapCenter;
        float scale = worldToMapScale * mapZoom;
        
        return new Vector2(offset.x * scale, offset.z * scale);
    }
    
    public void SetZoom(float zoom)
    {
        mapZoom = Mathf.Clamp(zoom, 0.5f, 3f);
        
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = mapSize / 2f * mapZoom;
        }
    }
    
    public void ZoomIn()
    {
        SetZoom(mapZoom - 0.25f);
    }
    
    public void ZoomOut()
    {
        SetZoom(mapZoom + 0.25f);
    }
    
    public void ToggleQuestMarkers()
    {
        showQuestMarkers = !showQuestMarkers;
    }
    
    public void ToggleEnemyMarkers()
    {
        showEnemyMarkers = !showEnemyMarkers;
    }
    
    public void CenterOnPlayer()
    {
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(mapCenter, Vector3.one * mapSize * 2f);
    }
}

public class CompassController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI directionText;
    public RectTransform compassNeedle;
    
    [Header("Settings")]
    public float updateInterval = 0.1f;
    
    private Transform playerTransform;
    private float lastUpdate = 0f;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void Update()
    {
        if (Time.time - lastUpdate < updateInterval) return;
        lastUpdate = Time.time;
        
        UpdateCompass();
    }
    
    void UpdateCompass()
    {
        if (playerTransform == null) return;
        
        float heading = playerTransform.eulerAngles.y;
        
        if (directionText != null)
        {
            directionText.text = GetCardinalDirection(heading);
        }
        
        if (compassNeedle != null)
        {
            compassNeedle.eulerAngles = new Vector3(0, 0, -heading);
        }
    }
    
    string GetCardinalDirection(float heading)
    {
        if (heading < 22.5f || heading >= 337.5f) return "N";
        if (heading < 67.5f) return "NE";
        if (heading < 112.5f) return "E";
        if (heading < 157.5f) return "SE";
        if (heading < 202.5f) return "S";
        if (heading < 247.5f) return "SW";
        if (heading < 292.5f) return "W";
        return "NW";
    }
}
