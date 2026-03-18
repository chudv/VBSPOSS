using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.Models
{
    public class PosRepresentative
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("RepresentativeType")]
        public string RepresentativeType { get; set; }

        [Column("MainPosCode")]
        public string MainPosCode { get; set; }

        [Column("MainPosName")]
        public string MainPosName { get; set; }

        [Column("PosCode")]
        public string PosCode { get; set; }

        [Column("PosName")]
        public string PosName { get; set; }

        [Column("PosMobileNo")]
        public string PosMobileNo { get; set; }

        [Column("PosFaxNo")]
        public string PosFaxNo { get; set; }

        [Column("PosAddressFull")]
        public string PosAddressFull { get; set; }

        [Column("PosAddressLine")]
        public string PosAddressLine { get; set; }

        [Column("PosSubCommune")]
        public string PosSubCommune { get; set; }

        [Column("PosCommune")]
        public string PosCommune { get; set; }

        [Column("PosDistrict")]
        public string PosDistrict { get; set; }

        [Column("PosProvince")]
        public string PosProvince { get; set; }

        [Column("PosSubCommuneId")]
        public string PosSubCommuneId { get; set; }

        [Column("StaffId")]
        public string StaffId { get; set; }

        [Column("StaffCode")]
        public string StaffCode { get; set; }

        [Column("StaffName")]
        public string StaffName { get; set; }

        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("Genders")]
        public string Genders { get; set; }

        [Column("StaffPosCode")]
        public string StaffPosCode { get; set; }

        [Column("StaffPosName")]
        public string StaffPosName { get; set; }

        [Column("StaffDepartmentCode")]
        public string StaffDepartmentCode { get; set; }

        [Column("StaffDepartmentName")]
        public string StaffDepartmentName { get; set; }

        [Column("StaffPositionCode")]
        public string StaffPositionCode { get; set; }

        [Column("StaffPositionName")]
        public string StaffPositionName { get; set; }

        [Column("StaffMobileNo")]
        public string StaffMobileNo { get; set; }

        [Column("StaffEmail")]
        public string StaffEmail { get; set; }

        [Column("EffectDate")]
        public DateTime? EffectDate { get; set; }

        [Column("ExpireDate")]
        public DateTime ExpireDate { get; set; }  

        [Column("DecisionNo")]
        public string DecisionNo { get; set; }

        [Column("DecisionTitle")]
        public string DecisionTitle { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("Notes")]
        public string Notes { get; set; }

        [Column("CreatedBy")]
        public string CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [Column("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }
}