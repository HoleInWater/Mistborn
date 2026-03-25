using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentalHazard : MonoBehaviour
{
    [Header("Hazard Type")]
    public HazardType hazardType = HazardType.Damage;
    public enum HazardType { Damage, Push, Pull, Slow, Stun, Trap }

    [Header("Settings")]
    public float damagePerSecond = 10f;
    public float damageOnEnter = 0f;
    public float pushForce = 10f;
    public float pullForce = 5f;
    public float slowMultiplier = 0.5f;
    public float stunDuration = 2f;

    [Header("Visuals")]
    public Color hazardColor = Color.red;
    public Color safeColor = Color.green;
    public float pulseSpeed = 2f;
    public GameObject particleEffect;

    [Header("Player Only")]
    public bool affectPlayerOnly = false;
    public bool affectEnemies = false;

    [Header("References")]
    private Renderer hazardRenderer;
    private List<GameObject> affectedObjects = new List<GameObject>();

    void Start()
    {
        hazardRenderer = GetComponent<Renderer>();
        UpdateHazardVisuals();
    }

    void Update()
    {
        if (hazardRenderer != null)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 0.5f) + 0.5f;
            hazardRenderer.material.color = Color.Lerp(hazardColor, hazardColor * 1.5f, pulse);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (ShouldAffect(other))
        {
            affectedObjects.Add(other.gameObject);

            if (damageOnEnter > 0)
            {
                ApplyDamage(other.gameObject, damageOnEnter);
            }

            ApplyEffect(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!affectedObjects.Contains(other.gameObject)) return;

        if (hazardType == HazardType.Damage)
        {
            ApplyDamage(other.gameObject, damagePerSecond * Time.deltaTime);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (affectedObjects.Contains(other.gameObject))
        {
            affectedObjects.Remove(other.gameObject);
            RemoveEffect(other.gameObject);
        }
    }

    bool ShouldAffect(Collider other)
    {
        if (affectPlayerOnly && !other.CompareTag("Player")) return false;
        if (affectEnemies && other.CompareTag("Enemy")) return true;
        return affectPlayerOnly || other.CompareTag("Player");
    }

    void ApplyDamage(GameObject target, float damage)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);
    }

    void ApplyEffect(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();

        switch (hazardType)
        {
            case HazardType.Push:
                if (rb != null)
                {
                    Vector3 pushDir = (target.transform.position - transform.position).normalized;
                    rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                }
                break;

            case HazardType.Pull:
                if (rb != null)
                {
                    Vector3 pullDir = (transform.position - target.transform.position).normalized;
                    rb.AddForce(pullDir * pullForce, ForceMode.Force);
                }
                break;

            case HazardType.Slow:
                BasicPlayerMove move = target.GetComponent<BasicPlayerMove>();
                if (move != null)
                {
                    move.externalSpeedMultiplier *= slowMultiplier;
                }
                break;

            case HazardType.Stun:
                StartCoroutine(StunTarget(target));
                break;

            case HazardType.Trap:
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                StartCoroutine(ReleaseTrap(target));
                break;
        }
    }

    void RemoveEffect(GameObject target)
    {
        BasicPlayerMove move = target.GetComponent<BasicPlayerMove>();
        if (move != null && hazardType == HazardType.Slow)
        {
            move.externalSpeedMultiplier /= slowMultiplier;
        }
    }

    IEnumerator StunTarget(GameObject target)
    {
        BasicPlayerMove move = target.GetComponent<BasicPlayerMove>();
        if (move != null)
        {
            float originalSpeed = move.moveSpeed;
            move.moveSpeed = 0;
            yield return new WaitForSeconds(stunDuration);
            move.moveSpeed = originalSpeed;
        }
    }

    IEnumerator ReleaseTrap(GameObject target)
    {
        yield return new WaitForSeconds(stunDuration);
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    void UpdateHazardVisuals()
    {
        if (hazardRenderer != null)
        {
            hazardRenderer.material.color = hazardColor;
        }
    }
}

