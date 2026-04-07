using AutoMapper;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VBSPOSS.Constants;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;

namespace VBSPOSS.Integration.Implements
{
    public class ApiInternalEsbService : IApiInternalEsbService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _clientInternalEsb;
        private readonly ILogger<ApiInternalEsbService> _logger; // Added for logging

        public ApiInternalEsbService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiInternalEsbService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            //_clientInternalEsb = new HttpClient(new HttpClientHandler { UseDefaultCredentials = false })
            //{
            //    BaseAddress = new Uri(_configuration["VBSPInternalEsbAPI"] ?? throw new ArgumentNullException("VBSPInternalEsbAPI configuration is missing")),
            //    Timeout = TimeSpan.FromSeconds(30) // Set a reasonable timeout
            //};
            //_clientInternalEsb.DefaultRequestHeaders.Accept.Clear();
            //_clientInternalEsb.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _clientInternalEsb = httpClientFactory.CreateClient("InternalEsbClient");
        }

        public async Task<GenericListResultJava<TideIntRateResposeViewModel>> GetListDepositInterestRate(TideIntRateRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting GetListDepositInterestRate with input: {Input}", JsonConvert.SerializeObject(requestInput));

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/getDepInterestRate");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/getDepInterestRate", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var result = JsonConvert.DeserializeObject<GenericListResultJava<TideIntRateResposeViewModel>>(responseContent);

                return result ?? GenericListResultJava<TideIntRateResposeViewModel>.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                return GenericListResultJava<TideIntRateResposeViewModel>.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                return GenericListResultJava<TideIntRateResposeViewModel>.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericListResultJava<TideIntRateResposeViewModel>.Fail($"An unexpected error occurred (Có lỗi xẩy ra): {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm lấy danh sách thông tin lãi suất của loại tài khoản của sản phẩm CASA ở thời điểm gần nhất với ngày hiệu lực truyền vào.
        /// Gọi đến API ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/getCasaIntRts
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "posCode": "0", "effectDate": "20210101", "sourceId": "MB", "userId": "IDCADMIN", "prodCode": "431" }</param>
        /// <returns>Danh sách bản tin</returns>
        public async Task<GenericListRecordJava<CasaIntRateReposeViewModel>> GetListCasaInterestRate(CasaIntRateRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting GetListCasaInterestRate with input: {Input}", JsonConvert.SerializeObject(requestInput));

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/getCasaIntRts");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/getCasaIntRts", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var result = JsonConvert.DeserializeObject<GenericListRecordJava<CasaIntRateReposeViewModel>>(responseContent);

                return result ?? GenericListRecordJava<CasaIntRateReposeViewModel>.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return GenericListRecordJava<CasaIntRateReposeViewModel>.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return GenericListRecordJava<CasaIntRateReposeViewModel>.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericListRecordJava<CasaIntRateReposeViewModel>.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm lấy danh sách thông tin lãi suất của sản phẩm Tide rút trước hạn
        /// Gọi đến API ESB: http://10.63.54.52:7003/vbsp/internal/api/v1/DepPenalIntRt
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "userId": "IDCADMIN", "productCode": "431", "currencyCode": "VND", "effDate": "20250520", "posCode": "0" }</param>
        /// <returns>Danh sách bản tin</returns>
        public async Task<GenericListRecordJava<DepositPenalIntRateReposeViewModel>> GetListDepositPenalInterestRate(DepositPenalIntRateRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting GetListDepositPenalInterestRate with input: {Input}", JsonConvert.SerializeObject(requestInput));

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/DepPenalIntRt");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/DepPenalIntRt", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var result = JsonConvert.DeserializeObject<GenericListRecordJava<DepositPenalIntRateReposeViewModel>>(responseContent);

                return result ?? GenericListRecordJava<DepositPenalIntRateReposeViewModel>.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return GenericListRecordJava<DepositPenalIntRateReposeViewModel>.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return GenericListRecordJava<DepositPenalIntRateReposeViewModel>.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericListRecordJava<DepositPenalIntRateReposeViewModel>.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi API casaIntRates cập nhật thông tin cấu hình lãi suất Casa vào iDC thông qua API 
        /// http://10.63.54.52:7003/vbsp/internal/api/v1/casaIntRates
        /// </summary>
        /// <param name="requestInput">Thông tin cấu hình lãi suất</param>
        /// <returns>Kết quả trả về. Ex: { "respRecord": [ { "txnStatus": "SUCCESS", "reqRecordSl": "1", "responseCode": "00000", "responseMsg": " " } ] }</returns>
        //public async Task<GenericStatusListResult> CasaIntRates(CasaIntRatesRequestViewModel requestInput)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Starting CasaIntRates with input: {Input}", JsonConvert.SerializeObject(requestInput));
        //        if (requestInput == null || requestInput.InterestRates?.Record == null || !requestInput.InterestRates.Record.Any())
        //        {
        //            _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
        //            return GenericStatusListResult.Fail("Invalid input data");
        //        }
        //        int iCountTmp = 0;
        //        foreach (var itemRate in requestInput.InterestRates.Record)
        //        {
        //            iCountTmp++;
        //            if (string.IsNullOrEmpty(itemRate.AccountType) || string.IsNullOrEmpty(itemRate.AccountSubType) || itemRate.AccountType.Length != 6)
        //            {
        //                return GenericStatusListResult.Fail($"Invalid input data. Loại tài khoản hoặc loại tài khoản phụ không hợp lệ (Dòng [{iCountTmp.ToString()}] có giá trị AccountType: [{itemRate.AccountType}] và AccountSubType: [{itemRate.AccountSubType}]");
        //            }
        //        }
        //        _logger.LogInformation("Starting CasaIntRates with UserId: {UserId}, BankCircularRefNum: {BankCircularRefNum}", requestInput.UserId, requestInput.BankCircularRefNum);

        //        // Serialize input object to JSON
        //        var json = JsonConvert.SerializeObject(requestInput);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");
        //        _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/casaIntRates");

        //        // Send POST request
        //        var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/casaIntRates", content);

        //        // Ensure the response is successful
        //        response.EnsureSuccessStatusCode();

        //        // Read and deserialize response
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        _logger.LogDebug("Received response: {ResponseContent}", responseContent);

        //        var resultUpdate = JsonConvert.DeserializeObject<GenericStatusListResult>(responseContent);
        //        if (resultUpdate != null && resultUpdate.StatusList.Any() && resultUpdate.StatusList.Count != 0)
        //        {
        //            resultUpdate.TxnStatus = resultUpdate.StatusList.FirstOrDefault().TxnStatus;
        //            resultUpdate.ResponseCode = resultUpdate.StatusList.FirstOrDefault().ResponseCode;
        //            resultUpdate.ResponseMsg = resultUpdate.StatusList.FirstOrDefault().ResponseMsg;
        //        }

        //        return resultUpdate ?? GenericStatusListResult.Fail("Response is null");
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Handle HTTP-specific errors (e.g., connection issues)
        //        return GenericStatusListResult.Fail($"HTTP request failed: {ex.Message}");
        //    }
        //    catch (JsonException ex)
        //    {
        //        // Handle JSON serialization/deserialization errors
        //        return GenericStatusListResult.Fail($"JSON processing failed: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle other unexpected errors
        //        return GenericStatusListResult.Fail($"An unexpected error occurred: {ex.Message}");
        //    }
        //}


        public async Task<GenericStatusListResult> CasaIntRates(CasaIntRatesRequestViewModel requestInput)
        {
            try
            {
                if (requestInput == null || requestInput.InterestRates?.Record == null || !requestInput.InterestRates.Record.Any())
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }

                int iCountTmp = 0;
                foreach (var itemRate in requestInput.InterestRates.Record)
                {
                    iCountTmp++;

                    if (string.IsNullOrWhiteSpace(itemRate.AccountType) || string.IsNullOrWhiteSpace(itemRate.AccountSubType))
                    {
                        return GenericStatusListResult.Fail(
                            $"Invalid input data. Loại tài khoản hoặc loại tài khoản phụ không hợp lệ (Dòng [{iCountTmp}] có giá trị AccountType: [{itemRate.AccountType}] và AccountSubType: [{itemRate.AccountSubType}])");
                    }
                }

                _logger.LogInformation("Starting CasaIntRates with UserId: {UserId}, BankCircularRefNum: {BankCircularRefNum}",
                    requestInput.UserId, requestInput.BankCircularRefNum);

                var jsonPayload = JsonConvert.SerializeObject(requestInput, Formatting.Indented);
                _logger.LogInformation("=== CASA PAYLOAD GỬI ĐI ===\n{Payload}", jsonPayload);

                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/casaIntRates", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("=== CASA RESPONSE NHẬN ĐƯỢC ===\n{Response}", responseContent);

                var resultUpdate = JsonConvert.DeserializeObject<GenericStatusListResult>(responseContent);

                if (resultUpdate != null && resultUpdate.StatusList?.Any() == true)
                {
                    var first = resultUpdate.StatusList.FirstOrDefault();
                    resultUpdate.TxnStatus = first?.TxnStatus;
                    resultUpdate.ResponseCode = first?.ResponseCode;
                    resultUpdate.ResponseMsg = first?.ResponseMsg;
                }

                return resultUpdate ?? GenericStatusListResult.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling casaIntRates");
                return GenericStatusListResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON error in CasaIntRates");
                return GenericStatusListResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CasaIntRates");
                return GenericStatusListResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi API tidePenalRates cập nhật thông tin cấu hình lãi suất rút trước hạn Tide vào iDC thông qua API 
        /// http://10.63.54.52:7003/vbsp/internal/api/v1/tidePenalRates
        /// </summary>
        /// <param name="requestInput">Thông tin cấu hình lãi suất</param>
        /// <returns>Kết quả trả về. Ex: 
        /// {
        ///     "respRecord": [
        ///         {
        ///             "txnStatus": "SUCCESS",
        ///             "reqRecordSl": "1",
        ///             "responseCode": "00000",
        ///             "responseMsg": " "
        ///         },
        ///         {
        ///             "txnStatus": "SUCCESS",
        ///             "reqRecordSl": "1",
        ///             "responseCode": "00000",
        ///             "responseMsg": " "
        ///         }
        ///     ]
        /// }
        /// </returns>
        public async Task<GenericStatusListResult> TidePenalRates(TidePenalIntRatesRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting TidePenalRates with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null || requestInput.TidePenalInterestRatesRequestViewModel?.RecordTidePenalInterestRateViewModel == null 
                    || !requestInput.TidePenalInterestRatesRequestViewModel.RecordTidePenalInterestRateViewModel.Any())
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                if (requestInput.TidePenalInterestRatesRequestViewModel.RecordTidePenalInterestRateViewModel == null)
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                if (requestInput.TidePenalInterestRatesRequestViewModel.RecordTidePenalInterestRateViewModel.Count <= 0)
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }

                _logger.LogInformation("Starting TidePenalRates with UserId: {UserId}, Count Records: {RecordSL}", requestInput.UserId,
                                requestInput.TidePenalInterestRatesRequestViewModel.RecordTidePenalInterestRateViewModel.Count.ToString());

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/tidePenalRates");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/tidePenalRates", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultUpdate = JsonConvert.DeserializeObject<GenericStatusListResult>(responseContent);
                if (resultUpdate != null && resultUpdate.StatusList.Any() && resultUpdate.StatusList.Count != 0)
                {
                    foreach (var itemResultTmp in resultUpdate.StatusList)
                    {
                        if (string.IsNullOrEmpty(itemResultTmp.ResponseCode))
                            itemResultTmp.ResponseCode = "99193";
                        if (string.IsNullOrEmpty(itemResultTmp.ResponseMsg))
                            itemResultTmp.ResponseMsg = "Entered Date is Invalid";
                        if (string.IsNullOrEmpty(itemResultTmp.TxnStatus))
                            itemResultTmp.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    resultUpdate.TxnStatus = resultUpdate.StatusList.OrderBy(o=>o.ResponseCode).FirstOrDefault().TxnStatus;
                    resultUpdate.ResponseCode = resultUpdate.StatusList.OrderBy(o => o.ResponseCode).FirstOrDefault().ResponseCode;
                    string sRecordSLTemp = "";
                    var objError = resultUpdate.StatusList.Where(w => w.TxnStatus != null && w.TxnStatus.ToLower() == ResultValueAPI.ResultValue_Status_Failed.ToLower()).ToList();
                    if (objError != null && objError.Count != 0)
                    {
                        sRecordSLTemp = string.Join(", ", objError.Select(s => s.ReqRecordSl)); // Nối các giá trị thành chuỗi, cách nhau bằng dấu phẩy
                        resultUpdate.ResponseMsg = $"{objError.FirstOrDefault().ResponseMsg}. Có {objError.Count.ToString()} bản ghi cập nhật không thành công. Các STT lỗi: {sRecordSLTemp}";
                    }
                    else resultUpdate.ResponseMsg = resultUpdate.StatusList.FirstOrDefault().ResponseMsg;
                }

                return resultUpdate ?? GenericStatusListResult.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return GenericStatusListResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return GenericStatusListResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericStatusListResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi API tideIntRates cập nhật thông tin cấu hình lãi suất tiền gửi có kỳ hạn Tide vào iDC thông qua API
        /// http://10.63.54.52:7003/vbsp/internal/api/v1/tideIntRates
        /// </summary>
        /// <param name="requestInput">Thông tin cấu hình lãi suất</param>
        /// <returns>Kết quả trả về. Ex: {"respRecord": [{"txnStatus": "SUCCESS","reqRecordSl": "1","responseCode": "00000","responseMsg": " "}]}</returns>
        public async Task<GenericStatusListResult> TideIntRates(TideIntRatesRequestViewModel requestInput)
        {
            try
            {
                /* Ví dụ về Input vào API
                {
                    "userId": "IDCADMIN",
                    "bankCircularDate": "20220101",
                    "bankCircularRefNum": "abcrefnum",
                    "upldIntRt": {
                        "tideIntRates": [
                            {
                                "recordSl": "1",
                                "productCode": "431",
                                "accountType": "43101",
                                "accountSubType": "0",
                                "currencyCode": "VND",
                                "effectiveDate": "20220426",
                                "amountSlab": "1",
                                "posCode": "002721",
                                "posRateExpiryDate": "20221103",
                                "interestRates": {
                                    "record": [
                                        {
                                            "tenorSl": "1",
                                            "interestRate": "5.25",
                                            "intRateType": " ",
                                            "spreadRate": "0"
                                        },
                                        {
                                            "tenorSl": "2",
                                            "interestRate": "1.25",
                                            "intRateType": " ",
                                            "spreadRate": " "
                                        }
                                    ]
                                }
                            }
                        ]
                    }
                }
                 */
                _logger.LogInformation("Starting TideIntRates with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null || requestInput.UpldIntRt?.TideIntRates == null
                    || !requestInput.UpldIntRt.TideIntRates.Any())
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                if (requestInput.UpldIntRt.TideIntRates == null)
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                if (requestInput.UpldIntRt.TideIntRates.Count <= 0)
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                _logger.LogInformation("Starting CasaIntRates with UserId: {UserId}, BankCircularRefNum: {BankCircularRefNum}", requestInput.UserId, requestInput.BankCircularRefNum);

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/tideIntRates");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/tideIntRates", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultUpdate = JsonConvert.DeserializeObject<GenericStatusListResult>(responseContent);
                if (resultUpdate != null && resultUpdate.StatusList.Any() && resultUpdate.StatusList.Count != 0)
                {
                    foreach (var itemResultTmp in resultUpdate.StatusList)
                    {
                        if (string.IsNullOrEmpty(itemResultTmp.ResponseCode))
                            itemResultTmp.ResponseCode = "99193";
                        if (string.IsNullOrEmpty(itemResultTmp.ResponseMsg))
                            itemResultTmp.ResponseMsg = "Entered Date is Invalid";
                        if (string.IsNullOrEmpty(itemResultTmp.TxnStatus))
                            itemResultTmp.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    resultUpdate.TxnStatus = resultUpdate.StatusList.OrderBy(o => o.ResponseCode).FirstOrDefault().TxnStatus;
                    resultUpdate.ResponseCode = resultUpdate.StatusList.OrderBy(o => o.ResponseCode).FirstOrDefault().ResponseCode;
                    string sRecordSLTemp = "";
                    var objError = resultUpdate.StatusList.Where(w => w.TxnStatus != null && w.TxnStatus.ToLower() == ResultValueAPI.ResultValue_Status_Failed.ToLower()).ToList();
                    if (objError != null && objError.Count != 0)
                    {
                        sRecordSLTemp = string.Join(", ", objError.Select(s => s.ReqRecordSl)); // Nối các giá trị thành chuỗi, cách nhau bằng dấu phẩy
                        resultUpdate.ResponseMsg = $"{objError.FirstOrDefault().ResponseMsg}. Có {objError.Count.ToString()} bản ghi cập nhật không thành công. Các STT lỗi: {sRecordSLTemp}";
                        resultUpdate.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        resultUpdate.ResponseMsg = resultUpdate.StatusList.FirstOrDefault().ResponseMsg;
                        resultUpdate.TxnStatus = ResultValueAPI.ResultValue_Status_Success;
                    }
                }

                return resultUpdate ?? GenericStatusListResult.Fail("Response is null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return GenericStatusListResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return GenericStatusListResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericStatusListResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }









        /// <summary>
        /// Hàm lấy thông tin người dùng trên iDC qua API
        /// Gọi đến API ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/viewUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "ticket": "", "userId": "DUYEN002" }</param>
        /// <returns>Thông tin người dùng cần lấy</returns>
        public async Task<GenericListRecordJava<ViewUserReposeViewModel>> GetUserIDCInfoByApiViewUser(ViewUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting GetUserIDCInfoByApiViewUser with input: {Input}", JsonConvert.SerializeObject(requestInput));

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/viewUser");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/viewUser", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                ViewUserReposeViewModel result = JsonConvert.DeserializeObject<ViewUserReposeViewModel>(responseContent);
                if(result==null || string.IsNullOrEmpty(result.UserId))
                    return GenericListRecordJava<ViewUserReposeViewModel>.Fail($"Không có thông tin người dùng: {requestInput.UserId}");
                else
                    return GenericListRecordJava<ViewUserReposeViewModel>.Success(new List<ViewUserReposeViewModel> { result });
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return GenericListRecordJava<ViewUserReposeViewModel>.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return GenericListRecordJava<ViewUserReposeViewModel>.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return GenericListRecordJava<ViewUserReposeViewModel>.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }
        /*
        Ví dụ đầu vào và đầu ra của API viewUser
            {
                "ticket": "",    
                "userId": "DUYEN002"
            }
        Đầu ra
            {
                "lastPWDChanged": "2025-09-26",
                "primaryChoicebasedAuthType": "0",
                "serviceStatusResponse": {
                    "sessionValReq": "true",
                    "prevStatus": "0",
                    "responseAttributes": {},
                    "responseCode": "0",
                    "responseMsg": "User Information Successfully displayed",
                    "status": "true"
                },
                "mobileNumber": "0983273000",
                "tranAuthType": "0",
                "reqNo": "0",
                "selfRegistration": "false",
                "fromRecord": "0",
                "language": "en_US",
                "userCreatedDate": "2025-09-26",
                "corporateName": "0",
                "emailAddress": "th@vbsp.vn",
                "authsecType": "0",
                "DOB": "2000-01-01",
                "invalidAttempt": "0",
                "userFromService": "false",
                "extraAttribute": {
                    "UserRole": "SUPER",
                    "BranchCode": "4203"
                },
                "nickName": "DUYEN002",
                "defaultBranch": "IDCPRODC",
                "hpinFlag": "0",
                "reqNumber": "0",
                "toRecord": "0",
                "appendEntity": "false",
                "firstName": "Hồng",
                "groupName": "VBSPR",
                "additionalInfoMap": {},
                "isWebSealUser": "false",
                "entityList": "IDCPRODC",
                "userIdentifierName": "All",
                "operationType": "-1",
                "userType": "0",
                "encryptExtraAttrib": "false",
                "lastName": "Duyên",
                "userIdentifierAlias": "All",
                "userStatus": "2",
                "secondaryChoicebasedAuthType": "0",
                "prevStatus": "-7",
                "appendRole": "false",
                "lastLoginDate": "20260324031929",
                "authTypeAttrib": {},
                "expiryDate": "2050-11-22",
                "checkerDate": "2025-09-26",
                "mailIdFlag": "0",
                "authType": "1",
                "credInfoEncryptType": "0",
                "makerId": "SYSTEMADMIN1",
                "reqActivity": "0",
                "extraAttribs": {},
                "makerDate": "2025-09-26",
                "appendEntityRoleMap": "false",
                "salt": "dummysalt",
                "userId": "DUYEN002",
                "checkerId": "SYSTEMADMIN1",
                "currLoginDate": "20260324065257"
            }
         */

        /// <summary>
        /// Hàm thực hiện gọi API addUser thêm mới thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin người dùng Intellect iDC cần thêm mới</param>
        /// <returns>Kết quả trả về. Ex: 
        /// {
        ///     "sessionValReq": "true",
        ///     "prevStatus": 0,
        ///     "responseAttributes": {
        ///         "USR_PASSWD": "s5j5SNHw"
        ///     },
        ///     "responseCode": 0,
        ///     "responseMsg": "User Successfully Registered",
        ///     "status": "true"
        /// }
        /// </returns>
        public async Task<UserIDCResponseResult> CreateUserIDCByAPIAddUser(AddUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting addUser with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null || requestInput.AddUserExtraAttributeRequestViewModel?.UserRole == null)
                {
                    _logger.LogWarning("Invalid input: Thông tin đầu vào hoặc Quyền người dùng trống. Vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data UserRole = {requestInput.AddUserExtraAttributeRequestViewModel?.UserRole}");
                }
                if (requestInput == null || requestInput.AddUserExtraAttributeRequestViewModel?.BranchCode == null)
                {
                    _logger.LogWarning("Invalid input: Thông tin đầu vào hoặc đơn vị POS người dùng trống. Vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data BranchCode = {requestInput.AddUserExtraAttributeRequestViewModel?.BranchCode}");
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cần tạo không được để trống vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data UserId = {requestInput.UserId}");
                }
                if (string.IsNullOrEmpty(requestInput.NickName))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cần tạo không được để trống vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data UserId = {requestInput.NickName}");
                }
                if (string.IsNullOrEmpty(requestInput.GroupName))
                {
                    _logger.LogWarning("Invalid input: Quyền của người dùng cần tạo không được để trống vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data GroupName = {requestInput.GroupName}");
                }
                if (string.IsNullOrEmpty(requestInput.ExpiryDate))
                {
                    _logger.LogWarning("Invalid input: Ngày hết hạn của người dùng cần tạo không được để trống vui lòng kiểm tra lại!");
                    return UserIDCResponseResult.Fail($"Invalid input data ExpiryDate= {requestInput.ExpiryDate}");
                }

                _logger.LogInformation($"Starting AddUser with UserId: {requestInput.UserId}");

                // Serialize input object to JSON
                requestInput.IpSet = string.IsNullOrEmpty(requestInput.IpSet) ? null : requestInput.IpSet;
                requestInput.SubType = string.IsNullOrEmpty(requestInput.SubType) ? null : requestInput.SubType;
                requestInput.RestrictSameTimeForAllDay = (requestInput.RestrictSameTimeForAllDay == null) ? null : requestInput.RestrictSameTimeForAllDay;
                if (requestInput.ListRestrictionRequest == null || requestInput.ListRestrictionRequest.Count <= 0)
                    requestInput.ListRestrictionRequest = null;

                var json = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                //var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/addUser");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/addUser", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultAddUser = JsonConvert.DeserializeObject<UserIDCResponseResult>(responseContent);

                if (resultAddUser != null)
                {
                    if (resultAddUser.ResponseCode == "0")
                        return UserIDCResponseResult.SetSuccess(resultAddUser.SessionValReq, resultAddUser.Status, resultAddUser.ResponseCode, resultAddUser.ResponseMsg, resultAddUser.ResponseAttributes);
                    else if (resultAddUser.ResponseCode == "4222")
                        return new UserIDCResponseResult(resultAddUser.SessionValReq, resultAddUser.Status, resultAddUser.ResponseCode, resultAddUser.ResponseMsg, resultAddUser.ResponseAttributes);   //return UserIDCResponseResult.Fail("ARX-004222: Exceeption occurred in registerUserInfo | ARX-004222: Ngoại lệ xảy ra trong hàm registerUserInfo");
                    else if (resultAddUser.ResponseCode == "101027")
                        return new UserIDCResponseResult(resultAddUser.SessionValReq, resultAddUser.Status, resultAddUser.ResponseCode, resultAddUser.ResponseMsg, resultAddUser.ResponseAttributes);   //return UserIDCResponseResult.Fail("ARX-001027 :Internal Error occurred.Contact Administrator | ARX-001027: Lỗi nội bộ đã xảy ra. Vui lòng liên hệ với quản trị viên");
                    else if (resultAddUser.ResponseCode.Contains("4622"))
                        return new UserIDCResponseResult(resultAddUser.SessionValReq, resultAddUser.Status, resultAddUser.ResponseCode, resultAddUser.ResponseMsg, resultAddUser.ResponseAttributes);
                        //return UserIDCResponseResult.Fail("ARX-004622: Register User Failed. User Already Exists | ARX-004622: Đăng ký người dùng thất bại. Người dùng đã tồn tại!");
                    else if (resultAddUser.ResponseCode.Contains("705"))
                        return new UserIDCResponseResult(resultAddUser.SessionValReq, resultAddUser.Status, resultAddUser.ResponseCode, resultAddUser.ResponseMsg, resultAddUser.ResponseAttributes); //return UserIDCResponseResult.Fail("ARX-000705: Invalid Ticket | ARX-000705: ticket không hợp lệ");
                    else return UserIDCResponseResult.Fail($"Error: {resultAddUser.ResponseCode} - {resultAddUser.ResponseMsg}");
                }
                else return UserIDCResponseResult.Fail("Response null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return UserIDCResponseResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return UserIDCResponseResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return UserIDCResponseResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi API tellerRoleAssign gán hoặc bỏ gán quyền tiền mặt cho người dùng đăng nhập Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/tellerRoleAssign
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "tellerId": "CHUDV002",
        ///         "tellerRoleAllowed": "1",
        ///         "mkrId": "IDCADMIN"
        ///     }
        /// </param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "txnStatus": "Success",
        ///         "responseMsg": "API Invocation Success",
        ///         "responseCode": "00000"
        ///     }
        ///     {
        ///         "txnStatus": "FAILED",
        ///         "responseMsg": "INVALID TELLER ID",
        ///         "responseCode": ""
        ///     }
        /// </returns>
        public async Task<TellerRoleAssignResponseResult> ChangeRoleToTransferCashByAPITellerRoleAssign(TellerRoleAssignRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting tellerRoleAssign with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("ChangeRoleToTransferCashByAPITellerRoleAssign failed: RequestInput is null");
                    return TellerRoleAssignResponseResult.Fail($"ChangeRoleToTransferCashByAPITellerRoleAssign failed: RequestInput is null");
                }
                if (string.IsNullOrEmpty(requestInput.TellerId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng thay đổi quyền tiền mặt không được để trống. Vui lòng kiểm tra lại!!");
                    return TellerRoleAssignResponseResult.Fail($"Invalid input data TellerId = {requestInput.TellerId}");
                }
                if (string.IsNullOrEmpty(requestInput.MkrId))
                {
                    _logger.LogWarning($"Invalid input: Tài khoản người dùng thực hiện việc thay đổi quyền tiền mặt người dùng {requestInput.TellerId} không được để trống. Vui lòng kiểm tra lại!!");
                    return TellerRoleAssignResponseResult.Fail($"Invalid input data MkrId = {requestInput.MkrId}");
                }
                if (requestInput.TellerRoleAllowed != 0 && requestInput.TellerRoleAllowed != 1)
                {
                    _logger.LogWarning($"Invalid input: Giá trị cờ gán/bỏ gán quyền tiền mặt cho người dùng không hợp lệ. Vui lòng kiểm tra lại!!");
                    return TellerRoleAssignResponseResult.Fail($"Invalid input data TellerRoleAllowed = {requestInput.TellerRoleAllowed.ToString()}");
                } 

                _logger.LogInformation($"Starting tellerRoleAssign with UserId: {requestInput.TellerId}");

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/tellerRoleAssign");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/tellerRoleAssign", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultRoleAssign = JsonConvert.DeserializeObject<TellerRoleAssignResponseResult>(responseContent);

                if (resultRoleAssign != null)
                {
                    if (resultRoleAssign.ResponseCode == "0" || resultRoleAssign.ResponseCode == "00000")
                        return new TellerRoleAssignResponseResult(resultRoleAssign.TxnStatus, resultRoleAssign.ResponseCode, resultRoleAssign.ResponseMsg);
                    else return TellerRoleAssignResponseResult.Fail($"TellerRoleAssign failed: {resultRoleAssign.TxnStatus} - {resultRoleAssign.ResponseCode} - {resultRoleAssign.ResponseMsg}");
                }
                else return TellerRoleAssignResponseResult.Fail("TellerRoleAssign Response null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return TellerRoleAssignResponseResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return TellerRoleAssignResponseResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return TellerRoleAssignResponseResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện Mở/Kích hoạt lại tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/enableUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "enabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "enabled_at": "2026-03-27T10:06:40+00:00",
        ///         "responseCode": 0,
        ///         "responseMsg": "Enable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already enabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        public async Task<ChangeUserStatusResponseResult> ChangeUserStatusByAPIEnableUser(ViewUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting enableUser with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("ChangeUserStatusByAPIEnableUser failed: RequestInput is null");
                    return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"Request input is null", "false", "", "", "", "", "", "", "", "");
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cần kích hoạt/mở lại không được để trống. Vui lòng kiểm tra lại!!");
                    return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"Invalid input data UserId = {requestInput.UserId}", "false", "", "", "", "", "", "", "", "");
                }
                _logger.LogInformation($"Starting enableUser with UserId: {requestInput.UserId}");

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/enableUser");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/enableUser", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultEnableUser = JsonConvert.DeserializeObject<ChangeUserStatusResponseResult>(responseContent);

                if (resultEnableUser != null)
                {
                    if (resultEnableUser.ResponseCode == "0" || resultEnableUser.ResponseCode == "00000")
                        return new ChangeUserStatusResponseResult(resultEnableUser.SessionValReq ?? "true",
                                                                  resultEnableUser.PrevStatus ?? 0, resultEnableUser.ResponseCode, resultEnableUser.ResponseMsg,
                                                                  resultEnableUser.Status ?? "true", resultEnableUser.EmailAddress ?? "",
                                                                  resultEnableUser.MobileNumber ?? "", resultEnableUser.UserId ?? "",
                                                                  resultEnableUser.EnabledAt ?? "", resultEnableUser.EnabledBy ?? "",
                                                                  resultEnableUser.DisabledAt ?? "", resultEnableUser.DisabledBy ?? "",
                                                                  resultEnableUser.StatusCode ?? ResultValueAPI.ResultValue_Status_Success);
                    else return new ChangeUserStatusResponseResult(resultEnableUser.SessionValReq ?? "true",
                                                                  resultEnableUser.PrevStatus ?? 0, resultEnableUser.ResponseCode, resultEnableUser.ResponseMsg,
                                                                  resultEnableUser.Status ?? "true", resultEnableUser.EmailAddress ?? "",
                                                                  resultEnableUser.MobileNumber ?? "", resultEnableUser.UserId ?? "",
                                                                  resultEnableUser.EnabledAt ?? "", resultEnableUser.EnabledBy ?? "",
                                                                  resultEnableUser.DisabledAt ?? "", resultEnableUser.DisabledBy ?? "",
                                                                  resultEnableUser.StatusCode ?? ResultValueAPI.ResultValue_Status_Failed);
                }
                else return ChangeUserStatusResponseResult.Fail("false", 0, "-1", "EnableUser Response null", "false", "", "", "", "", "", "", "", "");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"HTTP request failed: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"JSON processing failed: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"An unexpected error occurred: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
        }

        /// <summary>
        /// Hàm thực hiện Đóng/Khóa tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/disableUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "disabled_at": "2026-03-27T10:06:40+00:00",
        ///         "disabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "responseCode": 0,
        ///         "responseMsg": "Disable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already disabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        public async Task<ChangeUserStatusResponseResult> ChangeUserStatusByAPIDisableUser(ViewUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting disableUser with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("ChangeUserStatusByAPIDisableUser failed: RequestInput is null");
                    return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"Request input is null", "false", "", "", "", "", "", "", "", "");
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cần kích hoạt/mở lại không được để trống. Vui lòng kiểm tra lại!!");
                    return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"Invalid input data UserId = {requestInput.UserId}", "false", "", "", "", "", "", "", "", "");
                }
                _logger.LogInformation($"Starting disableUser with UserId: {requestInput.UserId}");

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/disableUser");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/disableUser", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultDisableUser = JsonConvert.DeserializeObject<ChangeUserStatusResponseResult>(responseContent);

                if (resultDisableUser != null)
                {
                    if (resultDisableUser.ResponseCode == "0" || resultDisableUser.ResponseCode == "00000")
                        return new ChangeUserStatusResponseResult(resultDisableUser.SessionValReq ?? "true",
                                                                  resultDisableUser.PrevStatus ?? 0, resultDisableUser.ResponseCode, resultDisableUser.ResponseMsg,
                                                                  resultDisableUser.Status ?? "true", resultDisableUser.EmailAddress ?? "",
                                                                  resultDisableUser.MobileNumber ?? "", resultDisableUser.UserId ?? "",
                                                                  resultDisableUser.EnabledAt ?? "", resultDisableUser.EnabledBy ?? "",
                                                                  resultDisableUser.DisabledAt ?? "", resultDisableUser.DisabledBy ?? "",
                                                                  resultDisableUser.StatusCode ?? ResultValueAPI.ResultValue_Status_Success);
                    else return new ChangeUserStatusResponseResult(resultDisableUser.SessionValReq ?? "true",
                                                                  resultDisableUser.PrevStatus ?? 0, resultDisableUser.ResponseCode, resultDisableUser.ResponseMsg,
                                                                  resultDisableUser.Status ?? "true", resultDisableUser.EmailAddress ?? "",
                                                                  resultDisableUser.MobileNumber ?? "", resultDisableUser.UserId ?? "",
                                                                  resultDisableUser.EnabledAt ?? "", resultDisableUser.EnabledBy ?? "",
                                                                  resultDisableUser.DisabledAt ?? "", resultDisableUser.DisabledBy ?? "",
                                                                  resultDisableUser.StatusCode ?? ResultValueAPI.ResultValue_Status_Failed);
                }
                else return ChangeUserStatusResponseResult.Fail("false", 0, "-1", "DisableUser Response null", "false", "", "", "", "", "", "", "", "");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"HTTP request failed: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"JSON processing failed: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return ChangeUserStatusResponseResult.Fail("false", 0, "-1", $"An unexpected error occurred: {ex.Message}", "false", "", "", "", "", "", "", "", "");
            }
        }

        /// <summary>
        /// Hàm thực hiện cấp lại mật khẩu tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/resetUserPw
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu thành công
        ///     {
        ///         "emailAddress": "chudv.cctt@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "reset_by": "SYSTEMADMIN2",
        ///         "userId": "CHUV12",
        ///         "reset_at": "2026-01-14T21:55:10+00:00",
        ///         "mail_flag": "0",
        ///         "responseCode": "0",
        ///         "responseMsg": "Password Reset Successful"
        ///     }
        /// Nếu không thành công
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": "0",
        ///         "responseAttributes": { },
        ///         "responseCode": "5317",
        ///         "responseMsg": "ARX-005317: User does not exist.",
        ///         "status": "true"
        ///     }
        /// </returns>
        public async Task<ResetUserPasswordResponseResult> ResetUserPasswordByAPIResetUserPw(ViewUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting resetUserPw with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("ResetUserPasswordByAPIResetUserPw failed: RequestInput is null");
                    return ResetUserPasswordResponseResult.Fail("false", 0, "-1", $"Request input is null", "false", "", "", "", "", "", "",
                                                                 null, ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cấp lại mật khẩu không được để trống. Vui lòng kiểm tra lại!!");
                    return ResetUserPasswordResponseResult.Fail("false", 0, "-1", $"Invalid input data UserId = {requestInput.UserId}", "false", "", "", "", "", "", "",
                                                                 null, ResultValueAPI.ResultValue_Status_Failed);
                }
                _logger.LogInformation($"Starting resetUserPw with UserId: {requestInput.UserId}");

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/resetUserPw");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/resetUserPw", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultResetUserPw = JsonConvert.DeserializeObject<ResetUserPasswordResponseResult>(responseContent);

                if (resultResetUserPw != null)
                {
                    if (resultResetUserPw.ResponseCode == "0" || resultResetUserPw.ResponseCode == "00000")
                        return new ResetUserPasswordResponseResult(resultResetUserPw.SessionValReq ?? "true",
                                                                  resultResetUserPw.PrevStatus ?? 0, resultResetUserPw.ResponseCode, resultResetUserPw.ResponseMsg,
                                                                  resultResetUserPw.Status ?? "true", resultResetUserPw.EmailAddress ?? "",
                                                                  resultResetUserPw.MobileNumber ?? "", resultResetUserPw.UserId ?? "",
                                                                  resultResetUserPw.ResetAt ?? "", resultResetUserPw.ResetBy ?? "",
                                                                  resultResetUserPw.MailFlag ?? "", resultResetUserPw.ResponseAttributes ?? null,
                                                                  resultResetUserPw.StatusCode ?? ResultValueAPI.ResultValue_Status_Success);
                    else 
                        return new ResetUserPasswordResponseResult(resultResetUserPw.SessionValReq ?? "true",
                                                                  resultResetUserPw.PrevStatus ?? 0, resultResetUserPw.ResponseCode, resultResetUserPw.ResponseMsg,
                                                                  resultResetUserPw.Status ?? "true", resultResetUserPw.EmailAddress ?? "",
                                                                  resultResetUserPw.MobileNumber ?? "", resultResetUserPw.UserId ?? "",
                                                                  resultResetUserPw.ResetAt ?? "", resultResetUserPw.ResetBy ?? "",
                                                                  resultResetUserPw.MailFlag ?? "", resultResetUserPw.ResponseAttributes ?? null,
                                                                  resultResetUserPw.StatusCode ?? ResultValueAPI.ResultValue_Status_Failed);
                }
                else return ResetUserPasswordResponseResult.Fail("false", 0, "-1", "ResetUserPw Response null", "false", "", "", "", "", "", "",
                                                                 null, ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return ResetUserPasswordResponseResult.Fail("false", 0, "-1", $"HTTP request failed: {ex.Message}", "false", "", "", "", "", "", "",
                                                              null, ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return ResetUserPasswordResponseResult.Fail("false", 0, "-1", $"JSON processing failed: {ex.Message}", "false", "", "", "", "", "", "",
                                                              null, ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return ResetUserPasswordResponseResult.Fail("false", 0, "-1", $"An unexpected error occurred: {ex.Message}", "false", "", "", "", "", "", "",
                                                              null, ResultValueAPI.ResultValue_Status_Failed);
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi API modifyUser thay đổi thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin người dùng Intellect iDC cần thay đổi thông tin 
        ///     {
        ///         "ticket": "{{access_token}}",
        ///         "userId": "CHUDV99",
        ///         "firstName": "Dương Văn",
        ///         "lastName": "Chữ",
        ///         "groupName": "POPGD",
        ///         "entityList": "IDCPRODC",
        ///         "mobileNumber": "0908688212",
        ///         "emailAddress": "chudv.2510@gmail.com",
        ///         "expiryDate": "2045-10-25",
        ///         "DOB": "1983-10-25",
        ///         "mailIdFlag": 1,
        ///         "language": "vi_VN",
        ///         "extraAttribute": {
        ///             "BranchCode": "2505",
        ///             "UserRole": "POPGD"
        ///         }
        ///     }
        /// </param>
        /// <returns>Kết quả trả về. Ex: 
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2505",
        ///         "userRole": "POPGD",
        ///         "responseCode": 0,
        ///         "responseMsg": "Modify User Done Successfully",
        ///         "status": "true"
        ///     }
        /// --Hoặc nếu sửa tiếp POS thì trả ra như sau:
        ///     {
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2502",
        ///         "userRole": "POPGD",
        ///         "status": "true",
        ///         "responseMsg": " BranchCode Modify Done Successfully",
        ///         "responseCode": 0
        ///     }
        /// </returns>
        public async Task<ModifyUserIDCResponseResult> ModifyUserIDCByAPIModifyUser(ModifyUserRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting modifyUser with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("ModifyUserIDCByAPIModifyUser failed: RequestInput is null");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"ModifyUserIDCByAPIModifyUser failed: RequestInput is null", 
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data UserId = {requestInput.UserId}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.AddUserExtraAttributeRequestViewModel?.UserRole))
                {
                    _logger.LogWarning("Invalid input: Quyền người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data UserRole = {requestInput.AddUserExtraAttributeRequestViewModel?.UserRole}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.AddUserExtraAttributeRequestViewModel?.BranchCode))
                {
                    _logger.LogWarning("Invalid input: Mã đơn vị (BranchCode) người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data BranchCode = {requestInput.AddUserExtraAttributeRequestViewModel?.BranchCode}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.GroupName))
                {
                    _logger.LogWarning("Invalid input: Quyền người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data UserRole = {requestInput.GroupName}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.FirstName))
                {
                    _logger.LogWarning("Invalid input: Họ của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data FirstName = {requestInput.FirstName}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.LastName))
                {
                    _logger.LogWarning("Invalid input: Họ đệm và tên của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data LastName = {requestInput.LastName}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.MobileNumber))
                {
                    _logger.LogWarning("Invalid input: Số điện thoại của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data MobileNumber = {requestInput.MobileNumber}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.EmailAddress))
                {
                    _logger.LogWarning("Invalid input: Địa chỉ email của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data EmailAddress = {requestInput.EmailAddress}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.ExpiryDate))
                {
                    _logger.LogWarning("Invalid input: Ngày hết hạn của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data ExpiryDate = {requestInput.ExpiryDate}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                if (string.IsNullOrEmpty(requestInput.DateOfBirth))
                {
                    _logger.LogWarning("Invalid input: Ngày sinh của người dùng cần thay đổi thông tin không được để trống. Vui lòng kiểm tra lại!!");
                    return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Invalid input data DOB = {requestInput.DateOfBirth}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
                }
                _logger.LogInformation($"Starting ModifyUser with UserId: {requestInput.UserId}");
                requestInput.EmailAddress = string.IsNullOrEmpty(requestInput.EmailAddress) ? null : requestInput.EmailAddress;
                requestInput.Language = string.IsNullOrEmpty(requestInput.Language) ? null : requestInput.Language;
                requestInput.StartDate = string.IsNullOrEmpty(requestInput.StartDate) ? null : requestInput.StartDate;
                requestInput.IpSet = string.IsNullOrEmpty(requestInput.IpSet) ? null : requestInput.IpSet;
                requestInput.SubType = string.IsNullOrEmpty(requestInput.SubType) ? null : requestInput.SubType;
                requestInput.AuthsecType = string.IsNullOrEmpty(requestInput.AuthsecType) ? null : requestInput.AuthsecType;
                requestInput.RestrictSameTimeForAllDay = string.IsNullOrEmpty(requestInput.RestrictSameTimeForAllDay) ? null : requestInput.RestrictSameTimeForAllDay;
                if (requestInput.ListRestrictionRequest == null || requestInput.ListRestrictionRequest.Count <= 0)
                    requestInput.ListRestrictionRequest = null;

                var json = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                // Serialize input object to JSON
                //var json = JsonConvert.SerializeObject(requestInput);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/modifyUser");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/modifyUser", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultModifyUser = JsonConvert.DeserializeObject<ModifyUserIDCResponseResult>(responseContent);

                if (resultModifyUser != null)
                {
                    if (resultModifyUser.ResponseCode == "0")
                        return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                        resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                        resultModifyUser.ResponseCode, resultModifyUser.ResponseMsg, ResultValueAPI.ResultValue_Status_Success);
                    else if (resultModifyUser.ResponseCode == "999509" || resultModifyUser.ResponseCode == "705" || resultModifyUser.ResponseCode == "000705")
                        return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                        resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                        resultModifyUser.ResponseCode, $"{resultModifyUser.ResponseMsg} (Có thể Ticket không hợp lệ)", ResultValueAPI.ResultValue_Status_Failed);
                    else if (resultModifyUser.ResponseCode == "5317" || resultModifyUser.ResponseCode == "005317")
                        return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                        resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                        resultModifyUser.ResponseCode, $"{resultModifyUser.ResponseMsg} (Người dùng không tồn tại)", ResultValueAPI.ResultValue_Status_Failed);
                    else if (resultModifyUser.ResponseCode == "4638" || resultModifyUser.ResponseCode.EndsWith("4638"))
                        return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                        resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                        resultModifyUser.ResponseCode, $"{resultModifyUser.ResponseMsg} (Không tìm thấy người dùng cần thay đổi thông tin)", ResultValueAPI.ResultValue_Status_Failed);
                    else if (resultModifyUser.ResponseCode == "4616" || resultModifyUser.ResponseCode.EndsWith("4616"))
                        return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                        resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                        resultModifyUser.ResponseCode, $"{resultModifyUser.ResponseMsg} (Thay đổi thông tin không thành công)", ResultValueAPI.ResultValue_Status_Failed);
                    else return new ModifyUserIDCResponseResult(resultModifyUser.SessionValReq, resultModifyUser.PrevStatus ?? 0, resultModifyUser.ResponseAttributes,
                                       resultModifyUser.Status, resultModifyUser.MobileNumber, resultModifyUser.EmailAddress, resultModifyUser.PosCode, resultModifyUser.UserRole,
                                       resultModifyUser.ResponseCode, $"{resultModifyUser.ResponseMsg}", ResultValueAPI.ResultValue_Status_Failed);
                }
                else return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"Response null", ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"HTTP request failed: {ex.Message}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"JSON processing failed: {ex.Message}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return ModifyUserIDCResponseResult.Fail("false", -1, null, "false", "", "", "", "", "-1", $"An unexpected error occurred: {ex.Message}",
                                                            ResultValueAPI.ResultValue_Status_Failed);
            }
        }


        /// <summary>
        /// Hàm thực hiện gọi API idcPendingTxn/lmsPendingTxn Lấy danh sách giao dịch Pending theo người dùng
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/idcPendingTxn
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/lmsPendingTxn
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "userId": "68510"
        ///     }
        ///     Hoặc
        ///     {
        ///         "userId": "20047"
        ///     }
        /// 
        /// </param>
        /// <param name="pApiName">Tên API truyền vào. Nếu trống sẽ lấy cả 2 API vào (EsbApiName.LMSPendingTxn)</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu là idcPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "6",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:46:38"
        ///             },
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "7",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:47:17"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// Nếu là lmsPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// </returns>
        public async Task<PendingTransResponseResult> GetPendingTransactionsByAPIPendingTxn(PendingTransRequestViewModel requestInput, string pApiName)
        {
            try
            {
                _logger.LogInformation("Starting idcPendingTxn/lmsPendingTxn with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null)
                {
                    _logger.LogWarning("GetPendingTransactionsByAPIPendingTxn failed: RequestInput is null");
                    return PendingTransResponseResult.Fail($"GetPendingTransactionsByAPIPendingTxn failed: RequestInput is null");
                }
                if (string.IsNullOrEmpty(requestInput.UserId))
                {
                    _logger.LogWarning("Invalid input: Tài khoản người dùng để lấy danh sách giao dịch chưa hoàn thành (Pending) không được để trống. Vui lòng kiểm tra lại!!");
                    return PendingTransResponseResult.Fail($"Invalid input data TellerId = {requestInput.UserId}");
                }
                if (string.IsNullOrEmpty(pApiName))
                {
                    pApiName = EsbApiName.LMSPendingTxn.Code;
                }

                _logger.LogInformation($"Starting {pApiName} with UserId: {requestInput.UserId}");

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/" + pApiName);

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/" + pApiName, content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultPendingTrans = JsonConvert.DeserializeObject<PendingTransResponseResult>(responseContent);

                if (resultPendingTrans != null)
                {
                    if (resultPendingTrans.ResponseCode == "0" || resultPendingTrans.ResponseCode == "00000" || resultPendingTrans.ResponseCode == "90000")
                        return new PendingTransResponseResult(resultPendingTrans.TxnStatus, resultPendingTrans.ResponseCode, resultPendingTrans.ResponseMsg, resultPendingTrans.Records);
                    else return PendingTransResponseResult.Fail($"{pApiName} failed: {resultPendingTrans.TxnStatus} - {resultPendingTrans.ResponseCode} - {resultPendingTrans.ResponseMsg}");
                }
                else return PendingTransResponseResult.Fail($"{pApiName} Response null");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-specific errors (e.g., connection issues)
                return PendingTransResponseResult.Fail($"HTTP request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors
                return PendingTransResponseResult.Fail($"JSON processing failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return PendingTransResponseResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }



    }






}
