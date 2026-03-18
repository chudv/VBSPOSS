using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
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

        //public async Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode)
        //{
        //    try
        //    {
        //        _logger.LogInformation($"Bắt đầu load cho group: {productGroupCode}");

        //        var configs = await _dbContext.ProductParameters
        //            .Where(p => p.ProductGroupCode == productGroupCode)
        //            .GroupBy(p => p.ProductCode)
        //            .Select(g => g.OrderByDescending(p => p.EffectedDate).FirstOrDefault())
        //            .ToListAsync();

        //        _logger.LogInformation($"Query DB trả về {configs.Count} config cho group {productGroupCode}");

        //        if (configs.Count == 0)
        //        {
        //            // Log thêm để debug
        //            var allGroups = await _dbContext.ProductParameters.Select(p => p.ProductGroupCode).Distinct().ToListAsync();
        //            _logger.LogWarning($"Không tìm thấy config. Các group có trong DB: {string.Join(", ", allGroups)}");
        //        }

        //        var result = configs.Select((config, index) => new ProductParameterDetailViewModel
        //        {
        //            STT = index + 1,
        //            ProductCode = config.ProductCode,
        //            ProductName = config.ProductName ?? config.ProductCode,
        //            ProductGroupCode = productGroupCode,
        //            CurrentApplyPos = (config.ApplyPosFlag == 1) ? "X" : "",
        //            CurrentApplyPosFlag = config.ApplyPosFlag == 1,
        //            CurrentMinSpread = config.MinInterestRateSpread,
        //            CurrentMaxSpread = config.MaxInterestRateSpread,
        //            NewApplyPosFlag = config.ApplyPosFlag == 1,
        //            NewMinSpread = config.MinInterestRateSpread,
        //            NewMaxSpread = config.MaxInterestRateSpread
        //        }).ToList();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi load trong service");
        //        throw;
        //    }
        //}



        public async Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode, DateTime effectedDate)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu load cho group: {productGroupCode}, EffectedDate: {effectedDate:yyyy-MM-dd}");

                var configs = await _dbContext.ProductParameters
                    .Where(p => p.ProductGroupCode == productGroupCode
                             && p.EffectedDate <= effectedDate.Date)  // 
                    .GroupBy(p => p.ProductCode)
                    .Select(g => g.OrderByDescending(p => p.EffectedDate).FirstOrDefault())
                    .ToListAsync();

                _logger.LogInformation($"Query DB trả về {configs.Count} config cho group {productGroupCode}");

                if (configs.Count == 0)
                {
                    var allGroups = await _dbContext.ProductParameters.Select(p => p.ProductGroupCode).Distinct().ToListAsync();
                    _logger.LogWarning($"Không tìm thấy config. Các group có trong DB: {string.Join(", ", allGroups)}");
                }

                var result = configs.Select((config, index) => new ProductParameterDetailViewModel
                {
                    STT = index + 1,
                    ProductCode = config.ProductCode,
                    ProductName = config.ProductName ?? config.ProductCode,
                    ProductGroupCode = productGroupCode,
                    CurrentApplyPos = (config.ApplyPosFlag == 1) ? "X" : "",
                    CurrentApplyPosFlag = config.ApplyPosFlag == 1,
                    CurrentMinSpread = config.MinInterestRateSpread,
                    CurrentMaxSpread = config.MaxInterestRateSpread,
                    NewApplyPosFlag = config.ApplyPosFlag == 1,
                    NewMinSpread = config.MinInterestRateSpread,
                    NewMaxSpread = config.MaxInterestRateSpread,
                    Remark = config.Remark ?? ""
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi load trong service");
                throw;
            }
        }

        // lưu
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

                var recordsToSave = new List<ProductParameter>();

                foreach (var item in items)
                {
                    // Ưu tiên Remark riêng trong grid, fallback Remark chung từ form
                    var finalRemark = !string.IsNullOrEmpty(item.Remark) ? item.Remark : remark?.Trim() ?? "";

                    var entity = new ProductParameter
                    {
                        ProductGroupCode = productGroupCode,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName ?? "",
                        ApplyPosFlag = item.NewApplyPosFlag ? 1 : 0,
                        MinInterestRateSpread = item.NewMinSpread,
                        MaxInterestRateSpread = item.NewMaxSpread,
                        EffectedDate = effectedDate.Date,
                        Remark = finalRemark,
                        Status = ConfigStatus.MAKER.Value, // 1 - Tạo lập
                       // StatusDesc = ConfigStatus.MAKER.Description ?? "Tạo lập",
                        CreatedBy = "system", // Lấy từ session hoặc User nếu có
                        CreatedDate = DateTime.Now
                    };

                    recordsToSave.Add(entity);
                }

                _dbContext.ProductParameters.AddRange(recordsToSave);
                var recordCount = await _dbContext.SaveChangesAsync();

                return recordCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lưu batch trong service");
                throw ex;
            }
        }

        //Load màn Index
     //   public async Task<List<ProductParametersView>> GetProductParametersViewListAsync(
     //string productGroupCode, string productCode, DateTime? effectDate)
     //   {
     //       var query = _dbContext.Set<ProductParametersView>().AsQueryable();

     //       if (!string.IsNullOrEmpty(productGroupCode))
     //           query = query.Where(x => x.ProductGroupCode == productGroupCode);

     //       if (!string.IsNullOrEmpty(productCode))
     //           query = query.Where(x => x.ProductCodeList.Contains(productCode));

     //       if (effectDate.HasValue)
     //           query = query.Where(x => x.EffectedDate.Date == effectDate.Value.Date);

     //       return await query
     //           .OrderBy(x => x.OrderNo)
     //           .ThenByDescending(x => x.EffectedDate)
     //           .ToListAsync();
     //   }



        public async Task<List<ProductParametersView>> GetProductParametersViewListAsync(
    string productGroupCode, string productCode, DateTime? effectDate)
        {
            var query = _dbContext.Set<ProductParametersView>().AsQueryable();

            if (!string.IsNullOrEmpty(productGroupCode))
                query = query.Where(x => x.ProductGroupCode == productGroupCode);

            if (!string.IsNullOrEmpty(productCode))
                query = query.Where(x => x.ProductCodeList.Contains(productCode));

            // if (effectDate.HasValue)
            //     query = query.Where(x => x.EffectedDate.Date == effectDate.Value.Date);

            var data = await query
                .OrderBy(x => x.OrderNo)
                .ThenByDescending(x => x.EffectedDate)
                .ToListAsync();

            
            Console.WriteLine($"Service returned {data.Count} records"); // hoặc dùng ILogger
            return data;
        }


    }
}