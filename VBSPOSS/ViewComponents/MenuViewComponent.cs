using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VBSPOSS.Data.Models;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Services.Interfaces ;

namespace VBSPOSS.ViewComponents
{
    public class MenuViewComponent: ViewComponent
    {

        private readonly ILogger<MenuViewComponent> _logger;
        private ISessionHelper _sessionHelper;
        private readonly IAdministrationService _adminService;

        public MenuViewComponent(ILogger<MenuViewComponent> logger, ISessionHelper sessionHelper, IAdministrationService adminService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _adminService = adminService;
        }
       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                
                if (_sessionHelper.IsUserAuthenticated())
                {
                    var _currentRole = _sessionHelper.GetUserRole();
                    var _lstMenu = _adminService.GetMenuByRole(_currentRole);

                    if (_lstMenu != null && _lstMenu.Count > 0)
                    {
                        try
                        {
                            // Xu ly ghi nho cac menu duoc selected
                            string action = string.Empty;//ControllerContext.RouteData.Values["action"].ToString();
                            string controller = string.Empty; // ControllerContext.RouteData.Values["controller"].ToString();

                            foreach (var item in _lstMenu)
                            {
                                if (item.Controller == controller && item.Action == action)
                                {
                                    ResetSelected(_lstMenu);
                                    item.IsSelect = true;
                                    SetParentIsSelect(item, _lstMenu);
                                    break;
                                }
                            }
                        }
                        catch (NullReferenceException e)
                        {
                            _logger.LogError(e.Message);
                        }
                        return await Task.FromResult((IViewComponentResult)View("_Menu", _lstMenu));
                    }
                    else
                    {
                        return await Task.FromResult((IViewComponentResult)View("Error"));
                    }
                }
                else
                {
                    return await Task.FromResult((IViewComponentResult)View("Error"));
                }
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                return await Task.FromResult((IViewComponentResult)View("Error"));
            }
            
            //return await Task.FromResult((IViewComponentResult)View("AdminLTE/_MainNavigation"));
        }


        // reset isSelected
        public void ResetSelected(List<MenuRoleView> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items.ElementAt(i).IsSelect = false;
            }
        }

        // set parent is selected
        public void SetParentIsSelect(MenuRoleView child, List<MenuRoleView> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                int id = items.ElementAt(i).Id;
                if (id == child.ParentId)
                {
                    items.ElementAt(i).IsSelect = true;
                    if (items.ElementAt(i).ParentId == 0)
                        return;
                    else
                        SetParentIsSelect(items.ElementAt(i), items);
                }
                else
                {
                    items.ElementAt(i).IsSelect = false;
                }
            }
        }
    }
}
