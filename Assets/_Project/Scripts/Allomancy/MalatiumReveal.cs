// NOTE: Lines 39 and 45 contain Debug.Log which should be removed for production
using System.Collections; // Fixes CS0246 (IEnumerator)
using UnityEngine;

public class MalatiumReveal : MonoBehaviour
{
    [Header("Settings")]
    public float metalCostPerSecond = 5f;
    public float revealRange = 20f;
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    private float metalReserve = 100f;
    private bool isBurning = false;

    // Public getter for your UI or other scripts
    public float GetMetalReserve() => metalReserve;
    
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
        Debug.Log("Stopped burning Malatium");
    }
    
    void RevealTrueNature()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, revealRange);
        
        foreach (Collider col in nearby)
        {
            // Fixes CS0619: Uses GetComponent instead of obsolete .renderer
            Renderer targetRenderer = col.GetComponent<Renderer>();

            // Ensure it has a renderer and hasn't already been changed
            if (targetRenderer != null && !targetRenderer.material.name.Contains("Malatium"))
            {
                // Save the original material before we change it
                Material originalMat = targetRenderer.material;
                
                // Apply the new visual
                targetRenderer.material.color = malatiumColor;
                targetRenderer.material.name += " (Malatium)"; 
                
                // Fixes CS1660: Start a Coroutine instead of using Invoke with a lambda
                StartCoroutine(RestoreMaterialAfterDelay(0.5f, targetRenderer, originalMat));
            }
        }
    }

    // The Coroutine that handles the waiting and restoring
    IEnumerator RestoreMaterialAfterDelay(float delay, Renderer renderer, Material originalMat)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material = originalMat;
        }
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
}
