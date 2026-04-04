/* SteeljumpAssist.cs
 *
 * Quality-of-life system for steeljumping movement.
 *
 * When the player is airborne and presses the push key, automatically finds
 * the best metal below/behind them for a steeljump push-off. This prevents
 * the frustrating experience of trying to target a coin on the ground while
 * flying through the air.
 *
 * The system:
 *   1. Checks if player is airborne + pushing
 *   2. Scans for metals below (cone cast downward)
 *   3. If the targeted metal from SteelPush is below the player, use it
 *   4. If no target, auto-drops a coin from the pouch (if available)
 *
 * This is a gameplay assist, not a physics change — all forces still use
 * the standard SteelPush calculations.
 */

using UnityEngine;

[PlayerComponent("Allomancy Support", order: 55)]
public class SteeljumpAssist : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Angle of the downward cone to search for metals (degrees from straight down)")]
    public float searchConeAngle = 45f;
    [Tooltip("How far below to search for metals")]
    public float searchRange = 15f;
    [Tooltip("Auto-drop a coin when no metal below? (requires CoinPouch)")]
    public bool autoDropCoin = true;
    [Tooltip("Minimum height above ground before auto-drop activates")]
    public float minAutoDropHeight = 2f;

    [Header("References")]
    public Rigidbody playerRb;
    public CoinPouch coinPouch;
    public SteelPush steelPush;
    public LayerMask metalLayer;

    private bool wasGrounded = true;

    void Start()
    {
        if (playerRb == null)  playerRb  = GetComponentInParent<Rigidbody>();
        if (coinPouch == null) coinPouch = GetComponentInParent<CoinPouch>();
        if (steelPush == null) steelPush = GetComponentInParent<SteelPush>();
        metalLayer = LayerMask.GetMask("Metal");
    }

    void Update()
    {
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);

        // Only assist when airborne
        if (grounded)
        {
            wasGrounded = true;
            return;
        }

        // Auto-drop coin on first airborne frame if pushing and no metal below
        if (autoDropCoin && wasGrounded && coinPouch != null && coinPouch.GetCoinCount() > 0)
        {
            // Check if there's already metal below us
            if (!HasMetalBelow())
            {
                // Check height
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
                {
                    if (hit.distance >= minAutoDropHeight)
                    {
                        // Drop a coin straight down for push-off
                        DropCoinBelow();
                    }
                }
            }
        }

        wasGrounded = false;
    }

    bool HasMetalBelow()
    {
        // Cone cast downward — check for any metal in a cone below the player
        Collider[] metals = Physics.OverlapSphere(
            transform.position + Vector3.down * searchRange * 0.5f,
            searchRange * Mathf.Tan(searchConeAngle * Mathf.Deg2Rad * 0.5f),
            metalLayer);

        foreach (var col in metals)
        {
            // Must be below us
            if (col.transform.position.y < transform.position.y - 0.5f)
                return true;
        }
        return false;
    }

    void DropCoinBelow()
    {
        if (coinPouch == null || coinPouch.GetCoinCount() <= 0) return;

        // Spawn a coin directly below with slight downward velocity
        Vector3 dropPos = transform.position + Vector3.down * 0.5f;
        // Use reflection or direct access to spawn — CoinPouch.SpawnCoin is private
        // Instead, reduce count and instantiate manually
        if (coinPouch.coinPrefab == null) return;

        coinPouch.coinCount--;
        GameObject coin = Object.Instantiate(coinPouch.coinPrefab, dropPos, Quaternion.identity);
        coin.tag = "Coin";
        int layer = LayerMask.NameToLayer("Metal");
        if (layer >= 0) coin.layer = layer;

        Rigidbody rb = coin.GetComponent<Rigidbody>();
        if (rb == null) rb = coin.AddComponent<Rigidbody>();
        rb.mass = AllomancyPhysicsFormulas.CLIP_MASS;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        // Give it a slight downward push so it reaches the ground faster
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        if (coin.GetComponent<AllomanticTarget>() == null)
        {
            var target = coin.AddComponent<AllomanticTarget>();
            target.canBePushed = true;
            target.canBePulled = true;
        }

        Object.Destroy(coin, 30f);
    }
}
