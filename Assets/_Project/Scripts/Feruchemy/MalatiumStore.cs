using UnityEngine;

namespace MistbornGameplay
{
    public class MalatiumStore : FeruchemyStore
    {
        [Header("Malatium Specific")]
        [SerializeField] private float revealDuration = 5f;
        [SerializeField] private ParticleSystem malatiumGlow;
        [SerializeField] private float revealRadius = 15f;
        [SerializeField] private LayerMask revealLayer;
        
        private bool isRevealActive = false;
        private float revealEndTime = 0f;
        
        protected override void InitializeStore()
        {
            base.InitializeStore();
            metalType = MetalType.Malatium;
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
            
            if (!isRevealActive)
            {
                ActivateMalatium();
            }
            
            isTapping = true;
            isStoring = false;
            
            float tapRate = CalculateTapRate();
            currentStoredAmount -= tapRate * Time.deltaTime;
            currentStoredAmount = Mathf.Max(currentStoredAmount, 0f);
            
            if (Time.time >= revealEndTime)
            {
                DeactivateMalatium();
            }
        }
        
        public override void StopUsing()
        {
            isStoring = false;
            isTapping = false;
            
            DeactivateMalatium();
            
            if (malatiumGlow != null)
            {
                malatiumGlow.Stop();
            }
        }
        
        private void ActivateMalatium()
        {
            isRevealActive = true;
            revealEndTime = Time.time + revealDuration;
            
            RevealHiddenObjects();
            
            if (malatiumGlow != null)
            {
                malatiumGlow.Play();
            }
            
            if (OnMalatiumActivated != null)
            {
                OnMalatiumActivated();
            }
        }
        
        private void DeactivateMalatium()
        {
            isRevealActive = false;
            HideRevealedObjects();
            
            if (OnMalatiumDeactivated != null)
            {
                OnMalatiumDeactivated();
            }
        }
        
        private void RevealHiddenObjects()
        {
            Collider[] hiddenObjects = Physics.OverlapSphere(transform.position, revealRadius, revealLayer);
            
            foreach (Collider obj in hiddenObjects)
            {
                HiddenObject hidden = obj.GetComponent<HiddenObject>();
                if (hidden != null)
                {
                    hidden.Reveal();
                }
                
                StealthSystem stealth = obj.GetComponent<StealthSystem>();
                if (stealth != null)
                {
                    stealth.SetRevealed(true);
                }
                
                TrapSystem trap = obj.GetComponent<TrapSystem>();
                if (trap != null)
                {
                    trap.SetRevealed(true);
                }
            }
        }
        
        private void HideRevealedObjects()
        {
            Collider[] revealedObjects = Physics.OverlapSphere(transform.position, revealRadius, revealLayer);
            
            foreach (Collider obj in revealedObjects)
            {
                HiddenObject hidden = obj.GetComponent<HiddenObject>();
                if (hidden != null)
                {
                    hidden.Hide();
                }
                
                StealthSystem stealth = obj.GetComponent<StealthSystem>();
                if (stealth != null)
                {
                    stealth.SetRevealed(false);
                }
                
                TrapSystem trap = obj.GetComponent<TrapSystem>();
                if (trap != null)
                {
                    trap.SetRevealed(false);
                }
            }
        }
        
        public bool IsMalatiumActive()
        {
            return isRevealActive;
        }
        
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, revealEndTime - Time.time);
        }
        
        public override float GetEffectiveness()
        {
            if (!isTapping)
            {
                return 0f;
            }
            
            float baseEffectiveness = currentStoredAmount / storageCapacity;
            float timeRemaining = GetRemainingTime() / revealDuration;
            return baseEffectiveness * timeRemaining;
        }
        
        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            
            if (visualIndicator != null)
            {
                float glowIntensity = isRevealActive ? 1f : 0.3f;
                Color glowColor = new Color(0.5f, 0.3f, 0.1f, glowIntensity);
                visualIndicator.material.color = glowColor;
            }
        }
        
        public event System.Action OnMalatiumActivated;
        public event System.Action OnMalatiumDeactivated;
    }
    
    public interface HiddenObject
    {
        void Reveal();
        void Hide();
    }
}
