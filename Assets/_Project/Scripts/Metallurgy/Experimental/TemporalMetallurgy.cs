using UnityEngine;

/// <summary>
/// Unified temporal Metallurgy: Oraculum (enemy futures), Gold (past self),
/// Electrum (own futures), Revelum (enemy nature).
/// Manages ghost renderers and temporal effects.
/// </summary>
public class TemporalMetallurgy : MonoBehaviour
{
    [Header("Oraculum Settings")]
    public float oraculumShadowLead = 0.5f;

    [Header("Gold Settings")]
    public float goldShadowTrail = 2f;
    public int goldHistoryFrames = 120;

    [Header("Electrum Settings")]
    public int electrumFutureCount = 3;
    public float electrumBranchAngle = 30f;

    [Header("References")]
    public Metallurgist metallurgist;

    // Gold: position history buffer
    private Vector3[] positionHistory;
    private Quaternion[] rotationHistory;
    private int historyIndex = 0;
    private GhostRenderer goldGhost;

    // Electrum: future path ghosts
    private GhostRenderer[] electrumGhosts;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();

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
        if (metallurgist == null) return;

        // Record position history every frame (for Gold)
        positionHistory[historyIndex] = transform.position;
        rotationHistory[historyIndex] = transform.rotation;
        historyIndex = (historyIndex + 1) % goldHistoryFrames;

        // Gold: show past shadow of self
        if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Gold))
        {
            UpdateGoldShadow();
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Gold, 0.667f * Time.deltaTime); // MAG: 10 in-game min = 2.5 real min
        }
        else if (goldGhost != null)
        {
            Destroy(goldGhost);
            goldGhost = null;
        }

        // Electrum: show possible future paths of self
        if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Electrum))
        {
            UpdateElectrumShadows();
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Electrum, 0.667f * Time.deltaTime); // MAG: 10 in-game min = 2.5 real min
        }
        else
        {
            ClearElectrumGhosts();
        }

        // Revelum: handled by Revelum.cs (reveals enemy weakness info)
    }

    void UpdateGoldShadow()
    {
        // Show where the player was N frames ago
        if (Time.deltaTime <= 0f) return;
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

            Vector3 futurePos = MetallurgyPhysicsFormulas.PredictPosition(
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
