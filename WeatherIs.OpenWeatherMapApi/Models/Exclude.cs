using System;

namespace WeatherIs.OpenWeatherMapApi.Models
{
    [Flags]
    public enum Exclude
    {
        None = 0,
        Current = 1,
        Minutely = 2,
        Hourly = 4,
        Daily = 8,
        Alerts = 16
    }
}
