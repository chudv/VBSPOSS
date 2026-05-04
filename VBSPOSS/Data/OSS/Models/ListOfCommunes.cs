using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.Data.OSS.Models
{
    public class ListOfCommunes
    {
        [Column("PosCode")]
        public string PosCode { get; set; }
        [Column("PosName")]
        public string PosName { get; set; }
        [Column("ProvinceCode")]
        public string ProvinceCode { get; set; }
        [Column("ProvinceName")]
        public string ProvinceName { get; set; }
        [Column("DistrictCode")]
        public string DistrictCode { get; set; }
        [Column("DistrictName")]
        public string DistrictName { get; set; }
        [Column("CommuneCode")]
        public string CommuneCode { get; set; }
        [Column("CommuneName")]
        public string CommuneName { get; set; }
        [Column("SubCommuneCode")]
        public string SubCommuneCode { get; set; }
        [Column("SubCommuneName")]
        public string SubCommuneName { get; set; }
        [Column("Status")]
        public int Status { get; set; }
        [Column("RecordStatus")]
        public string RecordStatus { get; set; }
        [Column("DistrictFlag30A")]
        public string DistrictFlag30A { get; set; }
        [Column("AreaEconomic")]
        public string AreaEconomic { get; set; }
        [Column("CommuneFlag135")]
        public string CommuneFlag135 { get; set; }
        [Column("Region_01")]
        public string Region_01 { get; set; }
        [Column("Region_02")]
        public string Region_02 { get; set; }
        [Column("Region_03")]
        public string Region_03 { get; set; }
        [Column("Region_04")]
        public string Region_04 { get; set; }
        [Column("IsNewCountryside")]
        public string IsNewCountryside { get; set; }
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
        [Column("EffectDate")]
        public DateTime EffectDate { get; set; }
        [Column("BusinessDate")]
        public DateTime BusinessDate { get; set; }
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
        [Column("StatusUpdateCore")]
        public int StatusUpdateCore { get; set; }
        [Column("CallApiTxnStatus")]
        public string CallApiTxnStatus { get; set; }
        [Column("CallApiResRecords")]
        public int CallApiResRecords { get; set; }  
        [Column("CallApiResponseCode")]
        public string CallApiResponseCode { get; set; }
        [Column("CallApiResponseMsg")]
        public string CallApiResponseMsg { get; set; }
    }
    public class ListOfCommunesHist
    {
        [Column("EventCode")]
        public string EventCode { get; set; }

        [Column("Id")]
        public long Id { get; set; }

        [Column("DateSync")]
        public DateTime DateSync { get; set; }

        [Column("PosCode")]
        public string PosCode { get; set; }

        [Column("PosName")]
        public string PosName { get; set; }

        [Column("ProvinceCode")]
        public string ProvinceCode { get; set; }

        [Column("ProvinceName")]
        public string ProvinceName { get; set; }

        [Column("DistrictCode")]
        public string DistrictCode { get; set; }

        [Column("DistrictName")]
        public string DistrictName { get; set; }

        [Column("CommuneCode")]
        public string CommuneCode { get; set; }

        [Column("CommuneName")]
        public string CommuneName { get; set; }

        [Column("SubCommuneCode")]
        public string SubCommuneCode { get; set; }

        [Column("SubCommuneName")]
        public string SubCommuneName { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("RecordStatus")]
        public string RecordStatus { get; set; }

        [Column("DistrictFlag30A")]
        public string DistrictFlag30A { get; set; }

        [Column("AreaEconomic")]
        public string AreaEconomic { get; set; }

        [Column("CommuneFlag135")]
        public string CommuneFlag135 { get; set; }

        [Column("Region_01")]
        public string Region_01 { get; set; }

        [Column("Region_02")]
        public string Region_02 { get; set; }

        [Column("Region_03")]
        public string Region_03 { get; set; }

        [Column("Region_04")]
        public string Region_04 { get; set; }

        [Column("DiffAreaCode")]
        public string DiffAreaCode { get; set; }

        [Column("IsNewCountryside")]
        public string IsNewCountryside { get; set; }

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

        [Column("EffectDate")]
        public DateTime? EffectDate { get; set; }

        [Column("BusinessDate")]
        public DateTime? BusinessDate { get; set; }

        [Column("DocumentId")]
        public long? DocumentId { get; set; }

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

        [Column("StatusUpdateCore")]
        public int StatusUpdateCore { get; set; }

        [Column("CallApiTxnStatus")]
        public string CallApiTxnStatus { get; set; }

        [Column("CallApiResRecords")]
        public int CallApiResRecords { get; set; }

        [Column("CallApiResponseCode")]
        public string CallApiResponseCode { get; set; }

        [Column("CallApiResponseMsg")]
        public string CallApiResponseMsg { get; set; }
    }
    public class ListOfCommunesWork
    {
        [Column("EventCode")]
        public string EventCode { get; set; }

        [Column("ParentId")]
        public long ParentId { get; set; }

        [Column("PosCode")]
        public string PosCode { get; set; }

        [Column("PosName")]
        public string PosName { get; set; }

        [Column("ProvinceCode")]
        public string ProvinceCode { get; set; }

        [Column("ProvinceName")]
        public string ProvinceName { get; set; }

        [Column("DistrictCode")]
        public string DistrictCode { get; set; }

        [Column("DistrictName")]
        public string DistrictName { get; set; }

        [Column("CommuneCode")]
        public string CommuneCode { get; set; }

        [Column("CommuneName")]
        public string CommuneName { get; set; }

        [Column("SubCommuneCode")]
        public string SubCommuneCode { get; set; }

        [Column("SubCommuneName")]
        public string SubCommuneName { get; set; }

        [Column("Status")]
        public int? Status { get; set; }

        [Column("RecordStatus")]
        public string RecordStatus { get; set; }

        [Column("DistrictFlag30A")]
        public string DistrictFlag30A { get; set; }

        [Column("AreaEconomic")]
        public string AreaEconomic { get; set; }

        [Column("CommuneFlag135")]
        public string CommuneFlag135 { get; set; }

        [Column("Region_01")]
        public string Region_01 { get; set; }

        [Column("Region_02")]
        public string Region_02 { get; set; }

        [Column("Region_03")]
        public string Region_03 { get; set; }

        [Column("Region_04")]
        public string Region_04 { get; set; }

        [Column("DiffAreaCode")]
        public string DiffAreaCode { get; set; }

        [Column("IsNewCountryside")]
        public string IsNewCountryside { get; set; }

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

        [Column("EffectDate")]
        public DateTime EffectDate { get; set; }

        [Column("BusinessDate")]
        public DateTime BusinessDate { get; set; }

        [Column("DocumentId")]
        public long? DocumentId { get; set; }

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

        [Column("StatusUpdateCore")]
        public int StatusUpdateCore { get; set; }

        [Column("CallApiTxnStatus")]
        public string CallApiTxnStatus { get; set; }

        [Column("CallApiResRecords")]
        public int CallApiResRecords { get; set; }

        [Column("CallApiResponseCode")]
        public string CallApiResponseCode { get; set; }

        [Column("CallApiResponseMsg")]
        public string CallApiResponseMsg { get; set; }
    }
}
