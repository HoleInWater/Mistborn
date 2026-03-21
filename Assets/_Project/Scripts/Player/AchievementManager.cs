using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Player
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "Mistborn/Achievement")]
    public class Achievement : ScriptableObject
    {
        public string achievementId;
        public string title;
        [TextArea(2, 3)]
        public string description;
        public Sprite icon;
        public int rewardPoints;
        public bool isSecret;

        [NonSerialized] public bool isUnlocked;
        [NonSerialized] public DateTime unlockTime;
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Achievements")]
        [SerializeField] private Achievement[] m_achievements;

        [Header("UI")]
        [SerializeField] private GameObject m_notificationPrefab;
        [SerializeField] private Transform m_notificationContainer;

        [Header("Settings")]
        [SerializeField] private float m_notificationDuration = 3f;
        [SerializeField] private bool m_loadOnStart = true;

        private Dictionary<string, Achievement> m_achievementLookup = new Dictionary<string, Achievement>();
        private List<string> m_unlockedIds = new List<string>();

        public event System.Action<Achievement> OnAchievementUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildLookup();
        }

        private void Start()
        {
            if (m_loadOnStart)
            {
                LoadProgress();
            }
        }

        private void BuildLookup()
        {
            m_achievementLookup.Clear();
            foreach (Achievement achievement in m_achievements)
            {
                if (achievement != null)
                {
                    m_achievementLookup[achievement.achievementId] = achievement;
                }
            }
        }

        public void Unlock(string achievementId)
        {
            if (!m_achievementLookup.TryGetValue(achievementId, out Achievement achievement))
            {
                Debug.LogWarning($"Achievement '{achievementId}' not found");
                return;
            }

            if (achievement.isUnlocked) return;

            achievement.isUnlocked = true;
            achievement.unlockTime = DateTime.Now;

            if (!m_unlockedIds.Contains(achievementId))
            {
                m_unlockedIds.Add(achievementId);
            }

            ShowNotification(achievement);
            OnAchievementUnlocked?.Invoke(achievement);
            SaveProgress();
        }

        public void IncrementProgress(string achievementId, int amount = 1)
        {
            if (!m_progressLookup.TryGetValue(achievementId, out int current))
            {
                current = 0;
            }

            m_progressLookup[achievementId] = current + amount;

            CheckProgressTriggers(achievementId);
        }

        public void SetProgress(string achievementId, int value)
        {
            m_progressLookup[achievementId] = value;
            CheckProgressTriggers(achievementId);
        }

        private void CheckProgressTriggers(string achievementId)
        {
        }

        public bool IsUnlocked(string achievementId)
        {
            return m_achievementLookup.TryGetValue(achievementId, out Achievement achievement) && achievement.isUnlocked;
        }

        public Achievement GetAchievement(string achievementId)
        {
            m_achievementLookup.TryGetValue(achievementId, out Achievement achievement);
            return achievement;
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            List<Achievement> unlocked = new List<Achievement>();
            foreach (Achievement achievement in m_achievements)
            {
                if (achievement.isUnlocked)
                {
                    unlocked.Add(achievement);
                }
            }
            return unlocked;
        }

        public int GetTotalRewardPoints()
        {
            int total = 0;
            foreach (Achievement achievement in m_achievements)
            {
                if (achievement.isUnlocked)
                {
                    total += achievement.rewardPoints;
                }
            }
            return total;
        }

        private void ShowNotification(Achievement achievement)
        {
            if (m_notificationPrefab == null || m_notificationContainer == null) return;

            GameObject notification = Instantiate(m_notificationPrefab, m_notificationContainer);

            UnityEngine.UI.Text[] texts = notification.GetComponentsInChildren<UnityEngine.UI.Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = $"Achievement Unlocked: {achievement.title}";
                texts[1].text = achievement.description;
            }

            UnityEngine.UI.Image icon = notification.GetComponentInChildren<UnityEngine.UI.Image>();
            if (icon != null && achievement.icon != null)
            {
                icon.sprite = achievement.icon;
            }

            Destroy(notification, m_notificationDuration);
        }

        private void SaveProgress()
        {
            string[] ids = m_unlockedIds.ToArray();
            PlayerPrefs.SetString("Achievements_Unlocked", string.Join(",", ids));
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            string saved = PlayerPrefs.GetString("Achievements_Unlocked", "");
            if (string.IsNullOrEmpty(saved)) return;

            string[] ids = saved.Split(',');
            foreach (string id in ids)
            {
                if (m_achievementLookup.TryGetValue(id, out Achievement achievement))
                {
                    achievement.isUnlocked = true;
                    if (!m_unlockedIds.Contains(id))
                    {
                        m_unlockedIds.Add(id);
                    }
                }
            }
        }

        public void ResetAll()
        {
            foreach (Achievement achievement in m_achievements)
            {
                achievement.isUnlocked = false;
            }
            m_unlockedIds.Clear();
            m_progressLookup.Clear();
            PlayerPrefs.DeleteKey("Achievements_Unlocked");
        }

        private Dictionary<string, int> m_progressLookup = new Dictionary<string, int>();
    }

    public static class AchievementTriggers
    {
        public static void OnEnemyKilled(string enemyType)
        {
            AchievementManager.Instance?.IncrementProgress($"kill_{enemyType}");

            int kills = PlayerPrefs.GetInt($"kill_{enemyType}_count", 0) + 1;
            PlayerPrefs.SetInt($"kill_{enemyType}_count", kills);

            if (kills >= 10)
                AchievementManager.Instance?.Unlock($"kill_{enemyType}_10");
            if (kills >= 50)
                AchievementManager.Instance?.Unlock($"kill_{enemyType}_50");
            if (kills >= 100)
                AchievementManager.Instance?.Unlock($"kill_{enemyType}_100");
        }

        public static void OnMetalBurned(string metalName)
        {
            int count = PlayerPrefs.GetInt($"burn_{metalName}", 0) + 1;
            PlayerPrefs.SetInt($"burn_{metalName}", count);
        }

        public static void OnCheckpointReached(string checkpointId)
        {
            AchievementManager.Instance?.Unlock($"checkpoint_{checkpointId}");
        }

        public static void OnDamageDealt(float amount, string damageType)
        {
            float total = PlayerPrefs.GetFloat($"damage_{damageType}_total", 0) + amount;
            PlayerPrefs.SetFloat($"damage_{damageType}_total", total);

            if (total >= 10000)
                AchievementManager.Instance?.Unlock($"damage_{damageType}_10k");
        }
    }
}
