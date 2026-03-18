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
    }
}
