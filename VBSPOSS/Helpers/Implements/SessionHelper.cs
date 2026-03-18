using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Implements.Helpers
{
    public class SessionHelper : ISessionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IAdministrationService _adminService;
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private readonly ILogger<SessionHelper> _logger;

        private readonly IMapper _mapper;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public SessionHelper(IHttpContextAccessor httpContextAccessor, ILogger<SessionHelper> logger, IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }

        public bool IsUserAuthenticated()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated ?? false;
        }

        private bool SetUserInfo()
        {
            try
            {
                var _userInfo = GetUserInfor(GetUserName());
                if (_userInfo != null)
                {

                    var mobile = _userInfo.Mobile;
                    _session.SetString("USER_PHONE", mobile ?? "");

                    var posCode = _userInfo.PosCode;
                    _session.SetString("POS_CODE", posCode ?? "");

                    var userFullName = _userInfo.FullName;
                    _session.SetString("FULL_NAME", userFullName ?? "");


                    var userGrade = _userInfo.Grade;
                    _session.SetInt32("USER_GRADE", userGrade);

                    var userRole = _userInfo.DefaultRole;
                    _session.SetString("USER_ROLE", userRole ?? "");

                    var sessionMark = Guid.NewGuid().ToString();
                    _session.SetString("SESSION_MARK", sessionMark);

                    return true;

                }
                else
                {
                    return false;
                }


            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public UserModel GetUserInfor(string userName)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // Tạo scope mới
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Lấy DbContext từ scope
                var _userInfo = _dbContext.UserViews.Where(w => w.UserName == userName).FirstOrDefault();
                return _mapper.Map<UserModel>(_userInfo);
            }
        }

        public string GetUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.Name;

        }

        public string GetUserPosCode()
        {
            var _grade = GetUserGrade();
            if (_grade == PosGrade.HEAD_POS)
            {
                return PosValue.HEAD_POS;
            }
            return GetSessionStringValue("POS_CODE");
        }

        public string GetUserFullName()
        {
            return GetSessionStringValue("FULL_NAME");
        }

        public string GetUserRole()
        {
            return GetSessionStringValue("USER_ROLE");
        }

        public string GetUserMobile()
        {
            return GetSessionStringValue("USER_PHONE");
        }

        public int GetUserGrade()
        {
            return GetSessionIntValue("USER_GRADE");
        }

        public string GetSessionStringValue(string tagName)
        {
            if (IsUserAuthenticated())
            {
                string data = _session.GetString($"{tagName}");
                if (string.IsNullOrEmpty(data))
                {
                    if (SetUserInfo())
                        return _session.GetString($"{tagName}");
                    else
                        return string.Empty;
                }
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        public int GetSessionIntValue(string tagName)
        {
            if (IsUserAuthenticated())
            {
                var data = _session.GetInt32($"{tagName}");
                if (data == null)
                {
                    if (SetUserInfo())
                        return _session.GetInt32($"{tagName}").Value;
                    else
                        return 0;
                }
                return data.Value;
            }
            else
            {
                return 0;
            }
        }

        public RolePermissionModel GetRolePermission(int menuId)
        {
            string userRole = GetUserRole();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _adminService = scope.ServiceProvider.GetRequiredService<IAdministrationService>();
                return _adminService.GetRolePermissionByMenuId(userRole, menuId);
            }
        }

        public RolePermissionModel GetRolePermission(string controller, string action)
        {
            string userRole = GetUserRole();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _adminService = scope.ServiceProvider.GetRequiredService<IAdministrationService>();
                return _adminService.GetRolePermissionByMenuId(userRole, controller, action);
            }
        }
    }
}
