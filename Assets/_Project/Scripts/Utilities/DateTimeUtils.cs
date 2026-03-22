using UnityEngine;

namespace MistbornGame.Utilities
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Gets the current date as a formatted string
        /// </summary>
        public static string GetDateString(string format = "yyyy-MM-dd")
        {
            return System.DateTime.Now.ToString(format);
        }

        /// <summary>
        /// Gets the current time as a formatted string
        /// </summary>
        public static string GetTimeString(string format = "HH:mm:ss")
        {
            return System.DateTime.Now.ToString(format);
        }

        /// <summary>
        /// Gets the current date and time as a formatted string
        /// </summary>
        public static string GetDateTimeString(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return System.DateTime.Now.ToString(format);
        }

        /// <summary>
        /// Gets the current Unix timestamp (seconds since Jan 1, 1970)
        /// </summary>
        public static long GetUnixTimestamp()
        {
            return System.DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Gets the current Unix timestamp in milliseconds
        /// </summary>
        public static long GetUnixTimestampMs()
        {
            return System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts a Unix timestamp to DateTime
        /// </summary>
        public static System.DateTime FromUnixTimestamp(long timestamp)
        {
            return System.DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        /// <summary>
        /// Converts a Unix timestamp in milliseconds to DateTime
        /// </summary>
        public static System.DateTime FromUnixTimestampMs(long timestamp)
        {
            return System.DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        }

        /// <summary>
        /// Checks if a given year is a leap year
        /// </summary>
        public static bool IsLeapYear(int year)
        {
            return System.DateTime.IsLeapYear(year);
        }

        /// <summary>
        /// Gets the number of days in a given month and year
        /// </summary>
        public static int DaysInMonth(int year, int month)
        {
            return System.DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        /// Adds days to a DateTime and returns the result
        /// </summary>
        public static System.DateTime AddDays(System.DateTime date, int days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Adds hours to a DateTime and returns the result
        /// </summary>
        public static System.DateTime AddHours(System.DateTime date, int hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Adds minutes to a DateTime and returns the result
        /// </summary>
        public static System.DateTime AddMinutes(System.DateTime date, int minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds seconds to a DateTime and returns the result
        /// </summary>
        public static System.DateTime AddSeconds(System.DateTime date, int seconds)
        {
            return date.AddSeconds(seconds);
        }
    }
}