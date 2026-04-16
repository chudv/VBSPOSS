namespace VBSPOSS.Services.Interfaces
{
    public interface IReportService
    {
        Task<string> GetTTLSCASA01(string documentId);


        /// <summary>
        /// In báo cáo qua API Report Gateway - Tờ trình cấu hình lãi suất sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="listId">Danh sách Id</param>
        /// <param name="circularRefNum">Số quyết định</param>
        /// <param name="circularDate">Ngày quyết định yyyyMMdd</param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        /// <exception cref="Exception"></exception>
        Task<string> GetTTLSTIDE01(string listId,string circularRefNum, string circularDate, string posCode);


        /// <summary>
        /// In báo cáo qua API Report Gateway - Tờ trình cấu hình lãi suất sản phẩm tiền gửi không kỳ hạn (CASA)
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="circularRefNum"></param>
        /// <param name="circularDate"></param>
        /// <param name="posCode"></param>
        /// <returns></returns>
        Task<string> GetTTLS_CASA_01(string listId, string circularRefNum, string circularDate, string posCode);

        /// <summary>
        /// Hàm thực hiện in báo cáo Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="pListId">Danh sách Id bản ghi</param>
        /// <param name="pCircularRefNum">Số quyết định</param>
        /// <returns>File báo cáo trả về</returns>
        /// <exception cref="CustomException"></exception>
        /// <exception cref="Exception"></exception>
        Task<string> GetDepositPenalIntRateConfig(string pListId, string pCircularRefNum, string circularDate, string posCode);
        /// <summary>
        /// Hàm thực hiện in báo cáo Tờ trình người dùng IDC
        /// </summary>
        /// <param name="pId">List Id người dùng</param>
        /// <param name="pMainPosName">Tên chi nhánh</param>
        /// <returns>File báo cáo trả về</returns>
        /// <exception cref="CustomException"></exception>
        /// <exception cref="Exception"></exception>
        Task<string> GetUserIDCReport(string pId, string pMainPosName, string pFunctionType);

    }

}
