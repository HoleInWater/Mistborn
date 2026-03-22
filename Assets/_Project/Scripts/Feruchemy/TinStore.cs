using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class TinStore : MonoBehaviour
    {
        [Header("Feruchemy - Tin (Physical Senses)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float sensesMultiplier = 0.04f;
        
        [Header("Sensory Effects")]
        [SerializeField] private float normalVisionRange = 100f;
        [SerializeField] private float enhancedVisionRange = 200f;
        [SerializeField] private AudioSource hearingEnhancement;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentSensesBonus => currentStored * sensesMultiplier;
        
        private PlayerStats playerStats;
        private Camera playerCamera;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerCamera = GetComponentInChildren<Camera>();
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
                float staminaCost = storeRate * Time.deltaTime;
                if (playerStats.CurrentStamina >= staminaCost)
                {
                    currentStored = Mathf.Min(currentStored + storeRate * Time.deltaTime, maxStored);
                    playerStats.UseStamina(staminaCost);
                    ApplySensesPenalty();
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                ApplySensesBonus();
            }
        }
        
        private void ApplySensesPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * sensesMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.Perception, -penalty, "TinStore");
            }
            
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = Mathf.Lerp(normalVisionRange, normalVisionRange * 0.5f, currentStored / maxStored);
            }
        }
        
        private void ApplySensesBonus()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.Perception, "TinStore");
                float bonus = currentStored * sensesMultiplier;
                playerStats.AddTemporaryModifier(StatType.Perception, bonus, "TinRetrieve");
            }
            
            if (playerCamera != null)
            {
                playerCamera.farClipPlane = Mathf.Lerp(normalVisionRange, enhancedVisionRange, currentStored / maxStored);
            }
            
            if (hearingEnhancement != null)
            {
                hearingEnhancement.volume = Mathf.Lerp(0f, 1f, currentStored / maxStored);
            }
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForSenses(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
    }
}
