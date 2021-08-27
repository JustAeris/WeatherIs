using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class Weather
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("main")] public string Main { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("icon")] public string Icon { get; set; }
    }
}