using UnityEngine;

namespace MistbornGameplay
{
    public class DuraluminStore : FeruchemyStore
    {
        [Header("Duralumin Specific")]
        [SerializeField] private float nicrosilBoostMultiplier = 3f;
        [SerializeField] private float nicrosilBoostDuration = 2f;
        [SerializeField] private ParticleSystem nicrosilEffect;
        
        private bool isTapModeActive = false;
        private float originalMetalBurnRate;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Duralumin;
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
            
            if (nicrosilEffect != null)
            {
                nicrosilEffect.Play();
            }
        }
        
        public override void StartTapping()
        {
            if (currentStoredAmount <= 0f)
            {
                PlayEmptyEffect();
                return;
            }
            
            isTapping = true;
            isStoring = false;
            
            if (!isTapModeActive)
            {
                ActivateNicrosilBoost();
            }
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
            
            if (currentStoredAmount <= 0f)
            {
                DeactivateNicrosilBoost();
            }
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            
            if (isTapModeActive)
            {
                DeactivateNicrosilBoost();
            }
            
            if (nicrosilEffect != null)
            {
                nicrosilEffect.Stop();
            }
        }
        
        private void ActivateNicrosilBoost()
        {
            isTapModeActive = true;
            
            Allomancer allomancer = GetComponent<Allomancer>();
            if (allomancer != null)
            {
                allomancer.ModifyMetalBurnRate(nicrosilBoostMultiplier);
            }
            
            PlayTapEffect();
            
            if (OnNicrosilBoostActivated != null)
            {
                OnNicrosilBoostActivated();
            }
        }
        
        private void DeactivateNicrosilBoost()
        {
            isTapModeActive = false;
            
            Allomancer allomancer = GetComponent<Allomancer>();
            if (allomancer != null)
            {
                allomancer.ResetMetalBurnRate();
            }
        }
        
        public bool IsBoostActive()
        {
            return isTapModeActive;
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping || !isTapModeActive)
            {
                return 0f;
            }
            
            float baseEffectiveness = currentStoredAmount / storageCapacity;
            return baseEffectiveness * nicrosilBoostMultiplier;
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                Color glowColor = isTapModeActive ? 
                    new Color(0.8f, 0.2f, 1f, 1f) : 
                    new Color(0.5f, 0.1f, 0.8f, 1f);
                
                visualIndicator.material.color = glowColor;
            }
        }
        
        private void PlayTapEffect()
        {
            if (nicrosilEffect != null)
            {
                nicrosilEffect.Play();
            }
            
            if (audioSource != null)
            {
                audioSource.clip = tapSound;
                audioSource.Play();
            }
        }
        
        public event System.Action OnNicrosilBoostActivated;
        public event System.Action OnNicrosilBoostDeactivated;
        
        public override void OnBoostActivated()
        {
            if (OnNicrosilBoostActivated != null)
            {
                OnNicrosilBoostActivated();
            }
        }
        
        public override void OnBoostDeactivated()
        {
            isTapModeActive = false;
            
            if (OnNicrosilBoostDeactivated != null)
            {
                OnNicrosilBoostDeactivated();
            }
        }
    }
}
