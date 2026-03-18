namespace VBSPOSS.Models
{
    public class PermissionModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public bool StatusFlag { get; set; }
        public string StatusDesc { get; set; }
    }
}
