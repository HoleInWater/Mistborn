using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Collectible lore books, notes, and journal entries.
/// When picked up, unlocks an entry in the Lore Codex.
/// </summary>
public class LoreCollectible : MonoBehaviour, IInteractable
{
    [Header("Lore Entry")]
    public string entryId;
    public string entryTitle;
    [TextArea(3, 10)] public string entryText;
    public LoreCategory category;
    public Sprite entryIcon;

    [Header("Pickup")]
    public bool destroyOnPickup = true;
    public GameObject pickupEffectPrefab;
    public bool collected = false;

    public enum LoreCategory
    {
        History,
        Metallurgy,
        Storecraft,
        Bloodforge,
        Religion,
        Geography,
        Characters,
        Creatures
    }

    public void Interact(GameObject player)
    {
        if (collected) return;
        collected = true;

        LoreCodex.Instance?.UnlockEntry(this);
        SoundManager.Instance?.PlayNotification();

        if (pickupEffectPrefab != null)
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        if (destroyOnPickup)
            Destroy(gameObject, 0.5f);
    }

    public string GetInteractionPrompt() => $"Press [F] to read \"{entryTitle}\"";
    public bool CanInteract() => !collected;
}

/// <summary>
/// Central codex that stores all collected lore entries.
/// Accessible from the pause menu.
/// </summary>
public class LoreCodex : MonoBehaviour
{
    public static LoreCodex Instance { get; private set; }

    [System.Serializable]
    public class LoreEntry
    {
        public string entryId;
        public string title;
        public string text;
        public LoreCollectible.LoreCategory category;
        public bool unlocked;
    }

    public List<LoreEntry> allEntries = new List<LoreEntry>();
    private Dictionary<string, LoreEntry> entryLookup = new Dictionary<string, LoreEntry>();

