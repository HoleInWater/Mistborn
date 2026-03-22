using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class ZincStore : MonoBehaviour
    {
        [Header("Feruchemy - Zinc (Mental Speed)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float mentalSpeedMultiplier = 0.04f;
        
        [Header("Cognitive Effects")]
        [SerializeField] private float normalCooldownReduction = 0f;
        [SerializeField] private float maxCooldownReduction = 0.5f;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentMentalSpeedBonus => currentStored * mentalSpeedMultiplier;
        public float CurrentCooldownReduction => isRetrieving ? currentStored / maxStored * maxCooldownReduction : 0f;
        
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
                    ApplyMentalSpeedPenalty();
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                ApplyMentalSpeedBonus();
            }
        }
        
        private void ApplyMentalSpeedPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * mentalSpeedMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.MentalSpeed, -penalty, "ZincStore");
            }
        }
        
        private void ApplyMentalSpeedBonus()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.MentalSpeed, "ZincStore");
                float bonus = currentStored * mentalSpeedMultiplier;
                playerStats.AddTemporaryModifier(StatType.MentalSpeed, bonus, "ZincRetrieve");
            }
        }
        
        public float GetModifiedCooldown(float baseCooldown)
        {
            if (isRetrieving)
            {
                return baseCooldown * (1f - CurrentCooldownReduction);
            }
            return baseCooldown;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForMentalSpeed(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
    }
}
