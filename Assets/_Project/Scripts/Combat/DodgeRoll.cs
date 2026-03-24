using UnityEngine;
using System.Collections;

/// <summary>
/// Implements a dodge roll mechanic with I-frames and Pewter synergy.
/// </summary>
public class DodgeRoll : MonoBehaviour
{
    [Header("Settings")]
    public float dodgeForce = 12f;
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 0.8f;
    public float iFrameDuration = 0.4f;

    [Header("Pewter Synergy")]
    public float pewterDodgeMultiplier = 1.5f;

    private BasicPlayerMove playerMove;
    private PlayerHealth playerHealth;
    private Allomancer allomancer;
    private Rigidbody rb;
    
    private bool isDodging = false;
    private float lastDodgeTime = -10f;

    void Awake()
    {
        playerMove = GetComponent<BasicPlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        allomancer = GetComponent<Allomancer>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDodging && Time.time > lastDodgeTime + dodgeCooldown)
        {
            StartCoroutine(PerformDodge());
        }
    }

    IEnumerator PerformDodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        // Determine Direction
        Vector3 dodgeDir = playerMove.GetInputDirection();
        if (dodgeDir == Vector3.zero) dodgeDir = transform.forward;

        // Pewter Check
        float currentMult = 1f;
        if (allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Pewter)
        {
            currentMult = pewterDodgeMultiplier;
        }

        // Apply Force
        rb.linearVelocity = dodgeDir * dodgeForce * currentMult;

        // I-Frames
        if (playerHealth != null) playerHealth.SetInvincible(true);
        yield return new WaitForSeconds(iFrameDuration);
        if (playerHealth != null) playerHealth.SetInvincible(false);

        // Transition out
        yield return new WaitForSeconds(dodgeDuration - iFrameDuration);
        isDodging = false;
    }
}
