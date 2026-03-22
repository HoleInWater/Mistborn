using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class BrassStore : MonoBehaviour
    {
        [Header("Feruchemy - Brass (Emotional Stability)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float warmthMultiplier = 0.03f;
        
        [Header("Temperature Effects")]
        [SerializeField] private float normalBodyTemp = 37f;
        [SerializeField] private float coldBodyTemp = 35f;
        [SerializeField] private float warmBodyTemp = 39f;
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem coldVaporParticles;
        [SerializeField] private ParticleSystem warmGlowParticles;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentWarmthBonus => currentStored * warmthMultiplier;
        public float CurrentBodyTemp => isRetrieving ? Mathf.Lerp(normalBodyTemp, warmBodyTemp, currentStored / maxStored) : 
                                          isStoring ? Mathf.Lerp(normalBodyTemp, coldBodyTemp, currentStored / maxStored) : normalBodyTemp;
        
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
            UpdateParticles();
        }
        
        public void StopStoring()
        {
            isStoring = false;
            UpdateParticles();
        }
        
        public void StartRetrieving()
        {
            if (currentStored <= 0) return;
            isRetrieving = true;
            isStoring = false;
            UpdateParticles();
        }
        
        public void StopRetrieving()
        {
            isRetrieving = false;
            UpdateParticles();
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
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
            }
        }
        
        private void UpdateParticles()
        {
            if (coldVaporParticles != null)
            {
                if (isStoring && currentStored > 0)
                    coldVaporParticles.Play();
                else
                    coldVaporParticles.Stop();
            }
            
            if (warmGlowParticles != null)
            {
                if (isRetrieving && currentStored > 0)
                    warmGlowParticles.Play();
                else
                    warmGlowParticles.Stop();
            }
        }
        
        public bool IsImmuneToEmotionalAllomancy()
        {
            return currentStored > maxStored * 0.5f;
        }
        
        public float GetEmotionalResistanceBonus()
        {
            return currentStored / maxStored * 0.8f;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForWarmth(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
        
        public void StoreCold(float amount)
        {
            currentStored = Mathf.Min(currentStored + amount, maxStored);
        }
    }
}
