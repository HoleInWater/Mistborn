using UnityEngine;
using UnityEditor;

/// <summary>
/// Ashwalker → Create Default Weapons
/// Creates WeaponData ScriptableObject assets for the standard Ashwalker weapon roster.
/// Run once — assets are saved to Assets/_Project/Data/Weapons/
/// </summary>
public static class CreateDefaultWeapons
{
    [MenuItem("Ashwalker/Weapons/Create Default Weapons")]
    public static void Create()
    {
        const string folder = "Assets/_Project/Data/Weapons";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
            AssetDatabase.CreateFolder("Assets/_Project", "Data");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Data", "Weapons");

        MakeWeapon(folder, "Dagger",
            WeaponData.WeaponType.Dagger,
            description: "A short, fast blade favoured by thieves and Ashwalker. Metal — can be Pushed.",
            damage: 12f, range: 1.8f, speed: 3.5f, heavyMult: 2f,
            knockback: 5f, heavyKnockback: 12f,
            isMetal: true, mass: 0.5f, buy: 80, sell: 40);

        MakeWeapon(folder, "Iron Sword",
            WeaponData.WeaponType.Sword,
            description: "A balanced dueling blade used by noblemen and guards. Metal — can be Pushed.",
            damage: 20f, range: 2.5f, speed: 2.2f, heavyMult: 2.5f,
            knockback: 8f, heavyKnockback: 20f,
            isMetal: true, mass: 1.5f, buy: 150, sell: 75);

        MakeWeapon(folder, "Obsidian Axe",
            WeaponData.WeaponType.Axe,
            description: "Lowborn-forged axe with an obsidian head. Non-metal — cannot be Pushed or Pulled.",
            damage: 30f, range: 2.2f, speed: 1.5f, heavyMult: 3f,
            knockback: 14f, heavyKnockback: 30f,
            isMetal: false, mass: 2.5f, buy: 120, sell: 60);

        MakeWeapon(folder, "Spear",
            WeaponData.WeaponType.Spear,
            description: "A long wooden shaft tipped with iron. Reach advantage in open ground.",
            damage: 18f, range: 3.5f, speed: 1.8f, heavyMult: 2.2f,
            knockback: 10f, heavyKnockback: 22f,
            isMetal: false, mass: 2.0f, buy: 100, sell: 50);

        MakeWeapon(folder, "Pewter Mace",
            WeaponData.WeaponType.Mace,
            description: "Heavy pewter-headed mace. Devastating knockback, slow swing. Metal.",
            damage: 35f, range: 2.0f, speed: 1.2f, heavyMult: 3f,
            knockback: 22f, heavyKnockback: 45f,
            isMetal: true, mass: 4f, buy: 200, sell: 100);

        MakeWeapon(folder, "Bloodbrute Blade",
            WeaponData.WeaponType.BloodbruteBlade,
            description: "A massive sword too heavy for most humans. Only Pewter Sparkbloods can wield it effectively.",
            damage: 70f, range: 3.0f, speed: 0.8f, heavyMult: 3.5f,
            knockback: 40f, heavyKnockback: 80f,
            isMetal: true, mass: 12f, buy: 999, sell: 500);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateDefaultWeapons] Done — weapons created in " + folder);
        EditorUtility.DisplayDialog("Weapons Created",
            "Default weapons created in Assets/_Project/Data/Weapons/\n\n" +
            "Next steps:\n" +
            "1. Assign weapon prefabs in each WeaponData asset\n" +
            "2. Assign a starting weapon on the player's EquipmentManager component\n" +
            "3. Drag WeaponData assets into the ShopSystem's weapon inventory items",
            "OK");
    }

    static void MakeWeapon(string folder, string name, WeaponData.WeaponType type,
        string description, float damage, float range, float speed, float heavyMult,
        float knockback, float heavyKnockback, bool isMetal, float mass, int buy, int sell)
    {
        string path = $"{folder}/{name}.asset";
        WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
        bool isNew = w == null;
        if (isNew) w = ScriptableObject.CreateInstance<WeaponData>();

        w.weaponName          = name;
        w.type                = type;
        w.description         = description;
        w.damage              = damage;
        w.attackRange         = range;
        w.attackSpeed         = speed;
        w.heavyMultiplier     = heavyMult;
        w.knockbackForce      = knockback;
        w.heavyKnockbackForce = heavyKnockback;
        w.isMetal             = isMetal;
        w.mass                = mass;
        w.buyPrice            = buy;
        w.sellPrice           = sell;

        // Grip offsets tuned for Mixamo humanoid rigs.
        // (90,0,0) rotates weapon Y-axis onto hand Z-axis so it points along the fingers.
        // If weapon points backward, change to (-90,0,0) in the Inspector.
        ApplyGripDefaults(w);

        if (isNew) AssetDatabase.CreateAsset(w, path);
        else       EditorUtility.SetDirty(w);
    }

    // ── Grip offset defaults ──────────────────────────────────────────────────

    [MenuItem("Ashwalker/Weapons/Fix Weapon Grip Offsets")]
    public static void FixGrips()
    {
        string[] guids = AssetDatabase.FindAssets("t:WeaponData");
        int count = 0;
        foreach (string guid in guids)
        {
            WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (w == null) continue;
            ApplyGripDefaults(w);
            EditorUtility.SetDirty(w);
            count++;
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done",
            $"Grip offsets updated on {count} WeaponData assets.\n\n" +
            "If a weapon still looks wrong, select it in the Project window and\n" +
            "change Hand Rotation Offset X from 90 to -90 (or 0 to disable).",
            "OK");
    }

    /// <summary>
    /// Sets handPositionOffset and handRotationOffset for each weapon type.
    /// Rotation (90,0,0) maps weapon's Y-up axis to the hand bone's Z-forward axis
    /// (the direction fingers point on a Mixamo humanoid rig).
    /// Position offset moves the grip (not the tip) to the hand pivot.
    /// </summary>
    static void ApplyGripDefaults(WeaponData w)
    {
        switch (w.type)
        {
            case WeaponData.WeaponType.Dagger:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.05f);
                break;
            case WeaponData.WeaponType.Sword:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.1f);
                break;
            case WeaponData.WeaponType.Spear:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.3f);
                break;
            case WeaponData.WeaponType.Axe:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.1f);
                break;
            case WeaponData.WeaponType.Mace:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.05f);
                break;
            case WeaponData.WeaponType.BloodbruteBlade:
                w.handRotationOffset = new Vector3(90, 0, 0);
                w.handPositionOffset = new Vector3(0, 0, 0.15f);
                break;
        }
    }
}
