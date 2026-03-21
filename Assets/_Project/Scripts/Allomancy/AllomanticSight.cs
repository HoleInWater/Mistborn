using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class AllomanticSight : MonoBehaviour
    {
        [Header("Sight")]
        [SerializeField] private Color m_color = Color.cyan;
        [SerializeField] private float m_range = 50f;
        [SerializeField] private KeyCode m_toggle = KeyCode.Tab;

        private List<AllomanticTarget> m_targets = new List<AllomanticTarget>();
        private bool m_active;
        private BlueLineRenderer m_lines;

        public bool isActive => m_active;

        private void Awake() => m_lines = GetComponent<BlueLineRenderer>();

        private void Update()
        {
            if (Input.GetKeyDown(m_toggle))
            {
                m_active = !m_active;
                if (!m_active) m_lines?.HideAllLines();
            }

            if (m_active)
            {
                FindTargets();
                DrawLines();
            }
        }

        public void FindTargets()
        {
            m_targets.Clear();
            foreach (Collider hit in Physics.OverlapSphere(transform.position, m_range))
                if (hit.TryGetComponent(out AllomanticTarget t)) m_targets.Add(t);
        }

        private void DrawLines()
        {
            if (m_lines != null)
                m_lines.UpdateLines(m_targets, transform.position);
            else
                foreach (var t in m_targets)
                    Debug.DrawLine(transform.position, t.transform.position, m_color);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = m_color;
            Gizmos.DrawWireSphere(transform.position, m_range);
        }
    }
}
