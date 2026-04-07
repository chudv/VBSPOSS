using System.DirectoryServices.Protocols;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Telerik.SvgIcons;

namespace VBSPOSS.Integration.ViewModel
{
    /// <summary>
    /// Đầu vào cho API lấy thông tin người dùng Intellect IDC: http://10.63.54.51:7003/vbsp/internal/api/v1/viewUser
    ///         {
    ///             "ticket": "",    
    ///             "userId": "CHUV13"
    ///         }
    /// </summary>
    public class ViewUserRequestViewModel
    {
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }
    }

    /// <summary>
    /// Đầu ra của API truy vấn thông tin người dùng Intellect IDC: http://10.63.54.51:7003/vbsp/internal/api/v1/viewUser
    /// </summary>
    public class ViewUserReposeViewModel
    {
        /// <summary>
        /// Ngày thay đổi mật khẩu gần nhất. Định dạng yyyy-MM-dd
        /// </summary>
        [JsonProperty("lastPWDChanged")]
        public string LastPWDChanged { get; set; }

        [JsonProperty("primaryChoicebasedAuthType")]
        public string PrimaryChoicebasedAuthType { get; set; }

        [JsonProperty("serviceStatusResponse")]
        public ServiceStatusResponse ServiceStatusResponseViewModel { get; set; }

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("tranAuthType")]
        public string TranAuthType { get; set; }

        [JsonProperty("reqNo")]
        public string ReqNo { get; set; }

        [JsonProperty("selfRegistration")]
        public bool SelfRegistration { get; set; }

        [JsonProperty("fromRecord")]
        public string FromRecord { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }        //en_US | vi_VN

        [JsonProperty("userCreatedDate")]
        public string UserCreatedDate { get; set; }     //Ngày tạo người dùng.Định dạng yyyy-MM-dd

        /// <summary>
        /// Tên công ty dành cho người dùng là doanh nghiệp.Giá trị đang là '0'
        /// </summary>
        [JsonProperty("corporateName")]
        public string CorporateName { get; set; }       //Tên công ty dành cho người dùng là doanh nghiệp.Giá trị đang là '0'

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }        //Địa chỉ email của người dùng

        [JsonProperty("authsecType")]
        public string AuthsecType { get; set; }         //Phương thức xác thực thứ 2. Giá trị mặc định '0'

        [JsonProperty("DOB")]
        public string DOB { get; set; }                 //Ngày sinh của người dùng.Định dạng yyyy-MM-dd

        [JsonProperty("invalidAttempt")]
        public string InvalidAttempt { get; set; }      //Số lần đăng nhập sai

        [JsonProperty("userFromService")]
        public bool UserFromService { get; set; }      //Không sử dụng, mặc định là false

        [JsonProperty("extraAttribute")]
        public ExtraAttributeResponse ExtraAttributeResponseViewModel { get; set; }

        [JsonProperty("nickName")]
        public string NickName { get; set; }      //Tên tài khoản người dùng cần lấy thông tin

        [JsonProperty("defaultBranch")]
        public string DefaultBranch { get; set; }      //Chỉ định chi nhánh mặc định cho người dùng sẽ được tạo. Ex: 'IDCPRODC'

        [JsonProperty("hpinFlag")]
        public int HpinFlag { get; set; }      //Có sử dụng hard PIN không, mặc định là 0

        [JsonProperty("reqNumber")]
        public int ReqNumber { get; set; }      //Không sử dụng, mặc định là 0

        [JsonProperty("toRecord")]
        public int ToRecord { get; set; }      //Không sử dụng, mặc định là 0

        [JsonProperty("appendEntity")]
        public bool AppendEntity { get; set; }      //Không sử dụng, mặc định là false

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }               //Nhóm quyền của LMS/ FAMS, COLLATERAL (Trừ phần hệ Core). Ex: IDCROLE,GRPLMSIT,GRPCLMSIT

        [JsonProperty("additionalInfoMap")]
        public JsonElement AdditionalInfoMap { get; set; }      //Danh sách rỗng

        [JsonProperty("isWebSealUser")]
        public bool IsWebSealUser { get; set; }               //Mặc định là false

        [JsonProperty("entityList")]
        public string EntityList { get; set; }               //Entity quản lý người dùng. Ex: UATVBSP hoặc Bank/IDCPRODC. Ex: 'IDCPRODC'

        [JsonProperty("userIdentifierName")]
        public string UserIdentifierName { get; set; }               //Giá trị là mặc định 'All'. Tùy theo lựa chọn có thể là: All/Functional User/Administrator/Retail

        [JsonProperty("operationType")]
        public int OperationType { get; set; }               //Giá trị là -1

        [JsonProperty("userType")]
        public int UserType { get; set; }               //Loại người dùng (IDL_ARX.TB_ARM_USER_TYPE@VBSPCBSLINK). Giá trị quy ước: 1: Bank; 2: Corporate; 3: Retail

        [JsonProperty("encryptExtraAttrib")]
        public bool EncryptExtraAttrib { get; set; }               //Giá trị mặc định false

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userIdentifierAlias")]
        public string UserIdentifierAlias { get; set; }         //Giá trị là 'All'

        /// <summary>
        /// Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active
        /// </summary>
        [JsonProperty("userStatus")]
        public int UserStatus { get; set; }                     //Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active

        [JsonProperty("secondaryChoicebasedAuthType")]
        public int SecondaryChoicebasedAuthType { get; set; }   //Giá trị là '0'

        [JsonProperty("prevStatus")]
        public int PrevStatus { get; set; }   //Trạng thái trước đó của User. Giá trị là -7

        [JsonProperty("appendRole")]
        public bool AppendRole { get; set; }                //Giá trị là false

        /// <summary>
        /// Lần cuối cùng login vào hệ thống (yyyyMMddHHmmss)
        /// </summary>
        [JsonProperty("lastLoginDate")]
        public string LastLoginDate { get; set; }             //Lần cuối cùng login vào hệ thống (yyyyMMddHHmmss)

        [JsonProperty("authTypeAttrib")]
        public JsonElement AuthTypeAttrib { get; set; }      //Danh sách rỗng

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }      //Ngày hết hiệu lực của người dùng, định dạng yyyy-MM-dd

        [JsonProperty("checkerDate")]
        public string CheckerDate { get; set; }      //Ngày duyệt tạo người dùng, định dạng yyyy-MM-dd

        /// <summary>
        /// Cờ xác định cấp mật khẩu cho người dùng. Giá trị: 
        ///         '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
        ///         '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
        ///         '2': Mật khẩu được gửi link vào email của người dùng
        ///         '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        /// Chú ý: Đối với các role có quyền tiền mặt gồm: POGD, POPGD, TKTTT, TKTTQ, TKTCB, CNGD, CNPGD, PKTTP, PKTPP, PKTTM, PKTTQ, SGDTQ, SGDTM, SGDPP, SGDTP, SGDPG, SGDGD, TTGD, TTKT, TTTQ, TTTKT, DTGD, DTKT, DTTQ, DTTKT, VPGD, VPKT, VPTQ thì bắt buộc Gía trị MailIdFlag = 4. Các role còn lại mặc định MailIdFlag = 0
        /// </summary>
        [JsonProperty("mailIdFlag")]
        public string MailIdFlag { get; set; }

        /// <summary>
        /// Phương thức đăng nhập. Giá trị: -1: Super (Áp dụng cho user hệ thống không đăng nhập được);
        ///                                 1: Native (Bình thường Mật khẩu); 2: LDAP; 3: Safeword; 10: SMS OTP(Citi MFA)
        /// </summary>
        [JsonProperty("authType")]
        public int AuthType { get; set; }

        [JsonProperty("credInfoEncryptType")]
        public int CredInfoEncryptType { get; set; }        //Giá trị là '0'

        [JsonProperty("makerId")]
        public string MakerId { get; set; }                 //Người tạo tài khoản người dùng

        [JsonProperty("reqActivity")]
        public int ReqActivity { get; set; }        //Giá trị là '0'

        [JsonProperty("extraAttribs")]
        public JsonElement ExtraAttribs { get; set; }      //Danh sách rỗng

        [JsonProperty("makerDate")]
        public string MakerDate { get; set; }      //Ngày tạo tài khoản người dùng, định dạng yyyy-MM-dd

        [JsonProperty("appendEntityRoleMap")]
        public bool AppendEntityRoleMap { get; set; }       //Giá trị là false

        [JsonProperty("salt")]
        public string Salt { get; set; }                    //Giá trị là 'dummysalt'

        [JsonProperty("userId")]
        public string UserId { get; set; }                  //Tài khoản người dùng như trường nickName

        [JsonProperty("checkerId")]
        public string CheckerId { get; set; }               //Người duyệt tạo người dùng

        /// <summary>
        /// Ngày giờ login gần nhất (2026 03 24 031929). Định dạng yyyyMMddHHmmss
        /// </summary>
        [JsonProperty("currLoginDate")]
        public string CurrLoginDate { get; set; }           //Ngày giờ login gần nhất (2026 03 24 031929)
    }

    public class ServiceStatusResponse
    {
        [JsonProperty("sessionValReq")]
        public bool SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public int PrevStatus { get; set; }

        [JsonProperty("responseAttributes")]
        public JsonElement ResponseAttributes { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class ExtraAttributeResponse
    {
        [JsonProperty("UserRole")]
        public string UserRole { get; set; }        //Nhóm quyền trên IDC(Không bao gồm Lending). Ex: POGD

        [JsonProperty("BranchCode")]
        public string BranchCode { get; set; }      //Mã POS của người dùng. Ex: 101
    }

    /// <summary>
    /// Model cho Request gọi API addUser để gọi API http://10.63.54.51:7003/vbsp/internal/api/v1/addUser. Ví dụ:
    ///     {
    ///          "ticket": "",    
    ///          "userId": "CHUDV001",
    ///          "nickName": "CHUDV001",
    ///          "firstName": "Dương",
    ///          "lastName": "Văn Chữ",
    ///          "emailAddress": "chudv.cctt@gmail.com",
    ///          "mobileNumber": "0908688212",
    ///          "DOB": "1983-10-25",
    ///          "groupName": "POGD",
    ///          "entityList": "IDCPRODC",
    ///          "authType": 1,
    ///          "userType": 1,
    ///          "mailIdFlag": 4,
    ///          "expiryDate": "2060-12-31",
    ///          "extraAttribute": {
    ///                 "BranchCode": "2505",
    ///                 "UserRole": "POGD"
    ///          }
    ///      }
    /// </summary>
    public class AddUserRequestViewModel
    {
        [JsonProperty("ticket")]
        public string Ticket;

        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("nickName")]
        public string NickName;

        [JsonProperty("firstName")]
        public string FirstName;

        [JsonProperty("lastName")]
        public string LastName;

        [JsonProperty("emailAddress")]
        public string EmailAddress;

        [JsonProperty("mobileNumber")]
        public string MobileNumber;

        /// <summary>
        /// Định dạng ngày sinh: yyyy-MM-dd     
        /// </summary>
        [JsonProperty("DOB")]
        public string DateOfBirth;

        [JsonProperty("groupName")]
        public string GroupName;

        [JsonProperty("entityList")]
        public string EntityList;

        /// <summary>
        /// Phương thức đăng nhập. Giá trị:
        ///         -1: Super(Áp dụng cho user hệ thống không đăng nhập được);
        ///         1: Native(Bình thường Mật khẩu);
        ///         2: LDAP;
        ///         3: Safeword;
        ///         10: SMS OTP (Citi MFA)
        /// </summary>
        [JsonProperty("authType")]
        public int AuthType;

        /// <summary>
        /// Loại người dùng: 1: Bank; 2: Corporate; 3: Retail
        /// </summary>
        [JsonProperty("userType")]
        public int UserType;

        /// <summary>
        /// Cờ xác định cấp mật khẩu cho người dùng. Giá trị: 
        ///         '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
        ///         '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
        ///         '2': Mật khẩu được gửi link vào email của người dùng
        ///         '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        /// Chú ý: Đối với các role có quyền tiền mặt gồm: POGD, POPGD, TKTTT, TKTTQ, TKTCB, CNGD, CNPGD, PKTTP, PKTPP, PKTTM, PKTTQ, SGDTQ, SGDTM, SGDPP, SGDTP, SGDPG, SGDGD, TTGD, TTKT, TTTQ, TTTKT, DTGD, DTKT, DTTQ, DTTKT, VPGD, VPKT, VPTQ thì bắt buộc Gía trị MailIdFlag = 4. Các role còn lại mặc định MailIdFlag = 0
        /// </summary>
        [JsonProperty("mailIdFlag")]
        public int MailIdFlag;

        /// <summary>
        /// Ngày hết hiệu lực người dùng. Định dạng yyyy-MM-dd. Giá trị mặc định: 2050-12-31
        /// </summary>
        [JsonProperty("expiryDate")]
        public string ExpiryDate;

        [JsonProperty("extraAttribute")]
        public AddUserExtraAttributeRequest AddUserExtraAttributeRequestViewModel { get; set; }

        [JsonProperty("ipSet")]
        public string IpSet;

        /// <summary>
        /// Phương thức xác thực thứ 2. Người dùng có quyền tiền mặt là 17, Còn lại là 0
        /// </summary>
        [JsonProperty("authsecType")]
        public string AuthsecType;

        /// <summary>
        /// Loại user. Giá trị là 1
        /// </summary>
        [JsonProperty("subType")]
        public string SubType;

        /// <summary>
        /// Ngày bắt đầu (yyyyMMdd) lớn hơn hoặc bằng ngày hiện tại
        /// </summary>
        [JsonProperty("startDate")]
        public string StartDate;

        /// <summary>
        /// Cờ hạn chế đăng nhập cho tất cả các ngày giống nhau hay không. Giá trị: 1 - Hạn chế tất cả các ngày giống nhau; 0 - Hạn chế với các ngày có giá trị khác nhau
        /// </summary>
        [JsonProperty("restrictSameTimeForAllDay")]
        public int? RestrictSameTimeForAllDay;

        [JsonProperty("restriction")]
        public List<RestrictionRequest> ListRestrictionRequest{ get; set; }
    }

    public class AddUserExtraAttributeRequest
    {
        [JsonProperty("BranchCode")]
        public string BranchCode { get; set; }      //Mã POS của người dùng. Ex: 101

        [JsonProperty("UserRole")]
        public string UserRole { get; set; }        //Nhóm quyền trên IDC(Không bao gồm Lending). Ex: POGD

    }

    public class RestrictionRequest
    {
        /// <summary>
        /// Ngày cho phép đăng nhập. Giá trị quy ươc: Nếu SameTimeForAllDays = 0 thì sẽ có giá trị từ 1 đến 7; Nếu SameTimeForAllDays = 0 thì chỉ nhận giá trị là 8;
        /// </summary>
        [JsonProperty("ALLOWED_DAYS")]
        public string AllowedDays { get; set; }

        /// <summary>
        /// Giờ bắt đầu giới hạn. Định dạng HH24:Mi. Ví dụ "08:00"
        /// </summary>
        [JsonProperty("START_RESTRICTION")]
        public string StartRestriction { get; set; }
        /// <summary>
        /// Giờ kết thúc giới hạn. Định dạng HH24:Mi. Ví dụ "17:00"
        /// </summary>
        [JsonProperty("END_RESTRICTION")]
        public string EndRestriction { get; set; }
    }


    /// <summary>
    /// Model cho Request gọi API tellerRoleAssign để gọi API gán hoặc bỏ gán quyền tiền mặt cho người dùng đăng nhập Intellect iDC
    ///         http://10.63.54.51:7003/vbsp/internal/api/v1/tellerRoleAssign => Ví dụ:
    ///     {
    ///         "tellerId": "CHUDV002",
    ///         "tellerRoleAllowed": "0",
    ///         "mkrId": "IDCADMIN"
    ///     }
    /// </summary>
    public class TellerRoleAssignRequestViewModel
    {
        /// <summary>
        /// Tài khoản người dùng trên iDC cần gán/bỏ gán quyền tiền mặt. Ex: 'CHUDV002'
        /// </summary>
        [JsonProperty("tellerId")]
        public string TellerId;

        /// <summary>
        /// Cờ xác nhận gán hay bỏ quyền tiền mặt cho người dùng (Flag để ghi nhận assign teller role hay không). Giá trị:
        ///             1: Allow (Gán quyền tiền mặt cho người dùng);
        ///             0: Block (Gỡ quyền tiền mặt của người dùng);
        /// </summary>
        [JsonProperty("tellerRoleAllowed")]
        public int TellerRoleAllowed;

        /// <summary>
        /// Tài khoản người thực hiện assign teller role (Thường user hệ thống). Ex: IDCADMIN có thể dùng ConstValueAPI.UserId_Call_ApiIDC
        /// </summary>
        [JsonProperty("mkrId")]
        public string MkrId;
    }


    /// <summary>
    /// Model cho Request gọi API modifyUser để gọi API http://10.63.54.51:7003/vbsp/internal/api/v1/modifyUser. Ví dụ:
    /// {
    ///     "ticket": "{{access_token}}",
    ///     "userId": "CHUDV99",
    ///     "firstName": "Dương Văn",
    ///     "lastName": "Chữ",
    ///     "groupName": "POPGD",
    ///     "entityList": "IDCPRODC",
    ///     "mobileNumber": "0908688212",
    ///     "emailAddress": "chudv.2510@gmail.com",
    ///     "expiryDate": "2045-10-25",
    ///     "DOB": "1983-10-25",
    ///     "mailIdFlag": 1,
    ///     "language": "vi_VN",
    ///     "extraAttribute": {
    ///         "BranchCode": "2505",
    ///         "UserRole": "POPGD"
    ///     }
    /// }
    /// {
    ///     "sessionValReq": "true",
    ///     "prevStatus": 0,
    ///     "responseAttributes": { },
    ///     "mobileNumber": "0908688212",
    ///     "posCode": "2505",
    ///     "userRole": "POPGD",
    ///     "responseCode": 0,
    ///     "responseMsg": "Modify User Done Successfully",
    ///     "status": "true"
    /// }
    /// --Hoặc nếu sửa tiếp POS thì trả ra như sau:
    /// {
    ///     "mobileNumber": "0908688212",
    ///     "posCode": "2502",
    ///     "userRole": "POPGD",
    ///     "status": "true",
    ///     "responseMsg": " BranchCode Modify Done Successfully",
    ///     "responseCode": 0
    /// }
    /// </summary>
    public class ModifyUserRequestViewModel
    {
        [JsonProperty("ticket")]
        public string Ticket;

        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("nickName")]
        public string NickName;

        [JsonProperty("firstName")]
        public string FirstName;

        [JsonProperty("lastName")]
        public string LastName;

        [JsonProperty("groupName")]
        public string GroupName;

        [JsonProperty("entityList")]
        public string EntityList;

        [JsonProperty("mobileNumber")]
        public string MobileNumber;

        [JsonProperty("emailAddress")]
        public string EmailAddress;

        /// <summary>
        /// Ngày hết hiệu lực người dùng. Định dạng yyyy-MM-dd. Giá trị mặc định: 2050-12-31
        /// </summary>
        [JsonProperty("expiryDate")]
        public string ExpiryDate;

        /// <summary>
        /// Định dạng ngày sinh: yyyy-MM-dd
        /// </summary>
        [JsonProperty("DOB")]
        public string DateOfBirth;

        ///// <summary>
        ///// Cờ xác định cấp mật khẩu cho người dùng. Giá trị: 
        /////         '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
        /////         '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
        /////         '2': Mật khẩu được gửi link vào email của người dùng
        /////         '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        ///// Chú ý: Đối với các role có quyền tiền mặt gồm: POGD, POPGD, TKTTT, TKTTQ, TKTCB, CNGD, CNPGD, PKTTP, PKTPP, PKTTM, PKTTQ, SGDTQ, SGDTM, SGDPP, SGDTP, SGDPG, SGDGD, TTGD, TTKT, TTTQ, TTTKT, DTGD, DTKT, DTTQ, DTTKT, VPGD, VPKT, VPTQ thì bắt buộc Gía trị MailIdFlag = 4. Các role còn lại mặc định MailIdFlag = 0
        ///// </summary>
        //[JsonProperty("mailIdFlag")]
        //public int MailIdFlag;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("extraAttribute")]
        public AddUserExtraAttributeRequest AddUserExtraAttributeRequestViewModel { get; set; }

        [JsonProperty("ipSet")]
        public string IpSet;

        /// <summary>
        /// Phương thức xác thực thứ 2. Người dùng có quyền tiền mặt là 17, Còn lại là 0
        /// </summary>
        [JsonProperty("authsecType")]
        public string AuthsecType;

        /// <summary>
        /// Loại user. Giá trị là 1
        /// </summary>
        [JsonProperty("subType")]
        public string SubType;

        /// <summary>
        /// Ngày bắt đầu (yyyyMMdd) lớn hơn hoặc bằng ngày hiện tại
        /// </summary>
        [JsonProperty("startDate")]
        public string StartDate;

        /// <summary>
        /// Cờ hạn chế đăng nhập cho tất cả các ngày giống nhau hay không. Giá trị: 1 - Hạn chế tất cả các ngày giống nhau; 0 - Hạn chế với các ngày có giá trị khác nhau
        /// </summary>
        [JsonProperty("restrictSameTimeForAllDay")]
        public string RestrictSameTimeForAllDay;

        [JsonProperty("restriction")]
        public List<RestrictionRequest> ListRestrictionRequest { get; set; }
    }


    /// <summary>
    /// Model cho Request gọi API idcPendingTxn/lmsPendingTxn để gọi API lấy danh sách giao dịch Pending
    ///         http://10.63.54.51:7003/vbsp/internal/api/v1/lmsPendingTxn => Ví dụ:
    ///     {
    ///         "userId": "68510"
    ///     }
    /// </summary>
    public class PendingTransRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId;
    }




    public class UserInfoIDCViewModelcs
    {
    }
}
