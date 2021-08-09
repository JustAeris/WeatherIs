using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels
{
    public class Clouds
    {
        [JsonProperty("all")]
        public float Cloudiness { get; set; }
    }
}