using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

namespace WeatherIs.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string ipString)
        {
            var unitsSettings = new { Auto = true, Type = 0 };
            if (Request.Cookies.TryGetValue("PreferredUnits", out var jsonUnits))
            {
                if (jsonUnits == null)
                {
                    _logger.LogWarning("Could not get preferred units cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                }
                
                if (jsonUnits != null) unitsSettings = JsonConvert.DeserializeAnonymousType(jsonUnits, unitsSettings);

                if (unitsSettings == null)
                {
                    _logger.LogWarning("Could not read preferred units cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                }
            }
            
            if (Request.Cookies.TryGetValue("PreferredLocation", out var json))
            {
                if (json == null)
                {
                    _logger.LogError("Could not get preferred location cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Error");
                }

                var item = JsonConvert.DeserializeObject<CityListItem>(json);

                if (item == null)
                {
                    _logger.LogError("Could not read preferred location cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Error");
                }

                try
                {
                    UnitsType unitType;
                    if (unitsSettings == null || unitsSettings.Auto)
                    {
                        var culture = new RegionInfo(item.Country);
                        unitType = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
                    }
                    else
                        unitType = (UnitsType)unitsSettings.Type;
                
                    using var weatherClient = new CurrentWeatherData(ConfigContext.Config.OpenWeatherMapApiKey);
                    var weather = await weatherClient.GetByCityIdAsync(int.Parse(item.Id.ToString(CultureInfo.InvariantCulture)),
                        unitType);

                    _logger.LogInformation("Successfully got the weather for IP '{IP}' using the stored cookie!", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Index", new HomeViewModel { WeatherData = weather, IsUsingAutoGeolocation = false, MetricUnits = unitType == UnitsType.Metric });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not read preferred location cookie!");
                    return View(new HomeViewModel());
                }
            }
            {
                var ip = string.IsNullOrEmpty(ipString)
                    ? Request.HttpContext.Connection.RemoteIpAddress
                    : IPAddress.Parse(ipString);

                if (ip.IsInternal() || ip == null)
                    return View(new HomeViewModel
                    {
                        ErrorMessage = "Oops! We could not your IP to automatically determinate your geolocation :(" +
                                       "\nYou might want to try to set your preferred location.",
                        WeatherData = new CurrentWeatherDataResponse()
                    });

                using var ipApiEndpoint = new IpApiEndpoint();
                var ipGeolocation = await ipApiEndpoint.GetIpGeolocationAsync(ip.ToString());

                UnitsType unitType;
                if (unitsSettings == null || unitsSettings.Auto)
                {
                    var culture = new RegionInfo(ipGeolocation.Country);
                    unitType = culture.IsMetric ? UnitsType.Metric : UnitsType.Imperial;
                }
                else
                    unitType = (UnitsType)unitsSettings.Type;
                
                using var weatherClient = new CurrentWeatherData(ConfigContext.Config.OpenWeatherMapApiKey);
                var weather = await weatherClient.GetByCoordsAsync(ipGeolocation.Latitude, ipGeolocation.Longitude,
                    unitType);

                _logger.LogInformation("Successfully got the weather for IP '{IP}'!", ip.ToString());
                
                return View("Index", new HomeViewModel { WeatherData = weather, IsUsingAutoGeolocation = true, MetricUnits = unitType == UnitsType.Metric });                
            }
        }

        public IActionResult Privacy()
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
