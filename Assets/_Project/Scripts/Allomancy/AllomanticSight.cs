using UnityEngine;
using System.Collections.Generic;

public class AllomanticSight : MonoBehaviour
{
    [Header("Settings")]
    public float metalRange = 100f;
    public float lineWidth = 0.05f;
    public Color metalColor = Color.cyan;
    public Color heavyMetalColor = Color.blue;
    public LayerMask metalLayer;
    
    [Header("References")]
    public Camera playerCamera;
    
    private bool isActive = false;
    private List<LineRenderer> activeLines = new List<LineRenderer>();
    private float metalReserve = 100f;
    private float metalCostPerSecond = 1f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSight();
        }
        
        if (isActive)
        {
            VisualizeMetals();
            DrainMetal();
        }
    }
    
    void ToggleSight()
    {
        isActive = !isActive;
        
        if (!isActive)
        {
            ClearLines();
        }
        
        Debug.Log(isActive ? "Allomantic Sight ACTIVE" : "Allomantic Sight OFF");
    }
    
    void VisualizeMetals()
    {
        ClearLines();
        
        Collider[] metals = Physics.OverlapSphere(transform.position, metalRange, metalLayer);
        
        foreach (Collider metal in metals)
        {
            Vector3 direction = (metal.transform.position - playerCamera.transform.position).normalized;
            float distance = Vector3.Distance(playerCamera.transform.position, metal.transform.position);
            
            GameObject lineObj = new GameObject("MetalLine");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            
            line.SetPosition(0, playerCamera.transform.position + playerCamera.transform.forward * 0.5f);
            line.SetPosition(1, metal.transform.position);
            
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            
            float mass = metal.attachedRigidbody != null ? metal.attachedRigidbody.mass : 1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = mass > 10f ? heavyMetalColor : metalColor;
            line.endColor = line.startColor;
            
            activeLines.Add(line);
        }
    }
    
    void ClearLines()
    {
        foreach (LineRenderer line in activeLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        activeLines.Clear();
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            ToggleSight();
        }
    }
    
    void OnDestroy()
    {
        ClearLines();
    }
    
    public float GetMetalReserve() => metalReserve;
}
