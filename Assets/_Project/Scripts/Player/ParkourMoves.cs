using UnityEngine;
using System.Collections;

public class VaultSystem : MonoBehaviour
{
    [Header("Vault Settings")]
    public float vaultSpeed = 8f;
    public float vaultHeight = 1.5f;
    public float vaultDistance = 2f;
    public float mantleHeight = 2.5f;
    public float mantleSpeed = 5f;
    public float vaultCooldown = 0.5f;

    [Header("Detection")]
    public float vaultDetectDistance = 1.5f;
    public float vaultDetectHeight = 0.5f;
    public LayerMask vaultableLayer;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public Camera playerCamera;
    public Transform vaultCheck;

    private bool isVaulting = false;
    private bool isMantling = false;
    private float lastVaultTime;
    private Vector3 vaultTarget;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (isVaulting || isMantling) return;

        if (Time.time - lastVaultTime < vaultCooldown) return;

        CheckForVault();
        CheckForMantle();
    }

    void CheckForVault()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, vaultDetectDistance, vaultableLayer))
        {
            float height = hit.point.y - transform.position.y;

            if (height < vaultHeight)
            {
                StartVault(hit.point);
            }
        }
    }

    void CheckForMantle()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * mantleHeight;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, vaultDetectDistance, vaultableLayer))
        {
            Vector3 mantlePos = hit.point + Vector3.up * 0.5f;
            StartMantle(mantlePos);
        }
    }

    void StartVault(Vector3 target)
    {
        if (isVaulting) return;

        isVaulting = true;
        lastVaultTime = Time.time;
        vaultTarget = target;

        if (rb != null) rb.isKinematic = true;

        if (animator != null) animator.SetTrigger("Vault");

        StartCoroutine(VaultRoutine());

        Debug.Log("[VAULT] Started vault");
    }

    void StartMantle(Vector3 target)
    {
        if (isMantling) return;

        isMantling = true;
        lastVaultTime = Time.time;
        vaultTarget = target;

        if (rb != null) rb.isKinematic = true;

        if (animator != null) animator.SetTrigger("Mantle");

        StartCoroutine(MantleRoutine());

        Debug.Log("[MANTLE] Started mantle");
    }

    IEnumerator VaultRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = Vector3.Lerp(startPos, vaultTarget + transform.forward * vaultDistance, t);
            pos.y = startPos.y + Mathf.Sin(t * Mathf.PI) * vaultHeight;

            transform.position = pos;

            yield return null;
        }

        transform.position = vaultTarget + transform.forward * vaultDistance;

        if (rb != null) rb.isKinematic = false;

        isVaulting = false;

        Debug.Log("[VAULT] Completed vault");
    }

    IEnumerator MantleRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = Vector3.Lerp(startPos, vaultTarget, t);
            transform.position = pos;

            yield return null;
        }

        transform.position = vaultTarget;

        if (rb != null) rb.isKinematic = false;

        isMantling = false;

        Debug.Log("[MANTLE] Completed mantle");
    }

    public bool IsVaulting() => isVaulting;
    public bool IsMantling() => isMantling;
}

public class SlideSystem : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideSpeed = 12f;
    public float slideDeceleration = 5f;
    public float slideDuration = 1f;
    public float slideHeight = 0.5f;
    public float slideCooldown = 0.5f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public CapsuleCollider playerCollider;

    private bool isSliding = false;
    private float lastSlideTime;
    private float originalHeight;
    private Vector3 originalCenter;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();

        if (playerCollider != null)
        {
            originalHeight = playerCollider.height;
            originalCenter = playerCollider.center;
        }
    }

    void Update()
    {
        if (isSliding) return;

        if (Time.time - lastSlideTime < slideCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftControl) && IsMoving())
        {
            StartSlide();
        }
    }

    bool IsMoving()
    {
        return rb.linearVelocity.magnitude > 2f;
    }

    void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;
        lastSlideTime = Time.time;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * slideSpeed;
        }

        if (playerCollider != null)
        {
            playerCollider.height = slideHeight;
            playerCollider.center = new Vector3(0, slideHeight / 2, 0);
        }

        if (animator != null) animator.SetBool("IsSliding", true);

        StartCoroutine(SlideRoutine());

        Debug.Log("[SLIDE] Started slide");
    }

    IEnumerator SlideRoutine()
    {
        float elapsed = 0;

        while (elapsed < slideDuration && isSliding)
        {
            elapsed += Time.deltaTime;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, slideDeceleration * Time.deltaTime);
            }

            yield return null;
        }

        EndSlide();
    }

    void EndSlide()
    {
        isSliding = false;

        if (playerCollider != null)
        {
            playerCollider.height = originalHeight;
            playerCollider.center = originalCenter;
        }

        if (animator != null) animator.SetBool("IsSliding", false);

        Debug.Log("[SLIDE] Ended slide");
    }

    public bool IsSliding() => isSliding;
}

