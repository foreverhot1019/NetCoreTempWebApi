using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NetCoreTemp.MVC.Models;
using NetCoreTemp.MVC.Models.Home;

namespace NetCoreTemp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IStringLocalizer<CommonLanguage.Language> _localizer1;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer, IStringLocalizer<CommonLanguage.Language> localizer1)
        {
            _logger = logger;
            _localizer = localizer;
            _localizer1 = localizer1;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var t = _localizer1.GetString("Create");
            ViewData["title"] = t;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var uiculture = System.Globalization.CultureInfo.CurrentUICulture;

            var _aa = new Test
            {
                a = 11,
                c = $"{_localizer["value from homecontroller"]}-123123123阿斯顿撒旦飞洒地方",
            };
            return View(_aa);
        }

        [HttpPost]
        public IActionResult Index(Test _aa)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var uiculture = System.Globalization.CultureInfo.CurrentUICulture;
            var s = _localizer.GetString("a");
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                TryValidateModel(_aa);
            }
            return View(_aa);
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
