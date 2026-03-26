using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Context-sensitive radial interaction wheel. Shows available actions
/// based on what the player is looking at (enemy, NPC, interactable, ally).
/// Hold middle mouse or E to open, release to select.
/// </summary>
public class InteractionWheel : MonoBehaviour
{
    [Header("UI")]
    public GameObject wheelPanel;
    public RectTransform wheelCenter;
    public Text actionNameText;
    public Text actionDescriptionText;
    public GameObject optionPrefab;

    [Header("Settings")]
    public float wheelRadius = 120f;
    public KeyCode openKey = KeyCode.E;
    public float holdTimeToOpen = 0.3f;
    public float detectionRange = 5f;

    [Header("References")]
    public Camera playerCamera;
    public Allomancer allomancer;

    private bool isOpen = false;
    private float holdTimer = 0f;
    private int selectedIndex = -1;
    private List<WheelOption> currentOptions = new List<WheelOption>();
    private List<GameObject> optionInstances = new List<GameObject>();
    private GameObject targetObject;

    public struct WheelOption
    {
        public string name;
        public string description;
        public System.Action onSelect;
    }

    void Start()
    {
        openKey = Keybinds.Interact;
        if (playerCamera == null) playerCamera = Camera.main;
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (wheelPanel != null) wheelPanel.SetActive(false);
    }

    void Update()
    {
        // Hold E to open wheel
        if (Input.GetKey(openKey))
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdTimeToOpen && !isOpen)
                OpenWheel();
        }

        if (Input.GetKeyUp(openKey))
        {
            if (isOpen)
                SelectAndClose();
            else if (holdTimer < holdTimeToOpen)
                QuickInteract();

            holdTimer = 0f;
        }

        if (isOpen)
            UpdateSelection();
    }

    void QuickInteract()
    {
        // Quick press E — interact with nearest object
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, detectionRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact(gameObject);
            }
        }
    }

    void OpenWheel()
    {
        // Detect what's in front of the player
        currentOptions.Clear();
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, detectionRange))
        {
            targetObject = hit.collider.gameObject;
            BuildOptionsForTarget(targetObject);
        }

        if (currentOptions.Count == 0)
        {
            // Default options
            currentOptions.Add(new WheelOption { name = "Drink Vial", description = "Replenish metal reserves",
                onSelect = () => GetComponent<MetalVialSystem>()?.DrinkVialForMetal(allomancer.GetCurrentMetal()) });
            currentOptions.Add(new WheelOption { name = "Recover Coins", description = "Pick up nearby coins",
                onSelect = () => {} });
        }

        isOpen = true;
        if (wheelPanel != null) wheelPanel.SetActive(true);
        Time.timeScale = 0.15f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PopulateWheelUI();
    }

    void BuildOptionsForTarget(GameObject target)
    {
        // NPC
        IInteractable interactable = target.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentOptions.Add(new WheelOption { name = "Talk", description = interactable.GetInteractionPrompt(),
                onSelect = () => interactable.Interact(gameObject) });
        }

        // Enemy
        EnemyAI enemy = target.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            currentOptions.Add(new WheelOption { name = "Steel Push", description = "Push enemy away",
                onSelect = () => {} });
            currentOptions.Add(new WheelOption { name = "Iron Pull", description = "Pull enemy toward you",
                onSelect = () => {} });

            if (target.GetComponent<KolossAI>() != null)
            {
                currentOptions.Add(new WheelOption { name = "Remove Spike", description = "Pull a Hemalurgic spike",
                    onSelect = () => target.GetComponent<KolossAI>()?.RemoveSpike() });
            }

            if (target.GetComponent<SteelInquisitorAI>() != null)
            {
                var inq = target.GetComponent<SteelInquisitorAI>();
                if (inq.CanRemoveLinchpin(transform))
                {
                    currentOptions.Add(new WheelOption { name = "Remove Linchpin", description = "Instant kill!",
                        onSelect = () => inq.TryRemoveLinchpin(transform) });
                }
            }
        }

        // Loot
        LootDrop loot = target.GetComponent<LootDrop>();
        if (loot != null)
        {
            currentOptions.Add(new WheelOption { name = "Loot", description = "Pick up items",
                onSelect = () => {} });
        }

        // Companion
        CompanionManager cm = CompanionManager.Instance;
        if (cm != null && enemy != null)
        {
            currentOptions.Add(new WheelOption { name = "Command Ally", description = "Order companion to attack",
                onSelect = () => cm.RequestCombatSupport(target.transform) });
        }
    }

    void PopulateWheelUI()
    {
        foreach (var inst in optionInstances) Destroy(inst);
        optionInstances.Clear();

        if (optionPrefab == null || wheelCenter == null) return;

        float angleStep = 360f / Mathf.Max(currentOptions.Count, 1);
        for (int i = 0; i < currentOptions.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * wheelRadius;

            GameObject opt = Instantiate(optionPrefab, wheelCenter);
            opt.GetComponent<RectTransform>().anchoredPosition = pos;
            Text text = opt.GetComponentInChildren<Text>();
            if (text != null) text.text = currentOptions[i].name;

            optionInstances.Add(opt);
        }
    }

    void UpdateSelection()
    {
        if (wheelCenter == null) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 center = wheelCenter.position;
        Vector2 dir = mousePos - center;

        if (dir.magnitude < 30f) { selectedIndex = -1; UpdateInfo(); return; }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float step = 360f / Mathf.Max(currentOptions.Count, 1);
        selectedIndex = Mathf.FloorToInt(angle / step) % currentOptions.Count;
        UpdateInfo();
    }

    void UpdateInfo()
    {
        if (selectedIndex >= 0 && selectedIndex < currentOptions.Count)
        {
            if (actionNameText != null) actionNameText.text = currentOptions[selectedIndex].name;
            if (actionDescriptionText != null) actionDescriptionText.text = currentOptions[selectedIndex].description;
        }
        else
        {
            if (actionNameText != null) actionNameText.text = "";
            if (actionDescriptionText != null) actionDescriptionText.text = "";
        }
    }

    void SelectAndClose()
    {
        if (selectedIndex >= 0 && selectedIndex < currentOptions.Count)
            currentOptions[selectedIndex].onSelect?.Invoke();

        isOpen = false;
        if (wheelPanel != null) wheelPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentOptions.Clear();
        selectedIndex = -1;
    }

    public bool IsOpen() => isOpen;
}
