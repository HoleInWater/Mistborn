using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class ReputationSystem : MonoBehaviour
    {
        [Header("Reputation Settings")]
        [SerializeField] private int startingReputation = 0;
        [SerializeField] private int minReputation = -100;
        [SerializeField] private int maxReputation = 100;
        [SerializeField] private int enemyThreshold = -50;
        [SerializeField] private int neutralThreshold = 0;
        [SerializeField] private int friendlyThreshold = 50;
        
        [Header("Reputation Changes")]
        [SerializeField] private int killEnemyRepChange = -5;
        [SerializeField] private int helpCivilianRepChange = 5;
        [SerializeField] private int completeQuestRepChange = 10;
        [SerializeField] private int stealFromNPCRepChange = -10;
        
        private int currentReputation = 0;
        private Dictionary<string, int> factionReputations = new Dictionary<string, int>();
        
        public enum ReputationLevel
        {
            Hostile,
            Unfriendly,
            Neutral,
            Friendly,
            Allied
        }
        
        private void Start()
        {
            currentReputation = startingReputation;
        }
        
        public void ModifyReputation(int amount)
        {
            ModifyReputation(amount, "General");
        }
        
        public void ModifyReputation(int amount, string faction)
        {
            if (factionReputations.ContainsKey(faction))
            {
                factionReputations[faction] += amount;
                factionReputations[faction] = Mathf.Clamp(factionReputations[faction], minReputation, maxReputation);
            }
            else
            {
                factionReputations[faction] = Mathf.Clamp(startingReputation + amount, minReputation, maxReputation);
            }
            
            if (faction == "General")
            {
                currentReputation = factionReputations[faction];
            }
            
            if (OnReputationChanged != null)
            {
                OnReputationChanged(GetReputationLevel(faction), amount);
            }
        }
        
        public void SetReputation(int amount)
        {
            SetReputation(amount, "General");
        }
        
        public void SetReputation(int amount, string faction)
        {
            factionReputations[faction] = Mathf.Clamp(amount, minReputation, maxReputation);
            
            if (faction == "General")
            {
                currentReputation = amount;
            }
            
            if (OnReputationChanged != null)
            {
                OnReputationChanged(GetReputationLevel(faction), 0);
            }
        }
        
        public int GetReputation()
        {
            return GetReputation("General");
        }
        
        public int GetReputation(string faction)
        {
            if (factionReputations.ContainsKey(faction))
            {
                return factionReputations[faction];
            }
            
            return startingReputation;
        }
        
        public ReputationLevel GetReputationLevel()
        {
            return GetReputationLevel("General");
        }
        
        public ReputationLevel GetReputationLevel(string faction)
        {
            int rep = GetReputation(faction);
            
            if (rep <= enemyThreshold)
            {
                return ReputationLevel.Hostile;
            }
            else if (rep < neutralThreshold)
            {
                return ReputationLevel.Unfriendly;
            }
            else if (rep < friendlyThreshold)
            {
                return ReputationLevel.Neutral;
            }
            else
            {
                return ReputationLevel.Allied;
            }
        }
        
        public bool IsHostile()
        {
            return GetReputationLevel() == ReputationLevel.Hostile;
        }
        
        public bool IsHostile(string faction)
        {
            return GetReputationLevel(faction) == ReputationLevel.Hostile;
        }
        
        public bool IsFriendly()
        {
            ReputationLevel level = GetReputationLevel();
            return level == ReputationLevel.Friendly || level == ReputationLevel.Allied;
        }
        
        public bool IsFriendly(string faction)
        {
            ReputationLevel level = GetReputationLevel(faction);
            return level == ReputationLevel.Friendly || level == ReputationLevel.Allied;
        }
        
        public float GetReputationPercentage()
        {
            return GetReputationPercentage("General");
        }
        
        public float GetReputationPercentage(string faction)
        {
            int rep = GetReputation(faction);
            return (float)(rep - minReputation) / (float)(maxReputation - minReputation);
        }
        
        public void OnEnemyKilled()
        {
            ModifyReputation(killEnemyRepChange);
        }
        
        public void OnHelpedCivilian()
        {
            ModifyReputation(helpCivilianRepChange);
        }
        
        public void OnQuestCompleted()
        {
            ModifyReputation(completeQuestRepChange);
        }
        
        public void OnStoleFromNPC()
        {
            ModifyReputation(stealFromNPCRepChange);
        }
        
        public void ResetReputation()
        {
            currentReputation = startingReputation;
            factionReputations.Clear();
        }
        
        public void ResetReputation(string faction)
        {
            if (faction == "General")
            {
                currentReputation = startingReputation;
            }
            
            if (factionReputations.ContainsKey(faction))
            {
                factionReputations.Remove(faction);
            }
        }
        
        public Dictionary<string, int> GetAllFactionReputations()
        {
            return new Dictionary<string, int>(factionReputations);
        }
        
        public event System.Action<ReputationLevel, int> OnReputationChanged;
    }
}
