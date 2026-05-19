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
        public ListOfCommuneController(ILogger<BaseController> logger, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper,
                IMapper mapper, IListOfCommuneService serviceCommune, IApiInternalService internalServiceAPI) : base(logger, adminService, sessionHelper)

        {
            _serviceLOV = serviceLOV;
            _serviceCommune = serviceCommune;
            _internalServiceAPI = internalServiceAPI;
            _mapper = mapper;
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

            ViewBag.EventBusinessCodes = EventBusinessCode.GetListOfTransPoint();

            return View("IndexListOfCommuneWork");
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
        //public ActionResult LoadGridData_CommuneWorks([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, int pStatus)
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
        //        var listCommuneWorks = _serviceCommune.GetListOfCommunesSearch("", pPosCode, pTxnPointCode, pTxnPointName, -1, "", pEventCode);
        //        return Json(listCommuneWorks.ToDataSourceResult(request, ModelState));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, $"LoadGridData_TransPointWorks('{pPosCode}','{pEventCode}','{pTxnPointCode}','{pTxnPointName}',{pStatus}) => Error: {ex.Message}");
        //        ModelState.AddModelError("ERROR", $"{ex.Message}");
        //        return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
        //    }
        //}

        //add
        /// <summary>
        /// Hiển thị form Thêm mới thông tin danh mục địa phương (Xã/Phường)
        /// </summary>
        /// <summary>
        /// Hiển thị form Thêm mới thông tin danh mục địa phương
        /// </summary>
        /// <summary>
        /// Hiển thị form Thêm mới thông tin danh mục địa phương
        /// </summary>
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
                model.EventCode = string.IsNullOrEmpty(pEventCode)
                                ? EventBusinessCode.EventCode_Locality_AddNew.Code
                                : pEventCode;

                model.EventName = EventBusinessCode.EventCode_Locality_AddNew.Description;

                model.ParentId = 0;
                model.PosCode = pPosCode;
                model.PosName = "";

                model.ProvinceCode = "";
                model.ProvinceName = "";
                model.DistrictCode = "";
                model.DistrictName = "";
                model.CommuneCode = "";
                model.CommuneName = "";
                model.SubCommuneCode = "";
                model.SubCommuneName = "";

                model.DistrictFlag30A = "";
                model.AreaEconomic = "";
                model.CommuneFlag135 = "";
                model.Region_01 = "";
                model.Region_02 = "";
                model.Region_03 = "";
                model.Region_04 = "";
                model.DiffAreaCode = "";
                model.IsNewCountryside = "0";

                model.TxnPointCode = "";
                model.TxnPointName = "";

                model.VisitDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
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

                model.Status = StatusBusinessFlow.Status_Created.Value;
                model.StatusText = StatusBusinessFlow.Status_Created.Description;
                model.RecordStatus = "1";
                model.RecordStatusText = "Hoạt động";

                model.EffectDate = _serviceTransPoint?.GetDateInCoreIDC("1").Date ?? DateTime.Now.Date;
                model.BusinessDate = _serviceTransPoint?.GetDateInCoreIDC("1").Date ?? DateTime.Now.Date;

                model.EffectDateText = model.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                model.BusinessDateText = model.BusinessDate.ToString(FormatParameters.FORMAT_DATE);

                model.DocumentId = 0;

                model.CreatedBy = UserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = UserName;
                model.ModifiedDate = DateTime.Now;
                model.ApproverBy = UserName;
                model.ApprovalDate = DateTime.Now;

                model.StatusUpdateCore = 0;
                model.CallApiTxnStatus = "";
                model.CallApiResRecords = 0;
                model.CallApiResponseCode = "";
                model.CallApiResponseMsg = "";

                // OldInfo để trống
                model.PosCodeOldInfo = "";
                model.ProvinceCodeOldInfo = "";
                model.DistrictCodeOldInfo = "";
                model.CommuneCodeOldInfo = "";
                model.SubCommuneCodeOldInfo = "";
                model.StatusOldInfo = 0;
             //   model.EffectDateOldInfo = DefaultValue.MinDate;
            //    model.BusinessDateOldInfo = DefaultValue.MinDate;
                model.DocumentIdOldInfo = 0;

                model.FlagCall = pFlagCall;

                #endregion
            }

            sNameView = "UpdateListOfCommuneWork";


            TempData["FlagCall"] = pFlagCall;
            TempData["ButtonType"] = pButtonType;
            TempData["UserPosCode"] = UserPosCode;

            return PartialView(sNameView, model);
        }


    }
}