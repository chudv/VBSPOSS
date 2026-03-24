using System;
using Newtonsoft.Json;
using VBSPOSS.Integration.Model;

namespace VBSPOSS.ViewModels
{
    public class UserManagementIDCViewModel
    {
        public long Id { get; set; }
        public string FunctionType { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string StaffId { get; set; }
        public string StaffCode { get; set; }
        public string UserId { get; set; }
        public string NickName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }            //Thêm
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string GroupName { get; set; }
        public string EntityList { get; set; }
        public string AuthType { get; set; }
        public string UserType { get; set; }
        public string MailIdFlag { get; set; }
        public string AuthsecType { get; set; }
        public string ExtraAttributeUserRole { get; set; }
        public string ExtraAttributeBranchCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Ticket { get; set; }
        public string Remark { get; set; }
        public string TiOrtherNotescket { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }            //Thêm
        public int StatusUpdateCore { get; set; }
        public bool SessionValReq { get; set; }
        public string PrevStatus { get; set; }
        public string ResponseAttributes { get; set; }
        public string CallApiStatus { get; set; }
        public int CallApiReqRecordSl { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
    }


    public class UserIDCMasterViewModel
    {
        public long OrderNo { get; set; }           //Thêm
        public long Id { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string StaffId { get; set; }
        public string StaffCode { get; set; }
        public string UserId { get; set; }
        public string NickName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string GroupName { get; set; }
        public string EntityList { get; set; }
        public string AuthType { get; set; }
        public string UserType { get; set; }
        public string MailIdFlag { get; set; }
        public string AuthsecType { get; set; }
        public string ExtraAttributeUserRole { get; set; }
        public string ExtraAttributeBranchCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Remark { get; set; }
        public string OrtherNotes { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }            //Thêm
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
    }


    public class ViewUserAPIReposeViewModel
    {
        /// <summary>
        /// Trạng thái trả về của API. Dù không tìm User truyền vào thì giá trị vẫn là true
        /// </summary>
        public bool ServiceStatusResponseSessionValReq { get; set; }
        
        /// <summary>
        /// Cho biết trạng thái trước đó của người dùng. Giá trị mặc định là 0 (Indicates previous status of User)
        /// </summary>
        public int ServiceStatusResponsePrevStatus { get; set; }

        /// <summary>
        /// 0-Thành công; 4654 - Không tìm thấy người dùng
        /// </summary>
        public string ServiceStatusResponseResponseCode { get; set; }

        /// <summary>
        /// Kết quả. Ex: 'ARX-004654: Please Provide correct User ID ' hoặc 'User Information Successfully displayed'
        /// </summary>
        public string ServiceStatusResponseResponseMsg { get; set; }

        /// <summary>
        /// Trạng thái trả ra. true/false
        /// </summary>
        public bool ServiceStatusResponseStatus { get; set; }

        /// <summary>
        /// Ngày thay đổi mật khẩu gần nhất. Định dạng yyyy-MM-dd
        /// </summary>
        public string LastPWDChanged { get; set; }

        /// <summary>
        /// Không sử dụng, mặc định là 0
        /// </summary>
        public string PrimaryChoicebasedAuthType { get; set; }

        public string MobileNumber { get; set; }

        public string TranAuthType { get; set; }

        public string ReqNo { get; set; }

        public bool SelfRegistration { get; set; }

        public string FromRecord { get; set; }

        public string Language { get; set; }        //en_US | vi_VN

        public string UserCreatedDate { get; set; }     //Ngày tạo người dùng.Định dạng yyyy-MM-dd
        /// <summary>
        /// Tên công ty dành cho người dùng là doanh nghiệp.Giá trị đang là '0'
        /// </summary>
        public string CorporateName { get; set; }       //Tên công ty dành cho người dùng là doanh nghiệp.Giá trị đang là '0'

        public string EmailAddress { get; set; }        //Địa chỉ email của người dùng
        /// <summary>
        /// Phương thức xác thực thứ 2. Giá trị mặc định '0'
        /// </summary>
        public string AuthsecType { get; set; }         //Phương thức xác thực thứ 2. Giá trị mặc định '0'

        public string DOB { get; set; }                 //Ngày sinh của người dùng.Định dạng yyyy-MM-dd

        public string InvalidAttempt { get; set; }      //Số lần đăng nhập sai

        public bool UserFromService { get; set; }      //Không sử dụng, mặc định là false

        public string UserRole { get; set; }        //Nhóm quyền trên IDC(Không bao gồm Lending). Ex: POGD

        public string BranchCode { get; set; }      //Mã POS của người dùng. Ex: 101

        public string NickName { get; set; }      //Tên tài khoản người dùng cần lấy thông tin

