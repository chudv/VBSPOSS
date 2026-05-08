
using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IListOfTransPointService
    {
        /// <summary>
        /// Hàm trả về Danh sách điểm giao dịch theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfTransPoint
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu POS Chi nhánh</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        List<ListOfTransPointViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pTxnLocation);

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




        /// <summary>
        /// Hàm lấy Danh sách điểm giao dịch ghi nhận thông tin Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectiveDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectiveDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        List<ListOfTransPointWorkViewModel> GetListOfTransPointWorkSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pEffectiveDateBegin, string pEffectiveDateEnd,
                                            int pStatus, string pTxnLocation);

        /// <summary>
        /// Hàm lấy danh sách điểm giao dịch để cho Lựa chọn thay đổi theo yêu cầu nghiệp vụ
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu của POS Chi nhánh</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào -1</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEventCode">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách điểm giao dịch theo Model ListOfTransPointWorkViewModel</returns>
        List<ListOfTransPointWorkViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pTxnPointCode, string pTxnPointName,
                                int pStatus, string pTxnLocation, string pEventCode);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pTransPointWorkUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfTransPointWorkViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Số bản ghi được thêm/sửa</returns>
        int UpdateListOfTransPointWork(ListOfTransPointWorkViewModel pTransPointWorkUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch (Bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pParentId">Chỉ số bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfTransPointHist) - Bản ghi trước khi được cập nhật vào bảng ListOfTransPointWork</param>
        /// <param name="pProvinceCode">Mã Tỉnh/TP</param>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực của điểm giao dịch</param>
        /// <param name="pBusinessDate">Ngày hệ thống Intellect iDC (Ngày hiệu lực thay đổi thông tin của điểm giao dịch của bản ghi)</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        bool DeleteListOfTransPointWork(string pEventCode, long pParentId, string pProvinceCode, string pPosCode, string pTxnPointCode,
                            DateTime pEffectiveDate, DateTime pBusinessDate, string pUserNameDelete, int pFlagDelete);

        /// <summary>
        /// Hàm lấy Danh sách điểm giao dịch ghi nhận thông tin Lịch sử Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectiveDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectiveDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        List<ListOfTransPointHistViewModel> GetListOfTransPointHistSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pEffectiveDateBegin, string pEffectiveDateEnd,
                                            int pStatus, string pTxnLocation);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pTransPointHistUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfTransPointHistViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật. Nếu truyền vào rỗng lấy theo người cập nhật từ pTransPointHistUpd</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Chỉ số Id bản ghi được Thêm/Chỉnh sửa</returns>
        long UpdateListOfTransPointHist(ListOfTransPointHistViewModel pTransPointHistUpd, string pUserNameUpd, string pFlagCall);

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch trong bảng lịch sử thay đổi (Bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pId">Chỉ số khóa bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfTransPointHist)</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        bool DeleteListOfTransPointHist(string pEventCode, long pId, string pTxnPointCode, string pUserNameDelete, int pFlagDelete);




        /// <summary>
        /// Hàm thực hiện thêm mới bản ghi Điểm giao dịch vào bảng IDL_IDC.ADD_NEW_TXN_POINT_ITC
        /// </summary>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointId">Mã điểm giao dịch</param>
        /// <param name="pVisitDate">Ngày giao dịch cố định</param>
        /// <param name="pVisitTime">Thời gian giao dịch. Ex: 8h00-12h00</param>
        /// <param name="pTranpointFileGen">Cờ có xuất file không. Giá trị: Y/N</param>
        /// <param name="pTxnPointName">Tên điểm giao dịch</param>
        /// <param name="pLatitude">Tọa độ vĩ độ của điểm giao dịch</param>
        /// <param name="pLongitude">Tọa độ kinh độ của điểm giao dịch</param>
        /// <param name="pTypeCode">Mã ký tự đầu của điểm. TXN</param>
        /// <param name="pMakerDate">Ngày tạo điểm. Định dạng yyyyMMdd</param>
        /// <param name="pErrMsg">Mô tả lỗi</param>
        /// <param name="pSynStatus">Trọng thái đồng bộ để trống</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        Task<ExecuteResultModelModel> InsertTransPointIDC(string pPosCode, string pTxnPointId, string pVisitDate, string pVisitTime, string pTranpointFileGen,
                                           string pTxnPointName, string pLatitude, string pLongitude, string pTypeCode,
                                           string pMakerDate, string pErrMsg, string pSynStatus);

        /// <summary>
        /// Hàm thực hiện tạo bảng ghi từ IDL_IDC.ADD_NEW_TXN_POINT_ITC vào 2 bảng IDL_IDC.TRANPOINT, IDL_IDC.TXIDMAP theo ngày SELECT PC_BUSINESS_DT FROM IDL_IDC.P_CTRL;
        /// </summary>
        /// <param name="pMakerDate">Không bắt buộc vì vào trong thủ tục CSDL chỉ sử dụng SELECT PC_BUSINESS_DT INTO LV_BUSINESS_DT FROM IDL_IDC.P_CTRL;</param>
        /// <param name="pCreatedBy">Người tạo lập</param>
        /// <param name="pApproverBy">Ngày tạo lập</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        Task<ExecuteResultModelModel> CreateTransPointByBusinessDateIDC(string pMakerDate, string pCreatedBy, string pApproverBy);

        /// <summary>
        /// Hàm lấy thông tin ngày giao dịch, EOD của hệ thống Core
        /// </summary>
        /// <param name="pFlagCall">Cờ xác định ngày muốn lấy. Giá trị: 
        ///             '1': Giá trị trả về là ngày BUSINESS_DT (định dạng yyyyMMdd)
        ///             '2': Giá trị trả về là ngày EOD_DT (định dạng yyyyMMdd)
        ///             '3': Giá trị trả về là ngày cuối tháng trước MON_END_DT
        ///             Còn lại là lấy ngày hiện thời hệ thống (máy chủ CSDL)
        /// </param>
        /// <returns></returns>
        DateTime GetDateInCoreIDC(string pFlagCall);

        /// <summary>
        /// Hàm thực hiện thêm mới bản ghi Thay đổi ngày giao dịch vào bảng IDL_LMS.TXN_DATE_CHANGE_ITC
        /// Ex: var resultChangeVisitDate = _serviceTransPoint.InsertTxnDateChangeIDC("TXN0234501", "17", "20260401", "19", "", "", "20260423");
        /// </summary>
        /// <param name="pTxnPointId">Mã điểm giao dịch</param>
        /// <param name="pNewVisitDate">Ngày giao dịch cố định Mới</param>
        /// <param name="pEffDate">Ngày hiệu lực. Định dạng yyyyMMdđ</param>
        /// <param name="pOldVisitDate">Ngày giao dịch cố định Cũ</param>
        /// <param name="pMsgResult">Thông tin cập nhật</param>
        /// <param name="pChangeFlag">Cờ thay đổi</param>
        /// <param name="pMakerDate">Ngày tạo lập yêu cầu thay đổi. Định dạng yyyyMMdđ</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        Task<ExecuteResultModelModel> InsertTxnDateChangeIDC(string pTxnPointId, string pNewVisitDate, string pEffDate, string pOldVisitDate, string pMsgResult,
                                                                          string pChangeFlag, string pMakerDate);
    }
        
}
