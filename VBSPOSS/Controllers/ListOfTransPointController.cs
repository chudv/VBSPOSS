using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

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
        public ActionResult LoadGridData_TransPointWorks([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, int pStatus)
        {
            try
            {
                string sTxnPointCode = "", sTxnPointName = "";
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pEventCode))
                    pEventCode = "";
                if (string.IsNullOrEmpty(pTxnPointCode))
                    pTxnPointCode = "";
                if (string.IsNullOrEmpty(pTxnPointName))
                    pTxnPointName = "";
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS) && (pPosCode != "000100" && pPosCode != "000199" && pPosCode != "000196" && pPosCode != "000197" && pPosCode != "000101"))
                {
                    if (!string.IsNullOrEmpty(pPosCode))
                        pPosCode = pPosCode.Substring(0, 4);
                }
                var listTransPointWorks = _serviceTransPoint.GetListOfTransPointSearch("", pPosCode, pTxnPointCode, pTxnPointName, -1, "", pEventCode);
                return Json(listTransPointWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_TransPointWorks('{pPosCode}','{pEventCode}','{pTxnPointCode}','{pTxnPointName}',{pStatus}) => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình nghiệp vụ Thêm mới hoặc Thay đổi thông tin bản ghi yêu cầu nghiệp vụ điểm giao dịch
        /// </summary>
        /// <param name="pButtonType">Giá trị yêu cầu. Ex: EventFlag.EventFlag_Add.Code/</param>
        /// <param name="pId">Chỉ số bản ghi của bảng ListOfTransPointWork</param>
        /// <param name="pPosCode">Mã Pos bản ghi của bảng ListOfTransPointWork</param>
        /// <param name="pUserId">Tài khoản người dùng</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực của yêu cầu nghiệp vụ của bản ghi. Định dạng: dd/MM/yyyy</param>
        /// <param name="pFlagCall">Cờ xác định: 1 - Thêm mới; 2 - Chỉnh sửa bản ghi; 9 - Thay đổi nghiệp vụ người dùng</param>
        /// <returns>Giá trị đối tượng ListOfTransPointWork</returns>
        public async Task<ActionResult> ShowUpdateListOfTransPointWork(string pButtonType, long pId, string pPosCode, string pUserId, string pEffectiveDate, string pFlagCall)
        {
            ListOfTransPointWorkViewModel objListOfTransPointWorkUpd = new ListOfTransPointWorkViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pUserId))
                pUserId = "";
            if (string.IsNullOrEmpty(pEffectiveDate))
                pEffectiveDate = CustConverter.StringToDate(DefaultValue.MinDate.ToString(), FormatParameters.FORMAT_DATE_INT).ToString(FormatParameters.FORMAT_DATE);
            DateTime dSystemDateIDCTmp = _serviceTransPoint.GetDateInCoreIDC("0").Date;
            DateTime dBusinessDateIDCTmp = _serviceTransPoint.GetDateInCoreIDC("1").Date;
            string sNameView = "";
            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())        //Trường hợp yêu cầu tạo mới điểm giao dịch
            {
                #region ---1. Sự kiện thêm mới bản ghi Yêu cầu tạo mới điểm giao dịch ---
                objListOfTransPointWorkUpd.OrderNo = 0;
                objListOfTransPointWorkUpd.OrderNoText = "";
                objListOfTransPointWorkUpd.EventCode = EventBusinessCode.EventCode_TransPoint_AddNew.Code;
                objListOfTransPointWorkUpd.EventName = EventBusinessCode.EventCode_TransPoint_AddNew.Description;
                objListOfTransPointWorkUpd.ParentId = 0;
                objListOfTransPointWorkUpd.ProvinceCode = "";
                objListOfTransPointWorkUpd.ProvinceName = "";
                objListOfTransPointWorkUpd.PosCode = "";
                objListOfTransPointWorkUpd.PosName = "";
                objListOfTransPointWorkUpd.DistrictCode = "";
                objListOfTransPointWorkUpd.DistrictName = "";
                objListOfTransPointWorkUpd.CommuneCode = "";
                objListOfTransPointWorkUpd.CommuneName = "";
                objListOfTransPointWorkUpd.TxnPointCode = "";
                objListOfTransPointWorkUpd.TxnPointName = "";
                objListOfTransPointWorkUpd.VisitDate = DateTime.Now.Day;
                objListOfTransPointWorkUpd.VisitDateText = "";
                objListOfTransPointWorkUpd.Times = "";

                objListOfTransPointWorkUpd.TimeBegin = "";
                objListOfTransPointWorkUpd.TimeEnd = "";
                objListOfTransPointWorkUpd.TimeBeginNum = 0;
                objListOfTransPointWorkUpd.TimeEndNum = 0;
                objListOfTransPointWorkUpd.Hours = 0;
                objListOfTransPointWorkUpd.Minutes = 0;
                objListOfTransPointWorkUpd.Longitude = 0;
                objListOfTransPointWorkUpd.Latitude = 0;
                objListOfTransPointWorkUpd.IsInCommune = "";
                objListOfTransPointWorkUpd.IsInPos = "";
                objListOfTransPointWorkUpd.IsInterWard = "";
                objListOfTransPointWorkUpd.InterWardName = "";
                objListOfTransPointWorkUpd.EffectiveDate = DateTime.Now;
                objListOfTransPointWorkUpd.EffectiveDateText = "";
                objListOfTransPointWorkUpd.TxnLocation = "";
                objListOfTransPointWorkUpd.AddressDetail = "";
                objListOfTransPointWorkUpd.AddressCode = "";

                objListOfTransPointWorkUpd.AddressFull = "";
                objListOfTransPointWorkUpd.PhoneSupport = "";
                objListOfTransPointWorkUpd.PhoneSupport01 = "";
                objListOfTransPointWorkUpd.PhoneSupport02 = "";
                objListOfTransPointWorkUpd.TxnStatus = "A";
                objListOfTransPointWorkUpd.TxnStatusText = "Mở";
                objListOfTransPointWorkUpd.Status = 1;
                objListOfTransPointWorkUpd.StatusText = "Tạo lập";
                objListOfTransPointWorkUpd.Remark = "";

                objListOfTransPointWorkUpd.CreatedBy = UserName;
                objListOfTransPointWorkUpd.CreatedDate = DateTime.Now;
                objListOfTransPointWorkUpd.ModifiedBy = UserName;
                objListOfTransPointWorkUpd.ModifiedDate = DateTime.Now;
                objListOfTransPointWorkUpd.ApproverBy = UserName;
                objListOfTransPointWorkUpd.ApprovalDate = DateTime.Now;
                objListOfTransPointWorkUpd.BusinessDate = DateTime.Now;
                objListOfTransPointWorkUpd.BusinessDateText = "";
                objListOfTransPointWorkUpd.DocumentId = 0;
                objListOfTransPointWorkUpd.StatusUpdateCore = 0;
                objListOfTransPointWorkUpd.CallApiTxnStatus = "";
                objListOfTransPointWorkUpd.CallApiResRecords = 0;

                objListOfTransPointWorkUpd.CallApiResponseCode = "";
                objListOfTransPointWorkUpd.CallApiResponseMsg = "";
                objListOfTransPointWorkUpd.ProvinceCodeOldInfo = "";
                objListOfTransPointWorkUpd.ProvinceNameOldInfo = "";
                objListOfTransPointWorkUpd.PosCodeOldInfo = "";

                objListOfTransPointWorkUpd.PosNameOldInfo = "";
                objListOfTransPointWorkUpd.DistrictCodeOldInfo = "";
                objListOfTransPointWorkUpd.DistrictNameOldInfo = "";
                objListOfTransPointWorkUpd.CommuneCodeOldInfo = "";
                objListOfTransPointWorkUpd.CommuneNameOldInfo = "";
                objListOfTransPointWorkUpd.TxnPointCodeOldInfo = "";

                objListOfTransPointWorkUpd.TxnPointNameOldInfo = "";
                objListOfTransPointWorkUpd.VisitDateOldInfo = DateTime.Now.Day;
                objListOfTransPointWorkUpd.VisitDateTextOldInfo = "";
                objListOfTransPointWorkUpd.TimesOldInfo = "";
                objListOfTransPointWorkUpd.TimeBeginOldInfo = "";
                objListOfTransPointWorkUpd.TimeEndOldInfo = "";
                objListOfTransPointWorkUpd.TimeBeginNumOldInfo = 0;
                objListOfTransPointWorkUpd.TimeEndNumOldInfo = 0;
                objListOfTransPointWorkUpd.HoursOldInfo = 0;
                objListOfTransPointWorkUpd.MinutesOldInfo = 0;
                objListOfTransPointWorkUpd.LongitudeOldInfo = 0;
                objListOfTransPointWorkUpd.LatitudeOldInfo = 0;
                objListOfTransPointWorkUpd.IsInCommuneOldInfo = "";
                objListOfTransPointWorkUpd.IsInPosOldInfo = "";
                objListOfTransPointWorkUpd.IsInterWardOldInfo = "";
                objListOfTransPointWorkUpd.InterWardNameOldInfo = "";
                objListOfTransPointWorkUpd.EffectiveDateOldInfo = DateTime.Now;
                objListOfTransPointWorkUpd.TxnLocationOldInfo = "";
                objListOfTransPointWorkUpd.AddressDetailOldInfo = "";
                objListOfTransPointWorkUpd.AddressCodeOldInfo = "";
                objListOfTransPointWorkUpd.AddressFullOldInfo = "";
                objListOfTransPointWorkUpd.PhoneSupportOldInfo = "";
                objListOfTransPointWorkUpd.PhoneSupport01OldInfo = "";
                objListOfTransPointWorkUpd.PhoneSupport02OldInfo = "";
                objListOfTransPointWorkUpd.TxnStatusOldInfo = "";
                objListOfTransPointWorkUpd.TxnStatusTextOldInfo = "";
                objListOfTransPointWorkUpd.StatusOldInfo = 0;
                objListOfTransPointWorkUpd.StatusTextOldInfo = "";
                objListOfTransPointWorkUpd.RemarkOldInfo = "";
                objListOfTransPointWorkUpd.CreatedByOldInfo = "";
                objListOfTransPointWorkUpd.CreatedDateOldInfo = DateTime.Now;
                objListOfTransPointWorkUpd.ModifiedByOldInfo = "";
                objListOfTransPointWorkUpd.ModifiedDateOldInfo = DateTime.Now;
                objListOfTransPointWorkUpd.ApproverByOldInfo = "";
                objListOfTransPointWorkUpd.ApprovalDateOldInfo = DateTime.Now;
                objListOfTransPointWorkUpd.BusinessDateOldInfo = DateTime.Now;
                objListOfTransPointWorkUpd.BusinessDateTextOldInfo = "";
                objListOfTransPointWorkUpd.DocumentIdOldInfo = 0;
                sNameView = "UpdateListOfTransPointWork";
                #endregion
            }
            //else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)        //Trường hợp chỉnh sửa bản ghi yêu cầu nghiệp vụ: Bản ghi có trong bảng UserIDCManagement
            //{
            //    #region ---2. Sự kiện chỉnh sửa bản ghi Yêu cầu tạo mới tài khoản người dùng ---
            //    var objUserManagementIDCFind01 = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();
            //    if (objUserManagementIDCFind01 != null && objUserManagementIDCFind01.Id > 0 && !string.IsNullOrEmpty(objUserManagementIDCFind01.FunctionType))
            //    {
            //        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

            //        objUserManagementIDCUpd.Id = objUserManagementIDCFind01.Id;
            //        objUserManagementIDCUpd.OrderNo = objUserManagementIDCFind01.OrderNo;
            //        objUserManagementIDCUpd.FunctionType = objUserManagementIDCFind01.FunctionType;

            //        objUserManagementIDCUpd.PosCode = objUserManagementIDCFind01.PosCode;
            //        objUserManagementIDCUpd.PosName = objUserManagementIDCFind01.PosName;
            //        objUserManagementIDCUpd.StaffId = objUserManagementIDCFind01.StaffId;
            //        objUserManagementIDCUpd.StaffCode = objUserManagementIDCFind01.StaffCode;
            //        objUserManagementIDCUpd.UserId = objUserManagementIDCFind01.UserId;
            //        objUserManagementIDCUpd.NickName = objUserManagementIDCFind01.NickName;
            //        objUserManagementIDCUpd.FirstName = objUserManagementIDCFind01.FirstName;
            //        objUserManagementIDCUpd.LastName = objUserManagementIDCFind01.LastName;
            //        objUserManagementIDCUpd.FullName = objUserManagementIDCFind01.FullName;
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementIDCFind01.EmailAddress;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementIDCFind01.MobileNumber;
            //        objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCFind01.DateOfBirth;
            //        objUserManagementIDCUpd.GroupName = objUserManagementIDCFind01.GroupName;
            //        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objUserManagementIDCUpd.AuthType = objUserManagementIDCFind01.AuthType;
            //        objUserManagementIDCUpd.UserType = objUserManagementIDCFind01.UserType;
            //        objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCFind01.MailIdFlag;
            //        objUserManagementIDCUpd.AuthsecType = objUserManagementIDCFind01.AuthsecType;
            //        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCFind01.GroupName;
            //        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCFind01.PosCode;
            //        objUserManagementIDCUpd.EffectiveDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCFind01.ExpiryDate;
            //        objUserManagementIDCUpd.Ticket = objUserManagementIDCFind01.Ticket;
            //        objUserManagementIDCUpd.Remark = objUserManagementIDCFind01.Remark;
            //        objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCFind01.OrtherNotes;
            //        objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Modified.Value; //objUserManagementIDCFind01.Status;
            //        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

            //        objUserManagementIDCUpd.UserStatus = objUserManagementIDCFind01.UserStatus;
            //        if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementIDCFind01.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
            //        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //        objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCFind01.StatusUpdateCore;
            //        objUserManagementIDCUpd.SessionValReq = objUserManagementIDCFind01.SessionValReq;
            //        objUserManagementIDCUpd.PrevStatus = objUserManagementIDCFind01.PrevStatus;
            //        objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCFind01.ResponseAttributes;
            //        objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCFind01.CallApiStatus;
            //        objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCFind01.CallApiReqRecordSl;
            //        objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCFind01.CallApiResponseCode;
            //        objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCFind01.CallApiResponseMsg;

            //        objUserManagementIDCUpd.CreatedBy = objUserManagementIDCFind01.CreatedBy;
            //        objUserManagementIDCUpd.CreatedDate = objUserManagementIDCFind01.CreatedDate;
            //        objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCFind01.ModifiedBy;
            //        objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCFind01.ModifiedDate;
            //        objUserManagementIDCUpd.ApproverBy = objUserManagementIDCFind01.ApproverBy;
            //        objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCFind01.ApprovalDate;
            //        objUserManagementIDCUpd.FunctionTypeName = FunctionTypeFlag.GetByCode(objUserManagementIDCFind01.FunctionType).Description;//GetDescriptionByCode
            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCFind01.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCFind01.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
            //        }
            //        objUserManagementIDCUpd.StartDate = objUserManagementIDCFind01.StartDate;
            //        objUserManagementIDCUpd.IpSetCode = objUserManagementIDCFind01.IpSetCode;
            //        objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCFind01.IpSetDetail;
            //        objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCFind01.RestrictionFlag;
            //        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

            //        objUserManagementIDCUpd.SubType = objUserManagementIDCFind01.SubType;
            //        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCFind01.AuthsecTypeName;
            //        objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCFind01.MailIdFlagName;
            //        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCFind01.CallApiAutoGeneratedPassword;
            //        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCFind01.StaffDepartmentName;
            //        objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCFind01.PosCodeOld;
            //        objUserManagementIDCUpd.PosNameOld = objUserManagementIDCFind01.PosNameOld;
            //        objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCFind01.GroupNameOld;
            //        objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCFind01.FirstNameOld;
            //        objUserManagementIDCUpd.LastNameOld = objUserManagementIDCFind01.LastNameOld;
            //        objUserManagementIDCUpd.FullNameOld = objUserManagementIDCFind01.FullNameOld;
            //        objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCFind01.EmailAddressOld;
            //        objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCFind01.MobileNumberOld;
            //        objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCFind01.DateOfBirthOld;
            //        objUserManagementIDCUpd.ListFileId = string.IsNullOrEmpty(objUserManagementIDCFind01.ListFileId) ? "" : objUserManagementIDCFind01.ListFileId;
            //        objUserManagementIDCUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementIDCFind01.ReasonReject) ? "" : objUserManagementIDCFind01.ReasonReject;
            //    }
            //    #endregion
            //}
            //else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString() && (string.IsNullOrEmpty(pButtonType)|| pButtonType.Length > 2))
            //{
            //    #region ---3. Sự kiện xem chi tiết bản ghi Yêu cầu nghiệp vụ tài khoản người dùng ---
            //    if (string.IsNullOrEmpty(pButtonType) || pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            //    {
            //        UserManagementIDCViewModel objUserManagementIDCViewTmp = new UserManagementIDCViewModel();
            //        if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)//Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ với tài khoản người dùng Intellect iDC
            //            objUserManagementIDCViewTmp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();
            //        else
            //        {
            //            //Xem chi tiết thông tin tài khoản người dùng Intellect iDC
            //            objUserManagementIDCViewTmp = (await _userManagementIDCService.GetListUserIDCManagement(0, "", "", pUserId, "", "", -1, "", true)).FirstOrDefault();
            //        }
            //        #region --- Xem chi tiết thông tin bản ghi yêu cầu nghiệp vụ Tạo mới tài khoản người dùng Intellect iDC ---
            //        if (objUserManagementIDCViewTmp != null && !string.IsNullOrEmpty(objUserManagementIDCViewTmp.UserId))
            //        {
            //            var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
            //            objUserManagementIDCUpd.Id = objUserManagementIDCViewTmp.Id;
            //            objUserManagementIDCUpd.OrderNo = objUserManagementIDCViewTmp.OrderNo;
            //            objUserManagementIDCUpd.FunctionType = objUserManagementIDCViewTmp.FunctionType;
            //            objUserManagementIDCUpd.PosCode = objUserManagementIDCViewTmp.PosCode;
            //            objUserManagementIDCUpd.PosName = objUserManagementIDCViewTmp.PosName;
            //            objUserManagementIDCUpd.StaffId = objUserManagementIDCViewTmp.StaffId;
            //            objUserManagementIDCUpd.StaffCode = objUserManagementIDCViewTmp.StaffCode;
            //            objUserManagementIDCUpd.UserId = objUserManagementIDCViewTmp.UserId;
            //            objUserManagementIDCUpd.NickName = objUserManagementIDCViewTmp.NickName;
            //            objUserManagementIDCUpd.FirstName = objUserManagementIDCViewTmp.FirstName;
            //            objUserManagementIDCUpd.LastName = objUserManagementIDCViewTmp.LastName;
            //            objUserManagementIDCUpd.FullName = objUserManagementIDCViewTmp.FullName;
            //            objUserManagementIDCUpd.EmailAddress = objUserManagementIDCViewTmp.EmailAddress;
            //            objUserManagementIDCUpd.MobileNumber = objUserManagementIDCViewTmp.MobileNumber;
            //            objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCViewTmp.DateOfBirth;
            //            objUserManagementIDCUpd.GroupName = objUserManagementIDCViewTmp.GroupName;
            //            objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //            objUserManagementIDCUpd.AuthType = objUserManagementIDCViewTmp.AuthType;
            //            objUserManagementIDCUpd.UserType = objUserManagementIDCViewTmp.UserType;
            //            objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCViewTmp.MailIdFlag;
            //            objUserManagementIDCUpd.AuthsecType = objUserManagementIDCViewTmp.AuthsecType;
            //            objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCViewTmp.GroupName;
            //            objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCViewTmp.PosCode;
            //            objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCViewTmp.EffectiveDate;
            //            objUserManagementIDCUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //            objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //            objUserManagementIDCUpd.SystemDate = dSystemDateIDCTmp.Date;
            //            objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //            objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCViewTmp.ExpiryDate;
            //            objUserManagementIDCUpd.Ticket = string.IsNullOrEmpty(objUserManagementIDCViewTmp.Ticket) ? "" : objUserManagementIDCViewTmp.Ticket;
            //            objUserManagementIDCUpd.Remark = objUserManagementIDCViewTmp.Remark;
            //            objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCViewTmp.OrtherNotes;
            //            objUserManagementIDCUpd.Status = objUserManagementIDCViewTmp.Status;
            //            objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

            //            objUserManagementIDCUpd.UserStatus = objUserManagementIDCViewTmp.UserStatus;
            //            if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //                objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //            else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //                objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //            else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //                objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
            //            else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //            objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCViewTmp.StatusUpdateCore;
            //            objUserManagementIDCUpd.SessionValReq = objUserManagementIDCViewTmp.SessionValReq;
            //            objUserManagementIDCUpd.PrevStatus = objUserManagementIDCViewTmp.PrevStatus;
            //            objUserManagementIDCUpd.ResponseAttributes = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ResponseAttributes) ? "" : objUserManagementIDCViewTmp.ResponseAttributes;
            //            objUserManagementIDCUpd.CallApiStatus = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiStatus) ? "" : objUserManagementIDCViewTmp.CallApiStatus;
            //            objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCViewTmp.CallApiReqRecordSl;
            //            objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCViewTmp.CallApiResponseCode;
            //            objUserManagementIDCUpd.CallApiResponseMsg = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiResponseMsg) ? "" : objUserManagementIDCViewTmp.CallApiResponseMsg;

            //            objUserManagementIDCUpd.CreatedBy = objUserManagementIDCViewTmp.CreatedBy;
            //            objUserManagementIDCUpd.CreatedDate = objUserManagementIDCViewTmp.CreatedDate;
            //            objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCViewTmp.ModifiedBy;
            //            objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCViewTmp.ModifiedDate;
            //            objUserManagementIDCUpd.ApproverBy = objUserManagementIDCViewTmp.ApproverBy;
            //            objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCViewTmp.ApprovalDate;
            //            objUserManagementIDCUpd.FunctionTypeName = string.IsNullOrEmpty(objUserManagementIDCViewTmp.FunctionType) ? "" : FunctionTypeFlag.GetByCode(objUserManagementIDCViewTmp.FunctionType).Description;
            //            if (listRoleUsers != null && listRoleUsers.Count != 0)
            //            {
            //                objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //                objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //                objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //                objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //                objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCViewTmp.RoleToTransferCashDescription;
            //                objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCViewTmp.GroupName} - {objUserManagementIDCViewTmp.GroupNameText}";
            //            }
            //            objUserManagementIDCUpd.StartDate = objUserManagementIDCViewTmp.StartDate;
            //            objUserManagementIDCUpd.StartDateOld = (objUserManagementIDCViewTmp.StartDateOld.Year <= 1900) ? objUserManagementIDCViewTmp.StartDate : objUserManagementIDCViewTmp.StartDateOld;
            //            objUserManagementIDCUpd.StartDateText = objUserManagementIDCViewTmp.StartDate.ToString(FormatParameters.FORMAT_DATE);
            //            objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);
            //            objUserManagementIDCUpd.IpSetCode = objUserManagementIDCViewTmp.IpSetCode;
            //            objUserManagementIDCUpd.IpSetDetail = objUserManagementIDCViewTmp.IpSetDetail;
            //            objUserManagementIDCUpd.RestrictionFlag = objUserManagementIDCViewTmp.RestrictionFlag;
            //            objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;
            //            objUserManagementIDCUpd.SubType = string.IsNullOrEmpty(objUserManagementIDCViewTmp.SubType) ? DefaultValue.UserIDC_SubType : objUserManagementIDCViewTmp.SubType;
            //            objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCViewTmp.AuthsecTypeName;
            //            objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCViewTmp.MailIdFlagName;
            //            objUserManagementIDCUpd.CallApiAutoGeneratedPassword = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword) ? "" : objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword;
            //            objUserManagementIDCUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOld) ? objUserManagementIDCViewTmp.GroupName : objUserManagementIDCViewTmp.GroupNameOld;
            //            objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOldText) ? objUserManagementIDCViewTmp.GroupNameText : objUserManagementIDCViewTmp.GroupNameOldText;
                        
            //            objUserManagementIDCUpd.PosCodeOld = string.IsNullOrEmpty(objUserManagementIDCUpd.PosCodeOld) ? objUserManagementIDCUpd.PosCode : objUserManagementIDCUpd.PosCodeOld;
            //            objUserManagementIDCUpd.PosNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.PosNameOld) ? objUserManagementIDCUpd.PosName : objUserManagementIDCUpd.PosNameOld;
            //            objUserManagementIDCUpd.FirstNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.FirstNameOld) ? objUserManagementIDCUpd.FirstName : objUserManagementIDCUpd.FirstNameOld;
            //            objUserManagementIDCUpd.LastNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.LastNameOld) ? objUserManagementIDCUpd.LastName : objUserManagementIDCUpd.LastNameOld;
            //            objUserManagementIDCUpd.FullNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.FullNameOld) ? objUserManagementIDCUpd.FullName : objUserManagementIDCUpd.FullNameOld;
            //            objUserManagementIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserManagementIDCUpd.EmailAddressOld) ? objUserManagementIDCUpd.EmailAddress : objUserManagementIDCUpd.EmailAddressOld;
            //            objUserManagementIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserManagementIDCUpd.MobileNumberOld) ? objUserManagementIDCUpd.MobileNumber : objUserManagementIDCUpd.MobileNumberOld;
            //            objUserManagementIDCUpd.DateOfBirthOld = (objUserManagementIDCUpd.DateOfBirthOld.Year <= 1900) ? objUserManagementIDCUpd.DateOfBirth : objUserManagementIDCUpd.DateOfBirthOld;

            //            objUserManagementIDCUpd.GenderCode = objUserManagementIDCViewTmp.GenderCode;
            //            objUserManagementIDCUpd.GenderText = objUserManagementIDCViewTmp.GenderText;
            //            objUserManagementIDCUpd.StaffPosCode = objUserManagementIDCViewTmp.StaffPosCode;
            //            objUserManagementIDCUpd.StaffPosName = objUserManagementIDCViewTmp.StaffPosName;
            //            objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementIDCViewTmp.StaffDepartmentCode;
            //            objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCViewTmp.StaffDepartmentName;
            //            objUserManagementIDCUpd.StaffPositionCode = objUserManagementIDCViewTmp.StaffPositionCode;
            //            objUserManagementIDCUpd.StaffPositionName = objUserManagementIDCViewTmp.StaffPositionName;
            //            objUserManagementIDCUpd.StaffEmail = objUserManagementIDCViewTmp.StaffEmail;
            //            objUserManagementIDCUpd.StaffMobileNo = objUserManagementIDCViewTmp.StaffMobileNo;
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld;
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld) ? objUserManagementIDCViewTmp.RoleToTransferCashDescription : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld;
            //            objUserManagementIDCUpd.RoleToTransferCashNameOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashNameOld) ? objUserManagementIDCViewTmp.RoleToTransferCashName : objUserManagementIDCViewTmp.RoleToTransferCashNameOld;
            //            objUserManagementIDCUpd.RoleToTransferCashValueOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashValueOld) ? objUserManagementIDCViewTmp.RoleToTransferCashValue : objUserManagementIDCViewTmp.RoleToTransferCashValueOld;
            //            objUserManagementIDCUpd.ListFileId = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ListFileId) ? "" : objUserManagementIDCViewTmp.ListFileId;
            //            objUserManagementIDCUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ReasonReject) ? "" : objUserManagementIDCViewTmp.ReasonReject;
            //        }
            //        #endregion
            //    }
            //    else
            //    {
            //        #region ---4. Sự kiện Chỉnh sửa thông tin bản ghi (Yêu cầu thay đổi tài khoản người dùng) ---
            //        var objUserManagementIDCTemp01 = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

            //        if (objUserManagementIDCTemp01 != null && !string.IsNullOrEmpty(objUserManagementIDCTemp01.UserId))
            //        {
            //            var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

            //            objUserManagementIDCUpd.Id = objUserManagementIDCTemp01.Id;
            //            objUserManagementIDCUpd.OrderNo = 1;
            //            objUserManagementIDCUpd.FunctionType = objUserManagementIDCTemp01.FunctionType;
            //            objUserManagementIDCUpd.FunctionTypeName = objUserManagementIDCTemp01.FunctionTypeName;

            //            objUserManagementIDCUpd.PosCode = objUserManagementIDCTemp01.PosCode;
            //            objUserManagementIDCUpd.PosName = objUserManagementIDCTemp01.PosName;
            //            objUserManagementIDCUpd.StaffId = objUserManagementIDCTemp01.StaffId;
            //            objUserManagementIDCUpd.StaffCode = objUserManagementIDCTemp01.StaffCode;
            //            objUserManagementIDCUpd.UserId = objUserManagementIDCTemp01.UserId;
            //            objUserManagementIDCUpd.NickName = objUserManagementIDCTemp01.NickName;
            //            objUserManagementIDCUpd.FirstName = objUserManagementIDCTemp01.FirstName;
            //            objUserManagementIDCUpd.LastName = objUserManagementIDCTemp01.LastName;
            //            objUserManagementIDCUpd.FullName = objUserManagementIDCTemp01.FullName;
            //            objUserManagementIDCUpd.EmailAddress = objUserManagementIDCTemp01.EmailAddress;
            //            objUserManagementIDCUpd.MobileNumber = objUserManagementIDCTemp01.MobileNumber;
            //            objUserManagementIDCUpd.DateOfBirth = objUserManagementIDCTemp01.DateOfBirth;
            //            objUserManagementIDCUpd.GroupName = objUserManagementIDCTemp01.GroupName;
            //            objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //            objUserManagementIDCUpd.AuthType = objUserManagementIDCTemp01.AuthType;
            //            objUserManagementIDCUpd.UserType = objUserManagementIDCTemp01.UserType;
            //            objUserManagementIDCUpd.MailIdFlag = objUserManagementIDCTemp01.MailIdFlag;
            //            objUserManagementIDCUpd.AuthsecType = objUserManagementIDCTemp01.AuthsecType;
            //            objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementIDCTemp01.GroupName;
            //            objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementIDCTemp01.PosCode;
            //            objUserManagementIDCUpd.EffectiveDate = objUserManagementIDCTemp01.EffectiveDate;
            //            objUserManagementIDCUpd.BusinessDate = objUserManagementIDCTemp01.BusinessDate;
            //            objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //            objUserManagementIDCUpd.SystemDate = dSystemDateIDCTmp.Date;
            //            objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE); 
            //            objUserManagementIDCUpd.ExpiryDate = objUserManagementIDCTemp01.ExpiryDate;
            //            objUserManagementIDCUpd.Ticket = objUserManagementIDCTemp01.Ticket;
            //            objUserManagementIDCUpd.Remark = objUserManagementIDCTemp01.Remark;
            //            objUserManagementIDCUpd.OrtherNotes = objUserManagementIDCTemp01.OrtherNotes;
            //            objUserManagementIDCUpd.Status = objUserManagementIDCTemp01.Status;
            //            objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
            //            objUserManagementIDCUpd.UserStatus = objUserManagementIDCTemp01.UserStatus;
            //            if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //                objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //            else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //                objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //            else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //                objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
            //            else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //            objUserManagementIDCUpd.StatusUpdateCore = objUserManagementIDCTemp01.StatusUpdateCore;
            //            objUserManagementIDCUpd.SessionValReq = objUserManagementIDCTemp01.SessionValReq;
            //            objUserManagementIDCUpd.PrevStatus = objUserManagementIDCTemp01.PrevStatus;
            //            objUserManagementIDCUpd.ResponseAttributes = objUserManagementIDCTemp01.ResponseAttributes;
            //            objUserManagementIDCUpd.CallApiStatus = objUserManagementIDCTemp01.CallApiStatus;
            //            objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementIDCTemp01.CallApiReqRecordSl;
            //            objUserManagementIDCUpd.CallApiResponseCode = objUserManagementIDCTemp01.CallApiResponseCode;
            //            objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementIDCTemp01.CallApiResponseMsg;

            //            objUserManagementIDCUpd.CreatedBy = objUserManagementIDCTemp01.CreatedBy;
            //            objUserManagementIDCUpd.CreatedDate = objUserManagementIDCTemp01.CreatedDate;
            //            objUserManagementIDCUpd.ModifiedBy = objUserManagementIDCTemp01.ModifiedBy;
            //            objUserManagementIDCUpd.ModifiedDate = objUserManagementIDCTemp01.ModifiedDate;
            //            objUserManagementIDCUpd.ApproverBy = objUserManagementIDCTemp01.ApproverBy;
            //            objUserManagementIDCUpd.ApprovalDate = objUserManagementIDCTemp01.ApprovalDate;

            //            if (listRoleUsers != null && listRoleUsers.Count != 0)
            //            {
            //                objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //                objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //                objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //                objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //                objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //                objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";

            //                objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();

            //            }
            //            objUserManagementIDCUpd.StartDate = objUserManagementIDCTemp01.StartDate;
            //            objUserManagementIDCUpd.IpSetCode = objUserManagementIDCTemp01.IpSetCode;
            //            objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementIDCTemp01.IpSetDetail) ? "" : objUserManagementIDCTemp01.IpSetDetail;
            //            objUserManagementIDCUpd.RestrictionFlag = 0;
            //            objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

            //            objUserManagementIDCUpd.SubType = objUserManagementIDCTemp01.SubType;
            //            objUserManagementIDCUpd.AuthsecTypeName = objUserManagementIDCTemp01.AuthsecTypeName;
            //            objUserManagementIDCUpd.MailIdFlagName = objUserManagementIDCTemp01.MailIdFlagName;
            //            objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementIDCTemp01.CallApiAutoGeneratedPassword;

            //            objUserManagementIDCUpd.PosCodeOld = objUserManagementIDCTemp01.PosCodeOld;
            //            objUserManagementIDCUpd.PosNameOld = objUserManagementIDCTemp01.PosNameOld;
            //            objUserManagementIDCUpd.GroupNameOld = objUserManagementIDCTemp01.GroupNameOld;
            //            objUserManagementIDCUpd.FirstNameOld = objUserManagementIDCTemp01.FirstNameOld;
            //            objUserManagementIDCUpd.LastNameOld = objUserManagementIDCTemp01.LastNameOld;
            //            objUserManagementIDCUpd.FullNameOld = objUserManagementIDCTemp01.FullNameOld;
            //            objUserManagementIDCUpd.EmailAddressOld = objUserManagementIDCTemp01.EmailAddressOld;
            //            objUserManagementIDCUpd.MobileNumberOld = objUserManagementIDCTemp01.MobileNumberOld;
            //            objUserManagementIDCUpd.DateOfBirthOld = objUserManagementIDCTemp01.DateOfBirthOld;
            //            objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameOldText : objUserManagementIDCUpd.GroupNameOldText;
            //            objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
            //            objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
            //            objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
            //            objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

            //            //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
            //            objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
            //            objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
            //            int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
            //            if (numberDays <= 90)
            //                objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

            //            objUserManagementIDCUpd.GenderCode = objUserManagementIDCTemp01.GenderCode;
            //            objUserManagementIDCUpd.GenderText = objUserManagementIDCTemp01.GenderText;
            //            objUserManagementIDCUpd.StaffPosCode = objUserManagementIDCTemp01.StaffPosCode;
            //            objUserManagementIDCUpd.StaffPosName = objUserManagementIDCTemp01.StaffPosName;
            //            objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementIDCTemp01.StaffDepartmentCode;
            //            objUserManagementIDCUpd.StaffDepartmentName = objUserManagementIDCTemp01.StaffDepartmentName;
            //            objUserManagementIDCUpd.StaffPositionCode = objUserManagementIDCTemp01.StaffPositionCode;
            //            objUserManagementIDCUpd.StaffPositionName = objUserManagementIDCTemp01.StaffPositionName;
            //            objUserManagementIDCUpd.StaffEmail = objUserManagementIDCTemp01.StaffEmail;
            //            objUserManagementIDCUpd.StaffMobileNo = objUserManagementIDCTemp01.StaffMobileNo;
            //            //Lấy theo QLNS khi thay đổi thông tin người dùng
            //            objUserManagementIDCUpd.EmailAddress = objUserManagementIDCTemp01.StaffEmail;
            //            objUserManagementIDCUpd.MobileNumber = objUserManagementIDCTemp01.StaffMobileNo;
            //            objUserManagementIDCUpd.ExistsInCore = objUserManagementIDCTemp01.ExistsInCore;
            //        }

            //        #endregion
            //        sNameView = "CreateChangeInforUserManagementIDC";
            //    }
            //    #endregion
            //}
            //else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
            //{
            //    #region ---4. Sự kiện Tạo lập yêu cầu thay đổi nghiệp vụ tài khoản người dùng (Tạo mới tài khoản yêu cầu thay đổi tài khoản người dùng) ---
            //    var listUserIDCMasterTemp = await _userManagementIDCService.GetListUserIDCMasters(0, "", pPosCode, pUserId, "", "", -1, true);
            //    var objUserManagementMTFind = listUserIDCMasterTemp.FirstOrDefault();
            //    if (objUserManagementMTFind != null && !string.IsNullOrEmpty(objUserManagementMTFind.UserId))
            //    {
            //        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

            //        objUserManagementIDCUpd.Id = 0;// objUserManagementMTFind.Id;
            //        objUserManagementIDCUpd.OrderNo = 1;
            //        objUserManagementIDCUpd.FunctionType = "";

            //        objUserManagementIDCUpd.PosCode = objUserManagementMTFind.PosCode;
            //        objUserManagementIDCUpd.PosName = objUserManagementMTFind.PosName;
            //        objUserManagementIDCUpd.StaffId = objUserManagementMTFind.StaffId;
            //        objUserManagementIDCUpd.StaffCode = objUserManagementMTFind.StaffCode;
            //        objUserManagementIDCUpd.UserId = objUserManagementMTFind.UserId;
            //        objUserManagementIDCUpd.NickName = objUserManagementMTFind.NickName;
            //        objUserManagementIDCUpd.FirstName = objUserManagementMTFind.FirstName;
            //        objUserManagementIDCUpd.LastName = objUserManagementMTFind.LastName;
            //        objUserManagementIDCUpd.FullName = objUserManagementMTFind.FullName;
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementMTFind.EmailAddress;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementMTFind.MobileNumber;
            //        objUserManagementIDCUpd.DateOfBirth = objUserManagementMTFind.DateOfBirth;
            //        objUserManagementIDCUpd.GroupName = objUserManagementMTFind.GroupName;
            //        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objUserManagementIDCUpd.AuthType = objUserManagementMTFind.AuthType;
            //        objUserManagementIDCUpd.UserType = objUserManagementMTFind.UserType;
            //        objUserManagementIDCUpd.MailIdFlag = objUserManagementMTFind.MailIdFlag;
            //        objUserManagementIDCUpd.AuthsecType = objUserManagementMTFind.AuthsecType;
            //        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementMTFind.GroupName;
            //        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementMTFind.PosCode;
            //        objUserManagementIDCUpd.EffectiveDate = objUserManagementMTFind.EffectiveDate;
            //        objUserManagementIDCUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.ExpiryDate = objUserManagementMTFind.ExpiryDate;
            //        objUserManagementIDCUpd.ExpiryDateOld = objUserManagementMTFind.ExpiryDate;
            //        objUserManagementIDCUpd.ExpiryDateOldText = objUserManagementMTFind.ExpiryDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.Ticket = "";
            //        objUserManagementIDCUpd.Remark = "";
            //        objUserManagementIDCUpd.OrtherNotes = "";
            //        objUserManagementIDCUpd.Status = StatusBusinessFlow.Status_Created.Value;
            //        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;

            //        objUserManagementIDCUpd.UserStatus = objUserManagementMTFind.UserStatus;
            //        if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objUserManagementIDCUpd.UserStatusText = "Tmaj khóa (Lock)";
            //        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //        objUserManagementIDCUpd.StatusUpdateCore = 0;
            //        objUserManagementIDCUpd.SessionValReq = true;
            //        objUserManagementIDCUpd.PrevStatus = 0;
            //        objUserManagementIDCUpd.ResponseAttributes = "";
            //        objUserManagementIDCUpd.CallApiStatus = "";
            //        objUserManagementIDCUpd.CallApiReqRecordSl = 0;
            //        objUserManagementIDCUpd.CallApiResponseCode = "";
            //        objUserManagementIDCUpd.CallApiResponseMsg = "";

            //        objUserManagementIDCUpd.CreatedBy = objUserManagementMTFind.CreatedBy;
            //        objUserManagementIDCUpd.CreatedDate = objUserManagementMTFind.CreatedDate;
            //        objUserManagementIDCUpd.ModifiedBy = objUserManagementMTFind.ModifiedBy;
            //        objUserManagementIDCUpd.ModifiedDate = objUserManagementMTFind.ModifiedDate;
            //        objUserManagementIDCUpd.ApproverBy = objUserManagementMTFind.ApproverBy;
            //        objUserManagementIDCUpd.ApprovalDate = objUserManagementMTFind.ApprovalDate;
            //        objUserManagementIDCUpd.FunctionTypeName = "";
            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
            //        }
            //        objUserManagementIDCUpd.StartDate = objUserManagementMTFind.StartDate.Value;
            //        objUserManagementIDCUpd.IpSetCode = objUserManagementMTFind.IpSetCode;
            //        objUserManagementIDCUpd.IpSetDetail = objUserManagementMTFind.IpSetDetail;
            //        objUserManagementIDCUpd.RestrictionFlag = 0;
            //        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

            //        objUserManagementIDCUpd.SubType = objUserManagementMTFind.SubType;
            //        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementMTFind.AuthsecTypeName;
            //        objUserManagementIDCUpd.MailIdFlagName = objUserManagementMTFind.MailIdFlagName;
            //        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = "";

            //        objUserManagementIDCUpd.PosCodeOld = objUserManagementMTFind.PosCode;
            //        objUserManagementIDCUpd.PosNameOld = objUserManagementMTFind.PosName;
            //        objUserManagementIDCUpd.GroupNameOld = objUserManagementMTFind.GroupName;
            //        objUserManagementIDCUpd.FirstNameOld = objUserManagementMTFind.FirstName;
            //        objUserManagementIDCUpd.LastNameOld = objUserManagementMTFind.LastName;
            //        objUserManagementIDCUpd.FullNameOld = objUserManagementMTFind.FullName;
            //        objUserManagementIDCUpd.EmailAddressOld = objUserManagementMTFind.EmailAddress;
            //        objUserManagementIDCUpd.MobileNumberOld = objUserManagementMTFind.MobileNumber;
            //        objUserManagementIDCUpd.DateOfBirthOld = objUserManagementMTFind.DateOfBirth;
            //        objUserManagementIDCUpd.GroupNameOldText = objUserManagementIDCUpd.GroupNameText;
            //        objUserManagementIDCUpd.RoleToTransferCashValueOld = objUserManagementIDCUpd.RoleToTransferCashValue;
            //        objUserManagementIDCUpd.RoleToTransferCashNameOld = objUserManagementIDCUpd.RoleToTransferCashName;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail;
            //        objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
            //        objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.StartDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.StartDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.SystemDate.AddDays(10);// CustConverter.StringToDateTime(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).Date;
            //        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
            //        objUserManagementIDCUpd.GenderCode = objUserManagementMTFind.GenderCode;
            //        objUserManagementIDCUpd.GenderText = objUserManagementMTFind.GenderText;
            //        objUserManagementIDCUpd.StaffPosCode = objUserManagementMTFind.StaffPosCode;
            //        objUserManagementIDCUpd.StaffPosName = objUserManagementMTFind.StaffPosName;
            //        objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementMTFind.StaffDepartmentCode;
            //        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementMTFind.StaffDepartmentName;
            //        objUserManagementIDCUpd.StaffPositionCode = objUserManagementMTFind.StaffPositionCode;
            //        objUserManagementIDCUpd.StaffPositionName = objUserManagementMTFind.StaffPositionName;
            //        objUserManagementIDCUpd.StaffEmail = objUserManagementMTFind.StaffEmail;
            //        objUserManagementIDCUpd.StaffMobileNo = objUserManagementMTFind.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementMTFind.StaffEmail;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementMTFind.StaffMobileNo;

            //        objUserManagementIDCUpd.ExistsInCore = objUserManagementMTFind.ExistsInCore;
            //        objUserManagementIDCUpd.ListFileId = "";
            //        objUserManagementIDCUpd.ReasonReject = "";
            //    }
            //    #endregion
            //}
            //else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType != FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            //{
            //    #region ---5. Sự kiện Chỉnh sửa thông tin bản ghi (Yêu cầu thay đổi tài khoản người dùng) ---
            //    var objUserManagementChangeTemp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

            //    if (objUserManagementChangeTemp != null && !string.IsNullOrEmpty(objUserManagementChangeTemp.UserId))
            //    {
            //        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

            //        objUserManagementIDCUpd.Id = objUserManagementChangeTemp.Id;
            //        objUserManagementIDCUpd.OrderNo = 1;
            //        objUserManagementIDCUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
            //        objUserManagementIDCUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

            //        objUserManagementIDCUpd.PosCode = objUserManagementChangeTemp.PosCode;
            //        objUserManagementIDCUpd.PosName = objUserManagementChangeTemp.PosName;
            //        objUserManagementIDCUpd.StaffId = objUserManagementChangeTemp.StaffId;
            //        objUserManagementIDCUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
            //        objUserManagementIDCUpd.UserId = objUserManagementChangeTemp.UserId;
            //        objUserManagementIDCUpd.NickName = objUserManagementChangeTemp.NickName;
            //        objUserManagementIDCUpd.FirstName = objUserManagementChangeTemp.FirstName;
            //        objUserManagementIDCUpd.LastName = objUserManagementChangeTemp.LastName;
            //        objUserManagementIDCUpd.FullName = objUserManagementChangeTemp.FullName;
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
            //        objUserManagementIDCUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
            //        objUserManagementIDCUpd.GroupName = objUserManagementChangeTemp.GroupName;
            //        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objUserManagementIDCUpd.AuthType = objUserManagementChangeTemp.AuthType;
            //        objUserManagementIDCUpd.UserType = objUserManagementChangeTemp.UserType;
            //        objUserManagementIDCUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
            //        objUserManagementIDCUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
            //        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
            //        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
            //        objUserManagementIDCUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
            //        objUserManagementIDCUpd.BusinessDate = objUserManagementChangeTemp.BusinessDate;
            //        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.SystemDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
            //        objUserManagementIDCUpd.Ticket = objUserManagementChangeTemp.Ticket;
            //        objUserManagementIDCUpd.Remark = objUserManagementChangeTemp.Remark;
            //        objUserManagementIDCUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
            //        objUserManagementIDCUpd.Status = objUserManagementChangeTemp.Status;
            //        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
            //        objUserManagementIDCUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
            //        if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
            //        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //        objUserManagementIDCUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
            //        objUserManagementIDCUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
            //        objUserManagementIDCUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
            //        objUserManagementIDCUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
            //        objUserManagementIDCUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
            //        objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
            //        objUserManagementIDCUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
            //        objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

            //        objUserManagementIDCUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
            //        objUserManagementIDCUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
            //        objUserManagementIDCUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
            //        objUserManagementIDCUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
            //        objUserManagementIDCUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
            //        objUserManagementIDCUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
            //            objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();
            //        }
            //        objUserManagementIDCUpd.StartDate = objUserManagementChangeTemp.StartDate;
            //        objUserManagementIDCUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
            //        objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
            //        objUserManagementIDCUpd.RestrictionFlag = 0;
            //        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

            //        objUserManagementIDCUpd.SubType = objUserManagementChangeTemp.SubType;
            //        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
            //        objUserManagementIDCUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
            //        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

            //        objUserManagementIDCUpd.PosCodeOld = objUserManagementChangeTemp.PosCodeOld;
            //        objUserManagementIDCUpd.PosNameOld = objUserManagementChangeTemp.PosNameOld;
            //        objUserManagementIDCUpd.GroupNameOld = objUserManagementChangeTemp.GroupNameOld;
            //        objUserManagementIDCUpd.FirstNameOld = objUserManagementChangeTemp.FirstNameOld;
            //        objUserManagementIDCUpd.LastNameOld = objUserManagementChangeTemp.LastNameOld;
            //        objUserManagementIDCUpd.FullNameOld = objUserManagementChangeTemp.FullNameOld;
            //        objUserManagementIDCUpd.EmailAddressOld = objUserManagementChangeTemp.EmailAddressOld;
            //        objUserManagementIDCUpd.MobileNumberOld = objUserManagementChangeTemp.MobileNumberOld;
            //        objUserManagementIDCUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
            //        objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameOldText : objUserManagementIDCUpd.GroupNameOldText;
            //        objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
            //        objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
            //        objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
            //        objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.StartDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.StartDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
            //        objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
            //        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
            //        int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
            //        if (numberDays > 0 && numberDays <= 90 && objUserManagementChangeTemp.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
            //            objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

            //        objUserManagementIDCUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
            //        objUserManagementIDCUpd.GenderText = objUserManagementChangeTemp.GenderText;
            //        objUserManagementIDCUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
            //        objUserManagementIDCUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
            //        objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
            //        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
            //        objUserManagementIDCUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
            //        objUserManagementIDCUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
            //        objUserManagementIDCUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
            //        objUserManagementIDCUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
            //        objUserManagementIDCUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
            //        objUserManagementIDCUpd.ListFileId = string.IsNullOrEmpty(objUserManagementChangeTemp.ListFileId) ? "" : objUserManagementChangeTemp.ListFileId;
            //        objUserManagementIDCUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementChangeTemp.ReasonReject) ? "" : objUserManagementChangeTemp.ReasonReject;
            //        var objUserInfoIDCTmp = await _userManagementIDCService.GetUserIDCInfoByApiViewUser(objUserManagementChangeTemp.UserId);
            //        if (objUserInfoIDCTmp != null && !string.IsNullOrEmpty(objUserInfoIDCTmp.UserId))
            //        {
            //            objUserManagementIDCUpd.ExpiryDateOld = CustConverter.StringToDate(objUserInfoIDCTmp.ExpiryDate.Trim().Replace("-", "").Replace("/", ""), FormatParameters.FORMAT_DATE_INT).Date;//yyyy-MM-dd
            //            objUserManagementIDCUpd.ExpiryDateOldText = objUserManagementIDCUpd.ExpiryDateOld.ToString(FormatParameters.FORMAT_DATE);
            //        }
            //    }
            //    #endregion

            //    sNameView = "CreateChangeInforUserManagementIDC";
            //}
            //else if (pFlagCall == EventFlag.EventFlag_Approval.Value.ToString() || pFlagCall == EventFlag.EventFlag_Authorize.Value.ToString() ||
            //    (pFlagCall == EventFlag.EventFlag_View.Value.ToString()) && pButtonType== EventFlag.EventFlag_Authorize.Value.ToString())
            //{
            //    #region ---6. Sự kiện gọi Form Trình duyệt/Phê duyệt yêu cầu người dùng Intellect IDC ---
            //    var objUserManagementChangeTemp = (await _userManagementIDCService.GetListUserIDCManagement(pId, "", pPosCode, pUserId, "", "", -1, "", false)).FirstOrDefault();

            //    if (objUserManagementChangeTemp != null && !string.IsNullOrEmpty(objUserManagementChangeTemp.UserId))
            //    {
            //        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

            //        objUserManagementIDCUpd.Id = objUserManagementChangeTemp.Id;
            //        objUserManagementIDCUpd.OrderNo = 1;
            //        objUserManagementIDCUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
            //        objUserManagementIDCUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

            //        objUserManagementIDCUpd.PosCode = objUserManagementChangeTemp.PosCode;
            //        objUserManagementIDCUpd.PosName = objUserManagementChangeTemp.PosName;
            //        objUserManagementIDCUpd.StaffId = objUserManagementChangeTemp.StaffId;
            //        objUserManagementIDCUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
            //        objUserManagementIDCUpd.UserId = objUserManagementChangeTemp.UserId;
            //        objUserManagementIDCUpd.NickName = objUserManagementChangeTemp.NickName;
            //        objUserManagementIDCUpd.FirstName = objUserManagementChangeTemp.FirstName;
            //        objUserManagementIDCUpd.LastName = objUserManagementChangeTemp.LastName;
            //        objUserManagementIDCUpd.FullName = objUserManagementChangeTemp.FullName;
            //        objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
            //        objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
            //        objUserManagementIDCUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
            //        objUserManagementIDCUpd.GroupName = objUserManagementChangeTemp.GroupName;
            //        objUserManagementIDCUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objUserManagementIDCUpd.AuthType = objUserManagementChangeTemp.AuthType;
            //        objUserManagementIDCUpd.UserType = objUserManagementChangeTemp.UserType;
            //        objUserManagementIDCUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
            //        objUserManagementIDCUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
            //        objUserManagementIDCUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
            //        objUserManagementIDCUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
            //        objUserManagementIDCUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
            //        objUserManagementIDCUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //        objUserManagementIDCUpd.BusinessDateText = objUserManagementIDCUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objUserManagementIDCUpd.SystemDateText = objUserManagementIDCUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //        objUserManagementIDCUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
            //        objUserManagementIDCUpd.Ticket = objUserManagementChangeTemp.Ticket;
            //        objUserManagementIDCUpd.Remark = objUserManagementChangeTemp.Remark;
            //        objUserManagementIDCUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
            //        objUserManagementIDCUpd.Status = objUserManagementChangeTemp.Status;
            //        objUserManagementIDCUpd.StatusText = StatusBusinessFlow.GetByValue(objUserManagementIDCUpd.Status).Description;
            //        objUserManagementIDCUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
            //        if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objUserManagementIDCUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objUserManagementIDCUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objUserManagementIDCUpd.UserStatusText = "Tạm khóa (Lock)";
            //        else objUserManagementIDCUpd.UserStatusText = "Không xác định";

            //        objUserManagementIDCUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
            //        objUserManagementIDCUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
            //        objUserManagementIDCUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
            //        objUserManagementIDCUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
            //        objUserManagementIDCUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
            //        objUserManagementIDCUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
            //        objUserManagementIDCUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
            //        objUserManagementIDCUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

            //        objUserManagementIDCUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
            //        objUserManagementIDCUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
            //        objUserManagementIDCUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
            //        objUserManagementIDCUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
            //        objUserManagementIDCUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
            //        objUserManagementIDCUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objUserManagementIDCUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objUserManagementIDCUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objUserManagementIDCUpd.RoleToTransferCashName = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objUserManagementIDCUpd.RoleToTransferCashDescription = (objUserManagementIDCUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCUpd.RoleToTransferCashDescription;
            //            objUserManagementIDCUpd.GroupNameDetail = $"{objUserManagementIDCUpd.GroupName} - {objUserManagementIDCUpd.GroupNameText}";
            //            objUserManagementIDCUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();
            //        }
            //        objUserManagementIDCUpd.StartDate = objUserManagementChangeTemp.StartDate;
            //        objUserManagementIDCUpd.StartDateText = string.IsNullOrEmpty(objUserManagementChangeTemp.StartDateText) ? objUserManagementChangeTemp.StartDate.ToString(FormatParameters.FORMAT_DATE) : objUserManagementChangeTemp.StartDateText;
            //        objUserManagementIDCUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
            //        objUserManagementIDCUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
            //        objUserManagementIDCUpd.RestrictionFlag = 0;
            //        objUserManagementIDCUpd.RestrictionFlagCheck = (objUserManagementIDCUpd.RestrictionFlag == 1) ? true : false;

            //        objUserManagementIDCUpd.SubType = objUserManagementChangeTemp.SubType;
            //        objUserManagementIDCUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
            //        objUserManagementIDCUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
            //        objUserManagementIDCUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

            //        objUserManagementIDCUpd.PosCodeOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosCodeOld) ? objUserManagementChangeTemp.PosCode : objUserManagementChangeTemp.PosCodeOld;
            //        objUserManagementIDCUpd.PosNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosNameOld) ? objUserManagementChangeTemp.PosName : objUserManagementChangeTemp.PosNameOld;
            //        objUserManagementIDCUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.GroupNameOld) ? objUserManagementChangeTemp.GroupName : objUserManagementChangeTemp.GroupNameOld;
            //        objUserManagementIDCUpd.FirstNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FirstNameOld) ? objUserManagementChangeTemp.FirstName : objUserManagementChangeTemp.FirstNameOld;
            //        objUserManagementIDCUpd.LastNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.LastNameOld) ? objUserManagementChangeTemp.LastName : objUserManagementChangeTemp.LastNameOld;
            //        objUserManagementIDCUpd.FullNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FullNameOld) ? objUserManagementChangeTemp.FullName : objUserManagementChangeTemp.FullNameOld;
            //        objUserManagementIDCUpd.EmailAddressOld = string.IsNullOrEmpty(objUserManagementChangeTemp.EmailAddressOld) ? objUserManagementChangeTemp.EmailAddress : objUserManagementChangeTemp.EmailAddressOld;
            //        objUserManagementIDCUpd.MobileNumberOld = string.IsNullOrEmpty(objUserManagementChangeTemp.MobileNumberOld) ? objUserManagementChangeTemp.MobileNumber : objUserManagementChangeTemp.MobileNumberOld;
            //        objUserManagementIDCUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
            //        objUserManagementIDCUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCUpd.GroupNameOldText) ? objUserManagementIDCUpd.GroupNameText : objUserManagementIDCUpd.GroupNameOldText;
            //        objUserManagementIDCUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashValueOld) ? objUserManagementIDCUpd.RoleToTransferCashValue : objUserManagementIDCUpd.RoleToTransferCashValueOld;
            //        objUserManagementIDCUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashNameOld) ? objUserManagementIDCUpd.RoleToTransferCashName : objUserManagementIDCUpd.RoleToTransferCashNameOld;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionOld) ? objUserManagementIDCUpd.RoleToTransferCashDescription : objUserManagementIDCUpd.RoleToTransferCashDescriptionOld;
            //        objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld) ? objUserManagementIDCUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCUpd.RoleToTransferCashDescriptionDetailOld;
            //        objUserManagementIDCUpd.StartDateOld = objUserManagementIDCUpd.StartDate;
            //        objUserManagementIDCUpd.StartDateOldText = objUserManagementIDCUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

            //        //objUserManagementIDCUpd.StartDate = objUserManagementIDCUpd.BusinessDate;
            //        objUserManagementIDCUpd.EndDateChangeRole = objUserManagementIDCUpd.ExpiryDate;
            //        objUserManagementIDCUpd.ChoiceEndDateChangeRole = 0;
            //        int numberDays = (objUserManagementIDCUpd.ExpiryDate - objUserManagementIDCUpd.StartDate).Days;
            //        if (numberDays <= 90)
            //            objUserManagementIDCUpd.ChoiceEndDateChangeRole = 1;

            //        objUserManagementIDCUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
            //        objUserManagementIDCUpd.GenderText = objUserManagementChangeTemp.GenderText;
            //        objUserManagementIDCUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
            //        objUserManagementIDCUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
            //        objUserManagementIDCUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
            //        objUserManagementIDCUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
            //        objUserManagementIDCUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
            //        objUserManagementIDCUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
            //        objUserManagementIDCUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
            //        objUserManagementIDCUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        //objUserManagementIDCUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
            //        //objUserManagementIDCUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
            //        objUserManagementIDCUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
            //        objUserManagementIDCUpd.ListFileId = string.IsNullOrEmpty(objUserManagementChangeTemp.ListFileId) ? "" : objUserManagementChangeTemp.ListFileId;
            //        objUserManagementIDCUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementChangeTemp.ReasonReject) ? "" : objUserManagementChangeTemp.ReasonReject;
            //    }

            //    #endregion

            //    sNameView = "AuthorizeUserManagementIDC";
            //}
            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() && pId == 0
                    && (pButtonType == EventBusinessCode.EventCode_TransPoint_AddNew.Code || pButtonType == ""))
                sNameView = "UpdateListOfTransPointWork";
            //else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pId != 0 && pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            //    sNameView = "UpdateUserManagementIDC";
            //else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString() && (string.IsNullOrEmpty(pButtonType) || pButtonType.Length > 2))
            //{
            //    if (string.IsNullOrEmpty(pButtonType) || pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            //    {
            //        //Xem chi tiết thông tin tài khoản người dùng Intellect iDC => Lấy thông tin trong UserIDCMaster, sau đó lấy tiếp trong Intellect IDC để gộp thành thông tin mới nhất
            //        sNameView = "UpdateUserManagementIDC";
            //    }
            //    else
            //    {
            //        sNameView = "CreateChangeInforUserManagementIDC";
            //        pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
            //    }
            //}
            //else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
            //{
            //    sNameView = "CreateChangeInforUserManagementIDC";
            //    pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
            //}
            //else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && pButtonType != FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
            //{
            //    sNameView = "CreateChangeInforUserManagementIDC";
            //    pButtonType = EventFlag.EventFlag_EditIDC.Value.ToString();
            //}
            TempData["EventCode_TransPoint_AddNew"] = EventBusinessCode.EventCode_TransPoint_AddNew.Code;
            TempData["EventCode_TransPoint_Change_VisitDate"] = EventBusinessCode.EventCode_TransPoint_Change_VisitDate.Code;
            TempData["EventCode_TransPoint_Change_Name"] = EventBusinessCode.EventCode_TransPoint_Change_Name.Code;
            TempData["EventCode_TransPoint_Change_OtherInfor"] = EventBusinessCode.EventCode_TransPoint_Change_OtherInfor.Code;

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
            return PartialView(sNameView, objListOfTransPointWorkUpd);
        }

        /// <summary>
        /// Hàm thực hiện lưu thông tin Thêm/Chỉnh sửa bản ghi bảng dữ liệu ListOfTransPointWork
        /// </summary>
        /// <param name="request"></param>
        /// <param name="objTranspointUpd">Thông tin lưu lại theo model ListOfTransPointWork</param>
        /// <param name="pFlagCall">Cờ xác định cập nhật Thêm/Sửa. 1 - Thêm mới (EventFlag.EventFlag_Add.Value); 2-Thay đổi thông tin (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdateListOfTransPointWork([DataSourceRequest] DataSourceRequest request, ListOfTransPointWorkViewModel objTranspointUpd, string pFlagCall)
        {
            try
            {
                string result = "0";
                //var resultCheck = await IsValidSaveUserManagementIDC(objTranspointUpd, objTranspointUpd.FlagCall);
                //result = resultCheck.ToString();
                if (result == "0" && objTranspointUpd != null && ModelState.IsValid)
                {
                    objTranspointUpd.PosCode = string.IsNullOrEmpty(objTranspointUpd.PosCode) ? "" : objTranspointUpd.PosCode;
                    objTranspointUpd.PosName = string.IsNullOrEmpty(objTranspointUpd.PosName) ? "" : objTranspointUpd.PosName.Replace(" - ","").Replace("PGD NHCSXH ","PGD ");
                    objTranspointUpd.ProvinceCode = string.IsNullOrEmpty(objTranspointUpd.ProvinceCode) ? "" : objTranspointUpd.ProvinceCode;
                    objTranspointUpd.ProvinceName = string.IsNullOrEmpty(objTranspointUpd.ProvinceName) ? "" : objTranspointUpd.ProvinceName;
                    objTranspointUpd.CommuneCode = string.IsNullOrEmpty(objTranspointUpd.CommuneCode) ? "" : objTranspointUpd.CommuneCode;
                    objTranspointUpd.CommuneName = string.IsNullOrEmpty(objTranspointUpd.CommuneName) ? "" : objTranspointUpd.CommuneName;
                    objTranspointUpd.TxnPointCode = string.IsNullOrEmpty(objTranspointUpd.TxnPointCode) ? "" : objTranspointUpd.TxnPointCode;
                    objTranspointUpd.TxnPointName = string.IsNullOrEmpty(objTranspointUpd.TxnPointName) ? "" : objTranspointUpd.TxnPointName;
                    objTranspointUpd.AddressCode = string.IsNullOrEmpty(objTranspointUpd.AddressCode) ?"" : objTranspointUpd.AddressCode;

                    objTranspointUpd.AddressDetail = string.IsNullOrEmpty(objTranspointUpd.AddressDetail) ? "" :objTranspointUpd.AddressDetail;
                    objTranspointUpd.PhoneSupport = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport) ? "" : objTranspointUpd.PhoneSupport;
                    objTranspointUpd.PhoneSupport01 = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport01) ? "" : objTranspointUpd.PhoneSupport01;
                    objTranspointUpd.PhoneSupport02 = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport02) ? "" : objTranspointUpd.PhoneSupport02;
                    int iResultUpdate = _serviceTransPoint.UpdateListOfTransPointWork(objTranspointUpd, UserName, pFlagCall);

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
    }
}
