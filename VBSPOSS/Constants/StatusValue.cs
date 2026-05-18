using NuGet.Configuration;
using System.Diagnostics;
using VBSPOSS.Models;

namespace VBSPOSS.Constants
{
    public class StatusValue
    {
        /// <summary>
        /// Trạng thái - Hoạt động
        /// </summary>
        public static ValueConstModel ACTIVE = new ValueConstModel { Value = 1, Code = "A", Description = "Hoạt động" };

        /// <summary>
        /// Trạng thái - Đóng
        /// </summary>
        public static ValueConstModel CLOSED = new ValueConstModel { Value = 0, Code = "C", Description = "Đóng" };

        /// <summary>
        /// Defines the StatusOpenPOS.
        /// </summary>
        public const string StatusOpenPOS = "O";//  Mở

        /// <summary>
        /// Defines the StatusClosedPOS.
        /// </summary>
        public const string StatusClosedPOS = "C";//  Mở


        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => CLOSED,
                1 => ACTIVE,
                _ => null // hoặc throw exception nếu muốn
            };
        }

        public const int StatusOpen = 1;

        public const int StatusClose = 0;
    }


    public class UpdateStatusValue
    {
        /// <summary>
        /// Trạng thái - Hoạt động
        /// </summary>
        public static ValueConstModel UPDATED = new ValueConstModel { Value = 1, Code = "U", Description = "Đã cập nhật" };

        /// <summary>
        /// Trạng thái - Đóng
        /// </summary>
        public static ValueConstModel NONE = new ValueConstModel { Value = 0, Code = "N", Description = "Chưa cập nhật" };

        /// <summary>
        /// Defines the StatusOpenPOS.
        /// </summary>
        public const string StatusOpenPOS = "O";//  Mở

        /// <summary>
        /// Defines the StatusClosedPOS.
        /// </summary>
        public const string StatusClosedPOS = "C";//  Mở


        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => NONE,
                1 => UPDATED,
                _ => null // hoặc throw exception nếu muốn
            };
        }
    }


    public class ConfigStatus
    {        
        public static ValueConstModel MAKER = new ValueConstModel { Value = 1, Code = "M", Description = "Tạo lập" };
        
        public static ValueConstModel CLOSED = new ValueConstModel { Value = 0, Code = "C", Description = "Đóng" };

        public static ValueConstModel PROCESS = new ValueConstModel { Value = 2, Code = "P", Description = "Chờ duyệt" };

        public static ValueConstModel AUTHORIZED = new ValueConstModel { Value = 3, Code = "A", Description = "Phê duyệt" };

        public static ValueConstModel REJECTED = new ValueConstModel { Value = 4, Code = "R", Description = "Từ chối" };

        public static ValueConstModel MODIFIED = new ValueConstModel { Value = 5, Code = "M", Description = "Chỉnh sửa" };
        /// <summary>
        /// Truyền vào giá trị Value → trả về đúng ValueConstModel
        /// </summary>
        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => CLOSED,
                1 => MAKER,
                2 => PROCESS,
                3 => AUTHORIZED,
                4 => REJECTED,
                5 => MODIFIED,
                _ => null // hoặc throw exception nếu muốn
            };
        }
    }

    public enum ConfigStatusEnum
    {
        Closed = 0,
        Maker = 1,
        Process = 2,
        Authorized = 3,
        Rejected = 4
    }


    //public class DebitCreditFlag
    //{
    //    /// <summary>
    //    /// Trạng thái - Hoạt động
    //    /// </summary>
    //    public static ValueConstModel CREDIT = new ValueConstModel { Value = 1, Code = "C", Description = "Dư có" };

    //    /// <summary>
    //    /// Trạng thái - Đóng
    //    /// </summary>
    //    public static ValueConstModel DEBIT = new ValueConstModel { Value = 0, Code = "D", Description = "Dư nợ" };


    //}

    public class ResultValueAPI
    {
        public const string ResultValue_Status_Success = "Success";

        public const string ResultValue_Status_Failed = "Failed";

        public const string ResultValue_Status_Errored = "Error";
    }

    public class TranferDataPosStatus
    {
        /// <summary>
        /// 0 - Khởi tạo, up tờ trình
        /// </summary>
        public static ValueConstModel INIT = new ValueConstModel
        {
            Value = 0,
            Code = "INIT",
            Description = "Khởi tạo"
        };

        /// <summary>
        /// 1 - Khai báo thôn điều chuyển
        /// </summary>
        public static ValueConstModel DECLARE_VILLAGE = new ValueConstModel
        {
            Value = 1,
            Code = "DECLARE",
            Description = "Khai báo thôn điều chuyển"
        };

        /// <summary>
        /// 2 - TW duyệt
        /// </summary>
        public static ValueConstModel TW_APPROVED = new ValueConstModel
        {
            Value = 2,
            Code = "APPROVED",
            Description = "TW duyệt"
        };

        /// <summary>
        /// 3 - Pos nguồn xuất số liệu
        /// </summary>
        public static ValueConstModel FROM_POS_EXPORTED = new ValueConstModel
        {
            Value = 3,
            Code = "EXPORT",
            Description = "POS nguồn xuất số liệu"
        };

        /// <summary>
        /// 4 - Pos đích xác nhận
        /// </summary>
        public static ValueConstModel TO_POS_CONFIRMED = new ValueConstModel
        {
            Value = 4,
            Code = "CONFIRM",
            Description = "POS đích xác nhận"
        };

        /// <summary>
        /// Lấy theo Value
        /// </summary>
        public static ValueConstModel GetByValue(int value)
        {
            return value switch
            {
                0 => INIT,
                1 => DECLARE_VILLAGE,
                2 => TW_APPROVED,
                3 => FROM_POS_EXPORTED,
                4 => TO_POS_CONFIRMED,
                _ => null
            };
        }

        /// <summary>
        /// Danh sách combobox
        /// </summary>
        public static List<ValueConstModel> GetOption()
        {
            return new List<ValueConstModel>
        {
            INIT,
            DECLARE_VILLAGE,
            TW_APPROVED,
            FROM_POS_EXPORTED,
            TO_POS_CONFIRMED
        };
        }
    }
}
