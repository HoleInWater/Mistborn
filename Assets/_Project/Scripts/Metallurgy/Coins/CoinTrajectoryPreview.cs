using UnityEngine;

/// <summary>
/// Previews coin trajectory before firing. Shows a dotted arc line using
/// the physics formula from PHYSICS-MATH-BOOK.md Section 3:
/// pos(t) = start + v×t + ½g×t²
/// Accounts for air drag at high velocities.
/// </summary>
public class CoinTrajectoryPreview : MonoBehaviour
{
    [Header("Settings")]
    public int previewPoints = 30;
    public float previewTimeStep = 0.05f;
    public float previewMaxTime = 1.5f;
    public bool showOnAim = true;

    [Header("Visual")]
    public Color trajectoryColor = new Color(1f, 0.8f, 0.2f, 0.5f);
    public float lineWidth = 0.02f;

    [Header("References")]
    public Camera playerCamera;
    public Metallurgist metallurgist;
    public CoinPouch coinPouch;
    public Rigidbody playerRb;

    private LineRenderer trajectoryLine;
    private bool isShowing = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
        if (coinPouch == null) coinPouch = GetComponent<CoinPouch>();
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();

        CreateTrajectoryLine();
    }

    void CreateTrajectoryLine()
    {
        GameObject go = new GameObject("CoinTrajectoryPreview");
        go.transform.SetParent(transform);
        trajectoryLine = go.AddComponent<LineRenderer>();
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor * 0.3f;
        trajectoryLine.startWidth = lineWidth;
        trajectoryLine.endWidth = lineWidth * 0.3f;
        trajectoryLine.positionCount = 0;
        trajectoryLine.useWorldSpace = true;
    }

    void Update()
    {
        // Show trajectory while aiming (holding right mouse)
        bool shouldShow = showOnAim && Input.GetMouseButton(1)
                       && coinPouch != null && coinPouch.GetCoinCount() > 0
                       && metallurgist != null
                       && metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Steel) > 0;

        if (shouldShow)
        {
            UpdateTrajectory();
            isShowing = true;
        }
        else if (isShowing)
        {
            trajectoryLine.positionCount = 0;
            isShowing = false;
        }
    }

    void UpdateTrajectory()
    {
        if (playerCamera == null || playerRb == null) return;

        Vector3 launchPos = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        Vector3 launchDir = playerCamera.transform.forward;

        // Calculate initial coin velocity using handbook formula
        // v = √(2 × F × d / m₂)
        float A = MetallurgyPhysicsFormulas.A_CONSERVATIVE;
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float force = MetallurgyPhysicsFormulas.CalculateMetallurgicForce(
            A * flare, playerRb.mass, MetallurgyPhysicsFormulas.COIN_MASS, 1f);
        float coinVel = MetallurgyPhysicsFormulas.CalculateCoinVelocity(
            force, 2f, MetallurgyPhysicsFormulas.COIN_MASS);

        // Cap for gameplay
        coinVel = Mathf.Min(coinVel, 200f);

        Vector3 velocity = launchDir * coinVel;

        // Generate trajectory points using pos(t) = start + v×t + ½g×t²
        trajectoryLine.positionCount = previewPoints;
        Vector3[] points = new Vector3[previewPoints];

        for (int i = 0; i < previewPoints; i++)
        {
            float t = i * previewTimeStep;
            points[i] = MetallurgyPhysicsFormulas.PredictPosition(launchPos, velocity, t);

            // Check for collision — stop the line if it hits something
            if (i > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(points[i - 1], (points[i] - points[i - 1]).normalized,
                    out hit, Vector3.Distance(points[i - 1], points[i])))
                {
                    points[i] = hit.point;
                    trajectoryLine.positionCount = i + 1;
                    break;
                }
            }
        }

        trajectoryLine.SetPositions(points);
    }

    public bool IsShowingPreview() => isShowing;
}
