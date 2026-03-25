using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleHook : MonoBehaviour
{
    [Header("Settings")]
    public float maxGrappleDistance = 50f;
    public float grappleSpeed = 20f;
    public float reelSpeed = 10f;
    public float swingForce = 5f;
    public float ropeLength = 30f;
    public float pullStrength = 15f;

    [Header("Rope Visual")]
    public LineRenderer ropeLine;
    public Material ropeMaterial;
    public float ropeWidth = 0.05f;
    public int ropeSegments = 20;

    [Header("References")]
    public Transform firePoint;
    public Rigidbody rb;
    public Camera playerCamera;
    public Animator animator;
    public LayerMask grappleLayer;

    private bool isGrappling = false;
    private bool isRoping = false;
    private Vector3 grapplePoint;
    private Transform grappleTarget;
    private float currentRopeLength;
    private SpringJoint springJoint;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();

        SetupRopeLine();
    }

    void SetupRopeLine()
    {
        if (ropeLine == null)
        {
            ropeLine = gameObject.AddComponent<LineRenderer>();
            ropeLine.material = ropeMaterial;
            ropeLine.startWidth = ropeWidth;
            ropeLine.endWidth = ropeWidth;
            ropeLine.positionCount = ropeSegments;
            ropeLine.enabled = false;
        }
    }

    void Update()
    {
        HandleInput();
        UpdateRope();
    }

    void LateUpdate()
    {
        if (isGrappling)
        {
            UpdateRopeVisual();
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (!isGrappling)
            {
                FireGrapple();
            }
            else
            {
                ReleaseGrapple();
            }
        }

        if (isGrappling)
        {
            if (Input.GetKey(KeyCode.W))
            {
                ReelIn();
            }
            if (Input.GetKey(KeyCode.S))
            {
                ReelOut();
            }
        }
    }

    void FireGrapple()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayer))
        {
            StartGrapple(hit.point, hit.transform);
        }
    }

    void StartGrapple(Vector3 point, Transform target = null)
    {
        isGrappling = true;
        grapplePoint = point;
        grappleTarget = target;
        currentRopeLength = Vector3.Distance(transform.position, point);

        springJoint = gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = point;
        springJoint.maxDistance = currentRopeLength;
        springJoint.minDistance = currentRopeLength * 0.5f;
        springJoint.spring = 4.5f;
        springJoint.damper = 7f;
        springJoint.massScale = 4.5f;

        ropeLine.enabled = true;

        if (animator != null) animator.SetBool("IsGrappling", true);

        Debug.Log($"[GRAPPLE] Grappled to {point}");
    }

    void ReleaseGrapple()
    {
        isGrappling = false;
        isRoping = false;

        if (springJoint != null)
        {
            Destroy(springJoint);
        }

        ropeLine.enabled = false;
        grappleTarget = null;

        if (animator != null) animator.SetBool("IsGrappling", false);

        Debug.Log("[GRAPPLE] Released grapple");
    }

    void UpdateRope()
    {
        if (!isGrappling) return;

        if (grappleTarget != null)
        {
            grapplePoint = grappleTarget.position;
        }

        if (springJoint != null)
        {
            springJoint.connectedAnchor = grapplePoint;
        }
    }

    void UpdateRopeVisual()
    {
        if (ropeLine == null || !isGrappling) return;

        Vector3 startPos = firePoint.position;
        Vector3 endPos = grapplePoint;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = (float)i / (ropeSegments - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            float sag = Mathf.Sin(t * Mathf.PI) * 0.5f;
            pos.y -= sag;

            ropeLine.SetPosition(i, pos);
        }
    }

    void ReelIn()
    {
        if (springJoint == null) return;

        currentRopeLength -= reelSpeed * Time.deltaTime;
        currentRopeLength = Mathf.Max(currentRopeLength, 2f);
        springJoint.maxDistance = currentRopeLength;
    }

    void ReelOut()
    {
        if (springJoint == null) return;

        currentRopeLength += reelSpeed * Time.deltaTime;
        currentRopeLength = Mathf.Min(currentRopeLength, ropeLength);
        springJoint.maxDistance = currentRopeLength;
    }

    public void PullObject(Vector3 direction, float force)
    {
        if (!isGrappling) return;

        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public bool IsGrappling() => isGrappling;
    public Vector3 GetGrapplePoint() => grapplePoint;
}

