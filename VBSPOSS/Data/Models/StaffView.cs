namespace VBSPOSS.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class StaffView
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string StaffCode { get; set; }

        [Required]
        [StringLength(128)]
        public string StaffName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [Required]
        [StringLength(1)]
        public string Sex { get; set; }

        [StringLength(8)]
        public string? TitleCode { get; set; }

        [StringLength(8)]
        public string? DepartmentCode { get; set; }

        [Required]
        [StringLength(8)]
        public string PosCode { get; set; }

        public string PosName { get; set; }

        [Required]
        [StringLength(8)]
        public string DegreeCode { get; set; }

        [Required]
        [StringLength(16)]
        public string IdNo { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string IssuedPlace { get; set; }

        [StringLength(128)]
        public string? Mobile { get; set; }

        [StringLength(128)]
        public string? Fax { get; set; }

        [StringLength(128)]
        public string? Email { get; set; }

        [StringLength(512)]
        public string? Address { get; set; }

        [StringLength(512)]
        public string? Note { get; set; }

        public byte? Status { get; set; }

        public byte? NoStaffFlag { get; set; }

        [StringLength(512)]
        public string? NoStaffOrganizationPlace { get; set; }

        [StringLength(512)]
        public string? NoStaffTitle { get; set; }

        public byte[]? Image { get; set; }

        [StringLength(32)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(32)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? TitleDesc { get; set; }  

        public string? DepartmentDesc { get; set; }
    }
}
