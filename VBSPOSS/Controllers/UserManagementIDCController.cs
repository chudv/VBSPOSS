using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        /// Menu: Tạo mới và thay đổi thông tin người dùng
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
            TempData["ProductGroupCode"] = ProductGroupCode.ProductGroupCode_DepositPenal;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();
            ViewBag.FunctionTypes = FunctionTypeFlag.GetAll();
            return View("IndexUserManagementIDC");
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

        /// <summary>
        /// Danh sách bản ghi Tạo mới/Thay đổi thông tin,... người dùng iDC => Tải dừ bảng dữ liệu UserIDCManagement
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pUserId">Mã UserId</param>
        /// <param name="pFunctionType">Loại chức năng chọn</param>
        /// <param name="pFullName">Họ tên người dùng tìm kiếm</param>
        /// <param name="pStatus">Trạng thái</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult LoadGridData_UserIDCManagement([DataSourceRequest] DataSourceRequest request,string pMainPosCode, string pPosCode, string pFunctionType, string pUserId, int pStatus,
                                                            string pFullName)
        {
            try
            {
                if (string.IsNullOrEmpty(pMainPosCode))
                    pMainPosCode = "";
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pUserId))
                    pUserId = "";
                if (string.IsNullOrEmpty(pFullName))
                    pFullName = "";
                if (string.IsNullOrEmpty(pFunctionType))
                    pFunctionType = "";
                if(pMainPosCode == pPosCode)
                    pPosCode = "";
                var listStaffVBSP = _userManagementIDCService.GetListUserIDCManagement(0, pMainPosCode, pPosCode, pUserId, pFullName, "", pFunctionType, pStatus);
                return Json(listStaffVBSP.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_UserIDCManagement('{pPosCode}','{pFunctionType}','{pUserId}',{pStatus},'{pFullName}') => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
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
        /// Hàm show màn hình Thêm/Sửa/Thay đổi POS, Quyền/Cấp lại mật khẩu... người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pUserId">Mã UserId</param>
        /// <param name="pFlagCall"></param>
        /// <param name="pFullName">Họ tên người dùng tìm kiếm</param>
        /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
        ///             1 - Thêm mới
        ///             2 - Chỉnh sửa
        ///             8 - Phê duyệt
        ///             9 - Trình duyệt
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult ShowUpdateUserManagementIDC(long pId,string pPosCode, string pUserId, string pFlagCall, string pFullName, string pButtonType)
        {
            UserManagementIDCViewModel objPosUserIDCMaster = new UserManagementIDCViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            string sNameView = "";
            var listStaffVBSPMaster = _userManagementIDCService.GetListUserIDCMasters(0, "", pPosCode, pUserId, pFullName, "",3).FirstOrDefault();
            var listStaffVBSP = (_userManagementIDCService.GetListUserIDCManagement(pId,"",pPosCode, pUserId,pFullName, "","",-1)).FirstOrDefault();
            if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Value.ToString())
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
                objPosUserIDCMaster.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");
                objPosUserIDCMaster.AuthType = "";
                objPosUserIDCMaster.UserType = "";
                objPosUserIDCMaster.MailIdFlag = MailIdFlag.MailIdFlag_DefaultPassword.Code;
                objPosUserIDCMaster.AuthsecType = AuthSecType.AuthSecType_Single.Code;
                objPosUserIDCMaster.ExtraAttributeUserRole = "";
                objPosUserIDCMaster.ExtraAttributeBranchCode = "";
                objPosUserIDCMaster.ExpiryDate = new DateTime(2060, 12, 31);
                objPosUserIDCMaster.EffectiveDate = DateTime.Now;
                objPosUserIDCMaster.Remark = "";
                objPosUserIDCMaster.OrtherNotes = "";
                objPosUserIDCMaster.Status = StatusBusinessFlow.Status_Created.Value;
                objPosUserIDCMaster.StatusText = StatusBusinessFlow.Status_Created.Description;               
                objPosUserIDCMaster.CreatedBy = "";
                objPosUserIDCMaster.CreatedDate = DateTime.Now;
                objPosUserIDCMaster.ModifiedBy = "";
                objPosUserIDCMaster.ModifiedDate = DateTime.Now;
                objPosUserIDCMaster.StartDate = DateTime.Now;
                objPosUserIDCMaster.IpSetCode = "";
                objPosUserIDCMaster.IpSetDetail = "";
                objPosUserIDCMaster.RestrictionFlag = 0;
                objPosUserIDCMaster.SubType = "";
                objPosUserIDCMaster.FunctionType = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
                objPosUserIDCMaster.GroupNameOld = "";
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
                objPosUserIDCMaster.ExpiryDate = listStaffVBSP.ExpiryDate.Date;
                objPosUserIDCMaster.Remark = listStaffVBSP.Remark;
                objPosUserIDCMaster.OrtherNotes = listStaffVBSP.OrtherNotes;
                objPosUserIDCMaster.Status = listStaffVBSP.Status;
                objPosUserIDCMaster.StatusText = listStaffVBSP.StatusText;
                objPosUserIDCMaster.CreatedBy = listStaffVBSP.CreatedBy;
                objPosUserIDCMaster.CreatedDate = listStaffVBSP.CreatedDate;
                objPosUserIDCMaster.ModifiedBy = listStaffVBSP.ModifiedBy;
                objPosUserIDCMaster.ModifiedDate = listStaffVBSP.ModifiedDate;
                objPosUserIDCMaster.FunctionTypeName = listStaffVBSP.FunctionTypeName;
                objPosUserIDCMaster.EffectiveDate = listStaffVBSP.EffectiveDate?.Date;
                objPosUserIDCMaster.FunctionType = listStaffVBSP.FunctionType;
                objPosUserIDCMaster.StartDate = listStaffVBSP.StartDate?.Date;
                objPosUserIDCMaster.IpSetCode = listStaffVBSP.IpSetCode;
                objPosUserIDCMaster.IpSetDetail = listStaffVBSP.IpSetDetail;
                objPosUserIDCMaster.RestrictionFlag = listStaffVBSP.RestrictionFlag;
                objPosUserIDCMaster.SubType = listStaffVBSP.SubType;
                objPosUserIDCMaster.AuthsecTypeName = listStaffVBSP.AuthsecTypeName;
                objPosUserIDCMaster.MailIdFlagName = listStaffVBSP.MailIdFlagName;
                if(listStaffVBSPMaster != null)
                    objPosUserIDCMaster.GroupNameOld = listStaffVBSPMaster.GroupName;
                else
                    objPosUserIDCMaster.GroupNameOld = listStaffVBSP.GroupName;
            }
            if(string.IsNullOrEmpty(pButtonType) || pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Value.ToString())
                sNameView = "UpdateUserManagementIDC";
            else if(pButtonType == FunctionTypeFlag.FunctionTypeFlag_APPROVAL.Value.ToString() || pButtonType == FunctionTypeFlag.FunctionTypeFlag_AUTHORIZE.Value.ToString())
                sNameView = "AuthorizeUserManagementIDC";
            else           
                sNameView = "DetailUserManagementIDC";
            ViewBag.FunctionTypes = FunctionTypeFlag.GetOption();
            ViewBag.MailIdFlags = MailIdFlag.GetAll();
            ViewBag.AuthSecTypes = AuthSecType.GetAll();
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["ButtonType"] = pButtonType;
            return PartialView(sNameView, objPosUserIDCMaster);
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
                if (objUserIDCFull.EffectiveDate?.ToString(FormatParameters.FORMAT_DATE) == objUserIDCFull.ExpiryDate.ToString(FormatParameters.FORMAT_DATE))
                    return 3;
                if (objUserIDCFull.EffectiveDate > objUserIDCFull.ExpiryDate && objUserIDCFull.ExpiryDate.ToString(FormatParameters.FORMAT_DATE) != "01/01/0001")
                    return 4;
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code && objUserIDCFull.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                    return 5;
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code ||  objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                if (objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code || objUserIDCFull.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                {
                    int iCheckOpenCash = _userManagementIDCService.CheckOpenCashByUserId(objUserIDCFull.UserId, objUserIDCFull.StartDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper());
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
                if (objUserIDCFull.StartDate?.Date < DateTime.Now.Date)
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
                    string startDate = item.StartDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper();        
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
        public async Task<IActionResult> SaveUpdate([DataSourceRequest] DataSourceRequest request, UserManagementIDCViewModel objUserIDC, string pFlagCall, string pButtonType)
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
                    long iVal = await _userManagementIDCService.SaveUserManagementIDC(objUserIDC, UserName, pFlagCall,pButtonType);
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
            var listStaffVBSP = (_userManagementIDCService.GetListUserIDCManagement(pId,"",pPosCode, pUserId,pFullName, "","",0)).FirstOrDefault();
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
