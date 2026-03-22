using UnityEngine;

namespace MistbornGameplay
{
    public class NicrosilStore : FeruchemyStore
    {
        [Header("Nicrosil Specific")]
        [SerializeField] private float duraluminBoostMultiplier = 3f;
        [SerializeField] private float boostDuration = 2f;
        [SerializeField] private ParticleSystem nicrosilGlow;
        
        private bool isStoringPower = false;
        private bool isReleasingPower = false;
        private float storedPowerBonus = 0f;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Nicrosil;
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
            isStoringPower = true;
            isReleasingPower = false;
            
            storeRate = CalculateStoreRate();
            currentStoredAmount += storeRate * Time.deltaTime;
            currentStoredAmount = Mathf.Min(currentStoredAmount, storageCapacity);
            
            ConsumeNearbyPower();
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
            isStoringPower = false;
            isReleasingPower = true;
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
            
            ReleasePowerToNearby();
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            isStoringPower = false;
            isReleasingPower = false;
            
            if (nicrosilGlow != null)
            {
                nicrosilGlow.Stop();
            }
        }
        
        private void ConsumeNearbyPower()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, powerRange);
            
            foreach (Collider collider in nearbyColliders)
            {
                FeruchemyStore otherStore = collider.GetComponent<FeruchemyStore>();
                if (otherStore != null && otherStore != this && otherStore.IsStoring())
                {
                    float powerDrained = otherStore.ConsumePower(storeRate * Time.deltaTime * powerEfficiency);
                    storedPowerBonus += powerDrained * duraluminBoostMultiplier;
                }
                
                Allomancer allomancer = collider.GetComponent<Allomancer>();
                if (allomancer != null && allomancer != GetComponent<Allomancer>())
                {
                    float powerDrained = allomancer.ConsumeActivePower(storeRate * Time.deltaTime * powerEfficiency);
                    storedPowerBonus += powerDrained * duraluminBoostMultiplier;
                }
            }
            
            if (nicrosilGlow != null)
            {
                nicrosilGlow.Play();
            }
        }
        
        private void ReleasePowerToNearby()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, powerRange);
            
            foreach (Collider collider in nearbyColliders)
            {
                FeruchemyStore otherStore = collider.GetComponent<FeruchemyStore>();
                if (otherStore != null && otherStore != this)
                {
                    float powerToGive = CalculateReleaseAmount() * powerEfficiency;
                    otherStore.ReceivePower(powerToGive);
                }
                
                Allomancer allomancer = collider.GetComponent<Allomancer>();
                if (allomancer != null && allomancer != GetComponent<Allomancer>())
                {
                    float powerToGive = CalculateReleaseAmount() * powerEfficiency;
                    allomancer.ReceiveActivePower(powerToGive);
                }
            }
            
            if (OnPowerReleased != null)
            {
                OnPowerReleased();
            }
        }
        
        private float CalculateReleaseAmount()
        {
            float baseAmount = tapRate * Time.deltaTime;
            float bonusFromStored = storedPowerBonus * Time.deltaTime;
            return baseAmount + bonusFromStored;
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping)
            {
                return 0f;
            }
            
            float baseEffectiveness = currentStoredAmount / storageCapacity;
            float bonusEffectiveness = storedPowerBonus / storageCapacity;
            return baseEffectiveness + bonusEffectiveness;
        }
        
        public void ReceivePower(float amount)
        {
            storedPowerBonus += amount;
        }
        
        public float GetStoredBonusPower()
        {
            return storedPowerBonus;
        }
        
        public void ClearStoredBonusPower()
        {
            storedPowerBonus = 0f;
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                float glowIntensity = isTapping ? 1f : (isStoring ? 0.5f : 0f);
                Color glowColor = new Color(0.6f, 0.1f, 0.9f, glowIntensity);
                visualIndicator.material.color = glowColor;
            }
        }
        
        public event System.Action OnPowerReleased;
        
        public override void OnBoostActivated()
        {
        }
        
        public override void OnBoostDeactivated()
        {
            isReleasingPower = false;
        }
    }
}
