using UnityEngine;

namespace MistbornGame.Environment
{
    public class Chandelier : MonoBehaviour
    {
        [Header("Chandelier Configuration")]
        [SerializeField] private int candleCount = 6;
        [SerializeField] private float swingForce = 5f;
        [SerializeField] private float maxSwingAngle = 45f;
        [SerializeField] private float swingDamping = 0.98f;
        
        [Header("Physics")]
        [SerializeField] private Rigidbody chandelierRigidbody;
        [SerializeField] private HingeJoint chainJoint;
        [SerializeField] private float chainLength = 5f;
        
        [Header("Candles")]
        [SerializeField] private GameObject candlePrefab;
        [SerializeField] private Transform candleContainer;
        
        [Header("Allomancy")]
        [SerializeField] private bool canBePushed = true;
        [SerializeField] private bool canBePulled = true;
        [SerializeField] private float pushForceMultiplier = 2f;
        
        [Header("Damage")]
        [SerializeField] private float fallDamage = 50f;
        [SerializeField] private float fallKnockback = 20f;
        [SerializeField] private float fallRadius = 3f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip swingSound;
        [SerializeField] private AudioClip crashSound;
        [SerializeField] private AudioClip candleLightSound;
        
        private bool hasFallen = false;
        private bool isSwinging = false;
        private Quaternion originalRotation;
        private float currentSwingAngle = 0f;
        
        private void Start()
        {
            originalRotation = transform.rotation;
            
            if (chandelierRigidbody == null)
                chandelierRigidbody = GetComponent<Rigidbody>();
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.centerOfMass = Vector3.down * 0.5f;
            }
            
            SetupChain();
            SpawnCandles();
        }
        
        private void SetupChain()
        {
            if (chainJoint != null)
            {
                JointSpring spring = chainJoint.spring;
                spring.damper = 5f;
                spring.spring = 100f;
                chainJoint.spring = spring;
            }
        }
        
        private void SpawnCandles()
        {
            if (candlePrefab == null || candleContainer == null)
                return;
            
            for (int i = 0; i < candleCount; i++)
            {
                float angle = i * (360f / candleCount);
                Vector3 position = Quaternion.Euler(0, angle, 0) * Vector3.forward * 0.5f;
                position.y = 0;
                
                GameObject candle = Instantiate(candlePrefab, candleContainer);
                candle.transform.localPosition = position;
                candle.transform.localRotation = Quaternion.identity;
                
                CandleHolder candleHolder = candle.AddComponent<CandleHolder>();
            }
        }
        
        private void Update()
        {
            if (hasFallen)
                return;
            
            if (chandelierRigidbody != null && !chandelierRigidbody.isKinematic)
            {
                currentSwingAngle = chandelierRigidbody.transform.eulerAngles.z;
                
                if (Mathf.Abs(currentSwingAngle) > 1f)
                {
                    if (!isSwinging)
                    {
                        isSwinging = true;
                        AudioSource.PlayClipAtPoint(swingSound, transform.position);
                    }
                }
                else
                {
                    isSwinging = false;
                }
                
                if (currentSwingAngle > maxSwingAngle || Mathf.Abs(chandelierRigidbody.velocity.y) > 10f)
                {
                    TriggerFall();
                }
            }
        }
        
        public void ApplyPushForce(Vector3 direction)
        {
            if (!canBePushed || hasFallen)
                return;
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.isKinematic = false;
                
                Vector3 pushDir = direction;
                pushDir.y = 0.5f;
                pushDir.Normalize();
                
                chandelierRigidbody.AddForce(pushDir * swingForce * pushForceMultiplier, ForceMode.Impulse);
            }
        }
        
        public void ApplyPullForce(Vector3 sourcePosition)
        {
            if (!canBePulled || hasFallen)
                return;
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.isKinematic = false;
                
                Vector3 pullDir = (transform.position - sourcePosition).normalized;
                pullDir.y = 0.3f;
                
                chandelierRigidbody.AddForce(pullDir * pullForce * pushForceMultiplier, ForceMode.Impulse);
            }
        }
        
        public void ApplySwingForce(Vector3 direction)
        {
            if (hasFallen)
                return;
            
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.AddTorque(perpendicular * swingForce, ForceMode.Impulse);
            }
        }
        
        private void TriggerFall()
        {
            hasFallen = true;
            
            AudioSource.PlayClipAtPoint(crashSound, transform.position);
            
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fallRadius);
            foreach (var obj in nearbyObjects)
            {
                if (obj.CompareTag("Player"))
                {
                    PlayerStats player = obj.GetComponent<PlayerStats>();
                    if (player != null)
                    {
                        player.TakeDamage(fallDamage, DamageType.Heavy);
                    }
                    
                    Vector3 knockbackDir = (obj.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.5f;
                    obj.GetComponent<Rigidbody>()?.AddForce(knockbackDir * fallKnockback * 100f, ForceMode.Impulse);
                }
                
                if (obj.GetComponent<CandleHolder>() != null)
                {
                    obj.SendMessage("ExtinguishCandle");
                }
            }
            
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.3f, 0.5f);
            
            StartCoroutine(FallenState());
        }
        
        private IEnumerator FallenState()
        {
            yield return new WaitForSeconds(3f);
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.isKinematic = true;
            }
        }
        
        public void ResetChandelier()
        {
            hasFallen = false;
            isSwinging = false;
            
            transform.rotation = originalRotation;
            
            if (chandelierRigidbody != null)
            {
                chandelierRigidbody.isKinematic = true;
                chandelierRigidbody.velocity = Vector3.zero;
                chandelierRigidbody.angularVelocity = Vector3.zero;
            }
            
            CandleHolder[] candles = GetComponentsInChildren<CandleHolder>();
            foreach (var candle in candles)
            {
                candle.LightCandle();
            }
        }
    }
}
