using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.Models
{
    public class TideTermWorking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string SessionId { get; set; }
        public string TermProductCode { get; set; }
        public string TermProductName { get; set; }
        public string TermAccountTypeCode { get; set; }
        public string TermAccountTypeName { get; set; }
        public string TermAccountSubTypeCode { get; set; }
        public DateTime? TermEffectiveDate { get; set; }
        public string TermPosCode { get; set; }
        public decimal? TermAmoutSlab { get; set; }
        public decimal? TermIntRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TermIntRateNew { get; set; }
        public string TermIntType { get; set; }
        public decimal? TermSpreadRate { get; set; }
        public string TermCurrencyCode { get; set; }
        public int TermSerial { get; set; }
        public string TermDesc { get; set; }
        public int TermValue { get; set; }
        public string InclusionFlag { get; set; }
        public string TermUnit { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public int ChangeFlag { get; set; }

        public decimal? MaxInterestRateSpread { get; set; }

        public decimal? MinInterestRateSpread { get; set; }
    }
}
