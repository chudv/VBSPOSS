using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.NetworkInformation;
using VBSPOSS.Constants;
using VBSPOSS.Controllers;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

public class PosRepresentativeController : BaseController
{
    private readonly IPosRepresentativeService _service;

    private readonly ILogger<ListOfValueController> _logger;

    private readonly IListOfValueService _serviceLOV;

    private readonly IApiInternalService _internalServiceAPI;

    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PosRepresentativeController"/> class.
    /// </summary>
    /// <param name="serviceLOV">The service<see cref="IListOfValueService"/>.</param>
    /// <param name="logger">The logger<see cref="ILogger{ListController}"/>.</param>
    /// <param name="menuService">The menuService<see cref="IPermitService"/>.</param>
    public PosRepresentativeController(ILogger<BaseController> logger, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper,
                             IMapper mapper, IPosRepresentativeService service, IApiInternalService internalServiceAPI) : base(logger, adminService, sessionHelper)
    {
        _serviceLOV = serviceLOV;
        _service = service;
        _internalServiceAPI = internalServiceAPI;
        _mapper = mapper;
    }

    public ActionResult Index()
    {
        return View(Index);
    }

    public ActionResult IndexPosRepresentative()
    {
        return View(IndexPosRepresentative);
    }

    /// <summary>
    /// Hàm lấy danh sách lên lưới dữ liệu Danh sách người đại diện các đơn vị
    /// </summary>
    /// <param name="request"></param>
    /// <param name="pPosCode">Mã đơn vị</param>
    /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
    /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
    /// <returns>Danh sách người đại diện các đơn vị</returns>
    public ActionResult LoadGridData_PosRepresentative([DataSourceRequest] DataSourceRequest request, string pPosCode, string pFromEffectiveDate, string pToEffectiveDate, string pStaffId, int pStatus,string pStaffName)
    {
        try
        {
            //if (string.IsNullOrEmpty(pPosCode))
            //{
            //    // Nếu user thuộc MainPosCode (ví dụ: 000100), lấy tất cả PosCode thuộc MainPosCode đó
            //    // Nếu user thuộc PosCode thường, chỉ lấy PosCode của user
            //    pPosCode = ""; // Để trống để lấy theo logic trong Service
            //}
            if (string.IsNullOrEmpty(pPosCode))
                pPosCode = (UserPosCode == "000100") ? "" : UserPosCode;
            if (string.IsNullOrEmpty(pStaffId))
                pStaffId = "";
            //int iFromEffectiveDate = 0, ToEffectiveDate = 0;
            //iFromEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pFromEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));
            //ToEffectiveDate = Convert.ToInt32(CustConverter.StringToDate(pToEffectiveDate.ToString(), FormatParameters.FORMAT_DATE).ToString(FormatParameters.FORMAT_DATE_INT));

            var listStaffVBSP = _service.GetPosRepresentativeList(pPosCode, pStaffId, pStaffName, pStatus);

            return Json(listStaffVBSP.ToDataSourceResult(request, ModelState));
        }
        catch (Exception ex)
        {
            return Json(new { Errors = "Có lỗi xảy ra khi lấy danh sách người đại diện các đơn vị." });
        }
    }


