using UnityEngine;

namespace MistbornGameplay
{
    public class BendalloyStore : FeruchemyStore
    {
        [Header("Bendalloy Specific")]
        [SerializeField] private float timeAccelerationMultiplier = 2f;
        [SerializeField] private float timeDecelerationMultiplier = 0.5f;
        [SerializeField] private ParticleSystem timeWarpEffect;
        [SerializeField] private float effectRadius = 15f;
        
        private bool isAcceleratingTime = false;
        private bool isDeceleratingTime = false;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Bendalloy;
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
            
            if (isAcceleratingTime)
            {
                DeactivateTimeAcceleration();
            }
            
            ActivateTimeDeceleration();
            
            storeRate = CalculateStoreRate();
            currentStoredAmount += storeRate * Time.deltaTime;
            currentStoredAmount = Mathf.Min(currentStoredAmount, storageCapacity);
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
            
            if (isDeceleratingTime)
            {
                DeactivateTimeDeceleration();
            }
            
            ActivateTimeAcceleration();
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            
            DeactivateTimeAcceleration();
            DeactivateTimeDeceleration();
            
            Time.timeScale = 1f;
            
            if (timeWarpEffect != null)
            {
                timeWarpEffect.Stop();
            }
        }
        
        private void ActivateTimeAcceleration()
        {
            isAcceleratingTime = true;
            Time.timeScale = timeAccelerationMultiplier;
            
            ApplyEffectToNearby(false);
            
            if (timeWarpEffect != null)
            {
                timeWarpEffect.Play();
            }
            
            if (OnTimeAccelerationActivated != null)
            {
                OnTimeAccelerationActivated();
            }
        }
        
        private void DeactivateTimeAcceleration()
        {
            if (isAcceleratingTime)
            {
                isAcceleratingTime = false;
                Time.timeScale = 1f;
                
                RemoveEffectFromNearby(false);
                
                if (OnTimeAccelerationDeactivated != null)
                {
                    OnTimeAccelerationDeactivated();
                }
            }
        }
        
        private void ActivateTimeDeceleration()
        {
            isDeceleratingTime = true;
            Time.timeScale = timeDecelerationMultiplier;
            
            ApplyEffectToNearby(true);
            
            if (timeWarpEffect != null)
            {
                timeWarpEffect.Play();
            }
            
            if (OnTimeDecelerationActivated != null)
            {
                OnTimeDecelerationActivated();
            }
        }
        
        private void DeactivateTimeDeceleration()
        {
            if (isDeceleratingTime)
            {
                isDeceleratingTime = false;
                Time.timeScale = 1f;
                
                RemoveEffectFromNearby(true);
                
                if (OnTimeDecelerationDeactivated != null)
                {
                    OnTimeDecelerationDeactivated();
                }
            }
        }
        
        private void ApplyEffectToNearby(bool decelerating)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, effectRadius);
            
            foreach (Collider collider in nearbyColliders)
            {
                TimeAffectedByBendalloy timeAffected = collider.GetComponent<TimeAffectedByBendalloy>();
                if (timeAffected != null)
                {
                    float multiplier = decelerating ? timeDecelerationMultiplier : timeAccelerationMultiplier;
                    timeAffected.ApplyTimeWarp(multiplier);
                }
            }
        }
        
        private void RemoveEffectFromNearby(bool decelerating)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, effectRadius);
            
            foreach (Collider collider in nearbyColliders)
            {
                TimeAffectedByBendalloy timeAffected = collider.GetComponent<TimeAffectedByBendalloy>();
                if (timeAffected != null)
                {
                    timeAffected.RemoveTimeWarp();
                }
            }
        }
        
        public bool IsTimeAccelerating()
        {
            return isAcceleratingTime;
        }
        
        public bool IsTimeDecelerating()
        {
            return isDeceleratingTime;
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping && !isStoring)
            {
                return 0f;
            }
            
            float baseEffectiveness = isTapping ? 
                (currentStoredAmount / storageCapacity) : 
                ((storageCapacity - currentStoredAmount) / storageCapacity);
            
            float timeMultiplier = isAcceleratingTime ? timeAccelerationMultiplier : 
                                   isDeceleratingTime ? timeDecelerationMultiplier : 1f;
            
            return baseEffectiveness * timeMultiplier;
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                Color glowColor;
                if (isAcceleratingTime)
                {
                    glowColor = new Color(1f, 0.5f, 0f, 1f);
                }
                else if (isDeceleratingTime)
                {
                    glowColor = new Color(0.2f, 0.5f, 1f, 1f);
                }
                else
                {
                    glowColor = new Color(0.8f, 0.4f, 0f, 0.5f);
                }
                
                visualIndicator.material.color = glowColor;
            }
        }
        
        public event System.Action OnTimeAccelerationActivated;
        public event System.Action OnTimeAccelerationDeactivated;
        public event System.Action OnTimeDecelerationActivated;
        public event System.Action OnTimeDecelerationDeactivated;
    }
    
    public interface TimeAffectedByBendalloy
    {
        void ApplyTimeWarp(float multiplier);
        void RemoveTimeWarp();
    }
}
