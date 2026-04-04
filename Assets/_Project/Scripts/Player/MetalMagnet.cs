using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Passive Iron Pull magnet effect — automatically pulls nearby loose metal objects
/// toward the player when burning Iron. Uses lore-accurate F = A × m1 × m2 / r²
/// from PHYSICS-MATH-BOOK.md. Light objects (coins, pennies) drift toward the player;
/// heavy objects only move if the force exceeds their friction.
/// </summary>
[PlayerComponent("Metallurgy Support", order: 30)]
public class MetalMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetRange = 8f;
    public float magnetStrength = 200f;
    public float maxPullSpeed = 5f;
    public float pickupDistance = 1.2f;
    public float updateInterval = 0.2f;

    [Header("Filtering")]
    public float maxObjectMass = 2f;
    public LayerMask metalLayer;

    [Header("References")]
    public Metallurgist metallurgist;
    public Rigidbody playerRb;

    private float updateTimer;
    private List<Rigidbody> nearbyMetals = new List<Rigidbody>();
    private Inventory inventory;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
        metalLayer = LayerMask.GetMask("Metal");
    }

    void FixedUpdate()
    {
        // Only active when burning Iron
        if (metallurgist == null || !metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Iron))
            return;

        // Periodic scan for nearby metals
        updateTimer -= Time.fixedDeltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanForMetals();
        }

        // Apply gentle pull to each nearby metal
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float playerMass = playerRb != null ? playerRb.mass : 80f;

        for (int i = nearbyMetals.Count - 1; i >= 0; i--)
        {
            Rigidbody metalRb = nearbyMetals[i];
            if (metalRb == null) { nearbyMetals.RemoveAt(i); continue; }

            float dist = Vector3.Distance(transform.position, metalRb.position);

            // Auto-pickup at close range
            if (dist < pickupDistance)
            {
                TryPickup(metalRb.gameObject);
                nearbyMetals.RemoveAt(i);
                continue;
            }

            // Lore-accurate pull: F = A × m1 × m2 / r²
            float force = MetallurgyPhysicsFormulas.CalculateMetallurgicForce(
                magnetStrength * flare, playerMass, metalRb.mass, dist);

            // Only pull light objects (coins, pennies, small metal items)
            if (metalRb.mass > maxObjectMass) continue;

            Vector3 pullDir = (transform.position - metalRb.position).normalized;

            // Clamp velocity to prevent metal rockets
            if (metalRb.linearVelocity.magnitude < maxPullSpeed)
                metalRb.AddForce(pullDir * force, ForceMode.Force);
        }
    }

    void ScanForMetals()
    {
        nearbyMetals.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, magnetRange, metalLayer);

        foreach (var col in hits)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb == playerRb) continue;
            if (rb.isKinematic) continue;
            if (rb.mass > maxObjectMass) continue;

            nearbyMetals.Add(rb);
        }
    }

    void TryPickup(GameObject metalObj)
    {
        // Try to add to coin pouch
        CoinPouch pouch = GetComponent<CoinPouch>();
        if (pouch != null && metalObj.CompareTag("Coin"))
        {
            pouch.AddCoins(1);
            Destroy(metalObj);
            return;
        }

        // Metal pickup — refill reserves directly
        MetalPickup pickup = metalObj.GetComponent<MetalPickup>();
        if (pickup != null && metallurgist != null)
        {
            metallurgist.RefillMetal(pickup.metalType, pickup.metalAmount);
            Destroy(metalObj);
            return;
        }
    }

    public int GetNearbyMetalCount() => nearbyMetals.Count;
}
