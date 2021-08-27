using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.OpenWeatherMapApi.Tests
{
    public class Tests
    {
        private CurrentWeatherData _currentWeatherClient;
        private OneCallApi _oneCallApiClient;
        private string _apiKey;

        [OneTimeSetUp]
        public void Setup()
        {
            var json = JObject.Parse(File.ReadAllText("appsettings.json"));

            _apiKey = (string) json["OpenWeatherMapApiKey"];
            _currentWeatherClient = new CurrentWeatherData(_apiKey);
            _oneCallApiClient = new OneCallApi(_apiKey);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _currentWeatherClient.Dispose();
        }

        [Test]
        public async Task ByCityName()
        {
            var amsterdam = await _currentWeatherClient.GetByCityNameAsync("amsterdam");
            var paris = await _currentWeatherClient.GetByCityNameAsync("paris", UnitsType.Metric, new CultureInfo("fr"));
            var london = await _currentWeatherClient.GetByCityNameAsync("london", UnitsType.Imperial);
            Assert.IsNotNull(amsterdam);
            Assert.IsNotNull(paris);
            Assert.IsNotNull(london);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetByCityNameAsync("this city does not exists"));
        }

        [Test]
        public async Task ByCityId()
        {
            var city1 = await _currentWeatherClient.GetByCityIdAsync(685445);
            var city2 = await _currentWeatherClient.GetByCityIdAsync(6535826, UnitsType.Metric, new CultureInfo("it"));
            var city3 = await _currentWeatherClient.GetByCityIdAsync(1798725, UnitsType.Imperial);
            Assert.IsNotNull(city1);
            Assert.IsNotNull(city2);
            Assert.IsNotNull(city3);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetByCityIdAsync(750149850));
        }

        [Test]
        public async Task ByCoords()
        {
            var city = await _currentWeatherClient.GetByCoordsAsync(48.864716, 2.349014);
            Assert.IsNotNull(city);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetByCoordsAsync(75014560, double.Epsilon));
        }

        [Test]
        public async Task ByZipCode()
        {
            var city = await _currentWeatherClient.GetByZipCodeAsync(75000, new RegionInfo("fr"));
            Assert.IsNotNull(city);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetByZipCodeAsync(75014560, new RegionInfo("fr")));
        }

        [Test]
        public async Task WithinARectangle()
        {
            var city = await _currentWeatherClient.GetWithinARectangleAsync(12, 32, 15, 37, 10);
            Assert.IsNotNull(city);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetWithinARectangleAsync(987, 465, 65, 645, 4));
        }

        [Test]
        public async Task WithinACircle()
        {
            var city = await _currentWeatherClient.GetWithinACircleAsync(12, 32, 15);
            Assert.IsNotNull(city);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await _currentWeatherClient.GetWithinACircleAsync(55, 37, 65));

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _currentWeatherClient.GetWithinACircleAsync(874, 211));
        }

        [Test]
        public async Task TestCityList()
        {
            await CityListRetriever.RetrieveCityList();

            Assert.IsTrue(CityListRetriever.CityList.Any());
        }

        [Test]
        public async Task OneCallApi()
        {
            if (CityListRetriever.CityList == null)
                await CityListRetriever.RetrieveCityList();

            var city = (CityListRetriever.CityList ?? throw new InvalidOperationException()).First(c => c.Name == "Paris" && c.Country == "FR");

            var forecast =
                await _oneCallApiClient.GetByCoordsAsync(city.Coords.Latitude, city.Coords.Longitude, UnitsType.Metric);

            Assert.IsNotNull(forecast);
        }
    }
}
