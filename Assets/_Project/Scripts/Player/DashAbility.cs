using UnityEngine;

public class SprintAbility : MonoBehaviour
{
    [Header("Sprint Settings")]
    public float sprintForce = 20f; // Increase this for more "oomph"
    public float sprintCooldown = 1f;
    public float metalCost = 10f;
    
    [Header("References")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public Allomancer allomancer;
    public MetalReserveManager metalManager;
    
    private float lastSprintTime = -999f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure drag isn't 0 so you don't slide forever
        if (rb.linearDamping == 0) rb.linearDamping = 2f; 
    }

    void Update()
    {
        if (Input.GetKeyDown(sprintKey) && Time.time - lastSprintTime >= sprintCooldown)
        {
            if (CanAffordSprint()) PerformInstantSprint();
        }
    }

    void PerformInstantSprint()
    {
        lastSprintTime = Time.time;
        DrainMetal();

        // Use Raw input for more reliability
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 sprintDir;
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            right.y = 0;
            sprintDir = (forward * v + right * h).normalized;
        }
        else
        {
            sprintDir = transform.forward;
        }

        // Apply an Impulse force - this is much more reliable
        rb.AddForce(sprintDir * sprintForce, ForceMode.Impulse);
    }

    // Existing Metal Logic
    bool CanAffordSprint() {
        if (allomancer != null) return allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        if (metalManager != null) return metalManager.GetReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        return true;
    }

    void DrainMetal() {
        if (allomancer != null) allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCost);
        else if (metalManager != null) metalManager.Drain(AllomancySkill.MetalType.Pewter, metalCost);
    }
}