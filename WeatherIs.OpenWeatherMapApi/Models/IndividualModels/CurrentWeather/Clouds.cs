using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.CurrentWeather
{
    public class Clouds
    {
        [JsonProperty("all")]
        public float Cloudiness { get; set; }
    }
}
