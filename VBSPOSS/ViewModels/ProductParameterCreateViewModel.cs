using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VBSPOSS.ViewModels
{
    public class ProductParameterCreateViewModel
    {
        public long Id { get; set; } = 0; // 0 = tạo mới

        // Thông tin chung
        public string ProductGroupCode { get; set; } = ""; // CASA / TIDE / PENAL
        public string ProductGroupDisplay { get; set; } = ""; // Casa / Tide / Rút trước hạn...
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";

        // Cấu hình lãi suất
        public bool ApplyPosFlag { get; set; } = false; // Áp dụng POS (checkbox → X nếu true)
        public decimal MinInterestRateSpread { get; set; } = 0.1m; //
        public decimal MaxInterestRateSpread { get; set; } = 5.6m; //

        // add
        // ==================== THÊM MỚI ====================
        [Display(Name = "LSĐC Tối thiểu (mặc định)")]
        public decimal DefaultMinSpread { get; set; } = 0.5m;

        [Display(Name = "LSĐC Tối đa (mặc định)")]
        public decimal DefaultMaxSpread { get; set; } = 3.0m;
        // ==================================================


        public DateTime EffectedDate { get; set; } = new DateTime(2026, 3, 1); // Mặc định theo mô tả 


        public string Remark { get; set; } = "";
        public long? DocumentId { get; set; } //

        // Danh sách sản phẩm để chọn (dropdown hoặc multi-select)
        public List<SelectListItem> ProductGroupOptions { get; set; } = new();
        public List<SelectListItem> ProductOptions { get; set; } = new(); // Load theo nhóm

     
        public List<ProductParameterDetailViewModel> GridItems { get; set; } = new();
    }


    public class ProductParameterDetailViewModel
    {
        // STT (tự tăng trong grid)
        public int STT { get; set; }

        // Thông tin sản phẩm (khóa, không edit)
        public string ProductGroupCode { get; set; }   // Để filter hoặc hiển thị nhóm
        public string ProductGroupDisplay { get; set; } // Tên nhóm hiển thị (Casa/Tide/Penal)
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        // Thông tin hiện tại (read-only, từ DB hoặc API)
        public string CurrentApplyPos { get; set; }      // "X" hoặc "" (Áp dụng POS hiện tại)
        public bool CurrentApplyPosFlag { get; set; }    // true/false (dùng để so sánh)
        public decimal CurrentMinSpread { get; set; }    // LS min hiện tại
        public decimal CurrentMaxSpread { get; set; }    // LS max hiện tại

        public string CurrentRemark { get; set; }

        // Thông tin mới (có thể edit trong grid)
        public bool NewApplyPosFlag { get; set; }        // Checkbox: Áp dụng POS mới
        public decimal NewMinSpread { get; set; }        // LS min mới
        public decimal NewMaxSpread { get; set; }        // LS max mới

        // Ghi chú riêng cho sản phẩm (nếu cần)
        public string Remark { get; set; }
        //add Thêm 
        public string AccountTypeCode { get; set; }

        // Các trường phụ trợ (tính toán thay đổi, không hiển thị)
        public bool HasChange =>
            NewApplyPosFlag != CurrentApplyPosFlag ||
            NewMinSpread != CurrentMinSpread ||
            NewMaxSpread != CurrentMaxSpread;
    }


    public class LoadProductRequest
    {
        public string productGroupCode { get; set; }
        public string effectedDate { get; set; }

        //add

        public decimal? defaultMinSpread { get; set; }
        public decimal? defaultMaxSpread { get; set; }
    }
    public class SaveBatchRequest
    {
        public string ProductGroupCode { get; set; }
        public DateTime EffectedDate { get; set; }
        public string Remark { get; set; }
        public string Items { get; set; } // JSON string của grid items
    }

    /// <summary>
    /// ViewModel cho Popup Duyệt Cấu hình Lãi suất
    /// </summary>
    public class ProductParameterApproveViewModel
    {
        public string ProductGroupCode { get; set; } = "";
        public string ProductGroupDisplay { get; set; } = "";
        public DateTime EffectedDate { get; set; }

        // Danh sách dữ liệu để bind vào Grid so sánh
        public List<ProductParameterComparisonViewModel> Items { get; set; } = new List<ProductParameterComparisonViewModel>();
    }

    /// <summary>
    /// Request khi bấm Duyệt hoặc Từ chối
    /// </summary>
    public class ApproveRequest
    {
        public string ProductGroupCode { get; set; } = "";
        public DateTime EffectedDate { get; set; }
        public string RejectReason { get; set; } = "";
    }



}