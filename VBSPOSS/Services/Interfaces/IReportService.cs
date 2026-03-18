namespace VBSPOSS.Services.Interfaces
{
    public interface IReportService
    {
        Task<string> GetTTLSCASA01(string documentId);
        
        Task<string> GetTTLSTIDE01(string listId,string circularRefNum);

      

        Task<string> GetTTLS_CASA_01(string listId, string circularRefNum);

        /// <summary>
        /// Hàm thực hiện in báo cáo Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="pListId">Danh sách Id bản ghi</param>
        /// <param name="pCircularRefNum">Số quyết định</param>
        /// <returns>File báo cáo trả về</returns>
        /// <exception cref="CustomException"></exception>
        /// <exception cref="Exception"></exception>
        Task<string> GetDepositPenalIntRateConfig(string pListId, string pCircularRefNum);
    }
}
