using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("Ragdoll Parts")]
    public Transform hips;
    public Transform spine;
    public Transform chest;
    public Transform head;
    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;

    [Header("Settings")]
    public float enableDelay = 0.5f;
    public float disableDelay = 2f;
    public float fallDamageThreshold = 8f;
    public float maxFallDamage = 50f;
    public float impulseMultiplier = 1f;

    [Header("References")]
    public Rigidbody mainRb;
    public Animator animator;

    private List<BodyPart> bodyParts = new List<BodyPart>();
    private bool isRagdollActive = false;
    private bool isTransitioning = false;

    private class BodyPart
    {
        public Transform transform;
        public Rigidbody rb;
        public Joint joint;
        public Collider col;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
    }

    void Start()
    {
        InitializeBodyParts();
    }

    void InitializeBodyParts()
    {
        AddBodyPart(hips, "Hips");
        AddBodyPart(spine, "Spine");
        AddBodyPart(chest, "Chest");
        AddBodyPart(head, "Head");
        AddBodyPart(leftUpperArm, "LeftUpperArm");
        AddBodyPart(leftLowerArm, "LeftLowerArm");
        AddBodyPart(rightUpperArm, "RightUpperArm");
        AddBodyPart(rightLowerArm, "RightLowerArm");
        AddBodyPart(leftUpperLeg, "LeftUpperLeg");
        AddBodyPart(leftLowerLeg, "LeftLowerLeg");
        AddBodyPart(rightUpperLeg, "RightUpperLeg");
        AddBodyPart(rightLowerLeg, "RightLowerLeg");

        foreach (var part in bodyParts)
        {
            part.initialPosition = part.transform.localPosition;
            part.initialRotation = part.transform.localRotation;
        }
    }

    void AddBodyPart(Transform t, string name)
    {
        if (t == null) return;

        BodyPart bp = new BodyPart { transform = t };
        bp.rb = t.GetComponent<Rigidbody>();
        bp.joint = t.GetComponent<Joint>();
        bp.col = t.GetComponent<Collider>();

        if (bp.rb == null)
        {
            bp.rb = t.gameObject.AddComponent<Rigidbody>();
            bp.rb.mass = 1f;
            bp.rb.drag = 0.5f;
            bp.rb.angularDrag = 0.5f;
        }

        if (bp.col == null)
        {
            bp.col = t.gameObject.AddComponent<CapsuleCollider>();
            ((CapsuleCollider)bp.col).radius = 0.1f;
            ((CapsuleCollider)bp.col).height = 0.3f;
        }

        bp.rb.isKinematic = true;
        bp.rb.detectCollisions = false;
        bp.col.enabled = false;

        bodyParts.Add(bp);
    }

    public void EnableRagdoll(Vector3 impactDirection, float force)
    {
        if (isRagdollActive || isTransitioning) return;
        StartCoroutine(EnableRagdollSequence(impactDirection, force));
    }

    IEnumerator EnableRagdollSequence(Vector3 impactDirection, float force)
    {
        isTransitioning = true;

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (mainRb != null)
        {
            mainRb.isKinematic = true;
            mainRb.detectCollisions = false;
        }

        foreach (var part in bodyParts)
        {
            part.rb.isKinematic = false;
            part.rb.detectCollisions = true;
            part.col.enabled = true;

            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            Vector3 finalImpulse = (impactDirection + randomOffset).normalized * force * impulseMultiplier;
            part.rb.AddForce(finalImpulse, ForceMode.Impulse);
        }

        isRagdollActive = true;

        yield return new WaitForSeconds(enableDelay);

        ApplyFallDamageCheck();

        yield return new WaitForSeconds(disableDelay);

        DisableRagdoll();
    }

    void ApplyFallDamageCheck()
    {
        if (mainRb != null)
        {
            float velocity = mainRb.linearVelocity.magnitude;
            if (velocity > fallDamageThreshold)
            {
                float damage = Mathf.Lerp(0, maxFallDamage, (velocity - fallDamageThreshold) / 10f);
                Debug.Log($"[RAGDOLL] Fall damage: {damage}");
            }
        }
    }

    public void DisableRagdoll()
    {
        if (!isRagdollActive) return;

        foreach (var part in bodyParts)
        {
            part.rb.isKinematic = true;
            part.rb.detectCollisions = false;
            part.col.enabled = false;

            part.transform.localPosition = part.initialPosition;
            part.transform.localRotation = part.initialRotation;

            part.rb.linearVelocity = Vector3.zero;
            part.rb.angularVelocity = Vector3.zero;
        }

        if (mainRb != null)
        {
            mainRb.isKinematic = false;
            mainRb.detectCollisions = true;
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
        }

        isRagdollActive = false;
        isTransitioning = false;
    }

    public void ApplyImpulseToPart(string partName, Vector3 direction, float force)
    {
        foreach (var part in bodyParts)
        {
            if (part.transform.name.Contains(partName))
            {
                part.rb.AddForce(direction * force, ForceMode.Impulse);
                break;
            }
        }
    }

    public bool IsRagdollActive() => isRagdollActive;

    public void SyncToPose(Transform targetPose)
    {
        if (targetPose == null || !isRagdollActive) return;

        foreach (var part in bodyParts)
        {
            Transform targetBone = FindMatchingBone(targetPose, part.transform.name);
            if (targetBone != null)
            {
                part.transform.position = targetBone.position;
                part.transform.rotation = targetBone.rotation;
            }
        }
    }

    Transform FindMatchingBone(Transform root, string boneName)
    {
        if (root.name.Contains(boneName)) return root;
        
        foreach (Transform child in root)
        {
            Transform found = FindMatchingBone(child, boneName);
            if (found != null) return found;
        }
        return null;
    }

    public void OnDeath()
    {
        Vector3 randomDir = Random.onUnitSphere * 10f;
        EnableRagdoll(randomDir, 15f);
    }
}

