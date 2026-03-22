// NOTE: Lines 40 and 47 contain Debug.Log which should be removed for production
using UnityEngine;
using System.Collections.Generic;

public class AtiumBurn : MonoBehaviour
{
    [Header("Settings")]
    public float visionRange = 50f;
    public float metalCostPerSecond = 10f;
    public float ghostAlpha = 0.3f;
    
    [Header("References")]
    public Camera playerCamera;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private List<GameObject> futureGhosts = new List<GameObject>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.T) && isBurning)
        {
            SeeFutures();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.T))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Atium - Seeing the future!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearFutures();
        Debug.Log("Stopped burning Atium");
    }
    
    void SeeFutures()
    {
        ClearFutures();
        
        AIController[] enemies = FindObjectsOfType<AIController>();
        
        foreach (AIController enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > visionRange) continue;
            
            GameObject ghost = CreateFutureGhost(enemy);
            futureGhosts.Add(ghost);
        }
    }
    
    GameObject CreateFutureGhost(AIController enemy)
    {
        GameObject ghost = Instantiate(enemy.gameObject, enemy.transform.position, enemy.transform.rotation);
        ghost.name = $"FutureGhost_{enemy.name}";
        
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = new Material(r.material);
            Color c = r.material.color;
            r.material.color = new Color(c.r, c.g, c.b, ghostAlpha);
        }
        
        ghost.GetComponent<AIController>().enabled = false;
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;
        
        return ghost;
    }
    
    void ClearFutures()
    {
        foreach (GameObject ghost in futureGhosts)
        {
            if (ghost != null)
                Destroy(ghost);
        }
        futureGhosts.Clear();
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
    
    void OnDestroy()
    {
        ClearFutures();
    }
}
