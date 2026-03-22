using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class CopperStore : MonoBehaviour
    {
        [Header("Feruchemy - Copper (Memory Storage)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float memoryMultiplier = 0.05f;
        
        [Header("Memory Storage")]
        [SerializeField] private int maxMemories = 100;
        [SerializeField] private float memoryCapacityPerUnit = 1f;
        
        [Header("Mental Effects")]
        [SerializeField] private float normalExperienceGain = 1f;
        [SerializeField] private float enhancedExperienceGain = 2f;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        private System.Collections.Generic.List<MemoryData> storedMemories = new System.Collections.Generic.List<MemoryData>();
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentMemoryBonus => currentStored * memoryMultiplier;
        public int StoredMemoryCount => storedMemories.Count;
        
        private PlayerStats playerStats;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
        }
        
        public void StartStoring()
        {
            if (currentStored >= maxStored) return;
            isStoring = true;
            isRetrieving = false;
        }
        
        public void StopStoring()
        {
            isStoring = false;
        }
        
        public void StartRetrieving()
        {
            if (currentStored <= 0) return;
            isRetrieving = true;
            isStoring = false;
        }
        
        public void StopRetrieving()
        {
            isRetrieving = false;
        }
        
        private void Update()
        {
            if (isStoring && playerStats != null)
            {
                float mentalEnergyCost = storeRate * Time.deltaTime;
                if (playerStats.CurrentMentalEnergy >= mentalEnergyCost)
                {
                    currentStored = Mathf.Min(currentStored + storeRate * Time.deltaTime, maxStored);
                    playerStats.UseMentalEnergy(mentalEnergyCost);
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
            }
        }
        
        public void StoreExperience(float amount)
        {
            float storageNeeded = amount * memoryCapacityPerUnit;
            if (currentStored + storageNeeded <= maxStored)
            {
                currentStored += storageNeeded;
                storedMemories.Add(new MemoryData
                {
                    type = MemoryType.Experience,
                    amount = amount,
                    timestamp = Time.time
                });
            }
        }
        
        public float RetrieveExperience()
        {
            if (isRetrieving && storedMemories.Count > 0)
            {
                MemoryData memory = storedMemories[0];
                if (memory.type == MemoryType.Experience)
                {
                    storedMemories.RemoveAt(0);
                    return memory.amount * enhancedExperienceGain;
                }
            }
            return 0f;
        }
        
        public void StoreSkillMemory(string skillName, float proficiency)
        {
            if (currentStored + memoryCapacityPerUnit <= maxStored)
            {
                currentStored += memoryCapacityPerUnit;
                storedMemories.Add(new MemoryData
                {
                    type = MemoryType.Skill,
                    skillName = skillName,
                    amount = proficiency,
                    timestamp = Time.time
                });
            }
        }
        
        public float GetStoredSkillProficiency(string skillName)
        {
            foreach (var memory in storedMemories)
            {
                if (memory.type == MemoryType.Skill && memory.skillName == skillName)
                {
                    return memory.amount * (isRetrieving ? enhancedExperienceGain : normalExperienceGain);
                }
            }
            return 0f;
        }
        
        public void StoreLocationMemory(Vector3 position, string locationName)
        {
            if (currentStored + memoryCapacityPerUnit <= maxStored)
            {
                currentStored += memoryCapacityPerUnit;
                storedMemories.Add(new MemoryData
                {
                    type = MemoryType.Location,
                    position = position,
                    skillName = locationName,
                    timestamp = Time.time
                });
            }
        }
        
        public Vector3? GetStoredLocation(string locationName)
        {
            foreach (var memory in storedMemories)
            {
                if (memory.type == MemoryType.Location && memory.skillName == locationName)
                {
                    return memory.position;
                }
            }
            return null;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForMemory(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
        
        private enum MemoryType
        {
            Experience,
            Skill,
            Location,
            Conversation
        }
        
        private struct MemoryData
        {
            public MemoryType type;
            public string skillName;
            public float amount;
            public Vector3 position;
            public float timestamp;
        }
    }
}
