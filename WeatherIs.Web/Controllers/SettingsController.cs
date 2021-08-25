using System;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi;
using WeatherIs.OpenWeatherMapApi.Models;
using WeatherIs.Web.Models;
using WeatherIs.Web.Models.Cookies;

namespace WeatherIs.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        // GET
        public IActionResult Index(string error = null)
        {
            if (CityListRetriever.CityList == null || !CityListRetriever.CityList.Any())
                CityListRetriever.RetrieveCityList();

            var dummy = Request.Cookies.TryParseCookie<CityListItem>("PreferredLocation", out var storedLocation);

            var _ = Request.Cookies.TryParseCookie<PreferredUnits>("PreferredUnits", out var unitsSettings);

            return View(new SettingsViewModel
            {
                StoredLocation = storedLocation, ErrorMessage = error,
                UnitTypeName = unitsSettings != null
                    ? unitsSettings.Automatic ? "Automatic" : Enum.GetName(unitsSettings.Type)
                    : "Automatic"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetPreferredLocation([Bind("Id,Name,Country")] SettingsViewModel item)
        {
            var city = CityListRetriever.CityList.FirstOrDefault(c =>
                c.Id.ToString(CultureInfo.InvariantCulture) == item.Id);

            if (city == null)
            {
                var encoded = HtmlEncoder.Default.Encode("Could not retrieve the city, make sure to click on your city in the list.");
                return Redirect($"/Settings?error={encoded}");
            }

            if (Request.Cookies.ContainsKey("PreferredLocation"))
                Response.Cookies.Delete("PreferredLocation");
            Response.Cookies.Append("PreferredLocation", JsonConvert.SerializeObject(city));

            return Redirect("/Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePreferredLocation()
        {
            if (Request.Cookies.ContainsKey("PreferredLocation"))
                Response.Cookies.Delete("PreferredLocation");

            return Redirect("/Settings");
        }

        [HttpGet]
        public ActionResult Search(string term)
        {
            return Json(CityListRetriever.CityList
                .Where(i => i.Name.ToLower().StartsWith(term.ToLower())).Distinct(new CityListItemEqualityComparer())
                .Select(i => new
                {
                    label = $"{i.Name}{(string.IsNullOrEmpty(i.State) ? null : $", {i.State}")}, {i.Country}",
                    id = i.Id,
                    country = i.Country
                }));
        }

        [HttpGet]
        public IActionResult SetPreferredUnits(int type)
        {
            var unit = (UnitsType)type;

            if (unit == UnitsType.Standard)
            {
                var encoded = HtmlEncoder.Default.Encode("Could not retrieve the desired units type, please retry.");
                return Redirect($"/Settings?error={encoded}");
            }

            if (Request.Cookies.ContainsKey("PreferredUnits"))
                Response.Cookies.Delete("PreferredUnits");
            Response.Cookies.Append("PreferredUnits", JsonConvert.SerializeObject(new PreferredUnits{ Automatic = type == 3, Type = unit }));

            return Redirect("/Settings");
        }
    }
}
