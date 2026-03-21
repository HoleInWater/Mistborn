using UnityEngine;
using Mistborn.Allomancy;

namespace Mistborn.World
{
    public class MetalPickup : MonoBehaviour
    {
        public enum PickupType
        {
            Coin,
            Brace,
            MetalFragment,
            Ingot
        }

        [Header("Metal Type")]
        [SerializeField] private AllomanticMetal m_metalType = AllomanticMetal.Iron;

        [Header("Pickup Settings")]
        [SerializeField] private PickupType m_pickupType = PickupType.Coin;
        [SerializeField] private int m_quantity = 1;
        [SerializeField] private float m_pickupRadius = 2f;
        [SerializeField] private float m_magnetSpeed = 10f;
        [SerializeField] private bool m_autoPickup = true;

        [Header("Visual")]
        [SerializeField] private GameObject m_pickupVFX;
        [SerializeField] private AudioClip m_pickupSound;
        [SerializeField] private float m_rotationSpeed = 45f;

        [Header("Physics")]
        [SerializeField] private float m_bounceForce = 5f;
        [SerializeField] private float m_friction = 0.95f;

        private Vector3 m_velocity;
        private bool m_isAttracted;
        private Transform m_player;
        private Renderer m_renderer;
        private AudioSource m_audioSource;

        private void Awake()
        {
            m_renderer = GetComponent<Renderer>();
            m_audioSource = GetComponent<AudioSource>();

            if (m_audioSource == null && m_pickupSound != null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
                m_audioSource.spatialBlend = 1f;
                m_audioSource.maxDistance = 10f;
            }
        }

        private void Start()
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y) + 0.5f;
            m_velocity = randomDir.normalized * m_bounceForce;
        }

        private void Update()
        {
            if (m_isAttracted)
            {
                AttractToPlayer();
            }
            else
            {
                ApplyPhysics();
                CheckForMagnetRange();
            }

            transform.Rotate(Vector3.up, m_rotationSpeed * Time.deltaTime);

            if (m_autoPickup && m_player != null)
            {
                float dist = Vector3.Distance(transform.position, m_player.position);
                if (dist < m_pickupRadius)
                {
                    Pickup();
                }
            }
        }

        private void ApplyPhysics()
        {
            m_velocity.y -= 20f * Time.deltaTime;

            transform.position += m_velocity * Time.deltaTime;

            if (transform.position.y < 0.5f)
            {
                transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                m_velocity.y = Mathf.Abs(m_velocity.y) * 0.6f;
                m_velocity.x *= m_friction;
                m_velocity.z *= m_friction;
            }
        }

        private void CheckForMagnetRange()
        {
            if (m_player == null) return;

            AllomancerController allomancer = m_player.GetComponent<AllomancerController>();
            if (allomancer == null || !allomancer.IsBurning(m_metalType)) return;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist < 8f)
            {
                m_isAttracted = true;
            }
        }

        private void AttractToPlayer()
        {
            if (m_player == null)
            {
                m_isAttracted = false;
                return;
            }

            Vector3 dir = (m_player.position - transform.position).normalized;
            m_velocity = dir * m_magnetSpeed;
            transform.position += m_velocity * Time.deltaTime;

            float dist = Vector3.Distance(transform.position, m_player.position);
            if (dist < m_pickupRadius)
            {
                Pickup();
            }
        }

        private void Pickup()
        {
            if (m_player == null) return;

            AllomancerController allomancer = m_player.GetComponent<AllomancerController>();
            if (allomancer != null)
            {
                float amount = GetMetalAmount();
                allomancer.AddMetal(m_metalType, amount);
            }

            if (m_pickupVFX != null)
            {
                Instantiate(m_pickupVFX, transform.position, Quaternion.identity);
            }

            if (m_audioSource != null && m_pickupSound != null)
            {
                m_audioSource.PlayOneShot(m_pickupSound);
            }

            Destroy(gameObject, 0.1f);
        }

        private float GetMetalAmount()
        {
            return m_pickupType switch
            {
                PickupType.Coin => 1f,
                PickupType.Brace => 3f,
                PickupType.MetalFragment => 5f,
                PickupType.Ingot => 15f,
                _ => 1f
            };
        }

        public void SetMetalType(AllomanticMetal metal)
        {
            m_metalType = metal;
        }

        public AllomanticMetal GetMetalType()
        {
            return m_metalType;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_pickupRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 8f);
        }
    }
}
