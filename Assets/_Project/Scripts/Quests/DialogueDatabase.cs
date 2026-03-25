using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Populates the BranchingDialogueManager with lore-accurate NPC dialogues at runtime.
/// 6 NPCs: Kelsier, Breeze, Ham, Clubs, Sazed, Vin — each with branching conversations.
/// </summary>
public class DialogueDatabase : MonoBehaviour
{
    void Start()
    {
        if (BranchingDialogueManager.Instance == null) return;
        PopulateDialogues();
    }

    void PopulateDialogues()
    {
        var mgr = BranchingDialogueManager.Instance;

        // ── Kelsier ──────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("kelsier_intro", "kelsier_1", new List<DialogueNode>
        {
            Node("kelsier_1", "Kelsier", "So you've Snapped. Good. The Lord Ruler's been ruling for a thousand years, and I intend to end that. You in?", new List<DialogueResponse>
            {
                Response("I'm in. What's the plan?", "kelsier_2", setFlag: "JOINED_CREW"),
                Response("Why should I trust you?", "kelsier_trust"),
                Response("I need to think about it.", "kelsier_wait")
            }),
            Node("kelsier_2", "Kelsier", "The plan is simple — we're going to overthrow the Final Empire. We steal the Lord Ruler's atium, fund an army, and tear it all down. But first, you need to learn your metals.", new List<DialogueResponse>
            {
                Response("Teach me about Allomancy.", "kelsier_teach", questToAdd: "main_01"),
                Response("Where do we start?", "kelsier_start", questToAdd: "main_01")
            }),
            Node("kelsier_trust", "Kelsier", "I survived the Pits of Hathsin. I lost my wife to the Lord Ruler. Trust isn't something I ask for — I earn it. Come train in the mists with me tonight, and you'll see.", new List<DialogueResponse>
            {
                Response("Alright, I'll give you a chance.", "kelsier_2", setFlag: "JOINED_CREW")
            }),
            Node("kelsier_wait", "Kelsier", "Take your time. But remember — every day you wait, the skaa suffer. When you're ready, find me at Clubs' shop.", isEnd: true),
            Node("kelsier_teach", "Kelsier", "Feel the metals in your stomach. Each one is like a separate well of energy. Focus on Steel first — that's your coin push. It'll save your life more than anything else.", isEnd: true, setFlag: "LEARNED_STEEL"),
            Node("kelsier_start", "Kelsier", "Meet me on the rooftops tonight. We'll practice Steel and Iron in the mists. Bring your coin pouch — you'll need it.", isEnd: true)
        }));

        mgr.LoadDialogue(CreateDialogue("kelsier_atium", "kat_1", new List<DialogueNode>
        {
            Node("kat_1", "Kelsier", "The Lord Ruler's power comes from atium. It lets you see the future — every possible move your opponent will make. We need to find his cache and destroy it.", new List<DialogueResponse>
            {
                Response("How do we find it?", "kat_2", setFlag: "LEARNED_ABOUT_ATIUM"),
                Response("Can we use it ourselves?", "kat_use")
            }),
            Node("kat_2", "Kelsier", "The Pits of Hathsin. That's where atium geodes grow. I know the way — I escaped from there, after all. But it won't be easy. Koloss guard the entrance.", new List<DialogueResponse>
            {
                Response("Let's do it.", "kat_go", questToAdd: "main_05"),
                Response("We need more preparation.", "kat_wait")
            }),
            Node("kat_use", "Kelsier", "Burn it if you must. But atium is finite — the Lord Ruler's been hoarding it for centuries. Every bead we burn is one less in his treasury. That's the real weapon.", isEnd: true, setFlag: "LEARNED_ABOUT_ATIUM"),
            Node("kat_go", "Kelsier", "That's the spirit! Survive, and you'll be the first person besides me to walk out of those pits alive.", isEnd: true),
            Node("kat_wait", "Kelsier", "Smart. Get your metals in order first. Come back when you're ready.", isEnd: true)
        }));

        // ── Breeze ───────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("breeze_intro", "breeze_1", new List<DialogueNode>
        {
            Node("breeze_1", "Breeze", "Ah, the new recruit. I'm Breeze — a Soother. I calm emotions, smooth over conflicts, and occasionally convince guards to look the other way. It's an art, really.", new List<DialogueResponse>
            {
                Response("Can you teach me to Soothe?", "breeze_teach"),
                Response("[Use Brass to Soothe Breeze]", "breeze_soothed", requiredFlag: "HAS_BRASS"),
                Response("Sounds manipulative.", "breeze_offended")
            }),
            Node("breeze_teach", "Breeze", "Brass is the key. Burn it and you can dampen any emotion in your target — fear, anger, suspicion. At the nobleman's ball, a well-placed Soothe is worth more than a hundred swords.", new List<DialogueResponse>
            {
                Response("I'll practice.", "breeze_end", questToAdd: "main_04", setFlag: "HELPED_BREEZE")
            }),
            Node("breeze_soothed", "Breeze", "Well now. You're Soothing me right now, aren't you? Impressive technique for a newcomer. Most people can't even tell when I'm doing it to them. You have real talent.", new List<DialogueResponse>
            {
                Response("Thanks, Breeze.", "breeze_end", setFlag: "BREEZE_IMPRESSED")
            }),
            Node("breeze_offended", "Breeze", "Manipulative? My dear, I prefer 'diplomatically persuasive.' Besides, when the alternative is a knife in the dark, a gentle Soothe seems positively humanitarian.", isEnd: true),
            Node("breeze_end", "Breeze", "Do come to me before the ball. I'll teach you the finer points of emotional Allomancy. It requires a light touch — nothing like that brute-force Steel Pushing.", isEnd: true)
        }));

        // ── Ham ──────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("ham_intro", "ham_1", new List<DialogueNode>
        {
            Node("ham_1", "Ham", "Here's a question for you: if a Pewterarm can heal from nearly any wound, does that make violence against them more or less ethical? I've been thinking about it all morning.", new List<DialogueResponse>
            {
                Response("Less ethical — they still feel the pain.", "ham_philosophical"),
                Response("More ethical — no lasting harm.", "ham_disagree"),
                Response("[Burn Pewter and arm-wrestle Ham]", "ham_wrestle", requiredFlag: "HAS_PEWTER")
            }),
            Node("ham_philosophical", "Ham", "Exactly! Pain is pain regardless of healing. That's what the obligators don't understand about the skaa — suffering doesn't become acceptable just because they survive it.", new List<DialogueResponse>
            {
                Response("You think about this a lot.", "ham_end", setFlag: "HAM_PHILOSOPHY")
            }),
            Node("ham_disagree", "Ham", "Hmm, interesting take. But consider this — if I punch a wall and my fist heals instantly, was the wall wrong to be hard? Sometimes I think the universe just enjoys watching us argue with it.", isEnd: true),
            Node("ham_wrestle", "Ham", "Ha! A Pewter challenge! Now you're speaking my language. Let's see what you've got — burn it hard, now!", new List<DialogueResponse>
            {
                Response("*burn Pewter at full intensity*", "ham_won", setFlag: "BEAT_HAM_WRESTLE")
            }),
            Node("ham_won", "Ham", "Hah! Good strength! You've got real potential. Kelsier chose well. Come find me when you need combat training — I'll teach you how to use Pewter in a fight, not just a bar brawl.", isEnd: true),
            Node("ham_end", "Ham", "Philosophy keeps the mind sharp. In our line of work, a sharp mind is the difference between a heist and a massacre. Come talk anytime.", isEnd: true)
        }));

        // ── Clubs ────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("clubs_intro", "clubs_1", new List<DialogueNode>
        {
            Node("clubs_1", "Clubs", "What do you want? If you're here to chat, go bother Ham. I'm busy maintaining the coppercloud.", new List<DialogueResponse>
            {
                Response("How does a coppercloud work?", "clubs_explain"),
                Response("Sorry to bother you.", "clubs_leave"),
                Response("Kelsier sent me for training.", "clubs_training")
            }),
            Node("clubs_explain", "Clubs", "Burn copper and you hide every Allomancer near you from Bronze Seekers. Without me, every Seeker in Luthadel would feel your metals burning from a mile away. You're welcome.", new List<DialogueResponse>
            {
                Response("Can you teach me Copper and Bronze?", "clubs_teach", questToAdd: "side_05")
            }),
            Node("clubs_leave", "Clubs", "Smart. First useful thing you've done today.", isEnd: true),
            Node("clubs_training", "Clubs", "Fine. Copper hides, Bronze seeks. Burn Copper to disappear from Seekers. Burn Bronze to find other Allomancers. Simple. Now practice until you can tell the difference between a Coinshot's pulses and a Thug's.", new List<DialogueResponse>
            {
                Response("I'll practice. Thanks, Clubs.", "clubs_end", setFlag: "CLUBS_TRUSTS_YOU")
            }),
            Node("clubs_teach", "Clubs", "Hmph. At least you're asking the right questions. Come back tonight — we'll practice with the coppercloud up so the Inquisitors can't feel us.", isEnd: true, setFlag: "CLUBS_TRUSTS_YOU"),
            Node("clubs_end", "Clubs", "Don't thank me. Just don't get caught. If the Inquisitors find this shop, we're all dead.", isEnd: true)
        }));

        // ── Sazed ────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("sazed_intro", "sazed_1", new List<DialogueNode>
        {
            Node("sazed_1", "Sazed", "I am Sazed, a Keeper of the Terris people. I store knowledge in my copperminds — the religions, histories, and sciences the Lord Ruler tried to destroy. How may I help you?", new List<DialogueResponse>
            {
                Response("Tell me about Feruchemy.", "sazed_feruchemy"),
                Response("What are the Keepers?", "sazed_keepers"),
                Response("Tell me about the Lord Ruler.", "sazed_lordr")
            }),
            Node("sazed_feruchemy", "Sazed", "Feruchemy is the power of the Terris people. We store attributes in metal — speed in steel, strength in pewter, health in gold. It is end-neutral: what you store, you may later retrieve. No more, no less.", new List<DialogueResponse>
            {
                Response("What about Compounding?", "sazed_compound", setFlag: "LEARNED_COMPOUNDING"),
                Response("Thank you, Sazed.", "sazed_end")
            }),
            Node("sazed_compound", "Sazed", "Ah. Compounding is... concerning. If one has both Allomancy and Feruchemy for the same metal, burning a charged metalmind produces a vast amplification of the Feruchemical attribute. The Lord Ruler uses gold Compounding — infinite health. That is why he cannot die by normal means.", new List<DialogueResponse>
            {
                Response("How do we beat him then?", "sazed_weakness"),
                Response("Fascinating.", "sazed_end")
            }),
            Node("sazed_weakness", "Sazed", "Remove his metalminds. Without them, he cannot Compound. Without Compounding gold, he is mortal — and a thousand years of age would claim him in moments. That is his one weakness, I believe.", isEnd: true, setFlag: "LEARNED_WEAKNESS"),
            Node("sazed_keepers", "Sazed", "The Keepers preserve the world's lost knowledge. The Lord Ruler destroyed every religion, every history that contradicted his narrative. We memorize them all, storing them in copperminds. It is our sacred duty to remember what was lost.", isEnd: true),
            Node("sazed_lordr", "Sazed", "He was once a Terris man named Rashek. He took the power at the Well of Ascension, reshaped the world, and has ruled for a thousand years. He is both Mistborn and Full Feruchemist — the most powerful being alive.", isEnd: true),
            Node("sazed_end", "Sazed", "If you wish to know more, I have three hundred religions stored in my copperminds. Each offers a different perspective on our world. The truth, I believe, lies somewhere in between them all.", isEnd: true)
        }));

        // ── Vin ──────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("vin_intro", "vin_1", new List<DialogueNode>
        {
            Node("vin_1", "Vin", "You're new too? I grew up on the streets — never knew I was Mistborn until Kelsier found me. It's... a lot to take in.", new List<DialogueResponse>
            {
                Response("Have you been training long?", "vin_training"),
                Response("What's it like, burning metals?", "vin_metals"),
                Response("We should spar sometime.", "vin_spar")
            }),
            Node("vin_training", "Vin", "A few months. Kelsier pushes hard, but he's the best teacher you'll find. The mist training at night is the hardest — flying between buildings with just Steel and Iron. One wrong push and you're street paste.", new List<DialogueResponse>
            {
                Response("Any tips?", "vin_tips")
            }),
            Node("vin_metals", "Vin", "Each metal feels different. Pewter is warm and strong, like fire in your veins. Tin is sharp — everything becomes so loud, so bright. Steel is... electric. Like lightning wanting to leap from your chest. You'll learn to love it.", isEnd: true),
            Node("vin_spar", "Vin", "Sure. But I warn you — I'm small, but Pewter makes up for it. And I've been practicing with coins. Kelsier says I'm a natural Coinshot.", new List<DialogueResponse>
            {
                Response("Let's do it!", "vin_end", setFlag: "VIN_FRIENDSHIP")
            }),
            Node("vin_tips", "Vin", "Always anchor to something heavier than you. If you push a coin and you're lighter than what's behind it, YOU move, not the coin. That's how you fly — push off coins on the ground, and the ground is heavier than you.", isEnd: true),
            Node("vin_end", "Vin", "Good. I could use someone else to practice with besides Kelsier. He always wins.", isEnd: true, setFlag: "VIN_FRIENDSHIP")
        }));

        mgr.LoadDialogue(CreateDialogue("vin_rebellion", "vr_1", new List<DialogueNode>
        {
            Node("vr_1", "Vin", "Kelsier says the rebellion is almost ready. The skaa are rising. Are you... scared?", new List<DialogueResponse>
            {
                Response("A little. You?", "vr_honest"),
                Response("No. We'll win.", "vr_confident"),
                Response("We have to be brave for the skaa.", "vr_brave")
            }),
            Node("vr_honest", "Vin", "Terrified. But I've been scared my whole life — of the streets, of the nobles, of the mists. At least now I'm scared while fighting for something that matters.", isEnd: true),
            Node("vr_confident", "Vin", "I wish I had your confidence. Kelsier does too — he never seems afraid. Maybe that's what it means to be a leader.", isEnd: true),
            Node("vr_brave", "Vin", "You sound like Kelsier. He says hope is the most important thing — more than Allomancy, more than armies. If the skaa believe they can win, they will.", isEnd: true, setFlag: "KELSIER_APPROVES")
        }));
    }

    // ── Helpers ───────────────────────────────────────────────────────

    Dialogue CreateDialogue(string id, string startNode, List<DialogueNode> nodes)
    {
        Dialogue d = ScriptableObject.CreateInstance<Dialogue>();
        d.dialogueId = id;
        d.startNodeId = startNode;
        d.nodes = nodes;
        return d;
    }

    DialogueNode Node(string id, string speaker, string text, List<DialogueResponse> responses = null,
        bool isEnd = false, string setFlag = null)
    {
        return new DialogueNode
        {
            nodeId = id,
            speakerName = speaker,
            text = text,
            responses = responses ?? new List<DialogueResponse>(),
            isEndNode = isEnd,
            setFlag = setFlag ?? "",
            nextNodeId = "",
            triggerEvent = ""
        };
    }

    DialogueResponse Response(string text, string nextNode, string requiredFlag = null,
        string setFlag = null, string questToAdd = null)
    {
        return new DialogueResponse
        {
            responseText = text,
            nextNodeId = nextNode,
            requiredFlag = requiredFlag ?? "",
            setFlagOnSelect = setFlag ?? "",
            questToAdd = questToAdd ?? "",
            conditionScript = ""
        };
    }
}