    /// <summary>
    /// Hàm show màn hình update người đại diện
    /// </summary>
    /// <param name="request"></param>
    /// <param name="pPosCode">Mã đơn vị</param>
    /// <param name="pFromEffectiveDate">Ngày HL bắt đầu. Định dạng dd/MM/yyyy</param>
    /// <param name="pToEffectiveDate">Ngày HL kết thúc. Định dạng dd/MM/yyyy</param>
    /// <returns>Danh sách người đại diện các đơn vị</returns>
    public ActionResult ShowUpdatePosRepresentative(string pPosCode, string pStaffId, string pFlagCall)
    {
        PosRepresentativeViewModel objPosRepresentativeUpd = new PosRepresentativeViewModel();
        if (string.IsNullOrEmpty(pPosCode))
            pPosCode = "";
        if (string.IsNullOrEmpty(pStaffId))
            pStaffId = "";
        string sNameView = "";
        var listStaffVBSP = (_service.GetPosRepresentativeList(pPosCode, pStaffId,"", 1)).FirstOrDefault();
        if (pFlagCall == "1" && listStaffVBSP == null)
        {
            objPosRepresentativeUpd.Id = 0;
            objPosRepresentativeUpd.RepresentativeType = "";
            objPosRepresentativeUpd.RepresentativeTypeDesc = "";
            objPosRepresentativeUpd.RepresentativeTypeText = "";
            objPosRepresentativeUpd.MainPosCode = "";
            objPosRepresentativeUpd.MainPosName = "";
            objPosRepresentativeUpd.PosCode = "";
            objPosRepresentativeUpd.PosName = "";
            objPosRepresentativeUpd.PosMobileNo = "";
            objPosRepresentativeUpd.PosFaxNo = "";
            objPosRepresentativeUpd.PosAddressFull = "";
            objPosRepresentativeUpd.PosAddressLine = "";
            objPosRepresentativeUpd.PosSubCommune = "";
            objPosRepresentativeUpd.PosSubCommuneText = "";
            objPosRepresentativeUpd.PosCommune = "";
            objPosRepresentativeUpd.PosCommuneText = "";
            objPosRepresentativeUpd.PosDistrict = "";
            objPosRepresentativeUpd.PosDistrictText = "";
            objPosRepresentativeUpd.PosProvince = "";
            objPosRepresentativeUpd.PosProvinceText = "";
            objPosRepresentativeUpd.PosSubCommuneId = "";
            objPosRepresentativeUpd.StaffId = "";
            objPosRepresentativeUpd.StaffCode = "";
            objPosRepresentativeUpd.StaffName = "";
            objPosRepresentativeUpd.DateOfBirth = DateTime.Now;
            objPosRepresentativeUpd.DateOfBirthText = "";
            objPosRepresentativeUpd.Genders = "";
            objPosRepresentativeUpd.GendersText = "";
            objPosRepresentativeUpd.StaffPosCode = "";
            objPosRepresentativeUpd.StaffPosName = "";
            objPosRepresentativeUpd.StaffDepartmentCode = "";
            objPosRepresentativeUpd.StaffDepartmentName = "";
            objPosRepresentativeUpd.StaffPositionCode = "";
            objPosRepresentativeUpd.StaffPositionName = "";
            objPosRepresentativeUpd.StaffMobileNo = "";
            objPosRepresentativeUpd.StaffEmail = "";
            objPosRepresentativeUpd.EffectDate = DateTime.Now;
            objPosRepresentativeUpd.EffectDateText = "";
            objPosRepresentativeUpd.ExpireDate = DateTime.Now;
            objPosRepresentativeUpd.ExpireDateText = "";
            objPosRepresentativeUpd.DecisionNo = "";
            objPosRepresentativeUpd.DecisionTitle = "";
            objPosRepresentativeUpd.Status = 1;
            objPosRepresentativeUpd.StatusDesc = "";
            objPosRepresentativeUpd.StatusText = "";
            objPosRepresentativeUpd.Notes = "";
            objPosRepresentativeUpd.CreatedBy = "";
            objPosRepresentativeUpd.CreatedDate = DateTime.Now;
            objPosRepresentativeUpd.ModifiedBy = "";
            objPosRepresentativeUpd.ModifiedDate = DateTime.Now;
            objPosRepresentativeUpd.OrderNo = 0;
        }
        else
        {
            objPosRepresentativeUpd.Id = listStaffVBSP.Id;
            objPosRepresentativeUpd.RepresentativeType = listStaffVBSP.RepresentativeType;
            objPosRepresentativeUpd.RepresentativeTypeDesc = listStaffVBSP.RepresentativeTypeDesc;
            objPosRepresentativeUpd.RepresentativeTypeText = listStaffVBSP.RepresentativeTypeText;
            objPosRepresentativeUpd.MainPosCode = listStaffVBSP.MainPosCode;
            objPosRepresentativeUpd.MainPosName = listStaffVBSP.MainPosName;
            objPosRepresentativeUpd.PosCode = listStaffVBSP.PosCode;
            objPosRepresentativeUpd.PosName = listStaffVBSP.PosName;
            objPosRepresentativeUpd.PosMobileNo = listStaffVBSP.PosMobileNo;
            objPosRepresentativeUpd.PosFaxNo = listStaffVBSP.PosFaxNo;
            objPosRepresentativeUpd.PosAddressFull = listStaffVBSP.PosAddressFull;
            objPosRepresentativeUpd.PosAddressLine = listStaffVBSP.PosAddressLine;
            objPosRepresentativeUpd.PosSubCommune = listStaffVBSP.PosSubCommune;
            objPosRepresentativeUpd.PosSubCommuneText = listStaffVBSP.PosSubCommuneText;
            objPosRepresentativeUpd.PosCommune = listStaffVBSP.PosCommune;
            objPosRepresentativeUpd.PosCommuneText = listStaffVBSP.PosCommuneText;
            objPosRepresentativeUpd.PosDistrict = listStaffVBSP.PosDistrict;
            objPosRepresentativeUpd.PosDistrictText = listStaffVBSP.PosDistrictText;
            objPosRepresentativeUpd.PosProvince = listStaffVBSP.PosProvince;
            objPosRepresentativeUpd.PosProvinceText = listStaffVBSP.PosProvinceText;
            objPosRepresentativeUpd.PosSubCommuneId = listStaffVBSP.PosSubCommuneId;
            objPosRepresentativeUpd.StaffId = listStaffVBSP.StaffId;
            objPosRepresentativeUpd.StaffCode = listStaffVBSP.StaffCode;
            objPosRepresentativeUpd.StaffName = listStaffVBSP.StaffName;
            objPosRepresentativeUpd.DateOfBirth = listStaffVBSP.DateOfBirth;
            objPosRepresentativeUpd.DateOfBirthText = listStaffVBSP.DateOfBirthText;
            objPosRepresentativeUpd.Genders = listStaffVBSP.Genders;
            objPosRepresentativeUpd.GendersText = listStaffVBSP.GendersText;
            objPosRepresentativeUpd.StaffPosCode = listStaffVBSP.StaffPosCode;
            objPosRepresentativeUpd.StaffPosName = listStaffVBSP.StaffPosName;
            objPosRepresentativeUpd.StaffDepartmentCode = listStaffVBSP.StaffDepartmentCode;
            objPosRepresentativeUpd.StaffDepartmentName = listStaffVBSP.StaffDepartmentName;
            objPosRepresentativeUpd.StaffPositionCode = listStaffVBSP.StaffPositionCode;
            objPosRepresentativeUpd.StaffPositionName = listStaffVBSP.StaffPositionName;
            objPosRepresentativeUpd.StaffMobileNo = listStaffVBSP.StaffMobileNo;
            objPosRepresentativeUpd.StaffEmail = listStaffVBSP.StaffEmail;
            objPosRepresentativeUpd.EffectDate = listStaffVBSP.EffectDate;
            objPosRepresentativeUpd.EffectDateText = listStaffVBSP.EffectDateText;
            objPosRepresentativeUpd.ExpireDate = listStaffVBSP.ExpireDate;
            objPosRepresentativeUpd.ExpireDateText = listStaffVBSP.ExpireDateText;
            objPosRepresentativeUpd.DecisionNo = listStaffVBSP.DecisionNo;
            objPosRepresentativeUpd.DecisionTitle = listStaffVBSP.DecisionTitle;
            objPosRepresentativeUpd.Status = listStaffVBSP.Status;
            objPosRepresentativeUpd.StatusDesc = listStaffVBSP.StatusDesc;
            objPosRepresentativeUpd.StatusText = listStaffVBSP.StatusText;
            objPosRepresentativeUpd.Notes = listStaffVBSP.Notes;
            objPosRepresentativeUpd.CreatedBy = listStaffVBSP.CreatedBy;
            objPosRepresentativeUpd.CreatedDate = listStaffVBSP.CreatedDate;
            objPosRepresentativeUpd.ModifiedBy = listStaffVBSP.ModifiedBy;
            objPosRepresentativeUpd.ModifiedDate = listStaffVBSP.ModifiedDate;
            objPosRepresentativeUpd.OrderNo = listStaffVBSP.OrderNo;
        }
        sNameView = "UpdatePosRepresentative";
        TempData["FlagCall"] = pFlagCall;
        TempData["UserPosCode"] = UserPosCode;
        return PartialView(sNameView, objPosRepresentativeUpd);
    }

