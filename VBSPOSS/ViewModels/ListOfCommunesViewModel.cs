using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.ViewModels
{
    public class ListOfCommunesViewModel
    {
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string CommuneCode { get; set; }
        public string CommuneName { get; set; }
        public string SubCommuneCode { get; set; }
        public string SubCommuneName { get; set; }
        public int Status { get; set; }
        public string RecordStatus { get; set; }
        public string DistrictFlag30A { get; set; }
        public string AreaEconomic { get; set; }
        public string CommuneFlag135 { get; set; }
        public string Region_01 { get; set; }
        public string Region_02 { get; set; }
        public string Region_03 { get; set; }
        public string Region_04 { get; set; }
        public string IsNewCountryside { get; set; }
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
        public DateTime EffectDate { get; set; }
        public DateTime BusinessDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int StatusUpdateCore { get; set; }
        public string CallApiTxnStatus { get; set; }
        public int CallApiResRecords { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
    }
    public class ListOfCommunesHistViewModel
    {
        public string EventCode { get; set; }
        public long Id { get; set; }
        public DateTime DateSync { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string CommuneCode { get; set; }
        public string CommuneName { get; set; }
        public string SubCommuneCode { get; set; }
        public string SubCommuneName { get; set; }
        public int Status { get; set; }
        public string RecordStatus { get; set; }
        public string DistrictFlag30A { get; set; }
        public string AreaEconomic { get; set; }
        public string CommuneFlag135 { get; set; }
        public string Region_01 { get; set; }
        public string Region_02 { get; set; }
        public string Region_03 { get; set; }
        public string Region_04 { get; set; }
        public string DiffAreaCode { get; set; }
        public string IsNewCountryside { get; set; }
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
        public DateTime EffectDate { get; set; }
        public DateTime BusinessDate { get; set; }
        public long DocumentId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int StatusUpdateCore { get; set; }
        public string CallApiTxnStatus { get; set; }
        public int CallApiResRecords { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
    }
    public class ListOfCommunesWorkViewModel
    {
        public string EventCode { get; set; }
        public long ParentId { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string CommuneCode { get; set; }
        public string CommuneName { get; set; }
        public string SubCommuneCode { get; set; }
        public string SubCommuneName { get; set; }
        public int Status { get; set; }
        public string RecordStatus { get; set; }
        public string DistrictFlag30A { get; set; }
        public string AreaEconomic { get; set; }
        public string CommuneFlag135 { get; set; }
        public string Region_01 { get; set; }
        public string Region_02 { get; set; }
        public string Region_03 { get; set; }
        public string Region_04 { get; set; }
        public string DiffAreaCode { get; set; }
        public string IsNewCountryside { get; set; }
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
        public DateTime EffectDate { get; set; }
        public DateTime BusinessDate { get; set; }
        public long DocumentId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int StatusUpdateCore { get; set; }
        public string CallApiTxnStatus { get; set; }
        public int CallApiResRecords { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
    }
}
