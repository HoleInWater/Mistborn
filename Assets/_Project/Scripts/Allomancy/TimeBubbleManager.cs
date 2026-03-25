using UnityEngine;
using System.Collections.Generic;

public class TimeBubbleManager : MonoBehaviour
{
    [Header("Bendalloy - Speed Bubble")]
    public float speedBubbleRadius = 10f;
    public float speedBubbleMultiplier = 2f;
    public float bendalloyMetalCostPerSecond = 3f;

    [Header("Cadmium - Slow Bubble")]
    public float slowBubbleRadius = 10f;
    public float slowBubbleMultiplier = 0.5f;
    public float cadmiumMetalCostPerSecond = 3f;

    [Header("References")]
    public Allomancer allomancer;
    public GameObject bubblePrefab;

    private bool isBendalloyBurning = false;
    private bool isCadmiumBurning = false;
    private GameObject currentBubble;

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBendalloy = isBendalloyBurning;
        bool wasCadmium = isCadmiumBurning;

        isBendalloyBurning = allomancer != null && allomancer.IsBurning() && 
                             allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Bendalloy;
        isCadmiumBurning = allomancer != null && allomancer.IsBurning() && 
                           allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Cadmium;

        if (isBendalloyBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            CreateOrUpdateBubble(speedBubbleRadius * flareMult, speedBubbleMultiplier);
            DrainMetal(AllomancySkill.MetalType.Bendalloy, bendalloyMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasBendalloy)
        {
            RemoveBubble();
        }

        if (isCadmiumBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            CreateOrUpdateBubble(slowBubbleRadius * flareMult, slowBubbleMultiplier);
            DrainMetal(AllomancySkill.MetalType.Cadmium, cadmiumMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasCadmium && !isBendalloyBurning)
        {
            RemoveBubble();
        }
    }

    void CreateOrUpdateBubble(float radius, float multiplier)
    {
        if (currentBubble == null)
        {
            currentBubble = new GameObject("TimeBubble");
            currentBubble.transform.SetParent(transform);
            currentBubble.transform.localPosition = Vector3.zero;

            SphereCollider sc = currentBubble.AddComponent<SphereCollider>();
            sc.radius = radius;
            sc.isTrigger = true;

            currentBubble.AddComponent<TimeBubbleEffect>().Initialize(multiplier);
        }
        else
        {
            SphereCollider sc = currentBubble.GetComponent<SphereCollider>();
            if (sc != null) sc.radius = radius;
        }
    }

    void RemoveBubble()
    {
        if (currentBubble != null)
        {
            TimeBubbleEffect effect = currentBubble.GetComponent<TimeBubbleEffect>();
            if (effect != null) effect.Shutdown();
            Destroy(currentBubble, 0.5f);
            currentBubble = null;
        }
    }

    void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(metal, amount);
    }
}

public class TimeBubbleEffect : MonoBehaviour
{
    private float timeScaleMultiplier = 1f;
    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();
    private List<Animator> affectedAnimators = new List<Animator>();

    public void Initialize(float multiplier)
    {
        timeScaleMultiplier = multiplier;
    }

    void Update()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null && r.material != null)
        {
            Color c = r.material.color;
            r.material.color = new Color(c.r, c.g, c.b, 0.2f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
            rb.linearVelocity *= timeScaleMultiplier;
        }

        Animator anim = other.GetComponent<Animator>();
        if (anim != null && !affectedAnimators.Contains(anim))
        {
            affectedAnimators.Add(anim);
            anim.speed = timeScaleMultiplier;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            rb.linearVelocity /= timeScaleMultiplier;
            affectedRigidbodies.Remove(rb);
        }

        Animator anim = other.GetComponent<Animator>();
        if (anim != null && affectedAnimators.Contains(anim))
        {
            anim.speed = 1f;
            affectedAnimators.Remove(anim);
        }
    }

    public void Shutdown()
    {
        foreach (Rigidbody rb in affectedRigidbodies)
        {
            if (rb != null) rb.linearVelocity /= timeScaleMultiplier;
        }

        foreach (Animator anim in affectedAnimators)
        {
            if (anim != null) anim.speed = 1f;
        }

        Destroy(gameObject, 0.5f);
    }
}