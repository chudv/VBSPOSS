using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;
using Telerik.SvgIcons;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Integration.Implements
{
    public class ApiInternalService: IApiInternalService
    {
        private readonly HttpClient _clientAPI;
        private readonly ILogger<ApiInternalService> _loggerInternalAPI;

        public ApiInternalService(IHttpClientFactory httpClientFactory,
            ILogger<ApiInternalService> loggerInternalAPI)
        {
            _loggerInternalAPI = loggerInternalAPI;
            _clientAPI = httpClientFactory.CreateClient("InternalApiClient");
        }

        /// <summary>
        /// Hàm lấy danh sách cán bộ NHCSXH theo POS truyền vào. Hàm sẽ gọi lấy dữ liệu từ API internal: list-staff-vbsp-by-poscode?pPosCode=002505
        /// </summary>
        /// <param name="pPosCode">Mã POS cần lấy danh sách cán bộ</param>
        /// <returns>Danh sách cán bộ NHCSXH</returns>
        public async Task<GenericResultInternalGateway<List<StaffVbspInforViewModel>>> GetListStaffVBSP(string pPosCode)
        {
            const string apiEndpoint = "/internal/list-staff-vbsp-by-poscode";
            try
            {
                var requestUrl = $"{apiEndpoint}?pPosCode={Uri.EscapeDataString(pPosCode)}";
                _loggerInternalAPI.LogInformation("Calling internal API: {Url}", requestUrl);

                //var responseApi = await _clientAPI.GetAsync(sUrlApi);
                using var responseApi = await _clientAPI.GetAsync(requestUrl);

                if (!responseApi.IsSuccessStatusCode)
                {
                    _loggerInternalAPI.LogWarning("API returned non-success status code: {StatusCode}", responseApi.StatusCode);
                    return GenericResultInternalGateway<List<StaffVbspInforViewModel>>
                        .Fail($"Lỗi khi lấy dữ liệu từ API: {(int)responseApi.StatusCode} {responseApi.ReasonPhrase}");

                }
                // Đọc và deserialize trực tiếp từ stream để tối ưu bộ nhớ
                var contentApi = await responseApi.Content.ReadAsStringAsync();
                _loggerInternalAPI.LogInformation("API call success. Response length: {Length}", contentApi?.Length ?? 0);
                var listStaffs = JsonConvert.DeserializeObject<GenericResultInternalGateway<List<StaffVbspInforViewModel>>>(contentApi);

                return listStaffs;
            }
            catch (HttpRequestException httpEx)
            {
                _loggerInternalAPI.LogError(httpEx, $"Network error when calling GetListStaffVBSP for PosCode: {pPosCode}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail("Lỗi kết nối đến API nội bộ.");
            }
            catch (JsonException jsonEx)
            {
                _loggerInternalAPI.LogError(jsonEx, $"JSON deserialization error when calling GetListStaffVBSP for PosCode: {pPosCode}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail("Dữ liệu trả về từ API không đúng định dạng.");
            }
            catch (Exception ex)
            {
                _loggerInternalAPI.LogError(ex, $"Unexpected error in GetListStaffVBSP for PosCode: {pPosCode}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail($"Đã xảy ra lỗi hệ thống khi lấy danh sách nhân viên VBSP.");
            }
        }

        /// <summary>
        /// Hàm lấy danh sách cán bộ NHCSXH theo Id cán bộ truyền vào. Hàm sẽ gọi lấy dữ liệu từ API internal: list-staff-vbsp-by-staffId?pStaffId=THGA00000000166
        /// </summary>
        /// <param name="pStaffId">Id cán bộ cần lấy thông tin Ex: THGA00000000166</param>
        /// <returns>Danh sách cán bộ NHCSXH</returns>
        public async Task<GenericResultInternalGateway<List<StaffVbspInforViewModel>>> GetListStaffByStaffId(string pStaffId)
        {
            const string apiEndpoint = "/internal/list-staff-vbsp-by-staffId";
            try
            {
                var requestUrl = $"{apiEndpoint}?pStaffId={Uri.EscapeDataString(pStaffId)}";
                _loggerInternalAPI.LogInformation("Calling internal API: {Url}", requestUrl);

                //var responseApi = await _clientAPI.GetAsync(sUrlApi);
                using var responseApi = await _clientAPI.GetAsync(requestUrl);

                if (!responseApi.IsSuccessStatusCode)
                {
                    _loggerInternalAPI.LogWarning("API returned non-success status code: {StatusCode}", responseApi.StatusCode);
                    return GenericResultInternalGateway<List<StaffVbspInforViewModel>>
                        .Fail($"Lỗi khi lấy dữ liệu từ API: {(int)responseApi.StatusCode} {responseApi.ReasonPhrase}");

                }
                // Đọc và deserialize trực tiếp từ stream để tối ưu bộ nhớ
                var contentApi = await responseApi.Content.ReadAsStringAsync();
                _loggerInternalAPI.LogInformation("API call success. Response length: {Length}", contentApi?.Length ?? 0);
                var listStaffs = JsonConvert.DeserializeObject<GenericResultInternalGateway<List<StaffVbspInforViewModel>>>(contentApi);

                return listStaffs;
            }
            catch (HttpRequestException httpEx)
            {
                _loggerInternalAPI.LogError(httpEx, $"Network error when calling GetListStaffByStaffId for StaffId: {pStaffId}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail("Lỗi kết nối đến API nội bộ.");
            }
            catch (JsonException jsonEx)
            {
                _loggerInternalAPI.LogError(jsonEx, $"JSON deserialization error when calling GetListStaffByStaffId for StaffId: {pStaffId}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail("Dữ liệu trả về từ API không đúng định dạng.");
            }
            catch (Exception ex)
            {
                _loggerInternalAPI.LogError(ex, $"Unexpected error in GetListStaffByStaffId for StaffId: {pStaffId}");
                return GenericResultInternalGateway<List<StaffVbspInforViewModel>>.Fail($"Đã xảy ra lỗi hệ thống khi lấy thông tin cán bộ VBSP.");
            }
        }

        /*
        https://localhost:7024/internal/list-staff-vbsp-by-staffId?pStaffId=THGA00000000166
        {
            "isSuccess": true,
            "code": 200,
            "message": "",
            "result": [
            {
                "id": 394,
                "mainPosCode": "000196",
                "mainPosName": "Trung tâm Công nghệ thông tin",
                "posCode": "000196",
                "posName": "Trung tâm Công nghệ thông tin",
                "staffId": "THGA00000000166",
                "staffCode": "03009",
                "staffName": "Dương Văn Chữ",
                "dateOfBirth": "1983-10-25T00:00:00",
                "genderCode": "M",
                "genderText": "Nam",
                "staffPosCode": "000196",
                "staffPosName": "Trung tâm Công nghệ thông tin",
                "staffDepartmentCode": "1561",
                "staffDepartmentName": "Phòng Phát triển phần mềm ứng dụng",
                "staffPositionCode": "1412",
                "staffPositionName": "Trưởng phòng",
                "staffMobileNo": "0908688212",
                "staffEmail": "chudv@vbsp.vn",
                "addressDetail": "Số 02 - Đồng Vinh - Chuyên Mỹ - TP. Hà Nội",
                "idNo": "001083010860",
                "issuedDate": "2024-01-29T00:00:00",
                "issuedPlace": "Bộ Công An",
                "degreeCode": "3802",
                "staffStatus": "1",
                "staffStatusText": "Lao động hợp đồng dài hạn đang làm việc",
                "departmentUnitCode": "000196.1561",
                "departmentUnitName": "Phòng Phát triển phần mềm ứng dụng Trung tâm Công nghệ Thông tin NHCSXH",
                "retirementDate": "1900-01-01T00:00:00",
                "notes": "",
                "createdBy": "system",
                "createdDate": "2026-03-10T13:17:08.68",
                "modifiedBy": "system",
                "modifiedDate": "2026-03-10T13:17:08.68"
            }
            ]
        }
         */

    }
}
