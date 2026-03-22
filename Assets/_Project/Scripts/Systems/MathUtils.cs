using UnityEngine;
/// <summary>
/// Common math utilities.
/// Usage: MathUtils.Damp(1f, 0f, 0.5f, deltaTime);
/// </summary>
public static class MathUtils
{
    // Smooth damping (like Mathf.Lerp but speed-based)
    public static float Damp(float current, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-smoothing * dt));
    }
    
    public static Vector3 Damp(Vector3 current, Vector3 target, float smoothing, float dt)
    {
        return Vector3.Lerp(current, target, 1f - Mathf.Exp(-smoothing * dt));
    }
    
    // Smooth step (ease in-out)
    public static float SmoothStep(float from, float to, float t)
    {
        t = Mathf.Clamp01(t);
        t = -2f * t * t * t + 3f * t * t;
        return to * t + from * (1f - t);
    }
    
    // Remap value from one range to another
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
    
    // Check if value is between (inclusive)
    public static bool Between(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    
    // Angular distance between two angles (0-360)
    public static float AngularDistance(float angle1, float angle2)
    {
        float diff = (angle2 - angle1 + 180f) % 360f - 180f;
        return diff < -180f ? diff + 360f : diff;
    }
    
    // Rotate towards target angle
    public static float RotateTowards(float current, float target, float maxDelta)
    {
        float delta = AngularDistance(current, target);
        if (Mathf.Abs(delta) < maxDelta)
        {
            return target;
        }
        return current + Mathf.Sign(delta) * maxDelta;
    }
    
    // Ease out (starts fast, ends slow)
    public static float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 2f);
    }
    
    // Ease in (starts slow, ends fast)
    public static float EaseIn(float t)
    {
        return t * t;
    }
    
    // Ease in-out (slow start and end)
    public static float EaseInOut(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
}
