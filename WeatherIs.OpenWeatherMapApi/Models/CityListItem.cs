using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi.Models.IndividualModels;

namespace WeatherIs.OpenWeatherMapApi.Models
{
    public class CityListItem
    {
        [JsonProperty("id")] public float Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("state")] public string State { get; set; }

        [JsonProperty("country")] public string Country { get; set; }
        
        [JsonProperty("coord")] public Coords Coords { get; set; }
    }
}