using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mistborn.UI
{
    public class MetalHUD : MonoBehaviour
    {
        [Header("Metal Bars")]
        [SerializeField] private Image m_steelBar;
        [SerializeField] private Image m_ironBar;
        [SerializeField] private Image m_pewterBar;
        [SerializeField] private Image m_tinBar;

        [Header("References")]
        [SerializeField] private AllomancerController m_allomancer;

        private void Start()
        {
            if (m_allomancer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    m_allomancer = player.GetComponent<AllomancerController>();
            }
        }

        private void Update()
        {
            if (m_allomancer == null) return;
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            float steel = m_allomancer.GetReserve(AllomanticMetal.Steel);
            float iron = m_allomancer.GetReserve(AllomanticMetal.Iron);
            float pewter = m_allomancer.GetReserve(AllomanticMetal.Pewter);
            float tin = m_allomancer.GetReserve(AllomanticMetal.Tin);

            if (m_steelBar != null) m_steelBar.fillAmount = steel / 100f;
            if (m_ironBar != null) m_ironBar.fillAmount = iron / 100f;
            if (m_pewterBar != null) m_pewterBar.fillAmount = pewter / 100f;
            if (m_tinBar != null) m_tinBar.fillAmount = tin / 100f;
        }
    }
}
