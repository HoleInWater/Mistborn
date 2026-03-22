using UnityEngine;

namespace MistbornGameplay
{
    public class PushableObject : MonoBehaviour
    {
        [Header("Pushable Settings")]
        [SerializeField] private float mass = 10f;
        [SerializeField] private float friction = 0.5f;
        [SerializeField] private bool affectedByAllomancy = true;
        [SerializeField] private bool affectedByFeruchemy = true;
        
        [Header("Metal Properties")]
        [SerializeField] private MetalType metalType = MetalType.Iron;
        [SerializeField] private bool isMetallic = true;
        [SerializeField] private float metalPurity = 1f;
        
        [Header("Push/Pull Properties")]
        [SerializeField] private bool canBePulled = true;
        [SerializeField] private bool canBePushed = true;
        [SerializeField] private float pullForceMultiplier = 1f;
        [SerializeField] private float pushForceMultiplier = 1f;
        
        [Header("Visual")]
        [SerializeField] private Renderer objectRenderer;
        [SerializeField] private Color metalHighlightColor = new Color(1f, 0.8f, 0f, 1f);
        [SerializeField] private ParticleSystem metalParticleEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pushSound;
        [SerializeField] private AudioClip pullSound;
        [SerializeField] private AudioClip impactSound;
        
        private Rigidbody objectRb;
        private AudioSource audioSource;
        private bool isBeingAffected = false;
        private Vector3 lastVelocity;
        
        public enum MetalType
        {
            Iron,
            Steel,
            Tin,
            Pewter,
            Copper,
            Bronze,
            Zinc,
            Brass,
            Chromium,
            Nickel
        }
        
        private void Awake()
        {
            objectRb = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            
            if (objectRb == null)
            {
                objectRb = gameObject.AddComponent<Rigidbody>();
            }
            
            objectRb.mass = mass;
            objectRb.drag = friction;
            objectRb.angularDrag = friction;
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (metalParticleEffect != null)
            {
                metalParticleEffect.Stop();
            }
        }
        
        private void Update()
        {
            lastVelocity = objectRb.velocity;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (audioSource != null && impactSound != null)
            {
                float impactForce = collision.relativeVelocity.magnitude;
                if (impactForce > 1f)
                {
                    audioSource.PlayOneShot(impactSound, Mathf.Clamp01(impactForce / 10f));
                }
            }
        }
        
        public void ApplyPushForce(Vector3 direction, float force)
        {
            if (!canBePushed || !affectedByAllomancy)
            {
                return;
            }
            
            isBeingAffected = true;
            
            float finalForce = force * pushForceMultiplier * metalPurity;
            objectRb.AddForce(direction * finalForce, ForceMode.Impulse);
            
            if (audioSource != null && pushSound != null)
            {
                audioSource.PlayOneShot(pushSound);
            }
            
            if (metalParticleEffect != null)
            {
                metalParticleEffect.Play();
            }
            
            Invoke(nameof(ResetBeingAffected), 0.5f);
        }
        
        public void ApplyPullForce(Vector3 direction, float force)
        {
            if (!canBePulled || !affectedByAllomancy)
            {
                return;
            }
            
            isBeingAffected = true;
            
            float finalForce = force * pullForceMultiplier * metalPurity;
            objectRb.AddForce(direction * finalForce, ForceMode.Impulse);
            
            if (audioSource != null && pullSound != null)
            {
                audioSource.PlayOneShot(pullSound);
            }
            
            if (metalParticleEffect != null)
            {
                metalParticleEffect.Play();
            }
            
            Invoke(nameof(ResetBeingAffected), 0.5f);
        }
        
        private void ResetBeingAffected()
        {
            isBeingAffected = false;
        }
        
        public void HighlightAsMetal()
        {
            if (objectRenderer != null)
            {
                foreach (Material mat in objectRenderer.materials)
                {
                    mat.SetColor("_OutlineColor", metalHighlightColor);
                    mat.SetFloat("_OutlineWidth", 0.05f);
                }
            }
        }
        
        public void RemoveHighlight()
        {
            if (objectRenderer != null)
            {
                foreach (Material mat in objectRenderer.materials)
                {
                    mat.SetFloat("_OutlineWidth", 0f);
                }
            }
        }
        
        public bool IsBeingAffected()
        {
            return isBeingAffected;
        }
        
        public bool IsMetallic()
        {
            return isMetallic;
        }
        
        public MetalType GetMetalType()
        {
            return metalType;
        }
        
        public void SetMetalType(MetalType type)
        {
            metalType = type;
        }
        
        public float GetMetalPurity()
        {
            return metalPurity;
        }
        
        public void SetMetalPurity(float purity)
        {
            metalPurity = Mathf.Clamp01(purity);
        }
        
        public void SetMass(float newMass)
        {
            mass = newMass;
            objectRb.mass = mass;
        }
        
        public void SetFriction(float newFriction)
        {
            friction = newFriction;
            objectRb.drag = friction;
            objectRb.angularDrag = friction;
        }
        
        public Vector3 GetVelocity()
        {
            return objectRb.velocity;
        }
        
        public Vector3 GetLastVelocity()
        {
            return lastVelocity;
        }
        
        public Rigidbody GetRigidbody()
        {
            return objectRb;
        }
    }
}
