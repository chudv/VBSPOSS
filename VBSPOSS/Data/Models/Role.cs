namespace VBSPOSS.Data.Models
{
    public class Role
    {
        public int Id { get;set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public int Grade { get; set; }  
        public int Status { get; set; }

        public int GrantType { get; set; }  
        public string? CreatedBy { get; set; }  
        public DateTime? CreatedDate { get; set; }  
        public string? ModifiedBy { get; set; } 
        public DateTime? ModifiedDate { get; set; }   
    }
}
