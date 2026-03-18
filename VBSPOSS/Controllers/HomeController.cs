using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telerik.SvgIcons;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{    
    public class HomeController : BaseController
    {
        private IWebHostEnvironment _hostingEnvironment;
        private readonly INotiService _notiservice;

        public HomeController(ILogger<HomeController> logger,INotiService notiservice, IWebHostEnvironment hostingEnvironment, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        {
                _hostingEnvironment = hostingEnvironment;
                _notiservice = notiservice;
            } 

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Query["culture"]))
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(context.HttpContext.Request.Query["culture"]);
            }
            base.OnActionExecuting(context);
        }


        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }


        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Forbidden()
        {
            Response.StatusCode = 403; // Set HTTP status 403 Forbidden
            return View();
        } 
    }
}
