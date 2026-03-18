using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;

namespace VBSPOSS.ViewModels
{

    public class DeleteInterestRateConfigRequest
    {
        public long Id { get; set; }

        public long? DocumentId { get; set; } // Nullable if not always required

        public string IdList { get; set; }
    }

    public class SaveTideRateConfigureRequest
    {
        [JsonPropertyName("model")]
        public TideRateConfigureViewModel Model { get; set; }

        [JsonPropertyName("gridItems")]
        public List<TideTermViewModel> GridItems { get; set; }
    }


    public class SaveTempTideRateConfigureRequest
    {
        [JsonPropertyName("depositTerms")]
        public List<DepositTermModel> DepositTerms { get; set; }

        [JsonPropertyName("interestRate")]
        public double InterestRate { get; set; }

        [JsonPropertyName("beforeTermInterestRate")]
        public double BeforeTermInterestRate { get; set; }

        [JsonPropertyName("partialInterestRate")]
        public double PartialInterestRate { get; set; }

        [JsonPropertyName("depositType")]
        public string DepositType { get; set; }
    }

    public class TideRateConfigureViewModel
    {
        public long Id { get; set; } //  InterestRateConfigMaster
        public string ProductCode { get; set; }
        public string ProductName { get; set; } // Để hiển thị tên sản phẩm
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string CurrencyCode { get; set; } = "VND"; // Mặc định VND
        public string DebitCreditFlag { get; set; } = "Credit"; // Mặc định Dư Có
        public DateTime? EffectiveDate { get; set; }
        public decimal InterestRate { get; set; } // Lãi suất hiện tại từ API
        public decimal? NewInterestRate { get; set; } // Lãi suất mới do người dùng nhập
        public DateTime? ExpiredDate { get; set; } // Ngày hết hiệu lực
        public string CircularRefNum { get; set; } // Số quyết định
        public string PosCode { get; set; }
        public decimal? PenalRate { get; set; } // Mặc định 0 nếu cần
        public decimal? AmoutSlab { get; set; }
        public DateTime? CircularDate { get; set; }

        public List<string> ApplyPosList { get; set; }

        //public List<TideTermViewModel> TideTerms { get; set; } = new List<TideTermViewModel>();
    }

    public class TideTermViewModel
    {
        public long Id { get; set; }
        public string TermProductCode { get; set; }
        public string TermProductName { get; set; } // Để hiển thị tên sản phẩm
        public string TermAccountTypeCode { get; set; }
        public string TermAccountTypeName { get; set; }
        public string TermAccountSubTypeCode { get; set; }
        public DateTime? TermEffectiveDate { get; set; }
        public string TermPosCode { get; set; }
        public decimal? TermAmoutSlab { get; set; }
        public decimal? TermIntRate { get; set; }
        public decimal? TermIntRateNew { get; set; }

        /// <summary>
        /// Phần cấu hình cho biên độ lãi suất 
        /// </summary>
        
        public decimal? MinInterestRateSpread { get; set; }

        public decimal? MaxInterestRateSpread { get; set; }

        public decimal? MinTermIntRateNew { get; set; }

        public decimal? MaxTermIntRateNew { get; set; }

        //Loại lãi suất
        public string TermIntType { get; set; } = "";

        //Biên độ
        public decimal? TermSpreadRate { get; set; } = 0;
        public string TermCurrencyCode { get; set; } = "VND"; // Mặc định VND
        public int? TermSerial { get; set; } // Thứ tự hiển thị trong grid   

        public string TermDesc { get; set; } // Mô tả kỳ hạn

        public int? TermValue { get; set; } // Giá trị kỳ hạn (số ngày, số tháng, số năm)    

        public string InclusionFlag { get; set; } //Bao gồm hay không bao gồm (Inclusion/Exclusion)

        public string TermUnit { get; set; } // Đơn vị kỳ hạn (D/M/Y)

        public int? ChangeFlag { get; set; }

    }


    public class TideRateConfigureBatchModel
    {
        public long Id { get; set; } //  InterestRateConfigMaster
        public string ProductCode { get; set; }
        public string ProductName { get; set; } // Để hiển thị tên sản phẩm
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string CurrencyCode { get; set; } = "VND"; // Mặc định VND
        public string DebitCreditFlag { get; set; } = "Credit"; // Mặc định Dư Có
        public DateTime? EffectiveDate { get; set; }
        public decimal InterestRate { get; set; } // Lãi suất hiện tại từ API
        public decimal? NewInterestRate { get; set; } // Lãi suất mới do người dùng nhập

        public decimal? BeforeTermNewInterestRate { get; set; } // Lãi suất mới do người dùng nhập

        public decimal? PartialNewInterestRate { get; set; } // Lãi suất mới do người dùng nhập
        public DateTime? ExpiredDate { get; set; } // Ngày hết hiệu lực
        public string CircularRefNum { get; set; } // Số quyết định
        public string PosCode { get; set; }
        public List<string> ApplyPosList { get; set; }
        public decimal? PenalRate { get; set; } // Mặc định 0 nếu cần
        public decimal? AmoutSlab { get; set; }
        public DateTime? CircularDate { get; set; }

        //public List<TideTermViewModel> TideTerms { get; set; } = new List<TideTermViewModel>();
    }
}
