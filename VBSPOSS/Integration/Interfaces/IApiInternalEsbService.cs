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



        /// <summary>
        /// Hàm lấy thông tin người dùng trên iDC qua API
        /// Gọi đến API ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/viewUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ví dụ: { "ticket": "", "userId": "DUYEN002" }</param>
        /// <returns>Thông tin người dùng cần lấy</returns>
        Task<GenericListRecordJava<ViewUserReposeViewModel>> GetUserIDCInfoByApiViewUser(ViewUserRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API addUser thêm mới thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin người dùng Intellect iDC cần thêm mới</param>
        /// <returns>Kết quả trả về. Ex: 
        /// {
        ///     "sessionValReq": "true",
        ///     "prevStatus": 0,
        ///     "responseAttributes": {
        ///         "USR_PASSWD": "s5j5SNHw"
        ///     },
        ///     "responseCode": 0,
        ///     "responseMsg": "User Successfully Registered",
        ///     "status": "true"
        /// }
        /// </returns>
        Task<UserIDCResponseResult> CreateUserIDCByAPIAddUser(AddUserRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API tellerRoleAssign gán hoặc bỏ gán quyền tiền mặt cho người dùng đăng nhập Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/tellerRoleAssign
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "tellerId": "CHUDV002",
        ///         "tellerRoleAllowed": "1",
        ///         "mkrId": "IDCADMIN"
        ///     }
        /// </param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "txnStatus": "Success",
        ///         "responseMsg": "API Invocation Success",
        ///         "responseCode": "00000"
        ///     }
        ///     {
        ///         "txnStatus": "FAILED",
        ///         "responseMsg": "INVALID TELLER ID",
        ///         "responseCode": ""
        ///     }
        /// </returns>
        Task<TellerRoleAssignResponseResult> ChangeRoleToTransferCashByAPITellerRoleAssign(TellerRoleAssignRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện Mở/Kích hoạt lại tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/enableUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "enabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "enabled_at": "2026-03-27T10:06:40+00:00",
        ///         "responseCode": 0,
        ///         "responseMsg": "Enable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already enabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        Task<ChangeUserStatusResponseResult> ChangeUserStatusByAPIEnableUser(ViewUserRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện Đóng/Khóa tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/disableUser
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "disabled_at": "2026-03-27T10:06:40+00:00",
        ///         "disabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "responseCode": 0,
        ///         "responseMsg": "Disable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already disabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        Task<ChangeUserStatusResponseResult> ChangeUserStatusByAPIDisableUser(ViewUserRequestViewModel requestInput);



    }
}