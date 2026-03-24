using System.Net.Mail;
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

        [JsonProperty("extraAttributeResponse")]
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
        public int UserType { get; set; }               //Loại người dùng (IDL_ARX.TB_ARM_USER_TYPE@VBSPCBSLINK). Giá trị quy ước: 0: Bank; 1: Corporate; 2: Retail

        [JsonProperty("encryptExtraAttrib")]
        public bool EncryptExtraAttrib { get; set; }               //Giá trị mặc định false

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userIdentifierAlias")]
        public string UserIdentifierAlias { get; set; }         //Giá trị là 'All'

        [JsonProperty("userStatus")]
        public int UserStatus { get; set; }                     //Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active

        [JsonProperty("secondaryChoicebasedAuthType")]
        public int SecondaryChoicebasedAuthType { get; set; }   //Giá trị là '0'

        [JsonProperty("prevStatus")]
        public int PrevStatus { get; set; }   //Trạng thái trước đó của User. Giá trị là -7

        [JsonProperty("appendRole")]
        public bool AppendRole { get; set; }                //Giá trị là false

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

        [JsonProperty("currLoginDate")]
        public string CurrLoginDate { get; set; }           //Ngày giờ login gần nhất (2026 03 24 031929)
    }

    public class ServiceStatusResponse
    {
        [JsonProperty("sessionValReq")]
        public string SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public string PrevStatus { get; set; }

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
    


    public class UserInfoIDCViewModelcs
    {
    }
}
