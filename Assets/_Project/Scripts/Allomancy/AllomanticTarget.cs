// AllomanticTarget.cs
// Component attached to metal objects that can be pushed/pulled.
// This marks the object as metal and provides metadata for Allomancy abilities.
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