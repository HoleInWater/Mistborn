using UnityEngine;
using System.Collections;

namespace MistbornGame.Environment
{
    public class MetalDoor : MonoBehaviour
    {
        [Header("Door Configuration")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private float closeDelay = 3f;
        
        [Header("Door Type")]
        [SerializeField] private DoorType doorType = DoorType.Standard;
        [SerializeField] private KeyType requiredKeyType = KeyType.Standard;
        [SerializeField] private string requiredKeyId = "";
        
        [Header("Allomancy Interaction")]
        [SerializeField] private bool canBePushed = true;
        [SerializeField] private bool canBePulled = true;
        [SerializeField] private float pushForceThreshold = 25f;
        [SerializeField] private float pullForceThreshold = 20f;
        [SerializeField] private float forcedOpenSpeed = 5f;
        
        [Header("Sounds")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip forcedOpenSound;
        [SerializeField] private AudioClip metalClangSound;
        
        [Header("Visual")]
        [SerializeField] private Renderer doorRenderer;
        [SerializeField] private Material lockedMaterial;
        [SerializeField] private Material openMaterial;
        [SerializeField] private GameObject lockIndicator;
        [SerializeField] private ParticleSystem forcedOpenParticles;
        
        [Header("Physics")]
        [SerializeField] private Rigidbody doorRigidbody;
        [SerializeField] private HingeJoint doorHinge;
        [SerializeField] private float doorMass = 50f;
        
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private float currentAngle = 0f;
        private bool isAnimating = false;
        private bool isForcedOpen = false;
        private Coroutine closeCoroutine;
        
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;
        
        public enum DoorType
        {
            Standard,
            Reinforced,
            BankVault,
            Inquisitor,
            LordRuler
        }
        
        public enum KeyType
        {
            None,
            Standard,
            HouseKey,
            NobleKey,
            InquisitorKey,
            LordRulerKey
        }
        
        private void Start()
        {
            closedRotation = transform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
            
            if (doorRigidbody == null)
                doorRigidbody = GetComponent<Rigidbody>();
            
            if (doorRigidbody != null)
            {
                doorRigidbody.mass = doorMass;
                doorRigidbody.isKinematic = true;
            }
            
            UpdateLockIndicator();
        }
        
        private void Update()
        {
            if (isAnimating)
            {
                AnimateDoor();
            }
        }
        
        private void AnimateDoor()
        {
            float targetAngle = isOpen ? openAngle : 0f;
            float speed = isForcedOpen ? forcedOpenSpeed : openSpeed;
            
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * speed);
            
            transform.localRotation = closedRotation * Quaternion.Euler(0, currentAngle, 0);
            
            if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
            {
                isAnimating = false;
                isForcedOpen = false;
                
                if (!isOpen)
                {
                    AudioSource.PlayClipAtPoint(closeSound, transform.position);
                }
            }
        }
        
        public void Open()
        {
            if (isOpen || isAnimating)
                return;
            
            if (isLocked)
            {
                AudioSource.PlayClipAtPoint(lockedSound, transform.position);
                return;
            }
            
            isOpen = true;
            isAnimating = true;
            
            AudioSource.PlayClipAtPoint(openSound, transform.position);
            
            if (closeCoroutine != null)
                StopCoroutine(closeCoroutine);
            
            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }
        
        public void Close()
        {
            if (!isOpen || isAnimating)
                return;
            
            isOpen = false;
            isAnimating = true;
        }
        
        public void ToggleDoor()
        {
            if (isOpen)
                Close();
            else
                Open();
        }
        
        private IEnumerator CloseAfterDelay()
        {
            yield return new WaitForSeconds(closeDelay);
            
            if (!isForcedOpen)
            {
                Close();
            }
        }
        
        public bool Unlock(string keyId = "")
        {
            if (!isLocked)
                return true;
            
            if (requiredKeyId != "" && keyId == requiredKeyId)
            {
                isLocked = false;
                UpdateLockIndicator();
                return true;
            }
            
            if (requiredKeyType == KeyType.Standard && !string.IsNullOrEmpty(keyId))
            {
                isLocked = false;
                UpdateLockIndicator();
                return true;
            }
            
            return false;
        }
        
        public void Lock()
        {
            isLocked = true;
            UpdateLockIndicator();
            
            if (isOpen)
            {
                Close();
            }
        }
        
        public bool TryUnlockWithItem(InventoryItem keyItem)
        {
            if (keyItem == null)
                return false;
            
            if (requiredKeyType == KeyType.Standard)
            {
                return Unlock(keyItem.ItemId);
            }
            
            if (keyItem.ItemType == ItemType.Key)
            {
                if (requiredKeyId == keyItem.ItemId)
                {
                    return Unlock(requiredKeyId);
                }
            }
            
            return false;
        }
        
        public void ForceOpen(Vector3 forceDirection)
        {
            if (isAnimating && isForcedOpen)
                return;
            
            float force = forceDirection.magnitude;
            
            if (force >= pushForceThreshold || force >= pullForceThreshold)
            {
                StartForcedOpen(forceDirection);
            }
        }
        
        private void StartForcedOpen(Vector3 forceDirection)
        {
            if (isLocked)
            {
                Unlock();
            }
            
            isForcedOpen = true;
            isOpen = true;
            isAnimating = true;
            
            AudioSource.PlayClipAtPoint(forcedOpenSound, transform.position);
            AudioSource.PlayClipAtPoint(metalClangSound, transform.position);
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.1f, 0.2f);
            
            if (forcedOpenParticles != null)
            {
                forcedOpenParticles.Play();
            }
            
            if (doorRigidbody != null)
            {
                doorRigidbody.isKinematic = false;
                doorRigidbody.AddForce(forceDirection * 500f, ForceMode.Impulse);
            }
            
            if (closeCoroutine != null)
                StopCoroutine(closeCoroutine);
        }
        
        public void ApplyPushForce(Vector3 direction)
        {
            if (!canBePushed)
                return;
            
            float dotProduct = Vector3.Dot(direction, -transform.right);
            
            if (dotProduct > 0.5f)
            {
                ForceOpen(direction * pushForceThreshold * 2f);
            }
        }
        
        public void ApplyPullForce(Vector3 sourcePosition)
        {
            if (!canBePulled)
                return;
            
            Vector3 direction = (transform.position - sourcePosition).normalized;
            
            float dotProduct = Vector3.Dot(direction, transform.right);
            
            if (dotProduct > 0.5f)
            {
                ForceOpen(direction * pullForceThreshold * 2f);
            }
        }
        
        private void UpdateLockIndicator()
        {
            if (lockIndicator != null)
            {
                lockIndicator.SetActive(isLocked);
            }
            
            if (doorRenderer != null && lockedMaterial != null)
            {
                doorRenderer.material = isLocked ? lockedMaterial : null;
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 10f)
            {
                AudioSource.PlayClipAtPoint(metalClangSound, transform.position);
            }
        }
        
        public enum ItemType
        {
            Key,
            Other
        }
        
        public class InventoryItem
        {
            public string ItemId { get; set; }
            public ItemType ItemType { get; set; }
        }
    }
}
