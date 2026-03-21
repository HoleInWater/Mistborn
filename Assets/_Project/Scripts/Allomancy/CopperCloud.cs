using UnityEngine;

namespace Mistborn.Allomancy
{
    public class CopperCloud : MonoBehaviour
    {
        [Header("Cloud")]
        [SerializeField] private float m_radius = 15f;
        [SerializeField] private KeyCode m_key = KeyCode.C;
        [SerializeField] private bool m_showEdge;

        private AllomancerController m_allomancer;
        private Renderer m_renderer;
        private bool m_isActive;

        public bool isActive => m_isActive;
        public float radius => m_radius;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            CreateVisuals();
        }

        private void CreateVisuals()
        {
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cloud.name = "CopperCloudVisual";
            cloud.transform.SetParent(transform);
            cloud.transform.localPosition = Vector3.zero;
            cloud.transform.localScale = Vector3.one * m_radius * 2;
            Destroy(cloud.GetComponent<Collider>());

            m_renderer = cloud.GetComponent<Renderer>();
            m_renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            m_renderer.material.color = new Color(0.5f, 0.3f, 0.2f, 0.1f);
            m_renderer.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_key))
            {
                if (m_isActive) Stop();
                else Start();
            }

            if (m_isActive && m_allomancer.GetReserve(AllomanticMetal.Copper).IsEmpty())
                Stop();
        }

        public void Start()
        {
            if (!m_allomancer.CanBurn(AllomanticMetal.Copper)) return;
            m_isActive = true;
            m_allomancer.StartBurning(AllomanticMetal.Copper);
            if (m_showEdge) m_renderer.enabled = true;
        }

        public void Stop()
        {
            m_isActive = false;
            m_allomancer.StopBurning(AllomanticMetal.Copper);
            m_renderer.enabled = false;
        }

        public bool ContainsPoint(Vector3 point) =>
            Vector3.Distance(transform.position, point) <= m_radius;

        public static bool IsAnyActiveAt(Vector3 point)
        {
            foreach (CopperCloud cloud in Object.FindObjectsOfType<CopperCloud>())
                if (cloud.m_isActive && cloud.ContainsPoint(point)) return true;
            return false;
        }
    }
}
