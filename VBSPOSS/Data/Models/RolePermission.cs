namespace VBSPOSS.Data.Models
{
    public class RolePermission
    {
        public int Id { get; set; } 
        public string RoleCode { get; set; }    
        public string PermissionCode { get; set; }  
        public int MenuId { get; set; } 
        public int Status { get; set; } 
    }
}
