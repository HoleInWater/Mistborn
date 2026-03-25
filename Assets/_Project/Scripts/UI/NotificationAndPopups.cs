using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("UI")]
    public GameObject notificationPanel;
    public Transform notificationContainer;
    public GameObject notificationPrefab;

    [Header("Settings")]
    public float defaultDuration = 3f;
    public int maxNotifications = 5;
    public float fadeTime = 0.5f;

    private Queue<Notification> notificationQueue = new Queue<Notification>();
    private List<NotificationUI> activeNotifications = new List<NotificationUI>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        if (notificationPanel != null) notificationPanel.SetActive(true);
    }

    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = -1)
    {
        if (duration < 0) duration = defaultDuration;

        Notification notification = new Notification
        {
            title = title,
            message = message,
            type = type,
            duration = duration
        };

        if (activeNotifications.Count >= maxNotifications)
        {
            notificationQueue.Enqueue(notification);
        }
        else
        {
            DisplayNotification(notification);
        }
    }

    void DisplayNotification(Notification notification)
    {
        if (notificationPrefab == null || notificationContainer == null) return;

        GameObject obj = Instantiate(notificationPrefab, notificationContainer);
        NotificationUI ui = obj.GetComponent<NotificationUI>();

        if (ui != null)
        {
            ui.Initialize(notification);
            activeNotifications.Add(ui);
        }

        StartCoroutine(RemoveAfterDelay(ui, notification.duration));
    }

    IEnumerator RemoveAfterDelay(NotificationUI ui, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ui != null)
        {
            activeNotifications.Remove(ui);
            Destroy(ui.gameObject);
        }

        if (notificationQueue.Count > 0)
        {
            DisplayNotification(notificationQueue.Dequeue());
        }
    }

    public void ShowWarning(string message)
    {
        ShowNotification("Warning", message, NotificationType.Warning);
    }

    public void ShowError(string message)
    {
        ShowNotification("Error", message, NotificationType.Error);
    }

    public void ShowSuccess(string message)
    {
        ShowNotification("Success", message, NotificationType.Success);
    }

    public void ShowInfo(string message)
    {
        ShowNotification("Info", message, NotificationType.Info);
    }
}

[System.Serializable]
public class Notification
{
    public string title;
    public string message;
    public NotificationType type;
    public float duration;
}

public enum NotificationType { Info, Warning, Error, Success, Achievement, Quest }

