using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models.Cookies
{
    public class PreferredUnits
    {
        public UnitsType Type { get; set; }

        public bool Automatic { get; set; }
    }
}
