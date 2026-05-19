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
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.SvgIcons;
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
            TempData["EventFlag_EditIDC"] = EventFlag.EventFlag_EditIDC.Value.ToString();

            TempData["FunctionTypeFlag_ADDNEW_USER"] = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
            TempData["FunctionTypeFlag_ResetPassword"] = FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code;
            TempData["FunctionTypeFlag_ENABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code;
            TempData["FunctionTypeFlag_DISABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code;
            TempData["FunctionTypeFlag_MODIFY_USER"] = FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code;
            TempData["FunctionTypeFlag_CHANGE_POS"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code;
            TempData["FunctionTypeFlag_CHANGE_ROLE"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code;
            TempData["FunctionTypeFlag_DELETE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code;
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll(true);
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
        public async Task<ActionResult> LoadGridData_UserIDCManagement([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFunctionType, 
                                string pNickName, string pFullName, int pStatus, string pEventCode = "")
        {
            try
            {
                string sMainPosCode = "";
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
                    {
                        sMainPosCode = pPosCode;
                        pPosCode = "";
                    }
                }
                DateTime dBusinessDateTmp = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                DateTime dSystemDateTemp = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                List<UserManagementIDCViewModel> listUserManagementIDCTmp = new List<UserManagementIDCViewModel>();
                if (pEventCode == EventFlag.EventFlag_Approval.Value.ToString())
                {
                    var listUserManagementIDCTmp01 = await _userManagementIDCService.GetListUserIDCManagement(0, sMainPosCode, pPosCode, pNickName, pFullName, "", -1, pFunctionType, true);
                    listUserManagementIDCTmp = listUserManagementIDCTmp01.Where(w => w.Status == StatusBusinessFlow.Status_Created.Value
                                    || w.Status == StatusBusinessFlow.Status_Modified.Value || w.Status == StatusBusinessFlow.Status_Submitted.Value).ToList();
                    if (listUserManagementIDCTmp != null && listUserManagementIDCTmp.Count != 0)
                    {
                        int iCountTemp = 0;
                        foreach(var itemUMIDC in listUserManagementIDCTmp)
                        {
                            iCountTemp++;
                            itemUMIDC.OrderNo = iCountTemp;
                            itemUMIDC.SystemDate = dSystemDateTemp;
                            itemUMIDC.SystemDateText = dSystemDateTemp.ToString(FormatParameters.FORMAT_DATE);
                            itemUMIDC.BusinessDate = dBusinessDateTmp;
                            itemUMIDC.BusinessDateText = dBusinessDateTmp.ToString(FormatParameters.FORMAT_DATE);
                        }    
                    } 
                }
                else if (pEventCode == EventFlag.EventFlag_Authorize.Value.ToString())
                {
                    var listUserManagementIDCTmp02 = await _userManagementIDCService.GetListUserIDCManagement(0, sMainPosCode, pPosCode, pNickName, pFullName, "", -1, pFunctionType, true);
                    listUserManagementIDCTmp = listUserManagementIDCTmp02.Where(w => w.Status == StatusBusinessFlow.Status_Submitted.Value).ToList();
                    if (listUserManagementIDCTmp != null && listUserManagementIDCTmp.Count != 0)
                    {
                        int iCountTemp = 0;
                        foreach (var itemUMIDC in listUserManagementIDCTmp)
                        {
                            iCountTemp++;
                            itemUMIDC.OrderNo = iCountTemp;
                            itemUMIDC.SystemDate = dSystemDateTemp;
                            itemUMIDC.SystemDateText = dSystemDateTemp.ToString(FormatParameters.FORMAT_DATE);
                            itemUMIDC.BusinessDate = dBusinessDateTmp;
                            itemUMIDC.BusinessDateText = dBusinessDateTmp.ToString(FormatParameters.FORMAT_DATE);
                        }
                    }
                }
                else
                {
                    listUserManagementIDCTmp = await _userManagementIDCService.GetListUserIDCManagement(0, sMainPosCode, pPosCode, pNickName, pFullName, "", pStatus, pFunctionType, true); 
                }

                return Json(listUserManagementIDCTmp.ToDataSourceResult(request, ModelState));
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
        /// <param name="pButtonType">Giá trị yêu cầu. Ex: EventFlag.EventFlag_Add.Code/</param>
        /// <param name="pId">Chỉ số bản ghi của bảng UserManagementIDC</param>
        /// <param name="pPosCode">Mã Pos bản ghi của bảng UserManagementIDC</param>
        /// <param name="pUserId">Tài khoản người dùng</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực của yêu cầu nghiệp vụ của bản ghi. Định dạng: dd/MM/yyyy</param>
        /// <param name="pFlagCall">Cờ xác định: 1 - Thêm mới; 2 - Chỉnh sửa bản ghi; 9 - Thay đổi nghiệp vụ người dùng</param>
        /// <returns>Giá trị đối tượng UserManagementIDC</returns>
        public async Task<ActionResult> ShowUpdateUserManagementIDC(string pButtonType, long pId, string pPosCode, string pUserId, string pEffectiveDate, string pFlagCall)
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

                objUserManagementIDCUpd.AuthType = DefaultValue.UserIDC_AuthType;
                objUserManagementIDCUpd.UserType = DefaultValue.UserIDC_UserType;
                objUserManagementIDCUpd.MailIdFlag = MailIdFlag.MailIdFlag_DefaultPassword.Code;
                objUserManagementIDCUpd.AuthsecType = AuthSecType.AuthSecType_Single.Code;
                objUserManagementIDCUpd.ExtraAttributeUserRole = "";
                objUserManagementIDCUpd.ExtraAttributeBranchCode = "";
                objUserManagementIDCUpd.EffectiveDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
                objUserManagementIDCUpd.ExpiryDate = CustConverter.StringToDate(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).AddYears(10).Date;
                objUserManagementIDCUpd.Ticket = "";
                objUserManagementIDCUpd.Remark = "";
                objUserManagementIDCUpd.OrtherNotes = "";
                objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Created.Value;
                objUserManagementIDCUpd.StatusText = StatusBusinessFlow.Status_Created.Description;

                objUserManagementIDCUpd.UserStatus = DefaultValue.UserIDC_UserStatus_Open;
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

                objUserManagementIDCUpd.SubType = DefaultValue.UserIDC_SubType;
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
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)        //Trường hợp chỉnh sửa bản ghi yêu cầu nghiệp vụ: Bản ghi có trong bảng UserIDCManagement
            {
                #region ---2. Sự kiện chỉnh sửa bản ghi Yêu cầu tạo mới tài khoản người dùng ---
                var objUserManagementIDCFind01 = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();
                if (objUserManagementIDCFind01 != null && objUserManagementIDCFind01.Id > 0 && !string.IsNullOrEmpty(objUserManagementIDCFind01.FunctionType))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                    objUserManagementIDCUpd.Id = objUserManagementIDCFind01.Id;
                    objUserManagementIDCUpd.OrderNo = objUserManagementIDCFind01.OrderNo;
                    objUserManagementIDCUpd.FunctionType = objUserManagementIDCFind01.FunctionType;

                    objUserManagementIDCUpd.PosCode = objUserManagementIDCFind01.PosCode;
                    objUserManagementIDCUpd.PosName = objUserManagementIDCFind01.PosName;
                    objUserManagementIDCUpd.StaffId = objUserManagementIDCFind01.StaffId;
                    objUserManagementIDCUpd.StaffCode = objUserManagementIDCFind01.StaffCode;
                    objUserManagementIDCUpd.UserId = objUserManagementIDCFind01.UserId;
                    objUserManagementIDCUpd.NickName = objUserManagementIDCFind01.NickName;
                    objUserManagementIDCUpd.FirstName = objUserManagementIDCFind01.FirstName;
                    objUserManagementIDCUpd.LastName = objUserManagementIDCFind01.LastName;
                    objUserManagementIDCUpd.FullName = objUserManagementIDCFind01.FullName;
                    objUserManagementIDCUpd.EmailAddress = objUserManagementIDCFind01.EmailAddress;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementIDCFind01.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCFind01.DateOfBirth;
                    objUserManagementIDCUpd.GroupName = objUserManagementIDCFind01.GroupName;
                    objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                    objUserManagementIDCUpd.AuthType = objUserManagementIDCFind01.AuthType;
                    objUserManagementIDCUpd.UserType = objUserManagementIDCFind01.UserType;
                    objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCFind01.MailIdFlag;
                    objUserManagementIDCUpd.AuthsecType = objUserManagementIDCFind01.AuthsecType;
                    objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCFind01.GroupName;
                    objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCFind01.PosCode;
                    objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCFind01.EffectiveDate;
                    objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                    objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCFind01.ExpiryDate;
                    objUserManagementIDCUpd.Ticket = objUserManagementIDCFind01.Ticket;
                    objUserManagementIDCUpd.Remark = objUserManagementIDCFind01.Remark;
                    objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCFind01.OrtherNotes;
                    objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Modified.Value; //objUserManagementIDCFind01.Status;
                    objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

                    objUserManagementIDCUpd.UserStatus = objUserManagementIDCFind01.UserStatus;
                    if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                    else if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                        objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                    else if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                        objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
                    else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                    objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCFind01.StatusUpdateCore;
                    objUserManagementIDCUpd.SessionValReq = objUserManagementIDCFind01.SessionValReq;
                    objUserManagementIDCUpd.PrevStatus = objUserManagementIDCFind01.PrevStatus;
                    objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCFind01.ResponseAttributes;
                    objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCFind01.CallApiStatus;
                    objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCFind01.CallApiReqRecordSl;
                    objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCFind01.CallApiResponseCode;
                    objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCFind01.CallApiResponseMsg;

                    objUserManagementIDCUpd.CreatedBy = objUserManagementIDCFind01.CreatedBy;
                    objUserManagementIDCUpd.CreatedDate = objUserManagementIDCFind01.CreatedDate;
                    objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCFind01.ModifiedBy;
                    objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCFind01.ModifiedDate;
                    objUserManagementIDCUpd.ApproverBy = objUserManagementIDCFind01.ApproverBy;
                    objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCFind01.ApprovalDate;
                    objUserManagementIDCUpd.FunctionTypeName = FunctionTypeFlag.GetByCode(objUserManagementIDCFind01.FunctionType).Description;//GetDescriptionByCode
                    if (listRoleUsers != null && listRoleUsers.Count != 0)
                    {
                        objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCFind01.GroupName).Select(s => s.ShortName).FirstOrDefault();
                        objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCFind01.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                        objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                        objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                        objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
                    }
                    objUserManagementIDCUpd.StartDate = objUserManagementIDCFind01.StartDate;
                    objUserManagementIDCUpd.IpSetCode = objUserManagementIDCFind01.IpSetCode;
                    objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCFind01.IpSetDetail;
                    objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCFind01.RestrictionFlag;
                    objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                    objUserManagementIDCUpd.SubType = objUserManagementIDCFind01.SubType;
                    objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCFind01.AuthsecTypeName;
                    objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCFind01.MailIdFlagName;
                    objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCFind01.CallApiAutoGeneratedPassword;
                    objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCFind01.StaffDepartmentName;
                    objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCFind01.PosCodeOld;
                    objUserManagementIDCUpd.PosNameOld = objUserManagementIDCFind01.PosNameOld;
                    objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCFind01.GroupNameOld;
                    objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCFind01.FirstNameOld;
                    objUserManagementIDCUpd.LastNameOld = objUserManagementIDCFind01.LastNameOld;
                    objUserManagementIDCUpd.FullNameOld = objUserManagementIDCFind01.FullNameOld;
                    objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCFind01.EmailAddressOld;
                    objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCFind01.MobileNumberOld;
                    objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCFind01.DateOfBirthOld;
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString())
            {
                #region ---3. Sự kiện xem chi tiết bản ghi Yêu cầu nghiệp vụ tài khoản người dùng ---
                if (string.IsNullOrEmpty(pButtonType) || pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                {
                    UserManagementIDCViewModel objUserManagementIDCViewTmp = new UserManagementIDCViewModel();
                    if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)//Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ với tài khoản người dùng Intellect iDC
                        objUserManagementIDCViewTmp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();
                    else
                    {
                        //Xem chi tiết thông tin tài khoản người dùng Intellect iDC
                        objUserManagementIDCViewTmp = (await _userManagementIDCService.GetListUserIDCManagement(0, "", "", pUserId, "", "", -1, "", true)).FirstOrDefault();
                    }
                    #region --- Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ Tạo mới tài khoản người dùng Intellect iDC ---
                    if (objUserManagementIDCViewTmp != null && !string.IsNullOrEmpty(objUserManagementIDCViewTmp.UserId))
                    {
                        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                        objUserManagementIDCUpd.Id = objUserManagementIDCViewTmp.Id;
                        objUserManagementIDCUpd.OrderNo = objUserManagementIDCViewTmp.OrderNo;
                        objUserManagementIDCUpd.FunctionType = objUserManagementIDCViewTmp.FunctionType;
                        objUserManagementIDCUpd.PosCode = objUserManagementIDCViewTmp.PosCode;
                        objUserManagementIDCUpd.PosName = objUserManagementIDCViewTmp.PosName;
                        objUserManagementIDCUpd.StaffId = objUserManagementIDCViewTmp.StaffId;
                        objUserManagementIDCUpd.StaffCode = objUserManagementIDCViewTmp.StaffCode;
                        objUserManagementIDCUpd.UserId = objUserManagementIDCViewTmp.UserId;
                        objUserManagementIDCUpd.NickName = objUserManagementIDCViewTmp.NickName;
                        objUserManagementIDCUpd.FirstName = objUserManagementIDCViewTmp.FirstName;
                        objUserManagementIDCUpd.LastName = objUserManagementIDCViewTmp.LastName;
                        objUserManagementIDCUpd.FullName = objUserManagementIDCViewTmp.FullName;
                        objUserManagementIDCUpd.EmailAddress = objUserManagementIDCViewTmp.EmailAddress;
                        objUserManagementIDCUpd.MobileNumber = objUserManagementIDCViewTmp.MobileNumber;
                        objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCViewTmp.DateOfBirth;
                        objUserManagementIDCUpd.GroupName = objUserManagementIDCViewTmp.GroupName;
                        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                        objUserManagementIDCUpd.AuthType = objUserManagementIDCViewTmp.AuthType;
                        objUserManagementIDCUpd.UserType = objUserManagementIDCViewTmp.UserType;
                        objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCViewTmp.MailIdFlag;
                        objUserManagementIDCUpd.AuthsecType = objUserManagementIDCViewTmp.AuthsecType;
                        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCViewTmp.GroupName;
                        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCViewTmp.PosCode;
                        objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCViewTmp.EffectiveDate;
                        objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                        objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                        objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
                        objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCViewTmp.ExpiryDate;
                        objUserManagementIDCUpd.Ticket = string.IsNullOrEmpty(objUserManagementIDCViewTmp.Ticket) ? "" : objUserManagementIDCViewTmp.Ticket;
                        objUserManagementIDCUpd.Remark = objUserManagementIDCViewTmp.Remark;
                        objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCViewTmp.OrtherNotes;
                        objUserManagementIDCUpd.Status = objUserManagementIDCViewTmp.Status;
                        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

                        objUserManagementIDCUpd.UserStatus = objUserManagementIDCViewTmp.UserStatus;
                        if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                        else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                        else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                            objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
                        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                        objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCViewTmp.StatusUpdateCore;
                        objUserManagementIDCUpd.SessionValReq = objUserManagementIDCViewTmp.SessionValReq;
                        objUserManagementIDCUpd.PrevStatus = objUserManagementIDCViewTmp.PrevStatus;
                        objUserManagementIDCUpd.ResponseAttributes = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ResponseAttributes) ? "" : objUserManagementIDCViewTmp.ResponseAttributes;
                        objUserManagementIDCUpd.CallApiStatus = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiStatus) ? "" : objUserManagementIDCViewTmp.CallApiStatus;
                        objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCViewTmp.CallApiReqRecordSl;
                        objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCViewTmp.CallApiResponseCode;
                        objUserManagementIDCUpd.CallApiResponseMsg = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiResponseMsg) ? "" : objUserManagementIDCViewTmp.CallApiResponseMsg;

                        objUserManagementIDCUpd.CreatedBy = objUserManagementIDCViewTmp.CreatedBy;
                        objUserManagementIDCUpd.CreatedDate = objUserManagementIDCViewTmp.CreatedDate;
                        objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCViewTmp.ModifiedBy;
                        objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCViewTmp.ModifiedDate;
                        objUserManagementIDCUpd.ApproverBy = objUserManagementIDCViewTmp.ApproverBy;
                        objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCViewTmp.ApprovalDate;
                        objUserManagementIDCUpd.FunctionTypeName = string.IsNullOrEmpty(objUserManagementIDCViewTmp.FunctionType) ? "" : FunctionTypeFlag.GetByCode(objUserManagementIDCViewTmp.FunctionType).Description;
                        if (listRoleUsers != null && listRoleUsers.Count != 0)
                        {
                            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.ShortName).FirstOrDefault();
                            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCViewTmp.RoleToTransferCashDescription;
                            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCViewTmp.GroupName} - {objUserManagementIDCViewTmp.GroupNameText}";
                        }
                        objUserManagementIDCUpd.StartDate = objUserManagementIDCViewTmp.StartDate;
                        objUserManagementIDCUpd.StartDateOld = (objUserManagementIDCViewTmp.StartDateOld.Year <= 1900) ? objUserManagementIDCViewTmp.StartDate : objUserManagementIDCViewTmp.StartDateOld;
                        objUserManagementIDCUpd.StartDateText = objUserManagementIDCViewTmp.StartDate.ToString(FormatParameters.FORMAT_DATE);
                        objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);
                        objUserManagementIDCUpd.IpSetCode = objUserManagementIDCViewTmp.IpSetCode;
                        objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCViewTmp.IpSetDetail;
                        objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCViewTmp.RestrictionFlag;
                        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;
                        objUserManagementIDCUpd.SubType = string.IsNullOrEmpty(objUserManagementIDCViewTmp.SubType) ? DefaultValue.UserIDC_SubType : objUserManagementIDCViewTmp.SubType;
                        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCViewTmp.AuthsecTypeName;
                        objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCViewTmp.MailIdFlagName;
                        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword) ? "" : objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword;
                        objUserManagementIDCUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOld) ? objUserManagementIDCViewTmp.GroupName : objUserManagementIDCViewTmp.GroupNameOld;
                        objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOldText) ? objUserManagementIDCViewTmp.GroupNameText : objUserManagementIDCViewTmp.GroupNameOldText;
                        
                        objUserManagementIDCUpd.PosCodeOld = string.IsNullOrEmpty(objUserManagementIDCUpd.PosCodeOld) ? objUserManagementIDCUpd.PosCode : objUserManagementIDCUpd.PosCodeOld;
                        objUserManagementIDCUpd.PosNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.PosNameOld) ? objUserManagementIDCUpd.PosName : objUserManagementIDCUpd.PosNameOld;
                        objUserManagementIDCUpd.FirstNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.FirstNameOld) ? objUserManagementIDCUpd.FirstName : objUserManagementIDCUpd.FirstNameOld;
                        objUserManagementIDCUpd.LastNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.LastNameOld) ? objUserManagementIDCUpd.LastName : objUserManagementIDCUpd.LastNameOld;
                        objUserManagementIDCUpd.FullNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.FullNameOld) ? objUserManagementIDCUpd.FullName : objUserManagementIDCUpd.FullNameOld;
                        objUserManagementIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserManagementIDCUpd.EmailAddressOld) ? objUserManagementIDCUpd.EmailAddress : objUserManagementIDCUpd.EmailAddressOld;
                        objUserManagementIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserManagementIDCUpd.MobileNumberOld) ? objUserManagementIDCUpd.MobileNumber : objUserManagementIDCUpd.MobileNumberOld;
                        objUserManagementIDCUpd.DateOfBirthOld = (objUserManagementIDCUpd.DateOfBirthOld.Year <= 1900) ? objUserManagementIDCUpd.DateOfBirth : objUserManagementIDCUpd.DateOfBirthOld;

                        objUserManagementIDCUpd.GenderCode = objUserManagementIDCViewTmp.GenderCode;
                        objUserManagementIDCUpd.GenderText = objUserManagementIDCViewTmp.GenderText;
                        objUserManagementIDCUpd.StaffPosCode = objUserManagementIDCViewTmp.StaffPosCode;
                        objUserManagementIDCUpd.StaffPosName = objUserManagementIDCViewTmp.StaffPosName;
                        objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementIDCViewTmp.StaffDepartmentCode;
                        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCViewTmp.StaffDepartmentName;
                        objUserManagementIDCUpd.StaffPositionCode = objUserManagementIDCViewTmp.StaffPositionCode;
                        objUserManagementIDCUpd.StaffPositionName = objUserManagementIDCViewTmp.StaffPositionName;
                        objUserManagementIDCUpd.StaffEmail = objUserManagementIDCViewTmp.StaffEmail;
                        objUserManagementIDCUpd.StaffMobileNo = objUserManagementIDCViewTmp.StaffMobileNo;
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld;
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld) ? objUserManagementIDCViewTmp.RoleToTransferCashDescription : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld;
                        objUserManagementIDCUpd.RoleToTransferCashNameOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashNameOld) ? objUserManagementIDCViewTmp.RoleToTransferCashName : objUserManagementIDCViewTmp.RoleToTransferCashNameOld;
                        objUserManagementIDCUpd.RoleToTransferCashValueOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashValueOld) ? objUserManagementIDCViewTmp.RoleToTransferCashValue : objUserManagementIDCViewTmp.RoleToTransferCashValueOld;
                    }
                    #endregion
                }
                else
                {
                    #region ---4. Sự kiện Chỉnh sửa thông tin bản ghi (Yêu cầu thay đổi tài khoản người dùng) ---
                    var objUserManagementIDCTemp01 = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

                    if (objUserManagementIDCTemp01 != null && !string.IsNullOrEmpty(objUserManagementIDCTemp01.UserId))
                    {
                        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                        objUserManagementIDCUpd.Id = objUserManagementIDCTemp01.Id;
                        objUserManagementIDCUpd.OrderNo = 1;
                        objUserManagementIDCUpd.FunctionType = objUserManagementIDCTemp01.FunctionType;
                        objUserManagementIDCUpd.FunctionTypeName = objUserManagementIDCTemp01.FunctionTypeName;

                        objUserManagementIDCUpd.PosCode = objUserManagementIDCTemp01.PosCode;
                        objUserManagementIDCUpd.PosName = objUserManagementIDCTemp01.PosName;
                        objUserManagementIDCUpd.StaffId = objUserManagementIDCTemp01.StaffId;
                        objUserManagementIDCUpd.StaffCode = objUserManagementIDCTemp01.StaffCode;
                        objUserManagementIDCUpd.UserId = objUserManagementIDCTemp01.UserId;
                        objUserManagementIDCUpd.NickName = objUserManagementIDCTemp01.NickName;
                        objUserManagementIDCUpd.FirstName = objUserManagementIDCTemp01.FirstName;
                        objUserManagementIDCUpd.LastName = objUserManagementIDCTemp01.LastName;
                        objUserManagementIDCUpd.FullName = objUserManagementIDCTemp01.FullName;
                        objUserManagementIDCUpd.EmailAddress = objUserManagementIDCTemp01.EmailAddress;
                        objUserManagementIDCUpd.MobileNumber = objUserManagementIDCTemp01.MobileNumber;
                        objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCTemp01.DateOfBirth;
                        objUserManagementIDCUpd.GroupName = objUserManagementIDCTemp01.GroupName;
                        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                        objUserManagementIDCUpd.AuthType = objUserManagementIDCTemp01.AuthType;
                        objUserManagementIDCUpd.UserType = objUserManagementIDCTemp01.UserType;
                        objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCTemp01.MailIdFlag;
                        objUserManagementIDCUpd.AuthsecType = objUserManagementIDCTemp01.AuthsecType;
                        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCTemp01.GroupName;
                        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCTemp01.PosCode;
                        objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCTemp01.EffectiveDate;
                        objUserManagementIDCUpd.BusinessDate = objUserManagementIDCTemp01.BusinessDate;//_serviceTransPoint.GetDateInCoreIDC("1").Date;
                        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                        objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                        objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE); 
                        objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCTemp01.ExpiryDate;
                        objUserManagementIDCUpd.Ticket = objUserManagementIDCTemp01.Ticket;
                        objUserManagementIDCUpd.Remark = objUserManagementIDCTemp01.Remark;
                        objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCTemp01.OrtherNotes;
                        objUserManagementIDCUpd.Status = objUserManagementIDCTemp01.Status;
                        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
                        objUserManagementIDCUpd.UserStatus = objUserManagementIDCTemp01.UserStatus;
                        if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                        else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                        else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                            objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
                        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                        objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCTemp01.StatusUpdateCore;
                        objUserManagementIDCUpd.SessionValReq = objUserManagementIDCTemp01.SessionValReq;
                        objUserManagementIDCUpd.PrevStatus = objUserManagementIDCTemp01.PrevStatus;
                        objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCTemp01.ResponseAttributes;
                        objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCTemp01.CallApiStatus;
                        objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCTemp01.CallApiReqRecordSl;
                        objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCTemp01.CallApiResponseCode;
                        objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCTemp01.CallApiResponseMsg;

                        objUserManagementIDCUpd.CreatedBy = objUserManagementIDCTemp01.CreatedBy;
                        objUserManagementIDCUpd.CreatedDate = objUserManagementIDCTemp01.CreatedDate;
                        objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCTemp01.ModifiedBy;
                        objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCTemp01.ModifiedDate;
                        objUserManagementIDCUpd.ApproverBy = objUserManagementIDCTemp01.ApproverBy;
                        objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCTemp01.ApprovalDate;

                        if (listRoleUsers != null && listRoleUsers.Count != 0)
                        {
                            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.ShortName).FirstOrDefault();
                            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";

                            objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();

                        }
                        objUserManagementIDCUpd.StartDate = objUserManagementIDCTemp01.StartDate;
                        objUserManagementIDCUpd.IpSetCode = objUserManagementIDCTemp01.IpSetCode;
                        objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementIDCTemp01.IpSetDetail) ? "" : objUserManagementIDCTemp01.IpSetDetail;
                        objUserManagementIDCUpd.RestrictionFlag = 0;
                        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                        objUserManagementIDCUpd.SubType = objUserManagementIDCTemp01.SubType;
                        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCTemp01.AuthsecTypeName;
                        objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCTemp01.MailIdFlagName;
                        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCTemp01.CallApiAutoGeneratedPassword;

                        objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCTemp01.PosCodeOld;
                        objUserManagementIDCUpd.PosNameOld = objUserManagementIDCTemp01.PosNameOld;
                        objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCTemp01.GroupNameOld;
                        objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCTemp01.FirstNameOld;
                        objUserManagementIDCUpd.LastNameOld = objUserManagementIDCTemp01.LastNameOld;
                        objUserManagementIDCUpd.FullNameOld = objUserManagementIDCTemp01.FullNameOld;
                        objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCTemp01.EmailAddressOld;
                        objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCTemp01.MobileNumberOld;
                        objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCTemp01.DateOfBirthOld;
                        objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameOldText : objUserManagementIDCUpd.GroupNameOldText;
                        objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
                        objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
                        objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
                        objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

                        //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
                        objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
                        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
                        int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
                        if (numberDays <= 90)
                            objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

                        objUserManagementIDCUpd.GenderCode = objUserManagementIDCTemp01.GenderCode;
                        objUserManagementIDCUpd.GenderText = objUserManagementIDCTemp01.GenderText;
                        objUserManagementIDCUpd.StaffPosCode = objUserManagementIDCTemp01.StaffPosCode;
                        objUserManagementIDCUpd.StaffPosName = objUserManagementIDCTemp01.StaffPosName;
                        objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementIDCTemp01.StaffDepartmentCode;
                        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCTemp01.StaffDepartmentName;
                        objUserManagementIDCUpd.StaffPositionCode = objUserManagementIDCTemp01.StaffPositionCode;
                        objUserManagementIDCUpd.StaffPositionName = objUserManagementIDCTemp01.StaffPositionName;
                        objUserManagementIDCUpd.StaffEmail = objUserManagementIDCTemp01.StaffEmail;
                        objUserManagementIDCUpd.StaffMobileNo = objUserManagementIDCTemp01.StaffMobileNo;
                        //Lấy theo QLNS khi thay đổi thông tin người dùng
                        objUserManagementIDCUpd.EmailAddress = objUserManagementIDCTemp01.StaffEmail;
                        objUserManagementIDCUpd.MobileNumber = objUserManagementIDCTemp01.StaffMobileNo;
                        objUserManagementIDCUpd.ExistsInCore = objUserManagementIDCTemp01.ExistsInCore;
                    }

                    #endregion
                    sNameView = "CreateChangeInforUserManagementIDC";
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
            {
                #region ---4. Sự kiện Tạo lập yêu cầu thay đổi nghiệp vụ tài khoản người dùng (Tạo mới tài khoản yêu cầu thay đổi tài khoản người dùng) ---
                var listUserIDCMasterTemp = await _userManagementIDCService.GetListUserIDCMasters(0, "", pPosCode, pUserId, "", "", -1, true);
                var objUserManagementMTFind = listUserIDCMasterTemp.FirstOrDefault();
                if (objUserManagementMTFind != null && !string.IsNullOrEmpty(objUserManagementMTFind.UserId))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                    objUserManagementIDCUpd.Id = 0;// objUserManagementMTFind.Id;
                    objUserManagementIDCUpd.OrderNo = 1;
                    objUserManagementIDCUpd.FunctionType = "";

                    objUserManagementIDCUpd.PosCode = objUserManagementMTFind.PosCode;
                    objUserManagementIDCUpd.PosName = objUserManagementMTFind.PosName;
                    objUserManagementIDCUpd.StaffId = objUserManagementMTFind.StaffId;
                    objUserManagementIDCUpd.StaffCode = objUserManagementMTFind.StaffCode;
                    objUserManagementIDCUpd.UserId = objUserManagementMTFind.UserId;
                    objUserManagementIDCUpd.NickName = objUserManagementMTFind.NickName;
                    objUserManagementIDCUpd.FirstName = objUserManagementMTFind.FirstName;
                    objUserManagementIDCUpd.LastName = objUserManagementMTFind.LastName;
                    objUserManagementIDCUpd.FullName = objUserManagementMTFind.FullName;
                    objUserManagementIDCUpd.EmailAddress = objUserManagementMTFind.EmailAddress;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementMTFind.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirth = objUserManagementMTFind.DateOfBirth;
                    objUserManagementIDCUpd.GroupName = objUserManagementMTFind.GroupName;
                    objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                    objUserManagementIDCUpd.AuthType = objUserManagementMTFind.AuthType;
                    objUserManagementIDCUpd.UserType = objUserManagementMTFind.UserType;
                    objUserManagementIDCUpd.MailIdFlag = objUserManagementMTFind.MailIdFlag;
                    objUserManagementIDCUpd.AuthsecType = objUserManagementMTFind.AuthsecType;
                    objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementMTFind.GroupName;
                    objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementMTFind.PosCode;
                    objUserManagementIDCUpd.EffectiveDate = objUserManagementMTFind.EffectiveDate;
                    objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
                    objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                    objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.ExpiryDate = objUserManagementMTFind.ExpiryDate;
                    objUserManagementIDCUpd.Ticket = "";
                    objUserManagementIDCUpd.Remark = "";
                    objUserManagementIDCUpd.OrtherNotes = "";
                    objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Created.Value;
                    objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

                    objUserManagementIDCUpd.UserStatus = objUserManagementMTFind.UserStatus;
                    if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                    else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                        objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                    else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                        objUserManagementIDCUpd.UserStatusText = "Tmaj khóa (Lock)";
                    else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                    objUserManagementIDCUpd.StatusUpdateCore = 0;
                    objUserManagementIDCUpd.SessionValReq = true;
                    objUserManagementIDCUpd.PrevStatus = 0;
                    objUserManagementIDCUpd.ResponseAttributes = "";
                    objUserManagementIDCUpd.CallApiStatus = "";
                    objUserManagementIDCUpd.CallApiReqRecordSl = 0;
                    objUserManagementIDCUpd.CallApiResponseCode = "";
                    objUserManagementIDCUpd.CallApiResponseMsg = "";

                    objUserManagementIDCUpd.CreatedBy = objUserManagementMTFind.CreatedBy;
                    objUserManagementIDCUpd.CreatedDate = objUserManagementMTFind.CreatedDate;
                    objUserManagementIDCUpd.ModifiedBy = objUserManagementMTFind.ModifiedBy;
                    objUserManagementIDCUpd.ModifiedDate = objUserManagementMTFind.ModifiedDate;
                    objUserManagementIDCUpd.ApproverBy = objUserManagementMTFind.ApproverBy;
                    objUserManagementIDCUpd.ApprovalDate = objUserManagementMTFind.ApprovalDate;
                    objUserManagementIDCUpd.FunctionTypeName = "";
                    if (listRoleUsers != null && listRoleUsers.Count != 0)
                    {
                        objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.ShortName).FirstOrDefault();
                        objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                        objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                        objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                        objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
                    }
                    objUserManagementIDCUpd.StartDate = objUserManagementMTFind.StartDate.Value;
                    objUserManagementIDCUpd.IpSetCode = objUserManagementMTFind.IpSetCode;
                    objUserManagementIDCUpd.IpSetDetail = objUserManagementMTFind.IpSetDetail;
                    objUserManagementIDCUpd.RestrictionFlag = 0;
                    objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                    objUserManagementIDCUpd.SubType = objUserManagementMTFind.SubType;
                    objUserManagementIDCUpd.AuthsecTypeName = objUserManagementMTFind.AuthsecTypeName;
                    objUserManagementIDCUpd.MailIdFlagName = objUserManagementMTFind.MailIdFlagName;
                    objUserManagementIDCUpd.CallApiAutoGeneratedPassword = "";

                    objUserManagementIDCUpd.PosCodeOld = objUserManagementMTFind.PosCode;
                    objUserManagementIDCUpd.PosNameOld = objUserManagementMTFind.PosName;
                    objUserManagementIDCUpd.GroupNameOld = objUserManagementMTFind.GroupName;
                    objUserManagementIDCUpd.FirstNameOld = objUserManagementMTFind.FirstName;
                    objUserManagementIDCUpd.LastNameOld = objUserManagementMTFind.LastName;
                    objUserManagementIDCUpd.FullNameOld = objUserManagementMTFind.FullName;
                    objUserManagementIDCUpd.EmailAddressOld = objUserManagementMTFind.EmailAddress;
                    objUserManagementIDCUpd.MobileNumberOld = objUserManagementMTFind.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirthOld = objUserManagementMTFind.DateOfBirth;
                    objUserManagementIDCUpd.GroupNameOldText = objUserManagementIDCUpd.GroupNameText;
                    objUserManagementIDCUpd.RoleToTransferCashValueOld = objUserManagementIDCUpd.RoleToTransferCashValue;
                    objUserManagementIDCUpd.RoleToTransferCashNameOld = objUserManagementIDCUpd.RoleToTransferCashName;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = objUserManagementIDCUpd.RoleToTransferCashDescription;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail;
                    objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
                    objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDate.ToString(FormatParameters.FORMAT_DATE);

                    objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
                    objUserManagementIDCUpd.EndDateChangeRole = DateTime.Now.Date;// CustConverter.StringToDateTime(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).Date;
                    objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
                    objUserManagementIDCUpd.GenderCode = objUserManagementMTFind.GenderCode;
                    objUserManagementIDCUpd.GenderText = objUserManagementMTFind.GenderText;
                    objUserManagementIDCUpd.StaffPosCode = objUserManagementMTFind.StaffPosCode;
                    objUserManagementIDCUpd.StaffPosName = objUserManagementMTFind.StaffPosName;
                    objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementMTFind.StaffDepartmentCode;
                    objUserManagementIDCUpd.StaffDepartmentName = objUserManagementMTFind.StaffDepartmentName;
                    objUserManagementIDCUpd.StaffPositionCode = objUserManagementMTFind.StaffPositionCode;
                    objUserManagementIDCUpd.StaffPositionName = objUserManagementMTFind.StaffPositionName;
                    objUserManagementIDCUpd.StaffEmail = objUserManagementMTFind.StaffEmail;
                    objUserManagementIDCUpd.StaffMobileNo = objUserManagementMTFind.StaffMobileNo;
                    //Lấy theo QLNS khi thay đổi thông tin người dùng
                    objUserManagementIDCUpd.EmailAddress = objUserManagementMTFind.StaffEmail;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementMTFind.StaffMobileNo;

                    objUserManagementIDCUpd.ExistsInCore = objUserManagementMTFind.ExistsInCore;
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType != FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            {
                #region ---4. Sự kiện Chỉnh sửa thông tin bản ghi (Yêu cầu thay đổi tài khoản người dùng) ---
                var objUserManagementChangeTemp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

                if (objUserManagementChangeTemp != null && !string.IsNullOrEmpty(objUserManagementChangeTemp.UserId))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                    objUserManagementIDCUpd.Id = objUserManagementChangeTemp.Id;
                    objUserManagementIDCUpd.OrderNo = 1;
                    objUserManagementIDCUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
                    objUserManagementIDCUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

                    objUserManagementIDCUpd.PosCode = objUserManagementChangeTemp.PosCode;
                    objUserManagementIDCUpd.PosName = objUserManagementChangeTemp.PosName;
                    objUserManagementIDCUpd.StaffId = objUserManagementChangeTemp.StaffId;
                    objUserManagementIDCUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
                    objUserManagementIDCUpd.UserId = objUserManagementChangeTemp.UserId;
                    objUserManagementIDCUpd.NickName = objUserManagementChangeTemp.NickName;
                    objUserManagementIDCUpd.FirstName = objUserManagementChangeTemp.FirstName;
                    objUserManagementIDCUpd.LastName = objUserManagementChangeTemp.LastName;
                    objUserManagementIDCUpd.FullName = objUserManagementChangeTemp.FullName;
                    objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
                    objUserManagementIDCUpd.GroupName = objUserManagementChangeTemp.GroupName;
                    objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                    objUserManagementIDCUpd.AuthType = objUserManagementChangeTemp.AuthType;
                    objUserManagementIDCUpd.UserType = objUserManagementChangeTemp.UserType;
                    objUserManagementIDCUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
                    objUserManagementIDCUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
                    objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
                    objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
                    objUserManagementIDCUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
                    objUserManagementIDCUpd.BusinessDate = objUserManagementChangeTemp.BusinessDate;//_serviceTransPoint.GetDateInCoreIDC("1").Date;
                    objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                    objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
                    objUserManagementIDCUpd.Ticket = objUserManagementChangeTemp.Ticket;
                    objUserManagementIDCUpd.Remark = objUserManagementChangeTemp.Remark;
                    objUserManagementIDCUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
                    objUserManagementIDCUpd.Status = objUserManagementChangeTemp.Status;
                    objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
                    objUserManagementIDCUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
                    if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                    else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                        objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                    else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                        objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
                    else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                    objUserManagementIDCUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
                    objUserManagementIDCUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
                    objUserManagementIDCUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
                    objUserManagementIDCUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
                    objUserManagementIDCUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
                    objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
                    objUserManagementIDCUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
                    objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

                    objUserManagementIDCUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
                    objUserManagementIDCUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
                    objUserManagementIDCUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
                    objUserManagementIDCUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
                    objUserManagementIDCUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
                    objUserManagementIDCUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

                    if (listRoleUsers != null && listRoleUsers.Count != 0)
                    {
                        objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
                        objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                        objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                        objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                        objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";

                        objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();

                    }
                    objUserManagementIDCUpd.StartDate = objUserManagementChangeTemp.StartDate;
                    objUserManagementIDCUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
                    objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
                    objUserManagementIDCUpd.RestrictionFlag = 0;
                    objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                    objUserManagementIDCUpd.SubType = objUserManagementChangeTemp.SubType;
                    objUserManagementIDCUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
                    objUserManagementIDCUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
                    objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

                    objUserManagementIDCUpd.PosCodeOld = objUserManagementChangeTemp.PosCodeOld;
                    objUserManagementIDCUpd.PosNameOld = objUserManagementChangeTemp.PosNameOld;
                    objUserManagementIDCUpd.GroupNameOld = objUserManagementChangeTemp.GroupNameOld;
                    objUserManagementIDCUpd.FirstNameOld = objUserManagementChangeTemp.FirstNameOld;
                    objUserManagementIDCUpd.LastNameOld = objUserManagementChangeTemp.LastNameOld;
                    objUserManagementIDCUpd.FullNameOld = objUserManagementChangeTemp.FullNameOld;
                    objUserManagementIDCUpd.EmailAddressOld = objUserManagementChangeTemp.EmailAddressOld;
                    objUserManagementIDCUpd.MobileNumberOld = objUserManagementChangeTemp.MobileNumberOld;
                    objUserManagementIDCUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
                    objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameOldText : objUserManagementIDCUpd.GroupNameOldText;
                    objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
                    objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
                    objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
                    objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

                    //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
                    objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
                    objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
                    int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
                    if (numberDays <= 90)
                        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

                    objUserManagementIDCUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
                    objUserManagementIDCUpd.GenderText = objUserManagementChangeTemp.GenderText;
                    objUserManagementIDCUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
                    objUserManagementIDCUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
                    objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
                    objUserManagementIDCUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
                    objUserManagementIDCUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
                    objUserManagementIDCUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
                    objUserManagementIDCUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
                    objUserManagementIDCUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
                    //Lấy theo QLNS khi thay đổi thông tin người dùng
                    objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
                    objUserManagementIDCUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
                }

                #endregion

                sNameView = "CreateChangeInforUserManagementIDC";
            }
            else if (pFlagCall == EventFlag.EventFlag_Approval.Value.ToString() || pFlagCall == EventFlag.EventFlag_Authorize.Value.ToString())
            {
                #region ---4. Sự kiện gọi Form Trình duyệt/Phê duyệt yêu cầu người dùng Intellect IDC ---
                var objUserManagementChangeTemp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

                if (objUserManagementChangeTemp != null && !string.IsNullOrEmpty(objUserManagementChangeTemp.UserId))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                    objUserManagementIDCUpd.Id = objUserManagementChangeTemp.Id;
                    objUserManagementIDCUpd.OrderNo = 1;
                    objUserManagementIDCUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
                    objUserManagementIDCUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

                    objUserManagementIDCUpd.PosCode = objUserManagementChangeTemp.PosCode;
                    objUserManagementIDCUpd.PosName = objUserManagementChangeTemp.PosName;
                    objUserManagementIDCUpd.StaffId = objUserManagementChangeTemp.StaffId;
                    objUserManagementIDCUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
                    objUserManagementIDCUpd.UserId = objUserManagementChangeTemp.UserId;
                    objUserManagementIDCUpd.NickName = objUserManagementChangeTemp.NickName;
                    objUserManagementIDCUpd.FirstName = objUserManagementChangeTemp.FirstName;
                    objUserManagementIDCUpd.LastName = objUserManagementChangeTemp.LastName;
                    objUserManagementIDCUpd.FullName = objUserManagementChangeTemp.FullName;
                    objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
                    objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
                    objUserManagementIDCUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
                    objUserManagementIDCUpd.GroupName = objUserManagementChangeTemp.GroupName;
                    objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

                    objUserManagementIDCUpd.AuthType = objUserManagementChangeTemp.AuthType;
                    objUserManagementIDCUpd.UserType = objUserManagementChangeTemp.UserType;
                    objUserManagementIDCUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
                    objUserManagementIDCUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
                    objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
                    objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
                    objUserManagementIDCUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
                    objUserManagementIDCUpd.BusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date; //objUserManagementChangeTemp.BusinessDate;
                    objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.SystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                    objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
                    objUserManagementIDCUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
                    objUserManagementIDCUpd.Ticket = objUserManagementChangeTemp.Ticket;
                    objUserManagementIDCUpd.Remark = objUserManagementChangeTemp.Remark;
                    objUserManagementIDCUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
                    objUserManagementIDCUpd.Status = objUserManagementChangeTemp.Status;
                    objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
                    objUserManagementIDCUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
                    if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                        objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
                    else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                        objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
                    else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                        objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
                    else objUserManagementIDCUpd.UserStatusText = "Không xác định";

                    objUserManagementIDCUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
                    objUserManagementIDCUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
                    objUserManagementIDCUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
                    objUserManagementIDCUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
                    objUserManagementIDCUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
                    objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
                    objUserManagementIDCUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
                    objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

                    objUserManagementIDCUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
                    objUserManagementIDCUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
                    objUserManagementIDCUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
                    objUserManagementIDCUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
                    objUserManagementIDCUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
                    objUserManagementIDCUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

                    if (listRoleUsers != null && listRoleUsers.Count != 0)
                    {
                        objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
                        objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                        objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                        objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
                        objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
                        objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();
                    }
                    objUserManagementIDCUpd.StartDate = objUserManagementChangeTemp.StartDate;
                    objUserManagementIDCUpd.StartDateText = string.IsNullOrEmpty(objUserManagementChangeTemp.StartDateText) ? objUserManagementChangeTemp.StartDate.ToString(FormatParameters.FORMAT_DATE) : objUserManagementChangeTemp.StartDateText;
                    objUserManagementIDCUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
                    objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
                    objUserManagementIDCUpd.RestrictionFlag = 0;
                    objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

                    objUserManagementIDCUpd.SubType = objUserManagementChangeTemp.SubType;
                    objUserManagementIDCUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
                    objUserManagementIDCUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
                    objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

                    objUserManagementIDCUpd.PosCodeOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosCodeOld) ? objUserManagementChangeTemp.PosCode : objUserManagementChangeTemp.PosCodeOld;
                    objUserManagementIDCUpd.PosNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosNameOld) ? objUserManagementChangeTemp.PosName : objUserManagementChangeTemp.PosNameOld;
                    objUserManagementIDCUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.GroupNameOld) ? objUserManagementChangeTemp.GroupName : objUserManagementChangeTemp.GroupNameOld;
                    objUserManagementIDCUpd.FirstNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FirstNameOld) ? objUserManagementChangeTemp.FirstName : objUserManagementChangeTemp.FirstNameOld;
                    objUserManagementIDCUpd.LastNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.LastNameOld) ? objUserManagementChangeTemp.LastName : objUserManagementChangeTemp.LastNameOld;
                    objUserManagementIDCUpd.FullNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FullNameOld) ? objUserManagementChangeTemp.FullName : objUserManagementChangeTemp.FullNameOld;
                    objUserManagementIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserManagementChangeTemp.EmailAddressOld) ? objUserManagementChangeTemp.EmailAddress : objUserManagementChangeTemp.EmailAddressOld;
                    objUserManagementIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserManagementChangeTemp.MobileNumberOld) ? objUserManagementChangeTemp.MobileNumber : objUserManagementChangeTemp.MobileNumberOld;
                    objUserManagementIDCUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
                    objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameText : objUserManagementIDCUpd.GroupNameOldText;
                    objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
                    objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
                    objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
                    objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
                    objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

                    //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
                    objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
                    objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
                    int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
                    if (numberDays <= 90)
                        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

                    objUserManagementIDCUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
                    objUserManagementIDCUpd.GenderText = objUserManagementChangeTemp.GenderText;
                    objUserManagementIDCUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
                    objUserManagementIDCUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
                    objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
                    objUserManagementIDCUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
                    objUserManagementIDCUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
                    objUserManagementIDCUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
                    objUserManagementIDCUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
                    objUserManagementIDCUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
                    //Lấy theo QLNS khi thay đổi thông tin người dùng
                    //objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
                    //objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
                    objUserManagementIDCUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
                }

                #endregion

                sNameView = "AuthorizeUserManagementIDC";
            }
            objUserManagementIDCUpd.FlagCall = pFlagCall;
            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() && pId == 0
                    && (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code || pButtonType == ""))
                sNameView = "UpdateUserManagementIDC";
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pId != 0 && pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                sNameView = "UpdateUserManagementIDC";
            else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString())
            {
                if (string.IsNullOrEmpty(pButtonType) || pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                {
                    //Xem chi tiết thông tin tài khoản người dùng Intellect iDC => Lấy thông tin trong UserIDCMaster, sau đó lấy tiếp trong Intellect IDC để gộp thành thông tin mới nhất
                    sNameView = "UpdateUserManagementIDC";
                }
                else
                {
                    sNameView = "CreateChangeInforUserManagementIDC";
                    pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
                }
            }
            else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
            {
                sNameView = "CreateChangeInforUserManagementIDC";
                pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
            }
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType != FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            {
                sNameView = "CreateChangeInforUserManagementIDC";
                pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
            }
            TempData["FunctionTypeFlag_ADDNEW_USER"] = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
            TempData["FunctionTypeFlag_ResetPassword"] = FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code;
            TempData["FunctionTypeFlag_ENABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code;
            TempData["FunctionTypeFlag_DISABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code;
            TempData["FunctionTypeFlag_MODIFY_USER"] = FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code;
            TempData["FunctionTypeFlag_CHANGE_POS"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code;
            TempData["FunctionTypeFlag_CHANGE_ROLE"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code;
            TempData["FunctionTypeFlag_DELETE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code;

            TempData["EventFlag_EditIDC"] = EventFlag.EventFlag_EditIDC.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();
            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();

            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_Reject"] = EventFlag.EventFlag_Reject.Value.ToString();
            TempData["UserGrade"] = UserGrade;

            ViewBag.FunctionTypes = FunctionTypeFlag.GetOption();
            ViewBag.MailIdFlags = MailIdFlag.GetAll();
            ViewBag.AuthSecTypes = AuthSecType.GetAll();
            TempData["FlagEventCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["ButtonType"] = pButtonType;
            return PartialView(sNameView, objUserManagementIDCUpd);
        }

        /// <summary>
        /// Hàm thực hiện kiểm tra thông tin người dùng Intellect IDC trước khi lưu vào bảng UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementIDCUpdate">Thông tin cập nhật theo model UserManagementIDCViewModel</param>
        /// <param name="pFlagCall">Cờ xác định cập nhật Thêm/Sửa. 
        ///                                 1 - Thêm mới bản ghi (EventFlag.EventFlag_Add.Value);
        ///                                 2 - Chỉnh sửa thông tin bản ghi (EventFlag.EventFlag_Edit.Value)
        ///                                 6 - Tạo mới/Chỉnh sửa bản ghi Yêu cầu nghiệp vụ (EventFlag.EventFlag_EditIDC.Value)
        /// </param>
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
        ///             5 - Tài khoản người dùng có phương thức xác thực thứ 2 là OTP nên không thể thực hiện tạo yêu cầu cấp lại mật khẩu
        ///             6 - Tài khoản người dùng cần khóa đã mở sổ tiền mặt đầu ngày
        ///             7 - Tài khoản người dùng có yêu cầu nghiệp vụ thay đổi thông tin nhưng thông tin (Quyền/Số điện thoại/Email) vẫn giữ nguyên
        ///             8 - Ngày bắt đầu nhỏ hơn ngày hiện tại
        ///             9 - Trạng thái người dùng đã được đóng, không thể thực hiện nghiệp vụ mở lại người dùng
        ///             11 - Địa chỉ email của người dùng bị trống hoặc không phải email nội bộ của NHCSXH
        ///             10 - Yêu cầu nghiệp vụ cấp mới tài khoản người dùng đã tồn tại trên hệ thống Intlelect iDC
        ///             16 - Yêu cầu nghiệp vụ thay đổi tài khoản người dùng nhưng tài khoản người dùng không tồn tại trên hệ thống Intlelect iDC
        ///             17 - Tài khoản người dùng khóa, nên không thể thực hiện yêu cầu nghiệp vụ Khóa tài khoản
        ///             18 - Tài khoản người dùng có trạng thái khác đóng, không thể thực hiện yêu cầu nghiệp vụ mở lại người dùng
        ///             19 - Tài khoản người dùng có ngày hết hiệu lực nhỏ hơn ngày hiện thời của Intellect, không thể thực hiện yêu cầu nghiệp vụ mở lại tài khoản
        ///             20 - Tài khoản người dùng có ngày hết hiệu lực nhỏ hơn ngày hiện thời, không thể thực hiện yêu cầu nghiệp vụ mở lại tài khoản
        ///             21 - Yêu cầu nghiệp vụ thay đổi quyền tài khoản người dùng có ngày bắt đầu lớn hơn ngày kết thúc
        ///             22 - Yêu cầu nghiệp vụ thay đổi POS tài khoản người dùng nhưng đơn vị mới thay đổi có giá trị như đơn vị cũ
        /// </returns>
        public async Task<int> IsValidSaveUserManagementIDC(UserManagementIDCViewModel pUserManagementIDCUpdate, string pFlagCall)
        {
            int iResult = 0;
            try
            {
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.PosCode))
                    return 1;
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.StaffCode))
                    return 2;
                if (pUserManagementIDCUpdate.Status == StatusTrans.Status_Closed.Value && pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    return 15;
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.MobileNumber))
                    return 14;
                if (pUserManagementIDCUpdate.StartDate.Date < pUserManagementIDCUpdate.BusinessDate.Date)
                    return 13;
                if (pUserManagementIDCUpdate.StartDate.Date < DateTime.Now.Date)
                    return 8;
                if (string.IsNullOrEmpty(pUserManagementIDCUpdate.EmailAddress))
                    return 23;
                if (!Utils.Utilities.IsValidEmail(pUserManagementIDCUpdate.EmailAddress))
                    return 24;
                pUserManagementIDCUpdate.MobileNumber = string.IsNullOrEmpty(pUserManagementIDCUpdate.MobileNumber) ? "" : pUserManagementIDCUpdate.MobileNumber.Replace(" ", "").Replace("_", "").Replace("-", "").Trim();
                if (!Utils.Utilities.IsValidMobileRegex(pUserManagementIDCUpdate.MobileNumber))
                    return 25;

                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                {
                    if (pUserManagementIDCUpdate.EffectiveDate.Date < pUserManagementIDCUpdate.BusinessDate.Date)
                        return 12;
                    if (pUserManagementIDCUpdate.EffectiveDate.Date == pUserManagementIDCUpdate.ExpiryDate.Date)
                        return 3;
                    if (pUserManagementIDCUpdate.EffectiveDate.Date >= pUserManagementIDCUpdate.ExpiryDate.Date)
                        return 4;
                }
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code)
                {
                    if (pUserManagementIDCUpdate.UserStatus != DefaultValue.UserIDC_UserStatus_Open)
                        return 17;
                }
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code)
                {
                    if (pUserManagementIDCUpdate.UserStatus != DefaultValue.UserIDC_UserStatus_Closed)
                        return 18;
                    if (pUserManagementIDCUpdate.ExpiryDate.Date < pUserManagementIDCUpdate.BusinessDate.Date)
                        return 19;
                    if (pUserManagementIDCUpdate.ExpiryDate.Date < DateTime.Now.Date)
                        return 20;
                }
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                {
                    if (pUserManagementIDCUpdate.ChoiceEndDateChangeRole == 1)
                    {
                        if (pUserManagementIDCUpdate.EndDateChangeRole.Date < pUserManagementIDCUpdate.StartDate.Date)
                            return 21;      //21 - Yêu cầu nghiệp vụ thay đổi quyền tài khoản người dùng có ngày bắt đầu lớn hơn ngày kết thúc
                    }
                }
                if (pUserManagementIDCUpdate.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code
                    && pUserManagementIDCUpdate.PosCode == pUserManagementIDCUpdate.PosCodeOld)
                {
                    return 22;
                }

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

                if ((objViewUserIDCByApi == null || string.IsNullOrEmpty(objViewUserIDCByApi.UserId))
                                && (pUserManagementIDCUpdate.FunctionType != FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code))
                    return 16;
                //Kiểm tra đã tồn tại bản ghi yêu cầu với tài khoản người dùng chưa?
                var listUserIDCManagementTmp = await _userManagementIDCService.GetListUserIDCManagement(0, "", "", pUserManagementIDCUpdate.UserId, "", "", -1, pUserManagementIDCUpdate.FunctionType, false);
                if (listUserIDCManagementTmp != null && listUserIDCManagementTmp.Count != 0)
                {
                    var objUserIDCManagementExists = listUserIDCManagementTmp.Where(w => w.Status != StatusBusinessFlow.Status_Closed.Value
                                    && w.Status != StatusBusinessFlow.Status_Branch_Rejected.Value && w.Status != StatusBusinessFlow.Status_HeadOffice_Rejected.Value
                                    && w.EffectiveDate == pUserManagementIDCUpdate.EffectiveDate.Date
                                    && w.ExpiryDate == pUserManagementIDCUpdate.ExpiryDate.Date
                                    && w.StartDate == pUserManagementIDCUpdate.StartDate.Date
                                    && w.GroupName == pUserManagementIDCUpdate.GroupName
                                    && w.PosCode == pUserManagementIDCUpdate.PosCode
                                    && w.MobileNumber == pUserManagementIDCUpdate.MobileNumber
                                    && w.EmailAddress == pUserManagementIDCUpdate.EmailAddress
                                    && (pUserManagementIDCUpdate.Id == 0 || w.Id == pUserManagementIDCUpdate.Id)
                                    ).OrderByDescending(o => o.Id).FirstOrDefault();
                    if (objUserIDCManagementExists != null && !string.IsNullOrEmpty(objUserIDCManagementExists.UserId))
                        return 26;
                }
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
                var resultCheck = await IsValidSaveUserManagementIDC(objUserIDCUpd, objUserIDCUpd.FlagCall);
                result = resultCheck.ToString();
                if (result == "0" && objUserIDCUpd != null && ModelState.IsValid)
                {
                    objUserIDCUpd.PosCode = string.IsNullOrEmpty(objUserIDCUpd.PosCode) ? "" : objUserIDCUpd.PosCode;
                    objUserIDCUpd.PosName = string.IsNullOrEmpty(objUserIDCUpd.PosName) ? "" : objUserIDCUpd.PosName.Replace(" - ","").Replace("PGD NHCSXH ","PGD ");
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
                    objUserIDCUpd.MobileNumber = string.IsNullOrEmpty(objUserIDCUpd.MobileNumber) ? "" : objUserIDCUpd.MobileNumber.Replace(" ", "").Replace("_", "").Replace("-", "");
                    objUserIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserIDCUpd.MobileNumberOld) ? "" : objUserIDCUpd.MobileNumberOld.Replace(" ", "").Replace("_", "").Replace("-", "");

                    objUserIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserIDCUpd.EmailAddressOld) ? objUserIDCUpd.EmailAddress : objUserIDCUpd.EmailAddressOld;
                    objUserIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserIDCUpd.MobileNumberOld) ? objUserIDCUpd.MobileNumber: objUserIDCUpd.MobileNumberOld;
                    if (objUserIDCUpd.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                        objUserIDCUpd.DateOfBirthOld = objUserIDCUpd.DateOfBirth;
                    objUserIDCUpd.DateOfBirthOld = objUserIDCUpd.DateOfBirthOld.ToString(FormatParameters.FORMAT_DATE) == "01/01/0001" ?
                        objUserIDCUpd.DateOfBirth : objUserIDCUpd.DateOfBirthOld;
                    objUserIDCUpd.Remark = string.IsNullOrEmpty(objUserIDCUpd.Remark) ? "" : objUserIDCUpd.Remark;
                    objUserIDCUpd.OrtherNotes = string.IsNullOrEmpty(objUserIDCUpd.OrtherNotes) ? "" : objUserIDCUpd.OrtherNotes;

                    objUserIDCUpd.IpSetCode = string.IsNullOrEmpty(objUserIDCUpd.IpSetCode) ? "" : objUserIDCUpd.IpSetCode;
                    objUserIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserIDCUpd.IpSetDetail) ? "" : objUserIDCUpd.IpSetDetail;

                    objUserIDCUpd.Ticket = string.IsNullOrEmpty(objUserIDCUpd.Ticket) ? "" : objUserIDCUpd.Ticket;
                    objUserIDCUpd.UserType = string.IsNullOrEmpty(objUserIDCUpd.UserType) ? DefaultValue.UserIDC_SubType : objUserIDCUpd.UserType;
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


        /// <summary>
        /// Menu Quản lý người dùng iDC => Hàng chờ phê duyệt
        /// </summary>
        /// <returns></returns>
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
            TempData["UserGrade"] = UserGrade;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_Reject"] = EventFlag.EventFlag_Reject.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();
            TempData["EventFlag_EditIDC"] = EventFlag.EventFlag_EditIDC.Value.ToString();

            TempData["FunctionTypeFlag_ADDNEW_USER"] = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
            TempData["FunctionTypeFlag_ResetPassword"] = FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code;
            TempData["FunctionTypeFlag_ENABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code;
            TempData["FunctionTypeFlag_DISABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code;
            TempData["FunctionTypeFlag_MODIFY_USER"] = FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code;
            TempData["FunctionTypeFlag_CHANGE_POS"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code;
            TempData["FunctionTypeFlag_CHANGE_ROLE"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code;
            TempData["FunctionTypeFlag_DELETE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code;

            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll(true);

            return View("IndexApproveUserManagementIDC");
        }


        /// <summary>
        /// Hàm lấy tổng hợp các yêu cầu nghiệp vụ về tài khoản người dùng Intellect iDC để liệt kê tổng hợp Hàng chờ phê duyệt
        /// </summary>
        /// <returns>Tổng hợp các yêu cầu nghiệp vụ về tài khoản người dùng Intellect iDC </returns>
        public ActionResult LoadGridData_UserManagementIDC_SumRequirements([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFromStartDate, string pToStartDate, string pFunctionType)
        {
            try
            {
                string sListStatus = $"{StatusBusinessFlow.Status_Created.Value},{StatusBusinessFlow.Status_Modified.Value},{StatusBusinessFlow.Status_Submitted.Value}";
                
                if (string.IsNullOrEmpty(pFunctionType))
                    pFunctionType = "";
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = "";
                string sPosCode = "", sMainPosCode = "";

                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS)
                    && (pPosCode != "000100" && pPosCode != "000199" && pPosCode != "000196" && pPosCode != "000197" && pPosCode != "000101"))
                {
                    if (!string.IsNullOrEmpty(pPosCode) && pPosCode == UserPosCode)
                    {
                        sMainPosCode = pPosCode;
                        pPosCode = "";
                    }
                }
                DateTime dSystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
                string sSystemDateText = dSystemDate.ToString(FormatParameters.FORMAT_DATE);

                var listSumRequirementUserIDC = _userManagementIDCService.UserManagementIDC_SumRequirement_GetSearch(pFromStartDate, pToStartDate, sMainPosCode, sPosCode, sListStatus, 1);
                return Json(listSumRequirementUserIDC.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, ex.Message);
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCSumRequirementViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm gọi Show màn hình Trình duyệt/Phê duyệt yêu cầu nghiệp vụ về tài khoản người dùng Intellect IDC
        /// </summary>
        /// <param name="pMainPosCode">Mã chi nhánh</param>
        /// <param name="pFlagCall">Sự kiện: Trình duyệt - EventFlag.EventFlag_Approval.Value; Phê duyệt - EventFlag.EventFlag_Authorize.Value</param>
        /// <param name="pButtonType">Chưa sử dụng</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public async Task<ActionResult> ShowApprovalOrAuthorizeUserManagementIDC(string pMainPosCode, string pFlagCall, string pButtonType)
        {
            UserManagementIDCViewModel objPosUserIDCManagement = new UserManagementIDCViewModel();

            if (string.IsNullOrEmpty(pMainPosCode))
                pMainPosCode = "";
            if (string.IsNullOrEmpty(pButtonType))
                pButtonType = "";
            DateTime dSystemDate = _serviceTransPoint.GetDateInCoreIDC("0").Date;
            string sSystemDateText = dSystemDate.ToString(FormatParameters.FORMAT_DATE);
            DateTime dBusinessDate = _serviceTransPoint.GetDateInCoreIDC("1").Date;
            string sBusinessDateText = dBusinessDate.ToString(FormatParameters.FORMAT_DATE);


            string sNameView = "";
            var listStaffVBSP = (await _userManagementIDCService.GetListUserIDCManagement(0, "", pMainPosCode, "", "", "", 0, "", true)).FirstOrDefault();
            sNameView = (pFlagCall == EventFlag.EventFlag_Approval.Value.ToString()) ? "ApproveUserManagementIDC" : "ApproveUserManagementIDC";
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["UserGrade"] = UserGrade;
            TempData["ButtonType"] = pButtonType;
            TempData["MainPosCode"] = pMainPosCode;
            TempData["SystemDateText"] = sSystemDateText;
            TempData["BusinessDateText"] = sBusinessDateText;
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll(false);
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();

            TempData["FunctionTypeFlag_ADDNEW_USER"] = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
            TempData["FunctionTypeFlag_ResetPassword"] = FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code;
            TempData["FunctionTypeFlag_ENABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code;
            TempData["FunctionTypeFlag_DISABLE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code;
            TempData["FunctionTypeFlag_MODIFY_USER"] = FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code;
            TempData["FunctionTypeFlag_CHANGE_POS"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code;
            TempData["FunctionTypeFlag_CHANGE_ROLE"] = FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code;
            TempData["FunctionTypeFlag_DELETE_USER"] = FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code;

            return PartialView(sNameView, objPosUserIDCManagement);
        }

        /// <summary>
        /// Hàm thực hiện lưu thông tin và thực thi cho trường hợp Trình duyệt hoặc Phê duyệt yêu cầu về Tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pFlagCall">Sự kiện: Trình duyệt - EventFlag.EventFlag_Approval.Value; Phê duyệt - EventFlag.EventFlag_Authorize.Value</param>
        /// <param name="pListApprovalData">Mảng dữ liệu danh sách Id (Cách nhau bởi dấu phẩy), UserId, Status</param>
        /// <param name="pFileUpload">Danh sách file upload (tờ trình) với trường hợp trình duyệt</param>
        /// <param name="pFunctionType">Nghiệp vụ thực hiện: Thêm/Thay đổi quyền.... Giá trị lấy theo:
        ///                         Thêm mới người dùng: FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
        ///                         Cấp lại mật khẩu: FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code;
        ///                         Mở lại người dùng: FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code;
        ///                         Khóa người dùng: FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code;
        ///                         Thay đổi thông tin người dùng: FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code;
        ///                         Thay đổi POS người dùng: FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code;
        ///                         Thay đổi quyền/vài trò người dùng: FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code;
        ///                         Hủy người dùng: FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code;
        /// </param>
        /// <param name="pMainPosCode"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdateApprovalOrAuthorize([DataSourceRequest] DataSourceRequest request, string pFlagCall, string pListApprovalData,
                    IFormFile pFileUpload, string pFunctionType, string pMainPosCode)
        {
            /*
            List<long> saveFileStatus = null;
            long iVal = 1;
            try
            {
                string sFunctionTypeNameTmp = string.IsNullOrEmpty(pFunctionType) ? "" : FunctionTypeFlag.GetByCode(pFunctionType).Description;
                string result = "0";
                var listData = JsonConvert.DeserializeObject<List<UserManagementIDCViewModel>>(pListApprovalData);
                if (listData == null || !listData.Any() || listData.Count <= 0)
                {
                    if (pFlagCall == EventFlag.EventFlag_Approval.Value.ToString())
                        return new JsonResult($"Không có dữ liệu danh sách yêu cầu [{sFunctionTypeNameTmp}] về tài khoản người dùng cần trình duyệt. Vui lòng kiểm tra lại!");
                    else if (pFlagCall == EventFlag.EventFlag_Authorize.Value.ToString())
                        return new JsonResult($"Không có dữ liệu danh sách yêu cầu [{sFunctionTypeNameTmp}] về tài khoản người dùng cần phê duyệt. Vui lòng kiểm tra lại!");
                }
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
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "ToTrinh");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var fileName = _userManagementIDCService.GetFileNameNewUpload(0, pFunctionType, "", DateTime.Now) + extension;
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
             */
            return null;
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
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll(true);
            return View("IndexUserIDCMaster");
        }



        /// <summary>
        /// Hàm lấy danh sách lên lưới dữ liệu Danh sách người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
        /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public async Task<ActionResult> LoadGridData_UserIDCMaster([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFromEffectiveDate, string pToEffectiveDate, string pUserId, int pStatus,string pFullName)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pUserId))
                    pUserId = "";
                if (string.IsNullOrEmpty(pFullName))
                    pFullName = "";
                var listStaffVBSP = await _userManagementIDCService.GetListUserIDCMasters(0,"",pPosCode, pUserId, pFullName, "",pStatus,false);

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
