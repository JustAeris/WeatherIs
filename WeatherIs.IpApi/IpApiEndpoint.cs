using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherIs.IpApi.Models;

namespace WeatherIs.IpApi
{
    public class IpApiEndpoint : IDisposable
    {
        private const string ApiUri = "http://ip-api.com/json/";

        public IpApiEndpoint()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(ApiUri)
            };

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private HttpClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
        }

        public async Task<IpApiResponse> GetIpGeolocationAsync(string ip)
        {
            var response = await Client.GetAsync(ip);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new HttpRequestException(
                    "You are rate-limited, the free endpoint accepts only 45 request max per minute!", null,
                    response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<IpApiResponse>(content);

            if (json == null || json.Status == "fail")
                throw new ArgumentException($"Could not get IP geolocation for '{ip}'", nameof(ip));

            return json;
        }
    }
}