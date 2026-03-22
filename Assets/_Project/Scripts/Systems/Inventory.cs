using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 20;
    public List<Item> items = new List<Item>();
    
    public System.Action OnInventoryChanged;
    
    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory full!");
            return false;
        }
        
        items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public bool RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool HasKey(int keyID)
    {
        return items.Exists(i => i.itemType == ItemType.Key && i.keyID == keyID);
    }
    
    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}

[System.Serializable]
public class Item
{
    public string itemName;
    public ItemType itemType;
    public int keyID;
    public int quantity;
    public Sprite icon;
}

public enum ItemType
{
    Key,
    HealthPotion,
    MetalPieces,
    Coin,
    QuestItem
}
