using UnityEngine;
using UnityEngine.UI;
using Mistborn.Allomancy;

namespace Mistborn.UI
{
    public class MetalReserveUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider steelSlider;
        [SerializeField] private Slider ironSlider;
        [SerializeField] private Text steelText;
        [SerializeField] private Text ironText;
        [SerializeField] private Image steelFill;
        [SerializeField] private Image ironFill;
        
        [Header("Colors")]
        [SerializeField] private Color steelColor = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color ironColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color burningColor = new Color(0.2f, 0.8f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private float lowThreshold = 20f;
        
        private AllomancerController allomancer;
        
        private void Start()
        {
            allomancer = FindObjectOfType<AllomancerController>();
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
            UpdateDisplay(AllomanticMetal.Steel, steelSlider, steelText, steelFill);
            UpdateDisplay(AllomanticMetal.Iron, ironSlider, ironText, ironFill);
        }
        
        private void UpdateDisplay(AllomanticMetal metal, Slider slider, Text text, Image fill)
        {
            MetalReserve reserve = allomancer.GetReserve(metal);
            if (reserve == null) return;
            
            if (slider != null)
                slider.value = reserve.CurrentAmount;
            
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(reserve.CurrentAmount)}";
                text.text += reserve.IsBurning ? " ●" : "";
            }
            
            if (fill != null)
            {
                Color color = (metal == AllomanticMetal.Steel) ? steelColor : ironColor;
                if (reserve.IsBurning)
                    color = burningColor;
                else if (reserve.CurrentAmount < lowThreshold)
                    color = warningColor;
                fill.color = color;
            }
        }
    }
}