public class ImpactReaction : MonoBehaviour
{
    [Header("Impact Settings")]
    public float minImpactForce = 5f;
    public float reactionStrength = 1f;
    public float stumbleDuration = 0.5f;

    [Header("Animation")]
    public Animator animator;
    public string impactTrigger = "Impact";

    private bool isReacting = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnImpact(Vector3 impactDirection, float force)
    {
        if (isReacting || force < minImpactForce) return;

        StartCoroutine(ImpactReactionSequence(impactDirection, force));
    }

    IEnumerator ImpactReactionSequence(Vector3 impactDirection, float force)
    {
        isReacting = true;

        if (animator != null)
        {
            animator.SetTrigger(impactTrigger);
        }

        Vector3 stumbleDir = -impactDirection.normalized * reactionStrength * (force / 10f);
        
        if (rb != null)
        {
            rb.linearVelocity = stumbleDir;
        }

        yield return new WaitForSeconds(stumbleDuration);

        isReacting = false;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * force;
        }
    }
}

public class ObjectPhysicsInteraction : MonoBehaviour
{
    [Header("Collision Settings")]
    public float pushPower = 10f;
    public float bounceFactor = 0.5f;
    public float friction = 0.8f;
    public LayerMask pushableLayers;

    [Header("Player Reference")]
    public Rigidbody playerRb;
    public float playerMass = 80f;

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & pushableLayers) == 0) return;

        Rigidbody objRb = collision.rigidbody;
        if (objRb == null) return;

        float objMass = objRb.mass;
        float massRatio = playerMass / objMass;

        Vector3 pushDir = collision.contacts[0].normal;
        float force = pushPower * massRatio;

        if (objMass > playerMass * 3)
        {
            Vector3 reactionForce = -pushDir * force * 0.5f;
            if (playerRb != null)
            {
                playerRb.linearVelocity += reactionForce;
            }
        }
        else
        {
            objRb.linearVelocity += pushDir * force;
        }

        ImpactReaction impact = GetComponent<ImpactReaction>();
        if (impact != null && massRatio < 0.5f)
        {
            impact.OnImpact(pushDir, force);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & pushableLayers) == 0) return;

        Rigidbody objRb = collision.rigidbody;
        if (objRb == null) return;

        Vector3 pushDir = collision.contacts[0].normal;
        float force = pushPower * Time.fixedDeltaTime * friction;

        if (objRb.mass > playerMass * 3)
        {
            if (playerRb != null)
            {
                playerRb.linearVelocity -= pushDir * force * 0.3f;
            }
        }
        else
        {
            objRb.linearVelocity += pushDir * force;
        }
    }
}