using UnityEngine;

namespace Mistborn.Allomancy
{
    public class IronPull : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float m_pullForce = 50f;
        [SerializeField] private float m_range = 30f;
        [SerializeField] private LayerMask m_metalLayers;

        private AllomancerController m_allomancer;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) && m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Iron))
            {
                PullMetals();
            }
        }

        private void PullMetals()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, m_range, m_metalLayers);

            foreach (Collider target in targets)
            {
                AllomanticTarget metal = target.GetComponent<AllomanticTarget>();
                if (metal == null) continue;

                Rigidbody rb = target.GetComponent<Rigidbody>();
                if (rb == null) continue;

                Vector3 direction = (transform.position - target.transform.position).normalized;

                if (metal.IsAnchored)
                {
                    rb.AddForce(direction * m_pullForce * metal.MetalMass, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(direction * m_pullForce * metal.MetalMass, ForceMode.Impulse);
                }
            }
        }
    }
}
