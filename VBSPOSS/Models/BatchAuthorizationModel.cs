namespace VBSPOSS.Models
{
    public class BatchAuthorizationModel
    {
        public string RoleCode { get; set; }

        public string RoleName { get; set; }

        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public int Status { get; set; }

        public List<MenuRolesViewModel> MenuRolesList { get; set; }
    }
}
