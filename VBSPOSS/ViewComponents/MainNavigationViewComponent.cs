using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.ViewComponents
{
    public class MainNavigationViewComponent : ViewComponent
    {
        private readonly ILogger<MainNavigationViewComponent> _logger;
        private ISessionHelper _sessionHelper;
        private readonly IAdministrationService _adminService;

        public MainNavigationViewComponent(ILogger<MainNavigationViewComponent> logger, ISessionHelper sessionHelper, IAdministrationService adminService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _adminService = adminService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {            
            try
            {
                
                if (User.Identity.IsAuthenticated)
                {
                    var _userName = User.Identity.Name;
                    var userInfor = _adminService.GetUserByUserName(_userName);
                    if (userInfor != null)
                    {
                        return await Task.FromResult((IViewComponentResult)View("_MainNavigation", userInfor));
                    }
                    else
                    {
                        ErrorViewModel error = new ErrorViewModel();
                        error.Message = "Không lấy được thông tin người dùng!";
                        return await Task.FromResult((IViewComponentResult)View("Error", error));
                    }
                }
                else
                {
                    ErrorViewModel error = new ErrorViewModel();
                    error.Message = "Bạn không có quyền truy cập hoặc phiên làm việc đã hết hạn!";
                    return await Task.FromResult((IViewComponentResult)View("Error", error));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                ErrorViewModel error = new ErrorViewModel();
                error.Message = e.Message;
                return await Task.FromResult((IViewComponentResult)View("Error", error));
            }
            
        }        
    }
}
