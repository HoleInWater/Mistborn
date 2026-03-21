using UnityEngine;

namespace Mistborn.Player
{
    public class SteelpushAssistedJump : MonoBehaviour
    {
        [Header("Jump")]
        [SerializeField] private float m_baseForce = 15f;
        [SerializeField] private float m_maxForce = 40f;
        [SerializeField] private float m_minMass = 50f;
        [SerializeField] private float m_detectionRange = 10f;

        [Header("Charge")]
        [SerializeField] private float m_holdDuration = 0.5f;
        [SerializeField] private float m_maxCharge = 2f;
        [SerializeField] private float m_chargeBoost = 1.5f;

        [Header("Input")]
        [SerializeField] private KeyCode m_key = KeyCode.Space;
        [SerializeField] private LayerMask m_metalLayer;

        private AllomancerController m_allomancer;
        private Rigidbody m_rb;
        private bool m_charging;
        private float m_chargeStart;
        private AllomanticTarget m_anchor;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (m_allomancer == null || !m_allomancer.CanBurn(AllomanticMetal.Steel))
                return;

            m_anchor = FindAnchor();

            if (Input.GetKeyDown(m_key) && m_anchor != null && IsGrounded())
                StartCharge();

            if (Input.GetKeyUp(m_key) && m_charging)
                Launch();

            if (m_charging && Time.time - m_chargeStart >= m_maxCharge)
                Launch();
        }

        private AllomanticTarget FindAnchor()
        {
            foreach (RaycastHit hit in Physics.RaycastAll(transform.position, Vector3.down, m_detectionRange, m_metalLayer))
            {
                if (hit.collider.TryGetComponent(out AllomanticTarget target) && 
                    target.IsAnchored && 
                    target.MetalMass >= m_minMass)
                {
                    return target;
                }
            }
            return null;
        }

        private void StartCharge()
        {
            m_charging = true;
            m_chargeStart = Time.time;
            m_allomancer.StartBurning(AllomanticMetal.Steel);
        }

        private void Launch()
        {
            if (!m_charging || m_anchor == null) return;
            m_charging = false;

            float chargeTime = Time.time - m_chargeStart;
            float multiplier = Mathf.Clamp(chargeTime / m_holdDuration, 1f, m_chargeBoost);
            float massMult = Mathf.Clamp(m_anchor.MetalMass / 100f, 0.5f, 2f);
            float force = Mathf.Min(m_baseForce * multiplier * massMult, m_maxForce);

            m_rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            m_allomancer.StopBurning(AllomanticMetal.Steel);
        }

        private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
