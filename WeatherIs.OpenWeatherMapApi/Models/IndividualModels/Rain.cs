using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels
{
    public class Rain
    {
        [JsonProperty("1h")]
        public float LastHour { get; set; }

        [JsonProperty("3h")]
        public float LastThreeHours { get; set; }
    }
}