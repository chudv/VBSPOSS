using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VBSPOSS.Integration.Model;

namespace VBSPOSS.Integration.ViewModel
{
    public class AccountTypeIntRateConfigViewModel
    {
    }
    
    /// <summary>
    /// Đầu vào cho API lấy lãi suất của loại tài khoản Tide: http://10.63.54.52:7003/vbsp/internal/api/v1/getDepInterestRate
    /// </summary>
    public class TideIntRateRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Nguồn truy vấn. Ex 'MB'
        /// </summary>
        [JsonProperty("sourceId")]
        public string SourceId { get; set; }

        [JsonProperty("posCode")]
        public string PosCode { get; set; }

        [JsonProperty("effectDate")]
        public string EffectDate { get; set; }

        [JsonProperty("prodCode")]
        public string ProdCode { get; set; }


    }

    /// <summary>
    /// Trả ra của API Lấy lãi suất Tide: getDepInterestRate
    /// </summary>
    public class TideIntRateResposeViewModel
    {
        [JsonProperty("prodDetails")]
        [JsonConverter(typeof(SingleOrArrayConverter<ProdDetail>))]
        public List<ProdDetail> ProdDetails { get; set; }
    }

    public class ProdDetail
    {
        [JsonProperty("prodCode")]
        public string ProdCode { get; set; }

        [JsonProperty("effectDate")]
        public string EffectDate { get; set; }

        [JsonProperty("slabRange")]
        public string SlabRange { get; set; }

        [JsonProperty("prodName")]
        public string ProdName { get; set; }

        [JsonProperty("termDetails")]
        public List<TermDetails> TermDetails { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("depositSubType")]
        public string DepositSubType { get; set; }

        [JsonProperty("depositType")]
        public string DepositType { get; set; }
    }

    public class TermDetails
    {
        [JsonProperty("intRate")]
        public string IntRate { get; set; }

        [JsonProperty("serial")]
        public string Serial { get; set; }

        [JsonProperty("termDesc")]
        public string TermDesc { get; set; }

        [JsonProperty("termValue")]
        public string TermValue { get; set; }

        [JsonProperty("inclusionFlag")]
        public string InclusionFlag { get; set; }

        [JsonProperty("termUnit")]
        public string TermUnit { get; set; }
    }

    


    /// <summary>
    /// Đầu vào cho API lấy lãi suất của loại tài khoản Casa: http://10.63.54.52:7003/vbsp/internal/api/v1/getCasaIntRts
    /// </summary>
    public class CasaIntRateRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("effectiveDate")]
        public string EffectiveDate { get; set; }
        /// <summary>
        /// Cờ nhận diện tính chất sản phẩm/loại tài khoản ghi nợ hay có. Giá trị: mặc định truyền vào là 'C'
        /// </summary>
        [JsonProperty("debitCreditFlag")]
        public string DebitCreditFlag { get; set; }

        [JsonProperty("posCode")]
        public string PosCode { get; set; }
    }


    public class CasaIntRateReposeViewModel
    {
        [JsonProperty("interestRate")]
        public string InterestRate { get; set; }

        [JsonProperty("prodCode")]
        public string ProdCode { get; set; }

        [JsonProperty("debitCreditFlag")]
        public string DebitCreditFlag { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("penalRate")]
        public string PenalRate { get; set; }

        [JsonProperty("posRateExpiryDate")]
        public string PosRateExpiryDate { get; set; }

        [JsonProperty("posCode")]
        public string PosCode { get; set; }

        [JsonProperty("subType")]
        public string SubType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("circularRef")]
        public string CircularRef { get; set; }

        [JsonProperty("effectiveDate")]
        public string EffectiveDate { get; set; }

        [JsonProperty("circularDate")]
        public string CircularDate { get; set; }
    }


    /// <summary>
    /// Đầu vào cho API lấy lãi suất của loại tài khoản Casa: http://10.63.54.52:7003/vbsp/internal/api/v1/DepPenalIntRt
    /// </summary>
    public class DepositPenalIntRateRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("effDate")]
        public string EffDate { get; set; }

        [JsonProperty("posCode")]
        public string PosCode { get; set; }
    }

    public class DepositPenalIntRateReposeViewModel
    {
        [JsonProperty("effDate")]
        public string EffDate { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("penalIntRate")]
        public string PenalIntRate { get; set; }

        [JsonProperty("posCode")]
        public string PosCode { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public class ResposeRecordViewModel
    {
        [JsonProperty("respRecord")]
        public List<RespRecord> RespRecord { get; set; }
    }

    public class RespRecord
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus;

        [JsonProperty("reqRecordSl")]
        public string ReqRecordSL;

        [JsonProperty("responseCode")]
        public string ResponseCode;

        [JsonProperty("ResponseMsg")]
        public string ResponseMsg;
    }

    public class CasaIntRatesRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("bankCircularDate")]
        public string BankCircularDate;

        [JsonProperty("bankCircularRefNum")]
        public string BankCircularRefNum;

        [JsonProperty("interestRates")]
        public InterestRates InterestRates { get; set; }
    }

    public class InterestRates
    {
        [JsonProperty("record")]
        public List<RecordInterestRatesViewModel> Record { get; set; }
    }

    public class RecordInterestRatesViewModel
    {
        [JsonProperty("recordSl")]
        public string RecordSl;

        [JsonProperty("productCode")]
        public string ProductCode;

        [JsonProperty("accountType")]
        public string AccountType;

        [JsonProperty("accountSubType")]
        public string AccountSubType;

        [JsonProperty("currencyCode")]
        public string CurrencyCode;

        [JsonProperty("effectiveDate")]
        public string EffectiveDate;

        [JsonProperty("debitCreditFlag")]
        public string DebitCreditFlag;

        [JsonProperty("posCode")]
        public string PosCode;

        [JsonProperty("posRateExpiryDate")]
        public string PosRateExpiryDate;

        [JsonProperty("interestRate")]
        public string InterestRate;

        [JsonProperty("penalRate")]
        public string PenalRate;
    }



    #region --- TidePenalRates: Đầu vào đầu ra cho cấu hình lãi suất Rút trước hạn Tide ---
    public class TidePenalIntRatesRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("interestRates")]
        public TidePenalInterestRatesRequestViewModel TidePenalInterestRatesRequestViewModel { get; set; }
    }

    public class TidePenalInterestRatesRequestViewModel
    {
        [JsonProperty("record")]
        public List<RecordTidePenalInterestRateViewModel> RecordTidePenalInterestRateViewModel { get; set; }
    }

    public class RecordTidePenalInterestRateViewModel
    {
        [JsonProperty("recordSl")]
        public string RecordSl;

        [JsonProperty("productCode")]
        public string ProductCode;

        [JsonProperty("currencyCode")]
        public string CurrencyCode;

        [JsonProperty("effectiveDate")]
        public string EffectiveDate;

        [JsonProperty("posCode")]
        public string PosCode;

        [JsonProperty("interestRate")]
        public string InterestRate;
    }
    #endregion

    #region --- TideIntRates: Đầu vào đầu ra cho cấu hình lãi suất tiền gửi có kỳ hạn Tide ---
    public class TideIntRatesRequestViewModel
    {
        [JsonProperty("userId")]
        public string UserId;

        [JsonProperty("bankCircularDate")]
        public string BankCircularDate;

        [JsonProperty("bankCircularRefNum")]
        [StringLength(28, ErrorMessage = "Số quyết định không thể vượt quá 28 ký tự (BankCircularRefNum cannot exceed 28 characters)")]
        public string BankCircularRefNum;

        /// <summary>
        /// Danh sách update
        /// </summary>
        [JsonProperty("upldIntRt")]
        public TideInterestRatesRequestViewModel UpldIntRt { get; set; }
    }

    public class TideInterestRatesRequestViewModel
    {
        /// <summary>
        /// Danh sách cấu hình
        /// </summary>
        [JsonProperty("tideIntRates")]
        public List<RecordTideInterestRateViewModel> TideIntRates { get; set; }
    }

    public class RecordTideInterestRateViewModel
    {
        [JsonProperty("recordSl")]
        public string RecordSl;

        [JsonProperty("productCode")]
        public string ProductCode;

        [JsonProperty("accountType")]
        public string AccountType;

        [JsonProperty("accountSubType")]
        public string AccountSubType;

        [JsonProperty("currencyCode")]
        public string CurrencyCode;
        /// <summary>
        /// Ngày hiệu lực. Định dạng: yyyyMMdd
        /// </summary>
        [JsonProperty("effectiveDate")]
        public string EffectiveDate;
        
        /// <summary>
        /// Mức tiền gửi
        /// </summary>
        [JsonProperty("amountSlab")]
        public string AmountSlab;

        [JsonProperty("posCode")]
        public string PosCode;

        /// <summary>
        /// Ngày hết hạn. Định dạng: yyyyMMdd
        /// </summary>
        [JsonProperty("posRateExpiryDate")]
        public string PosRateExpiryDate;

        /// <summary>
        /// Danh sách lãi suất tương ứng
        /// </summary>
        [JsonProperty("interestRates")]
        public ChildTideInterestRateViewModel InterestRates;
    }

    public class ChildTideInterestRateViewModel
    {
        [JsonProperty("record")]
        public List<ChildRecordTideInterestRateViewModel> ChildTideInterestRateRecord { get; set; }
    }

    public class ChildRecordTideInterestRateViewModel
    {
        /// <summary>
        /// Số thứ tự của kỳ hạn
        /// </summary>
        [JsonProperty("tenorSl")]
        public string TenorSl;
        
        /// <summary>
        /// Lãi suất tương ứng với kỳ hạn
        /// </summary>
        [JsonProperty("interestRate")]
        public string InterestRate;

        /// <summary>
        /// Loại lãi suất. Kiểu là lãi suất ăn theo kỳ nào đó linh hoạt mới dùng
        /// </summary>
        [JsonProperty("intRateType")]
        public string IntRateType;

        /// <summary>
        /// Biên độ lãi suất
        /// </summary>
        [JsonProperty("spreadRate")]
        public string SpreadRate;
    }
    #endregion

}
