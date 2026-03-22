using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Systems
{
    public class AchievementSystem : MonoBehaviour
    {
        [Header("Achievement Configuration")]
        [SerializeField] private List<Achievement> achievements = new List<Achievement>();
        [SerializeField] private bool loadOnStart = true;
        
        [Header("Notification")]
        [SerializeField] private GameObject achievementPopup;
        [SerializeField] private float popupDuration = 3f;
        
        [Header("Progress Tracking")]
        [SerializeField] private Dictionary<string, float> progressTracking = new Dictionary<string, float>();
        
        private HashSet<string> unlockedAchievements = new HashSet<string>();
        
        public static AchievementSystem instance;
        
        public event System.Action<Achievement> OnAchievementUnlocked;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            if (loadOnStart)
            {
                LoadAchievements();
            }
        }
        
        public void UnlockAchievement(string achievementId)
        {
            if (unlockedAchievements.Contains(achievementId))
                return;
            
            Achievement achievement = GetAchievement(achievementId);
            if (achievement == null)
                return;
            
            unlockedAchievements.Add(achievementId);
            
            SaveAchievements();
            
            OnAchievementUnlocked?.Invoke(achievement);
            
            ShowAchievementPopup(achievement);
            
            GrantAchievementRewards(achievement);
        }
        
        public void UpdateProgress(string progressId, float amount)
        {
            if (progressTracking.ContainsKey(progressId))
            {
                progressTracking[progressId] += amount;
            }
            else
            {
                progressTracking[progressId] = amount;
            }
            
            CheckProgressAchievements(progressId);
        }
        
        public void SetProgress(string progressId, float amount)
        {
            progressTracking[progressId] = amount;
            CheckProgressAchievements(progressId);
        }
        
        private void CheckProgressAchievements(string progressId)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.progressId == progressId && !IsUnlocked(achievement.achievementId))
                {
                    float currentProgress = GetProgress(progressId);
                    if (currentProgress >= achievement.requiredProgress)
                    {
                        UnlockAchievement(achievement.achievementId);
                    }
                }
            }
        }
        
        public float GetProgress(string progressId)
        {
            if (progressTracking.ContainsKey(progressId))
            {
                return progressTracking[progressId];
            }
            return 0f;
        }
        
        public bool IsUnlocked(string achievementId)
        {
            return unlockedAchievements.Contains(achievementId);
        }
        
        public Achievement GetAchievement(string achievementId)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.achievementId == achievementId)
                    return achievement;
            }
            return null;
        }
        
        public List<Achievement> GetUnlockedAchievements()
        {
            List<Achievement> unlocked = new List<Achievement>();
            foreach (var achievement in achievements)
            {
                if (IsUnlocked(achievement.achievementId))
                {
                    unlocked.Add(achievement);
                }
            }
            return unlocked;
        }
        
        public List<Achievement> GetLockedAchievements()
        {
            List<Achievement> locked = new List<Achievement>();
            foreach (var achievement in achievements)
            {
                if (!IsUnlocked(achievement.achievementId))
                {
                    locked.Add(achievement);
                }
            }
            return locked;
        }
        
        public int GetUnlockedCount()
        {
            return unlockedAchievements.Count;
        }
        
        public int GetTotalCount()
        {
            return achievements.Count;
        }
        
        public float GetCompletionPercentage()
        {
            if (achievements.Count == 0)
                return 0f;
            
            return (float)unlockedAchievements.Count / achievements.Count * 100f;
        }
        
        private void ShowAchievementPopup(Achievement achievement)
        {
            if (achievementPopup == null)
                return;
            
            GameObject popup = Instantiate(achievementPopup, transform.position, Quaternion.identity);
            AchievementPopup popupComponent = popup.GetComponent<AchievementPopup>();
            
            if (popupComponent != null)
            {
                popupComponent.Initialize(achievement);
                popupComponent.Show(popupDuration);
            }
        }
        
        private void GrantAchievementRewards(Achievement achievement)
        {
            if (achievement.experienceReward > 0)
            {
                PlayerStats stats = FindObjectOfType<PlayerStats>();
                if (stats != null)
                {
                    stats.AddExperience(achievement.experienceReward);
                }
            }
            
            if (achievement.metalReward > 0)
            {
                PlayerStats stats = FindObjectOfType<PlayerStats>();
                if (stats != null)
                {
                    stats.AddMetal(achievement.metalReward);
                }
            }
        }
        
        private void SaveAchievements()
        {
            string[] unlockedArray = new string[unlockedAchievements.Count];
            unlockedAchievements.CopyTo(unlockedArray);
            
            PlayerPrefs.SetString("UnlockedAchievements", string.Join(",", unlockedArray));
            
            SaveProgress();
            
            PlayerPrefs.Save();
        }
        
        private void LoadAchievements()
        {
            if (PlayerPrefs.HasKey("UnlockedAchievements"))
            {
                string saved = PlayerPrefs.GetString("UnlockedAchievements");
                if (!string.IsNullOrEmpty(saved))
                {
                    string[] unlockedArray = saved.Split(',');
                    foreach (var id in unlockedArray)
                    {
                        unlockedAchievements.Add(id);
                    }
                }
            }
            
            LoadProgress();
        }
        
        private void SaveProgress()
        {
            foreach (var kvp in progressTracking)
            {
                PlayerPrefs.SetFloat($"Progress_{kvp.Key}", kvp.Value);
            }
        }
        
        private void LoadProgress()
        {
            foreach (var achievement in achievements)
            {
                if (PlayerPrefs.HasKey($"Progress_{achievement.progressId}"))
                {
                    progressTracking[achievement.progressId] = PlayerPrefs.GetFloat($"Progress_{achievement.progressId}");
                }
            }
        }
        
        public void ResetAchievements()
        {
            unlockedAchievements.Clear();
            progressTracking.Clear();
            
            PlayerPrefs.DeleteKey("UnlockedAchievements");
        }
    }
    
    [System.Serializable]
    public class Achievement
    {
        public string achievementId;
        public string achievementName;
        public string description;
        public AchievementCategory category;
        public int experienceReward = 100;
        public int metalReward = 50;
        
        public bool hasProgress = false;
        public string progressId;
        public float requiredProgress = 100f;
    }
    
    public enum AchievementCategory
    {
        Combat,
        Exploration,
        Allomancy,
        Quests,
        Crafting,
        Social,
        Speedrun,
        Secret
    }
    
    public class AchievementPopup : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Text achievementNameText;
        [SerializeField] private UnityEngine.UI.Text descriptionText;
        [SerializeField] private UnityEngine.UI.Image iconImage;
        [SerializeField] private ParticleSystem unlockParticles;
        
        public void Initialize(Achievement achievement)
        {
            if (achievementNameText != null)
                achievementNameText.text = achievement.achievementName;
            
            if (descriptionText != null)
                descriptionText.text = achievement.description;
            
            if (iconImage != null && achievement.category.ToString() != null)
            {
            }
        }
        
        public void Show(float duration)
        {
            if (unlockParticles != null)
            {
                unlockParticles.Play();
            }
            
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
            
            Invoke(nameof(Hide), duration);
        }
        
        private void Hide()
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.3f).setEase(LeanTweenType.easeInBack)
                .setOnComplete(() => Destroy(gameObject));
        }
    }
}
