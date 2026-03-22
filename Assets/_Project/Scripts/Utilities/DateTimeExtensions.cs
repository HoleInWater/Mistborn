using System;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for DateTime struct
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the day of week as a string
        /// </summary>
        public static string GetDayOfWeekString(this DateTime dateTime)
        {
            return dateTime.DayOfWeek.ToString();
        }

        /// <summary>
        /// Gets the month name
        /// </summary>
        public static string GetMonthName(this DateTime dateTime)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        /// <summary>
        /// Checks if the date is today
        /// </summary>
        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today;
        }

        /// <summary>
        /// Checks if the date is yesterday
        /// </summary>
        public static bool IsYesterday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// Checks if the date is tomorrow
        /// </summary>
        public static bool IsTomorrow(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// Checks if the date is a weekend (Saturday or Sunday)
        /// </summary>
        public static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Checks if the date is a weekday (Monday-Friday)
        /// </summary>
        public static bool IsWeekday(this DateTime dateTime)
        {
            return !dateTime.IsWeekend();
        }

        /// <summary>
        /// Gets the start of the day (00:00:00)
        /// </summary>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Gets the end of the day (23:59:59.999)
        /// </summary>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Gets the start of the week (Sunday by default)
        /// </summary>
        public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Sunday)
        {
            int diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets the end of the week (Saturday by default)
        /// </summary>
        public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek endOfWeek = DayOfWeek.Saturday)
        {
            int diff = (7 + (endOfWeek - dateTime.DayOfWeek)) % 7;
            return dateTime.AddDays(diff).Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Gets the start of the month (1st day)
        /// </summary>
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// Gets the end of the month (last day)
        /// </summary>
        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59, 999);
        }

        /// <summary>
        /// Gets the start of the year (January 1st)
        /// </summary>
        public static DateTime StartOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1);
        }

        /// <summary>
        /// Gets the end of the year (December 31st)
        /// </summary>
        public static DateTime EndOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
        }

        /// <summary>
        /// Gets the quarter (1-4)
        /// </summary>
        public static int GetQuarter(this DateTime dateTime)
        {
            return (dateTime.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// Gets the start of the quarter
        /// </summary>
        public static DateTime StartOfQuarter(this DateTime dateTime)
        {
            int quarter = dateTime.GetQuarter();
            int startMonth = (quarter - 1) * 3 + 1;
            return new DateTime(dateTime.Year, startMonth, 1);
        }

        /// <summary>
        /// Gets the end of the quarter
        /// </summary>
        public static DateTime EndOfQuarter(this DateTime dateTime)
        {
            int quarter = dateTime.GetQuarter();
            int endMonth = quarter * 3;
            return new DateTime(dateTime.Year, endMonth, DateTime.DaysInMonth(dateTime.Year, endMonth), 23, 59, 59, 999);
        }

        /// <summary>
        /// Adds business days (skips weekends)
        /// </summary>
        public static DateTime AddBusinessDays(this DateTime dateTime, int days)
        {
            DateTime result = dateTime;
            while (days > 0)
            {
                result = result.AddDays(1);
                if (result.IsWeekday())
                {
                    days--;
                }
            }
            while (days < 0)
            {
                result = result.AddDays(-1);
                if (result.IsWeekday())
                {
                    days++;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the age from this date to today
        /// </summary>
        public static int GetAge(this DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// Gets the time elapsed since this date
        /// </summary>
        public static TimeSpan TimeSince(this DateTime dateTime)
        {
            return DateTime.Now - dateTime;
        }

        /// <summary>
        /// Gets the time until this date
        /// </summary>
        public static TimeSpan TimeUntil(this DateTime dateTime)
        {
            return dateTime - DateTime.Now;
        }

        /// <summary>
        /// Converts to Unix timestamp (seconds since epoch)
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Creates a DateTime from Unix timestamp
        /// </summary>
        public static DateTime FromUnixTimestamp(long unixTimestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimestamp).ToLocalTime();
        }

        /// <summary>
        /// Gets a human-readable relative time string
        /// </summary>
        public static string ToRelativeTimeString(this DateTime dateTime)
        {
            TimeSpan span = DateTime.Now - dateTime;
            
            if (span.TotalDays > 365)
            {
                int years = (int)(span.TotalDays / 365);
                return $"{years} {(years == 1 ? "year" : "years")} ago";
            }
            if (span.TotalDays > 30)
            {
                int months = (int)(span.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }
            if (span.TotalDays > 0)
            {
                int days = (int)span.TotalDays;
                return $"{days} {(days == 1 ? "day" : "days")} ago";
            }
            if (span.TotalHours > 0)
            {
                int hours = (int)span.TotalHours;
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }
            if (span.TotalMinutes > 0)
            {
                int minutes = (int)span.TotalMinutes;
                return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
            }
            return "just now";
        }
    }
}
