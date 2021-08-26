using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.Web.Models
{
    public class SettingsViewModel
    {
        public CityListItem StoredLocation { get; set; }

        public string Name =>
            StoredLocation == null 
                ? "None" 
                : $"{StoredLocation.Name}{(string.IsNullOrEmpty(StoredLocation.State) ? null : $", {StoredLocation.State}")}, {StoredLocation.Country}";


        public string Id { get; set; }
        
        public string UnitTypeName { get; set; }

        public string ErrorMessage { get; set; }
    }
}