using System;
using System.ComponentModel.DataAnnotations.Schema;
using VBSPOSS.Constants;

namespace VBSPOSS.Data.Models
{
    public class ProductParameter
    {
        public long Id { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int ApplyPosFlag { get; set; }
        public decimal MinInterestRateSpread { get; set; }
        public decimal MaxInterestRateSpread { get; set; }
        public DateTime EffectedDate { get; set; }
        public int Status { get; set; }
        [NotMapped]
        public string StatusDesc
        {
            get
            {
                int statusValue = Status;
                return ConfigStatus.GetByValue(statusValue)?.Description ?? "Không xác định";
            }
            private set { }
        }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        [NotMapped]
        public string ApplyPosDisplay => ApplyPosFlag == 1 ? "X" : "";

        [NotMapped]
        public string ProductGroupDisplay
        {
            get
            {
                return ProductGroupCode switch
                {
                    "CASA" => "Casa",
                    "TIDE" => "Tide",
                    "PENAL" or "PENALTIDE" => "Rút trước hạn (Penal Tide)",
                    _ => ProductGroupCode ?? "Không xác định"
                };
            }
        }

     }


    // Thêm cho màn Index tổng hợp 
    public class ProductParametersView
    {
        public int OrderNo { get; set; }

        public string ProductGroupCode { get; set; }

        public DateTime EffectedDate { get; set; }

        public int? Status { get; set; }

        public string IdList { get; set; }

        public string ProductCodeList { get; set; }

        public string ProductNameList { get; set; }

        public decimal MinInterestRateSpread { get; set; }

        public decimal MaxInterestRateSpread { get; set; }

        public string Remark { get; set; }

        public int ProductCount { get; set; }

        public int ProductCountApplyPos { get; set; }

        public int ProductCountNotApplyPos { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        // Nếu bạn muốn hiển thị trạng thái dạng chữ (giống StatusDesc trong mẫu CASA)
        [NotMapped]
       
        public string StatusDesc => Status switch
        {
            0 => "Đóng",
            1 => "Tạo mới",
            2 => "Chờ duyệt",
            3 => "Đã duyệt",
            4 => "Từ chối",
            null => "Chưa xác định",
            _ => $"Không xác định ({Status})"
        };


    }


    }