using UnityEngine;

/// <summary>
/// Static utility class containing core physics formulas for Allomancy.
/// Based on docs/PHYSICS-MATH-BOOK.md.
/// </summary>
public static class AllomancyPhysicsFormulas
{
    /// <summary>
    /// Calculates the inverse-square force magnitude for a Push or Pull.
    /// F = (G * m1 * m2) / r^2 (approximated for gameplay)
    /// </summary>
    public static float CalculateAllomanticForce(float senderMass, float targetMass, float distance, float intensity)
    {
        // Prevent division by zero and near-zero distances
        float r = Mathf.Max(0.5f, distance);
        
        // Gameplay formula: (Intensity * CombinedMass) / Distance^2
        float baseForce = (intensity * (senderMass + targetMass)) / (r * r);
        
        return baseForce;
    }

    /// <summary>
    /// Calculates the reaction force on the sender when pushing/pulling.
    /// </summary>
    public static Vector3 GetReactionForce(Vector3 forceVector, float senderMass, float targetMass)
    {
        // If target is much heavier, sender takes most of the force
        float ratio = targetMass / (senderMass + targetMass);
        return -forceVector * ratio;
    }

    /// <summary>
    /// Predicts the trajectory of a coin throw or jump.
    /// </summary>
    public static Vector3 PredictPosition(Vector3 startPos, Vector3 velocity, float time)
    {
        return startPos + (velocity * time) + (0.5f * Physics.gravity * time * time);
    }
}
