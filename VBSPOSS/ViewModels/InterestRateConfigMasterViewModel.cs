namespace VBSPOSS.ViewModels
{
    public class InterestRateConfigMasterViewModel
    {
        public int OrderNo { get; set; }
        public long Id { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public string UserId { get; set; }
        public DateTime? CircularDate { get; set; }
        public string CircularRefNum { get; set; }
        public int RecordSerialNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubTypeName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal InterestRate { get; set; }
        public decimal PenalRate { get; set; }
        public decimal NewInterestRate { get; set; }
        public int TenorSerialNo { get; set; }
        public decimal AmountSlab { get; set; }
        public string IntRateType { get; set; }
        public decimal SpreadRate { get; set; }
        public string Remark { get; set; }
        public string OrtherNotes { get; set; }
        public int Status { get; set; }

        public string StatusDesc { get; set; }
        public int StatusUpdateCore { get; set; }

        public string StatusUpdateCoreDesc { get; set; }
        public string? CallApiTxnStatus { get; set; }
        public int? CallApiReqRecordSl { get; set; }
        public string? CallApiResponseCode { get; set; }
        public string? CallApiResponseMsg { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public long DocumentId { get; set; }
        public string DebitCreditFlag { get; set; }
        public string ProductList { get; set; }
        public string AccountTypeList { get; set; }
        public string IdList { get; set; }

        public string ApplyPosList { get; set; }    


        public int RejectFlag { get; set; }

        public string RejectReason { get; set; }
    }
}
