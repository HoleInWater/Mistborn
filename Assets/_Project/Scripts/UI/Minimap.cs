using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class Minimap : MonoBehaviour
    {
        public static Minimap Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private RawImage m_mapImage;
        [SerializeField] private RectTransform m_playerIcon;
        [SerializeField] private RectTransform m_iconContainer;
        [SerializeField] private RectTransform m_borders;

        [Header("Settings")]
        [SerializeField] private float m_mapScale = 1f;
        [SerializeField] private float m_rotationSpeed = 5f;
        [SerializeField] private bool m_rotateWithPlayer = true;
        [SerializeField] private bool m_showQuestMarkers = true;
        [SerializeField] private bool m_showEnemies = true;
        [SerializeField] private bool m_showNPCs = true;

        [Header("Icons")]
        [SerializeField] private Sprite m_questIcon;
        [SerializeField] private Sprite m_enemyIcon;
        [SerializeField] private Sprite m_npcIcon;
        [SerializeField] private Sprite m_itemIcon;
        [SerializeField] private Sprite m_dangerIcon;

        [Header("Map Bounds")]
        [SerializeField] private Vector2 m_mapSize = new Vector2(500f, 500f);
        [SerializeField] private Vector2 m_worldOrigin = Vector2.zero;

        private Camera m_mainCamera;
        private Transform m_player;
        private List<MinimapMarker> m_markers = new List<MinimapMarker>();
        private float m_targetRotation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            m_mainCamera = Camera.main;
            m_player = m_mainCamera?.transform;

            if (m_playerIcon != null)
                m_playerIcon.rotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (m_player == null) return;

            UpdatePlayerPosition();
            UpdateMarkers();
        }

        private void UpdatePlayerPosition()
        {
            if (m_playerIcon == null) return;

            Vector2 mapPos = WorldToMapPosition(m_player.position);
            m_playerIcon.anchoredPosition = mapPos;

            if (m_rotateWithPlayer)
            {
                UpdateRotation();
            }
        }

        private void UpdateRotation()
        {
            if (m_playerIcon == null) return;

            float playerYaw = m_player.eulerAngles.y;
            m_targetRotation = -playerYaw;

            float currentRotation = m_playerIcon.rotation.eulerAngles.z;
            float newRotation = Mathf.LerpAngle(currentRotation, m_targetRotation, m_rotationSpeed * Time.deltaTime);
            m_playerIcon.rotation = Quaternion.Euler(0, 0, newRotation);
        }

        private void UpdateMarkers()
        {
            foreach (MinimapMarker marker in m_markers)
            {
                if (marker.uiElement == null || !marker.isVisible) continue;

                Vector2 mapPos = WorldToMapPosition(marker.worldPosition);
                marker.uiElement.anchoredPosition = mapPos;

                bool isInRange = Vector2.Distance(mapPos, m_playerIcon.anchoredPosition) < GetRadius();
                marker.uiElement.gameObject.SetActive(isInRange);
            }
        }

        public Vector2 WorldToMapPosition(Vector3 worldPos)
        {
            Vector2 offset = new Vector2(worldPos.x - m_worldOrigin.x, worldPos.z - m_worldOrigin.y);
            return offset * m_mapScale;
        }

        public Vector3 MapToWorldPosition(Vector2 mapPos)
        {
            Vector2 offset = mapPos / m_mapScale;
            return new Vector3(m_worldOrigin.x + offset.x, 0, m_worldOrigin.y + offset.y);
        }

        private float GetRadius()
        {
            if (m_borders == null) return 100f;
            return Mathf.Min(m_borders.rect.width, m_borders.rect.height) * 0.5f;
        }

        public void AddMarker(Vector3 worldPosition, MarkerType type, string label = "", bool important = false)
        {
            GameObject iconObj = new GameObject($"Marker_{label}");
            iconObj.transform.SetParent(m_iconContainer);

            Image icon = iconObj.AddComponent<Image>();
            icon.sprite = GetIconForType(type);
            icon.color = GetColorForType(type);

            float size = important ? 20f : 12f;
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            MinimapMarker marker = new MinimapMarker
            {
                worldPosition = worldPosition,
                type = type,
                label = label,
                uiElement = iconObj.GetComponent<RectTransform>(),
                isVisible = true,
                isImportant = important
            };

            m_markers.Add(marker);
        }

        public void AddQuestMarker(Vector3 worldPosition, Quest quest, bool isObjective = false)
        {
            if (!m_showQuestMarkers) return;

            AddMarker(worldPosition, MarkerType.Quest, quest.title, isObjective);
        }

        public void AddEnemyMarker(Transform enemy)
        {
            if (!m_showEnemies) return;

            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null && enemyBase.isDead) return;

            AddMarker(enemy.position, MarkerType.Enemy, "Enemy");
        }

        public void AddNPCMarker(Transform npc)
        {
            if (!m_showNPCs) return;

            AddMarker(npc.position, MarkerType.NPC, "NPC");
        }

        public void AddItemMarker(Vector3 position, string itemName = "")
        {
            AddMarker(position, MarkerType.Item, itemName);
        }

        public void AddDangerMarker(Vector3 position, string dangerType = "")
        {
            AddMarker(position, MarkerType.Danger, dangerType, true);
        }

        public void RemoveMarker(Vector3 worldPosition, float threshold = 1f)
        {
            for (int i = m_markers.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(m_markers[i].worldPosition, worldPosition) < threshold)
                {
                    if (m_markers[i].uiElement != null)
                        Destroy(m_markers[i].uiElement.gameObject);
                    m_markers.RemoveAt(i);
                }
            }
        }

        public void ClearMarkers(MarkerType? type = null)
        {
            for (int i = m_markers.Count - 1; i >= 0; i--)
            {
                if (type == null || m_markers[i].type == type.Value)
                {
                    if (m_markers[i].uiElement != null)
                        Destroy(m_markers[i].uiElement.gameObject);
                    m_markers.RemoveAt(i);
                }
            }
        }

        private Sprite GetIconForType(MarkerType type)
        {
            return type switch
            {
                MarkerType.Quest => m_questIcon,
                MarkerType.Enemy => m_enemyIcon,
                MarkerType.NPC => m_npcIcon,
                MarkerType.Item => m_itemIcon,
                MarkerType.Danger => m_dangerIcon,
                _ => null
            };
        }

        private Color GetColorForType(MarkerType type)
        {
            return type switch
            {
                MarkerType.Quest => Color.yellow,
                MarkerType.Enemy => Color.red,
                MarkerType.NPC => Color.green,
                MarkerType.Item => Color.cyan,
                MarkerType.Danger => Color.magenta,
                _ => Color.white
            };
        }

        public void SetZoom(float zoomLevel)
        {
            m_mapScale = Mathf.Clamp(zoomLevel, 0.1f, 5f);
        }

        public void ToggleMinimap()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void ShowMinimap()
        {
            gameObject.SetActive(true);
        }

        public void HideMinimap()
        {
            gameObject.SetActive(false);
        }

        private class MinimapMarker
        {
            public Vector3 worldPosition;
            public MarkerType type;
            public string label;
            public RectTransform uiElement;
            public bool isVisible;
            public bool isImportant;
        }

        public enum MarkerType
        {
            Quest,
            Enemy,
            NPC,
            Item,
            Danger
        }
    }
}
