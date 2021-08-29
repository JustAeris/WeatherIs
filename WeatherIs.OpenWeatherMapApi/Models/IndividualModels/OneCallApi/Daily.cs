using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherIs.OpenWeatherMapApi.Models.IndividualModels.OneCallApi
{
    public class Daily
    {
        [JsonProperty("dt")] public long Dt { get; set; }

        [JsonProperty("sunrise")] public long Sunrise { get; set; }

        [JsonProperty("sunset")] public long Sunset { get; set; }

        [JsonProperty("moonrise")] public long Moonrise { get; set; }

        [JsonProperty("moonset")] public long Moonset { get; set; }

        [JsonProperty("moon_phase")] public double MoonPhase { get; set; }

        [JsonProperty("temp")] public Temp Temp { get; set; }

        [JsonProperty("feels_like")] public FeelsLike FeelsLike { get; set; }

        [JsonProperty("pressure")] public int Pressure { get; set; }

        [JsonProperty("humidity")] public int Humidity { get; set; }

        [JsonProperty("dew_point")] public double DewPoint { get; set; }

        [JsonProperty("wind_speed")] public double WindSpeed { get; set; }

        [JsonProperty("wind_deg")] public int WindDeg { get; set; }

        [JsonProperty("weather")] public List<Weather> Weather { get; set; }

        [JsonProperty("clouds")] public float Clouds { get; set; }

        [JsonProperty("pop")] public double Pop { get; set; }

        [JsonProperty("rain")] public double Rain { get; set; }

        [JsonProperty("uvi")] public double Uvi { get; set; }
    }
}
