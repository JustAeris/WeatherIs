using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels
{
    public class Coords
    {
        [JsonProperty("lon")]
        public double Longitude { get; set; }
        
        [JsonProperty("lat")]
        public double Latitude { get; set; }
    }
}