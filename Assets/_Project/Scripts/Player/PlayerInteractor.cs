using UnityEngine;
using TMPro; // Assuming TMP is used for HUD

/// <summary>
/// Handles player-initiated interactions with the world.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    public TextMeshProUGUI promptText; // Interaction HUD element

    private IInteractable currentInteractable;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        interactKey = Keybinds.Interact;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact(gameObject);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract())
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowPrompt(interactable.GetInteractionPrompt());
                }
                return;
            }
        }

        // Nothing found
        if (currentInteractable != null)
        {
            currentInteractable = null;
            HidePrompt();
        }
    }

    private void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
