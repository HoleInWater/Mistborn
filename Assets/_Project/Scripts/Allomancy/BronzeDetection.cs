using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    public class BronzeDetection : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float m_range = 30f;
        [SerializeField] private float m_interval = 0.5f;
        [SerializeField] private bool m_canIdentify;

        private float m_timer;
        private List<Detected> m_detected = new List<Detected>();

        public struct Detected
        {
            public GameObject target;
            public AllomanticMetal? metal;
            public float distance;
            public bool inCloud;
        }

        private void Update()
        {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0)
            {
                m_timer = m_interval;
                Scan();
            }
        }

        private void Scan()
        {
            m_detected.Clear();

            foreach (AllomancerController allomancer in Object.FindObjectsOfType<AllomancerController>())
            {
                if (allomancer.gameObject == gameObject) continue;

                float dist = Vector3.Distance(transform.position, allomancer.transform.position);
                if (dist > m_range) continue;

                AllomanticMetal? metal = null;
                foreach (MetalReserve reserve in allomancer.Reserves)
                {
                    if (reserve.IsBurning)
                    {
                        metal = reserve.MetalType;
                        if (!m_canIdentify) break;
                    }
                }

                Detected d = new Detected
                {
                    target = allomancer.gameObject,
                    metal = metal,
                    distance = dist,
                    inCloud = CopperCloud.IsAnyActiveAt(allomancer.transform.position)
                };

                m_detected.Add(d);
                if (!d.inCloud) OnDetected?.Invoke(d);
            }
        }

        public List<Detected> GetVisible()
        {
            List<Detected> visible = new List<Detected>();
            foreach (var d in m_detected)
                if (!d.inCloud) visible.Add(d);
            return visible;
        }

        public bool HasAnyVisible()
        {
            foreach (var d in m_detected)
                if (!d.inCloud) return true;
            return false;
        }

        public event System.Action<Detected> OnDetected;
    }
}
