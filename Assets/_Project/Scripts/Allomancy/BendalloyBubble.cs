using UnityEngine;

namespace Mistborn.Allomancy
{
    public class BendalloyBubble : MonoBehaviour
    {
        [Header("Bubble")]
        [SerializeField] private float m_radius = 10f;
        [SerializeField] private float m_timeMult = 2f;
        [SerializeField] private KeyCode m_key = KeyCode.G;
        [SerializeField] private bool m_showEdge;

        private AllomancerController m_allomancer;
        private Renderer m_renderer;
        private bool m_isActive;

        public bool isActive => m_isActive;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            CreateVisuals();
        }

        private void CreateVisuals()
        {
            GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bubble.name = "BendalloyBubble";
            bubble.transform.SetParent(transform);
            bubble.transform.localPosition = Vector3.zero;
            bubble.transform.localScale = Vector3.one * m_radius * 2;
            Destroy(bubble.GetComponent<Collider>());

            m_renderer = bubble.GetComponent<Renderer>();
            m_renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            m_renderer.material.color = new Color(0.8f, 0.6f, 0.2f, 0.2f);
            m_renderer.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_key))
            {
                if (m_isActive) Stop();
                else Start();
            }

            if (m_isActive && m_allomancer.GetReserve(AllomanticMetal.Bendalloy).IsEmpty())
                Stop();
        }

        public void Start()
        {
            if (!m_allomancer.CanBurn(AllomanticMetal.Bendalloy)) return;
            m_isActive = true;
            m_allomancer.StartBurning(AllomanticMetal.Bendalloy);
            if (m_showEdge) m_renderer.enabled = true;
        }

        public void Stop()
        {
            m_isActive = false;
            m_allomancer.StopBurning(AllomanticMetal.Bendalloy);
            m_renderer.enabled = false;
        }

        public bool ContainsPoint(Vector3 point) =>
            Vector3.Distance(transform.position, point) <= m_radius;

        public float GetTimeMultiplier(Vector3 point)
        {
            if (!m_isActive || !ContainsPoint(point)) return 1f;
            return m_timeMult;
        }
    }
}
