using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            catch (JsonException ex)
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
            catch (JsonException ex)
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
        public async Task<GenericStatusListResult> CasaIntRates(CasaIntRatesRequestViewModel requestInput)
        {
            try
            {
                _logger.LogInformation("Starting CasaIntRates with input: {Input}", JsonConvert.SerializeObject(requestInput));
                if (requestInput == null || requestInput.InterestRates?.Record == null || !requestInput.InterestRates.Record.Any())
                {
                    _logger.LogWarning("Invalid input: requestInput or InterestRates.Record is null or empty");
                    return GenericStatusListResult.Fail("Invalid input data");
                }
                int iCountTmp = 0;
                foreach (var itemRate in requestInput.InterestRates.Record)
                {
                    iCountTmp++;
                    if (string.IsNullOrEmpty(itemRate.AccountType) || string.IsNullOrEmpty(itemRate.AccountSubType) || itemRate.AccountType.Length != 6)
                    {
                        return GenericStatusListResult.Fail($"Invalid input data. Loại tài khoản hoặc loại tài khoản phụ không hợp lệ (Dòng [{iCountTmp.ToString()}] có giá trị AccountType: [{itemRate.AccountType}] và AccountSubType: [{itemRate.AccountSubType}]");
                    }
                }
                _logger.LogInformation("Starting CasaIntRates with UserId: {UserId}, BankCircularRefNum: {BankCircularRefNum}", requestInput.UserId, requestInput.BankCircularRefNum);

                // Serialize input object to JSON
                var json = JsonConvert.SerializeObject(requestInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogDebug("Sending POST request to {Endpoint}", "vbsp/internal/api/v1/casaIntRates");

                // Send POST request
                var response = await _clientInternalEsb.PostAsync("vbsp/internal/api/v1/casaIntRates", content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and deserialize response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response: {ResponseContent}", responseContent);

                var resultUpdate = JsonConvert.DeserializeObject<GenericStatusListResult>(responseContent);
                if (resultUpdate != null && resultUpdate.StatusList.Any() && resultUpdate.StatusList.Count != 0)
                {
                    resultUpdate.TxnStatus = resultUpdate.StatusList.FirstOrDefault().TxnStatus;
                    resultUpdate.ResponseCode = resultUpdate.StatusList.FirstOrDefault().ResponseCode;
                    resultUpdate.ResponseMsg = resultUpdate.StatusList.FirstOrDefault().ResponseMsg;
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
    }






}
