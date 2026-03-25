using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ragdoll system for player death and heavy impacts.
/// Transitions between animated and ragdoll states smoothly.
/// Pewter prevents ragdoll from light impacts.
/// </summary>
public class PlayerRagdoll : MonoBehaviour
{
    [Header("Settings")]
    public float minImpactForce = 15f;
    public float ragdollDuration = 2f;
    public float recoverDuration = 0.5f;
    public float pewterImpactResistance = 3f;

    [Header("References")]
    public Animator animator;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private bool isRagdollActive = false;
    private bool isRecovering = false;
    private Rigidbody mainRb;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        mainRb = GetComponent<Rigidbody>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        DisableRagdoll();
    }

    void DisableRagdoll()
    {
        foreach (var rb in ragdollBodies)
        {
            if (rb == mainRb) continue;
            rb.isKinematic = true;
        }
        foreach (var col in ragdollColliders)
        {
            if (col.gameObject == gameObject) continue;
            col.enabled = false;
        }

        if (animator != null) animator.enabled = true;
        isRagdollActive = false;
    }

    void EnableRagdoll()
    {
        if (animator != null) animator.enabled = false;

        foreach (var rb in ragdollBodies)
        {
            if (rb == mainRb) continue;
            rb.isKinematic = false;
        }
        foreach (var col in ragdollColliders)
        {
            if (col.gameObject == gameObject) continue;
            col.enabled = true;
        }

        isRagdollActive = true;
    }

    /// <summary>
    /// Trigger ragdoll from impact. Pewter burning resists lighter impacts.
    /// </summary>
    public void OnImpact(Vector3 direction, float force)
    {
        if (isRagdollActive || isRecovering) return;

        // Pewter resistance
        float threshold = minImpactForce;
        Allomancer allo = GetComponent<Allomancer>();
        if (allo != null && allo.IsMetalBurning(AllomancySkill.MetalType.Pewter))
            threshold *= pewterImpactResistance;

        if (force < threshold) return;

        StartCoroutine(RagdollSequence(direction, force));
    }

    IEnumerator RagdollSequence(Vector3 direction, float force)
    {
        EnableRagdoll();

        // Apply impact force to all ragdoll bones
        foreach (var rb in ragdollBodies)
        {
            if (rb == mainRb) continue;
            rb.AddForce(direction * force * 0.3f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(ragdollDuration);

        // Recovery
        isRecovering = true;
        DisableRagdoll();

        yield return new WaitForSeconds(recoverDuration);
        isRecovering = false;
    }

    /// <summary>
    /// Force permanent ragdoll (death).
    /// </summary>
    public void OnDeath()
    {
        StopAllCoroutines();
        EnableRagdoll();
    }

    public bool IsRagdollActive() => isRagdollActive;
}
