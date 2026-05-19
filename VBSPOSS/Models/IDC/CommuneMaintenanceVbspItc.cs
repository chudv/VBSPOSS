using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Models.IDC
{
    [Table("COMMUNE_MAINTENANCE_VBSP_ITC")]
    public class CommuneMaintenanceVbspItc
    {
        [Key]
        [Column("JOB_ID")]
        public long JOB_ID { get; set; }

        [Column("PROV_CD")]
        public string PROV_CD { get; set; }

        [Column("DIST_CD")]
        public string DIST_CD { get; set; }

        [Column("COMM_CD")]
        public string COMM_CD { get; set; }

        [Column("SCOM_CD")]
        public string SCOM_CD { get; set; }

        [Column("COMM_NAME")]
        public string COMM_NAME { get; set; }

        [Column("TXN_ID")]
        public string TXN_ID { get; set; }

        [Column("VISIT_DT")]
        public string VISIT_DT { get; set; }

        [Column("POS_CD")]
        public string POS_CD { get; set; }

        [Column("NEW_COMMUNE")]
        public string NEW_COMMUNE { get; set; }

        [Column("FEE_LEN_AREA")]
        public string FEE_LEN_AREA { get; set; }

        [Column("COM_LEN_AREA")]
        public string COM_LEN_AREA { get; set; }

        [Column("CASA_AREA")]
        public string CASA_AREA { get; set; }

        [Column("TYPE_CD")]
        public string TYPE_CD { get; set; }

        [Column("MKR_DT")]
        public DateTime? MKR_DT { get; set; }

        [Column("STATUS")]
        public string STATUS { get; set; }

        [Column("NEW_POS_CD")]
        public string NEW_POS_CD { get; set; }

        [Column("DIFF_AREA")]
        public string DIFF_AREA { get; set; }
    }
}
