﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi;
using WeatherIs.OpenWeatherMapApi.Models;
using WeatherIs.Web.Models;

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

            CityListItem item = null;

            if (Request.Cookies.TryGetValue("PreferredLocation", out var jsonLocation))
            {
                if (jsonLocation == null)
                {
                    _logger.LogWarning("Could not get preferred location cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                }

                if (jsonLocation != null) item = JsonConvert.DeserializeObject<CityListItem>(jsonLocation);

                if (item == null)
                {
                    _logger.LogWarning("Could not read preferred location cookie for IP '{IP}'", Request.HttpContext.Connection.RemoteIpAddress);
                }
            }

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

            return View(new SettingsViewModel
            {
                StoredLocation = item, ErrorMessage = error,
                UnitTypeName = unitsSettings != null
                    ? unitsSettings.Auto ? "Automatic" : Enum.GetName((UnitsType)unitsSettings.Type)
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
            Response.Cookies.Append("PreferredUnits", JsonConvert.SerializeObject(new { Auto = type == 3, Type = unit }));

            return Redirect("/Settings");
        }
    }
}
