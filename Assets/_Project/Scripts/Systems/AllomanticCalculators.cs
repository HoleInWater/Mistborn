using UnityEngine;
using System.Collections.Generic;

public class AllomanticForceCalculator
{
    public static float CalculatePushForce(float allomancerStrength, float anchorMass, float anchorQuality, float distance)
    {
        float baseForce = allomancerStrength * anchorMass;
        float distanceFactor = 1f / (distance * distance);
        float qualityFactor = Mathf.Clamp01(anchorQuality);
        
        float force = baseForce * distanceFactor * qualityFactor;
        
        return Mathf.Clamp(force, 0f, allomancerStrength * 100f);
    }
    
    public static float CalculatePushStrength(float allomancerMass, float allomancerStrength)
    {
        return allomancerMass * allomancerStrength;
    }
    
    public static Vector3 CalculatePushDirection(Vector3 from, Vector3 to, bool isPushing)
    {
        Vector3 direction = (to - from).normalized;
        return isPushing ? direction : -direction;
    }
    
    public static float CalculateAnchorQuality(float anchorMass, float minimumMass = 0.1f)
    {
        return Mathf.Clamp01(anchorMass / minimumMass);
    }
    
    public static float CalculatePushVelocity(float force, float objectMass)
    {
        return force / objectMass;
    }
    
    public static float CalculateAllomancerRecoil(float force, float allomancerMass)
    {
        return force / allomancerMass;
    }
    
    public static bool ShouldAllomancerMove(float force, float allomancerMass, float frictionCoefficient, float gravity)
    {
        float frictionForce = frictionCoefficient * gravity * allomancerMass;
        return force > frictionForce;
    }
    
    public static Vector3 CalculateEquilibriumForce(float weight, float pushStrength, float distance, float equilibriumDistance)
    {
        if (distance <= equilibriumDistance)
        {
            float ratio = distance / equilibriumDistance;
            return Vector3.up * (weight - pushStrength * ratio);
        }
        return Vector3.zero;
    }
}

public class FeruchemyCalculator
{
    public static float CalculateStoredValue(float baseValue, float storageDuration, float recoveryRate)
    {
        return baseValue * storageDuration * recoveryRate;
    }
    
    public static float CalculateTappedValue(float storedValue, float tapRate)
    {
        return storedValue * tapRate;
    }
    
    public static float CalculateCompoundedValue(float storedValue, int cycles, float amplificationFactor = 10f)
    {
        return storedValue * Mathf.Pow(amplificationFactor, cycles);
    }
    
    public static float CalculateDiminishingReturns(float value, float maxValue, float curveFactor = 0.5f)
    {
        float normalized = value / maxValue;
        return maxValue * (1f - Mathf.Pow(1f - normalized, 1f / curveFactor));
    }
    
    public static float CalculateInertialMassEffect(float originalMass, float feruchemyMultiplier, bool gravitationalUnchanged = true)
    {
        if (gravitationalUnchanged)
        {
            float inertialMass = originalMass * feruchemyMultiplier;
            float gravitationalMass = originalMass;
            
            float terminalVelocityRatio = gravitationalMass / inertialMass;
            float fallTime = 1f / terminalVelocityRatio;
            
            return inertialMass;
        }
        return originalMass * feruchemyMultiplier;
    }
}

public class PewterCalculator
{
    public static float CalculateEnhancedStrength(float baseStrength, float pewterLevel, bool isFlared)
    {
        float multiplier = isFlared ? 1.5f : 1f;
        return baseStrength * pewterLevel * multiplier;
    }
    
    public static float CalculatePewterDrag(float burnDuration, float threshold, float maxDuration)
    {
        if (burnDuration < threshold) return 1f;
        
        float dragAmount = (burnDuration - threshold) / maxDuration;
        return Mathf.Clamp01(1f - dragAmount);
    }
    
    public static float CalculateEffectiveDamage(float baseDamage, float durabilityMultiplier, float painToleranceBonus)
    {
        float reducedDamage = baseDamage / durabilityMultiplier;
        float toleranceReduction = 1f - (painToleranceBonus / 100f);
        
        return reducedDamage * toleranceReduction;
    }
    
    public static float CalculateSpeedEnhancement(float baseSpeed, float pewterLevel, bool isFlared)
    {
        float multiplier = isFlared ? 1.5f : 1f;
        float speedBonus = (pewterLevel - 1f) * 0.2f;
        
        return baseSpeed * (1f + speedBonus) * multiplier;
    }
}

public class CompoundingCalculator
{
    public static float CalculateCompoundingGrowth(float baseValue, int cycles, float baseMultiplier = 10f)
    {
        float total = baseValue;
        for (int i = 0; i < cycles; i++)
        {
            total *= baseMultiplier;
        }
        return total;
    }
    
    public static int CalculateMaxSafeCycles(float initialMass, float fatalMassThreshold = 70000f)
    {
        int cycles = 0;
        float currentMass = initialMass;
        
        while (currentMass < fatalMassThreshold && cycles < 10)
        {
            currentMass *= 10f;
            cycles++;
        }
        
        return cycles - 1;
    }
    
    public static float CalculateSpeedCompounding(float storedSpeed, int cycles)
    {
        return storedSpeed * Mathf.Pow(10f, cycles);
    }
    
    public static float CalculateTimeDilation(float realTime, int speedCycles)
    {
        float subjectiveTime = realTime * Mathf.Pow(10f, speedCycles);
        return subjectiveTime;
    }
}
