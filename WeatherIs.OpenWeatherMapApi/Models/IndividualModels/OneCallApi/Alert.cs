using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class Alert
    {
        [JsonProperty("sender_name")] public string SenderName { get; set; }

        [JsonProperty("event")] public string Event { get; set; }

        [JsonProperty("start")] public int Start { get; set; }

        [JsonProperty("end")] public int End { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("tags")] public List<string> Tags { get; set; }
    }
}
