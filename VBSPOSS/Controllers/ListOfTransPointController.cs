using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
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
        /// Danh sách bản ghi điểm giao dịch  => Tải dừ bảng dữ liệu ListOfTranspoint
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pStatus">Trạng thái</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult LoadGridData_TransPoint([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, string pStatus)
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
                var listTransPointWorks = _serviceTransPoint.GetListOfTransPointSearch("", pPosCode, "",pTxnPointCode, pTxnPointName, 0,0,pStatus, "");
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
        /// Danh sách bản ghi Tạo mới/Thay đổi thông tin,... điểm giao dịch => Tải dừ bảng dữ liệu ListOfTranspointWork
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
                var listTransPointWorks = _serviceTransPoint.GetListOfTransPointWorkSearch("", pPosCode, pTxnPointCode, pTxnPointName, pStatus, "", pEventCode);
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
        /// <param name="pPosCode">Mã Pos bản ghi của bảng ListOfTransPointWork</param>
        /// <param name="pBusinessDate">Ngày hiệu lực của yêu cầu nghiệp vụ của bản ghi. Định dạng: dd/MM/yyyy</param>
        /// <param name="pFlagCall">Cờ xác định: 1 - Thêm mới; 2 - Chỉnh sửa bản ghi; 9 - Thay đổi nghiệp vụ điểm giao dịch</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch</param>
        /// <returns>Giá trị đối tượng ListOfTransPointWork</returns>
        /// 
        public async Task<ActionResult> ShowUpdateListOfTransPointWork(string pButtonType, string pPosCode, string pBusinessDate, string pFlagCall, string pTxnPointCode, string pEventCode)
        {
            ListOfTransPointWorkViewModel objListOfTransPointWorkUpd = new ListOfTransPointWorkViewModel();
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = "";
            if (string.IsNullOrEmpty(pBusinessDate))
                pBusinessDate = CustConverter.StringToDate(DefaultValue.MinDate.ToString(), FormatParameters.FORMAT_DATE_INT).ToString(FormatParameters.FORMAT_DATE);
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
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() || pFlagCall == EventFlag.EventFlag_View.Value.ToString())        //Trường hợp chỉnh sửa bản ghi yêu cầu nghiệp vụ: Bản ghi có trong bảng ListOfTransPointWork
            {
                #region ---2. Sự kiện chỉnh sửa bản ghi Yêu cầu tạo mới điểm giao dịch --- 
                var objTranspointFind01 = (_serviceTransPoint.GetListOfTransPointWorkSearch("", pPosCode, pTxnPointCode, "", -1, "","")).FirstOrDefault();
                if (objTranspointFind01 != null  && !string.IsNullOrEmpty(objTranspointFind01.EventCode))
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                    objListOfTransPointWorkUpd.OrderNo = objTranspointFind01.OrderNo;
                    objListOfTransPointWorkUpd.OrderNoText = objTranspointFind01.OrderNoText;
                    objListOfTransPointWorkUpd.EventCode = objTranspointFind01.EventCode;
                    objListOfTransPointWorkUpd.EventName = objTranspointFind01.EventName;
                    objListOfTransPointWorkUpd.ParentId = objTranspointFind01.ParentId;
                    objListOfTransPointWorkUpd.ProvinceCode = objTranspointFind01.ProvinceCode;
                    objListOfTransPointWorkUpd.ProvinceName = objTranspointFind01.ProvinceName;
                    objListOfTransPointWorkUpd.PosCode = objTranspointFind01.PosCode;
                    objListOfTransPointWorkUpd.PosName = objTranspointFind01.PosName;
                    objListOfTransPointWorkUpd.DistrictCode = objTranspointFind01.DistrictCode;
                    objListOfTransPointWorkUpd.DistrictName = objTranspointFind01.DistrictName;
                    objListOfTransPointWorkUpd.CommuneCode = objTranspointFind01.CommuneCode;
                    objListOfTransPointWorkUpd.CommuneName = objTranspointFind01.CommuneName;
                    objListOfTransPointWorkUpd.TxnPointCode = objTranspointFind01.TxnPointCode;
                    objListOfTransPointWorkUpd.TxnPointName = objTranspointFind01.TxnPointName;
                    objListOfTransPointWorkUpd.VisitDate = objTranspointFind01.VisitDate;
                    objListOfTransPointWorkUpd.VisitDateText = objTranspointFind01.VisitDateText;
                    objListOfTransPointWorkUpd.Times = objTranspointFind01.Times;

                    objListOfTransPointWorkUpd.TimeBegin = objTranspointFind01.TimeBegin;
                    objListOfTransPointWorkUpd.TimeEnd = objTranspointFind01.TimeEnd;
                    objListOfTransPointWorkUpd.TimeBeginNum = objTranspointFind01.TimeBeginNum;
                    objListOfTransPointWorkUpd.TimeEndNum = objTranspointFind01.TimeEndNum;
                    objListOfTransPointWorkUpd.TimeBeginDate = objTranspointFind01.TimeBeginDate;
                    objListOfTransPointWorkUpd.TimeEndDate = objTranspointFind01.TimeEndDate;
                    objListOfTransPointWorkUpd.Hours = objTranspointFind01.Hours;
                    objListOfTransPointWorkUpd.Minutes = objTranspointFind01.Minutes;
                    objListOfTransPointWorkUpd.Longitude = objTranspointFind01.Longitude;
                    objListOfTransPointWorkUpd.Latitude = objTranspointFind01.Latitude;
                    objListOfTransPointWorkUpd.IsInCommune = objTranspointFind01.IsInCommune;
                    objListOfTransPointWorkUpd.IsInPos = objTranspointFind01.IsInPos;
                    objListOfTransPointWorkUpd.IsInterWard = objTranspointFind01.IsInterWard;
                    objListOfTransPointWorkUpd.InterWardName = objTranspointFind01.InterWardName;
                    objListOfTransPointWorkUpd.EffectiveDate = objTranspointFind01.EffectiveDate;
                    objListOfTransPointWorkUpd.EffectiveDateText = objTranspointFind01.EffectiveDateText;
                    objListOfTransPointWorkUpd.TxnLocation = objTranspointFind01.TxnLocation;
                    objListOfTransPointWorkUpd.AddressDetail = objTranspointFind01.AddressDetail;
                    objListOfTransPointWorkUpd.AddressCode = objTranspointFind01.AddressCode;

                    objListOfTransPointWorkUpd.AddressFull = objTranspointFind01.AddressFull;
                    objListOfTransPointWorkUpd.PhoneSupport = objTranspointFind01.PhoneSupport;
                    objListOfTransPointWorkUpd.PhoneSupport01 = objTranspointFind01.PhoneSupport01;
                    objListOfTransPointWorkUpd.PhoneSupport02 = objTranspointFind01.PhoneSupport02;
                    objListOfTransPointWorkUpd.TxnStatus = objTranspointFind01.TxnStatus;
                    objListOfTransPointWorkUpd.TxnStatusText = objTranspointFind01.TxnStatusText;
                    objListOfTransPointWorkUpd.Status = objTranspointFind01.Status;
                    objListOfTransPointWorkUpd.StatusText = objTranspointFind01.StatusText;
                    objListOfTransPointWorkUpd.Remark = objTranspointFind01.Remark;

                    objListOfTransPointWorkUpd.CreatedBy = objTranspointFind01.CreatedBy;
                    objListOfTransPointWorkUpd.CreatedDate = objTranspointFind01.CreatedDate;
                    objListOfTransPointWorkUpd.ModifiedBy = objTranspointFind01.ModifiedBy;
                    objListOfTransPointWorkUpd.ModifiedDate = objTranspointFind01.ModifiedDate;
                    objListOfTransPointWorkUpd.ApproverBy = objTranspointFind01.ApproverBy;
                    objListOfTransPointWorkUpd.ApprovalDate = objTranspointFind01.ApprovalDate;
                    objListOfTransPointWorkUpd.BusinessDate = objTranspointFind01.BusinessDate;
                    objListOfTransPointWorkUpd.BusinessDateText = objTranspointFind01.BusinessDateText;
                    objListOfTransPointWorkUpd.DocumentId = objTranspointFind01.DocumentId;
                    objListOfTransPointWorkUpd.StatusUpdateCore = objTranspointFind01.StatusUpdateCore;
                    objListOfTransPointWorkUpd.CallApiTxnStatus = objTranspointFind01.CallApiTxnStatus;
                    objListOfTransPointWorkUpd.CallApiResRecords = objTranspointFind01.CallApiResRecords;
                    objListOfTransPointWorkUpd.MaApDungList = objTranspointFind01.MaApDungList;
                    objListOfTransPointWorkUpd.CallApiResponseCode = objTranspointFind01.CallApiResponseCode;
                    objListOfTransPointWorkUpd.CallApiResponseMsg = objTranspointFind01.CallApiResponseMsg;
                    sNameView = "UpdateListOfTransPointWork";
                }
                #endregion
            }

            else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())        //Trường hợp thêm mới nghiệp vụ thay đổi thông tin điểm giao dịch
            {
                #region ---3. Sự kiện thêm mới nghiệp vụ thay đổi thông tin điểm giao dịch --- 
                //Lấy thông tin điểm giao dịch ở bảng ListOfTranspoint
                var objTranspointFind01 = (_serviceTransPoint.GetListOfTransPointSearch("", pPosCode, "", pTxnPointCode, "", 0, 0, "", "")).FirstOrDefault();
                if (objTranspointFind01 != null)
                {
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                    objListOfTransPointWorkUpd.OrderNo = objTranspointFind01.OrderNo;
                    objListOfTransPointWorkUpd.OrderNoText = objTranspointFind01.OrderNoText;
                    objListOfTransPointWorkUpd.EventCode = "";
                    objListOfTransPointWorkUpd.EventName = "";
                    objListOfTransPointWorkUpd.ParentId = 0;
                    objListOfTransPointWorkUpd.ProvinceCode = objTranspointFind01.ProvinceCode;
                    objListOfTransPointWorkUpd.ProvinceName = objTranspointFind01.ProvinceName;
                    objListOfTransPointWorkUpd.PosCode = objTranspointFind01.PosCode;
                    objListOfTransPointWorkUpd.PosName = objTranspointFind01.PosName;
                    objListOfTransPointWorkUpd.DistrictCode = objTranspointFind01.DistrictCode;
                    objListOfTransPointWorkUpd.DistrictName = objTranspointFind01.DistrictName;
                    objListOfTransPointWorkUpd.CommuneCode = objTranspointFind01.CommuneCode;
                    objListOfTransPointWorkUpd.CommuneName = objTranspointFind01.CommuneName;
                    objListOfTransPointWorkUpd.TxnPointCode = objTranspointFind01.TxnPointCode;
                    objListOfTransPointWorkUpd.TxnPointName = objTranspointFind01.TxnPointName;
                    objListOfTransPointWorkUpd.VisitDate = objTranspointFind01.VisitDate;
                    objListOfTransPointWorkUpd.VisitDateText = objTranspointFind01.VisitDateText;
                    objListOfTransPointWorkUpd.Times = objTranspointFind01.Times;

                    objListOfTransPointWorkUpd.TimeBegin = objTranspointFind01.TimeBegin;
                    objListOfTransPointWorkUpd.TimeEnd = objTranspointFind01.TimeEnd;
                    objListOfTransPointWorkUpd.TimeBeginDate =DateTime.ParseExact(objTranspointFind01.TimeBegin,"H'h'mm",CultureInfo.InvariantCulture);                    
                    objListOfTransPointWorkUpd.TimeEndDate =DateTime.ParseExact(objTranspointFind01.TimeEnd, "H'h'mm", CultureInfo.InvariantCulture);

                    objListOfTransPointWorkUpd.TimeBeginNum = objTranspointFind01.TimeBeginNum;
                    objListOfTransPointWorkUpd.TimeEndNum = objTranspointFind01.TimeEndNum;
                    objListOfTransPointWorkUpd.Hours = objTranspointFind01.Hours;
                    objListOfTransPointWorkUpd.Minutes = objTranspointFind01.Minutes;
                    objListOfTransPointWorkUpd.Longitude = objTranspointFind01.Longitude;
                    objListOfTransPointWorkUpd.Latitude = objTranspointFind01.Latitude;
                    objListOfTransPointWorkUpd.IsInCommune = objTranspointFind01.IsInCommune;
                    objListOfTransPointWorkUpd.IsInPos = objTranspointFind01.IsInPos;
                    objListOfTransPointWorkUpd.IsInterWard = objTranspointFind01.IsInterWard;
                    objListOfTransPointWorkUpd.InterWardName = objTranspointFind01.InterWardName;
                    objListOfTransPointWorkUpd.EffectiveDate = objTranspointFind01.EffectiveDate;
                    objListOfTransPointWorkUpd.EffectiveDateText = objTranspointFind01.EffectiveDateText;
                    objListOfTransPointWorkUpd.TxnLocation = objTranspointFind01.TxnLocation;
                    objListOfTransPointWorkUpd.AddressDetail = objTranspointFind01.AddressDetail;
                    objListOfTransPointWorkUpd.AddressCode = objTranspointFind01.AddressCode;

                    objListOfTransPointWorkUpd.AddressFull = objTranspointFind01.AddressFull;
                    objListOfTransPointWorkUpd.PhoneSupport = objTranspointFind01.PhoneSupport;
                    objListOfTransPointWorkUpd.PhoneSupport01 = objTranspointFind01.PhoneSupport01;
                    objListOfTransPointWorkUpd.PhoneSupport02 = objTranspointFind01.PhoneSupport02;
                    objListOfTransPointWorkUpd.TxnStatus = objTranspointFind01.TxnStatus;
                    objListOfTransPointWorkUpd.TxnStatusText = objTranspointFind01.TxnStatusText;
                    objListOfTransPointWorkUpd.Status = StatusTrans.StatusCreated;
                    objListOfTransPointWorkUpd.StatusText = StatusTrans.GetByValue(objListOfTransPointWorkUpd.Status).Description;
                    objListOfTransPointWorkUpd.Remark = objTranspointFind01.Remark;

                    objListOfTransPointWorkUpd.CreatedBy = objTranspointFind01.CreatedBy;
                    objListOfTransPointWorkUpd.CreatedDate = objTranspointFind01.CreatedDate;
                    objListOfTransPointWorkUpd.ModifiedBy = objTranspointFind01.ModifiedBy;
                    objListOfTransPointWorkUpd.ModifiedDate = objTranspointFind01.ModifiedDate;
                    objListOfTransPointWorkUpd.ApproverBy = objTranspointFind01.ApproverBy;
                    objListOfTransPointWorkUpd.ApprovalDate = objTranspointFind01.ApprovalDate;
                    objListOfTransPointWorkUpd.BusinessDate = objTranspointFind01.BusinessDate;
                    objListOfTransPointWorkUpd.BusinessDateText = "";
                    objListOfTransPointWorkUpd.DocumentId = objTranspointFind01.DocumentId;
                    objListOfTransPointWorkUpd.StatusUpdateCore = objTranspointFind01.StatusUpdateCore;
                    objListOfTransPointWorkUpd.CallApiTxnStatus = objTranspointFind01.CallApiTxnStatus;
                    objListOfTransPointWorkUpd.CallApiResRecords = objTranspointFind01.CallApiResRecords;
                    objListOfTransPointWorkUpd.CallApiResponseCode = objTranspointFind01.CallApiResponseCode;
                    objListOfTransPointWorkUpd.CallApiResponseMsg = objTranspointFind01.CallApiResponseMsg;
                    if(!string.IsNullOrEmpty(objTranspointFind01.IsInCommune))
                        objListOfTransPointWorkUpd.MaApDungList = "1";
                    else if(!string.IsNullOrEmpty(objTranspointFind01.IsInPos))
                        objListOfTransPointWorkUpd.MaApDungList = "2";
                    pButtonType = EventFlag.EventFlag_Add.Value.ToString();
                    sNameView = "UpdateInfoListOfTransPointWork";
                }
                #endregion
            }
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
            //            objTranspointWorkUpd.Id = objUserManagementIDCViewTmp.Id;
            //            objTranspointWorkUpd.OrderNo = objUserManagementIDCViewTmp.OrderNo;
            //            objTranspointWorkUpd.FunctionType = objUserManagementIDCViewTmp.FunctionType;
            //            objTranspointWorkUpd.PosCode = objUserManagementIDCViewTmp.PosCode;
            //            objTranspointWorkUpd.PosName = objUserManagementIDCViewTmp.PosName;
            //            objTranspointWorkUpd.StaffId = objUserManagementIDCViewTmp.StaffId;
            //            objTranspointWorkUpd.StaffCode = objUserManagementIDCViewTmp.StaffCode;
            //            objTranspointWorkUpd.UserId = objUserManagementIDCViewTmp.UserId;
            //            objTranspointWorkUpd.NickName = objUserManagementIDCViewTmp.NickName;
            //            objTranspointWorkUpd.FirstName = objUserManagementIDCViewTmp.FirstName;
            //            objTranspointWorkUpd.LastName = objUserManagementIDCViewTmp.LastName;
            //            objTranspointWorkUpd.FullName = objUserManagementIDCViewTmp.FullName;
            //            objTranspointWorkUpd.EmailAddress = objUserManagementIDCViewTmp.EmailAddress;
            //            objTranspointWorkUpd.MobileNumber = objUserManagementIDCViewTmp.MobileNumber;
            //            objTranspointWorkUpd.DateOfBirth = objUserManagementIDCViewTmp.DateOfBirth;
            //            objTranspointWorkUpd.GroupName = objUserManagementIDCViewTmp.GroupName;
            //            objTranspointWorkUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //            objTranspointWorkUpd.AuthType = objUserManagementIDCViewTmp.AuthType;
            //            objTranspointWorkUpd.UserType = objUserManagementIDCViewTmp.UserType;
            //            objTranspointWorkUpd.MailIdFlag = objUserManagementIDCViewTmp.MailIdFlag;
            //            objTranspointWorkUpd.AuthsecType = objUserManagementIDCViewTmp.AuthsecType;
            //            objTranspointWorkUpd.ExtraAttributeUserRole = objUserManagementIDCViewTmp.GroupName;
            //            objTranspointWorkUpd.ExtraAttributeBranchCode = objUserManagementIDCViewTmp.PosCode;
            //            objTranspointWorkUpd.EffectiveDate = objUserManagementIDCViewTmp.EffectiveDate;
            //            objTranspointWorkUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //            objTranspointWorkUpd.BusinessDateText = objTranspointWorkUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //            objTranspointWorkUpd.SystemDate = dSystemDateIDCTmp.Date;
            //            objTranspointWorkUpd.SystemDateText = objTranspointWorkUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //            objTranspointWorkUpd.ExpiryDate = objUserManagementIDCViewTmp.ExpiryDate;
            //            objTranspointWorkUpd.Ticket = string.IsNullOrEmpty(objUserManagementIDCViewTmp.Ticket) ? "" : objUserManagementIDCViewTmp.Ticket;
            //            objTranspointWorkUpd.Remark = objUserManagementIDCViewTmp.Remark;
            //            objTranspointWorkUpd.OrtherNotes = objUserManagementIDCViewTmp.OrtherNotes;
            //            objTranspointWorkUpd.Status = objUserManagementIDCViewTmp.Status;
            //            objTranspointWorkUpd.StatusText = StatusBusinessFlow.GetByValue(objTranspointWorkUpd.Status).Description;

            //            objTranspointWorkUpd.UserStatus = objUserManagementIDCViewTmp.UserStatus;
            //            if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //                objTranspointWorkUpd.UserStatusText = "Khóa (Đóng)";
            //            else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //                objTranspointWorkUpd.UserStatusText = "Mở (Bình thường)";
            //            else if (objUserManagementIDCViewTmp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //                objTranspointWorkUpd.UserStatusText = "Tạm khóa (Lock)";
            //            else objTranspointWorkUpd.UserStatusText = "Không xác định";

            //            objTranspointWorkUpd.StatusUpdateCore = objUserManagementIDCViewTmp.StatusUpdateCore;
            //            objTranspointWorkUpd.SessionValReq = objUserManagementIDCViewTmp.SessionValReq;
            //            objTranspointWorkUpd.PrevStatus = objUserManagementIDCViewTmp.PrevStatus;
            //            objTranspointWorkUpd.ResponseAttributes = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ResponseAttributes) ? "" : objUserManagementIDCViewTmp.ResponseAttributes;
            //            objTranspointWorkUpd.CallApiStatus = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiStatus) ? "" : objUserManagementIDCViewTmp.CallApiStatus;
            //            objTranspointWorkUpd.CallApiReqRecordSl = objUserManagementIDCViewTmp.CallApiReqRecordSl;
            //            objTranspointWorkUpd.CallApiResponseCode = objUserManagementIDCViewTmp.CallApiResponseCode;
            //            objTranspointWorkUpd.CallApiResponseMsg = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiResponseMsg) ? "" : objUserManagementIDCViewTmp.CallApiResponseMsg;

            //            objTranspointWorkUpd.CreatedBy = objUserManagementIDCViewTmp.CreatedBy;
            //            objTranspointWorkUpd.CreatedDate = objUserManagementIDCViewTmp.CreatedDate;
            //            objTranspointWorkUpd.ModifiedBy = objUserManagementIDCViewTmp.ModifiedBy;
            //            objTranspointWorkUpd.ModifiedDate = objUserManagementIDCViewTmp.ModifiedDate;
            //            objTranspointWorkUpd.ApproverBy = objUserManagementIDCViewTmp.ApproverBy;
            //            objTranspointWorkUpd.ApprovalDate = objUserManagementIDCViewTmp.ApprovalDate;
            //            objTranspointWorkUpd.FunctionTypeName = string.IsNullOrEmpty(objUserManagementIDCViewTmp.FunctionType) ? "" : FunctionTypeFlag.GetByCode(objUserManagementIDCViewTmp.FunctionType).Description;
            //            if (listRoleUsers != null && listRoleUsers.Count != 0)
            //            {
            //                objTranspointWorkUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //                objTranspointWorkUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCViewTmp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //                objTranspointWorkUpd.RoleToTransferCashName = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //                objTranspointWorkUpd.RoleToTransferCashDescription = (objUserManagementIDCViewTmp.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //                objTranspointWorkUpd.RoleToTransferCashDescriptionDetail = objUserManagementIDCViewTmp.RoleToTransferCashDescription;
            //                objTranspointWorkUpd.GroupNameDetail = $"{objUserManagementIDCViewTmp.GroupName} - {objUserManagementIDCViewTmp.GroupNameText}";
            //            }
            //            objTranspointWorkUpd.StartDate = objUserManagementIDCViewTmp.StartDate;
            //            objTranspointWorkUpd.StartDateOld = (objUserManagementIDCViewTmp.StartDateOld.Year <= 1900) ? objUserManagementIDCViewTmp.StartDate : objUserManagementIDCViewTmp.StartDateOld;
            //            objTranspointWorkUpd.StartDateText = objUserManagementIDCViewTmp.StartDate.ToString(FormatParameters.FORMAT_DATE);
            //            objTranspointWorkUpd.StartDateOldText = objTranspointWorkUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);
            //            objTranspointWorkUpd.IpSetCode = objUserManagementIDCViewTmp.IpSetCode;
            //            objTranspointWorkUpd.IpSetDetail = objUserManagementIDCViewTmp.IpSetDetail;
            //            objTranspointWorkUpd.RestrictionFlag = objUserManagementIDCViewTmp.RestrictionFlag;
            //            objTranspointWorkUpd.RestrictionFlagCheck = (objTranspointWorkUpd.RestrictionFlag == 1) ? true : false;
            //            objTranspointWorkUpd.SubType = string.IsNullOrEmpty(objUserManagementIDCViewTmp.SubType) ? DefaultValue.UserIDC_SubType : objUserManagementIDCViewTmp.SubType;
            //            objTranspointWorkUpd.AuthsecTypeName = objUserManagementIDCViewTmp.AuthsecTypeName;
            //            objTranspointWorkUpd.MailIdFlagName = objUserManagementIDCViewTmp.MailIdFlagName;
            //            objTranspointWorkUpd.CallApiAutoGeneratedPassword = string.IsNullOrEmpty(objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword) ? "" : objUserManagementIDCViewTmp.CallApiAutoGeneratedPassword;
            //            objTranspointWorkUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOld) ? objUserManagementIDCViewTmp.GroupName : objUserManagementIDCViewTmp.GroupNameOld;
            //            objTranspointWorkUpd.GroupNameOldText = string.IsNullOrEmpty(objUserManagementIDCViewTmp.GroupNameOldText) ? objUserManagementIDCViewTmp.GroupNameText : objUserManagementIDCViewTmp.GroupNameOldText;

            //            objTranspointWorkUpd.PosCodeOld = string.IsNullOrEmpty(objTranspointWorkUpd.PosCodeOld) ? objTranspointWorkUpd.PosCode : objTranspointWorkUpd.PosCodeOld;
            //            objTranspointWorkUpd.PosNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.PosNameOld) ? objTranspointWorkUpd.PosName : objTranspointWorkUpd.PosNameOld;
            //            objTranspointWorkUpd.FirstNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.FirstNameOld) ? objTranspointWorkUpd.FirstName : objTranspointWorkUpd.FirstNameOld;
            //            objTranspointWorkUpd.LastNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.LastNameOld) ? objTranspointWorkUpd.LastName : objTranspointWorkUpd.LastNameOld;
            //            objTranspointWorkUpd.FullNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.FullNameOld) ? objTranspointWorkUpd.FullName : objTranspointWorkUpd.FullNameOld;
            //            objTranspointWorkUpd.EmailAddressOld = string.IsNullOrEmpty(objTranspointWorkUpd.EmailAddressOld) ? objTranspointWorkUpd.EmailAddress : objTranspointWorkUpd.EmailAddressOld;
            //            objTranspointWorkUpd.MobileNumberOld = string.IsNullOrEmpty(objTranspointWorkUpd.MobileNumberOld) ? objTranspointWorkUpd.MobileNumber : objTranspointWorkUpd.MobileNumberOld;
            //            objTranspointWorkUpd.DateOfBirthOld = (objTranspointWorkUpd.DateOfBirthOld.Year <= 1900) ? objTranspointWorkUpd.DateOfBirth : objTranspointWorkUpd.DateOfBirthOld;

            //            objTranspointWorkUpd.GenderCode = objUserManagementIDCViewTmp.GenderCode;
            //            objTranspointWorkUpd.GenderText = objUserManagementIDCViewTmp.GenderText;
            //            objTranspointWorkUpd.StaffPosCode = objUserManagementIDCViewTmp.StaffPosCode;
            //            objTranspointWorkUpd.StaffPosName = objUserManagementIDCViewTmp.StaffPosName;
            //            objTranspointWorkUpd.StaffDepartmentCode = objUserManagementIDCViewTmp.StaffDepartmentCode;
            //            objTranspointWorkUpd.StaffDepartmentName = objUserManagementIDCViewTmp.StaffDepartmentName;
            //            objTranspointWorkUpd.StaffPositionCode = objUserManagementIDCViewTmp.StaffPositionCode;
            //            objTranspointWorkUpd.StaffPositionName = objUserManagementIDCViewTmp.StaffPositionName;
            //            objTranspointWorkUpd.StaffEmail = objUserManagementIDCViewTmp.StaffEmail;
            //            objTranspointWorkUpd.StaffMobileNo = objUserManagementIDCViewTmp.StaffMobileNo;
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld) ? objTranspointWorkUpd.RoleToTransferCashDescriptionDetail : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionDetailOld;
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld) ? objUserManagementIDCViewTmp.RoleToTransferCashDescription : objUserManagementIDCViewTmp.RoleToTransferCashDescriptionOld;
            //            objTranspointWorkUpd.RoleToTransferCashNameOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashNameOld) ? objUserManagementIDCViewTmp.RoleToTransferCashName : objUserManagementIDCViewTmp.RoleToTransferCashNameOld;
            //            objTranspointWorkUpd.RoleToTransferCashValueOld= string.IsNullOrEmpty(objUserManagementIDCViewTmp.RoleToTransferCashValueOld) ? objUserManagementIDCViewTmp.RoleToTransferCashValue : objUserManagementIDCViewTmp.RoleToTransferCashValueOld;
            //            objTranspointWorkUpd.ListFileId = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ListFileId) ? "" : objUserManagementIDCViewTmp.ListFileId;
            //            objTranspointWorkUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementIDCViewTmp.ReasonReject) ? "" : objUserManagementIDCViewTmp.ReasonReject;
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

            //            objTranspointWorkUpd.Id = objUserManagementIDCTemp01.Id;
            //            objTranspointWorkUpd.OrderNo = 1;
            //            objTranspointWorkUpd.FunctionType = objUserManagementIDCTemp01.FunctionType;
            //            objTranspointWorkUpd.FunctionTypeName = objUserManagementIDCTemp01.FunctionTypeName;

            //            objTranspointWorkUpd.PosCode = objUserManagementIDCTemp01.PosCode;
            //            objTranspointWorkUpd.PosName = objUserManagementIDCTemp01.PosName;
            //            objTranspointWorkUpd.StaffId = objUserManagementIDCTemp01.StaffId;
            //            objTranspointWorkUpd.StaffCode = objUserManagementIDCTemp01.StaffCode;
            //            objTranspointWorkUpd.UserId = objUserManagementIDCTemp01.UserId;
            //            objTranspointWorkUpd.NickName = objUserManagementIDCTemp01.NickName;
            //            objTranspointWorkUpd.FirstName = objUserManagementIDCTemp01.FirstName;
            //            objTranspointWorkUpd.LastName = objUserManagementIDCTemp01.LastName;
            //            objTranspointWorkUpd.FullName = objUserManagementIDCTemp01.FullName;
            //            objTranspointWorkUpd.EmailAddress = objUserManagementIDCTemp01.EmailAddress;
            //            objTranspointWorkUpd.MobileNumber = objUserManagementIDCTemp01.MobileNumber;
            //            objTranspointWorkUpd.DateOfBirth = objUserManagementIDCTemp01.DateOfBirth;
            //            objTranspointWorkUpd.GroupName = objUserManagementIDCTemp01.GroupName;
            //            objTranspointWorkUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //            objTranspointWorkUpd.AuthType = objUserManagementIDCTemp01.AuthType;
            //            objTranspointWorkUpd.UserType = objUserManagementIDCTemp01.UserType;
            //            objTranspointWorkUpd.MailIdFlag = objUserManagementIDCTemp01.MailIdFlag;
            //            objTranspointWorkUpd.AuthsecType = objUserManagementIDCTemp01.AuthsecType;
            //            objTranspointWorkUpd.ExtraAttributeUserRole = objUserManagementIDCTemp01.GroupName;
            //            objTranspointWorkUpd.ExtraAttributeBranchCode = objUserManagementIDCTemp01.PosCode;
            //            objTranspointWorkUpd.EffectiveDate = objUserManagementIDCTemp01.EffectiveDate;
            //            objTranspointWorkUpd.BusinessDate = objUserManagementIDCTemp01.BusinessDate;
            //            objTranspointWorkUpd.BusinessDateText = objTranspointWorkUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //            objTranspointWorkUpd.SystemDate = dSystemDateIDCTmp.Date;
            //            objTranspointWorkUpd.SystemDateText = objTranspointWorkUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE); 
            //            objTranspointWorkUpd.ExpiryDate = objUserManagementIDCTemp01.ExpiryDate;
            //            objTranspointWorkUpd.Ticket = objUserManagementIDCTemp01.Ticket;
            //            objTranspointWorkUpd.Remark = objUserManagementIDCTemp01.Remark;
            //            objTranspointWorkUpd.OrtherNotes = objUserManagementIDCTemp01.OrtherNotes;
            //            objTranspointWorkUpd.Status = objUserManagementIDCTemp01.Status;
            //            objTranspointWorkUpd.StatusText = StatusBusinessFlow.GetByValue(objTranspointWorkUpd.Status).Description;
            //            objTranspointWorkUpd.UserStatus = objUserManagementIDCTemp01.UserStatus;
            //            if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //                objTranspointWorkUpd.UserStatusText = "Khóa (Đóng)";
            //            else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //                objTranspointWorkUpd.UserStatusText = "Mở (Bình thường)";
            //            else if (objUserManagementIDCTemp01.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //                objTranspointWorkUpd.UserStatusText = "Tạm khóa (Lock)";
            //            else objTranspointWorkUpd.UserStatusText = "Không xác định";

            //            objTranspointWorkUpd.StatusUpdateCore = objUserManagementIDCTemp01.StatusUpdateCore;
            //            objTranspointWorkUpd.SessionValReq = objUserManagementIDCTemp01.SessionValReq;
            //            objTranspointWorkUpd.PrevStatus = objUserManagementIDCTemp01.PrevStatus;
            //            objTranspointWorkUpd.ResponseAttributes = objUserManagementIDCTemp01.ResponseAttributes;
            //            objTranspointWorkUpd.CallApiStatus = objUserManagementIDCTemp01.CallApiStatus;
            //            objTranspointWorkUpd.CallApiReqRecordSl = objUserManagementIDCTemp01.CallApiReqRecordSl;
            //            objTranspointWorkUpd.CallApiResponseCode = objUserManagementIDCTemp01.CallApiResponseCode;
            //            objTranspointWorkUpd.CallApiResponseMsg = objUserManagementIDCTemp01.CallApiResponseMsg;

            //            objTranspointWorkUpd.CreatedBy = objUserManagementIDCTemp01.CreatedBy;
            //            objTranspointWorkUpd.CreatedDate = objUserManagementIDCTemp01.CreatedDate;
            //            objTranspointWorkUpd.ModifiedBy = objUserManagementIDCTemp01.ModifiedBy;
            //            objTranspointWorkUpd.ModifiedDate = objUserManagementIDCTemp01.ModifiedDate;
            //            objTranspointWorkUpd.ApproverBy = objUserManagementIDCTemp01.ApproverBy;
            //            objTranspointWorkUpd.ApprovalDate = objUserManagementIDCTemp01.ApprovalDate;

            //            if (listRoleUsers != null && listRoleUsers.Count != 0)
            //            {
            //                objTranspointWorkUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //                objTranspointWorkUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //                objTranspointWorkUpd.RoleToTransferCashName = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //                objTranspointWorkUpd.RoleToTransferCashDescription = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //                objTranspointWorkUpd.RoleToTransferCashDescriptionDetail = objTranspointWorkUpd.RoleToTransferCashDescription;
            //                objTranspointWorkUpd.GroupNameDetail = $"{objTranspointWorkUpd.GroupName} - {objTranspointWorkUpd.GroupNameText}";

            //                objTranspointWorkUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementIDCTemp01.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();

            //            }
            //            objTranspointWorkUpd.StartDate = objUserManagementIDCTemp01.StartDate;
            //            objTranspointWorkUpd.IpSetCode = objUserManagementIDCTemp01.IpSetCode;
            //            objTranspointWorkUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementIDCTemp01.IpSetDetail) ? "" : objUserManagementIDCTemp01.IpSetDetail;
            //            objTranspointWorkUpd.RestrictionFlag = 0;
            //            objTranspointWorkUpd.RestrictionFlagCheck = (objTranspointWorkUpd.RestrictionFlag == 1) ? true : false;

            //            objTranspointWorkUpd.SubType = objUserManagementIDCTemp01.SubType;
            //            objTranspointWorkUpd.AuthsecTypeName = objUserManagementIDCTemp01.AuthsecTypeName;
            //            objTranspointWorkUpd.MailIdFlagName = objUserManagementIDCTemp01.MailIdFlagName;
            //            objTranspointWorkUpd.CallApiAutoGeneratedPassword = objUserManagementIDCTemp01.CallApiAutoGeneratedPassword;

            //            objTranspointWorkUpd.PosCodeOld = objUserManagementIDCTemp01.PosCodeOld;
            //            objTranspointWorkUpd.PosNameOld = objUserManagementIDCTemp01.PosNameOld;
            //            objTranspointWorkUpd.GroupNameOld = objUserManagementIDCTemp01.GroupNameOld;
            //            objTranspointWorkUpd.FirstNameOld = objUserManagementIDCTemp01.FirstNameOld;
            //            objTranspointWorkUpd.LastNameOld = objUserManagementIDCTemp01.LastNameOld;
            //            objTranspointWorkUpd.FullNameOld = objUserManagementIDCTemp01.FullNameOld;
            //            objTranspointWorkUpd.EmailAddressOld = objUserManagementIDCTemp01.EmailAddressOld;
            //            objTranspointWorkUpd.MobileNumberOld = objUserManagementIDCTemp01.MobileNumberOld;
            //            objTranspointWorkUpd.DateOfBirthOld = objUserManagementIDCTemp01.DateOfBirthOld;
            //            objTranspointWorkUpd.GroupNameOldText = string.IsNullOrEmpty(objTranspointWorkUpd.GroupNameOldText) ? objTranspointWorkUpd.GroupNameOldText : objTranspointWorkUpd.GroupNameOldText;
            //            objTranspointWorkUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashValueOld) ? objTranspointWorkUpd.RoleToTransferCashValue : objTranspointWorkUpd.RoleToTransferCashValueOld;
            //            objTranspointWorkUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashNameOld) ? objTranspointWorkUpd.RoleToTransferCashName : objTranspointWorkUpd.RoleToTransferCashNameOld;
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionOld) ? objTranspointWorkUpd.RoleToTransferCashDescription : objTranspointWorkUpd.RoleToTransferCashDescriptionOld;
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld) ? objTranspointWorkUpd.RoleToTransferCashDescriptionDetail : objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld;
            //            objTranspointWorkUpd.StartDateOld = objTranspointWorkUpd.StartDate;
            //            objTranspointWorkUpd.StartDateOldText = objTranspointWorkUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

            //            //objTranspointWorkUpd.StartDate = objTranspointWorkUpd.BusinessDate;
            //            objTranspointWorkUpd.EndDateChangeRole = objTranspointWorkUpd.ExpiryDate;
            //            objTranspointWorkUpd.ChoiceEndDateChangeRole = 0;
            //            int numberDays = (objTranspointWorkUpd.ExpiryDate - objTranspointWorkUpd.StartDate).Days;
            //            if (numberDays <= 90)
            //                objTranspointWorkUpd.ChoiceEndDateChangeRole = 1;

            //            objTranspointWorkUpd.GenderCode = objUserManagementIDCTemp01.GenderCode;
            //            objTranspointWorkUpd.GenderText = objUserManagementIDCTemp01.GenderText;
            //            objTranspointWorkUpd.StaffPosCode = objUserManagementIDCTemp01.StaffPosCode;
            //            objTranspointWorkUpd.StaffPosName = objUserManagementIDCTemp01.StaffPosName;
            //            objTranspointWorkUpd.StaffDepartmentCode = objUserManagementIDCTemp01.StaffDepartmentCode;
            //            objTranspointWorkUpd.StaffDepartmentName = objUserManagementIDCTemp01.StaffDepartmentName;
            //            objTranspointWorkUpd.StaffPositionCode = objUserManagementIDCTemp01.StaffPositionCode;
            //            objTranspointWorkUpd.StaffPositionName = objUserManagementIDCTemp01.StaffPositionName;
            //            objTranspointWorkUpd.StaffEmail = objUserManagementIDCTemp01.StaffEmail;
            //            objTranspointWorkUpd.StaffMobileNo = objUserManagementIDCTemp01.StaffMobileNo;
            //            //Lấy theo QLNS khi thay đổi thông tin người dùng
            //            objTranspointWorkUpd.EmailAddress = objUserManagementIDCTemp01.StaffEmail;
            //            objTranspointWorkUpd.MobileNumber = objUserManagementIDCTemp01.StaffMobileNo;
            //            objTranspointWorkUpd.ExistsInCore = objUserManagementIDCTemp01.ExistsInCore;
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

            //        objTranspointWorkUpd.Id = 0;// objUserManagementMTFind.Id;
            //        objTranspointWorkUpd.OrderNo = 1;
            //        objTranspointWorkUpd.FunctionType = "";

            //        objTranspointWorkUpd.PosCode = objUserManagementMTFind.PosCode;
            //        objTranspointWorkUpd.PosName = objUserManagementMTFind.PosName;
            //        objTranspointWorkUpd.StaffId = objUserManagementMTFind.StaffId;
            //        objTranspointWorkUpd.StaffCode = objUserManagementMTFind.StaffCode;
            //        objTranspointWorkUpd.UserId = objUserManagementMTFind.UserId;
            //        objTranspointWorkUpd.NickName = objUserManagementMTFind.NickName;
            //        objTranspointWorkUpd.FirstName = objUserManagementMTFind.FirstName;
            //        objTranspointWorkUpd.LastName = objUserManagementMTFind.LastName;
            //        objTranspointWorkUpd.FullName = objUserManagementMTFind.FullName;
            //        objTranspointWorkUpd.EmailAddress = objUserManagementMTFind.EmailAddress;
            //        objTranspointWorkUpd.MobileNumber = objUserManagementMTFind.MobileNumber;
            //        objTranspointWorkUpd.DateOfBirth = objUserManagementMTFind.DateOfBirth;
            //        objTranspointWorkUpd.GroupName = objUserManagementMTFind.GroupName;
            //        objTranspointWorkUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objTranspointWorkUpd.AuthType = objUserManagementMTFind.AuthType;
            //        objTranspointWorkUpd.UserType = objUserManagementMTFind.UserType;
            //        objTranspointWorkUpd.MailIdFlag = objUserManagementMTFind.MailIdFlag;
            //        objTranspointWorkUpd.AuthsecType = objUserManagementMTFind.AuthsecType;
            //        objTranspointWorkUpd.ExtraAttributeUserRole = objUserManagementMTFind.GroupName;
            //        objTranspointWorkUpd.ExtraAttributeBranchCode = objUserManagementMTFind.PosCode;
            //        objTranspointWorkUpd.EffectiveDate = objUserManagementMTFind.EffectiveDate;
            //        objTranspointWorkUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //        objTranspointWorkUpd.BusinessDateText = objTranspointWorkUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objTranspointWorkUpd.SystemDateText = objTranspointWorkUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.ExpiryDate = objUserManagementMTFind.ExpiryDate;
            //        objTranspointWorkUpd.ExpiryDateOld = objUserManagementMTFind.ExpiryDate;
            //        objTranspointWorkUpd.ExpiryDateOldText = objUserManagementMTFind.ExpiryDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.Ticket = "";
            //        objTranspointWorkUpd.Remark = "";
            //        objTranspointWorkUpd.OrtherNotes = "";
            //        objTranspointWorkUpd.Status = StatusBusinessFlow.Status_Created.Value;
            //        objTranspointWorkUpd.StatusText = StatusBusinessFlow.GetByValue(objTranspointWorkUpd.Status).Description;

            //        objTranspointWorkUpd.UserStatus = objUserManagementMTFind.UserStatus;
            //        if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objTranspointWorkUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objTranspointWorkUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementMTFind.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objTranspointWorkUpd.UserStatusText = "Tmaj khóa (Lock)";
            //        else objTranspointWorkUpd.UserStatusText = "Không xác định";

            //        objTranspointWorkUpd.StatusUpdateCore = 0;
            //        objTranspointWorkUpd.SessionValReq = true;
            //        objTranspointWorkUpd.PrevStatus = 0;
            //        objTranspointWorkUpd.ResponseAttributes = "";
            //        objTranspointWorkUpd.CallApiStatus = "";
            //        objTranspointWorkUpd.CallApiReqRecordSl = 0;
            //        objTranspointWorkUpd.CallApiResponseCode = "";
            //        objTranspointWorkUpd.CallApiResponseMsg = "";

            //        objTranspointWorkUpd.CreatedBy = objUserManagementMTFind.CreatedBy;
            //        objTranspointWorkUpd.CreatedDate = objUserManagementMTFind.CreatedDate;
            //        objTranspointWorkUpd.ModifiedBy = objUserManagementMTFind.ModifiedBy;
            //        objTranspointWorkUpd.ModifiedDate = objUserManagementMTFind.ModifiedDate;
            //        objTranspointWorkUpd.ApproverBy = objUserManagementMTFind.ApproverBy;
            //        objTranspointWorkUpd.ApprovalDate = objUserManagementMTFind.ApprovalDate;
            //        objTranspointWorkUpd.FunctionTypeName = "";
            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objTranspointWorkUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objTranspointWorkUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementMTFind.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objTranspointWorkUpd.RoleToTransferCashName = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objTranspointWorkUpd.RoleToTransferCashDescription = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionDetail = objTranspointWorkUpd.RoleToTransferCashDescription;
            //            objTranspointWorkUpd.GroupNameDetail = $"{objTranspointWorkUpd.GroupName} - {objTranspointWorkUpd.GroupNameText}";
            //        }
            //        objTranspointWorkUpd.StartDate = objUserManagementMTFind.StartDate.Value;
            //        objTranspointWorkUpd.IpSetCode = objUserManagementMTFind.IpSetCode;
            //        objTranspointWorkUpd.IpSetDetail = objUserManagementMTFind.IpSetDetail;
            //        objTranspointWorkUpd.RestrictionFlag = 0;
            //        objTranspointWorkUpd.RestrictionFlagCheck = (objTranspointWorkUpd.RestrictionFlag == 1) ? true : false;

            //        objTranspointWorkUpd.SubType = objUserManagementMTFind.SubType;
            //        objTranspointWorkUpd.AuthsecTypeName = objUserManagementMTFind.AuthsecTypeName;
            //        objTranspointWorkUpd.MailIdFlagName = objUserManagementMTFind.MailIdFlagName;
            //        objTranspointWorkUpd.CallApiAutoGeneratedPassword = "";

            //        objTranspointWorkUpd.PosCodeOld = objUserManagementMTFind.PosCode;
            //        objTranspointWorkUpd.PosNameOld = objUserManagementMTFind.PosName;
            //        objTranspointWorkUpd.GroupNameOld = objUserManagementMTFind.GroupName;
            //        objTranspointWorkUpd.FirstNameOld = objUserManagementMTFind.FirstName;
            //        objTranspointWorkUpd.LastNameOld = objUserManagementMTFind.LastName;
            //        objTranspointWorkUpd.FullNameOld = objUserManagementMTFind.FullName;
            //        objTranspointWorkUpd.EmailAddressOld = objUserManagementMTFind.EmailAddress;
            //        objTranspointWorkUpd.MobileNumberOld = objUserManagementMTFind.MobileNumber;
            //        objTranspointWorkUpd.DateOfBirthOld = objUserManagementMTFind.DateOfBirth;
            //        objTranspointWorkUpd.GroupNameOldText = objTranspointWorkUpd.GroupNameText;
            //        objTranspointWorkUpd.RoleToTransferCashValueOld = objTranspointWorkUpd.RoleToTransferCashValue;
            //        objTranspointWorkUpd.RoleToTransferCashNameOld = objTranspointWorkUpd.RoleToTransferCashName;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionOld = objTranspointWorkUpd.RoleToTransferCashDescription;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld = objTranspointWorkUpd.RoleToTransferCashDescriptionDetail;
            //        objTranspointWorkUpd.StartDateOld = objTranspointWorkUpd.StartDate;
            //        objTranspointWorkUpd.StartDateOldText = objTranspointWorkUpd.StartDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.StartDate = dSystemDateIDCTmp.Date;
            //        objTranspointWorkUpd.StartDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.EndDateChangeRole = objTranspointWorkUpd.SystemDate.AddDays(10);// CustConverter.StringToDateTime(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).Date;
            //        objTranspointWorkUpd.ChoiceEndDateChangeRole = 0;
            //        objTranspointWorkUpd.GenderCode = objUserManagementMTFind.GenderCode;
            //        objTranspointWorkUpd.GenderText = objUserManagementMTFind.GenderText;
            //        objTranspointWorkUpd.StaffPosCode = objUserManagementMTFind.StaffPosCode;
            //        objTranspointWorkUpd.StaffPosName = objUserManagementMTFind.StaffPosName;
            //        objTranspointWorkUpd.StaffDepartmentCode = objUserManagementMTFind.StaffDepartmentCode;
            //        objTranspointWorkUpd.StaffDepartmentName = objUserManagementMTFind.StaffDepartmentName;
            //        objTranspointWorkUpd.StaffPositionCode = objUserManagementMTFind.StaffPositionCode;
            //        objTranspointWorkUpd.StaffPositionName = objUserManagementMTFind.StaffPositionName;
            //        objTranspointWorkUpd.StaffEmail = objUserManagementMTFind.StaffEmail;
            //        objTranspointWorkUpd.StaffMobileNo = objUserManagementMTFind.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        objTranspointWorkUpd.EmailAddress = objUserManagementMTFind.StaffEmail;
            //        objTranspointWorkUpd.MobileNumber = objUserManagementMTFind.StaffMobileNo;

            //        objTranspointWorkUpd.ExistsInCore = objUserManagementMTFind.ExistsInCore;
            //        objTranspointWorkUpd.ListFileId = "";
            //        objTranspointWorkUpd.ReasonReject = "";
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

            //        objTranspointWorkUpd.Id = objUserManagementChangeTemp.Id;
            //        objTranspointWorkUpd.OrderNo = 1;
            //        objTranspointWorkUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
            //        objTranspointWorkUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

            //        objTranspointWorkUpd.PosCode = objUserManagementChangeTemp.PosCode;
            //        objTranspointWorkUpd.PosName = objUserManagementChangeTemp.PosName;
            //        objTranspointWorkUpd.StaffId = objUserManagementChangeTemp.StaffId;
            //        objTranspointWorkUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
            //        objTranspointWorkUpd.UserId = objUserManagementChangeTemp.UserId;
            //        objTranspointWorkUpd.NickName = objUserManagementChangeTemp.NickName;
            //        objTranspointWorkUpd.FirstName = objUserManagementChangeTemp.FirstName;
            //        objTranspointWorkUpd.LastName = objUserManagementChangeTemp.LastName;
            //        objTranspointWorkUpd.FullName = objUserManagementChangeTemp.FullName;
            //        objTranspointWorkUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
            //        objTranspointWorkUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
            //        objTranspointWorkUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
            //        objTranspointWorkUpd.GroupName = objUserManagementChangeTemp.GroupName;
            //        objTranspointWorkUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objTranspointWorkUpd.AuthType = objUserManagementChangeTemp.AuthType;
            //        objTranspointWorkUpd.UserType = objUserManagementChangeTemp.UserType;
            //        objTranspointWorkUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
            //        objTranspointWorkUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
            //        objTranspointWorkUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
            //        objTranspointWorkUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
            //        objTranspointWorkUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
            //        objTranspointWorkUpd.BusinessDate = objUserManagementChangeTemp.BusinessDate;
            //        objTranspointWorkUpd.BusinessDateText = objTranspointWorkUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objTranspointWorkUpd.SystemDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
            //        objTranspointWorkUpd.Ticket = objUserManagementChangeTemp.Ticket;
            //        objTranspointWorkUpd.Remark = objUserManagementChangeTemp.Remark;
            //        objTranspointWorkUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
            //        objTranspointWorkUpd.Status = objUserManagementChangeTemp.Status;
            //        objTranspointWorkUpd.StatusText = StatusBusinessFlow.GetByValue(objTranspointWorkUpd.Status).Description;
            //        objTranspointWorkUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
            //        if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objTranspointWorkUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objTranspointWorkUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objTranspointWorkUpd.UserStatusText = "Tạm khóa (Lock)";
            //        else objTranspointWorkUpd.UserStatusText = "Không xác định";

            //        objTranspointWorkUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
            //        objTranspointWorkUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
            //        objTranspointWorkUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
            //        objTranspointWorkUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
            //        objTranspointWorkUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
            //        objTranspointWorkUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
            //        objTranspointWorkUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
            //        objTranspointWorkUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

            //        objTranspointWorkUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
            //        objTranspointWorkUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
            //        objTranspointWorkUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
            //        objTranspointWorkUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
            //        objTranspointWorkUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
            //        objTranspointWorkUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objTranspointWorkUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objTranspointWorkUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objTranspointWorkUpd.RoleToTransferCashName = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objTranspointWorkUpd.RoleToTransferCashDescription = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionDetail = objTranspointWorkUpd.RoleToTransferCashDescription;
            //            objTranspointWorkUpd.GroupNameDetail = $"{objTranspointWorkUpd.GroupName} - {objTranspointWorkUpd.GroupNameText}";
            //            objTranspointWorkUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();
            //        }
            //        objTranspointWorkUpd.StartDate = objUserManagementChangeTemp.StartDate;
            //        objTranspointWorkUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
            //        objTranspointWorkUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
            //        objTranspointWorkUpd.RestrictionFlag = 0;
            //        objTranspointWorkUpd.RestrictionFlagCheck = (objTranspointWorkUpd.RestrictionFlag == 1) ? true : false;

            //        objTranspointWorkUpd.SubType = objUserManagementChangeTemp.SubType;
            //        objTranspointWorkUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
            //        objTranspointWorkUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
            //        objTranspointWorkUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

            //        objTranspointWorkUpd.PosCodeOld = objUserManagementChangeTemp.PosCodeOld;
            //        objTranspointWorkUpd.PosNameOld = objUserManagementChangeTemp.PosNameOld;
            //        objTranspointWorkUpd.GroupNameOld = objUserManagementChangeTemp.GroupNameOld;
            //        objTranspointWorkUpd.FirstNameOld = objUserManagementChangeTemp.FirstNameOld;
            //        objTranspointWorkUpd.LastNameOld = objUserManagementChangeTemp.LastNameOld;
            //        objTranspointWorkUpd.FullNameOld = objUserManagementChangeTemp.FullNameOld;
            //        objTranspointWorkUpd.EmailAddressOld = objUserManagementChangeTemp.EmailAddressOld;
            //        objTranspointWorkUpd.MobileNumberOld = objUserManagementChangeTemp.MobileNumberOld;
            //        objTranspointWorkUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
            //        objTranspointWorkUpd.GroupNameOldText = string.IsNullOrEmpty(objTranspointWorkUpd.GroupNameOldText) ? objTranspointWorkUpd.GroupNameOldText : objTranspointWorkUpd.GroupNameOldText;
            //        objTranspointWorkUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashValueOld) ? objTranspointWorkUpd.RoleToTransferCashValue : objTranspointWorkUpd.RoleToTransferCashValueOld;
            //        objTranspointWorkUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashNameOld) ? objTranspointWorkUpd.RoleToTransferCashName : objTranspointWorkUpd.RoleToTransferCashNameOld;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionOld) ? objTranspointWorkUpd.RoleToTransferCashDescription : objTranspointWorkUpd.RoleToTransferCashDescriptionOld;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld) ? objTranspointWorkUpd.RoleToTransferCashDescriptionDetail : objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld;
            //        objTranspointWorkUpd.StartDateOld = objTranspointWorkUpd.StartDate;
            //        objTranspointWorkUpd.StartDateOldText = objTranspointWorkUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.StartDate = dSystemDateIDCTmp.Date;
            //        objTranspointWorkUpd.StartDateText = dSystemDateIDCTmp.ToString(FormatParameters.FORMAT_DATE);
            //        //objTranspointWorkUpd.StartDate = objTranspointWorkUpd.BusinessDate;
            //        objTranspointWorkUpd.EndDateChangeRole = objTranspointWorkUpd.ExpiryDate;
            //        objTranspointWorkUpd.ChoiceEndDateChangeRole = 0;
            //        int numberDays = (objTranspointWorkUpd.ExpiryDate - objTranspointWorkUpd.StartDate).Days;
            //        if (numberDays > 0 && numberDays <= 90 && objUserManagementChangeTemp.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
            //            objTranspointWorkUpd.ChoiceEndDateChangeRole = 1;

            //        objTranspointWorkUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
            //        objTranspointWorkUpd.GenderText = objUserManagementChangeTemp.GenderText;
            //        objTranspointWorkUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
            //        objTranspointWorkUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
            //        objTranspointWorkUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
            //        objTranspointWorkUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
            //        objTranspointWorkUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
            //        objTranspointWorkUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
            //        objTranspointWorkUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
            //        objTranspointWorkUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        objTranspointWorkUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
            //        objTranspointWorkUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
            //        objTranspointWorkUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
            //        objTranspointWorkUpd.ListFileId = string.IsNullOrEmpty(objUserManagementChangeTemp.ListFileId) ? "" : objUserManagementChangeTemp.ListFileId;
            //        objTranspointWorkUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementChangeTemp.ReasonReject) ? "" : objUserManagementChangeTemp.ReasonReject;
            //        var objUserInfoIDCTmp = await _userManagementIDCService.GetUserIDCInfoByApiViewUser(objUserManagementChangeTemp.UserId);
            //        if (objUserInfoIDCTmp != null && !string.IsNullOrEmpty(objUserInfoIDCTmp.UserId))
            //        {
            //            objTranspointWorkUpd.ExpiryDateOld = CustConverter.StringToDate(objUserInfoIDCTmp.ExpiryDate.Trim().Replace("-", "").Replace("/", ""), FormatParameters.FORMAT_DATE_INT).Date;//yyyy-MM-dd
            //            objTranspointWorkUpd.ExpiryDateOldText = objTranspointWorkUpd.ExpiryDateOld.ToString(FormatParameters.FORMAT_DATE);
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

            //        objTranspointWorkUpd.Id = objUserManagementChangeTemp.Id;
            //        objTranspointWorkUpd.OrderNo = 1;
            //        objTranspointWorkUpd.FunctionType = objUserManagementChangeTemp.FunctionType;
            //        objTranspointWorkUpd.FunctionTypeName = objUserManagementChangeTemp.FunctionTypeName;

            //        objTranspointWorkUpd.PosCode = objUserManagementChangeTemp.PosCode;
            //        objTranspointWorkUpd.PosName = objUserManagementChangeTemp.PosName;
            //        objTranspointWorkUpd.StaffId = objUserManagementChangeTemp.StaffId;
            //        objTranspointWorkUpd.StaffCode = objUserManagementChangeTemp.StaffCode;
            //        objTranspointWorkUpd.UserId = objUserManagementChangeTemp.UserId;
            //        objTranspointWorkUpd.NickName = objUserManagementChangeTemp.NickName;
            //        objTranspointWorkUpd.FirstName = objUserManagementChangeTemp.FirstName;
            //        objTranspointWorkUpd.LastName = objUserManagementChangeTemp.LastName;
            //        objTranspointWorkUpd.FullName = objUserManagementChangeTemp.FullName;
            //        objTranspointWorkUpd.EmailAddress = objUserManagementChangeTemp.EmailAddress;
            //        objTranspointWorkUpd.MobileNumber = objUserManagementChangeTemp.MobileNumber;
            //        objTranspointWorkUpd.DateOfBirth = objUserManagementChangeTemp.DateOfBirth;
            //        objTranspointWorkUpd.GroupName = objUserManagementChangeTemp.GroupName;
            //        objTranspointWorkUpd.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='{ConstValueAPI.EntityList_Code}' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");

            //        objTranspointWorkUpd.AuthType = objUserManagementChangeTemp.AuthType;
            //        objTranspointWorkUpd.UserType = objUserManagementChangeTemp.UserType;
            //        objTranspointWorkUpd.MailIdFlag = objUserManagementChangeTemp.MailIdFlag;
            //        objTranspointWorkUpd.AuthsecType = objUserManagementChangeTemp.AuthsecType;
            //        objTranspointWorkUpd.ExtraAttributeUserRole = objUserManagementChangeTemp.GroupName;
            //        objTranspointWorkUpd.ExtraAttributeBranchCode = objUserManagementChangeTemp.PosCode;
            //        objTranspointWorkUpd.EffectiveDate = objUserManagementChangeTemp.EffectiveDate;
            //        objTranspointWorkUpd.BusinessDate = dBusinessDateIDCTmp.Date;
            //        objTranspointWorkUpd.BusinessDateText = objTranspointWorkUpd.BusinessDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.SystemDate = dSystemDateIDCTmp.Date;
            //        objTranspointWorkUpd.SystemDateText = objTranspointWorkUpd.SystemDate.ToString(FormatParameters.FORMAT_DATE);
            //        objTranspointWorkUpd.ExpiryDate = objUserManagementChangeTemp.ExpiryDate;
            //        objTranspointWorkUpd.Ticket = objUserManagementChangeTemp.Ticket;
            //        objTranspointWorkUpd.Remark = objUserManagementChangeTemp.Remark;
            //        objTranspointWorkUpd.OrtherNotes = objUserManagementChangeTemp.OrtherNotes;
            //        objTranspointWorkUpd.Status = objUserManagementChangeTemp.Status;
            //        objTranspointWorkUpd.StatusText = StatusBusinessFlow.GetByValue(objTranspointWorkUpd.Status).Description;
            //        objTranspointWorkUpd.UserStatus = objUserManagementChangeTemp.UserStatus;
            //        if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
            //            objTranspointWorkUpd.UserStatusText = "Khóa (Đóng)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
            //            objTranspointWorkUpd.UserStatusText = "Mở (Bình thường)";
            //        else if (objUserManagementChangeTemp.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
            //            objTranspointWorkUpd.UserStatusText = "Tạm khóa (Lock)";
            //        else objTranspointWorkUpd.UserStatusText = "Không xác định";

            //        objTranspointWorkUpd.StatusUpdateCore = objUserManagementChangeTemp.StatusUpdateCore;
            //        objTranspointWorkUpd.SessionValReq = objUserManagementChangeTemp.SessionValReq;
            //        objTranspointWorkUpd.PrevStatus = objUserManagementChangeTemp.PrevStatus;
            //        objTranspointWorkUpd.ResponseAttributes = objUserManagementChangeTemp.ResponseAttributes;
            //        objTranspointWorkUpd.CallApiStatus = objUserManagementChangeTemp.CallApiStatus;
            //        objTranspointWorkUpd.CallApiReqRecordSl = objUserManagementChangeTemp.CallApiReqRecordSl;
            //        objTranspointWorkUpd.CallApiResponseCode = objUserManagementChangeTemp.CallApiResponseCode;
            //        objTranspointWorkUpd.CallApiResponseMsg = objUserManagementChangeTemp.CallApiResponseMsg;

            //        objTranspointWorkUpd.CreatedBy = objUserManagementChangeTemp.CreatedBy;
            //        objTranspointWorkUpd.CreatedDate = objUserManagementChangeTemp.CreatedDate;
            //        objTranspointWorkUpd.ModifiedBy = objUserManagementChangeTemp.ModifiedBy;
            //        objTranspointWorkUpd.ModifiedDate = objUserManagementChangeTemp.ModifiedDate;
            //        objTranspointWorkUpd.ApproverBy = objUserManagementChangeTemp.ApproverBy;
            //        objTranspointWorkUpd.ApprovalDate = objUserManagementChangeTemp.ApprovalDate;

            //        if (listRoleUsers != null && listRoleUsers.Count != 0)
            //        {
            //            objTranspointWorkUpd.GroupNameText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.ShortName).FirstOrDefault();
            //            objTranspointWorkUpd.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
            //            objTranspointWorkUpd.RoleToTransferCashName = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
            //            objTranspointWorkUpd.RoleToTransferCashDescription = (objTranspointWorkUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
            //            objTranspointWorkUpd.RoleToTransferCashDescriptionDetail = objTranspointWorkUpd.RoleToTransferCashDescription;
            //            objTranspointWorkUpd.GroupNameDetail = $"{objTranspointWorkUpd.GroupName} - {objTranspointWorkUpd.GroupNameText}";
            //            objTranspointWorkUpd.GroupNameOldText = listRoleUsers.Where(w => w.Code == objUserManagementChangeTemp.GroupNameOld).Select(s => s.ShortName).FirstOrDefault();
            //        }
            //        objTranspointWorkUpd.StartDate = objUserManagementChangeTemp.StartDate;
            //        objTranspointWorkUpd.StartDateText = string.IsNullOrEmpty(objUserManagementChangeTemp.StartDateText) ? objUserManagementChangeTemp.StartDate.ToString(FormatParameters.FORMAT_DATE) : objUserManagementChangeTemp.StartDateText;
            //        objTranspointWorkUpd.IpSetCode = objUserManagementChangeTemp.IpSetCode;
            //        objTranspointWorkUpd.IpSetDetail = string.IsNullOrEmpty(objUserManagementChangeTemp.IpSetDetail) ? "" : objUserManagementChangeTemp.IpSetDetail;
            //        objTranspointWorkUpd.RestrictionFlag = 0;
            //        objTranspointWorkUpd.RestrictionFlagCheck = (objTranspointWorkUpd.RestrictionFlag == 1) ? true : false;

            //        objTranspointWorkUpd.SubType = objUserManagementChangeTemp.SubType;
            //        objTranspointWorkUpd.AuthsecTypeName = objUserManagementChangeTemp.AuthsecTypeName;
            //        objTranspointWorkUpd.MailIdFlagName = objUserManagementChangeTemp.MailIdFlagName;
            //        objTranspointWorkUpd.CallApiAutoGeneratedPassword = objUserManagementChangeTemp.CallApiAutoGeneratedPassword;

            //        objTranspointWorkUpd.PosCodeOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosCodeOld) ? objUserManagementChangeTemp.PosCode : objUserManagementChangeTemp.PosCodeOld;
            //        objTranspointWorkUpd.PosNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.PosNameOld) ? objUserManagementChangeTemp.PosName : objUserManagementChangeTemp.PosNameOld;
            //        objTranspointWorkUpd.GroupNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.GroupNameOld) ? objUserManagementChangeTemp.GroupName : objUserManagementChangeTemp.GroupNameOld;
            //        objTranspointWorkUpd.FirstNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FirstNameOld) ? objUserManagementChangeTemp.FirstName : objUserManagementChangeTemp.FirstNameOld;
            //        objTranspointWorkUpd.LastNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.LastNameOld) ? objUserManagementChangeTemp.LastName : objUserManagementChangeTemp.LastNameOld;
            //        objTranspointWorkUpd.FullNameOld = string.IsNullOrEmpty(objUserManagementChangeTemp.FullNameOld) ? objUserManagementChangeTemp.FullName : objUserManagementChangeTemp.FullNameOld;
            //        objTranspointWorkUpd.EmailAddressOld = string.IsNullOrEmpty(objUserManagementChangeTemp.EmailAddressOld) ? objUserManagementChangeTemp.EmailAddress : objUserManagementChangeTemp.EmailAddressOld;
            //        objTranspointWorkUpd.MobileNumberOld = string.IsNullOrEmpty(objUserManagementChangeTemp.MobileNumberOld) ? objUserManagementChangeTemp.MobileNumber : objUserManagementChangeTemp.MobileNumberOld;
            //        objTranspointWorkUpd.DateOfBirthOld = objUserManagementChangeTemp.DateOfBirthOld;
            //        objTranspointWorkUpd.GroupNameOldText = string.IsNullOrEmpty(objTranspointWorkUpd.GroupNameOldText) ? objTranspointWorkUpd.GroupNameText : objTranspointWorkUpd.GroupNameOldText;
            //        objTranspointWorkUpd.RoleToTransferCashValueOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashValueOld) ? objTranspointWorkUpd.RoleToTransferCashValue : objTranspointWorkUpd.RoleToTransferCashValueOld;
            //        objTranspointWorkUpd.RoleToTransferCashNameOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashNameOld) ? objTranspointWorkUpd.RoleToTransferCashName : objTranspointWorkUpd.RoleToTransferCashNameOld;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionOld) ? objTranspointWorkUpd.RoleToTransferCashDescription : objTranspointWorkUpd.RoleToTransferCashDescriptionOld;
            //        objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld = string.IsNullOrEmpty(objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld) ? objTranspointWorkUpd.RoleToTransferCashDescriptionDetail : objTranspointWorkUpd.RoleToTransferCashDescriptionDetailOld;
            //        objTranspointWorkUpd.StartDateOld = objTranspointWorkUpd.StartDate;
            //        objTranspointWorkUpd.StartDateOldText = objTranspointWorkUpd.StartDateOld.ToString(FormatParameters.FORMAT_DATE);

            //        //objTranspointWorkUpd.StartDate = objTranspointWorkUpd.BusinessDate;
            //        objTranspointWorkUpd.EndDateChangeRole = objTranspointWorkUpd.ExpiryDate;
            //        objTranspointWorkUpd.ChoiceEndDateChangeRole = 0;
            //        int numberDays = (objTranspointWorkUpd.ExpiryDate - objTranspointWorkUpd.StartDate).Days;
            //        if (numberDays <= 90)
            //            objTranspointWorkUpd.ChoiceEndDateChangeRole = 1;

            //        objTranspointWorkUpd.GenderCode = objUserManagementChangeTemp.GenderCode;
            //        objTranspointWorkUpd.GenderText = objUserManagementChangeTemp.GenderText;
            //        objTranspointWorkUpd.StaffPosCode = objUserManagementChangeTemp.StaffPosCode;
            //        objTranspointWorkUpd.StaffPosName = objUserManagementChangeTemp.StaffPosName;
            //        objTranspointWorkUpd.StaffDepartmentCode = objUserManagementChangeTemp.StaffDepartmentCode;
            //        objTranspointWorkUpd.StaffDepartmentName = objUserManagementChangeTemp.StaffDepartmentName;
            //        objTranspointWorkUpd.StaffPositionCode = objUserManagementChangeTemp.StaffPositionCode;
            //        objTranspointWorkUpd.StaffPositionName = objUserManagementChangeTemp.StaffPositionName;
            //        objTranspointWorkUpd.StaffEmail = objUserManagementChangeTemp.StaffEmail;
            //        objTranspointWorkUpd.StaffMobileNo = objUserManagementChangeTemp.StaffMobileNo;
            //        //Lấy theo QLNS khi thay đổi thông tin người dùng
            //        //objTranspointWorkUpd.EmailAddress = objUserManagementChangeTemp.StaffEmail;
            //        //objTranspointWorkUpd.MobileNumber = objUserManagementChangeTemp.StaffMobileNo;
            //        objTranspointWorkUpd.ExistsInCore = objUserManagementChangeTemp.ExistsInCore;
            //        objTranspointWorkUpd.ListFileId = string.IsNullOrEmpty(objUserManagementChangeTemp.ListFileId) ? "" : objUserManagementChangeTemp.ListFileId;
            //        objTranspointWorkUpd.ReasonReject = string.IsNullOrEmpty(objUserManagementChangeTemp.ReasonReject) ? "" : objUserManagementChangeTemp.ReasonReject;
            //    }

            //    #endregion

            //    sNameView = "AuthorizeUserManagementIDC";
            //}
            //if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString()
            //        && (pButtonType == EventBusinessCode.EventCode_TransPoint_AddNew.Code || pButtonType == ""))
            //    sNameView = "UpdateListOfTransPointWork";
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

            ViewBag.EventCode = EventBusinessCode.GetListOfTransPointNoAdd();
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
                    objTranspointUpd.DistrictCode = string.IsNullOrEmpty(objTranspointUpd.DistrictCode) ? "00" : objTranspointUpd.DistrictCode;
                    objTranspointUpd.DistrictName = string.IsNullOrEmpty(objTranspointUpd.DistrictName) ? "-" : objTranspointUpd.DistrictName;
                    objTranspointUpd.CommuneCode = string.IsNullOrEmpty(objTranspointUpd.CommuneCode) ? "" : objTranspointUpd.CommuneCode;
                    objTranspointUpd.CommuneName = string.IsNullOrEmpty(objTranspointUpd.CommuneName) ? "" : objTranspointUpd.CommuneName;
                    objTranspointUpd.TxnPointCode = string.IsNullOrEmpty(objTranspointUpd.TxnPointCode) ? "" : objTranspointUpd.TxnPointCode;
                    objTranspointUpd.TxnPointName = string.IsNullOrEmpty(objTranspointUpd.TxnPointName) ? "" : objTranspointUpd.TxnPointName;
                    objTranspointUpd.AddressCode = string.IsNullOrEmpty(objTranspointUpd.AddressCode) ?"" : objTranspointUpd.AddressCode;
                    objTranspointUpd.TxnLocation = string.IsNullOrEmpty(objTranspointUpd.TxnLocation) ?"" : objTranspointUpd.TxnLocation;
                    objTranspointUpd.EventCode = string.IsNullOrEmpty(objTranspointUpd.EventCode) ?"" : objTranspointUpd.EventCode;
                    objTranspointUpd.CallApiTxnStatus = string.IsNullOrEmpty(objTranspointUpd.CallApiTxnStatus) ?"" : objTranspointUpd.CallApiTxnStatus;
                    objTranspointUpd.CallApiResponseCode = string.IsNullOrEmpty(objTranspointUpd.CallApiResponseCode) ?"" : objTranspointUpd.CallApiResponseCode;
                    objTranspointUpd.CallApiResponseMsg = string.IsNullOrEmpty(objTranspointUpd.CallApiResponseMsg) ?"" : objTranspointUpd.CallApiResponseMsg;
                    objTranspointUpd.TxnStatus = string.IsNullOrEmpty(objTranspointUpd.TxnStatus) ?"" : objTranspointUpd.TxnStatus;
                    objTranspointUpd.InterWardName = string.IsNullOrEmpty(objTranspointUpd.InterWardName) ?"" : objTranspointUpd.InterWardName;
                    objTranspointUpd.IsInterWard = string.IsNullOrEmpty(objTranspointUpd.InterWardName) ? "" : "x";
                    objTranspointUpd.AddressDetail = string.IsNullOrEmpty(objTranspointUpd.AddressDetail) ? "" :objTranspointUpd.AddressDetail;
                    objTranspointUpd.AddressFull = string.IsNullOrEmpty(objTranspointUpd.AddressFull) ? "" :objTranspointUpd.AddressFull;

                    objTranspointUpd.PhoneSupport = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport) ? "" : objTranspointUpd.PhoneSupport;
                    objTranspointUpd.PhoneSupport01 = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport01) ? "" : objTranspointUpd.PhoneSupport01;
                    objTranspointUpd.PhoneSupport02 = string.IsNullOrEmpty(objTranspointUpd.PhoneSupport02) ? "" : objTranspointUpd.PhoneSupport02;
                    objTranspointUpd.IsInCommune = objTranspointUpd.MaApDungList != null && objTranspointUpd.MaApDungList.Contains("1") ? "x" : "";
                    objTranspointUpd.IsInPos = objTranspointUpd.MaApDungList != null && objTranspointUpd.MaApDungList.Contains("2") ? "x" : "";
                    
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
