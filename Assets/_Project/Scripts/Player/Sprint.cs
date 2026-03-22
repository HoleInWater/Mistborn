// NOTE: Consider adding [RequireComponent(typeof(PlayerStamina))] attribute for dependency
using UnityEngine;

public class Sprint : MonoBehaviour
{
    [Header("Speed Settings")]
    // NOTE: Consider adding [Range(1f, 20f)] attribute for walkSpeed
    public float walkSpeed = 5f;
    // NOTE: Consider adding [Range(5f, 30f)] attribute for sprintSpeed
    public float sprintSpeed = 10f;
    
    [HideInInspector] 
    public float currentSpeed; // Your movement script will read this

    [Header("Stamina Costs")]
    // NOTE: Consider adding [Range(1f, 100f)] attribute for drainRate
    public float drainRate = 25f;
    
    private PlayerStamina staminaSystem;

    void Start()
    {
        staminaSystem = GetComponent<PlayerStamina>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        // Check if Shift is held AND we have stamina left
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        bool hasStamina = staminaSystem.currentStamina > 0.5f;

        if (isTryingToSprint && hasStamina)
        {
            currentSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate); // Tells the other script to lower the bar
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }
}
