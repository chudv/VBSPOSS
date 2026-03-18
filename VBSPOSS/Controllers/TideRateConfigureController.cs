using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text.Json;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class TideRateConfigureController : BaseController
    {
        private readonly IInterestRateConfigureService _interestRateConfigureService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public TideRateConfigureController(IInterestRateConfigureService interestRateConfigureService, IProductService productService, ILogger<TideRateConfigureController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper, IMapper mapper) : base(logger, administrationService, sessionHelper)
        {
            _interestRateConfigureService = interestRateConfigureService;
            _productService = productService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            // Hoặc cách khác qua RouteData
            var controllerFromRoute = RouteData.Values["controller"]?.ToString();
            var actionFromRoute = RouteData.Values["action"]?.ToString();
            SetPermitData(actionFromRoute, controllerFromRoute);

            RolePermissionModel userPermission = UserPermission;

            string role = UserRole.ToString();

            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);

            return View();
        }


        public ActionResult LoadInterestRateGridData([DataSourceRequest] DataSourceRequest request, string pProductGroupCode,
                            string pPosCode, string pCircularRefNum, string pFromEffectiveDate, string pToEffectiveDate, string searchText, string status)
        {
            try
            {
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                int iFromEffectiveDate = 0, ToEffectiveDate = 0;
                if (string.IsNullOrEmpty(pProductGroupCode))
                    pProductGroupCode = ProductGroupCode.TIDE.Code;
                if (string.IsNullOrEmpty(pCircularRefNum))
                    pCircularRefNum = "";
                iFromEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pFromEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));
                ToEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pToEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));

                var listMasterTidePenalRate = _interestRateConfigureService.GetListInterestRateConfigMasterViewsForTide(UserPosCode, pPosCode, pProductGroupCode, pCircularRefNum,
                                                                                                                    iFromEffectiveDate, ToEffectiveDate, 0, searchText, status != null ? int.Parse(status) : -1);

                return Json(listMasterTidePenalRate.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.INFOR, $"LoadInterestRateGridData('{pProductGroupCode}','{pPosCode}','{pCircularRefNum}','{pFromEffectiveDate}','{pToEffectiveDate}') => Error: {ex.Message}");
                return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách quyết định cấu hình lãi suất rút trức hạn." });
            }
        }

        [HttpGet]
        public IActionResult ShowCreateConfig(int pId, string pFlagCall)
        {
            var model = new TideRateConfigureViewModel();
            return PartialView("_Create", model);
        }

        [HttpGet]
        public IActionResult ShowBatchCreateConfig(int pId, string pFlagCall)
        {
            var model = new TideRateConfigureBatchModel();
            //return PartialView("_BatchCreate", model);
            //var model = new TideRateConfigureViewModel();
            TempData["PosGrade"] = UserGrade;
            return PartialView("_BatchCreate", model);
        }


        [HttpGet]
        public async Task<IActionResult> ShowBatchConfigDetail(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
        {
            var model = await _interestRateConfigureService.GetTideInterestRateDetailViews(circularRefNum, effectDate);
            return PartialView("_View", model);
        }

        [HttpGet]
        public async Task<IActionResult> ShowApprovalScreen(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
        {
            var model = await _interestRateConfigureService.GetTideInterestRateDetailViews(circularRefNum, effectDate);
            return PartialView("_Approval", model);
        }


        [HttpGet]
        public async Task<IActionResult> ShowAuthorizeScreen(int pId, string circularRefNum, string effectDate, string idList, string pFlagCall)
        {
            var model = await _interestRateConfigureService.GetTideInterestRateDetailViews(circularRefNum, effectDate);
            return PartialView("_Authorize", model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveApprovalForm([DataSourceRequest] DataSourceRequest request, InterestRateConfigMasterViewModel model, IFormFile fileUpload)
        {
            try
            {
                if (model == null || !ModelState.IsValid)
                {
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // ===== CHECK FILE UPLOAD =====
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    ModelState.AddModelError("fileUpload", "Vui lòng chọn file PDF");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // 1️⃣ Check extension
                var extension = Path.GetExtension(fileUpload.FileName).ToLower();
                if (extension != ".pdf")
                {
                    ModelState.AddModelError("fileUpload", "Chỉ cho phép upload file PDF");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // 2️⃣ Check MIME type
                if (fileUpload.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("fileUpload", "File không đúng định dạng PDF");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // 3️⃣ Check dung lượng (ví dụ: 5MB)
                long maxSize = 5 * 1024 * 1024;
                if (fileUpload.Length > maxSize)
                {
                    ModelState.AddModelError("fileUpload", "Dung lượng file tối đa 5MB");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

                // ===== LƯU FILE =====
                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "ToTrinh");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{Guid.NewGuid()}.pdf";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    fileUpload.CopyTo(stream);
                }

                var saveFileStatus = await _interestRateConfigureService.SaveAttachedFiles(0, new List<AttachedFileInfo>
                {
                    new AttachedFileInfo
                    {
                        FileType = extension.Replace('.',' ').Trim(),
                        FileName = fileUpload.FileName,
                        PathFile = filePath,
                        FileExtension = extension,
                        FileNameNew = fileName,
                        DocumentNumber = model.CircularRefNum,
                        Status = 1,
                        CreatedBy = UserName,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = UserName,
                        ModifiedDate = DateTime.Now,
                    }
                }, UserName);

                var lstId = StringHelper.ConvertToLongList(model.IdList, ';');
                var documentId = saveFileStatus != null ? saveFileStatus.FirstOrDefault() : 0;

                var updateStatus = await _interestRateConfigureService.UpdateInterestRateConfigMasterStatus(UserName, lstId, ConfigStatus.PROCESS.Value, documentId);

                if (saveFileStatus != null && saveFileStatus.Any() && updateStatus > 0)
                {
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }
                else
                {
                    WriteLog(LogType.ERROR, $"SaveApprovalForm - Failed to save approval for CircularRefNum: {model.CircularRefNum}");
                    ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }

            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                ModelState.AddModelError("ERROR", $"{e.Message}");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

        }


        [HttpPost]
        public async Task<IActionResult> SaveAuthorizeForm([DataSourceRequest] DataSourceRequest request, InterestRateConfigMasterViewModel model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    var lstId = StringHelper.ConvertToLongList(model.IdList, ';');
                    var status = await _interestRateConfigureService.SaveApprovalDecision(UserName, lstId, model.RejectFlag, model.RejectReason);

                    if (status > 0)
                    {
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                    else
                    {
                        ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                        return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                }
                else
                {
                    ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                ModelState.AddModelError("ERROR", $"{e.Message}");
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }

        }

        [HttpPost]
        public async Task<IActionResult> LoadTideConfigureAddGridData([DataSourceRequest] DataSourceRequest request)
        {
            var productCode = Request.Form["productCode"].ToString();
            var accountTypeCode = Request.Form["accountType"].ToString(); //             
            var accountSubTypeCode = Request.Form["accountSubType"].ToString(); // 
            var effectiveDate = Request.Form["effectiveDate"].ToString(); //    
            var posCode = Request.Form["posCode"].ToString(); //    

            //Chinh sua neu la head pos thi truyen ma la 0
            if (posCode == PosValue.HEAD_POS)
            {
                posCode = "0";
            }

            if (string.IsNullOrEmpty(productCode))
            {
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }

            try
            {
                var models = await _interestRateConfigureService.GetTideProdList(posCode, productCode, DateTime.Now);
                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoadProductGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var depositType = Request.Form["depositType"].ToString();
                var customerType = Request.Form["customerType"].ToString(); //   
                var models = _productService.GetFullProductList(ProductGroupCode.ProductGroupCode_Tide, depositType, customerType, UserPosCode, UserGrade);

                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoadDepositTermGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var termType = Request.Form["termType"].ToString(); //   
                var inclusionFlag = Request.Form["inclusionFlag"].ToString(); //   
                var termBasis = int.Parse( Request.Form["termBasis"].ToString()); //   
                if (string.IsNullOrEmpty(inclusionFlag))
                {
                    inclusionFlag = "INCLUSIVE";
                }
                if (string.IsNullOrEmpty(termType))
                {
                    termType = "M";
                }

                var models = _productService.GetDepositTerms(termType, termBasis, inclusionFlag);

                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LoadTideConfigureBatchGridData([DataSourceRequest] DataSourceRequest request)
        {
            var accountTypes = Request.Form["accountTypes"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var effectiveDate = Request.Form["effectiveDate"].ToString(); //    
            var posCode = Request.Form["posCode"].ToString(); //
            var sourceFlag = Request.Form["sourceFlag"].ToString(); //                           

            //Chinh sua neu la head pos thi truyen ma la 0
            if (posCode == PosValue.HEAD_POS)
            {
                posCode = "0";
            }

            if (string.IsNullOrEmpty(sourceFlag))
            {
                sourceFlag = "0";
            }

            if (accountTypes == null || accountTypes.Count == 0)
            {
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }

            try
            {
                var sessionId = HttpContext.Session.Id;
                var models = await _interestRateConfigureService.GetTideProdList(sessionId, UserName, posCode, accountTypes, DateTime.Now, sourceFlag);
                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTideRateConfigure()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();

            }

            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Request body is empty.");
            }

            try
            {
                // Manually deserialize the JSON
                var request = JsonSerializer.Deserialize<SaveTideRateConfigureRequest>(body);
                if (request == null)
                {
                    return BadRequest("Failed to deserialize request.");
                }

                if (request.Model == null)
                {
                    return BadRequest("Form model is null.");
                }

                if (request.GridItems == null || !request.GridItems.Any())
                {
                    return BadRequest("No grid items to save.");
                }

                if (string.IsNullOrEmpty(request.Model.PosCode))
                {
                    request.Model.PosCode = UserPosCode;
                }

                // Validate form model
                if (string.IsNullOrEmpty(request.Model.ProductCode) || string.IsNullOrEmpty(request.Model.AccountTypeCode))
                {
                    return BadRequest("Missing required form fields.");
                }

                // Save or update grid items
                var message = await _interestRateConfigureService.SaveTideRateConfigureData(request.Model, request.GridItems, UserName);


                return Ok(new { Message = $"{message}" });
            }
            catch (DbUpdateException ex)
            {

                return StatusCode(500, $"Lỗi khi lưu vào cơ sở dữ liệu: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBatchTideRateConfigure()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();

            }

            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Request body is empty.");
            }

            try
            {
                // Manually deserialize the JSON
                var request = JsonSerializer.Deserialize<SaveTideRateConfigureRequest>(body);
                if (request == null)
                {
                    return BadRequest("Failed to deserialize request.");
                }

                if (request.Model == null)
                {
                    return BadRequest("Form model is null.");
                }

                if (request.GridItems == null || !request.GridItems.Any())
                {
                    return BadRequest("No grid items to save.");
                }

                if (string.IsNullOrEmpty(request.Model.PosCode))
                {
                    request.Model.PosCode = UserPosCode;
                }

                // Validate form model
                //if (string.IsNullOrEmpty(request.Model.ProductCode) || string.IsNullOrEmpty(request.Model.AccountTypeCode))
                //{
                //    return BadRequest("Missing required form fields.");
                //}

                // Save or update grid items
                var message = await _interestRateConfigureService.SaveBatchTideRateConfigureData(request.Model, request.GridItems, UserName, UserPosCode);


                return Ok(new { Message = $"{message}" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTideRateTemp()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();

            }

            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Request body is empty.");
            }

            try
            {
                // Manually deserialize the JSON
                var request = JsonSerializer.Deserialize<SaveTempTideRateConfigureRequest>(body);
                if (request == null)
                {
                    return BadRequest("Failed to deserialize request.");
                }

                if (request.DepositTerms == null || !request.DepositTerms.Any())
                {
                    return BadRequest("No grid items to save.");
                }

                var sessionId = HttpContext.Session.Id;
                double interestRate = 0;

                if (request.DepositType == DepositType.BeforeOfTerm)
                {
                    interestRate = request.BeforeTermInterestRate;
                }
                else if (request.DepositType == DepositType.PartitalTerm)
                {
                    interestRate = request.PartialInterestRate;
                }
                else if (request.DepositType == DepositType.OnTerm)
                {
                    interestRate = request.InterestRate;
                }

                var updateRowCnt = await _interestRateConfigureService.UpdateTideConfigureTemp(request.DepositTerms, interestRate, sessionId, UserName, UserPosCode);

                if (updateRowCnt <= 0)
                {
                    return StatusCode(500, "Lỗi khi cập nhật dữ liệu tạm thời. Bạn hãy kiểm tra lại loại kỳ hạn tương ứng hoặc nhập đúng lãi suất thay đổi.");
                }

                return Ok(new { Message = $"Lưu dữ liệu thành công! Số kỳ hạn được cập nhật {updateRowCnt}." });
            }
            catch (DbUpdateException ex)
            {

                return StatusCode(500, $"Lỗi khi lưu vào cơ sở dữ liệu: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInterestRateConfigByDocumentId([FromBody] DeleteInterestRateConfigRequest request)
        {

            if (request == null)
            {
                return BadRequest(new { success = false, message = "Yêu cầu không hợp lệ. Vui lòng cung cấp Id hợp lệ." });
            }

            try
            {
                if (request.DocumentId != null && request.DocumentId.Value != 0)
                {
                    long documentId = request.DocumentId.Value;
                    await _interestRateConfigureService.DeleteInterestRateConfigureByDocumentId(documentId, UserName);
                }
                else
                {
                    List<long> lstId = request.IdList
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => long.Parse(x))
                        .ToList();
                    await _interestRateConfigureService.DeleteInterestRateConfigureByIdList(lstId, UserName);
                }

                return Ok(new { success = true, message = "Xóa thành công." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message}" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }



        [HttpPost]
        public async Task<IActionResult> LoadTideConfigureViewGridData([DataSourceRequest] DataSourceRequest request)
        {
            var lstId = Request.Form["idList"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();


            if (lstId == null || lstId.Count == 0)
            {
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }

            try
            {
                var sessionId = HttpContext.Session.Id;
                var models = await _interestRateConfigureService.GetTideTermsAsync(lstId);
                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }

        [HttpGet]
        public IActionResult CheckCircular(string circularRefNum, DateTime circularDate)
        {
            try
            {
                bool isValid = !string.IsNullOrWhiteSpace(circularRefNum);
                bool isExists = _interestRateConfigureService.CheckCircular(circularRefNum, circularDate);

                return Json(new
                {
                    isValid,
                    isExists
                });
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"CheckCircular('{circularRefNum}', '{circularDate}') => Error: {ex.Message}");
                return Json(new { isValid = false, isExists = false, error = "Có lỗi xảy ra khi kiểm tra số quyết định." });
            }

        }



        [HttpPost]
        public async Task<IActionResult> LoadAttachFileGridData([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var documentNumber = Request.Form["documentNumber"].ToString(); //                   
                var models = await _interestRateConfigureService.GetAttachedFilesAsync(documentNumber);
                var result = models.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling LoadAttachFileGridData: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AttachedFileInfo>(), Total = 0 });
            }
        }


        public async Task<ActionResult> DownloadFile(int fileId)
        {
            try
            {
                var fileInfo = await _interestRateConfigureService.GetAttachedFileById(fileId);
                // Lấy tên file từ đường dẫn
                string fileName = Path.GetFileName(fileInfo.PathFile);
                // 3. Trả file về trình duyệt
                return PhysicalFile(fileInfo.PathFile, "application/octet-stream", fileName);

            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return BadRequest($"Lỗi khi tạo file: {ex.Message}");
            }
        }

    }
}
