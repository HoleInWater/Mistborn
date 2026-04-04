using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Populates the BranchingDialogueManager with lore-accurate NPC dialogues at runtime.
/// 6 NPCs: Darius, Lysander, Tormund, Grimshaw, Idris, Ember — each with branching conversations.
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

        // ── Darius ──────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("darius_intro", "darius_1", new List<DialogueNode>
        {
            Node("darius_1", "Darius", "So you've Snapped. Good. The Ashen King's been ruling for a thousand years, and I intend to end that. You in?", new List<DialogueResponse>
            {
                Response("I'm in. What's the plan?", "darius_2", setFlag: "JOINED_CREW"),
                Response("Why should I trust you?", "darius_trust"),
                Response("I need to think about it.", "darius_wait")
            }),
            Node("darius_2", "Darius", "The plan is simple — we're going to overthrow the Ashen Dominion. We steal the Ashen King's oraculum, fund an army, and tear it all down. But first, you need to learn your metals.", new List<DialogueResponse>
            {
                Response("Teach me about Metallurgy.", "darius_teach", questToAdd: "main_01"),
                Response("Where do we start?", "darius_start", questToAdd: "main_01")
            }),
            Node("darius_trust", "Darius", "I survived the Ember Pits. I lost my wife to the Ashen King. Trust isn't something I ask for — I earn it. Come train in the mists with me tonight, and you'll see.", new List<DialogueResponse>
            {
                Response("Alright, I'll give you a chance.", "darius_2", setFlag: "JOINED_CREW")
            }),
            Node("darius_wait", "Darius", "Take your time. But remember — every day you wait, the lowborn suffer. When you're ready, find me at Grimshaw' shop.", isEnd: true),
            Node("darius_teach", "Darius", "Feel the metals in your stomach. Each one is like a separate well of energy. Focus on Steel first — that's your coin push. It'll save your life more than anything else.", isEnd: true, setFlag: "LEARNED_STEEL"),
            Node("darius_start", "Darius", "Meet me on the rooftops tonight. We'll practice Steel and Iron in the mists. Bring your coin pouch — you'll need it.", isEnd: true)
        }));

        mgr.LoadDialogue(CreateDialogue("darius_oraculum", "kat_1", new List<DialogueNode>
        {
            Node("kat_1", "Darius", "The Ashen King's power comes from oraculum. It lets you see the future — every possible move your opponent will make. We need to find his cache and destroy it.", new List<DialogueResponse>
            {
                Response("How do we find it?", "kat_2", setFlag: "LEARNED_ABOUT_ATIUM"),
                Response("Can we use it ourselves?", "kat_use")
            }),
            Node("kat_2", "Darius", "The Ember Pits. That's where oraculum geodes grow. I know the way — I escaped from there, after all. But it won't be easy. Bloodbrute guard the entrance.", new List<DialogueResponse>
            {
                Response("Let's do it.", "kat_go", questToAdd: "main_05"),
                Response("We need more preparation.", "kat_wait")
            }),
            Node("kat_use", "Darius", "Burn it if you must. But oraculum is finite — the Ashen King's been hoarding it for centuries. Every bead we burn is one less in his treasury. That's the real weapon.", isEnd: true, setFlag: "LEARNED_ABOUT_ATIUM"),
            Node("kat_go", "Darius", "That's the spirit! Survive, and you'll be the first person besides me to walk out of those pits alive.", isEnd: true),
            Node("kat_wait", "Darius", "Smart. Get your metals in order first. Come back when you're ready.", isEnd: true)
        }));

        // ── Lysander ───────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("breeze_intro", "breeze_1", new List<DialogueNode>
        {
            Node("breeze_1", "Lysander", "Ah, the new recruit. I'm Lysander — a Queller. I calm emotions, smooth over conflicts, and occasionally convince guards to look the other way. It's an art, really.", new List<DialogueResponse>
            {
                Response("Can you teach me to Soothe?", "breeze_teach"),
                Response("[Use Brass to Soothe Lysander]", "breeze_soothed", requiredFlag: "HAS_BRASS"),
                Response("Sounds manipulative.", "breeze_offended")
            }),
            Node("breeze_teach", "Lysander", "Brass is the key. Burn it and you can dampen any emotion in your target — fear, anger, suspicion. At the nobleman's ball, a well-placed Soothe is worth more than a hundred swords.", new List<DialogueResponse>
            {
                Response("I'll practice.", "breeze_end", questToAdd: "main_04", setFlag: "HELPED_BREEZE")
            }),
            Node("breeze_soothed", "Lysander", "Well now. You're Soothing me right now, aren't you? Impressive technique for a newcomer. Most people can't even tell when I'm doing it to them. You have real talent.", new List<DialogueResponse>
            {
                Response("Thanks, Lysander.", "breeze_end", setFlag: "BREEZE_IMPRESSED")
            }),
            Node("breeze_offended", "Lysander", "Manipulative? My dear, I prefer 'diplomatically persuasive.' Besides, when the alternative is a knife in the dark, a gentle Soothe seems positively humanitarian.", isEnd: true),
            Node("breeze_end", "Lysander", "Do come to me before the ball. I'll teach you the finer points of emotional Metallurgy. It requires a light touch — nothing like that brute-force Steel Pushing.", isEnd: true)
        }));

        // ── Tormund ──────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("ham_intro", "ham_1", new List<DialogueNode>
        {
            Node("ham_1", "Tormund", "Here's a question for you: if a Ironhide can heal from nearly any wound, does that make violence against them more or less ethical? I've been thinking about it all morning.", new List<DialogueResponse>
            {
                Response("Less ethical — they still feel the pain.", "ham_philosophical"),
                Response("More ethical — no lasting harm.", "ham_disagree"),
                Response("[Burn Pewter and arm-wrestle Tormund]", "ham_wrestle", requiredFlag: "HAS_PEWTER")
            }),
            Node("ham_philosophical", "Tormund", "Exactly! Pain is pain regardless of healing. That's what the prelates don't understand about the lowborn — suffering doesn't become acceptable just because they survive it.", new List<DialogueResponse>
            {
                Response("You think about this a lot.", "ham_end", setFlag: "HAM_PHILOSOPHY")
            }),
            Node("ham_disagree", "Tormund", "Hmm, interesting take. But consider this — if I punch a wall and my fist heals instantly, was the wall wrong to be hard? Sometimes I think the universe just enjoys watching us argue with it.", isEnd: true),
            Node("ham_wrestle", "Tormund", "Ha! A Pewter challenge! Now you're speaking my language. Let's see what you've got — burn it hard, now!", new List<DialogueResponse>
            {
                Response("*burn Pewter at full intensity*", "ham_won", setFlag: "BEAT_HAM_WRESTLE")
            }),
            Node("ham_won", "Tormund", "Hah! Good strength! You've got real potential. Darius chose well. Come find me when you need combat training — I'll teach you how to use Pewter in a fight, not just a bar brawl.", isEnd: true),
            Node("ham_end", "Tormund", "Philosophy keeps the mind sharp. In our line of work, a sharp mind is the difference between a heist and a massacre. Come talk anytime.", isEnd: true)
        }));

        // ── Grimshaw ────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("clubs_intro", "clubs_1", new List<DialogueNode>
        {
            Node("clubs_1", "Grimshaw", "What do you want? If you're here to chat, go bother Tormund. I'm busy maintaining the coppercloud.", new List<DialogueResponse>
            {
                Response("How does a coppercloud work?", "clubs_explain"),
                Response("Sorry to bother you.", "clubs_leave"),
                Response("Darius sent me for training.", "clubs_training")
            }),
            Node("clubs_explain", "Grimshaw", "Burn copper and you hide every Metallurgist near you from Bronze Seekers. Without me, every Seeker in Cinderhold would feel your metals burning from a mile away. You're welcome.", new List<DialogueResponse>
            {
                Response("Can you teach me Copper and Bronze?", "clubs_teach", questToAdd: "side_05")
            }),
            Node("clubs_leave", "Grimshaw", "Smart. First useful thing you've done today.", isEnd: true),
            Node("clubs_training", "Grimshaw", "Fine. Copper hides, Bronze seeks. Burn Copper to disappear from Seekers. Burn Bronze to find other Metallurgists. Simple. Now practice until you can tell the difference between a Launcher's pulses and a Thug's.", new List<DialogueResponse>
            {
                Response("I'll practice. Thanks, Grimshaw.", "clubs_end", setFlag: "CLUBS_TRUSTS_YOU")
            }),
            Node("clubs_teach", "Grimshaw", "Hmph. At least you're asking the right questions. Come back tonight — we'll practice with the coppercloud up so the Sentinels can't feel us.", isEnd: true, setFlag: "CLUBS_TRUSTS_YOU"),
            Node("clubs_end", "Grimshaw", "Don't thank me. Just don't get caught. If the Sentinels find this shop, we're all dead.", isEnd: true)
        }));

        // ── Idris ────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("idris_intro", "idris_1", new List<DialogueNode>
        {
            Node("idris_1", "Idris", "I am Idris, a Archivist of the Valdris people. I store knowledge in my copperminds — the religions, histories, and sciences the Ashen King tried to destroy. How may I help you?", new List<DialogueResponse>
            {
                Response("Tell me about Storecraft.", "idris_storecraft"),
                Response("What are the Archivists?", "idris_keepers"),
                Response("Tell me about the Ashen King.", "idris_lordr")
            }),
            Node("idris_storecraft", "Idris", "Storecraft is the power of the Valdris people. We store attributes in metal — speed in steel, strength in pewter, health in gold. It is end-neutral: what you store, you may later retrieve. No more, no less.", new List<DialogueResponse>
            {
                Response("What about Compounding?", "idris_compound", setFlag: "LEARNED_COMPOUNDING"),
                Response("Thank you, Idris.", "idris_end")
            }),
            Node("idris_compound", "Idris", "Ah. Compounding is... concerning. If one has both Metallurgy and Storecraft for the same metal, burning a charged metalmind produces a vast amplification of the Storecrafted attribute. The Ashen King uses gold Compounding — infinite health. That is why he cannot die by normal means.", new List<DialogueResponse>
            {
                Response("How do we beat him then?", "idris_weakness"),
                Response("Fascinating.", "idris_end")
            }),
            Node("idris_weakness", "Idris", "Remove his metalminds. Without them, he cannot Compound. Without Compounding gold, he is mortal — and a thousand years of age would claim him in moments. That is his one weakness, I believe.", isEnd: true, setFlag: "LEARNED_WEAKNESS"),
            Node("idris_keepers", "Idris", "The Archivists preserve the world's lost knowledge. The Ashen King destroyed every religion, every history that contradicted his narrative. We memorize them all, storing them in copperminds. It is our sacred duty to remember what was lost.", isEnd: true),
            Node("idris_lordr", "Idris", "He was once a Valdris man named Varek. He took the power at the The Wellspring, reshaped the world, and has ruled for a thousand years. He is both Ashwalker and Full Storecrafter — the most powerful being alive.", isEnd: true),
            Node("idris_end", "Idris", "If you wish to know more, I have three hundred religions stored in my copperminds. Each offers a different perspective on our world. The truth, I believe, lies somewhere in between them all.", isEnd: true)
        }));

        // ── Ember ──────────────────────────────────────────────────────
        mgr.LoadDialogue(CreateDialogue("vin_intro", "vin_1", new List<DialogueNode>
        {
            Node("vin_1", "Ember", "You're new too? I grew up on the streets — never knew I was Ashwalker until Darius found me. It's... a lot to take in.", new List<DialogueResponse>
            {
                Response("Have you been training long?", "vin_training"),
                Response("What's it like, burning metals?", "vin_metals"),
                Response("We should spar sometime.", "vin_spar")
            }),
            Node("vin_training", "Ember", "A few months. Darius pushes hard, but he's the best teacher you'll find. The mist training at night is the hardest — flying between buildings with just Steel and Iron. One wrong push and you're street paste.", new List<DialogueResponse>
            {
                Response("Any tips?", "vin_tips")
            }),
            Node("vin_metals", "Ember", "Each metal feels different. Pewter is warm and strong, like fire in your veins. Tin is sharp — everything becomes so loud, so bright. Steel is... electric. Like lightning wanting to leap from your chest. You'll learn to love it.", isEnd: true),
            Node("vin_spar", "Ember", "Sure. But I warn you — I'm small, but Pewter makes up for it. And I've been practicing with coins. Darius says I'm a natural Launcher.", new List<DialogueResponse>
            {
                Response("Let's do it!", "vin_end", setFlag: "VIN_FRIENDSHIP")
            }),
            Node("vin_tips", "Ember", "Always anchor to something heavier than you. If you push a coin and you're lighter than what's behind it, YOU move, not the coin. That's how you fly — push off coins on the ground, and the ground is heavier than you.", isEnd: true),
            Node("vin_end", "Ember", "Good. I could use someone else to practice with besides Darius. He always wins.", isEnd: true, setFlag: "VIN_FRIENDSHIP")
        }));

        mgr.LoadDialogue(CreateDialogue("vin_rebellion", "vr_1", new List<DialogueNode>
        {
            Node("vr_1", "Ember", "Darius says the rebellion is almost ready. The lowborn are rising. Are you... scared?", new List<DialogueResponse>
            {
                Response("A little. You?", "vr_honest"),
                Response("No. We'll win.", "vr_confident"),
                Response("We have to be brave for the lowborn.", "vr_brave")
            }),
            Node("vr_honest", "Ember", "Terrified. But I've been scared my whole life — of the streets, of the nobles, of the mists. At least now I'm scared while fighting for something that matters.", isEnd: true),
            Node("vr_confident", "Ember", "I wish I had your confidence. Darius does too — he never seems afraid. Maybe that's what it means to be a leader.", isEnd: true),
            Node("vr_brave", "Ember", "You sound like Darius. He says hope is the most important thing — more than Metallurgy, more than armies. If the lowborn believe they can win, they will.", isEnd: true, setFlag: "KELSIER_APPROVES")
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
