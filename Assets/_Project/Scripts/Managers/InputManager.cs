using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input State")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jumpPressed;
    public bool sprintHeld;
    public bool attackPressed;
    public bool blockHeld;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        attackPressed = Input.GetMouseButtonDown(0);
        blockHeld = Input.GetKey(KeyCode.Mouse1);
    }
}
