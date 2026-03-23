// AllomanticSight.cs
// This script implements the Allomantic Sight ability (Tab key) which shows blue lines to metal objects.
// Lore: In Mistborn, Allomancers can see metal through the Spiritual Realm, represented as blue lines.
// This ability reveals all metal within range, passing through walls and geometry.

using UnityEngine;
using System.Collections;
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
    
    [Header("Line Animation")]
    [Tooltip("Should the blue lines pulse/shimmer as described in the books?")]
    public bool enableLinePulse = true;
    [Tooltip("Speed of line pulsing effect")]
    public float pulseSpeed = 2f;
    [Tooltip("How much the line width varies during pulse")]
    public float pulseAmplitude = 0.02f;
    [Tooltip("Reference to player's chest transform (if null, uses camera)")]
    public Transform chestTransform;
    
    // ===== REFERENCES =====
    [Header("References")]
    [Tooltip("Reference to the player's camera for line rendering origin")]
    public Camera playerCamera;
    
    // ===== PRIVATE STATE =====
    private bool isActive = false; // Whether the sight is currently active
    private List<LineRenderer> activeLines = new List<LineRenderer>(); // Pool of active line renderers
    private float metalReserve = 100f; // Current metal reserve for burning Tin
    private float metalCostPerSecond = 1f; // How fast metal drains while sight is active
    
    void Start()
    {
        // Auto-assign playerCamera to main camera if not set
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("AllomanticSight: No main camera found! Please assign playerCamera in Inspector.");
            }
        }
    }
    
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
#if UNITY_EDITOR
        Debug.Log(isActive ? "Allomantic Sight ACTIVE" : "Allomantic Sight OFF");
#endif
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
        
        // Check if playerCamera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("AllomanticSight: playerCamera is not assigned!");
            return;
        }
        
        // Determine origin point: chest if available, otherwise camera
        Vector3 originPoint = chestTransform != null ? chestTransform.position : playerCamera.transform.position;
        
        // Find all metal objects within range using Physics.OverlapSphere
        // This checks all colliders on the metalLayer within metalRange of this object
        Collider[] metals = Physics.OverlapSphere(transform.position, metalRange, metalLayer);
        
        // Loop through each metal object found
        foreach (Collider metal in metals)
        {
            // Calculate direction and distance for line properties
            Vector3 direction = (metal.transform.position - originPoint).normalized;
            float distance = Vector3.Distance(originPoint, metal.transform.position);
            
            // Create a new GameObject to hold the LineRenderer
            // PERFORMANCE NOTE: Creating new GameObjects every frame is inefficient
            // TODO: Implement object pooling for LineRenderers to improve performance
            GameObject lineObj = new GameObject("MetalLine");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            
            // Set line start and end positions
            // Start: origin point (chest or camera)
            // End: at the metal object's position
            line.SetPosition(0, originPoint);
            line.SetPosition(1, metal.transform.position);
            
            // Determine color based on mass: heavier objects get darker blue lines
            float mass = metal.attachedRigidbody != null ? metal.attachedRigidbody.mass : 1f;
            
            // Line width with pulsing effect
            float currentLineWidth = lineWidth;
            if (enableLinePulse)
            {
                // Add sinusoidal pulsing based on time and object position for variety
                float pulse = Mathf.Sin(Time.time * pulseSpeed + metal.GetInstanceID() * 0.1f);
                currentLineWidth += pulse * pulseAmplitude;
                currentLineWidth = Mathf.Max(0.01f, currentLineWidth); // Prevent zero or negative width
            }
            
            // Also make width based on distance (closer = thicker)
            float distanceFactor = 1f - Mathf.Clamp01(distance / metalRange);
            currentLineWidth *= (0.5f + distanceFactor * 0.5f);
            
            line.startWidth = currentLineWidth;
            line.endWidth = currentLineWidth * 0.8f; // Slightly thinner at the end
            
            // Safely create material - use default sprite material
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                line.material = new Material(shader);
            }
            else
            {
                // Fallback to default material
                line.material = new Material(Shader.Find("Unlit/Color"));
            }
            
            // Color with pulsing alpha for shimmer effect
            Color baseColor = mass > 10f ? heavyMetalColor : metalColor;
            if (enableLinePulse)
            {
                float alphaPulse = Mathf.Sin(Time.time * pulseSpeed * 0.5f + metal.GetInstanceID() * 0.2f);
                baseColor.a = 0.7f + alphaPulse * 0.3f; // Vary alpha between 0.4 and 1.0
            }
            
            line.startColor = baseColor;
            line.endColor = baseColor;
            
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
