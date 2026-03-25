using UnityEngine;
using System.Collections;

public class SwimmingSystem : MonoBehaviour
{
    [Header("Settings")]
    public float swimSpeed = 3f;
    public float swimUpSpeed = 4f;
    public float swimDownSpeed = 2f;
    public float waterDrag = 2f;
    public float waterGravity = 0.5f;
    public float oxygenMax = 100f;
    public float oxygenDrainRate = 5f;
    public float oxygenRegenRate = 20f;
    public float drowningDamagePerSecond = 10f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public LayerMask waterLayer;

    private bool isInWater = false;
    private bool isSubmerged = false;
    private float oxygen = 100f;
    private float originalDrag;
    private float originalGravity;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        oxygen = oxygenMax;
    }

    void Update()
    {
        CheckWaterState();
        HandleSwimming();
        HandleOxygen();
    }

    void CheckWaterState()
    {
        RaycastHit hit;
        bool inWater = Physics.Raycast(transform.position, Vector3.up, out hit, 2f, waterLayer);

        if (inWater && !isInWater)
        {
            EnterWater();
        }
        else if (!inWater && isInWater)
        {
            ExitWater();
        }

        isSubmerged = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f, waterLayer);
    }

    void EnterWater()
    {
        isInWater = true;
        originalDrag = rb.drag;
        originalGravity = rb.mass;
        rb.drag = waterDrag;

        if (animator != null) animator.SetBool("IsSwimming", true);

        Debug.Log("[SWIMMING] Entered water");
    }

    void ExitWater()
    {
        isInWater = false;
        rb.drag = originalDrag;
        rb.mass = originalGravity;
        oxygen = Mathf.Min(oxygen, oxygenMax);

        if (animator != null) animator.SetBool("IsSwimming", false);

        Debug.Log("[SWIMMING] Exited water");
    }

    void HandleSwimming()
    {
        if (!isInWater) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool swimUp = Input.GetKey(KeyCode.Space);
        bool swimDown = Input.GetKey(KeyCode.LeftControl);

        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        if (moveDir.magnitude > 0)
        {
            rb.AddForce(moveDir * swimSpeed, ForceMode.Force);
        }

        if (swimUp)
        {
            rb.AddForce(Vector3.up * swimUpSpeed, ForceMode.Force);
        }
        else if (swimDown)
        {
            rb.AddForce(Vector3.down * swimDownSpeed, ForceMode.Force);
        }

        rb.AddForce(Vector3.down * waterGravity, ForceMode.Force);
    }

    void HandleOxygen()
    {
        if (!isInWater) return;

        if (isSubmerged)
        {
            oxygen -= oxygenDrainRate * Time.deltaTime;

            if (oxygen <= 0)
            {
                oxygen = 0;
                ApplyDrowningDamage();
            }
        }
        else
        {
            oxygen += oxygenRegenRate * Time.deltaTime;
            oxygen = Mathf.Min(oxygen, oxygenMax);
        }

        UpdateOxygenUI();
    }

    void ApplyDrowningDamage()
    {
        IDamageable damageable = GetComponent<IDamageable>();
        damageable?.TakeDamage(drowningDamagePerSecond * Time.deltaTime);
    }

    void UpdateOxygenUI()
    {
        // Update oxygen bar UI
    }

    public bool IsInWater() => isInWater;
    public bool IsSubmerged() => isSubmerged;
    public float GetOxygenPercent() => oxygen / oxygenMax;
}

public class LadderSystem : MonoBehaviour
{
    [Header("Settings")]
    public float climbSpeed = 3f;
    public float mountSpeed = 2f;
    public float dismountForce = 5f;
    public float ladderMountDistance = 1.5f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;

