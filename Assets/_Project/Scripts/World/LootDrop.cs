// NOTE: Lines 28, 31, 34, 37 contain Debug.Log which should be removed for production
using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootType lootType;
    public int minAmount = 1;
    public int maxAmount = 5;
    
    [Header("References")]
    public GameObject pickupEffect;
    
    public enum LootType
    {
        MetalPieces,
        HealthPotion,
        SkillPoint,
        Coin
    }
    
    public void GenerateLoot()
    {
        int amount = Random.Range(minAmount, maxAmount + 1);
        
        switch (lootType)
        {
            case LootType.MetalPieces:
                Debug.Log($"Dropped {amount} metal pieces!");
                break;
            case LootType.HealthPotion:
                Debug.Log($"Dropped {amount} health potions!");
                break;
            case LootType.SkillPoint:
                Debug.Log($"Dropped {amount} skill points!");
                break;
            case LootType.Coin:
                Debug.Log($"Dropped {amount} coins!");
                break;
        }
        
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
    }
}
