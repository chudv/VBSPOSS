namespace VBSPOSS.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        //public string RoleCode { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Sex { get; set; }
        public string TitleCode { get; set; }
        public string DepartmentCode { get; set; }
        public string PosCode { get; set; }
        public string DegreeCode { get; set; }
        public string IdCode { get; set; }
        public DateTime IssuedDate { get; set; }
        public string IssuedPlace { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public string StaffId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string DefaultRole { get; set; } 

    }
}
