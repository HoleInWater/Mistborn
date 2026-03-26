using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach one of these to each keybind row in the Keybinds settings tab.
///
/// Inspector setup per row:
///   actionName  — must match a KeyCode field in Keybinds.cs (e.g. "Jump")
///   defaultKey  — the compile-time default for that action
///   actionLabel — Text that shows the human-readable action name
///   rebindButton — Button the player clicks to start listening
///   keyLabel    — Text inside the button that shows the current key
///
/// Press Esc while listening to cancel the rebind without closing the menu.
/// Mouse buttons and modifier keys are supported.
/// Cannot rebind to Escape (reserved for Pause).
/// </summary>
public class KeybindRebinder : MonoBehaviour
{
    [Header("Config")]
    public string  actionName;   // field name in Keybinds.cs, e.g. "Sprint"
    public KeyCode defaultKey;   // compile-time default for this action

    [Header("UI")]
    public Text   actionLabel;   // shows "Sprint", "Jump", etc.
    public Button rebindButton;
    public Text   keyLabel;      // shows "LeftShift", "Space", etc.

    // ── Static flag so PauseMenuSystem can check before handling Esc ──────
    public static bool IsRebinding => s_currentListener != null;
    static KeybindRebinder s_currentListener;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (actionLabel != null) actionLabel.text = FormatActionName(actionName);
        RefreshDisplay();

        if (rebindButton != null)
            rebindButton.onClick.AddListener(StartRebind);
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void RefreshDisplay()
    {
        if (keyLabel == null) return;
        KeyCode current = GetCurrentKey();
        keyLabel.text = FormatKeyName(current);
    }

    public void ResetToDefault()
    {
        SettingsManager.Instance?.ResetKeybind(actionName, defaultKey);
        RefreshDisplay();
    }

    // ── Rebind Flow ───────────────────────────────────────────────────────

    void StartRebind()
    {
        // Cancel any other row that's currently listening
        if (s_currentListener != null && s_currentListener != this)
            s_currentListener.CancelRebind();

        s_currentListener = this;
        if (keyLabel != null) keyLabel.text = "Press a key…";
        if (rebindButton != null) rebindButton.interactable = false;
    }

    void CancelRebind()
    {
        if (s_currentListener == this) s_currentListener = null;
        RefreshDisplay();
        if (rebindButton != null) rebindButton.interactable = true;
    }

    void ApplyKey(KeyCode key)
    {
        SettingsManager.Instance?.SaveKeybind(actionName, key);
        if (s_currentListener == this) s_currentListener = null;
        RefreshDisplay();
        if (rebindButton != null) rebindButton.interactable = true;
    }

    void Update()
    {
        if (s_currentListener != this) return;

        // Esc cancels without applying
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRebind();
            return;
        }

        // Check every KeyCode for a fresh press
        foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (kc == KeyCode.None || kc == KeyCode.Escape) continue;
            if (Input.GetKeyDown(kc))
            {
                ApplyKey(kc);
                return;
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    KeyCode GetCurrentKey()
    {
        var field = typeof(Keybinds).GetField(actionName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (field != null && field.FieldType == typeof(KeyCode))
            return (KeyCode)field.GetValue(null);
        return defaultKey;
    }

    // "SteelPush" → "Steel Push", "DodgeRoll" → "Dodge Roll", etc.
    static string FormatActionName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        var sb = new System.Text.StringBuilder();
        sb.Append(raw[0]);
        for (int i = 1; i < raw.Length; i++)
        {
            if (char.IsUpper(raw[i]) && char.IsLower(raw[i - 1]))
                sb.Append(' ');
            sb.Append(raw[i]);
        }
        return sb.ToString();
    }

    // "LeftShift" → "L-Shift", "LeftControl" → "L-Ctrl", etc.
    static string FormatKeyName(KeyCode kc)
    {
        switch (kc)
        {
            case KeyCode.LeftShift:   return "L-Shift";
            case KeyCode.RightShift:  return "R-Shift";
            case KeyCode.LeftControl: return "L-Ctrl";
            case KeyCode.RightControl:return "R-Ctrl";
            case KeyCode.LeftAlt:     return "L-Alt";
            case KeyCode.RightAlt:    return "R-Alt";
            case KeyCode.Mouse0:      return "LMB";
            case KeyCode.Mouse1:      return "RMB";
            case KeyCode.Mouse2:      return "MMB";
            case KeyCode.Return:      return "Enter";
            case KeyCode.BackQuote:   return "`";
            default:                  return kc.ToString();
        }
    }
}
