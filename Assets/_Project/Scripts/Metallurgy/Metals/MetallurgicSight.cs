// MetallurgicSight.cs
// This script implements the Metallurgic Sight ability (Tab key) which shows blue lines to metal objects.
// Lore: In Ashwalker, Metallurgists can see metal through the Spiritual Realm, represented as blue lines.
// This ability reveals all metal within range, passing through walls and geometry.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[PlayerComponent("Metallurgy Support", order: 10)]
public class MetallurgicSight : MonoBehaviour
{
    // ===== SETTINGS =====
    // Maximum distance to detect metal objects (lore: about 100 meters for a skilled Ashwalker)
    [Header("Settings")]
    [Tooltip("Maximum distance to detect metal objects")]
    public float metalRange = 100f;
    
    [Tooltip("Width of the blue lines showing metal")]
    public float lineWidth = 0.05f;
    
    [Tooltip("Color for normal metal objects (coins, brackets, etc.)")]
    public Color metalColor = Color.cyan; // Blue/cyan as described in the books
    
    [Tooltip("Color for heavy metal objects (Bloodbrute weapons, large structures)")]
    public Color heavyMetalColor = Color.blue; // Darker blue for heavier objects
    
    [Tooltip("Color for metals that cannot be pushed (aluminum, etc.)")]
    public Color nonPushableColor = Color.gray; // Gray for aluminum and other non-pushable metals
    
    [Tooltip("Color for anchored/fixed metal objects")]
    public Color anchoredColor = Color.red; // Red for anchored objects
    
    [Tooltip("Layer mask for metal objects (set in Unity Editor)")]
    public LayerMask metalLayer;
    
    [Header("Line Animation")]
    [Tooltip("Should the blue lines pulse/shimmer as described in the books?")]
    public bool enableLinePulse = true;
    [Tooltip("Speed of line pulsing effect")]
    public float pulseSpeed = 2f;
    [Tooltip("How much the line width varies during pulse")]
    public float pulseAmplitude = 0.02f;
    [Tooltip("Reference to player's chest transform (if null, uses camera)")]
    public Transform chestTransform;
    
    [Tooltip("Tag of player object to auto-find chest (if chestTransform is null)")]
    public string playerTag = "Player";
    
    // ===== REFERENCES =====
    [Header("References")]
    [Tooltip("Reference to the player's camera for line rendering origin")]
    public Camera playerCamera;
    
    [Header("Metal Cost")]
    [Tooltip("Tin drained per second while sight is active. MAG: 1 hr burn → TinDrainRate ≈ 0.0278/s")]
    public float metalCostPerSecond = MetallurgyConstants.TinDrainRate;

    [Tooltip("Maximum number of blue lines to pool (prevents infinite growth)")]
    public int maxLines = 100;

    // ===== PRIVATE STATE =====
    private bool isActive = false;
    private List<LineRenderer> activeLines = new List<LineRenderer>();
    private List<LineRenderer> linePool = new List<LineRenderer>();
    private Metallurgist metallurgist;
    
    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        metallurgist = GetComponentInParent<Metallurgist>();
        
