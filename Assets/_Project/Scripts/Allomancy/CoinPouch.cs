// ============================================================
// FILE: CoinPouch.cs
// SYSTEM: Allomancy / Combat
// STATUS: PLANNED — Sprint 2
// AUTHOR: 
//
// PURPOSE:
//   Manages the player's coin pouch for Steelpush combat.
//   Allows shooting coins as projectiles.
//
// LORE:
//   Coinshots are known for their coin-shooting ability.
//   "A steel Misting is known as a Coinshot" — Coppermind
//
// TODO:
//   - Implement coin spawning
//   - Add throwing arc visualization
//   - Connect to Steelpush for launch force
//
// TODO (Team):
//   - Unlimited coins or limited ammo?
//   - Cooldown between shots?
//   - Coin respawn mechanic?
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class CoinPouch : MonoBehaviour
    {
        [Header("Coin Settings")]
        public GameObject coinPrefab;
        public Transform spawnPoint;
        public int maxCoins = 50;
        public int currentCoins = 50;
        
        [Header("Throw Settings")]
        public float throwForce = 100f;
        public float throwArc = 0.1f; // How much arc in trajectory
        public float cooldown = 0.1f;
        
        [Header("Auto-Collect")]
        public bool autoCollect = true;
        public float collectRadius = 5f;
        public float collectDelay = 2f;
        
        private float lastThrowTime;
        private SteelPushAbility steelPush;
        
        private void Start()
        {
            steelPush = GetComponent<SteelPushAbility>();
            if (spawnPoint == null)
            {
                spawnPoint = transform;
            }
        }
        
        public bool CanThrow()
        {
            return currentCoins > 0 && Time.time - lastThrowTime >= cooldown;
        }
        
        public void ThrowCoin(Vector3 direction)
        {
            if (!CanThrow()) return;
            
            // Spawn coin
            GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            
            if (coinRb != null)
            {
                // Add arc to throw
                Vector3 throwDir = direction.normalized;
                throwDir.y += throwArc;
                throwDir = throwDir.normalized;
                
                coinRb.AddForce(throwDir * throwForce, ForceMode.Impulse);
                
                // Let steel push add extra force
                if (steelPush != null && steelPush.GetComponent<AllomancerController>().CanBurn(AllomanticMetal.Steel))
                {
                    // Coin will be affected by Steelpush automatically
                }
            }
            
            currentCoins--;
            lastThrowTime = Time.time;
            
            // Schedule coin return
            if (autoCollect)
            {
                Invoke(nameof(TryCollectCoins), collectDelay);
            }
        }
        
        public void ThrowCoinAtTarget(Transform target)
        {
            if (target == null) return;
            Vector3 direction = (target.position - spawnPoint.position).normalized;
            ThrowCoin(direction);
        }
        
        private void TryCollectCoins()
        {
            // Find coins in radius and add back to pouch
            Collider[] coins = Physics.OverlapSphere(transform.position, collectRadius);
            foreach (Collider coin in coins)
            {
                if (coin.CompareTag("Coin") && currentCoins < maxCoins)
                {
                    currentCoins++;
                    Destroy(coin.gameObject);
                }
            }
        }
        
        public void AddCoins(int amount)
        {
            currentCoins = Mathf.Min(currentCoins + amount, maxCoins);
        }
    }
}
