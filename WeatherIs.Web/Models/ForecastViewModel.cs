using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models
{
    public class ForecastViewModel
    {
        public OneCallApiResponse ForecastData { get; set; }

        public string ErrorMessage { get; set; }

        public bool MetricUnits { get; set; }
        public string TemperatureUnit => MetricUnits switch
        {
            true => "°C",
            false => "°F"
        };

        public string WindSpeedUnit => MetricUnits switch
        {
            true => "km/h",
            false => "mph"
        };

        public bool IsUsingAutoGeolocation { get; set; }
    }
}