    /// <summary>
    /// Hàm thực hiện Xóa (Đóng) bản ghi thông tin phân giao cán bộ
    /// Nếu pLocalityId <> "" Thì cần bộ tham số pLocalityId,pExpireDate,pModifiedBy,pFlagDelete
    /// Nếu pLocalityId = ""  Thì cần bộ tham số pStaffId,pEffectDate,pExpireDate,pStatus,pCommuneCodeList,pModifiedBy,pFlagDelete
    /// </summary>
    [AcceptVerbs("Post")]
    public async Task<IActionResult> SaveUpdate([DataSourceRequest] DataSourceRequest request, PosRepresentativeViewModel objPosRepresentativeFull)
    {
        try
        {
            string result = "0";
            result = IsValidPosRepresentative(objPosRepresentativeFull).ToString();
            if (result == "0" && objPosRepresentativeFull != null && ModelState.IsValid)
            {
                objPosRepresentativeFull.PosCode = string.IsNullOrEmpty(objPosRepresentativeFull.PosCode) ? "" : objPosRepresentativeFull.PosCode;
                objPosRepresentativeFull.StaffCode = string.IsNullOrEmpty(objPosRepresentativeFull.StaffCode) ? "" : objPosRepresentativeFull.StaffCode;
                objPosRepresentativeFull.StaffPosCode = string.IsNullOrEmpty(objPosRepresentativeFull.StaffPosCode) ? "" : objPosRepresentativeFull.StaffPosCode;
                objPosRepresentativeFull.StaffDepartmentCode = string.IsNullOrEmpty(objPosRepresentativeFull.StaffDepartmentCode) ? "" : objPosRepresentativeFull.StaffDepartmentCode;
                objPosRepresentativeFull.StaffPositionCode = string.IsNullOrEmpty(objPosRepresentativeFull.StaffPositionCode) ? "" : objPosRepresentativeFull.StaffPositionCode;
                objPosRepresentativeFull.ExpireDate = objPosRepresentativeFull.ExpireDate.ToString(FormatParameters.FORMAT_DATE) == "01/01/0001" ? VBSPOSS.Transformations.Converters.StringToDateTime("31-12-2050", FormatParameters.FORMAT_DATE_VN) : objPosRepresentativeFull.ExpireDate;
                objPosRepresentativeFull.Notes = string.IsNullOrEmpty(objPosRepresentativeFull.Notes) ? "" : objPosRepresentativeFull.Notes;

                int iFlagCall = 1;           //1: Thêm mới; 2: Sửa đổi;
                string iVal = await _service.UpdatePosRepresentative(objPosRepresentativeFull, iFlagCall, UserName);
                result = (iVal != "") ? "0" : "1";
            }
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
            return new JsonResult("99");
        }
    }

