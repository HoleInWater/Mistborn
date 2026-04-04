using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Coinshot's primary weapon system. Carries coins that can be individually Pushed.
/// Supports: single throw, shotgun spread, ground bounce, and rapid-fire trail.
/// </summary>
[PlayerComponent("Allomancy Support", order: 50)]
public class CoinPouch : MonoBehaviour
{
    [Header("Coin Inventory")]
    public int coinCount = 50;
    public int maxCoins = 100;
    public GameObject coinPrefab;

    [Header("Throw Settings")]
    public float throwForce = 5f;
    public float pushForce = 800f;
    public float coinMass = AllomancyPhysicsFormulas.CLIP_MASS; // 0.003 kg — official Shire Post Mint
    public float coinLifetime = 30f;
    public Transform throwPoint;

    [Header("Shotgun")]
    public int shotgunCoinCount = 5;
    public float shotgunSpreadAngle = 15f;
    public float shotgunCooldown = 1.5f;

    [Header("Coin Trail")]
    public float trailFireRate = 0.15f;
    public float trailPushForce = 600f;

    [Header("Coin Bounce")]
    public float bounceAngle = 60f;
    public float bouncePushForce = 500f;

    [Header("Recovery")]
    public float recoveryRadius = 2f;
    public LayerMask coinLayer;

    [Header("References")]
    public Camera playerCamera;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;

    private float lastShotgunTime;
    private float lastTrailTime;
    private bool trailActive = false;
    private List<GameObject> activeCoins = new List<GameObject>();

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (throwPoint == null) throwPoint = playerCamera != null ? playerCamera.transform : transform;

