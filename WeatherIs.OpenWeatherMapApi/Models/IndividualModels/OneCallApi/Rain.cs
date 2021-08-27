using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class Rain
    {
        [JsonProperty("1h")] public double _1h { get; set; }
    }
}