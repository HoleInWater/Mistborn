using UnityEngine;

/// <summary>
/// Legacy save system — now delegates to SaveLoadManager for unified persistence.
/// Kept for API compatibility with existing code that references SaveSystem.Instance.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void SaveGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(0, "QuickSave");
        }
        else
        {
            Debug.LogWarning("[SAVE] SaveLoadManager not found — cannot save.");
        }
    }

    public void LoadGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame(0);
        }
        else
        {
            Debug.LogWarning("[SAVE] SaveLoadManager not found — cannot load.");
        }
    }
}
