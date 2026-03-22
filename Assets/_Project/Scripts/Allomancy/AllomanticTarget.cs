/* AllomanticTarget.cs
 * 
 * PURPOSE:
 * This component is attached to any metal object that can be pushed or pulled using Allomancy.
 * It marks the object as metal and provides metadata about the metal type, whether it's anchored,
 * and its mass for physics calculations.
 * 
 * KEY FIELDS:
 * - metalType: The type of metal (Steel, Iron, etc.) - used for Allomancy targeting
 * - isAnchored: If true, the object is fixed in place and will pull/push the player instead
 * - mass: Mass for push/pull calculations (if 0, uses Rigidbody mass)
 * 
 * HOW IT WORKS:
 * Allomantic abilities (SteelPush, IronPull) look for this component to determine
 * if an object can be pushed/pulled and how it should behave.
 * 
 * IMPORTANT NOTES:
 * - Requires a Rigidbody component for physics interactions
 * - Set isAnchored=true for wall brackets or fixed objects
 * - mass should be set for accurate push/pull force calculations
 */

using UnityEngine;

public class AllomanticTarget : MonoBehaviour
{
    [Header("Metal Properties")]
    [Tooltip("Type of metal this object is made of")]
    public AllomancySkill.MetalType metalType = AllomancySkill.MetalType.Steel;
    
    [Tooltip("If true, this object is anchored (fixed) and will pull the player instead of moving")]
    public bool isAnchored = false;
    
    [Tooltip("Mass of the metal (used for push/pull calculations). If 0, uses Rigidbody mass.")]
    public float mass = 0f;
    
    [Header("References")]
    [Tooltip("Reference to the Rigidbody (if any). Automatically assigned if left empty.")]
    public Rigidbody rigidbody;
    
    void Awake()
    {
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        
        if (mass <= 0f && rigidbody != null)
        {
            mass = rigidbody.mass;
        }
    }
    
    // Returns the effective mass for push/pull calculations
    public float GetEffectiveMass()
    {
        if (mass <= 0f)
        {
            if (rigidbody != null)
                return rigidbody.mass;
            else
                return 1f; // default mass
        }
        return mass;
    }
}