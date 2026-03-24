using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages enemy targeting and lock-on camera behavior.
/// </summary>
public class LockOnSystem : MonoBehaviour
{
    [Header("Settings")]
    public float searchRadius = 20f;
    public LayerMask enemyLayer;
    public KeyCode lockOnKey = KeyCode.Mouse2; // Middle click

    [Header("References")]
    public BasicPlayerMove playerMove;
    public Camera mainCamera;

    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    void Awake()
    {
        if (playerMove == null) playerMove = GetComponentInParent<BasicPlayerMove>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(lockOnKey))
        {
            ToggleLockOn();
        }

        if (currentTarget != null)
        {
            // First check if the transform reference itself was nulled by Unity destruction
            if (!currentTarget || currentTarget.gameObject == null)
            {
                ClearLockOn();
                return;
            }

            // Check if target is still valid/alive
            IDamageable d = currentTarget.GetComponentInParent<IDamageable>();
            if (d == null || (d is MonoBehaviour mb && !mb.enabled) || Vector3.Distance(transform.position, currentTarget.position) > searchRadius * 1.5f)
            {
                ClearLockOn();
            }
        }
    }

    public void ToggleLockOn()
    {
        if (currentTarget != null)
        {
            ClearLockOn();
        }
        else
        {
            FindTarget();
        }
    }

    private void FindTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRadius, enemyLayer);
        float closestWeight = float.MaxValue;
        Transform bestTarget = null;

        foreach (var col in enemies)
        {
            // Weight based on screen center proximity and distance
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(col.transform.position);
            bool isOnScreen = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            if (isOnScreen)
            {
                float distToCenter = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), new Vector2(0.5f, 0.5f));
                float distToPlayer = Vector3.Distance(transform.position, col.transform.position);
                
                float weight = distToCenter * 10f + distToPlayer; // Prefer center over distance

                if (weight < closestWeight)
                {
                    closestWeight = weight;
                    bestTarget = col.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            Debug.Log($"[LOCK-ON] Targeted {currentTarget.name}");
        }
    }

    public void ClearLockOn()
    {
        currentTarget = null;
        Debug.Log("[LOCK-ON] Cleared");
    }
}
