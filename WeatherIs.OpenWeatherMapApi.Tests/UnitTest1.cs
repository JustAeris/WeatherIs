using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.OpenWeatherMapApi.Tests
{
    public class Tests
    {
        private CurrentWeatherData _client;
        private string _apiKey;
        
        [OneTimeSetUp]
        public void Setup()
        {
            if (!File.Exists("appsettings.json"))
                throw new FileNotFoundException("Settings file has not been found", "appsettings.json");

            var json = JObject.Parse(File.ReadAllText("appsettings.json"));

            _apiKey = (string) json["OpenWeatherMapApiKey"];
            _client = new CurrentWeatherData(_apiKey);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        [Test]
        public async Task ByCityName()
        {
            var amsterdam = await _client.GetByCityNameAsync("amsterdam");
            var paris = await _client.GetByCityNameAsync("paris", UnitsType.Metric, new CultureInfo("fr"));
            var london = await _client.GetByCityNameAsync("london", UnitsType.Imperial);
            Assert.IsNotNull(amsterdam);
            Assert.IsNotNull(paris);
            Assert.IsNotNull(london);
            
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetByCityNameAsync("this city does not exists"));
        }
        
        [Test]
        public async Task ByCityId()
        {
            var city1 = await _client.GetByCityIdAsync(685445);
            var city2 = await _client.GetByCityIdAsync(6535826, UnitsType.Metric, new CultureInfo("it"));
            var city3 = await _client.GetByCityIdAsync(1798725, UnitsType.Imperial);
            Assert.IsNotNull(city1);
            Assert.IsNotNull(city2);
            Assert.IsNotNull(city3);
            
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetByCityIdAsync(750149850));
        }
        
        [Test]
        public async Task ByCoords()
        {
            var city = await _client.GetByCoordsAsync(48.864716, 2.349014);
            Assert.IsNotNull(city);
            
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetByCoordsAsync(75014560, double.Epsilon));
        }
        
        [Test]
        public async Task ByZipCode()
        {
            var city = await _client.GetByZipCodeAsync(75000, new RegionInfo("fr"));
            Assert.IsNotNull(city);
            
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetByZipCodeAsync(75014560, new RegionInfo("fr")));
        }
    }
}