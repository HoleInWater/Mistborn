using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shop system for purchasing metal vials, weapons, and supplies from lowborn merchants.
/// Uses crown (currency) as the monetary system. Integrated with Inventory and MetalVialSystem.
/// </summary>
public class ShopSystem : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public string itemName;
        public string description;
        public ShopCategory category;
        public int price;
        public int stock;
        public MetallurgySkill.MetalType? metalType;
        public float metalPurity = 1f;
        public float metalAmount = 80f;
        [Tooltip("Assign a WeaponData asset for Weapons category items")]
        public WeaponData weaponData;
    }

    public enum ShopCategory { MetalVials, Weapons, Armor, Supplies, Special }

    [Header("Shop")]
    public string shopName = "Lowborn Black Market";
    public List<ShopItem> inventory = new List<ShopItem>();

    [Header("Player Currency")]
    public int playerBoxings = 500;

    public System.Action OnShopUpdated;

    void Start()
    {
        if (inventory.Count == 0) PopulateDefaultShop();
    }

    void PopulateDefaultShop()
    {
        // Metal Vials
        AddItem("vial_steel", "Steel Vial", "Pure steel flakes in alcohol. Essential for Launchers.",
            ShopCategory.MetalVials, 25, 10, MetallurgySkill.MetalType.Steel, 1f, 80f);
        AddItem("vial_iron", "Iron Vial", "Pure iron flakes. For Haulers.",
            ShopCategory.MetalVials, 25, 10, MetallurgySkill.MetalType.Iron, 1f, 80f);
        AddItem("vial_pewter", "Pewter Vial", "Pewter alloy. Thugs' bread and butter.",
            ShopCategory.MetalVials, 30, 8, MetallurgySkill.MetalType.Pewter, 1f, 80f);
        AddItem("vial_tin", "Tin Vial", "Pure tin. Keensenses love this stuff.",
            ShopCategory.MetalVials, 20, 10, MetallurgySkill.MetalType.Tin, 1f, 60f);
        AddItem("vial_zinc", "Zinc Vial", "Zinc flakes for Igniters.",
            ShopCategory.MetalVials, 35, 5, MetallurgySkill.MetalType.Zinc, 1f, 50f);
        AddItem("vial_brass", "Brass Vial", "Brass alloy for Quellers.",
            ShopCategory.MetalVials, 35, 5, MetallurgySkill.MetalType.Brass, 1f, 50f);
        AddItem("vial_oraculum", "Oraculum Bead", "Extremely rare god metal. See the future.",
            ShopCategory.Special, 500, 1, MetallurgySkill.MetalType.Oraculum, 1f, 30f);

        // Impure (cheaper but dangerous)
        AddItem("vial_steel_impure", "Impure Steel Vial", "Cheap steel. Might make you sick.",
            ShopCategory.MetalVials, 10, 20, MetallurgySkill.MetalType.Steel, 0.6f, 60f);

        // Supplies
        AddItem("coins_50", "Bag of Coins (50)", "Ashwalker currency and ammunition.",
            ShopCategory.Supplies, 40, 5);
        AddItem("health_potion", "Healing Salve", "Restores 50 health.",
            ShopCategory.Supplies, 30, 10);
    }

    void AddItem(string id, string name, string desc, ShopCategory cat, int price, int stock,
        MetallurgySkill.MetalType? metal = null, float purity = 1f, float amount = 0f)
    {
        inventory.Add(new ShopItem
        {
            itemId = id,
            itemName = name,
            description = desc,
            category = cat,
            price = price,
            stock = stock,
            metalType = metal,
            metalPurity = purity,
            metalAmount = amount
        });
    }

    /// <summary>
    /// Purchase an item. Returns true if successful.
    /// </summary>
    public bool Purchase(string itemId)
    {
        ShopItem item = inventory.Find(i => i.itemId == itemId && i.stock > 0);
        if (item == null) return false;
        if (playerBoxings < item.price) return false;

        playerBoxings -= item.price;
        item.stock--;

        // Apply purchase
        if (item.metalType.HasValue)
        {
            MetalVialSystem vials = FindObjectOfType<MetalVialSystem>();
            if (vials != null)
                vials.AddVial(item.metalType.Value, item.metalAmount, item.metalPurity);
        }
        else if (item.itemId == "coins_50")
        {
            CoinPouch pouch = FindObjectOfType<CoinPouch>();
            if (pouch != null) pouch.AddCoins(50);
        }
        else if (item.itemId == "health_potion")
        {
            PlayerHealth.Instance?.Heal(50f);
        }
        else if (item.category == ShopCategory.Weapons && item.weaponData != null)
        {
            // Add to inventory and auto-equip
            Inventory inv = FindObjectOfType<Inventory>();
            if (inv != null)
            {
                inv.AddItem(new InventoryItem
                {
                    itemId     = item.itemId,
                    itemName   = item.itemName,
                    description= item.description,
                    type       = InventoryItem.ItemType.Weapon,
                    quantity   = 1,
                    maxStack   = 1,
                    weight     = item.weaponData.mass,
                    weaponData = item.weaponData
                });
            }
            EquipmentManager.Instance?.EquipWeapon(item.weaponData);
        }

        NotificationSystem.Instance?.ShowNotification($"Purchased: {item.itemName}");
        OnShopUpdated?.Invoke();
        return true;
    }

    public bool CanAfford(string itemId)
    {
        ShopItem item = inventory.Find(i => i.itemId == itemId);
        return item != null && item.stock > 0 && playerBoxings >= item.price;
    }

    public void AddBoxings(int amount)
    {
        playerBoxings += amount;
        NotificationSystem.Instance?.ShowNotification($"+{amount} crowns");
    }

    public int GetBoxings() => playerBoxings;
    public List<ShopItem> GetItemsByCategory(ShopCategory cat) => inventory.FindAll(i => i.category == cat && i.stock > 0);
}
