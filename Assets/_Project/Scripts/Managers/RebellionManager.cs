using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The central brain of the Skaa Rebellion. Tracks global variables that affect gameplay.
/// </summary>
public class RebellionManager : MonoBehaviour
{
    public static RebellionManager Instance { get; private set; }

    [Header("Global Rebellion Stats")]
    public float globalMorale = 50f;
    public float empireHeat = 10f; // Likelihood of Inquisitor spawns
    public int totalReserves = 1000; // Skaa warriors ready to revolt
    public int coinReserve = 5000;   // Currency for operations

    [System.Serializable]
    public class DominanceState
    {
        public string name;
        public float localMorale = 50f;
        public float localHeat = 0f;
        public bool isSupplied = true;
    }

    [Header("Regional Tracking")]
    public List<DominanceState> dominances = new List<DominanceState>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        InitializeEmpire();
    }

    private void InitializeEmpire()
    {
        string[] names = { "Central", "Northern", "Southern", "East", "West", "Venture", "Terris", "Remote" };
        foreach (var n in names)
        {
            dominances.Add(new DominanceState { name = n });
        }
    }

    /// <summary>
    /// Increases Empire Heat based on player Allomancy usage or visible kills.
    /// </summary>
    public void IncreaseHeat(float amount)
    {
        empireHeat = Mathf.Clamp(empireHeat + amount, 0f, 100f);
        Debug.Log($"[REBELLION] Empire Heat rising: {empireHeat}");
    }

    /// <summary>
    /// Rewards the player for successful mission completion.
    /// </summary>
    public void AddResources(int coins, int morale)
    {
        coinReserve += coins;
        globalMorale = Mathf.Clamp(globalMorale + morale, 0f, 100f);
        Debug.Log($"[REBELLION] Supplies Secured. Morale: {globalMorale}%");
    }

    void Update()
    {
        // Slowly decay heat if player is stealthy
        if (empireHeat > 0)
            empireHeat -= Time.deltaTime * 0.05f;
    }
}
