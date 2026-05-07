using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Policy;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
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
        private readonly IInterestRateConfigureService _interestRateConfigureService;
        private readonly IMapper _mapper;
        private readonly IProductService _createConfigService;
        private readonly IProductService _productService;
        private readonly IListOfTransPointService _transpointService;


        /// <summary>
        /// Initializes a new instance of the <see cref="ListController"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="service">The service<see cref="IListOfValueService"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger{ListController}"/>.</param>
        /// <param name="menuService">The menuService<see cref="IPermitService"/>.</param>
        public NotiController(ILogger<BaseController> logger, IWebHostEnvironment hostingEnvironment, INotiService service, IApiNotiGatewayService notiService, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper,
            IInterestRateConfigureService interestRateConfigureService, IListOfTransPointService transpointService,
            IMapper mapper,
            IProductService createConfigService,
            IProductService productService) : base(logger, adminService, sessionHelper)
        {
            _serviceLOV = serviceLOV;
            _service = service;
            _hostingEnvironment = hostingEnvironment;
            _notiService = notiService;
            _interestRateConfigureService = interestRateConfigureService;
            _mapper = mapper;
            _createConfigService = createConfigService;
            _productService = productService;
            _transpointService = transpointService;
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
                    if (objNotiTemp.NotiSend == "1")
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
                        notiMsg.EmailTemp = objNotiTempUpd.EmailTemp;
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
            var listNoti = _serviceLOV.GetListOfValueSearch(pParentId, "", 0, "", "", 1, 0);
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
            if (listNoti != null)
            {
                var listNotibyType = listNoti.Where(w => w.NotiType == pNotiType).ToList();
                result = listNotibyType.Select(x => new
                {
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

       

        //[HttpGet]
        //public async Task<IActionResult> TestGetNotificationDataAuto()
        //{
        //    try
        //    {
        //        var request = new NotificationSearchRequest
        //        {
        //            NotiType = "USER_OFFLINE",
        //            Conditions = new Dictionary<string, string>
        //            {
        //                { "d2", "BDA080" },
        //                { "d5", "COMMUNE_HEAD" }
        //            }
        //        };

        //        var result = await _notiService.GetNotificationDataAutoAsync(request);

        //        if (result != null && result.Result != null)
        //        {
        //            // Gọi UpdateNotiDataList với dữ liệu lấy được
        //            var updateResult = await _notiService.UpdateNotiDataList(
        //                new List<NotificationDataResponse> { result.Result }
        //            );
        //            //_logger.LogInformation("UpdateNotiDataList result: {UpdateResult}", updateResult);
        //        }
        //        else
        //        {
        //            //_logger.LogWarning("Không có dữ liệu NotificationDataResponse để cập nhật");
        //        }

        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError(ex, "Lỗi TestGetNotificationDataAuto");

        //        return Json(new
        //        {
        //            code = "-1",
        //            message = ex.Message,
        //            result = (object?)null,
        //            success = false
        //        });
        //    }
        //}


        public IActionResult IndexNotiOffline()
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



        public IActionResult LoadTranspointGridData([DataSourceRequest] DataSourceRequest request, string pPosCodeFind, string pTranspointCode,
            string pTranspointName, string pBeginDateFind, string pEndDateFind)
        {
            string sPosCodeFind = "", sTranspointName = "", sPosCode = "", sTranspointCode;
            sPosCodeFind = (string.IsNullOrEmpty(pPosCodeFind) || pPosCodeFind == "" || pPosCodeFind == "null") ? "" : pPosCodeFind;
            sTranspointName = (string.IsNullOrEmpty(pTranspointName) || pTranspointName == "" || pTranspointName == "null") ? "" : pTranspointName;
            sTranspointCode = (string.IsNullOrEmpty(pTranspointCode) || pTranspointCode == "" || pTranspointCode == "null" || pTranspointCode == "-1") ? "" : pTranspointCode;
            DateTime sBeginDateFind = DateTime.ParseExact(pBeginDateFind, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime sEndDateFind = DateTime.ParseExact(pEndDateFind, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            int iBeginDate = sBeginDateFind.Day;
            int iEndDate = sEndDateFind.Day;
            if (UserPosCode == "" || UserPosCode == "000100" || UserPosCode == "000199")
            {
                sPosCode = sPosCodeFind;
            }
            else
            {
                if (string.IsNullOrEmpty(sPosCodeFind) || sPosCodeFind == "0" || sPosCodeFind == "000000")
                {
                    string sSQL = $"Select Top 1 IsNull(MainPosCode,'') Code From ListOfPos Where Code = {UserPosCode}";
                    string sMainPosTMP = _serviceLOV.GetCellValueForQuery(sSQL);
                    sPosCode = sMainPosTMP;
                }
                else sPosCode = sPosCodeFind;
            }
            if (!string.IsNullOrWhiteSpace(sPosCode))
            {
                //sTranspointCode chuyển thành pCommuneCode
                var tranpointLists = _transpointService.GetListOfTransPointSearch("", sPosCode, sTranspointCode, "", sTranspointName, 1, 31, "", "");
                var pointActive = new List<ListOfTransPointViewModel>();
                foreach (var tranpoint in tranpointLists)
                {
                    var data = _notiService
                        .GetNotificationDataUserOffline(NotiDataType.USER_OFFLINE, tranpoint.PosCode, tranpoint.TxnPointCode)
                        .GetAwaiter().GetResult();

                    if (data?.Result == null || data.Result.Count() == 0)
                    {
                        continue;
                    }

                    if (!DateTime.TryParseExact(
                            data.Result[0].d6,
                            "yyyyMMdd",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out var visitDate))
                    {
                        continue; // skip nếu parse lỗi
                    }

                    // Check nằm trong khoảng
                    if (visitDate < sBeginDateFind || visitDate > sEndDateFind)
                        continue;

                    tranpoint.UserNumer = data.Result.Count;
                    tranpoint.VisitDateD6 = visitDate.ToString("dd/MM/yyyy");

                    pointActive.Add(tranpoint);
                }
                //order by
                pointActive = pointActive
                .OrderByDescending(x => x.VisitDateD6) 
                .ToList();
                // Gán lại OrderNo sau khi sort
                for (int i = 0; i < pointActive.Count; i++)
                {
                    pointActive[i].OrderNo = i + 1;
                }
                return Json(pointActive.ToDataSourceResult(request, ModelState));
            }
            else
                return Json(new List<ListOfTransPointViewModel>().ToDataSourceResult(request, ModelState));
        }


        [HttpGet]
        public JsonResult GetTXNPointOptions()
        {

            //var noti = new NotificationDataResponse
            //{

            //    notiType = "SMS0001",
            //    sourceId = "SRC001",
            //    businessDate = DateTime.Parse("2026-03-31"),

            //    posCode = "POS123",
            //    posName = "Điểm giao dịch Hà Nội",

            //    customerId = "CUST001",
            //    customerName = "Nguyễn Văn A",
            //    mobileNo = "0975177188",
            //    email = "quyenk48@vbsp.vn",

            //    d1 = "data1",
            //    d2 = "data2",
            //    d3 = "data3",
            //    d4 = "data4",
            //    d5 = "TELLER",
            //    d6 = "20260331",
            //    d7 = "data7",
            //    d8 = "data8",
            //    d9 = "data9",
            //    d10 = "data10",
            //    d11 = "data11",
            //    d12 = "data12",
            //    d13 = "data13",
            //    d14 = "data14",
            //    d15 = "data15",
            //    d16 = "data16",
            //    d17 = "data17",
            //    d18 = "data18",
            //    d19 = "data19",
            //    d20 = "data20",

            //    status = "0",
            //    errorCode = null,
            //    errorMessage = null,

            //    createdTime = DateTime.Now,
            //    createdBy = "system",
            //    updatedTime = DateTime.Now,
            //    updatedBy = "admin",

            //    sendTime = DateTime.Now,
            //    sendBy = "system",
            //    messageId = "MSG123456",
            //    sendType = "1/2/3",



            //    status2 = "0",


            //    status3 = "0",

            //};

            //var result =  _notiService.InsertNotiDataList(new List<NotificationDataResponse> { noti });
            //if(result.Result.Code == "00")
            //{
            //    var g = 1;
            //}    

            //var listCommunes = _serviceLOV.GetLovCommuneList("", "", "", "000301", "");

            //var tranpointLists = _transpointService.GetListOfTransPointSearch("", "000301", "", "", "", 1, 31, "");
            var statuses = new List<object>();
            statuses.Add(new { Value = "-1", Description = "Tất cả", Code = "ALL" });

            ////statuses.Add(new { Value = "0", Description = "Chưa có dữ liệu", Code = "ALL" });

            //statuses.Add(new { Value = "06001G", Description = "Ba Vì", Code = "06001G" });
            //statuses.Add(new { Value = "06001G", Description = "Hợp Nhất", Code = "06001G" });
            //statuses.Add(new { Value = "06001G", Description = "Khánh Thượng", Code = "06001G" });

            //statuses.Add(new { Value = "06001H", Description = "Bất Bạt", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Cẩm Lĩnh", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Thuần Mỹ", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Tòng Bạt", Code = "06001H" });

            //statuses.Add(new { Value = "06001I", Description = "Cổ Đô", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Kiều Mộc", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phong Vân", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Cường", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Đông", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Hồng", Code = "06001I" });

            //statuses.Add(new { Value = "06001J", Description = "Minh Châu", Code = "06001J" });

            //statuses.Add(new { Value = "06001K", Description = "Cam Thượng", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Chu Minh", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Đông Quang", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Quảng Oai", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Thuỵ An", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Tiên Phong", Code = "06001K" });

            //statuses.Add(new { Value = "06001L", Description = "Ba Trại", Code = "06001L" });
            //statuses.Add(new { Value = "06001L", Description = "Suối Hai", Code = "06001L" });

            //statuses.Add(new { Value = "06001M", Description = "Cộng Hòa", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Đồng Thái", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Phú Châu", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Phú Sơn", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Vật Lại", Code = "06001M" });

            //statuses.Add(new { Value = "06001N", Description = "Thôn Bài", Code = "06001N" });
            //statuses.Add(new { Value = "06001N", Description = "Yên Bài", Code = "06001N" });
            //foreach (var item in listCommunes)
            //{
            //    statuses.Add(new
            //    {
            //        Value = item.TxnPointCode,
            //        Description = item.TxnPointName,
            //        Code = "1"
            //    });
            //}
            statuses.Add(new { Value = "1", Description = "Bạn chưa chọn đơn vị", Code = "ALL" });
            return Json(statuses);
        }

        public JsonResult GetTXNPointChange(string posCode)
        {
            //var tranpointLists = _transpointService.GetListOfTransPointSearch("", posCode, "", "", "", 1, 31, "");

            var listCommunes = _serviceLOV.GetLovCommuneList("", "", "", posCode, "");
            var statuses = new List<object>();
            statuses.Add(new { Value = "-1", Description = "Tất cả", Code = "ALL" });

            if (listCommunes == null || listCommunes.Count == 0)
            {
                statuses.Add(new { Value = "1", Description = "Danh sách xã rỗng", Code = "ALL" });
            }

            foreach (var item in listCommunes)
            {
                statuses.Add(new
                {
                    Value = item.TxnPointCode,
                    Description = item.TxnPointName,
                    Code = "1"
                });
            }
            return Json(statuses);
            //var statuses = new List<object>();
            //statuses.Add(new { Value = "-1", Description = "Tất cả", Code = "ALL" });

            ////statuses.Add(new { Value = "0", Description = "Chưa có dữ liệu", Code = "ALL" });

            //statuses.Add(new { Value = "06001G", Description = "Ba Vì", Code = "06001G" });
            //statuses.Add(new { Value = "06001G", Description = "Hợp Nhất", Code = "06001G" });
            //statuses.Add(new { Value = "06001G", Description = "Khánh Thượng", Code = "06001G" });

            //statuses.Add(new { Value = "06001H", Description = "Bất Bạt", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Cẩm Lĩnh", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Thuần Mỹ", Code = "06001H" });
            //statuses.Add(new { Value = "06001H", Description = "Tòng Bạt", Code = "06001H" });

            //statuses.Add(new { Value = "06001I", Description = "Cổ Đô", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Kiều Mộc", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phong Vân", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Cường", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Đông", Code = "06001I" });
            //statuses.Add(new { Value = "06001I", Description = "Phú Hồng", Code = "06001I" });

            //statuses.Add(new { Value = "06001J", Description = "Minh Châu", Code = "06001J" });

            //statuses.Add(new { Value = "06001K", Description = "Cam Thượng", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Chu Minh", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Đông Quang", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Quảng Oai", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Thuỵ An", Code = "06001K" });
            //statuses.Add(new { Value = "06001K", Description = "Tiên Phong", Code = "06001K" });

            //statuses.Add(new { Value = "06001L", Description = "Ba Trại", Code = "06001L" });
            //statuses.Add(new { Value = "06001L", Description = "Suối Hai", Code = "06001L" });

            //statuses.Add(new { Value = "06001M", Description = "Cộng Hòa", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Đồng Thái", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Phú Châu", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Phú Sơn", Code = "06001M" });
            //statuses.Add(new { Value = "06001M", Description = "Vật Lại", Code = "06001M" });

            //statuses.Add(new { Value = "06001N", Description = "Thôn Bài", Code = "06001N" });
            //statuses.Add(new { Value = "06001N", Description = "Yên Bài", Code = "06001N" });
            //return Json(statuses);
        }

        [HttpGet]
        public async Task<string> GetApplyPosListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return "Không có POS áp dụng";

            var lstIds = ids.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Where(s => long.TryParse(s, out _))
                .Select(long.Parse)
                .ToList();

            if (!lstIds.Any()) return "Không có POS áp dụng";

            return await _interestRateConfigureService.GetApplyPosListByIdsAsync(lstIds);
        }

        public async Task<ActionResult> ResendNotiInfo(string pNotiType, string pPosCode, string pTransPoint, string pTransDate, string pTxnPointName)
        {
            var result = await _notiService.GetNotificationDataUserOffline(pNotiType, pPosCode, pTransPoint);
            var data = result.Result;

            if (data == null)
            {
                throw new Exception("DATA NULL");
            }
            TempData["pNotiType"] = pNotiType;
            TempData["pPosCode"] = pPosCode;
            TempData["pTransPoint"] = pTransPoint;
            TempData["pTransDate"] = pTransDate;
            TempData["pTxnPointName"] = pTxnPointName;
            return PartialView("ReSendNotiInfo", result.Result); 
        }

        [HttpPost]
        public async Task<ActionResult> UpdateNoti(string notiType, string posCode, string transPoint, string transDate)
        {
            var result = await _notiService.UpdateNotiDataOffline(notiType, posCode, transPoint, transDate,UserName);
            return Json(result);
        }
    }
}
