using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Models;

namespace VBSPOSS.Filters
{
    public class AuthorizeFilter : Attribute, IAuthorizationFilter
    {
        //private readonly IServiceScopeFactory _serviceScopeFactory;
        //private readonly ILogger<SessionHelper> _logger;
        //private readonly IMapper _mapper;

        //public AuthorizeFilter(ILogger<AuthorizeFilter> logger, IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        //{
            
        //    _serviceScopeFactory = serviceScopeFactory;
        //    _mapper = mapper;
        //    _logger = logger;
        //}

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<AuthorizeFilter>)) as ILogger<AuthorizeFilter>;

            var user = context.HttpContext.User;
            var httpMethod = context.HttpContext.Request.Method.ToUpper();

            // Lấy tên Controller và Action
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            if (controllerName?.ToLower() == "account" && actionName?.ToLower() == "login")
            {
                logger?.LogInformation($"Bỏ qua kiểm tra cho Account/Login");
                return;
            }
            if (controllerName?.ToLower() == "account" && actionName?.ToLower() == "logout")
            {
                logger?.LogInformation($"Bỏ qua kiểm tra cho Account/Logout");
                return;
            }

            if (controllerName?.ToLower() == "account" && actionName?.ToLower() == "accessdenied")
            {
                logger?.LogInformation($"Bỏ qua kiểm tra cho Account/AccessDenied");
                return;
            }

            if (controllerName?.ToLower() == "home" && actionName?.ToLower() == "index")
            {
                logger?.LogInformation($"Bỏ qua kiểm tra cho Home/Index");
                return;
            }


            // Kiểm tra xem User đã đăng nhập chưa
            if (user == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Method POST bo qua khong check
            if (httpMethod == "POST")
            {
                logger?.LogInformation($"✅ Đây là yêu cầu POST");
                return;
            }

            var serviceScopeFactory = context.HttpContext.RequestServices.GetService<IServiceScopeFactory>();

            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            List<ExcludedAction> excludedAction = configuration.GetSection("ExcludedActions").Get<List<ExcludedAction>>();

            var _count = excludedAction.Where(w => w.Controller?.ToLower() == controllerName.ToLower() && w.Action?.ToLower() == actionName.ToLower()).ToList()?.Count; 

            if (_count != null && _count > 0) 
            {
                logger?.LogInformation($"Action ngoại lệ");
                return;
            }

            using (var scope = serviceScopeFactory?.CreateScope()) // Tạo scope mới
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Lấy DbContext từ scope
                var _userInfo = _dbContext.UserViews.Where(w => w.UserName == user.Identity.Name).FirstOrDefault();
                var _role = _userInfo.DefaultRole;
                var _lstMenuRole = _dbContext.MenuRoleViews.Where(w => w.RoleCode == _role).ToList();

                // Kiểm tra quyền (Role) nếu cần
                if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName)) 
                {
                    context.Result = new ForbidResult(); // Trả về 403 Forbidden nếu không có quyền
                    return;
                } else
                {
                    _count = _lstMenuRole.Where(w => w.Controller?.ToLower() == controllerName.ToLower() 
                    && w.Action?.ToLower() == actionName.ToLower() && w.Permission == PermissionStatus.ALLOW && w.Status == StatusValue.ACTIVE.Value).ToList()?.Count;
                    if (_count == null || _count == 0)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }

            }            
        }
    }
}
