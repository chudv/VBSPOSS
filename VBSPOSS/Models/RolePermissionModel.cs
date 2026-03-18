namespace VBSPOSS.Models
{
    public class RolePermissionModel
    {
        public string RoleCode { get; set; }

        public int MenuId { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        /// <summary>
        /// Hiển thị
        /// </summary>
        public int PermissionFlag { get; set; }

        /// <summary>
        /// Xem
        /// </summary>
        public int ViewPermissionFlag { get; set; }

        /// <summary>
        /// Tạo/thêm mới
        /// </summary>
        public int CreatePermissionFlag { get; set; }

        /// <summary>
        /// Sửa
        /// </summary>
        public int EditPermissionFlag { get; set; }

        /// <summary>
        /// Xóa
        /// </summary>
        public int DeletePermissionFlag { get; set; }

        /// <summary>
        /// Duyệt
        /// </summary>
        public int AuthorizePermissionFlag { get; set; }

        /// <summary>
        /// Trình duyệt
        /// </summary>
        public int ReportPermissionFlag { get; set; }
    }
}
