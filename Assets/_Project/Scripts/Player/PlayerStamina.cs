using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float sprintDrainRate = 10f;
    public float recoveryRate = 5f;
    public float minStaminaToSprint = 10f;
    
    [Header("References")]
    public PlayerMove playerController;
    
    private bool isSprinting = false;
    
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > minStaminaToSprint)
        {
            DrainStamina();
        }
        else
        {
            RecoverStamina();
        }
    }
    
    void DrainStamina()
    {
        currentStamina -= sprintDrainRate * Time.deltaTime;
        currentStamina = Mathf.Max(0, currentStamina);
        
        if (playerController != null)
        {
            playerController.moveSpeed = playerController.moveSpeed * 1.5f;
        }
        
        isSprinting = true;
    }
    
    void RecoverStamina()
    {
        if (isSprinting && currentStamina < minStaminaToSprint)
        {
            Debug.Log("Too tired to sprint!");
        }
        
        currentStamina += recoveryRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        
        if (playerController != null && isSprinting)
        {
            playerController.moveSpeed = playerController.moveSpeed / 1.5f;
        }
        
        isSprinting = false;
    }
    
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
    
    public bool CanSprint()
    {
        return currentStamina > minStaminaToSprint;
    }
}
