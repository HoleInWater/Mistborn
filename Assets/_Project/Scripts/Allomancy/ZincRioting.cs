using UnityEngine;

namespace Mistborn.Allomancy
{
    public class ZincRioting : MonoBehaviour
    {
        [Header("Rioting")]
        [SerializeField] private float m_radius = 15f;
        [SerializeField] private float m_strength = 0.5f;
        [SerializeField] private KeyCode m_key = KeyCode.X;

        private AllomancerController m_allomancer;
        private bool m_isActive;

        public bool isActive => m_isActive;

        private void Awake() => m_allomancer = GetComponent<AllomancerController>();

        private void Update()
        {
            if (Input.GetKeyDown(m_key)) StartRioting();
            else if (Input.GetKeyUp(m_key)) StopRioting();

            if (m_isActive && m_allomancer.GetReserve(AllomanticMetal.Zinc).IsEmpty())
                StopRioting();
        }

        public void StartRioting()
        {
            if (!m_allomancer.CanBurn(AllomanticMetal.Zinc)) return;
            m_isActive = true;
            m_allomancer.StartBurning(AllomanticMetal.Zinc);
        }

        public void StopRioting()
        {
            m_isActive = false;
            m_allomancer.StopBurning(AllomanticMetal.Zinc);
        }

        public float GetEffectStrength(float distance)
        {
            if (distance > m_radius || !m_isActive) return 0f;
            return m_strength * (1f - distance / m_radius);
        }
    }
}
