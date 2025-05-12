using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Samples.BuildingTools
{
    public class SimpleTimeOfDayController : MonoBehaviour
    {
        [Tooltip("Time of day normalized between 0 and 24h. For example 6.5 amounts to 6:30am.")]
        [BoxGroup("Settings")] [Range(0, 23.99f)] public float timeOfDay = 12f;

        [BoxGroup("Settings")] [SerializeField] private Light sun;

        [Tooltip("Determines whether time automatically passes when in run mode.")]
        [BoxGroup("Settings")] [SerializeField] private bool timeProgression = true;

        [Tooltip("Sets the speed at which the time of day passes.")]
        [BoxGroup("Settings")] [SerializeField] private float timeSpeed = 0.5f;

        // Yuma, sunniest place on Earth
        [BoxGroup("Settings")] [SerializeField] private float latitude = 32.6927f;

        [BoxGroup("Event Settings")] [SerializeField] private int dayStartsAtHour = 6;
        [BoxGroup("Event Settings")] [SerializeField] private int nightStartsAtHour = 20;

        [FoldoutGroup("Events")] public UnityEvent<float> timeChangedEvent;
        [FoldoutGroup("Events")] public UnityEvent<int> hourPassedEvent;
        [FoldoutGroup("Events")] public UnityEvent<int> minutePassedEvent;
        [FoldoutGroup("Events")] public UnityEvent startOfDayEvent;
        [FoldoutGroup("Events")] public UnityEvent startOfNightEvent;

        // Arbitrary date to have the sunset framed in the camera frustum
        private readonly DateTime _date = new DateTime(2024, 4, 21).Date;
        private DateTime _time;

        private int _currentHour;
        private int _currentMinute;

        private void Awake()
        {
            GetHoursMinutesSecondsFromTimeOfDay(out int hours, out int minutes, out int _);
            _time = _date + new TimeSpan(hours, minutes, 0);

            _currentHour = Convert.ToInt32(Math.Floor(timeOfDay));
            _currentMinute = (int)((timeOfDay - (int)timeOfDay) * 60);

            _currentHour %= 24;
            dayStartsAtHour %= 24;
            nightStartsAtHour %= 24;

            // Call the event for the start state
            // If the day starts before night, e.g., day = 6, night = 18
            if (dayStartsAtHour < nightStartsAtHour)
            {
                if (_currentHour >= dayStartsAtHour && _currentHour < nightStartsAtHour)
                {
                    // It's day
                    Debug.Log("Day has broken!");
                    startOfDayEvent?.Invoke();
                }
                else
                {
                    // It's night
                    Debug.Log("Night has come!");
                    startOfNightEvent?.Invoke();
                }
            }
            else
            {
                // Handles cases like day = 20, night = 6 (day wraps around midnight)
                if (_currentHour >= dayStartsAtHour || _currentHour < nightStartsAtHour)
                {
                    // It's day
                    Debug.Log("Day has broken!");
                    startOfDayEvent?.Invoke();
                }
                else
                {
                    // It's night
                    Debug.Log("Night has come!");
                    startOfNightEvent?.Invoke();
                }
            }
        }

        private void OnValidate()
        {
            if (!sun)
            {
                return;
            }

            GetHoursMinutesSecondsFromTimeOfDay(out int hours, out int minutes, out int seconds);
            _time = _date + new TimeSpan(hours, minutes, seconds);
            SetSunPosition();
        }

        /// <summary>
        /// Update the sun position if time progression is enabled
        /// </summary>
        private void Update()
        {
            if (!timeProgression)
            {
                return;
            }

            AdvanceTimeOfDay();
            SetSunPosition();
        }

        private void AdvanceTimeOfDay()
        {
            timeOfDay += timeSpeed * Time.deltaTime;

            // This is for the variable to loop for easier use.
            if (timeOfDay > 24f)
            {
                timeOfDay = 0f;
            }

            if (timeOfDay < 0f)
            {
                timeOfDay = 24f;
            }

            timeChangedEvent?.Invoke(timeOfDay);
            // Determine if Events need to trigger
            int newHour = Convert.ToInt32(Math.Floor(timeOfDay));
            int newMinute = (int)((timeOfDay - (int)timeOfDay) * 60);

            if (newHour != _currentHour)
            {
                _currentHour = newHour;
                hourPassedEvent?.Invoke(_currentHour);

                if (newHour == dayStartsAtHour)
                {
                    Debug.Log("Day has broken!");
                    startOfDayEvent?.Invoke();
                }

                if (newHour == nightStartsAtHour)
                {
                    Debug.Log("Night has come!");
                    startOfNightEvent?.Invoke();
                }
            }

            if (newMinute != _currentMinute)
            {
                _currentMinute = newMinute;
                minutePassedEvent?.Invoke(_currentMinute);
            }
        }

        private void SetSunPosition()
        {
            CalculateSunPosition(_time, latitude, timeOfDay, out double azi, out double alt);

            if (double.IsNaN(azi))
            {
                azi = sun.transform.localRotation.y;
            }

            Vector3 angles = new((float)alt, (float)azi, 0);
            sun.transform.localRotation = Quaternion.Euler(angles);
        }

        private void CalculateSunPosition(DateTime dateTime, double sunLatitude, float currentTimeOfDay,
            out double outAzimuth, out double outAltitude)
        {
            float declination = -23.45f * Mathf.Cos(Mathf.PI * 2f * (dateTime.DayOfYear + 10) / 365f);

            float localSolarTime = currentTimeOfDay;
            float localHourAngle = 15f * (localSolarTime - 12f);
            localHourAngle *= Mathf.Deg2Rad;

            declination *= Mathf.Deg2Rad;
            float latRad = (float)sunLatitude * Mathf.Deg2Rad;

            float latSin = Mathf.Sin(latRad);
            float latCos = Mathf.Cos(latRad);

            float hourCos = Mathf.Cos(localHourAngle);

            float declinationSin = Mathf.Sin(declination);
            float declinationCos = Mathf.Cos(declination);

            float elevation = Mathf.Asin(declinationSin * latSin + declinationCos * latCos * hourCos);
            float elevationCos = Mathf.Cos(elevation);
            float azimuth =
                Mathf.Acos((declinationSin * latCos - declinationCos * latSin * hourCos) / elevationCos);

            elevation *= Mathf.Rad2Deg;
            azimuth *= Mathf.Rad2Deg;

            if (localHourAngle >= 0f)
                azimuth = 360 - azimuth;

            outAltitude = elevation;
            outAzimuth = azimuth;
        }

        private void GetHoursMinutesSecondsFromTimeOfDay(out int hours, out int minutes, out int seconds)
        {
            hours = Mathf.FloorToInt(timeOfDay);
            minutes = Mathf.FloorToInt((timeOfDay - hours) * 60f);
            seconds = Mathf.FloorToInt((timeOfDay - hours - minutes / 60f) * 60f * 60f);
        }
    }
}