using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.ViewModels;


namespace VBSPOSS.Integration.Implements
{
    public class ApiNotiGatewayService : IApiNotiGatewayService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ApiNotiGatewayService> _logger;
        private readonly ApplicationDbContext _dbContext;

        public ApiNotiGatewayService(IHttpClientFactory httpClientFactory,
            ApplicationDbContext dbContext,
            ILogger<ApiNotiGatewayService> logger)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("NotiGatewayClient");
            _dbContext = dbContext;
        }

        /// <summary>
        /// Hàm gọi lấy dữ liệu gửi thông báo sang NotiGateway
        /// Gọi đến API ESB: http://10.63.54.52:8085/api/v1/noti-send-by-type
        /// </summary>
        /// <returns>Danh sách bản tin</returns>
        public async Task<string> GetNotiByTypeAsync(string notiType, string sendType)
        {
            try
            {
                var url = $"api/v1/noti-send-by-type?notiType={notiType}&sendType={sendType}";
                _logger.LogInformation("Calling Noti API: {Url}", url);

                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API call success. Response length: {Length}", content?.Length ?? 0);

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when calling Noti API");
                return null;
            }
        }

        /// <summary>
        /// Hàm update bảng dữ liệu Template Noti trong NotiType (NOTI_MSG_TEMPLATE) 
        /// Gọi đến API ESB: http://10.63.54.52:8085/api/v1/update-notimsg-temp
        /// </summary>
        /// <returns></returns>
        public async Task<string> UpdateNotiMsgTempAsync(NotiMsgTempRequest request)
        {
            try
            {
                var url = "api/v1/update-notimsg-temp";
                var requestList = new List<NotiMsgTempRequest> { request };
                _logger.LogInformation("Calling Noti API: {Url} with body: {@Body}", url, requestList);
                var response = await _client.PostAsJsonAsync(url, requestList);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API returned error. Status: {StatusCode}, Body: {Body}", response.StatusCode, errorContent);
                    return null;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API call success. Response: {Response}", responseContent);

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when calling Noti API");
                return null;
            }
        }

        /// <summary>
        /// Hàm lấy danh sách bản ghi Template Noti trong NotiType (NOTI_MSG_TEMPLATE) theo trạng thái. 
        /// Gọi đến API ESB: http://10.63.54.52:8085/api/v1/noti-msg-list-temp
        /// </summary>
        /// <returns>Danh sách bản tin</returns>
        public async Task<List<NotiTempViewModel>> GetListNotiTempAsync(string pStatus)
        {
            try
            {
                var url = $"/api/v1/noti-msg-list-temp?status={pStatus}";
                _logger.LogInformation("Calling Noti API: {Url}", url);

                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code: {StatusCode}", response.StatusCode);
                    return new List<NotiTempViewModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API call success. Response length: {Length}", content?.Length ?? 0);

                var list = JsonConvert.DeserializeObject<List<NotiTempViewModel>>(content);

                return list ?? new List<NotiTempViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when calling Noti API");
                return new List<NotiTempViewModel>();
            }
        }

        /// <summary>
        /// Hàm 1 lấy bản tin gần nhất trong noti data (VBSP_NOTIFICATION_DATA) theo điều kiện động D1-> D20. 
        /// Gọi đến API ESB: http://10.63.54.52:8085/api/v1/get-notification-data-auto
        /// </summary>
        //{
        //  "notiType": "USER_OFFLINE",
        //  "conditions": {
        //    "d1": "TXN0273003",
        //    "d6": "20250614",
        //    ...
        //  }
        //}
        /// <returns>Danh sách bản tin</returns>

        public async Task<GenericResultCode<NotificationDataResponse>?> GetNotificationDataAutoAsync(NotificationSearchRequest request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.NotiType))
                    throw new ArgumentException("NotiType không được để trống");

                ValidateConditions(request.Conditions);

                const string url = "/api/v1/get-notification-data-auto";

                var json = System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Calling API: {Url}", $"{_client.BaseAddress}{url}");
                _logger.LogInformation("Request body: {Body}", json);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _client.PostAsync(url, content);

                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("StatusCode: {StatusCode}", (int)response.StatusCode);
                _logger.LogInformation("Response: {Response}", responseText);

                response.EnsureSuccessStatusCode();

                var result = System.Text.Json.JsonSerializer.Deserialize<GenericResultCode<NotificationDataResponse>>(
                    responseText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Request bị null");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Tham số không hợp lệ");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "API timeout hoặc bị hủy");
                throw;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Lỗi khi parse JSON");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định");
                throw;
            }
        }


        private static void ValidateConditions(Dictionary<string, string> conditions)
        {
            try
            {
                if (conditions == null || conditions.Count == 0)
                    return;

                var allowedKeys = Enumerable.Range(1, 20)
                    .Select(i => $"d{i}")
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var key in conditions.Keys)
                {
                    if (!allowedKeys.Contains(key))
                    {
                        throw new ArgumentException(
                            $"Condition key không hợp lệ: {key}. Chỉ nhận từ d1 đến d20.");
                    }
                }
            }
            catch (ArgumentException ex)
            {
                // Lỗi tham số không hợp lệ
                Console.WriteLine($"[ValidateConditions] Lỗi tham số: {ex.Message}");
                throw; // vẫn ném ra để caller xử lý tiếp
            }
            catch (Exception ex)
            {
                // Bắt tất cả lỗi khác
                Console.WriteLine($"[ValidateConditions] Lỗi không xác định: {ex.Message}");
                throw;
            }
        }


        public async Task<UpdateNotiResult> UpdateNotiDataList(List<NotificationDataResponse> request)
        {
            try
            {
                if (request == null || !request.Any())
                    throw new ArgumentException("request không được null hoặc rỗng");

                var url = "api/v1/update-notification-data";

                // Serialize request
                var json = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Noti API: {Url} with body: {Body}", url, json);

                var response = await _client.PostAsync(url, content);

                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("StatusCode: {StatusCode}", (int)response.StatusCode);
                _logger.LogInformation("Response: {Response}", responseText);

                response.EnsureSuccessStatusCode();

                // ===== DESERIALIZE =====
                var result = System.Text.Json.JsonSerializer.Deserialize<UpdateNotiResult>(
                    responseText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null)
                    throw new Exception("Không parse được response từ Noti API");

                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Lỗi parse JSON từ Noti API");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi HTTP khi gọi Noti API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Noti API");
                throw;
            }
        }


        public async Task<GenericResultCode<List<NotificationDataResponse>?>> GetNotificationDataUserOffline(string notiType, string posCode, string transPoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(notiType))
                    throw new ArgumentException("notiType không được để trống");

                if (string.IsNullOrWhiteSpace(posCode))
                    throw new ArgumentException("posCode không được để trống");

                if (string.IsNullOrWhiteSpace(transPoint))
                    throw new ArgumentException("transPoint không được để trống");


                var url = $"/api/v1/get-list-notification-data-user-offline" +
                          $"?notiType={Uri.EscapeDataString(notiType)}" +
                          $"&posCode={Uri.EscapeDataString(posCode)}" +
                          $"&transPoint={Uri.EscapeDataString(transPoint)}";

                _logger.LogInformation("Calling API: {Url}", url);

                var response = await _client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = System.Text.Json.JsonSerializer.Deserialize<GenericResultCode<List<NotificationDataResponse>?>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GetNotificationDataUserOffline");
                throw;
            }
        }

        /// <summary>
        /// Hàm lấy danh Cập nhật thông tin gửi thông báo người dùng đi GDX
        /// </summary>
        /// <param name="notiType">Loại thông báo ( bắt buộc)</param>
        /// <param name="posCode">Mã POS ( bắt buộc)</param>
        /// <param name="transPoint">Điểm giao dịch</param>
        /// <returns>Kết quả cập nhật</returns>
        public async Task<UpdateNotiResult> UpdateNotiDataOffline(string notiType, string posCode, string transPoint, string transDate, string username)
        {
            try
            {
                username = username ?? "USERNULL";
                var result = await GetNotificationDataUserOffline(notiType, posCode, transPoint);
                var data = result?.Result;

                if (data == null)
                {
                    return new UpdateNotiResult
                    {
                        Code = "01",
                        Message = "Không có dữ liệu để xử lý"
                    };
                }

                if (!(data is IEnumerable<NotificationDataResponse>))
                {
                    return new UpdateNotiResult
                    {
                        Code = "02",
                        Message = "Dữ liệu trả về không đúng định dạng"
                    };
                }

                if (!data.Any())
                {
                    return new UpdateNotiResult
                    {
                        Code = "03",
                        Message = "Danh sách dữ liệu trống"
                    };
                }

                foreach (var item in data)
                {
                    item.status = "0";
                    item.resendTimes = 0;
                    item.status2 = "0";
                    item.resendTimes2 = 0;
                    item.status3 = "0";
                    item.resendTimes3 = 0;
                }

                var updateResponse =  UpdateNotiDataList(data);

                if (updateResponse == null || updateResponse.Result == null)
                {
                    return new UpdateNotiResult
                    {
                        Code = "04",
                        Message = "Không nhận được phản hồi từ hệ thống cập nhật"
                    };
                }
                if (updateResponse.Result.Code != "00")
                {
                    _logger.LogError($"Update thất bại. Code: {updateResponse.Result.Code}, Msg: {updateResponse.Result.Message}");

                    return new UpdateNotiResult
                    {
                        Code = updateResponse.Result.Code,
                        Message = "Cập nhật thất bại: " + updateResponse.Result.Message
                    };
                }


                _logger.LogInformation("UpdateNotiDataList thành công");
                var entities = new List<UserOfflineSendOTTHist>();

                foreach (var item in result.Result)
                {
                    var transDateTXN = DateTime.ParseExact(item.d6, "yyyyMMdd", null);
                    var dateNow = DateTime.Now;

                    var entity = new UserOfflineSendOTTHist
                    {
                        PosCode = item.posCode,
                        PosName = item.posName,
                        CommuneCode = transPoint,
                        CommuneName = transPoint,
                        TxnPointCode = item.d1,
                        TxnPointName = transPoint,
                        TransDate = transDateTXN,
                        UserIdOffline = item.d2,
                        PassWord = item.d3,
                        RoleCode = item.d5,
                        MobileNo = item.mobileNo,
                        EmailId = item.email,
                        Status = 1, // Done
                        Remark = "Inserted from Noti",
                        CreatedBy = username,
                        CreatedDate = dateNow
                    };

                    entities.Add(entity);
                }

                await _dbContext.ListUserOfflineSendOTTHists.AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();

                return new UpdateNotiResult
                {
                    Code = "00",
                    Message = "Cập nhật và lưu dữ liệu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi UpdateNotiDataOffline");

                return new UpdateNotiResult
                {
                    Code = "99",
                    Message = "Lỗi hệ thống: " + ex.Message
                };
            }
        }
    }
}
