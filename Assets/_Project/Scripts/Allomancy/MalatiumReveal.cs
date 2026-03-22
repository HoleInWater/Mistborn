using UnityEngine;
using System.Collections.Generic;

public class MalatiumReveal : MonoBehaviour
{
    [Header("Settings")]
    public float metalCostPerSecond = 5f;
    public float revealRange = 20f;
    public float revealDuration = 0.5f;
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    private HashSet<Renderer> revealedRenderers = new HashSet<Renderer>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.Y) && isBurning)
        {
            RevealTrueNature();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.Y))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Malatium - Revealing true nature!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreAllMaterials();
        Debug.Log("Stopped burning Malatium");
    }
    
    void RevealTrueNature()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, revealRange);
        
        foreach (Collider col in nearby)
        {
            Renderer renderer = col.GetComponent<Renderer>();
            if (renderer != null && !revealedRenderers.Contains(renderer))
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material;
                }
                revealedRenderers.Add(renderer);
                renderer.material.color = malatiumColor;
            }
        }
    }
    
    void RestoreAllMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material = kvp.Value;
            }
        }
        originalMaterials.Clear();
        revealedRenderers.Clear();
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            StopBurning();
        }
    }
    
    public float GetMetalReserve() => metalReserve;
}
