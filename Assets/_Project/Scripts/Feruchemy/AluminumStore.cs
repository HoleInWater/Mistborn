using UnityEngine;

namespace MistbornGame.Feruchemy
{
    public class AluminumStore : MonoBehaviour
    {
        [Header("Feruchemy - Aluminum (Identity Storage)")]
        [SerializeField] private float storeRate = 10f;
        [SerializeField] private float retrieveRate = 10f;
        [SerializeField] private float maxStored = 100f;
        
        [Header("Identity Effects")]
        [SerializeField] private bool useIdentityMask = false;
        [SerializeField] private string storedIdentityName = "Unknown";
        [SerializeField] private Sprite storedIdentityIcon;
        
        [Header("World Hopping")]
        [SerializeField] private float identityRestorationRate = 10f;
        
        private float currentStored = 0f;
        private bool isStoring = false;
        private bool isRetrieving = false;
        
        private string originalIdentityName;
        private Sprite originalIdentityIcon;
        
        public float CurrentStored => currentStored;
        public float MaxStored => maxStored;
        public bool IsIdentityMasked => useIdentityMask;
        public string CurrentIdentityName => useIdentityMask ? storedIdentityName : originalIdentityName;
        public Sprite CurrentIdentityIcon => useIdentityMask ? storedIdentityIcon : originalIdentityIcon;
        
        private PlayerStats playerStats;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
            
            originalIdentityName = gameObject.name;
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
            RestoreOriginalIdentity();
        }
        
        private void Update()
        {
            if (isStoring)
            {
                float mentalEnergyCost = storeRate * Time.deltaTime;
                if (playerStats != null && playerStats.CurrentMentalEnergy >= mentalEnergyCost)
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
        
        public void StoreIdentity()
        {
            StartStoring();
        }
        
        public void ApplyMaskedIdentity()
        {
            useIdentityMask = true;
            gameObject.name = storedIdentityName;
            var nameTag = GetComponentInChildren<TMPro.TMP_Text>();
            if (nameTag != null)
            {
                nameTag.text = storedIdentityName;
            }
        }
        
        public void RestoreOriginalIdentity()
        {
            useIdentityMask = false;
            gameObject.name = originalIdentityName;
            var nameTag = GetComponentInChildren<TMPro.TMP_Text>();
            if (nameTag != null)
            {
                nameTag.text = originalIdentityName;
            }
        }
        
        public void SetStoredIdentity(string name, Sprite icon)
        {
            storedIdentityName = name;
            storedIdentityIcon = icon;
        }
        
        public bool IsDetectableAsIdentity(float detectionSkill)
        {
            if (!useIdentityMask) return true;
            float detectionChance = detectionSkill * 0.1f;
            return Random.value < detectionChance;
        }
        
        public void SetStoredAmount(float amount)
        {
            currentStored = Mathf.Clamp(amount, 0f, maxStored);
        }
        
        public void TapForIdentity(float duration)
        {
            StartRetrieving();
            Invoke(nameof(StopRetrieving), duration);
        }
        
        public void FullyStoreIdentity()
        {
            StartStoring();
            while (currentStored < maxStored)
            {
                float amount = storeRate * Time.deltaTime;
                currentStored = Mathf.Min(currentStored + amount, maxStored);
            }
            StopStoring();
            ApplyMaskedIdentity();
        }
        
        public void FullyRestoreIdentity()
        {
            StartRetrieving();
            while (currentStored > 0)
            {
                float amount = retrieveRate * Time.deltaTime;
                currentStored = Mathf.Max(currentStored - amount, 0f);
            }
            StopRetrieving();
            RestoreOriginalIdentity();
        }
    }
}
