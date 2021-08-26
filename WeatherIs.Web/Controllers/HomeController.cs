using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherIs.IpApi;
using WeatherIs.OpenWeatherMapApi;
using WeatherIs.OpenWeatherMapApi.Models;
using WeatherIs.Web.Configuration;
using WeatherIs.Web.Models;
using WeatherIs.Web.Models.Cookies;

namespace WeatherIs.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(bool forceRefresh = false, string ipString = null)
        {
            if (!Request.Cookies.TryParseCookie<PreferredUnits>("PreferredUnits", out var unitsSettings))
            {
                _logger.LogWarning("Could not get preferred units cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
            }

            if (!Request.Cookies.TryParseCookie<CityListItem>("PreferredLocation", out var preferredLocation))
            {
                var ip = string.IsNullOrEmpty(ipString)
                    ? Request.HttpContext.Connection.RemoteIpAddress
                    : IPAddress.Parse(ipString);

                if (ip == null || ip.IsInternal())
                    return View(new HomeViewModel
                    {
                        ErrorMessage = "Oops! We could not your IP to automatically determinate your geolocation :(" +
                                       "\nYou might want to try to set your preferred location.",
                        WeatherData = new CurrentWeatherDataResponse()
                    });

                if (Request.Cookies.TryParseCookie<WeatherCache>("WeatherCache", out var ipCache))
                {
                    if (ipCache.ExpiryDate > DateTime.UtcNow && ip.ToString() == ipCache.IpAddress && !forceRefresh)
                    {
                        _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                        return View("Index",
                            new HomeViewModel
                            {
                                WeatherData = ipCache.WeatherData, MetricUnits = ipCache.MetricUnits,
                                IsUsingAutoGeolocation = true
                            });
                    }
                }

                using var ipApiEndpoint = new IpApiEndpoint();
                var ipGeolocation = await ipApiEndpoint.GetIpGeolocationAsync(ip.ToString());

                UnitsType unitTypeByIp;
                if (unitsSettings == null || unitsSettings.Automatic)
                {
                    var culture = new RegionInfo(ipGeolocation.CountryCode);
                    unitTypeByIp = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
                }
                else
                    unitTypeByIp = unitsSettings.Type;

                using var weatherClientByIp = new CurrentWeatherData(ConfigContext.Config.OpenWeatherMapApiKey);
                var weatherByIp = await weatherClientByIp.GetByCoordsAsync(ipGeolocation.Latitude, ipGeolocation.Longitude,
                    unitTypeByIp);

                _logger.LogInformation("Successfully got the weather for IP '{IP}'!", ip.ToString());

                var newIpCache = new WeatherCache
                {
                    IpAddress = ip.ToString(),
                    ExpiryDate = DateTime.UtcNow + TimeSpan.FromMinutes(10),
                    WeatherData = weatherByIp,
                    MetricUnits = unitTypeByIp == UnitsType.Metric
                };

                if (Request.Cookies.ContainsKey("WeatherCache"))
                    Response.Cookies.Delete("WeatherCache");
                Response.Cookies.Append("WeatherCache", JsonConvert.SerializeObject(newIpCache));

                return View("Index", new HomeViewModel { WeatherData = weatherByIp, IsUsingAutoGeolocation = true, MetricUnits = unitTypeByIp == UnitsType.Metric });
            }

            if (Request.Cookies.TryParseCookie<WeatherCache>("WeatherCache", out var cache))
            {
                if (cache.ExpiryDate > DateTime.UtcNow && Math.Abs(cache.CityId - preferredLocation.Id) < 0.1 && !forceRefresh)
                {
                    _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Index",
                        new HomeViewModel
                        {
                            WeatherData = cache.WeatherData, MetricUnits = cache.MetricUnits,
                            IsUsingAutoGeolocation = false
                        });
                }
            }

            UnitsType unitType;
            if (unitsSettings == null || unitsSettings.Automatic)
            {
                var culture = new RegionInfo(preferredLocation.Country);
                unitType = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
            }
            else
                unitType = unitsSettings.Type;

            using var weatherClient = new CurrentWeatherData(ConfigContext.Config.OpenWeatherMapApiKey);
            var weather = await weatherClient.GetByCityIdAsync((int) preferredLocation.Id, unitType);

            _logger.LogInformation("Successfully got the weather for city ID '{CityID}'!", preferredLocation.Id);

            var newCache = new WeatherCache
            {
                CityId = preferredLocation.Id,
                ExpiryDate = DateTime.UtcNow + TimeSpan.FromMinutes(10),
                WeatherData = weather,
                MetricUnits = unitType == UnitsType.Metric
            };

            if (Request.Cookies.ContainsKey("WeatherCache"))
                Response.Cookies.Delete("WeatherCache");
            Response.Cookies.Append("WeatherCache", JsonConvert.SerializeObject(newCache));

            return View("Index", new HomeViewModel { WeatherData = weather, IsUsingAutoGeolocation = false, MetricUnits = unitType == UnitsType.Metric });
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
