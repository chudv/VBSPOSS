using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;

namespace VBSPOSS.Integration.Implements
{
    public class ApiReportGateway : IApiReportGateway
    {
        private readonly IConfiguration _configuration;

        private readonly HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = false });


        public ApiReportGateway(IConfiguration configuration)
        {
            _configuration = configuration;
            client.BaseAddress = new Uri(_configuration["ReportGatewayUrl"]);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public GenericResultReportGateway<ReportResultDto> GetReport(ReportInput inputModel)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/view-oss-report"))
                {
                    var json = JsonConvert.SerializeObject(inputModel);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.SendAsync(request).ConfigureAwait(false);
                    var x = response.GetAwaiter().GetResult().Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GenericResultReportGateway<ReportResultDto>>(x.Result);
                }
            }
            catch (Exception ex)
            {
                return GenericResultReportGateway<ReportResultDto>.Fail(ex.Message);
            }
        }
    }
}
