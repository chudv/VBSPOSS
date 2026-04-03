// ListOfCommunesController.cs
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
using VBSPOSS.Data.Models;
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
    public class ListOfCommunesController : BaseController
    {
        private readonly IListOfCommunesService _service;
        private readonly ILogger<ListOfCommunesController> _logger;
        private readonly IListOfValueService _serviceLOV;
        private readonly IApiInternalService _internalServiceAPI;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfCommunesController"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{BaseController}"/>.</param>
        /// <param name="adminService">The adminService<see cref="IAdministrationService"/>.</param>
        /// <param name="serviceLOV">The serviceLOV<see cref="IListOfValueService"/>.</param>
        /// <param name="sessionHelper">The sessionHelper<see cref="ISessionHelper"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        /// <param name="service">The service<see cref="IListOfCommunesService"/>.</param>
        /// <param name="internalServiceAPI">The internalServiceAPI<see cref="IApiInternalService"/>.</param>
        public ListOfCommunesController( ILogger<BaseController> logger, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper, 
                IMapper mapper, IListOfCommunesService service, IApiInternalService internalServiceAPI) : base(logger, adminService, sessionHelper)

        {
            _serviceLOV = serviceLOV;
            _service = service;
            _internalServiceAPI = internalServiceAPI;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        // ───────────────────────────────────────────────────────────
        // GET - JSON CHO COMBOBOX / LOV
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Trả về JSON cho ComboBox chọn Xã/Phường/Thị trấn.
        /// pFlagTextShow:
        ///   1 = Tên
        ///   2 = [Mã - Tên]
        ///   3 = Mã => Tên huyện - Tên xã
        ///   4 = Mã => Tên tỉnh - Tên huyện - Tên xã
        ///   5 = Tên huyện - Tên xã
        ///   6 = Tên tỉnh - Tên huyện - Tên xã
        /// </summary>
        [HttpGet]
        public JsonResult GetListCommunes( string pProvinceCode = "", string pDistrictCode = "", string pCommuneCode = "", string pPosCode = "", string pStatus = "0", string pTitleChoice = "", string pFlagTextShow = "1")
        {
            try
            {
                string sTitleChoice = string.IsNullOrEmpty(pTitleChoice)
                    ? "---Chọn Xã/Phường/Thị trấn---"
                    : pTitleChoice;

                ArrayList data = new ArrayList();

                var listCommunes = _service.GetLovCommuneList(
                    pProvinceCode, pDistrictCode, pCommuneCode, pPosCode, "");

                if (!string.IsNullOrEmpty(sTitleChoice) && string.IsNullOrEmpty(pCommuneCode))
                    data.Add(new { id = "", value = sTitleChoice });

                foreach (var item in listCommunes)
                {
                    bool statusMatch = pStatus == "0"
                        || (pStatus == "1" && item.Status == StatusValue.StatusOpenPOS);

                    if (!statusMatch) continue;

                    string displayValue = pFlagTextShow switch
                    {
                        "2" => $"{item.CommuneCode} - {item.CommuneName}",
                        "3" => $"{item.CommuneCode} => {item.DistrictName} - {item.CommuneName}",
                        "4" => $"{item.CommuneCode} => {item.ProvinceName} - {item.DistrictName} - {item.CommuneName}",
                        "5" => $"{item.DistrictName} - {item.CommuneName}",
                        "6" => $"{item.ProvinceName} - {item.DistrictName} - {item.CommuneName}",
                        _ => item.CommuneName?.Trim()
                    };

                    data.Add(new { id = item.CommuneCode, value = displayValue });
                }

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi GetListCommunes");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách xã/phường." });
            }
        }

        // ───────────────────────────────────────────────────────────
        // CREATE
        // ───────────────────────────────────────────────────────────

        [HttpPost]
        public JsonResult CreateCommune([FromBody] ListOfCommunesViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

                string currentUser = User.Identity?.Name ?? "System";
                bool result = _service.CreateCommune(model, currentUser);
                return Json(new
                {
                    success = result,
                    message = result ? "Thêm mới thành công." : "Thêm mới thất bại."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi CreateCommune");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm mới." });
            }
        }

        // ───────────────────────────────────────────────────────────
        // UPDATE
        // ───────────────────────────────────────────────────────────

        [HttpPost]
        public JsonResult UpdateCommune([FromBody] ListOfCommunesViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

                string currentUser = User.Identity?.Name ?? "System";
                bool result = _service.UpdateCommune(model, currentUser);
                return Json(new
                {
                    success = result,
                    message = result ? "Cập nhật thành công." : "Cập nhật thất bại."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi UpdateCommune");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật." });
            }
        }

        // ───────────────────────────────────────────────────────────
        // DELETE
        // ───────────────────────────────────────────────────────────

        [HttpPost]
        public JsonResult DeleteCommune(string pCommuneCode, string pPosCode)
        {
            try
            {
                bool result = _service.DeleteCommune(pCommuneCode, pPosCode);
                return Json(new
                {
                    success = result,
                    message = result ? "Xóa thành công." : "Xóa thất bại."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi DeleteCommune: {Code}", pCommuneCode);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa." });
            }
        }
    }
}