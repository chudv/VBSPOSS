using System;
using System.Reflection.Emit;
using Telerik.SvgIcons;
using VBSPOSS.Models;

namespace VBSPOSS.Constants
{
    public class Common
    {
        public static string UploadDirFileDocument = @"wwwroot/Uploads";
        public const string FirstNameFile = "FinlitPhotoInContent";
        public const string UploadDirPhotoInContent = @"Upload/PhotoInContents";

    }

    /// <summary>
    /// Cờ xác định sự kiện
    /// </summary>
    public class EventFlag
    {
        /// <summary>
        /// Sự kiện đánh dấu xóa: EventFlag_MarkDeleted = new ValueConstModel { Value = 0, Code = "MARKDELETED", Description = "Đánh dấu xóa" };
        /// </summary>
        public static ValueConstModel EventFlag_MarkDeleted = new ValueConstModel { Value = 0, Code = "MARKDELETED", Description = "Đánh dấu xóa" };
        
        /// <summary>
        /// Sự kiện thêm mới: EventFlag_Add = new ValueConstModel { Value = 1, Code = "ADD", Description = "Thêm mới" };
        /// </summary>
        public static ValueConstModel EventFlag_Add = new ValueConstModel { Value = 1, Code = "ADD", Description = "Thêm mới" };
        
        /// <summary>
        /// Sự kiện chỉnh sửa: EventFlag_Edit = new ValueConstModel { Value = 2, Code = "EDIT", Description = "Chỉnh sửa" };
        /// </summary>
        public static ValueConstModel EventFlag_Edit = new ValueConstModel { Value = 2, Code = "EDIT", Description = "Chỉnh sửa" };

        /// <summary>
        /// Sự kiện xóa hẳn bản ghi: EventFlag_Delete = new ValueConstModel { Value = 3, Code = "DELETE", Description = "Xóa bỏ" };
        /// </summary>
        public static ValueConstModel EventFlag_Delete = new ValueConstModel { Value = 3, Code = "DELETE", Description = "Xóa bỏ" };

        /// <summary>
        /// Sự kiện gọi trình duyệt: EventFlag_Approval = new ValueConstModel { Value = 3, Code = "APPROVAL", Description = "Trình duyệt" };
        /// </summary>
        public static ValueConstModel EventFlag_Approval = new ValueConstModel { Value = 4, Code = "APPROVAL", Description = "Trình duyệt" };

        /// <summary>
        /// Sự kiện gọi phê duyệt: EventFlag_Authorize = new ValueConstModel { Value = 5, Code = "AUTHORIZE", Description = "Phê duyệt" };
        /// </summary>
        public static ValueConstModel EventFlag_Authorize = new ValueConstModel { Value = 5, Code = "AUTHORIZE", Description = "Phê duyệt" };

        /// <summary>
        /// Sự kiện gọi phê duyệt: EventFlag_View = new ValueConstModel { Value = 6, Code = "VIEW", Description = "Xem chi tiết" };
        /// </summary>
        public static ValueConstModel EventFlag_View = new ValueConstModel { Value = 6, Code = "VIEW", Description = "Xem chi tiết" };

