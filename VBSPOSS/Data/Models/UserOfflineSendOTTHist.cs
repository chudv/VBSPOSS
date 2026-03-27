using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VBSPOSS.Data.Models
{
    public class UserOfflineSendOTTHist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required, StringLength(8)]
        public string PosCode { get; set; }

        [Required, StringLength(128)]
        public string PosName { get; set; }

        [Required, StringLength(8)]
        public string CommuneCode { get; set; }

        [Required, StringLength(128)]
        public string CommuneName { get; set; }

        [Required, StringLength(64)]
        public string TxnPointCode { get; set; }

        [Required, StringLength(128)]
        public string TxnPointName { get; set; }

        [Column(TypeName = "date")]
        public DateTime TransDate { get; set; }

        [Required, StringLength(64)]
        public string UserIdOffline { get; set; }

        [Required, StringLength(128)]
        public string PassWord { get; set; }

        [Required, StringLength(64)]
        public string RoleCode { get; set; }

        [Required, StringLength(16)]
        public string MobileNo { get; set; }

        [Required, StringLength(256)]
        public string EmailId { get; set; }

        public int Status { get; set; }

        [StringLength(512)]
        public string? Remark { get; set; }

        [StringLength(64)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(64)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [StringLength(64)]
        public string? ApproverBy { get; set; }

        public DateTime? ApprovalDate { get; set; }
    }
}
