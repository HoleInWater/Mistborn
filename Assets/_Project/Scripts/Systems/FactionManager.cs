using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class FactionManager : MonoBehaviour
    {
        [Header("Faction Settings")]
        [SerializeField] private Faction[] factions;
        
        [Header("Player Faction")]
        [SerializeField] private string playerFaction = "Player";
        
        private Dictionary<string, Faction> factionDictionary = new Dictionary<string, Faction>();
        private Dictionary<string, Dictionary<string, int>> factionRelations = new Dictionary<string, Dictionary<string, int>>();
        
        [System.Serializable]
        public class Faction
        {
            public string factionName;
            public Color factionColor;
            public int baseRelation = 0;
            public int hostileThreshold = -50;
            public int friendlyThreshold = 50;
            public bool isPlayerFaction = false;
            public Sprite factionIcon;
        }
        
        private void Awake()
        {
            InitializeFactions();
        }
        
        private void Start()
        {
            InitializeRelations();
        }
        
        private void InitializeFactions()
        {
            foreach (Faction faction in factions)
            {
                factionDictionary[faction.factionName] = faction;
            }
        }
        
        private void InitializeRelations()
        {
            foreach (string factionName in factionDictionary.Keys)
            {
                factionRelations[factionName] = new Dictionary<string, int>();
                
                foreach (string otherFactionName in factionDictionary.Keys)
                {
                    if (factionName != otherFactionName)
                    {
                        Faction faction = factionDictionary[factionName];
                        Faction otherFaction = factionDictionary[otherFactionName];
                        
                        int relation = (faction.baseRelation + otherFaction.baseRelation) / 2;
                        factionRelations[factionName][otherFactionName] = relation;
                    }
                }
            }
        }
        
        public void SetRelation(string faction1, string faction2, int relation)
        {
            if (!factionRelations.ContainsKey(faction1))
            {
                factionRelations[faction1] = new Dictionary<string, int>();
            }
            
            factionRelations[faction1][faction2] = Mathf.Clamp(relation, -100, 100);
            factionRelations[faction2][faction1] = Mathf.Clamp(relation, -100, 100);
            
            if (OnRelationChanged != null)
            {
                OnRelationChanged(faction1, faction2, relation);
            }
        }
        
        public void ModifyRelation(string faction1, string faction2, int amount)
        {
            int currentRelation = GetRelation(faction1, faction2);
            SetRelation(faction1, faction2, currentRelation + amount);
        }
        
        public int GetRelation(string faction1, string faction2)
        {
            if (factionRelations.ContainsKey(faction1) && factionRelations[faction1].ContainsKey(faction2))
            {
                return factionRelations[faction1][faction2];
            }
            
            return 0;
        }
        
        public bool AreFactionsHostile(string faction1, string faction2)
        {
            if (factionDictionary.ContainsKey(faction1) && factionDictionary.ContainsKey(faction2))
            {
                int relation = GetRelation(faction1, faction2);
                return relation <= factionDictionary[faction1].hostileThreshold;
            }
            
            return false;
        }
        
        public bool AreFactionsFriendly(string faction1, string faction2)
        {
            if (factionDictionary.ContainsKey(faction1) && factionDictionary.ContainsKey(faction2))
            {
                int relation = GetRelation(faction1, faction2);
                return relation >= factionDictionary[faction1].friendlyThreshold;
            }
            
            return false;
        }
        
        public Faction GetFaction(string factionName)
        {
            if (factionDictionary.ContainsKey(factionName))
            {
                return factionDictionary[factionName];
            }
            
            return null;
        }
        
        public List<string> GetAllFactionNames()
        {
            return new List<string>(factionDictionary.Keys);
        }
        
        public List<string> GetHostileFactions(string faction)
        {
            List<string> hostileFactions = new List<string>();
            
            foreach (string otherFaction in factionDictionary.Keys)
            {
                if (otherFaction != faction && AreFactionsHostile(faction, otherFaction))
                {
                    hostileFactions.Add(otherFaction);
                }
            }
            
            return hostileFactions;
        }
        
        public List<string> GetFriendlyFactions(string faction)
        {
            List<string> friendlyFactions = new List<string>();
            
            foreach (string otherFaction in factionDictionary.Keys)
            {
                if (otherFaction != faction && AreFactionsFriendly(faction, otherFaction))
                {
                    friendlyFactions.Add(otherFaction);
                }
            }
            
            return friendlyFactions;
        }
        
        public void AddFaction(Faction newFaction)
        {
            if (!factionDictionary.ContainsKey(newFaction.factionName))
            {
                factionDictionary[newFaction.factionName] = newFaction;
                factionRelations[newFaction.factionName] = new Dictionary<string, int>();
                
                foreach (string existingFaction in factionDictionary.Keys)
                {
                    if (existingFaction != newFaction.factionName)
                    {
                        int relation = (newFaction.baseRelation + factionDictionary[existingFaction].baseRelation) / 2;
                        factionRelations[newFaction.factionName][existingFaction] = relation;
                        factionRelations[existingFaction][newFaction.factionName] = relation;
                    }
                }
            }
        }
        
        public void RemoveFaction(string factionName)
        {
            if (factionDictionary.ContainsKey(factionName))
            {
                factionDictionary.Remove(factionName);
                factionRelations.Remove(factionName);
                
                foreach (string otherFaction in factionRelations.Keys)
                {
                    if (factionRelations[otherFaction].ContainsKey(factionName))
                    {
                        factionRelations[otherFaction].Remove(factionName);
                    }
                }
            }
        }
        
        public void ResetAllRelations()
        {
            InitializeRelations();
        }
        
        public string GetPlayerFaction()
        {
            return playerFaction;
        }
        
        public void SetPlayerFaction(string faction)
        {
            playerFaction = faction;
        }
        
        public bool IsPlayerFaction(string faction)
        {
            return faction == playerFaction;
        }
        
        public event System.Action<string, string, int> OnRelationChanged;
    }
}
