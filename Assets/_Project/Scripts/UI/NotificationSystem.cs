using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Notification queue — shows popups for achievements, quest updates, pickups, etc.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }

    [Header("UI")]
    public GameObject notificationPrefab;
    public Transform notificationContainer;
    public float displayDuration = 3f;
    public float fadeSpeed = 2f;
    public int maxVisibleNotifications = 3;

    private Queue<string> pendingNotifications = new Queue<string>();
    private int activeCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void ShowNotification(string message)
    {
        pendingNotifications.Enqueue(message);
        TryShowNext();
    }

    void TryShowNext()
    {
        if (activeCount >= maxVisibleNotifications || pendingNotifications.Count == 0) return;
        string msg = pendingNotifications.Dequeue();
        StartCoroutine(DisplayNotification(msg));
    }

    IEnumerator DisplayNotification(string message)
    {
        activeCount++;

        GameObject notif = null;
        if (notificationPrefab != null && notificationContainer != null)
        {
            notif = Instantiate(notificationPrefab, notificationContainer);
            Text text = notif.GetComponentInChildren<Text>();
            if (text != null) text.text = message;
        }

<<<<<<< HEAD
        yield return new WaitForSeconds(displayDuration);

        // Fade out
=======
        // WaitForSecondsRealtime so notifications still appear while the game is paused
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out (also realtime so it completes during pause)
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        if (notif != null)
        {
            CanvasGroup cg = notif.GetComponent<CanvasGroup>();
            if (cg == null) cg = notif.AddComponent<CanvasGroup>();

            float alpha = 1f;
            while (alpha > 0f)
            {
<<<<<<< HEAD
                alpha -= Time.deltaTime * fadeSpeed;
=======
                alpha -= Time.unscaledDeltaTime * fadeSpeed;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
                cg.alpha = alpha;
                yield return null;
            }
            Destroy(notif);
        }

        activeCount--;
        TryShowNext();
    }

    // Convenience methods
    public void ShowAchievement(string title) => ShowNotification($"Achievement Unlocked: {title}");
    public void ShowQuestUpdate(string text) => ShowNotification($"Quest: {text}");
    public void ShowPickup(string itemName) => ShowNotification($"Picked up: {itemName}");
    public void ShowLoreEntry(string title) => ShowNotification($"Lore Discovered: {title}");
}
