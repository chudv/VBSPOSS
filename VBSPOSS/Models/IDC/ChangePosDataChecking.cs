using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Models.IDC
{
    [Table(
        "CHANGE_POS_DATA_CHECKING",
        Schema = "IDL_LMS")]
    public class ChangePosDataChecking
    {
        [Column("TYPE_CD")]
        [StringLength(10)]
        public string TYPE_CD { get; set; }

        [Column("POS_CD")]
        [StringLength(6)]
        public string POS_CD { get; set; }

        [Key]
        [Column("SCOM_CD", Order = 0)]
        [StringLength(10)]
        public string SCOM_CD { get; set; }

        [Key]
        [Column("CUST_ID", Order = 1)]
        [StringLength(10)]
        public string CUST_ID { get; set; }

        [Column("CUST_NAME")]
        [StringLength(200)]
        public string CUST_NAME { get; set; }

        [Column("AC_NO")]
        [StringLength(20)]
        public string AC_NO { get; set; }

        [Column("PROD_CD")]
        [StringLength(200)]
        public string PROD_CD { get; set; }

        [Column("BOOKING_DT")]
        [StringLength(10)]
        public string BOOKING_DT { get; set; }

        [Column("MAT_DT")]
        [StringLength(10)]
        public string MAT_DT { get; set; }

        [Column("PRIN_AMOUNT")]
        public decimal? PRIN_AMOUNT { get; set; }

        [Column("INT_AMOUNT")]
        public decimal? INT_AMOUNT { get; set; }

        [Column("GROUP_ID")]
        [StringLength(10)]
        public string GROUP_ID { get; set; }

        [Column("GROUP_NAME")]
        [StringLength(100)]
        public string GROUP_NAME { get; set; }

        [Column("INV_ID")]
        [StringLength(50)]
        public string INV_ID { get; set; }

        [Column("NEW_POS_CD")]
        [StringLength(6)]
        public string NEW_POS_CD { get; set; }

        [Column("NEW_SCOM_CD")]
        [StringLength(10)]
        public string NEW_SCOM_CD { get; set; }

        [Column("MKR_DT")]
        public DateTime? MKR_DT { get; set; }

        [Column("ACCEPT_MOV")]
        [StringLength(1)]
        public string ACCEPT_MOV { get; set; }

        [Column("MKR_ACCEPT_MOV")]
        [StringLength(200)]
        public string MKR_ACCEPT_MOV { get; set; }

        [Column("MOV_STATUS")]
        [StringLength(1000)]
        public string MOV_STATUS { get; set; }
    }
}