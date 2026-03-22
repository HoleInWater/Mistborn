using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for AnimationCurve operations
    /// </summary>
    public static class AnimationCurveUtils
    {
        /// <summary>
        /// Creates a linear AnimationCurve between two values
        /// </summary>
        public static AnimationCurve Linear(float startTime, float startValue, float endTime, float endValue)
        {
            return AnimationCurve.Linear(startTime, startValue, endTime, endValue);
        }

        /// <summary>
        /// Creates an ease-in-out AnimationCurve
        /// </summary>
        public static AnimationCurve EaseInOut(float startTime, float startValue, float endTime, float endValue)
        {
            return AnimationCurve.EaseInOut(startTime, startValue, endTime, endValue);
        }

        /// <summary>
        /// Creates a constant AnimationCurve
        /// </summary>
        public static AnimationCurve Constant(float time, float value)
        {
            return new AnimationCurve(
                new Keyframe(time, value),
                new Keyframe(time + 0.0001f, value)
            );
        }

        /// <summary>
        /// Gets the value of an AnimationCurve at a given time
        /// </summary>
        public static float Evaluate(AnimationCurve curve, float time)
        {
            if (curve == null) return 0f;
            return curve.Evaluate(time);
        }

        /// <summary>
        /// Adds a key to an AnimationCurve at a specific time and value
        /// </summary>
        public static void AddKey(AnimationCurve curve, float time, float value)
        {
            if (curve == null) return;
            curve.AddKey(time, value);
        }

        /// <summary>
        /// Removes all keys from an AnimationCurve
        /// </summary>
        public static void ClearKeys(AnimationCurve curve)
        {
            if (curve == null) return;
            curve.keys = new Keyframe[0];
        }

        /// <summary>
        /// Gets the length of an AnimationCurve (time range)
        /// </summary>
        public static float GetLength(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return 0f;
            return curve[curve.length - 1].time - curve[0].time;
        }

        /// <summary>
        /// Gets the minimum value of an AnimationCurve
        /// </summary>
        public static float GetMinValue(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return 0f;
            float min = float.MaxValue;
            foreach (Keyframe key in curve.keys)
            {
                if (key.value < min) min = key.value;
            }
            return min;
        }

        /// <summary>
        /// Gets the maximum value of an AnimationCurve
        /// </summary>
        public static float GetMaxValue(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return 0f;
            float max = float.MinValue;
            foreach (Keyframe key in curve.keys)
            {
                if (key.value > max) max = key.value;
            }
            return max;
        }

        /// <summary>
        /// Normalizes an AnimationCurve to have values between 0 and 1
        /// </summary>
        public static AnimationCurve Normalize(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return curve;
            
            float min = GetMinValue(curve);
            float max = GetMaxValue(curve);
            float range = max - min;
            if (range == 0f) return curve;
            
            AnimationCurve normalized = new AnimationCurve();
            foreach (Keyframe key in curve.keys)
            {
                float normalizedValue = (key.value - min) / range;
                normalized.AddKey(new Keyframe(key.time, normalizedValue, key.inTangent, key.outTangent));
            }
            return normalized;
        }

        /// <summary>
        /// Reverses an AnimationCurve (time direction)
        /// </summary>
        public static AnimationCurve Reverse(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return curve;
            
            float startTime = curve[0].time;
            float endTime = curve[curve.length - 1].time;
            AnimationCurve reversed = new AnimationCurve();
            
            for (int i = curve.length - 1; i >= 0; i--)
            {
                Keyframe key = curve[i];
                float reversedTime = endTime - (key.time - startTime);
                reversed.AddKey(new Keyframe(reversedTime, key.value, key.outTangent, key.inTangent));
            }
            return reversed;
        }

        /// <summary>
        /// Creates a random AnimationCurve with specified number of keys
        /// </summary>
        public static AnimationCurve RandomCurve(int keyCount, float timeRange, float valueRange)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < keyCount; i++)
            {
                float time = Random.Range(0f, timeRange);
                float value = Random.Range(-valueRange, valueRange);
                curve.AddKey(time, value);
            }
            return curve;
        }

        /// <summary>
        /// Smooths an AnimationCurve using moving average
        /// </summary>
        public static AnimationCurve Smooth(AnimationCurve curve, int passes = 1)
        {
            if (curve == null || curve.length <= 2) return curve;
            
            for (int pass = 0; pass < passes; pass++)
            {
                Keyframe[] smoothed = new Keyframe[curve.length];
                for (int i = 0; i < curve.length; i++)
                {
                    if (i == 0 || i == curve.length - 1)
                    {
                        smoothed[i] = curve[i];
                    }
                    else
                    {
                        float prevValue = curve[i - 1].value;
                        float currValue = curve[i].value;
                        float nextValue = curve[i + 1].value;
                        smoothed[i] = new Keyframe(curve[i].time, (prevValue + currValue + nextValue) / 3f, curve[i].inTangent, curve[i].outTangent);
                    }
                }
                curve.keys = smoothed;
            }
            return curve;
        }
    }
}
