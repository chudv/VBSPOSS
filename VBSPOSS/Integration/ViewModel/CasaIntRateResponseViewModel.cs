using System.Collections.Generic;
using Newtonsoft.Json;

namespace VBSPOSS.Integration.ViewModel
{
    public class CasaIntRateResponseViewModel
    {
        public string txnStatus { get; set; }
        public List<CasaIntRateRecord> record { get; set; }
        public string responseCode { get; set; }
        public string responseMsg { get; set; }
    }

    public class CasaIntRateRecord
    {
        [JsonProperty("interestRate")]
        public string interestRateString { get; set; } // Lưu giá trị chuỗi từ API

        [JsonIgnore]
        public decimal interestRate
        {
            get => decimal.TryParse(interestRateString, out var result) ? result : 0;
            set => interestRateString = value.ToString();
        }

        public string prodCode { get; set; }
        public string debitCreditFlag { get; set; }
        public string accountType { get; set; }

        [JsonProperty("penalRate")]
        public string penalRateString { get; set; }

        [JsonIgnore]
        public decimal penalRate
        {
            get => decimal.TryParse(penalRateString, out var result) ? result : 0;
            set => penalRateString = value.ToString();
        }

        public string posRateExpiryDate { get; set; }
        public string posCode { get; set; }
        public string subType { get; set; }
        public string currency { get; set; }
        public string circularRef { get; set; }
        public string effectiveDate { get; set; }
        public string circularDate { get; set; }
    }
}