using AutoMapper;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class ListOfTransPointService : IListOfTransPointService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;
       

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;


        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public ListOfTransPointService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Hàm trả về Danh sách danh mục chung theo những điều kiện truyền vào: ListOfTransPoint
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <returns>Danh sách bản ghi</returns>
        public List<ListOfTransPointViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pEffectiveDate, string pTxnStatus)
        {
            var answer = new List<ListOfTransPointViewModel>();
            try
            {
                int iCount = 0;
                var profileListRoots = _dbContext.ListOfValues.Where(w => w.ParentId == 0).OrderBy(o => o.Code).ToList();

                var profileListTMPs = _dbContext.ListOfTransPoints.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                        && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                                        && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
                                        && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                        && (string.IsNullOrEmpty(pTxnStatus) || w.TxnStatus.Contains(pTxnStatus))
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();

                foreach (var item in profileListTMPs)
                {
                    iCount++;
                    ListOfTransPointViewModel objItem = new ListOfTransPointViewModel();
                    objItem = _mapper.Map<ListOfTransPointViewModel>(item);
                    objItem.STT = iCount;
                    answer.Add(objItem);
                }
                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        public int UpdateListOfTransPoint(ListOfTransPointViewModel model, string pUserName)
        {
            int iResultId = 0, iSaveChanges = 0;
            try
            {
                DateTime currentDateVal = DateTime.Now;
                var objTranspoint = _dbContext.ListOfTransPoints.Where(m => m.TxnPointCode == model.TxnPointCode).FirstOrDefault();

                if (objTranspoint != null)
                {
                    objTranspoint.ProvinceCode = model.ProvinceCode;
                    objTranspoint.ProvinceName = model.ProvinceName;
                    objTranspoint.PosCode = model.PosCode;
                    objTranspoint.PosName = model.PosName;
                    objTranspoint.DistrictCode = model.DistrictCode;
                    objTranspoint.DistrictName = model.DistrictName;
                    objTranspoint.CommuneCode = model.CommuneCode;
                    objTranspoint.CommuneName = model.CommuneName;
                    objTranspoint.TxnPointCode = model.TxnPointCode;
                    objTranspoint.TxnPointName = model.TxnPointName;
                    objTranspoint.VisitDate = model.VisitDate;
                    objTranspoint.Times = model.Times;
                    objTranspoint.TimeBegin = model.TimeBegin;
                    objTranspoint.TimeEnd = model.TimeEnd;
                    objTranspoint.TimeBeginNum = model.TimeBeginNum;
                    objTranspoint.TimeEndNum = model.TimeEndNum;
                    objTranspoint.Hours = model.Hours;
                    objTranspoint.Minutes = model.Minutes;
                    objTranspoint.Longitude = model.Longitude;
                    objTranspoint.Latitude = model.Latitude;
                    objTranspoint.IsInCommune = model.IsInCommune;
                    objTranspoint.IsInPos = model.IsInPos;
                    objTranspoint.IsInterWard = model.IsInterWard;
                    objTranspoint.InterWardName = model.InterWardName;
                    objTranspoint.EffectiveDate = model.EffectiveDate;
                    objTranspoint.TxnLocation = model.TxnLocation;
                    objTranspoint.AddressDetail = model.AddressDetail;
                    objTranspoint.AddressCode = model.AddressCode;
                    objTranspoint.AddressFull = model.AddressFull;
                    objTranspoint.PhoneSupport = model.PhoneSupport;
                    objTranspoint.PhoneSupport01 = model.PhoneSupport01;
                    objTranspoint.PhoneSupport02 = model.PhoneSupport02;
                    objTranspoint.TxnStatus = model.TxnStatus;
                    objTranspoint.Status = model.Status;
                    objTranspoint.Remark = model.Remark;
                    objTranspoint.CreatedBy = model.CreatedBy;
                    objTranspoint.CreatedDate = model.CreatedDate;
                    objTranspoint.ModifiedBy = pUserName;
                    objTranspoint.ModifiedDate = DateTime.Now;
                    objTranspoint.ApproverBy = model.ApproverBy;
                    objTranspoint.ApprovalDate = model.ApprovalDate;
                    _dbContext.Entry(objTranspoint).State = EntityState.Modified;
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = iSaveChanges;
                }
                else
                {
                    ListOfTransPoint objModelTranspoint = new ListOfTransPoint();
                    objModelTranspoint.ProvinceCode = model.ProvinceCode;
                    objModelTranspoint.ProvinceName = model.ProvinceName;
                    objModelTranspoint.PosCode = model.PosCode;
                    objModelTranspoint.PosName = model.PosName;
                    objModelTranspoint.DistrictCode = model.DistrictCode;
                    objModelTranspoint.DistrictName = model.DistrictName;
                    objModelTranspoint.CommuneCode = model.CommuneCode;
                    objModelTranspoint.CommuneName = model.CommuneName;
                    objModelTranspoint.TxnPointCode = model.TxnPointCode;
                    objModelTranspoint.TxnPointName = model.TxnPointName;
                    objModelTranspoint.VisitDate = model.VisitDate;
                    objModelTranspoint.Times = model.Times;
                    objModelTranspoint.TimeBegin = model.TimeBegin;
                    objModelTranspoint.TimeEnd = model.TimeEnd;
                    objModelTranspoint.TimeBeginNum = model.TimeBeginNum;
                    objModelTranspoint.TimeEndNum = model.TimeEndNum;
                    objModelTranspoint.Hours = model.Hours;
                    objModelTranspoint.Minutes = model.Minutes;
                    objModelTranspoint.Longitude = model.Longitude;
                    objModelTranspoint.Latitude = model.Latitude;
                    objModelTranspoint.IsInCommune = model.IsInCommune;
                    objModelTranspoint.IsInPos = model.IsInPos;
                    objModelTranspoint.IsInterWard = model.IsInterWard;
                    objModelTranspoint.InterWardName = model.InterWardName;
                    objModelTranspoint.EffectiveDate = model.EffectiveDate;
                    objModelTranspoint.TxnLocation = model.TxnLocation;
                    objModelTranspoint.AddressDetail = model.AddressDetail;
                    objModelTranspoint.AddressCode = model.AddressCode;
                    objModelTranspoint.AddressFull = model.AddressFull;
                    objModelTranspoint.PhoneSupport = model.PhoneSupport;
                    objModelTranspoint.PhoneSupport01 = model.PhoneSupport01;
                    objModelTranspoint.PhoneSupport02 = model.PhoneSupport02;
                    objModelTranspoint.TxnStatus = model.TxnStatus;
                    objModelTranspoint.Status = model.Status;
                    objModelTranspoint.Remark = model.Remark;
                    objModelTranspoint.CreatedBy = pUserName;
                    objModelTranspoint.CreatedDate = DateTime.Now;
                    objModelTranspoint.ModifiedBy = pUserName;
                    objModelTranspoint.ModifiedDate = DateTime.Now;
                    objModelTranspoint.ApproverBy = model.ApproverBy;
                    objModelTranspoint.ApprovalDate = model.ApprovalDate;

                    _dbContext.ListOfTransPoints.Add(objModelTranspoint);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = iSaveChanges;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultId;
        }

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch
        /// </summary>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        public bool DeleteListOfTransPoint(string pTxnPointCode, string pUserName, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objListOfTransPoint = _dbContext.ListOfTransPoints.Where(m => m.TxnPointCode == pTxnPointCode).FirstOrDefault();
                if (objListOfTransPoint != null)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfTransPoints.Remove(objListOfTransPoint);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objListOfTransPoint.TxnStatus = StatusLov.StatusClosedPOS;
                        objListOfTransPoint.ModifiedBy = pUserName;
                        objListOfTransPoint.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objListOfTransPoint).Property(x => x.TxnStatus).IsModified = true;
                        _dbContext.Entry(objListOfTransPoint).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objListOfTransPoint).Property(x => x.ModifiedDate).IsModified = true;
                        return (_dbContext.SaveChanges() > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }
    }
}
