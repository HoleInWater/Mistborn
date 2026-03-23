// AllomanticSight.cs
// This script implements the Allomantic Sight ability (Tab key) which shows blue lines to metal objects.
// Lore: In Mistborn, Allomancers can see metal through the Spiritual Realm, represented as blue lines.
// This ability reveals all metal within range, passing through walls and geometry.

using UnityEngine;
using System.Collections
using System.Collections.Generic;

public class AllomanticSight : MonoBehaviour
{
    // ===== SETTINGS =====
    // Maximum distance to detect metal objects (lore: about 100 meters for a skilled Mistborn)
    [Header("Settings")]
    [Tooltip("Maximum distance to detect metal objects")]
    public float metalRange = 100f;
    
    [Tooltip("Width of the blue lines showing metal")]
    public float lineWidth = 0.05f;
    
    [Tooltip("Color for normal metal objects (coins, brackets, etc.)")]
    public Color metalColor = Color.cyan; // Blue/cyan as described in the books
    
    [Tooltip("Color for heavy metal objects (Koloss weapons, large structures)")]
    public Color heavyMetalColor = Color.blue; // Darker blue for heavier objects
    
    [Tooltip("Layer mask for metal objects (set in Unity Editor)")]
    public LayerMask metalLayer;
    
    // ===== REFERENCES =====
    [Header("References")]
    [Tooltip("Reference to the player's camera for line rendering origin")]
    public Camera playerCamera;
    
    // ===== PRIVATE STATE =====
    private bool isActive = false; // Whether the sight is currently active
    private List<LineRenderer> activeLines = new List<LineRenderer>(); // Pool of active line renderers
    private float metalReserve = 100f; // Current metal reserve for burning Tin
    private float metalCostPerSecond = 1f; // How fast metal drains while sight is active
    
    // Update is called once per frame
    void Update()
    {
        // Check for Tab key press to toggle Allomantic Sight
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSight();
        }
        
        // If sight is active, continuously update metal visualization and drain metal
        if (isActive)
        {
            VisualizeMetals(); // Draw lines to all metals
            DrainMetal();      // Consume metal reserve
        }
    }
    
    // Toggles the Allomantic Sight on/off
    void ToggleSight()
    {
        isActive = !isActive;
        
        // If turning off, clear all existing lines
        if (!isActive)
        {
            ClearLines();
            Time.timeScale = 1f; // Reset time scale when turning off
        }
        else
        {
            // Slow-motion effect when turning on
            StartCoroutine(SlowMotionEffect(0.3f));
        }
        
        // Log state change (this Debug.Log should be removed for production)
        Debug.Log(isActive ? "Allomantic Sight ACTIVE" : "Allomantic Sight OFF");
    }
    
    IEnumerator SlowMotionEffect(float duration)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
    
    // Draws lines from the player to all metal objects within range
    void VisualizeMetals()
    {
        // First, clear any existing lines from previous frame
        ClearLines();
        
        // Find all metal objects within range using Physics.OverlapSphere
        // This checks all colliders on the metalLayer within metalRange of this object
        Collider[] metals = Physics.OverlapSphere(transform.position, metalRange, metalLayer);
        
        // Loop through each metal object found
        foreach (Collider metal in metals)
        {
            // Calculate direction and distance for potential future use (e.g., distance-based line thickness)
            Vector3 direction = (metal.transform.position - playerCamera.transform.position).normalized;
            float distance = Vector3.Distance(playerCamera.transform.position, metal.transform.position);
            
            // Create a new GameObject to hold the LineRenderer
            GameObject lineObj = new GameObject("MetalLine");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            
            // Set line start and end positions
            // Start: slightly in front of camera (to avoid clipping with camera model)
            // End: at the metal object's position
            line.SetPosition(0, playerCamera.transform.position + playerCamera.transform.forward * 0.5f);
            line.SetPosition(1, metal.transform.position);
            
            // Set line width
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            
            // Determine color based on mass: heavier objects get darker blue lines
            float mass = metal.attachedRigidbody != null ? metal.attachedRigidbody.mass : 1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = mass > 10f ? heavyMetalColor : metalColor;
            line.endColor = line.startColor;
            
            // Add line to our tracking list for cleanup
            activeLines.Add(line);
        }
    }
    
    // Destroys all active line renderers and clears the list
    void ClearLines()
    {
        foreach (LineRenderer line in activeLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        activeLines.Clear();
    }
    
    // Drains metal reserve while sight is active
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            ToggleSight(); // Auto-turn off when out of metal
        }
    }
    
    // Cleanup when object is destroyed
    void OnDestroy()
    {
        ClearLines();
    }
    
    // Public getter for metal reserve (for UI display)
    public float GetMetalReserve() => metalReserve;
}
