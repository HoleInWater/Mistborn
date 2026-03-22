using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class VendorSystem : MonoBehaviour
    {
        [Header("Vendor Settings")]
        [SerializeField] private string vendorName = "Merchant";
        [SerializeField] private VendorType vendorType = VendorType.General;
        [SerializeField] private float buyPriceMultiplier = 0.5f;
        [SerializeField] private float sellPriceMultiplier = 0.7f;
        
        [Header("Inventory")]
        [SerializeField] private VendorItem[] availableItems;
        [SerializeField] private int maxInventorySlots = 20;
        
        [Header("Reputation")]
        [SerializeField] private int startingReputation = 0;
        [SerializeField] private int maxReputation = 100;
        [SerializeField] private float reputationPriceBonus = 0.1f;
        
        [Header("Visual")]
        [SerializeField] private GameObject vendorUI;
        [SerializeField] private Sprite vendorPortrait;
        
        private List<VendorItem> currentInventory = new List<VendorItem>();
        private int currentReputation = 0;
        private PlayerStats playerStats;
        private bool isShopOpen = false;
        
        public enum VendorType
        {
            General,
            Weapons,
            Armor,
            Alchemy,
            AllomanticSupplies,
            FeruchemySupplies
        }
        
        [System.Serializable]
        public class VendorItem
        {
            public string itemName;
            public GameObject itemPrefab;
            public int basePrice = 100;
            public int stockQuantity = 1;
            public int currentStock = 1;
            public bool unlimitedStock = false;
            public int requiredReputation = 0;
        }
        
        private void Awake()
        {
            InitializeVendor();
        }
        
        private void Start()
        {
            if (vendorUI != null)
            {
                vendorUI.SetActive(false);
            }
            
            RefreshInventory();
        }
        
        private void InitializeVendor()
        {
            currentReputation = startingReputation;
            currentInventory = new List<VendorItem>();
        }
        
        private void RefreshInventory()
        {
            currentInventory.Clear();
            
            foreach (VendorItem item in availableItems)
            {
                if (item.requiredReputation <= currentReputation)
                {
                    VendorItem newItem = new VendorItem
                    {
                        itemName = item.itemName,
                        itemPrefab = item.itemPrefab,
                        basePrice = item.basePrice,
                        stockQuantity = item.stockQuantity,
                        currentStock = item.currentStock,
                        unlimitedStock = item.unlimitedStock,
                        requiredReputation = item.requiredReputation
                    };
                    
                    currentInventory.Add(newItem);
                }
            }
        }
        
        public void OpenShop(PlayerStats stats)
        {
            if (isShopOpen)
            {
                return;
            }
            
            playerStats = stats;
            isShopOpen = true;
            
            RefreshInventory();
            
            if (vendorUI != null)
            {
                vendorUI.SetActive(true);
            }
            
            if (OnShopOpened != null)
            {
                OnShopOpened();
            }
        }
        
        public void CloseShop()
        {
            if (!isShopOpen)
            {
                return;
            }
            
            isShopOpen = false;
            playerStats = null;
            
            if (vendorUI != null)
            {
                vendorUI.SetActive(false);
            }
            
            if (OnShopClosed != null)
            {
                OnShopClosed();
            }
        }
        
        public bool BuyItem(string itemName, int quantity = 1)
        {
            if (!isShopOpen || playerStats == null)
            {
                return false;
            }
            
            VendorItem item = FindItem(itemName);
            if (item == null)
            {
                return false;
            }
            
            if (!item.unlimitedStock && item.currentStock < quantity)
            {
                return false;
            }
            
            int price = CalculateBuyPrice(item);
            if (playerStats.GetGold() < price * quantity)
            {
                return false;
            }
            
            playerStats.SpendGold(price * quantity);
            
            if (!item.unlimitedStock)
            {
                item.currentStock -= quantity;
            }
            
            InventorySystem inventory = playerStats.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                inventory.AddItem(itemName, quantity);
            }
            
            if (OnItemPurchased != null)
            {
                OnItemPurchased(itemName, quantity, price);
            }
            
            return true;
        }
        
        public bool SellItem(string itemName, int quantity = 1)
        {
            if (!isShopOpen || playerStats == null)
            {
                return false;
            }
            
            InventorySystem inventory = playerStats.GetComponent<InventorySystem>();
            if (inventory == null || !inventory.HasItem(itemName, quantity))
            {
                return false;
            }
            
            VendorItem item = FindItem(itemName);
            if (item == null)
            {
                return false;
            }
            
            int price = CalculateSellPrice(item);
            playerStats.AddGold(price * quantity);
            
            inventory.RemoveItem(itemName, quantity);
            
            if (!item.unlimitedStock)
            {
                item.currentStock += quantity;
            }
            
            if (OnItemSold != null)
            {
                OnItemSold(itemName, quantity, price);
            }
            
            return true;
        }
        
        private VendorItem FindItem(string itemName)
        {
            foreach (VendorItem item in currentInventory)
            {
                if (item.itemName == itemName)
                {
                    return item;
                }
            }
            
            return null;
        }
        
        private int CalculateBuyPrice(VendorItem item)
        {
            float price = item.basePrice;
            
            int reputationLevel = currentReputation / 10;
            price *= (1f - (reputationLevel * reputationPriceBonus));
            
            return Mathf.RoundToInt(price * buyPriceMultiplier);
        }
        
        private int CalculateSellPrice(VendorItem item)
        {
            float price = item.basePrice;
            
            int reputationLevel = currentReputation / 10;
            price *= (1f + (reputationLevel * reputationPriceBonus));
            
            return Mathf.RoundToInt(price * sellPriceMultiplier);
        }
        
        public void AddReputation(int amount)
        {
            currentReputation += amount;
            currentReputation = Mathf.Clamp(currentReputation, 0, maxReputation);
            
            RefreshInventory();
            
            if (OnReputationChanged != null)
            {
                OnReputationChanged(currentReputation);
            }
        }
        
        public void RemoveReputation(int amount)
        {
            currentReputation -= amount;
            currentReputation = Mathf.Clamp(currentReputation, 0, maxReputation);
            
            RefreshInventory();
            
            if (OnReputationChanged != null)
            {
                OnReputationChanged(currentReputation);
            }
        }
        
        public List<VendorItem> GetAvailableItems()
        {
            return currentInventory;
        }
        
        public VendorItem GetItem(string itemName)
        {
            return FindItem(itemName);
        }
        
        public string GetVendorName()
        {
            return vendorName;
        }
        
        public VendorType GetVendorType()
        {
            return vendorType;
        }
        
        public int GetReputation()
        {
            return currentReputation;
        }
        
        public bool IsShopOpen()
        {
            return isShopOpen;
        }
        
        public void RestockInventory()
        {
            foreach (VendorItem item in availableItems)
            {
                item.currentStock = item.stockQuantity;
            }
            
            RefreshInventory();
        }
        
        public void AddNewItem(VendorItem newItem)
        {
            VendorItem[] newItems = new VendorItem[availableItems.Length + 1];
            availableItems.CopyTo(newItems, 0);
            newItems[newItems.Length - 1] = newItem;
            availableItems = newItems;
            
            RefreshInventory();
        }
        
        public event System.Action OnShopOpened;
        public event System.Action OnShopClosed;
        public event System.Action<string, int, int> OnItemPurchased;
        public event System.Action<string, int, int> OnItemSold;
        public event System.Action<int> OnReputationChanged;
    }
}
