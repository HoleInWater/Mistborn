using UnityEngine;

namespace MistbornGameplay
{
    public class StealthSystem : MonoBehaviour
    {
        [Header("Stealth Settings")]
        [SerializeField] private float baseStealthValue = 0f;
        [SerializeField] private float maxStealthValue = 100f;
        [SerializeField] private float stealthDecayRate = 5f;
        [SerializeField] private float stealthGainRate = 10f;
        
        [Header("Detection Ranges")]
        [SerializeField] private float detectionRangeClose = 5f;
        [SerializeField] private float detectionRangeMedium = 10f;
        [SerializeField] private float detectionRangeFar = 20f;
        
        [Header("Cover System")]
        [SerializeField] private bool isInCover = false;
        [SerializeField] private float coverStealthBonus = 50f;
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer stealthIndicator;
        [SerializeField] private Color stealthColorHigh = Color.green;
        [SerializeField] private Color stealthColorMedium = Color.yellow;
        [SerializeField] private Color stealthColorLow = Color.red;
        
        private float currentStealthValue;
        private bool isRevealed = false;
        private bool isCrouching = false;
        private PlayerStats playerStats;
        
        public float GetStealthValue()
        {
            return currentStealthValue;
        }
        
        public float GetStealthPercentage()
        {
            return currentStealthValue / maxStealthValue;
        }
        
        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }
        
        private void Start()
        {
            currentStealthValue = baseStealthValue;
            UpdateStealthVisuals();
        }
        
        private void Update()
        {
            UpdateStealthValue();
            HandleCrouchInput();
            UpdateStealthVisuals();
        }
        
        private void UpdateStealthValue()
        {
            if (isRevealed)
            {
                currentStealthValue -= stealthDecayRate * 2f * Time.deltaTime;
            }
            else if (isCrouching || isInCover)
            {
                currentStealthValue += stealthGainRate * Time.deltaTime;
            }
            else
            {
                currentStealthValue -= stealthDecayRate * Time.deltaTime;
            }
            
            currentStealthValue = Mathf.Clamp(currentStealthValue, 0f, maxStealthValue);
        }
        
        private void HandleCrouchInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                SetCrouching(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                SetCrouching(false);
            }
        }
        
        public void SetCrouching(bool crouching)
        {
            isCrouching = crouching;
            
            BasicPlayerMove playerMove = GetComponent<BasicPlayerMove>();
            if (playerMove != null)
            {
                playerMove.SetCrouching(crouching);
            }
        }
        
        public void EnterCover()
        {
            isInCover = true;
            currentStealthValue += coverStealthBonus;
            currentStealthValue = Mathf.Min(currentStealthValue, maxStealthValue);
        }
        
        public void ExitCover()
        {
            isInCover = false;
        }
        
        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
        }
        
        public bool IsRevealed()
        {
            return isRevealed;
        }
        
        public bool IsCrouching()
        {
            return isCrouching;
        }
        
        public bool IsInCover()
        {
            return isInCover;
        }
        
        public float GetDetectionChance(Transform detector)
        {
            if (currentStealthValue >= maxStealthValue)
            {
                return 0f;
            }
            
            float distance = Vector3.Distance(transform.position, detector.position);
            
            float detectionRange;
            if (distance <= detectionRangeClose)
            {
                detectionRange = detectionRangeClose;
            }
            else if (distance <= detectionRangeMedium)
            {
                detectionRange = detectionRangeMedium;
            }
            else
            {
                detectionRange = detectionRangeFar;
            }
            
            float distanceModifier = 1f - (distance / detectionRange);
            float stealthModifier = 1f - GetStealthPercentage();
            
            float detectionChance = distanceModifier * stealthModifier;
            
            if (isCrouching)
            {
                detectionChance *= 0.5f;
            }
            
            if (isInCover)
            {
                detectionChance *= 0.25f;
            }
            
            return detectionChance;
        }
        
        public void ModifyStealth(float amount)
        {
            currentStealthValue += amount;
            currentStealthValue = Mathf.Clamp(currentStealthValue, 0f, maxStealthValue);
        }
        
        private void UpdateStealthVisuals()
        {
            if (stealthIndicator == null)
            {
                return;
            }
            
            float stealthPercentage = GetStealthPercentage();
            
            Color indicatorColor;
            if (stealthPercentage > 0.66f)
            {
                indicatorColor = stealthColorHigh;
            }
            else if (stealthPercentage > 0.33f)
            {
                indicatorColor = stealthColorMedium;
            }
            else
            {
                indicatorColor = stealthColorLow;
            }
            
            stealthIndicator.color = indicatorColor;
        }
        
        public void ResetStealth()
        {
            currentStealthValue = baseStealthValue;
            isCrouching = false;
            isInCover = false;
            isRevealed = false;
        }
        
        public event System.Action OnStealthBroken;
        public event System.Action OnFullyStealthy;
    }
}
