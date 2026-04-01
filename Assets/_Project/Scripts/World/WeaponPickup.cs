using UnityEngine;

/// <summary>
/// A weapon lying on the ground waiting to be picked up.
/// Works two ways:
///   1. Player looks at it and presses G  (PlayerInteractor raycast + IInteractable)
///   2. Player walks within range and presses G  (trigger-based fallback)
///
/// Spawned automatically when an enemy dies with a weapon equipped.
/// Can also be placed manually in the scene and assigned a WeaponData asset.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("Display")]
    [Tooltip("Height the weapon bobs above its spawn position")]
    public float bobHeight    = 0.15f;
    [Tooltip("Seconds the weapon exists before disappearing")]
    public float lifetime     = 30f;

    private bool      _playerNearby;
    private float     _spawnTime;
    private Vector3   _basePosition;
    private GameObject _visual;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Trigger sphere so player proximity is detected
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = 1.5f;
    }

    void Start()
    {
        _spawnTime    = Time.time;
        _basePosition = transform.position;

        BuildVisual();
    }

    void Update()
    {
        // Expire
        if (Time.time - _spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Bob up and down
        float bob = Mathf.Sin((Time.time - _spawnTime) * 2f) * bobHeight;
        transform.position = _basePosition + Vector3.up * (0.3f + bob);

        // Spin
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);

        // Proximity G-key pickup (fallback when PlayerInteractor layer isn't set)
        if (_playerNearby && Input.GetKeyDown(Keybinds.Interact))
            PickUp();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = true;
        string name = weaponData != null ? weaponData.weaponName : "Weapon";
        NotificationSystem.Instance?.ShowNotification($"Press [G] to pick up {name}");
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerNearby = false;
    }

    // ── IInteractable — used by PlayerInteractor raycast ─────────────────────

    public void Interact(GameObject player)   => PickUp();
    public string GetInteractionPrompt()      => $"Pick up {weaponData?.weaponName ?? "Weapon"}";
    public bool   CanInteract()               => weaponData != null;

    // ── Pickup logic ──────────────────────────────────────────────────────────

    void PickUp()
    {
        if (weaponData == null) return;

        // Equip immediately
        EquipmentManager.Instance?.EquipWeapon(weaponData);

        // Add to inventory (maxStack 1 — weapons don't stack)
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

    // ── Visual ────────────────────────────────────────────────────────────────

    void BuildVisual()
    {
        if (weaponData == null) return;

        if (weaponData.prefab != null)
        {
            // Use the real weapon model
            _visual = Instantiate(weaponData.prefab, transform);
            _visual.transform.localPosition = Vector3.zero;
            _visual.transform.localRotation = Quaternion.identity;

            // Remove any colliders on the visual so they don't interfere
            foreach (var c in _visual.GetComponentsInChildren<Collider>())
                Destroy(c);
        }
        else
        {
            // Fallback: glowing cyan capsule so it's visible even without a model
            _visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _visual.transform.SetParent(transform);
            _visual.transform.localPosition = Vector3.zero;
            _visual.transform.localScale    = new Vector3(0.15f, 0.35f, 0.15f);
            Destroy(_visual.GetComponent<Collider>());

            var rend = _visual.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                       Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.8f, 1f);
                rend.sharedMaterial = mat;
            }
        }
    }
}
