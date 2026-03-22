using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class AllomanticTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float maxTimerValue = 100f;
        [SerializeField] private float regenerationRate = 5f;
        [SerializeField] private float depletionRate = 10f;
        
        [Header("Metal Costs")]
        [SerializeField] private Dictionary<MetalType, float> metalCosts = new Dictionary<MetalType, float>();
        
        [Header("Visual Feedback")]
        [SerializeField] private UnityEngine.UI.Slider timerSlider;
        [SerializeField] private Gradient timerGradient;
        [SerializeField] private UnityEngine.UI.Image timerFillImage;
        
        private float currentTimerValue;
        private bool isRegenerating = false;
        private Allomancer allomancer;
        
        private void Awake()
        {
            allomancer = GetComponent<Allomancer>();
            InitializeMetalCosts();
        }
        
        private void Start()
        {
            currentTimerValue = maxTimerValue;
            
            if (timerSlider != null)
            {
                timerSlider.maxValue = maxTimerValue;
                timerSlider.value = currentTimerValue;
            }
        }
        
        private void InitializeMetalCosts()
        {
            metalCosts[MetalType.Iron] = 5f;
            metalCosts[MetalType.Steel] = 5f;
            metalCosts[MetalType.Tin] = 8f;
            metalCosts[MetalType.Pewter] = 8f;
            metalCosts[MetalType.Zinc] = 6f;
            metalCosts[MetalType.Brass] = 6f;
            metalCosts[MetalType.Copper] = 4f;
            metalCosts[MetalType.Bronze] = 4f;
            metalCosts[MetalType.Duralumin] = 15f;
            metalCosts[MetalType.Nicrosil] = 15f;
            metalCosts[MetalType.Chromium] = 10f;
            metalCosts[MetalType.Bendalloy] = 12f;
        }
        
        private void Update()
        {
            UpdateTimer();
            UpdateVisuals();
        }
        
        private void UpdateTimer()
        {
            if (isRegenerating && currentTimerValue < maxTimerValue)
            {
                currentTimerValue += regenerationRate * Time.deltaTime;
                currentTimerValue = Mathf.Min(currentTimerValue, maxTimerValue);
            }
            else if (!isRegenerating && currentTimerValue > 0)
            {
                currentTimerValue -= depletionRate * Time.deltaTime;
                currentTimerValue = Mathf.Max(currentTimerValue, 0f);
            }
            
            if (allomancer != null && !allomancer.IsAnyPowerActive())
            {
                isRegenerating = true;
            }
            else
            {
                isRegenerating = false;
            }
        }
        
        private void UpdateVisuals()
        {
            if (timerSlider != null)
            {
                timerSlider.value = currentTimerValue;
            }
            
            if (timerFillImage != null && timerGradient != null)
            {
                float t = currentTimerValue / maxTimerValue;
                timerFillImage.color = timerGradient.Evaluate(t);
            }
        }
        
        public bool CanUsePower(MetalType metal)
        {
            if (!metalCosts.ContainsKey(metal))
            {
                return false;
            }
            
            float cost = metalCosts[metal];
            return currentTimerValue >= cost;
        }
        
        public bool UsePower(MetalType metal)
        {
            if (!CanUsePower(metal))
            {
                return false;
            }
            
            float cost = metalCosts[metal];
            currentTimerValue -= cost;
            
            isRegenerating = false;
            
            if (OnPowerUsed != null)
            {
                OnPowerUsed(metal, cost);
            }
            
            return true;
        }
        
        public void SetMetalCost(MetalType metal, float cost)
        {
            if (metalCosts.ContainsKey(metal))
            {
                metalCosts[metal] = cost;
            }
            else
            {
                metalCosts.Add(metal, cost);
            }
        }
        
        public float GetMetalCost(MetalType metal)
        {
            if (metalCosts.ContainsKey(metal))
            {
                return metalCosts[metal];
            }
            
            return 0f;
        }
        
        public float GetCurrentTimerValue()
        {
            return currentTimerValue;
        }
        
        public float GetTimerPercentage()
        {
            return currentTimerValue / maxTimerValue;
        }
        
        public void SetMaxTimerValue(float value)
        {
            maxTimerValue = value;
            
            if (timerSlider != null)
            {
                timerSlider.maxValue = maxTimerValue;
            }
        }
        
        public void SetRegenerationRate(float rate)
        {
            regenerationRate = rate;
        }
        
        public void SetDepletionRate(float rate)
        {
            depletionRate = rate;
        }
        
        public void RefillTimer()
        {
            currentTimerValue = maxTimerValue;
        }
        
        public void DrainTimer(float amount)
        {
            currentTimerValue -= amount;
            currentTimerValue = Mathf.Max(currentTimerValue, 0f);
        }
        
        public bool IsRegenerating()
        {
            return isRegenerating;
        }
        
        public event System.Action<MetalType, float> OnPowerUsed;
    }
}
