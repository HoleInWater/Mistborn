using UnityEngine;

namespace MistbornGameplay
{
    public class SkimmingController : MonoBehaviour
    {
        [Header("Skimming Settings")]
        [SerializeField] private float baseSkimSpeed = 15f;
        [SerializeField] private float maxSkimHeight = 20f;
        [SerializeField] private float skimAcceleration = 10f;
        [SerializeField] private float skimDeceleration = 5f;
        
        [Header("Height Control")]
        [SerializeField] private float heightChangeSpeed = 5f;
        [SerializeField] private KeyCode raiseKey = KeyCode.E;
        [SerializeField] private KeyCode lowerKey = KeyCode.Q;
        
        [Header("Visual Settings")]
        [SerializeField] private ParticleSystem skimEffect;
        [SerializeField] private TrailRenderer skimTrail;
        [SerializeField] private AudioClip skimSound;
        
        [Header("Steering")]
        [SerializeField] private float turnSpeed = 50f;
        [SerializeField] private float maxTurnAngle = 45f;
        
        private BasicPlayerMove playerMove;
        private Rigidbody playerRb;
        private bool isSkimming = false;
        private float currentHeight = 2f;
        private float currentSpeed = 0f;
        private float targetHeight = 2f;
        private AudioSource audioSource;
        
        private void Awake()
        {
            playerMove = GetComponent<BasicPlayerMove>();
            playerRb = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (skimEffect != null)
            {
                skimEffect.Stop();
            }
            
            if (skimTrail != null)
            {
                skimTrail.enabled = false;
            }
        }
        
        private void Update()
        {
            HandleInput();
            
            if (isSkimming)
            {
                UpdateSkimming();
                UpdateHeight();
            }
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F) && !isSkimming)
            {
                StartSkimming();
            }
            else if (Input.GetKeyDown(KeyCode.F) && isSkimming)
            {
                StopSkimming();
            }
        }
        
        private void UpdateSkimming()
        {
            if (playerRb != null)
            {
                Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
                playerRb.MovePosition(playerRb.position + movement);
            }
            
            HandleSteering();
            UpdateSpeed();
        }
        
        private void HandleSteering()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            float yawRotation = horizontalInput * turnSpeed * Time.deltaTime;
            float pitchRotation = verticalInput * turnSpeed * Time.deltaTime;
            
            pitchRotation = Mathf.Clamp(pitchRotation, -maxTurnAngle, maxTurnAngle);
            
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.y += yawRotation;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, pitchRotation, Time.deltaTime * turnSpeed);
            
            transform.eulerAngles = currentEuler;
        }
        
        private void UpdateSpeed()
        {
            float targetSpeed = baseSkimSpeed;
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetSpeed *= 1.5f;
            }
            
            if (Input.GetKey(KeyCode.LeftControl))
            {
                targetSpeed *= 0.5f;
            }
            
            if (currentSpeed < targetSpeed)
            {
                currentSpeed += skimAcceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, targetSpeed);
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed -= skimDeceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, targetSpeed);
            }
        }
        
        private void UpdateHeight()
        {
            if (Input.GetKey(raiseKey))
            {
                targetHeight += heightChangeSpeed * Time.deltaTime;
            }
            
            if (Input.GetKey(lowerKey))
            {
                targetHeight -= heightChangeSpeed * Time.deltaTime;
            }
            
            targetHeight = Mathf.Clamp(targetHeight, 2f, maxSkimHeight);
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightChangeSpeed);
            
            if (playerRb != null)
            {
                Vector3 position = playerRb.position;
                position.y = currentHeight;
                playerRb.MovePosition(position);
            }
        }
        
        public void StartSkimming()
        {
            if (isSkimming)
            {
                return;
            }
            
            isSkimming = true;
            currentHeight = 2f;
            targetHeight = 2f;
            currentSpeed = 0f;
            
            if (playerMove != null)
            {
                playerMove.enabled = false;
            }
            
            if (playerRb != null)
            {
                playerRb.useGravity = false;
                playerRb.velocity = Vector3.zero;
            }
            
            if (skimEffect != null)
            {
                skimEffect.Play();
            }
            
            if (skimTrail != null)
            {
                skimTrail.enabled = true;
            }
            
            if (audioSource != null && skimSound != null)
            {
                audioSource.clip = skimSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            if (OnSkimmingStarted != null)
            {
                OnSkimmingStarted();
            }
        }
        
        public void StopSkimming()
        {
            if (!isSkimming)
            {
                return;
            }
            
            isSkimming = false;
            
            if (playerMove != null)
            {
                playerMove.enabled = true;
            }
            
            if (playerRb != null)
            {
                playerRb.useGravity = true;
            }
            
            if (skimEffect != null)
            {
                skimEffect.Stop();
            }
            
            if (skimTrail != null)
            {
                skimTrail.enabled = false;
            }
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            if (OnSkimmingStopped != null)
            {
                OnSkimmingStopped();
            }
        }
        
        public bool IsSkimming()
        {
            return isSkimming;
        }
        
        public float GetCurrentHeight()
        {
            return currentHeight;
        }
        
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        
        public void SetSkimSpeed(float speed)
        {
            baseSkimSpeed = speed;
        }
        
        public void SetMaxHeight(float height)
        {
            maxSkimHeight = height;
        }
        
        public event System.Action OnSkimmingStarted;
        public event System.Action OnSkimmingStopped;
    }
}
