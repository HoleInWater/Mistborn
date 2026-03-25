using UnityEngine;
using System.Collections.Generic;

public class TemporalAllomancy : MonoBehaviour
{
    [Header("Atium - See Enemy Futures")]
    public float atiumRange = 50f;
    public float atiumMetalCostPerSecond = 4f;
    public int maxGhostImages = 3;
    public float ghostLifetime = 0.5f;
    public Material ghostMaterial;

    [Header("Malatium - See Future Selves")]
    public float malatiumRange = 30f;
    public float malatiumMetalCostPerSecond = 3f;
    public Material futureSelfMaterial;

    [Header("Gold - See Past Selves")]
    public float goldRange = 20f;
    public float goldMetalCostPerSecond = 2f;
    public Material pastSelfMaterial;

    [Header("Electrum - See Your Futures")]
    public float electrumMetalCostPerSecond = 3f;
    public int maxFuturePaths = 5;
    public Material futurePathMaterial;

    [Header("References")]
    public Allomancer allomancer;
    public LayerMask targetLayer;

    private bool isAtiumBurning = false;
    private bool isMalatiumBurning = false;
    private bool isGoldBurning = false;
    private bool isElectrumBurning = false;

    private Dictionary<int, List<GameObject>> atiumGhosts = new Dictionary<int, List<GameObject>>();
    private List<GameObject> malatiumGhosts = new List<GameObject>();
    private List<GameObject> goldGhosts = new List<GameObject>();
    private List<GameObject> electrumGhosts = new List<GameObject>();

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
        targetLayer = LayerMask.GetMask("Character", "Enemy");
    }

    void Update()
    {
        bool wasAtium = isAtiumBurning;
        bool wasMalatium = isMalatiumBurning;
        bool wasGold = isGoldBurning;
        bool wasElectrum = isElectrumBurning;

        isAtiumBurning = allomancer != null && allomancer.IsBurning() && 
                        allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Atium;
        isMalatiumBurning = allomancer != null && allomancer.IsBurning() && 
                            allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Malatium;
        isGoldBurning = allomancer != null && allomancer.IsBurning() && 
                        allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Gold;
        isElectrumBurning = allomancer != null && allomancer.IsBurning() && 
                           allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Electrum;

        if (isAtiumBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SeeEnemyFutures(flareMult);
            DrainMetal(AllomancySkill.MetalType.Atium, atiumMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasAtium)
        {
            ClearAtiumGhosts();
        }

        if (isMalatiumBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SeeFutureSelves(flareMult);
            DrainMetal(AllomancySkill.MetalType.Malatium, malatiumMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasMalatium)
        {
            ClearGhosts(malatiumGhosts);
        }

        if (isGoldBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SeePastSelves(flareMult);
            DrainMetal(AllomancySkill.MetalType.Gold, goldMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasGold)
        {
            ClearGhosts(goldGhosts);
        }

        if (isElectrumBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SeeYourFutures(flareMult);
            DrainMetal(AllomancySkill.MetalType.Electrum, electrumMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasElectrum)
        {
            ClearGhosts(electrumGhosts);
        }
    }

    void SeeEnemyFutures(float flareMult)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, atiumRange * flareMult, targetLayer);

        foreach (Collider c in targets)
        {
            int id = c.GetInstanceID();
            if (!atiumGhosts.ContainsKey(id)) atiumGhosts[id] = new List<GameObject>();

            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 predictedPos = PredictPosition(rb, ghostLifetime);
                CreateGhost(id, predictedPos, ghostMaterial);
            }
        }

        foreach (var kvp in atiumGhosts)
        {
            if (!IsTargetInRange(kvp.Key))
            {
                ClearGhosts(kvp.Value);
                atiumGhosts.Remove(kvp.Key);
            }
        }
    }

    Vector3 PredictPosition(Rigidbody rb, float time)
    {
        Vector3 velocity = rb.linearVelocity;
        return rb.position + velocity * time;
    }

    void SeeFutureSelves(float flareMult)
    {
        ClearGhosts(malatiumGhosts);

        Vector3 currentPos = transform.position;
        Vector3 currentVel = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>().linearVelocity : Vector3.zero;

        for (int i = 0; i < maxGhostImages; i++)
        {
            float timeOffset = (i + 1) * 0.5f;
            Vector3 predictedPos = currentPos + currentVel * timeOffset;
            GameObject ghost = CreateGhostObject(predictedPos, futureSelfMaterial);
            malatiumGhosts.Add(ghost);
        }
    }

    void SeePastSelves(float flareMult)
    {
        ClearGhosts(goldGhosts);
        Debug.Log("[TEMPORAL] Gold shows possible past selves - visual effect placeholder");
    }

    void SeeYourFutures(float flareMult)
    {
        ClearGhosts(electrumGhosts);

        Vector3 currentPos = transform.position;
        Vector3 currentVel = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>().linearVelocity : Vector3.zero;

        for (int i = 0; i < maxFuturePaths; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 2f;
            Vector3 predictedPos = currentPos + (currentVel + offset) * 0.5f;
            GameObject ghost = CreateGhostObject(predictedPos, futurePathMaterial);
            electrumGhosts.Add(ghost);
        }
    }

    bool IsTargetInRange(int instanceId)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, atiumRange, targetLayer);
        foreach (Collider c in targets)
        {
            if (c.GetInstanceID() == instanceID) return true;
        }
        return false;
    }

    void CreateGhost(int targetId, Vector3 position, Material mat)
    {
        if (atiumGhosts[targetId].Count >= maxGhostImages) return;

        GameObject ghost = CreateGhostObject(position, mat);
        atiumGhosts[targetId].Add(ghost);
    }

    GameObject CreateGhostObject(Vector3 position, Material mat)
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        ghost.transform.position = position;
        
        if (mat != null)
        {
            Renderer r = ghost.GetComponent<Renderer>();
            r.material = mat;
            r.material.color = new Color(1f, 1f, 1f, 0.3f);
        }

        Destroy(ghost, ghostLifetime);
        return ghost;
    }

    void ClearGhosts(List<GameObject> ghosts)
    {
        foreach (GameObject g in ghosts)
        {
            if (g != null) Destroy(g);
        }
        ghosts.Clear();
    }

    void ClearAtiumGhosts()
    {
        foreach (var kvp in atiumGhosts)
        {
            foreach (GameObject g in kvp.Value)
            {
                if (g != null) Destroy(g);
            }
        }
        atiumGhosts.Clear();
    }

    void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(metal, amount);
    }
}