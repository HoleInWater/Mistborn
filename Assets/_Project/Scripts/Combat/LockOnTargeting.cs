using UnityEngine;
using System.Collections.Generic;

public class LockOnTargeting : MonoBehaviour
{
    [Header("Settings")]
    public float lockOnRange = 30f;
    public float lockOnAngle = 60f;
    public float switchTargetTime = 0.3f;
    public float targetUpdateRate = 0.1f;

    [Header("Visual")]
    public GameObject targetReticlePrefab;
    public Color lockedColor = Color.red;
    public Color suspectColor = Color.yellow;

    [Header("References")]
    public Transform lockOnCenter;
    public Camera playerCamera;

    private Transform currentTarget;
    private GameObject reticleInstance;
    private float lastTargetSwitch = 0f;
    private List<Transform> availableTargets = new List<Transform>();

    public static LockOnTargeting Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        if (lockOnCenter == null) lockOnCenter = transform;
        if (playerCamera == null) playerCamera = Camera.main;

        if (targetReticlePrefab != null)
        {
            reticleInstance = Instantiate(targetReticlePrefab);
            reticleInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("LockOn"))
        {
            ToggleLockOn();
        }

        if (currentTarget != null)
        {
            if (Input.GetAxis("TargetSwitch") > 0.5f && Time.time - lastTargetSwitch > switchTargetTime)
            {
                SwitchTarget(1);
                lastTargetSwitch = Time.time;
            }

            UpdateReticle();

            if (!IsTargetValid(currentTarget))
            {
                ClearTarget();
            }
        }
    }

    void ToggleLockOn()
    {
        if (currentTarget != null)
        {
            ClearTarget();
        }
        else
        {
            ScanForTargets();
            if (availableTargets.Count > 0)
            {
                SetTarget(availableTargets[0]);
            }
        }
    }

    void ScanForTargets()
    {
        availableTargets.Clear();

        Collider[] targets = Physics.OverlapSphere(lockOnCenter.position, lockOnRange, LayerMask.GetMask("Enemy"));

        foreach (Collider c in targets)
        {
            Vector3 direction = c.transform.position - lockOnCenter.position;
            float angle = Vector3.Angle(lockOnCenter.forward, direction);

            if (angle <= lockOnAngle)
            {
                availableTargets.Add(c.transform);
            }
        }

        availableTargets.Sort((a, b) => 
            Vector3.Distance(lockOnCenter.position, a.position).CompareTo(
                Vector3.Distance(lockOnCenter.position, b.position)));
    }

    void SetTarget(Transform target)
    {
        currentTarget = target;
        if (reticleInstance != null)
        {
            reticleInstance.SetActive(true);
            reticleInstance.GetComponent<Renderer>().material.color = lockedColor;
        }

        if (GetComponent<AllomanticSight>())
        {
            GetComponent<AllomanticSight>().ForceHighlightTarget(target);
        }
    }

    void ClearTarget()
    {
        currentTarget = null;
        if (reticleInstance != null)
        {
            reticleInstance.SetActive(false);
        }
    }

    void SwitchTarget(int direction)
    {
        ScanForTargets();
        if (availableTargets.Count == 0) return;

        int currentIndex = currentTarget != null ? availableTargets.IndexOf(currentTarget) : -1;
        int newIndex = (currentIndex + direction + availableTargets.Count) % availableTargets.Count;

        SetTarget(availableTargets[newIndex]);
    }

    void UpdateReticle()
    {
        if (currentTarget == null || reticleInstance == null) return;

        Vector3 screenPos = playerCamera.WorldToScreenPoint(currentTarget.position);
        
        if (screenPos.z > 0)
        {
            reticleInstance.transform.position = screenPos;
        }
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        
        float distance = Vector3.Distance(lockOnCenter.position, target.position);
        Vector3 direction = target.position - lockOnCenter.position;
        float angle = Vector3.Angle(lockOnCenter.forward, direction);

        return distance <= lockOnRange && angle <= lockOnAngle;
    }

    public Transform GetCurrentTarget() => currentTarget;
    public bool IsLockedOn() => currentTarget != null;
}