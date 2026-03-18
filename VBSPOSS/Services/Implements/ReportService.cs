using VBSPOSS.ExceptionHandling;
using VBSPOSS.Helpers;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Services.Implements
{
    public class ReportService : IReportService
    {
        private IWebHostEnvironment _hostingEnvironment;

        private readonly IApiReportGateway _apiReportGateway;

        public ReportService(IWebHostEnvironment hostingEnvironment, IApiReportGateway apiReportGateway)
        {
            _hostingEnvironment = hostingEnvironment;
            _apiReportGateway = apiReportGateway;   
        }   

        public async Task<string> GetTTLSCASA01(string documentId)
        {
            try
            {
                //Định nghĩa thuộc tính chung
                string dirSourceReport = $@"{_hostingEnvironment.ContentRootPath}\Documents\Reports\";
                string dirDesReport = $@"{_hostingEnvironment.ContentRootPath}\wwwroot\Reports\";

                if (!FileHelper.IsDirectory(dirSourceReport))
                    Directory.CreateDirectory(dirSourceReport);
                if (!FileHelper.IsDirectory(dirDesReport))
                    Directory.CreateDirectory(dirDesReport);

                //string fileTem = "TempCreditNote.html"; //Tên file nguồn báo cáo
                //string fileName = "";
                //string html = "";
                //string genReports = "";
                //string Content = DateTime.Now.ToString("ddMMyyyyHHmmss");

                //fileName = $"CreditNote{DateTime.Now:ddMMyyyyHHmmss}.pdf"; // Tên File khi xuất báo cáo

               

                ReportInput _reportInput = new ReportInput { ReportId = "BC0111111", FileType = "PDF" };               

                List<Parameter> parameters = new List<Parameter>();
                parameters.Add(new Parameter() { ParaName = "documentId", ParaType = "string", ParaValue = $"{documentId}" });
                //parameters.Add(new Parameter() { ParaName = "PARA_TONGHOP", ParaType = "", ParaValue = "N" });
                //parameters.Add(new Parameter() { ParaName = "PARA_MATO", ParaType = "", ParaValue = $"{groupId}" });
                //parameters.Add(new Parameter() { ParaName = "PARA_MAPGD", ParaType = "", ParaValue = $"{posCode}" });
                //parameters.Add(new Parameter() { ParaName = "PARA_MAKH", ParaType = "", ParaValue = $"{customerId}" });
                //parameters.Add(new Parameter() { ParaName = "PARA_API", ParaType = "", ParaValue = "Y" });
                _reportInput.Parameters = new List<Parameter>(parameters);

                GenericResultReportGateway<ReportResultDto> _response = _apiReportGateway.GetReport(_reportInput);

                if (_response.Success)
                {
                    byte[] pdfBytes = Convert.FromBase64String(_response.Result.Data);
                    string filePath = $@"{dirSourceReport}{_response.Result.FileName}";
                    File.WriteAllBytes(filePath, pdfBytes);
                    return filePath;
                }
                else
                {
                    throw new CustomException($"Lỗi tạo file báo cáo {_reportInput.ReportId}: {_response.Message}", 404);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                throw new Exception("Lỗi khi lấy báo cáo TTLSCASA01", ex);
            }
            
        }



        public async Task<string> GetTTLSTIDE01(string listId, string circularRefNum)
        {
            try
            {
                //Định nghĩa thuộc tính chung
                string dirSourceReport = $@"{_hostingEnvironment.ContentRootPath}\Documents\Reports\";
                string dirDesReport = $@"{_hostingEnvironment.ContentRootPath}\wwwroot\Reports\";

                if (!FileHelper.IsDirectory(dirSourceReport))
                    Directory.CreateDirectory(dirSourceReport);
                if (!FileHelper.IsDirectory(dirDesReport))
                    Directory.CreateDirectory(dirDesReport);
             

                ReportInput _reportInput = new ReportInput { ReportId = "BC0111112", FileType = "PDF" };

                List<Parameter> parameters = new List<Parameter>();
                parameters.Add(new Parameter() { ParaName = "Id", ParaType = "string", ParaValue = $"{listId}" });
                parameters.Add(new Parameter() { ParaName = "CircularRefNum", ParaType = "string", ParaValue = $"{circularRefNum}" });               
                _reportInput.Parameters = new List<Parameter>(parameters);

                GenericResultReportGateway<ReportResultDto> _response = _apiReportGateway.GetReport(_reportInput);

                if (_response.Success)
                {
                    byte[] pdfBytes = Convert.FromBase64String(_response.Result.Data);
                    string filePath = $@"{dirSourceReport}{_response.Result.FileName}";
                    File.WriteAllBytes(filePath, pdfBytes);
                    return filePath;
                }
                else
                {
                    throw new CustomException($"Lỗi tạo file báo cáo {_reportInput.ReportId}: {_response.Message}", 404);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                throw new Exception("Lỗi khi lấy báo cáo TTLSCASA01", ex);
            }
        }


        // thêm 
        public async Task<string> GetTTLS_CASA_01(string listId, string circularRefNum)
        {
            try
            {
                string dirSourceReport = $@"{_hostingEnvironment.ContentRootPath}\Documents\Reports\";
                string dirDesReport = $@"{_hostingEnvironment.ContentRootPath}\wwwroot\Reports\";
                if (!FileHelper.IsDirectory(dirSourceReport))
                    Directory.CreateDirectory(dirSourceReport);
                if (!FileHelper.IsDirectory(dirDesReport))
                    Directory.CreateDirectory(dirDesReport);

                // 
               
                ReportInput _reportInput = new ReportInput { ReportId = "BC0111111", FileType = "PDF" };

                List<Parameter> parameters = new List<Parameter>();
                parameters.Add(new Parameter() { ParaName = "Id", ParaType = "string", ParaValue = $"{listId}" });
                parameters.Add(new Parameter() { ParaName = "CircularRefNum", ParaType = "string", ParaValue = $"{circularRefNum}" });
                _reportInput.Parameters = new List<Parameter>(parameters);

                GenericResultReportGateway<ReportResultDto> _response = _apiReportGateway.GetReport(_reportInput);
                if (_response.Success)
                {
                    byte[] pdfBytes = Convert.FromBase64String(_response.Result.Data);
                    string filePath = $@"{dirSourceReport}{_response.Result.FileName}";
                    File.WriteAllBytes(filePath, pdfBytes);
                    return filePath;
                }
                else
                {
                    throw new CustomException($"Lỗi tạo file báo cáo {_reportInput.ReportId}: {_response.Message}", 404);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy báo cáo TTLS_CASA_01", ex);
            }
        }


        /// <summary>
        /// Hàm thực hiện in báo cáo Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="pListId">Danh sách Id bản ghi</param>
        /// <param name="pCircularRefNum">Số quyết định</param>
        /// <returns>File báo cáo trả về</returns>
        /// <exception cref="CustomException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetDepositPenalIntRateConfig(string pListId, string pCircularRefNum)
        {
            try
            {
                //Định nghĩa thuộc tính chung
                string dirSourceReport = $@"{_hostingEnvironment.ContentRootPath}\Documents\Reports\";
                string dirDesReport = $@"{_hostingEnvironment.ContentRootPath}\wwwroot\Reports\";

                if (!FileHelper.IsDirectory(dirSourceReport))
                    Directory.CreateDirectory(dirSourceReport);
                if (!FileHelper.IsDirectory(dirDesReport))
                    Directory.CreateDirectory(dirDesReport);
                

                ReportInput objReportInput = new ReportInput { ReportId = "BC0111113", FileType = "PDF" };

                List<Parameter> parameters = new List<Parameter>();
                parameters.Add(new Parameter() { ParaName = "Id", ParaType = "string", ParaValue = $"{pListId}" });
                parameters.Add(new Parameter() { ParaName = "CircularRefNum", ParaType = "string", ParaValue = $"{pCircularRefNum}" });
                objReportInput.Parameters = new List<Parameter>(parameters);

                GenericResultReportGateway<ReportResultDto> _response = _apiReportGateway.GetReport(objReportInput);

                if (_response.Success)
                {
                    byte[] pdfBytes = Convert.FromBase64String(_response.Result.Data);
                    string filePath = $@"{dirSourceReport}{_response.Result.FileName}";
                    File.WriteAllBytes(filePath, pdfBytes);
                    return filePath;
                }
                else
                {
                    throw new CustomException($"Lỗi tạo file báo cáo cấu hình lãi suất rút trước hạn sản phẩm tiền gửi CKH: {objReportInput.ReportId}: {_response.Message}", 404);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy báo cáo cấu hình lãi suất rút trước hạn sản phẩm tiền gửi CKH", ex);
            }
        }


    }
}
