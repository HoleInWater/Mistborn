using UnityEngine;

namespace MistbornGameplay
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 600f;
        [SerializeField] private float nightDuration = 300f;
        [SerializeField] private float dawnDuration = 60f;
        [SerializeField] private float duskDuration = 60f;
        
        [Header("Light Settings")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color dayColor = Color.white;
        [SerializeField] private Color nightColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        [SerializeField] private Color dawnColor = new Color(1f, 0.7f, 0.5f, 1f);
        [SerializeField] private Color duskColor = new Color(1f, 0.4f, 0.3f, 1f);
        
        [Header("Intensity Settings")]
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensity = 0.1f;
        [SerializeField] private float dawnDuskIntensity = 0.5f;
        
        [Header("Ambient Settings")]
        [SerializeField] private Gradient ambientDayColor;
        [SerializeField] private Gradient ambientNightColor;
        
        [Header("Mist Settings")]
        [SerializeField] private bool enableMistAtNight = true;
        [SerializeField] private ParticleSystem mistParticles;
        [SerializeField] private float mistStartTime = 0.7f;
        [SerializeField] private float mistEndTime = 0.3f;
        
        private float currentTime = 0f;
        private float totalDayDuration;
        private int currentDay = 1;
        private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
        private bool isPaused = false;
        
        public enum TimeOfDay
        {
            Dawn,
            Day,
            Dusk,
            Night
        }
        
        private void Awake()
        {
            totalDayDuration = dayDuration + dawnDuration + duskDuration + nightDuration;
        }
        
        private void Start()
        {
            currentTime = 0f;
            
            if (sunLight == null)
            {
                sunLight = GetComponent<Light>();
            }
            
            if (mistParticles != null)
            {
                mistParticles.Stop();
            }
        }
        
        private void Update()
        {
            if (isPaused)
            {
                return;
            }
            
            UpdateTime();
            UpdateLighting();
            UpdateMist();
        }
        
        private void UpdateTime()
        {
            currentTime += Time.deltaTime;
            
            if (currentTime >= totalDayDuration)
            {
                currentTime = 0f;
                currentDay++;
                
                if (OnNewDay != null)
                {
                    OnNewDay(currentDay);
                }
            }
            
            UpdateTimeOfDay();
        }
        
        private void UpdateTimeOfDay()
        {
            float normalizedTime = currentTime / totalDayDuration;
            
            TimeOfDay newTimeOfDay;
            
            if (normalizedTime < dawnDuration / totalDayDuration)
            {
                newTimeOfDay = TimeOfDay.Dawn;
            }
            else if (normalizedTime < (dawnDuration + dayDuration) / totalDayDuration)
            {
                newTimeOfDay = TimeOfDay.Day;
            }
            else if (normalizedTime < (dawnDuration + dayDuration + duskDuration) / totalDayDuration)
            {
                newTimeOfDay = TimeOfDay.Dusk;
            }
            else
            {
                newTimeOfDay = TimeOfDay.Night;
            }
            
            if (newTimeOfDay != currentTimeOfDay)
            {
                currentTimeOfDay = newTimeOfDay;
                
                if (OnTimeOfDayChanged != null)
                {
                    OnTimeOfDayChanged(currentTimeOfDay);
                }
            }
        }
        
        private void UpdateLighting()
        {
            float normalizedTime = currentTime / totalDayDuration;
            
            Color lightColor;
            float lightIntensity;
            
            if (normalizedTime < dawnDuration / totalDayDuration)
            {
                float t = normalizedTime / (dawnDuration / totalDayDuration);
                lightColor = Color.Lerp(nightColor, dawnColor, t);
                lightIntensity = Mathf.Lerp(nightIntensity, dawnDuskIntensity, t);
                UpdateSunRotation(90f, t);
            }
            else if (normalizedTime < (dawnDuration + dayDuration) / totalDayDuration)
            {
                float t = (normalizedTime - dawnDuration / totalDayDuration) / (dayDuration / totalDayDuration);
                lightColor = Color.Lerp(dawnColor, dayColor, t);
                lightIntensity = Mathf.Lerp(dawnDuskIntensity, dayIntensity, t);
                UpdateSunRotation(180f, t);
            }
            else if (normalizedTime < (dawnDuration + dayDuration + duskDuration) / totalDayDuration)
            {
                float t = (normalizedTime - (dawnDuration + dayDuration) / totalDayDuration) / (duskDuration / totalDayDuration);
                lightColor = Color.Lerp(dayColor, duskColor, t);
                lightIntensity = Mathf.Lerp(dayIntensity, dawnDuskIntensity, t);
                UpdateSunRotation(270f, t);
            }
            else
            {
                float t = (normalizedTime - (dawnDuration + dayDuration + duskDuration) / totalDayDuration) / (nightDuration / totalDayDuration);
                lightColor = Color.Lerp(duskColor, nightColor, t);
                lightIntensity = Mathf.Lerp(dawnDuskIntensity, nightIntensity, t);
                UpdateSunRotation(0f, t);
            }
            
            if (sunLight != null)
            {
                sunLight.color = lightColor;
                sunLight.intensity = lightIntensity;
            }
            
            RenderSettings.ambientLight = Color.Lerp(ambientNightColor.Evaluate(normalizedTime), ambientDayColor.Evaluate(normalizedTime), GetDayNightBlend());
        }
        
        private void UpdateSunRotation(float targetAngle, float progress)
        {
            if (sunLight != null)
            {
                float currentAngle = sunLight.transform.eulerAngles.x;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, progress * Time.deltaTime);
                sunLight.transform.eulerAngles = new Vector3(newAngle, sunLight.transform.eulerAngles.y, 0f);
            }
        }
        
        private float GetDayNightBlend()
        {
            float normalizedTime = currentTime / totalDayDuration;
            
            if (normalizedTime < dawnDuration / totalDayDuration)
            {
                return 0f;
            }
            else if (normalizedTime < (dawnDuration + dayDuration) / totalDayDuration)
            {
                return 1f;
            }
            else if (normalizedTime < (dawnDuration + dayDuration + duskDuration) / totalDayDuration)
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
        
        private void UpdateMist()
        {
            if (!enableMistAtNight || mistParticles == null)
            {
                return;
            }
            
            float normalizedTime = currentTime / totalDayDuration;
            
            if (normalizedTime >= mistStartTime || normalizedTime <= mistEndTime)
            {
                if (!mistParticles.isPlaying)
                {
                    mistParticles.Play();
                }
            }
            else
            {
                if (mistParticles.isPlaying)
                {
                    mistParticles.Stop();
                }
            }
        }
        
        public float GetTimeOfDay()
        {
            return currentTime / totalDayDuration;
        }
        
        public TimeOfDay GetCurrentTimeOfDay()
        {
            return currentTimeOfDay;
        }
        
        public int GetCurrentDay()
        {
            return currentDay;
        }
        
        public bool IsNight()
        {
            return currentTimeOfDay == TimeOfDay.Night;
        }
        
        public bool IsDay()
        {
            return currentTimeOfDay == TimeOfDay.Day;
        }
        
        public void SetTime(float time)
        {
            currentTime = Mathf.Clamp(time, 0f, totalDayDuration);
        }
        
        public void SetDay(int day)
        {
            currentDay = day;
        }
        
        public void PauseTime()
        {
            isPaused = true;
        }
        
        public void ResumeTime()
        {
            isPaused = false;
        }
        
        public bool IsPaused()
        {
            return isPaused;
        }
        
        public void SetDayDuration(float duration)
        {
            dayDuration = duration;
            totalDayDuration = dayDuration + dawnDuration + duskDuration + nightDuration;
        }
        
        public void SetNightDuration(float duration)
        {
            nightDuration = duration;
            totalDayDuration = dayDuration + dawnDuration + duskDuration + nightDuration;
        }
        
        public event System.Action<int> OnNewDay;
        public event System.Action<TimeOfDay> OnTimeOfDayChanged;
    }
}
