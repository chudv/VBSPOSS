
using VBSPOSS.Data.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IListOfValueService
    {
        /// <summary>
        /// Hàm thực hiện trả về danh sách giá trị Trực thuộc để lấy danh sách Phòng ban theo mã POS truyền vào.
        /// </summary>
        /// <param name="pPosCode">Mã Pos truyền vào</param>
        /// <returns>Chỉ số xác định trực thuộc với Quy ước: 
        ///                              "1" - Hoi so chinh;
        ///                              "2" - Chi nhanh Tinh/TP;
        ///                              "3" - Van phong dai dien;
        ///                              "4" - Phong giao dich Quan/Huyen;
        ///                              "5" - Trung tam CNTT;
        ///                              "7" - Trung tam Đao tạo;
        ///                              "9" - Sở giao dịch;
        /// </returns>
        public string GetCodeApplyByPosCode(string pPosCode);

        string ReplaceName_ListMain(string pValueName);

        /// <summary>
        /// Hàm lấy giá trị của ô (Cell) trong bảng dữ liệu cần lấy theo câu truy vấn SQL truyền vào
        /// </summary>
        /// <param name="pQuerySelect">Truy vấn SQL truyền vào. Ex: "Select Code From ListOfValue Where ParentId=14" /param>
        /// <returns>Giá trị trả về. Ex: '1400'</returns>
        string GetCellValueForQuery(string pQuerySelect);

        /// <summary>
        /// Hàm sinh mã tự động cho danh mục cần thêm mới
        /// </summary>
        /// <param name="pParentId">Phân loại danh mục. Ex: 14 - Chức vụ</param>
        /// <param name="pLevelList">Cấp của danh mục cần thêm mới. Ex: Cấp danh mục cần thêm. Ex: 2</param>
        /// <returns>Mã danh mục tự sinh</returns>
        string GetCodeOfList_AutoGen(int pParentId, int pLevelList);

        /// <summary>
        /// Hàm trả về Danh sách danh mục chung theo những điều kiện truyền vào
        /// </summary>
        /// <param name="pParentId">Chỉ số phân loại danh mục cha (Không bắt buộc)</param>
        /// <param name="pParentCode">Mã số danh mục cha (Không bắt buộc)</param>
        /// <param name="pId">Chỉ số Id danh mục (Không bắt buộc)</param>
        /// <param name="pCode">Mã danh mục (Không bắt buộc)</param>
        /// <param name="pName">Tên danh mục (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <param name="pFlagCallRoot">Cờ xác định Chỉ lấy danh mục gốc/con/tất cả: 0 - Lấy tất cả; 1 - Chỉ Lấy gốc; 2 - Chỉ Lấy dm con</param>
        /// <returns>Danh sách bản ghi</returns>
        List<ListOfValueViewModel> GetListOfValueSearch(int pParentId, string pParentCode, int pId, string pCode, string pName, int pStatus, int pFlagCallRoot);

        /// <summary>
        /// Bản ghi thông tin danh mục theo Id danh mục truyền vào
        /// </summary>
        /// <param name="pId">Chỉ số xác định bản ghi</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <returns>Bản ghi danh mục trả ra</returns>
        ListOfValueViewModel GetListOfValueForId(int pId, int pStatus);

        /// <summary>
        /// Bản ghi thông tin danh mục theo Mã số danh mục truyền vào
        /// </summary>
        /// <param name="pCode">Mã số xác định bản ghi</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <returns>Bản ghi danh mục trả ra</returns>
        ListOfValueViewModel GetListOfValueByCode(string pCode, int pStatus);

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục chung
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        int UpdateListOfValue(ListOfValue objModelUpd, string pUserName);

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục chung
        /// </summary>
        /// <param name="pListId">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        bool DeleteListOfValue(int pListId, string pUserName, int pFlagDelete);

        /// <summary>
        /// Hàm lấy danh sách bản ghi Chi nhánh theo điều kiện truyền vào
        /// </summary>
        /// <param name="pFlagCondi">Điều kiện lấy dữ liệu chính Chi nhánh. Giá trị quy ước:
        ///          '1' - Lấy duy nhất POS Hội sở chính (000100)
        ///          '2' - Lấy danh sách các POS HSC và Chi nhánh Tỉnh/TP (Danh sách MainPOS ĐK MaSo = MaSoCN Hoặc PhanLoai IN (0,1)
        ///          '3' - Lấy danh sách các POS Chi nhánh/PGD, trừ POS Hội sở chính (000100)
        ///          '4' - Lấy danh sách các POS HSC/Chi nhánh: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; => Phải truyền thêm pPosCodeUser
        ///          '5' - Lấy danh sách các POS HSC/Chi nhánh/PGD: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; PGD lấy duy nhất POS PGD => Phải truyền thêm pPosCodeUser
        /// </param>
        /// <param name="pDefaultValue">Giá trị mặc định (ví dụ: 0 cho logic mặc định, có thể dùng để giới hạn hoặc điều kiện bổ sung)</param>
        /// <param name="pMainPosCode">Mã chi nhánh. Không sử dụng truyền vào là ''</param>
        /// <param name="pPosCode">Mã POS. Không sử dụng truyền vào là ''</param>
        /// <param name="pStatus">Trạng thái bản ghi</param>
        /// <param name="pPosCodeUser">Mã pos của người dùng gọi đến</param>
        /// <param name="pUserName">Tên đăng nhập người dùng</param>
        /// <returns>Danh sách bản ghi Chi nhánh</returns>
        List<ListOfPosViewModel> GetBranchSearch(string pFlagCondi, int pDefaultValue, string pMainPosCode, string pPosCode, string pStatus, string pPosCodeUser, string pUserName);


        //List<ListOfPosViewModel> GetBranchSearch(string pFlagCondi, string pMainPosCode, string pPosCode, string pStatus, string pPosCodeUser, string pUserName);

        List<ListOfCommuneViewModel> GetLovCommuneList(string pProvinceCode, string pDistrictCode, string pCommuneCode, string pPosCode, string pSubCommuneCode);

        /// <summary>
        /// Hàm lấy danh sách sản phẩm TIDE/CASA theo điều kiện truyền vào (Danh sách trong bảng ListOfProducts)
        /// </summary>
        /// <param name="pProductGroupCode">Loại sản phẩm: CASA/TIDE</param>
        /// <param name="pProductCode">Mã sản phẩm</param>
        /// <param name="pAccountTypeCode">Loại tài khoản</param>
        /// <param name="pCode">Mã sản phẩm/Loại tài khoản</param>
        /// <param name="pName">Tên sản phẩm/Loại tài khoản</param>
        /// <param name="pStatus">Trạng thái (Nếu -1 lấy tất cả)</param>
        /// <param name="pIsApplyPosFlag">Xác định xem có lấy theo sản phẩm được áp dụng cho POS không. 0 - Không xét; 1 - Có xét</param>
        /// <param name="pUserGrade">Cấp của người dùng: PosGrade.SUB_POS = 1; PosGrade.MAIN_POS = 2; PosGrade.HEAD_POS = 3;</param>
        /// <param name="pProductGroupCodeParams">Loại sản phẩm để ánh xạ vào bảng DL tham số cấu hình ProductParameters: CASA/TIDE/DEPOSITPENAL</param>
        /// <returns>Danh sách trong bảng ListOfProducts</returns>
        List<ListOfProducts> GetListOfProductsSearch(string pProductGroupCode, string pProductCode, string pAccountTypeCode, string pCode, string pName,
                                    int pStatus, int pIsApplyPosFlag, int pUserGrade, string pProductGroupCodeParams);
    }
}