        public string DefaultBranch { get; set; }      //Chỉ định chi nhánh mặc định cho người dùng sẽ được tạo. Ex: 'IDCPRODC'

        public int HpinFlag { get; set; }      //Có sử dụng hard PIN không, mặc định là 0

        public int ReqNumber { get; set; }      //Không sử dụng, mặc định là 0

        public int ToRecord { get; set; }      //Không sử dụng, mặc định là 0

        public bool AppendEntity { get; set; }      //Không sử dụng, mặc định là false

        public string FirstName { get; set; }

        public string GroupName { get; set; }               //Nhóm quyền của LMS/ FAMS, COLLATERAL (Trừ phần hệ Core). Ex: IDCROLE,GRPLMSIT,GRPCLMSIT

        public bool IsWebSealUser { get; set; }               //Mặc định là false

        public string EntityList { get; set; }               //Entity quản lý người dùng. Ex: UATVBSP hoặc Bank/IDCPRODC. Ex: 'IDCPRODC'

        public string UserIdentifierName { get; set; }               //Giá trị là mặc định 'All'. Tùy theo lựa chọn có thể là: All/Functional User/Administrator/Retail

        public int OperationType { get; set; }               //Giá trị là -1

        public int UserType { get; set; }               //Loại người dùng (IDL_ARX.TB_ARM_USER_TYPE@VBSPCBSLINK). Giá trị quy ước: 0: Bank; 1: Corporate; 2: Retail

        public bool EncryptExtraAttrib { get; set; }               //Giá trị mặc định false

        public string LastName { get; set; }

        public string UserIdentifierAlias { get; set; }         //Giá trị là 'All'
        
        /// <summary>
        /// Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active
        /// </summary>
        public int UserStatus { get; set; }                     //Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active

        public int SecondaryChoicebasedAuthType { get; set; }   //Giá trị là '0'

        public int PrevStatus { get; set; }             //Trạng thái trước đó của User. Giá trị là -7

        public bool AppendRole { get; set; }            //Giá trị là false

        /// <summary>
        /// Lần cuối cùng login vào hệ thống (yyyyMMddHHmmss)
        /// </summary>
        public string LastLoginDate { get; set; }       //Lần cuối cùng login vào hệ thống (yyyyMMddHHmmss)

        public string ExpiryDate { get; set; }          //Ngày hết hiệu lực của người dùng, định dạng yyyy-MM-dd

        public string CheckerDate { get; set; }         //Ngày duyệt tạo người dùng, định dạng yyyy-MM-dd

        /// <summary>
        /// Cờ xác định cấp mật khẩu cho người dùng. Giá trị: 
        ///         '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
        ///         '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
        ///         '2': Mật khẩu được gửi link vào email của người dùng
        ///         '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        /// Chú ý: Đối với các role có quyền tiền mặt gồm: POGD, POPGD, TKTTT, TKTTQ, TKTCB, CNGD, CNPGD, PKTTP, PKTPP, PKTTM, PKTTQ, SGDTQ, SGDTM, SGDPP, SGDTP, SGDPG, SGDGD, TTGD, TTKT, TTTQ, TTTKT, DTGD, DTKT, DTTQ, DTTKT, VPGD, VPKT, VPTQ thì bắt buộc Gía trị MailIdFlag = 4. Các role còn lại mặc định MailIdFlag = 0
        /// </summary>
        public string MailIdFlag { get; set; }

        /// <summary>
        /// Phương thức đăng nhập. Giá trị: -1: Super (Áp dụng cho user hệ thống không đăng nhập được);
        ///                                 1: Native (Bình thường Mật khẩu); 2: LDAP; 3: Safeword; 10: SMS OTP(Citi MFA)
        /// </summary>
        public int AuthType { get; set; }

        public int CredInfoEncryptType { get; set; }        //Giá trị là '0'

        public string MakerId { get; set; }                 //Người tạo tài khoản người dùng

        public int ReqActivity { get; set; }        //Giá trị là '0'

        public string MakerDate { get; set; }      //Ngày tạo tài khoản người dùng, định dạng yyyy-MM-dd

        public bool AppendEntityRoleMap { get; set; }       //Giá trị là false

        public string Salt { get; set; }                    //Giá trị là 'dummysalt'

        public string UserId { get; set; }                  //Tài khoản người dùng như trường nickName

        public string CheckerId { get; set; }               //Người duyệt tạo người dùng

        /// <summary>
        /// Ngày giờ login gần nhất (2026 03 24 031929). Định dạng yyyyMMddHHmmss
        /// </summary>
        public string CurrLoginDate { get; set; }           //Ngày giờ login gần nhất (2026 03 24 031929)
    }

}