public class AshFall : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerSecond = 2f;
    public float visibilityReduction = 0.3f;
    public float ashDensity = 0.5f;
    public LayerMask damageLayer;

    [Header("Visuals")]
    public ParticleSystem ashParticles;
    public Color ashColor = Color.gray;

    [Header("References")]
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        
        if (ashParticles == null)
        {
            ashParticles = GetComponent<ParticleSystem>();
        }
    }

    void Update()
    {
        if (ashParticles != null)
        {
            var emission = ashParticles.emission;
            emission.rateOverTime = ashDensity * 50f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & damageLayer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}

public class LavaPool : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerSecond = 30f;
    public float pushUpward = 5f;
    public float surfaceLevel = 0f;

    [Header("Visuals")]
    public Color lavaColor = new Color(1f, 0.3f, 0f);
    public float bubbleSpeed = 2f;
    public GameObject steamEffect;

    [Header("References")]
    private Renderer poolRenderer;

    void Start()
    {
        poolRenderer = GetComponent<Renderer>();
        if (poolRenderer != null)
        {
            poolRenderer.material.color = lavaColor;
            poolRenderer.material.EnableKeyword("_EMISSION");
            poolRenderer.material.SetColor("_EmissionColor", lavaColor * 2f);
        }
    }

    void Update()
    {
        if (poolRenderer != null)
        {
            float emission = Mathf.PingPong(Time.time * bubbleSpeed, 0.5f) + 1f;
            poolRenderer.material.SetColor("_EmissionColor", lavaColor * emission);
        }
    }

    void OnTriggerStay(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(damagePerSecond * Time.deltaTime);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * pushUpward, ForceMode.Force);
        }
    }
}

public class MistZone : MonoBehaviour
{
    [Header("Settings")]
    public float visibilityRange = 10f;
    public float allomancyBoost = 1.5f;
    public bool enhanceTin = true;
    public bool enhanceSteelePush = false;

    [Header("Visuals")]
    public Color mistColor = new Color(0.8f, 0.8f, 0.9f, 0.5f);
    public float density = 0.5f;
    public ParticleSystem mistParticles;

    [Header("References")]
    private Allomancer playerAllomancer;

    void Start()
    {
        if (mistParticles == null)
        {
            GameObject particles = new GameObject("MistParticles");
            particles.transform.SetParent(transform);
            mistParticles = particles.AddComponent<ParticleSystem>();
            
            var main = mistParticles.main;
            main.startLifetime = 5f;
            main.startSpeed = 0.5f;
            main.startColor = mistColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = mistParticles.emission;
            emission.rateOverTime = 20f;
            
            var shape = mistParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 5f, 20f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enhanceTin)
            {
                Tin tin = other.GetComponent<Tin>();
                if (tin != null)
                {
                    Debug.Log("[MIST] Tin enhanced in mist zone");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[MIST] Left mist zone");
        }
    }
}

public class SpikeTrap : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 25f;
    public float spikeForce = 15f;
    public float armTime = 0.5f;
    public float retractTime = 1f;

    [Header("Visuals")]
    public Transform spikeVisual;
    public Vector3 retractedPosition;
    public Vector3 extendedPosition;
    public GameObject extendEffect;

    private bool isActive = false;
    private bool isArmed = false;

    void Start()
    {
        if (spikeVisual != null)
        {
            retractedPosition = spikeVisual.localPosition;
            extendedPosition = retractedPosition + Vector3.up * 1f;
        }
    }

    void Update()
    {
        if (isActive && !isArmed)
        {
            StartCoroutine(ActivateSpikes());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isArmed)
        {
            isActive = true;
        }
    }

    IEnumerator ActivateSpikes()
    {
        isArmed = true;

        yield return new WaitForSeconds(armTime);

        if (spikeVisual != null)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 4f;
                spikeVisual.localPosition = Vector3.Lerp(retractedPosition, extendedPosition, t);
                yield return null;
            }
        }

        Collider[] hitObjects = Physics.OverlapBox(transform.position + Vector3.up, Vector3.one, Quaternion.identity);
        foreach (Collider c in hitObjects)
        {
            IDamageable damageable = c.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);

            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * spikeForce, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.2f);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            spikeVisual.localPosition = Vector3.Lerp(extendedPosition, retractedPosition, t);
            yield return null;
        }

        isActive = false;
        isArmed = false;
    }
}