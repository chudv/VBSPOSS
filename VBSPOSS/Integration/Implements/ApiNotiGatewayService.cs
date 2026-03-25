using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.ViewModels;


namespace VBSPOSS.Integration.Implements
{
    public class ApiNotiGatewayService : IApiNotiGatewayService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ApiNotiGatewayService> _logger;

        public ApiNotiGatewayService(IHttpClientFactory httpClientFactory,
            ILogger<ApiNotiGatewayService> logger)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("NotiGatewayClient");
        }

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


        public async Task<string> UpdateNotiDataList(List<NotificationDataResponse> request)
        {
            try
            {
                var url = "api/v1/update-notification-data";

                // Serialize request thành JSON
                var json = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Noti API: {Url} with body: {Body}", url, json);

                var response = await _client.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("StatusCode: {StatusCode}", (int)response.StatusCode);
                _logger.LogInformation("Response: {Response}", responseText);

                response.EnsureSuccessStatusCode();

                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Noti API");
                throw;
            }
        }

    }
}
