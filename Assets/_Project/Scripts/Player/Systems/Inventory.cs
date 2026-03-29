using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[PlayerComponent("Progression", order: 20)]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public string description;
    public ItemType type;
    public int quantity = 1;
    public int maxStack = 99;
    public float weight = 0.1f;
    public Sprite icon;

    public enum ItemType { Metal, Key, Weapon, Armor, Consumable, Quest, Lore }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 30;
    public float maxWeight = 100f;

    [Header("Items")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Keys (legacy)")]
    public int[] ownedKeys = new int[0];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddItem(InventoryItem item)
    {
        if (item == null) return false;

        // Try stacking
        InventoryItem existing = items.Find(i => i.itemId == item.itemId && i.quantity < i.maxStack);
        if (existing != null)
        {
            existing.quantity += item.quantity;
            return true;
        }

        if (items.Count >= maxSlots) return false;
        items.Add(item);
        return true;
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        if (item == null) return false;

        item.quantity -= quantity;
        if (item.quantity <= 0) items.Remove(item);
        return true;
    }

    public InventoryItem GetItem(string itemId)
    {
        return items.Find(i => i.itemId == itemId);
    }

    public int GetItemCount(string itemId)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        return item != null ? item.quantity : 0;
    }

    public bool HasItem(string itemId) => GetItem(itemId) != null;

    // Legacy key support
    public bool HasKey(int keyID)
    {
        foreach (int key in ownedKeys)
            if (key == keyID) return true;
        return HasItem($"key_{keyID}");
    }

    public void AddKey(int keyID)
    {
        int[] newKeys = new int[ownedKeys.Length + 1];
        System.Array.Copy(ownedKeys, newKeys, ownedKeys.Length);
        newKeys[ownedKeys.Length] = keyID;
        ownedKeys = newKeys;
    }
}
