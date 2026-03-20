// ============================================================
// FILE: MetalReserveUI.cs
// SYSTEM: UI
// STATUS: STUB — Basic implementation
// AUTHOR: 
//
// PURPOSE:
//   Displays metal reserve bars in the HUD.
//   Shows current amount, burn state, and depletion warning.
//
// TODO (AI Agent):
//   - Hook up to actual MetalReserve data
//   - Add depletion warning (flash when low)
//   - Add flaring indicator
//
// TODO (Team):
//   - Design HUD layout
//   - Choose colors for different metals
//   - Decide on bar style (fill vs segments)
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using Mistborn.Allomancy;

namespace Mistborn.UI
{
    public class MetalReserveUI : MonoBehaviour
    {
        [Header("UI References")]
        public Slider steelSlider;
        public Slider ironSlider;
        public Text steelText;
        public Text ironText;
        public Image steelFill;
        public Image ironFill;
        
        [Header("Colors")]
        public Color steelColor = new Color(0.3f, 0.5f, 1f); // Blue
        public Color ironColor = new Color(0.6f, 0.6f, 0.6f); // Grey
        public Color burningColor = new Color(0.2f, 0.8f, 1f); // Bright cyan
        public Color lowWarningColor = new Color(1f, 0.3f, 0.3f); // Red
        public float lowThreshold = 20f;
        
        private AllomancerController allomancer;
        
        private void Start()
        {
            allomancer = FindObjectOfType<AllomancerController>();
            if (allomancer == null)
            {
                Debug.LogWarning("MetalReserveUI: No AllomancerController found");
            }
            
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            if (steelSlider != null)
            {
                steelSlider.maxValue = 100f;
                steelSlider.value = 100f;
            }
            if (ironSlider != null)
            {
                ironSlider.maxValue = 100f;
                ironSlider.value = 100f;
            }
        }
        
        private void Update()
        {
            if (allomancer == null) return;
            
            UpdateMetalDisplay(AllomanticMetal.Steel, steelSlider, steelText, steelFill);
            UpdateMetalDisplay(AllomanticMetal.Iron, ironSlider, ironText, ironFill);
        }
        
        private void UpdateMetalDisplay(
            AllomanticMetal metal, 
            Slider slider, 
            Text text, 
            Image fillImage)
        {
            MetalReserve reserve = allomancer.GetReserve(metal);
            if (reserve == null) return;
            
            // Update slider
            if (slider != null)
            {
                slider.value = reserve.currentAmount;
            }
            
            // Update text
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(reserve.currentAmount)}";
                
                // Show burning indicator
                if (reserve.isBurning)
                {
                    text.text += " ●"; // Filled circle = burning
                }
            }
            
            // Update color based on state
            if (fillImage != null)
            {
                Color baseColor = (metal == AllomanticMetal.Steel) ? steelColor : ironColor;
                
                if (reserve.isBurning)
                {
                    fillImage.color = burningColor;
                }
                else if (reserve.currentAmount < lowThreshold)
                {
                    fillImage.color = lowWarningColor;
                }
                else
                {
                    fillImage.color = baseColor;
                }
            }
        }
    }
}
