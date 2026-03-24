using UnityEngine;

/// <summary>
/// A simple interactable door that can be opened and closed.
/// </summary>
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string doorName = "Door";
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float speed = 2f;
    
    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion targetRot;

    void Start()
    {
        closedRot = transform.localRotation;
        targetRot = closedRot;
    }

    void Update()
    {
        // Smoothly rotate toward target
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
    }

    public void Interact(GameObject player)
    {
        isOpen = !isOpen;
        targetRot = isOpen ? Quaternion.Euler(openRotation) * closedRot : closedRot;
        Debug.Log($"[DOOR] {doorName} is now {(isOpen ? "Open" : "Closed")}");
    }

    public string GetInteractionPrompt()
    {
        return $"Press [F] to {(isOpen ? "Close" : "Open")} {doorName}";
    }

    public bool CanInteract() => true;
}
