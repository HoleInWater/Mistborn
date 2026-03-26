// NOTE: Lines 28, 31, 34, 37 contain Debug.Log which should be removed for production
using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootType lootType;
    // NOTE: Consider adding [Range(1, 100)] attribute for minAmount
    public int minAmount = 1;
    // NOTE: Consider adding [Range(1, 100)] attribute for maxAmount
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
                break;
            case LootType.HealthPotion:
                break;
            case LootType.SkillPoint:
                break;
            case LootType.Coin:
                break;
        }
        
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
    }
}
