using UnityEngine;
using UnityEngine.UI;

namespace MistbornGameplay
{
    public class LockpickMinigame : MonoBehaviour
    {
        [Header("Minigame Settings")]
        [SerializeField] private int totalPins = 5;
        [SerializeField] private float difficultyMultiplier = 1f;
        [SerializeField] private float pinSensitivity = 0.1f;
        
        [Header("Timing Windows")]
        [SerializeField] private float easyWindowSize = 0.3f;
        [SerializeField] private float mediumWindowSize = 0.15f;
        [SerializeField] private float hardWindowSize = 0.08f;
        
        [Header("Visual Settings")]
        [SerializeField] private Image lockpickImage;
        [SerializeField] private Image[] pinImages;
        [SerializeField] private Color pinDefaultColor = Color.gray;
        [SerializeField] private Color pinSuccessColor = Color.green;
        [SerializeField] private Color pinFailColor = Color.red;
        [SerializeField] private Color sweetSpotColor = Color.yellow;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pickMovementSound;
        [SerializeField] private AudioClip pinSuccessSound;
        [SerializeField] private AudioClip pinFailSound;
        [SerializeField] private AudioClip lockOpenSound;
        
        [Header("References")]
        [SerializeField] private GameObject minigameUI;
        
        private AudioSource audioSource;
        private bool isMinigameActive = false;
        private int currentPinIndex = 0;
        private float[] pinPositions;
        private float[] sweetSpotPositions;
        private float currentPickPosition = 0f;
        private int pinsSuccessfullyPicked = 0;
        private float pickMoveDirection = 1f;
        private float pickMoveSpeed = 0.5f;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (minigameUI != null)
            {
                minigameUI.SetActive(false);
            }
            
            InitializePins();
        }
        
        private void InitializePins()
        {
            pinPositions = new float[totalPins];
            sweetSpotPositions = new float[totalPins];
            
            for (int i = 0; i < totalPins; i++)
            {
                pinPositions[i] = 0f;
                sweetSpotPositions[i] = Random.Range(0.2f, 0.8f);
            }
        }
        
        public void StartMinigame()
        {
            if (isMinigameActive)
            {
                return;
            }
            
            isMinigameActive = true;
            currentPinIndex = 0;
            pinsSuccessfullyPicked = 0;
            currentPickPosition = 0f;
            pickMoveDirection = 1f;
            
            InitializePins();
            
            if (minigameUI != null)
            {
                minigameUI.SetActive(true);
            }
            
            UpdatePinVisuals();
            
            if (OnMinigameStarted != null)
            {
                OnMinigameStarted();
            }
        }
        
        public void StopMinigame()
        {
            isMinigameActive = false;
            
            if (minigameUI != null)
            {
                minigameUI.SetActive(false);
            }
            
            if (OnMinigameEnded != null)
            {
                OnMinigameEnded();
            }
        }
        
        private void Update()
        {
            if (!isMinigameActive)
            {
                return;
            }
            
            UpdatePickPosition();
            HandleInput();
            CheckPickPosition();
        }
        
        private void UpdatePickPosition()
        {
            currentPickPosition += pickMoveDirection * pickMoveSpeed * Time.deltaTime;
            
            if (currentPickPosition >= 1f)
            {
                currentPickPosition = 1f;
                pickMoveDirection = -1f;
            }
            else if (currentPickPosition <= 0f)
            {
                currentPickPosition = 0f;
                pickMoveDirection = 1f;
            }
            
            UpdatePickVisual();
        }
        
        private void UpdatePickVisual()
        {
            if (lockpickImage != null)
            {
                lockpickImage.rectTransform.anchorMin = new Vector2(currentPickPosition - 0.05f, 0.4f);
                lockpickImage.rectTransform.anchorMax = new Vector2(currentPickPosition + 0.05f, 0.6f);
            }
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AttemptPick();
            }
        }
        
        private void AttemptPick()
        {
            float sweetSpot = sweetSpotPositions[currentPinIndex];
            float distance = Mathf.Abs(currentPickPosition - sweetSpot);
            
            float windowSize = GetCurrentWindowSize();
            
            if (distance <= windowSize)
            {
                OnPinSuccess();
            }
            else
            {
                OnPinFail();
            }
        }
        
        private float GetCurrentWindowSize()
        {
            float adjustedDifficulty = difficultyMultiplier;
            
            switch (currentPinIndex)
            {
                case 0:
                case 1:
                    return easyWindowSize / adjustedDifficulty;
                case 2:
                case 3:
                    return mediumWindowSize / adjustedDifficulty;
                default:
                    return hardWindowSize / adjustedDifficulty;
            }
        }
        
        private void OnPinSuccess()
        {
            if (audioSource != null && pinSuccessSound != null)
            {
                audioSource.PlayOneShot(pinSuccessSound);
            }
            
            pinsSuccessfullyPicked++;
            
            if (pinImages != null && currentPinIndex < pinImages.Length)
            {
                pinImages[currentPinIndex].color = pinSuccessColor;
            }
            
            currentPinIndex++;
            
            if (currentPinIndex >= totalPins)
            {
                OnLockpickSuccess();
            }
        }
        
        private void OnPinFail()
        {
            if (audioSource != null && pinFailSound != null)
            {
                audioSource.PlayOneShot(pinFailSound);
            }
            
            if (pinImages != null && currentPinIndex < pinImages.Length)
            {
                pinImages[currentPinIndex].color = pinFailColor;
            }
            
            Invoke(nameof(ResetCurrentPin), 0.5f);
            
            if (OnPinFailed != null)
            {
                OnPinFailed(currentPinIndex);
            }
        }
        
        private void ResetCurrentPin()
        {
            if (pinImages != null && currentPinIndex < pinImages.Length)
            {
                pinImages[currentPinIndex].color = pinDefaultColor;
            }
        }
        
        private void UpdatePinVisuals()
        {
            if (pinImages == null)
            {
                return;
            }
            
            for (int i = 0; i < pinImages.Length; i++)
            {
                if (i < currentPinIndex)
                {
                    pinImages[i].color = pinSuccessColor;
                }
                else if (i == currentPinIndex)
                {
                    pinImages[i].color = sweetSpotColor;
                }
                else
                {
                    pinImages[i].color = pinDefaultColor;
                }
            }
        }
        
        private void OnLockpickSuccess()
        {
            if (audioSource != null && lockOpenSound != null)
            {
                audioSource.PlayOneShot(lockOpenSound);
            }
            
            if (OnLockpickCompleted != null)
            {
                OnLockpickCompleted(true);
            }
            
            StopMinigame();
        }
        
        public bool IsMinigameActive()
        {
            return isMinigameActive;
        }
        
        public int GetCurrentPinIndex()
        {
            return currentPinIndex;
        }
        
        public float GetPickPosition()
        {
            return currentPickPosition;
        }
        
        public void SetDifficulty(float difficulty)
        {
            difficultyMultiplier = difficulty;
            pickMoveSpeed = 0.5f * difficulty;
        }
        
        public void SetTotalPins(int pins)
        {
            totalPins = pins;
            InitializePins();
        }
        
        public event System.Action OnMinigameStarted;
        public event System.Action OnMinigameEnded;
        public event System.Action<bool> OnLockpickCompleted;
        public event System.Action<int> OnPinFailed;
    }
}
