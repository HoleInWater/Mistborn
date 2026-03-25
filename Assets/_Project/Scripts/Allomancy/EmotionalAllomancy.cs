using UnityEngine;
using System.Collections.Generic;

public class EmotionalAllomancy : MonoBehaviour
{
    [Header("Zinc - Riot (Intensify Emotions)")]
    public float riotRange = 20f;
    public float riotIntensity = 1.5f;
    public float riotMetalCostPerSecond = 2f;

    [Header("Brass - Soothe (Calm Emotions)")]
    public float sootheRange = 20f;
    public float sootheIntensity = 0.5f;
    public float sootheMetalCostPerSecond = 2f;

    [Header("References")]
    public Allomancer allomancer;
    public LayerMask targetLayer;

    private bool isZincBurning = false;
    private bool isBrassBurning = false;
    private List<EmotionalTarget> affectedTargets = new List<EmotionalTarget>();

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
        targetLayer = LayerMask.GetMask("Character");
    }

    void Update()
    {
        bool wasZinc = isZincBurning;
        bool wasBrass = isBrassBurning;

        isZincBurning = allomancer != null && allomancer.IsBurning() && 
                       allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Zinc;
        isBrassBurning = allomancer != null && allomancer.IsBurning() && 
                         allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Brass;

        if (isZincBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            RiotEmotions(flareMult);
            DrainMetal(AllomancySkill.MetalType.Zinc, riotMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasZinc)
        {
            ClearEmotionalEffects();
        }

        if (isBrassBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            SootheEmotions(flareMult);
            DrainMetal(AllomancySkill.MetalType.Brass, sootheMetalCostPerSecond * flareMult * Time.deltaTime);
        }
        else if (wasBrass)
        {
            ClearEmotionalEffects();
        }
    }

    void RiotEmotions(float flareMult)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, riotRange, targetLayer);
        
        foreach (Collider c in targets)
        {
            EmotionalTarget emotional = c.GetComponent<EmotionalTarget>();
            if (emotional != null && emotional != GetComponent<EmotionalTarget>())
            {
                emotional.ApplyRiotEffect(riotIntensity * flareMult);
            }
        }
    }

    void SootheEmotions(float flareMult)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, sootheRange, targetLayer);
        
        foreach (Collider c in targets)
        {
            EmotionalTarget emotional = c.GetComponent<EmotionalTarget>();
            if (emotional != null && emotional != GetComponent<EmotionalTarget>())
            {
                emotional.ApplySootheEffect(sootheIntensity * flareMult);
            }
        }
    }

    void ClearEmotionalEffects()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, riotRange + sootheRange, targetLayer);
        
        foreach (Collider c in targets)
        {
            EmotionalTarget emotional = c.GetComponent<EmotionalTarget>();
            if (emotional != null)
            {
                emotional.ResetEmotions();
            }
        }
    }

    void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(metal, amount);
    }
}

public class EmotionalTarget : MonoBehaviour
{
    public enum EmotionType { Fear, Anger, Joy, Sadness, Calm, Rage }

    [Header("Current Emotions")]
    public float fear = 0.3f;
    public float anger = 0.3f;
    public float joy = 0.5f;
    public float sadness = 0.2f;

    private float[] emotionLevels;
    private Dictionary<EmotionType, float> activeEffects = new Dictionary<EmotionType, float>();

    void Start()
    {
        emotionLevels = new float[] { fear, anger, joy, sadness };
    }

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var effect in activeEffects)
        {
            emotionLevels[(int)effect.Key] = Mathf.MoveTowards(
                emotionLevels[(int)effect.Key], 
                effect.Value, 
                dt * 0.5f
            );
        }

        fear = emotionLevels[0];
        anger = emotionLevels[1];
        joy = emotionLevels[2];
        sadness = emotionLevels[3];
    }

    public void ApplyRiotEffect(float intensity)
    {
        for (int i = 0; i < 4; i++)
        {
            activeEffects[(EmotionType)i] = Mathf.Clamp01(emotionLevels[i] + intensity * 0.3f);
        }
        
        if (GetComponent<AICombat>())
        {
            GetComponent<AICombat>().SetAggressionMultiplier(intensity);
        }
    }

    public void ApplySootheEffect(float intensity)
    {
        for (int i = 0; i < 4; i++)
        {
            activeEffects[(EmotionType)i] = Mathf.Clamp01(emotionLevels[i] - intensity * 0.5f);
        }

        if (GetComponent<AICombat>())
        {
            GetComponent<AICombat>().SetAggressionMultiplier(1f - intensity);
        }
    }

    public void ResetEmotions()
    {
        activeEffects.Clear();
    }

    public float GetOverallEmotion()
    {
        return (fear + anger + joy - sadness) / 2f;
    }
}