                /// <summary>
        /// Sự kiện gọi phê duyệt: EventFlag_View = new ValueConstModel { Value = 6, Code = "VIEW", Description = "Xem chi tiết" };
        /// </summary>
        public static ValueConstModel EventFlag_EditIDC = new ValueConstModel { Value = 7, Code = "EDITIDC", Description = "Yêu cầu chỉnh sửa trên IDC" };

        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => EventFlag_MarkDeleted,
                1 => EventFlag_Add,
                2 => EventFlag_Edit,
                3 => EventFlag_Delete,
                4 => EventFlag_Approval,
                5 => EventFlag_Authorize,
                6 => EventFlag_View,
                7 => EventFlag_EditIDC,
                _ => null
            };
        }
    }

    /// <summary>
    /// Phân nhóm sản phẩm cấu hình lãi suất: Tide / Casa / DepositPenal
    /// </summary>
    public class ProductGroupCode
    {
        public static ValueConstModel CASA = new ValueConstModel { Value = 1, Code = "CASA", Description = "Lãi suất tiền gửi không kỳ hạn" };
        public static ValueConstModel TIDE = new ValueConstModel { Value = 2, Code = "TIDE", Description = "Lãi suất tiền gửi có kỳ hạn" };
        public static ValueConstModel DEPOSITPENAL = new ValueConstModel { Value = 3, Code = "DEPOSITPENAL", Description = "Lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn" };

        public const string ProductGroupCode_Casa = "CASA";

        public const string ProductGroupCode_Tide = "TIDE";

        public const string ProductGroupCode_DepositPenal = "DEPOSITPENAL";
    }
    /// <summary>
    /// Trạng thái bản ghi: Tạo lập/Phê duyệt/Từ chối/
    /// </summary>
    public class StatusTrans
    {
        public static ValueConstModel Status_Closed = new ValueConstModel { Value = 0, Code = "Closed", Description = "Đóng" };
        public static ValueConstModel Status_Created = new ValueConstModel { Value = 1, Code = "Created", Description = "Tạo lập" };
        public static ValueConstModel Status_Process = new ValueConstModel { Value = 2, Code = "Process", Description = "Chờ duyệt" };
        public static ValueConstModel Status_Authorized = new ValueConstModel { Value = 3, Code = "Authorized", Description = "Phê duyệt" };
        public static ValueConstModel Status_Rejected = new ValueConstModel { Value = 4, Code = "Rejected", Description = "Từ chối" };
        public static ValueConstModel Status_Modified = new ValueConstModel { Value = 5, Code = "Modified", Description = "Chỉnh sửa" };
        
        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => Status_Closed,
                1 => Status_Created,
                2 => Status_Process,
                3 => Status_Authorized,
                4 => Status_Rejected,
                5 => Status_Modified,
                _ => null
            };
        }

        public static ValueConstModel Status_CallApi_Updated = new ValueConstModel { Value = 1, Code = "CallApi_Updated", Description = "Đã cập nhật vào iDC" };
        public static ValueConstModel Status_CallApi_NotUpdated = new ValueConstModel { Value = 0, Code = "CallApi_NotUpdated", Description = "Chưa cập nhật vào iDC" };

        public const int StatusClosed = 0;

        public const int StatusCreated = 1;

        public const int StatusModified = 2;

        public const int StatusAuthorized = 3;

        public const int StatusRejected = 4;

        public const int StatusCallApi_NotUpdated = 0;

        public const int StatusCallApi_Updated = 1;
    }

    /// <summary>
    /// Cờ nhận diện tính chất sản phẩm/loại tài khoản ghi nợ hay có. Giá trị: C/D
    /// </summary>
    public class DebitCreditFlag
    {
        public static ValueConstModel CreditFlag = new ValueConstModel { Value = 0, Code = "C", Description = "Dư có" };
        public static ValueConstModel DebitFlag = new ValueConstModel { Value = 1, Code = "D", Description = "Dư nợ" };
        public static string DebitCreditFlag_Debit = "D";
        public static string DebitCreditFlag_Credit = "C";
    }

    /// <summary>
    /// Loại file của nghiệp vụ nào. Giá trị quy ước:
    ///         1 - File cấu hình lãi suất Tide/Casa/DepositPenal; 
    ///         2 - File đính kèm của người dùng iDC;
    /// </summary>
    public class FileType
    {
        public static ValueConstModel FileType_ConfigIntRate = new ValueConstModel { Value = 1, Code = "IntRate", Description = "File cấu hình lãi suất Tide/Casa/DepositPenal" };
        public static ValueConstModel FileType_User_IDC = new ValueConstModel { Value = 2, Code = "User_IDC", Description = "File đính kèm của người dùng iDC" };
    }


    //
    /// <summary>
    /// Giá trị cho việc gọi API
    /// </summary>
    public class ConstValueAPI
    {
        public const string SourceId = "MB";
        public const string DebitCreditFlagDefault = "C";
        public const string CurrencyValueDefault = "VND";
        public const string Ticket = "";
        public const string EntityList_Code = "EntityList";
        public const string UserId_Call_ApiIDC_Code = "UserIdCallAPIIDC";
    }

    public class DepositType
    {
        public const string BeforeOfTerm = "B"; // Đầu ký
        public const string PartitalTerm = "P"; // Định kỳ
        public const string OnTerm = "E"; // Định kỳ
    }

    /// <summary>
    /// Kỳ hạn: D - Ngày, M - Tháng, Y - Năm
    /// </summary>
    public class TenureUnit
    {
        public const string Tenure_Daily = "D"; 
        public const string Tenure_Monthly = "M";
        public const string Tenure_Yearly = "Y";
        public const string Tenure_Daily_Text = "Ngày";
        public const string Tenure_Monthly_Text = "Tháng";
        public const string Tenure_Yearly_Text = "Năm";
    }

    /// <summary>
    /// Cấp Phòng giao dịch; Chi nhánh; Toàn quốc
    /// </summary>
    public static class PosGrade
    {
        /// <summary>
        /// Cấp Phòng giao dịch
        /// </summary>
        public const int SUB_POS = 1;

        /// <summary>
        /// Cấp Chi nhánh
        /// </summary>
        public const int MAIN_POS = 2;

        /// <summary>
        /// Cấp Toàn quốc (Hội sở chính)
        /// </summary>
        public const int HEAD_POS = 3;


        /// <summary>
        /// Cấp Phòng giao dịch
        /// </summary>
        public const string SUB_POS_NAME = "Phòng giao dịch";

        /// <summary>
        /// Cấp Chi nhánh
        /// </summary>
        public const string MAIN_POS_NAME = "Chi nhánh";

        /// <summary>
        /// Cấp Toàn quốc (Hội sở chính)
        /// </summary>
        public const string HEAD_POS_NAME = "Hội sở chính";


        /// <summary>
        /// Cấp Phòng giao dịch
        /// </summary>
        public const string PosGrade_SubPos = "S";

        /// <summary>
        /// Cấp Chi nhánh
        /// </summary>
        public const string PosGrade_MainPos = "M";

        /// <summary>
        /// Cấp Toàn quốc (Hội sở chính)
        /// </summary>
        public const string PosGrade_HeadPos = "H";
    }

    /// <summary>
    /// Trạng thái bản ghi: Đóng/Mở/Phê duyệt/Từ chói/Chỉnh sửa
    /// </summary>
    public static class StatusLov
    {
        /// <summary>
        /// Trạng thái Mở/Bình thường/Hoạt động
        /// </summary>
        public const int StatusOpen = 1;

        /// <summary>
        /// Trạng thái Đóng/Tạm xóa
        /// </summary>
        public const int StatusClosed = 0;

        /// <summary>
        /// Trạng thái đã phê duyệt
        /// </summary>
        public const int StatusAccept = 3;

        /// <summary>
        /// Trạng thái đã Từ chối
        /// </summary>
        public const int StatusReject = 4;

        /// <summary>
        /// Trạng thái yêu cầu chỉnh sửa
        /// </summary>
        public const int StatusEdit = 2;

        /// <summary>
        /// Trạng thái Mở/Bình thường/Hoạt động
        /// </summary>
        public const string StatusOpen_Text = "Mở";

        /// <summary>
        /// Trạng thái Đóng/Tạm xóa
        /// </summary>
        public const string StatusClosed_Text = "Đóng";

        /// <summary>
        /// Trạng thái đã phê duyệt
        /// </summary>
        public const string StatusAccept_Text = "Phê duyệt";

        /// <summary>
        /// Trạng thái đã Từ chối
        /// </summary>
        public const string StatusReject_Text = "Từ chối";

        /// <summary>
        /// Trạng thái yêu cầu chỉnh sửa
        /// </summary>
        public const string StatusEdit_Text = "Chỉnh sửa";

        /// <summary>
        /// Trạng thái Mở/Bình thường/Hoạt động của POS
        /// </summary>
        public const string StatusOpenPOS = "O";

        /// <summary>
        /// Trạng thái Đóng/Tạm xóa của POS
        /// </summary>
        public const string StatusClosedPOS = "C";


        public const string StatusYes = "Y";
        public const string StatusYesText = "Có";
        public const string StatusNo = "N";
        public const string StatusNoText = "Không";
    }

    /// <summary>
    /// Kiểu in của bản ghi: Thường; Nghiêng; Đậm; Nghiêng đậm; Gạch chân
    /// </summary>
    public static class PrintTypeValue
    {
        /// <summary>
        /// Defines the BoldValue.
        /// </summary>
        public const int BoldValue = 1;

        /// <summary>
        /// Defines the ItalicValue.
        /// </summary>
        public const int ItalicValue = 2;

        /// <summary>
        /// Defines the NormalValue.
        /// </summary>
        public const int NormalValue = 3;

        /// <summary>
        /// Defines the ItalicBoldValue.
        /// </summary>
        public const int ItalicBoldValue = 4;

        /// <summary>
        /// Defines the UnderlineValue.
        /// </summary>
        public const int UnderlineValue = 5;

        /// <summary>
        /// Defines the BoldValue.
        /// </summary>
        public const string BoldValue_Text = "Chữ đậm";

        /// <summary>
        /// Defines the ItalicValue.
        /// </summary>
        public const string ItalicValue_Text = "Chữ nghiêng";

        /// <summary>
        /// Defines the NormalValue.
        /// </summary>
        public const string NormalValue_Text = "Chữ thường";

        /// <summary>
        /// Defines the ItalicBoldValue.
        /// </summary>
        public const string ItalicBoldValue_Text = "Chữ nghiêng đậm";

        /// <summary>
        /// Defines the UnderlineValue.
        /// </summary>
        public const string UnderlineValue_Text = "Chữ gạch chân";
    }

    /// <summary>
    /// LOV quy định trực thuộc cấp nào đối với danh mục Phòng ban/Chức vụ dùng cho đơn vị nào: HSC, Chi nhánh, PGD, TTCNTT,...
    /// </summary>
    public static class CodeOfLovUsed
    {
        /// <summary>
        /// Áp dụng cho Hội sở chính
        /// </summary>
        public const string CodeOfLovUsed_Head = "1";

        /// <summary>
        /// Áp dụng cho Chi nhánh Tỉnh/TP
        /// </summary>
        public const string CodeOfLovUsed_Branch = "2";

        /// <summary>
        /// Áp dụng cho văn phòng miền
        /// </summary>
        public const string CodeOfLovUsed_DomainOffice = "3";

        /// <summary>
        /// Áp dụng cho PGD NHCSXH Quận/Huyện
        /// </summary>
        public const string CodeOfLovUsed_District = "4";

        /// <summary>
        /// Áp dụng cho TTCNTT
        /// </summary>
        public const string CodeOfLovUsed_ITC = "5";

        /// <summary>
        /// Cơ sở đào tạo
        /// </summary>
        public const string CodeOfLovUsed_TrainingFacility = "6";

        /// <summary>
        /// Áp dụng cho Trung tâm Đào tạo
        /// </summary>
        public const string CodeOfLovUsed_TrainingCenter = "7";

        /// <summary>
        /// Áp dụng cho Sở giao dịch
        /// </summary>
        public const string CodeOfLovUsed_BankTransactionOffice = "8";
    }

    /// <summary>
    /// LOV quy định trực thuộc cấp nào đối với danh mục Phòng ban/Chức vụ dùng cho đơn vị nào: HSC, Chi nhánh, PGD, TTCNTT,...
    /// </summary>
    public static class CodeOfLovUsedText
    {
        /// <summary>
        /// Áp dụng cho Hội sở chính
        /// </summary>
        public const string CodeOfLovUsed_Head = "Hội sở chính";

        /// <summary>
        /// Áp dụng cho Chi nhánh Tỉnh/TP
        /// </summary>
        public const string CodeOfLovUsed_Branch = "Chi nhánh Tỉnh/TP";

        /// <summary>
        /// Áp dụng cho văn phòng miền
        /// </summary>
        public const string CodeOfLovUsed_DomainOffice = "Văn phòng miền";

        /// <summary>
        /// Áp dụng cho PGD NHCSXH Quận/Huyện
        /// </summary>
        public const string CodeOfLovUsed_District = "PGD NHCSXH Quận/Huyện";

        /// <summary>
        /// Áp dụng cho TTCNTT
        /// </summary>
        public const string CodeOfLovUsed_ITC = "TTCNTT";

        /// <summary>
        /// Cơ sở đào tạo
        /// </summary>
        public const string CodeOfLovUsed_TrainingFacility = "Cơ sở đào tạo";

        /// <summary>
        /// Áp dụng cho Trung tâm Đào tạo
        /// </summary>
        public const string CodeOfLovUsed_TrainingCenter = "TTĐT";

        /// <summary>
        /// Áp dụng cho Sở giao dịch
        /// </summary>
        public const string CodeOfLovUsed_BankTransactionOffice = "Sở Giao dịch";
    }

    /// <summary>
    /// Vùng kinh tế
    /// </summary>
    public static class RegionValue
    {
        public const string DongBangSongHong = "01";

        public const string TrungDuMienNuiPhiaBac = "02";

        public const string BacTrungBoDuyenHaiMienTrung = "03";

        public const string TayNguyen = "04";

        public const string DongNamBo = "05";

        public const string DongBangSongCL = "06";
    }
    
    /// <summary>
    /// Vùng kinh tế
    /// </summary>
    public static class RegionValueText
    {
        public const string DongBangSongHong = "Đồng bằng sông Hồng";

        public const string TrungDuMienNuiPhiaBac = "Trung du và Miền núi phía Bắc";

        public const string BacTrungBoDuyenHaiMienTrung = "Bắc Trung Bộ và Duyên hải miền Trung";

        public const string TayNguyen = "Tây Nguyên";

        public const string DongNamBo = "Đông Nam Bộ";

        public const string DongBangSongCL = "Đồng bằng sông Cửu Long";
    }

    /// <summary>
    /// Defines the <see cref="TypeLOVValue" />.
    /// </summary>
    public static class TypeLOVValue
    {
        /// <summary>
        /// Defines the PrintType.
        /// </summary>
        public const string PrintType = "PrintType";

        public const string CodeOfLovUsed = "CodeOfLovUsed";

        public const string RegionList = "Region";
    }

    public static class PosValue
    {
        public const string HEAD_POS = "000100";

        public const string BANK_WIDE = "0";
        
        public const string SYSTEM_WIDE = "000000";
       
    }

    public static class DefaultValue
    {
        /// <summary>
        /// Defines the StatusOpen.
        /// </summary>
        public const int StatusOpen = 1;

        /// <summary>
        /// Defines the StatusAccept in CN.
        /// </summary>
        public const string StatusAcceptCN = "2";

        /// <summary>
        /// Defines the StatusUnAccept in CN.
        /// </summary>
        public const string StatusUnAcceptCN = "3";
        /// <summary>
        /// Defines the StatusAccept in TW.
        /// </summary>
        public const string StatusAcceptTW = "4";

        /// <summary>
        /// Defines the StatusUnAccept in TW.
        /// </summary>
        public const string StatusUnAcceptTW = "5";

        /// <summary>
        /// Defines the StatusClosed .
        /// </summary>
        public const int StatusClosed = 0;

        /// <summary>
        /// Defines the StatusOpenPOS.
        /// </summary>
        public const string StatusOpenPOS = "O";//  Mở

        /// <summary>
        /// Defines the StatusClosedPOS.
        /// </summary>
        public const string StatusClosedPOS = "C";//  Mở

        /// <summary>
        /// Defines the StatusOpenLocal.
        /// </summary>
        public const string StatusOpenA = "A";//  Mở

        public const string StatusAcceptTH = "3";
        public const string StatusUnAcceptTH = "4";
        public const string StatusTW = "5";

        public const string StatusOpenText = "1";

        public const string StatusCloseText = "0";

        public const string StatusOpenStr = "Hoạt động";

        public const string StatusOpenCls = "Không hoạt động";


    }

    /// <summary>
    /// Mã danh mục cha (ParentCode)
    /// </summary>
    public static class ListOfValueParentValue
    {
        /// <summary>
        /// Id danh mục cha của danh mục Phòng ban: ParentIdDepartment = 15;
        /// </summary>
        public const int ParentIdDepartment = 15;

        /// <summary>
        /// Mã danh mục cha của danh mục Phòng ban: ParentCodeDepartment = "1500";
        /// </summary>
        public const string ParentCodeDepartment = "1500";

        /// <summary>
        /// Id danh mục cha của danh mục Chức vụ: ParentIdPosition = 14;
        /// </summary>
        public const int ParentIdPosition = 14;

        /// <summary>
        /// Mã danh mục cha của danh mục Chức vụ: ParentCodePosition = "1400";
        /// </summary>
        public const string ParentCodePosition = "1400";

        /// <summary>
        /// Id danh mục cha của danh mục nhóm quyền người dùng trên iDC để thiết lập khi tạo, thay đổi quyền,.... cho người dùng Intellect iDC: ParentId_UserRoleIDC = 1;
        /// </summary>
        public const int ParentId_UserRoleIDC = 1;

        /// <summary>
        /// Mã danh mục cha của danh mục nhóm quyền người dùng trên iDC để thiết lập khi tạo, thay đổi quyền,.... cho người dùng Intellect iDC: ParentCode_UserRoleIDC = "0100";
        /// </summary>
        public const string ParentCode_UserRoleIDC = "0100";

        /// <summary>
        /// Id danh mục cha của danh mục Trình độ: ParentIdProfessionalQualifications = 38;
        /// </summary>
        public const int ParentIdProfessionalQualifications = 38;

        /// <summary>
        /// Mã danh mục cha của danh mục Trình độ: ParentCodeProfessionalQualifications = "3800";
        /// </summary>
        public const string ParentCodeProfessionalQualifications = "3800";

        /// <summary>
        /// Id danh mục cha của danh mục Cấu hình một số liên quan tới Intelect: ParentIdConfigIntellectIDC = 2;
        /// </summary>
        public const int ParentIdConfigIntellectIDC = 2;

        /// <summary>
        /// Mã danh mục cha của danh mục Cấu hình một số liên quan tới Intelect: ParentCodeConfigIntellectIDC = "0200";
        /// </summary>
        public const string ParentCodeConfigIntellectIDC = "0200";
    }

    public static class FormatParameters
    {
        public const string FORMAT_DATE_VN = "dd-MM-yyyy";

        public const string FORMAT_DATE = "dd/MM/yyyy";

        public const string FORMAT_DATE_INT = "yyyyMMdd";

        public const string FORMAT_DATE_TIME_INT = "yyyyMMddHHmmss";

        public const string FORMAT_DATE_TIME_INT_SHORT = "yyMMddHHmmss";

        public const string FORMAT_DATE_VN_LONG = "dd/MM/yyyy HH:mm:ss";

        public const string FORMAT_DATE_VN_SHORT = "ddMMyy";
        /// <summary>
        /// Dùng để Format dữ liệu ngày tạo, ngày duyệt khi Export dữ liệu
        /// </summary>
        public const string FORMAT_DATE_TIME_INT_LONG = "yyyyMMdd HHmmss";

        public const string FORMAT_DATE_TIME_LONG_UPD = "yyyy-MM-dd HH:mm:ss";

        public const string FORMAT_DATE_TIME_SHORT_UPD = "yyyy-MM-dd";
    }

    /// <summary>
    /// Cờ xác định cấp mật khẩu cho người dùng. Giá trị:
    ///     '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
    ///     '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
    ///     '2': Mật khẩu được gửi link vào email của người dùng
    ///     '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
    /// </summary>
    public class MailIdFlag
    {
        public static ValueConstModel MailIdFlag_DefaultPassword = new ValueConstModel { Value = 0, Code = "0", Description = "Mật khẩu mặc định gồm 4 ký tự đầu của tài khoản người dùng và ngày sinh ddMMyyyy" };

        public static ValueConstModel MailIdFlag_RandomSendEmail = new ValueConstModel { Value = 1, Code = "1", Description = "Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng" };

        public static ValueConstModel MailIdFlag_LinkPassword = new ValueConstModel { Value = 2, Code = "2", Description = "Mật khẩu được gửi qua link vào email của người dùng" };

        /// <summary>
        /// Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        /// </summary>
        public static ValueConstModel MailIdFlag_RandomSendAPI = new ValueConstModel { Value = 4, Code = "4", Description = "Được thông báo qua OTT trên ứng dụng QLTDCS cho người dùng" };

        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => MailIdFlag_DefaultPassword,
                1 => MailIdFlag_RandomSendEmail,
                2 => MailIdFlag_LinkPassword,
                4 => MailIdFlag_RandomSendAPI,
                _ => null
            };
        }

        public static List<ValueConstModel> GetAll()
        {
            return new List<ValueConstModel>
            { 
                MailIdFlag_DefaultPassword,
                MailIdFlag_RandomSendEmail,
                MailIdFlag_LinkPassword,
                MailIdFlag_RandomSendAPI
            };
        }

    }

    /// <summary>
    /// Cờ xác định phân loại chức năng. Giá trị:
    /// </summary>
    public class FunctionTypeFlag
    {
        public static ValueConstModel FunctionTypeFlag_ADDNEW_USER = new ValueConstModel { Value = 1, Code = "ADDNEW_USER", Description = "Thêm mới người dùng" };

        public static ValueConstModel FunctionTypeFlag_ResetPassword = new ValueConstModel { Value = 2, Code = "RESET_PASSWORD", Description = "Thay đổi mật khẩu người dùng" };

        public static ValueConstModel FunctionTypeFlag_ENABLE_USER = new ValueConstModel { Value = 3, Code = "ENABLE_USER", Description = "Mở lại người dùng" };

        public static ValueConstModel FunctionTypeFlag_DISABLE_USER = new ValueConstModel { Value = 4, Code = "DISABLE_USER", Description = "Khóa người dùng" };

        public static ValueConstModel FunctionTypeFlag_MODIFY_USER = new ValueConstModel { Value = 5, Code = "MODIFY_USER", Description = "Thay đổi thông tin người dùng" };

        public static ValueConstModel FunctionTypeFlag_CHANGE_POS = new ValueConstModel { Value = 6, Code = "CHANGE_POS", Description = "Thay đổi Pos người dùng" };

        public static ValueConstModel FunctionTypeFlag_CHANGE_ROLE = new ValueConstModel { Value = 7, Code = "CHANGE_ROLE", Description = "Thay đổi quyền người dùng" };

        public static ValueConstModel FunctionTypeFlag_APPROVAL = new ValueConstModel { Value = 8, Code = "APPROVAL", Description = "Trình duyệt" };

        public static ValueConstModel FunctionTypeFlag_AUTHORIZE = new ValueConstModel { Value = 9, Code = "AUTHORIZE", Description = "Phê duyệt" };

        public static ValueConstModel FunctionTypeFlag_EDIT = new ValueConstModel { Value = 10, Code = "EDIT", Description = "Chỉnh sửa thông tin người dùng IDC" };

        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                1 => FunctionTypeFlag_ADDNEW_USER,
                2 => FunctionTypeFlag_ResetPassword,
                3 => FunctionTypeFlag_ENABLE_USER,
                4 => FunctionTypeFlag_DISABLE_USER,
                5 => FunctionTypeFlag_MODIFY_USER,
                6 => FunctionTypeFlag_CHANGE_POS,
                7 => FunctionTypeFlag_CHANGE_ROLE,

                _ => null
            };
        }

        public static List<ValueConstModel> GetAll()
        {
            return new List<ValueConstModel>
            {
                FunctionTypeFlag_ResetPassword,
                FunctionTypeFlag_ENABLE_USER,
                FunctionTypeFlag_DISABLE_USER,
                FunctionTypeFlag_MODIFY_USER,
                FunctionTypeFlag_CHANGE_POS,
                FunctionTypeFlag_CHANGE_ROLE
            };
        }
    }

    /// <summary>
    /// Phương thức xác thực của người dùng khi đăng nhập vào iDC
    /// </summary>
    public class AuthSecType
    {
        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => Không dùng phương thức xác thực thứ 2: AuthSecType_Single = 0
        /// </summary>
        public static ValueConstModel AuthSecType_Single = new ValueConstModel { Value = 0, Code = "0", Description = "None" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => Native: AuthSecType_Native = 1
        /// </summary>
        public static ValueConstModel AuthSecType_Native = new ValueConstModel { Value = 1, Code = "1", Description = "Native" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => LDAP: AuthSecType_LDAP = 2
        /// </summary>
        public static ValueConstModel AuthSecType_LDAP = new ValueConstModel { Value = 2, Code = "2", Description = "LDAP" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => Safeword: AuthSecType_Safeword = 3
        /// </summary>
        public static ValueConstModel AuthSecType_Safeword = new ValueConstModel { Value = 3, Code = "3", Description = "Safeword" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => RSA: AuthSecType_RSA = 4
        /// </summary>
        public static ValueConstModel AuthSecType_RSA = new ValueConstModel { Value = 4, Code = "4", Description = "RSA" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => SMS OTP: AuthSecType_SMSOTP = 5
        /// </summary>
        public static ValueConstModel AuthSecType_SMSOTP = new ValueConstModel { Value = 5, Code = "5", Description = "SMS OTP" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => VASCO Token: AuthSecType_VascoToken = 6
        /// </summary>
        public static ValueConstModel AuthSecType_VascoToken = new ValueConstModel { Value = 6, Code = "6", Description = "VASCO Token" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => EMV Card: AuthSecType_EMVCard = 7
        /// </summary>
        public static ValueConstModel AuthSecType_EMVCard = new ValueConstModel { Value = 7, Code = "7", Description = "EMV Card" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => VASCO Token for transaction: AuthSecType_VASCOTokenTrans = 8
        /// </summary>
        public static ValueConstModel AuthSecType_VASCOTokenTrans = new ValueConstModel { Value = 8, Code = "8", Description = "VASCO Token for transaction" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => SMS OTP for Transaction: AuthSecType_SMSOTPTrans = 9
        /// </summary>
        public static ValueConstModel AuthSecType_SMSOTPTrans = new ValueConstModel { Value = 9, Code = "9", Description = "SMS OTP for Transaction" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => USB eToken: AuthSecType_USBEToken = 11
        /// </summary>
        public static ValueConstModel AuthSecType_USBEToken = new ValueConstModel { Value = 11, Code = "11", Description = "USB eToken" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => SMS OTP (TIBCO): AuthSecType_SMSOTP_TIBCO = 12
        /// </summary>
        public static ValueConstModel AuthSecType_SMSOTP_TIBCO = new ValueConstModel { Value = 12, Code = "12", Description = "SMS OTP (TIBCO)" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => Native Transaction: AuthSecType_NativeTrans = 13
        /// </summary>
        public static ValueConstModel AuthSecType_NativeTrans = new ValueConstModel { Value = 13, Code = "13", Description = "Native Transaction - Xác thực nội bộ theo phương thức nền tảng tự xử lý)" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => Blackshiel - Giải pháp xác thực đa nhân tố của SafeNet (Gemalto/Thales): AuthSecType_Blackshiel = 14
        /// </summary>
        public static ValueConstModel AuthSecType_Blackshiel = new ValueConstModel { Value = 14, Code = "14", Description = "Blackshiel - Giải pháp xác thực đa nhân tố của SafeNet (Gemalto/Thales)" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => ARX Security Question: AuthSecType_ARXSecurityQuestion = 16
        /// </summary>
        public static ValueConstModel AuthSecType_ARXSecurityQuestion = new ValueConstModel { Value = 16, Code = "16", Description = "ARX Security Question" };

        /// <summary>
        /// Phương thức xác thực thứ 2 của người dùng khi đăng nhập vào iDC => ARX OTP (Dùng OTP để đăng nhâp) Hiện được gửi qua QLTDCS: AuthSecType_ARXOTP = 17
        /// </summary>
        public static ValueConstModel AuthSecType_ARXOTP = new ValueConstModel { Value = 17, Code = "17", Description = "ARX OTP (Dùng OTP để đăng nhâp)" };

        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => AuthSecType_Single,
                1 => AuthSecType_Native,
                2 => AuthSecType_LDAP,
                3 => AuthSecType_Safeword,
                4 => AuthSecType_RSA,
                5 => AuthSecType_SMSOTP,
                6 => AuthSecType_VascoToken,
                7 => AuthSecType_EMVCard,
                8 => AuthSecType_VASCOTokenTrans,
                9 => AuthSecType_SMSOTPTrans,
                11 => AuthSecType_USBEToken,
                12 => AuthSecType_SMSOTP_TIBCO,
                13 => AuthSecType_NativeTrans,
                14 => AuthSecType_Blackshiel,
                16 => AuthSecType_ARXSecurityQuestion,
                17 => AuthSecType_ARXOTP,
                _ => null
            };
        }

        public static List<ValueConstModel> GetAll()
        {
            return new List<ValueConstModel>
            { 
                AuthSecType_Single,
                AuthSecType_Native,
                AuthSecType_LDAP,
                AuthSecType_Safeword,
                AuthSecType_RSA,
                AuthSecType_SMSOTP,
                AuthSecType_VascoToken,
                AuthSecType_EMVCard,
                AuthSecType_VASCOTokenTrans,
                AuthSecType_SMSOTPTrans,
                AuthSecType_USBEToken,
                AuthSecType_SMSOTP_TIBCO,
                AuthSecType_NativeTrans,
                AuthSecType_Blackshiel,
                AuthSecType_ARXSecurityQuestion,
                AuthSecType_ARXOTP
            };
        }
    }

    public class EsbApiName
    {
        public static ValueConstModel IDCPendingTxn = new ValueConstModel { Value = 8, Code = "idcPendingTxn", Description = "Danh sách giao dịch tài chính (iDC) Pending" };

        public static ValueConstModel LMSPendingTxn = new ValueConstModel { Value = 9, Code = "lmsPendingTxn", Description = "Danh sách giao dịch LMS Pending" };

        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                8 => IDCPendingTxn,
                9 => LMSPendingTxn,
                _ => null
            };
        }

    }

}
