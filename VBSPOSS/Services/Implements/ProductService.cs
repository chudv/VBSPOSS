using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger; // Thêm logger

        public ProductService(IHttpClientFactory httpClientFactory, ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CasaApiClient"); // Sử dụng CasaApiClient
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Inject logger
        }

        public List<SelectListItem> GetAccountTypes(string productCode)
        {
            var query = _context.ListOfProducts.Where(x => x.Status == 1); // Chỉ lấy bản ghi có Status = 1

            if (!string.IsNullOrEmpty(productCode))
                query = query.Where(x => x.ProductCode == productCode);

            var result = query.Select(at => new SelectListItem
            {
                Value = at.AccountTypeCode,
                Text = at.AccountTypeName
            }).Distinct().ToList();

            return result;


            //return _context.ListOfProducts.Where(p => p.ProductCode == productCode) 
            //    .Select(at => new SelectListItem
            //    {
            //        Value = at.AccountTypeCode,
            //        Text = at.AccountTypeName
            //    })
            //    .Distinct()
            //    .ToList();
        }

        public async Task<List<AddCasaProductViewModel>> GetCasaGridDataFromApi(string productCode)
        {
            _logger.LogInformation($"Starting GetCasaGridDataFromApi with productCode: {productCode}");
            var casaApiSettings = new
            {
                userId = "IDCADMIN",
                productCode = productCode,
                currency = "VND",
                effectiveDate = DateTime.Today.ToString("yyyyMMdd"),
                debitCreditFlag = "C", // Mặc định là 'C'
                posCode = "0"
            };

            _logger.LogInformation($"Request data: {JsonConvert.SerializeObject(casaApiSettings)}");
            var content = new StringContent(JsonConvert.SerializeObject(casaApiSettings), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("vbsp/internal/api/v1/getCasaIntRts", content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Raw API Response: {jsonResponse}");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

            if (apiResponse?.txnStatus == "Success" && apiResponse.record != null)
            {
                var accountTypes = GetAccountTypes("");
                var accountSubTypes = GetAccountSubTypes("");
                var models = apiResponse.record.Select(item => new AddCasaProductViewModel
                {
                    ProductCode = item.prodCode ?? string.Empty,
                    ProductName = item.prodCode ?? string.Empty,
                    AccountTypeCode = item.accountType ?? string.Empty,
                    AccountTypeName = GetAccountTypeName(item.accountType, accountTypes),
                    AccountSubTypeCode = item.subType ?? string.Empty,
                    CurrencyCode = item.currency ?? "VND",
                    EffectiveDate = DateTime.TryParse(item.effectiveDate, out var effDate) ? effDate : (DateTime?)null,
                    InterestRate = decimal.TryParse(item.interestRate, out var interest) ? interest : 0m,
                    NewInterestRate = null,
                    ExpiredDate = string.IsNullOrEmpty(item.posRateExpiryDate) ? (DateTime?)null : DateTime.TryParse(item.posRateExpiryDate, out var expDate) ? expDate : (DateTime?)null,
                    CircularRefNum = item.circularRef ?? string.Empty,
                    PenalRate = decimal.TryParse(item.penalRate, out var penal) ? penal : 0m,
                    Id = 0,
                    DebitCreditFlag = item.debitCreditFlag ?? "C", // Mặc định 'C' nếu API không cung cấp
                    AccountTypes = accountTypes,
                    AccountSubTypes = accountSubTypes
                }).ToList();
                _logger.LogInformation($"Mapped models: {JsonConvert.SerializeObject(models)}");
                return models;
            }
            _logger.LogWarning($"No data or unsuccessful response for productCode {productCode}. Response: {jsonResponse}");
            return new List<AddCasaProductViewModel>();
        }

        private string GetAccountTypeName(string accountType, List<SelectListItem> accountTypes)
        {
            var accountTypeItem = accountTypes.FirstOrDefault(at => at.Value == accountType);
            return accountTypeItem?.Text ?? "0000";
        }

        public List<SelectListItem> GetProductGroups()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "CASA", Text = "Tiền gửi không kỳ hạn" },
                new SelectListItem { Value = "TIDE", Text = "Tiền gửi tiết kiệm có kỳ hạn" },
                new SelectListItem { Value = "DEPOSITPENAL", Text = "Lãi suất rút trước hạn Tiền gửi tiết kiệm có kỳ hạn" },
            };
        }

        public List<Product> GetProductList(string productGroupCode)
        {
            return _context.ListOfProducts.Where(p => p.ProductGroupCode == productGroupCode)
                .Select(p => new Product
                {
                    ProductGroupCode = p.ProductGroupCode,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    // AccountTypeCode = p.AccountTypeCode,
                    // AccountTypeName = p.AccountTypeName,
                    // AccountSubTypeCode = p.AccountSubTypeCode,
                    CurrencyCode = p.CurrencyCode,
                    // DebitCreditFlag = p.DebitCreditFlag,
                    //EffectiveDate = p.EffectiveDate,
                    //InterestRate = p.InterestRate
                }).Distinct().ToList();
        }


        public List<ListOfProducts> GetFullProductList(string productGroupCode, string depositType, string customerType, string userPosCode, int userGrade)
        {

            var query = _context.ListOfProducts
                    .Where(x => x.Status == StatusValue.ACTIVE.Value);

            if (!string.IsNullOrEmpty(productGroupCode))
                query = query.Where(x => x.ProductGroupCode == productGroupCode);

            if (!string.IsNullOrEmpty(depositType))
                query = query.Where(x => x.DepositeType == depositType);

            if (!string.IsNullOrEmpty(customerType) && customerType != CustomerSegmentType.ALL)
                query = query.Where(x => x.ApplyCustomerType == customerType);

            var lstProduct = query.ToList();
            List<ListOfProducts> result = new List<ListOfProducts>();

            if (userGrade != PosGrade.HEAD_POS)
            {
                foreach (var t in lstProduct)
                {
                    var productParameter = GetProductParameter(ProductGroupCode.ProductGroupCode_Tide, t.ProductCode, null);
                    if (productParameter != null && productParameter.ApplyPosFlag == 1)
                    {
                        result.Add(t);
                    }
                }
            }
            else
            {
                result = lstProduct;
            }

            return result;
        }

        public List<SelectListItem> GetAccountSubTypes(string accountType)
        {
            return new List<SelectListItem> { new SelectListItem { Value = "0", Text = "0" } };

        }

        public List<Product> GetProductByCode(string productCode)
        {
            return _context.ListOfProducts.Where(p => p.ProductCode == productCode)
                .Select(p => new Product
                {
                    ProductGroupCode = p.ProductGroupCode,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    AccountTypeCode = p.AccountTypeCode,
                    AccountTypeName = p.AccountTypeName,
                    AccountSubTypeCode = p.AccountSubTypeCode,
                    CurrencyCode = p.CurrencyCode,
                    DebitCreditFlag = p.DebitCreditFlag,
                    EffectiveDate = p.EffectiveDate,
                    InterestRate = p.InterestRate
                }).ToList();
        }



        public List<DepositTermModel> GetDepositTerms(string termType, int termBasis, string inclusionFlag)
        {
            var list = new List<DepositTermModel>();
            string termUnitDesc = "";
            switch (termType)
            {
                case "M":
                    termUnitDesc = "Tháng";
                    break;
                case "Q":
                    termUnitDesc = "Quý";
                    break;
                case "Y":
                    termUnitDesc = "Năm";
                    break;
            }

            for (int i = 0; i < 60; i++)
            {
                string termDesc = "";
                int termValue = 0;
                int minTermValue = 0;   

                if (i == 0)
                {
                    if (inclusionFlag == "INCLUSIVE")
                    {
                        termDesc = $"Nhỏ hơn hoặc bằng 1 {termUnitDesc}";
                    }    
                    else
                    {
                        termDesc = $"Nhỏ hơn 1 {termUnitDesc}";
                    }
                    termValue = 1;
                    minTermValue = 1;
                }
                else
                {
                    if (termBasis == 1)
                    {
                        if (inclusionFlag == "INCLUSIVE")
                        {
                            termDesc = $"Lớn hơn {(i - 1) * termBasis+1 } {termUnitDesc} Và Nhỏ hơn hoặc bằng {i * termBasis+1} {termUnitDesc}";
                        }
                        else
                        {
                            termDesc = $"Lớn hơn hoặc bằng {(i - 1) * termBasis+1} {termUnitDesc} Và Nhỏ hơn {i * termBasis+1} {termUnitDesc}";
                        }
                        termValue = i * termBasis + 1;
                        minTermValue = (i - 1) * termBasis + 1;
                    }
                    else
                    {
                        if (inclusionFlag == "INCLUSIVE")
                        {
                            termDesc = $"Lớn hơn {Math.Max((i - 1) * termBasis, 1)} {termUnitDesc} Và Nhỏ hơn hoặc bằng {i * termBasis} {termUnitDesc}";
                        }
                        else
                        {
                            termDesc = $"Lớn hơn hoặc bằng {Math.Max((i - 1) * termBasis, 1)} {termUnitDesc} Và Nhỏ hơn {i * termBasis} {termUnitDesc}";
                        }
                        termValue = i * termBasis;
                        minTermValue = Math.Max((i - 1) * termBasis, 1);
                    }
                    
                    
                }

                var model = new DepositTermModel
                {
                    Id = i+1,
                    TermCode = i.ToString(),
                    TermDesc = termDesc,
                    TermUnitCode = termType,
                    TermUnitName = termUnitDesc,
                    TermValue = termValue,
                    InclusionFlag = inclusionFlag,
                    MinTermValue = minTermValue
                };

                list.Add(model);
            }

            return list;
        }


        /// <summary>
        /// Lấy danh sách cấu hình sản phẩm (ProductParameter) theo productCode và effectedDate
        /// </summary>
        /// <param name="productGroupCode">Loại sản phẩm để ánh xạ vào bảng DL tham số cấu hình ProductParameters: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="productCode"></param>
        /// <param name="effectedDate"></param>
        /// <returns></returns>
        public ProductParameter GetProductParameter(string productGroupCode, string productCode, DateTime? effectedDate)
        {
            try
            {
                var _maxEffectedDate = _context.ProductParameters
                    .Where(w => w.ProductCode == productCode &&
                                (effectedDate == null || w.EffectedDate <= effectedDate) &&
                                w.Status == ConfigStatus.AUTHORIZED.Value).Select(s => s.EffectedDate).DefaultIfEmpty().Max();

                if (_maxEffectedDate == null)
                {
                    return null;
                }
                else
                {
                    var _result = _context.ProductParameters
                        .Where(w => w.ProductCode == productCode && w.EffectedDate == _maxEffectedDate &&
                                    w.Status == ConfigStatus.AUTHORIZED.Value).FirstOrDefault();
                    return _result;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Hàm lấy danh sách thông tin cấu hình thông số cho sản phẩm: Cờ áp dụng POS; LS điều chỉnh tối thiểu và tối đa => Lấy từ bảng ProductParameters
        /// </summary>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pProductCode">Mã sản phẩm</param>
        /// <param name="pEffectedDate">Ngày hiệu lực</param>
        /// <param name="pStatus">Trạng thái (Không bắt buộc). Nếu truyền -1 lấy tất; 1: Bản ghi mở; 3: Bản ghi được phê duyệt</param>
        /// <returns>Danh sách thông tin cấu hình thông số cho sản phẩm</returns>
        public List<ProductParameter> GetListProductParametersSearch(string pProductGroupCode, string pProductCode, DateTime? pEffectedDate, int pStatus)
        {
            try
            {
                var dMaxEffectedDate = _context.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
                               && (pEffectedDate == null || w.EffectedDate <= pEffectedDate)
                               && (pStatus == -1 || w.Status == pStatus)).Select(s => s.EffectedDate).DefaultIfEmpty().Max();

                List<ProductParameter> listProductParameters = new List<ProductParameter>();
                if (dMaxEffectedDate != null)
                {
                    listProductParameters = _context.ProductParameters.Where(w => w.ProductGroupCode == pProductGroupCode
                                                    && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode)
                                                    && (w.EffectedDate == dMaxEffectedDate.Date) && (pStatus == -1 || w.Status == pStatus))
                                            .OrderBy(o => o.ProductCode).ThenByDescending(o => o.EffectedDate).ToList();
                }
                return listProductParameters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}