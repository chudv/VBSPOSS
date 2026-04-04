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

        /// <summary>
        /// Hàm thực hiện cấp lại mật khẩu tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/resetUserPw
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu thành công
        ///     {
        ///         "emailAddress": "chudv.cctt@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "reset_by": "SYSTEMADMIN2",
        ///         "userId": "CHUV12",
        ///         "reset_at": "2026-01-14T21:55:10+00:00",
        ///         "mail_flag": "0",
        ///         "responseCode": "0",
        ///         "responseMsg": "Password Reset Successful"
        ///     }
        /// Nếu không thành công
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": "0",
        ///         "responseAttributes": { },
        ///         "responseCode": "5317",
        ///         "responseMsg": "ARX-005317: User does not exist.",
        ///         "status": "true"
        ///     }
        /// </returns>
        Task<ResetUserPasswordResponseResult> ResetUserPasswordByAPIResetUserPw(ViewUserRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API modifyUser thay đổi thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin người dùng Intellect iDC cần thay đổi thông tin 
        ///     {
        ///         "ticket": "{{access_token}}",
        ///         "userId": "CHUDV99",
        ///         "firstName": "Dương Văn",
        ///         "lastName": "Chữ",
        ///         "groupName": "POPGD",
        ///         "entityList": "IDCPRODC",
        ///         "mobileNumber": "0908688212",
        ///         "emailAddress": "chudv.2510@gmail.com",
        ///         "expiryDate": "2045-10-25",
        ///         "DOB": "1983-10-25",
        ///         "mailIdFlag": 1,
        ///         "language": "vi_VN",
        ///         "extraAttribute": {
        ///             "BranchCode": "2505",
        ///             "UserRole": "POPGD"
        ///         }
        ///     }
        /// </param>
        /// <returns>Kết quả trả về. Ex: 
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2505",
        ///         "userRole": "POPGD",
        ///         "responseCode": 0,
        ///         "responseMsg": "Modify User Done Successfully",
        ///         "status": "true"
        ///     }
        /// --Hoặc nếu sửa tiếp POS thì trả ra như sau:
        ///     {
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2502",
        ///         "userRole": "POPGD",
        ///         "status": "true",
        ///         "responseMsg": " BranchCode Modify Done Successfully",
        ///         "responseCode": 0
        ///     }
        /// </returns>
        Task<ModifyUserIDCResponseResult> ModifyUserIDCByAPIModifyUser(ModifyUserRequestViewModel requestInput);

        /// <summary>
        /// Hàm thực hiện gọi API idcPendingTxn/lmsPendingTxn Lấy danh sách giao dịch Pending theo người dùng
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/idcPendingTxn
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/lmsPendingTxn
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "userId": "68510"
        ///     }
        ///     Hoặc
        ///     {
        ///         "userId": "20047"
        ///     }
        /// 
        /// </param>
        /// <param name="pApiName">Tên API truyền vào. Nếu trống sẽ lấy cả 2 API vào (EsbApiName.LMSPendingTxn)</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu là idcPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "6",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:46:38"
        ///             },
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "7",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:47:17"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// Nếu là lmsPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// </returns>
        Task<PendingTransResponseResult> GetPendingTransactionsByAPIPendingTxn(PendingTransRequestViewModel requestInput, string pApiName);
    }
}