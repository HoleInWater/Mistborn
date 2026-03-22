using UnityEngine;

public class Sprint : MonoBehaviour
{
    [Header("Speed Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float currentSpeed;

    [Header("Stamina Costs")]
    public float drainRate = 25f;
    
    private PlayerStamina staminaSystem;
    private CharacterController controller; // Or Rigidbody

    void Start()
    {
        staminaSystem = GetComponent<PlayerStamina>();
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        // Determine if player wants to sprint and has stamina left
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        bool hasStamina = staminaSystem.currentStamina > 0.1f;

        if (isTryingToSprint && hasStamina)
        {
            currentSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate);
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Apply movement (Example using CharacterController)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
}
