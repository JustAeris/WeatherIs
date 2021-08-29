using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class Minutely
    {
        [JsonProperty("dt")] public long Dt { get; set; }

        [JsonProperty("precipitation")] public double Precipitation { get; set; }
    }
}
