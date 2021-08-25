using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models
{
    public class HomeViewModel
    {
        public CurrentWeatherDataResponse WeatherData { get; set; }
        public bool MetricUnits { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsUsingAutoGeolocation { get; set; }
    }
}