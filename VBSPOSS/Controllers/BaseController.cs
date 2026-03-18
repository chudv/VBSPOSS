using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    public class BaseController : Controller
    {
        private int MenuId { get; set; }
        private string ActionName { get; set; }
        private string ControllerName { get; set; }

        private readonly ILogger<BaseController> _logger;
        protected readonly IAdministrationService _administrationService;
        private readonly ISessionHelper _sessionHelper;

        public BaseController(ILogger<BaseController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper    )
        {
            _logger = logger;
            _administrationService = administrationService;
            _sessionHelper = sessionHelper;
        }

        public void WriteLog(int type, string message)
        {
            switch (type)
            {
                case LogType.INFOR:
                    _logger.LogInformation(message);
                    break;
                case LogType.ERROR:
                    _logger.LogError(message);
                    break;
            }
        }

        protected int UserGrade
        {
            get
            {
                return _sessionHelper.GetUserGrade();
            }
        }

        protected void SetPermitData(string actionName, string controllerName)
        {
            ActionName = actionName;
            ControllerName = controllerName;
        }

        protected void SetPermitData(string actionName, string controllerName, int menuId)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            MenuId = menuId;
        }

        protected string UserName
        {
            get
            {
                var userName = string.Empty;
                if (User.Identity.IsAuthenticated)
                {
                    userName = User.Identity.Name;
                }
                return userName;
            }
        }

        protected string UserPosCode
        {
            get
            {
                //var userPosCode = string.Empty;
                //if (User.Identity.IsAuthenticated)
                //{
                //    userPosCode = "000100";
                //}
                //return userPosCode;
                return _sessionHelper.GetUserPosCode();
            }
        }

        //protected string GroupCode
        //{
        //    get
        //    {
        //        var groupCode = string.Empty;
        //        if (User.Identity.IsAuthenticated)
        //        {
        //            var _userGroup = _administrationService.GetGroupOfUser(UserName);
        //            if (_userGroup != null)
        //            {
        //                groupCode = _userGroup.RoleCode;
        //            }
        //        }
        //        return groupCode;
        //    }
        //}

        //protected int UserPermit
        //{
        //    get
        //    {
        //        return _administrationService.GetPermitOrUserGroup(GroupCode, ActionName, ControllerName, MenuId);
        //    }
        //}

        protected string UserRole
        {
            get
            {
                return _sessionHelper.GetUserRole();
            }
        }

        //protected int GetUserGrade
        //{
        //    get
        //    {
        //        return _sessionHelper.GetUserGrade();
        //    }
        //}

        protected RolePermissionModel UserPermission
        {
            get
            {
                return _sessionHelper.GetRolePermission(ControllerName, ActionName);
            }
        }
    }
}
