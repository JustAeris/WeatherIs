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
using WeatherIs.Web.Models.Cookies;

namespace WeatherIs.Web.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ILogger<ForecastController> _logger;

        public ForecastController(ILogger<ForecastController> logger)
        {
            _logger = logger;
        }

        // GET
        public async Task<IActionResult> Index(bool forceRefresh, string ipString)
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
                    return View(new ForecastViewModel
                    {
                        ErrorMessage = "Oops! We could not your IP to automatically determinate your geolocation :(" +
                                       "\nYou might want to try to set your preferred location.",
                        ForecastData = new OneCallApiResponse()
                    });

                if (Request.Cookies.TryParseCookie<ForecastCache>("WeatherCache", out var ipCache))
                {
                    if (ipCache.ExpiryDate > DateTime.UtcNow && ip.ToString() == ipCache.IpAddress && !forceRefresh)
                    {
                        _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                        return View("Index",
                            new ForecastViewModel
                            {
                                ForecastData = ipCache.ForecastData, MetricUnits = ipCache.MetricUnits,
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

                using var forecastClientByIp = new OneCallApi(ConfigContext.Config.OpenWeatherMapApiKey);
                var forecastByIp = await forecastClientByIp.GetByCoordsAsync(ipGeolocation.Latitude, ipGeolocation.Longitude,
                    unitTypeByIp);

                _logger.LogInformation("Successfully got the weather for IP '{IP}'!", ip.ToString());

                var newIpCache = new ForecastCache
                {
                    IpAddress = ip.ToString(),
                    ExpiryDate = DateTime.UtcNow + TimeSpan.FromMinutes(10),
                    ForecastData = forecastByIp,
                    MetricUnits = unitTypeByIp == UnitsType.Metric
                };

                if (Request.Cookies.ContainsKey("ForecastCache"))
                    Response.Cookies.Delete("ForecastCache");
                Response.Cookies.Append("ForecastCache", JsonConvert.SerializeObject(newIpCache));

                return View("Index", new ForecastViewModel { ForecastData = forecastByIp, IsUsingAutoGeolocation = true, MetricUnits = unitTypeByIp == UnitsType.Metric });
            }

            if (Request.Cookies.TryParseCookie<ForecastCache>("WeatherCache", out var cache))
            {
                if (cache.ExpiryDate > DateTime.UtcNow && Math.Abs(cache.CityId - preferredLocation.Id) < 0.1 && !forceRefresh)
                {
                    _logger.LogInformation("Returned cache for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                    return View("Index",
                        new ForecastViewModel
                        {
                            ForecastData = cache.ForecastData, MetricUnits = cache.MetricUnits,
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
            var forecast = await forecastClient.GetByCoordsAsync(city.Coords.Latitude, city.Coords.Longitude, unitType);

            _logger.LogInformation("Successfully got the weather for city ID '{CityID}'!", preferredLocation.Id);

            var newCache = new ForecastCache
            {
                CityId = preferredLocation.Id,
                ExpiryDate = (DateTime.UtcNow + TimeSpan.FromDays(1)).Date,
                ForecastData = forecast,
                MetricUnits = unitType == UnitsType.Metric
            };

            if (Request.Cookies.ContainsKey("WeatherCache"))
                Response.Cookies.Delete("WeatherCache");
            Response.Cookies.Append("WeatherCache", JsonConvert.SerializeObject(newCache));

            return View("Index", new ForecastViewModel { ForecastData = forecast, IsUsingAutoGeolocation = false, MetricUnits = unitType == UnitsType.Metric });
        }
    }
}
