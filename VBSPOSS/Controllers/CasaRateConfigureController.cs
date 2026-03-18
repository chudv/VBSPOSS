using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class CasaRateConfigureController : BaseController
    {
        private readonly ILogger<CasaRateConfigureController> _logger;
        private readonly IInterestRateConfigureService _interestRateConfigureService;
        private readonly IProductService _createConfigService;
        private readonly IMapper _mapper;

        private readonly IProductService _productService; // 

        //public CasaRateConfigureController(
        //    ILogger<CasaRateConfigureController> logger,
        //    IInterestRateConfigureService interestRateConfigureService,
        //    IProductService createConfigService,
        //    IMapper mapper,
        //    IAdministrationService administrationService,
        //    ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        //{
        //    _logger = logger;
        //    _interestRateConfigureService = interestRateConfigureService;
        //    _createConfigService = createConfigService;
        //    _mapper = mapper;
        //}

        public CasaRateConfigureController(
    ILogger<CasaRateConfigureController> logger,
    IInterestRateConfigureService interestRateConfigureService,
    IProductService createConfigService,
    IProductService productService, // ← THÊM THAM SỐ NÀY
    IMapper mapper,
    IAdministrationService administrationService,
    ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        {
            _logger = logger;
            _interestRateConfigureService = interestRateConfigureService;
            _createConfigService = createConfigService;
            _productService = productService; // ← THÊM DÒNG GÁN NÀY
            _mapper = mapper;
        }



        public IActionResult Index()
        {
            var controllerFromRoute = RouteData.Values["controller"]?.ToString();
            var actionFromRoute = RouteData.Values["action"]?.ToString();
            SetPermitData(actionFromRoute, controllerFromRoute);

            RolePermissionModel userPermission = UserPermission;
            string role = UserRole.ToString();

            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);

            return View();
        }

        //add

        //adđ lại từ Db View


        //   public async Task<ActionResult> LoadInterestRateGridData([DataSourceRequest] DataSourceRequest request, string pProductGroupCode,
        //string pPosCode, string pProductCode, string pCircularRefNum, string pFromEffectiveDate, string pToEffectiveDate, string searchText, string pStatusDesc =null )
        //   {
        //       try
        //       {
        //           if (string.IsNullOrEmpty(pPosCode))
        //               pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
        //           if (string.IsNullOrEmpty(pProductGroupCode))
        //               pProductGroupCode = ProductGroupCode.CASA.Code;
        //           if (string.IsNullOrEmpty(pCircularRefNum))
        //               pCircularRefNum = "";
        //           // Parsing date an toàn
        //           DateTime? fromDate = null;
        //           DateTime? toDate = null;
        //           if (!string.IsNullOrEmpty(pFromEffectiveDate) && DateTime.TryParseExact(pFromEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        //           {
        //               fromDate = parsedFromDate.Date;
        //           }
        //           if (!string.IsNullOrEmpty(pToEffectiveDate) && DateTime.TryParseExact(pToEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
        //           {
        //               toDate = parsedToDate.Date.AddDays(1);
        //           }
        //           // Log input
        //           WriteLog(LogType.INFOR, $"LoadInterestRateGridData Params - pProductGroupCode: '{pProductGroupCode ?? "NULL"}', pPosCode: '{pPosCode ?? "NULL"}', pProductCode: '{pProductCode ?? "NULL"}', pCircularRefNum: '{pCircularRefNum ?? "NULL"}', fromDate: {fromDate}, toDate: {toDate}");
        //           // Gọi service
        //           var list = await _interestRateConfigureService.GetInterestRateConfigMasterViewListAsync(pProductGroupCode, pPosCode, pProductCode ?? "", pCircularRefNum, fromDate, toDate, searchText, pStatusDesc);
        //           // Log output
        //           var firstProductListFromService = list?.FirstOrDefault()?.ProductList ?? "NULL_FROM_SERVICE";
        //           var firstIdFromService = list?.FirstOrDefault()?.Id ?? 0;
        //           WriteLog(LogType.INFOR, $"Service returned {list?.Count ?? 0} records. First entity ProductList: '{firstProductListFromService}', First Id from View: {firstIdFromService}");
        //           if (list == null || !list.Any())
        //           {
        //               WriteLog(LogType.ERROR, "No data from service - check query filters or DB View data for CASA");
        //           }
        //           // Map
        //           var viewModels = _mapper.Map<List<InterestRateConfigMasterModel>>(list ?? new List<InterestRateConfigMasterView>());


        //           foreach (var vm in viewModels)
        //           {
        //               vm.IsSelected = false;

        //               if (string.IsNullOrEmpty(vm.ProductList))
        //               {
        //                   vm.ProductList = firstProductListFromService == "NULL_FROM_SERVICE" ? "No data" : "N/A";
        //               }


        //           }
        //           // FIX: Explicit set Id nếu =0
        //           int fixedCount = 0; // FIX: Thêm biến count
        //           for (int i = 0; i < viewModels.Count && i < list.Count; i++)
        //           {
        //               if (viewModels[i].Id == 0)
        //               {
        //                   var sourceId = list[i].Id; // FIX: Log source Id để check
        //                   viewModels[i].Id = sourceId > 0 ? sourceId : (list[i].DocumentId > 0 ? list[i].DocumentId : viewModels[i].Id);
        //                   fixedCount++;
        //                   WriteLog(LogType.INFOR, $"Fixed Id[{i}] = {viewModels[i].Id} (source Id={sourceId}, DocId={list[i].DocumentId}) for ProductCode = {viewModels[i].ProductCode}");
        //               }
        //           }
        //           WriteLog(LogType.INFOR, $"Fixed {fixedCount}/{viewModels.Count} Ids"); // FIX: Uncomment log
        //           WriteLog(LogType.INFOR, $"LoadInterestRateGridData - Returning {viewModels.Count} records");
        //           var firstProductListAfterMap = viewModels.FirstOrDefault()?.ProductList ?? "NULL_AFTER_MAP";
        //           var firstIdAfterFix = viewModels.FirstOrDefault()?.Id ?? 0;
        //           WriteLog(LogType.INFOR, $"After mapping/fix: First model ProductList: '{firstProductListAfterMap}', First Id: {firstIdAfterFix}");
        //           return Json(viewModels.ToDataSourceResult(request, ModelState));
        //       }
        //       catch (Exception ex)
        //       {
        //           WriteLog(LogType.INFOR, $"LoadInterestRateGridData(...) => Error: {ex.Message}");
        //           WriteLog(LogType.ERROR, $"LoadInterestRateGridData Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "None"} | Stack: {ex.StackTrace}");
        //           return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách cấu hình lãi suất CASA." });
        //       }
        //   }

        //    public async Task<ActionResult> LoadInterestRateGridData(
        //[DataSourceRequest] DataSourceRequest request,
        //string pProductGroupCode,
        //string pPosCode,
        //string pCircularRefNum,
        //string pFromEffectiveDate,
        //string pToEffectiveDate,
        //string searchText = null,
        //string status = null)  // 
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(pPosCode))
        //                pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;

        //            if (string.IsNullOrEmpty(pProductGroupCode))
        //                pProductGroupCode = ProductGroupCode.CASA.Code;

        //            if (string.IsNullOrEmpty(pCircularRefNum))
        //                pCircularRefNum = "";

        //            // Parsing date an toàn (giữ nguyên của bạn, tốt hơn sếp)
        //            DateTime? fromDate = null;
        //            DateTime? toDate = null;
        //            if (!string.IsNullOrEmpty(pFromEffectiveDate) && DateTime.TryParseExact(pFromEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
        //            {
        //                fromDate = parsedFrom.Date;
        //            }
        //            if (!string.IsNullOrEmpty(pToEffectiveDate) && DateTime.TryParseExact(pToEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
        //            {
        //                toDate = parsedTo.Date.AddDays(1);
        //            }

        //            // Parse status thành int (giống sếp)
        //            int? statusValue = null;
        //            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int parsedStatus))
        //            {
        //                statusValue = parsedStatus;
        //            }

        //            // Log params (giữ chi tiết của bạn)
        //            WriteLog(LogType.INFOR, $"LoadInterestRateGridData Params - pProductGroupCode: '{pProductGroupCode ?? "NULL"}', pPosCode: '{pPosCode ?? "NULL"}', pCircularRefNum: '{pCircularRefNum ?? "NULL"}', searchText: '{searchText ?? "NULL"}', status: '{status ?? "NULL"}', fromDate: {fromDate}, toDate: {toDate}");

        //            // Gọi service (đã có client-side filter)
        //            var list = await _interestRateConfigureService.GetInterestRateConfigMasterViewListAsync(
        //                pProductGroupCode,
        //                pPosCode,
        //                "",  // pProductCode nếu không dùng thì truyền rỗng
        //                pCircularRefNum,
        //                fromDate,
        //                toDate,
        //                searchText,
        //                //status != null ? int.Parse(status) : -1
        //                statusValue.HasValue ? statusValue.Value.ToString() : null);  // Truyền string để service xử lý

        //            // Log output (giữ của bạn)
        //            var firstProductList = list?.FirstOrDefault()?.ProductList ?? "NULL";
        //            var firstId = list?.FirstOrDefault()?.Id ?? 0;
        //            WriteLog(LogType.INFOR, $"Service returned {list?.Count ?? 0} records. First ProductList: '{firstProductList}', First Id: {firstId}");

        //            if (list == null || !list.Any())
        //            {
        //                WriteLog(LogType.ERROR, "No data from service - check filters or DB View");
        //            }

        //            // Map & fix Id (giữ nguyên logic tốt của bạn)
        //            var viewModels = _mapper.Map<List<InterestRateConfigMasterModel>>(list ?? new List<InterestRateConfigMasterView>());

        //            foreach (var vm in viewModels)
        //            {
        //                vm.IsSelected = false;
        //                if (string.IsNullOrEmpty(vm.ProductList))
        //                    vm.ProductList = "N/A";
        //            }

        //            // Fix Id
        //            int fixedCount = 0;
        //            for (int i = 0; i < viewModels.Count && i < (list?.Count ?? 0); i++)
        //            {
        //                if (viewModels[i].Id == 0)
        //                {
        //                    var sourceId = list[i].Id;
        //                    viewModels[i].Id = sourceId > 0 ? sourceId : (list[i].DocumentId > 0 ? list[i].DocumentId : viewModels[i].Id);
        //                    fixedCount++;
        //                }
        //            }

        //            return Json(viewModels.ToDataSourceResult(request, ModelState));
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(LogType.ERROR, $"LoadInterestRateGridData Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "None"}");
        //            return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách cấu hình lãi suất CASA." });
        //        }
        //    }

        public async Task<ActionResult> LoadInterestRateGridData(
    [DataSourceRequest] DataSourceRequest request,
    string pProductGroupCode,
    string pPosCode,
    string pCircularRefNum,
    string pFromEffectiveDate,
    string pToEffectiveDate,
    string searchText = null,
    string status = null)  // 
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;

                if (string.IsNullOrEmpty(pProductGroupCode))
                    pProductGroupCode = ProductGroupCode.CASA.Code;

                if (string.IsNullOrEmpty(pCircularRefNum))
                    pCircularRefNum = "";


                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(pFromEffectiveDate) && DateTime.TryParseExact(pFromEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
                {
                    fromDate = parsedFrom.Date;
                }
                if (!string.IsNullOrEmpty(pToEffectiveDate) && DateTime.TryParseExact(pToEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
                {
                    toDate = parsedTo.Date.AddDays(1);
                }


                int? statusValue = null;
                if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int parsedStatus))
                {
                    statusValue = parsedStatus;
                }

                WriteLog(LogType.INFOR, $"LoadInterestRateGridData Params - pProductGroupCode: '{pProductGroupCode ?? "NULL"}', pPosCode: '{pPosCode ?? "NULL"}', pCircularRefNum: '{pCircularRefNum ?? "NULL"}', searchText: '{searchText ?? "NULL"}', status: '{status ?? "NULL"}' (parsed: {statusValue?.ToString() ?? "NULL"}), fromDate: {fromDate}, toDate: {toDate}");

                // Gọi service
                var list = await _interestRateConfigureService.GetInterestRateConfigMasterViewListAsync(
                    pProductGroupCode,
                    pPosCode,
                    "",  // pProductCode nếu không dùng thì để rỗng
                    pCircularRefNum,
                    fromDate,
                    toDate,
                    searchText,
                    statusValue
                );

                // Log output
                var firstProductList = list?.FirstOrDefault()?.ProductList ?? "NULL";
                var firstId = list?.FirstOrDefault()?.Id ?? 0;
                WriteLog(LogType.INFOR, $"Service returned {list?.Count ?? 0} records. First ProductList: '{firstProductList}', First Id: {firstId}");

                if (list == null || !list.Any())
                {
                    WriteLog(LogType.ERROR, "No data from service - check filters or DB View");
                }

                
                var viewModels = _mapper.Map<List<InterestRateConfigMasterModel>>(list ?? new List<InterestRateConfigMasterView>());

                foreach (var vm in viewModels)
                {
                    vm.IsSelected = false;
                    if (string.IsNullOrEmpty(vm.ProductList))
                        vm.ProductList = "N/A";
                }

               
                int fixedCount = 0;
                for (int i = 0; i < viewModels.Count && i < (list?.Count ?? 0); i++)
                {
                    if (viewModels[i].Id == 0)
                    {
                        var sourceId = list[i].Id;
                        viewModels[i].Id = sourceId > 0 ? sourceId : (list[i].DocumentId > 0 ? list[i].DocumentId : viewModels[i].Id);
                        fixedCount++;
                    }
                }

                return Json(viewModels.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"LoadInterestRateGridData Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "None"}");
                return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách cấu hình lãi suất CASA." });
            }
        }

        [HttpGet]
        public JsonResult GetConfigStatusOptions()
        {
            var statuses = new List<object>
    {
        new { Value = -1, Description = "Tất cả ", Code = "ALL" },
        ConfigStatus.MAKER,      // Value = 1
        ConfigStatus.CLOSED,     // Value = 0
        ConfigStatus.PROCESS,    // Value = 2
        ConfigStatus.AUTHORIZED, // Value = 3
        ConfigStatus.REJECTED,   // Value = 4
        ConfigStatus.MODIFIED    // Value = 5
    };

            return Json(statuses);
        }
        //    public async Task<ActionResult> LoadInterestRateGridData([DataSourceRequest] DataSourceRequest request, string pProductGroupCode,
        //string pPosCode, string pProductCode, string pCircularRefNum, string pFromEffectiveDate, string pToEffectiveDate)
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(pPosCode))
        //                pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
        //            if (string.IsNullOrEmpty(pProductGroupCode))
        //                pProductGroupCode = ProductGroupCode.CASA.Code;
        //            if (string.IsNullOrEmpty(pCircularRefNum))
        //                pCircularRefNum = "";

        //            DateTime? fromDate = null;
        //            DateTime? toDate = null;
        //            if (!string.IsNullOrEmpty(pFromEffectiveDate) && DateTime.TryParseExact(pFromEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        //            {
        //                fromDate = parsedFromDate.Date;
        //            }
        //            if (!string.IsNullOrEmpty(pToEffectiveDate) && DateTime.TryParseExact(pToEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
        //            {
        //                toDate = parsedToDate.Date.AddDays(1);
        //            }

        //            WriteLog(LogType.INFOR, $"LoadInterestRateGridData Params - pProductGroupCode: '{pProductGroupCode}', pPosCode: '{pPosCode}', pProductCode: '{pProductCode}', pCircularRefNum: '{pCircularRefNum}', fromDate: {fromDate}, toDate: {toDate}");

        //            var list = await _interestRateConfigureService.GetInterestRateConfigMasterViewListAsync(pProductGroupCode, pPosCode, pProductCode ?? "", pCircularRefNum, fromDate, toDate);

        //            WriteLog(LogType.INFOR, $"Service returned {list?.Count ?? 0} records.");

        //            if (list == null || !list.Any())
        //            {
        //                WriteLog(LogType.ERROR, "No data from service - check query filters or DB View data for CASA");
        //            }

        //            var viewModels = _mapper.Map<List<InterestRateConfigMasterModel>>(list ?? new List<InterestRateConfigMasterView>());

        //            foreach (var vm in viewModels)
        //            {
        //                vm.IsSelected = false;
        //                if (string.IsNullOrEmpty(vm.ProductList))
        //                {
        //                    vm.ProductList = "N/A";
        //                }
        //            }

        //            // Fix Id = 0
        //            int fixedCount = 0;
        //            for (int i = 0; i < viewModels.Count && i < list.Count; i++)
        //            {
        //                if (viewModels[i].Id == 0)
        //                {
        //                    var sourceId = list[i].Id;
        //                    viewModels[i].Id = sourceId > 0 ? sourceId : (list[i].DocumentId > 0 ? list[i].DocumentId : viewModels[i].Id);
        //                    fixedCount++;
        //                }
        //            }
        //            WriteLog(LogType.INFOR, $"Fixed {fixedCount}/{viewModels.Count} Ids");

        //            return Json(viewModels.ToDataSourceResult(request, ModelState));
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(LogType.ERROR, $"LoadInterestRateGridData Error: {ex.Message} | Stack: {ex.StackTrace}");
        //            return Json(new DataSourceResult { Errors = "Có lỗi xảy ra khi lấy danh sách cấu hình lãi suất CASA." });
        //        }
        //    }


        [HttpGet]
        public async Task<string> GetApplyPosListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return "Không có POS áp dụng";

            var lstIds = ids.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Where(s => long.TryParse(s, out _))
                .Select(long.Parse)
                .ToList();

            if (!lstIds.Any()) return "Không có POS áp dụng";

            return await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        }





        //    public async Task<ActionResult> LoadInterestRateGridData([DataSourceRequest] DataSourceRequest request, string pProductGroupCode,
        //string pPosCode, string pProductCode, string pCircularRefNum, string pFromEffectiveDate, string pToEffectiveDate)
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(pPosCode))
        //                pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
        //            if (string.IsNullOrEmpty(pProductGroupCode))
        //                pProductGroupCode = ProductGroupCode.CASA.Code;
        //            if (string.IsNullOrEmpty(pCircularRefNum))
        //                pCircularRefNum = "";

        //            // Parsing date an toàn
        //            DateTime? fromDate = null;
        //            DateTime? toDate = null;
        //            if (!string.IsNullOrEmpty(pFromEffectiveDate) && DateTime.TryParseExact(pFromEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        //            {
        //                fromDate = parsedFromDate.Date;
        //            }
        //            if (!string.IsNullOrEmpty(pToEffectiveDate) && DateTime.TryParseExact(pToEffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
        //            {
        //                toDate = parsedToDate.Date.AddDays(1);
        //            }

        //            // Log input
        //            WriteLog(LogType.INFOR, $"LoadInterestRateGridData Params - pProductGroupCode: '{pProductGroupCode ?? "NULL"}', pPosCode: '{pPosCode ?? "NULL"}', pProductCode: '{pProductCode ?? "NULL"}', pCircularRefNum: '{pCircularRefNum ?? "NULL"}', fromDate: {fromDate}, toDate: {toDate}");

        //            // Gọi service
        //            var list = await _interestRateConfigureService.GetInterestRateConfigMasterViewListAsync(pProductGroupCode, pPosCode, pProductCode ?? "", pCircularRefNum, fromDate, toDate);

        //            // Log output
        //            var firstProductListFromService = list?.FirstOrDefault()?.ProductList ?? "NULL_FROM_SERVICE";
        //            var firstIdFromService = list?.FirstOrDefault()?.Id ?? 0;
        //            WriteLog(LogType.INFOR, $"Service returned {list?.Count ?? 0} records. First entity ProductList: '{firstProductListFromService}', First Id from View: {firstIdFromService}");

        //            if (list == null || !list.Any())
        //            {
        //                WriteLog(LogType.ERROR, "No data from service - check query filters or DB View data for CASA");
        //            }

        //            // Map
        //            var viewModels = _mapper.Map<List<InterestRateConfigMasterModel>>(list ?? new List<InterestRateConfigMasterView>());

        //            foreach (var vm in viewModels)
        //            {
        //                vm.IsSelected = false;
        //                if (string.IsNullOrEmpty(vm.ProductList))
        //                {
        //                    vm.ProductList = firstProductListFromService == "NULL_FROM_SERVICE" ? "No data" : "N/A";
        //                }

        //                // === THÊM MỚI: XỬ LÝ DANH SÁCH POS ÁP DỤNG CHO GRID INDEX ===
        //                List<long> lstIds = new List<long>();

        //                // Ưu tiên dùng IdList (vì một tờ trình có thể gồm nhiều master record)
        //                if (!string.IsNullOrEmpty(vm.IdList))
        //                {
        //                    lstIds = vm.IdList
        //                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        //                        .Where(s => long.TryParse(s, out _))
        //                        .Select(long.Parse)
        //                        .ToList();
        //                }

        //                // Fallback: nếu IdList rỗng thì dùng Id hiện tại
        //                if (!lstIds.Any() && vm.Id > 0)
        //                {
        //                    lstIds.Add(vm.Id);
        //                }

        //                // Lấy tên POS áp dụng
        //                if (lstIds.Any())
        //                {
        //                    vm.ApplyPosList = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        //                }
        //                else
        //                {
        //                    vm.ApplyPosList = "Không có POS áp dụng";
        //                }
        //                // ===================================================
        //            }

        //            // FIX: Explicit set Id nếu =0 (giữ nguyên logic cũ của bạn)
        //            int fixedCount = 0;
        //            for (int i = 0; i < viewModels.Count && i < list.Count; i++)
        //            {
        //                if (viewModels[i].Id == 0)
        //                {
        //                    var sourceId = list[i].Id;
        //                    viewModels[i].Id = sourceId > 0 ? sourceId : (list[i].DocumentId > 0 ? list[i].DocumentId : viewModels[i].Id);
        //                    fixedCount++;
        //                    WriteLog(LogType.INFOR, $"Fixed Id[{i}] = {viewModels[i].Id} (source Id={sourceId}, DocId={list[i].DocumentId}) for ProductCode = {viewModels[i].ProductCode}");
        //                }
        //            }
        //            WriteLog(LogType.INFOR, $"Fixed {fixedCount}/{viewModels.Count} Ids");

        //            WriteLog(LogType.INFOR, $"LoadInterestRateGridData - Returning {viewModels.Count} records");

        //            return Json(viewModels.ToDataSourceResult(request, ModelState));
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(LogType.INFOR, $"LoadInterestRateGridData(...) => Error: {ex.Message}");
        //            WriteLog(LogType.ERROR, $"LoadInterestRateGridData Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "None"} | Stack: {ex.StackTrace}");

        //            // === SỬA Ở ĐÂY ===
        //            return Json(new DataSourceResult
        //            {
        //                Errors = "Có lỗi xảy ra khi lấy danh sách cấu hình lãi suất CASA. Vui lòng thử lại hoặc liên hệ IT."
        //            });
        //        }
        //    }





        [HttpGet]
        public IActionResult ShowCreateConfig(int pId, string pFlagCall)
        {
            var model = new AddCasaProductViewModel
            {
                ProductList = _createConfigService.GetProductList(ProductGroupCode.CASA.Code)
                    .Select(p => new Product
                    {
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        AccountTypeCode = p.AccountTypeCode,
                        AccountTypeName = p.AccountTypeName,
                        AccountSubTypeCode = p.AccountSubTypeCode,
                        CurrencyCode = p.CurrencyCode,
                        DebitCreditFlag = p.DebitCreditFlag,
                        EffectiveDate = p.EffectiveDate,
                        InterestRate = p.InterestRate,
                        NewInterestRate = p.NewInterestRate ?? 0
                    }).ToList(),
                AccountTypes = _createConfigService.GetAccountTypes(""),
                AccountSubTypes = _createConfigService.GetAccountSubTypes(""),
                DebitCreditFlag = "C",
                NewInterestRate = 0
            };
            return PartialView("_Create", model);
        }



        ////add


        // THÊM: Action mới cho load grid VIEW DETAIL 
        [HttpPost]
        public async Task<IActionResult> LoadCasaConfigureViewGridData([DataSourceRequest] DataSourceRequest request)
        {
            var lstId = Request.Form["idList"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (lstId == null || lstId.Count == 0)
            {
                return Json(new DataSourceResult { Data = new List<CasaRateProductViewModel>(), Total = 0 });
            }
            try
            {
                var models = await _interestRateConfigureService.GetCasaTermsAsync(lstId);
                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<CasaRateProductViewModel>(), Total = 0 });
            }
        }

        // SỬA: LoadCasaConfigureAddGridData (giữ cho "add", simplify "detail" - không fallback, chỉ dùng idList nếu có)
        //[HttpPost]
        //public async Task<IActionResult> LoadCasaConfigureAddGridData([DataSourceRequest] DataSourceRequest request)
        //{
        //    try
        //    {
        //        var formData = Request.Form;
        //        // Parse idListStr an toàn
        //        var idListStr = formData["idList"].ToString(); // Trả về "" nếu empty
        //        var lstId = idListStr.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        //        var productCode = (formData["productCode"].ToString() ?? "").Trim();
        //        var accountTypeCode = (formData["accountType"].ToString() ?? "").Trim();
        //        var accountSubTypeCode = (formData["accountSubType"].ToString() ?? "").Trim();
        //        var effectiveDate = formData["fromEffectiveDate"].ToString() ?? "";
        //        var posCode = (formData["posCode"].ToString() ?? "0").Trim();
        //        var circularRefNum = (formData["circularRefNum"].ToString() ?? "").Trim();
        //        var newInterestRateStr = formData["NewInterestRate"].ToString() ?? "0";
        //        var mode = (formData["mode"].ToString() ?? "detail").Trim();
        //        var masterIdStr = formData["masterId"].ToString() ?? "0";
        //        var documentIdStr = formData["documentId"].ToString() ?? "0";
        //        long masterId = long.TryParse(masterIdStr, out long outMasterVar) ? outMasterVar : 0;
        //        long documentId = long.TryParse(documentIdStr, out long outDocVar) ? outDocVar : 0;

        //        WriteLog(LogType.INFOR, $"Params: idList='{idListStr}' ({lstId.Count} IDs), productCode='{productCode}', circularRefNum='{circularRefNum}', masterId={masterId}, documentId={documentId}, mode={mode}");
        //        decimal? newInterestRate = string.IsNullOrEmpty(newInterestRateStr) ? 0m : Convert.ToDecimal(newInterestRateStr, CultureInfo.InvariantCulture);
        //        DateTime? fromDate = null;
        //        if (!string.IsNullOrEmpty(effectiveDate) && DateTime.TryParseExact(effectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        //        {
        //            fromDate = parsedFromDate;
        //        }
        //        else
        //        {
        //            fromDate = DateTime.Today;
        //        }
        //        List<CasaRateProductViewModel> models = new();
        //        List<InterestRateConfigMasterModel> dbList = new();

        //        if (mode == "detail")
        //        {

        //            if (lstId.Any())
        //            {
        //                WriteLog(LogType.INFOR, $"Using idList filter for detail mode: {lstId.Count} IDs (e.g. {string.Join(",", lstId.Take(3))})");
        //                models = await _interestRateConfigureService.GetCasaTermsAsync(lstId); // Gọi service mới: Chỉ lấy matching IDs
        //                                                                                       // Override NewInterestRate nếu pass (cho edit/view)
        //                foreach (var modelItem in models)
        //                    modelItem.RateProductNewInterestRate = newInterestRate ?? modelItem.RateProductNewInterestRate ?? 0m;

        //                WriteLog(LogType.INFOR, $"idList mode returned {models.Count} rows");
        //            }
        //            else
        //            {
        //                // Không có idList → empty (không fallback, tránh load nhầm data)
        //                WriteLog(LogType.ERROR, "No idList for detail mode, returning empty");
        //            }
        //            return Json(models.ToDataSourceResult(request, ModelState)); // Return sớm cho detail
        //        }
        //        else
        //        {
        //            // Mode add: Giữ nguyên (load từ API)
        //            if (posCode == PosValue.HEAD_POS)
        //                posCode = "0";
        //            models = await _interestRateConfigureService.GetCasaProdList(posCode, productCode, DateTime.Now);
        //            foreach (var model in models)
        //                model.RateProductNewInterestRate = newInterestRate ?? model.RateProductNewInterestRate ?? 0m;
        //        }
        //        // SỬA MỚI: Return trực tiếp ở cuối (bỏ var result hoàn toàn)
        //        WriteLog(LogType.INFOR, $"Returning {models.Count} rows");
        //        return Json(models.ToDataSourceResult(request, ModelState));
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
        //        return Json(new DataSourceResult { Data = new List<CasaRateProductViewModel>(), Total = 0 });
        //    }
        //}


        // đóng tạm LoadCasaConfigureAddGridData
        [HttpPost]
        public async Task<IActionResult> LoadCasaConfigureAddGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var formData = Request.Form;

                // Parse các tham số batch (mới)
                var productCodesStr = formData["productCodes"].ToString();
                var accountTypesStr = formData["accountTypes"].ToString();
                var posCode = (formData["posCode"].ToString() ?? "0").Trim();
                var effectiveDateStr = formData["effectiveDate"].ToString();
                var newInterestRateStr = formData["newInterestRate"].ToString() ?? "0";

                // Parse tham số cũ (cho mode detail hoặc fallback)
                var mode = (formData["mode"].ToString() ?? "add").Trim().ToLower();
                var idListStr = formData["idList"].ToString();
                var lstId = string.IsNullOrEmpty(idListStr)
                    ? new List<string>()
                    : idListStr.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

                WriteLog(LogType.INFOR, $"LoadCasaConfigureAddGridData - mode: {mode}, productCodes: '{productCodesStr}', accountTypes: '{accountTypesStr}', posCode: '{posCode}'");

                List<CasaRateProductViewModel> models = new();

                // MODE DETAIL - giữ nguyên
                if (mode == "detail")
                {
                    if (lstId.Any())
                    {
                        models = await _interestRateConfigureService.GetCasaTermsAsync(lstId);
                        if (decimal.TryParse(newInterestRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal newRate))
                        {
                            foreach (var item in models)
                                item.RateProductNewInterestRate = newRate;
                        }
                    }
                    return Json(models.ToDataSourceResult(request, ModelState));
                }

                // MODE ADD / BATCH
                if (posCode == PosValue.HEAD_POS)
                    posCode = "0";

                DateTime referenceDate = DateTime.Now;
                if (!string.IsNullOrEmpty(effectiveDateStr) && DateTime.TryParseExact(effectiveDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    referenceDate = parsed;
                }

                // Parse batch parameters
                var productCodes = string.IsNullOrEmpty(productCodesStr)
                    ? new List<string>()
                    : productCodesStr.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();

                var accountTypes = string.IsNullOrEmpty(accountTypesStr)
                    ? new List<string>()
                    : accountTypesStr.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();

                // Áp dụng lãi suất mới chung
                decimal? commonNewRate = decimal.TryParse(newInterestRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) ? rate : (decimal?)null;

                // 
                if (productCodes.Any() && accountTypes.Any())
                {
                    WriteLog(LogType.INFOR, $"Trying batch load with {productCodes.Count} products and {accountTypes.Count} account types");

                    models = await _interestRateConfigureService.GetCasaRateByProductsAndTypesAsyncWithHO(productCodes, accountTypes, posCode, referenceDate);

                    if (models.Any())
                    {
                        WriteLog(LogType.INFOR, $"Batch load success - returned {models.Count} rows");
                    }
                    else
                    {
                        WriteLog(LogType.ERROR, "Batch load returned empty → fallback to GetCasaProdList");
                    }
                }


                if (!models.Any())
                {
                    WriteLog(LogType.INFOR, $"Batch load empty - fallback to load multiple account types");
                    var fallbackModels = new List<CasaRateProductViewModel>();
                    foreach (var accType in accountTypes)
                    {
                        var temp = await _interestRateConfigureService.GetCasaProdList(posCode, "0", referenceDate); // "0" load tất cả product
                        fallbackModels.AddRange(temp.Where(x => x.RateProductAccountTypeCode == accType));
                    }
                    models = fallbackModels.DistinctBy(x => new { x.RateProductCode, x.RateProductAccountTypeCode }).ToList();
                }

                //if (!models.Any())
                //{
                //    var fallbackProductCode = productCodes.FirstOrDefault() ?? "0"; // Nếu có chọn 1 cái thì dùng, không thì "0" để load tất cả

                //    WriteLog(LogType.INFOR, $"Fallback to GetCasaProdList with posCode='{posCode}', productCode='{fallbackProductCode}'");

                //    models = await _interestRateConfigureService.GetCasaProdList(posCode, fallbackProductCode, referenceDate);
                //}

                // Áp dụng lãi suất mới chung (nếu có)
                if (commonNewRate.HasValue)
                {
                    foreach (var item in models)
                    {
                        item.RateProductNewInterestRate = commonNewRate.Value;
                    }
                }

                WriteLog(LogType.INFOR, $"Final return {models.Count} rows");

                return Json(models.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"LoadCasaConfigureAddGridData Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new DataSourceResult { Data = new List<CasaRateProductViewModel>(), Total = 0 });
            }
        }






        //Sửa thêm Grid Mã sản phẩm
        //[HttpPost]
        //public ActionResult LoadCasaProductGridData([DataSourceRequest] DataSourceRequest request)
        //{
        //    try
        //    {
        //        var customerType = Request.Form["customerType"].ToString();

        //        // Bỏ await, vì method đồng bộ
        //        var data = _createConfigService.GetProductList(ProductGroupCode.CASA.Code);

        //        // Nếu cần lọc theo customerType (tùy model của bạn)
        //        // if (!string.IsNullOrEmpty(customerType))
        //        // {
        //        //     data = data.Where(p => p.CustomerType == customerType).ToList();
        //        // }

        //        var result = data.ToDataSourceResult(request);

        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"LoadCasaProductGridData Error: {ex.Message} | Stack: {ex.StackTrace}");
        //        return Json(new DataSourceResult
        //        {
        //            Data = new List<object>(),
        //            Total = 0
        //        });
        //    }
        //}
        // sửa phần hiển thị mã loại tk
        [HttpPost]
        public IActionResult LoadCasaProductGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                // CASA không cần depositType và customerType → để rỗng hoặc null
                var depositType = ""; // hoặc string.Empty
                var customerType = ""; // hoặc string.Empty

                // 
                var models = _productService.GetFullProductList(ProductGroupCode.CASA.Code, depositType, customerType, UserPosCode, UserGrade);

                // Log 
                if (models.Any())
                {
                    var first = models.First();
                    WriteLog(LogType.INFOR, $"LoadCasaProductGridData - First item: ProductCode={first.ProductCode}, AccountTypeCode={first.AccountTypeCode}, AccountTypeName={first.AccountTypeName}");
                }

                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"LoadCasaProductGridData Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new DataSourceResult { Data = new List<object>(), Total = 0 });
            }
        }

        //SỬA: ShowCasaConfigDetail(thêm log, đảm bảo IdList không empty)
        [HttpGet]
        public async Task<IActionResult> ShowCasaConfigDetail(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
        {
            if (string.IsNullOrEmpty(circularRefNum))
            {
                WriteLog(LogType.ERROR, "ShowCasaConfigDetail: Missing circularRefNum");
                return NotFound("Thiếu thông tin quyết định");
            }
            var model = await _interestRateConfigureService.GetCasaInterestRateDetailViews(circularRefNum,effectDate);
            // Sau khi có lstIds (danh sách Id của các master record)

            if (model == null)
            {
                WriteLog(LogType.ERROR, $"ShowCasaConfigDetail: No data for circularRefNum={circularRefNum}");
                return NotFound("Không tìm thấy dữ liệu chi tiết");
            }
            // THÊM: Log IdList để debug
            WriteLog(LogType.INFOR, $"ShowCasaConfigDetail: Loaded model with IdList='{model.IdList}', Count expected ~{model.IdList?.Split(';').Length ?? 0}");
            ViewBag.IdList = idList ?? model.IdList; // Fallback từ model nếu ViewBag empty
            ViewBag.FlagCall = pFlagCall;
            return PartialView("_Detail", model);
        }


        //[HttpGet]
        //public async Task<IActionResult> ShowCasaConfigDetail(int pId, string circularRefNum, string idList, string pFlagCall)
        //{
        //    if (string.IsNullOrEmpty(circularRefNum))
        //    {
        //        WriteLog(LogType.ERROR, "ShowCasaConfigDetail: Missing circularRefNum");
        //        return NotFound("Thiếu thông tin quyết định");
        //    }

        //    var model = await _interestRateConfigureService.GetCasaInterestRateDetailViews(circularRefNum);
        //    if (model == null)
        //    {
        //        WriteLog(LogType.ERROR, $"ShowCasaConfigDetail: No data for circularRefNum={circularRefNum}");
        //        return NotFound("Không tìm thấy dữ liệu chi tiết");
        //    }

        //    // === THÊM: Xử lý IdList và lấy danh sách POS áp dụng ===
        //    string finalIdList = idList ?? model.IdList ?? "";
        //    List<long> lstIds = new List<long>();

        //    if (!string.IsNullOrEmpty(finalIdList))
        //    {
        //        lstIds = finalIdList
        //            .Split(';', StringSplitOptions.RemoveEmptyEntries)
        //            .Where(s => long.TryParse(s, out _))
        //            .Select(long.Parse)
        //            .ToList();
        //    }


        //    string applyPosListDisplay = "";
        //    if (lstIds.Any())
        //    {
        //        applyPosListDisplay = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        //    }

        //    // Gán vào model để View hiển thị
        //    //   model.ApplyPosList = applyPosListDisplay;

        //    // Log để debug
        //    WriteLog(LogType.INFOR, $"ShowCasaConfigDetail: Loaded model with IdList='{finalIdList}', POS áp dụng: '{applyPosListDisplay}'");

        //    // Truyền thêm cho grid (nếu cần)
        //    ViewBag.IdList = finalIdList;
        //    ViewBag.FlagCall = pFlagCall;

        //    return PartialView("_Detail", model);
        //}








        //Đóng tạm



        [HttpPost]
        [Route("SaveCasaRateConfigure")]
        public async Task<IActionResult> SaveCasaRateConfigure()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            Console.WriteLine($"Received body: {body}");

            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Request body is empty.");
            }

            try
            {
                var request = System.Text.Json.JsonSerializer.Deserialize<SaveCasaRateConfigureRequest>(body);
                Console.WriteLine($"Deserialized request: {JsonConvert.SerializeObject(request)}");
                if (request == null)
                {
                    return BadRequest("Failed to deserialize request.");
                }

                // 
                var message = await _interestRateConfigureService.SaveCasaRateConfigureData(request.Model, request.GridItems, UserName);
                Console.WriteLine($"Save result: {message}");
                return Ok(new { Success = true, Message = $"{message}" });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}, Inner: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Lỗi khi lưu vào cơ sở dữ liệu: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        // Sửa lại lưu DB Create casa




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseCasa([FromBody] DeleteInterestRateConfigRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new { success = false, message = "Yêu cầu không hợp lệ. Vui lòng cung cấp Id hợp lệ." });
            }

            try
            {
                string result;
                if (request.DocumentId != null && request.DocumentId.Value != 0)
                {
                    long documentId = request.DocumentId.Value;
                    result = await _interestRateConfigureService.CloseCasaByDocumentId(documentId, UserName);
                }
                else
                {
                    result = await _interestRateConfigureService.CloseCasaById(request.Id, UserName);
                }

                if (result.StartsWith("Lỗi"))
                {
                    return StatusCode(500, new { success = false, message = result });
                }

                return Ok(new { success = true, message = "Đóng Casa thành công." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi đóng Casa: {ex.Message}" });
            }
        }

        public async Task<IActionResult> ShowUpdateCasaConfig(long pId, string pFlagCall)
        {
            var casa = await _interestRateConfigureService.GetCasaByIdAsync(pId);
            if (casa == null)
            {
                return NotFound("Không tìm thấy cấu hình lãi suất.");
            }

            var model = new AddCasaProductViewModel
            {
                Id = casa.Id,
                ProductCode = casa.ProductCode ?? "",
                ProductName = casa.ProductName ?? "",
                AccountTypeCode = casa.AccountTypeCode ?? "",
                AccountTypeName = casa.AccountTypeName ?? "",
                AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
                CurrencyCode = casa.CurrencyCode ?? "VND",
                DebitCreditFlag = casa.DebitCreditFlag ?? "C",
                EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
                ExpiredDate = casa.ExpiryDate ?? DateTime.Today.AddYears(1),
                CircularRefNum = casa.CircularRefNum ?? "",
                CircularDate = casa.CreatedDate ?? DateTime.Today,
                PosCode = casa.PosCode ?? "",
                InterestRate = casa.InterestRate ?? 0,
                NewInterestRate = casa.NewInterestRate ?? 0,
                PenalRate = casa.PenalRate ?? 0,
                AmoutSlab = casa.AmountSlab ?? 0
            };

            return PartialView("_UpdateCasa", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCasa([FromBody] SaveCasaRateConfigureRequest request)
        {
            if (request == null || request.Model == null || request.Model.Id <= 0)
            {
                WriteLog(LogType.ERROR, "Yêu cầu không hợp lệ. Model null hoặc Id không hợp lệ.");
                return BadRequest(new { success = false, message = "Yêu cầu không hợp lệ. Vui lòng cung cấp Id hợp lệ." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Model.PosCode) || string.IsNullOrEmpty(request.Model.ProductCode))
                {
                    WriteLog(LogType.ERROR, "Thiếu các trường bắt buộc: PosCode hoặc ProductCode.");
                    return BadRequest(new { success = false, message = "Thiếu các trường bắt buộc: PosCode hoặc ProductCode." });
                }

                var existingCasa = await _interestRateConfigureService.GetCasaByIdAsync(request.Model.Id);
                if (existingCasa == null)
                {
                    WriteLog(LogType.ERROR, $"Không tìm thấy bản ghi Casa với Id={request.Model.Id}");
                    return NotFound(new { success = false, message = $"Không tìm thấy bản ghi Casa với Id={request.Model.Id}" });
                }

                var model = new InterestRateConfigMasterModel
                {
                    Id = request.Model.Id,
                    PosCode = request.Model.PosCode,
                    ProductCode = request.Model.ProductCode,
                    ProductName = request.Model.ProductName,
                    AccountTypeCode = request.Model.AccountTypeCode,
                    AccountTypeName = request.Model.AccountTypeName,
                    AccountSubTypeCode = request.Model.AccountSubTypeCode,
                    CircularRefNum = request.Model.CircularRefNum,
                    InterestRate = request.Model.InterestRate,
                    NewInterestRate = request.Model.NewInterestRate,
                    PenalRate = request.Model.PenalRate,
                    EffectiveDate = request.Model.EffectiveDate,
                    ExpiryDate = request.Model.ExpiredDate,
                    DocumentId = null,
                    Status = existingCasa.Status ?? 1
                };

                var message = await _interestRateConfigureService.UpdateCasaAsync(model, UserName);
                if (message.StartsWith("Lỗi"))
                {
                    WriteLog(LogType.ERROR, message);
                    return StatusCode(500, new { success = false, message = message });
                }

                if (request.GridItems != null && request.GridItems.Any())
                {
                    foreach (var item in request.GridItems)
                    {
                        var gridExistingCasa = await _interestRateConfigureService.GetCasaByIdAsync(item.Id);
                        var gridModel = new InterestRateConfigMasterModel
                        {
                            Id = item.Id,
                            PosCode = item.RateProductPosCode,
                            ProductCode = item.RateProductCode,
                            ProductName = item.RateProductName,
                            AccountTypeCode = item.RateProductAccountTypeCode,
                            AccountTypeName = item.RateProductAccountTypeName,
                            AccountSubTypeCode = item.RateProductAccountSubTypeCode,
                            CircularRefNum = request.Model.CircularRefNum,
                            InterestRate = item.RateProductInterestRate,
                            NewInterestRate = item.RateProductNewInterestRate,
                            PenalRate = item.RateProductPenalRate,
                            EffectiveDate = item.RateProductEffectiveDate,
                            ExpiryDate = item.RateProductExpiredDate,
                            DocumentId = null,
                            Status = gridExistingCasa?.Status ?? 1
                        };
                        await _interestRateConfigureService.UpdateCasaAsync(gridModel, UserName);
                    }
                }

                WriteLog(LogType.INFOR, $"Cập nhật Casa thành công cho Id={model.Id}");
                return Ok(new { success = true, message = "Cập nhật Casa thành công." });
            }
            catch (DbUpdateException ex)
            {
                WriteLog(LogType.ERROR, $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Lỗi khi cập nhật Casa: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật Casa: {ex.Message}" });
            }
        }




        //public async Task<IActionResult> ShowDetailCasaConfig(long id)
        //{
        //    var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
        //    if (casa == null)
        //    {
        //        return NotFound("Không tìm thấy cấu hình lãi suất.");
        //    }

        //    var model = new AddCasaProductViewModel
        //    {
        //        Id = casa.Id,
        //        ProductCode = casa.ProductCode ?? "",
        //        ProductName = casa.ProductName ?? "",
        //        AccountTypeCode = casa.AccountTypeCode ?? "",
        //        AccountTypeName = casa.AccountTypeName ?? "",
        //        AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
        //        CurrencyCode = casa.CurrencyCode ?? "VND",
        //        DebitCreditFlag = casa.DebitCreditFlag ?? "C",
        //        EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
        //        ExpiredDate = casa.ExpiryDate ?? DateTime.Today.AddYears(1),
        //        CircularRefNum = casa.CircularRefNum ?? "",
        //        CircularDate = casa.CreatedDate ?? DateTime.Today,
        //        PosCode = casa.PosCode ?? "",
        //        InterestRate = casa.InterestRate ?? 0,
        //        NewInterestRate = casa.NewInterestRate ?? 0,
        //        PenalRate = casa.PenalRate ?? 0,
        //        AmoutSlab = casa.AmountSlab ?? 0
        //    };

        //    // Không cần gán GridItems trực tiếp vì grid sẽ load từ LoadCasaConfigureAddGridData
        //    return PartialView("_Detail", model);
        //}

        //add ShowDetailCasaConfig


        public async Task<IActionResult> ShowDetailCasaConfig(string id)
        {
            try
            {
                if (!long.TryParse(id, out var longId) || longId <= 0)
                {
                    WriteLog(LogType.ERROR, $"ID không hợp lệ hoặc bằng 0: {id}");
                    return BadRequest("ID không hợp lệ (phải lớn hơn 0).");
                }
                WriteLog(LogType.INFOR, $"ShowDetailCasaConfig called with longId = {longId}"); // Log Id truyền vào
                var casa = await _interestRateConfigureService.GetCasaByIdAsync(longId);
                if (casa == null)
                {
                    WriteLog(LogType.ERROR, $"Không tìm thấy Casa với Id={longId} (ProductGroupCode='CASA')"); // Log chi tiết
                    return NotFound("Không tìm thấy cấu hình lãi suất CASA.");
                }
                // Fallback date nếu null
                if (!casa.EffectiveDate.HasValue)
                    casa.EffectiveDate = DateTime.Today; // SỬA: Today thay 08/12/2025 để match hiện tại
                WriteLog(LogType.INFOR, $"casa from DB: Id={casa.Id}, ProductCode='{casa.ProductCode}', CircularRefNum='{casa.CircularRefNum}', DocumentId={casa.DocumentId}, NewInterestRate={casa.NewInterestRate}");
                if (string.IsNullOrWhiteSpace(casa.CircularRefNum))
                {
                    WriteLog(LogType.ERROR, $"Casa Id={longId} has empty CircularRefNum, fallback to DocumentId={casa.DocumentId}");
                }
                var model = casa;
                ViewBag.ProductCode = (casa.ProductCode ?? "").Trim();
                ViewBag.CircularRefNum = (casa.CircularRefNum ?? "").Trim();
                ViewBag.DocumentId = casa.DocumentId ?? 0;
                ViewBag.MasterId = longId;
                ViewBag.PosCode = (casa.PosCode ?? "0").Trim();
                ViewBag.EffectiveDate = casa.EffectiveDate?.ToString("dd/MM/yyyy") ?? DateTime.Today.ToString("dd/MM/yyyy");
                ViewBag.CasaId = longId;
                ViewBag.Mode = "detail";
                WriteLog(LogType.INFOR, $"ViewBag set: CircularRefNum='{ViewBag.CircularRefNum}', DocumentId={ViewBag.DocumentId}, CasaId={ViewBag.CasaId}");
                return PartialView("_Detail", model);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Lỗi trong ShowDetailCasaConfig (id={id}): {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "Có lỗi xảy ra khi tải chi tiết cấu hình lãi suất CASA.");
            }
        }

        // Đóng tạm
        //public async Task<IActionResult> ShowDetailCasaConfig(string id)
        //{
        //    try
        //    {
        //        if (!long.TryParse(id, out var longId))
        //        {
        //            WriteLog(LogType.ERROR, $"ID không hợp lệ: {id}");
        //            return BadRequest("ID không hợp lệ.");
        //        }

        //        var casa = await _interestRateConfigureService.GetCasaByIdAsync(longId);
        //        var model = new AddCasaProductViewModel
        //        {
        //            Id = casa.Id,
        //            ProductCode = casa.ProductCode ?? "",
        //            ProductName = casa.ProductName ?? "",
        //            AccountTypeCode = casa.AccountTypeCode ?? "",
        //            AccountTypeName = casa.AccountTypeName ?? "",
        //            AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
        //            CurrencyCode = casa.CurrencyCode ?? "VND",
        //            DebitCreditFlag = casa.DebitCreditFlag ?? "Credit",
        //            EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
        //            ExpiredDate = casa.ExpiryDate,
        //            CircularRefNum = casa.CircularRefNum ?? "",
        //            CircularDate = casa.CircularDate ?? DateTime.Today,
        //            PosCode = casa.PosCode ?? "0",
        //            InterestRate = casa.InterestRate ?? 0m, // Xử lý decimal?
        //            NewInterestRate = casa.NewInterestRate ?? 0m,
        //            PenalRate = casa.PenalRate ?? 0m,
        //            AmoutSlab = casa.AmountSlab ?? 0m,
        //            DocumentId = casa.DocumentId
        //        };

        //        WriteLog(LogType.INFOR, $"View model for ShowDetailCasaConfig: {JsonConvert.SerializeObject(model)}");
        //        return PartialView("_Detail", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"Lỗi trong ShowDetailCasaConfig (id={id}): {ex.Message}\nStackTrace: {ex.StackTrace}");
        //        return StatusCode(500, "Lỗi khi tải chi tiết cấu hình.");
        //    }
        //}






        //[HttpGet]
        //public async Task<IActionResult> ShowDocumentConfig(string filter = "", long id = 0)
        //{
        //    try
        //    {
        //        AddCasaProductViewModel model = null;
        //        List<long> lstIds = new List<long>();
        //        long documentId = 0;

        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            // Parse filter JSON từ Index (PosCode, CircularRefNum, dates...)
        //            var filterObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
        //            string posCode = filterObj.ContainsKey("pPosCode") ? filterObj["pPosCode"].ToString() : "";
        //            string circularRefNum = filterObj.ContainsKey("pCircularRefNum") ? filterObj["pCircularRefNum"].ToString() : "";
        //            string productCode = filterObj.ContainsKey("pProductCode") ? filterObj["pProductCode"].ToString() : "";
        //            DateTime? fromDate = null;
        //            if (filterObj.ContainsKey("pFromEffectiveDate") && DateTime.TryParse(filterObj["pFromEffectiveDate"].ToString(), out var parsedFrom))
        //                fromDate = parsedFrom;
        //            DateTime? toDate = null;
        //            if (filterObj.ContainsKey("pToEffectiveDate") && DateTime.TryParse(filterObj["pToEffectiveDate"].ToString(), out var parsedTo))
        //                toDate = parsedTo;

        //            WriteLog(LogType.INFOR, $"ShowDocumentConfig: Filter - PosCode={posCode}, Circular={circularRefNum}, From={fromDate}, To={toDate}");

        //            // GỌI SERVICE: Load multiple + build model/IdList
        //            var (summaryModel, ids) = await _interestRateConfigureService.GetCasaMasterListByFilterAsync(ProductGroupCode.CASA.Code, posCode, productCode, circularRefNum, fromDate, toDate);
        //            model = summaryModel;
        //            lstIds = ids;
        //            if (model != null && lstIds.Any())
        //            {
        //                model.IdList = string.Join(";", lstIds);  // Cho grid load nếu cần
        //                WriteLog(LogType.INFOR, $"ShowDocumentConfig: Loaded {lstIds.Count} records, IdList={model.IdList}");
        //            }
        //        }
        //        else if (id > 0)
        //        {
        //            // Fallback single id (giữ nguyên cũ)
        //            var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
        //            if (casa == null)
        //            {
        //                WriteLog(LogType.ERROR, $"ShowDocumentConfig - Id: {id}, Casa not found");
        //                return NotFound("Không tìm thấy cấu hình lãi suất.");
        //            }
        //            model = new AddCasaProductViewModel
        //            {
        //                Id = casa.Id,
        //                ProductCode = casa.ProductCode ?? "",
        //                ProductName = casa.ProductName ?? "",
        //                AccountTypeCode = casa.AccountTypeCode ?? "",
        //                AccountTypeName = casa.AccountTypeName ?? "",
        //                AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
        //                CurrencyCode = casa.CurrencyCode ?? "VND",
        //                DebitCreditFlag = casa.DebitCreditFlag ?? "C",
        //                EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
        //                ExpiredDate = casa.ExpiryDate ?? DateTime.Today.AddYears(1),
        //                CircularRefNum = casa.CircularRefNum ?? "",
        //                CircularDate = casa.CircularDate ?? DateTime.Today,
        //                PosCode = casa.PosCode ?? "",
        //                InterestRate = casa.InterestRate ?? 0m,
        //                NewInterestRate = casa.NewInterestRate ?? 0m,
        //                PenalRate = casa.PenalRate ?? 0m,
        //                AmoutSlab = casa.AmountSlab ?? 0m,
        //                DocumentId = casa.DocumentId
        //            };
        //            lstIds.Add(id);
        //            model.IdList = id.ToString();
        //        }

        //        if (model == null)
        //        {
        //            WriteLog(LogType.ERROR, $"ShowDocumentConfig: No data with filter={filter} or id={id}");
        //            return NotFound("Không tìm thấy dữ liệu tờ trình.");
        //        }

        //        // DocumentId logic (gọi service batch update)
        //        documentId = model.DocumentId ?? 0;
        //        if (documentId == 0)
        //        {
        //            documentId = await _interestRateConfigureService.CreateNewDocumentId();
        //            await _interestRateConfigureService.UpdateCasaDocumentIdBatchAsync(lstIds, documentId);  // GỌI SERVICE BATCH
        //            model.DocumentId = documentId;
        //            WriteLog(LogType.INFOR, $"ShowDocumentConfig: Created new DocumentId={documentId} for {lstIds.Count} records");
        //        }
        //        else
        //        {
        //            WriteLog(LogType.INFOR, $"ShowDocumentConfig: Using existing DocumentId={documentId} for {lstIds.Count} records");
        //        }

        //        // GỌI SERVICE: Build ApplyPosList từ lstIds
        //        model.ApplyPosList = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);

        //        ViewBag.IdList = model.IdList;  // Pass cho grid nếu _Document có
        //        ViewBag.DocumentId = documentId;  // Cho upload
        //        return PartialView("_Document", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"ShowDocumentConfig error: {ex.Message}");
        //        return StatusCode(500, $"Lỗi khi tải tờ trình: {ex.Message}");
        //    }
        //}








        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachedFile(long id)
        {
            try
            {
                var files = Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, No file selected");
                    return Json(new { success = false, message = "Vui lòng chọn một file để tải lên." });
                }

                var file = files[0];
                if (file.Length == 0)
                {
                    WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, Invalid file");
                    return Json(new { success = false, message = "File không hợp lệ." });
                }

                // Lấy Casa để kiểm tra
                var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
                if (casa == null)
                {
                    WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, Casa not found");
                    return Json(new { success = false, message = "Không tìm thấy cấu hình lãi suất." });
                }

                var documentIdStr = Request.Form["documentId"];
                WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, Received DocumentId from form: {documentIdStr}");
                var documentId = long.TryParse(documentIdStr, out var parsedDocId) ? parsedDocId : 0;

                // Chỉ tạo mới DocumentId nếu không có hoặc không hợp lệ
                if (documentId <= 0)
                {
                    documentId = await _interestRateConfigureService.CreateNewDocumentId();
                    await _interestRateConfigureService.UpdateCasaDocumentIdAsync(id, documentId);
                    WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, Generated new DocumentId: {documentId}, updated casa");
                }
                else
                {
                    // Đảm bảo DocumentId được lưu vào casa
                    await _interestRateConfigureService.UpdateCasaDocumentIdAsync(id, documentId);
                    WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, Using existing DocumentId: {documentId}, updated casa");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, Deleted existing file: {filePath}");
                    }
                    catch (IOException ex)
                    {
                        WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, Failed to delete existing file {filePath}: {ex.Message}");
                        return Json(new { success = false, message = $"Không thể xóa file cũ: {ex.Message}" });
                    }
                }

                using (var stream = new System.IO.FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                var documentNumber = await _interestRateConfigureService.GetCircularRefNumByDocumentIdAsync(documentId);
                if (string.IsNullOrEmpty(documentNumber))
                {
                    documentNumber = $"DOC-{DateTime.UtcNow:yyyyMMdd}-{documentId:D3}";
                    WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, No CircularRefNum found for DocumentId {documentId}. Using default DocumentNumber: {documentNumber}");
                }

                var attachedFile = new AttachedFileInfo
                {
                    DocumentId = documentId,
                    FileType = file.ContentType,
                    FileName = file.FileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    PathFile = filePath,
                    FileNameNew = uniqueFileName,
                    DocumentNumber = documentNumber,
                    ContentDescription = "File đính kèm tờ trình",
                    Status = 1,
                    CreatedBy = UserName ?? "UnknownUser",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _interestRateConfigureService.SaveAttachedFiles(id, new List<AttachedFileInfo> { attachedFile }, UserName ?? "UnknownUser");
                if (result != null && result.Any())
                {
                    //  WriteLog(LogType.INFOR, $"UploadAttachedFile - Id: {id}, Upload file thành công: {file.FileName}, DocumentId: {documentId}, DocumentNumber: {documentNumber}");
                    var filesList = await _interestRateConfigureService.GetAttachedFilesAsync(documentId);
                    return Json(new { success = true, message = "Tải file lên thành công.", files = filesList, documentId = documentId });
                }
                else
                {
                    // WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, Upload file thất bại: {file.FileName}, DocumentId: {documentId}");
                    return Json(new { success = false, message = "Tải file lên thất bại." });
                }
            }
            catch (Exception ex)
            {
                // WriteLog(LogType.ERROR, $"UploadAttachedFile - Id: {id}, Lỗi khi upload file: {ex.Message}\nInner Exception: {ex.InnerException?.Message ?? "Không có"}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi khi tải file: {ex.Message}\nChi tiết: {ex.InnerException?.Message}" });
            }
        }




        [HttpPost]
        public async Task<IActionResult> LoadAttachedFiles([DataSourceRequest] DataSourceRequest request, long documentId)
        {
            try
            {
                if (documentId == 0)
                {
                    WriteLog(LogType.INFOR, "LoadAttachedFiles - documentId is null or 0, returning empty list");
                    return Json(new DataSourceResult { Data = new List<AttachedFileInfo>(), Total = 0 });
                }

                var files = await _interestRateConfigureService.GetAttachedFilesAsync(documentId);
                WriteLog(LogType.INFOR, $"LoadAttachedFiles - documentId: {documentId}, found {files.Count} files, Data: {JsonConvert.SerializeObject(files)}");
                return Json(files.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Lỗi trong LoadAttachedFiles: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách file đính kèm." });
            }
        }



        //add download
        [HttpGet]
        public async Task<IActionResult> DownloadFile(long fileId)
        {
            try
            {
                var (fileBytes, fileName, contentType) = await _interestRateConfigureService.GetAttachedFileForDownloadAsync(fileId);
                WriteLog(LogType.INFOR, $"DownloadFile - FileId: {fileId}, File: {fileName}"); // Log thêm nếu cần
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"DownloadFile - FileId: {fileId}, Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "Lỗi khi tải file: " + ex.Message);
            }
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteAttachedFile(long fileId)
        //{
        //    try
        //    {
        //        var result = await _interestRateConfigureService.DeleteAttachedFilesAsync(fileId, UserName);
        //        if (result > 0)
        //        {
        //            WriteLog(LogType.INFOR, $"Xóa file thành công: FileId={fileId}");
        //            return Json(new { success = true, message = "Xóa file thành công." });
        //        }
        //        else
        //        {
        //            WriteLog(LogType.ERROR, $"Xóa file thất bại: FileId={fileId}");
        //            return Json(new { success = false, message = "Xóa file thất bại." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"Lỗi khi xóa file: {ex.Message}\nStackTrace: {ex.StackTrace}");
        //        return Json(new { success = false, message = $"Lỗi khi xóa file: {ex.Message}" });
        //    }
        //}


        //add submit






        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppoveCasaRateConfigure()
        {
            //if (request == null || request.Model == null)
            //{
            //    _logger.LogWarning("Request or Model is null.");
            //    return BadRequest(new { success = false, message = "Yêu cầu không hợp lệ." });
            //}
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();

            }

            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Request body is empty.");
            }
            try
            {
                // Không kiểm tra quyền, giống SaveTideRateConfigure
                _logger.LogInformation("User: {UserName}, Role: {UserRole}", UserName, UserRole);
                // Manually deserialize the JSON
                var request = System.Text.Json.JsonSerializer.Deserialize<SaveCasaRateConfigureRequest>(body);
                // Kiểm tra dữ liệu
                if (request.Model.Id <= 0)
                {
                    _logger.LogWarning("Invalid ID: {Id}", request.Model.Id);
                    return BadRequest(new { success = false, message = "ID không hợp lệ." });
                }
                if (string.IsNullOrEmpty(request.Model.CircularRefNum))
                {
                    _logger.LogWarning("CircularRefNum is empty for Id: {Id}", request.Model.Id);
                    return BadRequest(new { success = false, message = "Số Quyết định không được để trống." });
                }
                if (string.IsNullOrEmpty(request.Model.ProductCode))
                {
                    _logger.LogWarning("ProductCode is empty for Id: {Id}", request.Model.Id);
                    return BadRequest(new { success = false, message = "Mã sản phẩm không được để trống." });
                }

                // Lấy bản ghi từ DB
                var casa = await _interestRateConfigureService.GetCasaByIdAsync(request.Model.Id);
                if (casa == null)
                {
                    _logger.LogWarning("No record found for Id: {Id}", request.Model.Id);
                    return NotFound(new { success = false, message = $"Không tìm thấy cấu hình lãi suất với ID: {request.Model.Id}" });
                }

                if (casa.Status == 2)
                {
                    _logger.LogWarning("Record already in approval process: Id: {Id}", request.Model.Id);
                    return BadRequest(new { success = false, message = "Bản ghi đã trong quá trình phê duyệt." });
                }

                // Ánh xạ từ AddCasaProductViewModel sang InterestRateConfigMasterModel
                var casaEntity = _mapper.Map<InterestRateConfigMasterModel>(request.Model);
                casaEntity.Status = 2; // Chờ duyệt
                casaEntity.ModifiedBy = UserName ?? "SYSTEM";
                casaEntity.ModifiedDate = DateTime.Now;

                // Cập nhật bản ghi chính
                var message = await _interestRateConfigureService.UpdateCasaAsync(casaEntity, casaEntity.ModifiedBy);
                if (message.StartsWith("Lỗi"))
                {
                    _logger.LogWarning("UpdateCasaAsync failed for Id: {Id}, Message: {Message}", request.Model.Id, message);
                    return StatusCode(500, new { success = false, message = message });
                }

                // Cập nhật grid items (nếu có)
                if (request.GridItems != null && request.GridItems.Any())
                {
                    foreach (var item in request.GridItems)
                    {
                        var gridCasa = await _interestRateConfigureService.GetCasaByIdAsync(item.Id);
                        if (gridCasa != null)
                        {
                            var gridEntity = _mapper.Map<InterestRateConfigMasterModel>(item);
                            gridEntity.Status = 2; // Chờ duyệt
                            gridEntity.ModifiedBy = UserName ?? "SYSTEM";
                            gridEntity.ModifiedDate = DateTime.Now;
                            await _interestRateConfigureService.UpdateCasaAsync(gridEntity, gridEntity.ModifiedBy);
                        }
                    }
                }

                _logger.LogInformation("Successfully submitted approval for Id: {Id}, User: {UserName}", request.Model.Id, casaEntity.ModifiedBy);
                return Ok(new { success = true, message = "Gửi phê duyệt thành công." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error processing approval for {1}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval for {1}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Lỗi khi gửi phê duyệt: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ShowApproval(string filter = "", long id = 0, string pFlagCall = "")
        {
            try
            {
                var model = new InterestRateConfigMasterViewModel();
                var lstIds = new List<long>();
                string idListStr = "";
                string applyPosListStr = "";
                long documentId = 0;

                // Case 1: Load nhiều bản ghi theo filter (batch approval)
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
                    if (filterObj == null)
                        return BadRequest("Filter không hợp lệ.");

                    string posCode = filterObj.GetValueOrDefault("pPosCode")?.ToString() ?? "";
                    string productCode = filterObj.GetValueOrDefault("pProductCode")?.ToString() ?? "";
                    string circularRefNum = filterObj.GetValueOrDefault("pCircularRefNum")?.ToString() ?? "";

                    DateTime? fromDate = null;
                    if (filterObj.ContainsKey("pFromEffectiveDate") &&
                        DateTime.TryParse(filterObj["pFromEffectiveDate"]?.ToString(), out var f))
                        fromDate = f;

                    DateTime? toDate = null;
                    if (filterObj.ContainsKey("pToEffectiveDate") &&
                        DateTime.TryParse(filterObj["pToEffectiveDate"]?.ToString(), out var t))
                        toDate = t;

                    var (summaryModel, ids) = await _interestRateConfigureService
                        .GetCasaMasterListByFilterAsync(ProductGroupCode.CASA.Code, posCode, productCode, circularRefNum, fromDate, toDate);

                    if (summaryModel == null || !ids.Any())
                        return NotFound("Không tìm thấy dữ liệu để trình duyệt.");

                    lstIds = ids;
                    idListStr = string.Join(";", lstIds);
                    applyPosListStr = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);

                    // Ưu tiên AutoMapper, nếu fail thì map tay
                    model = _mapper.Map<InterestRateConfigMasterViewModel>(summaryModel) ?? new InterestRateConfigMasterViewModel
                    {
                        Id = summaryModel.Id,
                        ProductCode = summaryModel.ProductCode ?? "",
                        ProductName = summaryModel.ProductName ?? "",
                        AccountTypeCode = summaryModel.AccountTypeCode ?? "",
                        AccountTypeName = summaryModel.AccountTypeName ?? "",
                        AccountSubTypeCode = summaryModel.AccountSubTypeCode ?? "0",
                        CurrencyCode = summaryModel.CurrencyCode ?? "VND",
                        DebitCreditFlag = summaryModel.DebitCreditFlag ?? "C",
                        EffectiveDate = summaryModel.EffectiveDate ?? DateTime.Today,
                        ExpiryDate = summaryModel.ExpiredDate ?? DateTime.Today.AddYears(1),
                        CircularRefNum = summaryModel.CircularRefNum ?? "",
                        CircularDate = summaryModel.CircularDate,
                        PosCode = summaryModel.PosCode ?? "",
                        InterestRate = summaryModel.InterestRate,
                        NewInterestRate = summaryModel.NewInterestRate ?? 0m,
                        PenalRate = summaryModel.PenalRate ?? 0m,
                        AmountSlab = summaryModel.AmoutSlab.GetValueOrDefault(0m),
                        DocumentId = summaryModel.DocumentId.GetValueOrDefault(0)
                    };
                }
                // Case 2: Load 1 bản ghi theo id
                else if (id > 0)
                {
                    var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
                    if (casa == null)
                        return NotFound($"Không tìm thấy cấu hình lãi suất với ID: {id}");

                    model = _mapper.Map<InterestRateConfigMasterViewModel>(casa) ?? new InterestRateConfigMasterViewModel
                    {
                        Id = casa.Id,
                        ProductCode = casa.ProductCode ?? "",
                        ProductName = casa.ProductName ?? "",
                        AccountTypeCode = casa.AccountTypeCode ?? "",
                        AccountTypeName = casa.AccountTypeName ?? "",
                        AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
                        CurrencyCode = casa.CurrencyCode ?? "VND",
                        DebitCreditFlag = casa.DebitCreditFlag ?? "C",
                        EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
                        ExpiryDate = casa.ExpiryDate ?? DateTime.Today.AddYears(1),
                        CircularRefNum = casa.CircularRefNum ?? "",
                        CircularDate = casa.CircularDate,
                        PosCode = casa.PosCode ?? "",
                        InterestRate = casa.InterestRate ?? 0m,
                        NewInterestRate = casa.NewInterestRate ?? 0m,
                        PenalRate = casa.PenalRate ?? 0m,
                        AmountSlab = casa.AmountSlab.GetValueOrDefault(0m),
                        DocumentId = casa.DocumentId.GetValueOrDefault(0)
                    };

                    lstIds.Add(id);
                    idListStr = id.ToString();
                    applyPosListStr = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
                }
                else
                {
                    return BadRequest("Cần cung cấp filter hoặc id.");
                }

                // Gán IdList cho model (rất quan trọng cho JS phía view)
                model.IdList = idListStr;

                // Xử lý DocumentId: nếu chưa có thì tạo mới + update batch
                documentId = model.DocumentId;
                if (documentId == 0)
                {
                    documentId = await _interestRateConfigureService.CreateNewDocumentId();
                    await _interestRateConfigureService.UpdateCasaDocumentIdBatchAsync(lstIds, documentId);
                    model.DocumentId = documentId;
                }

                // ViewBag để view dùng
                ViewBag.IdList = idListStr;
                ViewBag.ApplyPosList = applyPosListStr;
                ViewBag.DocumentId = documentId;
                ViewBag.FlagCall = pFlagCall;

                // Mô tả trạng thái
                model.StatusDesc = ConfigStatus.GetByValue(model.Status)?.Description ?? "Không xác định";

                _logger.LogInformation($"ShowApproval thành công - Số bản ghi: {lstIds.Count}, DocumentId: {documentId}");

                return PartialView("_Approval", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải màn hình Approval");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tải thông tin trình duyệt.", detail = ex.Message });
            }
        }


        //Check số qđ
        [HttpGet]
        public IActionResult CheckCircular(string circularRefNum, DateTime circularDate)
        {
            try
            {
                bool isValid = !string.IsNullOrWhiteSpace(circularRefNum);
                bool isExists = _interestRateConfigureService.CheckCircular(circularRefNum.Trim().ToUpper(), circularDate);

                return Json(new
                {
                    isValid,
                    isExists
                });
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"CheckCircular CASA error: {ex.Message}");
                return Json(new { isValid = false, isExists = false, error = "Có lỗi xảy ra khi kiểm tra số quyết định." });
            }
        }


        //add hàm save authorize
        [HttpPost]
        public async Task<IActionResult> SaveAuthorizeForm([DataSourceRequest] DataSourceRequest request, InterestRateConfigMasterViewModel model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    var lstId = StringHelper.ConvertToLongList(model.IdList, ';');
                    var status = await _interestRateConfigureService.SaveApprovalDecision(UserName, lstId, model.RejectFlag, model.RejectReason);

                    if (status > 0)
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    else
                    {
                        ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                }
                else
                {
                    ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                ModelState.AddModelError("ERROR", $"{e.Message}");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

        }

        // SỬA: ShowCasaConfigDetail (thêm log, đảm bảo IdList không empty)
        [HttpGet]
        
            public async Task<IActionResult> ShowApprovalConfig(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
        {
            if (string.IsNullOrEmpty(circularRefNum))
            {
                WriteLog(LogType.ERROR, "ShowApprovalConfig: Missing circularRefNum");
                return NotFound("Thiếu thông tin quyết định");
            }
            var model = await _interestRateConfigureService.GetCasaInterestRateDetailViews(circularRefNum, effectDate);
            if (model == null)
            {
                WriteLog(LogType.ERROR, $"ShowApprovalConfig: No data for circularRefNum={circularRefNum}");
                return NotFound("Không tìm thấy dữ liệu chi tiết");
            }
            // THÊM: Log IdList để debug
            WriteLog(LogType.INFOR, $"ShowCasaConfigDetail: Loaded model with IdList='{model.IdList}', Count expected ~{model.IdList?.Split(';').Length ?? 0}");
            ViewBag.IdList = idList ?? model.IdList; // Fallback từ model nếu ViewBag empty
            ViewBag.FlagCall = pFlagCall;
            return PartialView("_Approval", model);
        }
        [HttpGet]
        public async Task<IActionResult> ShowAuthorizeScreen(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
      
        {
            if (string.IsNullOrEmpty(circularRefNum))
            {
                // Có thể tạm bỏ kiểm tra này để test
                // WriteLog(LogType.ERROR, "Thiếu circularRefNum");
                // return NotFound();
            }
            var model = await _interestRateConfigureService.GetCasaInterestRateDetailViews(circularRefNum, effectDate);
           // var model = await _interestRateConfigureService.GetCasaInterestRateDetailViews(circularRefNum ??);

            if (model == null)
            {
                return Content("Không tìm thấy dữ liệu (model null)");
            }

            ViewBag.IdList = idList ?? model.IdList;
            ViewBag.FlagCall = pFlagCall;

            return PartialView("_Authorize", model); // hoặc _Authorize
        }
        //upload attached file for approval
        // 1. Upload file - nhận documentId từ form (không cần id Casa nữa)


        [HttpPost]
        public async Task<IActionResult> LoadAttachFileGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                // Lấy documentNumber từ form (gửi từ JS paraAttachFileGridData)
                var documentNumber = Request.Form["documentNumber"].ToString();

                // Gọi service lấy danh sách file đính kèm theo DocumentNumber (Số quyết định)
                // Nếu service của bạn dùng DocumentId thay vì DocumentNumber thì sửa lại ở đây
                var models = await _interestRateConfigureService.GetAttachedFilesAsync(documentNumber);

                // Nếu service GetAttachedFilesAsync nhận DocumentId (long), thì dùng cách khác:
                // long documentId = long.Parse(documentNumber); // hoặc lấy từ ViewBag/model
                // var models = await _interestRateConfigureService.GetAttachedFilesAsync(documentId);

                var result = models.ToDataSourceResult(request, ModelState);

                return Json(result);
            }
            catch (Exception ex)
            {
                //WriteLog(LogType.ERROR, $"LoadAttachFileGridData CASA Error: {ex.Message} | Stack: {ex.StackTrace}", ex);
                return Json(new DataSourceResult { Data = new List<AttachedFileInfo>(), Total = 0 });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadAttachedFile()
        {
            try
            {
                var files = Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một file để tải lên." });
                }

                var documentIdStr = Request.Form["documentId"].ToString();
                if (!long.TryParse(documentIdStr, out long documentId) || documentId <= 0)
                {
                    return Json(new { success = false, message = "DocumentId không hợp lệ." });
                }

                var uploadResults = new List<object>();
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var documentNumber = await _interestRateConfigureService.GetCircularRefNumByDocumentIdAsync(documentId)
                                        ?? $"DOC-{DateTime.Now:yyyyMMdd}-{documentId}";

                    var attachedFile = new AttachedFileInfo
                    {
                        DocumentId = documentId,
                        FileType = file.ContentType,
                        FileName = file.FileName,
                        FileExtension = Path.GetExtension(file.FileName),
                        PathFile = filePath,
                        FileNameNew = uniqueFileName,
                        DocumentNumber = documentNumber,
                        ContentDescription = "File đính kèm tờ trình lãi suất CASA",
                        Status = 1,
                        CreatedBy = UserName ?? "System",
                        CreatedDate = DateTime.Now
                    };

                    var savedId = await _interestRateConfigureService.SaveAttachedFileAsync(attachedFile); // Nên có method Save single
                    if (savedId > 0)
                    {
                        uploadResults.Add(new
                        {
                            FileId = savedId,
                            FileNameNew = attachedFile.FileNameNew,
                            FileName = attachedFile.FileName,
                            FileType = attachedFile.FileType,
                            CreatedDate = attachedFile.CreatedDate
                        });
                    }
                }

                // Refresh grid
                var updatedList = await _interestRateConfigureService.GetAttachedFilesAsync(documentId);

                return Json(new
                {
                    success = true,
                    message = $"Tải lên thành công {files.Count} file.",
                    files = updatedList
                });
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"UploadAttachedFile error: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi khi tải file: " + ex.Message });
            }
        }

        // 2. Load danh sách file theo documentId (đã có, nhưng sửa để nhận từ query hoặc body)
        [HttpPost]
        public async Task<IActionResult> LoadAttachedFiles([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                // Lấy documentId từ form (View gửi qua Data("function() { return { documentId: ... } }"))
                var documentIdStr = Request.Form["documentId"].ToString();
                if (!long.TryParse(documentIdStr, out long documentId) || documentId <= 0)
                {
                    return Json(new DataSourceResult { Data = new List<AttachedFileInfo>(), Total = 0 });
                }

                var files = await _interestRateConfigureService.GetAttachedFilesAsync(documentId);
                return Json(files.ToDataSourceResult(request));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"LoadAttachedFiles error: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AttachedFileInfo>(), Total = 0 });
            }
        }

        // 3. Download file (đã có, giữ nguyên hoặc sửa tên action cho rõ)
        [HttpGet]
        public async Task<IActionResult> DownloadAttachedFile(long fileId) // Đổi tên cho rõ
        {
            try
            {
                var (fileBytes, fileName, contentType) = await _interestRateConfigureService.GetAttachedFileForDownloadAsync(fileId);
                if (fileBytes == null)
                    return NotFound("File không tồn tại.");

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"DownloadAttachedFile fileId={fileId} error: {ex.Message}");
                return StatusCode(500, "Lỗi tải file.");
            }
        }

        // 4. Xóa file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachedFile(long fileId)
        {
            try
            {
                var result = await _interestRateConfigureService.DeleteAttachedFileAsync(fileId, UserName ?? "System");
                if (result > 0)
                {
                    return Json(new { success = true, message = "Xóa file thành công." });
                }
                return Json(new { success = false, message = "Xóa file thất bại." });
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"DeleteAttachedFile fileId={fileId} error: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xóa file." });
            }
        }



        // Save
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> SaveApprovalForm(
        //[DataSourceRequest] DataSourceRequest request,
        //InterestRateConfigMasterViewModel model,
        //IFormFile fileUpload)
        //    {
        //        try
        //        {
        //            if (model == null || string.IsNullOrEmpty(model.IdList))
        //            {
        //                ModelState.AddModelError("ERROR", "Dữ liệu không hợp lệ hoặc không có bản ghi.");
        //                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //            }

        //            Kiểm tra file
        //            if (fileUpload == null || fileUpload.Length == 0)
        //            {
        //                ModelState.AddModelError("ERROR", "Vui lòng chọn file tờ trình PDF.");
        //                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //            }

        //            if (Path.GetExtension(fileUpload.FileName).ToLowerInvariant() != ".pdf")
        //            {
        //                ModelState.AddModelError("ERROR", "Chỉ chấp nhận file PDF.");
        //                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //            }

        //            Tạo DocumentId mới(bắt buộc)
        //            long documentId = await _interestRateConfigureService.CreateNewDocumentId();

        //            Lưu file vật lý
        //            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ToTrinh");
        //            Directory.CreateDirectory(uploadPath);

        //            var fileNameNew = $"{Guid.NewGuid()}.pdf";
        //            var filePath = Path.Combine(uploadPath, fileNameNew);

        //            await using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await fileUpload.CopyToAsync(stream);
        //            }

        //            Fallback DocumentNumber nếu rỗng
        //            var documentNumber = string.IsNullOrWhiteSpace(model.CircularRefNum)
        //                ? $"CASA_{documentId}_{DateTime.Now:yyyyMMdd}"
        //                : model.CircularRefNum.Trim();

        //            Tạo attached file với DocumentId bắt buộc
        //           var attachedFile = new AttachedFileInfo
        //           {
        //               DocumentId = documentId, // ← BẮT BUỘC PHẢI CÓ
        //               FileType = "application/pdf",
        //               FileName = fileUpload.FileName,
        //               PathFile = filePath,
        //               FileExtension = ".pdf",
        //               FileNameNew = fileNameNew,
        //               DocumentNumber = documentNumber,
        //               Status = 1,
        //               CreatedBy = UserName ?? "System",
        //               CreatedDate = DateTime.Now,
        //               ModifiedBy = UserName ?? "System",
        //               ModifiedDate = DateTime.Now
        //           };

        //            Lưu vào DB
        //           var saveResult = await _interestRateConfigureService.SaveAttachedFiles(
        //               0, // configureId = 0 vẫn được, vì đã có DocumentId
        //               new List<AttachedFileInfo> { attachedFile },
        //               UserName ?? "System");

        //            if (saveResult == null || !saveResult.Any())
        //            {
        //                ModelState.AddModelError("ERROR", "Lưu file tờ trình vào CSDL thất bại.");
        //                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //            }

        //            Cập nhật trạng thái
        //            var lstId = StringHelper.ConvertToLongList(model.IdList, ';');
        //            var updateCount = await _interestRateConfigureService.UpdateInterestRateConfigMasterStatus(
        //                UserName ?? "System",
        //                lstId,
        //                ConfigStatus.PROCESS.Value,
        //                documentId);

        //            if (updateCount <= 0)
        //            {
        //                ModelState.AddModelError("ERROR", "Cập nhật trạng thái trình duyệt thất bại.");
        //                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //            }

        //            Thành công thật
        //            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog(LogType.ERROR, $"SaveApprovalForm failed: {ex.Message} | Inner: {ex.InnerException?.Message}", ex);
        //            ModelState.AddModelError("ERROR", "Lỗi hệ thống: " + ex.Message);
        //            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        //        }
        //    }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveApprovalForm(
    [DataSourceRequest] DataSourceRequest request,
    InterestRateConfigMasterViewModel model,
    IFormFile fileUpload)
        {
            var logPrefix = $"SaveApprovalForm CASA - CircularRefNum: '{model?.CircularRefNum}', IdList: '{model?.IdList}'";
            WriteLog(LogType.INFOR, logPrefix + " - Bắt đầu xử lý trình duyệt");

            try
            {
                if (model == null || string.IsNullOrEmpty(model.IdList))
                {
                    WriteLog(LogType.ERROR, logPrefix + " - Model null hoặc IdList rỗng");
                    ModelState.AddModelError("ERROR", "Dữ liệu không hợp lệ hoặc không có bản ghi để trình duyệt.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // Kiểm tra file PDF bắt buộc
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    WriteLog(LogType.ERROR, logPrefix + " - Không có file upload");
                    ModelState.AddModelError("ERROR", "Vui lòng chọn file tờ trình PDF.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                var extension = Path.GetExtension(fileUpload.FileName).ToLowerInvariant();
                if (extension != ".pdf" || fileUpload.ContentType != "application/pdf")
                {
                    WriteLog(LogType.ERROR, logPrefix + " - File không phải PDF");
                    ModelState.AddModelError("ERROR", "Chỉ chấp nhận file định dạng PDF.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                if (fileUpload.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ERROR", "Dung lượng file tối đa 5MB.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // Tạo DocumentId mới
                long documentId = await _interestRateConfigureService.CreateNewDocumentId();
                WriteLog(LogType.INFOR, logPrefix + $" - Tạo DocumentId mới: {documentId}");

                // Lưu file vật lý
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ToTrinh");
                Directory.CreateDirectory(uploadPath);

                var fileNameNew = $"{Guid.NewGuid()}.pdf";
                var filePath = Path.Combine(uploadPath, fileNameNew);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }
                WriteLog(LogType.INFOR, logPrefix + $" - Lưu file vật lý thành công: {filePath}");

                // Fallback DocumentNumber nếu CircularRefNum rỗng
                var documentNumber = string.IsNullOrWhiteSpace(model.CircularRefNum)
                    ? $"CASA_TRINHDUYET_{documentId}"
                    : model.CircularRefNum.Trim();

                // Tạo object AttachedFileInfo
                var attachedFile = new AttachedFileInfo
                {
                    DocumentId = documentId,
                    FileType = "application/pdf",
                    FileName = fileUpload.FileName,
                    PathFile = filePath,
                    FileExtension = ".pdf",
                    FileNameNew = fileNameNew,
                    DocumentNumber = documentNumber,
                    Status = 1,
                    CreatedBy = UserName ?? "System",
                    CreatedDate = DateTime.Now,
                    ModifiedBy = UserName ?? "System",
                    ModifiedDate = DateTime.Now
                };

                // Lưu file vào bảng AttachedFileInfo
                var saveResult = await _interestRateConfigureService.SaveAttachedFiles(
                    0,
                    new List<AttachedFileInfo> { attachedFile },
                    UserName ?? "System");

                if (saveResult == null || !saveResult.Any())
                {
                    WriteLog(LogType.ERROR, logPrefix + " - SaveAttachedFiles thất bại (return null/empty)");
                    ModelState.AddModelError("ERROR", "Lưu file tờ trình vào hệ thống thất bại.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                WriteLog(LogType.INFOR, logPrefix + $" - Lưu file DB thành công, FileId: {saveResult.FirstOrDefault()}");

                // Cập nhật trạng thái các bản ghi → Chờ duyệt (Value = 2)
                var lstId = StringHelper.ConvertToLongList(model.IdList, ';');

                var updateCount = await _interestRateConfigureService.UpdateInterestRateConfigMasterStatus(
                    UserName ?? "System",
                    lstId,
                    ConfigStatus.PROCESS.Value,  // ← Value = 2 → "Chờ duyệt"
                    documentId);

                if (updateCount <= 0)
                {
                    WriteLog(LogType.ERROR, logPrefix + $" - Cập nhật trạng thái thất bại, updateCount = {updateCount}");
                    ModelState.AddModelError("ERROR", "Cập nhật trạng thái 'Chờ duyệt' thất bại.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                WriteLog(LogType.INFOR, logPrefix + $" - TRÌNH DUYỆT THÀNH CÔNG - DocumentId: {documentId}, Cập nhật {updateCount} bản ghi → Status = Chờ duyệt");

                // Thành công hoàn toàn
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                //WriteLog(LogType.ERROR, logPrefix + $" - EXCEPTION: {ex.Message} | Inner: {ex.InnerException?.Message ?? "null"}", ex);
                ModelState.AddModelError("ERROR", "Có lỗi xảy ra trong quá trình trình duyệt: " + ex.Message);
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
        }



        //[HttpGet]
        //public async Task<IActionResult> ShowApproval(string filter = "", long id = 0, string pFlagCall = "")
        //{
        //    try
        //    {
        //        InterestRateConfigMasterViewModel model = null;
        //        List<long> lstIds = new List<long>();
        //        long documentId = 0;
        //        string idListStr = "";
        //        string applyPosListStr = "";
        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            // Parse filter (giữ nguyên JsonConvert, thêm try-catch)
        //            try
        //            {
        //                var filterObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
        //                string posCode = filterObj.ContainsKey("pPosCode") ? filterObj["pPosCode"].ToString() : "";
        //                string circularRefNum = filterObj.ContainsKey("pCircularRefNum") ? filterObj["pCircularRefNum"].ToString() : "";
        //                string productCode = filterObj.ContainsKey("pProductCode") ? filterObj["pProductCode"].ToString() : "";
        //                DateTime? fromDate = null;
        //                if (filterObj.ContainsKey("pFromEffectiveDate") && DateTime.TryParse(filterObj["pFromEffectiveDate"].ToString(), out var parsedFrom))
        //                    fromDate = parsedFrom;
        //                DateTime? toDate = null;
        //                if (filterObj.ContainsKey("pToEffectiveDate") && DateTime.TryParse(filterObj["pToEffectiveDate"].ToString(), out var parsedTo))
        //                    toDate = parsedTo;
        //                WriteLog(LogType.INFOR, $"ShowApproval: Filter - PosCode={posCode}, Circular={circularRefNum}, From={fromDate}, To={toDate}");
        //                // GỌI SERVICE: Load multiple
        //                var (summaryCasaModel, ids) = await _interestRateConfigureService.GetCasaMasterListByFilterAsync(ProductGroupCode.CASA.Code, posCode, productCode, circularRefNum, fromDate, toDate);
        //                if (summaryCasaModel != null && ids.Any())
        //                {
        //                    lstIds = ids;
        //                    idListStr = string.Join(";", lstIds);
        //                    applyPosListStr = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        //                    WriteLog(LogType.INFOR, $"ShowApproval: Loaded {lstIds.Count} records, IdList={idListStr}");
        //                    // Map AddCasaProductViewModel → InterestRateConfigMasterViewModel
        //                    model = _mapper.Map<InterestRateConfigMasterViewModel>(summaryCasaModel);
        //                    if (model == null)
        //                    {
        //                        // MANUAL MAP (giữ nguyên, thêm set IdList nếu model có property)
        //                        model = new InterestRateConfigMasterViewModel
        //                        {
        //                            Id = summaryCasaModel.Id,
        //                            ProductCode = summaryCasaModel.ProductCode ?? "",
        //                            ProductName = summaryCasaModel.ProductName ?? "",
        //                            AccountTypeCode = summaryCasaModel.AccountTypeCode ?? "",
        //                            AccountTypeName = summaryCasaModel.AccountTypeName ?? "",
        //                            AccountSubTypeCode = summaryCasaModel.AccountSubTypeCode ?? "0",
        //                            CurrencyCode = summaryCasaModel.CurrencyCode ?? "VND",
        //                            DebitCreditFlag = summaryCasaModel.DebitCreditFlag ?? "C",
        //                            EffectiveDate = summaryCasaModel.EffectiveDate ?? DateTime.Today,
        //                            ExpiryDate = summaryCasaModel.ExpiredDate ?? DateTime.Today.AddYears(1),
        //                            CircularRefNum = summaryCasaModel.CircularRefNum ?? "",
        //                            CircularDate = summaryCasaModel.CircularDate,
        //                            PosCode = summaryCasaModel.PosCode ?? "",
        //                            InterestRate = summaryCasaModel.InterestRate,
        //                            NewInterestRate = summaryCasaModel.NewInterestRate ?? 0m,
        //                            PenalRate = summaryCasaModel.PenalRate ?? 0m,
        //                            AmountSlab = summaryCasaModel.AmoutSlab.GetValueOrDefault(0m),
        //                            DocumentId = summaryCasaModel.DocumentId.GetValueOrDefault(0),
        //                            IdList = idListStr,  // THÊM: Set nếu model có IdList property (từ code bạn, có)
        //                                                 // Thêm fields khác nếu cần (Status = 1, etc.)
        //                        };
        //                    }
        //                    else
        //                    {
        //                        // Nếu mapper OK, set IdList thủ công
        //                        model.IdList = idListStr;
        //                    }
        //                }
        //            }
        //            catch (Newtonsoft.Json.JsonException jsonEx)
        //            {
        //                WriteLog(LogType.ERROR, $"ShowApproval: Filter JSON parse error: {jsonEx.Message}");
        //                return BadRequest("Filter không hợp lệ.");
        //            }
        //        }
        //        else if (id > 0)
        //        {
        //            // Fallback single (giữ nguyên)
        //            var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
        //            if (casa == null)
        //            {
        //                _logger.LogWarning($"ShowApproval - No record found for Id: {id}");
        //                return StatusCode(404, new { message = $"Không tìm thấy cấu hình lãi suất với ID: {id}" });
        //            }
        //            model = _mapper.Map<InterestRateConfigMasterViewModel>(casa);
        //            if (model == null)
        //            {

        //                model = new InterestRateConfigMasterViewModel
        //                {
        //                    Id = casa.Id,
        //                    ProductCode = casa.ProductCode ?? "",
        //                    AccountTypeCode = casa.AccountTypeCode ?? "",
        //                    AccountTypeName = casa.AccountTypeName ?? "",
        //                    AccountSubTypeCode = casa.AccountSubTypeCode ?? "0",
        //                    CurrencyCode = casa.CurrencyCode ?? "VND",
        //                    DebitCreditFlag = casa.DebitCreditFlag ?? "C",
        //                    EffectiveDate = casa.EffectiveDate ?? DateTime.Today,
        //                    ExpiryDate = casa.ExpiryDate ?? DateTime.Today.AddYears(1),
        //                    CircularRefNum = casa.CircularRefNum ?? "",
        //                    CircularDate = casa.CircularDate,
        //                    PosCode = casa.PosCode ?? "",
        //                    InterestRate = casa.InterestRate ?? 0m,
        //                    NewInterestRate = casa.NewInterestRate ?? 0m,
        //                    PenalRate = casa.PenalRate ?? 0m,
        //                    AmountSlab = casa.AmountSlab.GetValueOrDefault(0m),
        //                    DocumentId = casa.DocumentId.GetValueOrDefault(0),
        //                    IdList = id.ToString(),  // THÊM: Set IdList cho single
        //                                             // Thêm fields khác nếu cần
        //                };
        //            }
        //            else
        //            {
        //                model.IdList = id.ToString();  // Set nếu mapper OK
        //            }
        //            lstIds.Add(id);
        //            idListStr = id.ToString();
        //            applyPosListStr = await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        //        }
        //        if (model == null)
        //        {
        //            _logger.LogWarning($"ShowApproval - No data with filter={filter} or id={id}");
        //            return StatusCode(404, new { message = "Không tìm thấy dữ liệu để trình duyệt." });
        //        }

        //        documentId = model.DocumentId;
        //        if (documentId == 0)
        //        {
        //            documentId = await _interestRateConfigureService.CreateNewDocumentId();
        //            await _interestRateConfigureService.UpdateCasaDocumentIdBatchAsync(lstIds, documentId);
        //            model.DocumentId = documentId;
        //            WriteLog(LogType.INFOR, $"ShowApproval: Created new DocumentId={documentId} for {lstIds.Count} records");
        //        }

        //        ViewBag.IdList = model.IdList ?? idListStr;
        //        ViewBag.ApplyPosList = applyPosListStr;
        //        ViewBag.DocumentId = documentId;
        //        ViewBag.FlagCall = pFlagCall;
        //        model.StatusDesc = ConfigStatus.GetByValue(model.Status).Description; // Giữ nguyên
        //        _logger.LogInformation($"ShowApproval - Rendering _Approval for filter={filter}/id={id}, Count={lstIds.Count}, DocumentId={documentId}");
        //        return PartialView("_Approval", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"ShowApproval error: {ex.Message}");
        //        WriteLog(LogType.ERROR, $"ShowApproval exception: {ex.Message} | Stack: {ex.StackTrace}");  // THÊM: Log chi tiết để debug 500
        //        return StatusCode(500, new { message = "Có lỗi xảy ra khi tải thông tin trình duyệt.", detail = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> ShowApproval(long id)
        //{
        //    try
        //    {
        //        if (id <= 0)
        //        {
        //            _logger.LogWarning($"ShowApproval - Invalid ID: {id}");
        //            return StatusCode(400, new { message = "ID không hợp lệ." });
        //        }

        //        var casa = await _interestRateConfigureService.GetCasaByIdAsync(id);
        //        if (casa == null)
        //        {
        //            _logger.LogWarning($"ShowApproval - No record found for Id: {id}");
        //            return StatusCode(404, new { message = $"Không tìm thấy cấu hình lãi suất với ID: {id}" });
        //        }

        //        var viewModel = new AddCasaProductViewModel
        //        {
        //            Id = casa.Id,
        //            ProductCode = casa.ProductCode,
        //            ProductName = casa.ProductName,
        //            AccountTypeName = casa.AccountTypeName,
        //            CircularRefNum = casa.CircularRefNum,
        //            PosCode = casa.PosCode,
        //            InterestRate = casa.InterestRate ?? 0,
        //            EffectiveDate = casa.EffectiveDate,
        //            ExpiredDate = casa.ExpiryDate,
        //            DocumentId = casa.DocumentId,
        //            AccountTypeCode = casa.AccountTypeCode ?? casa.AccountTypeName,
        //            AccountSubTypeCode = casa.AccountSubTypeCode ?? "DEFAULT",
        //            CurrencyCode = casa.CurrencyCode ?? "VND",
        //            DebitCreditFlag = casa.DebitCreditFlag ?? "Credit",
        //            NewInterestRate = casa.NewInterestRate ?? casa.InterestRate,
        //            PenalRate = casa.PenalRate ?? 0,
        //            AmoutSlab = casa.AmountSlab,
        //            CircularDate = casa.CircularDate ?? casa.ModifiedDate ?? DateTime.Now,
        //            ProductList = new List<Product>(),
        //            AccountTypes = new List<SelectListItem>(),
        //            AccountSubTypes = new List<SelectListItem>(),
        //            GridItems = new List<CasaRateProductViewModel>()
        //        };

        //        _logger.LogInformation($"ShowApproval - Rendering _Approval.cshtml for Id: {id}, ProductCode: {casa.ProductCode}, DocumentId: {viewModel.DocumentId}");
        //        return PartialView("_Approval", viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"ShowApproval - Id: {id}");
        //        return StatusCode(500, new { message = "Có lỗi xảy ra khi tải thông tin cấu hình lãi suất.", detail = ex.Message });
        //    }
        //}




    }
}