using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using VBSPOSS.Constants;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;


namespace VBSPOSS.Controllers
{
    public class AttachedFileController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IAttachedFileService _attachedFile;
        public AttachedFileController(ILogger<BaseController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper, IMapper mapper,
            IAttachedFileService attachedFileService) : base(logger, administrationService, sessionHelper)
        {
            _mapper = mapper;
            _attachedFile = attachedFileService;
        }

        public IActionResult Index()
        {
            var controllerFromRoute = RouteData.Values["controller"]?.ToString();
            var actionFromRoute = RouteData.Values["action"]?.ToString();
            SetPermitData(actionFromRoute, controllerFromRoute);

            RolePermissionModel userPermission = UserPermission;
            string role = UserRole.ToString();

            TempData["Role"] = role;
            TempData.Put("UserPermission", userPermission);
            return View();
        }

        [HttpGet]
        public JsonResult GetFileType()
        {
            var statuses = new List<object>
            {
                new { Value = "-1", Description = "Tất cả", Code = "ALL" },

                new { Value = "1", Description = "File cấu hình lãi suất Tide/Casa/DepositPenal", Code = "INT_RATE" },
                new { Value = "2", Description = "File đính kèm của người dùng iDC", Code = "IDC" },
                new { Value = "3", Description = "File đính kèm thay đổi/thêm mới điểm giao dịch", Code = "POS" },
                new { Value = "4", Description = "File đính kèm thay đổi/thêm mới danh mục địa phương", Code = "LOCATION" },
                new { Value = "5", Description = "File đính kèm bản cập nhật phần mềm Offline (Execution File)", Code = "OFFLINE_EXE" },
                new { Value = "6", Description = "File dữ liệu đầu ngày giao dịch Offline", Code = "OFFLINE_TXN" },
                new { Value = "7", Description = "File tài liệu khác", Code = "OTHER" }
            };

            return Json(statuses);
        }

        public async Task<ActionResult> LoadAttachedFileGridData(
       [DataSourceRequest] DataSourceRequest request,
       string pPosCode,
       string pFileType)
        {
           
            try
            {
                if ((string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || pPosCode == "-1") && pFileType !="6")
                {
                    return Json(new List<AttachedFileInfoView>().ToDataSourceResult(request));
                }

                var list = await _attachedFile.GetttachedFileSync(pPosCode, pFileType);

                return Json(list.ToDataSourceResult(request));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR,
                    $"LoadAttachedFileGridData Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "None"}");

                return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách file đính kèm." });
            }
        }

        public IActionResult DownloadFile(string fileName)
        {
            string folder = @"D:\CBS\TXN";
            string fullPath = Path.Combine(folder, fileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return File(stream, "application/octet-stream", fileName);
        }

        public ActionResult UploadFileInit()
        {
            var model = new AttachedFileInfoView
            {
                FileId = new Random().Next(1, 999999),
                FileName = "FILE_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                ContentDescription = "Random upload file",
                CreatedDate = DateTime.Now
            };
            return PartialView("_UploadFile", model);
        }

        [HttpPost]
        public async Task<string> Upload(IFormFile files, string Mo_Ta)
        {
            string result = await  _attachedFile.UploadFileAsync(files, Mo_Ta, User.Identity?.Name ?? "system");
            return result;
        }


    }
}