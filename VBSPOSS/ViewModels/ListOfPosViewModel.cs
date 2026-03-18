using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.ViewModels
{
    public class ListOfPosViewModel
    {
        public int STT { get; set; }  //Thêm

        public string Code { get; set; }
        
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string AddressFull { get; set; }

        public string FaxNumber { get; set; }

        public string MobileNo { get; set; }

        public string SbvCode { get; set; }

        public string PosFlag { get; set; }

        public string MainPosCode { get; set; }

        public string MainPosName { get; set; }        

        public string Status { get; set; }

        public string EconomicArea { get; set; }         

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        public string AddressDetail { get; set; }

        public string ProvinceCode { get; set; }

        public string ProvinceName { get; set; }

        public string DistrictCode { get; set; }

        public string DistrictName { get; set; }

        public string CommuneCode { get; set; }

        public string CommuneName { get; set; }

        public string EstablishmentDate { get; set; }

        public string ClosedDate { get; set; }       

        public string DecisionDate { get; set; }

        public string DecisionNo { get; set; }      

        public string Email { get; set; }

        public string Website { get; set; }

        public string ManagerName { get; set; }

        public string TitleName { get; set; }

        public string ManagerMobile { get; set; }

        public string Telephone { get; set; }
        
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
    }

    public class ListOfCommuneViewModel
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
        
        public string Status { get; set; }

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

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
