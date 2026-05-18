using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.OSS.Models
{
    [Table("TransferDataPosDetail")]
    public class TransferDataPosDetail
    {
        [Column("Id")]
        public long Id { get; set; }

        [Column("MasterId")]
        public long MasterId { get; set; }

        [Column("FromCommuneId")]
        public string FromCommuneId { get; set; }

        [Column("FromVillageId")]
        public string FromVillageId { get; set; }

        [Column("ToCommuneId")]
        public string ToCommuneId { get; set; }

        [Column("ToVillageId")]
        public string ToVillageId { get; set; }

        [Column("CreatedBy")]
        public string CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }
}
