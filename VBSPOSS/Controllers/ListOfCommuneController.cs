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
    }
}