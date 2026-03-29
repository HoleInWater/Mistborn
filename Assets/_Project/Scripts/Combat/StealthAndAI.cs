using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enemy-side stealth detection system. Tracks player visibility, suspicion, alerts.
/// Copper burning hides from detection. Crouching reduces detection radius.
/// </summary>
public class StealthDetection : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 15f;
    public float detectionAngle = 90f;
    public float suspicionBuildRate = 1f;
    public float suspicionDecayRate = 0.5f;

    [Header("Alert Levels")]
    public float suspiciousThreshold = 0.4f;
    public float alertThreshold = 0.8f;

    public enum AlertState { Hidden, Suspicious, Alert }
    public AlertState currentAlert = AlertState.Hidden;

    [Header("References")]
    public Transform eyePoint;

    private Transform player;
    private float currentSuspicion = 0f;
    private CrouchSystem playerCrouch;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCrouch = playerObj.GetComponent<CrouchSystem>();
        }

        if (eyePoint == null) eyePoint = transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(eyePoint.position, player.position);
        float effectiveRadius = detectionRadius;

        // Crouching reduces detection range
        if (playerCrouch != null)
            effectiveRadius *= playerCrouch.GetStealthMultiplier();

        // Copper burning hides from detection
        if (CognitiveAllomancy.IsHiddenByCloud(player.position))
            effectiveRadius *= 0.2f;

        bool canSee = false;
        if (dist <= effectiveRadius)
        {
            // Check angle
            Vector3 dir = (player.position - eyePoint.position).normalized;
            float angle = Vector3.Angle(eyePoint.forward, dir);

            if (angle <= detectionAngle * 0.5f)
            {
                // Line of sight check
                RaycastHit hit;
                if (Physics.Raycast(eyePoint.position, dir, out hit, effectiveRadius))
                {
                    if (hit.transform == player || hit.transform.IsChildOf(player))
                        canSee = true;
                }
            }
        }

        // Update suspicion
        if (canSee)
        {
            float proximity = 1f - (dist / effectiveRadius);
            currentSuspicion += suspicionBuildRate * proximity * Time.deltaTime;
        }
        else
        {
            currentSuspicion -= suspicionDecayRate * Time.deltaTime;
        }
        currentSuspicion = Mathf.Clamp01(currentSuspicion);

        // Update alert state
        if (currentSuspicion >= alertThreshold)
            currentAlert = AlertState.Alert;
        else if (currentSuspicion >= suspiciousThreshold)
            currentAlert = AlertState.Suspicious;
        else
            currentAlert = AlertState.Hidden;
    }

    public float GetSuspicion() => currentSuspicion;
    public bool IsAlert() => currentAlert == AlertState.Alert;
    public bool IsSuspicious() => currentAlert != AlertState.Hidden;
}
