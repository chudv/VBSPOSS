namespace VBSPOSS.Data.Models
{
    public class MenuRole
    {
        public int Id { get; set; }
        public string RoleCode { get; set; }
        public int MenuId { get; set; }        
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
