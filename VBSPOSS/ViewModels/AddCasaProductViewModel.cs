using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VBSPOSS.ViewModels
{



    public class SaveCasaRateConfigureRequest
    {
        [JsonPropertyName("model")]
        public AddCasaProductViewModel Model { get; set; }

        [JsonPropertyName("gridItems")]
        public List<CasaRateProductViewModel> GridItems { get; set; }
    }

   


    public class AddCasaProductViewModel
    {
        public long Id { get; set; } //  InterestRateConfigMaster
        public string ProductCode { get; set; }
        public string ProductName { get; set; } // Để hiển thị tên sản phẩm
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string CurrencyCode { get; set; } = "VND"; // Mặc định VND
        public string DebitCreditFlag { get; set; } = "Credit"; // Mặc định Dư Có
        public DateTime? EffectiveDate { get; set; } = DateTime.Now.AddDays(1); // Mặc định 21/08/2025
        public decimal InterestRate { get; set; } // Lãi suất hiện tại từ API
        public decimal? NewInterestRate { get; set; } // Lãi suất mới do người dùng nhập
        public DateTime? ExpiredDate { get; set; } // Ngày hết hiệu lực

        public string CircularRefNum { get; set; } // Số quyết định
        public string PosCode { get; set; }
        public decimal? PenalRate { get; set; } // Mặc định 0 nếu cần
        public decimal? AmoutSlab { get; set; }
        public DateTime CircularDate { get; set; }

         public long? DocumentId { get; set; }

        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<SelectListItem> AccountTypes { get; set; } = new List<SelectListItem>(); // Thêm danh sách loại tài khoản
        public List<SelectListItem> AccountSubTypes { get; set; } = new List<SelectListItem>(); // Thêm danh sách loại tài khoản phụ

        // Thêm thuộc tính mới để chứa dữ liệu grid
        public List<CasaRateProductViewModel> GridItems { get; set; }

        public string IdList { get; set; } = "";  // Default empty string

        // THÊM: Property cho ApplyPosList (POS apply names, "POS1, POS2")
       // public string ApplyPosList { get; set; } = "";  // Nếu dùng trong View (fallback "")
      // public List<string> ApplyPosList { get; set; }

        public List<string> ApplyPosList { get; set; } = new List<string>();
    }


    //add CasaRateProductViewModel

    public class CasaRateProductViewModel
    {
        public long Id { get; set; }
        public string RateProductCode { get; set; }
        public string? RateProductName { get; set; }
        public string RateProductAccountTypeCode { get; set; }
        public string? RateProductAccountTypeName { get; set; }
        public string RateProductAccountSubTypeCode { get; set; }
        public string? RateProductCurrencyCode { get; set; }  // Mặc định VND
        public string RateProductDebitCreditFlag { get; set; }  // Mặc định Dư Có
        public DateTime? RateProductEffectiveDate { get; set; }
        public decimal? RateProductInterestRate { get; set; }
        public decimal? RateProductNewInterestRate { get; set; }
        public decimal? RateProductPenalRate { get; set; }
        public DateTime? RateProductExpiredDate { get; set; }
        public string? RateProductPosCode { get; set; }
        public decimal? RateProductAmoutSlab { get; set; }
        public int ChangeFlag { get; set; } = 0;

        public decimal InterestRateHO { get; set; }              // Thêm
        public decimal MinInterestRateSpread { get; set; }       // Thêm
        public decimal MaxInterestRateSpread { get; set; }       // Thêm

        //public decimal? MinInterestRateSpread { get; set; }

        //public decimal? MaxInterestRateSpread { get; set; }

        public decimal? MinTermIntRateNew { get; set; }

        public decimal? MaxTermIntRateNew { get; set; }
    }

    public class Product
    {
        public string ProductGroupCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubTypeName { get; set; }
        public string CurrencyCode { get; set; }
        public string DebitCreditFlag { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public decimal? InterestRate { get; set; } // Lãi suất hiện tại
        public decimal? NewInterestRate { get; set; }// Lãi suất mới 
    }
}
