using UnityEngine;

/// <summary>
/// Centralized cursor state. All UI scripts call this instead of setting
/// Cursor.visible / Cursor.lockState directly.
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    public enum CursorState { Gameplay, Menu, Dialogue, Cutscene }

    [Header("Current State")]
    public CursorState currentState = CursorState.Gameplay;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(CursorState state)
    {
        currentState = state;
        Apply();
    }

    public void ShowCursor()  => SetState(CursorState.Menu);
    public void HideCursor()  => SetState(CursorState.Gameplay);

    void Apply()
    {
        switch (currentState)
        {
            case CursorState.Gameplay:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case CursorState.Menu:
            case CursorState.Dialogue:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;

            case CursorState.Cutscene:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }
}
