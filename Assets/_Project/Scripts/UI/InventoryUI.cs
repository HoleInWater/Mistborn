using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Inventory screen UI — shows items, metal vials, coins, and equipment.
/// Toggle with I key. Supports item selection, use, and drop.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemGridContainer;
    public GameObject itemSlotPrefab;
    public Text itemNameText;
    public Text itemDescriptionText;
    public Text currencyText;

    [Header("Metal Vial Display")]
    public Transform vialContainer;
    public Text coinCountText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;

    private Inventory inventory;
    private MetalVialSystem vialSystem;
    private CoinPouch coinPouch;
    private bool isOpen = false;
    private int selectedIndex = -1;

    void Start()
    {
<<<<<<< HEAD
=======
        toggleKey = Keybinds.Inventory;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inventory = player.GetComponent<Inventory>();
            vialSystem = player.GetComponent<MetalVialSystem>();
            coinPouch = player.GetComponent<CoinPouch>();
        }

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            Refresh();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Refresh()
    {
        // Clear existing slots
        if (itemGridContainer != null)
        {
            foreach (Transform child in itemGridContainer)
                Destroy(child.gameObject);
        }

        if (inventory == null) return;

        // Populate item slots
        for (int i = 0; i < inventory.items.Count; i++)
        {
            InventoryItem item = inventory.items[i];
            CreateItemSlot(item, i);
        }

        // Metal vial counts
        if (vialSystem != null && vialContainer != null)
        {
            foreach (Transform child in vialContainer)
                Destroy(child.gameObject);

            foreach (var vial in vialSystem.vials)
            {
                if (vial.quantity <= 0) continue;
                CreateVialDisplay(vial);
            }
        }

        // Coin count
        if (coinPouch != null && coinCountText != null)
            coinCountText.text = $"Coins: {coinPouch.GetCoinCount()}/{coinPouch.GetMaxCoins()}";

        // Currency
        ShopSystem shop = FindObjectOfType<ShopSystem>();
        if (shop != null && currencyText != null)
            currencyText.text = $"Boxings: {shop.GetBoxings()}";

        UpdateSelectedInfo();
    }

    void CreateItemSlot(InventoryItem item, int index)
    {
        if (itemSlotPrefab == null || itemGridContainer == null) return;

        GameObject slot = Instantiate(itemSlotPrefab, itemGridContainer);
        Text text = slot.GetComponentInChildren<Text>();
        if (text != null)
            text.text = $"{item.itemName} x{item.quantity}";

        Image img = slot.GetComponent<Image>();
        if (img != null && item.icon != null)
            img.sprite = item.icon;

        Button btn = slot.GetComponent<Button>();
        if (btn != null)
        {
            int idx = index;
            btn.onClick.AddListener(() => SelectItem(idx));
        }
    }

    void CreateVialDisplay(MetalVialSystem.MetalVial vial)
    {
        if (itemSlotPrefab == null || vialContainer == null) return;

        GameObject slot = Instantiate(itemSlotPrefab, vialContainer);
        Text text = slot.GetComponentInChildren<Text>();
        if (text != null)
            text.text = $"{vial.metalType} x{vial.quantity}";
    }

    void SelectItem(int index)
    {
        selectedIndex = index;
        UpdateSelectedInfo();
    }

    void UpdateSelectedInfo()
    {
        if (inventory == null || selectedIndex < 0 || selectedIndex >= inventory.items.Count)
        {
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescriptionText != null) itemDescriptionText.text = "Select an item";
            return;
        }

        InventoryItem item = inventory.items[selectedIndex];
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;
    }

    public void UseSelectedItem()
    {
        if (inventory == null || selectedIndex < 0 || selectedIndex >= inventory.items.Count) return;
        InventoryItem item = inventory.items[selectedIndex];

        if (item.type == InventoryItem.ItemType.Consumable)
        {
            inventory.RemoveItem(item.itemId);
            Refresh();
        }
    }

    public void DropSelectedItem()
    {
        if (inventory == null || selectedIndex < 0 || selectedIndex >= inventory.items.Count) return;
        inventory.RemoveItem(inventory.items[selectedIndex].itemId);
        selectedIndex = -1;
        Refresh();
    }

    public bool IsOpen() => isOpen;
}
