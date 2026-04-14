using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VBSPOSS.Constants;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;

namespace VBSPOSS.Controllers
{
    [AuthorizeFilter]
    public class UsersController : BaseController
    {

        public UsersController(ILogger<UsersController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper) { }

        /// <summary>
        /// Sự kiện gọi Menu Hệ thống => Quản lý tài khoản người dùng
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Index()
        {
            var _userName = UserName;

            if (string.IsNullOrEmpty(_userName))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Login", "Account");
            }

            var objUserInfo = _administrationService.GetUserByUserName(_userName);

            if (objUserInfo == null || string.IsNullOrEmpty(objUserInfo.UserName))
            {
                TempData["Error"] = $"Không tìm thấy thông tin người dùng [{_userName}]. Vui lòng kiểm tra lại!";
                return RedirectToAction("Login", "Account");
            }

            TempData["UserGrade"] = objUserInfo.Grade;
            TempData["UserRole"] = objUserInfo.DefaultRole;
            TempData["UserName"] = objUserInfo.UserName;
            TempData["PosCode"] = objUserInfo.PosCode;

            return View("Index");
        }



        [Authorize]
        /// <summary>
        /// Hàm tải danh sách người dùng lên lưới dữ liệu chức năng Quản lý tài khoản người dùng
        /// </summary>
        /// <param name="request"></param>
        /// <param name="findPosCode">Mã POS</param>
        /// <param name="findDepartment">Mã phòng ban</param>
        /// <param name="findTitleCode">Mã chức vụ người dùng</param>
        /// <param name="fromBirthDay">Ngày sinh từ ngày</param>
        /// <param name="toBirthDay">Ngày sinh đến ngày</param>
        /// <param name="findFullName">Họ tên</param>
        /// <param name="findSex">Giới tính</param>
        /// <param name="findUserName">Tài khoản</param>
        /// <param name="findRoleCode">Quyền</param>
        /// <returns>Danh sách thông tin tài khoản người dùng</returns>
        public ActionResult LoadUsersGridData([DataSourceRequest] DataSourceRequest request, string findPosCode, string findDepartment, string findTitleCode, 
                                              string fromBirthDay, string toBirthDay, string findFullName, string findSex, string findUserName, string findRoleCode)
        {

            //var sessionUser = SessionManager.GetUser();
            string _findPosCode = "";
            var _userGrade = UserGrade;
            if (_userGrade == PosGrade.HEAD_POS)
            {
                if (string.IsNullOrEmpty(findPosCode) || findPosCode.Trim() == "0" || findPosCode.Trim() == "000000")
                    _findPosCode = "000100";
                else
                    _findPosCode = findPosCode;
            }
            else
            {
                if (string.IsNullOrEmpty(findPosCode.Trim()) || findPosCode.Trim() == "0" || findPosCode.Trim() == "000000")
                    _findPosCode = UserPosCode;
                else
                    _findPosCode = findPosCode;
            }
            DateTime? fromDate = string.IsNullOrEmpty(fromBirthDay) ? null : CustConverter.StringToDate(fromBirthDay, "dd/MM/yyyy");
            DateTime? toDate = string.IsNullOrEmpty(fromBirthDay) ? null : CustConverter.StringToDate(toBirthDay, "dd/MM/yyyy");

            var list = _administrationService.GetUsers(_findPosCode, findDepartment, findTitleCode, fromDate, toDate, findFullName, findSex, findUserName, findRoleCode, "");

            return Json(list.ToDataSourceResult(request));
        }


        /// <summary>
        /// Hiển thị màn hình thêm mới/chỉnh sửa tài khoản người dùng
        /// </summary>
        /// <param name="userName">Tài khoản người dùng cần Thay đổi thông tin. Nếu rỗng là thêm mới</param>
        /// <returns></returns>
        public ActionResult ShowUserUpdate(string userName)
        {
            UserModel objUserUpd = new UserModel();
            if (string.IsNullOrEmpty(userName))
            {
                TempData["IsEdit"] = 0;
                objUserUpd.Order = 0;
                objUserUpd.Id = 0;
                objUserUpd.UserName = "";
                objUserUpd.FullName = "";
                objUserUpd.UserBirthday = DateTime.Now;
                objUserUpd.BirthdayText = "";
                objUserUpd.Sex = "";
                objUserUpd.SexDesc = "";

                objUserUpd.TitleCode = "";
                objUserUpd.TitleDesc = "";
                objUserUpd.DepartmentCode = "";
                objUserUpd.DepartmentDesc = "";
                objUserUpd.PosCode = "";
                objUserUpd.PosName = "";
                objUserUpd.DegreeCode = "";
                objUserUpd.DegreeDesc = "";
                objUserUpd.IdCode = "";
                objUserUpd.IssuedDate = DateTime.Now;
                objUserUpd.IssuedPlace = "";
                objUserUpd.IdExpDate = DateTime.Now.AddYears(10);
                objUserUpd.Mobile = "";
                objUserUpd.Email = "";
                objUserUpd.Status = StatusLov.StatusOpen;
                objUserUpd.StaffId = "";
                objUserUpd.CreatedBy = UserName;
                objUserUpd.CreatedDate = DateTime.Now;
                objUserUpd.ModifiedBy = UserName;
                objUserUpd.ModifiedDate = DateTime.Now;
                objUserUpd.DefaultRole = "";
                objUserUpd.RoleName = "";
                objUserUpd.Grade = 0;
                objUserUpd.Password = "";
            }
            else
            {
                TempData["IsEdit"] = 1;
                objUserUpd = _administrationService.GetUserByUserName(userName);
                if (objUserUpd != null && !string.IsNullOrEmpty(objUserUpd.StaffId))
                {
                    //Lấy thông tin dữ liệu một số trường hiện tại đã điều chỉnh QLNS để vào Object so sanh




                } 
            }
            return PartialView("_Update", objUserUpd);
        }


