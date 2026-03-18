using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.ViewModels
{
    public class ProductParameterComparisonViewModel
    {
        /// <summary>
        /// Số thứ tự (STT) trong bảng, tính tự động ở service hoặc frontend
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// Tên nhóm sản phẩm hiển thị (Casa / Tide / Rút trước hạn (Penal Tide))
        /// </summary>
        public string ProductGroupCode { get; set; } = "";


        /// <summary>
        /// Tên nhóm sản phẩm hiển thị (Casa / Tide / Rút trước hạn (Penal Tide))
        /// </summary>
        public string ProductGroupDisplay { get; set; } = "";

        /// <summary>
        /// Mã sản phẩm (401, 402, ...)
        /// </summary>
        public string ProductCode { get; set; } = "";

        /// <summary>
        /// Tên sản phẩm (lấy từ ProductName trong DB)
        /// </summary>
        public string ProductName { get; set; } = "";

        //
        public string CurrentApplyPos { get; set; } = "";     // "X" hoặc ""
        public decimal CurrentMinSpread { get; set; }                   // LS điều chỉnh tối thiểu hiện tại
        public decimal CurrentMaxSpread { get; set; }                   // LS điều chỉnh tối đa hiện tại
        public DateTime? CurrentEffectedDate { get; set; }              // Ngày hiệu lực hiện tại

        // ── CẤU HÌNH ĐỀ XUẤT MỚI (Pending, hiệu lực tương lai) ──
        public bool NewApplyPosFlagChoice { get; set; } = false;
        public string NewApplyPos { get; set; } = "";         // "X" hoặc ""
        public decimal? NewMinSpread { get; set; }                      // LS min mới (nullable vì có thể chưa có đề xuất)
        public decimal? NewMaxSpread { get; set; }                      // LS max mới
        public DateTime? NewEffectedDate { get; set; }                  // Ngày hiệu lực đề xuất (ví dụ 01/03/2026)

        /// <summary>
        /// Có thay đổi gì so với hiện tại không? (dùng để highlight dòng đỏ/vàng ở grid)
        /// </summary>
        public bool HasChange { get; set; }

        /// <summary>
        /// Trạng thái của đề xuất (Pending / Approved / Rejected / ...)
        /// </summary>
        public string ProposalStatus { get; set; } = "";

        /// <summary>
        /// Ghi chú chung (nếu có từ proposal)
        /// </summary>
        public string Remark { get; set; } = "";

        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }

        public string ApproverBy { get; set; } = "";
        public DateTime? ApprovalDate { get; set; }

        // Helper property (không lưu DB, chỉ dùng ở UI)
        [NotMapped]
        public string ChangeStatusDisplay => HasChange ? "Có thay đổi" : "Không thay đổi";

        [NotMapped]
        public string ChangeCssClass => HasChange ? "text-danger fw-bold" : "text-success";
        
        public int ApplyPosFlag { get; set; } = 0;

        public decimal MinInterestRateSpread { get; set; } = 0;
        public decimal MaxInterestRateSpread { get; set; } = 0;
        public DateTime EffectedDate { get; set; }

        public int Status { get; set; } = 0;
        public string StatusDesc { get; set; } = "";
        public long Id { get; set; } = 0;
    }
}