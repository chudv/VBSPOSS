namespace VBSPOSS.Data.IntellectIDC.Models
{
    public class ExecuteResultModelModel
    {
        /// <summary>
        /// Số dòng bị ảnh hưởng (xóa/sửa/thêm)
        /// </summary>
        public int RowsAffected { get; set; }
        /// <summary>
        /// 1: Thành công, 0: Không có dữ liệu, -1: Lỗi
        /// </summary>
        public int Success { get; set; }
        /// <summary>
        /// Mô tả kết quả trả về
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Thành công: ResultValueAPI.ResultValue_Status_Success = "Success"; Lỗi: ResultValueAPI.ResultValue_Status_Errored; Không thành công ResultValueAPI.ResultValue_Status_Failed
        /// </summary>
        public string TxnStatus { get; set; }
    }
}
