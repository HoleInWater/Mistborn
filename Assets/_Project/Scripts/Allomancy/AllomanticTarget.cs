using UnityEngine;

namespace Mistborn.Allomancy
{
    [RequireComponent(typeof(Rigidbody))]
    public class AllomanticTarget : MonoBehaviour
    {
        [Header("Metal Properties")]
        [SerializeField] private AllomanticMetal metalType = AllomanticMetal.Steel;
        [SerializeField] private float metalMass = 1f;
        [SerializeField] private bool isAnchored;

        public AllomanticMetal MetalType => metalType;
        public float MetalMass => metalMass;
        public bool IsAnchored => isAnchored;
        public Rigidbody Rigidbody { get; private set; }

        public void SetMetalMass(float mass) => metalMass = Mathf.Max(0.01f, mass);
        public void SetAnchored(bool anchored) => isAnchored = anchored;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (metalMass <= 0)
                metalMass = Rigidbody.mass;
        }

        private void OnValidate()
        {
            metalMass = Mathf.Max(0.01f, metalMass);
        }

        public Vector3 GetPushDirection(Vector3 fromPosition)
        {
            return (transform.position - fromPosition).normalized;
        }

        public Vector3 GetPullDirection(Vector3 toPosition)
        {
            return (toPosition - transform.position).normalized;
        }
    }
}
