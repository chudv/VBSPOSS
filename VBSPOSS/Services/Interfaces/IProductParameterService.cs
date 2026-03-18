using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using VBSPOSS.Data.Models;
using VBSPOSS.ViewModels; // Giả sử Anh đã tạo các ViewModel ở đây

namespace VBSPOSS.Services.Interfaces
{
    public interface IProductParameterService
    {
        /// <summary>
        /// Lấy danh sách so sánh cấu hình hiện tại vs đề xuất mới
        /// </summary>
        Task<List<ProductParameterComparisonViewModel>> GetComparisonListAsync(
            string productGroupCode = null,
            string productCode = null,
            DateTime? targetFutureDate = null);

        /// <summary>
        /// Tạo đề xuất cấu hình lãi suất mới (Status = Pending)
        /// </summary>
        // Task<ProductParameter> CreateProposalAsync(ProductParameter proposal);

        /// <summary>
        /// Lấy chi tiết một cấu hình theo Id
        /// </summary>
        //  Task<ProductParameter> GetByIdAsync(long id);

        /// <summary>
        /// Cập nhật đề xuất (chỉ cho phép khi còn Pending)
        /// </summary>
        // Task UpdateAsync(ProductParameter entity);

        /// <summary>
        /// Phê duyệt đề xuất (Pending → Active)
        /// </summary>
        // Task<bool> ApproveAsync(long id, string approver, string remark = null);

        /// <summary>
        /// Từ chối đề xuất (Pending → Rejected)
        /// </summary>
        //  Task<bool> RejectAsync(long id, string approver, string rejectReason);

        Task<List<SelectListItem>> GetProductOptionsByGroupAsync(string productGroupCode = null);

        /// <summary>
        /// Hàm lấy danh sách thông tin tham số cấu hình của sản phẩm TIDE/CASA/DEPOSITPENAL
        /// </summary>
        /// <param name="pFlagCall">Cờ xác định sự kiến gọi. Nếu không truyền vào thì là 0</param>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pEffectedDate">Ngày hiệu lực</param>
        /// <param name="pId">Chỉ số xác định bản ghi (Không bắt buộc truyền vào là 0)</param>
        /// <param name="pProductCode">Mã sản phẩm (Không bắt buộc)</param>
        /// <param name="pRemark">Ghi chú (Không bắt buộc)</param>
        /// <returns>Danh sách thông tin tham số cấu hình của sản phẩm TIDE/CASA/DEPOSITPENAL theo model ProductParameterViewModel</returns>
        List<ProductParameterComparisonViewModel> GetListProductParameters(string pFlagCall, string pProductGroupCode, string pEffectedDate,
                                            long pId, string pProductCode, string pRemark);

      

        Task<List<ProductParameterDetailViewModel>> GetProductsForCreateAsync(string productGroupCode, DateTime effectedDate);
        Task<List<ProductParameterDetailViewModel>> LoadProductsForCreateAsync(string productGroupCode, DateTime effectedDate);

        // luu
        Task<int> SaveBatchProductParameterAsync(string productGroupCode, DateTime effectedDate, string remark, List<ProductParameterDetailViewModel> items);

        // Load màn Index
        Task<List<ProductParametersView>> GetProductParametersViewListAsync(
            string productGroupCode,
            string productCode,
            DateTime? effectDate);
    }
}