using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    public class TimeUtils : MonoBehaviour
    {
        /// <summary>
        /// Converts seconds to minutes:seconds format
        /// </summary>
        public static string SecondsToMMSS(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, secs);
        }

        /// <summary>
        /// Converts seconds to hours:minutes:seconds format
        /// </summary>
        public static string SecondsToHHMMSS(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, secs);
        }

        /// <summary>
        /// Returns true if approximately equal (within epsilon)
        /// </summary>
        public static bool Approximately(float a, float b, float epsilon = 0.001f)
        {
            return Mathf.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Returns true if approximately equal (using Mathf.Epsilon)
        /// </summary>
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <summary>
        /// Gets the time scale independent delta time
        /// </summary>
        public static float UnscaledDeltaTime => Time.unscaledDeltaTime;

        /// <summary>
        /// Gets the time scale independent time since start
        /// </summary>
        public static float UnscaledTime => Time.unscaledTime;

        /// <summary>
        /// Waits for the specified number of unscaled seconds
        /// </summary>
        public static IEnumerator WaitForUnscaledSeconds(float seconds)
        {
            float endTime = Time.unscaledTime + seconds;
            while (Time.unscaledTime < endTime)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Returns a timestamp string in format HH:MM:SS.mmm
        /// </summary>
        public static string Timestamp()
        {
            float time = Time.time;
            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, milliseconds);
        }
    }
}
