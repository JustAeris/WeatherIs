using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels
{
    public class Wind
    {
        [JsonProperty("speed")]
        public float Speed { get; set; }

        [JsonProperty("deg")]
        public float Degrees { get; set; }

        [JsonProperty("gust")]
        public float Gust { get; set; }
    }
}