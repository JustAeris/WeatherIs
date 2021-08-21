using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherIs.IpApi.Models;

namespace WeatherIs.IpApi
{
    /// <summary>
    /// This class is used to interact with the <a href="https://ip-api.com">ip-api.com</a> free endpoint.<br/>
    /// Please note that the free endpoint is limited to 45 requests per minute.
    /// <remarks>Going over the free limit will throw an <see cref="HttpRequestException"/> (<see cref="HttpStatusCode.TooManyRequests"/>).</remarks>
    /// </summary>
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

        /// <summary>
        /// This methods return the geolocation of an IP using
        /// the free <a href="https://ip-api.com">ip-api.com</a> endpoint
        /// </summary>
        /// <param name="ip">The IP to get the geolocation of. Can be IPv4/IPv6 or a domain name.</param>
        /// <param name="fields">The fields the API must return. Can be calculated at the <i>Returned data</i> section
        /// in the <a href="https://ip-api.com/docs/api:json">docs</a>.</param>
        /// <returns>A response containing all of the basic elements.</returns>
        /// <exception cref="HttpRequestException">You made over 45 request in 1 minute (<see cref="HttpStatusCode.TooManyRequests"/>).</exception>
        /// <exception cref="ArgumentException">The given IP is in correct or not an IP.</exception>
        public async Task<IpApiResponse> GetIpGeolocationAsync(string ip, int fields = 61439)
        {
            var response = await Client.GetAsync($"{ip}?fields={fields}");

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