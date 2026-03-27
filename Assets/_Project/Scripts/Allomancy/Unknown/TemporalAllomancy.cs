using UnityEngine;

/// <summary>
/// Unified temporal Allomancy: Atium (enemy futures), Gold (past self),
/// Electrum (own futures), Malatium (enemy nature).
/// Manages ghost renderers and temporal effects.
/// </summary>
public class TemporalAllomancy : MonoBehaviour
{
    [Header("Atium Settings")]
    public float atiumShadowLead = 0.5f;

    [Header("Gold Settings")]
    public float goldShadowTrail = 2f;
    public int goldHistoryFrames = 120;

    [Header("Electrum Settings")]
    public int electrumFutureCount = 3;
    public float electrumBranchAngle = 30f;

    [Header("References")]
    public Allomancer allomancer;

    // Gold: position history buffer
    private Vector3[] positionHistory;
    private Quaternion[] rotationHistory;
    private int historyIndex = 0;
    private GhostRenderer goldGhost;

    // Electrum: future path ghosts
    private GhostRenderer[] electrumGhosts;

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();

        // Gold history buffer
        positionHistory = new Vector3[goldHistoryFrames];
        rotationHistory = new Quaternion[goldHistoryFrames];
        for (int i = 0; i < goldHistoryFrames; i++)
        {
            positionHistory[i] = transform.position;
            rotationHistory[i] = transform.rotation;
        }
    }

    void Update()
    {
        if (allomancer == null) return;

        // Record position history every frame (for Gold)
        positionHistory[historyIndex] = transform.position;
        rotationHistory[historyIndex] = transform.rotation;
        historyIndex = (historyIndex + 1) % goldHistoryFrames;

        // Gold: show past shadow of self
        if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Gold))
        {
            UpdateGoldShadow();
            allomancer.DrainMetal(AllomancySkill.MetalType.Gold, 2f * Time.deltaTime);
        }
        else if (goldGhost != null)
        {
            Destroy(goldGhost);
            goldGhost = null;
        }

        // Electrum: show possible future paths of self
        if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Electrum))
        {
            UpdateElectrumShadows();
            allomancer.DrainMetal(AllomancySkill.MetalType.Electrum, 2f * Time.deltaTime);
        }
        else
        {
            ClearElectrumGhosts();
        }

        // Malatium: handled by Malatium.cs (reveals enemy weakness info)
    }

    void UpdateGoldShadow()
    {
        // Show where the player was N frames ago
        int pastIndex = (historyIndex - Mathf.RoundToInt(goldShadowTrail / Time.deltaTime) + goldHistoryFrames) % goldHistoryFrames;

        if (goldGhost == null)
        {
            goldGhost = gameObject.AddComponent<GhostRenderer>();
            goldGhost.SetupGhost(gameObject, new Color(1f, 0.8f, 0.2f), 0.3f);
        }

        goldGhost.UpdateTransform(positionHistory[pastIndex], rotationHistory[pastIndex]);
    }

    void UpdateElectrumShadows()
    {
        if (electrumGhosts == null)
        {
            electrumGhosts = new GhostRenderer[electrumFutureCount];
            for (int i = 0; i < electrumFutureCount; i++)
            {
                electrumGhosts[i] = gameObject.AddComponent<GhostRenderer>();
                Color c = new Color(0.8f, 0.8f, 0.3f, 0.2f);
                electrumGhosts[i].SetupGhost(gameObject, c, 0.2f);
            }
        }

        // Show branching possible futures based on current velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        for (int i = 0; i < electrumFutureCount; i++)
        {
            float angle = (i - (electrumFutureCount - 1) * 0.5f) * electrumBranchAngle;
            Vector3 futureVel = Quaternion.Euler(0, angle, 0) * rb.linearVelocity;
            float futureTime = 0.5f + i * 0.3f;

            Vector3 futurePos = AllomancyPhysicsFormulas.PredictPosition(
                transform.position, futureVel, futureTime);

            electrumGhosts[i].UpdateTransform(futurePos, transform.rotation);
        }
    }

    void ClearElectrumGhosts()
    {
        if (electrumGhosts == null) return;
        foreach (var ghost in electrumGhosts)
            if (ghost != null) Destroy(ghost);
        electrumGhosts = null;
    }

    void OnDestroy()
    {
        if (goldGhost != null) Destroy(goldGhost);
        ClearElectrumGhosts();
    }
}
