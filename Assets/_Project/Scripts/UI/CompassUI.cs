using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class CompassUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image m_compassBackground;
        [SerializeField] private RectTransform m_needleContainer;
        [SerializeField] private Text m_directionText;
        [SerializeField] private Text m_cardinalDirectionText;

        [Header("Markers")]
        [SerializeField] private GameObject m_markerPrefab;
        [SerializeField] private Transform m_markerContainer;
        [SerializeField] private float m_markerSpacing = 30f;
        [SerializeField] private int m_maxVisibleMarkers = 10;

        [Header("Settings")]
        [SerializeField] private float m_rotationSpeed = 5f;
        [SerializeField] private bool m_showDistance = true;

        private Camera m_playerCamera;
        private List<CompassMarker> m_markers = new List<CompassMarker>();
        private float m_currentAngle;
        private string[] m_cardinalDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        private void Awake()
        {
            m_playerCamera = Camera.main;
        }

        private void Update()
        {
            UpdateCompassRotation();
            UpdateMarkers();
        }

        private void UpdateCompassRotation()
        {
            if (m_playerCamera == null) return;

            float targetAngle = m_playerCamera.transform.eulerAngles.y;
            m_currentAngle = Mathf.LerpAngle(m_currentAngle, targetAngle, m_rotationSpeed * Time.deltaTime);

            if (m_needleContainer != null)
            {
                m_needleContainer.localEulerAngles = new Vector3(0, 0, -m_currentAngle);
            }

            UpdateDirectionText();
        }

        private void UpdateDirectionText()
        {
            float normalizedAngle = (m_currentAngle % 360f + 360f) % 360f;

            if (m_directionText != null)
            {
                m_directionText.text = Mathf.RoundToInt(normalizedAngle).ToString() + "°";
            }

            if (m_cardinalDirectionText != null)
            {
                int index = Mathf.RoundToInt(normalizedAngle / 45f) % 8;
                m_cardinalDirectionText.text = m_cardinalDirections[index];
            }
        }

        private void UpdateMarkers()
        {
            foreach (CompassMarker marker in m_markers)
            {
                if (marker == null || marker.Target == null) continue;

                Vector3 direction = marker.Target.position - m_playerCamera.transform.position;
                direction.y = 0;

                float angleToTarget = Vector3.SignedAngle(m_playerCamera.transform.forward, direction, Vector3.up);
                float screenAngle = angleToTarget - m_currentAngle;

                float screenWidth = Screen.width * 0.4f;
                float xPos = Mathf.Lerp(-screenWidth, screenWidth, (screenAngle + 180f) / 360f);

                RectTransform markerRect = marker.UIElement.GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchoredPosition = new Vector2(xPos, 0);

                    bool isVisible = Mathf.Abs(screenAngle) < 90f;
                    marker.UIElement.SetActive(isVisible);

                    if (isVisible && m_showDistance)
                    {
                        float distance = Vector3.Distance(m_playerCamera.transform.position, marker.Target.position);
                        marker.DistanceText.text = Mathf.RoundToInt(distance).ToString();
                    }
                }
            }
        }

        public void AddMarker(Transform target, string name, CompassMarkerType type)
        {
            if (m_markerPrefab == null || m_markerContainer == null) return;
            if (m_markers.Count >= m_maxVisibleMarkers) return;

            GameObject markerObj = Instantiate(m_markerPrefab, m_markerContainer);
            CompassMarker marker = new CompassMarker
            {
                Target = target,
                UIElement = markerObj,
                Type = type
            };

            Text[] texts = markerObj.GetComponentsInChildren<Text>();
            if (texts.Length > 0)
            {
                marker.NameText = texts[0];
                marker.NameText.text = name;
            }

            Text[] distanceTexts = markerObj.GetComponentsInChildren<Text>();
            if (distanceTexts.Length > 1)
            {
                marker.DistanceText = distanceTexts[1];
            }

            Image icon = markerObj.GetComponentInChildren<Image>();
            if (icon != null)
            {
                icon.color = GetMarkerColor(type);
            }

            m_markers.Add(marker);
        }

        public void RemoveMarker(Transform target)
        {
            CompassMarker marker = m_markers.Find(m => m.Target == target);
            if (marker != null)
            {
                if (marker.UIElement != null)
                    Destroy(marker.UIElement);
                m_markers.Remove(marker);
            }
        }

        public void ClearAllMarkers()
        {
            foreach (CompassMarker marker in m_markers)
            {
                if (marker.UIElement != null)
                    Destroy(marker.UIElement);
            }
            m_markers.Clear();
        }

        private Color GetMarkerColor(CompassMarkerType type)
        {
            return type switch
            {
                CompassMarkerType.Checkpoint => Color.green,
                CompassMarkerType.Enemy => Color.red,
                CompassMarkerType.NPC => Color.yellow,
                CompassMarkerType.Item => Color.cyan,
                CompassMarkerType.Quest => Color.magenta,
                CompassMarkerType.Warning => Color.orange,
                _ => Color.white
            };
        }

        private class CompassMarker
        {
            public Transform Target;
            public GameObject UIElement;
            public Text NameText;
            public Text DistanceText;
            public CompassMarkerType Type;
        }

        public enum CompassMarkerType
        {
            Default,
            Checkpoint,
            Enemy,
            NPC,
            Item,
            Quest,
            Warning
        }
    }
}
