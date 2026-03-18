using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VBSPOSS.Models;

namespace VBSPOSS.Helpers.Interfaces
{
    public interface ISessionHelper
    {

        
            //	Lấy tên đăng nhập của người dùng
            string GetUserName();

            //  Lấy mã điểm giao dịch(nơi làm việc) của người dùng
            string GetUserPosCode();

            //Lấy họ tên đầy đủ của người dùng
            string GetUserFullName();
            //Lấy vai trò (role) hiện tại của người dùng
            string GetUserRole();
            //Lấy cấp bậc (grade) hoặc chức vụ
            int GetUserGrade();
            //	Lấy số điện thoại người dùng
            string GetUserMobile();
            //Kiểm tra người dùng đã đăng nhập chưa (authenticated)

            bool IsUserAuthenticated();

            RolePermissionModel GetRolePermission(int menuId);

            RolePermissionModel GetRolePermission(string controller, string action);
        

    }
}