public class RopePhysics : MonoBehaviour
{
    [Header("Settings")]
    public int ropeSegments = 20;
    public float ropeLength = 10f;
    public float ropeWidth = 0.02f;
    public float ropeDrag = 0.5f;
    public float ropeGravity = 9.81f;
    public float ropeDamping = 0.95f;

    [Header("End Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Visual")]
    public LineRenderer ropeLine;
    public Material ropeMaterial;

    private List<RopeSegment> segments = new List<RopeSegment>();
    private bool isActive = false;

    void Start()
    {
        SetupRope();
    }

    void SetupRope()
    {
        if (ropeLine == null)
        {
            ropeLine = gameObject.AddComponent<LineRenderer>();
            ropeLine.material = ropeMaterial;
            ropeLine.startWidth = ropeWidth;
            ropeLine.endWidth = ropeWidth;
            ropeLine.positionCount = ropeSegments;
        }

        float segmentLength = ropeLength / ropeSegments;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = (float)i / (ropeSegments - 1);
            Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, t);
            segments.Add(new RopeSegment(pos, segmentLength));
        }

        isActive = true;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        SimulateRope();
        ApplyConstraints();
    }

    void SimulateRope()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RopeSegment seg = segments[i];

            Vector3 velocity = (seg.position - seg.prevPosition) * ropeDamping;
            seg.prevPosition = seg.position;
            seg.position += velocity;
            seg.position += Vector3.down * ropeGravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        }
    }

    void ApplyConstraints()
    {
        segments[0].position = startPoint.position;
        segments[segments.Count - 1].position = endPoint.position;

        for (int i = 0; i < segments.Count - 1; i++)
        {
            RopeSegment segA = segments[i];
            RopeSegment segB = segments[i + 1];

            float dist = (segA.position - segB.position).magnitude;
            float diff = dist - segA.length;

            Vector3 dir = (segB.position - segA.position).normalized;

            if (i > 0)
                segA.position += dir * diff * 0.5f;
            segB.position -= dir * diff * 0.5f;
        }
    }

    void LateUpdate()
    {
        if (!isActive || ropeLine == null) return;

        for (int i = 0; i < segments.Count; i++)
        {
            ropeLine.SetPosition(i, segments[i].position);
        }
    }

    class RopeSegment
    {
        public Vector3 position;
        public Vector3 prevPosition;
        public float length;

        public RopeSegment(Vector3 pos, float len)
        {
            position = pos;
            prevPosition = pos;
            length = len;
        }
    }
}

public class SwingPoint : MonoBehaviour
{
    [Header("Settings")]
    public float swingForce = 10f;
    public float maxSwingSpeed = 20f;
    public float swingDamping = 0.95f;
    public float releaseBoost = 5f;

    [Header("Visual")]
    public LineRenderer ropeLine;
    public Transform attachPoint;

    [Header("References")]
    public Rigidbody playerRb;

    private bool isSwinging = false;
    private Transform swingTarget;
    private float ropeLength;
    private SpringJoint joint;

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        if (isSwinging && ropeLine != null)
        {
            UpdateRopeVisual();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isSwinging)
        {
            ReleaseSwing();
        }

        if (isSwinging)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 swingDir = new Vector3(h, 0, v).normalized;
            playerRb.AddForce(swingDir * swingForce);
        }
    }

    public void StartSwing(Transform target, Rigidbody player)
    {
        if (isSwinging) return;

        isSwinging = true;
        swingTarget = target;
        playerRb = player;
        ropeLength = Vector3.Distance(transform.position, target.position);

        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = target.position;
        joint.maxDistance = ropeLength;
        joint.minDistance = 0;
        joint.spring = 4.5f;
        joint.damper = 7f;

        if (ropeLine != null)
        {
            ropeLine.enabled = true;
        }

        Debug.Log("[SWING] Started swinging");
    }

    void UpdateRopeVisual()
    {
        if (ropeLine == null || swingTarget == null) return;

        ropeLine.positionCount = 2;
        ropeLine.SetPosition(0, attachPoint.position);
        ropeLine.SetPosition(1, swingTarget.position);
    }

    public void ReleaseSwing()
    {
        if (!isSwinging) return;

        isSwinging = false;

        if (joint != null)
        {
            Destroy(joint);
        }

        if (ropeLine != null)
        {
            ropeLine.enabled = false;
        }

        playerRb.AddForce(Vector3.up * releaseBoost, ForceMode.Impulse);

        Debug.Log("[SWING] Released swing");
    }

    public bool IsSwinging() => isSwinging;
}