using System;
using System.Collections.Generic;
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

    public class CityListItemEqualityComparer : IEqualityComparer<CityListItem>
    {
        public bool Equals(CityListItem x, CityListItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && x.State == y.State && x.Country == y.Country;
        }

        public int GetHashCode(CityListItem obj)
        {
            return HashCode.Combine(obj.Name, obj.State, obj.Country);
        }
    }
}