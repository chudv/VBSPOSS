using System;
using System.Reflection.Emit;
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
        public const string UserId_Call_ApiIDC = "IDCADMIN";
        public const string SourceId = "MB";
        public const string DebitCreditFlagDefault = "C";
        public const string CurrencyValueDefault = "VND";
        
    }

    public class DepositType
    {
        public const string BeforeOfTerm = "B"; // Đầu ký
        public const string PartitalTerm = "P"; // Định kỳ
        public const string OnTerm = "E"; // Định kỳ
    }   

    /// <summary>
    /// Chỉ số xác định danh mục gốc: Chức vụ; Phòng ban; Trình độ chuyên mốn;...
    /// </summary>
    public static class ParentLovValue
    {
        /// <summary>
        /// Chỉ số xác định Danh mục chức vụ
        /// </summary>
        public const int Parent_Title_Id = 14;
        
        /// <summary>
        /// Mã xác định Danh mục chức vụ
        /// </summary>
        public const string Parent_Title_Code = "1400";

        /// <summary>
        /// Chỉ số xác định Danh mục Phòng ban
        /// </summary>
        public const int Parent_Department_Id = 15;

        /// <summary>
        /// Mã xác định Danh mục Phòng ban
        /// </summary>
        public const string Parent_Department_Code = "1500";

        /// <summary>
        /// Chỉ số xác định Danh mục Phòng ban
        /// </summary>
        public const int Parent_Degree_Id = 38;

        /// <summary>
        /// Mã xác định Danh mục Phòng ban
        /// </summary>
        public const string Parent_Degree_Code = "3800";
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
        /// Id danh mục cha của danh mục Chúc vụ: ParentIdPosition = 14;
        /// </summary>
        public const int ParentIdPosition = 14;

        /// <summary>
        /// Mã danh mục cha của danh mục Chúc vụ: ParentCodePosition = "1400";
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
        /// Id danh mục cha của danh mục Chúc vụ: ParentIdProfessionalQualifications = 38;
        /// </summary>
        public const int ParentIdProfessionalQualifications = 38;

        /// <summary>
        /// Mã danh mục cha của danh mục Chúc vụ: ParentIdProfessionalQualifications = 38;
        /// </summary>
        public const string ParentCodeProfessionalQualifications = "3800";
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
}
