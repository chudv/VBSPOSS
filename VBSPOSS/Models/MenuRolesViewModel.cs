using Telerik.SvgIcons;

namespace VBSPOSS.Models
{
    public class MenuRolesViewModel
    {
        public int Order { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public string ImageClass { get; set; }
        public string ActiveLi { get; set; }
        public int Status { get; set; }
        public string StatusDesc { get; set; }
        public int ParentId { get; set; }
        public bool IsParent { get; set; }
        public bool? HasChild { get; set; }
        public bool? IsSelect { get; set; }
        public int? GroupOrder { get; set; }
        public int Permission { get; set; }        
        //V Xem
        //C Tạo
        //U Sửa
        //D Xóa
        //A Duyệt
        //G Tạo tờ trình
        public string ViewPermissionDesc { get; set; }
        public int ViewPermissionFlag { get; set; }

        public string CreatePermissionDesc { get; set; }
        public int CreatePermissionFlag { get; set; }

        public string EditPermissionDesc { get; set; }
        public int EditPermissionFlag { get; set; }

        public string DeletePermissionDesc { get; set; }
        public int DeletePermissionFlag { get; set; }

        public string AuthorizationPermissionDesc { get; set; }
        public int AuthorizationPermissionFlag { get; set; }

        /// <summary>
        /// Tạo tờ tình
        /// </summary>
        public string ReportPermissionDesc { get; set; }
        public int ReportPermissionFlag { get; set; }

        public string NoAccessPermissionDesc { get; set; }
        public int NoAccessPermissionFlag { get; set; }

        public string CurrentPermissionDesc { get; set; }

        public int Grade { get; set; }

    }
}
