// IListOfCommunesService.cs
using System.Collections.Generic;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IListOfCommuneService
    {

        /// <summary>
        /// Hàm trả về Danh mục địa phương theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfCommunes
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pRecordStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfCommunesViewModel</returns>
        List<ListOfCommunesViewModel> GetListOfCommunesSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pTxnLocation);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục địa phương
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        int UpdateListOfCommune(ListOfCommunesViewModel model, string pUserName);
        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục địa phương
        /// </summary>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        bool DeleteListOfCommune(string pTxnPointCode, string pUserName, int pFlagDelete);

        /// <summary>
        /// Hàm lấy Danh mục đia phương ghi nhận thông tin Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfCommunesWork)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pRecordStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi địa phương theo Model ListOfCommunesViewModel</returns>
        List<ListOfCommuneWorksViewModel> GetListOfCommuneWorkSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pEffectDateBegin, string pEffectDateEnd,
                                            int pStatus, string pTxnLocation);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục địa phương (Bảng ListOfCommunesWork)
        /// </summary>
        /// <param name="pCommuneWorkUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfCommunesWorkViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Số bản ghi được thêm/sửa</returns>
        int UpdateListOfCommuneWork(ListOfCommuneWorksViewModel pCommuneWorkUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục địa phương (Bảng ListOfCommunesWork)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pParentId">Chỉ số bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfCommunesHist) - Bản ghi trước khi được cập nhật vào bảng ListOfCommunesWork</param>
        /// <param name="pProvinceCode">Mã Tỉnh/TP</param>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pEffectDate">Ngày hiệu lực của điểm giao dịch</param>
        /// <param name="pBusinessDate">Ngày hệ thống Intellect iDC (Ngày hiệu lực thay đổi thông tin của điểm giao dịch của bản ghi)</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        bool DeleteListOfCommuneWork(string pEventCode, long pParentId, string pProvinceCode, string pPosCode, string pTxnPointCode,
                            DateTime pEffectDate, DateTime pBusinessDate, string pUserNameDelete, int pFlagDelete);

        /// <summary>
        /// Hàm lấy Danh sách địa phương ghi nhận thông tin Lịch sử Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfCommunesHist)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pRecordStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi danh mục địa phương theo Model ListOfCommunesViewModel</returns>
        List<ListOfCommuneHistsViewModel> GetListOfCommuneHistSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pEffectDateBegin, string pEffectDateEnd,
                                            int pStatus, string pTxnLocation);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục địa phương (Bảng ListOfCommunesHist)
        /// </summary>
        /// <param name="pCommunesHistUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfCommunesHistViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật. Nếu truyền vào rỗng lấy theo người cập nhật từ pCommunesHistUpd</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Chỉ số Id bản ghi được Thêm/Chỉnh sửa</returns>
        long UpdateListOfCommuneHist(ListOfCommuneHistsViewModel pCommunesHistUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục địa phương trong bảng lịch sử thay đổi (Bảng ListOfCommunesHist)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pId">Chỉ số khóa bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfCommunesHist)</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        bool DeleteListOfCommuneHist(string pEventCode, long pId, string pTxnPointCode, string pUserNameDelete, int pFlagDelete);
    }
}