using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    public class ListOfTransPointController : BaseController
    {
        private readonly ILogger<UserManagementIDCController> _logger;
        private readonly IListOfValueService _serviceLOV;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IListOfTransPointService _serviceTransPoint;

        public ListOfTransPointController(ILogger<UserManagementIDCController> logger, IAdministrationService adminService, ISessionHelper sessionHelper, 
                    IListOfTransPointService serviceTransPoint, IListOfValueService serviceLOV, 
                    IMapper mapper, ApplicationDbContext context) : base(logger, adminService, sessionHelper)
        {
            _logger = logger;
            _serviceLOV = serviceLOV;
            _mapper = mapper;
            _context = context;
            _serviceTransPoint = serviceTransPoint;
        }

        /// <summary>
        /// Gọi menu Quản lý điểm giao dịch\Đề nghị thêm mới/thay đổi => Đề nghị thêm mới/thay đổi thông tin điểm giao dịch (Thêm/Sửa/Đóng)
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexListOfTransPoint()
        {
            string sessionUser = UserName;
            string posCode = UserPosCode;
            // Hoặc cách khác qua RouteData
            var controllerFromRoute = RouteData.Values["controller"]?.ToString();
            var actionFromRoute = RouteData.Values["action"]?.ToString();
            SetPermitData(actionFromRoute, controllerFromRoute);

            RolePermissionModel userPermission = UserPermission;

            string role = UserRole.ToString();

            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);
            TempData["UserName"] = UserName;
            TempData["UserPosCode"] = UserPosCode;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();
            
            ViewBag.EventBusinessCodes = EventBusinessCode.GetListOfTransPoint();

            return View("IndexListOfTransPointWork");
        }



    }
}
