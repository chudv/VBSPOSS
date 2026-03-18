namespace VBSPOSS.Data.Models
{
    public class MenuRoleView
    {
        public string RoleCode { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public string ImageClass { get; set; }
        public string Activeli { get; set; }
        public int Status { get; set; }
        public int ParentId { get; set; }
        public bool IsParent { get; set; }
        public bool HasChild { get; set; }
        public bool IsSelect { get; set; }        
        public int GroupOrder { get; set; }
        public int Permission { get; set; }
        public string RoleName { get; set; }

        public int Grade { get; set; }

        public int ViewPermissionFlag { get; set; }

        public int CreatePermissionFlag { get; set; }
        public int EditPermissionFlag { get; set; }
        public int DeletePermissionFlag { get; set; }
        public int AuthorizationPermissionFlag { get; set; }
        public int ReportPermissionFlag { get; set; }

    }
}
