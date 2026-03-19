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

        [Authorize]
        //public IActionResult Index()
        //{
        //    var _userName = UserName;
        //    var _user = _administrationService.GetUserByUserName(_userName);

        //    TempData["UserGrade"] = _user.Grade;
        //    TempData["UserRole"] = _user.DefaultRole;
        //    TempData["UserName"] = _user.UserName;
        //    TempData["PosCode"] = _user.PosCode;

        //    return View();
        //}

        public IActionResult Index()
        {
            var _userName = UserName;

            if (string.IsNullOrEmpty(_userName))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Login", "Account");
            }

            var _user = _administrationService.GetUserByUserName(_userName);

            if (_user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Account");
            }

            TempData["UserGrade"] = _user.Grade;
            TempData["UserRole"] = _user.DefaultRole;
            TempData["UserName"] = _user.UserName;
            TempData["PosCode"] = _user.PosCode;

            return View();
        }



        [Authorize]
        /// <summary>
        /// Load dữ liệu lên lưới người dùng
        /// </summary>
        /// <param name="request"></param>
        /// <param name="findPosCode"></param>
        /// <param name="findDepartment"></param>
        /// <param name="findTitleCode"></param>
        /// <param name="fromBirthDay"></param>
        /// <param name="toBirthDay"></param>
        /// <param name="findFullName"></param>
        /// <param name="findSex"></param>
        /// <param name="findUserName"></param>
        /// <param name="findRoleCode"></param>
        /// <returns></returns>
        public ActionResult LoadUsersGridData([DataSourceRequest] DataSourceRequest request, string findPosCode, string findDepartment, string findTitleCode, string fromBirthDay, string toBirthDay, string findFullName, string findSex, string findUserName, string findRoleCode)
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
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ShowUserUpdate(string userName)
        {
            UserModel _user = null;
            if (string.IsNullOrEmpty(userName))
            {
                TempData["IsEdit"] = 0;
                _user = new UserModel();
            }
            else
            {
                TempData["IsEdit"] = 1;
                _user = _administrationService.GetUserByUserName(userName);
            }

            return PartialView("_Update", _user);
        }



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
