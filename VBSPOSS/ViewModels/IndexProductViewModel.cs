namespace VBSPOSS.ViewModels
{
    public class IndexProductViewModel
    {

        public int Id { get; set; }
        public string ProductGroupCode { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeName { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubTypeName { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string DebitCreditFlag { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
