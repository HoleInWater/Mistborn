using UnityEngine;

public class MalatiumReveal : MonoBehaviour
{
    [Header("Settings")]
    public float metalCostPerSecond = 5f;
    public float revealRange = 20f;
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    
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
            Renderer renderer = col.GetComponent<Renderer>();
            if (renderer != null && !renderer.material.name.Contains("Malatium"))
            {
                Material originalMat = new Material(renderer.material);
                renderer.material.color = malatiumColor;
                
                Invoke(() => RestoreMaterial(renderer, originalMat), 0.5f);
            }
        }
    }
    
    void RestoreMaterial(Renderer renderer, Material originalMat)
    {
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
    
    public float GetMetalReserve() => metalReserve;
}
