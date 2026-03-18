using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
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
    }
}
