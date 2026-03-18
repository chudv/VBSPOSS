using System;

namespace VBSPOSS.ViewModels
{
    public class PosRepresentativeViewModel
    {
        public long Id { get; set; }  

        public string RepresentativeType { get; set; }
        public string? RepresentativeTypeDesc { get; set; }
        public string RepresentativeTypeText { get; set; }

        public string MainPosCode { get; set; }
        public string MainPosName { get; set; }

        public string PosCode { get; set; }
        public string PosName { get; set; }

        public string PosMobileNo { get; set; }
        public string PosFaxNo { get; set; }

        public string PosAddressFull { get; set; }
        public string PosAddressLine { get; set; }

        public string PosSubCommune { get; set; }
        public string PosSubCommuneText { get; set; }

        public string PosCommune { get; set; }
        public string PosCommuneText { get; set; }

        public string PosDistrict { get; set; }
        public string PosDistrictText { get; set; }

        public string PosProvince { get; set; }
        public string PosProvinceText { get; set; }

        public string PosSubCommuneId { get; set; }

        public string StaffId { get; set; }
        public string StaffCode { get; set; }
        public string StaffName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string DateOfBirthText { get; set; }

        public string Genders { get; set; }
        public string GendersText { get; set; }

        public string StaffPosCode { get; set; }
        public string StaffPosName { get; set; }

        public string StaffDepartmentCode { get; set; }
        public string StaffDepartmentName { get; set; }

        public string StaffPositionCode { get; set; }
        public string StaffPositionName { get; set; }

        public string StaffMobileNo { get; set; }
        public string StaffEmail { get; set; }

        public DateTime? EffectDate { get; set; }
        public string EffectDateText { get; set; }

        public DateTime ExpireDate { get; set; }
        public string ExpireDateText { get; set; }

        public string DecisionNo { get; set; }
        public string DecisionTitle { get; set; }

        public int Status { get; set; }
        public string StatusDesc { get; set; }
        public string StatusText { get; set; }

        public string Notes { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public int OrderNo { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateExpireDateRequest
    {
        public string StaffId { get; set; }
        public string EffectDate { get; set; }
        public string ExpireDate { get; set; }
        public int Status { get; set; } = 1;
    }
}