        /// <summary>
        /// Hàm lưu dữ liệu khi Thêm mới/Thay đổi thông tin tài khoản người dùng trong bảng Users
        /// </summary>
        /// <param name="request"></param>
        /// <param name="data">Dữ liệu cập nhật theo Model UserModel (vUsers)</param>
        /// <returns>Kết quả trả về. Giá trị:
        ///         "1" - Tên đăng nhập người dùng đã tồn tại. Vui lòng kiểm tra lại!"
        ///         "2" - Ngày sinh lớn hơn hoặc bằng ngày hiện tại. Vui lòng kiểm tra lại!";
        ///         "3" - Ngày sinh lớn hơn hoặc bằng ngày ngày cấp CMND/Thẻ căn cước. Vui lòng kiểm tra lại!";
        ///         "4" - Số  căn cước [" + $("#SoCMT").val() + "] của người dùng đã tồn tại. Vui lòng kiểm tra lại!";
        ///         "5" - Id của người dùng được thêm mới != 0, Bạn hãy kiểm tra lại.
        ///         "6" - Có lỗi xảy ra, bạn hãy kiểm tra lại!"
        ///         "7" - Đơn vị của người dùng không được để trống. Vui lòng kiểm tra lại!
        ///         "8" - Phòng ban của người dùng không được để trống. Vui lòng kiểm tra lại!
        ///         "9" - Chức vụ của người dùng không được để trống. Vui lòng kiểm tra lại!
        ///         "10" - Trình độ chuyên môn của người dùng không được để trống. Vui lòng kiểm tra lại!
        ///         "11" - Mật khẩu người dùng phải có ký tự chữ hoa, chữ thường, ký tự số và ký tự đặc biệt. Vui lòng kiểm tra lại!
        ///         "12" - Mật khẩu người dùng tối thiểu phải 6 ký tự (trong đó có ký tự chữ hoa, chữ thường, ký tự số và ký tự đặc biệt). Vui lòng kiểm tra lại!
        ///         "13" - Mật khẩu người dùng phải có ít nhất một ký tự đặc biệt. Vui lòng kiểm tra lại!
        ///         "14" - Mật khẩu người dùng phải có ít nhất một ký tự chữ thường. Vui lòng kiểm tra lại!
        ///         "15" - Mật khẩu người dùng phải có ít nhất một ký tự số. Vui lòng kiểm tra lại!
        ///         "16" - Mật khẩu người dùng phải có ít nhất 6 ký tự khác nhau. Vui lòng kiểm tra lại!
        ///         "17" - Mật khẩu người dùng không chính xác. Vui lòng kiểm tra lại!
        ///         "18" - Người dùng đã có mật khẩu. Vui lòng kiểm tra lại!
        /// </returns>
        [Authorize]
        [AcceptVerbs("Post")]
        public async Task<IActionResult> CreateUser([DataSourceRequest] DataSourceRequest request, UserModel data)
        {
            string result = "0";

            if (data != null && ModelState.IsValid)
            {
                result = await _administrationService.UpdateUser(data, UserName);
            }
            else
            {
                return View(data);
            }
            return Json(result);
        }

        /// <summary>
        /// Xóa tài khoản người dùng .
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        public async Task<string> DeleteUser(string userName)
        {
            string result = await _administrationService.DeleteUser(userName);
            return result;
        }

        /// <summary>
        /// Khởi tạo lại mật khẩu tài khoản người dùng
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<string> ResetPassword(string userName)
        {
            string result = await _administrationService.ResetPassword(userName);
            return result;
        }
    }
}
