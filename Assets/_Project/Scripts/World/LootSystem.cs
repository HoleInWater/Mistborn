using UnityEngine;
using System.Collections.Generic;

public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootTable lootTable;
    public float dropChance = 100f;
    public int minDrops = 1;
    public int maxDrops = 3;
    
    [Header("Experience")]
    public int experienceDrop = 10;
    
    [Header("Gold")]
    public int minGold = 5;
    public int maxGold = 20;
    
    [Header("Effects")]
    public GameObject lootVFX;
    public AudioClip dropSound;
    
    private bool hasDropped = false;
    
    public void DropLoot()
    {
        if (hasDropped) return;
        hasDropped = true;
        
        DropExperience();
        DropGold();
        DropItems();
        PlayDropEffects();
    }
    
    void DropExperience()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            stats.AddExperience(experienceDrop);
        }
    }
    
    void DropGold()
    {
        int goldAmount = Random.Range(minGold, maxGold + 1);
        
        GameObject goldPickup = CreatePickup("Gold", goldAmount);
        goldPickup.transform.position = transform.position + Vector3.up;
    }
    
    void DropItems()
    {
        if (lootTable == null) return;
        
        int dropCount = Random.Range(minDrops, maxDrops + 1);
        
        for (int i = 0; i < dropCount; i++)
        {
            LootItem item = lootTable.GetRandomItem();
            
            if (item != null && Random.value * 100f <= dropChance)
            {
                GameObject itemPickup = CreateItemPickup(item);
                Vector3 offset = Random.insideUnitSphere * 2f;
                offset.y = 1f;
                itemPickup.transform.position = transform.position + offset;
            }
        }
    }
    
    GameObject CreatePickup(string type, int amount)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.transform.localScale = Vector3.one * 0.3f;
        
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        
        LootPickup pickupScript = pickup.AddComponent<LootPickup>();
        pickupScript.lootType = type;
        pickupScript.amount = amount;
        
        return pickup;
    }
    
    GameObject CreateItemPickup(LootItem item)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = pickup.GetComponent<Renderer>();
        if (item.rarity == ItemRarity.Common) renderer.material.color = Color.gray;
        else if (item.rarity == ItemRarity.Uncommon) renderer.material.color = Color.green;
        else if (item.rarity == ItemRarity.Rare) renderer.material.color = Color.blue;
        else if (item.rarity == ItemRarity.Epic) renderer.material.color = Color.magenta;
        else if (item.rarity == ItemRarity.Legendary) renderer.material.color = Color.yellow;
        
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        
        LootPickup pickupScript = pickup.AddComponent<LootPickup>();
        pickupScript.lootType = "Item";
        pickupScript.item = item;
        
        return pickup;
    }
    
    void PlayDropEffects()
    {
        if (lootVFX != null)
        {
            Instantiate(lootVFX, transform.position, Quaternion.identity);
        }
        
        if (dropSound != null)
        {
            AudioSource.PlayClipAtPoint(dropSound, transform.position);
        }
    }
}

public class LootPickup : MonoBehaviour
{
    public string lootType;
    public int amount;
    public LootItem item;
    
    [Header("Settings")]
    public float pickupRadius = 2f;
    public float magnetSpeed = 10f;
    public bool attractToPlayer = false;
    
    private Transform playerTransform;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void Update()
    {
        if (attractToPlayer && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance < pickupRadius * 3f)
            {
                attractToPlayer = true;
            }
            
            if (attractToPlayer)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    playerTransform.position, 
                    magnetSpeed * Time.deltaTime
                );
            }
        }
        
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance < pickupRadius)
            {
                Collect();
            }
        }
    }
    
    void Collect()
    {
        switch (lootType)
        {
            case "Gold":
                CollectGold();
                break;
            case "Experience":
                CollectExperience();
                break;
            case "Item":
                CollectItem();
                break;
        }
        
        Destroy(gameObject);
    }
    
    void CollectGold()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.AddGold(amount);
        }
        
        Debug.Log($"Collected {amount} gold!");
    }
    
    void CollectExperience()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            stats.AddExperience(amount);
        }
        
        Debug.Log($"Collected {amount} experience!");
    }
    
    void CollectItem()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null && item != null)
        {
            inventory.AddItem(item.itemId);
        }
        
        Debug.Log($"Collected item: {item.itemName}");
    }
}

[System.Serializable]
public class LootTable
{
    public LootItem[] commonItems;
    public LootItem[] uncommonItems;
    public LootItem[] rareItems;
    public LootItem[] epicItems;
    public LootItem[] legendaryItems;
    
    public LootItem GetRandomItem()
    {
        float roll = Random.value * 100f;
        
        if (roll < 0.5f && legendaryItems != null && legendaryItems.Length > 0)
        {
            return legendaryItems[Random.Range(0, legendaryItems.Length)];
        }
        else if (roll < 2f && epicItems != null && epicItems.Length > 0)
        {
            return epicItems[Random.Range(0, epicItems.Length)];
        }
        else if (roll < 10f && rareItems != null && rareItems.Length > 0)
        {
            return rareItems[Random.Range(0, rareItems.Length)];
        }
        else if (roll < 30f && uncommonItems != null && uncommonItems.Length > 0)
        {
            return uncommonItems[Random.Range(0, uncommonItems.Length)];
        }
        else if (commonItems != null && commonItems.Length > 0)
        {
            return commonItems[Random.Range(0, commonItems.Length)];
        }
        
        return null;
    }
}

[System.Serializable]
public class LootItem
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public List<ItemStat> stats;
}

[System.Serializable]
public class Item
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;
    public ItemRarity rarity;
    public ItemType type;
    public int value;
    public int stackCount = 1;
    public bool isUsable = false;
    public bool isEquipable = false;
    public List<ItemStat> stats;
    
    public Item()
    {
        stats = new List<ItemStat>();
    }
    
    public Item(LootItem lootItem)
    {
        itemId = lootItem.itemId;
        itemName = lootItem.itemName;
        icon = lootItem.icon;
        description = lootItem.description;
        rarity = lootItem.rarity;
        value = lootItem.value;
        stats = lootItem.stats;
    }
}

[System.Serializable]
public class ItemStat
{
    public string statName;
    public float value;
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Quest,
    Misc
}
