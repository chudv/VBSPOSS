using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VBSPOSS.Constants;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements; //
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;
using System.Text.Json;

namespace VBSPOSS.Controllers
{
    public class ProductParameterController : BaseController
    {
        private readonly ILogger<ProductParameterController> _logger;
        private readonly IProductParameterService _service; //
        private readonly IMapper _mapper;

        public ProductParameterController(
            ILogger<ProductParameterController> logger,
            IProductParameterService service,
            IMapper mapper,
            IAdministrationService administrationService,
            ISessionHelper sessionHelper)
            : base(logger, administrationService, sessionHelper)
        {
            _logger = logger;
            _service = service;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var controller = RouteData.Values["controller"]?.ToString();
            var action = RouteData.Values["action"]?.ToString();
            SetPermitData(action, controller);

            RolePermissionModel userPermission = UserPermission;
            string role = UserRole.ToString();
            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);

            // Truyền danh sách ProductGroup để filter dropdown
            ViewBag.ProductGroups = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Tất cả" },
                new SelectListItem { Value = "CASA", Text = "Casa" },
                new SelectListItem { Value = "TIDE", Text = "Tide" },
                new SelectListItem { Value = "PENAL", Text = "Rút trước hạn (Penal Tide)" }
            };

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadComparisonGrid([DataSourceRequest] DataSourceRequest request,
            string productGroupCode = null, string productCode = null, DateTime? effectDate = null)
        {
            try
            {
                // Gọi service lấy danh sách so sánh (current vs proposed)
                var comparisonList = await _service.GetComparisonListAsync(
                    productGroupCode, productCode, effectDate ?? new DateTime(2026, 3, 1));

                // Log để debug
                _logger.LogInformation($"LoadComparisonGrid: {comparisonList.Count} records returned. Group: {productGroupCode ?? "All"}, Product: {productCode ?? "All"}");

                return Json(comparisonList.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadComparisonGrid error");
                return Json(new DataSourceResult { Errors = "Có lỗi khi tải bảng so sánh lãi suất." });
            }
        }


     

        [HttpPost]
        public async Task<ActionResult> LoadProductParametersGrid([DataSourceRequest] DataSourceRequest request,
    string productGroupCode = null,
    string productCode = null,
    string effectDateBegin = null,   // Từ ngày
    string effectDateEnd = null)     // Đến ngày
        {
            try
            {
                DateTime? fromDate = null;
                DateTime? toDate = null;

              
                if (!string.IsNullOrEmpty(effectDateBegin)
                    && DateTime.TryParseExact(effectDateBegin, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
                {
                    fromDate = parsedFrom.Date;
                }

                // Đến ngày
                if (!string.IsNullOrEmpty(effectDateEnd)
                    && DateTime.TryParseExact(effectDateEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
                {
                    toDate = parsedTo.Date;
                }

                var list = await _service.GetProductParametersViewListAsync(
                    productGroupCode,
                    productCode,
                    fromDate,
                    toDate
                );

                return Json(list.ToDataSourceResult(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu grid ProductParameter");
                return Json(new { Errors = "Lỗi khi tải dữ liệu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowCreateConfig(int pId = 0, string pFlagCall = "1")
        {
            try
            {
                var model = new ProductParameterCreateViewModel
                {
                    Id = 0,
                    EffectedDate = DateTime.Today.AddDays(1),
                };


                model.ProductGroupOptions = new List<SelectListItem>
{
    new SelectListItem { Value = "CASA", Text = "Casa" },
    new SelectListItem { Value = "TIDE", Text = "Tide" },
    new SelectListItem { Value = "DEPOSITPENAL", Text = "Rút trước hạn (Penal Tide)" } 
};



                ViewBag.FlagCall = pFlagCall;
                ViewBag.Title = "";

                return PartialView("_Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải màn hình tạo mới ProductParameter");
                return Content("Có lỗi xảy ra khi tải form thêm mới: " + ex.Message);
            }
        }


      

        [HttpPost]
        public async Task<JsonResult> LoadProductsForCreate([FromBody] LoadProductRequest request)
        {
            try
            {
                string productGroupCode = request?.productGroupCode;
                string effectedDateStr = request?.effectedDate;

                if (string.IsNullOrEmpty(productGroupCode))
                    return Json(new { error = "Vui lòng chọn phân loại" });

                DateTime effectedDt;
                if (string.IsNullOrEmpty(effectedDateStr) || !DateTime.TryParseExact(effectedDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out effectedDt))
                {
                    effectedDt = new DateTime(2026, 1, 1); // Fallback đúng DB của anh
                }

                var result = await _service.LoadProductsForCreateAsync(productGroupCode, effectedDt);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi load sản phẩm cho Create");
                return Json(new { error = "Lỗi hệ thống: " + ex.Message });
            }
        }

        

        // sửa hàm lưu thay đổi 

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<JsonResult> SaveBatchProductParameter([FromForm] SaveBatchRequest request)
        //{
        //    try
        //    {
        //        var remarkChung = request.Remark?.Trim() ?? "";

        //        if (string.IsNullOrEmpty(request.Items))
        //            return Json(new { success = false, message = "Không có dữ liệu thay đổi để lưu" });

        //        var items = JsonSerializer.Deserialize<List<ProductParameterDetailViewModel>>(request.Items);

        //        if (items == null || items.Count == 0)
        //            return Json(new { success = false, message = "Không có dữ liệu thay đổi để lưu" });

        //        var recordCount = await _service.SaveBatchProductParameterAsync(
        //            request.ProductGroupCode,
        //            request.EffectedDate,
        //            remarkChung,
        //            items
        //        );

        //        if (recordCount > 0)
        //        {
        //            return Json(new { success = true, message = $"Đã lưu thành công {items.Count} đề xuất!" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Lưu không thành công!" });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)   // ← Bắt riêng DbUpdateException
        //    {
        //        var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
        //        var innerInner = dbEx.InnerException?.InnerException?.Message;

        //        _logger.LogError(dbEx, "DbUpdateException khi lưu batch. Inner: {Inner} | InnerInner: {InnerInner}",
        //            innerMessage, innerInner);

        //        Console.WriteLine($"=== DbUpdateException ===");
        //        Console.WriteLine($"Message: {dbEx.Message}");
        //        Console.WriteLine($"Inner: {innerMessage}");
        //        if (!string.IsNullOrEmpty(innerInner))
        //            Console.WriteLine($"Inner Inner: {innerInner}");

        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Lỗi lưu dữ liệu: {innerMessage}"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi không xác định khi lưu batch");
        //        return Json(new { success = false, message = "Lỗi hệ thống khi lưu: " + ex.Message });
        //    }
        //}

        // sửa al
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveBatchProductParameter([FromForm] SaveBatchRequest request)
        {
            try
            {
                var remarkChung = request.Remark?.Trim() ?? "";

                if (string.IsNullOrEmpty(request.Items))
                    return Json(new { success = false, message = "Không có dữ liệu thay đổi để lưu" });

                var items = JsonSerializer.Deserialize<List<ProductParameterDetailViewModel>>(request.Items);

                if (items == null || items.Count == 0)
                    return Json(new { success = false, message = "Không có dữ liệu thay đổi để lưu" });

                var recordCount = await _service.SaveBatchProductParameterAsync(
                    request.ProductGroupCode,
                    request.EffectedDate,
                    remarkChung,
                    items
                );

                if (recordCount > 0)
                {
                    return Json(new { success = true, message = $"Đã lưu thành công {items.Count} đề xuất!" });
                }
                else
                {
                    return Json(new { success = false, message = "Lưu không thành công!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu batch ProductParameter");

                string message = ex.Message;

                // Xử lý riêng trường hợp trùng ngày hiệu lực
                if (message.Contains("Đã tồn tại cấu hình") || message.Contains("trùng"))
                {
                    // Làm cho thông báo ngắn gọn và rõ ràng hơn
                    if (message.Contains("Không thể tạo trùng"))
                    {
                        // Giữ nguyên thông báo từ Service
                        return Json(new { success = false, message = message });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Đã tồn tại cấu hình cho phân loại {request.ProductGroupCode} với ngày hiệu lực {request.EffectedDate:dd/MM/yyyy}. Không thể tạo trùng."
                        });
                    }
                }

                // Các lỗi khác (DbUpdateException, lỗi database, v.v.)
                return Json(new { success = false, message = "Lỗi hệ thống khi lưu: " + ex.Message });
            }
        }

    }
}