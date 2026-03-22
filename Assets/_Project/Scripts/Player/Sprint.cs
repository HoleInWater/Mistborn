using UnityEngine;

public class Sprint : MonoBehaviour
{
    [Header("Speed Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    
    [HideInInspector] 
    public float currentSpeed; // Your movement script will read this

    [Header("Stamina Costs")]
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
