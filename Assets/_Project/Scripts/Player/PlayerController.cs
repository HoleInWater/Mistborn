using UnityEngine;

namespace MistbornGameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private BasicPlayerMove playerMove;
        [SerializeField] private Allomancer allomancer;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private IronPull ironPull;
        [SerializeField] private SteelPush steelPush;
        [SerializeField] private EnhancedMovement enhancedMovement;
        
        [Header("Input Settings")]
        [SerializeField] private KeyCode ironPullKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode steelPushKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode tinEyesKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode pewterKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode flareKey = KeyCode.F;
        
        [Header("Combat Settings")]
        [SerializeField] private float lockOnRange = 20f;
        [SerializeField] private LayerMask targetableLayers;
        
        private Transform currentTarget;
        private bool isLockOnActive = false;
        private Camera mainCamera;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }
        
        private void Start()
        {
            if (playerMove == null)
            {
                playerMove = GetComponent<BasicPlayerMove>();
            }
            
            if (allomancer == null)
            {
                allomancer = GetComponent<Allomancer>();
            }
            
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }
        }
        
        private void Update()
        {
            HandleInput();
            HandleTargeting();
            UpdateAllomancy();
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(ironPullKey))
            {
                ToggleIronPull();
            }
            
            if (Input.GetKeyDown(steelPushKey))
            {
                ToggleSteelPush();
            }
            
            if (Input.GetKeyDown(tinEyesKey))
            {
                ToggleTinEyes();
            }
            
            if (Input.GetKeyDown(pewterKey))
            {
                TogglePewter();
            }
            
            if (Input.GetKeyDown(flareKey))
            {
                ToggleFlare();
            }
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleLockOn();
            }
        }
        
        private void HandleTargeting()
        {
            if (isLockOnActive && currentTarget != null)
            {
                Vector3 targetDirection = currentTarget.position - transform.position;
                targetDirection.y = 0f;
                
                if (targetDirection.magnitude > lockOnRange)
                {
                    ReleaseLockOn();
                    return;
                }
                
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        
        private void UpdateAllomancy()
        {
            if (allomancer != null)
            {
                allomancer.UpdatePowers();
            }
        }
        
        public void ToggleIronPull()
        {
            if (ironPull != null)
            {
                if (ironPull.IsPulling())
                {
                    ironPull.StopPull();
                }
                else
                {
                    ironPull.StartPull();
                }
            }
        }
        
        public void ToggleSteelPush()
        {
            if (steelPush != null)
            {
                if (steelPush.IsPushing())
                {
                    steelPush.StopPush();
                }
                else
                {
                    steelPush.StartPush();
                }
            }
        }
        
        public void ToggleTinEyes()
        {
            TinEyes tinEyes = GetComponent<TinEyes>();
            if (tinEyes != null)
            {
                if (tinEyes.IsActive())
                {
                    tinEyes.Deactivate();
                }
                else
                {
                    tinEyes.Activate();
                }
            }
        }
        
        public void TogglePewter()
        {
            if (enhancedMovement != null)
            {
                if (enhancedMovement.IsEnhanced())
                {
                    enhancedMovement.DeactivateEnhancement();
                }
                else
                {
                    enhancedMovement.ActivateEnhancement();
                }
            }
        }
        
        public void ToggleFlare()
        {
            AllomanticFlare flare = GetComponent<AllomanticFlare>();
            if (flare != null)
            {
                if (flare.IsFlaring())
                {
                    flare.StopFlare();
                }
                else
                {
                    flare.StartFlare();
                }
            }
        }
        
        public void ToggleLockOn()
        {
            if (isLockOnActive)
            {
                ReleaseLockOn();
            }
            else
            {
                AcquireLockOn();
            }
        }
        
        private void AcquireLockOn()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, lockOnRange, targetableLayers);
            
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider target in targets)
            {
                if (target.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target.transform;
                    }
                }
            }
            
            if (closestTarget != null)
            {
                currentTarget = closestTarget;
                isLockOnActive = true;
                
                if (OnLockOnAcquired != null)
                {
                    OnLockOnAcquired(currentTarget);
                }
            }
        }
        
        private void ReleaseLockOn()
        {
            if (isLockOnActive && OnLockOnReleased != null)
            {
                OnLockOnReleased(currentTarget);
            }
            
            currentTarget = null;
            isLockOnActive = false;
        }
        
        public Transform GetCurrentTarget()
        {
            return currentTarget;
        }
        
        public bool IsLockOnActive()
        {
            return isLockOnActive;
        }
        
        public event System.Action<Transform> OnLockOnAcquired;
        public event System.Action<Transform> OnLockOnReleased;
    }
}
