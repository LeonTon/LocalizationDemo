using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Localization.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Localization.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer _localizer;


        public HomeController(IStringLocalizerFactory localizerFactory)
        {
            _localizer = localizerFactory.Create("SharedResource", "Localization");
        }

        // GET: /<controller>/
        public IActionResult Index(string cultures)
        {
            if (!string.IsNullOrWhiteSpace(cultures))
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(cultures);

            }
            return Content(_localizer["Greeting"]);
        }
    }
}
