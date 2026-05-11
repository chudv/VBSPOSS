using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class TransferDataPosController : BaseController
    {
        private readonly ILogger<UserManagementIDCController> _logger;
        private readonly IListOfValueService _serviceLOV;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IListOfTransPointService _serviceTransPoint;
        private readonly IUserManagementIDCService _userManagementIDCService;

        private readonly ITransferDataPosService _serviceTranferDataPos;
        private readonly IListOfValueService _serviceListOfalues;

        public TransferDataPosController(ILogger<UserManagementIDCController> logger, IAdministrationService adminService, ISessionHelper sessionHelper,
                    IListOfTransPointService serviceTransPoint,
                    ITransferDataPosService serviceTranferDataPos,
                    IListOfValueService serviceListOfalues,
                    IUserManagementIDCService userManagementIDCService, IListOfValueService serviceLOV,
                    IMapper mapper, ApplicationDbContext context) : base(logger, adminService, sessionHelper)
        {
            _logger = logger;
            _serviceLOV = serviceLOV;
            _mapper = mapper;
            _context = context;
            _serviceTransPoint = serviceTransPoint;
            _serviceTranferDataPos = serviceTranferDataPos;
            _userManagementIDCService = userManagementIDCService;
            _serviceListOfalues = serviceListOfalues;
        }

        /// <summary>
        /// Gọi menu Quản lý điểm giao dịch\Đề nghị thêm mới/thay đổi => Đề nghị thêm mới/thay đổi thông tin điểm giao dịch (Thêm/Sửa/Đóng)
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexTransferDataPosMaster()
        {
            string sessionUser = UserName;
            string posCode = UserPosCode;
            // Hoặc cách khác qua RouteData
            var controllerFromRoute = RouteData.Values["controller"]?.ToString();
            var actionFromRoute = RouteData.Values["action"]?.ToString();
            SetPermitData(actionFromRoute, controllerFromRoute);

            RolePermissionModel userPermission = UserPermission;

            string role = UserRole.ToString();

            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);
            TempData["UserName"] = UserName;
            TempData["UserPosCode"] = UserPosCode;

            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();

            ViewBag.EventBusinessCodes = EventBusinessCode.GetListOfTransPoint();

            return View("IndexTransferDataPosMaster");
        }



        /// <summary>
        /// Danh sách bản ghi Tạo mới/Thay đổi thông tin,... người dùng iDC => Tải dừ bảng dữ liệu UserIDCManagement
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pUserId">Mã UserId</param>
        /// <param name="pFunctionType">Loại chức năng chọn</param>
        /// <param name="pFullName">Họ tên người dùng tìm kiếm</param>
        /// <param name="pStatus">Trạng thái</param>
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult LoadGridData_TransPointWorks([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, int pStatus)
        {
            try
            {
                string sTxnPointCode = "", sTxnPointName = "";
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
                if (string.IsNullOrEmpty(pEventCode))
                    pEventCode = "";
                if (string.IsNullOrEmpty(pTxnPointCode))
                    pTxnPointCode = "";
                if (string.IsNullOrEmpty(pTxnPointName))
                    pTxnPointName = "";
                if ((UserGrade == PosGrade.MAIN_POS || UserGrade == PosGrade.HEAD_POS) && (pPosCode != "000100" && pPosCode != "000199" && pPosCode != "000196" && pPosCode != "000197" && pPosCode != "000101"))
                {
                    if (!string.IsNullOrEmpty(pPosCode))
                        pPosCode = pPosCode.Substring(0, 4);
                }
                var listTransPointWorks = _serviceTranferDataPos.GetListOfTranferDataPosSearch("", pPosCode, pTxnPointCode, pTxnPointName, -1, "", pEventCode);
                return Json(listTransPointWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_TransPointWorks('{pPosCode}','{pEventCode}','{pTxnPointCode}','{pTxnPointName}',{pStatus}) => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình Thêm/Sửa/Thay đổi POS, Quyền/Cấp lại mật khẩu... người dùng IDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pUserId">Mã UserId</param>
        /// <param name="pFlagCall"></param>
        /// <param name="pFullName">Họ tên người dùng tìm kiếm</param>
        /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
        ///             1 - Thêm mới
        ///             2 - Chỉnh sửa
        ///             8 - Phê duyệt
        ///             9 - Trình duyệt
        /// <returns>Danh sách người đại diện các đơn vị</returns>
        public ActionResult ShowTransferDataPosMaster(long pId, string pPosCode, string pUserId, string pFlagCall, string pFullName, string pButtonType)
        {
            TransferDataPosMasterViewModel transferDataPosDetailView = new TransferDataPosMasterViewModel();

            transferDataPosDetailView.Id = 0;
            transferDataPosDetailView.EffectiveDate = DateTime.Now;
            transferDataPosDetailView.CreatedBy = "";
            transferDataPosDetailView.CreatedDate = DateTime.Now;
            transferDataPosDetailView.ModifiedBy = "";
            transferDataPosDetailView.ModifiedDate = DateTime.Now;



            ViewBag.FunctionTypes = FunctionTypeFlag.GetOption();
            ViewBag.MailIdFlags = MailIdFlag.GetAll();
            ViewBag.AuthSecTypes = AuthSecType.GetAll();
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            TempData["ButtonType"] = pButtonType;
            //var pos = _serviceListOfalues.GetBranchSearch("4",0, UserPosCode,"","", UserPosCode,"");

            ViewBag.PosList = _serviceTranferDataPos.GetListPosOfBranch(UserPosCode);
            return PartialView("CreateTransferDataPosMaster", transferDataPosDetailView);
        }

        /// <summary>
        /// Hàm thực hiện lưu thông tin người dùng IDC
        /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
        ///             1 - Thêm mới
        ///             2 - Chỉnh sửa
        ///             8 - Phê duyệt
        ///             9 - Trình duyệt 
        /// </summary>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdate([DataSourceRequest] DataSourceRequest request, TransferDataPosMasterViewModel objTranferMaster,
            IFormFile files,
            string pFlagCall,
            string pButtonType)
        {
            try
            {
                string result = "0";
                int sValid = await IsValidSaveTransferDataPosMaster(objTranferMaster);
                if (sValid > 0)
                {
                    return new JsonResult(sValid.ToString());
                }


                long iVal = await _serviceTranferDataPos.SaveTranferPosMaster(objTranferMaster, UserName, pFlagCall, pButtonType);
                result = (iVal > 0) ? "0" : "99";

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
                return new JsonResult("99");
            }
        }

        public async Task<int> IsValidSaveTransferDataPosMaster(TransferDataPosMasterViewModel objTranferMaster)
        {
            int iResult = 0;
            try
            {
                // 1: dữ liệu đã tồn tại
                bool isExist = _serviceTranferDataPos.CheckExistTransferDataPosMaster(objTranferMaster.FromPosCode, objTranferMaster.ToPosCode, objTranferMaster.EffectiveDate);
                if (isExist)
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {

            }
            return iResult;
        }

        public IActionResult TransferDataPosDetail(long pId)
        {
            var master = _context.TransferDataPosMasters
                .FirstOrDefault(x => x.Id == pId);

            if (master == null)
            {
                return PartialView("_TransferDataPosDetail");
            }

            var model = new TransferDataPosMasterViewModel
            {
                Id = master.Id,

                FromPosCode = master.FromPosCode,
                ToPosCode = master.ToPosCode,

                FromPosName = _serviceTranferDataPos.GetPosName(master.FromPosCode),
                ToPosName = _serviceTranferDataPos.GetPosName(master.ToPosCode)
            };

            // THÔN NGUỒN
            ViewData["FromVillages"] = _serviceTranferDataPos
             .GetListSubCommuneOfPos(master.FromPosCode)
             .Select(x => new SelectListItem
             {
                 Value = x.Code,
                 Text = x.Description
             })
             .ToList();

            // THÔN ĐÍCH

            ViewData["ToVillages"] = _serviceTranferDataPos
            .GetListSubCommuneOfPos(master.ToPosCode)
            .Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Description
            })
            .ToList();


            return PartialView("_TransferDataPosDetail", model);
        }

        public IActionResult ReadTransferVillage(
    [DataSourceRequest] DataSourceRequest request,
    long masterId)
        {
            var data = _context.TransferDataPosDetails
                .Where(x => x.MasterId == masterId)
                .Select(x => new TransferDataPosDetailViewModel
                {
                    Id = x.Id,

                    MasterId = x.MasterId,

                    FromVillageId = x.FromVillageId,
                    ToVillageId = x.ToVillageId
                })
                .ToList();

            return Json(data.ToDataSourceResult(request));
        }


        [HttpPost]
        public IActionResult SaveTransferVillage(
    [FromBody] SaveTransferVillageRequest request)
        {
            try
            {
                var master = _context.TransferDataPosMasters
                    .FirstOrDefault(x => x.Id == request.MasterId);

                if (master == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy master"
                    });
                }

                // xóa detail cũ
                var oldDetails = _context.TransferDataPosDetails
                    .Where(x => x.MasterId == request.MasterId)
                    .ToList();

                _context.TransferDataPosDetails.RemoveRange(oldDetails);

                // insert mới
                foreach (var item in request.Details)
                {
                    var entity = new TransferDataPosDetail
                    {
                        MasterId = request.MasterId,

                        FromVillageId = item.FromVillageId,
                        ToVillageId = item.ToVillageId,

                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now
                    };

                    _context.TransferDataPosDetails.Add(entity);
                }

                // update master
                master.TotalVillage = request.Details.Count;

                master.ModifiedBy = User.Identity.Name;
                master.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Lưu dữ liệu thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }


}
