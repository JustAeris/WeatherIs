using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WeatherIs.IpApi;
using WeatherIs.OpenWeatherMapApi;
using WeatherIs.OpenWeatherMapApi.Models;
using WeatherIs.Web.Configuration;
using WeatherIs.Web.Models;
using WeatherIs.Web.Models.Cookies;
using static WeatherIs.Web.Utils.Knn;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace WeatherIs.Web.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ILogger<ForecastController> _logger;
        private readonly IMemoryCache _cache;

        public ForecastController(ILogger<ForecastController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        // GET
        public async Task<IActionResult> Index(bool forceRefresh = false, string ipString = null)
        {
            if (!Request.Cookies.TryParseCookie<PreferredUnits>("PreferredUnits", out var unitsSettings))
            {
                _logger.LogInformation("Could not get preferred units cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
            }

            if (!Request.Cookies.TryParseCookie<CityListItem>("PreferredLocation", out var preferredLocation))
            {
                var ip = string.IsNullOrEmpty(ipString)
                    ? Request.HttpContext.Connection.RemoteIpAddress
                    : IPAddress.Parse(ipString);

                if (ip == null || ip.IsInternal())
                    return View(new ForecastViewModel
                    {
                        ErrorMessage = "Oops! We could not your IP to automatically determinate your geolocation :(" +
                                       "\nYou might want to try to set your preferred location.",
                        ForecastData = new OneCallApiResponse()
                    });

                var cityList = CityListRetriever.CityList ?? await CityListRetriever.RetrieveCityList();

                using var ipApiEndpoint = new IpApiEndpoint();
                var ipGeolocation = await ipApiEndpoint.GetIpGeolocationAsync(ip.ToString());

                var points = cityList.Select(c => new PointF(c.Coords.Longitude, c.Coords.Latitude)).ToList();
                var kdTree = new KDTree();
                kdTree.BuildKDTree(points);
                var point = kdTree.NearestPoint(new PointF(ipGeolocation.Longitude, ipGeolocation.Latitude));
                var closest = cityList.First(c => c.Coords.Longitude == point.X && c.Coords.Latitude == point.Y);

                if (!forceRefresh && _cache.TryParseCache<ForecastCache>(CacheKeys.Forecast + closest.Id, out var ipCache))
                {
                    _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Index",
                        new ForecastViewModel
                        {
                            ForecastData = ipCache.ForecastData, MetricUnits = ipCache.MetricUnits,
                            IsUsingAutoGeolocation = true
                        });
                }

                UnitsType unitTypeByIp;
                if (unitsSettings == null || unitsSettings.Automatic)
                {
                    var culture = new RegionInfo(ipGeolocation.CountryCode);
                    unitTypeByIp = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
                }
                else
                    unitTypeByIp = unitsSettings.Type;

                using var forecastClientByIp = new OneCallApi(ConfigContext.Config.OpenWeatherMapApiKey);
                var forecastByIp = await forecastClientByIp.GetByCoordsAsync(ipGeolocation.Latitude, ipGeolocation.Longitude,
                    Exclude.Minutely, unitTypeByIp);

                _logger.LogInformation("Successfully got the weather for IP '{IP}'!", ip.ToString());

                var newIpCache = new ForecastCache
                {
                    IpAddress = ip.ToString(),
                    ExpiryDate = DateTime.UtcNow + TimeSpan.FromMinutes(10),
                    ForecastData = forecastByIp,
                    MetricUnits = unitTypeByIp == UnitsType.Metric
                };

                _cache.Set(CacheKeys.Forecast + closest.Id, newIpCache, (DateTime.UtcNow + TimeSpan.FromDays(1)).Date);

                return View("Index", new ForecastViewModel { ForecastData = forecastByIp, IsUsingAutoGeolocation = true, MetricUnits = unitTypeByIp == UnitsType.Metric });
            }

            if (!forceRefresh && _cache.TryParseCache<ForecastCache>(CacheKeys.Forecast + preferredLocation.Id, out var cache))
            {
                _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                return View("Index",
                    new ForecastViewModel
                    {
                        ForecastData = cache.ForecastData, MetricUnits = cache.MetricUnits,
                        IsUsingAutoGeolocation = false
                    });
            }

            UnitsType unitType;
            if (unitsSettings == null || unitsSettings.Automatic)
            {
                var culture = new RegionInfo(preferredLocation.Country);
                unitType = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
            }
            else
                unitType = unitsSettings.Type;

            if (CityListRetriever.CityList == null)
                await CityListRetriever.RetrieveCityList();

            Debug.Assert(CityListRetriever.CityList != null, "Is initialized above");
            var city = CityListRetriever.CityList.FirstOrDefault(c => Math.Abs(preferredLocation.Id - c.Id) < 0.1);

            if (city == null)
            {
                _logger.LogError("Could not retrieve the city for ID '{CityID}'", preferredLocation.Id);
                return View(new ForecastViewModel
                {
                    ErrorMessage = "Could not retrieve the forecast right now. Try changing your preferred location.",
                    ForecastData = new OneCallApiResponse()
                });
            }

            using var forecastClient = new OneCallApi(ConfigContext.Config.OpenWeatherMapApiKey);
            var forecast = await forecastClient.GetByCoordsAsync(city.Coords.Latitude, city.Coords.Longitude,
                Exclude.Minutely, unitType);

            _logger.LogInformation("Successfully got the weather for city ID '{CityID}'!", preferredLocation.Id);

            var newCache = new ForecastCache
            {
                CityId = preferredLocation.Id,
                ExpiryDate = (DateTime.UtcNow + TimeSpan.FromDays(1)).Date,
                ForecastData = forecast,
                MetricUnits = unitType == UnitsType.Metric
            };

            _cache.Set(CacheKeys.Forecast + preferredLocation.Id, newCache, (DateTime.UtcNow + TimeSpan.FromDays(1)).Date);

            return View("Index", new ForecastViewModel { ForecastData = forecast, IsUsingAutoGeolocation = false, MetricUnits = unitType == UnitsType.Metric });
        }
    }
}
