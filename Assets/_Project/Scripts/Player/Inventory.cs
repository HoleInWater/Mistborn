// NOTE: Line 31 contains Debug.Log which should be removed for production
// NOTE: Consider adding [System.Serializable] attribute for the inventory system
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    // NOTE: Consider adding [Range(1, 100)] attribute for maxSlots
    public int maxSlots = 20;
    // NOTE: currentSlotCount could be a property that returns ownedKeys.Length
    public int currentSlotCount = 0;
    
    [Header("Keys")]
    public int[] ownedKeys;
    
    public bool HasKey(int keyID)
    {
        foreach (int key in ownedKeys)
        {
            if (key == keyID) return true;
        }
        return false;
    }
    
    public void AddKey(int keyID)
    {
        int[] newKeys = new int[ownedKeys.Length + 1];
        for (int i = 0; i < ownedKeys.Length; i++)
        {
            newKeys[i] = ownedKeys[i];
        }
        newKeys[ownedKeys.Length] = keyID;
        ownedKeys = newKeys;
        
        Debug.Log($"Acquired key {keyID}!");
    }
}
