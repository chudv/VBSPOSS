using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
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
        private readonly IListOfTransPointService _serviceTransPoint;
        private readonly IInterestRateConfigureService _intRateConfigService;

        public UserManagementIDCController(ILogger<UserManagementIDCController> logger, IAdministrationService adminService,
            ISessionHelper sessionHelper, IUserManagementIDCService userManagementIDCService, IListOfTransPointService serviceTransPoint,
                    IListOfValueService serviceLOV,IInterestRateConfigureService intRateConfigService,IMapper mapper, ApplicationDbContext context) : base(logger, adminService, sessionHelper)
        {
            _logger = logger;
            _userManagementIDCService = userManagementIDCService;
            _serviceLOV = serviceLOV;
            _mapper = mapper;
            _context = context;
            _serviceTransPoint = serviceTransPoint;
            _intRateConfigService = intRateConfigService;
        }
        /// <summary>
        /// Menu: Quản lý người dùng iDC => Đề nghị cấp/thay đổi người dùng iDC
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexUserManagementIDC()
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
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll();
            ViewBag.StatusFlowUserIDC = StatusBusinessFlow.GetListStatusOfUserIDC();
            return View("IndexUserManagementIDC");
        }

        /// <summary>
        /// Hàm tải danh sách bản ghi Tạo mới/Thay đổi thông tin,... người dùng iDC => Tải dừ bảng dữ liệu UserIDCManagement (Lịch sử thay đổi người dùng)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pFunctionType">Loại nghiệp vụ yêu cầu. Nễu rỗng tìm tất cả</param>
        /// <param name="pNickName">Mã UserId tài khoản người dùng</param>
        /// <param name="pFullName">Họ tên người dùng tìm kiếm</param>
        /// <param name="pStatus">Trạng thái</param>
        /// <returns>Danh sách người người dùng thay đổi</returns>
        public ActionResult LoadGridData_UserIDCManagement([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFunctionType, string pNickName, string pFullName, int pStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(pNickName))
                    pNickName = "";
                if (string.IsNullOrEmpty(pFullName))
                    pFullName = "";
                if (string.IsNullOrEmpty(pFunctionType))
                    pFunctionType = "";
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS) 
                    && (pPosCode != "000100" && pPosCode != "000199" && pPosCode != "000196" && pPosCode != "000197" && pPosCode != "000101"))
                {
                    if (!string.IsNullOrEmpty(pPosCode) && pPosCode == UserPosCode)
                        pPosCode = pPosCode.Substring(0, 4);
                }
                var listStaffVBSP = _userManagementIDCService.GetListUserIDCManagement(0, pPosCode, pNickName, pFullName, "", pStatus, pFunctionType, true);
                return Json(listStaffVBSP.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_UserIDCManagement('{pPosCode}','{pFunctionType}','{pNickName}','{pFullName}',{pStatus}) => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình nghiệp vụ Thêm mới hoặc Thay đổi thông tin bản ghi yêu cầu nghiệp vụ tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pButtonType">Giá trị xác định sự kiện. 1 - Thêm mới (EventFlag.EventFlag_Add.Value); 2-Thay đổi thông tin (EventFlag.EventFlag_Edit.Value)</param>
        /// <param name="pId">Chỉ số bản ghi của bảng UserManagementIDC</param>
        /// <param name="pPosCode">Mã Pos bản ghi của bảng UserManagementIDC</param>
        /// <param name="pUserId">Tài khoản người dùng</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực của yêu cầu nghiệp vụ của bản ghi. Định dạng: dd/MM/yyyy</param>
        /// <param name="pFlagCall">Cờ xác định: 1 - Thêm mới; 2 - Chỉnh sửa bản ghi; 9 - Thay đổi nghiệp vụ người dùng</param>
        /// <returns>Giá trị đối tượng UserManagementIDC</returns>
        public ActionResult ShowUpdateUserManagementIDC(string pButtonType, long pId, string pPosCode, string pUserId, string pEffectiveDate, string pFlagCall)
        {
            UserManagementIDCViewModel objUserManagementIDCUpd = new UserManagementIDCViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            if (string.IsNullOrEmpty(pEffectiveDate))
                pEffectiveDate = CustConverter.StringToDate(DefaultValue.MinDate.ToString(), FormatParameters.FORMAT_DATE_INT).ToString(FormatParameters.FORMAT_DATE);

            string sNameView = "";
            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())        //Trường hợp yêu cầu tạo mới tài khoản
            {
                #region ---1. Sự kiện thêm mới bản ghi Yêu cầu tạo mới tài khoản người dùng ---
                objUserManagementIDCUpd.OrderNo = 0;
                objUserManagementIDCUpd.Id = 0;
                objUserManagementIDCUpd.FunctionType = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
                objUserManagementIDCUpd.PosCode = "";
                objUserManagementIDCUpd.PosName = "";
                objUserManagementIDCUpd.StaffId = "";
                objUserManagementIDCUpd.StaffCode = "";
                objUserManagementIDCUpd.UserId = "";
                objUserManagementIDCUpd.NickName = "";
                objUserManagementIDCUpd.FirstName = "";
                objUserManagementIDCUpd.LastName = "";
                objUserManagementIDCUpd.FullName = "";
                objUserManagementIDCUpd.EmailAddress = "";
                objUserManagementIDCUpd.MobileNumber = "";
                objUserManagementIDCUpd.DateOfBirth = DateTime.Now;
                objUserManagementIDCUpd.GroupName = "";
                objUserManagementIDCUpd.GroupNameText = "";
                objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                objUserManagementIDCUpd.AuthType = "1";
                objUserManagementIDCUpd.UserType = "1";
                objUserManagementIDCUpd.MailIdFlag = MailIdFlag.MailIdFlag_DefaultPassword.Code;
                objUserManagementIDCUpd.AuthsecType = AuthSecType.AuthSecType_Single.Code;
                objUserManagementIDCUpd.ExtraAttributeUserRole = "";
                objUserManagementIDCUpd.ExtraAttributeBranchCode = "";
                objUserManagementIDCUpd.EffectiveDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                
                objUserManagementIDCUpd.ExpiryDate = CustConverter.StringToDate(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).AddYears(10).Date;
                objUserManagementIDCUpd.Ticket = "";
                objUserManagementIDCUpd.Remark = "";
                objUserManagementIDCUpd.OrtherNotes = "";
                objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Created.Value;
                objUserManagementIDCUpd.StatusText = StatusBusinessFlow.Status_Created.Description;

                objUserManagementIDCUpd.UserStatus = "2";
                objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                objUserManagementIDCUpd.StatusUpdateCore = 0;
                objUserManagementIDCUpd.SessionValReq = false;
                objUserManagementIDCUpd.PrevStatus = 0;
                objUserManagementIDCUpd.ResponseAttributes = "";
                objUserManagementIDCUpd.CallApiStatus = "";
                objUserManagementIDCUpd.CallApiReqRecordSl = 0;
                objUserManagementIDCUpd.CallApiResponseCode = "";
                objUserManagementIDCUpd.CallApiResponseMsg = "";

                objUserManagementIDCUpd.CreatedBy = UserName;
                objUserManagementIDCUpd.CreatedDate = DateTime.Now;
                objUserManagementIDCUpd.ModifiedBy = UserName;
                objUserManagementIDCUpd.ModifiedDate = DateTime.Now;
                objUserManagementIDCUpd.ApproverBy = UserName;
                objUserManagementIDCUpd.ApprovalDate = DateTime.Now;
                objUserManagementIDCUpd.FunctionTypeName = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Description;
                objUserManagementIDCUpd.RoleToTransferCashDescription = "";
                objUserManagementIDCUpd.RoleToTransferCashValue = "";
                objUserManagementIDCUpd.GroupNameDetail = "";
                objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = "";
                objUserManagementIDCUpd.RoleToTransferCashName = "";

                objUserManagementIDCUpd.StartDate = DateTime.Now;
                objUserManagementIDCUpd.IpSetCode = "";
                objUserManagementIDCUpd.IpSetDetail = "";
                objUserManagementIDCUpd.RestrictionFlag = 0;
                objUserManagementIDCUpd.RestrictionFlagCheck = false;
                
                objUserManagementIDCUpd.SubType = "1";
                objUserManagementIDCUpd.AuthsecTypeName = "";
                objUserManagementIDCUpd.MailIdFlagName = "";
                objUserManagementIDCUpd.CallApiAutoGeneratedPassword = "";
                objUserManagementIDCUpd.StaffDepartmentName = "";
                objUserManagementIDCUpd.GroupNameOld = "";

                objUserManagementIDCUpd.PosCodeOld = "";
                objUserManagementIDCUpd.PosNameOld = "";
                objUserManagementIDCUpd.GroupNameOld = "";
                objUserManagementIDCUpd.FirstNameOld = "";
                objUserManagementIDCUpd.LastNameOld = "";
                objUserManagementIDCUpd.FullNameOld = "";
                objUserManagementIDCUpd.EmailAddressOld = "";
                objUserManagementIDCUpd.MobileNumberOld = "";
                objUserManagementIDCUpd.DateOfBirthOld = DateTime.Now;
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())        //Trường hợp chỉnh sửa bản ghi yêu cầu nghiệp vụ: Bản ghi có trong bảng UserIDCManagement
            {
                //var listStaffVBSPMaster = _userManagementIDCService.GetListUserIDCMasters(0, "", pPosCode, pUserId, pFullName, "", 3).FirstOrDefault();
                ////var listStaffVBSP = (_userManagementIDCService.GetListUserIDCManagement(pId,"",pPosCode, pUserId,pFullName, "","",-1)).FirstOrDefault();
                //var listStaffVBSP = (_userManagementIDCService.GetListUserIDCManagement(pId, pPosCode, pUserId, pFullName, "", -1, "")).FirstOrDefault();
                #region ---2. Sự kiện chỉnh sửa bản ghi Yêu cầu tạo mới tài khoản người dùng ---
                var objUserManagementIDCFind = _userManagementIDCService.GetListUserIDCManagement(pId, pPosCode, pUserId, "", "", -1, "", false).FirstOrDefault();
                if (objUserManagementIDCFind != null && objUserManagementIDCFind.Id > 0 && !string.IsNullOrEmpty(objUserManagementIDCFind.FunctionType))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                    objUserManagementIDCUpd.Id = objUserManagementIDCFind.Id;
                    objUserManagementIDCUpd.OrderNo = objUserManagementIDCFind.OrderNo;
                    objUserManagementIDCUpd.FunctionType = objUserManagementIDCFind.FunctionType;

                    objUserManagementIDCUpd.PosCode = objUserManagementIDCFind.PosCode;
                    objUserManagementIDCUpd.PosName = objUserManagementIDCFind.PosName;
                    objUserManagementIDCUpd.StaffId = objUserManagementIDCFind.StaffId;
                    objUserManagementIDCUpd.StaffCode = objUserManagementIDCFind.StaffCode;
                    objUserManagementIDCUpd.UserId = objUserManagementIDCFind.UserId;
                    objUserManagementIDCUpd.NickName = objUserManagementIDCFind.NickName;
                    objUserManagementIDCUpd.FirstName = objUserManagementIDCFind.FirstName;
                    objUserManagementIDCUpd.LastName = objUserManagementIDCFind.LastName;
                    objUserManagementIDCUpd.FullName = objUserManagementIDCFind.FullName;
                    objUserManagementIDCUpd.EmailAddress = objUserManagementIDCFind.EmailAddress;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementIDCFind.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCFind.DateOfBirth;
                    objUserManagementIDCUpd.GroupName = objUserManagementIDCFind.GroupName;
                    objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                    objUserManagementIDCUpd.AuthType = objUserManagementIDCFind.AuthType;
                    objUserManagementIDCUpd.UserType = objUserManagementIDCFind.UserType;
                    objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCFind.MailIdFlag;
                    objUserManagementIDCUpd.AuthsecType = objUserManagementIDCFind.AuthsecType;
                    objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCFind.GroupName;
                    objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCFind.PosCode;
                    objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCFind.EffectiveDate;
                    objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                    objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCFind.ExpiryDate;
                    objUserManagementIDCUpd.Ticket = objUserManagementIDCFind.Ticket;
                    objUserManagementIDCUpd.Remark = objUserManagementIDCFind.Remark;
                    objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCFind.OrtherNotes;
                    objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Modified.Value; //objUserManagementIDCFind.Status;
                    objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

                    objUserManagementIDCUpd.UserStatus = objUserManagementIDCFind.UserStatus;
                    if (objUserManagementIDCFind.UserStatus == "1")
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                    else if (objUserManagementIDCFind.UserStatus == "2")
                        objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                    else if (objUserManagementIDCFind.UserStatus == "4")
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Block)";
                    else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                    objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCFind.StatusUpdateCore;
                    objUserManagementIDCUpd.SessionValReq = objUserManagementIDCFind.SessionValReq;
                    objUserManagementIDCUpd.PrevStatus = objUserManagementIDCFind.PrevStatus;
                    objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCFind.ResponseAttributes;
                    objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCFind.CallApiStatus;
                    objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCFind.CallApiReqRecordSl;
                    objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCFind.CallApiResponseCode;
                    objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCFind.CallApiResponseMsg;

                    objUserManagementIDCUpd.CreatedBy = objUserManagementIDCFind.CreatedBy;
                    objUserManagementIDCUpd.CreatedDate = objUserManagementIDCFind.CreatedDate;
                    objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCFind.ModifiedBy;
                    objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCFind.ModifiedDate;
                    objUserManagementIDCUpd.ApproverBy = objUserManagementIDCFind.ApproverBy;
                    objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCFind.ApprovalDate;
                    objUserManagementIDCUpd.FunctionTypeName = FunctionTypeFlag.GetByCode(objUserManagementIDCFind.FunctionType).Description;//GetDescriptionByCode
                    if (listRoleUsers != null && listRoleUsers.Count != 0)
                    {
                        objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCFind.GroupName).Select(s => s.ShortName).FirstOrDefault();
                        objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCFind.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                        objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                        objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                        objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
                    }
                    objUserManagementIDCUpd.StartDate = objUserManagementIDCFind.StartDate;
                    objUserManagementIDCUpd.IpSetCode = objUserManagementIDCFind.IpSetCode;
                    objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCFind.IpSetDetail;
                    objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCFind.RestrictionFlag;
                    objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                    objUserManagementIDCUpd.SubType = objUserManagementIDCUpd.SubType;
                    objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCUpd.AuthsecTypeName;
                    objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCUpd.MailIdFlagName;
                    objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCUpd.CallApiAutoGeneratedPassword;
                    objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCUpd.StaffDepartmentName;
                    objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCUpd.GroupNameOld;

                    objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCUpd.PosCodeOld;
                    objUserManagementIDCUpd.PosNameOld = objUserManagementIDCUpd.PosNameOld;
                    objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCUpd.GroupNameOld;
                    objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCUpd.FirstNameOld;
                    objUserManagementIDCUpd.LastNameOld = objUserManagementIDCUpd.LastNameOld;
                    objUserManagementIDCUpd.FullNameOld = objUserManagementIDCUpd.FullNameOld;
                    objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCUpd.EmailAddressOld;
                    objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCUpd.MobileNumberOld;
                    objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCUpd.DateOfBirthOld;
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString())
            {
                #region ---3. Sự kiện xem chi tiết bản ghi Yêu cầu nghiệp vụ tài khoản người dùng ---
                if (pButtonType == "")
                {
                    //Xem chi tiết thông tin tài khoản người dùng Intellect iDC
                }
                else
                {
                    //Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ với tài khoản người dùng Intellect iDC
                    if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    {
                        #region --- Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ Tạo mới tài khoản người dùng Intellect iDC ---
                        var objUserManagementIDCFind = _userManagementIDCService.GetListUserIDCManagement(pId, pPosCode, pUserId, "", "", -1, "", false).FirstOrDefault();
                        if (objUserManagementIDCFind != null && objUserManagementIDCFind.Id > 0 && !string.IsNullOrEmpty(objUserManagementIDCFind.FunctionType))
                        {
                            var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                            objUserManagementIDCUpd.Id = objUserManagementIDCFind.Id;
                            objUserManagementIDCUpd.OrderNo = objUserManagementIDCFind.OrderNo;
                            objUserManagementIDCUpd.FunctionType = objUserManagementIDCFind.FunctionType;
                            objUserManagementIDCUpd.PosCode = objUserManagementIDCFind.PosCode;
                            objUserManagementIDCUpd.PosName = objUserManagementIDCFind.PosName;
                            objUserManagementIDCUpd.StaffId = objUserManagementIDCFind.StaffId;
                            objUserManagementIDCUpd.StaffCode = objUserManagementIDCFind.StaffCode;
                            objUserManagementIDCUpd.UserId = objUserManagementIDCFind.UserId;
                            objUserManagementIDCUpd.NickName = objUserManagementIDCFind.NickName;
                            objUserManagementIDCUpd.FirstName = objUserManagementIDCFind.FirstName;
                            objUserManagementIDCUpd.LastName = objUserManagementIDCFind.LastName;
                            objUserManagementIDCUpd.FullName = objUserManagementIDCFind.FullName;
                            objUserManagementIDCUpd.EmailAddress = objUserManagementIDCFind.EmailAddress;
                            objUserManagementIDCUpd.MobileNumber = objUserManagementIDCFind.MobileNumber;
                            objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCFind.DateOfBirth;
                            objUserManagementIDCUpd.GroupName = objUserManagementIDCFind.GroupName;
                            objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                            objUserManagementIDCUpd.AuthType = objUserManagementIDCFind.AuthType;
                            objUserManagementIDCUpd.UserType = objUserManagementIDCFind.UserType;
                            objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCFind.MailIdFlag;
                            objUserManagementIDCUpd.AuthsecType = objUserManagementIDCFind.AuthsecType;
                            objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCFind.GroupName;
                            objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCFind.PosCode;
                            objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCFind.EffectiveDate;
                            objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                            objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCFind.ExpiryDate;
                            objUserManagementIDCUpd.Ticket = objUserManagementIDCFind.Ticket;
                            objUserManagementIDCUpd.Remark = objUserManagementIDCFind.Remark;
                            objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCFind.OrtherNotes;
                            objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Modified.Value; //objUserManagementIDCFind.Status;
                            objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

                            objUserManagementIDCUpd.UserStatus = objUserManagementIDCFind.UserStatus;
                            if (objUserManagementIDCFind.UserStatus == "1")
                                objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                            else if (objUserManagementIDCFind.UserStatus == "2")
                                objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                            else if (objUserManagementIDCFind.UserStatus == "4")
                                objUserManagementIDCUpd.UserStatusText = "Khóa (Block)";
                            else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                            objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCFind.StatusUpdateCore;
                            objUserManagementIDCUpd.SessionValReq = objUserManagementIDCFind.SessionValReq;
                            objUserManagementIDCUpd.PrevStatus = objUserManagementIDCFind.PrevStatus;
                            objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCFind.ResponseAttributes;
                            objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCFind.CallApiStatus;
                            objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCFind.CallApiReqRecordSl;
                            objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCFind.CallApiResponseCode;
                            objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCFind.CallApiResponseMsg;

                            objUserManagementIDCUpd.CreatedBy = objUserManagementIDCFind.CreatedBy;
                            objUserManagementIDCUpd.CreatedDate = objUserManagementIDCFind.CreatedDate;
                            objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCFind.ModifiedBy;
                            objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCFind.ModifiedDate;
                            objUserManagementIDCUpd.ApproverBy = objUserManagementIDCFind.ApproverBy;
                            objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCFind.ApprovalDate;
                            objUserManagementIDCUpd.FunctionTypeName = FunctionTypeFlag.GetByCode(objUserManagementIDCFind.FunctionType).Description;//GetDescriptionByCode
                            if (listRoleUsers != null && listRoleUsers.Count != 0)
                            {
                                objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCFind.GroupName).Select(s => s.ShortName).FirstOrDefault();
                                objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCFind.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                                objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                                objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                                objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                                objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
                            }
                            objUserManagementIDCUpd.StartDate = objUserManagementIDCFind.StartDate;
                            objUserManagementIDCUpd.IpSetCode = objUserManagementIDCFind.IpSetCode;
                            objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCFind.IpSetDetail;
                            objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCFind.RestrictionFlag;
                            objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                            objUserManagementIDCUpd.SubType = objUserManagementIDCUpd.SubType;
                            objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCUpd.AuthsecTypeName;
                            objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCUpd.MailIdFlagName;
                            objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCUpd.CallApiAutoGeneratedPassword;
                            objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCUpd.StaffDepartmentName;
                            objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCUpd.GroupNameOld;

                            objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCUpd.PosCodeOld;
                            objUserManagementIDCUpd.PosNameOld = objUserManagementIDCUpd.PosNameOld;
                            objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCUpd.GroupNameOld;
                            objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCUpd.FirstNameOld;
                            objUserManagementIDCUpd.LastNameOld = objUserManagementIDCUpd.LastNameOld;
                            objUserManagementIDCUpd.FullNameOld = objUserManagementIDCUpd.FullNameOld;
                            objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCUpd.EmailAddressOld;
                            objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCUpd.MobileNumberOld;
                            objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCUpd.DateOfBirthOld;
                        }
                        #endregion
                    }
                }
                #endregion
            }
            objUserManagementIDCUpd.FlagCall = pFlagCall;

            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() && pId == 0)
                sNameView = "UpdateUserManagementIDC";
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pId != 0)
                sNameView = "UpdateUserManagementIDC";
            else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString())
            {
                if (pButtonType == "")
                {
                    //Xem chi tiết thông tin tài khoản người dùng Intellect iDC => Lấy thông tin trong UserIDCMaster, sau đó lấy tiếp trong Intellect IDC để gộp thành thông tin mới nhất
                    //=>Show lên xem chi tiết View DetailUserIDC

                }
                else if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                {
                    sNameView = "DetailUserManagementIDC";
                }
            }    
            ViewBag.FunctionTypes = FunctionTypeFlag.GetOption();
            ViewBag.MailIdFlags = MailIdFlag.GetAll();
            ViewBag.AuthSecTypes = AuthSecType.GetAll();
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["ButtonType"] = pButtonType;
            return PartialView(sNameView, objUserManagementIDCUpd);
        }


        /*
          /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
    ///             1 - Thêm mới
    ///             2 - Chỉnh sửa
    ///             8 - Phê duyệt
    ///             9 - Trình duyệt
         */




        /// <summary>
        /// Hàm thực hiện kiểm tra thông tin người dùng Intellect IDC trước khi lưu vào bảng UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementIDCUpdate">Thông tin cập nhật theo model UserManagementIDCViewModel</param>
        /// <returns>Kết quả. Giá trị: 
        ///             0 - Hợp lệ;
        ///             15 - Yêu cầu tạo mới tài khoản người dùng nhưng trạng thái bản ghi là đóng
        ///             1 - Mã đơn vị của tài khoản người dùng Intellect iDC không được để trống
        ///             2 - Mã cán bộ của tài khoản người dùng Intellect iDC không được để trống
        ///             14 - Số điện thoại của người dùng không được để trống
        ///             3 - Tài khoản người dùng có ngày hiệu lực bằng ngày hết hiệu lực
        ///             4 - Tài khoản người dùng có ngày hiệu lực lớn hơn ngày hết hiệu lực
        ///             12 - Tài khoản người dùng Intellect iDC cập nhật có ngày hiệu lực [" + lcEffectiveDate + "] nhỏ hơn ngày hiện tại của hệ thống Intelect iDC [" + lcBusinessDate + "]. Vui lòng kiểm tra lại!
        ///             13 - Tài khoản người dùng Intellect iDC cập nhật có ngày bắt đầu [" + lcStartDate + "] nhỏ hơn ngày hiện tại của hệ thống Intelect iDC [" + lcBusinessDate + "]. Vui lòng kiểm tra lại
        ///             5 - Tài khoản người dùng có phương thức xác thực khi đăng nhập là OTP nên không thể thực hiện tạo yêu cầu cấp lại mật khẩu
        ///             6 - Tài khoản người dùng cần khóa đã mở sổ tiền mặt đầu ngày
        ///             7 - Tài khoản người dùng có yêu cầu nghiệp vụ thay đổi thông tin nhưng thông tin (Quyền/Số điện thoại/Email) vẫn giữ nguyên
        ///             8 - Ngày bắt đầu nhỏ hơn ngày hiện tại
        ///             9 - Trạng thái người dùng đã được đóng, không thể thực hiện nghiệp vụ mở lại người dùng
        ///             11 - Địa chỉ email của người dùng bị trống hoặc không phải email nội bộ của NHCSXH
        ///             10 - Yêu cầu nghiệp vụ cấp mới tài khoản người dùng đã tồn tại trên hệ thống Intlelect iDC
        /// </returns>
        public async Task<int> IsValidSaveUserManagementIDC(UserManagementIDCViewModel pUserManagementIDCUpdate)
        {
            int iResult = 0;
            try
            {
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.PosCode))
                    return 1;
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.StaffCode))
                    return 2;
                if (pUserManagementIDCUpdate.Status== StatusTrans.Status_Closed.Value && pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    return 15;
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.MobileNumber))
                    return 14;
                if (pUserManagementIDCUpdate.EffectiveDate.Date == pUserManagementIDCUpdate.ExpiryDate.Date)
                    return 3;
                if (pUserManagementIDCUpdate.EffectiveDate.Date >= pUserManagementIDCUpdate.ExpiryDate.Date)
                    return 4;
                if (pUserManagementIDCUpdate.StartDate.Date < DateTime.Now.Date)
                    return 8;
                if (pUserManagementIDCUpdate.EffectiveDate.Date < pUserManagementIDCUpdate.BusinessDate.Date)
                    return 12;
                if (pUserManagementIDCUpdate.StartDate.Date < pUserManagementIDCUpdate.BusinessDate.Date)
                    return 13;
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code 
                        && pUserManagementIDCUpdate.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                    return 5;
                if (string.IsNullOrWhiteSpace(pUserManagementIDCUpdate.EmailAddress) || !pUserManagementIDCUpdate.EmailAddress.Trim().ToLower().EndsWith("@vbsp.vn"))
                    return 11;

                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code || pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                {
                    int iCheckOpenCash = _userManagementIDCService.CheckOpenCashByUserId(pUserManagementIDCUpdate.UserId, 
                                                pUserManagementIDCUpdate.StartDate.ToString(FormatParameters.FORMAT_DATE_ORA));
                    if (iCheckOpenCash != 0 && iCheckOpenCash != 3)
                        return 6;
                }
                var objViewUserIDCByApi = await _userManagementIDCService.GetUserIDCInfoByApiViewUser(pUserManagementIDCUpdate.UserId);
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code 
                    || pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code)
                {
                    if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                    {
                        if ((pUserManagementIDCUpdate.MobileNumber == objViewUserIDCByApi.MobileNumber && pUserManagementIDCUpdate.EmailAddress == objViewUserIDCByApi.EmailAddress)
                            && pUserManagementIDCUpdate.GroupName == objViewUserIDCByApi.GroupName)
                            return 7;
                    }
                }
                if (objViewUserIDCByApi.UserStatus == 1 && pUserManagementIDCUpdate.FunctionType != FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code)
                    return 9;
                if (objViewUserIDCByApi != null && !string.IsNullOrEmpty(objViewUserIDCByApi.UserId)
                                && pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    return 10;                
                return iResult;
            }
            catch
            {
                return 99;
            }
        }

        /// <summary>
        /// Hàm hực hiện lưu thông tin Thêm/Chỉnh sửa bản ghi bảng dữ liệu UserManagementIDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="objUserIDCUpd">Thông tin lưu lại theo model UserManagementIDC</param>
        /// <param name="pFlagCall">Cờ xác định cập nhật Thêm/Sửa. 1 - Thêm mới (EventFlag.EventFlag_Add.Value); 2-Thay đổi thông tin (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveAddOrUpdateUserManagementIDC([DataSourceRequest] DataSourceRequest request, UserManagementIDCViewModel objUserIDCUpd, string pFlagCall)
        {
            try
            {
                string result = "0";
                var resultCheck = await IsValidSaveUserManagementIDC(objUserIDCUpd);
                result = resultCheck.ToString();
                if (result == "0" && objUserIDCUpd != null && ModelState.IsValid)
                {
                    objUserIDCUpd.PosCode = string.IsNullOrEmpty(objUserIDCUpd.PosCode) ? "" : objUserIDCUpd.PosCode;
                    objUserIDCUpd.PosName = string.IsNullOrEmpty(objUserIDCUpd.PosName) ? "" : objUserIDCUpd.PosName;
                    objUserIDCUpd.StaffId = string.IsNullOrEmpty(objUserIDCUpd.StaffId) ? "" : objUserIDCUpd.StaffId;
                    objUserIDCUpd.StaffCode = string.IsNullOrEmpty(objUserIDCUpd.StaffCode) ? "" : objUserIDCUpd.StaffCode;
                    objUserIDCUpd.UserId = string.IsNullOrEmpty(objUserIDCUpd.UserId) ? "" : objUserIDCUpd.UserId;
                    objUserIDCUpd.NickName = string.IsNullOrEmpty(objUserIDCUpd.NickName) ? objUserIDCUpd.UserId : objUserIDCUpd.NickName;
                    objUserIDCUpd.FirstName = string.IsNullOrEmpty(objUserIDCUpd.FirstName) ? "" : objUserIDCUpd.FirstName;
                    objUserIDCUpd.LastName = string.IsNullOrEmpty(objUserIDCUpd.LastName) ? "" : objUserIDCUpd.LastName;
                    objUserIDCUpd.FullName = string.IsNullOrEmpty(objUserIDCUpd.FullName) ? "" : objUserIDCUpd.FullName;
                    objUserIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");
                    objUserIDCUpd.PosCodeOld = string.IsNullOrEmpty(objUserIDCUpd.PosCodeOld) ? objUserIDCUpd.PosCode : objUserIDCUpd.PosCodeOld;

                    objUserIDCUpd.PosNameOld = string.IsNullOrEmpty(objUserIDCUpd.PosNameOld) ? objUserIDCUpd.PosName : objUserIDCUpd.PosNameOld;
                    objUserIDCUpd.GroupNameOld = string.IsNullOrEmpty(objUserIDCUpd.GroupNameOld) ? objUserIDCUpd.GroupName : objUserIDCUpd.GroupNameOld;
                    objUserIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserIDCUpd.GroupNameOldText) ? objUserIDCUpd.GroupNameText : objUserIDCUpd.GroupNameOldText;
                    objUserIDCUpd.FirstNameOld = string.IsNullOrEmpty(objUserIDCUpd.FirstNameOld) ? objUserIDCUpd.FirstName : objUserIDCUpd.FirstNameOld;
                    objUserIDCUpd.LastNameOld = string.IsNullOrEmpty(objUserIDCUpd.LastNameOld) ? objUserIDCUpd.LastName : objUserIDCUpd.LastNameOld;
                    objUserIDCUpd.FullNameOld = string.IsNullOrEmpty(objUserIDCUpd.FullNameOld) ? objUserIDCUpd.FullName : objUserIDCUpd.FullNameOld;
                    objUserIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserIDCUpd.EmailAddressOld) ? objUserIDCUpd.EmailAddress : objUserIDCUpd.EmailAddressOld;
                    objUserIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserIDCUpd.MobileNumberOld) ? objUserIDCUpd.MobileNumber: objUserIDCUpd.MobileNumberOld;
                    if (objUserIDCUpd.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                        objUserIDCUpd.DateOfBirthOld = objUserIDCUpd.DateOfBirth;
                    objUserIDCUpd.DateOfBirthOld = objUserIDCUpd.DateOfBirthOld.ToString(FormatParameters.FORMAT_DATE) == "01/01/0001" ?
                        objUserIDCUpd.DateOfBirth : objUserIDCUpd.DateOfBirthOld;
                    objUserIDCUpd.Remark = string.IsNullOrEmpty(objUserIDCUpd.Remark) ? "" : objUserIDCUpd.Remark;
                    objUserIDCUpd.OrtherNotes = string.IsNullOrEmpty(objUserIDCUpd.OrtherNotes) ? "" : objUserIDCUpd.OrtherNotes;

                    objUserIDCUpd.IpSetCode = string.IsNullOrEmpty(objUserIDCUpd.IpSetCode) ? "" : objUserIDCUpd.IpSetCode;
                    
                    objUserIDCUpd.Ticket = string.IsNullOrEmpty(objUserIDCUpd.Ticket) ? "" : objUserIDCUpd.Ticket;
                    objUserIDCUpd.UserType = string.IsNullOrEmpty(objUserIDCUpd.UserType) ? "1" : objUserIDCUpd.UserType;
                    objUserIDCUpd.ExtraAttributeBranchCode = objUserIDCUpd.PosCode;
                    objUserIDCUpd.ExtraAttributeUserRole = objUserIDCUpd.GroupName;

                    objUserIDCUpd.StatusUpdateCore = (objUserIDCUpd.StatusUpdateCore == null) ? 0 : objUserIDCUpd.StatusUpdateCore;
                    objUserIDCUpd.SessionValReq = (objUserIDCUpd.SessionValReq == null) ? true : objUserIDCUpd.SessionValReq;
                    objUserIDCUpd.PrevStatus = (objUserIDCUpd.PrevStatus == null) ? 0 : objUserIDCUpd.PrevStatus;
                    objUserIDCUpd.ResponseAttributes = string.IsNullOrEmpty(objUserIDCUpd.ResponseAttributes) ? "" : objUserIDCUpd.ResponseAttributes;
                    objUserIDCUpd.CallApiReqRecordSl = (objUserIDCUpd.CallApiReqRecordSl == null) ? 0 : objUserIDCUpd.CallApiReqRecordSl;
                    objUserIDCUpd.CallApiStatus = string.IsNullOrEmpty(objUserIDCUpd.CallApiStatus) ? "" : objUserIDCUpd.CallApiStatus;
                    objUserIDCUpd.CallApiResponseCode = string.IsNullOrEmpty(objUserIDCUpd.CallApiResponseCode) ? "" : objUserIDCUpd.CallApiResponseCode;
                    objUserIDCUpd.CallApiResponseMsg = string.IsNullOrEmpty(objUserIDCUpd.CallApiResponseMsg) ? "" : objUserIDCUpd.CallApiResponseMsg;
                    objUserIDCUpd.CallApiAutoGeneratedPassword = string.IsNullOrEmpty(objUserIDCUpd.CallApiAutoGeneratedPassword) ? "" : objUserIDCUpd.CallApiAutoGeneratedPassword;
                    if (string.IsNullOrEmpty(pFlagCall))
                        pFlagCall = objUserIDCUpd.FlagCall;
                    long iResultUpdate = await _userManagementIDCService.SaveUserManagementIDC(objUserIDCUpd, UserName, pFlagCall);

                    result = (iResultUpdate > 0) ? "0" : "99";
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
        /// Hàm thực hiện Xóa (Đóng) bản ghi nghiệp vụ thêm mới hoặc thay đổi thông tin tài khoản người dùng Intellect iDC (Bảng UserManagementIDC)
        /// Nếu pLocalityId <> "" Thì cần bộ tham số pLocalityId,pExpireDate,pModifiedBy,pFlagDelete
        /// Nếu pLocalityId = ""  Thì cần bộ tham số pStaffId,pEffectDate,pExpireDate,pStatus,pCommuneCodeList,pModifiedBy,pFlagDelete
        /// </summary>
        /// <param name="pId">Chỉ số khóa bản ghi</param>
        /// <param name="pUserId">Tài khoản người dùng Intellect iDC</param>
        /// <param name="pStaffId">Id Cán bộ Có tài khoản</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1</param>
        /// <param name="pFunctionType">Nghiệp vụ thêm mới hoặc thay đổi thông tin người dùng Intellect iDC (Bắt buộc)</param>
        /// <returns>Kết quả: 1 - Thành công; 0 - Không thành công; 9 - Lỗi xẩy ra</returns>
        public string DeleteMarkUserManagementIDC(long pId, string pUserId, string pStaffId, int pStatus, string pFunctionType)
        {
            string resultVal = "";
            try
            {
                bool bSuccess = _userManagementIDCService.DeleteUserManagementIDC(pId, pUserId, pStaffId, pStatus, pFunctionType, UserName, Convert.ToByte("2"));
                resultVal = (bSuccess) ? "1" : "0";
            }
            catch
            {
                resultVal = "9";
            }
            return resultVal;
        }

        public async Task<IActionResult> IndexUserIDCMaster()
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
            TempData["ProductGroupCode"] = ProductGroupCode.ProductGroupCode_DepositPenal;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll();
            return View("IndexUserIDCMaster");
        }



        public async Task<IActionResult> IndexApproveUserManagementIDC()
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
                var listStaffVBSP = _userManagementIDCService.GetListUserIDCMasters(0,"",pPosCode, pUserId, pFullName, "",pStatus);

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
        /// Hàm thực hiện kiểm tra thông tin người dùng IDC trước khi lưu
        ///  1 - Kiểm tra PosCode
        ///  2 - Kiểm tra Mã cán bộ
        ///  3 - Kiểm tra ngày hiệu lực bằng ngày hết hạn thì báo lỗi
        ///  4 - Ngày hiệu lực lớn hơn ngày hết hạn
        ///  5 - Kiểm tra xem Người dùng IDC được phép cấp lại mật khẩu hay không?
        ///  6 - Kiểm tra mở sổ tiền mặt đầu ngày
        ///  7 - Kiểm tra thay đổi thông tin người dùng IDC có trùng khớp với thông tin trên hệ thống không?
        ///  8 - Kiểm tra ngày bắt đầu có nhỏ hơn ngày hiện tại không?
        ///  9 - Kiểm tra trạng thái người dùng nếu là khóa thì sẽ báo lỗi
        ///  10 - Kiểm tra khi thêm mới người dùng IDC nếu đã tồn tại trên hệ thống rồi sẽ báo lỗi
        ///  11 - Kiểm tra Email nếu không phải Mail của NHCSXH thì báo lỗi
        /// </summary>
        public async Task<int> IsValidSaveUserIDC(UserManagementIDCViewModel objUserIDCFull)
        {
            int iResult = 0;
            try
            {
                var objViewUserIDCByApi = await _userManagementIDCService.GetUserIDCInfoByApiViewUser(objUserIDCFull.UserId);
                if (objViewUserIDCByApi != null && !string.IsNullOrEmpty(objViewUserIDCByApi.UserId) && objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    return 10;
                if (objViewUserIDCByApi.UserStatus == 1 && objUserIDCFull.FunctionType != FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code)
                    return 9;       //Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active
                if (string.IsNullOrEmpty(objUserIDCFull.PosCode))
                    return 1;
                if (string.IsNullOrEmpty(objUserIDCFull.StaffCode))
                    return 2;
                if (objUserIDCFull.EffectiveDate.ToString(FormatParameters.FORMAT_DATE) == objUserIDCFull.ExpiryDate.ToString(FormatParameters.FORMAT_DATE))
                    return 3;
                if (objUserIDCFull.EffectiveDate > objUserIDCFull.ExpiryDate && objUserIDCFull.ExpiryDate.ToString(FormatParameters.FORMAT_DATE) != "01/01/0001")
                    return 4;
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code && objUserIDCFull.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                    return 5;
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code ||  objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code || objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                {
                    int iCheckOpenCash = _userManagementIDCService.CheckOpenCashByUserId(objUserIDCFull.UserId, objUserIDCFull.StartDate.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper());
                    if (iCheckOpenCash > 0)
                        return 6;
                }
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code || objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code)
                {                    
                    if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                    {
                        if ((objUserIDCFull.MobileNumber == objViewUserIDCByApi.MobileNumber && objUserIDCFull.EmailAddress == objViewUserIDCByApi.EmailAddress)
                            && objUserIDCFull.GroupName == objViewUserIDCByApi.GroupName)
                            return 7;
                    }
                }    
                if (objUserIDCFull.StartDate.Date < DateTime.Now.Date)
                    return 8;
                if (string.IsNullOrWhiteSpace(objUserIDCFull.EmailAddress) || !objUserIDCFull.EmailAddress.Trim().ToLower().EndsWith("@vbsp.vn"))
                    return 11;
                return iResult;
            }
            catch
            {
                return 99;
            }
        }

        /// <summary>
        /// Hàm thực hiện kiểm tra thông tin người dùng IDC trước khi lưu
        ///  6 - Kiểm tra mở sổ tiền mặt đầu ngày
        ///  9 - Kiểm tra trạng thái người dùng nếu là khóa thì sẽ báo lỗi
        /// </summary>
        public async Task<int> IsValidApprovalUserIDC(List<UserManagementIDCViewModel> listData)
        {
            try
            {
                if (listData == null || !listData.Any())
                    return 99;       
                foreach (var item in listData)
                {
                    if (item == null)
                        continue;        
                    var objViewUserIDCByApi = await _userManagementIDCService.GetUserIDCInfoByApiViewUser(item.UserId);        
                    // Kiểm tra Trạng thái người dùng: 1 = Đóng/Khóa ; 2 = Mở/Active
                    if (objViewUserIDCByApi.UserStatus == 1 && item.FunctionType != FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code)
                        return 9;
                    //Kiểm tra đảm bảo user KHÔNG mở tiền mặt
                    string startDate = item.StartDate.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper();        
                    int iCheckOpenCash = _userManagementIDCService.CheckOpenCashByUserId(item.UserId,startDate);        
                    if (iCheckOpenCash > 0)
                        return 6;
                }        
                return 0;
            }
            catch
            {
                return 99;
            }
        }


        /// <summary>
        /// Hàm thực hiện lưu thông tin người dùng IDC
        /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
        ///             1 - Thêm mới
        ///             2 - Chỉnh sửa
        ///             8 - Phê duyệt
        ///             9 - Trình duyệt
        /// </summary>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> Xoa_SaveUpdate([DataSourceRequest] DataSourceRequest request, UserManagementIDCViewModel objUserIDC, string pFlagCall, string pButtonType)
        {
            try
            {
                string result = "0";
                var resultValue = await IsValidSaveUserIDC(objUserIDC);
                result = resultValue.ToString();
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
                    //long iVal = await _userManagementIDCService.SaveUserManagementIDC(objUserIDC, UserName, pFlagCall,pButtonType);
                    long iVal = await _userManagementIDCService.SaveUserManagementIDC(objUserIDC, UserName, pFlagCall);
                    result = (iVal > 0) ? "0" : "99";
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
        /// Hàm thực hiện lưu trình duyệt/phê duyệt người dùng IDC
        /// </summary>
        [AcceptVerbs("Post")]public async Task<IActionResult> SaveUpdateApproval([DataSourceRequest] DataSourceRequest request,string listApprovalData,string pFlagCall,IFormFile fileUpload, string pFunctionType, string pMainPosCode)
        {
            List<long> saveFileStatus = null;
            long iVal = 1; 
            try
            {
                string result = "0";                
                var listData = JsonConvert.DeserializeObject<List<UserManagementIDCViewModel>>(listApprovalData);       
                if (listData == null || !listData.Any())
                    return new JsonResult("Không có dữ liệu");
                var resultValue = await IsValidApprovalUserIDC(listData);
                result = resultValue.ToString();  
                if (result != "0")
                {
                    return new JsonResult(result);
                }    
                foreach (var objUserIDC in listData)
                {
                    if (!TryValidateModel(objUserIDC))
                    {
                        return new JsonResult("Dữ liệu không hợp lệ");
                    }
        
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
                    string pButtonType = objUserIDC.Status.ToString();
                    iVal = await _userManagementIDCService.SaveApproveUserManagementIDC(objUserIDC, UserName, pFlagCall, pButtonType);
                    if (iVal <= 0)
                    {
                        result = iVal.ToString();
                        break;
                    }
                }

                if (fileUpload != null && fileUpload.Length > 0)
                {
                    var extension = Path.GetExtension(fileUpload.FileName).ToLower();          
                    string sPathFileUpload = Common.UploadDirFileDocument.Replace("~", "").Replace("/", @"\");            
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","Uploads","ToTrinh");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var fileName = _userManagementIDCService.GetFileNameNewUpload(0,pFunctionType,"",DateTime.Now) + extension;               
                    var fullFilePath = Path.Combine(folderPath, fileName);                   
                    var relativeFilePath = Path.Combine("wwwroot", "Uploads", "ToTrinh");
                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }
                    
                    saveFileStatus = await _userManagementIDCService.SaveAttachedFiles(0,
                        new List<AttachedFileInfo>
                        {
                            new AttachedFileInfo
                            {
                                DocumentId = long.Parse(pMainPosCode),
                                FileType = "2",
                                FileName = fileUpload.FileName,
                                PathFile = relativeFilePath,
                                FileExtension = extension,
                                FileNameNew = fileName,
                                DocumentNumber = pFunctionType,
                                Status = 1,
                                CreatedBy = UserName,
                                CreatedDate = DateTime.Now,
                                ModifiedBy = UserName,
                                ModifiedDate = DateTime.Now,
                            }
                        },
                        UserName
                    );
                    if (saveFileStatus?.Any() != true)
                    {
                        throw new Exception("Lưu file thất bại!");
                    }
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
        public ActionResult ShowApprovalUserIDC(long pId,string pPosCode, string pUserId, string pFlagCall, string pFullName, string pButtonType)
        {
            UserManagementIDCViewModel objPosUserIDCManagement = new UserManagementIDCViewModel();
            
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            string sNameView = "";
            var listStaffVBSP = (_userManagementIDCService.GetListUserIDCManagement(pId, pPosCode, pUserId, pFullName, "", 0, "", true)).FirstOrDefault();
            sNameView = (pFlagCall == "1")?"ApproveUserManagementIDC":"ApproveUserManagementIDC";
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["ButtonType"] = pButtonType;
            TempData["MainPosCode"] = pPosCode;
            //objPosUserIDCManagement.PosCode = pPosCode;
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll();
            return PartialView(sNameView, objPosUserIDCManagement);
        }

       
        /// <summary>
        /// Hàm lấy danh sách file đính kèm theo Phân loại file và Chỉ số danh mục chứa file (
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định mã Pos Chi nhánh</param>
        /// <param name="pDocumentNumber">Chỉ số xác định loại nghiệp vụ </param>
        /// <returns>Danh sách các file đính kèm</returns>
        public JsonResult GetListAttachFile_ForGroupFile(int pDocumentId, string pDocumentNumber)
        {
            ArrayList data = new ArrayList();
            var files = _userManagementIDCService.GetAttachFileSearch(0, pDocumentId, "", "", pDocumentNumber, 1);
            for (int i = 0; i < files.Count; i++)
            {
                data.Add(new { OwnerId = files[i].DocumentId, Id = files[i].FileId, FileName = files[i].FileName, Description = files[i].ContentDescription, FileNameNew = files[i].FileNameNew, PhanLoaiChiTiet = files[i].DocumentNumber });
            }
            return new JsonResult(data);
        }

        /// <summary>
        /// Hàm hiển thị file đính kèm lên Tab mới của trình duyệt
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định VB/TL/QĐ Khác có file đính kèm</param>
        /// <param name="pFileId">Chỉ số file đính kèm</param>
        /// <param name="pFileName">Tên file đính kèm cần show</param>
        /// <returns></returns>
        public IActionResult LoadPdfFile(int pDocumentId, int pFileId, string pFileName)
        {
            string sFileNameNew = "", filePath = "", sSQL = "";
            string sPathFileUpload = Common.UploadDirFileDocument.Replace("~", "").Replace("/", @"\") + @"\";
            var sUploadPathTemp = Path.Combine(Directory.GetCurrentDirectory(), sPathFileUpload, "ToTrinh");
            if (pFileId != 0)
            {
                var objFileInfo = _userManagementIDCService.GetAttachFileSearch(pFileId, pDocumentId, "", "", "", 1).FirstOrDefault();
                if (objFileInfo != null && !string.IsNullOrEmpty(objFileInfo.FileNameNew))
                {
                    sUploadPathTemp = Path.Combine(Directory.GetCurrentDirectory(), objFileInfo.PathFile, "");
                    if (objFileInfo.FileNameNew.Contains(objFileInfo.FileExtension))
                        //filePath = string.Format("{0}/{1}", sUploadPathTemp);
                        filePath = string.Format("{0}/{1}", sUploadPathTemp, $"{objFileInfo.FileNameNew}");
                    else filePath = string.Format("{0}/{1}", sUploadPathTemp, $"{objFileInfo.FileNameNew}{objFileInfo.FileExtension}");
                }
            }
            else
            {
                sFileNameNew = pFileName;
                filePath = string.Format("{0}/{1}", sUploadPathTemp, pFileName);
            }
            if (System.IO.File.Exists(filePath))
            {
                if (filePath.ToUpper().Contains(".PDF"))
                {
                    if (filePath.ToUpper().Contains(".PDF"))
                    {
                        byte[] pdfByte = FilesUtils.GetBytesFromFile(filePath);
                        return File(pdfByte, "application/pdf");
                    }
                }
                else
                {
                    TempData["OK"] = "0";
                    return View("_PdfContainer");
                }
            }
            else TempData["OK"] = "2";
            return View("_PdfContainer");
        }
    }
}
