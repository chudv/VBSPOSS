using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.ViewModels
{
    public class ListOfTransPointViewModel
    {
        public int STT { get; set; }

        public string ProvinceCode { get; set; }

        public string ProvinceName { get; set; }

        public string PosCode { get; set; }

        public string PosName { get; set; }

        public string DistrictCode { get; set; }

        public string DistrictName { get; set; }

        public string CommuneCode { get; set; }

        public string CommuneName { get; set; }

        public string TxnPointCode { get; set; }

        public string TxnPointName { get; set; }
        
        public string VisitDate { get; set; }

        public string Times { get; set; }
        
        public string TimeBegin { get; set; }

        public string TimeEnd { get; set; }

        public decimal TimeBeginNum { get; set; }

        public decimal TimeEndNum { get; set; }

        public decimal Hours { get; set; }

        public decimal Minutes { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        public string IsInCommune { get; set; }

        public string IsInPos { get; set; }

        public string IsInterWard { get; set; }
        
        public string InterWardName { get; set; }

        public DateTime EffectiveDate { get; set; }
        
        public string TxnLocation { get; set; }

        public string AddressDetail { get; set; }

        public string AddressCode { get; set; }

        public string AddressFull { get; set; }

        public string PhoneSupport { get; set; }

        public string PhoneSupport01 { get; set; }

        public string PhoneSupport02 { get; set; }

        public string TxnStatus { get; set; }

        public int Status { get; set; }

        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime ApprovalDate { get; set; }
    }
}
