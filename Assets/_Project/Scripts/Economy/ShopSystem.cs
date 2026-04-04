/* ShopSystem.cs
 *
 * Shop/merchant system for buying and selling items.
 *
 * Lore: In the Final Empire, boxing (brass coins) are the primary currency.
 * Clips (copper coins) are smaller denominations.
 *   1 boxing = 50 clips
 *
 * Shop prices are affected by:
 *   - Faction reputation (friendly factions give discounts)
 *   - Time of day (some shops close at night)
 *   - Story progression (war economy, rebellion effects)
 *   - Haggling skill (if implemented)
 *
 * Metal vials, weapons, armor, and supplies are all purchasable.
 * Black market shops sell restricted items (Allomantic metals, aluminum).
 */

using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopSystem : MonoBehaviour, IInteractable
{
    [Header("Shop Identity")]
    public string shopName = "General Store";
    public string shopkeeperName = "Merchant";
    public string factionId = "merchants"; // which faction this shop belongs to
    public ShopType shopType = ShopType.General;
    public bool isBlackMarket = false;

    [Header("Operating Hours")]
    public bool hasOperatingHours = true;
    public float openHour = 7f;
    public float closeHour = 20f;

    [Header("Inventory")]
    public List<ShopItem> inventory = new List<ShopItem>();
    public float restockIntervalDays = 3f;
    private float restockTimer;

    [Header("Economy")]
    [Tooltip("Base price multiplier for this shop (1.0 = normal)")]
    public float basePriceMultiplier = 1f;
    [Tooltip("How much the shop pays when buying from the player (0.3 = 30% of sell price)")]
    public float buybackRatio = 0.3f;

    [Header("Dialogue")]
    [TextArea(2, 3)]
    public string welcomeMessage = "Welcome. What can I get for you?";
    [TextArea(2, 3)]
    public string closedMessage = "Come back during business hours.";
    [TextArea(2, 3)]
    public string hostileMessage = "Get out. I don't serve your kind.";

    public event Action<ShopItem> OnItemPurchased;
    public event Action<ShopItem> OnItemSold;

    void Start()
    {
        if (inventory.Count == 0)
            PopulateDefaultInventory();
    }

    void Update()
    {
        // Restock timer
        if (DayNightCycle.Instance != null)
        {
            float hoursPerSecond = 24f / (DayNightCycle.Instance.dayLengthMinutes * 60f);
            float daysPerSecond = hoursPerSecond / 24f;
            restockTimer += daysPerSecond * Time.deltaTime;

            if (restockTimer >= restockIntervalDays)
            {
                restockTimer = 0f;
                RestockInventory();
            }
        }
    }

    // ── IInteractable ────────────────────────────────────────────────────────

    public string GetInteractPrompt()
    {
        if (!IsOpen()) return closedMessage;
        if (IsHostile()) return hostileMessage;
        return $"Press [F] to trade with {shopkeeperName}";
    }

    public bool CanInteract() => IsOpen() && !IsHostile();

    public void OnInteract(GameObject interactor)
    {
        if (!CanInteract()) return;
        ShopUI.Instance?.Open(this);
    }

    // ── Trading ──────────────────────────────────────────────────────────────

    public PurchaseResult TryBuy(ShopItem item, Inventory playerInventory, ref int playerBoxings)
    {
        if (item.stock <= 0)
            return new PurchaseResult { success = false, message = "Out of stock." };

        int price = GetBuyPrice(item);
        if (playerBoxings < price)
            return new PurchaseResult { success = false, message = $"Not enough boxings. Need {price}." };

        // Deduct payment
        playerBoxings -= price;

        // Add item to player inventory
        playerInventory.AddItem(item.itemName, item.quantity);

        // Reduce stock
        item.stock--;

        OnItemPurchased?.Invoke(item);

        return new PurchaseResult
        {
            success = true,
            price = price,
            message = $"Purchased {item.quantity}x {item.itemName} for {price} boxings."
        };
    }

    public PurchaseResult TrySell(string itemName, int quantity, Inventory playerInventory, ref int playerBoxings)
    {
        if (!playerInventory.HasItem(itemName, quantity))
            return new PurchaseResult { success = false, message = "You don't have enough." };

        // Calculate sell price
        var shopItem = inventory.Find(i => i.itemName == itemName);
        int basePrice = shopItem != null ? shopItem.basePrice : 10; // default price for unknown items
        int sellPrice = Mathf.Max(1, Mathf.RoundToInt(basePrice * buybackRatio * GetFactionDiscount()));

        playerInventory.RemoveItem(itemName, quantity);
        playerBoxings += sellPrice * quantity;

        return new PurchaseResult
        {
            success = true,
            price = sellPrice * quantity,
            message = $"Sold {quantity}x {itemName} for {sellPrice * quantity} boxings."
        };
    }

    // ── Pricing ──────────────────────────────────────────────────────────────

    public int GetBuyPrice(ShopItem item)
    {
        float price = item.basePrice * basePriceMultiplier;

        // Faction discount/markup
        price *= GetFactionDiscount();

        // Rarity markup
        if (item.stock <= 2) price *= 1.5f; // low stock = higher price

        // Black market markup
        if (isBlackMarket) price *= 1.3f;

        return Mathf.Max(1, Mathf.RoundToInt(price));
    }

    float GetFactionDiscount()
    {
        if (FactionSystem.Instance == null) return 1f;
        return FactionSystem.Instance.GetPriceMultiplier(factionId);
    }

    // ── Status ───────────────────────────────────────────────────────────────

    public bool IsOpen()
    {
        if (!hasOperatingHours) return true;
        if (DayNightCycle.Instance == null) return true;

        float hour = DayNightCycle.Instance.GetHour();
        return hour >= openHour && hour < closeHour;
    }

    public bool IsHostile()
    {
        if (FactionSystem.Instance == null) return false;
        return FactionSystem.Instance.IsHostile(factionId);
    }

    // ── Restocking ───────────────────────────────────────────────────────────

    void RestockInventory()
    {
        foreach (var item in inventory)
        {
            item.stock = Mathf.Min(item.stock + item.restockAmount, item.maxStock);
        }
    }

    // ── Default Inventory ────────────────────────────────────────────────────

    void PopulateDefaultInventory()
    {
        switch (shopType)
        {
            case ShopType.General:
                inventory = new List<ShopItem>
                {
                    new ShopItem { itemName = "Health Potion", basePrice = 15, stock = 10, maxStock = 10, restockAmount = 3, quantity = 1 },
                    new ShopItem { itemName = "Bandage", basePrice = 5, stock = 20, maxStock = 20, restockAmount = 5, quantity = 1 },
                    new ShopItem { itemName = "Torch", basePrice = 3, stock = 15, maxStock = 15, restockAmount = 5, quantity = 1 },
                    new ShopItem { itemName = "Rope", basePrice = 8, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Glass Vial", basePrice = 2, stock = 30, maxStock = 30, restockAmount = 10, quantity = 1 },
                    new ShopItem { itemName = "Alcohol Solution", basePrice = 4, stock = 15, maxStock = 15, restockAmount = 5, quantity = 1 },
                };
                break;

            case ShopType.Metalsmith:
                inventory = new List<ShopItem>
                {
                    new ShopItem { itemName = "Iron Ore", basePrice = 8, stock = 20, maxStock = 20, restockAmount = 5, quantity = 1 },
                    new ShopItem { itemName = "Tin Ingot", basePrice = 12, stock = 10, maxStock = 10, restockAmount = 3, quantity = 1 },
                    new ShopItem { itemName = "Copper Nugget", basePrice = 10, stock = 15, maxStock = 15, restockAmount = 4, quantity = 1 },
                    new ShopItem { itemName = "Zinc Dust", basePrice = 14, stock = 8, maxStock = 8, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Lead Ingot", basePrice = 6, stock = 12, maxStock = 12, restockAmount = 4, quantity = 1 },
                    new ShopItem { itemName = "Carbon", basePrice = 5, stock = 15, maxStock = 15, restockAmount = 5, quantity = 1 },
                    new ShopItem { itemName = "Steel Flakes", basePrice = 20, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Pewter Beads", basePrice = 22, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                };
                break;

            case ShopType.Apothecary:
                inventory = new List<ShopItem>
                {
                    new ShopItem { itemName = "Steel Vial", basePrice = 30, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Pewter Vial", basePrice = 35, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Tin Vial", basePrice = 25, stock = 8, maxStock = 8, restockAmount = 3, quantity = 1 },
                    new ShopItem { itemName = "Mistborn Vial", basePrice = 100, stock = 1, maxStock = 2, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Health Potion", basePrice = 12, stock = 10, maxStock = 10, restockAmount = 5, quantity = 1 },
                    new ShopItem { itemName = "Antidote", basePrice = 20, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                };
                break;

            case ShopType.WeaponSmith:
                inventory = new List<ShopItem>
                {
                    new ShopItem { itemName = "Iron Sword", basePrice = 50, stock = 3, maxStock = 3, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Dagger", basePrice = 20, stock = 5, maxStock = 5, restockAmount = 2, quantity = 1 },
                    new ShopItem { itemName = "Spear", basePrice = 35, stock = 3, maxStock = 3, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Obsidian Axe", basePrice = 40, stock = 2, maxStock = 2, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Mistcloak", basePrice = 80, stock = 1, maxStock = 1, restockAmount = 0, quantity = 1 },
                };
                break;

            case ShopType.BlackMarket:
                inventory = new List<ShopItem>
                {
                    new ShopItem { itemName = "Aluminum Ingot", basePrice = 200, stock = 1, maxStock = 1, restockAmount = 0, quantity = 1 },
                    new ShopItem { itemName = "Gold Nugget", basePrice = 150, stock = 2, maxStock = 2, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Atium Bead", basePrice = 500, stock = 1, maxStock = 1, restockAmount = 0, quantity = 1 },
                    new ShopItem { itemName = "Chromium Dust", basePrice = 180, stock = 1, maxStock = 1, restockAmount = 0, quantity = 1 },
                    new ShopItem { itemName = "Duralumin Beads", basePrice = 250, stock = 1, maxStock = 1, restockAmount = 0, quantity = 1 },
                    new ShopItem { itemName = "Mistborn Vial", basePrice = 80, stock = 2, maxStock = 2, restockAmount = 1, quantity = 1 },
                    new ShopItem { itemName = "Stolen Map", basePrice = 30, stock = 3, maxStock = 3, restockAmount = 1, quantity = 1 },
                };
                isBlackMarket = true;
                break;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DATA STRUCTURES
// ═══════════════════════════════════════════════════════════════════════════

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public int basePrice;       // in boxings
    public int stock;
    public int maxStock;
    public int restockAmount;   // how many restock per cycle
    public int quantity;        // how many of the item per purchase
    public Sprite icon;
    [TextArea(1, 2)]
    public string description;
}

public struct PurchaseResult
{
    public bool success;
    public int price;
    public string message;
}

public enum ShopType
{
    General,
    Metalsmith,
    Apothecary,
    WeaponSmith,
    BlackMarket
}
