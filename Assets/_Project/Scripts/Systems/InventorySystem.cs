using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySlots = 20;
        [SerializeField] private int maxStackSize = 99;
        
        [Header("Starting Items")]
        [SerializeField] private List<InventoryItem> startingItems = new List<InventoryItem>();
        
        [Header("Weight System")]
        [SerializeField] private bool useWeightSystem = false;
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private float currentWeight = 0f;
        
        private Dictionary<string, InventoryItem> inventory = new Dictionary<string, InventoryItem>();
        private List<string> itemKeys = new List<string>();
        
        [System.Serializable]
        public class InventoryItem
        {
            public string itemName;
            public int quantity;
            public float weight;
            public GameObject itemPrefab;
            public Sprite itemIcon;
            public string itemDescription;
        }
        
        private void Start()
        {
            InitializeStartingItems();
        }
        
        private void InitializeStartingItems()
        {
            foreach (InventoryItem item in startingItems)
            {
                AddItem(item.itemName, item.quantity);
            }
        }
        
        public bool AddItem(string itemName, int quantity = 1)
        {
            if (inventory.ContainsKey(itemName))
            {
                int newQuantity = inventory[itemName].quantity + quantity;
                
                if (newQuantity > maxStackSize)
                {
                    return false;
                }
                
                inventory[itemName].quantity = newQuantity;
                
                if (useWeightSystem)
                {
                    currentWeight += inventory[itemName].weight * quantity;
                }
            }
            else
            {
                if (inventory.Count >= maxInventorySlots)
                {
                    return false;
                }
                
                InventoryItem newItem = CreateNewItem(itemName, quantity);
                inventory.Add(itemName, newItem);
                itemKeys.Add(itemName);
                
                if (useWeightSystem)
                {
                    currentWeight += newItem.weight * quantity;
                }
            }
            
            if (OnInventoryChanged != null)
            {
                OnInventoryChanged(itemName, quantity, true);
            }
            
            return true;
        }
        
        public bool RemoveItem(string itemName, int quantity = 1)
        {
            if (!inventory.ContainsKey(itemName))
            {
                return false;
            }
            
            if (inventory[itemName].quantity < quantity)
            {
                return false;
            }
            
            if (useWeightSystem)
            {
                currentWeight -= inventory[itemName].weight * quantity;
                currentWeight = Mathf.Max(0f, currentWeight);
            }
            
            inventory[itemName].quantity -= quantity;
            
            if (inventory[itemName].quantity <= 0)
            {
                inventory.Remove(itemName);
                itemKeys.Remove(itemName);
            }
            
            if (OnInventoryChanged != null)
            {
                OnInventoryChanged(itemName, quantity, false);
            }
            
            return true;
        }
        
        public bool HasItem(string itemName, int quantity = 1)
        {
            if (!inventory.ContainsKey(itemName))
            {
                return false;
            }
            
            return inventory[itemName].quantity >= quantity;
        }
        
        public int GetItemQuantity(string itemName)
        {
            if (inventory.ContainsKey(itemName))
            {
                return inventory[itemName].quantity;
            }
            
            return 0;
        }
        
        public InventoryItem GetItem(string itemName)
        {
            if (inventory.ContainsKey(itemName))
            {
                return inventory[itemName];
            }
            
            return null;
        }
        
        public List<string> GetAllItemNames()
        {
            return new List<string>(itemKeys);
        }
        
        public Dictionary<string, InventoryItem> GetAllItems()
        {
            return new Dictionary<string, InventoryItem>(inventory);
        }
        
        public int GetInventoryCount()
        {
            return inventory.Count;
        }
        
        public bool IsInventoryFull()
        {
            return inventory.Count >= maxInventorySlots;
        }
        
        public void ClearInventory()
        {
            inventory.Clear();
            itemKeys.Clear();
            currentWeight = 0f;
            
            if (OnInventoryCleared != null)
            {
                OnInventoryCleared();
            }
        }
        
        public bool CanCarryItem(string itemName, int quantity = 1)
        {
            if (!useWeightSystem)
            {
                return !IsInventoryFull();
            }
            
            InventoryItem item = FindItemTemplate(itemName);
            if (item == null)
            {
                return false;
            }
            
            float additionalWeight = item.weight * quantity;
            return (currentWeight + additionalWeight) <= maxWeight;
        }
        
        private InventoryItem CreateNewItem(string itemName, int quantity)
        {
            InventoryItem template = FindItemTemplate(itemName);
            
            if (template != null)
            {
                return new InventoryItem
                {
                    itemName = template.itemName,
                    quantity = quantity,
                    weight = template.weight,
                    itemPrefab = template.itemPrefab,
                    itemIcon = template.itemIcon,
                    itemDescription = template.itemDescription
                };
            }
            
            return new InventoryItem
            {
                itemName = itemName,
                quantity = quantity,
                weight = 1f
            };
        }
        
        private InventoryItem FindItemTemplate(string itemName)
        {
            foreach (InventoryItem item in startingItems)
            {
                if (item.itemName == itemName)
                {
                    return item;
                }
            }
            
            return null;
        }
        
        public float GetCurrentWeight()
        {
            return currentWeight;
        }
        
        public float GetMaxWeight()
        {
            return maxWeight;
        }
        
        public float GetWeightPercentage()
        {
            if (maxWeight <= 0f)
            {
                return 0f;
            }
            
            return currentWeight / maxWeight;
        }
        
        public void SetMaxWeight(float weight)
        {
            maxWeight = weight;
        }
        
        public void AddWeightCapacity(float amount)
        {
            maxWeight += amount;
        }
        
        public void SetMaxStackSize(int size)
        {
            maxStackSize = size;
        }
        
        public void SetMaxInventorySlots(int slots)
        {
            maxInventorySlots = slots;
        }
        
        public event System.Action<string, int, bool> OnInventoryChanged;
        public event System.Action OnInventoryCleared;
    }
}
