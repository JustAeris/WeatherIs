using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models
{
    public class ForecastViewModel
    {
        public OneCallApiResponse ForecastData { get; set; }

        public string ErrorMessage { get; set; }

        public bool MetricUnits { get; set; }

        public bool IsUsingAutoGeolocation { get; set; }
    }
}
