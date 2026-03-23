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
using System.Collections.Generic;

public class AllomanticTarget : MonoBehaviour
{
    // Static registry of all metal objects for efficient lookup
    private static HashSet<AllomanticTarget> allMetalObjects = new HashSet<AllomanticTarget>();
    
    [Header("Metal Properties")]
    [Tooltip("Type of metal this object is made of")]
    public AllomancySkill.MetalType metalType = AllomancySkill.MetalType.Steel;
    
    [Tooltip("If true, this object is anchored (fixed) and will pull the player instead of moving")]
    public bool isAnchored = false;
    
    [Tooltip("Mass of the metal (used for push/pull calculations). If 0, uses Rigidbody mass.")]
    public float mass = 0f;
    
    [Header("Physics Settings")]
    [Tooltip("Air drag for this metal object (0 = no drag, higher = more resistance)")]
    public float drag = 0.1f;
    
    [Header("References")]
    [Tooltip("Reference to the Rigidbody (if any). Automatically assigned if left empty.")]
    public Rigidbody rigidbody;
    
    void Awake()
    {
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        
        if (rigidbody != null)
        {
            // Ensure gravity is enabled for realistic physics
            if (!rigidbody.useGravity)
            {
                rigidbody.useGravity = true;
            }
            
            // Apply air drag for more realistic projectile motion
            rigidbody.drag = drag;
            rigidbody.angularDrag = drag * 2f; // More angular drag for stability
        }
        
        if (mass <= 0f && rigidbody != null)
        {
            mass = rigidbody.mass;
        }
        
        // Register this metal object
        allMetalObjects.Add(this);
    }
    
    void OnDestroy()
    {
        // Unregister when destroyed
        allMetalObjects.Remove(this);
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
    
    // Static helper to check if a collider is a metal object
    public static bool IsMetal(Collider collider)
    {
        if (collider == null) return false;
        return collider.GetComponent<AllomanticTarget>() != null;
    }
    
    // Static helper to get AllomanticTarget from collider (null if not metal)
    public static AllomanticTarget GetMetalObject(Collider collider)
    {
        if (collider == null) return null;
        return collider.GetComponent<AllomanticTarget>();
    }
    
    // Get all registered metal objects (for debugging or special abilities)
    public static HashSet<AllomanticTarget> GetAllMetalObjects()
    {
        return new HashSet<AllomanticTarget>(allMetalObjects);
    }
}