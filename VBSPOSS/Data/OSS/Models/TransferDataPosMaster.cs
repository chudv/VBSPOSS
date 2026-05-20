using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Models
{
    [Table("TransferDataPosMaster")]
    public class TransferDataPosMaster
    {
        [Column("Id")]
        public long Id { get; set; }

        [Column("MainPos")]
        public string MainPos { get; set; }

        [Column("FromPosCode")]
        public string FromPosCode { get; set; }

        [Column("ToPosCode")]
        public string ToPosCode { get; set; }

        [Column("EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Column("Remark")]
        public string Remark { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("TotalVillage")]
        public int TotalVillage { get; set; }

        [Column("CreatedBy")]
        public string CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ApproverBy")]
        public string ApproverBy { get; set; }

        [Column("ApprovalDate")]
        public DateTime? ApprovalDate { get; set; }

        [Column("RejectReason")]
        public string RejectReason { get; set; }

        //[Column("IsDeleted")]
        //public bool IsDeleted { get; set; }

       
    }
}
