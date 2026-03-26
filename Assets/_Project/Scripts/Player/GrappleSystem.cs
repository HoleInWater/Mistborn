using UnityEngine;

/// <summary>
/// Allomantic grapple system — Iron Pull toward anchor points or metal objects.
/// Uses lore-accurate physics: pulling toward a heavier anchor launches the player.
/// Middle mouse button to fire, hold to swing.
/// </summary>
public class GrappleSystem : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxGrappleRange = 30f;
    public float grappleSpeed = 15f;
    public float swingForce = 12f;
    public float arrivalDistance = 2f;
    public float metalCostPerSecond = 3f;

    [Header("Physics — PHYSICS-MATH-BOOK.md")]
    [Tooltip("Iron Pull force uses F = A × m1 × m2 / r², stronger at close range")]
    public float grappleStrength = 800f;

    [Header("Visual")]
    public LineRenderer ropeLine;
    public Color ropeColor = new Color(0.2f, 0.6f, 1f, 0.8f);

    [Header("References")]
    public Camera playerCamera;
    public Rigidbody playerRb;
    public Allomancer allomancer;
    public LayerMask metalLayer;

    private bool isGrappling = false;
    private Transform grappleTarget;
    private Vector3 grapplePoint;
    private float grappleDistance;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        if (ropeLine == null)
        {
            GameObject lineObj = new GameObject("GrappleLine");
            lineObj.transform.SetParent(transform);
            ropeLine = lineObj.AddComponent<LineRenderer>();
            ropeLine.startWidth = 0.03f;
            ropeLine.endWidth = 0.02f;
            ropeLine.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
            ropeLine.startColor = ropeColor;
            ropeLine.endColor = ropeColor;
            ropeLine.positionCount = 0;
        }
    }

    void Update()
    {
        // Middle mouse to grapple
        if (Input.GetMouseButtonDown(2) && !isGrappling)
        {
            TryGrapple();
        }

        if (Input.GetMouseButtonUp(2) && isGrappling)
        {
            ReleaseGrapple();
        }

        if (isGrappling)
        {
            UpdateGrapple();
            UpdateRopeLine();
            DrainMetal();
        }
        else if (ropeLine != null)
        {
            ropeLine.positionCount = 0;
        }
    }

    void TryGrapple()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrappleRange, metalLayer))
        {
            grappleTarget = hit.transform;
            grapplePoint = hit.point;
            grappleDistance = Vector3.Distance(transform.position, grapplePoint);
            isGrappling = true;

            // Start burn
            allomancer?.StartBurning(AllomancySkill.MetalType.Iron);
            SoundManager.Instance?.PlayPullSound();
        }
    }

    void UpdateGrapple()
    {
        if (grappleTarget == null)
        {
            ReleaseGrapple();
            return;
        }

        // Update grapple point if target moves
        grapplePoint = grappleTarget.position;
        float dist = Vector3.Distance(transform.position, grapplePoint);

        if (dist < arrivalDistance)
        {
            ReleaseGrapple();
            return;
        }

        // Lore-accurate pull force: F = A × m1 × m2 / r²
        // Target is anchored (building/heavy), so player gets all the force
        Rigidbody targetRb = grappleTarget.GetComponent<Rigidbody>();
        float targetMass = (targetRb != null && !targetRb.isKinematic) ? targetRb.mass : 10000f;

        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float force = AllomancyPhysicsFormulas.CalculateAllomanticForce(
            grappleStrength * flare, playerRb.mass, targetMass, dist);

        // Cap force for gameplay feel
        force = Mathf.Min(force, playerRb.mass * grappleSpeed);

        Vector3 pullDir = (grapplePoint - transform.position).normalized;

        // Apply pull + slight upward bias for arcing motion
<<<<<<< HEAD
        playerRb.AddForce(pullDir * force * Time.deltaTime, ForceMode.Force);
=======
        playerRb.AddForce(pullDir * force, ForceMode.Force);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a

        // Swing component (perpendicular input)
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.1f)
        {
            Vector3 swingDir = Vector3.Cross(pullDir, Vector3.up) * h;
            playerRb.AddForce(swingDir * swingForce, ForceMode.Force);
        }
    }

    void ReleaseGrapple()
    {
        isGrappling = false;
        grappleTarget = null;
    }

    void UpdateRopeLine()
    {
        if (ropeLine == null) return;
        ropeLine.positionCount = 2;

        // Rope from chest to grapple point
        Vector3 start = transform.position + Vector3.up * 1f;
        ropeLine.SetPosition(0, start);
        ropeLine.SetPosition(1, grapplePoint);

        // Color based on distance (closer = brighter)
        float dist = Vector3.Distance(start, grapplePoint);
        float t = 1f - Mathf.Clamp01(dist / maxGrappleRange);
        ropeLine.startColor = Color.Lerp(ropeColor * 0.5f, ropeColor, t);
    }

    void DrainMetal()
    {
        if (allomancer != null)
            allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond * Time.deltaTime);
    }

    public bool IsGrappling() => isGrappling;
    public Vector3 GetGrapplePoint() => grapplePoint;
}
