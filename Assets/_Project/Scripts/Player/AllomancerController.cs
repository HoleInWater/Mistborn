using UnityEngine;
using Mistborn.Allomancy;

namespace Mistborn.Player
{
    public class AllomancerController : MonoBehaviour
    {
        [Header("Metal Reserves")]
        [SerializeField] private float m_steelAmount = 100f;
        [SerializeField] private float m_ironAmount = 100f;
        [SerializeField] private float m_pewterAmount = 100f;
        [SerializeField] private float m_tinAmount = 100f;

        [Header("Drain Rates")]
        [SerializeField] private float m_steelDrainRate = 5f;
        [SerializeField] private float m_ironDrainRate = 5f;

        private bool m_isBurningSteel;
        private bool m_isBurningIron;
        private bool m_isBurningPewter;
        private bool m_isBurningTin;

        private void Update()
        {
            HandleSteelPush();
            HandleIronPull();
            DrainReserves();
        }

        private void HandleSteelPush()
        {
            if (Input.GetMouseButton(1) && m_steelAmount > 0)
            {
                m_isBurningSteel = true;
                // Steel push logic here
            }
            else
            {
                m_isBurningSteel = false;
            }
        }

        private void HandleIronPull()
        {
            if (Input.GetMouseButton(0) && m_ironAmount > 0)
            {
                m_isBurningIron = true;
                // Iron pull logic here
            }
            else
            {
                m_isBurningIron = false;
            }
        }

        private void DrainReserves()
        {
            if (m_isBurningSteel)
                m_steelAmount -= m_steelDrainRate * Time.deltaTime;

            if (m_isBurningIron)
                m_ironAmount -= m_ironDrainRate * Time.deltaTime;

            m_steelAmount = Mathf.Max(0, m_steelAmount);
            m_ironAmount = Mathf.Max(0, m_ironAmount);
        }

        public bool IsBurning(AllomanticMetal metal)
        {
            return metal switch
            {
                AllomanticMetal.Steel => m_isBurningSteel,
                AllomanticMetal.Iron => m_isBurningIron,
                AllomanticMetal.Pewter => m_isBurningPewter,
                AllomanticMetal.Tin => m_isBurningTin,
                _ => false
            };
        }

        public float GetReserve(AllomanticMetal metal)
        {
            return metal switch
            {
                AllomanticMetal.Steel => m_steelAmount,
                AllomanticMetal.Iron => m_ironAmount,
                AllomanticMetal.Pewter => m_pewterAmount,
                AllomanticMetal.Tin => m_tinAmount,
                _ => 0f
            };
        }
    }
}
