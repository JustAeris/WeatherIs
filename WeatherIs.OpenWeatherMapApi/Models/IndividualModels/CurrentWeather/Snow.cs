using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.CurrentWeather
{
    public class Snow
    {
        [JsonProperty("1h")]
        public float LastHour { get; set; }

        [JsonProperty("3h")]
        public float LastThreeHours { get; set; }
    }
}