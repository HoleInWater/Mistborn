using UnityEngine;

/// <summary>
/// Coin physics behavior — coins stick on ground after landing (high friction),
/// embed in walls on hard impact, and make metallic sounds.
/// Attached automatically by CoinPouch when spawning coins.
/// </summary>
public class CoinPhysics : MonoBehaviour
{
    [Header("Settings")]
    public float stickFriction = 2f;
    public float embedSpeedThreshold = 30f;
    public float bounceReduction = 0.1f;
    public float sleepAfterSeconds = 3f;

    private Rigidbody rb;
    private float airTime = 0f;
    private bool hasLanded = false;
    private bool isEmbedded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Coin physics material — low bounce, medium friction
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                PhysicsMaterial coinMat = new PhysicsMaterial("CoinPhysics");
                coinMat.bounciness = bounceReduction;
                coinMat.dynamicFriction = stickFriction;
                coinMat.staticFriction = stickFriction;
                coinMat.bounceCombine = PhysicsMaterialCombine.Minimum;
                coinMat.frictionCombine = PhysicsMaterialCombine.Maximum;
                col.material = coinMat;
            }
        }
    }

    void Update()
    {
        if (hasLanded || rb == null) return;

        airTime += Time.deltaTime;

        // Auto-sleep coins that have been still for a while
        if (rb.linearVelocity.magnitude < 0.1f && airTime > 1f)
        {
            hasLanded = true;
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;

        float speed = collision.relativeVelocity.magnitude;

        // Embed in wall on hard impact
        if (speed > embedSpeedThreshold && !isEmbedded)
        {
            isEmbedded = true;
            rb.isKinematic = true;

            // Stick coin into surface
            if (collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                transform.position = contact.point + contact.normal * 0.005f;
                transform.rotation = Quaternion.LookRotation(-contact.normal);
            }

            SoundManager.Instance?.PlayImpactSound();
            return;
        }

        // Normal landing — increase damping to prevent bouncing
        if (!hasLanded)
        {
            hasLanded = true;
            rb.linearDamping = 3f;
            rb.angularDamping = 3f;
        }

        // Impact sound
        if (speed > 2f)
            SoundManager.Instance?.PlayImpactSound();
    }

    /// <summary>
    /// Called when coin is pushed/pulled again — reset physics state
    /// </summary>
    public void ResetForFlight()
    {
        hasLanded = false;
        isEmbedded = false;
        airTime = 0f;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
        }
    }
}
