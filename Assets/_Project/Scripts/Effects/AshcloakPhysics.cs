using UnityEngine;

/// <summary>
/// Ashcloak physics — the iconic tasseled cloak that billows dramatically
/// when the Ashwalker burns metals, pushes/pulls, or moves through the mists.
/// Uses cloth simulation parameters driven by Metallurgic activity.
/// </summary>
public class AshcloakPhysics : MonoBehaviour
{
    [Header("Cloth Reference")]
    public Cloth cloakCloth;

    [Header("Base Settings")]
    public float baseStiffness = 0.2f;
    public float baseDamping = 0.3f;
    public float baseExternalAcceleration = 0f;

    [Header("Metallurgy Effects")]
    [Tooltip("Wind force when Steel Pushing")]
    public float steelPushWindForce = 15f;
    [Tooltip("Inward pull when Iron Pulling")]
    public float ironPullSuckForce = 8f;
    [Tooltip("Extra billow when burning Pewter (body heat)")]
    public float pewterHeatBillow = 3f;
    [Tooltip("Dramatic flutter during Oraculum burn")]
    public float oraculumFlutter = 20f;

    [Header("Movement Effects")]
    public float sprintWindForce = 5f;
    public float fallWindForce = 10f;
    public float wallRunWindForce = 8f;

    [Header("References")]
    public Metallurgist metallurgist;
    public BasicPlayerMove playerMove;
    public Rigidbody playerRb;

    // Cached values
    private Vector3 currentWind = Vector3.zero;
    private float currentBillow = 0f;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponentInParent<Metallurgist>();
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

        // Metallurgic effects
        if (metallurgist != null)
        {
            // Steel Push — massive backward billow
            if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Steel))
            {
                currentWind -= transform.forward * steelPushWindForce;
                currentBillow += 5f;
            }

            // Iron Pull — cloak sucked forward
            if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Iron))
            {
                currentWind += transform.forward * ironPullSuckForce;
                currentBillow += 3f;
            }

            // Pewter — body heat causes gentle billow
            if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Pewter))
            {
                currentWind += Vector3.up * pewterHeatBillow;
                currentBillow += 1f;
            }

            // Oraculum — dramatic swirling flutter
            if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Oraculum))
            {
                float swirl = Mathf.Sin(Time.time * 5f) * oraculumFlutter;
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
