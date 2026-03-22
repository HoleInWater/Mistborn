using UnityEngine;

namespace MistbornGameplay
{
    public class AtiumStore : FeruchemyStore
    {
        [Header("Atium Specific")]
        [SerializeField] private float futureSightDuration = 5f;
        [SerializeField] private ParticleSystem atiumGlow;
        [SerializeField] private float dodgeWindowMultiplier = 2f;
        [SerializeField] private float counterAttackWindow = 1.5f;
        
        private bool isAtiumActive = false;
        private float atiumEndTime = 0f;
        private Vector3 predictedEnemyPosition;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Atium;
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
        }
        
        public override void StartTapping()
        {
            if (currentStoredAmount <= 0f)
            {
                PlayEmptyEffect();
                return;
            }
            
            if (!isAtiumActive)
            {
                ActivateAtium();
            }
            
            isTapping = true;
            isStoring = false;
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
            
            if (Time.time >= atiumEndTime)
            {
                DeactivateAtium();
            }
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            
            DeactivateAtium();
            
            if (atiumGlow != null)
            {
                atiumGlow.Stop();
            }
        }
        
        private void ActivateAtium()
        {
            isAtiumActive = true;
            atiumEndTime = Time.time + futureSightDuration;
            
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.DodgeChance, dodgeWindowMultiplier - 1f);
            }
            
            if (atiumGlow != null)
            {
                atiumGlow.Play();
            }
            
            if (OnAtiumActivated != null)
            {
                OnAtiumActivated();
            }
        }
        
        private void DeactivateAtium()
        {
            isAtiumActive = false;
            
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ModifyStat(StatType.DodgeChance, 1f - dodgeWindowMultiplier);
            }
            
            if (OnAtiumDeactivated != null)
            {
                OnAtiumDeactivated();
            }
        }
        
        public Vector3 PredictEnemyMovement(Transform enemy, float predictionTime)
        {
            if (!isAtiumActive)
            {
                return enemy.position;
            }
            
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                Vector3 velocity = ai.GetVelocity();
                float timeSincePlayerSeen = Time.time - ai.GetLastSeenPlayerTime();
                
                if (timeSincePlayerSeen < 2f)
                {
                    predictedEnemyPosition = enemy.position + velocity * predictionTime * counterAttackWindow;
                    return predictedEnemyPosition;
                }
            }
            
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                predictedEnemyPosition = enemy.position + rb.velocity * predictionTime * counterAttackWindow;
                return predictedEnemyPosition;
            }
            
            return enemy.position;
        }
        
        public bool IsAtiumActive()
        {
            return isAtiumActive;
        }
        
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, atiumEndTime - Time.time);
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping)
            {
                return 0f;
            }
            
            float baseEffectiveness = currentStoredAmount / storageCapacity;
            float timeRemaining = GetRemainingTime() / futureSightDuration;
            return baseEffectiveness * (1f + timeRemaining);
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                float glowIntensity = isAtiumActive ? 1f : 0.3f;
                Color glowColor = new Color(1f, 0.9f, 0.5f, glowIntensity);
                visualIndicator.material.color = glowColor;
            }
        }
        
        public event System.Action OnAtiumActivated;
        public event System.Action OnAtiumDeactivated;
    }
}
