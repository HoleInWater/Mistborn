/* FactionReputationUI.cs
 *
 * UI panel showing the player's reputation with all factions.
 * Accessible from the pause menu or a dedicated key.
 *
 * Shows each faction as a horizontal bar from -100 to +100 with
 * colored zones for Hostile/Unfriendly/Neutral/Friendly/Allied.
 * Current standing is highlighted with the faction's color.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FactionReputationUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Transform factionListContent;
    public GameObject factionEntryPrefab;
    public Button closeButton;

    [Header("Colors")]
    public Color hostileColor    = new Color(0.8f, 0.15f, 0.15f);
    public Color unfriendlyColor = new Color(0.7f, 0.4f, 0.2f);
    public Color neutralColor    = new Color(0.5f, 0.5f, 0.5f);
    public Color friendlyColor   = new Color(0.3f, 0.6f, 0.3f);
    public Color alliedColor     = new Color(0.2f, 0.7f, 0.9f);

    private bool isOpen;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    void Update()
    {
        // Toggle with J key
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isOpen) Close();
            else Open();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        if (panel == null) return;
        isOpen = true;
        panel.SetActive(true);
        Refresh();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Menu);
    }

    public void Close()
    {
        if (panel == null) return;
        isOpen = false;
        panel.SetActive(false);

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Gameplay);
    }

    void Refresh()
    {
        if (factionListContent == null || FactionSystem.Instance == null) return;

        // Clear existing entries
        foreach (Transform child in factionListContent)
            Destroy(child.gameObject);

        foreach (var faction in FactionSystem.Instance.factions)
        {
            CreateFactionEntry(faction);
        }
    }

    void CreateFactionEntry(Faction faction)
    {
        if (factionEntryPrefab == null)
        {
            // Create entry programmatically if no prefab
            var entry = new GameObject(faction.factionId);
            entry.transform.SetParent(factionListContent, false);

            var layout = entry.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;

            // Faction name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(entry.transform, false);
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = faction.displayName;
            nameTMP.fontSize = 16;
            nameTMP.color = faction.color;
            var nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.preferredWidth = 200f;

            // Reputation bar background
            var barBg = new GameObject("BarBG");
            barBg.transform.SetParent(entry.transform, false);
            var bgImg = barBg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.18f);
            var barLE = barBg.AddComponent<LayoutElement>();
            barLE.preferredWidth = 300f;
            barLE.preferredHeight = 20f;

            // Reputation fill
            var barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBg.transform, false);
            var fillImg = barFill.AddComponent<Image>();
            var fillRT = barFill.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(0f, 1f);
            fillRT.pivot = new Vector2(0f, 0.5f);

            // Map -100..+100 to 0..1 for the bar
            float normalized = (faction.reputation + 100f) / 200f;
            fillRT.anchorMax = new Vector2(normalized, 1f);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            // Color based on standing
            FactionStanding standing = FactionSystem.Instance.GetStanding(faction.factionId);
            fillImg.color = standing switch
            {
                FactionStanding.Hostile    => hostileColor,
                FactionStanding.Unfriendly => unfriendlyColor,
                FactionStanding.Neutral    => neutralColor,
                FactionStanding.Friendly   => friendlyColor,
                FactionStanding.Allied     => alliedColor,
                _ => neutralColor
            };

            // Center line (zero point)
            var centerLine = new GameObject("CenterLine");
            centerLine.transform.SetParent(barBg.transform, false);
            var lineImg = centerLine.AddComponent<Image>();
            lineImg.color = new Color(1f, 1f, 1f, 0.3f);
            var lineRT = centerLine.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.5f, 0f);
            lineRT.anchorMax = new Vector2(0.5f, 1f);
            lineRT.sizeDelta = new Vector2(2f, 0f);

            // Value text
            var valObj = new GameObject("Value");
            valObj.transform.SetParent(entry.transform, false);
            var valTMP = valObj.AddComponent<TextMeshProUGUI>();
            valTMP.text = $"{faction.reputation:+0;-0;0} ({standing})";
            valTMP.fontSize = 14;
            valTMP.color = fillImg.color;
            var valLE = valObj.AddComponent<LayoutElement>();
            valLE.preferredWidth = 120f;

            // Entry height
            var entryLE = entry.AddComponent<LayoutElement>();
            entryLE.preferredHeight = 30f;
        }
    }
}
