using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class CombatLog : MonoBehaviour
    {
        public static CombatLog Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int m_maxEntries = 50;
        [SerializeField] private float m_entryLifetime = 5f;
        [SerializeField] private bool m_fadeOut = true;
        [SerializeField] private float m_fadeSpeed = 2f;

        [Header("UI")]
        [SerializeField] private GameObject m_logEntryPrefab;
        [SerializeField] private Transform m_logContainer;
        [SerializeField] private bool m_showInWorld = false;

        [Header("Filters")]
        [SerializeField] private bool m_showDamage = true;
        [SerializeField] private bool m_showHealing = true;
        [SerializeField] private bool m_showEffects = true;
        [SerializeField] private bool m_showSystem = true;

        private List<LogEntry> m_entries = new List<LogEntry>();
        private bool m_isPaused;

        public enum LogType
        {
            Damage,
            Healing,
            Effect,
            System,
            Quest,
            Achievement,
            Combat
        }

        [Serializable]
        public class LogEntry
        {
            public string message;
            public LogType type;
            public Color color;
            public float lifetime;
            public float age;
            public GameObject uiElement;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (m_isPaused) return;

            UpdateEntries();
        }

        private void UpdateEntries()
        {
            for (int i = m_entries.Count - 1; i >= 0; i--)
            {
                LogEntry entry = m_entries[i];
                entry.age += Time.deltaTime;

                if (entry.age >= entry.lifetime)
                {
                    RemoveEntry(entry);
                    m_entries.RemoveAt(i);
                }
                else if (m_fadeOut && entry.uiElement != null)
                {
                    float fadeProgress = (entry.age / entry.lifetime);
                    SetEntryOpacity(entry.uiElement, 1f - fadeProgress);
                }
            }
        }

        public void Log(string message, LogType type = LogType.System)
        {
            if (!ShouldLog(type)) return;

            LogEntry entry = new LogEntry
            {
                message = message,
                type = type,
                color = GetColor(type),
                lifetime = m_entryLifetime,
                age = 0f
            };

            CreateUIEntry(entry);
            m_entries.Add(entry);

            if (m_entries.Count > m_maxEntries)
            {
                RemoveEntry(m_entries[0]);
                m_entries.RemoveAt(0);
            }
        }

        public void LogDamage(float amount, Transform source, Transform target)
        {
            if (!m_showDamage) return;

            string message = $"{target.name} took {Mathf.RoundToInt(amount)} damage";
            Log(message, LogType.Damage);

            if (m_showInWorld)
            {
                CreateWorldText(target.position, $"-{Mathf.RoundToInt(amount)}", Color.red);
            }
        }

        public void LogHealing(float amount, Transform target)
        {
            if (!m_showHealing) return;

            string message = $"{target.name} healed for {Mathf.RoundToInt(amount)}";
            Log(message, LogType.Healing);

            if (m_showInWorld)
            {
                CreateWorldText(target.position, $"+{Mathf.RoundToInt(amount)}", Color.green);
            }
        }

        public void LogEffect(string effectName, Transform target, bool applied)
        {
            if (!m_showEffects) return;

            string action = applied ? "affected by" : "removed from";
            string message = $"{target.name} {action} {effectName}";
            Log(message, LogType.Effect);
        }

        public void LogQuest(string message)
        {
            Log(message, LogType.Quest);
        }

        public void LogAchievement(string achievementName)
        {
            string message = $"Achievement Unlocked: {achievementName}";
            Log(message, LogType.Achievement);
        }

        public void LogSystem(string message)
        {
            Log(message, LogType.System);
        }

        public void LogCombat(string message)
        {
            Log($"[Combat] {message}", LogType.Combat);
        }

        private bool ShouldLog(LogType type)
        {
            return type switch
            {
                LogType.Damage => m_showDamage,
                LogType.Healing => m_showHealing,
                LogType.Effect => m_showEffects,
                LogType.System => m_showSystem,
                LogType.Quest => true,
                LogType.Achievement => true,
                LogType.Combat => true,
                _ => true
            };
        }

        private Color GetColor(LogType type)
        {
            return type switch
            {
                LogType.Damage => Color.red,
                LogType.Healing => Color.green,
                LogType.Effect => Color.yellow,
                LogType.System => Color.cyan,
                LogType.Quest => Color.magenta,
                LogType.Achievement => Color.gray,
                LogType.Combat => Color.white,
                _ => Color.white
            };
        }

        private void CreateUIEntry(LogEntry entry)
        {
            if (m_logEntryPrefab == null || m_logContainer == null) return;

            GameObject entryObj = Instantiate(m_logEntryPrefab, m_logContainer);

            UnityEngine.UI.Text[] texts = entryObj.GetComponentsInChildren<UnityEngine.UI.Text>();
            if (texts.Length > 0)
            {
                texts[0].text = entry.message;
                texts[0].color = entry.color;
            }

            entry.uiElement = entryObj;
        }

        private void RemoveEntry(LogEntry entry)
        {
            if (entry.uiElement != null)
            {
                Destroy(entry.uiElement);
            }
        }

        private void SetEntryOpacity(GameObject uiElement, float opacity)
        {
            if (uiElement == null) return;

            UnityEngine.UI.Text[] texts = uiElement.GetComponentsInChildren<UnityEngine.UI.Text>();
            foreach (UnityEngine.UI.Text text in texts)
            {
                Color color = text.color;
                color.a = opacity;
                text.color = color;
            }
        }

        private void CreateWorldText(Vector3 position, string text, Color color)
        {
            GameObject textObj = new GameObject("WorldLogText");
            textObj.transform.position = position + Vector3.up * 2f;

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 24;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            textObj.AddComponent<Billboard>();

            Destroy(textObj, m_entryLifetime);
        }

        public void Clear()
        {
            foreach (LogEntry entry in m_entries)
            {
                RemoveEntry(entry);
            }
            m_entries.Clear();
        }

        public void Pause()
        {
            m_isPaused = true;
        }

        public void Resume()
        {
            m_isPaused = false;
        }

        public void TogglePause()
        {
            m_isPaused = !m_isPaused;
        }

        public List<LogEntry> GetEntries(LogType type)
        {
            List<LogEntry> filtered = new List<LogEntry>();
            foreach (LogEntry entry in m_entries)
            {
                if (entry.type == type)
                {
                    filtered.Add(entry);
                }
            }
            return filtered;
        }

        public int GetEntryCount()
        {
            return m_entries.Count;
        }
    }

    public class Billboard : MonoBehaviour
    {
        private Camera m_camera;

        private void Start()
        {
            m_camera = Camera.main;
        }

        private void Update()
        {
            if (m_camera != null)
            {
                transform.LookAt(transform.position + m_camera.transform.forward);
            }
        }
    }

    public class HitboxDebugger : MonoBehaviour
    {
        [SerializeField] private bool m_showHitboxes = true;
        [SerializeField] private Color m_hitboxColor = Color.red;
        [SerializeField] private float m_alpha = 0.3f;

        private Collider[] m_hitboxes;

        private void Awake()
        {
            m_hitboxes = GetComponentsInChildren<Collider>();
        }

        private void OnDrawGizmos()
        {
            if (!m_showHitboxes) return;

            Gizmos.color = new Color(m_hitboxColor.r, m_hitboxColor.g, m_hitboxColor.b, m_alpha);

            foreach (Collider col in m_hitboxes)
            {
                if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
                else if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(transform.position + box.center, box.size);
                }
                else if (col is CapsuleCollider capsule)
                {
                    Gizmos.DrawWireSphere(transform.position + capsule.center, capsule.radius);
                }
            }
        }
    }

    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private float m_lifetime = 1f;
        [SerializeField] private float m_floatSpeed = 2f;
        [SerializeField] private float m_spread = 0.5f;
        [SerializeField] private Vector3 m_offset = Vector3.up;

        private TextMesh m_textMesh;
        private float m_timer;

        public static void Create(Vector3 position, float damage, bool isCritical = false)
        {
            GameObject obj = new GameObject("DamagePopup");
            obj.transform.position = position + Vector3.up * 0.5f;

            DamagePopup popup = obj.AddComponent<DamagePopup>();
            popup.Setup(damage, isCritical);
        }

        private void Awake()
        {
            m_textMesh = gameObject.AddComponent<TextMesh>();
            m_textMesh.anchor = TextAnchor.MiddleCenter;
            m_textMesh.alignment = TextAlignment.Center;
        }

        private void Setup(float damage, bool isCritical)
        {
            m_textMesh.text = Mathf.RoundToInt(damage).ToString();
            m_textMesh.fontSize = isCritical ? 36 : 24;
            m_textMesh.color = isCritical ? Color.yellow : Color.white;
            m_textMesh.fontStyle = isCritical ? FontStyle.Bold : FontStyle.Normal;

            Vector3 randomSpread = new Vector3(
                UnityEngine.Random.Range(-m_spread, m_spread),
                0,
                UnityEngine.Random.Range(-m_spread, m_spread)
            );
            m_offset += randomSpread;

            Destroy(gameObject, m_lifetime);
        }

        private void Update()
        {
            m_timer += Time.deltaTime;

            transform.position += m_offset * m_floatSpeed * Time.deltaTime;

            float progress = m_timer / m_lifetime;
            Color color = m_textMesh.color;
            color.a = 1f - progress;
            m_textMesh.color = color;

            transform.localScale = Vector3.one * (1f + progress * 0.5f);
        }
    }
}
