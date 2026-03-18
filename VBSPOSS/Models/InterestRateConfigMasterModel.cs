using VBSPOSS.Constants;

namespace VBSPOSS.Models
{
    public class InterestRateConfigMasterModel
    {
        public long Id { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProductGroupCode { get; set; }
        public string UserId { get; set; }
        public DateTime? CircularDate { get; set; }
        public string CircularRefNum { get; set; }
        public string RecordSerialNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubTypeName { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string DebitCreditFlag { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? PenalRate { get; set; }
        public decimal? AmountSlab { get; set; }
        public int? TenorSerialNo { get; set; }
        //public int? IntRateType { get; set; }

        public string IntRateType { get; set; }
        public decimal? SpreadRate { get; set; }
        public string Remark { get; set; }
        public string OrtherNotes { get; set; }
        public int? Status { get; set; }

        public string StatusDesc
        {
            get
            {

                int statusValue = Status ?? 0; // Nếu Status là int? → tự unbox, nếu null thì 0
                return ConfigStatus.GetByValue(statusValue)?.Description ?? "Không xác định";
            }
            private set { } // 
        }
        public int? StatusUpdateCore { get; set; }
        public string CallApiTxnStatus { get; set; }
        public string CallApiReqRecordSl { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        //  public string? DocumentId { get; set; }

        public long? DocumentId { get; set; }
        public bool IsSelected { get; set; }


        public decimal? NewInterestRate { get; set; }

        public string FlagCall { get; set; }
        public string ProductList { get; set; }

        public string IdList { get; set; } = string.Empty;

       // public string ApplyPosList { get; set; } = string.Empty;
    }
}