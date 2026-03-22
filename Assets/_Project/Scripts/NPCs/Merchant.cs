using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.NPCs
{
    public class Merchant : NPCController
    {
        [Header("Merchant Configuration")]
        [SerializeField] private MerchantType merchantType = MerchantType.General;
        [SerializeField] private string shopName = "General Store";
        [SerializeField] private int merchantLevel = 1;
        
        [Header("Inventory")]
        [SerializeField] private List<MerchantItem> itemsForSale = new List<MerchantItem>();
        [SerializeField] private int maxInventorySize = 20;
        [SerializeField] private bool restockDaily = true;
        
        [Header("Pricing")]
        [SerializeField] private float priceMultiplier = 1f;
        [SerializeField] private bool haggleEnabled = true;
        [SerializeField] private int haggleSkill = 1;
        
        [Header("Specialization")]
        [SerializeField] private MetalType[] preferredMetals;
        [SerializeField] private bool sellsRareItems = false;
        [SerializeField] private float rareItemChance = 0.1f;
        
        [Header("Services")]
        [SerializeField] private bool offersRepair = true;
        [SerializeField] private bool offersUpgrades = false;
        [SerializeField] private bool buysFromPlayer = true;
        [SerializeField] private float buyMultiplier = 0.5f;
        
        private Dictionary<string, int> itemStock = new Dictionary<string, int>();
        private List<MerchantItem> rareItems = new List<MerchantItem>();
        private float lastRestockTime = 0f;
        
        public MerchantType Type => merchantType;
        public string ShopName => shopName;
        
        protected override void Start()
        {
            base.Start();
            
            InitializeInventory();
        }
        
        private void InitializeInventory()
        {
            foreach (var item in itemsForSale)
            {
                itemStock[item.itemId] = item.stock;
            }
            
            if (sellsRareItems)
            {
                GenerateRareItems();
            }
            
            UpdateMerchantLevel();
        }
        
        private void UpdateMerchantLevel()
        {
            priceMultiplier = 1f - (merchantLevel * 0.05f);
            priceMultiplier = Mathf.Max(priceMultiplier, 0.5f);
            
            maxInventorySize += merchantLevel * 5;
        }
        
        private void Update()
        {
            if (restockDaily)
            {
                CheckForRestock();
            }
        }
        
        private void CheckForRestock()
        {
            if (Time.time - lastRestockTime > 86400f)
            {
                RestockInventory();
                lastRestockTime = Time.time;
            }
        }
        
        private void RestockInventory()
        {
            foreach (var item in itemsForSale)
            {
                int currentStock = itemStock.ContainsKey(item.itemId) ? itemStock[item.itemId] : 0;
                int restockAmount = Random.Range(1, item.maxStock - currentStock);
                itemStock[item.itemId] = Mathf.Min(currentStock + restockAmount, item.maxStock);
            }
            
            if (sellsRareItems)
            {
                GenerateRareItems();
            }
        }
        
        private void GenerateRareItems()
        {
            if (!sellsRareItems || preferredMetals == null)
                return;
            
            if (Random.value < rareItemChance)
            {
                MetalType rareMetal = preferredMetals[Random.Range(0, preferredMetals.Length)];
                MerchantItem rareItem = new MerchantItem
                {
                    itemId = $"Rare_{rareMetal}",
                    itemName = $"Rare {rareMetal} Metal",
                    price = GetBasePrice(rareMetal) * 3,
                    stock = 1,
                    maxStock = 1
                };
                
                rareItems.Add(rareItem);
            }
        }
        
        public List<MerchantItem> GetItemsForSale()
        {
            List<MerchantItem> availableItems = new List<MerchantItem>();
            
            foreach (var item in itemsForSale)
            {
                if (itemStock.ContainsKey(item.itemId) && itemStock[item.itemId] > 0)
                {
                    MerchantItem forSale = new MerchantItem
                    {
                        itemId = item.itemId,
                        itemName = item.itemName,
                        price = Mathf.RoundToInt(item.price * priceMultiplier),
                        stock = itemStock[item.itemId],
                        maxStock = item.maxStock
                    };
                    availableItems.Add(forSale);
                }
            }
            
            foreach (var rare in rareItems)
            {
                availableItems.Add(rare);
            }
            
            return availableItems;
        }
        
        public bool PurchaseItem(string itemId, int quantity, int offeredPrice)
        {
            if (!itemStock.ContainsKey(itemId))
                return false;
            
            int stock = itemStock[itemId];
            if (stock < quantity)
                return false;
            
            MerchantItem item = itemsForSale.Find(i => i.itemId == itemId);
            if (item == null)
                return false;
            
            int actualPrice = Mathf.RoundToInt(item.price * priceMultiplier) * quantity;
            
            if (haggleEnabled && offeredPrice < actualPrice)
            {
                int haggleResult = TryHaggle(offeredPrice, actualPrice);
                if (haggleResult < 0)
                {
                    return false;
                }
                actualPrice = haggleResult;
            }
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                if (!playerStats.SpendMetal(actualPrice))
                {
                    return false;
                }
            }
            
            itemStock[itemId] -= quantity;
            
            GiveItemToPlayer(itemId, quantity);
            
            return true;
        }
        
        private int TryHaggle(int offered, int asking)
        {
            int difference = asking - offered;
            int threshold = asking / (haggleSkill + 2);
            
            if (difference <= threshold)
            {
                return Mathf.RoundToInt(offered + difference * 0.5f);
            }
            
            return -1;
        }
        
        private void GiveItemToPlayer(string itemId, int quantity)
        {
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null)
            {
                inventory.AddItem(itemId, quantity);
            }
        }
        
        public int GetSellPrice(string itemId, int quantity)
        {
            MerchantItem item = itemsForSale.Find(i => i.itemId == itemId);
            if (item == null)
            {
                return 0;
            }
            
            int basePrice = item.price * quantity;
            return Mathf.RoundToInt(basePrice * buyMultiplier);
        }
        
        public bool SellToMerchant(string itemId, int quantity)
        {
            int sellPrice = GetSellPrice(itemId, quantity);
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddMetal(sellPrice);
            }
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null)
            {
                inventory.RemoveItem(itemId, quantity);
            }
            
            return true;
        }
        
        public bool CanRepair()
        {
            return offersRepair;
        }
        
        public float GetRepairCost(float itemDurability)
        {
            if (!offersRepair)
                return 0f;
            
            return (1f - itemDurability) * 100f * priceMultiplier;
        }
        
        public bool RepairItem(float durability)
        {
            if (!offersRepair)
                return false;
            
            float cost = GetRepairCost(durability);
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                if (!playerStats.SpendMetal(Mathf.RoundToInt(cost)))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private int GetBasePrice(MetalType metal)
        {
            switch (metal)
            {
                case MetalType.Iron:
                case MetalType.Steel:
                    return 10;
                case MetalType.Tin:
                case MetalType.Pewter:
                    return 15;
                case MetalType.Copper:
                case MetalType.Brass:
                    return 20;
                case MetalType.Bronze:
                case MetalType.Zinc:
                    return 25;
                case MetalType.Gold:
                case MetalType.Electrum:
                    return 100;
                case MetalType.Atium:
                case MetalType.Malatium:
                    return 1000;
                case MetalType.Lerasium:
                case MetalType.Harmonium:
                    return 10000;
                default:
                    return 10;
            }
        }
        
        public void AddNewStock(MerchantItem item)
        {
            if (itemsForSale.Count >= maxInventorySize)
                return;
            
            if (!itemsForSale.Exists(i => i.itemId == item.itemId))
            {
                itemsForSale.Add(item);
                itemStock[item.itemId] = item.stock;
            }
        }
        
        public override void StartTrade()
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMerchantUI(this);
            }
        }
    }
    
    public enum MerchantType
    {
        General,
        Weapons,
        Armor,
        Metal,
        Alchemy,
        Information,
        BlackMarket
    }
    
    [System.Serializable]
    public class MerchantItem
    {
        public string itemId;
        public string itemName;
        public int price;
        public int stock;
        public int maxStock;
    }
}
