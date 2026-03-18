using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Policy;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VBSPOSS.Controllers
{
    public class NotiController : BaseController
    {
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private readonly ILogger<NotiController> _logger;

        /// <summary>
        /// Defines the _service.
        /// </summary>
        private readonly IListOfValueService _serviceLOV;
        //
        private readonly INotiService _service;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IApiNotiGatewayService _notiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListController"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="service">The service<see cref="IListOfValueService"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger{ListController}"/>.</param>
        /// <param name="menuService">The menuService<see cref="IPermitService"/>.</param>
        public NotiController(ILogger<BaseController> logger,IWebHostEnvironment hostingEnvironment, INotiService service, IApiNotiGatewayService notiService, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper) : base(logger, adminService, sessionHelper)
        {
            _serviceLOV = serviceLOV;
            _service = service;
            _hostingEnvironment = hostingEnvironment;
            _notiService = notiService;
        }

        public IActionResult IndexNotiTemplate()
        {
            return View();
        }

        /// <summary>
        /// Menu gọi hiển thị danh sách Danh mục chung
        /// </summary>
        /// <param name="pMenuId">Chỉ số xác định Menu</param>
        /// <returns>View danh sách danh mục chung</returns>
        public IActionResult UpdateNotiTemplate(int? pMenuId = null)
        {
            // Kiểm tra quyền truy cập (giữ nguyên logic cũ nếu cần)
            /*
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            SetPermitData(actionName, controllerName, pMenuId);

            int permit = UserPermit;
            if (permit == Permit._VIEW)
            {
                return View("IndexListMain_View");
            }
            else if (permit == Permit._EDIT)
            {
                return View("IndexListOfProducts");
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            */

            // Giả lập dữ liệu hoặc gọi service để lấy danh sách sản phẩm
            ViewBag.Message = "Danh sách sản phẩm";
            return View("UpdateNotiTemplate");
        }

        /// <summary>
        /// Hàm thực hiện Lưu thông tin cập nhật Bài học
        /// </summary>
        /// <param name="request"></param>
        /// <param name="objNotiTemp">Thông tin Bài học</param>
        /// <param name="imageFile">File ảnh nếu có</param>
        /// <returns>Giá trị trả về</returns>
        [AcceptVerbs("Post")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUpdateNotiTemp([DataSourceRequest] DataSourceRequest request, NotiTempViewModel objNotiTemp, IFormFile imageFile)
        {
            try
            {
                int iRetId = 0;
                string result = "0";
                //result = IsValidArticlesCourse(objNotiTemp).ToString();
                if (result == "0" && objNotiTemp != null && ModelState.IsValid)
                { 
                    string fileDest = "", sfileNameNew = "", sIcon = "";
                    //objNotiTemp.Description = WebUtility.HtmlDecode(objNotiTemp.Description);
                    //if (imageFile != null)
                    //{
                    //    string fileName = Path.GetFileName(imageFile.FileName);
                    //    sfileNameNew = $"{Guid.NewGuid().ToString()}.{ FileHelper.GetExtentionByFileName(fileName).ToLower().Replace(".", "")}";
                    //    bool isImage = Constants.Common.ImagesExtensions.Contains(FileHelper.GetExtentionByFileName(fileName).ToLower().Replace(".", ""));
                    //    bool isAudio = Constants.Common.AudiosExtensions.Contains(FileHelper.GetExtentionByFileName(fileName).ToLower().Replace(".", ""));
                    //    if (isImage)
                    //    {
                    //        fileDest = Path.Combine(_hostingEnvironment.WebRootPath, Constants.Common.UploadDirImages);        //public const string UploadDirImages = @"Upload/Images";
                    //        sIcon = $"{Constants.Common.UploadDirImages}/{sfileNameNew}";
                    //    }
                    //    else if (isAudio)
                    //    {
                    //        fileDest = Path.Combine(_hostingEnvironment.WebRootPath, Constants.Common.UploadDirAudio);        //public const string UploadDirAudio = @"Upload/Audios";
                    //        sIcon = $"{Constants.Common.UploadDirAudio}/{sfileNameNew}";
                    //    }
                    //    else
                    //    {
                    //        fileDest = Path.Combine(_hostingEnvironment.WebRootPath, Constants.Common.UploadDirFiles);        //public const string UploadDirFiles = @"Upload/Files";
                    //        sIcon = $"{Constants.Common.UploadDirFiles}/{sfileNameNew}";
                    //    }
                    //}
                    //else if (objNotiTemp.DeleteImage == 1 && objNotiTemp.ChoiseImage == 0)
                    //    sIcon = "";
                    //else sIcon = string.IsNullOrEmpty(objNotiTemp.Images) ? "" : objNotiTemp.Images;

                    if (!string.IsNullOrEmpty(sIcon))
                        sIcon = sIcon.Replace(@"\", @"/").Replace(@"\\", @"//");

                    NotiTempViewModel objNotiTempUpd = new NotiTempViewModel();
                    objNotiTempUpd.NotiType = string.IsNullOrEmpty(objNotiTemp.NotiType) ? "" : objNotiTemp.NotiType;
                    if(objNotiTemp.NotiSend == "1")
                        objNotiTempUpd.SmsTemp = string.IsNullOrEmpty(objNotiTemp.Detail) ? "" : objNotiTemp.Detail;
                    else if (objNotiTemp.NotiSend == "2")
                        objNotiTempUpd.OttTemp = string.IsNullOrEmpty(objNotiTemp.Detail) ? "" : objNotiTemp.Detail;
                    else
                        objNotiTempUpd.EmailTemp = string.IsNullOrEmpty(objNotiTemp.Detail) ? "" : objNotiTemp.Detail;
                    objNotiTempUpd.Detail = string.IsNullOrEmpty(objNotiTemp.Detail) ? "" : objNotiTemp.Detail;
                    objNotiTempUpd.Status = objNotiTemp.Status;
                    objNotiTempUpd.Id = objNotiTemp.Id;
                    objNotiTempUpd.MailSubject = string.IsNullOrEmpty(objNotiTemp.MailSubject) ? "" : objNotiTemp.MailSubject;
                    ////Xét trường hợp Xóa ảnh đại diện đi => Thực hiện xóa ảnh lưu trong thư mục theo đường dẫn.
                    //if (objNotiTemp.DeleteImage == 1 && objNotiTemp.ChoiseImage == 0 && !string.IsNullOrEmpty(sIcon))
                    //{
                    //    objNotiTemp.ImagesHistory = "";
                    //    objNotiTempUpd.Images = "";
                    //}

                    iRetId = _service.UpdateNotiTemp(objNotiTempUpd, UserName);
                    result = (iRetId > 0) ? "0" : "-1";
                    if (result == "0")
                    {
                        NotiMsgTempRequest notiMsg = new NotiMsgTempRequest();
                        notiMsg.NotiType = objNotiTemp.NotiType;
                        notiMsg.SmsTemp = objNotiTempUpd.SmsTemp;
                        notiMsg.OttTemp = objNotiTempUpd.OttTemp;
                        notiMsg.EmailTemp= objNotiTempUpd.EmailTemp;
                        notiMsg.MailSubject = objNotiTempUpd.MailSubject;
                        var resultNoti = await _notiService.UpdateNotiMsgTempAsync(notiMsg);
                        if (resultNoti == null)
                            return StatusCode(500, "Failed to call update-notimsg-temp API");
                    }     
                    //if (imageFile != null)
                    //{
                    //    using (FileStream stream = new FileStream(Path.Combine(fileDest, sfileNameNew), FileMode.Create))
                    //    {
                    //        imageFile.CopyTo(stream);
                    //    }
                    //}

                    ////Xét trường hợp Xóa ảnh đại diện đi => Thực hiện xóa ảnh lưu trong thư mục theo đường dẫn.
                    //if (objNotiTemp.DeleteImage == 1 && objNotiTemp.ChoiseImage == 0 && !string.IsNullOrEmpty(sIcon))
                    //{
                    //    //Thực hiện xóa file đính kèm
                    //    fileDest = Path.Combine(_hostingEnvironment.WebRootPath, sIcon);
                    //    bool isDeleteFile = FileHelper.Delete_File(fileDest);
                    //}
                    //if (!string.IsNullOrEmpty(objNotiTemp.ImagesHistory) && !string.IsNullOrEmpty(objNotiTempUpd.Images) && objNotiTemp.ImagesHistory.Substring(objNotiTemp.ImagesHistory.LastIndexOf("/") + 1) != objNotiTempUpd.Images.Substring(objNotiTemp.Images.LastIndexOf("/") + 1))
                    //{
                    //    //Thực hiện xóa file đính kèm
                    //    if (objNotiTemp.ImagesHistory.Substring(objNotiTemp.ImagesHistory.LastIndexOf("/") + 1) != "FILE_NO_FOUND.jpg")
                    //    {
                    //        fileDest = Path.Combine(_hostingEnvironment.WebRootPath, objNotiTemp.ImagesHistory);
                    //        bool isDeleteFile = FileHelper.Delete_File(fileDest);
                    //    }
                    //    objNotiTemp.ImagesHistory = sIcon;
                    //}
                    //else if (!string.IsNullOrEmpty(objNotiTemp.ImagesHistory) && string.IsNullOrEmpty(objNotiTempUpd.Images))
                    //{
                    //    //Thực hiện xóa file đính kèm
                    //    if (objNotiTemp.ImagesHistory.Substring(objNotiTemp.ImagesHistory.LastIndexOf("/") + 1) != "FILE_NO_FOUND.jpg")
                    //    {
                    //        fileDest = Path.Combine(_hostingEnvironment.WebRootPath, objNotiTemp.ImagesHistory);
                    //        bool isDeleteFile = FileHelper.Delete_File(fileDest);
                    //    }
                    //    objNotiTemp.ImagesHistory = sIcon;
                    //}
                    //else objNotiTemp.ImagesHistory = sIcon;

                    //result = result + "#" + iRetId.ToString() + "#" + objNotiTemp.ImagesHistory;
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error SaveUpdateArticlesCourse: {ex.Message}");
                return new JsonResult("99");
            }
        }

        /// <summary>
        ///  Hàm lấy danh sách Mẫu thông báo
        /// </summary>
        public async Task<JsonResult> GetListNotiType(string pId, string pNotiType, string pStatus)
        {
            string sTitleChoice = "";
            ArrayList data = new ArrayList();
        
            var listNoti = await _notiService.GetListNotiTempAsync(pStatus);
        
            if (sTitleChoice != "")
                data.Add(new { id = "", value = sTitleChoice });
        
            foreach (NotiTempViewModel item in listNoti)
            {
                data.Add(new { id = item.NotiType, value = item.NotiType.Trim() });
            }
        
            return Json(data);
        }

        /// <summary>
        ///  Hàm lấy danh sách các trường tham chiếu bảng VBSP_NOTIFICATION_DATA
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái. Nếu là 0 lấy tất</param>
        /// <param name="pTitleChoice">Tiêu đề lựa chọn danh sách</param>
        /// <param name="pFlagTextShow">Trang thái hiển thị trên Combobox. Giá trị:
        ///                      1 - Hiển thị duy nhất Tên trên danh sách ComBoBox
        ///                      2 - Hiển thị [Mã - Tên] trên danh sách ComBoBox
        ///                      3 - Hiển thị [Vùng -> Mã - Tên] trên danh sách ComBoBox
        /// </param>
        /// <returns>Danh sách Tỉnh/Thành phố trên Combobox</returns>
        public JsonResult GetListNotiLink(int pParentId)
        {
            string sTitleChoice = "";
            ArrayList data = new ArrayList();
            var listNoti = _serviceLOV.GetListOfValueSearch(pParentId,"",0,"","",1,0);
            if (sTitleChoice != "")
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfValueViewModel item in listNoti)
            {            
                data.Add(new { id = item.Id, value = item.ShortName.Trim() });            
            }
            return Json(data);
        }

        //[Route("upload_ckeditor")]
        [HttpPost]
        public async Task<JsonResult> UploadCKEditor(IFormFile upload)
        {            
            //var uploadShortPath = Path.Combine("upload", _utilities.GetFolderByDate());//timeStamp.ToString("yyyy") 
            //var uploadShortPath = Path.Combine("upload", );
            var uploadFullPath = Path.Combine(Directory.GetCurrentDirectory(), _hostingEnvironment.WebRootPath, Constants.Common.UploadDirPhotoInContent);
            if (!Directory.Exists(uploadFullPath))
            {
                Directory.CreateDirectory(uploadFullPath);
            }
            var fileName = upload.FileName;
            string sfileNameNew = $"{Constants.Common.FirstNameFile}_{DateTime.Now.ToString("yyyy")}_{Guid.NewGuid().ToString()}.{_service.GetExtentionByFileName(fileName).ToLower().Replace(".", "")}";
            var path = Path.Combine(uploadFullPath, sfileNameNew);           
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }
            var url = $"../{Constants.Common.UploadDirPhotoInContent}/{sfileNameNew}";
            var success = new
            {
                uploaded = 1,
                fileName = sfileNameNew,
                url = url
            };
            return new JsonResult(success);
        }

        public async Task<IActionResult> GetNotiTemplateDetail(string pNotiType, string pNotiSend)
        {
            object result = null;
            string pStatus = "A";
            var listNoti = await _notiService.GetListNotiTempAsync(pStatus);
            if(listNoti != null)
            {
                var listNotibyType = listNoti.Where(w=>w.NotiType == pNotiType).ToList();
                result = listNotibyType.Select(x => new {
                    notiType = x.NotiType,
                    mailSubject = x.MailSubject,
                    status = x.Status,
                    detail = pNotiSend switch
                    {
                        "1" => x.SmsTemp,   // SMS
                        "2" => x.OttTemp,   // OTT
                        "3" => x.EmailTemp,   // Email
                    },
                });
            }                               
            return Json(result);
        }

        /// <summary>
        /// Load dữ liệu lên lưới thông báo
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LoadGridData([DataSourceRequest] DataSourceRequest request, string findNotiType, string findStatus)
        {
            findNotiType = (string.IsNullOrEmpty(findNotiType) || findNotiType == "null") ? "" : findNotiType;
            findStatus = (string.IsNullOrEmpty(findStatus) || findStatus == "null") ? "" : findStatus;
            var listNoti = await _service.GetNotiTemplate(findStatus, findNotiType);                        
            return Json(listNoti.ToDataSourceResult(request));
        }

        /// <summary>
        /// Hàm gọi Show màn hình chi tiết thông báo Noti
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ShowNotiUpdate(string pNotiType, string pStatus)
        {
            NotiTempViewModel objResultNoti = new NotiTempViewModel();
            if (!string.IsNullOrEmpty(pNotiType))
            {
                var listNoti = await _service.GetNotiTemplate(pStatus, pNotiType);
                if (listNoti != null)
                {
                    foreach (NotiTempViewModel _item in listNoti)
                    {

                        objResultNoti.NotiType = _item.NotiType;
                        objResultNoti.SmsTemp = _item.SmsTemp;
                        objResultNoti.OttTemp = _item.OttTemp;
                        objResultNoti.EmailTemp = _item.EmailTemp;
                        objResultNoti.Status = _item.Status;
                        objResultNoti.NotiSend = "1";
                        objResultNoti.Detail = _item.SmsTemp;
                        objResultNoti.Description = _item.Description;
                        objResultNoti.NotiLink = _item.NotiLink;
                        objResultNoti.MailSubject = _item.MailSubject;
                        objResultNoti.StatusHT = _item.StatusHT;                       
                        TempData["FlagCall"] = "3";
                        break;
                    }
                }                   
            }
            else
            {
                objResultNoti.NotiType = "";
                objResultNoti.SmsTemp = "";
                objResultNoti.OttTemp = "";
                objResultNoti.EmailTemp = "";
                objResultNoti.Status = "A";
                objResultNoti.NotiSend = "1";
                objResultNoti.Detail = "";
                objResultNoti.Description = "";
                objResultNoti.NotiLink = "";
                objResultNoti.MailSubject = "";
                objResultNoti.StatusHT = "";
            }
            return PartialView("UpdateNotiTemplate", objResultNoti);
        }

        /// <summary>
        /// Xóa thông báo Noti 
        /// </summary>
        /// <returns></returns>
        public async Task<string> DeleteNotiTemp(string pNotiType, string pStatus = "A")
        {
            string result = await _service.DeleteNotiTemp(pNotiType, pStatus);
            return result;
        }
    }
}
