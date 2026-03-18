using System.Threading.Tasks;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;

namespace VBSPOSS.Integration.Interfaces
{
    public interface IApiInternalEsbService
    {
        /// <summary>
        /// Hàm lấy danh sách thông tin lãi suất của loại tài khoản của sản phẩm ở thời điểm gần nhất với ngày hiệu lực truyền vào.
        /// Gọi đến API ESB: http://10.63.54.52:7003/vbsp/internal/api/v1/getDepInterestRate
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "posCode": "0", "effectDate": "20210101", "sourceId": "MB", "userId": "IDCADMIN", "prodCode": "431" }</param>
        /// <returns>Danh sách bản tin</returns>
        Task<GenericListResultJava<TideIntRateResposeViewModel>> GetListDepositInterestRate(TideIntRateRequestViewModel requestInput);

        /// <summary>
        /// Hàm lấy danh sách thông tin lãi suất của loại tài khoản của sản phẩm CASA ở thời điểm gần nhất với ngày hiệu lực truyền vào.
        /// Gọi đến API ESB: http://10.63.54.52:7003/vbsp/internal/api/v1/getCasaIntRts
        /// </summary>
        /// <param name="request">Thông tin đầu vào. Ví dụ: { "posCode": "0", "effectDate": "20210101", "sourceId": "MB", "userId": "IDCADMIN", "prodCode": "431" }</param>
        /// <returns>Danh sách bản tin</returns>
        Task<GenericListRecordJava<CasaIntRateReposeViewModel>> GetListCasaInterestRate(CasaIntRateRequestViewModel requestInput);

        /// <summary>
        /// Hàm lấy danh sách thông tin lãi suất của sản phẩm Tide rút trước hạn
        /// Gọi đến API ESB: http://10.63.54.52:7003/vbsp/internal/api/v1/DepPenalIntRt
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "userId": "IDCADMIN", "productCode": "431", "currencyCode": "VND", "effDate": "20250520", "posCode": "0" }</param>
        /// <returns>Danh sách bản tin</returns>
        Task<GenericListRecordJava<DepositPenalIntRateReposeViewModel>> GetListDepositPenalInterestRate(DepositPenalIntRateRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API casaIntRates cập nhật thông tin cấu hình lãi suất Casa vào iDC thông qua API 
        /// http://10.63.54.52:7003/vbsp/internal/api/v1/casaIntRates
        /// </summary>
        /// <param name="requestInput">Thông tin cấu hình lãi suất</param>
        /// <returns>Kết quả trả về. Ex: { "respRecord": [ { "txnStatus": "SUCCESS", "reqRecordSl": "1", "responseCode": "00000", "responseMsg": " " } ] }</returns>
        Task<GenericStatusListResult> CasaIntRates(CasaIntRatesRequestViewModel requestInput);

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
        Task<GenericStatusListResult> TidePenalRates(TidePenalIntRatesRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API tideIntRates cập nhật thông tin cấu hình lãi suất tiền gửi có kỳ hạn Tide vào iDC thông qua API
        /// http://10.63.54.52:7003/vbsp/internal/api/v1/tideIntRates
        /// </summary>
        /// <param name="requestInput">Thông tin cấu hình lãi suất</param>
        /// <returns>Kết quả trả về. Ex: {"respRecord": [{"txnStatus": "SUCCESS","reqRecordSl": "1","responseCode": "00000","responseMsg": " "}]}</returns>
        Task<GenericStatusListResult> TideIntRates(TideIntRatesRequestViewModel requestInput);
    }
}