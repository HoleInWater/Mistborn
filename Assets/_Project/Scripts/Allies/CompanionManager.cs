using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's active AI companions.
/// </summary>
public class CompanionManager : MonoBehaviour
{
    public static CompanionManager Instance { get; private set; }

    [Header("Active Companion")]
    public GameObject currentCompanion;
    public Transform companionFollowTarget; // Usually an offset behind the player

    [Header("Settings")]
    public float followDistance = 2.5f;
    public float maxCombatDistance = 15f;
    
    private List<GameObject> availableCompanions = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void RegisterCompanion(GameObject companion)
    {
        if (!availableCompanions.Contains(companion))
            availableCompanions.Add(companion);
    }

    public void SwitchCompanion(int index)
    {
        if (index < 0 || index >= availableCompanions.Count) return;

        if (currentCompanion != null) 
            currentCompanion.SetActive(false);

        currentCompanion = availableCompanions[index];
        currentCompanion.SetActive(true);
        
        Debug.Log($"[COMPANION] {currentCompanion.name} is now following you.");
    }

    public void RequestCombatSupport(Transform enemyTarget)
    {
        if (currentCompanion == null) return;

        // Message the active companion script
        currentCompanion.SendMessage("OnSupportRequested", enemyTarget, SendMessageOptions.DontRequireReceiver);
    }
}
