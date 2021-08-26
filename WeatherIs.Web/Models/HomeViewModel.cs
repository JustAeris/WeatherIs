using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models
{
    public class HomeViewModel
    {
        public CurrentWeatherDataResponse WeatherData { get; set; }
        public bool MetricUnits { get; set; }

        public string ResolveWindDirection => WeatherData.Wind.Degrees switch
        {
            > 337.5f => "N",
            > 292.5f => "NW",
            > 247.5f => "W",
            > 202.5f => "SW",
            > 157.5f => "S",
            > 112.5f => "SE",
            > 67.5f => "E",
            > 22.5f => "NE",
            > 0 => "N",
            _ => "Unresolved"
        };

        public string ErrorMessage { get; set; }

        public bool IsUsingAutoGeolocation { get; set; }
    }
}
