using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class FeelsLike
    {
        [JsonProperty("day")] public double Day { get; set; }

        [JsonProperty("night")] public double Night { get; set; }

        [JsonProperty("eve")] public double Eve { get; set; }

        [JsonProperty("morn")] public double Morn { get; set; }
    }
}