using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.OpenWeatherMapApi
{
    public class CurrentWeatherData : IDisposable
    {
        private const string ApiUri = "https://api.openweathermap.org/data/2.5/";

        public CurrentWeatherData(string apiKey)
        {
            ApiKey = apiKey;

            Client = new HttpClient
            {
                BaseAddress = new Uri(ApiUri)
            };

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string ApiKey { get; }
        private HttpClient Client { get; }

        public async Task<CurrentWeatherDataResponse> GetByCityNameAsync(string cityName, UnitsType unitsType = UnitsType.Standard, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            var parameters = $"weather?q={cityName}&appid={ApiKey}&units={Enum.GetName(unitsType)?.ToLower()}&lang={culture.TwoLetterISOLanguageName}";

            var response = await Client.GetAsync(parameters);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Could not get weather data for {cityName}", null, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrentWeatherDataResponse>(content);
        }
        
        public async Task<CurrentWeatherDataResponse> GetByCityIdAsync(int cityId, UnitsType unitsType = UnitsType.Standard, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            var parameters = $"weather?id={cityId}&appid={ApiKey}&units={Enum.GetName(unitsType)?.ToLower()}&lang={culture.TwoLetterISOLanguageName}";

            var response = await Client.GetAsync(parameters);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Could not get weather data for ID {cityId}", null, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrentWeatherDataResponse>(content);
        }
        
        public async Task<CurrentWeatherDataResponse> GetByCoordsAsync(double lat, double lon, UnitsType unitsType = UnitsType.Standard, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            var parameters = $"weather?lat={lat}&lon={lon}&appid={ApiKey}&units={Enum.GetName(unitsType)?.ToLower()}&lang={culture.TwoLetterISOLanguageName}";

            var response = await Client.GetAsync(parameters);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Could not get weather data for coords {lat},{lon}", null, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrentWeatherDataResponse>(content);
        }
        
        public async Task<CurrentWeatherDataResponse> GetByZipCodeAsync(int zipCode, RegionInfo country, UnitsType unitsType = UnitsType.Standard, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            var parameters = $"weather?zip={zipCode},{country.TwoLetterISORegionName.ToUpper()}&appid={ApiKey}&units={Enum.GetName(unitsType)?.ToLower()}&lang={culture.TwoLetterISOLanguageName}";

            var response = await Client.GetAsync(parameters);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Could not get weather data for ZIP code {zipCode} in country {country.TwoLetterISORegionName}", null, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CurrentWeatherDataResponse>(content);
        }
        
        public void Dispose()
        {
            Client.Dispose();
        }
    }
}