    public System.Action<LoreEntry> OnEntryUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        PopulateCodex();
    }

    void PopulateCodex()
    {
        // History
        Add("lore_deepness", "The The Blight", LoreCollectible.LoreCategory.History,
            "Before the Ashen King's Ascension, a terrible force known as the The Blight " +
            "threatened to destroy the world. Legends say it was a dark mist that consumed " +
            "everything in its path. The Ashen King claims to have defeated it at the Well " +
            "of Ascension, though some scholars whisper that the truth is far more complex.");

        Add("lore_ascension", "The Ashen King's Ascension", LoreCollectible.LoreCategory.History,
            "A thousand years ago, the The Prophesied One was prophesied to take the power at the " +
            "The Wellspring and save the world. The Ashen King — then known as Varek — " +
            "was a Valdris packman who seized the power instead. He remade the world: moving " +
            "the planet closer to the sun, creating the ashfalls, and establishing his eternal empire.");

        Add("lore_final_empire", "The Ashen Dominion", LoreCollectible.LoreCategory.History,
            "For a thousand years, the Ashen King has ruled with absolute power. The nobility " +
            "— descendants of his original allies — live in luxury, while the lowborn toil as slaves. " +
            "The Prelates serve as his bureaucracy, and the Iron Sentinels enforce his will. " +
            "No rebellion has ever succeeded. Until now.");

        Add("lore_lowborn_rebellion", "The Lowborn Rebellion", LoreCollectible.LoreCategory.History,
            "Darius, the Survivor of Ember Pits, has done what no one thought possible — he has " +
            "united the underground lowborn resistance into a true rebellion. His plan is audacious: " +
            "overthrow the Ashen King himself. The crew he has assembled includes Sparkbloods, " +
            "scholars, and ordinary lowborn who have had enough of a thousand years of oppression.");

        // Metallurgy
        Add("lore_metallurgy_origin", "The Origin of Metallurgy", LoreCollectible.LoreCategory.Metallurgy,
            "Metallurgy is a power of The Warden, one of the two primal forces of creation. " +
            "An Metallurgist ingests and 'burns' specific metals, using them as a catalyst to draw " +
            "upon The Warden's power. The metal is not the source of power — it is a key that " +
            "unlocks a connection to something far greater. Each metal produces a different effect.");

        Add("lore_ashwalker", "What is a Ashwalker?", LoreCollectible.LoreCategory.Metallurgy,
            "Most Metallurgists can only burn a single metal — they are called Sparkbloods. A Launcher " +
            "burns Steel, a Hauler burns Iron, a Thug burns Pewter. But extremely rarely, a person " +
            "can burn ALL sixteen Metallurgic metals. These people are called Ashwalker, and they are " +
            "among the most powerful beings in the Ashen Dominion.");

        Add("lore_snapping", "Snapping", LoreCollectible.LoreCategory.Metallurgy,
            "Metallurgic power is hereditary, passed through noble bloodlines. But the power lies " +
            "dormant until activated through extreme physical or emotional trauma — a process called " +
            "'Snapping.' The nobility beat their children in secret ceremonies, hoping to awaken " +
            "Metallurgic abilities. Many Snap and gain nothing. Some Snap and gain everything.");

        Add("lore_metal_purity", "Metal Purity", LoreCollectible.LoreCategory.Metallurgy,
            "An Metallurgist must burn metals of precise alloy composition. Impure metals cause " +
            "violent illness — nausea, pain, and potentially death. This is why Metallurgists carry " +
            "carefully prepared metal vials. The exact ratios of each alloy are closely guarded " +
            "secrets, often passed down through noble houses.");

        // Storecraft
        Add("lore_storecraft", "The Art of Storecraft", LoreCollectible.LoreCategory.Storecraft,
            "Storecraft is the power of the Valdris people, balanced between The Warden and The Unmaker. " +
            "Unlike Metallurgy which creates power from nothing, Storecraft is end-neutral — it stores " +
            "an attribute now to use later. A Storecrafter wearing a steel metalmind can store speed, " +
            "becoming slow now but blazingly fast when they tap that stored speed later.");

        Add("lore_compounding", "Compounding", LoreCollectible.LoreCategory.Storecraft,
            "When a person has BOTH Metallurgic and Storecrafted ability with the same metal, they " +
            "can 'Compound' — burning a charged metalmind Metallurgically produces a massive burst " +
            "of Storecrafted power. This is the Ashen King's secret to immortality: he Compounds " +
            "gold, gaining virtually unlimited healing. He has survived for a thousand years this way.");

        // Bloodforge
        Add("lore_bloodforge", "The Dark Art of Bloodforge", LoreCollectible.LoreCategory.Bloodforge,
            "Bloodforge is the power of The Unmaker — it steals abilities by driving metal spikes through " +
            "one person and into another. The Iron Sentinels are created through Bloodforge, their " +
            "eye-socket spikes granting them stolen Metallurgic abilities. But Bloodforge always loses " +
            "something in the transfer, and the spikes create a weakness The Unmaker can exploit.");

        Add("lore_sentinels", "Iron Sentinels", LoreCollectible.LoreCategory.Bloodforge,
            "The Ashen King's most terrifying enforcers. Each Sentinel has metal spikes driven " +
            "through their eyes, granting them Metallurgic abilities stolen from murdered Sparkbloods. " +
            "They can burn Steel, Iron, Pewter, Tin, and sometimes Oraculum. Their one weakness: a " +
            "single 'linchpin' spike in their back. Remove it, and they die instantly.");

        // Religion
        Add("lore_church_survivor", "The Church of the Survivor", LoreCollectible.LoreCategory.Religion,
            "Among the lowborn, a new religion has begun to spread — the Church of the Survivor. " +
            "They worship Darius, the only man to escape the Ember Pits alive. His message " +
            "is simple: the lowborn can fight back. The Ashen King is not a god. Hope is not dead.");

        Add("lore_terris_prophecy", "The Valdris Prophecy", LoreCollectible.LoreCategory.Religion,
            "The Valdris people have long prophesied the coming of the The Prophesied One — one who would " +
            "take the power at the The Wellspring and use it to save the world. The prophecy " +
            "has been altered over the centuries, its original meaning obscured. Some believe " +
            "Darius is the Hero. Others say the Hero has not yet come.");

        // Creatures
        Add("lore_bloodbrute", "Bloodbrute", LoreCollectible.LoreCategory.Creatures,
            "Bloodbrute are massive, blue-skinned creatures created through Bloodforge. They grow " +
            "continuously throughout their lives — their skin tears and stretches over their " +
            "expanding bodies, held together by metal spikes. They are mindlessly violent, " +
            "controlled only by the Ashen King's Metallurgic power. Without control, they rampage.");

        Add("lore_mistwraith", "Mistwraiths", LoreCollectible.LoreCategory.Creatures,
            "Mistwraiths are shapeless creatures that absorb the bones and flesh of the dead, " +
            "incorporating them into their own amorphous bodies. They lurk in the mists at night, " +
            "terrifying to encounter. Legend says they were once something else entirely — something " +
            "that was changed when the Ashen King remade the world.");

        // Build lookup
        foreach (var entry in allEntries)
            entryLookup[entry.entryId] = entry;
    }

    void Add(string id, string title, LoreCollectible.LoreCategory cat, string text)
    {
        allEntries.Add(new LoreEntry
        {
            entryId = id,
            title = title,
            text = text,
            category = cat,
            unlocked = false
        });
    }

    public void UnlockEntry(LoreCollectible collectible)
    {
        if (entryLookup.ContainsKey(collectible.entryId))
        {
            LoreEntry entry = entryLookup[collectible.entryId];
            if (!entry.unlocked)
            {
                entry.unlocked = true;
                OnEntryUnlocked?.Invoke(entry);

                // Check achievement
                CheckAllCollectedAchievement();
            }
        }
    }

    void CheckAllCollectedAchievement()
    {
        int total = allEntries.Count;
        int unlocked = 0;
        foreach (var e in allEntries)
            if (e.unlocked) unlocked++;

        if (unlocked >= total)
        {
            EventManager.TriggerEvent("AllLoreCollected");
        }
    }

    public LoreEntry GetEntry(string entryId)
    {
        return entryLookup.ContainsKey(entryId) ? entryLookup[entryId] : null;
    }

    public List<LoreEntry> GetEntriesByCategory(LoreCollectible.LoreCategory category)
    {
        return allEntries.FindAll(e => e.category == category);
    }

    public List<LoreEntry> GetUnlockedEntries()
    {
        return allEntries.FindAll(e => e.unlocked);
    }

    public int GetTotalEntryCount() => allEntries.Count;
    public int GetUnlockedEntryCount() => allEntries.FindAll(e => e.unlocked).Count;

    public List<string> GetUnlockedEntryIds()
    {
        List<string> ids = new List<string>();
        foreach (var e in allEntries)
            if (e.unlocked) ids.Add(e.entryId);
        return ids;
    }

    public void LoadUnlockedEntries(List<string> ids)
    {
        foreach (string id in ids)
        {
            if (entryLookup.ContainsKey(id))
                entryLookup[id].unlocked = true;
        }
    }
}
