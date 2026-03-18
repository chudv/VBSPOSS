using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.Models
{
    [Table("InterestRateTermDetail")]
    public class InterestRateTermDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long IntRateConfigId { get; set; }

        [Required]
        public int Serial { get; set; }

        [StringLength(500)]
        public string? TermDesc { get; set; }

        [Required]
        public int TermValue { get; set; }

        [Required]
        [StringLength(50)]
        public string TermUnit { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string InclusionFlag { get; set; } = null!;

        [Column(TypeName = "decimal(18,6)")]
        public decimal? IntRate { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? IntRateNew { get; set; }

        [StringLength(50)]
        public string? IntRateType { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? Spread { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public int Status { get; set; } 

        public int? ChangeFlag { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal? MinInterestRateSpread { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal? MaxInterestRateSpread { get; set; }
    }
}
