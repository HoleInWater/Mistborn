using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class PewterStore : MonoBehaviour
    {
        [Header("Feruchemy - Pewter (Physical Toughness)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float toughnessMultiplier = 0.03f;
        
        [Header("Visual Feedback")]
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material toughenedMaterial;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentToughnessBonus => currentStored * toughnessMultiplier;
        
        private PlayerStats playerStats;
        private Health playerHealth;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerHealth = GetComponent<Health>();
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
            if (playerHealth == null)
                playerHealth = FindObjectOfType<Health>();
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
                    ApplyToughnessPenalty();
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
                ApplyToughnessBonus();
            }
        }
        
        private void ApplyToughnessPenalty()
        {
            if (playerStats != null)
            {
                float penalty = currentStored * toughnessMultiplier * 0.5f;
                playerStats.AddTemporaryModifier(StatType.Armor, -penalty, "PewterStore");
            }
            
            UpdateVisualMaterial(false);
        }
        
        private void ApplyToughnessBonus()
        {
            if (playerStats != null)
            {
                playerStats.RemoveTemporaryModifier(StatType.Armor, "PewterStore");
                float bonus = currentStored * toughnessMultiplier;
                playerStats.AddTemporaryModifier(StatType.Armor, bonus, "PewterRetrieve");
            }
            
            UpdateVisualMaterial(true);
        }
        
        private void UpdateVisualMaterial(bool toughened)
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.material = toughened ? toughenedMaterial : normalMaterial;
            }
        }
        
        public void TakeReducedDamage(float baseDamage)
        {
            if (isRetrieving)
            {
                float reduction = currentStored * toughnessMultiplier * 0.1f;
                float reducedDamage = baseDamage * (1f - Mathf.Clamp(reduction, 0f, 0.8f));
                return reducedDamage;
            }
            return baseDamage;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForToughness(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
    }
}
