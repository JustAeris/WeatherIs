using System;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models.Cookies
{
    public class WeatherCache
    {
        public float CityId { get; set; }

        public string IpAddress { get; set; }

        public DateTime ExpiryDate { get; set; }

        public CurrentWeatherDataResponse WeatherData { get; set; }

        public bool MetricUnits { get; set; }
    }
}
