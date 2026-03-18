using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService, ILogger<ReportController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        {
            _reportService = reportService;
        }   

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GenerateFile(string id)
        {
            try
            {               
                var filePath = await _reportService.GetTTLSCASA01(id);
                // Lấy tên file từ đường dẫn
                string fileName = Path.GetFileName(filePath);

                // 3. Trả file về trình duyệt
                return PhysicalFile(filePath, "application/octet-stream", fileName);

            } catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return BadRequest($"Lỗi khi tạo file: {ex.Message}");
            }
           
        }


        [HttpPost]
        public async Task<IActionResult> GenerateToTrinh_Tide(string listId, string circularRefNum)
        {
            try
            {
                var filePath = await _reportService.GetTTLSTIDE01(listId, circularRefNum);
                // Lấy tên file từ đường dẫn
                string fileName = Path.GetFileName(filePath);

                // 3. Trả file về trình duyệt
                return PhysicalFile(filePath, "application/octet-stream", fileName);

            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return BadRequest($"Lỗi khi tạo file: {ex.Message}");
            }

        }


        // add Casa
        [HttpPost]
        public async Task<IActionResult> GenerateToTrinh_Casa(string listId, string circularRefNum)
        {
            try
            {
               
                var filePath = await _reportService.GetTTLS_CASA_01(listId, circularRefNum);  //

                string fileName = Path.GetFileName(filePath);

                // Quan trọng: Để file PDF hiển thị inline (mở trực tiếp) hoặc download với tên đúng (có tiếng Việt)
                // Nên set Content-Type chính xác là "application/pdf"
                // Và dùng ContentDisposition để encode tên file tiếng Việt đúng cách
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = false  // false = download, true = mở inline trong browser
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());
                Response.Headers.Add("X-File-Name", fileName);  // optional, hỗ trợ một số case

                return PhysicalFile(filePath, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tạo file: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện in báo cáo Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="pListId">Danh sách Id thông tin cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn</param>
        /// <param name="pCircularRefNum">Số quyết định cấu hình lãi suất</param>
        /// <returns>File tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn</returns>
        [HttpPost]
        public async Task<IActionResult> GenerateFileReport_DepositPenalIntRateConfig(string pListId, string pCircularRefNum)
        {
            try
            {
                var filePath = await _reportService.GetDepositPenalIntRateConfig(pListId, pCircularRefNum);
                // Lấy tên file từ đường dẫn
                string fileName = Path.GetFileName(filePath);

                // 3. Trả file về trình duyệt
                return PhysicalFile(filePath, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return BadRequest($"Lỗi khi tạo file tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn. Chi tiết lỗi: {ex.Message}");
            }
        }


    }
}
