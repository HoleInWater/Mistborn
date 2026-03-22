/// <summary>
/// Interactive door that can be locked/unlocked.
/// Usage: Door door = GetComponent<Door>();
/// 
/// METHODS:
///   door.TryOpen(player);
///   door.Lock();
///   door.Unlock();
///   door.IsLocked
/// </summary>
public class Door : MonoBehaviour
{
    // SETTINGS
    public bool isLocked = false;              // Is door initially locked
    public int requiredKeyID = 0;              // Key ID required to unlock
    public float openAngle = 90f;              // How much door opens
    public float openSpeed = 3f;               // Door open/close speed
    
    // AUDIO
    public AudioClip openSound;               // Sound when door opens
    public AudioClip lockedSound;              // Sound when door is locked
    public AudioClip unlockSound;              // Sound when door unlocks
    
    // EFFECTS
    public GameObject openEffect;              // VFX when door opens
    
    // INTERNAL STATE
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private AudioSource audioSource;
    
    // EVENTS
    public System.Action OnDoorOpened;
    public System.Action OnDoorClosed;
    public System.Action OnDoorLocked;
    
    // PUBLIC API
    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, target, openSpeed * Time.deltaTime);
    }
    
    void OnTriggerInteract(Collider player)
    {
        TryOpen(player.gameObject);
    }
    
    public bool TryOpen(GameObject player)
    {
        if (isLocked)
        {
            Inventory inventory = player.GetComponent<Inventory>();
            
            if (inventory != null && inventory.HasKey(requiredKeyID))
            {
                Unlock();
            }
            else
            {
                PlaySound(lockedSound);
                Debug.Log($"Door is locked! Requires key ID: {requiredKeyID}");
                OnDoorLocked?.Invoke();
                return false;
            }
        }
        
        Open();
        return true;
    }
    
    public void Open()
    {
        if (isOpen) return;
        
        isOpen = true;
        PlaySound(openSound);
        
        if (openEffect != null)
        {
            Instantiate(openEffect, transform.position, Quaternion.identity);
        }
        
        OnDoorOpened?.Invoke();
    }
    
    public void Close()
    {
        if (!isOpen) return;
        
        isOpen = false;
        PlaySound(openSound);
        OnDoorClosed?.Invoke();
    }
    
    public void Lock()
    {
        isLocked = true;
    }
    
    public void Unlock()
    {
        isLocked = false;
        PlaySound(unlockSound);
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
