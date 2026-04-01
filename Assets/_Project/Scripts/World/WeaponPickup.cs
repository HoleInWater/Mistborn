using UnityEngine;

/// <summary>
/// A weapon lying on the ground waiting to be picked up.
/// Detects the player by distance every frame — no tags, no triggers, no layer setup needed.
/// Press G (Keybinds.Interact) when nearby to pick it up.
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("Settings")]
    public float pickupRadius = 2.5f;
    public float bobHeight    = 0.15f;
    public float lifetime     = 30f;

    private Transform  _player;
    private float      _spawnTime;
    private Vector3    _basePosition;
    private bool       _promptShowing;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _spawnTime    = Time.time;
        _basePosition = transform.position;

        // Find player once — works regardless of tag
        _player = FindPlayer();

        BuildVisual();

        Debug.Log($"[WeaponPickup] Spawned: {weaponData?.weaponName} — press G within {pickupRadius}m");
    }

    void Update()
    {
        if (Time.time - _spawnTime > lifetime) { Destroy(gameObject); return; }

        // Bob and spin
        float bob = Mathf.Sin((Time.time - _spawnTime) * 2.5f) * bobHeight;
        transform.position = _basePosition + Vector3.up * (0.4f + bob);
        transform.Rotate(0f, 80f * Time.deltaTime, 0f);

        if (_player == null) { _player = FindPlayer(); return; }

        float dist = Vector3.Distance(transform.position, _player.position);
        bool nearby = dist <= pickupRadius;

        // Show / hide prompt
        if (nearby && !_promptShowing)
        {
            _promptShowing = true;
            NotificationSystem.Instance?.ShowNotification(
                $"Press [G] to pick up {weaponData?.weaponName ?? "Weapon"}");
            Debug.Log($"[WeaponPickup] Player in range ({dist:F1}m) — press G");
        }
        else if (!nearby && _promptShowing)
        {
            _promptShowing = false;
        }

        // Pick up on G press
        if (nearby && Input.GetKeyDown(Keybinds.Interact))
        {
            Debug.Log("[WeaponPickup] G pressed — picking up");
            PickUp();
        }
    }

    // ── IInteractable — PlayerInteractor raycast path ─────────────────────────

    public void   Interact(GameObject player) => PickUp();
    public string GetInteractionPrompt()      => $"Pick up {weaponData?.weaponName ?? "Weapon"}";
    public bool   CanInteract()               => weaponData != null;

    // ── Core pickup ───────────────────────────────────────────────────────────

    void PickUp()
    {
        if (weaponData == null) return;

        EquipmentManager.Instance?.EquipWeapon(weaponData);

        Inventory.Instance?.AddItem(new InventoryItem
        {
            itemId      = "weapon_" + weaponData.weaponName.ToLower().Replace(" ", "_"),
            itemName    = weaponData.weaponName,
            description = weaponData.description,
            type        = InventoryItem.ItemType.Weapon,
            quantity    = 1,
            maxStack    = 1,
            weight      = weaponData.mass,
            icon        = weaponData.icon,
            weaponData  = weaponData
        });

        Debug.Log($"[WeaponPickup] Picked up: {weaponData.weaponName}");
        Destroy(gameObject);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Transform FindPlayer()
    {
        // Try EquipmentManager singleton first (most reliable)
        if (EquipmentManager.Instance != null)
            return EquipmentManager.Instance.transform;

        // Fall back to tag
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) return go.transform;

        // Last resort: find by PlayerCombat component
        PlayerCombat pc = FindObjectOfType<PlayerCombat>();
        return pc != null ? pc.transform : null;
    }

    void BuildVisual()
    {
        if (weaponData == null) return;

        GameObject visual;

        if (weaponData.prefab != null)
        {
            visual = Instantiate(weaponData.prefab, transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            foreach (var c in visual.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
                Destroy(c);
            }
        }
        else
        {
            // Cyan capsule fallback so it's always visible
            visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale    = new Vector3(0.12f, 0.3f, 0.12f);
            Destroy(visual.GetComponent<Collider>());

            var rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                       Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.85f, 1f);
                rend.sharedMaterial = mat;
            }
        }
    }
}
