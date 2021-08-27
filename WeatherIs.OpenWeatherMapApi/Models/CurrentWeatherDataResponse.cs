using System.Collections.Generic;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi.Models.IndividualModels.CurrentWeather;

namespace WeatherIs.OpenWeatherMapApi.Models
{
    public class CurrentWeatherDataResponse
    {
        [JsonProperty("coord")]
        public Coords Coords { get; set; }

        [JsonProperty("weather")]
        public IList<Weather> Weathers { get; set; }

        [JsonProperty("main")]
        public MainData MainData { get; set; }

        [JsonProperty("wind")]
        public Wind Wind { get; set; }

        [JsonProperty("clouds")]
        public Clouds Clouds { get; set; }

        [JsonProperty("rain")]
        public Rain Rain { get; set; }

        [JsonProperty("snow")]
        public Snow Snow { get; set; }

        [JsonProperty("sys")]
        public Sys Sys { get; set; }

        [JsonProperty("timezone")]
        public int Timezone { get; set; }

        [JsonProperty("id")]
        public int CityId { get; set; }

        [JsonProperty("name")]
        public string CityName { get; set; }
    }
}
