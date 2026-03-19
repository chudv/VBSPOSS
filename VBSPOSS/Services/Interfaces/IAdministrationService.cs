using VBSPOSS.Data.Models;
using VBSPOSS.Models;

namespace VBSPOSS.Services.Interfaces
{
    public interface IAdministrationService
    {
        List<MenuRoleView> GetMenuByRole(string groupCode);
        //int GetPermitOrUserGroup(string groupCode, string actionName, string controllerName, int menuId);
        List<MenuRoleView> GetMenuRoleViews();
        MenuRoleView GetMenuRoleById(int roleId);
        MenuRoleView GetMenuRoleById(int menuId, string groupUserCode);
        List<Role> GetRoles(int gantType, int grade);

        // CreateProject
        //List<CreateProject> GetCreateProjects(int gantType, int grade);
        //CreateProjectModel GetCreateProjectById(int id);

        Role GetGroupOfUser(string userName);
        List<Menu> GetMenus();
        List<MenuRoleView> GetMenusForAddNew(string userGroup);
        int UpdateMenuRole(MenuRole menuRole);
        int CreateMenuRole(MenuRole menuRole);
        /// <summary>
        /// Hàm thực hiện cập nhật dữ liệu phân quyền cho nhóm người dùng. Bảng dữ liệu MenuRoles
        /// </summary>
        /// <param name="model">Model MenuRoles</param>
        /// <param name="pUserName">Người dùng thực hiện</param>
        /// <returns>Id bản ghi bảng cập nhật MenuRoles</returns>
        int UpdateMenuRole(MenuRole model, string pUserName);
        int DeleteMenuRole(MenuRole menuRole);
        int DeleteMenuRole(int pMenuId, string pGroupUser);
        int CreateMenu(Menu entity);
        int UpdateMenu(Menu entity);
        int DeleteMenu(Menu entity);
        int CreateUserGroup(Role entity);
        int UpdateUserGroup(Role entity);
        int DeleteUserGroup(Role entity);

        //int UpdateUserGroup(CreateProject entity);
        //int DeleteUserGroup (CreateProject entity);
        //int CreateUserGroup(CreateProject entity);

        List<PermissionModel> GetPermissions();
        int CreatePermission(Permission entity);
        int UpdatePermission(Permission entity);
        int DeletePermission(Permission entity);

        // GetUserByUserName
        UserModel GetUserByUserName(string userName);

        public Task<string> UpdateUser(UserModel data, string modifiedBy);

        public Task<string> DeleteUser(string userName);
        public Task<string> ResetPassword(string userName);

        //List<UserInforView> GetUserInfor(string posCode, string department, string position, DateTime birthDayFromDate, DateTime birthDayToDate, string fullName, string sex, string userName, string userGroup);

        //UserInforViewModel GetUserById(string userId);

        //UserInforViewModel GetUserByUserName(string userName);
        public List<StaffView> GetStaffs(string posCode);

        /// <summary>
        /// Lấy thông tin cán bộ theo mã Id
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public StaffView GetStaffById(int staffId);

        /// <summary>
        /// Lấy thông tin cán bộ theo Code
        /// </summary>
        /// <param name="staffCode"></param>
        /// <returns></returns>
        public StaffView GetStaffByCode(string staffCode);

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        //List<StaffView> GetStaffList(string posCode);

        //StaffView GetStaffById(string staffId);

        //Task<int> CreateUser(UserInforViewModel userInfor, string createdBy);

        //List<UserType> GetUserTypeList();

        //List<UserRole> GetUserRoleList();

        //bool CheckRoleForUser(string userId, string roleCode);

        /// <summary>
        /// Hàm lấy danh sách Bản ghi Menu, Nhóm người dùng, Các quyền được phân hay chưa được phân quền
        /// </summary>
        /// <param name="pGroupCode">Mã Nhóm người dùng</param>
        /// <param name="pGroupName">Tên Nhóm người dùng</param>
        /// <param name="pMenuId">Chỉ số xác định bản ghi Menu</param>
        /// <param name="pMenuText">Tên menu</param>
        /// <param name="pPermitId">Quyền được truy cập. Nếu chọn tất là -1</param>
        /// <param name="pFlagCall">Cờ xác định cách lấy dữ liệu. Giá trị mặc định là 1</param>
        /// <returns>Danh sách Bản ghi Menu, Nhóm người dùng, Các quyền được phân hay chưa được phân quền</returns>
        List<MenuRolesViewModel> GetMenuRoles(string groupCode, string groupName, int menuId, string menuText, int flagCall);

        /// <summary>
        /// Cập nhật theo lô phân quyền
        /// </summary>
        /// <param name="batchUpdateData"></param>
        /// <returns></returns>
        int BatchUpdateRole(BatchAuthorizationModel batchUpdateData);
       
        /// <summary>
        /// Lấy danh sách thông tin người dùng từ vUsers trả ra Model UserModel
        /// </summary>
        /// <param name="posCode">Mã POS</param>
        /// <param name="departmentCode">Mã phòng ban</param>
        /// <param name="titleCode">Mã chức vụ người dùng</param>
        /// <param name="fromBirthDay">Ngày sinh từ ngày</param>
        /// <param name="toBirthDay">Ngày sinh đến ngày</param>
        /// <param name="fullName">Họ và tên</param>
        /// <param name="sex">Giới tính</param>
        /// <param name="userName">Tài khoản người dùng</param>
        /// <param name="roleCode">Mã nhóm người dùng</param>
        /// <param name="staffId">Chỉ số IdCanBo (Không bắt buộc). Ex: 'CNTT00000000087'</param>
        /// <returns>Danh sách thông tin người dùng từ vUsers trả ra Model UserModel</returns>
        List<UserModel> GetUsers(string posCode, string departmentCode, string titleCode, DateTime? fromBirthDay, DateTime? toBirthDay,
                            string fullName, string sex, string userName, string roleCode, string staffId);

        /// <summary>
        /// Hàm lấy thông tin phân quyền của nhóm người dùng theo mã MenuId
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        RolePermissionModel GetRolePermissionByMenuId(string roleCode, int menuId);

        RolePermissionModel GetRolePermissionByMenuId(string roleCode, string controller, string action);

    }
}
