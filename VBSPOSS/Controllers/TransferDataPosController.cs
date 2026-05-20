using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Newtonsoft.Json;
using System.Collections;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
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
        /// Danh sách bản ghi Tạo mới/Thay đổi thông tin,... yêu cầu điều chuyển dữ liệu khác pos => Tải dừ bảng dữ liệu TransferDataPosMaster
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <returns>Danh sách yêu cầu điều chuyển dữ liệu khác pos</returns>
        public ActionResult LoadGridData_TransPointWorks([DataSourceRequest] DataSourceRequest request, string pPosCode, int pStatus)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pPosCode))
                {
                    pPosCode = UserPosCode;
                }

                var listTransPointWorks = _serviceTranferDataPos.GetListOfTranferDataPosSearch(UserPosCode, pStatus.ToString(), UserGrade.ToString());
                return Json(listTransPointWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_TranferDataPos('{pPosCode}',{pStatus}) => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm show màn hình Thêm/Sửa/Thay yêu cầu điều chuyển pos
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
        public ActionResult ShowTransferDataPosMaster(long pId, string pFlagCall, string pButtonType)
        {
            var model = new TransferDataPosSaveModel();

            try
            {
                // CREATE NEW
                if (pId <= 0)
                {
                    model.Master = new TransferDataPosMasterViewModel
                    {
                        Id = 0,
                        EffectiveDate = DateTime.Now,
                        CreatedBy = "",
                        CreatedDate = DateTime.Now,
                        ModifiedBy = "",
                        ModifiedDate = DateTime.Now
                    };
                    model.Details = new List<TransferDataPosDetailViewModel>();
                }
                // EDIT
                else
                {
                    var entity = _serviceTranferDataPos.GetTransferDataPosMasterById(pId);
                    model.Master = new TransferDataPosMasterViewModel
                    {
                        Id = entity.Id,
                        FromPosCode = entity.FromPosCode,
                        ToPosCode = entity.ToPosCode,
                        EffectiveDate = entity.EffectiveDate,
                        Remark = entity.Remark,
                        Status = entity.Status,
                        MainPos = entity.MainPos,
                        CreatedBy = entity.CreatedBy,
                        CreatedDate = entity.CreatedDate,
                        ModifiedBy = entity.ModifiedBy,
                        ModifiedDate = entity.ModifiedDate
                    };
                    model.Details = _serviceTranferDataPos.GetTransferDataPosDetailByMasterId(pId);
                }

                TempData["FlagCall"] = pFlagCall;
                TempData["UserPosCode"] = UserPosCode;
                TempData["ButtonType"] = pButtonType;

                ViewBag.PosList = _serviceTranferDataPos.GetListPosOfBranch(UserPosCode);
                ViewBag.FromPosList = _serviceTranferDataPos.GetListPosOfBranch(UserPosCode);
                ViewBag.ToPosList = _serviceTranferDataPos.GetListPosOfBranch("000000");

                return PartialView("_TransferDataPosSave", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShowTransferDataPosMaster");
                return Content("Có lỗi xảy ra");
            }
        }

        public ActionResult ShowTransferDataPosMasterDetail(long pId, string pFlagCall, string pButtonType)
        {
            var model = new TransferDataPosSaveModel();

            try
            {

                var entity = _serviceTranferDataPos.GetTransferDataPosMasterById(pId);
                model.Master = new TransferDataPosMasterViewModel
                {
                    Id = entity.Id,
                    FromPosCode = entity.FromPosCode,
                    ToPosCode = entity.ToPosCode,
                    EffectiveDate = entity.EffectiveDate,
                    Remark = entity.Remark,
                    Status = entity.Status,
                    MainPos = entity.MainPos,
                    CreatedBy = entity.CreatedBy,
                    CreatedDate = entity.CreatedDate,
                    ModifiedBy = entity.ModifiedBy,
                    ModifiedDate = entity.ModifiedDate
                };
                model.Details = _serviceTranferDataPos.GetTransferDataPosDetailByMasterId(pId);

                TempData["FlagCall"] = pFlagCall;
                TempData["UserPosCode"] = UserPosCode;
                TempData["ButtonType"] = pButtonType;

                ViewBag.PosList = _serviceTranferDataPos.GetListPosOfBranch(UserPosCode);
                ViewBag.FromPosList = _serviceTranferDataPos.GetListPosOfBranch(UserPosCode);
                ViewBag.ToPosList = _serviceTranferDataPos.GetListPosOfBranch("000000");

                return PartialView("_TransferDataPosDetail", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShowTransferDataPosMasterDetail");
                return Content("Có lỗi xảy ra");
            }
        }


        /// <summary>
        /// Hàm thực hiện lưu yêu cầu điều chuyển dữ liệu khác pos
        /// <param name="pButtonType">Cờ phân biệt thêm mới/Chỉnh sửa/Phê duyệt</param>
        ///             1 - Thêm mới
        ///             2 - Chỉnh sửa
        ///             8 - Phê duyệt
        ///             9 - Trình duyệt
        /// </summary>
        [AcceptVerbs("Post")]
        public async Task<IActionResult> SaveUpdate([DataSourceRequest] DataSourceRequest request, TransferDataPosMasterViewModel objTranferMaster,
            IFormFile files, string pFlagCall, string pButtonType)
        {
            try
            {
                string result = "0";
                int sValid = await IsValidSaveTransferDataPosMaster(objTranferMaster);
                if (sValid > 0)
                {
                    return new JsonResult(sValid.ToString());
                }

                long iVal = await _serviceTranferDataPos.SaveTranferPosMaster(objTranferMaster, UserName, pFlagCall, pButtonType, UserPosCode);
                if (iVal > 0)
                {
                    if (files != null)
                    {
                        await _serviceTranferDataPos.SaveAttachedFile(iVal, files, "", UserName);
                    }
                }
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

        public IActionResult TransferDataPosDetail(long pId, string pTypeAction)
        {
            var master = _context.TransferDataPosMasters.FirstOrDefault(x => x.Id == pId);
            if (master == null) return PartialView("_TransferDataPosDetail");

            var model = new TransferDataPosMasterViewModel
            {
                Id = master.Id,
                FromPosCode = master.FromPosCode,
                ToPosCode = master.ToPosCode,
                FromPosName = _serviceTranferDataPos.GetPosName(master.FromPosCode),
                ToPosName = _serviceTranferDataPos.GetPosName(master.ToPosCode)
            };

            // THÔN NGUỒN
            ViewData["FromVillages"] = _serviceTranferDataPos.GetListSubCommuneOfPos(master.FromPosCode)
                .Select(x => new SelectListItem { Value = x.Code, Text = x.Description })
                .ToList();

            // THÔN ĐÍCH
            ViewData["ToVillages"] = _serviceTranferDataPos.GetListSubCommuneOfPos(master.ToPosCode)
                .Select(x => new SelectListItem { Value = x.Code, Text = x.Description })
                .ToList();

            TempData["UserPosCode"] = UserPosCode;

            return pTypeAction == "2"
                ? PartialView("_ApproveTransferDataPos", model)
                : PartialView("_TransferDataPosDetail", model);
        }


        //public IActionResult ReadTransferVillage([DataSourceRequest] DataSourceRequest request, long masterId)
        //{
        //    var data = _context.TransferDataPosDetails
        //        .Where(x => x.MasterId == masterId)
        //        .Select(x => new TransferDataPosDetailViewModel
        //        {
        //            Id = x.Id,
        //            MasterId = x.MasterId,
        //            FromVillageId = x.FromVillageId,
        //            ToVillageId = x.ToVillageId
        //        })
        //        .ToList();
        //    return Json(data.ToDataSourceResult(request));
        //}
        public IActionResult ReadTransferVillage([DataSourceRequest] DataSourceRequest request, long masterId)
        {
            var data = _context.TransferDataPosDetails
                .Where(x => x.MasterId == masterId)
                .Select(x => new TransferDataPosDetailViewModel
                {
                    Id = x.Id,
                    MasterId = x.MasterId,
                    FromVillageId = x.FromVillageId,
                    FromVillageName = _serviceTranferDataPos.GetVillageNameByCode(x.FromVillageId),
                    ToVillageId = x.ToVillageId,
                    ToVillageName = _serviceTranferDataPos.GetVillageNameByCode(x.ToVillageId)
                })
                .ToList();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        public IActionResult SaveTransferVillage([FromBody] SaveTransferVillageRequest request)
        {
            try
            {
                var master = _context.TransferDataPosMasters.FirstOrDefault(x => x.Id == request.MasterId);
                if (master == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy master" });
                }

                // Xóa detail cũ
                var oldDetails = _context.TransferDataPosDetails.Where(x => x.MasterId == request.MasterId).ToList();
                _context.TransferDataPosDetails.RemoveRange(oldDetails);

                // Insert mới
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

                // Update master
                master.TotalVillage = request.Details.Count;
                master.ModifiedBy = User.Identity.Name;
                master.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, message = "Lưu dữ liệu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> DownloadTransferFile(long documentId)
        {
            try
            {
                var fileInfo = await _serviceTranferDataPos.DownloadTransferAttachFile(documentId);
                if (fileInfo == null)
                {
                    return NotFound("Không tìm thấy file");
                }
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileInfo.PathFile, fileInfo.FileNameNew);
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("File không tồn tại");
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, "application/octet-stream", fileInfo.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadTransferFile Error");
                return BadRequest("Có lỗi xảy ra");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTransferDataPos(long pId)
        {
            try
            {
                bool isDeleted = await _serviceTranferDataPos.DeleteTransferDataPos(pId, UserName);
                return new JsonResult(isDeleted ? "1" : "0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteTransferDataPos Error");
                return new JsonResult("0");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTransferDataPos(long pId, string pRemark, string pAction)
        {
            try
            {
                int result = await _serviceTranferDataPos.ApproveTransferDataPos(pId, pRemark, pAction, UserName);
                return Json(result.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveTransferDataPos");
                return Json("0");
            }
        }


        public JsonResult GetVillageByPos(string pPosCode)
        {
            var data = _serviceTranferDataPos.GetListSubCommuneOfPos(pPosCode)
                .Select(x => new
                {
                    Value = x.Code,
                    Text = x.Description
                })
                .ToList();

            return Json(data);
        }


        [HttpPost]
        public async Task<IActionResult> SaveTransferDataPos(TransferDataPosSaveModel model, IFormFile files, string DetailsJson)
        {
            try
            {
                // VALIDATE
                if (model == null || model.Master == null) return Json(0);

                // PARSE DETAIL
                if (!string.IsNullOrEmpty(DetailsJson))
                {
                    model.Details = JsonConvert.DeserializeObject<List<TransferDataPosDetailViewModel>>(DetailsJson);
                }

                // SAVE MASTER
                var validateCode = ValidateTransferDataPosDetail(model);
                if (validateCode != 1) return Json(validateCode);

                long masterId = await _serviceTranferDataPos.SaveTransferDataPosMaster(model.Master, User.Identity.Name, model.Details.Count, UserPosCode);
                if (masterId <= 0) return Json(0);

                // SAVE DETAIL
                if (model.Details != null && model.Details.Count > 0)
                {
                    await _serviceTranferDataPos.SaveTransferDataPosDetail(masterId, model.Details, User.Identity.Name);
                }

                // SAVE FILE
                if (files != null && files.Length > 0)
                {
                    await _serviceTranferDataPos.SaveAttachedFile(masterId, files, "", UserName);
                }

                return Json(1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveTransferDataPos");
                return Json(0);
            }
        }


        private int ValidateTransferDataPosDetail(TransferDataPosSaveModel model)
        {
            // CHECK MASTER
            if (model == null || model.Master == null) return 0;

            // CHECK POS
            if (model.Master.FromPosCode == model.Master.ToPosCode) return 2; // POS nguồn và đích trùng

            // CHECK DETAIL EMPTY
            if (model.Details == null || model.Details.Count <= 0) return 3; // Chưa có detail

            // CHECK EMPTY VILLAGE
            if (model.Details.Any(x => string.IsNullOrEmpty(x.FromVillageId) || string.IsNullOrEmpty(x.ToVillageId))) return 4; // Chưa chọn thôn

            // CHECK DUP SOURCE
            var fromVillageList = model.Details.Select(x => x.FromVillageId).ToList();
            if (fromVillageList.Count != fromVillageList.Distinct().Count()) return 5; // Trùng thôn nguồn

            // CHECK DUP TARGET
            var toVillageList = model.Details.Select(x => x.ToVillageId).ToList();
            if (toVillageList.Count != toVillageList.Distinct().Count()) return 6; // Trùng thôn đích

            return 1;
        }


        public ActionResult UploadFileInit(string pId)
        {
            var model = new AttachedFileInfoView
            {
                FileId = new Random().Next(1, 999999),
                FileName = "FILE_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                ContentDescription = "Random upload file",
                CreatedDate = DateTime.Now
            };
            //ViewBag.UserPosCode = valueFileType;
            //ViewBag.nameFileType = nameFileType;
            ViewBag.IdTranferDataPos = pId;
            return PartialView("_UploadFile", model);
        }

        public async Task<string> UploadTotrinh(IFormFile files, string Mo_Ta, string idTranfer, string DocumentNumber)
        {
            // string result = await _attachedFile.UploadFileAsync(files, Mo_Ta, UserName, valueFileType, DocumentNumber);
            var kk = await _serviceTranferDataPos.SaveAttachedFile(
                Convert.ToInt64(idTranfer),
                files,
                DocumentNumber,
                UserName);

            return kk.ToString();
        }


        //-------------------------------------------------------------------------------------------------------------------
        public IActionResult IndexTransferDataPosSoure()
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

            return View("IndexTransferDataPosSoure");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransferData(long pId)
        {
            try
            {
                // VALIDATE
                if (pId <= 0) return Json(0);

                // GET DATA
                var data = await _serviceTranferDataPos.GetTransferDataForOracleAsync(pId);
                if (data == null || data.Count <= 0) return Json(2);

                // INSERT ORACLE
                var result = await _serviceTranferDataPos.BulkInsertCommuneTransferAsync(data);
                if (result <= 0) return Json(3);

                // SUCCESS
                return Json(1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateTransferData");
                return Json(0);
            }
        }

        public ActionResult ShowTransferDataLoanDetail(long pId, string pFromPos)
        {
            ViewBag.PId = pId;
            ViewBag.PFromPos = pFromPos;

            return PartialView("_ShowTransferDataLoanDetail");
        }

        //--------------------------------------------------------------------Target
        public IActionResult IndexTransferDataPosTarget()
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

            return View("IndexTransferDataPosTarget");
        }

        public ActionResult LoadGridData_TransPointTarget([DataSourceRequest] DataSourceRequest request, string pPosCode, string pEventCode, string pTxnPointCode, string pTxnPointName, int pStatus, string pTotrinh)
        {
            try
            {
                string sTxnPointCode = "", sTxnPointName = "";
                if (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "000199" || pPosCode == "000196")
                {
                    pPosCode = (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196") ? "" : UserPosCode;
                }
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
                var listTransPointWorks = _serviceTranferDataPos.GetListChangePosDataChecking(pPosCode, pTotrinh);
                return Json(listTransPointWorks.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"LoadGridData_TransPointTarget('{pPosCode}','{pEventCode}','{pTxnPointCode}','{pTxnPointName}',{pStatus}) => Error: {ex.Message}");
                ModelState.AddModelError("ERROR", $"{ex.Message}");
                return Json(new DataSourceResult { Data = new List<UserManagementIDCViewModel>(), Total = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAcceptMove([FromBody] List<ChangePosDataCheckingViewModel> data)
        {
            try
            {
                var result = _serviceTranferDataPos.SaveAcceptMove(data);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveAcceptMove");
                return Json(0);
            }
        }


        [HttpGet]
        public JsonResult GetToTrinh(string pUserPosCode)
        {
            try
            {
                ArrayList data = _serviceTranferDataPos.GetToTrinh(UserPosCode);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetToTrinh");
                return Json(new ArrayList());
            }
        }


        public ActionResult LoadGridData_TransPointTarget1(
    [DataSourceRequest] DataSourceRequest request,
    long pId,
    string pFromPos)
        {
            var data =
                _serviceTranferDataPos
                    .GetListChangePosDataChecking(
                        pFromPos,
                        pId.ToString());

            return Json(
                data.ToDataSourceResult(request)
            );
        }

    }
}