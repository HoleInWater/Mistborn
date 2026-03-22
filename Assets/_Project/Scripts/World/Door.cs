using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public int requiredKeyID = 0;
    public float openAngle = 90f;
    public float openSpeed = 3f;
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip lockedSound;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private AudioSource audioSource;
    
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
        if (isOpen)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, openSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, openSpeed * Time.deltaTime);
        }
    }
    
    public void TryOpen(GameObject player)
    {
        if (isLocked)
        {
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory != null && inventory.HasKey(requiredKeyID))
            {
                Unlock();
                Open();
            }
            else
            {
                PlaySound(lockedSound);
                Debug.Log("Door is locked!");
            }
        }
        else
        {
            Open();
        }
    }
    
    public void Open()
    {
        isOpen = true;
        PlaySound(openSound);
    }
    
    public void Close()
    {
        isOpen = false;
    }
    
    public void Lock()
    {
        isLocked = true;
    }
    
    public void Unlock()
    {
        isLocked = false;
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
