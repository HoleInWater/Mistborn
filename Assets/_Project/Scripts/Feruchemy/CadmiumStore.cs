using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class CadmiumStore : MonoBehaviour
    {
        [Header("Feruchemy - Cadmium (Air Storage)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float airCapacityMultiplier = 0.03f;
        
        [Header("Breathing Effects")]
        [SerializeField] private float normalBreathHoldTime = 30f;
        [SerializeField] private float extendedBreathHoldTime = 300f;
        [SerializeField] private O2Meter playerO2Meter;
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem breathBubblesParticles;
        [SerializeField] private Material normalLungsMaterial;
        [SerializeField] private Material expandedLungsMaterial;
        [SerializeField] private SkinnedMeshRenderer lungsRenderer;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentAirBonus => currentStored * airCapacityMultiplier;
        public float CurrentBreathHoldTime => Mathf.Lerp(normalBreathHoldTime, extendedBreathHoldTime, currentStored / maxStored);
        
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
            UpdateVisualMaterial(false);
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
            if (isStoring)
            {
                float airStorageAmount = storeRate * Time.deltaTime;
                currentStored = Mathf.Min(currentStored + airStorageAmount, maxStored);
                ApplyAirCapacityPenalty();
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                ApplyAirCapacityBonus();
            }
        }
        
        private void ApplyAirCapacityPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * airCapacityMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.AirCapacity, -penalty, "CadmiumStore");
            }
            UpdateVisualMaterial(false);
        }
        
        private void ApplyAirCapacityBonus()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.AirCapacity, "CadmiumStore");
                float bonus = currentStored * airCapacityMultiplier;
                playerStats.AddTemporaryModifier(StatType.AirCapacity, bonus, "CadmiumRetrieve");
            }
            UpdateVisualMaterial(true);
            
            if (playerO2Meter != null)
            {
                playerO2Meter.AddOxygen(retrieveRate * Time.deltaTime * 10f);
            }
        }
        
        private void UpdateVisualMaterial(bool expanded)
        {
            if (lungsRenderer != null)
            {
                lungsRenderer.material = expanded ? expandedLungsMaterial : normalLungsMaterial;
            }
            
            if (breathBubblesParticles != null)
            {
                if (expanded && isRetrieving)
                    breathBubblesParticles.Play();
                else
                    breathBubblesParticles.Stop();
            }
        }
        
        public float GetBreathHoldDuration()
        {
            float baseDuration = normalBreathHoldTime;
            if (isRetrieving)
            {
                baseDuration = CurrentBreathHoldTime;
            }
            return baseDuration;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForAir(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
        
        private class O2Meter : MonoBehaviour
        {
            public float currentO2 = 100f;
            public float maxO2 = 100f;
            
            public void AddOxygen(float amount)
            {
                currentO2 = Mathf.Min(currentO2 + amount, maxO2);
            }
            
            public void UseOxygen(float amount)
            {
                currentO2 = Mathf.Max(currentO2 - amount, 0f);
            }
        }
    }
}
