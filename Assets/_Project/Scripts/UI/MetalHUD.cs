// ============================================================
// FILE: MetalHUD.cs
// SYSTEM: UI
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   UI controller for displaying metal reserves to the player.
//   Shows current burn state and remaining amount for active metals.
//
// DEPENDENCIES:
//   - AllomancerController
//   - Unity UI Slider components
//
// TODO:
//   - Hook up to Unity UI Slider components in TestArena scene
//   - Add visual indicators for burning state
//   - Implement flaring indicator
//
// TODO (Team):
//   - Design HUD layout
//   - Choose UI color scheme
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using Mistborn.Allomancy;

namespace Mistborn.UI
{
    public class MetalHUD : MonoBehaviour
    {
        [Header("Metal Reserve UI")]
        public AllomancerController allomancer;
        
        [Header("Steel UI")]
        public Image steelFillImage;
        public Text steelText;
        
        [Header("Iron UI")]
        public Image ironFillImage;
        public Text ironText;

        private void Start()
        {
            if (allomancer == null)
            {
                allomancer = FindObjectOfType<AllomancerController>();
            }
        }

        private void Update()
        {
            UpdateMetalUI(AllomanticMetal.Steel, steelFillImage, steelText);
            UpdateMetalUI(AllomanticMetal.Iron, ironFillImage, ironText);
        }

        private void UpdateMetalUI(AllomanticMetal metal, Image fillImage, Text text)
        {
            MetalReserve reserve = allomancer?.GetReserve(metal);
            if (reserve == null) return;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = reserve.currentAmount / reserve.maxAmount;
            }
            
            if (text != null)
            {
                text.text = $"{reserve.currentAmount:F0}/{reserve.maxAmount}";
                text.color = reserve.isBurning ? Color.cyan : Color.white;
            }
        }
    }
}
