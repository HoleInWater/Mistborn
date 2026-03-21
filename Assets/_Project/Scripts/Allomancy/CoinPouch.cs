using UnityEngine;

namespace Mistborn.Allomancy
{
    /// <summary>
    /// Manages the player's coin pouch for Steelpush combat.
    /// Allows throwing coins as projectiles that can be pushed.
    /// </summary>
    public class CoinPouch : MonoBehaviour
    {
        [Header("Coins")]
        [SerializeField] private GameObject m_coinPrefab;
        [SerializeField] private Transform m_spawnPoint;
        [SerializeField] private int m_maxCoins = 50;
        [SerializeField] private int m_currentCoins = 50;
        
        [Header("Throw")]
        [SerializeField] private float m_throwForce = 100f;
        [Range(-1f, 1f)]
        [SerializeField] private float m_throwArc = 0.1f;
        [Range(0.01f, 2f)]
        [SerializeField] private float m_cooldown = 0.1f;
        
        [Header("Auto-Collect")]
        [SerializeField] private bool m_autoCollect = true;
        [SerializeField] private float m_collectRadius = 5f;
        [SerializeField] private float m_collectDelay = 2f;

        private float m_lastThrowTime;
        private SteelPushAbility m_steelPush;

        public int currentCoins => m_currentCoins;
        public int maxCoins => m_maxCoins;

        private void Awake()
        {
            m_steelPush = GetComponent<SteelPushAbility>();
            if (m_spawnPoint == null)
            {
                m_spawnPoint = transform;
            }
        }

        public bool CanThrow() => m_currentCoins > 0 && Time.time - m_lastThrowTime >= m_cooldown;

        /// <summary>Throws a coin in the specified direction.</summary>
        public void ThrowCoin(Vector3 direction)
        {
            if (!CanThrow()) return;
            
            GameObject coin = Instantiate(m_coinPrefab, m_spawnPoint.position, Quaternion.identity);
            Rigidbody rb = coin.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                Vector3 throwDir = direction.normalized;
                throwDir.y += m_throwArc;
                throwDir = throwDir.normalized;
                rb.AddForce(throwDir * m_throwForce, ForceMode.Impulse);
            }
            
            m_currentCoins--;
            m_lastThrowTime = Time.time;
            
            if (m_autoCollect)
            {
                Invoke(nameof(TryCollectCoins), m_collectDelay);
            }
        }

        /// <summary>Throws a coin at the target.</summary>
        public void ThrowCoinAtTarget(Transform target)
        {
            if (target == null) return;
            Vector3 dir = (target.position - m_spawnPoint.position).normalized;
            ThrowCoin(dir);
        }

        private void TryCollectCoins()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_collectRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Coin") && m_currentCoins < m_maxCoins)
                {
                    m_currentCoins++;
                    Destroy(hit.gameObject);
                }
            }
        }

        public void AddCoins(int amount)
        {
            m_currentCoins = Mathf.Min(m_currentCoins + amount, m_maxCoins);
        }

        public void RefillCoins()
        {
            m_currentCoins = m_maxCoins;
        }
    }
}
