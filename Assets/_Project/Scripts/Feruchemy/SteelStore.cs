using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class SteelStore : MonoBehaviour
    {
        [Header("Feruchemy - Steel (Physical Speed)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float speedMultiplier = 0.03f;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentSpeedBonus => currentStored * speedMultiplier;
        
        private PlayerStats playerStats;
        private CharacterController characterController;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            characterController = GetComponent<CharacterController>();
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
                    ApplySpeedPenalty();
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                ApplySpeedBonus();
            }
        }
        
        private void ApplySpeedPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * speedMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.MoveSpeed, -penalty, "SteelStore");
            }
        }
        
        private void ApplySpeedBonus()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.MoveSpeed, "SteelStore");
                float bonus = currentStored * speedMultiplier;
                playerStats.AddTemporaryModifier(StatType.MoveSpeed, bonus, "SteelRetrieve");
            }
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForSpeed(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
        
        public float GetCurrentMoveSpeed()
        {
            float baseSpeed = playerStats != null ? playerStats.MoveSpeed : 5f;
            float bonus = isRetrieving ? currentStored * speedMultiplier : 0f;
            return baseSpeed + bonus;
        }
    }
}
