using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform inventoryGrid;
    public Transform equipmentSlots;
    public GameObject inventorySlotPrefab;
    
    [Header("Item Details")]
    public GameObject itemDetailsPanel;
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI itemStats;
    public Button useButton;
    public Button equipButton;
    public Button dropButton;
    public Button closeDetailsButton;
    
    [Header("Gold Display")]
    public TextMeshProUGUI goldAmount;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;
    public int maxInventorySlots = 24;
    
    private Inventory inventory;
    private Item selectedItem;
    private static InventoryUI instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        
        SetupButtons();
        InitializeInventorySlots();
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (itemDetailsPanel != null)
        {
            itemDetailsPanel.SetActive(false);
        }
    }
    
    void SetupButtons()
    {
        if (useButton != null)
        {
            useButton.onClick.AddListener(UseSelectedItem);
        }
        
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipSelectedItem);
        }
        
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(DropSelectedItem);
        }
        
        if (closeDetailsButton != null)
        {
            closeDetailsButton.onClick.AddListener(CloseItemDetails);
        }
    }
    
    void InitializeInventorySlots()
    {
        if (inventorySlotPrefab == null || inventoryGrid == null) return;
        
        for (int i = 0; i < maxInventorySlots; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGrid);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            
            if (slot != null)
            {
                slot.slotIndex = i;
                slot.inventoryUI = this;
            }
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryPanel.activeSelf)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }
    
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshInventory();
        Time.timeScale = 0f;
    }
    
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        CloseItemDetails();
        Time.timeScale = 1f;
    }
    
    public void RefreshInventory()
    {
        if (inventory == null) return;
        
        UpdateGoldDisplay();
        RefreshInventorySlots();
    }
    
    void UpdateGoldDisplay()
    {
        if (goldAmount != null && inventory != null)
        {
            goldAmount.text = $"Gold: {inventory.gold}";
        }
    }
    
    void RefreshInventorySlots()
    {
        InventorySlot[] slots = inventoryGrid.GetComponentsInChildren<InventorySlot>();
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                slots[i].SetItem(inventory.items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
    
    public void SelectItem(Item item)
    {
        selectedItem = item;
        ShowItemDetails(item);
    }
    
    void ShowItemDetails(Item item)
    {
        if (itemDetailsPanel == null) return;
        
        itemDetailsPanel.SetActive(true);
        
        if (itemIcon != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        
        if (itemName != null)
        {
            itemName.text = item.itemName;
        }
        
        if (itemDescription != null)
        {
            itemDescription.text = item.description;
        }
        
        if (itemStats != null)
        {
            itemStats.text = GetItemStatsString(item);
        }
        
        UpdateButtonStates(item);
    }
    
    string GetItemStatsString(Item item)
    {
        if (item.stats == null || item.stats.Count == 0)
            return "No stats";
        
        string stats = "";
        foreach (var stat in item.stats)
        {
            string sign = stat.value > 0 ? "+" : "";
            stats += $"{stat.statName}: {sign}{stat.value}\n";
        }
        return stats;
    }
    
    void UpdateButtonStates(Item item)
    {
        if (useButton != null)
        {
            useButton.gameObject.SetActive(item.isUsable);
        }
        
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(item.isEquipable);
        }
        
        if (dropButton != null)
        {
            dropButton.interactable = true;
        }
    }
    
    public void CloseItemDetails()
    {
        selectedItem = null;
        
        if (itemDetailsPanel != null)
        {
            itemDetailsPanel.SetActive(false);
        }
    }
    
    public void UseSelectedItem()
    {
        if (selectedItem == null || inventory == null) return;
        
        if (selectedItem.isUsable)
        {
            inventory.UseItem(selectedItem);
            CloseItemDetails();
            RefreshInventory();
        }
    }
    
    public void EquipSelectedItem()
    {
        if (selectedItem == null || inventory == null) return;
        
        if (selectedItem.isEquipable)
        {
            inventory.EquipItem(selectedItem);
            CloseItemDetails();
            RefreshInventory();
        }
    }
    
    public void DropSelectedItem()
    {
        if (selectedItem == null || inventory == null) return;
        
        inventory.RemoveItem(selectedItem);
        CloseItemDetails();
        RefreshInventory();
    }
}

public class InventorySlot : MonoBehaviour
{
    public int slotIndex;
    public InventoryUI inventoryUI;
    
    [Header("Slot Elements")]
    public Image slotBackground;
    public Image itemIcon;
    public TextMeshProUGUI stackCount;
    public GameObject emptyIndicator;
    
    private Item currentItem;
    
    void Start()
    {
        ClearSlot();
        
        GetComponent<Button>()?.onClick.AddListener(OnSlotClick);
    }
    
    public void SetItem(Item item)
    {
        currentItem = item;
        
        if (itemIcon != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        
        if (stackCount != null)
        {
            if (item.stackCount > 1)
            {
                stackCount.text = item.stackCount.ToString();
                stackCount.gameObject.SetActive(true);
            }
            else
            {
                stackCount.gameObject.SetActive(false);
            }
        }
        
        if (emptyIndicator != null)
        {
            emptyIndicator.SetActive(false);
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }
        
        if (stackCount != null)
        {
            stackCount.gameObject.SetActive(false);
        }
        
        if (emptyIndicator != null)
        {
            emptyIndicator.SetActive(true);
        }
    }
    
    void OnSlotClick()
    {
        if (currentItem != null && inventoryUI != null)
        {
            inventoryUI.SelectItem(currentItem);
        }
    }
}
