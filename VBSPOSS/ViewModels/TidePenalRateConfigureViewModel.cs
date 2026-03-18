using System.Text.Json.Serialization;
using VBSPOSS.Constants;

namespace VBSPOSS.ViewModels
{
    public class TidePenalRateConfigureViewModel
    {
    }

    public class SaveTidePenalRateConfigRequest
    {
        [JsonPropertyName("flagCall")]
        public string FlagCall { get; set; }

        [JsonPropertyName("posCode")]
        public string PosCode { get; set; }

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("effectiveDate")]
        public string EffectiveDate { get; set; }
        
        [JsonPropertyName("expiredDate")]
        public string ExpiredDate { get; set; }

        [JsonPropertyName("circularRefNum")]
        public string CircularRefNum { get; set; }

        [JsonPropertyName("circularDate")]
        public string CircularDate { get; set; }

        [JsonPropertyName("interestRate")]
        public decimal InterestRate { get; set; }

        [JsonPropertyName("penalIntRate")]
        public decimal PenalIntRate { get; set; }

        [JsonPropertyName("listItemUpdates")]
        public List<UpdateTidePenalRateConfigViewModel> ListItemUpdates { get; set; }

        [JsonPropertyName("listPosCodeChoice")]
        public string ListPosCodeChoice { get; set; }

        [JsonPropertyName("listProductCodeChoice")]
        public string ListProductCodeChoice { get; set; }
    }

    public class UpdateTidePenalRateConfigViewModel
    {
        public string IdList { get; set; }
        public long Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string CurrencyCode { get; set; } = ConstValueAPI.CurrencyValueDefault;
        public string DebitCreditFlag { get; set; } = VBSPOSS.Constants.DebitCreditFlag.DebitCreditFlag_Credit;
        public DateTime EffectiveDate { get; set; }
        public DateTime EffectiveDateNew { get; set; }
        public decimal InterestRate { get; set; } // Lãi suất hiện tại từ API
        public decimal InterestRateNew { get; set; } // Lãi suất mới do người dùng nhập
        public DateTime ExpiredDate { get; set; } // Ngày hết hiệu lực
        public string CircularRefNum { get; set; } // Số quyết định
        public string CircularRefNumNew { get; set; } // Số quyết định
        public string PosCode { get; set; }
        public decimal PenalIntRate { get; set; } // Mặc định 0 nếu cần
        public decimal TenorSerialNo { get; set; }
        public DateTime CircularDate { get; set; }
        public int OrderNo { get; set; }
        public string PosName { get; set; }
        public string EffectiveDateText { get; set; }
        public string EffectiveDateTextNew { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int StatusUpdateCore { get; set; }
        public string ProductCodeList { get; set; }
        public string ExpiredDateText { get; set; }
        public string CircularDateText { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedDateText { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedDateText { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string ApprovalDateText { get; set; }

        public string ProductList { get; set; }
        public long DocumentId { get; set; }

        public int RejectFlag { get; set; }

        public string RejectReason { get; set; }
        public string ListPosCodeChoice { get; set; }
        public string ListProductCodeChoice { get; set; }

        public List<string> ListPosCode { get; set; }
        public List<string> ListProductCode { get; set; }
        public decimal MinInterestRateSpread { get; set; }
        public decimal MaxInterestRateSpread { get; set; }

        public decimal InterestRateHO { get; set; }
    }

}