    private bool isClimbing = false;
    private bool isNearLadder = false;
    private Transform currentLadder;
    private Vector3 ladderTop;
    private Vector3 ladderBottom;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;
    }

    void Update()
    {
        CheckLadder();
        HandleClimbing();
    }

    void CheckLadder()
    {
        RaycastHit hit;
        bool nearLadder = Physics.Raycast(transform.position, transform.forward, out hit, ladderMountDistance);

        if (nearLadder && hit.collider.CompareTag("Ladder"))
        {
            isNearLadder = true;
            currentLadder = hit.collider.transform;

            if (Input.GetKeyDown(KeyCode.E) && !isClimbing)
            {
                MountLadder();
            }
        }
        else
        {
            isNearLadder = false;
        }
    }

    void MountLadder()
    {
        if (currentLadder == null) return;

        isClimbing = true;
        rb.isKinematic = true;

        ladderTop = currentLadder.position + Vector3.up * currentLadder.localScale.y;
        ladderBottom = currentLadder.position - Vector3.up * currentLadder.localScale.y;

        if (animator != null) animator.SetBool("IsClimbing", true);

        Debug.Log("[LADDER] Mounted ladder");
    }

    void HandleClimbing()
    {
        if (!isClimbing) return;

        float v = Input.GetAxis("Vertical");

        Vector3 targetPos = transform.position + Vector3.up * v * climbSpeed * Time.deltaTime;

        targetPos.y = Mathf.Clamp(targetPos.y, ladderBottom.y, ladderTop.y);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mountSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DismountLadder();
        }

        if (v != 0)
        {
            if (animator != null) animator.SetFloat("ClimbSpeed", Mathf.Abs(v));
        }
    }

    void DismountLadder()
    {
        isClimbing = false;
        rb.isKinematic = false;

        rb.AddForce(transform.forward * dismountForce, ForceMode.Impulse);

        if (animator != null) animator.SetBool("IsClimbing", false);

        currentLadder = null;

        Debug.Log("[LADDER] Dismounted ladder");
    }

    public bool IsClimbing() => isClimbing;
    public bool IsNearLadder() => isNearLadder;
}

public class ZiplineSystem : MonoBehaviour
{
    [Header("Settings")]
    public float zipSpeed = 15f;
    public float mountSpeed = 5f;
    public float dismountForce = 8f;
    public float zipMountDistance = 3f;
    public float gravityWhileZipping = 0f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public LineRenderer zipLine;

    private bool isZipping = false;
    private bool isNearZipline = false;
    private Transform currentZipline;
    private Transform ziplineStart;
    private Transform ziplineEnd;
    private float zipProgress = 0f;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();

        if (zipLine != null) zipLine.enabled = false;
    }

    void Update()
    {
        CheckZipline();
        HandleZipping();
    }

    void CheckZipline()
    {
        RaycastHit hit;
        bool nearZipline = Physics.Raycast(transform.position, transform.forward, out hit, zipMountDistance);

        if (nearZipline && hit.collider.CompareTag("Zipline"))
        {
            isNearZipline = true;
            currentZipline = hit.collider.transform;

            ZiplineData data = currentZipline.GetComponent<ZiplineData>();
            if (data != null)
            {
                ziplineStart = data.startPoint;
                ziplineEnd = data.endPoint;
            }

            if (Input.GetKeyDown(KeyCode.E) && !isZipping)
            {
                MountZipline();
            }
        }
        else
        {
            isNearZipline = false;
        }
    }

    void MountZipline()
    {
        if (currentZipline == null || ziplineStart == null || ziplineEnd == null) return;

        isZipping = true;
        rb.isKinematic = true;
        zipProgress = 0f;

        if (animator != null) animator.SetBool("IsZipping", true);

        if (zipLine != null)
        {
            zipLine.enabled = true;
            zipLine.positionCount = 2;
            zipLine.SetPosition(0, ziplineStart.position);
            zipLine.SetPosition(1, ziplineEnd.position);
        }

        Debug.Log("[ZIPLINE] Mounted zipline");
    }

    void HandleZipping()
    {
        if (!isZipping) return;

        zipProgress += zipSpeed * Time.deltaTime / Vector3.Distance(ziplineStart.position, ziplineEnd.position);

        Vector3 targetPos = Vector3.Lerp(ziplineStart.position, ziplineEnd.position, zipProgress);

        transform.position = targetPos;

        Vector3 direction = (ziplineEnd.position - ziplineStart.position).normalized;
        transform.forward = direction;

        if (zipProgress >= 1f)
        {
            DismountZipline();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DismountZipline();
        }
    }

    void DismountZipline()
    {
        isZipping = false;
        rb.isKinematic = false;

        rb.AddForce(transform.forward * dismountForce + Vector3.up * 5f, ForceMode.Impulse);

        if (animator != null) animator.SetBool("IsZipping", false);

        if (zipLine != null) zipLine.enabled = false;

        currentZipline = null;

        Debug.Log("[ZIPLINE] Dismounted zipline");
    }

    public bool IsZipping() => isZipping;
    public bool IsNearZipline() => isNearZipline;
}

public class ZiplineData : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float zipSpeed = 15f;
}