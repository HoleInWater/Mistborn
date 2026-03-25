using UnityEngine;

public class WorldInteractions : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public float pickupDistance = 2f;
    public LayerMask interactableLayer;

    [Header("References")]
    public Camera playerCamera;
    public Transform interactPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;
    private Rigidbody heldObjectRb;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteractables();
        HandleInteractions();
    }

    void CheckInteractables()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
            }
        }
        else
        {
            currentInteractable = null;
        }
    }

    void HandleInteractions()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }

        if (Input.GetKeyDown(KeyCode.F) && heldObject == null)
        {
            TryPickup();
        }
        else if (Input.GetKeyDown(KeyCode.F) && heldObject != null)
        {
            DropObject();
        }
    }

    void TryPickup()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupDistance))
        {
            IPickupable pickup = hit.collider.GetComponent<IPickupable>();
            if (pickup != null)
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = heldObject.GetComponent<Rigidbody>();
                
                if (heldObjectRb != null)
                {
                    heldObjectRb.isKinematic = true;
                }
                
                heldObject.transform.SetParent(interactPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void DropObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.linearVelocity = playerCamera.transform.forward * 5f;
        }

        heldObject = null;
        heldObjectRb = null;
    }

    public void ThrowObject(float force)
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.linearVelocity = playerCamera.transform.forward * force;
        }

        heldObject = null;
        heldObjectRb = null;
    }

    public bool HasHeldObject() => heldObject != null;
    public GameObject GetHeldObject() => heldObject;
}

public interface IInteractable
{
    void Interact(GameObject player);
    string GetInteractionText();
}

public interface IPickupable
{
    GameObject GetGameObject();
}

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string interactText = "Press E to interact";
    public bool canInteract = true;
    public bool destroyOnInteract = false;

    [Header("Visuals")]
    public GameObject highlightVisual;
    public Color highlightColor = Color.yellow;

    private Renderer objectRenderer;
    private Color originalColor;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public virtual void Interact(GameObject player)
    {
        if (!canInteract) return;

        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }

    public string GetInteractionText()
    {
        return interactText;
    }

    public void ShowHighlight()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlightColor;
        }
    }

    public void HideHighlight()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
}

public class Door : InteractableObject
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool requiresKey = false;
    public string requiredKeyId = "";
    public Transform hingePoint;

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Update()
    {
        if (isOpen)
            targetAngle = openAngle;
        else
            targetAngle = 0f;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);
        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
    }

    public override void Interact(GameObject player)
    {
        if (requiresKey)
        {
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory != null && inventory.GetItem(requiredKeyId) != null)
            {
                isOpen = !isOpen;
                base.Interact(player);
            }
            else
            {
                Debug.Log("[DOOR] Requires key: " + requiredKeyId);
            }
        }
        else
        {
            isOpen = !isOpen;
            base.Interact(player);
        }
    }
}

public class Lever : InteractableObject
{
    [Header("Lever Settings")]
    public bool isOn = false;
    public Transform leverTransform;
    public float rotationAngle = 45f;

    [Header("Linked Objects")]
    public GameObject[] linkedObjects;

    public override void Interact(GameObject player)
    {
        isOn = !isOn;

        if (leverTransform != null)
        {
            leverTransform.localRotation = Quaternion.Euler(isOn ? rotationAngle : -rotationAngle, 0, 0);
        }

        foreach (GameObject obj in linkedObjects)
        {
            IInteractable interactable = obj.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(player);
            }
        }

        base.Interact(player);
    }
}

public class Book : InteractableObject
{
    [Header("Book Settings")]
    public string title = "";
    [TextArea] public string contents = "";
    public bool hasBeenRead = false;

    public override void Interact(GameObject player)
    {
        hasBeenRead = true;
        Debug.Log($"[BOOK] Reading: {title}");
        base.Interact(player);
    }
}

public class Container : InteractableObject, IPickupable
{
    [Header("Container Settings")]
    public InventoryItem[] contents;
    public bool isEmpty = false;

    public override void Interact(GameObject player)
    {
        if (isEmpty) return;

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            foreach (var item in contents)
            {
                inventory.AddItem(item);
            }
        }

        isEmpty = true;
        Debug.Log("[CONTAINER] Opened container");
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

public class Teleporter : InteractableObject
{
    [Header("Teleport Settings")]
    public Transform destination;
    public float teleportDelay = 0f;
    public bool instant = true;

    public override void Interact(GameObject player)
    {
        if (destination == null) return;

        if (instant)
        {
            player.transform.position = destination.position;
            player.transform.rotation = destination.rotation;
        }
        else
        {
            StartCoroutine(TeleportAfterDelay(player));
        }

        base.Interact(player);
    }

    System.Collections.IEnumerator TeleportAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(teleportDelay);
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
    }
}