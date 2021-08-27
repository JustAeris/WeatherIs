using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.CurrentWeather
{
    public class Sys
    {
        [JsonProperty("country")]
        public string CountryCode { get; set; }

        [JsonProperty("sunrise")]
        public int SunriseTime { get; set; }
        
        [JsonProperty("sunset")]
        public int SunsetTime { get; set; }
    }
}