using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.CurrentWeather
{
    public class MainData
    {
        [JsonProperty("temp")]
        public float Temperature { get; set; }

        [JsonProperty("feels_like")]
        public float TemperatureFeelsLike { get; set; }

        [JsonProperty("pressure")]
        public float Pressure { get; set; }
        
        [JsonProperty("humidity")] 
        public float Humidity { get; set; }

        [JsonProperty("temp_min")]
        public float TemperatureMinimum { get; set; }
        
        [JsonProperty("temp_max")]
        public float TemperatureMaximum { get; set; }

        [JsonProperty("sea_level")]
        public float PressureAtSeaLevel { get; set; }

        [JsonProperty("grnd_level")]
        public float PressureAtGroundLevel { get; set; }
    }
}