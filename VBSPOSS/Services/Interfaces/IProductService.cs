using Microsoft.AspNetCore.Mvc.Rendering;
using VBSPOSS.Data.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IProductService
    {
        List<SelectListItem> GetProductGroups();
        List<SelectListItem> GetAccountTypes(string productCode);
        List<Product> GetProductList(string productGroupCode);
        List<Product> GetProductByCode(string productCode);
      //add
        Task<List<AddCasaProductViewModel>> GetCasaGridDataFromApi(string productCode);

        List<SelectListItem> GetAccountSubTypes(string accountType);

        List<ListOfProducts> GetFullProductList(string productGroupCode, string depositType, string customerType, string userPosCode, int userGrade);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termType">I - Bao gồm, E - Không bao gồm</param>
        /// <returns></returns>
        List<DepositTermModel> GetDepositTerms(string termType, int termBasis, string inclusionFlag);

        ProductParameter GetProductParameter(string productGroupCode, string productCode, DateTime? effectedDate);

        /// <summary>
        /// Hàm lấy danh sách thông tin cấu hình thông số cho sản phẩm: Cờ áp dụng POS; LS điều chỉnh tối thiểu và tối đa => Lấy từ bảng ProductParameters
        /// </summary>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pProductCode">Mã sản phẩm</param>
        /// <param name="pEffectedDate">Ngày hiệu lực</param>
        /// <param name="pStatus">Trạng thái (Không bắt buộc). Nếu truyền -1 lấy tất; 1: Bản ghi mở; 3: Bản ghi được phê duyệt</param>
        /// <returns>Danh sách thông tin cấu hình thông số cho sản phẩm</returns>
        List<ProductParameter> GetListProductParametersSearch(string pProductGroupCode, string pProductCode, DateTime? pEffectedDate, int pStatus);
    }
}




