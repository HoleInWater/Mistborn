using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    public class BrassSoothing : MonoBehaviour
    {
        [Header("Soothing")]
        [SerializeField] private float m_radius = 15f;
        [SerializeField] private float m_strength = 0.5f;
        [SerializeField] private KeyCode m_key = KeyCode.Z;

        private AllomancerController m_allomancer;
        private bool m_isActive;

        public bool isActive => m_isActive;

        private void Awake() => m_allomancer = GetComponent<AllomancerController>();

        private void Update()
        {
            if (Input.GetKeyDown(m_key)) StartSoothing();
            else if (Input.GetKeyUp(m_key)) StopSoothing();

            if (m_isActive && m_allomancer.GetReserve(AllomanticMetal.Brass).IsEmpty())
                StopSoothing();
        }

        public void StartSoothing()
        {
            if (!m_allomancer.CanBurn(AllomanticMetal.Brass)) return;
            m_isActive = true;
            m_allomancer.StartBurning(AllomanticMetal.Brass);
        }

        public void StopSoothing()
        {
            m_isActive = false;
            m_allomancer.StopBurning(AllomanticMetal.Brass);
        }

        public float GetEffectStrength(float distance)
        {
            if (distance > m_radius || !m_isActive) return 0f;
            return m_strength * (1f - distance / m_radius);
        }
    }
}