        coinLayer = LayerMask.GetMask("Metal");
    }

    void Update()
    {
        // Single coin throw + push: Mouse Right (only when burning Steel)
        bool isBurning = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        if (Input.GetMouseButtonDown(1) && coinCount > 0 && isBurning)
        {
            ThrowAndPushCoin();
        }

        // Shotgun: V key
        if (Input.GetKeyDown(Keybinds.CoinShotgun) && coinCount >= shotgunCoinCount
            && Time.time - lastShotgunTime > shotgunCooldown)
        {
            CoinShotgun();
        }

        // Coin bounce: C key
        if (Input.GetKeyDown(Keybinds.CoinBounce) && coinCount > 0)
        {
            CoinBounce();
        }

        // Coin trail: hold Z while moving
        trailActive = Input.GetKey(Keybinds.CoinTrail) && coinCount > 0;
        if (trailActive && Time.time - lastTrailTime > trailFireRate)
        {
            FireTrailCoin();
        }

        // Recovery: R key to pick up nearby coins
        if (Input.GetKeyDown(Keybinds.CoinRecover))
        {
            RecoverCoins();
        }

        // Clean up destroyed coins from tracking list
        activeCoins.RemoveAll(c => c == null);
    }

    // ── Single Throw + Push ──────────────────────────────────────────────

    void ThrowAndPushCoin()
    {
        if (coinPrefab == null || coinCount <= 0) return;
        if (playerCamera == null) return;
        coinCount--;

        Vector3 dir = playerCamera.transform.forward;
        Vector3 spawnPos = throwPoint.position + dir * 0.5f;

        GameObject coin = SpawnCoin(spawnPos);
        Rigidbody coinRb = coin.GetComponent<Rigidbody>();
        if (coinRb != null && playerRigidbody != null)
        {
            // Lore-accurate: F(a) = A × m1 × m2 / r² (PHYSICS-MATH-BOOK.md Section 2)
            // At close range (r≈1m), this is essentially A × m1 × m2
            float A = AllomancyPhysicsFormulas.A_CONSERVATIVE;
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float force = AllomancyPhysicsFormulas.CalculateAllomanticForce(
                A * flare, playerRigidbody.mass, coinMass, 1f);

            // Coin velocity from handbook Section 3: v = √(2 × F × d / m₂)
            float coinVel = AllomancyPhysicsFormulas.CalculateCoinVelocity(force, 2f, coinMass);
            coinRb.AddForce(dir * coinVel * coinMass, ForceMode.Impulse);

            // Newton's 3rd Law mass ratios
            float playerRatio, objectRatio;
            AllomancyPhysicsFormulas.CalculateMassRatios(
                playerRigidbody.mass, coinMass, false, out playerRatio, out objectRatio);
            playerRigidbody.AddForce(-dir * force * playerRatio, ForceMode.Impulse);
        }

        DrainMetal(1f);
        SoundManager.Instance?.PlayPushSound();
    }

    // ── Shotgun ──────────────────────────────────────────────────────────

    void CoinShotgun()
    {
        if (coinPrefab == null || coinCount < shotgunCoinCount) return;

        lastShotgunTime = Time.time;
        Vector3 baseDir = playerCamera.transform.forward;

        for (int i = 0; i < shotgunCoinCount; i++)
        {
            coinCount--;
            float hAngle = (i - (shotgunCoinCount - 1) * 0.5f) * shotgunSpreadAngle / shotgunCoinCount;
            float vAngle = Random.Range(-shotgunSpreadAngle * 0.3f, shotgunSpreadAngle * 0.3f);
            Vector3 spreadDir = Quaternion.Euler(vAngle, hAngle, 0) * baseDir;

            Vector3 spawnPos = throwPoint.position + spreadDir * 0.5f;
            GameObject coin = SpawnCoin(spawnPos);
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb != null)
                coinRb.AddForce(spreadDir * pushForce, ForceMode.Impulse);
        }

        // Big recoil
        if (playerRigidbody != null)
            playerRigidbody.AddForce(-baseDir * pushForce * 0.3f, ForceMode.Impulse);

        CameraShakeManager.Instance?.Shake(0.3f, 0.2f);
        DrainMetal(shotgunCoinCount * 0.5f);
        SoundManager.Instance?.PlayPushSound();
    }

    // ── Coin Bounce ──────────────────────────────────────────────────────

    void CoinBounce()
    {
        if (coinPrefab == null || coinCount <= 0) return;
        coinCount--;

        // Push coin at ground at an angle for vertical repositioning
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 bounceDir = Quaternion.Euler(-bounceAngle, 0, 0) * forward;

        Vector3 spawnPos = transform.position + Vector3.down * 0.5f + forward * 0.3f;
        GameObject coin = SpawnCoin(spawnPos);
        Rigidbody coinRb = coin.GetComponent<Rigidbody>();
        if (coinRb != null)
            coinRb.AddForce(bounceDir * bouncePushForce, ForceMode.Impulse);

        // Player gets launched upward (push off the coin below)
        if (playerRigidbody != null)
            playerRigidbody.AddForce(Vector3.up * bouncePushForce * 0.15f, ForceMode.Impulse);

        DrainMetal(1f);
        SoundManager.Instance?.PlayPushSound();
    }

    // ── Coin Trail ───────────────────────────────────────────────────────

    void FireTrailCoin()
    {
        if (coinPrefab == null || coinCount <= 0) return;
        coinCount--;
        lastTrailTime = Time.time;

        Vector3 dir = playerCamera.transform.forward;
        Vector3 spawnPos = throwPoint.position + dir * 0.3f;

        GameObject coin = SpawnCoin(spawnPos);
        Rigidbody coinRb = coin.GetComponent<Rigidbody>();
        if (coinRb != null)
            coinRb.AddForce(dir * trailPushForce, ForceMode.Impulse);

        DrainMetal(0.3f);
    }

    // ── Recovery ─────────────────────────────────────────────────────────

    void RecoverCoins()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, recoveryRadius, coinLayer);
        int recovered = 0;

        foreach (var col in nearby)
        {
            if (col.gameObject.CompareTag("Coin") && coinCount < maxCoins)
            {
                coinCount++;
                recovered++;
                Destroy(col.gameObject);
            }
        }

    }

    // ── Coin Spawning ────────────────────────────────────────────────────

    GameObject SpawnCoin(Vector3 position)
    {
        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
        coin.tag = "Coin";
        coin.layer = LayerMask.NameToLayer("Metal");

        Rigidbody rb = coin.GetComponent<Rigidbody>();
        if (rb == null) rb = coin.AddComponent<Rigidbody>();
        rb.mass = coinMass;

        if (coin.GetComponent<Collider>() == null)
            coin.AddComponent<SphereCollider>();

        if (coin.GetComponent<AllomanticTarget>() == null)
        {
            AllomanticTarget target = coin.AddComponent<AllomanticTarget>();
            target.canBePushed = true;
            target.canBePulled = true;
        }

        activeCoins.Add(coin);
        Destroy(coin, coinLifetime);

        return coin;
    }

    void DrainMetal(float amount)
    {
        if (allomancer != null)
            allomancer.DrainMetal(AllomancySkill.MetalType.Steel, amount);
    }

    // ── Public API ───────────────────────────────────────────────────────

    public int GetCoinCount() => coinCount;
    public int GetMaxCoins() => maxCoins;
    public void AddCoins(int amount) => coinCount = Mathf.Min(coinCount + amount, maxCoins);
    public int GetActiveCoinCount() => activeCoins.Count;
}