public class WallRunSystem : MonoBehaviour
{
    [Header("Wall Run Settings")]
    public float wallRunSpeed = 8f;
    public float wallRunGravity = 0.5f;
    public float wallRunDuration = 2f;
    public float wallRunJumpForce = 10f;
    public float wallCheckDistance = 1.5f;
    public float minWallRunSpeed = 3f;

    [Header("Visual")]
    public float tiltAngle = 15f;
    public float tiltSpeed = 5f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public Camera playerCamera;
    public Transform wallCheck;

    private bool isWallRunning = false;
    private float wallRunTimer;
    private Vector3 wallNormal;
    private Vector3 wallRunDirection;
    private bool isWallLeft;
    private bool isWallRight;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        CheckForWall();
        HandleWallRun();
    }

    void CheckForWall()
    {
        RaycastHit leftHit, rightHit;

        isWallLeft = Physics.Raycast(transform.position, -transform.right, out leftHit, wallCheckDistance);
        isWallRight = Physics.Raycast(transform.position, transform.right, out rightHit, wallCheckDistance);

        if (isWallLeft)
        {
            wallNormal = leftHit.normal;
            wallRunDirection = Vector3.Cross(wallNormal, Vector3.up).normalized;
        }
        else if (isWallRight)
        {
            wallNormal = rightHit.normal;
            wallRunDirection = Vector3.Cross(wallNormal, Vector3.up).normalized;
        }
    }

    void HandleWallRun()
    {
        if ((isWallLeft || isWallRight) && !isWallRunning && Input.GetKey(KeyCode.Space))
        {
            if (rb.linearVelocity.magnitude >= minWallRunSpeed)
            {
                StartWallRun();
            }
        }

        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 || (!isWallLeft && !isWallRight))
            {
                EndWallRun();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallRunJump();
            }
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = wallRunDuration;

        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (animator != null) animator.SetBool("IsWallRunning", true);

        Debug.Log("[WALLRUN] Started wall run");
    }

    void WallRunJump()
    {
        Vector3 jumpDir = (transform.up + wallNormal).normalized;

        rb.useGravity = true;
        rb.linearVelocity = jumpDir * wallRunJumpForce;

        EndWallRun();

        Debug.Log("[WALLRUN] Wall run jump");
    }

    void EndWallRun()
    {
        isWallRunning = false;

        rb.useGravity = true;

        if (animator != null) animator.SetBool("IsWallRunning", false);

        Debug.Log("[WALLRUN] Ended wall run");
    }

    void FixedUpdate()
    {
        if (isWallRunning)
        {
            rb.linearVelocity = wallRunDirection * wallRunSpeed;
            rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Acceleration);
        }
    }

    void LateUpdate()
    {
        if (isWallRunning)
        {
            float targetTilt = isWallLeft ? -tiltAngle : tiltAngle;
            float currentTilt = playerCamera.transform.localEulerAngles.z;
            float newTilt = Mathf.LerpAngle(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
            playerCamera.transform.localEulerAngles = new Vector3(playerCamera.transform.localEulerAngles.x, playerCamera.transform.localEulerAngles.y, newTilt);
        }
        else
        {
            float currentTilt = playerCamera.transform.localEulerAngles.z;
            float newTilt = Mathf.LerpAngle(currentTilt, 0, tiltSpeed * Time.deltaTime);
            playerCamera.transform.localEulerAngles = new Vector3(playerCamera.transform.localEulerAngles.x, playerCamera.transform.localEulerAngles.y, newTilt);
        }
    }

    public bool IsWallRunning() => isWallRunning;
}