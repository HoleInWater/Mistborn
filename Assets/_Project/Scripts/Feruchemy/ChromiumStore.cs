using UnityEngine;

namespace MistbornGameplay
{
    public class ChromiumStore : FeruchemyStore
    {
        [Header("Chromium Specific")]
        [SerializeField] private float luckBoostDuration = 3f;
        [SerializeField] private ParticleSystem luckEffect;
        [SerializeField] private float criticalHitChanceBonus = 0.25f;
        [SerializeField] private float dodgeChanceBonus = 0.3f;
        
        private bool isLuckActive = false;
        private float luckEndTime = 0f;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Chromium;
            storageCapacity = 100f;
            currentStoredAmount = 50f;
        }
        
        public override void StartStoring()
        {
            if (currentStoredAmount >= storageCapacity)
            {
                PlayFullStorageEffect();
                return;
            }
            
            isStoring = true;
            isTapping = false;
            
            storeRate = CalculateStoreRate();
            currentStoredAmount += storeRate * Time.deltaTime;
            currentStoredAmount = Mathf.Min(currentStoredAmount, storageCapacity);
            
            ApplyBadLuckEffect();
        }
        
        public override void StartTapping()
        {
            if (currentStoredAmount <= 0f)
            {
                PlayEmptyEffect();
                return;
            }
            
            if (!isLuckActive)
            {
                ActivateLuck();
            }
            
            isTapping = true;
            isStoring = false;
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
            
            if (Time.time >= luckEndTime)
            {
                DeactivateLuck();
            }
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            
            if (luckEffect != null)
            {
                luckEffect.Stop();
            }
        }
        
        private void ActivateLuck()
        {
            isLuckActive = true;
            luckEndTime = Time.time + luckBoostDuration;
            
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.CriticalHitChance, criticalHitChanceBonus);
                stats.ModifyStat(StatType.DodgeChance, dodgeChanceBonus);
            }
            
            if (luckEffect != null)
            {
                luckEffect.Play();
            }
            
            if (OnLuckActivated != null)
            {
                OnLuckActivated();
            }
        }
        
        private void DeactivateLuck()
        {
            isLuckActive = false;
            
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.CriticalHitChance, -criticalHitChanceBonus);
                stats.ModifyStat(StatType.DodgeChance, -dodgeChanceBonus);
            }
            
            if (OnLuckDeactivated != null)
            {
                OnLuckDeactivated();
            }
        }
        
        private void ApplyBadLuckEffect()
        {
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.CriticalHitChance, -criticalHitChanceBonus * 0.5f);
                stats.ModifyStat(StatType.DodgeChance, -dodgeChanceBonus * 0.5f);
            }
        }
        
        public bool IsLuckActive()
        {
            return isLuckActive;
        }
        
        public float GetRemainingLuckTime()
        {
            return Mathf.Max(0f, luckEndTime - Time.time);
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping)
            {
                return 0f;
            }
            
            float baseEffectiveness = currentStoredAmount / storageCapacity;
            float timeRemaining = GetRemainingLuckTime() / luckBoostDuration;
            return baseEffectiveness * (1f + timeRemaining);
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                Color glowColor = isLuckActive ? 
                    new Color(1f, 0.9f, 0.2f, 1f) : 
                    new Color(0.8f, 0.7f, 0f, 0.5f);
                
                visualIndicator.material.color = glowColor;
            }
        }
        
        public event System.Action OnLuckActivated;
        public event System.Action OnLuckDeactivated;
    }
}
