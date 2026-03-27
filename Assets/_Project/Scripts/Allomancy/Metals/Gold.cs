using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Gold Allomancy ability (Augur).
/// Lore: Shows a "Gold Shadow" (potential past/alternate self) of the player.
/// </summary>
public class Gold : MonoBehaviour
{
    [Header("Settings")]
    public float ghostAlpha = 0.4f;
    public Color goldShadowColor = new Color(1f, 0.8f, 0f, 1f);

    private Allomancer allomancer;
    private GhostRenderer goldShadow;
    private bool isBurning = false;

    // Buffer for past states
    private struct PlayerState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
    }
    private Queue<PlayerState> stateBuffer = new Queue<PlayerState>();
    public float delayInSeconds = 2f;

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        // Record state
        stateBuffer.Enqueue(new PlayerState { position = transform.position, rotation = transform.rotation, time = Time.time });

        // Maintain buffer size (keep approx 2 seconds of state)
        while (stateBuffer.Count > 0 && Time.time - stateBuffer.Peek().time > delayInSeconds)
        {
            stateBuffer.Dequeue();
        }

        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Gold;

        if (isBurning)
        {
            if (goldShadow == null) CreateShadow();
            UpdateShadow();
        }
        else if (wasBurning)
        {
            DestroyShadow();
        }
    }

    void CreateShadow()
    {
        GameObject go = new GameObject("GoldShadow");
        goldShadow = go.AddComponent<GhostRenderer>();
        goldShadow.SetupGhost(gameObject, goldShadowColor, ghostAlpha);
    }

    void UpdateShadow()
    {
        if (goldShadow == null || stateBuffer.Count == 0) return;
        
        // Show the state from 2 seconds ago
        var pastState = stateBuffer.Peek();
        goldShadow.UpdateTransform(pastState.position, pastState.rotation);
    }

    void DestroyShadow()
    {
        if (goldShadow != null)
        {
            Destroy(goldShadow.gameObject);
            goldShadow = null;
        }
    }

    void OnDestroy() => DestroyShadow();
}
