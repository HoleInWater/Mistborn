using UnityEngine;

/// <summary>
/// Pewter-enhanced ground slam. Press Ctrl while airborne to slam downward.
/// Deals AOE damage on impact, enhanced by Pewter burning.
/// Uses PHYSICS-MATH-BOOK.md Section 8 for Pewter strength scaling.
/// </summary>
public class GroundSlam : MonoBehaviour
{
    [Header("Slam Settings")]
    public float slamForce = 30f;
    public float minHeightToSlam = 3f;
    public float impactRadius = 4f;
    public float baseSlamDamage = 40f;
    public float impactKnockback = 15f;

    [Header("Pewter Enhancement")]
    public float pewterDamageMultiplier = 2.5f;
    public float pewterRadiusMultiplier = 1.5f;

    [Header("Effects")]
    public GameObject impactEffectPrefab;

    [Header("References")]
    public Rigidbody playerRb;
    public BasicPlayerMove playerMove;
    public Allomancer allomancer;
    public Animator animator;
    public LayerMask groundLayer;

    private bool isSlamming = false;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Ctrl while airborne
        if (Input.GetKeyDown(Keybinds.Crouch) && !playerMove.IsGrounded() && !isSlamming)
        {
            float height = GetHeightAboveGround();
            if (height >= minHeightToSlam)
                StartSlam();
        }

        // Detect landing during slam
        if (isSlamming && playerMove.IsGrounded())
            OnSlamImpact();
    }

    void StartSlam()
    {
        isSlamming = true;

        // Pewter makes the slam faster
        float force = slamForce;
        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
        {
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float P = Mathf.Clamp01(flare / 2.5f);
            force *= AllomancyPhysicsFormulas.CalculatePewterStrength(1f, 1f, P);
        }

        playerRb.linearVelocity = new Vector3(0, -force, 0);
        animator?.SetTrigger("GroundSlam");
    }

    void OnSlamImpact()
    {
        isSlamming = false;

        bool pewterActive = allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter);
        float damage = baseSlamDamage * (pewterActive ? pewterDamageMultiplier : 1f);
        float radius = impactRadius * (pewterActive ? pewterRadiusMultiplier : 1f);

        // AOE damage
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = radius > 0f ? 1f - (dist / radius) : 1f;
                damageable.TakeDamage(damage * Mathf.Max(0f, falloff));
            }

            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized + Vector3.up * 0.3f;
                rb.AddForce(dir * impactKnockback, ForceMode.Impulse);
            }
        }

        // VFX
        CameraShakeManager.Instance?.Shake(0.4f, 0.3f);
        SoundManager.Instance?.PlayImpactSound();

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }

    float GetHeightAboveGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, groundLayer))
            return hit.distance;
        return 100f;
    }

    public bool IsSlamming() => isSlamming;
}
