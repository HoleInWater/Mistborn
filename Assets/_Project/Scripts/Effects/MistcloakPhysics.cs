using UnityEngine;

/// <summary>
/// Mistcloak physics — the iconic tasseled cloak that billows dramatically
/// when the Mistborn burns metals, pushes/pulls, or moves through the mists.
/// Uses cloth simulation parameters driven by Allomantic activity.
/// </summary>
public class MistcloakPhysics : MonoBehaviour
{
    [Header("Cloth Reference")]
    public Cloth cloakCloth;

    [Header("Base Settings")]
    public float baseStiffness = 0.2f;
    public float baseDamping = 0.3f;
    public float baseExternalAcceleration = 0f;

    [Header("Allomancy Effects")]
    [Tooltip("Wind force when Steel Pushing")]
    public float steelPushWindForce = 15f;
    [Tooltip("Inward pull when Iron Pulling")]
    public float ironPullSuckForce = 8f;
    [Tooltip("Extra billow when burning Pewter (body heat)")]
    public float pewterHeatBillow = 3f;
    [Tooltip("Dramatic flutter during Atium burn")]
    public float atiumFlutter = 20f;

    [Header("Movement Effects")]
    public float sprintWindForce = 5f;
    public float fallWindForce = 10f;
    public float wallRunWindForce = 8f;

    [Header("References")]
    public Allomancer allomancer;
    public BasicPlayerMove playerMove;
    public Rigidbody playerRb;

    // Cached values
    private Vector3 currentWind = Vector3.zero;
    private float currentBillow = 0f;

    void Start()
    {
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (playerMove == null) playerMove = GetComponentInParent<BasicPlayerMove>();
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();

        if (cloakCloth == null)
            cloakCloth = GetComponent<Cloth>();
    }

    void Update()
    {
        if (cloakCloth == null) return;

        currentWind = Vector3.zero;
        currentBillow = 0f;

        // Movement wind
        if (playerRb != null)
        {
            Vector3 velocity = playerRb.linearVelocity;
            float speed = velocity.magnitude;

            // Horizontal movement blows cloak backward
            if (speed > 2f)
            {
                currentWind -= velocity.normalized * speed * 0.5f;
            }

            // Falling billows cloak upward
            if (velocity.y < -3f)
            {
                currentWind += Vector3.up * Mathf.Abs(velocity.y) * fallWindForce * 0.1f;
                currentBillow += 2f;
            }
        }

        // Sprint effect
        if (playerMove != null && playerMove.IsSprinting())
        {
            currentWind -= transform.forward * sprintWindForce;
        }

        // Allomantic effects
        if (allomancer != null)
        {
            // Steel Push — massive backward billow
            if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Steel))
            {
                currentWind -= transform.forward * steelPushWindForce;
                currentBillow += 5f;
            }

            // Iron Pull — cloak sucked forward
            if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Iron))
            {
                currentWind += transform.forward * ironPullSuckForce;
                currentBillow += 3f;
            }

            // Pewter — body heat causes gentle billow
            if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
            {
                currentWind += Vector3.up * pewterHeatBillow;
                currentBillow += 1f;
            }

            // Atium — dramatic swirling flutter
            if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Atium))
            {
                float swirl = Mathf.Sin(Time.time * 5f) * atiumFlutter;
                currentWind += transform.right * swirl;
                currentBillow += 8f;
            }

            // Flaring any metal adds extra billow
            if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
            {
                currentBillow += FlareManager.Instance.FlareMultiplier * 2f;
            }
        }

        // Apply to cloth
        cloakCloth.externalAcceleration = currentWind;
        cloakCloth.randomAcceleration = Vector3.one * (baseExternalAcceleration + currentBillow);
        cloakCloth.stretchingStiffness = baseStiffness;
        cloakCloth.damping = Mathf.Max(baseDamping - currentBillow * 0.02f, 0.05f);
    }

    /// <summary>
    /// Burst the cloak outward (Duralumin burst, landing impact, etc.)
    /// </summary>
    public void BurstCloak(Vector3 direction, float force)
    {
        if (cloakCloth == null) return;
        cloakCloth.externalAcceleration = direction * force;
    }
}
