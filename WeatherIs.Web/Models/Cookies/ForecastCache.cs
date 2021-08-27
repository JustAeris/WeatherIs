using System;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models.Cookies
{
    public class ForecastCache
    {
        public float CityId { get; set; }

        public string IpAddress { get; set; }

        public DateTime ExpiryDate { get; set; }

        public OneCallApiResponse ForecastData { get; set; }

        public bool MetricUnits { get; set; }
    }
}
