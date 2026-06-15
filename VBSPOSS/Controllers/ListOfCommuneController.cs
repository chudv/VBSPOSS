using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Globalization;
using VBSPOSS.Constants;
using VBSPOSS.Controllers;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class ListOfCommuneController : BaseController
    {
        private readonly IListOfCommuneService _serviceCommune;
        private readonly ILogger<ListOfCommuneController> _logger;
        private readonly IListOfValueService _serviceLOV;
        private readonly IApiInternalService _internalServiceAPI;
        private readonly IMapper _mapper;
        private readonly IListOfTransPointService _serviceTransPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfCommuneController"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{BaseController}"/>.</param>
        /// <param name="adminService">The adminService<see cref="IAdministrationService"/>.</param>
        /// <param name="serviceLOV">The serviceLOV<see cref="IListOfValueService"/>.</param>
        /// <param name="sessionHelper">The sessionHelper<see cref="ISessionHelper"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        /// <param name="service">The service<see cref="IListOfCommuneService"/>.</param>
        /// <param name="internalServiceAPI">The internalServiceAPI<see cref="IApiInternalService"/>.</param>
        public ListOfCommuneController(ILogger<BaseController> logger, IAdministrationService adminService, IListOfTransPointService serviceTransPoint, IListOfValueService serviceLOV, ISessionHelper sessionHelper,
                IMapper mapper, IListOfCommuneService serviceCommune, IApiInternalService internalServiceAPI) : base(logger, adminService, sessionHelper)

        {
            _serviceLOV = serviceLOV;
            _serviceCommune = serviceCommune;
            _internalServiceAPI = internalServiceAPI;
            _mapper = mapper;
            _serviceTransPoint = serviceTransPoint;
        }
        public IActionResult IndexListOfCommune()
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

            ViewBag.EventBusinessCodes = EventBusinessCode.GetListOfCommune();

            return View("IndexListOfCommuneWork");
        }

        /// <summary>
        /// Danh sách bản ghi Xã/Phường/Thôn => Tải từ bảng ListOfCommune
        /// </summary>
        public ActionResult LoadGridData_ListOfCommune([DataSourceRequest] DataSourceRequest request,
            string pPosCode, string pProvinceCode, string pDistrictCode, string pCommuneCode,
            string pSubCommuneCode, string pStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;

                // Giới hạn theo cấp người dùng
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS)
                    && !string.IsNullOrEmpty(pPosCode)
                    && pPosCode.Length >= 4)
                {
                    pPosCode = pPosCode.Substring(0, 4);
                }

                var listCommune = _serviceCommune.GetListOfCommuneSearch(
                    pPosCode: pPosCode,
                    pProvinceCode: pProvinceCode,
                    pDistrictCode: pDistrictCode,
                    pCommuneCode: pCommuneCode,
                    pSubCommuneCode: pSubCommuneCode,
                    pStatus: pStatus,
                    pUserPosCode: UserPosCode,           // ← Truyền vào
                    pUserGrade: UserGrade
                    );

                return Json(listCommune.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_ListOfCommune Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<ListOfCommuneViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Danh sách bản ghi Yêu cầu Thêm mới / Chỉnh sửa Xã - Thôn => Tải từ bảng ListOfCommuneWorks
        /// </summary>
        public ActionResult LoadGridData_ListOfCommuneWorks([DataSourceRequest] DataSourceRequest request,
            string pPosCode, string pEventCode, string pProvinceCode, string pCommuneCode,
            string pSubCommuneCode, string pStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;

                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS)
                    && !string.IsNullOrEmpty(pPosCode)
                    && pPosCode.Length >= 4)
                {
                    pPosCode = pPosCode.Substring(0, 4);
                }
                if (pStatus == "-1") pStatus = "";
                var listCommuneWorks = _serviceCommune.GetListOfCommuneWorkSearch(
                    pPosCode: pPosCode,
                    pEventCode: pEventCode,
                    pProvinceCode: pProvinceCode,
                    pCommuneCode: pCommuneCode,
                    pSubCommuneCode: pSubCommuneCode,
                    pStatus: pStatus,
                    pUserPosCode: UserPosCode,      // ← Truyền vào
                    pUserGrade: UserGrade);

                return Json(listCommuneWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_ListOfCommuneWorks Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<ListOfCommuneWorksViewModel>(), Total = 0 });
            }
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
        //public ActionResult LoadGridData_Commune([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, string pStatus)
        //{
        //    try
        //    {
        //        string sTxnPointCode = "", sTxnPointName = "";
        //        if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
        //            pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
        //        if (string.IsNullOrEmpty(pEventCode))
        //            pEventCode = "";
        //        if (string.IsNullOrEmpty(pTxnPointCode))
        //            pTxnPointCode = "";
        //        if (string.IsNullOrEmpty(pTxnPointName))
        //            pTxnPointName = "";
        //        if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS) && (pPosCode != "000100" && pPosCode != "000199" && pPosCode != "000196" && pPosCode != "000197" && pPosCode != "000101"))
        //        {
        //            if (!string.IsNullOrEmpty(pPosCode))
        //                pPosCode = pPosCode.Substring(0, 4);
        //        }
        //        var listCommuneWorks = _serviceCommune.GetListOfCommunesSearch("", pPosCode, pTxnPointCode, pTxnPointName, -1, pEventCode);
        //        return Json(listCommuneWorks.ToDataSourceResult(request, ModelState));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, $"LoadGridData_Commune('{pPosCode}','{pEventCode}','{pTxnPointCode}','{pTxnPointName}',{pStatus}) => Error: {ex.Message}");
        //        ModelState.AddModelError("ERROR", $"{ex.Message}");
        //        return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
        //    }
        //}


        // sửa
        public ActionResult LoadGridData_Commune([DataSourceRequest] DataSourceRequest request,
    string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, string pStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;

                if (string.IsNullOrEmpty(pEventCode)) pEventCode = "";
                if (string.IsNullOrEmpty(pTxnPointCode)) pTxnPointCode = "";
                if (string.IsNullOrEmpty(pTxnPointName)) pTxnPointName = "";

                // Xử lý phân quyền
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS)
                    && !string.IsNullOrEmpty(pPosCode) && pPosCode.Length >= 4)
                {
                    pPosCode = pPosCode.Substring(0, 4);
                }

                // Gọi hàm đúng hiện tại
                var listCommuneWorks = _serviceCommune.GetListOfCommuneWorkSearch(
                    pPosCode: pPosCode,
                    pEventCode: pEventCode,
                    pProvinceCode: "",
                    pCommuneCode: "",
                    pSubCommuneCode: "",
                    pStatus: pStatus,
                    pUserPosCode: UserPosCode,
                    pUserGrade: UserGrade
                );

                return Json(listCommuneWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_Commune Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<ListOfCommuneWorksViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hiển thị form Thêm mới thông tin danh mục địa phương (Xã/Phường)
        /// </summary>fLo
        /// <summary>
        /// Hiển thị form Thêm mới thông tin danh mục địa phương
        /// </summary>
        /// <summary>

        public ActionResult ShowUpdateListOfCommuneWork(string pButtonType, long pId, string pPosCode, string pEventCode, string pFlagCall)
        {
            ListOfCommuneWorksViewModel model = new ListOfCommuneWorksViewModel();
            string sNameView = "";   //

            if (string.IsNullOrEmpty(pPosCode)) pPosCode = "";
            if (string.IsNullOrEmpty(pEventCode)) pEventCode = "";
            if (string.IsNullOrEmpty(pFlagCall)) pFlagCall = EventFlag.EventFlag_Add.Value.ToString();

            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() || pFlagCall == "1")
            {
                #region --- THÊM MỚI ---
                model.OrderNo = 0;
                model.EventCode = EventBusinessCode.EventCode_Locality_AddNew.Code;
                model.EventName = EventBusinessCode.EventCode_Locality_AddNew.Description;
                model.ParentId = 0;
                model.PosCode = pPosCode;
                model.PosName = "";

                // Thông tin hành chính
                model.ProvinceCode = "";
                model.ProvinceName = "";
                model.DistrictCode = "";
                model.DistrictName = "";
                model.CommuneCode = "";
                model.CommuneName = "";
                model.SubCommuneCode = "";
                model.SubCommuneName = "";

                // Các cờ
                model.DistrictFlag30A = "N";
                model.AreaEconomic = "";
                model.CommuneFlag135 = "N";
                model.Region_01 = "N";
                model.Region_02 = "N";
                model.Region_03 = "N";
                model.Region_04 = "N";
                model.DiffAreaCode = "0";
                model.IsNewCountryside = "N";

                // Điểm giao dịch
                model.TxnPointCode = "";
                model.TxnPointName = "";
               // model.VisitDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                model.VisitDateText = DateTime.Now.ToString(FormatParameters.FORMAT_DATE);
                model.TimeBegin = "08:00";
                model.TimeEnd = "17:00";
                model.TimeBeginNum = 8.0m;
                model.TimeEndNum = 17.0m;
                model.Hours = 8.0m;
                model.Minutes = 0.0m;
                model.Longitude = 0;
                model.Latitude = 0;

                model.IsInCommune = "1";
                model.IsInPos = "1";
                model.IsInterWard = "0";
                model.InterWardName = "";

                // Workflow
                model.Status = StatusBusinessFlow.Status_Created.Value;
                model.StatusText = StatusBusinessFlow.Status_Created.Description;
                model.RecordStatus = "A";
                model.RecordStatusText = "Hoạt động";

                model.EffectDate = _serviceTransPoint?.GetDateInCoreIDC("1").Date ?? DateTime.Now.Date;
                model.BusinessDate = model.EffectDate;
                model.EffectDateText = model.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                model.BusinessDateText = model.BusinessDate.ToString(FormatParameters.FORMAT_DATE);

                model.DocumentId = 0;

                model.CreatedBy = UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = UserName;
                model.ModifiedDate = DateTime.Now;
                model.ApproverBy = "";
            //    model.ApprovalDate = null;

                model.StatusUpdateCore = 0;
                model.CallApiTxnStatus = "";
                model.CallApiResRecords = 0;
                model.CallApiResponseCode = "";
                model.CallApiResponseMsg = "";

                // Old Info
                model.PosCodeOldInfo = "";
                model.ProvinceCodeOldInfo = "";
               

                model.FlagCall = pFlagCall;
                #endregion
            }
            sNameView = "UpdateListOfCommuneWork";


            TempData["FlagCall"] = pFlagCall;
            TempData["ButtonType"] = pButtonType;
            TempData["UserPosCode"] = UserPosCode;

            return PartialView(sNameView, model);
        }
        // Hàm xử lý Lưu Thêm/ Sửa bảng dữ liệu ListOfCommunesWork
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdateListOfCommuneWork(ListOfCommuneWorksViewModel objCommuneUpd, string pFlagCall)
        {
            try
            {
                if (objCommuneUpd == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                string addType = objCommuneUpd.AddType ?? "Thon";

                if (addType == "Xa")
                {
                    if (string.IsNullOrWhiteSpace(objCommuneUpd.CommuneCode) || string.IsNullOrWhiteSpace(objCommuneUpd.CommuneName))
                        return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin Xã" });
                }
                else if (addType == "Thon")
                {
                    if (string.IsNullOrWhiteSpace(objCommuneUpd.CommuneCode))
                        return Json(new { success = false, message = "Vui lòng chọn Phường/Xã" });
                    if (string.IsNullOrWhiteSpace(objCommuneUpd.SubCommuneCode))
                        return Json(new { success = false, message = "Vui lòng nhập Mã Thôn" });
                    if (string.IsNullOrWhiteSpace(objCommuneUpd.SubCommuneName))
                        return Json(new { success = false, message = "Vui lòng nhập Tên Thôn" });
                }

                if (string.IsNullOrEmpty(pFlagCall)) pFlagCall = "1";

                //int result = _serviceCommune.UpdateListOfCommuneWork(objCommuneUpd, UserName, pFlagCall);
                int result = _serviceCommune.UpdateListOfCommuneWork(
    objCommuneUpd,
    UserName,
    pFlagCall,
    UserPosCode);   // ← Thêm dòng này

                return Json(new
                {
                    success = result > 0,
                    message = result > 0 ? "Thêm mới thành công!" : "Lưu thất bại"
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SaveUpdateListOfCommuneWork Error");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }


    }
}