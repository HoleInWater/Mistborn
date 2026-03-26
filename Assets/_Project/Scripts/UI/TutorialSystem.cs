using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tutorial and help system. Shows contextual tips when the player does things for the first time.
/// Also provides an F1 help overlay with keybindings.
/// </summary>
public class TutorialSystem : MonoBehaviour
{
    public static TutorialSystem Instance { get; private set; }

    [Header("UI")]
    public GameObject tutorialPanel;
    public Text tutorialText;
    public GameObject helpOverlayPanel;
    public float tipDisplayDuration = 5f;

    private HashSet<string> shownTips = new HashSet<string>();
    private Queue<string> tipQueue = new Queue<string>();
    private bool isShowingTip = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
<<<<<<< HEAD
=======
        DontDestroyOnLoad(gameObject);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (helpOverlayPanel != null) helpOverlayPanel.SetActive(false);
    }

    void Update()
    {
        // F1 toggles help overlay
<<<<<<< HEAD
        if (Input.GetKeyDown(KeyCode.F1))
=======
        if (Input.GetKeyDown(Keybinds.Help))
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        {
            if (helpOverlayPanel != null)
                helpOverlayPanel.SetActive(!helpOverlayPanel.activeSelf);
        }
    }

    /// <summary>
    /// Show a contextual tip (only once per session).
    /// </summary>
    public void ShowTip(string tipId, string message)
    {
        if (shownTips.Contains(tipId)) return;
        shownTips.Add(tipId);
        tipQueue.Enqueue(message);

        if (!isShowingTip)
            StartCoroutine(ShowNextTip());
    }

    IEnumerator ShowNextTip()
    {
        while (tipQueue.Count > 0)
        {
            isShowingTip = true;
            string msg = tipQueue.Dequeue();

            if (tutorialPanel != null && tutorialText != null)
            {
                tutorialText.text = msg;
                tutorialPanel.SetActive(true);

                yield return new WaitForSecondsRealtime(tipDisplayDuration);

                tutorialPanel.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
        isShowingTip = false;
    }

    // Pre-built tutorial triggers for common first-time actions
    public void OnFirstSteelPush() => ShowTip("steel_push", "Steel Push (E): Push metal objects away from you. The heavier the object, the more YOU move instead.");
    public void OnFirstIronPull() => ShowTip("iron_pull", "Iron Pull (Q): Pull metal objects toward you. Anchor to heavy metals to launch yourself.");
    public void OnFirstTinBurn() => ShowTip("tin_burn", "Tin: Enhances all 5 senses. Watch out for sensory overload from bright lights and loud noises!");
    public void OnFirstPewterBurn() => ShowTip("pewter_burn", "Pewter: Enhanced strength, speed, and healing. You can survive falls and hits that would normally kill.");
    public void OnFirstFlare() => ShowTip("flare", "Flaring: Scroll wheel increases burn intensity. Only physical metals (Steel, Iron, Pewter, Tin) can be flared.");
    public void OnFirstCrouch() => ShowTip("crouch", "Crouch (Left Ctrl): Reduces your detection range. Combine with Copper burning to become nearly invisible.");
    public void OnFirstDodge() => ShowTip("dodge", "Dodge (Left Alt): Roll with invincibility frames. Pewter burning extends the i-frame window.");
}
