using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadialMetalMenu : MonoBehaviour
{
    [Header("UI References")]
    public Canvas radialCanvas;
    public Image selectionRing;
    public Text metalNameText;
    public Text metalDescriptionText;

    [Header("Settings")]
    public float rotationSpeed = 100f;
    public float selectionAngle = 30f;
    public bool slowTimeOnOpen = true;
    public float timeScaleSlow = 0.3f;

    [Header("Metal Icons")]
    public Sprite[] metalIcons;

    private int selectedIndex = 0;
    private bool isOpen = false;
    private float currentAngle = 0f;
    private Allomancer allomancer;
    private MetalSelector metalSelector;

    private readonly string[] metalNames = new string[]
    {
        "Steel", "Iron", "Pewter", "Tin", "Zinc", "Brass",
        "Copper", "Bronze", "Atium", "Malatium", "Gold",
        "Electrum", "Aluminum", "Duralumin", "Bendalloy", "Cadmium"
    };

    private readonly string[] metalDescriptions = new string[]
    {
        "Push metals away from you",
        "Pull metals toward you",
        "Enhanced strength, speed, healing",
        "Enhanced all five senses",
        "Riot emotions (intensify feelings)",
        "Soothe emotions (calm feelings)",
        "Hide Allomantic pulses",
        "Detect Allomantic pulses",
        "See enemy's immediate future",
        "See enemy's future self",
        "See your possible pasts",
        "See your possible futures",
        "Purge all metals instantly",
        "Mega burst - burn all metals at once",
        "Time bubble - speed up",
        "Time bubble - slow down"
    };

    void Start()
    {
        radialCanvas.gameObject.SetActive(false);
        allomancer = GetComponent<Allomancer>();
        metalSelector = GetComponent<MetalSelector>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetAxis("MetalMenu") > 0.5f)
        {
            ToggleMenu();
        }

        if (!isOpen) return;

        HandleRotation();
        UpdateSelection();
        HandleSelection();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CloseMenu();
        }
    }

    void ToggleMenu()
    {
        isOpen = !isOpen;
        radialCanvas.gameObject.SetActive(isOpen);

        if (slowTimeOnOpen && isOpen)
        {
            Time.timeScale = timeScaleSlow;
            Time.fixedDeltaTime = 0.02f * timeScaleSlow;
        }
        else if (!isOpen)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        if (isOpen && metalSelector != null)
        {
            selectedIndex = (int)metalSelector.GetActiveMetal();
            currentAngle = selectedIndex * (360f / 16f);
        }
    }

    void CloseMenu()
    {
        isOpen = false;
        radialCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (metalSelector != null && selectedIndex >= 0 && selectedIndex < 16)
        {
            metalSelector.SetActiveMetalByIndex(selectedIndex);
        }
    }

    void HandleRotation()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            float inputAngle = Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg;
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, inputAngle, rotationSpeed * Time.unscaledDeltaTime);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? 1 : -1;
            selectedIndex = (selectedIndex + direction + 16) % 16;
            currentAngle = selectedIndex * (360f / 16f);
        }
    }

    void UpdateSelection()
    {
        selectedIndex = Mathf.RoundToInt(currentAngle / (360f / 16f)) % 16;
        selectedIndex = (selectedIndex + 16) % 16;

        if (metalNameText != null)
            metalNameText.text = metalNames[selectedIndex];
        if (metalDescriptionText != null)
            metalDescriptionText.text = metalDescriptions[selectedIndex];

        if (selectionRing != null)
        {
            float ringAngle = selectedIndex * (360f / 16f) - 90f;
            selectionRing.transform.rotation = Quaternion.Euler(0, 0, ringAngle);
        }
    }

    void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            CloseMenu();
        }
    }

    public void SelectMetal(int index)
    {
        if (index >= 0 && index < 16)
        {
            selectedIndex = index;
            CloseMenu();
        }
    }
}