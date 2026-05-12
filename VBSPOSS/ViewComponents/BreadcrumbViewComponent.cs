using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.ViewComponents
{
    public class BreadcrumbViewComponent
       : ViewComponent
    {
        private readonly IAdministrationService _menuService;

        public BreadcrumbViewComponent(
            IAdministrationService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult>
            InvokeAsync()
        {
            var controller =
                RouteData.Values["controller"]
                    ?.ToString();

            var action =
                RouteData.Values["action"]
                    ?.ToString();

            var breadcrumb =
                _menuService.GetBreadcrumb(
                    controller,
                    action);

            return View(breadcrumb);
        }
    }
}
