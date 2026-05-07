using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class ProductParameterService : IProductParameterService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductParameterService> _logger;

        public ProductParameterService(ApplicationDbContext dbContext, IMapper mapper, ILogger<ProductParameterService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ProductParameterComparisonViewModel>> GetComparisonListAsync(
     string productGroupCode = null,
     string productCode = null,
     DateTime? targetFutureDate = null)
        {
            try
            {
                var today = DateTime.Today;

                
                var currentQuery = _dbContext.ProductParameters
                    .Where(p => p.EffectedDate <= today
                             && p.Status == ConfigStatus.AUTHORIZED.Value);  // 3 = Phê duyệt / Active

             
                var proposedQuery = _dbContext.ProductParameters
                    .Where(p => p.EffectedDate > today
                             && (p.Status == ConfigStatus.MAKER.Value     // 1 = Tạo lập
                              || p.Status == ConfigStatus.PROCESS.Value)); // 2 = Chờ duyệt

                // Filter nếu có
                if (!string.IsNullOrEmpty(productGroupCode))
                {
                    currentQuery = currentQuery.Where(p => p.ProductGroupCode == productGroupCode);
                    proposedQuery = proposedQuery.Where(p => p.ProductGroupCode == productGroupCode);
                }

                if (!string.IsNullOrEmpty(productCode))
                {
                    currentQuery = currentQuery.Where(p => p.ProductCode == productCode);
                    proposedQuery = proposedQuery.Where(p => p.ProductCode == productCode);
                }

                if (targetFutureDate.HasValue)
                {
                    proposedQuery = proposedQuery.Where(p => p.EffectedDate <= targetFutureDate.Value);
                }

                var currents = await currentQuery.ToListAsync();
                var proposeds = await proposedQuery.ToListAsync();

                // Gộp theo ProductGroupCode + ProductCode
                var allKeys = currents.Select(c => new { c.ProductGroupCode, c.ProductCode })
                                        .Union(proposeds.Select(p => new { p.ProductGroupCode, p.ProductCode })).Distinct().ToList();

                var result = new List<ProductParameterComparisonViewModel>();
                int iOrderNoTemp = 1;

                foreach (var key in allKeys)
                {
                    var current = currents.FirstOrDefault(c => c.ProductGroupCode == key.ProductGroupCode && c.ProductCode == key.ProductCode);
                    var proposed = proposeds.FirstOrDefault(p => p.ProductGroupCode == key.ProductGroupCode && p.ProductCode == key.ProductCode);

                    var vm = new ProductParameterComparisonViewModel
                    {
                        OrderNo = iOrderNoTemp++,
                        ProductGroupDisplay = current?.ProductGroupDisplay ?? proposed?.ProductGroupDisplay ?? key.ProductGroupCode,
                        ProductCode = key.ProductCode,
                        ProductName = current?.ProductName ?? proposed?.ProductName ?? "",

                        CurrentApplyPos = current?.ApplyPosDisplay ?? "",
                        CurrentMinSpread = current?.MinInterestRateSpread ?? 0,
                        CurrentMaxSpread = current?.MaxInterestRateSpread ?? 0,
                        CurrentEffectedDate = current?.EffectedDate,

                        NewApplyPos = proposed?.ApplyPosDisplay ?? "",
                        NewMinSpread = proposed?.MinInterestRateSpread,
                        NewMaxSpread = proposed?.MaxInterestRateSpread,
                        NewEffectedDate = proposed?.EffectedDate,

                        HasChange = proposed != null && (current == null ||
                            current.ApplyPosFlag != proposed.ApplyPosFlag ||
                            current.MinInterestRateSpread != proposed.MinInterestRateSpread ||
                            current.MaxInterestRateSpread != proposed.MaxInterestRateSpread),

                        ProposalStatus = proposed != null
                            ? ConfigStatus.GetByValue(proposed.Status)?.Description ?? "Không xác định"
                            : ""
                    };

                    result.Add(vm);
                }

                _logger.LogInformation($"GetComparisonListAsync: Returned {result.Count} comparison items (current: {currents.Count}, proposed: {proposeds.Count})");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComparisonListAsync");
                throw;
            }
        }

        public async Task<List<SelectListItem>> GetProductOptionsByGroupAsync(string productGroupCode = null)
        {
            try
            {
                var query = _dbContext.ListOfProducts.AsQueryable();

                if (!string.IsNullOrEmpty(productGroupCode))
                {
                    query = query.Where(p => p.ProductGroupCode == productGroupCode);
                }

                var products = await query
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProductCode,
                        Text = $"{p.ProductCode} - {p.ProductName ?? p.ProductCode}"
                    })
                    .OrderBy(p => p.Text)
                    .ToListAsync();

                _logger.LogInformation($"GetProductOptionsByGroupAsync: Loaded {products.Count} products for group '{productGroupCode ?? "All"}'");
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi load danh sách sản phẩm theo nhóm: {Group}", productGroupCode);
                return new List<SelectListItem>();
            }
        }




        /// <summary>
        /// Hàm lấy danh sách thông tin tham số cấu hình của sản phẩm TIDE/CASA/DEPOSITPENAL
        /// </summary>
        /// <param name="pFlagCall">Cờ xác định sự kiến gọi. Nếu không truyền vào thì là 0</param>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pEffectedDate">Ngày hiệu lực</param>
        /// <param name="pId">Chỉ số xác định bản ghi (Không bắt buộc truyền vào là 0)</param>
        /// <param name="pProductCode">Mã sản phẩm (Không bắt buộc)</param>
        /// <param name="pRemark">Ghi chú (Không bắt buộc)</param>
        /// <returns>Danh sách thông tin tham số cấu hình của sản phẩm TIDE/CASA/DEPOSITPENAL theo model ProductParameterViewModel</returns>
        //public List<ProductParameterComparisonViewModel> GetListProductParameters(string pFlagCall, string pProductGroupCode, string pEffectedDate,
        //                                    long pId, string pProductCode, string pRemark)
        //{
        //    try
        //    {
        //        List<ProductParameterComparisonViewModel> listProductParameters = new List<ProductParameterComparisonViewModel>();
        //        DateTime dEffectedDate = CustConverter.StringToDate(pEffectedDate.ToString(), FormatParameters.FORMAT_DATE_INT);
        //        List<ProductParameter> listProductParametersTmp = new List<ProductParameter>();


        //        if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
        //        {
        //            var dEffectedDateMax = _dbContext.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
        //                                                && w.EffectedDate <= dEffectedDate.Date).Select(s => s.EffectedDate).Max();

        //            listProductParametersTmp = _dbContext.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
        //              && w.EffectedDate == dEffectedDateMax.Date).OrderByDescending(o => o.ProductCode).ThenBy(o => o.EffectedDate).ToList();
        //        }
        //        else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
        //        {
        //            listProductParametersTmp = _dbContext.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
        //              && w.EffectedDate == dEffectedDate.Date).OrderByDescending(o => o.ProductCode).ThenBy(o => o.EffectedDate).ToList();

        //        }
        //        else
        //        {
        //            listProductParametersTmp = _dbContext.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
        //                  && (pId == 0 || w.Id == pId)
        //                    && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode)
        //                    && w.EffectedDate == dEffectedDate.Date)
        //                 .Where(delegate (ProductParameter c)
        //                 {
        //                     if (string.IsNullOrEmpty(pRemark)
        //                         || (c.Remark != null && c.Remark.ToLower().Contains(pRemark.ToLower()))
        //                         || (c.Remark != null && Utilities.ConvertToUnSign(c.Remark.ToLower()).IndexOf(pRemark.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
        //                         )
        //                         return true;
        //                     else
        //                         return false;
        //                 })
        //                .OrderByDescending(o => o.ProductCode).ThenBy(o => o.EffectedDate).ToList();

        //        }
        //        if (listProductParametersTmp != null && listProductParametersTmp.Count != 0)
        //        {
        //            int iCountTemp = 0;
        //            foreach (var item in listProductParametersTmp)
        //            {
        //                ProductParameterComparisonViewModel objProductParameter = new ProductParameterComparisonViewModel();
        //                iCountTemp++;
        //                objProductParameter.OrderNo = iCountTemp;
        //                objProductParameter.OrderNo = iCountTemp;
        //                objProductParameter.ProductGroupCode = item.ProductGroupCode;
        //                if (item.ProductGroupCode == ProductGroupCode.TIDE.Code)
        //                    objProductParameter.ProductGroupDisplay = ProductGroupCode.TIDE.Description;
        //                else if (item.ProductGroupCode == ProductGroupCode.CASA.Code)
        //                    objProductParameter.ProductGroupDisplay = ProductGroupCode.CASA.Description;
        //                else if (item.ProductGroupCode == ProductGroupCode.DEPOSITPENAL.Code)
        //                    objProductParameter.ProductGroupDisplay = ProductGroupCode.DEPOSITPENAL.Description;
        //                else objProductParameter.ProductGroupDisplay = "";
        //                objProductParameter.ProductCode = item.ProductCode;
        //                objProductParameter.ProductName = item.ProductName;
        //                objProductParameter.ApplyPosFlag = item.ApplyPosFlag;

        //                objProductParameter.MinInterestRateSpread = item.MinInterestRateSpread;
        //                objProductParameter.MaxInterestRateSpread = item.MaxInterestRateSpread;
        //                objProductParameter.EffectedDate = item.EffectedDate;

        //                objProductParameter.Status = item.Status;
        //                objProductParameter.StatusDesc = item.StatusDesc;
        //                objProductParameter.Remark = item.Remark;

        //                objProductParameter.CreatedBy = item.CreatedBy;
        //                objProductParameter.CreatedDate = item.CreatedDate;
        //                objProductParameter.ModifiedBy = item.ModifiedBy;
        //                objProductParameter.ModifiedDate = item.ModifiedDate;
        //                objProductParameter.ApproverBy = item.ApproverBy;
        //                objProductParameter.ApprovalDate = item.ApprovalDate;

        //                objProductParameter.CurrentApplyPos = (item.ApplyPosFlag == 1) ? "X" : "";
        //                objProductParameter.CurrentMinSpread = item.MinInterestRateSpread;
        //                objProductParameter.CurrentMaxSpread = item.MaxInterestRateSpread;
        //                objProductParameter.CurrentEffectedDate = item.EffectedDate;

        //                objProductParameter.NewApplyPos = (item.ApplyPosFlag == 1) ? "X" : "";
        //                objProductParameter.NewMinSpread = item.MinInterestRateSpread;
        //                objProductParameter.NewMaxSpread = item.MaxInterestRateSpread;
        //                objProductParameter.NewEffectedDate = item.EffectedDate;
        //                objProductParameter.NewApplyPosFlagChoice = (item.ApplyPosFlag == 1) ? true : false;
        //                item.OrderNo = iCountTemp;
        //                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
        //                {
        //                    objProductParameter.Id = 0;
        //                    objProductParameter.Remark = pRemark;
        //                    objProductParameter.EffectedDate = dEffectedDate.Date;

        //                    objProductParameter.NewApplyPos = (item.ApplyPosFlag == 1) ? "X" : "";
        //                    objProductParameter.NewMinSpread = item.MinInterestRateSpread;
        //                    objProductParameter.NewMaxSpread = item.MaxInterestRateSpread;
        //                    objProductParameter.NewEffectedDate = dEffectedDate.Date;
        //                }
        //                listProductParameters.Add(objProductParameter);
        //            }
        //        }
        //        return listProductParameters;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public List<ProductParameterComparisonViewModel> GetListProductParameters(
    string pFlagCall,
    string pProductGroupCode,
    string pEffectedDate,
    long pId = 0,
    string pProductCode = null,
    string pRemark = null)
        {
            try
            {
                DateTime effectedDt = CustConverter.StringToDate(pEffectedDate, FormatParameters.FORMAT_DATE_INT);

                // Xác định ngày hiệu lực cần lấy
                DateTime targetDate;
                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                {
                    // Lấy ngày Max <= effectedDt
                    targetDate = _dbContext.ProductParameters
                        .Where(w => w.ProductGroupCode == pProductGroupCode && w.EffectedDate <= effectedDt.Date)
                        .Select(s => s.EffectedDate)
                        .Max();
                }
                else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                {
                    targetDate = effectedDt.Date;
                }
                else
                {
                    targetDate = effectedDt.Date;
                }

                
                var query = _dbContext.ProductParameters
                    .Where(w => w.ProductGroupCode == pProductGroupCode && w.EffectedDate == targetDate);

                if (pFlagCall != EventFlag.EventFlag_Add.Value.ToString() && pFlagCall != EventFlag.EventFlag_Edit.Value.ToString())
                {
                    query = query.Where(w => (pId == 0 || w.Id == pId)
                                          && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode));

                    if (!string.IsNullOrEmpty(pRemark))
                    {
                        var remarkLower = pRemark.ToLower();
                        query = query.Where(c => c.Remark != null
                                              && (c.Remark.ToLower().Contains(remarkLower)
                                                  || Utilities.ConvertToUnSign(c.Remark.ToLower()).Contains(remarkLower)));
                    }
                }

                var listTmp = query
                    .OrderByDescending(o => o.ProductCode)
                    .ThenBy(o => o.EffectedDate)
                    .ToList();

                var result = new List<ProductParameterComparisonViewModel>();

                if (listTmp.Count == 0)
                    return result;

                int index = 0;
                foreach (var item in listTmp)
                {
                    index++;
                    var vm = new ProductParameterComparisonViewModel
                    {
                        OrderNo = index,
                        ProductGroupCode = item.ProductGroupCode,
                        ProductGroupDisplay = item.ProductGroupCode switch
                        {
                            var code when code == ProductGroupCode.TIDE.Code => ProductGroupCode.TIDE.Description,
                            var code when code == ProductGroupCode.CASA.Code => ProductGroupCode.CASA.Description,
                            var code when code == ProductGroupCode.DEPOSITPENAL.Code => ProductGroupCode.DEPOSITPENAL.Description,
                            _ => ""
                        },
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        ApplyPosFlag = item.ApplyPosFlag,
                        MinInterestRateSpread = item.MinInterestRateSpread,
                        MaxInterestRateSpread = item.MaxInterestRateSpread,
                        EffectedDate = item.EffectedDate,
                        Status = item.Status,
                        StatusDesc = item.StatusDesc,
                        Remark = item.Remark,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = item.CreatedDate,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedDate = item.ModifiedDate,
                        ApproverBy = item.ApproverBy,
                        ApprovalDate = item.ApprovalDate,
                        CurrentApplyPos = item.ApplyPosFlag == 1 ? "X" : "",
                        CurrentMinSpread = item.MinInterestRateSpread,
                        CurrentMaxSpread = item.MaxInterestRateSpread,
                        CurrentEffectedDate = item.EffectedDate,
                        NewApplyPos = item.ApplyPosFlag == 1 ? "X" : "",
                        NewMinSpread = item.MinInterestRateSpread,
                        NewMaxSpread = item.MaxInterestRateSpread,
                        NewEffectedDate = item.EffectedDate,
                        NewApplyPosFlagChoice = item.ApplyPosFlag == 1
                    };

                    // Chỉ thay đổi khi ADD
                    if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                    {
                        vm.Id = 0;
                        vm.Remark = pRemark;
                        vm.EffectedDate = effectedDt.Date;
                        vm.NewEffectedDate = effectedDt.Date;
                    }

                    result.Add(vm);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có logger
                throw;
            }
        }


        public async Task<List<ProductParameterDetailViewModel>> GetProductsForCreateAsync(string productGroupCode, DateTime effectedDate)
        {
            try
            {
                //
                var currentConfigs = GetListProductParameters(
                    pFlagCall: EventFlag.EventFlag_Add.Value.ToString(),
                    pProductGroupCode: productGroupCode,
                    pEffectedDate: effectedDate.ToString("yyyyMMdd"),
                    pId: 0,
                    pProductCode: null,
                    pRemark: null
                );

              
                var allProducts = await _dbContext.ListOfProducts
                    .Where(p => p.ProductGroupCode == productGroupCode)
                    .Select(p => new
                    {
                        p.ProductCode,
                        p.ProductName
                    })
                    .ToListAsync();

             
                var result = allProducts.Select((prod, index) =>
                {
                    var config = currentConfigs.FirstOrDefault(c => c.ProductCode == prod.ProductCode);

                    return new ProductParameterDetailViewModel
                    {
                        STT = index + 1,
                        ProductCode = prod.ProductCode,
                        ProductName = prod.ProductName,
                        ProductGroupCode = productGroupCode,

                       
                        CurrentApplyPos = (config?.ApplyPosFlag == 1) ? "X" : "",
                        CurrentApplyPosFlag = config?.ApplyPosFlag == 1,
                        CurrentMinSpread = config?.MinInterestRateSpread ?? 0.1m,
                        CurrentMaxSpread = config?.MaxInterestRateSpread ?? 5.6m,

                        NewApplyPosFlag = config?.ApplyPosFlag == 1,
                        NewMinSpread = config?.MinInterestRateSpread ?? 0.1m,
                        NewMaxSpread = config?.MaxInterestRateSpread ?? 5.6m
                    };
                }).ToList();

                _logger.LogInformation($"GetProductsForCreateAsync: Loaded {result.Count} products for group {productGroupCode} on {effectedDate:dd/MM/yyyy}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi load sản phẩm cho Create trong service");
                throw;
            }
        }

       
        //public async Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode, DateTime effectedDate)
        //{
        //    try
        //    {
        //        _logger.LogInformation($"LoadProductsForCreateAsync - Group: {productGroupCode}");

        //        var products = await _dbContext.Set<ProductParameterWithDefaultView>()
        //            .Where(v => v.ProductGroupCode == productGroupCode)
        //            .OrderBy(v => v.ProductCode)
        //            .ThenBy(v => v.ListOfProductId)       
        //            .ToListAsync();

        //        _logger.LogInformation($"View trả về {products.Count} records cho group {productGroupCode}");

        //        if (products.Count == 0)
        //        {
        //            _logger.LogWarning($"Không tìm thấy sản phẩm nào cho group {productGroupCode}");
        //            return new List<ProductParameterDetailViewModel>();
        //        }

        //        var result = products.Select((v, index) => new ProductParameterDetailViewModel
        //        {
        //            STT = index + 1,
        //            ProductGroupCode = v.ProductGroupCode,
        //            ProductCode = v.ProductCode,
        //            ProductName = v.ProductName ?? v.ProductCode ?? "",
        //            AccountTypeCode = v.AccountTypeCode ?? "",

        //            CurrentApplyPos = v.ApplyPosFlag == 1 ? "X" : "",
        //            CurrentApplyPosFlag = v.ApplyPosFlag == 1,
        //            CurrentMinSpread = v.CurrentMinSpread ?? 0m,
        //            CurrentMaxSpread = v.CurrentMaxSpread ?? 0m,

        //            NewApplyPosFlag = true,
        //            NewMinSpread = v.CurrentMinSpread ?? 0.5m,
        //            NewMaxSpread = v.CurrentMaxSpread ?? 3.0m,

        //            Remark = v.CurrentRemark ?? ""
        //        }).ToList();

        //        _logger.LogInformation($"Trả về grid {result.Count} dòng");

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Lỗi LoadProductsForCreateAsync - Group {productGroupCode}");
        //        throw;
        //    }
        //}

       


        //public async Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode, DateTime effectedDate)
        //{
        //    try
        //    {
        //        bool isPenalTide = productGroupCode == "PENAL" ||
        //                          productGroupCode == "DEPOSITPENAL" ||
        //                          productGroupCode == "PENALTIDE";

        //        string actualGroupCode = productGroupCode;

        //        if (isPenalTide)
        //        {
        //            // Kiểm tra xem đã có dữ liệu DEPOSITPENAL chưa
        //            var penalCount = await _dbContext.ProductParameters
        //                .CountAsync(p => p.ProductGroupCode == "DEPOSITPENAL");

        //            if (penalCount > 0)
        //            {
        //                actualGroupCode = "DEPOSITPENAL";
        //                _logger.LogInformation($"✅ Penal Tide: Tìm thấy {penalCount} records → Load từ DEPOSITPENAL (bản mới nhất)");
        //            }
        //            else
        //            {
        //                actualGroupCode = "TIDE";
        //                _logger.LogInformation("Penal Tide lần đầu → Fallback load từ TIDE");
        //            }
        //        }

        //        _logger.LogInformation($"Final Load - Input: {productGroupCode} | Actual: {actualGroupCode}");

        //        // Load dữ liệu
        //        List<ProductParameter> dataList;

        //        if (actualGroupCode == "DEPOSITPENAL")
        //        {
        //            // Load từ bảng ProductParameters và lấy bản mới nhất
        //            dataList = await _dbContext.ProductParameters
        //                .Where(p => p.ProductGroupCode == "DEPOSITPENAL")
        //                .ToListAsync();

        //            dataList = dataList
        //                .GroupBy(p => new { p.ProductCode, p.ProductName })
        //                .Select(g => g.OrderByDescending(p => p.EffectedDate).FirstOrDefault())
        //                .Where(x => x != null)
        //                .OrderBy(x => x.ProductCode)
        //                .ToList();
        //        }
        //        else
        //        {
        //            // Load từ View cho CASA và TIDE
        //            dataList = await _dbContext.Set<ProductParameterWithDefaultView>()
        //                .Where(v => v.ProductGroupCode == actualGroupCode)
        //                .Select(v => new ProductParameter
        //                {
        //                    ProductCode = v.ProductCode,
        //                    ProductName = v.ProductName,
        //                    ApplyPosFlag = v.ApplyPosFlag ?? 0,
        //                    MinInterestRateSpread = v.CurrentMinSpread ?? 0,
        //                    MaxInterestRateSpread = v.CurrentMaxSpread ?? 0,
        //                    Remark = v.CurrentRemark
        //                })
        //                .ToListAsync();
        //        }

        //        var result = dataList.Select((p, index) => new ProductParameterDetailViewModel
        //        {
        //            STT = index + 1,
        //            ProductGroupCode = productGroupCode,   // Giữ nguyên để hiển thị đúng tên "Penal Tide"
        //            ProductCode = p.ProductCode,
        //            ProductName = p.ProductName ?? "",

        //            CurrentApplyPos = p.ApplyPosFlag == 1 ? "X" : "",
        //            CurrentApplyPosFlag = p.ApplyPosFlag == 1,
        //            CurrentMinSpread = p.MinInterestRateSpread,
        //            CurrentMaxSpread = p.MaxInterestRateSpread,

        //            NewApplyPosFlag = true,
        //            NewMinSpread = p.MinInterestRateSpread,
        //            NewMaxSpread = p.MaxInterestRateSpread,
        //            Remark = p.Remark ?? ""
        //        }).ToList();

        //        _logger.LogInformation($"Trả về grid {result.Count} dòng cho {productGroupCode}");
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Lỗi LoadProductsForCreateAsync - {productGroupCode}");
        //        throw;
        //    }
        //}

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="productGroupCode"></param>
        /// <param name="effectedDate"></param>
        /// <returns></returns>

       

        public async Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode, DateTime effectedDate)
        {
            try
            {
                string displayGroup = productGroupCode;  
                string loadGroup = productGroupCode;      

                // Xử lý Penal Tide
                if (productGroupCode == "PENAL" || productGroupCode == "DEPOSITPENAL" || productGroupCode == "PENALTIDE")
                {
                    var hasPenalData = await _dbContext.ProductParameters
                        .AnyAsync(p => p.ProductGroupCode == "DEPOSITPENAL");

                    loadGroup = hasPenalData ? "DEPOSITPENAL" : "TIDE";
                    _logger.LogInformation($"Penal Tide → Load từ {loadGroup}");
                }

                _logger.LogInformation($"Load - Display: {displayGroup} | Query: {loadGroup}");

                // ==================== LOAD DỮ LIỆU ====================
                List<ProductParameterDetailViewModel> result;

                if (loadGroup == "DEPOSITPENAL")
                {
                    // Load Penal Tide 
                    var rawData = await _dbContext.ProductParameters
                        .Where(p => p.ProductGroupCode == "DEPOSITPENAL")
                        .ToListAsync();

                    var latestData = rawData
                        .GroupBy(p => new { p.ProductCode, p.ProductName })
                        .Select(g => g.OrderByDescending(x => x.EffectedDate).FirstOrDefault())
                        .Where(x => x != null)
                        .OrderBy(x => x.ProductCode)
                        .ToList();

                    result = CreateDetail(latestData, displayGroup);
                }
                else
                {
                    // CASA và TIDE 
                    var viewData = await _dbContext.Set<ProductParameterWithDefaultView>()
                        .Where(v => v.ProductGroupCode == loadGroup)
                        .ToListAsync();

                    var latestData = viewData
                        .GroupBy(v => new { v.ProductCode, v.ProductName })
                        .Select(g => g.OrderByDescending(v => v.CurrentEffectedDate ?? DateTime.MinValue).FirstOrDefault())
                        .Where(x => x != null)
                        .OrderBy(x => x.ProductCode)
                        .ToList();

                    result = CreateDetailFromView(latestData, displayGroup);
                }

                _logger.LogInformation($"Trả về grid {result.Count} dòng cho {displayGroup}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi Load - {productGroupCode}");
                throw;
            }
        }

        // 
        private List<ProductParameterDetailViewModel> CreateDetail(List<ProductParameter> list, string displayGroup)
        {
            return list.Select((p, i) => new ProductParameterDetailViewModel
            {
                STT = i + 1,
                ProductGroupCode = displayGroup,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName ?? "",
                CurrentApplyPos = p.ApplyPosFlag == 1 ? "X" : "",
                CurrentApplyPosFlag = p.ApplyPosFlag == 1,
                CurrentMinSpread = p.MinInterestRateSpread,
                CurrentMaxSpread = p.MaxInterestRateSpread,
                NewApplyPosFlag = true,
                NewMinSpread = p.MinInterestRateSpread,
                NewMaxSpread = p.MaxInterestRateSpread,
                Remark = p.Remark ?? ""
            }).ToList();
        }

        //
        private List<ProductParameterDetailViewModel> CreateDetailFromView(List<ProductParameterWithDefaultView> list, string displayGroup)
        {
            return list.Select((v, i) => new ProductParameterDetailViewModel
            {
                STT = i + 1,
                ProductGroupCode = displayGroup,
                ProductCode = v.ProductCode,
                ProductName = v.ProductName ?? "",
                CurrentApplyPos = v.ApplyPosFlag == 1 ? "X" : "",
                CurrentApplyPosFlag = v.ApplyPosFlag == 1,
                CurrentMinSpread = v.CurrentMinSpread ?? 0m,
                CurrentMaxSpread = v.CurrentMaxSpread ?? 0m,
                NewApplyPosFlag = true,
                NewMinSpread = v.CurrentMinSpread ?? 0.5m,
                NewMaxSpread = v.CurrentMaxSpread ?? 3.0m,
                Remark = v.CurrentRemark ?? ""
            }).ToList();
        }
        // Thay đổi khi lưu trùng ngày hiệu lực 

       

       



        public async Task<int> SaveBatchProductParameterAsync(string productGroupCode, DateTime effectedDate, string remark, List<ProductParameterDetailViewModel> items)
        {
            try
            {
                if (string.IsNullOrEmpty(productGroupCode))
                    throw new ArgumentException("Vui lòng chọn phân loại");

                if (effectedDate <= DateTime.Today)
                    throw new ArgumentException("Ngày hiệu lực phải lớn hơn hôm nay");

                if (items == null || items.Count == 0)
                    throw new ArgumentException("Không có dữ liệu thay đổi để lưu");

                _logger.LogInformation($"[Save] Bắt đầu - Group: {productGroupCode}, Ngày: {effectedDate:dd/MM/yyyy}, Số items: {items.Count}");

                // Kiểm tra trùng ngày hiệu lực theo ProductGroupCode
                var existing = await _dbContext.ProductParameters
                    .AnyAsync(x => x.ProductGroupCode == productGroupCode
                                && x.EffectedDate.Date == effectedDate.Date);

                if (existing)
                    throw new Exception($"Đã tồn tại cấu hình cho phân loại {productGroupCode} với ngày hiệu lực {effectedDate:dd/MM/yyyy}. Không thể tạo trùng.");

                // Lấy bản cũ gần nhất của group này
                var maxEffectedDate = await _dbContext.ProductParameters
                    .Where(x => x.ProductGroupCode == productGroupCode)
                    .MaxAsync(x => (DateTime?)x.EffectedDate);

                var lstOldParameter = new List<ProductParameter>();
                if (maxEffectedDate.HasValue)
                {
                    lstOldParameter = await _dbContext.ProductParameters
                        .Where(x => x.ProductGroupCode == productGroupCode
                                 && x.EffectedDate.Date == maxEffectedDate.Value.Date)
                        .ToListAsync();
                }

                var lstNewParameter = new List<ProductParameter>();

                foreach (var old in lstOldParameter)
                {
                    // Tìm theo ProductCode + ProductName
                    var updatedItem = items.FirstOrDefault(item =>
                        item.ProductCode == old.ProductCode &&
                        item.ProductName == old.ProductName);

                    if (updatedItem != null)
                    {
                        // Có thay đổi → lưu mới
                        lstNewParameter.Add(new ProductParameter
                        {
                            ProductGroupCode = productGroupCode,           //
                            ProductCode = updatedItem.ProductCode,
                            ProductName = updatedItem.ProductName ?? old.ProductName ?? "",
                            AccountTypeCode = updatedItem.AccountTypeCode ?? old.AccountTypeCode ?? "",
                            ApplyPosFlag = updatedItem.NewApplyPosFlag ? 1 : 0,
                            MinInterestRateSpread = updatedItem.NewMinSpread,
                            MaxInterestRateSpread = updatedItem.NewMaxSpread,
                            EffectedDate = effectedDate.Date,
                            Remark = !string.IsNullOrEmpty(updatedItem.Remark) ? updatedItem.Remark.Trim() : remark?.Trim() ?? "",
                            Status = ConfigStatus.MAKER.Value,
                            CreatedBy = "system",
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        // Không thay đổi → copy từ bản cũ
                        lstNewParameter.Add(new ProductParameter
                        {
                            ProductGroupCode = old.ProductGroupCode,
                            ProductCode = old.ProductCode,
                            ProductName = old.ProductName,
                            AccountTypeCode = old.AccountTypeCode,
                            ApplyPosFlag = old.ApplyPosFlag,
                            MinInterestRateSpread = old.MinInterestRateSpread,
                            MaxInterestRateSpread = old.MaxInterestRateSpread,
                            EffectedDate = effectedDate.Date,
                            Remark = remark?.Trim() ?? "",
                            Status = ConfigStatus.MAKER.Value,
                            CreatedBy = "system",
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                // Trường hợp lần đầu tiên (chưa có dữ liệu cũ)
                if (lstOldParameter.Count == 0)
                {
                    foreach (var item in items)
                    {
                        lstNewParameter.Add(new ProductParameter
                        {
                            ProductGroupCode = productGroupCode,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName ?? "",
                            AccountTypeCode = item.AccountTypeCode ?? "",
                            ApplyPosFlag = item.NewApplyPosFlag ? 1 : 0,
                            MinInterestRateSpread = item.NewMinSpread,
                            MaxInterestRateSpread = item.NewMaxSpread,
                            EffectedDate = effectedDate.Date,
                            Remark = item.Remark?.Trim() ?? remark?.Trim() ?? "",
                            Status = ConfigStatus.MAKER.Value,
                            CreatedBy = "system",
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                _dbContext.ProductParameters.AddRange(lstNewParameter);
                var recordCount = await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"[Save] Thành công {recordCount} bản ghi cho {productGroupCode}");

                return recordCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi SaveBatchProductParameterAsync");
                throw;
            }
        }







        


        public async Task<List<ProductParametersView>> GetProductParametersViewListAsync( string productGroupCode = null,string productCode = null, DateTime? fromDate = null,     DateTime? toDate = null)        // Đến ngày
        {
            var query = _dbContext.Set<ProductParametersView>().AsQueryable();

            // Lọc theo Phân loại
            if (!string.IsNullOrEmpty(productGroupCode))
                query = query.Where(x => x.ProductGroupCode == productGroupCode);

            // Lọc theo Mã SP
            if (!string.IsNullOrEmpty(productCode))
                query = query.Where(x => x.ProductCodeList.Contains(productCode));

            // Lọc theo khoảng Ngày hiệu lực
            if (fromDate.HasValue)
                query = query.Where(x => x.EffectedDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.EffectedDate.Date <= toDate.Value.Date);

            var data = await query
                .OrderBy(x => x.OrderNo)
                .ThenByDescending(x => x.EffectedDate)
                .ToListAsync();

            Console.WriteLine($"Service returned {data.Count} records for Group={productGroupCode}, From={fromDate:dd/MM/yyyy}, To={toDate:dd/MM/yyyy}");

            return data;
        }








    }
}