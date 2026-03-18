using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;
using VBSPOSS.Constants;
using VBSPOSS.Utils;

public class PosRepresentativeService : IPosRepresentativeService
{
    /// <summary>
    /// Defines the _dbContext.
    /// </summary>
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Defines the _mapper.
    /// </summary>
    private readonly IMapper _mapper;
    private readonly IListOfValueService _serviceLOV;
    private readonly ILogger<PosRepresentativeService> _logger;
    private readonly IApiInternalService _internalServiceAPI;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
    /// </summary>
    /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
    /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
    public PosRepresentativeService(ApplicationDbContext dbContext, IMapper mapper, IListOfValueService serviceLOV, ILogger<PosRepresentativeService> logger, IApiInternalService internalServiceAPI)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _serviceLOV = serviceLOV;
        _logger = logger;
        _internalServiceAPI = internalServiceAPI;
    }

    /// <summary>
    /// Hàm lấy danh sách người đại diện các đơn vị theo api
    /// </summary>
    /// <returns></returns>
    public async Task<List<PosRepresentativeViewModel>> GetPosRepresentativeListByApi(string pPosCode, string pStaffId)
    {
        var answer = new List<PosRepresentativeViewModel>();
        try
        {
            var response = await _internalServiceAPI.GetListStaffVBSP(pPosCode);
            if (response.Result != null)
            {
                var listStaff = response.Result;
                if (!string.IsNullOrEmpty(pStaffId))
                    listStaff = listStaff.Where(w => w.StaffId == pStaffId).ToList();
                foreach (var item in listStaff)
                {
                    PosRepresentativeViewModel objItem = new PosRepresentativeViewModel();
                    objItem = _mapper.Map<PosRepresentativeViewModel>(item);
                    answer.Add(objItem);
                }
            }
            return answer;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Hàm lấy danh sách người đại diện các đơn vị trong bảng PosRepresentative
    /// </summary>
    /// <returns></returns>
    public List<PosRepresentativeViewModel> GetPosRepresentativeList(string pPosCode, string pStaffId,string pStaffName, int pStatus)
    {
        int iCount = 0;
        var answer = new List<PosRepresentativeViewModel>();
        try
        {
            var localityListByStaff = _dbContext.PosRepresentatives.Where(w =>w.StaffId != ""
                                    && (string.IsNullOrEmpty(pStaffId) || w.StaffId == pStaffId)
                                    && (string.IsNullOrEmpty(pPosCode) || w.MainPosCode == pPosCode)
                                    && (pStatus == -1 || w.Status == pStatus))
                                    .Where(delegate (PosRepresentative c)
                                    {
                                        if (string.IsNullOrEmpty(pStaffName)
                                            || (c.StaffName != null && c.StaffName.ToLower().Contains(pStaffName.ToLower()))
                                            || (c.StaffName != null && Utilities.ConvertToUnSign(c.StaffName.ToLower()).IndexOf(pStaffName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                            )
                                            return true;
                                        else
                                            return false;
                                    })
                                    .OrderBy(o => o.PosCode).ThenBy(o => o.StaffDepartmentCode).ThenBy(o => o.StaffPositionCode).ToList();

            if (localityListByStaff != null && localityListByStaff.Count != 0)
            {
                foreach (var item in localityListByStaff)
                {
                    iCount++;
                    PosRepresentativeViewModel objItem = new PosRepresentativeViewModel();
                    objItem = _mapper.Map<PosRepresentativeViewModel>(item);
                    objItem.OrderNo = iCount;
                    answer.Add(objItem);
                }
            }
            return answer;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tải dữ liệu: {ex.Message}", ex);
        }
    }
    public List<PosRepresentativeViewModel> GetPosRepresentativeList(string pPosCode, string pStaffId,
    string pStaffName, int pStatus, string userPosCode)
    {
        int iCount = 0;
        var answer = new List<PosRepresentativeViewModel>();
        try
        {
            var query = _dbContext.PosRepresentatives.Where(w => w.StaffId != "");

            // Logic lọc theo PosCode
            if (!string.IsNullOrEmpty(pPosCode))
            {
                // Tìm kiếm theo PosCode cụ thể
                query = query.Where(w => w.PosCode == pPosCode);
            }
            else if (!string.IsNullOrEmpty(userPosCode))
            {
                // Kiểm tra xem userPosCode có phải là MainPosCode không
                var isMainPos = _dbContext.PosRepresentatives
                    .Any(x => x.MainPosCode == userPosCode);

                if (isMainPos || userPosCode == "000100")
                {
                    // Nếu là MainPosCode, lấy tất cả PosCode thuộc MainPosCode đó
                    query = query.Where(w => w.MainPosCode == userPosCode);
                }
                else
                {
                    // Nếu là PosCode thường, chỉ lấy PosCode của user
                    query = query.Where(w => w.PosCode == userPosCode);
                }
            }

            // Các điều kiện lọc khác
            if (!string.IsNullOrEmpty(pStaffId))
                query = query.Where(w => w.StaffId == pStaffId);

            if (pStatus != -1)
                query = query.Where(w => w.Status == pStatus);

            // Lọc theo tên cán bộ
            if (!string.IsNullOrEmpty(pStaffName))
            {
                query = query.Where(c =>
                    (c.StaffName != null && c.StaffName.ToLower().Contains(pStaffName.ToLower())) ||
                    (c.StaffName != null && Utilities.ConvertToUnSign(c.StaffName.ToLower())
                        .IndexOf(pStaffName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                );
            }

            var localityListByStaff = query
                .OrderBy(o => o.PosCode)
                .ThenBy(o => o.StaffDepartmentCode)
                .ThenBy(o => o.StaffPositionCode)
                .ToList();

            if (localityListByStaff != null && localityListByStaff.Count != 0)
            {
                foreach (var item in localityListByStaff)
                {
                    iCount++;
                    PosRepresentativeViewModel objItem = new PosRepresentativeViewModel();
                    objItem = _mapper.Map<PosRepresentativeViewModel>(item);
                    objItem.OrderNo = iCount;
                    answer.Add(objItem);
                }
            }
            return answer;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tải dữ liệu: {ex.Message}", ex);
        }
    }
    /// <summary>
    /// Hàm thực hiện cập nhật dữ liệu thông tin người đại diện
    /// </summary>
    /// <param name="model">Thông tin dữ liệu người đại diện</param>
    /// <param name="pFlagCall">Cờ xác định gọi: 1: Thêm mới; 2: Sửa đổi (Trường này không ý nghĩa nhiều với trường hợp truyền vào LIST COMMUNE</param>
    /// <param name="pUserName">Username thực hiện cập nhật</param>
    /// <returns></returns>
    public async Task<string> UpdatePosRepresentative(PosRepresentativeViewModel model, int pFlagCall, string pUserName)
    {
        string sResult = "", provinceCode = "", districtCode = "";
        int iSaveChanges = 0;
        try
        {
            var response = await _internalServiceAPI.GetListStaffVBSP(model.PosCode);
            //Xử lý đổi trạng thái Đóng nếu đã có
            bool retDelete = ChangeStatusPosRepresentative(model.PosCode, pUserName);
            //Cập nhật Thêm mới/Sửa đổi với người đại diện
            var listStaff = response.Result.Where(w => w.StaffCode == model.StaffCode).FirstOrDefault();
            if (listStaff != null && !string.IsNullOrEmpty(listStaff.StaffCode)) //Thêm mới
            {
                PosRepresentative objPosRepresentative = new PosRepresentative();
                objPosRepresentative.RepresentativeType = "1";
                objPosRepresentative.MainPosCode = listStaff.MainPosCode;
                objPosRepresentative.MainPosName = listStaff.MainPosName;
                objPosRepresentative.PosCode = listStaff.PosCode;
                objPosRepresentative.PosName = listStaff.PosName;
                objPosRepresentative.PosMobileNo = "";
                objPosRepresentative.PosFaxNo = "";
                objPosRepresentative.PosAddressFull = "";
                objPosRepresentative.PosAddressLine = "";
                objPosRepresentative.PosSubCommune = "";
                objPosRepresentative.PosCommune = "";
                objPosRepresentative.PosDistrict = "";
                objPosRepresentative.PosProvince = "";
                objPosRepresentative.PosSubCommuneId = "";
                objPosRepresentative.StaffId = listStaff.StaffId;
                objPosRepresentative.StaffCode = listStaff.StaffCode;
                objPosRepresentative.StaffName = listStaff.StaffName;
                objPosRepresentative.DateOfBirth = listStaff.DateOfBirth;
                objPosRepresentative.Genders = listStaff.GenderCode;
                objPosRepresentative.StaffPosCode = listStaff.StaffPosCode;
                objPosRepresentative.StaffPosName = listStaff.StaffPosName;
                objPosRepresentative.StaffDepartmentCode = listStaff.StaffDepartmentCode;
                objPosRepresentative.StaffDepartmentName = listStaff.StaffDepartmentName;
                objPosRepresentative.StaffPositionCode = listStaff.StaffPositionCode;
                objPosRepresentative.StaffPositionName = listStaff.StaffPositionName;
                objPosRepresentative.StaffMobileNo = listStaff.StaffMobileNo;
                objPosRepresentative.StaffEmail = listStaff.StaffEmail;
                objPosRepresentative.EffectDate = model.EffectDate;
                objPosRepresentative.ExpireDate = model.ExpireDate;
                objPosRepresentative.DecisionNo = model.DecisionNo;
                objPosRepresentative.DecisionTitle = model.DecisionTitle;
                objPosRepresentative.Status = model.Status;
                objPosRepresentative.Notes = model.Notes;
                objPosRepresentative.CreatedBy = pUserName;
                objPosRepresentative.CreatedDate = DateTime.Now;
                objPosRepresentative.ModifiedBy = pUserName;
                objPosRepresentative.ModifiedDate = DateTime.Now;

                _dbContext.PosRepresentatives.Add(objPosRepresentative);
                iSaveChanges = _dbContext.SaveChanges();
                if (iSaveChanges > 0)
                {
                    sResult = objPosRepresentative.Id.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return sResult;
    }

    /// <summary>
    /// Hàm xóa hoặc đánh dấu xóa (Chuyển trạng thái đóng) bản ghi của bảng dữ liệu: .
    /// </summary>
    /// <param name="pLocalityId">Chỉ số khóa bản ghi</param>
    /// <param name="pStaffId">Id Cán bộ phân giao theo dõi địa bàn</param>
    /// <param name="pEffectDate">Ngày hiệu lực phân giao theo dõi địa bàn</param>
    /// <param name="pExpireDate">Ngày kết thúc phân giao theo dõi địa bàn</param>
    /// <param name="pStatus">Trạng thái bản ghi (A,C)</param>
    /// <param name="pCommuneCodeList">Danh sách xã/phường được phân giao cách nhau dấu phẩy</param>
    /// <param name="pModifiedBy">Username thực hiện</param>
    /// <param name="pFlagDelete">Cờ xác định Xóa/Đánh dấu xóa: 1: Xóa hẳn; 2: Đánh dấu xóa;</param>
    /// <returns>True - Thành công; False - Không thành công</returns>
    public bool DeletePosRepresentative(string pStaffId, string pEffectDate, string pExpireDate, int pStatus, string pModifiedBy, int pFlagDelete)
    {
        bool bResult = false;
        int iCountDetele = 0;
        try
        {
            DateTime dEffectDate = VBSPOSS.Helpers.DateTimeUtils.StringToDateTime(pEffectDate, "dd/MM/yyyy");
            List<PosRepresentative> objPosRepresentativeList = new List<PosRepresentative>();
            if (!string.IsNullOrEmpty(pStaffId))
            {
                objPosRepresentativeList = _dbContext.PosRepresentatives.Where(m => m.StaffId == pStaffId && m.EffectDate == dEffectDate.Date && m.Status == pStatus).ToList();
            }
            if (objPosRepresentativeList != null && objPosRepresentativeList.Count != 0)
            {
                foreach (var item in objPosRepresentativeList)
                {
                    var objPosRepresentative = objPosRepresentativeList.Where(m => m.Status == VBSPOSS.Constants.StatusValue.StatusOpen).FirstOrDefault();
                    objPosRepresentative.ExpireDate = (pExpireDate == "31/12/2050") ? DateTime.Now.Date : VBSPOSS.Helpers.DateTimeUtils.StringToDateTime(pExpireDate, "dd/MM/yyyy");
                    objPosRepresentative.ModifiedBy = pModifiedBy;
                    objPosRepresentative.ModifiedDate = DateTime.Now;
                    objPosRepresentative.Status = VBSPOSS.Constants.StatusValue.StatusClose;

                    if (pFlagDelete == 1)
                    {
                        _dbContext.PosRepresentatives.Remove(objPosRepresentative);
                        if (_dbContext.SaveChanges() > 0)
                            iCountDetele++;
                    }
                    else if (pFlagDelete == 2)
                    {
                        _dbContext.Entry(objPosRepresentative).Property(x => x.ExpireDate).IsModified = true;
                        _dbContext.Entry(objPosRepresentative).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objPosRepresentative).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objPosRepresentative).Property(x => x.ModifiedDate).IsModified = true;
                        if (_dbContext.SaveChanges() > 0)
                            iCountDetele++;
                    }
                }
            }
            bResult = (iCountDetele > 0);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return bResult;
    }

    /// <summary>
    /// Hàm xóa hoặc đánh dấu xóa (Chuyển trạng thái đóng) bản ghi của bảng dữ liệu: .
    /// </summary>
    /// <returns>True - Thành công; False - Không thành công</returns>
    public bool ChangeStatusPosRepresentative(string pPosCode,string pModifiedBy)
    {
        bool bResult = false;
        int iCountDetele = 0;
        try
        {
            List<PosRepresentative> objPosRepresentativeList = new List<PosRepresentative>();
            if (!string.IsNullOrEmpty(pPosCode))
            {
                objPosRepresentativeList = _dbContext.PosRepresentatives.Where(m => m.PosCode == pPosCode && m.Status == VBSPOSS.Constants.StatusValue.StatusOpen).ToList();
            }
            if (objPosRepresentativeList != null && objPosRepresentativeList.Count != 0)
            {
                foreach (var item in objPosRepresentativeList)
                {
                    item.ExpireDate =  DateTime.Now.Date;
                    item.ModifiedBy = pModifiedBy;
                    item.ModifiedDate = DateTime.Now;
                    item.Status = VBSPOSS.Constants.StatusValue.StatusClose;
                    _dbContext.Entry(item).Property(x => x.ExpireDate).IsModified = true;
                    _dbContext.Entry(item).Property(x => x.Status).IsModified = true;
                    _dbContext.Entry(item).Property(x => x.ModifiedBy).IsModified = true;
                    _dbContext.Entry(item).Property(x => x.ModifiedDate).IsModified = true;
                    if (_dbContext.SaveChanges() > 0)
                        iCountDetele++;
                }
            }
            bResult = (iCountDetele > 0);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return bResult;
    }

    public bool HasOpenPosRepresentative(string posCode)
    {
        return _dbContext.PosRepresentatives
            .Any(x => x.PosCode == posCode
                   && x.Status == VBSPOSS.Constants.StatusValue.StatusOpen);
    }
}
