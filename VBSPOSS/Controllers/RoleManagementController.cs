using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    
    public class RoleManagementController : BaseController
    {
        //private IAdministrationService _administrationService;

        public RoleManagementController(ILogger<RoleManagementController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        {
           // _administrationService = administrationService;
        }

        [Authorize]
        [AuthorizeFilter]
        public IActionResult Index(int menuId)
        {
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            //Set gia tri de kiem tra quyen truy cap
            SetPermitData(actionName, controllerName, menuId);
            TempData["UserName"] = UserName;
            TempData["UserPosCode"] = UserPosCode;
            //int permit = UserPermit;

            //if (permit == PermitValue.VIEW)
            //{
            //    return View();
            //}
            //else if (permit == PermitValue.EDIT)
            //{
            //    return View();
            //}
            //else
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            return View();
        }

        [Authorize]        
        public IActionResult LoadMenuRoleToGridData([DataSourceRequest] DataSourceRequest request, string roleCode, string roleName, int menuId, string menuText)
        {
            var _roleCode = (string.IsNullOrEmpty(roleCode) || roleCode == "null") ? "" : roleCode;
            var _roleName = (string.IsNullOrEmpty(roleName) || roleName == "null") ? "" : roleName;
            var _menuId = (menuId == null) ? 0 : menuId;
            
            var _menuText = (string.IsNullOrEmpty(menuText) || menuText == "null") ? "" : menuText;
            var _lstMenuRole = _administrationService.GetMenuRoles(_roleCode, _roleName, _menuId, _menuText, 1);

            var jsonResult = Json(_lstMenuRole.ToDataSourceResult(request));
            return jsonResult;

        }

        
        public JsonResult GetRoles(int grantType = 0, string pTitleChoice = "")
        {
            var lstRoles = _administrationService.GetRoles(grantType, UserGrade);
            ArrayList data = new ArrayList();
            if (!string.IsNullOrEmpty(pTitleChoice))
                data.Add(new { id = "", value = pTitleChoice });
            foreach (Role role in lstRoles)
            {
                data.Add(new { id = role.RoleCode, value = role.RoleName });
            }
            return Json(data);
        }

        [Authorize]        
        public ActionResult ShowBatchAuthorization(int menuId, string roleCode, string pFlagCall)
        {
            BatchAuthorizationModel _data = new BatchAuthorizationModel();
            _data.RoleCode = "";
            _data.RoleName = "";
            _data.MenuName = "";
            _data.MenuId = 0;
            _data.Status = StatusValue.ACTIVE.Value;
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            return PartialView("BatchAuthorization", _data);
        }

        [Authorize]
        public IActionResult LoadBatchAuthorizationGridData([DataSourceRequest] DataSourceRequest request, string roleCode, string roleName, int menuId, string menuText, int permitId)
        {
            var _roleCode = (string.IsNullOrEmpty(roleCode) || roleCode == "null") ? "" : roleCode;
            var _roleName = (string.IsNullOrEmpty(roleName) || roleName == "null") ? "" : roleName;
            var _menuId = (menuId == null) ? 0 : menuId;            
            var _menuText = (string.IsNullOrEmpty(menuText) || menuText == "null") ? "" : menuText;
            var _lstRoles = _administrationService.GetMenuRoles(_roleCode, _roleName, _menuId, _menuText, 1);
            return Json(_lstRoles.ToDataSourceResult(request));            
        }

        [Authorize]
        /// <summary>
        /// Cập nhật phân quyền theo lô
        /// </summary>
        /// <param name="request"></param>
        /// <param name="updateData"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        public IActionResult SaveBatchAuthorization([DataSourceRequest] DataSourceRequest request, BatchAuthorizationModel updateData)
        {
            try
            {
                string result = "0";                
                if (updateData != null && ModelState.IsValid)
                {
                    result = _administrationService.BatchUpdateRole(updateData).ToString();                    
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
                return new JsonResult("0");
            }
        }

        /// <summary>
        /// Lấy danh sách menu
        /// </summary>
        /// <param name="pTitleChoice"></param>
        /// <returns></returns>
        [Authorize]
        public JsonResult GetMenus(string pTitleChoice = "")
        {
            var listMenu = _administrationService.GetMenus();
            ArrayList data = new ArrayList() ;
            if (!string.IsNullOrEmpty(pTitleChoice))
            {
                data.Add(new { id = 0, value = pTitleChoice });
            }
                
            foreach (Data.Models.Menu item in listMenu)
            {
                data.Add(new { id = item.Id, value = item.Text });
            }
            return Json(data);
        }

        /// <summary>
        /// Lấy thông tin cán bộ
        /// </summary>
        /// <param name="posCode"></param>
        /// <returns></returns>
        public JsonResult GetStaffs(string posCode)
        {
            ArrayList data = new ArrayList { new { id = 0, value = "--- Chọn thành viên muốn kế thừa thông tin từ Hồ sơ cán bộ ---" } };
            
            var _posCode = posCode;
            if (string.IsNullOrEmpty(posCode))
                _posCode = UserGrade == PosGrade.HEAD_POS ? "000100" : UserPosCode;

            var _lstStaff = _administrationService.GetStaffs(_posCode);

            foreach (StaffView item in _lstStaff)
            {
                if (!string.IsNullOrEmpty(item.StaffCode))
                {
                    string _DonVi = item.PosCode.Substring(2).Trim() + " - " + item.PosName.Replace("Công nghệ thông tin", "CNTT").Replace("huyện ", "H.").Trim();
                    string _NgaySinh = (item.Birthday == null) ? "" : item.Birthday.ToString("dd-MM-yyyy");
                    data.Add(new { id = item.Id, value = _DonVi + " -> " + item.StaffName + " -> " + item.DepartmentDesc + " -> " + item.TitleDesc + " (" + _NgaySinh + ")" });
                }
            }
            return Json(data);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo Code
        /// </summary>
        /// <param name="staffCode"></param>
        /// <returns></returns>
        public JsonResult GetStaffByCode(string staffCode)
        {            
            var _lstStaff = _administrationService.GetStaffByCode(staffCode);
            return Json(_lstStaff);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo Id
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public JsonResult GetStaffById(int staffId)
        {
            var _lstStaff = _administrationService.GetStaffById(staffId);
            return Json(_lstStaff);
        }
    }
}
