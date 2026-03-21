using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    public class BlueLineRenderer : MonoBehaviour
    {
        [Header("Lines")]
        [SerializeField] private Color m_color = new Color(0.2f, 0.5f, 1f, 0.8f);
        [SerializeField] private float m_width = 0.05f;
        [SerializeField] private float m_maxDist = 50f;
        [SerializeField] private float m_minWidth = 0.02f;
        [SerializeField] private float m_maxWidth = 0.15f;
        [SerializeField] private float m_minMass = 50f;

        private const int POOL_SIZE = 20;
        private LineRenderer[] m_pool;
        private bool m_init;

        private void Awake() => Init();

        private void Init()
        {
            m_pool = new LineRenderer[POOL_SIZE];
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject obj = new GameObject($"Line_{i}");
                obj.transform.SetParent(transform);
                obj.SetActive(false);

                LineRenderer lr = obj.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = m_color;
                lr.endColor = m_color;
                lr.startWidth = m_width;
                lr.endWidth = m_width;
                lr.useWorldSpace = true;
                lr.receiveShadows = false;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_pool[i] = lr;
            }
            m_init = true;
        }

        public void UpdateLines(List<AllomanticTarget> targets, Vector3 center)
        {
            if (!m_init) return;
            HideAll();

            for (int i = 0; i < targets.Count && i < POOL_SIZE; i++)
            {
                if (targets[i] == null) continue;
                LineRenderer lr = m_pool[i];
                lr.gameObject.SetActive(true);
                lr.SetPosition(0, center);
                lr.SetPosition(1, targets[i].transform.position);

                float w = Mathf.Lerp(m_minWidth, m_maxWidth, Mathf.Clamp01(targets[i].MetalMass / m_minMass));
                lr.startWidth = w;
                lr.endWidth = w * 0.5f;

                float brightness = 1f - Vector3.Distance(center, targets[i].transform.position) / m_maxDist;
                Color c = new Color(m_color.r, m_color.g, m_color.b, Mathf.Lerp(0.3f, m_color.a, brightness));
                lr.startColor = c;
                lr.endColor = new Color(c.r, c.g, c.b, c.a * 0.5f);
            }
        }

        public void HideAll()
        {
            for (int i = 0; i < POOL_SIZE; i++)
                if (m_pool[i] != null) m_pool[i].gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < POOL_SIZE; i++)
                if (m_pool[i] != null) Destroy(m_pool[i].gameObject);
        }
    }
}