    public int IsValidPosRepresentative(PosRepresentativeViewModel objPosRepresentativeFull)
    {
        int iResult = 0;
        try
        {
            if (string.IsNullOrEmpty(objPosRepresentativeFull.PosCode))
                return 1;
            if (string.IsNullOrEmpty(objPosRepresentativeFull.StaffCode))
                return 2;
            if (objPosRepresentativeFull.EffectDate?.ToString(FormatParameters.FORMAT_DATE) == objPosRepresentativeFull.ExpireDate.ToString(FormatParameters.FORMAT_DATE))
                return 3;
            if (objPosRepresentativeFull.EffectDate > objPosRepresentativeFull.ExpireDate && objPosRepresentativeFull.ExpireDate.ToString(FormatParameters.FORMAT_DATE) != "01/01/0001")
                return 4;
            return iResult;
        }
        catch
        {
            return 99;
        }
    }

    /// <summary>
    /// Hàm thực hiện Xóa (Đóng) bản ghi thông tin phân giao cán bộ
    /// Nếu pLocalityId <> "" Thì cần bộ tham số pLocalityId,pExpireDate,pModifiedBy,pFlagDelete
    /// Nếu pLocalityId = ""  Thì cần bộ tham số pStaffId,pEffectDate,pExpireDate,pStatus,pCommuneCodeList,pModifiedBy,pFlagDelete
    /// </summary>
    /// <param name="pLocalityId">Chỉ số khóa bản ghi</param>
    /// <param name="pStaffId">Id Cán bộ phân giao theo dõi địa bàn</param>
    /// <param name="pEffectDate">Ngày hiệu lực phân giao theo dõi địa bàn</param>
    /// <param name="pStatus">Trạng thái bản ghi (A,C)</param>
    /// <param name="pExpireDate">Ngày kết thúc phân giao theo dõi địa bàn</param>
    /// <param name="pCommuneCodeList">Danh sách xã/phường được phân giao cách nhau dấu phẩy</param>
    /// <returns>Kết quả: 1 - Thành công; 0 - Không thành công</returns>
    public string DeleteMarkPosRepresentative(string pLocalityId, string pStaffId, string pEffectDate, int pStatus, string pExpireDate)
    {
        string resultVal = "";
        if (string.IsNullOrEmpty(pExpireDate))
            pExpireDate = "31/12/2050";
        DateTime dEffectDate = VBSPOSS.Transformations.Converters.StringToDateTime(pEffectDate, FormatParameters.FORMAT_DATE).Date;
        DateTime dExpireDate = VBSPOSS.Transformations.Converters.StringToDateTime(pExpireDate, FormatParameters.FORMAT_DATE).Date;
        try
        {
            bool bSuccess = _service.DeletePosRepresentative(pStaffId, pEffectDate, pExpireDate, pStatus, UserName, Convert.ToByte("2"));
            resultVal = (bSuccess) ? "1" : "0";
        }
        catch
        {
            resultVal = "9";
        }
        return resultVal;
    }

    [HttpPost]
    public JsonResult CheckOpenPosRepresentative(string pPosCode)
    {
       string resultVal = "";
       bool bSuccess = _service.HasOpenPosRepresentative(pPosCode);
        resultVal = (bSuccess) ? "1" : "0";
       return Json(new { resultVal });
    }
}