public class NotificationUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image backgroundImage;
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image progressBar;

    private Notification notification;
    private float timer;

    public void Initialize(Notification notif)
    {
        notification = notif;

        if (titleText != null) titleText.text = notif.title;
        if (messageText != null) messageText.text = notif.message;

        SetColorByType(notif.type);
        SetIconByType(notif.type);
    }

    void SetColorByType(NotificationType type)
    {
        if (backgroundImage == null) return;

        switch (type)
        {
            case NotificationType.Info:
                backgroundImage.color = new Color(0.2f, 0.5f, 0.8f, 0.9f);
                break;
            case NotificationType.Warning:
                backgroundImage.color = new Color(0.9f, 0.6f, 0.1f, 0.9f);
                break;
            case NotificationType.Error:
                backgroundImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
                break;
            case NotificationType.Success:
                backgroundImage.color = new Color(0.2f, 0.7f, 0.3f, 0.9f);
                break;
            case NotificationType.Achievement:
                backgroundImage.color = new Color(0.9f, 0.8f, 0.2f, 0.9f);
                break;
            case NotificationType.Quest:
                backgroundImage.color = new Color(0.5f, 0.3f, 0.7f, 0.9f);
                break;
        }
    }

    void SetIconByType(NotificationType type)
    {
        if (iconImage == null) return;
        // Icons would be assigned via inspector or code
    }

    void Update()
    {
        if (notification == null) return;

        timer += Time.deltaTime;
        if (progressBar != null)
        {
            progressBar.fillAmount = timer / notification.duration;
        }
    }
}

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;
    public LayoutGroup layoutGroup;

    [Header("Settings")]
    public float showDelay = 0.5f;
    public Vector2 offset = new Vector2(10, -10);

    private bool isShowing = false;
    private Coroutine showCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (isShowing)
        {
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = mousePos + offset;
        }
    }

    public void ShowTooltip(string title, string description, float delay = -1)
    {
        if (delay < 0) delay = showDelay;

        if (showCoroutine != null) StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(ShowAfterDelay(title, description, delay));
    }

    IEnumerator ShowAfterDelay(string title, string description, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        tooltipPanel?.SetActive(true);
        isShowing = true;

        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());
        }
    }

    public void HideTooltip()
    {
        if (showCoroutine != null) StopCoroutine(showCoroutine);

        tooltipPanel?.SetActive(false);
        isShowing = false;
    }

    public void ShowItemTooltip(InventoryItem item)
    {
        string title = item.itemName;
        string description = item.description;

        ShowTooltip(title, description);
    }

    public void ShowMetalTooltip(AllomancySkill.MetalType metal)
    {
        string title = metal.ToString();
        string description = GetMetalDescription(metal);

        ShowTooltip(title, description);
    }

    string GetMetalDescription(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel: return "Push metals away from you";
            case AllomancySkill.MetalType.Iron: return "Pull metals toward you";
            case AllomancySkill.MetalType.Pewter: return "Enhanced strength, speed, healing";
            case AllomancySkill.MetalType.Tin: return "Enhanced all five senses";
            case AllomancySkill.MetalType.Zinc: return "Riot emotions (intensify feelings)";
            case AllomancySkill.MetalType.Brass: return "Soothe emotions (calm feelings)";
            case AllomancySkill.MetalType.Copper: return "Hide Allomantic pulses";
            case AllomancySkill.MetalType.Bronze: return "Detect Allomantic pulses";
            case AllomancySkill.MetalType.Atium: return "See enemy's immediate future";
            case AllomancySkill.MetalType.Gold: return "See your possible pasts";
            case AllomancySkill.MetalType.Electrum: return "See your possible futures";
            case AllomancySkill.MetalType.Aluminum: return "Purge all metals instantly";
            case AllomancySkill.MetalType.Duralumin: return "Burn all metals at once (mega burst)";
            case AllomancySkill.MetalType.Bendalloy: return "Speed time bubble";
            case AllomancySkill.MetalType.Cadmium: return "Slow time bubble";
            case AllomancySkill.MetalType.Malatium: return "See person's potential future self";
            case AllomancySkill.MetalType.Chromium: return "Wipe enemy's metal reserves";
            case AllomancySkill.MetalType.Nicrosil: return "Amplify ally's metal burning";
            default: return "";
        }
    }

    public bool IsShowing() => isShowing;
}

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image popupImage;
    public UnityEngine.UI.Button confirmButton;
    public UnityEngine.UI.Button cancelButton;
    public GameObject customContent;

    private System.Action onConfirm;
    private System.Action onCancel;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        if (popupPanel != null) popupPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    public void ShowPopup(string title, string message, System.Action onConfirm = null, System.Action onCancel = null)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        if (cancelButton != null) cancelButton.gameObject.SetActive(onCancel != null);

        popupPanel?.SetActive(true);
    }

    public void ShowConfirm(string title, string message, System.Action onConfirm)
    {
        ShowPopup(title, message, onConfirm, () => HidePopup());
    }

    public void ShowInfo(string title, string message)
    {
        ShowPopup(title, message, () => HidePopup(), null);
    }

    public void ShowQuitConfirm()
    {
        ShowPopup("Quit", "Are you sure you want to quit?", () => Application.Quit(), () => HidePopup());
    }

    public void ShowSaveConfirm()
    {
        ShowPopup("Save", "Save your progress?", () =>
        {
            SaveLoadManager.Instance?.SaveGame(0, "Auto Save");
            HidePopup();
        }, () => HidePopup());
    }

    public void ShowCustomPopup(string title, string message, GameObject content, System.Action onConfirm = null)
    {
        this.onConfirm = onConfirm;

        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        if (customContent != null)
        {
            foreach (Transform child in customContent.transform)
            {
                Destroy(child.gameObject);
            }

            if (content != null)
            {
                GameObject obj = Instantiate(content, customContent.transform);
            }
        }

        popupPanel?.SetActive(true);
    }

    void OnConfirm()
    {
        onConfirm?.Invoke();
        HidePopup();
    }

    void OnCancel()
    {
        onCancel?.Invoke();
        HidePopup();
    }

    public void HidePopup()
    {
        popupPanel?.SetActive(false);
        onConfirm = null;
        onCancel = null;
    }

    public bool IsPopupOpen()
    {
        return popupPanel != null && popupPanel.activeSelf;
    }
}