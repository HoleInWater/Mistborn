/* ScadrialCalendar.cs
 *
 * Tracks in-game date and time for the world of Scadrial.
 * Integrates with DayNightCycle for time-of-day and provides
 * lore-accurate calendar features.
 *
 * The Final Empire uses a 10-month calendar:
 *   Each month has approximately 5 weeks of 5 days each (~25 days per month)
 *   The year has ~250 days
 *
 * The game starts in 1022 FE (Final Empire dating).
 *
 * From AllomancyConstants:
 *   InGameDayRealMinutes = 20 (1 real minute = 1.2 in-game hours)
 *   InGameSecondsPerRealSecond = 72
 */

using UnityEngine;

public class ScadrialCalendar : MonoBehaviour
{
    public static ScadrialCalendar Instance { get; private set; }

    [Header("Starting Date")]
    public int startYear = 1022;
    public int startMonth = 1;
    public int startDay = 1;

    [Header("Calendar Constants")]
    public int monthsPerYear = 10;
    public int daysPerMonth = 25;
    public int daysPerWeek = 5;

    [Header("Current Date (read-only in Inspector)")]
    [SerializeField] private int currentYear;
    [SerializeField] private int currentMonth;
    [SerializeField] private int currentDay;
    [SerializeField] private int totalDaysPassed;

    [Header("Tracking")]
    private float dayAccumulator;

    // Day names (speculation based on lore)
    public static readonly string[] DayNames = {
        "Ashday", "Mistday", "Burnday", "Coinday", "Metalday"
    };

    // Month names (from the Coppermind where available, filled in with lore-appropriate names)
    public static readonly string[] MonthNames = {
        "Firstmonth", "Ashmonth", "Mistmonth", "Seedmonth", "Growmonth",
        "Harvestmonth", "Fallingmonth", "Darkmonth", "Deepmonth", "Lastmonth"
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        currentYear = startYear;
        currentMonth = startMonth;
        currentDay = startDay;
        totalDaysPassed = 0;
    }

    void Update()
    {
        // Advance calendar based on DayNightCycle
        if (DayNightCycle.Instance != null)
        {
            float hoursPerSecond = 24f / (DayNightCycle.Instance.dayLengthMinutes * 60f);
            float daysPerSecond = hoursPerSecond / 24f;
            dayAccumulator += daysPerSecond * Time.deltaTime;

            while (dayAccumulator >= 1f)
            {
                dayAccumulator -= 1f;
                AdvanceDay();
            }
        }
    }

    void AdvanceDay()
    {
        totalDaysPassed++;
        currentDay++;

        if (currentDay > daysPerMonth)
        {
            currentDay = 1;
            currentMonth++;

            if (currentMonth > monthsPerYear)
            {
                currentMonth = 1;
                currentYear++;
            }
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public string GetDateString()
    {
        return $"{currentDay} {GetMonthName()}, {currentYear} FE";
    }

    public string GetDayName()
    {
        int dayOfWeek = (totalDaysPassed % daysPerWeek);
        return DayNames[dayOfWeek];
    }

    public string GetMonthName()
    {
        return MonthNames[Mathf.Clamp(currentMonth - 1, 0, MonthNames.Length - 1)];
    }

    public int GetYear() => currentYear;
    public int GetMonth() => currentMonth;
    public int GetDay() => currentDay;
    public int GetTotalDaysPassed() => totalDaysPassed;

    public bool IsMistSeason()
    {
        // Lore: mists are thicker during certain months
        return currentMonth >= 7; // Fallingmonth through Lastmonth
    }

    public void SetDate(int year, int month, int day)
    {
        currentYear = year;
        currentMonth = Mathf.Clamp(month, 1, monthsPerYear);
        currentDay = Mathf.Clamp(day, 1, daysPerMonth);
    }

    // Save/Load support
    public int[] GetSaveData() => new int[] { currentYear, currentMonth, currentDay, totalDaysPassed };

    public void LoadSaveData(int[] data)
    {
        if (data.Length >= 4)
        {
            currentYear = data[0];
            currentMonth = data[1];
            currentDay = data[2];
            totalDaysPassed = data[3];
        }
    }
}
