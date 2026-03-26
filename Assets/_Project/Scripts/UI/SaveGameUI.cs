using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Save/Load screen UI. Shows 5 save slots with timestamps.
/// Accessible from pause menu.
/// </summary>
public class SaveGameUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject savePanel;
    public Transform slotContainer;
    public GameObject slotPrefab;
    public Text statusText;

    [Header("Settings")]
    public int maxSlots = 5;

    private bool isOpen = false;
    private bool isSaveMode = true;

    void Start()
    {
        if (savePanel != null) savePanel.SetActive(false);
    }

    public void OpenSaveMenu()
    {
        isSaveMode = true;
        Open();
    }

    public void OpenLoadMenu()
    {
        isSaveMode = false;
        Open();
    }

    void Open()
    {
        isOpen = true;
        if (savePanel != null) savePanel.SetActive(true);
        RefreshSlots();
    }

    public void Close()
    {
        isOpen = false;
        if (savePanel != null) savePanel.SetActive(false);
    }

    void RefreshSlots()
    {
        if (slotContainer == null) return;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < maxSlots; i++)
        {
            CreateSlotUI(i);
        }
    }

    void CreateSlotUI(int slotIndex)
    {
        if (SaveLoadManager.Instance == null) return;

        bool hasSave = SaveLoadManager.Instance.HasSave(slotIndex);
        string label;

        if (hasSave)
        {
            var data = SaveLoadManager.Instance.PeekSaveData(slotIndex);
            if (data != null)
            {
                string timeStr = System.DateTime.TryParse(data.saveTimeString,
                    null, System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime dt)
                    ? dt.ToString("MMM d, yyyy  h:mm tt")
                    : data.saveTimeString;
                label = $"Slot {slotIndex + 1}  •  {data.saveName}  •  Ch.{data.chapterIndex + 1}  •  {timeStr}";
            }
            else
            {
                label = $"Slot {slotIndex + 1}  •  Save Data";
            }
        }
        else
        {
            label = $"Slot {slotIndex + 1}  •  Empty";
        }

        if (slotPrefab != null)
        {
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            Text text = slot.GetComponentInChildren<Text>();
            if (text != null) text.text = label;

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                int idx = slotIndex;
                btn.onClick.AddListener(() => OnSlotClicked(idx));
            }
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        if (SaveLoadManager.Instance == null) return;

        if (isSaveMode)
        {
            SaveLoadManager.Instance.SaveGame(slotIndex, $"Save {slotIndex + 1}");
            if (statusText != null) statusText.text = "Game Saved!";
            NotificationSystem.Instance?.ShowNotification("Game Saved");
        }
        else
        {
            if (SaveLoadManager.Instance.HasSave(slotIndex))
            {
                SaveLoadManager.Instance.LoadGame(slotIndex);
                if (statusText != null) statusText.text = "Game Loaded!";
                NotificationSystem.Instance?.ShowNotification("Game Loaded");
                Close();
            }
            else
            {
                if (statusText != null) statusText.text = "No save in this slot.";
            }
        }

        RefreshSlots();
    }

    /// <summary>
    /// Peek at save data without loading it.
    /// </summary>
    public SaveLoadManager.SaveData PeekSaveData(int slot)
    {
        return SaveLoadManager.Instance?.PeekSaveData(slot);
    }
}
