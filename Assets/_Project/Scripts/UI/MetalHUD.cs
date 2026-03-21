using UnityEngine;
using UnityEngine.UI;
using Mistborn.Allomancy;

namespace Mistborn.UI
{
    /// <summary>
    /// Displays metal reserve HUD elements for the player.
    /// Shows burn state and remaining amount for each metal.
    /// </summary>
    public class MetalHUD : MonoBehaviour
    {
        [Header("Metal Controller")]
        [SerializeField] private AllomancerController m_allomancer;
        
        [Header("Metal UI - Steel")]
        [SerializeField] private Image m_steelFillImage;
        [SerializeField] private Text m_steelText;
        [SerializeField] private Image m_steelBurningIndicator;
        
        [Header("Metal UI - Iron")]
        [SerializeField] private Image m_ironFillImage;
        [SerializeField] private Text m_ironText;
        [SerializeField] private Image m_ironBurningIndicator;
        
        [Header("Colors")]
        [SerializeField] private Color m_burningColor = Color.cyan;
        [SerializeField] private Color m_idleColor = Color.white;

        private void Start()
        {
            if (m_allomancer == null)
            {
                m_allomancer = FindObjectOfType<AllomancerController>();
            }
        }

        private void Update()
        {
            UpdateMetalDisplay(AllomanticMetal.Steel, m_steelFillImage, m_steelText, m_steelBurningIndicator);
            UpdateMetalDisplay(AllomanticMetal.Iron, m_ironFillImage, m_ironText, m_ironBurningIndicator);
        }

        private void UpdateMetalDisplay(AllomanticMetal metal, Image fillImage, Text text, Image burningIndicator)
        {
            if (m_allomancer == null) return;
            
            MetalReserve reserve = m_allomancer.GetReserve(metal);
            if (reserve == null) return;
            
            float percent = reserve.CurrentAmount / reserve.MaxAmount;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = percent;
            }
            
            if (text != null)
            {
                text.text = $"{reserve.CurrentAmount:F0}/{reserve.MaxAmount:F0}";
            }
            
            if (burningIndicator != null)
            {
                burningIndicator.enabled = reserve.IsBurning;
            }
            
            if (text != null)
            {
                text.color = reserve.IsBurning ? m_burningColor : m_idleColor;
            }
        }

        /// <summary>Updates display for a specific metal.</summary>
        public void UpdateDisplay(AllomanticMetal metal)
        {
            switch (metal)
            {
                case AllomanticMetal.Steel:
                    UpdateMetalDisplay(metal, m_steelFillImage, m_steelText, m_steelBurningIndicator);
                    break;
                case AllomanticMetal.Iron:
                    UpdateMetalDisplay(metal, m_ironFillImage, m_ironText, m_ironBurningIndicator);
                    break;
            }
        }
    }
}
