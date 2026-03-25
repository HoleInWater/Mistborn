using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Death recap screen showing what killed the player and stats from the run.
/// </summary>
public class DeathRecapUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject deathPanel;
    public Text causeOfDeathText;
    public Text statsText;
    public Text tipText;

    [Header("Tips")]
    public string[] deathTips = {
        "Tip: Burn Pewter to survive otherwise-fatal hits.",
        "Tip: Iron Pull toward building anchors to escape quickly.",
        "Tip: Burning Copper hides you from enemy Seekers.",
        "Tip: Flare your metals for a burst of power when cornered.",
        "Tip: Duralumin + Steel Push creates an explosive escape launch.",
        "Tip: Dodge roll (Left Alt) grants invincibility frames.",
        "Tip: Ground slam with Pewter deals massive AOE on landing.",
        "Tip: Time bubbles (Bendalloy) let you act 10x faster than enemies.",
        "Tip: Drink metal vials (X key) to replenish your reserves mid-combat."
    };

    private float sessionDamageDealt;
    private float sessionDamageTaken;
    private int sessionKills;
    private string lastDamageSource = "Unknown";

    void Start()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        EventManager.RegisterEvent("PlayerDied", OnPlayerDied);
    }

    public void RecordDamageDealt(float amount) => sessionDamageDealt += amount;
    public void RecordDamageTaken(float amount, string source)
    {
        sessionDamageTaken += amount;
        lastDamageSource = source;
    }
    public void RecordKill() => sessionKills++;

    void OnPlayerDied()
    {
        ShowDeathRecap();
    }

    public void ShowDeathRecap()
    {
        if (deathPanel == null) return;
        deathPanel.SetActive(true);

        if (causeOfDeathText != null)
            causeOfDeathText.text = $"Killed by: {lastDamageSource}";

        if (statsText != null)
        {
            float playTime = GameManager.Instance != null ? GameManager.Instance.GetPlayTime() : 0f;
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            statsText.text = $"Time: {minutes}m {seconds}s\n" +
                           $"Enemies Defeated: {sessionKills}\n" +
                           $"Damage Dealt: {sessionDamageDealt:F0}\n" +
                           $"Damage Taken: {sessionDamageTaken:F0}";
        }

        if (tipText != null && deathTips.Length > 0)
            tipText.text = deathTips[Random.Range(0, deathTips.Length)];
    }

    public void OnRespawnClicked()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        GameManager.Instance?.RestartFromCheckpoint();
    }

    public void OnQuitClicked()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        GameManager.Instance?.ReturnToMainMenu();
    }

    public void ResetStats()
    {
        sessionDamageDealt = 0;
        sessionDamageTaken = 0;
        sessionKills = 0;
    }
}
