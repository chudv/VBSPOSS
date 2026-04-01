using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class UserManagementIDCController : BaseController
    {
        private readonly ILogger<UserManagementIDCController> _logger;
        private readonly IUserManagementIDCService _userManagementIDCService;
        private readonly IListOfValueService _serviceLOV;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IPosRepresentativeService _service;

        public UserManagementIDCController(ILogger<UserManagementIDCController> logger, IAdministrationService adminService,
            ISessionHelper sessionHelper, IUserManagementIDCService userManagementIDCService,
                    IListOfValueService serviceLOV,IPosRepresentativeService service, IMapper mapper, ApplicationDbContext context) : base(logger, adminService, sessionHelper)
        {
            _logger = logger;
            _userManagementIDCService = userManagementIDCService;
            _serviceLOV = serviceLOV;
            _mapper = mapper;
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> IndexUserManagementIDC()
        {

            //public async Task<ChangeUserStatusAPIResponseViewModel> ChangeUserStatusByApiEnableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd)
            ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
            requestInput.UserId = "CHUDV002";
            requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
            var objEnableUserResult = _userManagementIDCService.ChangeUserStatusByApiDisableUser(requestInput, UserName);
            if (objEnableUserResult != null && objEnableUserResult.Result != null)
            {
                if (objEnableUserResult.Result.ResponseCode == "0" || objEnableUserResult.Result.ResponseCode == "00000")
                {
                }
            }

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
            TempData["ProductGroupCode"] = ProductGroupCode.ProductGroupCode_DepositPenal;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();

            return View("IndexUserManagementIDC");
        }

        public async Task<IActionResult> IndexApproveUserManagementIDC()
        {

            //public async Task<ChangeUserStatusAPIResponseViewModel> ChangeUserStatusByApiEnableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd)
            ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
            requestInput.UserId = "CHUDV002";
            requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
            var objEnableUserResult = _userManagementIDCService.ChangeUserStatusByApiDisableUser(requestInput, UserName);
            if (objEnableUserResult != null && objEnableUserResult.Result != null)
            {
                if (objEnableUserResult.Result.ResponseCode == "0" || objEnableUserResult.Result.ResponseCode == "00000")
                {
                }
            }

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
            TempData["ProductGroupCode"] = ProductGroupCode.ProductGroupCode_DepositPenal;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();

            return View("IndexApproveUserManagementIDC");
        }

        /// <summary>
        /// Hàm lấy danh sách lên lưới dữ liệu Danh sách người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
        /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult LoadGridData_UserIDCMaster([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFromEffectiveDate, string pToEffectiveDate, string pUserId, int pStatus,string pFullName)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pUserId))
                    pUserId = "";
                if (string.IsNullOrEmpty(pFullName))
                    pFullName = "";
                var listStaffVBSP = _userManagementIDCService.GetListUserIDCMasters(0,"",pPosCode, pUserId, pFullName, "");

                return Json(listStaffVBSP.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, ex.Message);
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserIDCMasterViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình update người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
        /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult ShowUpdateUserManagementIDC(long pId,string pPosCode, string pUserId, string pFlagCall, string pFullName)
        {
            UserIDCMasterViewModel objPosUserIDCMaster = new UserIDCMasterViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            string sNameView = "";
            var listStaffVBSP = (_userManagementIDCService.GetListUserIDCMasters(pId,"",pPosCode, pUserId,pFullName, "")).FirstOrDefault();
            if (pFlagCall == "1")
            {
                objPosUserIDCMaster.Id = 0;
                objPosUserIDCMaster.OrderNo = 0;
                objPosUserIDCMaster.PosCode = "";
                objPosUserIDCMaster.PosName = "";
                objPosUserIDCMaster.StaffId = "";
                objPosUserIDCMaster.StaffCode = "";
                objPosUserIDCMaster.UserId = "";
                objPosUserIDCMaster.NickName = "";
                objPosUserIDCMaster.FirstName = "";
                objPosUserIDCMaster.LastName = "";
                objPosUserIDCMaster.FullName = "";
                objPosUserIDCMaster.EmailAddress = "";
                objPosUserIDCMaster.MobileNumber = "";
                objPosUserIDCMaster.DateOfBirth = DateTime.Now;
                objPosUserIDCMaster.GroupName = "";
                objPosUserIDCMaster.EntityList = "";
                objPosUserIDCMaster.AuthType = "";
                objPosUserIDCMaster.UserType = "";
                objPosUserIDCMaster.MailIdFlag = "";
                objPosUserIDCMaster.AuthsecType = "";
                objPosUserIDCMaster.ExtraAttributeUserRole = "";
                objPosUserIDCMaster.ExtraAttributeBranchCode = "";
                objPosUserIDCMaster.ExpiryDate = DateTime.Now;
                objPosUserIDCMaster.Remark = "";
                objPosUserIDCMaster.OrtherNotes = "";
                objPosUserIDCMaster.Status = 1;
                objPosUserIDCMaster.StatusText = "";               
                objPosUserIDCMaster.CreatedBy = "";
                objPosUserIDCMaster.CreatedDate = DateTime.Now;
                objPosUserIDCMaster.ModifiedBy = "";
                objPosUserIDCMaster.ModifiedDate = DateTime.Now;
            }
            else
            {
                objPosUserIDCMaster.Id = listStaffVBSP.Id;
                objPosUserIDCMaster.OrderNo = listStaffVBSP.OrderNo;         
                objPosUserIDCMaster.PosCode = listStaffVBSP.PosCode;
                objPosUserIDCMaster.PosName = listStaffVBSP.PosName;
                objPosUserIDCMaster.StaffId = listStaffVBSP.StaffId;
                objPosUserIDCMaster.StaffCode = listStaffVBSP.StaffCode;
                objPosUserIDCMaster.UserId = listStaffVBSP.UserId;
                objPosUserIDCMaster.NickName = listStaffVBSP.NickName;
                objPosUserIDCMaster.FirstName = listStaffVBSP.FirstName;
                objPosUserIDCMaster.LastName = listStaffVBSP.LastName;
                objPosUserIDCMaster.FullName = listStaffVBSP.FullName;
                objPosUserIDCMaster.EmailAddress = listStaffVBSP.EmailAddress;
                objPosUserIDCMaster.MobileNumber = listStaffVBSP.MobileNumber; 
                objPosUserIDCMaster.DateOfBirth = listStaffVBSP.DateOfBirth;
                objPosUserIDCMaster.GroupName = listStaffVBSP.GroupName;
                objPosUserIDCMaster.EntityList = listStaffVBSP.EntityList;
                objPosUserIDCMaster.AuthType = listStaffVBSP.AuthType;
                objPosUserIDCMaster.UserType = listStaffVBSP.UserType;
                objPosUserIDCMaster.MailIdFlag = listStaffVBSP.MailIdFlag;
                objPosUserIDCMaster.AuthsecType = listStaffVBSP.AuthsecType;
                objPosUserIDCMaster.ExtraAttributeUserRole = listStaffVBSP.ExtraAttributeUserRole;
                objPosUserIDCMaster.ExtraAttributeBranchCode = listStaffVBSP.ExtraAttributeBranchCode;
                objPosUserIDCMaster.ExpiryDate = listStaffVBSP.ExpiryDate;
                objPosUserIDCMaster.Remark = listStaffVBSP.Remark;
                objPosUserIDCMaster.OrtherNotes = listStaffVBSP.OrtherNotes;
                objPosUserIDCMaster.Status = listStaffVBSP.Status;
                objPosUserIDCMaster.StatusText = listStaffVBSP.StatusText;
                objPosUserIDCMaster.CreatedBy = listStaffVBSP.CreatedBy;
                objPosUserIDCMaster.CreatedDate = listStaffVBSP.CreatedDate;
                objPosUserIDCMaster.ModifiedBy = listStaffVBSP.ModifiedBy;
                objPosUserIDCMaster.ModifiedDate = listStaffVBSP.ModifiedDate;
            }
            sNameView = (pFlagCall == "1")?"UpdateUserManagementIDC":"DetailUserManagementIDC";
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            return PartialView(sNameView, objPosUserIDCMaster);
        }

        /// <summary>
        /// Hàm thực hiện lưu thông tin người dùng IDC
        /// </summary>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdate([DataSourceRequest] DataSourceRequest request, UserIDCMasterViewModel objUserIDC, string pFlagCall, string pButtonType)
        {
            try
            {
                string result = "0";
                //result = IsValidPosRepresentative(objUserIDC).ToString();
                if (result == "0" && objUserIDC != null && ModelState.IsValid)
                {
                    foreach (var prop in objUserIDC.GetType().GetProperties())
                    {
                        var type = prop.PropertyType;
                        if (type == typeof(string))
                        {
                            var val = prop.GetValue(objUserIDC) as string;
                            prop.SetValue(objUserIDC, val ?? "");
                        }
                        else if (type == typeof(DateTime))
                        {
                            var val = (DateTime)prop.GetValue(objUserIDC);
                            if (val == DateTime.MinValue)
                                prop.SetValue(objUserIDC, DateTime.Now);
                        }
                        else if (type == typeof(int))
                        {
                            var val = (int)prop.GetValue(objUserIDC);
                            if (val == 0)
                                prop.SetValue(objUserIDC, 1);
                        }
                        else if (type == typeof(long))
                        {
                            var val = (long)prop.GetValue(objUserIDC);
                            if (val == 0)
                                prop.SetValue(objUserIDC, 0);
                        }
                    }
                    long iVal = await _userManagementIDCService.SaveUserIDCMaster(objUserIDC, UserName, pFlagCall);
                    result = (iVal > 0) ? "1" : "0";
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
                return new JsonResult("99");
            }
        }

        /// <summary>
        /// Hàm lấy danh sách lên lưới dữ liệu Danh sách trình duyệt người dùng IDC theo Pos
        /// </summary>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult LoadGridData_UserIDCApproval([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFromEffectiveDate, string pToEffectiveDate, string pUserId, int pStatus,string pFullName)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pUserId))
                    pUserId = "";
                if (string.IsNullOrEmpty(pFullName))
                    pFullName = "";
                var listStaffVBSP = _userManagementIDCService.UserIDCApproval_GetSearch(pFromEffectiveDate,pToEffectiveDate,pPosCode, 1, "");

                return Json(listStaffVBSP.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, ex.Message);
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserIDCApprovalViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình phê duyệt người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
        /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult ShowApprovalUserIDC(long pId,string pPosCode, string pUserId, string pFlagCall, string pFullName)
        {
            UserIDCMasterViewModel objPosUserIDCMaster = new UserIDCMasterViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            string sNameView = "";
            var listStaffVBSP = (_userManagementIDCService.GetListUserIDCMasters(pId,"",pPosCode, pUserId,pFullName, "")).FirstOrDefault();
            if (pFlagCall == "1")
            {
                objPosUserIDCMaster.Id = 0;
                objPosUserIDCMaster.OrderNo = 0;
                objPosUserIDCMaster.PosCode = "";
                objPosUserIDCMaster.PosName = "";
                objPosUserIDCMaster.StaffId = "";
                objPosUserIDCMaster.StaffCode = "";
                objPosUserIDCMaster.UserId = "";
                objPosUserIDCMaster.NickName = "";
                objPosUserIDCMaster.FirstName = "";
                objPosUserIDCMaster.LastName = "";
                objPosUserIDCMaster.FullName = "";
                objPosUserIDCMaster.EmailAddress = "";
                objPosUserIDCMaster.MobileNumber = "";
                objPosUserIDCMaster.DateOfBirth = DateTime.Now;
                objPosUserIDCMaster.GroupName = "";
                objPosUserIDCMaster.EntityList = "";
                objPosUserIDCMaster.AuthType = "";
                objPosUserIDCMaster.UserType = "";
                objPosUserIDCMaster.MailIdFlag = "";
                objPosUserIDCMaster.AuthsecType = "";
                objPosUserIDCMaster.ExtraAttributeUserRole = "";
                objPosUserIDCMaster.ExtraAttributeBranchCode = "";
                objPosUserIDCMaster.ExpiryDate = DateTime.Now;
                objPosUserIDCMaster.Remark = "";
                objPosUserIDCMaster.OrtherNotes = "";
                objPosUserIDCMaster.Status = 1;
                objPosUserIDCMaster.StatusText = "";               
                objPosUserIDCMaster.CreatedBy = "";
                objPosUserIDCMaster.CreatedDate = DateTime.Now;
                objPosUserIDCMaster.ModifiedBy = "";
                objPosUserIDCMaster.ModifiedDate = DateTime.Now;
            }
            else
            {
                objPosUserIDCMaster.Id = listStaffVBSP.Id;
                objPosUserIDCMaster.OrderNo = listStaffVBSP.OrderNo;         
                objPosUserIDCMaster.PosCode = listStaffVBSP.PosCode;
                objPosUserIDCMaster.PosName = listStaffVBSP.PosName;
                objPosUserIDCMaster.StaffId = listStaffVBSP.StaffId;
                objPosUserIDCMaster.StaffCode = listStaffVBSP.StaffCode;
                objPosUserIDCMaster.UserId = listStaffVBSP.UserId;
                objPosUserIDCMaster.NickName = listStaffVBSP.NickName;
                objPosUserIDCMaster.FirstName = listStaffVBSP.FirstName;
                objPosUserIDCMaster.LastName = listStaffVBSP.LastName;
                objPosUserIDCMaster.FullName = listStaffVBSP.FullName;
                objPosUserIDCMaster.EmailAddress = listStaffVBSP.EmailAddress;
                objPosUserIDCMaster.MobileNumber = listStaffVBSP.MobileNumber; 
                objPosUserIDCMaster.DateOfBirth = listStaffVBSP.DateOfBirth;
                objPosUserIDCMaster.GroupName = listStaffVBSP.GroupName;
                objPosUserIDCMaster.EntityList = listStaffVBSP.EntityList;
                objPosUserIDCMaster.AuthType = listStaffVBSP.AuthType;
                objPosUserIDCMaster.UserType = listStaffVBSP.UserType;
                objPosUserIDCMaster.MailIdFlag = listStaffVBSP.MailIdFlag;
                objPosUserIDCMaster.AuthsecType = listStaffVBSP.AuthsecType;
                objPosUserIDCMaster.ExtraAttributeUserRole = listStaffVBSP.ExtraAttributeUserRole;
                objPosUserIDCMaster.ExtraAttributeBranchCode = listStaffVBSP.ExtraAttributeBranchCode;
                objPosUserIDCMaster.ExpiryDate = listStaffVBSP.ExpiryDate;
                objPosUserIDCMaster.Remark = listStaffVBSP.Remark;
                objPosUserIDCMaster.OrtherNotes = listStaffVBSP.OrtherNotes;
                objPosUserIDCMaster.Status = listStaffVBSP.Status;
                objPosUserIDCMaster.StatusText = listStaffVBSP.StatusText;
                objPosUserIDCMaster.CreatedBy = listStaffVBSP.CreatedBy;
                objPosUserIDCMaster.CreatedDate = listStaffVBSP.CreatedDate;
                objPosUserIDCMaster.ModifiedBy = listStaffVBSP.ModifiedBy;
                objPosUserIDCMaster.ModifiedDate = listStaffVBSP.ModifiedDate;
            }
            sNameView = (pFlagCall == "1")?"ApproveUserManagementIDC":"ApproveUserManagementIDC";
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            return PartialView(sNameView, objPosUserIDCMaster);
        }
    }
}
