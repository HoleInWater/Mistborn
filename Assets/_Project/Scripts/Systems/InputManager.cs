using UnityEngine;

using UnityEngine;

/// <summary>
/// Centralized input manager for all game controls.
/// Usage: InputManager.Instance.GetKeyDown(InputManager.Action.Dash)
/// 
/// STATIC:
///   InputManager.Instance - Access the singleton
/// 
/// METHODS:
///   InputManager.Instance.GetKeyDown(Action.SteelPush);
///   InputManager.Instance.GetKey(Action.Dash);
///   InputManager.Instance.GetAxisRaw(Action.MoveX);
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    // ACTION ENUM - Maps all game inputs
    public enum Action
    {
        // MOVEMENT
        MoveX,           // A/D or Left/Right arrows
        MoveZ,           // W/S or Up/Down arrows
        Jump,
        Sprint,
        
        // ALLOMANCY - PUSHING
        SteelPush,        // Right Mouse Button
        
        // ALLOMANCY - PULLING
        IronPull,         // Left Mouse Button
        
        // ALLOMANCY - ENHANCERS
        Pewter,          // Q
        Tin,             // E
        
        // ALLOMANCY - MENTAL
        Zinc,            // Z - Riot
        Brass,           // X - Soothe
        Copper,          // C - Cloud
        Bronze,          // V - Detect
        
        // ALLOMANCY - GOD METALS
        Atium,           // T
        Gold,            // G
        Electrum,        // 5
        Aluminum,        // O
        Duralumin,       // P
        
        // ALLOMANCY - TIME
        Bendalloy,       // 8 - Speed up
        Cadmium,         // 9 - Slow down
        
        // ABILITIES
        Dash,            // Left Shift
        Block,           // Right Shift
        Interact,        // F
        
        // UI
        SkillTree,       // Tab
        Inventory,       // I
        Map,             // M
        Pause,           // Escape
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // KEYBOARD INPUT
    
    public bool GetKeyDown(Action action)
    {
        return Input.GetKeyDown(GetKeyCode(action));
    }
    
    public bool GetKey(Action action)
    {
        return Input.GetKey(GetKeyCode(action));
    }
    
    public bool GetKeyUp(Action action)
    {
        return Input.GetKeyUp(GetKeyCode(action));
    }
    
    // AXIS INPUT
    
    public float GetAxisRaw(Action action)
    {
        if (action == Action.MoveX) return Input.GetAxisRaw("Horizontal");
        if (action == Action.MoveZ) return Input.GetAxisRaw("Vertical");
        return 0f;
    }
    
    // KEY CODE MAPPING
    
    KeyCode GetKeyCode(Action action)
    {
        switch (action)
        {
            // Movement
            case Action.Jump: return KeyCode.Space;
            case Action.Sprint: return KeyCode.LeftShift;
            
            // Allomancy - Push/Pull
            case Action.SteelPush: return KeyCode.Mouse1;
            case Action.IronPull: return KeyCode.Mouse0;
            
            // Allomancy - Enhancers
            case Action.Pewter: return KeyCode.Q;
            case Action.Tin: return KeyCode.E;
            
            // Allomancy - Mental
            case Action.Zinc: return KeyCode.Z;
            case Action.Brass: return KeyCode.X;
            case Action.Copper: return KeyCode.C;
            case Action.Bronze: return KeyCode.V;
            
            // Allomancy - God Metals
            case Action.Atium: return KeyCode.T;
            case Action.Gold: return KeyCode.G;
            case Action.Electrum: return KeyCode.Alpha5;
            case Action.Aluminum: return KeyCode.O;
            case Action.Duralumin: return KeyCode.R;
            
            // Allomancy - Time
            case Action.Bendalloy: return KeyCode.Alpha8;
            case Action.Cadmium: return KeyCode.Alpha9;
            
            // Abilities
            case Action.Dash: return KeyCode.LeftShift;
            case Action.Block: return KeyCode.RightShift;
            case Action.Interact: return KeyCode.F;
            
            // UI
            case Action.SkillTree: return KeyCode.Tab;
            case Action.Inventory: return KeyCode.I;
            case Action.Map: return KeyCode.M;
            case Action.Pause: return KeyCode.Escape;
            
            default: return KeyCode.None;
        }
    }
}
