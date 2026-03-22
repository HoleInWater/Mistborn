using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class IronStore : MonoBehaviour
    {
        [Header("Feruchemy - Iron (Physical Strength)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float strengthMultiplier = 0.02f;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentStrengthBonus => currentStored * strengthMultiplier;
        
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
                float staminaCost = storeRate * Time.deltaTime;
                if (playerStats.CurrentStamina >= staminaCost)
                {
                    currentStored = Mathf.Min(currentStored + storeRate * Time.deltaTime, maxStored);
                    playerStats.UseStamina(staminaCost);
                    ApplyStrengthPenalty();
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                RemoveStrengthPenalty();
            }
        }
        
        private void ApplyStrengthPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * strengthMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.Strength, -penalty, "IronStore");
            }
        }
        
        private void RemoveStrengthPenalty()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.Strength, "IronStore");
                float bonus = currentStored * strengthMultiplier;
                playerStats.AddTemporaryModifier(StatType.Strength, bonus, "IronRetrieve");
            }
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForStrength(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
    }
}
