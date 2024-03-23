using System;
using UnityEngine;

public class TimeService {
    readonly TimeSettings settings;
    DateTime currentTime;
    readonly TimeSpan sunriseTime;
    readonly TimeSpan sunsetTime;

    public DateTime CurrentTime => currentTime;

    public event Action OnSunrise = delegate { };
    public event Action OnSunset = delegate { };
    public event Action OnHourChange = delegate { };

    readonly Observable<bool> isDayTime;
    readonly Observable<int> currentHour;

    public TimeService(TimeSettings settings) {
        this.settings = settings;
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(settings.startHour);
        sunriseTime = TimeSpan.FromHours(settings.sunriseHour);
        sunsetTime = TimeSpan.FromHours(settings.sunsetHour);
        
        isDayTime = new Observable<bool>(IsDayTime());
        currentHour = new Observable<int>(currentTime.Hour);
        
        isDayTime.ValueChanged += day => (day ? OnSunrise : OnSunset)?.Invoke();
        currentHour.ValueChanged += _ => OnHourChange?.Invoke();
    }

    public void UpdateTime(float deltaTime) {
        currentTime = currentTime.AddSeconds(deltaTime * settings.timeMultiplier);
        isDayTime.Value = IsDayTime();
        currentHour.Value = currentTime.Hour;
    }
    
    public float CalculateSunAngle() {
        bool isDay = IsDayTime();
        float startDegree = isDay ? 0 : 180;
        TimeSpan start = isDay ? sunriseTime : sunsetTime;
        TimeSpan end = isDay ? sunsetTime : sunriseTime;
        
        TimeSpan totalTime = CalculateDifference(start, end);
        TimeSpan elapsedTime = CalculateDifference(start, currentTime.TimeOfDay);

        double percentage = elapsedTime.TotalMinutes / totalTime.TotalMinutes;
        return Mathf.Lerp(startDegree, startDegree + 180, (float) percentage);
    }

    // This method checks whether the current game time falls within the daytime period.
    // It returns true if the current time of day is later than sunriseTime and earlier than sunsetTime,
    // representing daytime. Otherwise, it returns false, indicating it is nighttime.
    bool IsDayTime() => currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime;
    
    // This method calculates the difference between two TimeSpan objects ("from" and "to").
    // If the calculated difference is negative, this indicates that the "from" time is ahead of the "to" time.
    // In such cases, 24 hours (representing a full day) is added to the negative difference to calculate the actual
    // time difference taking into account the next day.    
    TimeSpan CalculateDifference(TimeSpan from, TimeSpan to) {
        TimeSpan difference = to - from;
        return difference.TotalHours < 0 ? difference + TimeSpan.FromHours(24) : difference;
    }
}