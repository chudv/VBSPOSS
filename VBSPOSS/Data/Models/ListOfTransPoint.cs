using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.Data.Models
{
    #region ---Model ListOfTransPoint - Danh mục điểm giao dịch ---
    public class ListOfTransPoint
    {
        [Column("ProvinceCode")]
        public string ProvinceCode { get; set; }

        [Column("ProvinceName")]
        public string ProvinceName { get; set; }

        [Column("PosCode")]
        public string PosCode { get; set; }

        [Column("PosName")]
        public string PosName { get; set; }

        [Column("DistrictCode")]
        public string DistrictCode { get; set; }

        [Column("DistrictName")]
        public string DistrictName { get; set; }

        [Column("CommuneCode")]
        public string CommuneCode { get; set; }

        [Column("CommuneName")]
        public string CommuneName { get; set; }

        [Column("TxnPointCode")]
        public string TxnPointCode { get; set; }

        [Column("TxnPointName")]
        public string TxnPointName { get; set; }
        
        [Column("VisitDate")]
        public string VisitDate { get; set; }

        [Column("Times")]
        public string Times { get; set; }
        
        [Column("TimeBegin")]
        public string TimeBegin { get; set; }

        [Column("TimeEnd")]
        public string TimeEnd { get; set; }

        [Column("TimeBeginNum")]
        public decimal TimeBeginNum { get; set; }

        [Column("TimeEndNum")]
        public decimal TimeEndNum { get; set; }

        [Column("Hours")]
        public decimal Hours { get; set; }

        [Column("Minutes")]
        public decimal Minutes { get; set; }

        [Column("Longitude")]
        public decimal Longitude { get; set; }

        [Column("Latitude")]
        public decimal Latitude { get; set; }

        [Column("IsInCommune")]
        public string IsInCommune { get; set; }

        [Column("IsInPos")]
        public string IsInPos { get; set; }

        [Column("IsInterWard")]
        public string IsInterWard { get; set; }
        
        [Column("InterWardName")]
        public string InterWardName { get; set; }

        [Column("EffectiveDate")]
        public DateTime EffectiveDate { get; set; }
        
        [Column("TxnLocation")]
        public string TxnLocation { get; set; }

        [Column("AddressDetail")]
        public string AddressDetail { get; set; }

        [Column("AddressCode")]
        public string AddressCode { get; set; }

        [Column("AddressFull")]
        public string AddressFull { get; set; }

        [Column("PhoneSupport")]
        public string PhoneSupport { get; set; }

        [Column("PhoneSupport01")]
        public string PhoneSupport01 { get; set; }

        [Column("PhoneSupport02")]
        public string PhoneSupport02 { get; set; }

        [Column("TxnStatus")]
        public string TxnStatus { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("Remark")]
        public string Remark { get; set; }

        [Column("CreatedBy")]
        public string CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [Column("ApproverBy")]
        public string ApproverBy { get; set; }

        [Column("ApprovalDate")]
        public DateTime ApprovalDate { get; set; }
    }
    #endregion
}
