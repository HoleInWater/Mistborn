using UnityEngine;

namespace Mistborn.Allomancy
{
    public class AllomanticTarget : MonoBehaviour
    {
        [Header("Metal Properties")]
        [SerializeField] private AllomanticMetal m_metalType = AllomanticMetal.Iron;
        [SerializeField] private float m_metalMass = 1f;
        [SerializeField] private bool m_isAnchored = false;

        public AllomanticMetal MetalType => m_metalType;
        public float MetalMass => m_metalMass;
        public bool IsAnchored => m_isAnchored;

        public void SetMetalMass(float mass)
        {
            m_metalMass = mass;
        }

        public void SetAnchored(bool anchored)
        {
            m_isAnchored = anchored;
        }
    }
}
