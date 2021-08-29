using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.OpenWeatherMapApi
{
    public class OneCallApi : IDisposable
    {
        private const string ApiUri = "https://api.openweathermap.org/data/2.5/onecall";

        public OneCallApi(string apiKey)
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

        public void Dispose()
        {
            Client.Dispose();
        }

        public async Task<OneCallApiResponse> GetByCoordsAsync(double lat, double lon, Exclude exclude = Exclude.None,
            UnitsType unitsType = UnitsType.Standard, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            var excludeOptions = exclude.ToString().ToLower().Replace(" ", "");

            var parameters =
                $"?lat={lat}&lon={lon}&appid={ApiKey}&units={Enum.GetName(unitsType)?.ToLower()}&lang={culture.TwoLetterISOLanguageName}{(exclude == Exclude.None ? null : $"&exclude={excludeOptions}")}";

            var response = await Client.GetAsync(parameters);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Could not get weather forecast data for coords {lat},{lon}", null,
                    response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<OneCallApiResponse>(content);
        }
    }
}
