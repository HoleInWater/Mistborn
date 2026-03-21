using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Player
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public int quantity;
        public int maxStack;
        public ItemType type;
        public bool isKeyItem;
        public Dictionary<string, float> stats = new Dictionary<string, float>();

        public InventoryItem(string id, string name, int qty = 1)
        {
            itemId = id;
            itemName = name;
            quantity = qty;
        }
    }

    public enum ItemType
    {
        Consumable,
        Material,
        KeyItem,
        QuestItem,
        Metal
    }

    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Inventory Settings")]
        [SerializeField] private int m_maxSlots = 24;
        [SerializeField] private int m_goldCapacity = 99999;

        [Header("Starting Items")]
        [SerializeField] private string[] m_startingItemIds;
        [SerializeField] private int[] m_startingQuantities;

        private List<InventoryItem> m_items = new List<InventoryItem>();
        private int m_gold;
        private Dictionary<string, InventoryItem> m_itemLookup = new Dictionary<string, InventoryItem>();

        public event System.Action<InventoryItem> OnItemAdded;
        public event System.Action<InventoryItem> OnItemRemoved;
        public event System.Action<InventoryItem, int> OnItemQuantityChanged;
        public event System.Action<int> OnGoldChanged;

        public int Gold => m_gold;
        public int UsedSlots => m_items.Count;
        public int MaxSlots => m_maxSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadInventory();
        }

        private void Start()
        {
            SetupStartingItems();
        }

        private void SetupStartingItems()
        {
            if (m_startingItemIds == null) return;

            for (int i = 0; i < m_startingItemIds.Length; i++)
            {
                string itemId = m_startingItemIds[i];
                int qty = i < m_startingQuantities.Length ? m_startingQuantities[i] : 1;

                if (!HasItem(itemId))
                {
                    AddItem(itemId, qty);
                }
            }
        }

        public bool AddItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0) return false;

            if (m_itemLookup.TryGetValue(itemId, out InventoryItem existing))
            {
                int spaceAvailable = existing.maxStack - existing.quantity;
                int toAdd = Mathf.Min(quantity, spaceAvailable);

                if (toAdd > 0)
                {
                    existing.quantity += toAdd;
                    OnItemQuantityChanged?.Invoke(existing, existing.quantity);

                    if (toAdd < quantity)
                    {
                        return AddNewItem(itemId, quantity - toAdd);
                    }
                    return true;
                }
            }
            else
            {
                return AddNewItem(itemId, quantity);
            }

            return false;
        }

        private bool AddNewItem(string itemId, int quantity)
        {
            if (m_items.Count >= m_maxSlots) return false;

            InventoryItem item = CreateItem(itemId, quantity);
            if (item == null) return false;

            m_items.Add(item);
            m_itemLookup[itemId] = item;
            OnItemAdded?.Invoke(item);

            return true;
        }

        private InventoryItem CreateItem(string itemId, int quantity)
        {
            InventoryItem item = new InventoryItem(itemId, itemId, quantity);

            switch (itemId)
            {
                case "coin":
                    item.itemName = "Coin";
                    item.type = ItemType.Material;
                    item.maxStack = 999;
                    break;
                case "brace":
                    item.itemName = "Metal Brace";
                    item.type = ItemType.Metal;
                    item.maxStack = 50;
                    break;
                case "health_potion":
                    item.itemName = "Health Potion";
                    item.type = ItemType.Consumable;
                    item.maxStack = 10;
                    item.stats["heal_amount"] = 25f;
                    break;
                case "metal_vial":
                    item.itemName = "Metal Vial";
                    item.type = ItemType.Consumable;
                    item.maxStack = 20;
                    break;
                default:
                    item.maxStack = 99;
                    break;
            }

            item.quantity = Mathf.Min(quantity, item.maxStack);
            return item;
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (!m_itemLookup.TryGetValue(itemId, out InventoryItem item)) return false;

            if (item.quantity < quantity) return false;

            item.quantity -= quantity;
            OnItemQuantityChanged?.Invoke(item, item.quantity);

            if (item.quantity <= 0)
            {
                m_items.Remove(item);
                m_itemLookup.Remove(itemId);
                OnItemRemoved?.Invoke(item);
            }

            return true;
        }

        public bool HasItem(string itemId, int requiredQuantity = 1)
        {
            if (!m_itemLookup.TryGetValue(itemId, out InventoryItem item)) return false;
            return item.quantity >= requiredQuantity;
        }

        public InventoryItem GetItem(string itemId)
        {
            m_itemLookup.TryGetValue(itemId, out InventoryItem item);
            return item;
        }

        public List<InventoryItem> GetAllItems()
        {
            return new List<InventoryItem>(m_items);
        }

        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            List<InventoryItem> result = new List<InventoryItem>();
            foreach (InventoryItem item in m_items)
            {
                if (item.type == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            m_gold = Mathf.Min(m_gold + amount, m_goldCapacity);
            OnGoldChanged?.Invoke(m_gold);
        }

        public bool RemoveGold(int amount)
        {
            if (amount <= 0 || m_gold < amount) return false;

            m_gold -= amount;
            OnGoldChanged?.Invoke(m_gold);
            return true;
        }

        public void UseItem(string itemId)
        {
            if (!m_itemLookup.TryGetValue(itemId, out InventoryItem item)) return;

            switch (item.type)
            {
                case ItemType.Consumable:
                    UseConsumable(item);
                    break;
            }
        }

        private void UseConsumable(InventoryItem item)
        {
            switch (item.itemId)
            {
                case "health_potion":
                    PlayerHealth health = GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        float healAmount = item.stats.ContainsKey("heal_amount") ? item.stats["heal_amount"] : 25f;
                        health.Heal(healAmount);
                    }
                    break;
            }

            RemoveItem(item.itemId, 1);
        }

        public void ClearInventory()
        {
            m_items.Clear();
            m_itemLookup.Clear();
            m_gold = 0;
        }

        public void SaveInventory()
        {
            SaveLoadSystem.SaveObject("PlayerInventory", this);
        }

        private void LoadInventory()
        {
            var data = SaveLoadSystem.LoadObject<InventorySaveData>("PlayerInventory");
            if (data != null)
            {
                m_gold = data.gold;
                m_items = data.items;
                m_itemLookup.Clear();
                foreach (var item in m_items)
                {
                    m_itemLookup[item.itemId] = item;
                }
            }
        }

        private void OnDestroy()
        {
            SaveInventory();
        }
    }

    [Serializable]
    public class InventorySaveData
    {
        public int gold;
        public List<InventoryItem> items;
    }
}
