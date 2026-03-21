using UnityEngine;

namespace Mistborn.Allomancy
{
    public class SteelPush : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float m_pushForce = 50f;
        [SerializeField] private float m_range = 30f;
        [SerializeField] private LayerMask m_metalLayers;

        private AllomancerController m_allomancer;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(1) && m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Steel))
            {
                PushMetals();
            }
        }

        private void PushMetals()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, m_range, m_metalLayers);

            foreach (Collider target in targets)
            {
                AllomanticTarget metal = target.GetComponent<AllomanticTarget>();
                if (metal == null) continue;

                Rigidbody rb = target.GetComponent<Rigidbody>();
                if (rb == null) continue;

                Vector3 direction = (target.transform.position - transform.position).normalized;

                if (metal.IsAnchored)
                {
                    rb.AddForce(-direction * m_pushForce * metal.MetalMass, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(direction * m_pushForce * metal.MetalMass, ForceMode.Impulse);
                }
            }
        }
    }
}
