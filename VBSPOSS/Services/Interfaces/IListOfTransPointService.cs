
using VBSPOSS.Data.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IListOfTransPointService
    {
        /// <summary>
        /// Hàm trả về Danh sách danh mục chung theo những điều kiện truyền vào: ListOfTransPoint
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <returns>Danh sách bản ghi</returns>
        List<ListOfTransPointViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pEffectiveDate, string pTxnStatus);
        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        int UpdateListOfTransPoint(ListOfTransPointViewModel model, string pUserName);
        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch
        /// </summary>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        bool DeleteListOfTransPoint(string pTxnPointCode, string pUserName, int pFlagDelete);
    }
        
}
