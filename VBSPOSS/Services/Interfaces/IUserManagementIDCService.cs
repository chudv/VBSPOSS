using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IUserManagementIDCService
    {
        /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <param name="pIsCallGetInfoCoreIDC">Cờ xác định: True: Sau khi lấy thông tin trong UserIDCMaster thì lấy tiếp trong iDC để ra thông tin cuối cùng của người dùng. False: Chỉ lấy thông tin trong UserIDCMaster</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        Task<List<UserIDCMasterViewModel>> GetListUserIDCMasters(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode, int pStatus, bool pIsCallGetInfoCoreIDC);

        /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserManagementIDC Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã Chi nhánh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Lấy tất cả truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pFunctionType">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với người dùng Intellect iDC (Không bắt buộc)</param>
        /// <param name="pIsJoinUserIDCMaster">Cờ xác định có Union Với UserIDCMaster không</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        Task<List<UserManagementIDCViewModel>> GetListUserIDCManagement(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode,
                                int pStatus, string pFunctionType, bool pIsJoinUserIDCMaster);

        /// <summary>
        /// Hàm thực hiện Xóa (Đóng) bản ghi nghiệp vụ thêm mới hoặc thay đổi thông tin tài khoản người dùng Intellect iDC (Bảng UserManagementIDC)
        /// </summary>
        /// <param name="pId">Chỉ số khóa bản ghi</param>
        /// <param name="pUserId">Tài khoản người dùng Intellect iDC</param>
        /// <param name="pStaffId">Id Cán bộ Có tài khoản</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1</param>
        /// <param name="pFunctionType">Nghiệp vụ thêm mới hoặc thay đổi thông tin người dùng Intellect iDC (Bắt buộc)</param>
        /// <param name="pModifiedBy">Username thực hiện</param>
        /// <param name="pFlagDelete">Cờ xác định Xóa/Đánh dấu xóa: 1: Xóa hẳn; 2: Đánh dấu xóa;</param>
        /// <returns>True - Thành công; False - Không thành công</returns>
        bool DeleteUserManagementIDC(long pId, string pUserId, string pStaffId, int pStatus, string pFunctionType, string pModifiedBy, int pFlagDelete);

        /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bản ghi bảng dữ liệu quản lý người dùng trên Intellect iDC UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        Task<long> SaveUserManagementIDC(UserManagementIDCViewModel pUserManagementUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm thực hiện cập nhật DocumentId (FileId) vào bảng UserManagementIDC - Cập nhật chỉ số xác định file tờ trình vào bảng UserManagementIDC
        /// </summary>
        /// <param name="pListIdUpdate">Danh sách Id bản ghi trong UserManagementIDC cần cập nhật DocumentId</param>
        /// <param name="pUserName">Người thực hiện</param>
        /// <param name="pFileId">Chuỗi Chỉ số xác định danh sách FileId (DocumentId) cần cập nhật vào bản ghi UserManagementIDC</param>
        /// <returns>Số lượng bản ghi được cập nhật </returns>
        Task<int> UpdateDocumentIdUserManagementIDC(List<long> pListIdUpdate, string pUserName, string pListFileId);

        /// <summary>
        /// Hàm thực hiện Trình duyệt bản ghi Yêu cầu về tài khoản người dùng Intellect iDC
        /// Cập nhật trạng thái các bản ghi sang trình duyệt Status = StatusBusinessFlow.Status_Submitted.Value
        /// </summary>
        /// <param name="pListUserIdApprove">Danh sách người dùng cần trình duyệt. Ví dụ: [{"Id":"101","UserId":"20032","Status":"2"},{"Id":"102","UserId":"20004","Status":"5"}]</param>
        /// <param name="pFunctionType">Mã loại yêu cầu về người dùng</param>
        /// <param name="pSystemDateText">Ngày hiện thời của máy chủ hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pUserNameUpd">Người thực hiện trình duyệt</param>
        /// <param name="pFlagCall">Cờ Trình duyệt/Phê duyệt. Giá trị: EventFlag.EventFlag_Approval.Value; EventFlag.EventFlag_Authorize.Value</param>
        /// <returns>Danh sách Id bản ghi được Update thành công</returns>
        /// <exception cref="Exception"></exception>
        Task<List<long>> UpdateStatusApproveUserManagementIDC(List<UserManagementIDCViewModel> pListUserIdApprove, string pFunctionType, string pSystemDateText,
                                    string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm tổng hợp số lượng yêu cầu của các chi nhánh về người dùng iDC để hiển thị hàng chờ phê duyệt
        /// </summary>
        /// <param name="pStartDateBegin">Ngày bắt đầu - Bắt đầu. Định dạng dd/MM/yyyy (Bắt buộc phải truyền)</param>
        /// <param name="pStartDateEnd">Ngày bắt đầu - Kết thúc. Định dạng dd/MM/yyyy (Bắt buộc phải truyền)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Bắt buộc phải truyền)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc phải truyền)</param>
        /// <param name="pUserGrade">Cấp User cần thống kê: 1 - PGD; 2 - Chi nhánh; 3 - TQ</param>
        /// <param name="pListStatus">Danh sách trạng thái truyền vào cách nhau bởi dấu phẩy. Ex: 1,5,2</param>
        /// <param name="pFlagCall">Cờ xác định cách tổng hợp (Chưa sử dụng)</param>
        /// <returns></returns>
        List<UserManagementIDCSumRequirementViewModel> UserManagementIDC_SumRequirement_GetSearch(string pStartDateBegin, string pStartDateEnd, string pMainPosCode,
            string pPosCode, int pUserGrade, string pListStatus, int pFlagCall);

        /// <summary>
        /// Hàm thực hiện Phê duyệt bản ghi Yêu cầu về tài khoản người dùng Intellect iDC
        ///     - Cập nhật trạng thái các bản ghi sang trình duyệt Status = StatusBusinessFlow.Status_Submitted.Value;
        ///     - Thực hiện gọi API vào Intellect iDC để tạo các yêu cầu;
        /// </summary>
        /// <param name="pListUserIdAuthorize">Danh sách người dùng cần phê duyệt. Ví dụ: [{"Id":"101","UserId":"20032","Status":"2"},{"Id":"102","UserId":"20004","Status":"5"}]</param>
        /// <param name="pFunctionType">Mã loại yêu cầu về người dùng</param>
        /// <param name="pSystemDateText">Ngày hiện thời của máy chủ hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pBusinessDateText">Ngày mở sổ của hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pUserNameUpd">Người thực hiện Phê duyệt</param>
        /// <param name="pUserGradeUpd">Cấp thực hiện: Phê duyệt. Giá trị: 
        ///                 1 - PGD (PosGrade.SUB_POS);
        ///                 2 - Chi nhánh (PosGrade.MAIN_POS);
        ///                 3 - TW (PosGrade.HEAD_POS)
        /// </param>
        /// <param name="pFlagCall">Cờ Trình duyệt/Phê duyệt. Giá trị: EventFlag.EventFlag_Approval.Value; EventFlag.EventFlag_Authorize.Value</param>
        /// <returns>Danh sách Id bản ghi được Update thành công</returns>
        /// <exception cref="Exception"></exception>
        Task<List<long>> SaveAuthorizeUserManagementIDC(List<UserManagementIDCViewModel> pListUserIdAuthorize, string pFunctionType, string pSystemDateText,
                                    string pBusinessDateText, string pUserNameUpd, int pUserGradeUpd, string pFlagCall);


        /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bảng dữ liệu người dùng trên Intellect iDC UserIDCMaster
        /// </summary>
        /// <param name="pUserIDCMasterUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        Task<long> SaveUserIDCMaster(UserIDCMasterViewModel pUserIDCMasterUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm lấy thông tin người dùng trên iDC qua việc gọi đến API viewUser của ESB đến iDC
        /// </summary>
        /// <param name="pUserId">Tên người dùng cần lấy. Ex 'CHUDV13'</param>
        /// <returns>Thông tin user ánh xạ vào Model ViewUserAPIReposeViewModel</returns>
        /// <exception cref="Exception"></exception>
        Task<ViewUserAPIReposeViewModel> GetUserIDCInfoByApiViewUser(string pUserId);

        /// <summary>
        /// Hàm thực hiện thêm mới tài khoản người dùng Intellect iDC => Gọi đến API addUser thêm mới thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin tài khoản người dùng Intellect iDC cần tạo</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về theo Model AddUserAPIResponseViewModel</returns>
        /// <exception cref="Exception"></exception>
        Task<AddUserAPIResponseViewModel> CreateUserIDCByApiAddUser(AddUserRequestViewModel requestInput, string pUserNameUpd);

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
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
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
        /// <exception cref="Exception"></exception>
        Task<TellerRoleAssignAPIResponseViewModel> ChangeRoleToTransferCashByApiTellerRoleAssign(TellerRoleAssignRequestViewModel requestInput, string pUserNameUpd);
        /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bảng dữ liệu quản lý người dùng trên Intellect iDC UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        Task<ChangeInforUserIDCAPIResponseViewModel> ChangeUserStatusByApiEnableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd);

        /// <summary>
        /// Hàm thực hiện Đóng/Khóa tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/disableUser
        /// Ví dụ cách sử dụng:
        ///     ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
        ///     requestInput.UserId = "CHUDV002";
        ///     requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
        ///     var objDisableUserResult = _userManagementIDCService.ChangeUserStatusByApiDisableUser(requestInput, UserName);
        ///     if (objDisableUserResult != null && objDisableUserResult.Result != null)
        ///     {
        ///         if (objDisableUserResult.Result.ResponseCode == "0" || objDisableUserResult.Result.ResponseCode == "00000")
        ///         {
        ///         }
        ///     }
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
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
        /// <exception cref="Exception"></exception>
        Task<ChangeInforUserIDCAPIResponseViewModel> ChangeUserStatusByApiDisableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd);

        /// <summary>
        /// Hàm thực hiện cấp lại mật khẩu tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/resetUserPw
        /// Ví dụ cách sử dụng:
        ///     ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
        ///     requestInput.UserId = "CHUDV002";
        ///     requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
        ///     var objResetUserPwUserResult = _userManagementIDCService.ResetUserPasswordByApiResetUserPw(requestInput, UserName);
        ///     if (objResetUserPwUserResult != null && objResetUserPwUserResult.Result != null)
        ///     {
        ///         if (objResetUserPwUserResult.Result.ResponseCode == "0" || objResetUserPwUserResult.Result.ResponseCode == "00000")
        ///         {
        ///         }
        ///     }
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
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
        /// <exception cref="Exception"></exception>
        Task<ChangeInforUserIDCAPIResponseViewModel> ResetUserPasswordByApiResetUserPw(ViewUserRequestViewModel requestInput, string pUserNameUpd);

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
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
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
        /// <exception cref="Exception"></exception>
        Task<ChangeInforUserIDCAPIResponseViewModel> ModifyUserByApiModifyUser(ModifyUserRequestViewModel requestInput, string pUserNameUpd);
        //List<UserIDCApprovalViewModel> UserIDCApproval_GetSearch(string pNgayHLBatDau,string pNgayHLKetThuc,string pDonVi, int pFlagCall, string pTrangThai);

        //List<UserManagementIDCViewModel> GetListUserIDCManagement(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode, string pFunctionType, int iStatus);



        /// <summary>
        /// Hàm kiểm tra xem người dùng có mở sổ tiền mặt đầu ngày không
        /// Ex: SELECT VBSP_OSS_GET.FN_CHECK_OPENCASH_BY_USERID('44573', '03-SEP-2025') FROM DUAL
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng trên iDC</param>
        /// <param name="pReportDate">Ngày kiểm tra định dạng dd-MON-yyyy</param>
        /// <returns>Kết quả trả về:
        ///                 0 - Chưa mở sổ tiền mặt đầu ngày;
        ///                 1 - Đã mở chưa đóng;
        ///                 2 - Đã mở và đóng nhưng còn tồn quỹ tiền mặt chưa chuyển về quỹ chính
        ///                 3 - Đã mở và đóng không còn tồn quỹ tiền mặt
        /// </returns>
        int CheckOpenCashByUserId(string pUserId, string pReportDate);
        
        //Task<List<long>> SaveAttachedFiles(long configureId, List<AttachedFileInfo> attachedFiles, string userId);

        /// <summary>
        /// Hàm xóa thông tin phân quyền chức năng của người dùng trên iDC khi người dùng bị khóa tài khoản hoặc xóa tài khoản trên iDC. Thực hiện xóa bản ghi trong bảng AuthSecType theo UserId
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng cần hủy xác thực 2 bước (Xóa xác thực bằng OTP)</param>
        /// <returns>Kết quả</returns>
        Task<ExecuteResultModelModel> DeleteAuthSecTypeByUserIdAsync(string pUserId);

        /// <summary>
        /// Hàm thực hiện đăng ký hoặc hủy đăng ký xác thực 2 lớp (Lớp user/password và OTP) cho người dùng (Tương đương hàm IDL_ARX.PRC_OTP_REG của Intellect)
        ///     Nếu đăng ký: Thêm bản ghi vào bảng IDL_ARX.TB_ARM_USER_AUTH_TYPE với AUTH_TYPE_ID = 17
        ///     Nếu hủy đăng ký: Xóa bản ghi bảng IDL_ARX.TB_ARM_USER_AUTH_TYPE với AUTH_TYPE_ID = 17
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng cần đăng ký/hủy đăng ký xác thực 2 bước (Xác thực lớp 2 bằng OTP)</param>
        /// <param name="pRegisterFlag">Cờ xác định: 1-Đăng ký; 0 - Hủy đăng ký</param>
        /// <returns>Kết quả</returns>
        Task<ExecuteResultModelModel> ChangeOTPRegisterByUserId(string pUserId, int pRegisterFlag);

        Task<long> SaveApproveUserManagementIDC(UserManagementIDCViewModel pUserManagementUpd, string pUserNameUpd, string pFlagCall, string pButtonType);
    }
}