        if (chestTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                Transform chest = player.transform.Find("Chest")
                               ?? player.transform.Find("ChestBone")
                               ?? player.transform.Find("Spine2")
                               ?? player.transform;
                chestTransform = chest;
            }
        }
        
        // Initialize line pool
        InitializeLinePool();
    }
    
    void InitializeLinePool()
    {
        // Pre-create line renderers and add to pool
        for (int i = 0; i < maxLines; i++)
        {
            CreateLineRenderer();
        }
    }
    
    LineRenderer CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("MetalLine_Pooled");
        lineObj.SetActive(false); // Start inactive
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // Set up line renderer properties
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            line.material = new Material(shader);
        }
        else
        {
            line.material = new Material(Shader.Find("Unlit/Color"));
        }
        
        line.positionCount = 2;
        line.useWorldSpace = true;
        
        // Add to pool
        linePool.Add(line);
        return line;
    }
    
    LineRenderer GetLineFromPool()
    {
        // If pool is empty, create a new one (unless we've hit max)
        if (linePool.Count == 0)
        {
            if (activeLines.Count + linePool.Count < maxLines)
            {
                return CreateLineRenderer();
            }
            else
            {
                // Reuse the oldest active line
                if (activeLines.Count > 0)
                {
                    LineRenderer oldest = activeLines[0];
                    activeLines.RemoveAt(0);
                    return oldest;
                }
                return null; // Shouldn't happen, but just in case
            }
        }
        
        // Get last line from pool
        LineRenderer line = linePool[linePool.Count - 1];
        linePool.RemoveAt(linePool.Count - 1);
        
        // Activate and return
        line.gameObject.SetActive(true);
        return line;
    }
    
    void ReturnLineToPool(LineRenderer line)
    {
        if (line == null) return;
        
        line.gameObject.SetActive(false);
        linePool.Add(line);
    }
    
    void Update()
    {
        // T key toggles Metallurgic Sight (Tab is the metal selection wheel)
        if (Input.GetKeyDown(Keybinds.MetalSight))
        {
            // Require Tin reserve to activate
            bool hasTin = metallurgist == null
                       || metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Tin) > 0f;
            if (!isActive && !hasTin) return;
            ToggleSight();
        }

        if (isActive)
        {
            // Auto-off when Tin runs out
            if (metallurgist != null && metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Tin) <= 0f)
            {
                ToggleSight();
                return;
            }

            VisualizeMetals();
            DrainMetal();
        }
    }

    void ToggleSight()
    {
        isActive = !isActive;
        if (!isActive)
            ClearLines();
    }
    
    // Draws lines from the player to all metal objects within range
    void VisualizeMetals()
    {
        // First, return all active lines to pool
        ReturnAllActiveLinesToPool();
        
        // Check if playerCamera is assigned
        if (playerCamera == null)
        {
            return;
        }
        
        // Determine origin point: chest if available, otherwise camera
        Vector3 originPoint = chestTransform != null ? chestTransform.position : playerCamera.transform.position;
        
        // Find all metal objects within range using Physics.OverlapSphere
        // This checks all colliders on the metalLayer within metalRange of this object
        Collider[] metals = Physics.OverlapSphere(transform.position, metalRange, metalLayer);
        
        // Track processed GameObjects to avoid duplicates
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();
        
        // Loop through each metal object found
        foreach (Collider metal in metals)
        {
            // Skip if we've already processed this GameObject
            GameObject metalObject = metal.gameObject;
            if (processedObjects.Contains(metalObject))
                continue;
            processedObjects.Add(metalObject);
            
            // Get a line renderer from pool
            LineRenderer line = GetLineFromPool();
            if (line == null) continue;
            
            // Set line start and end positions
            // Start: origin point (chest or camera)
            // End: at the metal object's center (transform.position)
            line.SetPosition(0, originPoint);
            line.SetPosition(1, metalObject.transform.position);
            
            // GetComponentInParent so MetallurgicTarget on a root object is found
            // even when the collider is on a child mesh
            MetallurgicTarget target = metal.GetComponentInParent<MetallurgicTarget>();
            
            // Determine color based on metal properties
            Color baseColor;
            if (target != null)
            {
                if (!target.canBePushed)
                {
                    // Non-pushable metals (aluminum, etc.)
                    baseColor = nonPushableColor;
                }
                else if (target.isAnchored || (metal.attachedRigidbody != null && metal.attachedRigidbody.isKinematic))
                {
                    // Anchored/fixed metals
                    baseColor = anchoredColor;
                }
                else
                {
                    // Normal pushable metals
                    float mass = target.GetEffectiveMass();
                    baseColor = mass > 10f ? heavyMetalColor : metalColor;
                }
            }
            else
            {
                // No MetallurgicTarget component - use mass-based color
                float mass = metal.attachedRigidbody != null ? metal.attachedRigidbody.mass : 1f;
                baseColor = mass > 10f ? heavyMetalColor : metalColor;
            }
            
            // Calculate distance for width calculation
            float distance = Vector3.Distance(originPoint, metalObject.transform.position);
            
            // Add pulsing alpha for shimmer effect
            if (enableLinePulse)
            {
                float alphaPulse = Mathf.Sin(Time.time * pulseSpeed * 0.5f + metal.GetInstanceID() * 0.2f);
                baseColor.a = 0.7f + alphaPulse * 0.3f; // Vary alpha between 0.4 and 1.0
            }
            
            // Make width based on distance (closer = thicker)
            float distanceFactor = 1f - Mathf.Clamp01(distance / metalRange);
            float currentLineWidth = lineWidth * (0.5f + distanceFactor * 0.5f);
            
            line.startWidth = currentLineWidth;
            line.endWidth = currentLineWidth * 0.8f; // Slightly thinner at the end
            
            line.startColor = baseColor;
            line.endColor = baseColor;
            
            // Add line to active list
            activeLines.Add(line);
        }
    }
    
    // Returns all active lines to the pool and clears the active list
    void ReturnAllActiveLinesToPool()
    {
        foreach (LineRenderer line in activeLines)
        {
            ReturnLineToPool(line);
        }
        activeLines.Clear();
    }
    
    // Legacy method for compatibility
    void ClearLines()
    {
        ReturnAllActiveLinesToPool();
    }
    
    void DrainMetal()
    {
        if (metallurgist != null)
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Tin, metalCostPerSecond * Time.deltaTime);
    }
    
    // Cleanup when object is destroyed
    void OnDestroy()
    {
        // Destroy all pooled and active line GameObjects
        foreach (LineRenderer line in linePool)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        foreach (LineRenderer line in activeLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        
        linePool.Clear();
        activeLines.Clear();
    }
    
    public bool IsActive() => isActive;
}
