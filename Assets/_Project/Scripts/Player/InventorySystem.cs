using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType type;
    public int quantity;
    public bool isStackable;
    public int maxStackSize = 99;
    public float weight;
    public Dictionary<string, float> stats;

    public enum ItemType { Weapon, Armor, Consumable, Material, QuestItem, Metal }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 50;
    public float maxWeight = 100f;

    [Header("State")]
    public List<InventoryItem> items = new List<InventoryItem>();
    public float currentWeight = 0f;

    [Header("UI")]
    public GameObject inventoryPanel;
    public Transform itemGrid;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public bool AddItem(InventoryItem item)
    {
        if (currentWeight + item.weight > maxWeight)
        {
            Debug.Log("[INVENTORY] Too heavy!");
            return false;
        }

        if (item.isStackable)
        {
            InventoryItem existing = items.Find(i => i.itemId == item.itemId);
            if (existing != null)
            {
                existing.quantity = Mathf.Min(existing.quantity + item.quantity, item.maxStackSize);
                return true;
            }
        }

        if (items.Count >= maxSlots)
        {
            Debug.Log("[INVENTORY] Inventory full!");
            return false;
        }

        items.Add(item);
        currentWeight += item.weight * item.quantity;
        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        if (item == null) return false;

        item.quantity -= amount;
        currentWeight -= item.weight;

        if (item.quantity <= 0)
        {
            items.Remove(item);
        }

        return true;
    }

    public InventoryItem GetItem(string itemId)
    {
        return items.Find(i => i.itemId == itemId);
    }

    public List<InventoryItem> GetItemsByType(InventoryItem.ItemType type)
    {
        return items.FindAll(i => i.type == type);
    }

    public int GetItemCount(string itemId)
    {
        InventoryItem item = GetItem(itemId);
        return item != null ? item.quantity : 0;
    }
}

public class ItemPickup : MonoBehaviour
{
    public InventoryItem item;
    public int quantity = 1;
    public float pickUpRange = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Pickup"))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if (Vector3.Distance(transform.position, player.position) <= pickUpRange)
        {
            InventoryItem pickupItem = new InventoryItem
            {
                itemId = item.itemId,
                itemName = item.itemName,
                description = item.description,
                icon = item.icon,
                type = item.type,
                quantity = quantity,
                isStackable = item.isStackable,
                maxStackSize = item.maxStackSize,
                weight = item.weight
            };

            if (Inventory.Instance.AddItem(pickupItem))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
    }
}