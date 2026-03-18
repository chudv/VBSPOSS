using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Configuration;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Extensions;
using VBSPOSS.Filters;
using VBSPOSS.Helpers;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Transformations;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{

    public class IntRateConfigController : BaseController
    {
        private readonly ILogger<IntRateConfigController> _logger;
        private readonly IInterestRateConfigureService _intRateConfigService;
        private readonly IListOfValueService _serviceLOV;
        private readonly IProductService _serviceProduct;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public IntRateConfigController(ILogger<IntRateConfigController> logger, IAdministrationService adminService,
            ISessionHelper sessionHelper, IInterestRateConfigureService intRateConfigService,
                    IListOfValueService serviceLOV, IProductService serviceProduct, IMapper mapper, ApplicationDbContext context) : base(logger, adminService, sessionHelper)
        {
            _logger = logger;
            _intRateConfigService = intRateConfigService;
            _serviceLOV = serviceLOV;
            _mapper = mapper;
            _context = context;
            _serviceProduct = serviceProduct;
        }

        public IActionResult IndexTidePenalRates()
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
            TempData["ProductGroupCode"] = ProductGroupCode.ProductGroupCode_DepositPenal;
            
            TempData["EventFlag_Add"] = EventFlag.EventFlag_Add.Value.ToString();
            TempData["EventFlag_Edit"] = EventFlag.EventFlag_Edit.Value.ToString();
            TempData["EventFlag_Delete"] = EventFlag.EventFlag_Delete.Value.ToString();
            TempData["EventFlag_MarkDeleted"] = EventFlag.EventFlag_MarkDeleted.Value.ToString();
            TempData["EventFlag_Approval"] = EventFlag.EventFlag_Approval.Value.ToString();
            TempData["EventFlag_Authorize"] = EventFlag.EventFlag_Authorize.Value.ToString();
            TempData["EventFlag_View"] = EventFlag.EventFlag_View.Value.ToString();

            return View("IndexTidePenalRates");
        }

        /// <summary>
        /// Hàm lấy danh sách lên lưới dữ liệu Danh sách quyết định cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pPosCode">Mã đơn vị</param>
        /// <param name="pCircularRefNum">Số quyết định hoặc nội dung quyết định</param>
        /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
        /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
        /// <returns>Danh sách Quyết định cấu hình LS rút trước hạn sản phẩm Tide</returns>
        public ActionResult LoadGridData_MasterTidePenalRate([DataSourceRequest] DataSourceRequest request, string pProductGroupCode,
                            string pPosCode, string pCircularRefNum, string pFromEffectiveDate, string pToEffectiveDate)
        {
            try
            {
                string sMainPosCode = "";
                
                if (string.IsNullOrEmpty(pPosCode))
                    pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
                if (UserGrade == PosGrade.MAIN_POS && !(pPosCode != "000100" || pPosCode != "000101" || pPosCode != "000196" || pPosCode != "000197" || pPosCode != "000199"))
                {
                    sMainPosCode = pPosCode;
                }
                int iFromEffectiveDate = 0, ToEffectiveDate = 0;
                if (string.IsNullOrEmpty(pProductGroupCode))
                    pProductGroupCode = ProductGroupCode.ProductGroupCode_DepositPenal;
                if (string.IsNullOrEmpty(pCircularRefNum))
                    pCircularRefNum = "";
                iFromEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pFromEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));
                ToEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pToEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));

                var listMasterTidePenalRate = _intRateConfigService.GetListInterestRateConfigMasterViews(sMainPosCode, pPosCode, pProductGroupCode, pCircularRefNum, 
                                                                                                                    iFromEffectiveDate, ToEffectiveDate, 0, "");
                var viewModels = _mapper.Map<List<InterestRateConfigMasterView>>(listMasterTidePenalRate);
                WriteLog(LogType.INFOR, $"LoadGridData_MasterTidePenalRate - Returning {viewModels.Count} records");
                return Json(viewModels.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                WriteLog(LogType.INFOR, $"LoadGridData_MasterTidePenalRate('{pProductGroupCode}','{pPosCode}','{pCircularRefNum}','{pFromEffectiveDate}','{pToEffectiveDate}') => Error: {ex.Message}");
                return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách quyết định cấu hình lãi suất rút trức hạn." });
            }
        }

        /// <summary>
        /// Hàm thực hiện gọi sự kiện Show Form: Thêm/Xem/Sửa/Phê duyệt/Trình duyệt
        /// </summary>
        /// <param name="pPosCode">Mã POS/Chi nhánh/Toàn quốc cần gọi</param>
        /// <param name="pDocumentId">Chỉ số xác định Quyết định thay đổi LS cần gọi (Chỉ khi trình duyệt mới có giá trị)</param>
        /// <param name="pListId">Danh sách Id trong bảng InterestRateConfigMaster có chung QĐ thay đổi lãi suất</param>
        /// <param name="pProductGroupCode">Phân loại: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện EventFlag.Biến. Giá trị: 1 - Thêm; 2 - Sửa; 3 - Xóa; 4 - Trình duyệt; 5 - Phê duyệt; 6 - Xem chi tiết; 0-Đánh dấu xóa</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ShowUpdateTidePenalRate(string pPosCode, long pDocumentId, string pListId, string pProductGroupCode, string pFlagCall)
        {
            UpdateTidePenalRateConfigViewModel objPenalRateConfigUpd = new UpdateTidePenalRateConfigViewModel();
            if (string.IsNullOrEmpty(pListId))
                pListId = "";
            string sNameView = "";
            
            if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() && string.IsNullOrEmpty(pListId))
            {
                #region --- Sự kiện gọi Form Thêm mới cấu hình lãi suất rút trước hạn SP tiền gửi CKH ---
                objPenalRateConfigUpd.IdList = pListId;
                objPenalRateConfigUpd.Id = 0;
                objPenalRateConfigUpd.DocumentId = 0;
                objPenalRateConfigUpd.ProductCode = "";
                objPenalRateConfigUpd.ProductName = "";
                objPenalRateConfigUpd.AccountTypeCode = "";
                objPenalRateConfigUpd.AccountTypeName = "";
                objPenalRateConfigUpd.CurrencyCode = ConstValueAPI.CurrencyValueDefault;
                objPenalRateConfigUpd.DebitCreditFlag = VBSPOSS.Constants.DebitCreditFlag.DebitCreditFlag_Credit;
                objPenalRateConfigUpd.EffectiveDate = DateTime.Now.Date;
                objPenalRateConfigUpd.InterestRate = 0;
                objPenalRateConfigUpd.InterestRateNew = 0;
                objPenalRateConfigUpd.ExpiredDate = CustConverter.StringToDate("20501231", FormatParameters.FORMAT_DATE_INT).Date;
                objPenalRateConfigUpd.CircularRefNum = "";
                objPenalRateConfigUpd.PosCode = UserPosCode;
                objPenalRateConfigUpd.PenalIntRate = 0;
                objPenalRateConfigUpd.TenorSerialNo = 0;
                objPenalRateConfigUpd.CircularDate = DateTime.Now.Date;
                objPenalRateConfigUpd.PosName = "";
                objPenalRateConfigUpd.OrderNo = 0;
                objPenalRateConfigUpd.Status = StatusTrans.Status_Created.Value;
                objPenalRateConfigUpd.StatusText = StatusTrans.Status_Created.Description;
                objPenalRateConfigUpd.EffectiveDateText = objPenalRateConfigUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);

                objPenalRateConfigUpd.ProductCodeList = "";
                objPenalRateConfigUpd.ExpiredDateText = objPenalRateConfigUpd.ExpiredDate.ToString(FormatParameters.FORMAT_DATE);
                objPenalRateConfigUpd.CircularDateText = objPenalRateConfigUpd.CircularDate.ToString(FormatParameters.FORMAT_DATE);

                objPenalRateConfigUpd.CreatedBy = UserName;
                objPenalRateConfigUpd.CreatedDate = DateTime.Now;
                objPenalRateConfigUpd.CreatedDateText = DateTime.Now.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                objPenalRateConfigUpd.ModifiedBy = UserName;
                objPenalRateConfigUpd.ModifiedDate = DateTime.Now;
                objPenalRateConfigUpd.ModifiedDateText = DateTime.Now.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                objPenalRateConfigUpd.ApproverBy = UserName;
                objPenalRateConfigUpd.ApprovalDate = DateTime.Now;
                objPenalRateConfigUpd.ApprovalDateText = DateTime.Now.ToString(FormatParameters.FORMAT_DATE_VN_LONG);
                objPenalRateConfigUpd.RejectFlag = 0;
                objPenalRateConfigUpd.RejectReason = "";
                objPenalRateConfigUpd.ListPosCode = new List<string>();
                objPenalRateConfigUpd.ListPosCodeChoice = "";
                objPenalRateConfigUpd.ListProductCode = new List<string>();

                objPenalRateConfigUpd.ListProductCodeChoice = "";
                if (UserPosCode == PosValue.HEAD_POS || UserPosCode == PosValue.BANK_WIDE || UserPosCode == PosValue.SYSTEM_WIDE)
                {
                    objPenalRateConfigUpd.MinInterestRateSpread = 0;
                    objPenalRateConfigUpd.MaxInterestRateSpread = 10;
                }
                else
                {
                    var listProductParameters = _serviceProduct.GetListProductParametersSearch(ProductGroupCode.ProductGroupCode_DepositPenal, "",
                                                    DateTime.Now.Date, ConfigStatus.AUTHORIZED.Value);
                    if (listProductParameters != null && listProductParameters.Count != 0)
                        objPenalRateConfigUpd.MinInterestRateSpread = listProductParameters.OrderByDescending(o => o.Status).Select(s => s.MinInterestRateSpread).Min();
                    else objPenalRateConfigUpd.MinInterestRateSpread = 0;

                    if (listProductParameters != null && listProductParameters.Count != 0)
                        objPenalRateConfigUpd.MaxInterestRateSpread = listProductParameters.OrderByDescending(o => o.Status).Select(s => s.MaxInterestRateSpread).Max();
                    else objPenalRateConfigUpd.MaxInterestRateSpread = 0;
                }
                objPenalRateConfigUpd.InterestRateHO = 0;
                sNameView = "_UpdateTidePenalRates";
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString() && !string.IsNullOrEmpty(pListId))
            {
                #region --- Sự kiện gọi Form chỉnh sửa cấu hình lãi suất rút trước hạn SP tiền gửi CKH ---
                var objParentIntRateConfigFind = _intRateConfigService.GetListInterestRateConfigMasterViews("", pPosCode, pProductGroupCode, "", 20250101, 20501231,
                                pDocumentId, pListId).OrderByDescending(o => o.EffectiveDate).ThenByDescending(o => o.Status).ThenByDescending(o => o.RecordSerialNo).FirstOrDefault();
                if (objParentIntRateConfigFind != null && !string.IsNullOrEmpty(objParentIntRateConfigFind.IdList))
                {
                    objPenalRateConfigUpd.IdList = pListId;
                    objPenalRateConfigUpd.Id = objParentIntRateConfigFind.Id;
                    objPenalRateConfigUpd.DocumentId = objParentIntRateConfigFind.DocumentId;
                    objPenalRateConfigUpd.ProductCode = objParentIntRateConfigFind.ProductCode;
                    objPenalRateConfigUpd.ProductName = objParentIntRateConfigFind.ProductName;
                    objPenalRateConfigUpd.AccountTypeCode = objParentIntRateConfigFind.AccountTypeCode;
                    objPenalRateConfigUpd.AccountTypeName = objParentIntRateConfigFind.AccountTypeName;
                    objPenalRateConfigUpd.CurrencyCode = objParentIntRateConfigFind.CurrencyCode;
                    objPenalRateConfigUpd.DebitCreditFlag = objParentIntRateConfigFind.DebitCreditFlag;
                    objPenalRateConfigUpd.EffectiveDate = objParentIntRateConfigFind.EffectiveDate.Date;
                    objPenalRateConfigUpd.InterestRate = objParentIntRateConfigFind.InterestRate;
                    objPenalRateConfigUpd.InterestRateNew = objParentIntRateConfigFind.NewInterestRate;
                    objPenalRateConfigUpd.ExpiredDate = objParentIntRateConfigFind.ExpiryDate;
                    objPenalRateConfigUpd.CircularRefNum = objParentIntRateConfigFind.CircularRefNum;
                    objPenalRateConfigUpd.PosCode = objParentIntRateConfigFind.PosCode;
                    objPenalRateConfigUpd.PenalIntRate = objParentIntRateConfigFind.PenalRate;
                    objPenalRateConfigUpd.TenorSerialNo = objParentIntRateConfigFind.TenorSerialNo;
                    objPenalRateConfigUpd.CircularDate = objParentIntRateConfigFind.CircularDate.Value.Date;
                    objPenalRateConfigUpd.PosName = objParentIntRateConfigFind.PosName;
                    objPenalRateConfigUpd.OrderNo = objParentIntRateConfigFind.RecordSerialNo;
                    objPenalRateConfigUpd.EffectiveDateText = objPenalRateConfigUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.Status = objParentIntRateConfigFind.Status;
                    objPenalRateConfigUpd.StatusText = StatusTrans.GetByValue(objParentIntRateConfigFind.Status).Description;
                    if (string.IsNullOrEmpty(objPenalRateConfigUpd.StatusText))
                        objPenalRateConfigUpd.StatusText = "";

                    objPenalRateConfigUpd.ExpiredDateText = objPenalRateConfigUpd.ExpiredDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.CircularDateText = objPenalRateConfigUpd.CircularDate.ToString(FormatParameters.FORMAT_DATE);

                    objPenalRateConfigUpd.CreatedBy = objParentIntRateConfigFind.CreatedBy;
                    objPenalRateConfigUpd.CreatedDate = objParentIntRateConfigFind.CreatedDate.Value;
                    objPenalRateConfigUpd.CreatedDateText = objParentIntRateConfigFind.CreatedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ModifiedBy = objParentIntRateConfigFind.ModifiedBy;
                    objPenalRateConfigUpd.ModifiedDate = objParentIntRateConfigFind.ModifiedDate.Value;
                    objPenalRateConfigUpd.ModifiedDateText = objParentIntRateConfigFind.ModifiedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ApproverBy = objParentIntRateConfigFind.ApproverBy;
                    objPenalRateConfigUpd.ApprovalDate = objParentIntRateConfigFind.ApprovalDate.Value;
                    objPenalRateConfigUpd.ApprovalDateText = objParentIntRateConfigFind.ApprovalDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);
                    objPenalRateConfigUpd.RejectFlag = 0;
                    objPenalRateConfigUpd.RejectReason = "";
                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ListPosCode))
                        objParentIntRateConfigFind.ListPosCode = "";
                    string[] listPosCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ListPosCode, ";");
                    objPenalRateConfigUpd.ListPosCode = new List<string>();
                    objPenalRateConfigUpd.ListPosCode.AddRange(listPosCodes);
                    objPenalRateConfigUpd.ListPosCodeChoice = objParentIntRateConfigFind.ListPosCode;

                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ProductCodeList))
                        objParentIntRateConfigFind.ProductCodeList = "";
                    objPenalRateConfigUpd.ListProductCodeChoice = objParentIntRateConfigFind.ProductCodeList;
                    string[] listProductCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ProductCodeList, ";");
                    objPenalRateConfigUpd.ListProductCode = new List<string>();
                    objPenalRateConfigUpd.ListProductCode.AddRange(listProductCodes);

                    objPenalRateConfigUpd.MinInterestRateSpread = objParentIntRateConfigFind.MinInterestRateSpread;
                    objPenalRateConfigUpd.MaxInterestRateSpread = objParentIntRateConfigFind.MaxInterestRateSpread;

                    sNameView = "_UpdateTidePenalRates";
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_Approval.Value.ToString() && !string.IsNullOrEmpty(pListId))
            {
                #region --- Sự kiện gọi Form trình duyệt cấu hình lãi suất rút trước hạn SP tiền gửi CKH ---
                var objParentIntRateConfigFind = _intRateConfigService.GetListInterestRateConfigMasterViews("", pPosCode, pProductGroupCode, "", 20250101, 20501231,
                                pDocumentId, pListId).OrderByDescending(o => o.EffectiveDate).ThenByDescending(o => o.Status).ThenByDescending(o => o.RecordSerialNo).FirstOrDefault();

                if (objParentIntRateConfigFind != null && !string.IsNullOrEmpty(objParentIntRateConfigFind.IdList))
                {
                    objPenalRateConfigUpd.IdList = pListId;
                    objPenalRateConfigUpd.Id = objParentIntRateConfigFind.Id;
                    objPenalRateConfigUpd.DocumentId = objParentIntRateConfigFind.DocumentId;
                    objPenalRateConfigUpd.ProductCode = objParentIntRateConfigFind.ProductCode;
                    objPenalRateConfigUpd.ProductName = objParentIntRateConfigFind.ProductName;
                    objPenalRateConfigUpd.AccountTypeCode = objParentIntRateConfigFind.AccountTypeCode;
                    objPenalRateConfigUpd.AccountTypeName = objParentIntRateConfigFind.AccountTypeName;
                    objPenalRateConfigUpd.CurrencyCode = objParentIntRateConfigFind.CurrencyCode;
                    objPenalRateConfigUpd.DebitCreditFlag = objParentIntRateConfigFind.DebitCreditFlag;
                    objPenalRateConfigUpd.EffectiveDate = objParentIntRateConfigFind.EffectiveDate.Date;
                    objPenalRateConfigUpd.InterestRate = objParentIntRateConfigFind.InterestRate;
                    objPenalRateConfigUpd.InterestRateNew = objParentIntRateConfigFind.NewInterestRate;
                    objPenalRateConfigUpd.ExpiredDate = objParentIntRateConfigFind.ExpiryDate;
                    objPenalRateConfigUpd.CircularRefNum = objParentIntRateConfigFind.CircularRefNum;
                    objPenalRateConfigUpd.PosCode = objParentIntRateConfigFind.PosCode;
                    objPenalRateConfigUpd.PenalIntRate = objParentIntRateConfigFind.PenalRate;
                    objPenalRateConfigUpd.TenorSerialNo = objParentIntRateConfigFind.TenorSerialNo;
                    objPenalRateConfigUpd.CircularDate = objParentIntRateConfigFind.CircularDate.Value.Date;
                    objPenalRateConfigUpd.PosName = objParentIntRateConfigFind.PosName;
                    objPenalRateConfigUpd.OrderNo = objParentIntRateConfigFind.RecordSerialNo;
                    objPenalRateConfigUpd.EffectiveDateText = objPenalRateConfigUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.Status = objParentIntRateConfigFind.Status;
                    objPenalRateConfigUpd.StatusText = StatusTrans.GetByValue(objParentIntRateConfigFind.Status).Description;
                    objPenalRateConfigUpd.ProductCodeList = objParentIntRateConfigFind.ProductCodeList;
                    objPenalRateConfigUpd.ExpiredDateText = objPenalRateConfigUpd.ExpiredDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.CircularDateText = objPenalRateConfigUpd.CircularDate.ToString(FormatParameters.FORMAT_DATE);

                    objPenalRateConfigUpd.CreatedBy = objParentIntRateConfigFind.CreatedBy;
                    objPenalRateConfigUpd.CreatedDate = objParentIntRateConfigFind.CreatedDate.Value;
                    objPenalRateConfigUpd.CreatedDateText = objParentIntRateConfigFind.CreatedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ModifiedBy = objParentIntRateConfigFind.ModifiedBy;
                    objPenalRateConfigUpd.ModifiedDate = objParentIntRateConfigFind.ModifiedDate.Value;
                    objPenalRateConfigUpd.ModifiedDateText = objParentIntRateConfigFind.ModifiedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ApproverBy = objParentIntRateConfigFind.ApproverBy;
                    objPenalRateConfigUpd.ApprovalDate = objParentIntRateConfigFind.ApprovalDate.Value;
                    objPenalRateConfigUpd.ApprovalDateText = objParentIntRateConfigFind.ApprovalDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);
                    objPenalRateConfigUpd.ProductList = objParentIntRateConfigFind.ProductList;
                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ProductCodeList))
                        objParentIntRateConfigFind.ProductCodeList = "";
                    objPenalRateConfigUpd.ListProductCodeChoice = objParentIntRateConfigFind.ProductCodeList;
                    
                    objPenalRateConfigUpd.RejectFlag = 0;
                    objPenalRateConfigUpd.RejectReason = "";
                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ListPosCode))
                        objParentIntRateConfigFind.ListPosCode = "";
                    string[] listPosCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ListPosCode, ";");
                    objPenalRateConfigUpd.ListPosCode = new List<string>();
                    objPenalRateConfigUpd.ListPosCode.AddRange(listPosCodes);
                    objPenalRateConfigUpd.ListPosCodeChoice = objParentIntRateConfigFind.ListPosCode;

                    string[] listProductCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ProductCodeList, ";");
                    objPenalRateConfigUpd.ListProductCode = new List<string>(); 
                    objPenalRateConfigUpd.ListProductCode.AddRange(listProductCodes);

                    if (string.IsNullOrEmpty(objPenalRateConfigUpd.StatusText))
                        objPenalRateConfigUpd.StatusText = "";
                    objPenalRateConfigUpd.MinInterestRateSpread = objParentIntRateConfigFind.MinInterestRateSpread;
                    objPenalRateConfigUpd.MaxInterestRateSpread = objParentIntRateConfigFind.MaxInterestRateSpread;

                    sNameView = "_ApprovalTidePenalRates";
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_Authorize.Value.ToString() && !string.IsNullOrEmpty(pListId))
            {
                #region --- Sự kiện gọi Form Phê duyệt cấu hình lãi suất rút trước hạn SP tiền gửi CKH ---
                var objParentIntRateConfigFind = _intRateConfigService.GetListInterestRateConfigMasterViews("", pPosCode, pProductGroupCode, "", 20250101, 20501231,
                                pDocumentId, pListId).OrderByDescending(o => o.EffectiveDate).ThenByDescending(o => o.Status).ThenByDescending(o => o.RecordSerialNo).FirstOrDefault();

                if (objParentIntRateConfigFind != null && !string.IsNullOrEmpty(objParentIntRateConfigFind.IdList))
                {
                    objPenalRateConfigUpd.IdList = pListId;
                    objPenalRateConfigUpd.Id = objParentIntRateConfigFind.Id;
                    objPenalRateConfigUpd.DocumentId = objParentIntRateConfigFind.DocumentId;
                    objPenalRateConfigUpd.ProductCode = objParentIntRateConfigFind.ProductCode;
                    objPenalRateConfigUpd.ProductName = objParentIntRateConfigFind.ProductName;
                    objPenalRateConfigUpd.AccountTypeCode = objParentIntRateConfigFind.AccountTypeCode;
                    objPenalRateConfigUpd.AccountTypeName = objParentIntRateConfigFind.AccountTypeName;
                    objPenalRateConfigUpd.CurrencyCode = objParentIntRateConfigFind.CurrencyCode;
                    objPenalRateConfigUpd.DebitCreditFlag = objParentIntRateConfigFind.DebitCreditFlag;
                    objPenalRateConfigUpd.EffectiveDate = objParentIntRateConfigFind.EffectiveDate.Date;
                    objPenalRateConfigUpd.InterestRate = objParentIntRateConfigFind.InterestRate;
                    objPenalRateConfigUpd.InterestRateNew = objParentIntRateConfigFind.NewInterestRate;
                    objPenalRateConfigUpd.ExpiredDate = objParentIntRateConfigFind.ExpiryDate;
                    objPenalRateConfigUpd.CircularRefNum = objParentIntRateConfigFind.CircularRefNum;
                    objPenalRateConfigUpd.PosCode = objParentIntRateConfigFind.PosCode;
                    objPenalRateConfigUpd.PenalIntRate = objParentIntRateConfigFind.PenalRate;
                    objPenalRateConfigUpd.TenorSerialNo = objParentIntRateConfigFind.TenorSerialNo;
                    objPenalRateConfigUpd.CircularDate = objParentIntRateConfigFind.CircularDate.Value.Date;
                    objPenalRateConfigUpd.PosName = objParentIntRateConfigFind.PosName;
                    objPenalRateConfigUpd.OrderNo = objParentIntRateConfigFind.RecordSerialNo;
                    objPenalRateConfigUpd.EffectiveDateText = objPenalRateConfigUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.Status = objParentIntRateConfigFind.Status;
                    objPenalRateConfigUpd.StatusText = StatusTrans.GetByValue(objParentIntRateConfigFind.Status).Description;
                    objPenalRateConfigUpd.ProductCodeList = objParentIntRateConfigFind.ProductCodeList;
                    objPenalRateConfigUpd.ExpiredDateText = objPenalRateConfigUpd.ExpiredDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.CircularDateText = objPenalRateConfigUpd.CircularDate.ToString(FormatParameters.FORMAT_DATE);

                    objPenalRateConfigUpd.CreatedBy = objParentIntRateConfigFind.CreatedBy;
                    objPenalRateConfigUpd.CreatedDate = objParentIntRateConfigFind.CreatedDate.Value;
                    objPenalRateConfigUpd.CreatedDateText = objParentIntRateConfigFind.CreatedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ModifiedBy = objParentIntRateConfigFind.ModifiedBy;
                    objPenalRateConfigUpd.ModifiedDate = objParentIntRateConfigFind.ModifiedDate.Value;
                    objPenalRateConfigUpd.ModifiedDateText = objParentIntRateConfigFind.ModifiedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ApproverBy = objParentIntRateConfigFind.ApproverBy;
                    objPenalRateConfigUpd.ApprovalDate = objParentIntRateConfigFind.ApprovalDate.Value;
                    objPenalRateConfigUpd.ApprovalDateText = objParentIntRateConfigFind.ApprovalDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);
                    objPenalRateConfigUpd.ProductList = objParentIntRateConfigFind.ProductList;
                    objPenalRateConfigUpd.RejectFlag = 0;
                    objPenalRateConfigUpd.RejectReason = "";
                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ProductCodeList))
                        objParentIntRateConfigFind.ProductCodeList = "";
                    objPenalRateConfigUpd.ListProductCodeChoice = objParentIntRateConfigFind.ProductCodeList;

                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ListPosCode))
                        objParentIntRateConfigFind.ListPosCode = "";
                    string[] listPosCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ListPosCode, ";");
                    objPenalRateConfigUpd.ListPosCode = new List<string>();
                    objPenalRateConfigUpd.ListPosCode.AddRange(listPosCodes);
                    objPenalRateConfigUpd.ListPosCodeChoice = objParentIntRateConfigFind.ListPosCode;

                    string[] listProductCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ProductCodeList, ";");
                    objPenalRateConfigUpd.ListProductCode = new List<string>();
                    objPenalRateConfigUpd.ListProductCode.AddRange(listProductCodes);


                    if (string.IsNullOrEmpty(objPenalRateConfigUpd.StatusText))
                        objPenalRateConfigUpd.StatusText = "";
                    objPenalRateConfigUpd.MinInterestRateSpread = objParentIntRateConfigFind.MinInterestRateSpread;
                    objPenalRateConfigUpd.MaxInterestRateSpread = objParentIntRateConfigFind.MaxInterestRateSpread;

                    sNameView = "_AuthorizeTidePenalRates";
                }
                #endregion
            }
            else if (pFlagCall == EventFlag.EventFlag_View.Value.ToString() && !string.IsNullOrEmpty(pListId))
            {
                #region --- Sự kiện gọi Form Xem chi tiết cấu hình lãi suất rút trước hạn SP tiền gửi CKH ---
                var objParentIntRateConfigFind = _intRateConfigService.GetListInterestRateConfigMasterViews("", pPosCode, pProductGroupCode, "", 20250101, 20501231,
                                pDocumentId, pListId).OrderByDescending(o => o.EffectiveDate).ThenByDescending(o => o.Status).ThenByDescending(o => o.RecordSerialNo).FirstOrDefault();

                if (objParentIntRateConfigFind != null && !string.IsNullOrEmpty(objParentIntRateConfigFind.IdList))
                {
                    objPenalRateConfigUpd.IdList = pListId;
                    objPenalRateConfigUpd.Id = objParentIntRateConfigFind.Id;
                    objPenalRateConfigUpd.DocumentId = objParentIntRateConfigFind.DocumentId;
                    objPenalRateConfigUpd.ProductCode = objParentIntRateConfigFind.ProductCode;
                    objPenalRateConfigUpd.ProductName = objParentIntRateConfigFind.ProductName;
                    objPenalRateConfigUpd.AccountTypeCode = objParentIntRateConfigFind.AccountTypeCode;
                    objPenalRateConfigUpd.AccountTypeName = objParentIntRateConfigFind.AccountTypeName;
                    objPenalRateConfigUpd.CurrencyCode = objParentIntRateConfigFind.CurrencyCode;
                    objPenalRateConfigUpd.DebitCreditFlag = objParentIntRateConfigFind.DebitCreditFlag;
                    objPenalRateConfigUpd.EffectiveDate = objParentIntRateConfigFind.EffectiveDate.Date;
                    objPenalRateConfigUpd.InterestRate = objParentIntRateConfigFind.InterestRate;
                    objPenalRateConfigUpd.InterestRateNew = objParentIntRateConfigFind.NewInterestRate;
                    objPenalRateConfigUpd.ExpiredDate = objParentIntRateConfigFind.ExpiryDate;
                    objPenalRateConfigUpd.CircularRefNum = objParentIntRateConfigFind.CircularRefNum;
                    objPenalRateConfigUpd.PosCode = objParentIntRateConfigFind.PosCode;
                    objPenalRateConfigUpd.PenalIntRate = objParentIntRateConfigFind.PenalRate;
                    objPenalRateConfigUpd.TenorSerialNo = objParentIntRateConfigFind.TenorSerialNo;
                    objPenalRateConfigUpd.CircularDate = objParentIntRateConfigFind.CircularDate.Value.Date;
                    objPenalRateConfigUpd.PosName = objParentIntRateConfigFind.PosName;
                    objPenalRateConfigUpd.OrderNo = objParentIntRateConfigFind.RecordSerialNo;
                    objPenalRateConfigUpd.EffectiveDateText = objPenalRateConfigUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.Status = objParentIntRateConfigFind.Status;
                    objPenalRateConfigUpd.StatusText = StatusTrans.GetByValue(objParentIntRateConfigFind.Status).Description;
                    objPenalRateConfigUpd.ProductCodeList = objParentIntRateConfigFind.ProductCodeList;
                    objPenalRateConfigUpd.ExpiredDateText = objPenalRateConfigUpd.ExpiredDate.ToString(FormatParameters.FORMAT_DATE);
                    objPenalRateConfigUpd.CircularDateText = objPenalRateConfigUpd.CircularDate.ToString(FormatParameters.FORMAT_DATE);

                    objPenalRateConfigUpd.CreatedBy = objParentIntRateConfigFind.CreatedBy;
                    objPenalRateConfigUpd.CreatedDate = objParentIntRateConfigFind.CreatedDate.Value;
                    objPenalRateConfigUpd.CreatedDateText = objParentIntRateConfigFind.CreatedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ModifiedBy = objParentIntRateConfigFind.ModifiedBy;
                    objPenalRateConfigUpd.ModifiedDate = objParentIntRateConfigFind.ModifiedDate.Value;
                    objPenalRateConfigUpd.ModifiedDateText = objParentIntRateConfigFind.ModifiedDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);

                    objPenalRateConfigUpd.ApproverBy = objParentIntRateConfigFind.ApproverBy;
                    objPenalRateConfigUpd.ApprovalDate = objParentIntRateConfigFind.ApprovalDate.Value;
                    objPenalRateConfigUpd.ApprovalDateText = objParentIntRateConfigFind.ApprovalDate.Value.ToString(FormatParameters.FORMAT_DATE_VN_LONG);
                    objPenalRateConfigUpd.ProductList = objParentIntRateConfigFind.ProductList;
                    objPenalRateConfigUpd.RejectFlag = 0;
                    objPenalRateConfigUpd.RejectReason = objParentIntRateConfigFind.Remark;
                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ProductCodeList))
                        objParentIntRateConfigFind.ProductCodeList = "";
                    objPenalRateConfigUpd.ListProductCodeChoice = objParentIntRateConfigFind.ProductCodeList;

                    if (string.IsNullOrEmpty(objParentIntRateConfigFind.ListPosCode))
                        objParentIntRateConfigFind.ListPosCode = "";
                    string[] listPosCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ListPosCode, ";");
                    objPenalRateConfigUpd.ListPosCode = new List<string>();
                    objPenalRateConfigUpd.ListPosCode.AddRange(listPosCodes);
                    objPenalRateConfigUpd.ListPosCodeChoice = objParentIntRateConfigFind.ListPosCode;

                    string[] listProductCodes = Utilities.Splip_Strings(objParentIntRateConfigFind.ProductCodeList, ";");
                    objPenalRateConfigUpd.ListProductCode = new List<string>();
                    objPenalRateConfigUpd.ListProductCode.AddRange(listProductCodes);

                    if (string.IsNullOrEmpty(objPenalRateConfigUpd.StatusText))
                        objPenalRateConfigUpd.StatusText = "";
                    sNameView = "_DetailTidePenalRates";
                }
                #endregion
            }
            TempData["UserName"] = UserName;
            TempData["FlagCall"] = pFlagCall;
            TempData["UserPosCode"] = UserPosCode;
            return PartialView(sNameView, objPenalRateConfigUpd);
        }

        /// <summary>
        /// Hàm lấy danh sách lãi suất rút trước hạn của sản phẩm tiền gửi có kỳ hạn ra lưới DL để nhập Quyết định LS mới
        /// </summary>
        /// <param name="request">Thông tin cầu tìm kiếm</param>
        /// <returns>Danh sách lãi suất rút trước hạn theo sản phẩm</returns>
        [HttpPost]
        public async Task<IActionResult> LoadGridData_UpdateTidePenalRateConfig([DataSourceRequest] DataSourceRequest request, string pListPosCode, string pListProductCodeChoice)
        {
            var posCode = Request.Form["PosCode"].ToString();
            var productCode = Request.Form["ProductCode"].ToString();
            var currencyCode = Request.Form["CurrencyCode"].ToString();     //ConstValueAPI.CurrencyValueDefault
            var effectiveDate = Request.Form["EffectiveDate"].ToString();
            var idListIntPenalRateConfig = Request.Form["IdList"].ToString();

            string sExpiredDateTmp = Request.Form["ExpiredDate"].ToString();
            string sCircularRefNumTmp = Request.Form["CircularRefNum"].ToString();
            string sCircularDateTmp = Request.Form["CircularDate"].ToString();
            decimal dInterestRateNewTmp = decimal.Parse(Request.Form["InterestRateNew"].ToString());
            decimal dPenalIntRateTmp = decimal.Parse(Request.Form["PenalIntRate"].ToString());
            if (string.IsNullOrEmpty(pListPosCode))
                pListPosCode = "";
            string[] listPosCodes = Utilities.Splip_Strings(pListPosCode, ";");

            if (string.IsNullOrEmpty(pListProductCodeChoice))
                pListProductCodeChoice = "";
            string[] listProductCodes = Utilities.Splip_Strings(pListProductCodeChoice, ";");

            if (posCode == PosValue.HEAD_POS)
            {
                if (listPosCodes == null || listPosCodes.Length <= 0)
                    listPosCodes[0] = "0";
                posCode = "0";
            }
            try
            {
                DateTime dExpiredDateTmp = CustConverter.StringToDate(sExpiredDateTmp, FormatParameters.FORMAT_DATE);
                DateTime dCircularDateTmp = CustConverter.StringToDate(sCircularDateTmp, FormatParameters.FORMAT_DATE);

                DateTime dEffectDate = CustConverter.StringToDate(effectiveDate, FormatParameters.FORMAT_DATE);
                List<UpdateTidePenalRateConfigViewModel> listResult = new List<UpdateTidePenalRateConfigViewModel>();
                
                var posCodeList = new List<String>();
                if (listPosCodes != null && listPosCodes.Length != 0)
                    posCodeList.AddRange(listPosCodes);
                
                foreach (var itemPos in posCodeList)
                {                    
                    List<UpdateTidePenalRateConfigViewModel> listResultTemp = new List<UpdateTidePenalRateConfigViewModel>();

                    string sPosCodeTemp = itemPos.Trim();
                    if (!string.IsNullOrEmpty(idListIntPenalRateConfig))
                    {
                        listResultTemp = await _intRateConfigService.GetListDepPenalIntRate(sPosCodeTemp, "", currencyCode, dEffectDate,
                                        dEffectDate, sCircularRefNumTmp, dCircularDateTmp.Date, idListIntPenalRateConfig);
                    }
                    else
                    {
                        listResultTemp = await _intRateConfigService.GetListDepPenalIntRate(sPosCodeTemp, "", currencyCode, dEffectDate,
                                                        dExpiredDateTmp, sCircularRefNumTmp, dCircularDateTmp, dInterestRateNewTmp, dPenalIntRateTmp);
                    }
                    if (listResultTemp != null && listResultTemp.Count != 0)
                    {
                        if (listProductCodes != null && listProductCodes.Length > 0)
                        {
                            var listResultTemp01 = listResultTemp.Where(w => listProductCodes.Contains(w.ProductCode)).ToList();
                            listResult.AddRange(listResultTemp01);
                        }
                        else
                            listResult.AddRange(listResultTemp);
                    }
                }
                var result = listResult.ToDataSourceResult(request, ModelState);
                return Json(result);
            }
            catch (Exception ex)
            {
                WriteLog(LogType.ERROR, $"Error calling API or processing data LoadGridData_UpdateTidePenalRateConfig() => Error: {ex.Message}");
                return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
            }
        }

        /// <summary>
        /// Hàm thực hiện lưu dữ liệu (Thêm/Sửa) cấu hình lãi suất rút trước hạn tiền gửi có kỳ hạn. Cập nhật thông tin vào bảng InterestRateConfigMaster
        /// </summary>
        /// <returns>Thành công hoặc Thất bại</returns>
        public async Task<IActionResult> SaveTidePenalRateConfig()
        {
            string sCheckInfo = "", body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            if (string.IsNullOrEmpty(body))
            {
                return BadRequest("Dữ liệu cấu hình lãi suất rút trước hạn tiền gửi có kỳ hạn bị trống. Vui lòng kiểm tra lại!");
            }
            try
            {
                // Manually deserialize the JSON
                var requestUpd = JsonSerializer.Deserialize<SaveTidePenalRateConfigRequest>(body);
                if (requestUpd == null)
                {
                    return BadRequest("Không có dữ liệu cấu hình lãi suất rút trước hạn theo sản phẩm cần cập nhật.");
                }
                if (requestUpd.ListItemUpdates == null || !requestUpd.ListItemUpdates.Any() || requestUpd.ListItemUpdates.Count <= 0)
                {
                    return BadRequest("Không có danh sách cấu hình lãi suất rút trước hạn theo sản phẩm cần cập nhật.");
                }
                if (string.IsNullOrEmpty(requestUpd.CircularRefNum))
                {
                    return BadRequest("Bạn chưa nhập số và tiêu đề quyết định cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH. Vui lòng kiểm tra lại!");
                }
                DateTime dEffectiveDateTmp = CustConverter.StringToDate(requestUpd.EffectiveDate, FormatParameters.FORMAT_DATE);
                DateTime dCircularDateTmp = CustConverter.StringToDate(requestUpd.CircularDate, FormatParameters.FORMAT_DATE);
                DateTime dExpiredDateTmp = CustConverter.StringToDate(requestUpd.ExpiredDate, FormatParameters.FORMAT_DATE);

                string listPosAplly = string.IsNullOrEmpty(requestUpd.ListPosCodeChoice) ? "" : requestUpd.ListPosCodeChoice;
                string[] listPosCodes = Utilities.Splip_Strings(listPosAplly, ";");

                string listProductAplly = string.IsNullOrEmpty(requestUpd.ListProductCodeChoice) ? "" : requestUpd.ListProductCodeChoice;
                string[] listProductCodes = Utilities.Splip_Strings(listProductAplly, ";");
                if (dEffectiveDateTmp < dCircularDateTmp)
                {
                    return BadRequest($"Ngày hiệu lực [{requestUpd.EffectiveDate}] quyết định thay đổi lãi suất nhỏ hơn ngày ký [{requestUpd.CircularDate}] quyết định. Vui lòng kiểm tra lại!");
                }
                if (dExpiredDateTmp <= dEffectiveDateTmp)
                {
                    return BadRequest($"Thông tin quyết định thay đổi lãi suất có Ngày hết hiệu [{requestUpd.ExpiredDate}] lực nhỏ hơn hoặc bằng ngày hiệu lực [{requestUpd.EffectiveDate}]. Vui lòng kiểm tra lại!");
                }
                var sSessionId = HttpContext.Session.Id;
                string sUserNameUpdate = UserName;
                List<InterestRateConfigMaster> listInterestRateConfigMasterUpds = new List<InterestRateConfigMaster>();
                foreach (var itemData in requestUpd.ListItemUpdates)
                {
                    InterestRateConfigMaster itemUpdAdd = new InterestRateConfigMaster();
                    itemUpdAdd.Id = itemData.Id;
                    itemUpdAdd.PosCode = itemData.PosCode;
                    itemUpdAdd.PosName = itemData.PosName;
                    itemUpdAdd.ProductGroupCode = ProductGroupCode.ProductGroupCode_DepositPenal;
                    itemUpdAdd.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                    itemUpdAdd.CircularDate = dCircularDateTmp.Date;
                    itemUpdAdd.CircularRefNum = requestUpd.CircularRefNum;
                    itemUpdAdd.RecordSerialNo = itemData.OrderNo;
                    itemUpdAdd.ProductCode = itemData.ProductCode;
                    itemUpdAdd.ProductName = itemData.ProductName;
                    itemUpdAdd.AccountTypeCode = string.IsNullOrEmpty(itemData.AccountTypeCode) ? "" : itemData.AccountTypeCode;
                    itemUpdAdd.AccountTypeName = string.IsNullOrEmpty(itemData.AccountTypeName) ? "" : itemData.AccountTypeName;
                    itemUpdAdd.AccountSubTypeCode = "";
                    itemUpdAdd.AccountSubTypeName = "";
                    itemUpdAdd.CurrencyCode = string.IsNullOrEmpty(requestUpd.CurrencyCode) ? itemData.CurrencyCode : requestUpd.CurrencyCode;
                    itemUpdAdd.EffectiveDate = dEffectiveDateTmp;
                    itemUpdAdd.ExpiryDate = dExpiredDateTmp;
                    itemUpdAdd.DebitCreditFlag = ConstValueAPI.DebitCreditFlagDefault;
                    itemUpdAdd.InterestRate = itemData.InterestRate;
                    itemUpdAdd.NewInterestRate = itemData.InterestRateNew;
                    itemUpdAdd.PenalRate = requestUpd.PenalIntRate;
                    itemUpdAdd.AmountSlab = 0;
                    itemUpdAdd.TenorSerialNo = 1;
                    itemUpdAdd.IntRateType = "";
                    itemUpdAdd.SpreadRate = 0;
                    itemUpdAdd.Remark = "";
                    itemUpdAdd.OrtherNotes = $"{UserName} | {sSessionId} | {itemUpdAdd.CircularRefNum} | {dEffectiveDateTmp.ToString(FormatParameters.FORMAT_DATE_INT)} | {itemData.ProductCode} | {itemData.InterestRate.ToString()} | {itemData.InterestRateNew.ToString()}";
                    itemUpdAdd.Status = itemData.Status;
                    itemUpdAdd.StatusUpdateCore = itemData.StatusUpdateCore;
                    itemUpdAdd.CallApiTxnStatus = "";
                    itemUpdAdd.CallApiReqRecordSl = 0;
                    itemUpdAdd.CallApiResponseCode = "";
                    itemUpdAdd.CallApiResponseMsg = "";
                    itemUpdAdd.CreatedBy = UserName;
                    itemUpdAdd.CreatedDate = DateTime.Now;
                    itemUpdAdd.ModifiedBy = UserName;
                    itemUpdAdd.ModifiedDate = DateTime.Now;
                    itemUpdAdd.ApproverBy = UserName;
                    itemUpdAdd.ApprovalDate = DateTime.Now;
                    itemUpdAdd.DocumentId = 0;
                    decimal dMaxInterestRateSpread = itemData.MaxInterestRateSpread;
                    decimal dMinInterestRateSpread = itemData.MinInterestRateSpread;
                    decimal dInterestRateHO = itemData.InterestRateHO;
                    var dMinRate = dInterestRateHO - dMinInterestRateSpread;
                    var dMaxRate = dInterestRateHO + dMaxInterestRateSpread;
                    if (itemUpdAdd.InterestRate < dMinRate || itemUpdAdd.InterestRate > dMaxRate)
                    {
                        sCheckInfo = $"Sản phẩm {itemUpdAdd.ProductCode} cấu hình lãi suất mới phải nằm trong khoảng từ LS tối thiểu {dMinRate.ToString()} đến LS tối đa {dMaxRate.ToString()}";
                        break;
                    }
                    listInterestRateConfigMasterUpds.Add(itemUpdAdd);
                }
                if (!string.IsNullOrEmpty(sCheckInfo))
                {
                    return BadRequest($"{sCheckInfo}. Vui lòng kiểm tra lại!");
                }
                var iCountUpdateRet = await _intRateConfigService.SaveInterestRateConfigMaster(listInterestRateConfigMasterUpds, sUserNameUpdate, requestUpd.FlagCall);

                if (iCountUpdateRet <= 0)
                {
                    return StatusCode(500, "Lỗi cập nhật cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH");
                }
                return Ok(new { Message = $"Bạn đã cập nhật thành công quyết định [{requestUpd.CircularRefNum}] lãi suất rút trước hạn của sản phẩm tiền gửi CKH (Có {iCountUpdateRet.ToString()} bản ghi được cập nhật)!" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi khi lưu cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH vào cơ sở dữ liệu. Chi tiết: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu dữ liệu cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH. Chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm thực hiện đánh dấu xóa dữ liệu cấu hình lãi suất rút trước hạn sản phẩm tiền gửi CKH
        /// </summary>
        /// <param name="request">Thông tin đầu vào. Danh sách IdList Id của bảng InterestRateConfigMaster cần cập nhật trạng thái là đánh dấu xóa 0</param>
        /// <returns>Thành công hoặc thất bại</returns>
        public async Task<IActionResult> DeleteIntPenalRateConfig([FromBody] DeleteInterestRateConfigRequest request)
        {
            bool bIsDelete = false;
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Thông tin cần xóa không hợp lệ, không thấy quyết định cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH cần xóa. Vui lòng kiểm tra lại" });
            }
            if (string.IsNullOrEmpty(request.IdList) && request.Id == 0 && request.DocumentId.Value == 0)
            {
                return BadRequest(new { success = false, message = "Thông tin cần xóa không hợp lệ, không thấy quyết định cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH cần xóa. Vui lòng kiểm tra lại" });
            }
            try
            {
                if (!string.IsNullOrEmpty(request.IdList))
                {
                    string listIdDelete = request.IdList;
                    bIsDelete = await _intRateConfigService.DeleteInterestRateConfigMaster(ProductGroupCode.ProductGroupCode_DepositPenal, 0, 0, listIdDelete, UserName, 2);
                }
                else if (request.Id != 0)
                {
                    bIsDelete = await _intRateConfigService.DeleteInterestRateConfigMaster(ProductGroupCode.ProductGroupCode_DepositPenal, 0, request.Id, "", UserName, 2);
                }
                else if (request.DocumentId != 0)
                {
                    bIsDelete = await _intRateConfigService.DeleteInterestRateConfigMaster(ProductGroupCode.ProductGroupCode_DepositPenal, request.DocumentId.Value, 0, "", UserName, 2);
                }
                if (bIsDelete)
                    return Ok(new { success = true, message = "Xóa quyết định cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH thành công!" });
                else return NotFound(new { success = false, message = "Xóa quyết định cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH không thành công" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật cơ sở dữ liệu. Chi tiết lỗi: {ex.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi xóa cấu hình lãi suất rút trước hạn của sản phẩm tiền gửi CKH. Chi tiết lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Hàm thực hiện cập nhật Trình Duyệt thay đổi LS rút trước hạn sản phẩm tiền gửi CKH: 
        ///         - Cập nhật Bản ghi (Thêm/Sửa) Vào bảng AttachedFileInfo
        ///         - Thực hiện xóa file đính kèm trước đó nếu có.
        ///         - Upload file đính kèm đã chọn vào thư mục wwwroot\Uploads\ToTrinh\
        /// </summary>
        /// <param name="request">Thông tin</param>
        /// <param name="objApprovalInfor">Thông tin cập nhật AttachedFileInfo</param>
        /// <param name="fileUpload">File upload đính kèm</param>
        /// <returns>Thành công hoặc Thất bại</returns>
        [HttpPost]
        public async Task<IActionResult> SaveApprovalTidePenalRateConfig([DataSourceRequest] DataSourceRequest request, UpdateTidePenalRateConfigViewModel objApprovalInfor,
                        IFormFile fileUpload)
        {
            try
            {
                if (objApprovalInfor == null || !ModelState.IsValid)
                {
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    ModelState.AddModelError("fileUpload", "Bạn chưa chọn tệp tin tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn đính kèm. Vui lòng kiểm tra lại!");
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                var sExtensionTmp = Path.GetExtension(fileUpload.FileName).ToLower();
                if (sExtensionTmp != ".pdf")
                {
                    ModelState.AddModelError("pFileUpload", "Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn đính kèm không phải file định dạng .PDF. Vui lòng kiểm tra lại!");
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                if (fileUpload.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("pFileUpload", "Tờ trình cấu hình lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn đính kèm không đúng định dạng PDF. Vui lòng kiểm tra lại!");
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                // Kiểm tra dung lượng (ví dụ: 5MB)
                long maxSize = 5 * 1024 * 1024;
                if (fileUpload.Length > maxSize)
                {
                    ModelState.AddModelError("pFileUpload", "Dung lượng file tờ trình đính kèm vượt quá dung lượng quy định (Tối đa là 5MB). Vui lòng kiểm tra lại!");
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                string sPathFileUpload = Common.UploadDirFileDocument.Replace("~", "").Replace("/", @"\") + @"\";
                var sUploadPathTemp = Path.Combine(Directory.GetCurrentDirectory(), sPathFileUpload, "ToTrinh");

                List<AttachedFileInfo> listFileUpload = new List<AttachedFileInfo>();
                AttachedFileInfo objFileInfo = new AttachedFileInfo();
                if (objApprovalInfor.DocumentId != 0)
                {
                    var listAttachedFile = _intRateConfigService.GetListAttachedFileInfoSearch(0, objApprovalInfor.DocumentId, "1", "", "").FirstOrDefault();
                    if (listAttachedFile != null && listAttachedFile.FileId != 0)
                    {
                        objFileInfo.FileId = listAttachedFile.FileId;
                        objFileInfo.DocumentId = listAttachedFile.DocumentId;
                        objFileInfo.FileType = FileType.FileType_ConfigIntRate.Value.ToString();
                        objFileInfo.FileName = fileUpload.FileName;
                        objFileInfo.FileExtension = Path.GetExtension(fileUpload.FileName);
                        objFileInfo.PathFile = Path.Combine(sPathFileUpload, "ToTrinh") + @"\";
                        if (!string.IsNullOrEmpty(listAttachedFile.FileNameNew))
                        {
                            if (listAttachedFile.FileNameNew.Contains(listAttachedFile.FileExtension))
                                objFileInfo.FileNameNew = string.IsNullOrEmpty(listAttachedFile.FileNameNew) ? $"{Guid.NewGuid()}.pdf" : $"{listAttachedFile.FileNameNew}";
                            else objFileInfo.FileNameNew = string.IsNullOrEmpty(listAttachedFile.FileNameNew) ? $"{Guid.NewGuid()}.pdf" : $"{listAttachedFile.FileNameNew}{listAttachedFile.FileExtension}";
                        }    
                        else
                            objFileInfo.FileNameNew = string.IsNullOrEmpty(listAttachedFile.FileNameNew) ? $"{Guid.NewGuid()}.pdf" : $"{listAttachedFile.FileNameNew}{listAttachedFile.FileExtension}";

                        objFileInfo.DocumentNumber = objApprovalInfor.CircularRefNum;
                        objFileInfo.CircularRefNum = objApprovalInfor.CircularRefNum;
                        objFileInfo.ContentDescription = $"{objApprovalInfor.CircularRefNum} ~ {objApprovalInfor.CircularDate.ToString(FormatParameters.FORMAT_DATE)} ~ {objApprovalInfor.EffectiveDate.ToString(FormatParameters.FORMAT_DATE)} ~ {objApprovalInfor.ProductList} ";
                        objFileInfo.Status = StatusTrans.Status_Modified.Value;
                        objFileInfo.ModifiedBy = UserName;
                        objFileInfo.ModifiedDate = DateTime.Now;
                        listFileUpload.Add(objFileInfo);
                    }
                }
                else
                {
                    objFileInfo.FileId = 0;
                    objFileInfo.DocumentId = 0;
                    objFileInfo.FileType = FileType.FileType_ConfigIntRate.Value.ToString();
                    objFileInfo.FileName = fileUpload.FileName;
                    objFileInfo.FileExtension = Path.GetExtension(fileUpload.FileName);
                    objFileInfo.PathFile = Path.Combine(sPathFileUpload, "ToTrinh") + @"\";
                    objFileInfo.FileNameNew = $"{Guid.NewGuid()}.pdf";
                    objFileInfo.DocumentNumber = objApprovalInfor.CircularRefNum;
                    objFileInfo.CircularRefNum = objApprovalInfor.CircularRefNum;
                    objFileInfo.ContentDescription = $"{objApprovalInfor.CircularRefNum} ~ {objApprovalInfor.CircularDate.ToString(FormatParameters.FORMAT_DATE)} ~ {objApprovalInfor.EffectiveDate.ToString(FormatParameters.FORMAT_DATE)} ~ {objApprovalInfor.ProductList} ";
                    objFileInfo.Status = StatusTrans.Status_Created.Value;
                    objFileInfo.CreatedBy = UserName;
                    objFileInfo.CreatedDate = DateTime.Now;
                    objFileInfo.ModifiedBy = UserName;
                    objFileInfo.ModifiedDate = DateTime.Now;
                    objFileInfo.ApproverBy = UserName;
                    objFileInfo.ApprovalDate = DateTime.Now;
                    listFileUpload.Add(objFileInfo);
                }
                var listIdAttachFileUpd = await _intRateConfigService.SaveAttachedFileInfo(objApprovalInfor.DocumentId, listFileUpload, FileType.FileType_ConfigIntRate.Value.ToString(),
                                                    ProductGroupCode.ProductGroupCode_DepositPenal, UserName);
                long iDocumentIdUpd = 0;
                int iResultUpdateDocumentId = 0;
                if (listIdAttachFileUpd != null)
                {
                    if (!Directory.Exists(sUploadPathTemp))
                    {
                        Directory.CreateDirectory(sUploadPathTemp);
                    }
                    foreach (var itemFileId in listIdAttachFileUpd)
                    {
                        var listAttachedFile = _intRateConfigService.GetListAttachedFileInfoSearch(itemFileId, 0, "1", "", "").FirstOrDefault();
                        string fileNameNew = "";
                        if (listAttachedFile.FileNameNew.Contains(listAttachedFile.FileExtension))
                            fileNameNew = $"{listAttachedFile.FileNameNew}";
                        else
                            fileNameNew = $"{listAttachedFile.FileNameNew}{listAttachedFile.FileExtension}";

                        var filePathNew = Path.Combine(sUploadPathTemp, fileNameNew);
                        iDocumentIdUpd = listAttachedFile.DocumentId;
                        using (var stream = new FileStream(filePathNew, FileMode.Create))
                        {
                            fileUpload.CopyTo(stream);
                        }
                    }
                    //Thực hiện cập nhật DocumentId vào bảng InterestRateConfigMaster
                    var lstId = StringHelper.ConvertToLongList(objApprovalInfor.IdList, ';');
                    iResultUpdateDocumentId = await _intRateConfigService.UpdateInterestRateConfigMasterStatus(UserName, lstId, ConfigStatus.PROCESS.Value, iDocumentIdUpd);
                }
                if (iResultUpdateDocumentId > 0)
                {
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }
                else
                {
                    WriteLog(LogType.ERROR, $"SaveApprovalTidePenalRateConfig - Failed to save approval for CircularRefNum: {objApprovalInfor.CircularRefNum}");
                    ModelState.AddModelError("ERROR", "Lưu phê duyệt thất bại.");
                    return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
                }

            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                ModelState.AddModelError("ERROR", $"{e.Message}");
                return Json(new[] { objApprovalInfor }.ToDataSourceResult(request, ModelState));
            }

        }

        /// <summary>
        /// Hàm thực hiện mở file đính kèm trên trình duyệt (File tờ trình, quyết định,....)
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định Văn bản/Tài liệu có file đính kèm</param>
        /// <param name="pFileId">Chỉ số xác định file đính kèm</param>
        /// <param name="pFileName">Tên file (FileNameNew)</param>
        /// <returns>File cần mở</returns>
        public ActionResult LoadPdfAttachedFile(long pDocumentId, long pFileId, string pFileName)
        {
            string sFileNameNew = "", filePath = "";
            string sPathFileUpload = Common.UploadDirFileDocument.Replace("~", "").Replace("/", @"\") + @"\";
            var sUploadPathTemp = Path.Combine(Directory.GetCurrentDirectory(), sPathFileUpload, "ToTrinh");

            if (pFileId != 0 || pDocumentId != 0)
            {
                var objFileInfo = _intRateConfigService.GetListAttachedFileInfoSearch(pFileId, pDocumentId, FileType.FileType_ConfigIntRate.Value.ToString(), "", "").FirstOrDefault();
                if (objFileInfo != null && !string.IsNullOrEmpty(objFileInfo.FileNameNew))
                {
                    sUploadPathTemp = Path.Combine(Directory.GetCurrentDirectory(), objFileInfo.PathFile, "");
                    if (objFileInfo.FileNameNew.Contains(objFileInfo.FileExtension))
                        filePath = string.Format("{0}/{1}", sUploadPathTemp, $"{objFileInfo.FileNameNew}");
                    else filePath = string.Format("{0}/{1}", sUploadPathTemp, $"{objFileInfo.FileNameNew}{objFileInfo.FileExtension}");
                }
            }
            else
            {
                sFileNameNew = pFileName;
                filePath = string.Format("{0}/{1}", sUploadPathTemp, pFileName);
            }
            if (System.IO.File.Exists(filePath))
            {
                if (filePath.ToUpper().Contains(".PDF"))
                {
                    if (filePath.ToUpper().Contains(".PDF"))
                    {
                        byte[] pdfByte = FilesUtils.GetBytesFromFile(filePath);
                        return File(pdfByte, "application/pdf");
                    }
                }
                else
                {
                    TempData["OK"] = "0";
                    return View("_PdfContainer");
                }
            }
            else TempData["OK"] = "2";
            return View("_PdfContainer");
        }



        [HttpPost]
        public async Task<IActionResult> SaveAuthorizeTidePenalRateConfig([DataSourceRequest] DataSourceRequest request, 
                                                        UpdateTidePenalRateConfigViewModel pAuthorizeTidePenalRateConfigUpd)
        {
            try
            {
                if (pAuthorizeTidePenalRateConfigUpd != null && ModelState.IsValid)
                {
                    string sMessageTemp = "";
                    sMessageTemp = (pAuthorizeTidePenalRateConfigUpd.RejectFlag == 1) ? "Từ chối" : "Phê duyệt";
                    var listIdUpd = StringHelper.ConvertToLongList(pAuthorizeTidePenalRateConfigUpd.IdList, ';');
                    var status = await _intRateConfigService.SaveAuthorizeOrRejectTidePenalRateConfig(pAuthorizeTidePenalRateConfigUpd.DocumentId,UserName,
                                            listIdUpd, pAuthorizeTidePenalRateConfigUpd.RejectFlag, pAuthorizeTidePenalRateConfigUpd.RejectReason);
                    if (status > 0)
                    {
                        return Json(new[] { pAuthorizeTidePenalRateConfigUpd }.ToDataSourceResult(request, ModelState));
                    }
                    else
                    {
                        ModelState.AddModelError("ERROR", $"Cập nhật {sMessageTemp} quyết định thay đổi lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn thất bại.");
                        return Json(new[] { pAuthorizeTidePenalRateConfigUpd }.ToDataSourceResult(request, ModelState));
                    }
                }
                else
                {
                    ModelState.AddModelError("ERROR", "Không có dữ liệu về quyết định thay đổi lãi suất rút trước hạn sản phẩm tiền gửi có kỳ hạn để Phê duyệt/Từ chối. Vui lòng kiểm tra lại");
                    return Json(new[] { pAuthorizeTidePenalRateConfigUpd }.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                ModelState.AddModelError("ERROR", $"{e.Message}");
                return Json(new[] { pAuthorizeTidePenalRateConfigUpd }.ToDataSourceResult(request, ModelState));
            }

        }

        ////ShowApprovalTidePenalRate
        //[HttpGet]
        //public async Task<IActionResult> ShowApprovalTidePenalRate(int pId, string pCircularRefNum, string pIdList, string pFlagCall)
        //{
        //    var model = await _interestRateConfigureService.GetTideInterestRateDetailViews(circularRefNum);
        //    return PartialView("_Approval", model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> LoadTidePenalRateAddGridDataUpdate([DataSourceRequest] DataSourceRequest requestLoad)
        //{
        //    var productCode = Request.Form["productCode"].ToString();
        //    var accountTypeCode = Request.Form["accountType"].ToString(); //             
        //    var accountSubTypeCode = Request.Form["accountSubType"].ToString(); // 
        //    var effectiveDate = Request.Form["effectiveDate"].ToString(); //    
        //    var posCode = Request.Form["posCode"].ToString(); //    

        //    //Chinh sua neu la head pos thi truyen ma la 0
        //    if (posCode == PosValue.HEAD_POS)
        //    {
        //        posCode = "0";
        //    }

        //    //if (string.IsNullOrEmpty(productCode))
        //    //{
        //    //    return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
        //    //}

        //    try
        //    {
        //        DateTime dEffectDate = new DateTime(2025, 10, 01);
        //        var listResult = await _intRateConfigService.GetListDepPenalIntRate("0", "0", "VND", dEffectDate);
        //        // var models = await _interestRateConfigureService.GetTideProdList(posCode, productCode, DateTime.Now);
        //        //var existingRecords = await _interestRateConfigureService.GetInterestRateConfigMasterListAsync("", "", productCode, "", null, null);
        //        //foreach (var model in models)
        //        //{
        //        //    var matchingRecord = existingRecords.FirstOrDefault(r => r.ProductCode == model.ProductCode &&
        //        //                                                           r.AccountTypeCode == model.AccountTypeCode &&
        //        //                                                           r.EffectiveDate == model.EffectiveDate);
        //        //    if (matchingRecord != null)
        //        //    {
        //        //        model.Id = matchingRecord.Id;
        //        //    }
        //        //}
        //        //var result = models.ToDataSourceResult(request, ModelState);
        //        //return Json(result);
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(LogType.ERROR, $"Error calling API or processing data: {ex.Message}");
        //        return Json(new DataSourceResult { Data = new List<AddCasaProductViewModel>(), Total = 0 });
        //    }
        //}

    }
}
