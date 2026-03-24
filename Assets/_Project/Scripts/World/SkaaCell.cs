using UnityEngine;

/// <summary>
/// A hidden hub for rebels in a specific city district.
/// </summary>
public class SkaaCell : MonoBehaviour, IInteractable
{
    public string cellName = "Luthadel Sub-District 4";
    public int cellStrength = 10;
    public float suspicionLevel = 0f;

    [Header("Missions")]
    public ObjectiveData[] availableMissions;

    public void Interact(GameObject player)
    {
        if (suspicionLevel > 80f)
        {
            Debug.Log("[CELL] Too high suspicion! We cannot talk now.");
            return;
        }

        // Trigger recruitment logic or mission handout
        if (availableMissions.Length > 0 && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AddObjective(availableMissions[Random.Range(0, availableMissions.Length)]);
        }
    }

    public string GetInteractionPrompt() => $"Press [F] to enter {cellName} Hideout";
    public bool CanInteract() => suspicionLevel < 90f;

    /// <summary>
    /// Called when the player brings "Heat" near the cell.
    /// </summary>
    public void IncreaseSuspicion(float val)
    {
        suspicionLevel = Mathf.Clamp(suspicionLevel + val, 0f, 100f);
    }
}
