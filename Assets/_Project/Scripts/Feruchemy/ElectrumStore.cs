using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class ElectrumStore : MonoBehaviour
    {
        [Header("Feruchemy - Electrum (Determination Storage)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        [SerializeField] private float determinationMultiplier = 0.04f;
        
        [Header("Psychological Effects")]
        [SerializeField] private float normalFearResistance = 0f;
        [SerializeField] private float maxFearResistance = 1f;
        [SerializeField] private float normalWillpower = 1f;
        [SerializeField] private float boostedWillpower = 3f;
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem determinationGlow;
        [SerializeField] private Color normalGlowColor = Color.white;
        [SerializeField] private Color boostedGlowColor = Color.yellow;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public float CurrentDeterminationBonus => currentStored * determinationMultiplier;
        public float CurrentFearResistance => isRetrieving ? Mathf.Lerp(normalFearResistance, maxFearResistance, currentStored / maxStored) : normalFearResistance;
        public float CurrentWillpower => isRetrieving ? Mathf.Lerp(normalWillpower, boostedWillpower, currentStored / maxStored) : normalWillpower;
        
        private PlayerStats playerStats;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
            
            if (determinationGlow != null)
                determinationGlow.Stop();
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
            UpdateDeterminationGlow(false);
        }
        
        public void StartRetrieving()
        {
            if (currentStored <= 0) return;
            isRetrieving = true;
            isStoring = false;
            UpdateDeterminationGlow(true);
        }
        
        public void StopRetrieving()
        {
            isRetrieving = false;
            UpdateDeterminationGlow(false);
        }
        
        private void Update()
        {
            if (isStoring && playerStats != null)
            {
                float mentalEnergyCost = storeRate * Time.deltaTime;
                if (playerStats.CurrentMentalEnergy >= mentalEnergyCost)
                {
                    currentStored = Mathf.Min(currentStored + storeRate * Time.deltaTime, maxStored);
                    playerStats.UseMentalEnergy(mentalEnergyCost);
                }
            }
            else if (isRetrieving)
            {
                float retrieved = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - retrieved, 0f);
            }
        }
        
        private void UpdateDeterminationGlow(bool active)
        {
            if (determinationGlow != null)
            {
                if (active && currentStored > 0)
                {
                    determinationGlow.Play();
                    var main = determinationGlow.main;
                    main.startColor = boostedGlowColor;
                }
                else
                {
                    determinationGlow.Stop();
                }
            }
        }
        
        public bool IsImmuneToFear()
        {
            return currentStored > maxStored * 0.7f;
        }
        
        public bool IsImmuneToDespair()
        {
            return currentStored > maxStored * 0.5f;
        }
        
        public float GetStaminaRegenBonus()
        {
            if (isRetrieving)
            {
                return 1f + (currentStored / maxStored) * 0.5f;
            }
            return 1f;
        }
        
        public float GetMentalFortitudeBonus()
        {
            if (isRetrieving)
            {
                return currentStored / maxStored * 0.5f;
            }
            return 0f;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForDetermination(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
    }
}
