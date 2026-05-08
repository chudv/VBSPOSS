using AutoMapper;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Net.NetworkInformation;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Data.OSS.Models;
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
        /// Kết nối CSDL Oracle của iDC
        /// </summary>
        private readonly IntellectIDCDbContext _dbContextIDC;

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;

        private readonly ILogger<ListOfTransPointService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public ListOfTransPointService(ApplicationDbContext dbContext, IMapper mapper, IntellectIDCDbContext dbContextIDC, ILogger<ListOfTransPointService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dbContextIDC = dbContextIDC;
            _logger = logger;
        }

        /// <summary>
        /// Hàm trả về Danh sách điểm giao dịch theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfTransPoint
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu POS Chi nhánh</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        public List<ListOfTransPointViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pTxnLocation)
        {
            var answer = new List<ListOfTransPointViewModel>();
            if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
                pVisitDateBegin = 0;
            if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
                pVisitDateEnd = 31;
            try
            {
                int iCountTMP = 0;
                List<ListOfTransPoint> listOfTransPointTmp = new List<ListOfTransPoint>();
                var listOfTransPointTmp01 = _dbContext.ListOfTransPoints.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                                            && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (string.IsNullOrEmpty(pTxnStatus) || w.TxnStatus.Contains(pTxnStatus))
                                        && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
                                        )
                                        .Where(delegate (ListOfTransPoint c)
                                        {
                                            if (string.IsNullOrEmpty(pTxnPointName)
                                                || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                                                || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
                                                )
                                                return true;
                                            else
                                                return false;
                                        }
                                        ).ToList();
                if (string.IsNullOrEmpty(pTxnLocation))
                {
                    listOfTransPointTmp = listOfTransPointTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                }
                else
                {
                    if (listOfTransPointTmp01 != null && listOfTransPointTmp01.Count != 0)
                    {
                        listOfTransPointTmp = listOfTransPointTmp01.Where(w => w.TxnPointCode != "")
                                            .Where(delegate (ListOfTransPoint c)
                                            {
                                                if (string.IsNullOrEmpty(pTxnLocation)
                                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
                                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
                                                    )
                                                    return true;
                                                else
                                                    return false;
                                            }
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                    }
                }
                if (listOfTransPointTmp != null && listOfTransPointTmp.Count != 0)
                {
                    foreach (var item in listOfTransPointTmp)
                    {
                        iCountTMP++;
                        ListOfTransPointViewModel objItem = new ListOfTransPointViewModel();
                        objItem = _mapper.Map<ListOfTransPointViewModel>(item);
                        objItem.OrderNo = iCountTMP;
                        objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EffectiveDateText = item.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.TxnStatusText = (item.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
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
        /// Hàm lấy danh sách điểm giao dịch để cho Lựa chọn thay đổi theo yêu cầu nghiệp vụ
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu của POS Chi nhánh</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào -1</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEventCode">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách điểm giao dịch theo Model ListOfTransPointWorkViewModel</returns>
        public List<ListOfTransPointWorkViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pTxnPointCode, string pTxnPointName,
                                int pStatus, string pTxnLocation, string pEventCode)
        {
            var listTransPointWorks = new List<ListOfTransPointWorkViewModel>();
            try
            {
                int iCountTMP = 0;

                List<ListOfTransPoint> listOfTransPointTmp = new List<ListOfTransPoint>();
                var listOfTransPointTmp01 = _dbContext.ListOfTransPoints.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (pStatus == -1 || w.Status == pStatus)
                                        )
                                        .Where(delegate (ListOfTransPoint c)
                                        {
                                            if (string.IsNullOrEmpty(pTxnPointName)
                                                || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                                                || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
                                                )
                                                return true;
                                            else
                                                return false;
                                        }
                                        ).ToList();
                if (string.IsNullOrEmpty(pTxnLocation))
                {
                    listOfTransPointTmp = listOfTransPointTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                }
                else
                {
                    if (listOfTransPointTmp01 != null && listOfTransPointTmp01.Count != 0)
                    {
                        listOfTransPointTmp = listOfTransPointTmp01.Where(w => w.TxnPointCode != "")
                                            .Where(delegate (ListOfTransPoint c)
                                            {
                                                if (string.IsNullOrEmpty(pTxnLocation)
                                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
                                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
                                                    )
                                                    return true;
                                                else
                                                    return false;
                                            }
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                    }
                }
                if (listOfTransPointTmp != null && listOfTransPointTmp.Count != 0)
                {
                    var listOfTransPointtWorkTmp = _dbContext.ListOfTransPointWorks.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                            && (pStatus == -1 || w.Status == pStatus)
                        )
                        .Where(delegate (ListOfTransPointWork c)
                        {
                            if (string.IsNullOrEmpty(pTxnPointName)
                                || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                                || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
                                )
                                return true;
                            else
                                return false;
                        }
                        ).ToList();

                    foreach (var item in listOfTransPointTmp)
                    {
                        iCountTMP++;
                        ListOfTransPointWorkViewModel objItem = new ListOfTransPointWorkViewModel();
                        objItem = _mapper.Map<ListOfTransPointWorkViewModel>(item);
                        objItem.EventCode = "";
                        objItem.EventName = "";
                        objItem.ParentId = 0;
                        objItem.OrderNo = iCountTMP;
                        objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EffectiveDateText = item.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.TxnStatusText = (item.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;

                        if (listOfTransPointtWorkTmp != null && listOfTransPointtWorkTmp.Count != 0)
                        {
                            var objTransPointtWorkTmp = listOfTransPointtWorkTmp.Where(w => w.TxnPointCode == item.TxnPointCode).OrderByDescending(o => o.ModifiedDate).ThenByDescending(o => o.ApprovalDate).FirstOrDefault();
                            if (objTransPointtWorkTmp != null && !string.IsNullOrEmpty(objTransPointtWorkTmp.TxnPointCode))
                            {
                                objItem = _mapper.Map<ListOfTransPointWorkViewModel>(objTransPointtWorkTmp);
                                objItem.OrderNo = iCountTMP;
                                objItem.VisitDateText = objTransPointtWorkTmp.VisitDate.ToString("D2");
                                objItem.EffectiveDateText = objTransPointtWorkTmp.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                                objItem.TxnStatusText = (objTransPointtWorkTmp.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                                objItem.StatusText = StatusTrans.GetByValue(objTransPointtWorkTmp.Status).Description;
                            }
                        }
                        if (!string.IsNullOrEmpty(pEventCode))
                        {
                            if (objItem.EventCode == pEventCode)
                                listTransPointWorks.Add(objItem);
                        }
                        else listTransPointWorks.Add(objItem);
                    }
                }
                return listTransPointWorks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfTransPoint)
        /// </summary>
        /// <param name="pTransPointUpd">Thông tin điểm giao dịch cập nhật</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Số bản ghi được thêm/sửa</returns>
        public int UpdateListOfTransPoint(ListOfTransPointViewModel pTransPointUpd, string pUserName)
        {
            int iResultId = 0, iSaveChanges = 0;
            try
            {
                var objTranspointUpdate = _dbContext.ListOfTransPoints.Where(m => m.TxnPointCode == pTransPointUpd.TxnPointCode).FirstOrDefault();
                DateTime dCurrentDateVal = DateTime.Now;
                if (objTranspointUpdate != null && !string.IsNullOrEmpty(objTranspointUpdate.TxnPointCode))
                {
                    //objTranspointUpdate.ProvinceCode = model.ProvinceCode;
                    objTranspointUpdate.ProvinceName = pTransPointUpd.ProvinceName;
                    //objTranspointUpdate.PosCode = model.PosCode;
                    objTranspointUpdate.PosName = pTransPointUpd.PosName;
                    //objTranspointUpdate.DistrictCode = model.DistrictCode;
                    objTranspointUpdate.DistrictName = pTransPointUpd.DistrictName;
                    //objTranspointUpdate.CommuneCode = model.CommuneCode;
                    objTranspointUpdate.CommuneName = pTransPointUpd.CommuneName;
                    //objTranspointUpdate.TxnPointCode = model.TxnPointCode;
                    objTranspointUpdate.TxnPointName = pTransPointUpd.TxnPointName;
                    objTranspointUpdate.VisitDate = pTransPointUpd.VisitDate;
                    objTranspointUpdate.Times = pTransPointUpd.Times;
                    objTranspointUpdate.TimeBegin = pTransPointUpd.TimeBegin;
                    objTranspointUpdate.TimeEnd = pTransPointUpd.TimeEnd;
                    objTranspointUpdate.TimeBeginNum = pTransPointUpd.TimeBeginNum;
                    objTranspointUpdate.TimeEndNum = pTransPointUpd.TimeEndNum;
                    objTranspointUpdate.Hours = pTransPointUpd.Hours;
                    objTranspointUpdate.Minutes = pTransPointUpd.Minutes;
                    objTranspointUpdate.Longitude = pTransPointUpd.Longitude;
                    objTranspointUpdate.Latitude = pTransPointUpd.Latitude;
                    objTranspointUpdate.IsInCommune = pTransPointUpd.IsInCommune;
                    objTranspointUpdate.IsInPos = pTransPointUpd.IsInPos;
                    objTranspointUpdate.IsInterWard = pTransPointUpd.IsInterWard;
                    objTranspointUpdate.InterWardName = pTransPointUpd.InterWardName;
                    //objTranspointUpdate.EffectiveDate = pTransPointUpd.EffectiveDate;
                    objTranspointUpdate.TxnLocation = pTransPointUpd.TxnLocation;
                    objTranspointUpdate.AddressDetail = pTransPointUpd.AddressDetail;
                    objTranspointUpdate.AddressCode = pTransPointUpd.AddressCode;
                    objTranspointUpdate.AddressFull = pTransPointUpd.AddressFull;
                    objTranspointUpdate.PhoneSupport = pTransPointUpd.PhoneSupport;
                    objTranspointUpdate.PhoneSupport01 = pTransPointUpd.PhoneSupport01;
                    objTranspointUpdate.PhoneSupport02 = pTransPointUpd.PhoneSupport02;
                    objTranspointUpdate.TxnStatus = pTransPointUpd.TxnStatus;
                    objTranspointUpdate.Status = pTransPointUpd.Status;
                    objTranspointUpdate.Remark = pTransPointUpd.Remark;
                    objTranspointUpdate.ModifiedBy = pUserName;
                    objTranspointUpdate.ModifiedDate = dCurrentDateVal;
                    _dbContext.Entry(objTranspointUpdate).State = EntityState.Modified;
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = iSaveChanges;
                }
                else
                {
                    ListOfTransPoint objTranspointAddNew = new ListOfTransPoint();
                    objTranspointAddNew = _mapper.Map<ListOfTransPoint>(pTransPointUpd);
                    objTranspointAddNew.Status = StatusTrans.Status_Created.Value;
                    objTranspointAddNew.CreatedBy = pUserName;
                    objTranspointAddNew.CreatedDate = dCurrentDateVal;
                    objTranspointAddNew.ModifiedBy = pUserName;
                    objTranspointAddNew.ModifiedDate = dCurrentDateVal;
                    objTranspointAddNew.ApproverBy = pUserName;
                    objTranspointAddNew.ApprovalDate = dCurrentDateVal;

                    _dbContext.ListOfTransPoints.Add(objTranspointAddNew);
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
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch (Bảng ListOfTransPoint)
        /// </summary>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public bool DeleteListOfTransPoint(string pTxnPointCode, string pUserName, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objListOfTransPoint = _dbContext.ListOfTransPoints.Where(m => m.TxnPointCode == pTxnPointCode).FirstOrDefault();
                if (objListOfTransPoint != null && !string.IsNullOrEmpty(objListOfTransPoint.TxnPointCode))
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfTransPoints.Remove(objListOfTransPoint);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objListOfTransPoint.Status = StatusTrans.Status_Closed.Value;
                        objListOfTransPoint.ModifiedBy = pUserName;
                        objListOfTransPoint.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objListOfTransPoint).Property(x => x.Status).IsModified = true;
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

        /// <summary>
        /// Hàm lấy Danh sách điểm giao dịch ghi nhận thông tin Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectiveDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectiveDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        public List<ListOfTransPointWorkViewModel> GetListOfTransPointWorkSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pEffectiveDateBegin, string pEffectiveDateEnd,
                                            int pStatus, string pTxnLocation)
        {
            var listTransPointWorkAnswer = new List<ListOfTransPointWorkViewModel>();
            if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
                pVisitDateBegin = 0;
            if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
                pVisitDateEnd = 31;
            if (string.IsNullOrEmpty(pEffectiveDateBegin))
                pEffectiveDateBegin = DefaultValue.MinDate.ToString();
            if (string.IsNullOrEmpty(pEffectiveDateEnd))
                pEffectiveDateEnd = DefaultValue.MaxDate.ToString();
            DateTime dEffectiveDateBegin = CustConverter.StringToDate(pEffectiveDateBegin, FormatParameters.FORMAT_DATE_INT);
            DateTime dEffectiveDateEnd = CustConverter.StringToDate(pEffectiveDateEnd, FormatParameters.FORMAT_DATE_INT);
            try
            {
                int iCountTMP = 0;
                List<ListOfTransPointWork> listOfTransPointTmp = new List<ListOfTransPointWork>();
                var listOfTransPointTmp01 = _dbContext.ListOfTransPointWorks.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                                            && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (string.IsNullOrEmpty(pTxnStatus) || w.TxnStatus.Contains(pTxnStatus))
                                            && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
                                            && (w.EffectiveDate >= dEffectiveDateBegin.Date && w.EffectiveDate <= dEffectiveDateEnd.Date)
                                            && (pStatus == -1 || w.Status == pStatus)
                                        )
                                        .Where(delegate (ListOfTransPointWork c)
                                        {
                                            if (string.IsNullOrEmpty(pTxnPointName)
                                                || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                                                || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
                                                )
                                                return true;
                                            else
                                                return false;
                                        }
                                    ).ToList();
                if (string.IsNullOrEmpty(pTxnLocation))
                {
                    listOfTransPointTmp = listOfTransPointTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                }
                else
                {
                    if (listOfTransPointTmp01 != null && listOfTransPointTmp01.Count != 0)
                    {
                        listOfTransPointTmp = listOfTransPointTmp01.Where(w => w.TxnPointCode != "")
                                            .Where(delegate (ListOfTransPointWork c)
                                            {
                                                if (string.IsNullOrEmpty(pTxnLocation)
                                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
                                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
                                                    )
                                                    return true;
                                                else
                                                    return false;
                                            }
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                    }    
                }

                if (listOfTransPointTmp != null && listOfTransPointTmp.Count != 0)
                {
                    foreach (var item in listOfTransPointTmp)
                    {
                        iCountTMP++;
                        ListOfTransPointWorkViewModel objItem = new ListOfTransPointWorkViewModel();
                        objItem = _mapper.Map<ListOfTransPointWorkViewModel>(item);
                        objItem.OrderNo = iCountTMP;
                        objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EventName = EventBusinessCode.GetByCode(item.EventCode).Description;
                        objItem.EffectiveDateText = item.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.BusinessDateText = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
                        objItem.TxnStatusText = (item.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
                        if (item.ParentId != 0)
                        {
                            var listOfTransPointHistTmp = _dbContext.ListOfTransPointHists.Where(w => w.Id == item.ParentId).FirstOrDefault();
                            if (listOfTransPointHistTmp != null && listOfTransPointHistTmp.Id > 0)
                            {
                                objItem.ProvinceCodeOldInfo = listOfTransPointHistTmp.ProvinceCode;
                                objItem.ProvinceNameOldInfo = listOfTransPointHistTmp.ProvinceName;
                                objItem.PosCodeOldInfo = listOfTransPointHistTmp.PosCode;
                                objItem.PosNameOldInfo = listOfTransPointHistTmp.PosName;
                                objItem.DistrictCodeOldInfo = listOfTransPointHistTmp.DistrictCode;
                                objItem.DistrictNameOldInfo = listOfTransPointHistTmp.DistrictName;
                                objItem.CommuneCodeOldInfo = listOfTransPointHistTmp.CommuneCode;
                                objItem.TxnPointCodeOldInfo = listOfTransPointHistTmp.TxnPointCode;
                                objItem.TxnPointNameOldInfo = listOfTransPointHistTmp.TxnPointName;
                                objItem.VisitDateOldInfo = listOfTransPointHistTmp.VisitDate;
                                objItem.VisitDateTextOldInfo = listOfTransPointHistTmp.VisitDate.ToString(FormatParameters.FORMAT_DATE);
                                objItem.TimesOldInfo = listOfTransPointHistTmp.Times;
                                objItem.TimeBeginOldInfo = listOfTransPointHistTmp.TimeBegin;
                                objItem.TimeEndOldInfo = listOfTransPointHistTmp.TimeEnd;
                                objItem.TimeBeginNumOldInfo = listOfTransPointHistTmp.TimeBeginNum;
                                objItem.TimeEndNumOldInfo = listOfTransPointHistTmp.TimeEndNum;
                                objItem.HoursOldInfo = listOfTransPointHistTmp.Hours;
                                objItem.MinutesOldInfo = listOfTransPointHistTmp.Minutes;
                                objItem.LongitudeOldInfo = listOfTransPointHistTmp.Longitude;
                                objItem.LatitudeOldInfo = listOfTransPointHistTmp.Latitude;
                                objItem.IsInCommuneOldInfo = listOfTransPointHistTmp.IsInCommune;
                                objItem.IsInPosOldInfo = listOfTransPointHistTmp.IsInPos;
                                objItem.IsInterWardOldInfo = listOfTransPointHistTmp.IsInterWard;
                                objItem.InterWardNameOldInfo = listOfTransPointHistTmp.InterWardName;
                                objItem.EffectiveDateOldInfo = listOfTransPointHistTmp.EffectiveDate;
                                objItem.TxnLocationOldInfo = listOfTransPointHistTmp.TxnLocation;
                                objItem.AddressDetailOldInfo = listOfTransPointHistTmp.AddressDetail;
                                objItem.AddressCodeOldInfo = listOfTransPointHistTmp.AddressCode;
                                objItem.AddressFullOldInfo = listOfTransPointHistTmp.AddressFull;
                                objItem.PhoneSupportOldInfo = listOfTransPointHistTmp.PhoneSupport;
                                objItem.PhoneSupport01OldInfo = listOfTransPointHistTmp.PhoneSupport01;
                                objItem.PhoneSupport02OldInfo = listOfTransPointHistTmp.PhoneSupport02;
                                objItem.TxnStatusOldInfo = listOfTransPointHistTmp.TxnStatus;
                                objItem.TxnStatusTextOldInfo = (listOfTransPointHistTmp.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                                objItem.StatusOldInfo = listOfTransPointHistTmp.Status;
                                objItem.StatusTextOldInfo = StatusTrans.GetByValue(listOfTransPointHistTmp.Status).Description;
                                objItem.RemarkOldInfo = listOfTransPointHistTmp.Remark;
                                objItem.CreatedByOldInfo = listOfTransPointHistTmp.CreatedBy;
                                objItem.CreatedDateOldInfo = listOfTransPointHistTmp.CreatedDate;
                                objItem.ModifiedByOldInfo = listOfTransPointHistTmp.ModifiedBy;
                                objItem.ModifiedDateOldInfo = listOfTransPointHistTmp.ModifiedDate;
                                objItem.ApproverByOldInfo = listOfTransPointHistTmp.ApproverBy;
                                objItem.ApprovalDateOldInfo = listOfTransPointHistTmp.ApprovalDate;
                                objItem.BusinessDateOldInfo = listOfTransPointHistTmp.BusinessDate.Value;
                                objItem.BusinessDateTextOldInfo = listOfTransPointHistTmp.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
                                objItem.DocumentIdOldInfo = listOfTransPointHistTmp.DocumentId.Value;
                            }
                        }
                        else
                        {
                            objItem.ProvinceCodeOldInfo = item.ProvinceCode;
                            objItem.ProvinceNameOldInfo = item.ProvinceName;
                            objItem.PosCodeOldInfo = item.PosCode;
                            objItem.PosNameOldInfo = item.PosName;
                            objItem.DistrictCodeOldInfo = item.DistrictCode;
                            objItem.DistrictNameOldInfo = item.DistrictName;
                            objItem.CommuneCodeOldInfo = item.CommuneCode;
                            objItem.TxnPointCodeOldInfo = item.TxnPointCode;
                            objItem.TxnPointNameOldInfo = item.TxnPointName;
                            objItem.VisitDateOldInfo = item.VisitDate;
                            objItem.VisitDateTextOldInfo = item.VisitDate.ToString(FormatParameters.FORMAT_DATE);
                            objItem.TimesOldInfo = item.Times;
                            objItem.TimeBeginOldInfo = item.TimeBegin;
                            objItem.TimeEndOldInfo = item.TimeEnd;
                            objItem.TimeBeginNumOldInfo = item.TimeBeginNum;
                            objItem.TimeEndNumOldInfo = item.TimeEndNum;
                            objItem.HoursOldInfo = item.Hours;
                            objItem.MinutesOldInfo = item.Minutes;
                            objItem.LongitudeOldInfo = item.Longitude;
                            objItem.LatitudeOldInfo = item.Latitude;
                            objItem.IsInCommuneOldInfo = item.IsInCommune;
                            objItem.IsInPosOldInfo = item.IsInPos;
                            objItem.IsInterWardOldInfo = item.IsInterWard;
                            objItem.InterWardNameOldInfo = item.InterWardName;
                            objItem.EffectiveDateOldInfo = item.EffectiveDate;
                            objItem.TxnLocationOldInfo = item.TxnLocation;
                            objItem.AddressDetailOldInfo = item.AddressDetail;
                            objItem.AddressCodeOldInfo = item.AddressCode;
                            objItem.AddressFullOldInfo = item.AddressFull;
                            objItem.PhoneSupportOldInfo = item.PhoneSupport;
                            objItem.PhoneSupport01OldInfo = item.PhoneSupport01;
                            objItem.PhoneSupport02OldInfo = item.PhoneSupport02;
                            objItem.TxnStatusOldInfo = item.TxnStatus;
                            objItem.TxnStatusTextOldInfo = (item.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                            objItem.StatusOldInfo = item.Status;
                            objItem.StatusTextOldInfo = StatusTrans.GetByValue(item.Status).Description;
                            objItem.RemarkOldInfo = item.Remark;
                            objItem.CreatedByOldInfo = item.CreatedBy;
                            objItem.CreatedDateOldInfo = item.CreatedDate;
                            objItem.ModifiedByOldInfo = item.ModifiedBy;
                            objItem.ModifiedDateOldInfo = item.ModifiedDate;
                            objItem.ApproverByOldInfo = item.ApproverBy;
                            objItem.ApprovalDateOldInfo = item.ApprovalDate;
                            objItem.BusinessDateOldInfo = item.BusinessDate.Value;
                            objItem.BusinessDateTextOldInfo = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
                            objItem.DocumentIdOldInfo = item.DocumentId.Value;
                        }
                        listTransPointWorkAnswer.Add(objItem);
                    }
                }
                return listTransPointWorkAnswer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pTransPointWorkUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfTransPointWorkViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Số bản ghi được thêm/sửa</returns>
        public int UpdateListOfTransPointWork(ListOfTransPointWorkViewModel pTransPointWorkUpd, string pUserNameUpd, string pFlagCall)
        {
            int iResultCountUpd = 0, iSaveChanges = 0;
            try
            {
                if (pTransPointWorkUpd != null && !string.IsNullOrEmpty(pTransPointWorkUpd.TxnPointCode))
                {
                    pTransPointWorkUpd.EffectiveDate = pTransPointWorkUpd.EffectiveDate.Date;
                    pTransPointWorkUpd.BusinessDate = pTransPointWorkUpd.BusinessDate.Date;
                }
                DateTime dCurrentDateVal = DateTime.Now;
                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                {
                    ListOfTransPointWork objTranspointAddNew = new ListOfTransPointWork();
                    objTranspointAddNew = _mapper.Map<ListOfTransPointWork>(pTransPointWorkUpd);
                    objTranspointAddNew.Status = StatusTrans.Status_Created.Value;
                    objTranspointAddNew.CreatedBy = pUserNameUpd;
                    objTranspointAddNew.CreatedDate = dCurrentDateVal;
                    objTranspointAddNew.ModifiedBy = pUserNameUpd;
                    objTranspointAddNew.ModifiedDate = dCurrentDateVal;
                    objTranspointAddNew.ApproverBy = pUserNameUpd;
                    objTranspointAddNew.ApprovalDate = dCurrentDateVal;
                    _dbContext.ListOfTransPointWorks.Add(objTranspointAddNew);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultCountUpd++;
                }
                else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                {
                    var objTranspointWorkUpdate = _dbContext.ListOfTransPointWorks.Where(m => m.TxnPointCode == pTransPointWorkUpd.TxnPointCode
                                && m.EventCode == pTransPointWorkUpd.EventCode && m.ParentId == pTransPointWorkUpd.ParentId
                                && m.ProvinceCode == pTransPointWorkUpd.ProvinceCode && m.PosCode == pTransPointWorkUpd.PosCode
                                && m.EffectiveDate == pTransPointWorkUpd.EffectiveDate.Date && m.BusinessDate == pTransPointWorkUpd.BusinessDate.Date).FirstOrDefault();
                    if (objTranspointWorkUpdate != null && !string.IsNullOrEmpty(objTranspointWorkUpdate.TxnPointCode))
                    {
                        #region --- Sửa bản ghi bình thường (Áp dụng cho trạng thái bản ghi (Status) tạo lập/chỉnh sửa) ---
                        //objTranspointWorkUpdate.ProvinceCode = pTransPointWorkUpd.ProvinceCode; 
                        //objTranspointWorkUpdate.ParentId = pTransPointWorkUpd.ParentId;
                        //objTranspointWorkUpdate.EventCode = pTransPointWorkUpd.EventCode;
                        objTranspointWorkUpdate.ProvinceName = pTransPointWorkUpd.ProvinceName;
                        //objTranspointWorkUpdate.PosCode = pTransPointWorkUpd.PosCode;
                        objTranspointWorkUpdate.PosName = pTransPointWorkUpd.PosName;
                        objTranspointWorkUpdate.DistrictCode = pTransPointWorkUpd.DistrictCode;
                        objTranspointWorkUpdate.DistrictName = pTransPointWorkUpd.DistrictName;
                        objTranspointWorkUpdate.CommuneCode = pTransPointWorkUpd.CommuneCode;
                        objTranspointWorkUpdate.CommuneName = pTransPointWorkUpd.CommuneName;
                        //objTranspointWorkUpdate.TxnPointCode = pTransPointWorkUpd.TxnPointCode;
                        objTranspointWorkUpdate.TxnPointName = pTransPointWorkUpd.TxnPointName;
                        objTranspointWorkUpdate.VisitDate = pTransPointWorkUpd.VisitDate;
                        objTranspointWorkUpdate.Times = pTransPointWorkUpd.Times;
                        objTranspointWorkUpdate.TimeBegin = pTransPointWorkUpd.TimeBegin;
                        objTranspointWorkUpdate.TimeEnd = pTransPointWorkUpd.TimeEnd;
                        objTranspointWorkUpdate.TimeBeginNum = pTransPointWorkUpd.TimeBeginNum;
                        objTranspointWorkUpdate.TimeEndNum = pTransPointWorkUpd.TimeEndNum;
                        objTranspointWorkUpdate.Hours = pTransPointWorkUpd.Hours;
                        objTranspointWorkUpdate.Minutes = pTransPointWorkUpd.Minutes;
                        objTranspointWorkUpdate.Longitude = pTransPointWorkUpd.Longitude;
                        objTranspointWorkUpdate.Latitude = pTransPointWorkUpd.Latitude;
                        objTranspointWorkUpdate.IsInCommune = pTransPointWorkUpd.IsInCommune;
                        objTranspointWorkUpdate.IsInPos = pTransPointWorkUpd.IsInPos;
                        objTranspointWorkUpdate.IsInterWard = pTransPointWorkUpd.IsInterWard;
                        objTranspointWorkUpdate.InterWardName = pTransPointWorkUpd.InterWardName;
                        objTranspointWorkUpdate.TxnLocation = pTransPointWorkUpd.TxnLocation;
                        objTranspointWorkUpdate.AddressDetail = pTransPointWorkUpd.AddressDetail;
                        objTranspointWorkUpdate.AddressCode = pTransPointWorkUpd.AddressCode;
                        objTranspointWorkUpdate.AddressFull = pTransPointWorkUpd.AddressFull;
                        objTranspointWorkUpdate.PhoneSupport = pTransPointWorkUpd.PhoneSupport;
                        objTranspointWorkUpdate.PhoneSupport01 = pTransPointWorkUpd.PhoneSupport01;
                        objTranspointWorkUpdate.PhoneSupport02 = pTransPointWorkUpd.PhoneSupport02;
                        objTranspointWorkUpdate.TxnStatus = pTransPointWorkUpd.TxnStatus;
                        objTranspointWorkUpdate.Status = pTransPointWorkUpd.Status;
                        objTranspointWorkUpdate.Remark = pTransPointWorkUpd.Remark;
                        objTranspointWorkUpdate.ModifiedBy = pUserNameUpd;
                        objTranspointWorkUpdate.ModifiedDate = dCurrentDateVal;
                        objTranspointWorkUpdate.DocumentId = pTransPointWorkUpd.DocumentId;
                        objTranspointWorkUpdate.StatusUpdateCore = pTransPointWorkUpd.StatusUpdateCore;
                        objTranspointWorkUpdate.CallApiTxnStatus = pTransPointWorkUpd.CallApiTxnStatus;
                        objTranspointWorkUpdate.CallApiResRecords = pTransPointWorkUpd.CallApiResRecords;
                        objTranspointWorkUpdate.CallApiResponseCode = pTransPointWorkUpd.CallApiResponseCode;
                        objTranspointWorkUpdate.CallApiResponseMsg = pTransPointWorkUpd.CallApiResponseMsg;

                        _dbContext.Entry(objTranspointWorkUpdate).State = EntityState.Modified;
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                            iResultCountUpd++;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultCountUpd;
        }

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch (Bảng ListOfTransPointWork)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pParentId">Chỉ số bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfTransPointHist) - Bản ghi trước khi được cập nhật vào bảng ListOfTransPointWork</param>
        /// <param name="pProvinceCode">Mã Tỉnh/TP</param>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pEffectiveDate">Ngày hiệu lực của điểm giao dịch</param>
        /// <param name="pBusinessDate">Ngày hệ thống Intellect iDC (Ngày hiệu lực thay đổi thông tin của điểm giao dịch của bản ghi)</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public bool DeleteListOfTransPointWork(string pEventCode, long pParentId, string pProvinceCode, string pPosCode, string pTxnPointCode,
                            DateTime pEffectiveDate, DateTime pBusinessDate, string pUserNameDelete, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objTranspointWorkUpdate = _dbContext.ListOfTransPointWorks.Where(m => m.TxnPointCode == pTxnPointCode
                               && m.EventCode == pEventCode && m.ParentId == pParentId
                               && m.ProvinceCode == pProvinceCode && m.PosCode == pPosCode
                               && m.EffectiveDate == pEffectiveDate.Date && m.BusinessDate == pBusinessDate.Date).OrderByDescending(o => o.ModifiedDate).FirstOrDefault();

                if (objTranspointWorkUpdate != null && !string.IsNullOrEmpty(objTranspointWorkUpdate.TxnPointCode))
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfTransPointWorks.Remove(objTranspointWorkUpdate);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objTranspointWorkUpdate.Status = StatusTrans.Status_Closed.Value;
                        objTranspointWorkUpdate.ModifiedBy = pUserNameDelete;
                        objTranspointWorkUpdate.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objTranspointWorkUpdate).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objTranspointWorkUpdate).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objTranspointWorkUpdate).Property(x => x.ModifiedDate).IsModified = true;
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

        /// <summary>
        /// Hàm lấy Danh sách điểm giao dịch ghi nhận thông tin Lịch sử Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy điểm GD hoạt động</param>
        /// <param name="pEffectiveDateBegin">Ngày hiệu lực bắt đầu. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pEffectiveDateEnd">Ngày hiệu lực kết thúc. Định dạng yyyyMMdd (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        public List<ListOfTransPointHistViewModel> GetListOfTransPointHistSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus, string pEffectiveDateBegin, string pEffectiveDateEnd,
                                            int pStatus, string pTxnLocation)
        {
            var listTransPointHistAnswer = new List<ListOfTransPointHistViewModel>();
            if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
                pVisitDateBegin = 0;
            if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
                pVisitDateEnd = 31;
            if (string.IsNullOrEmpty(pEffectiveDateBegin))
                pEffectiveDateBegin = DefaultValue.MinDate.ToString();
            if (string.IsNullOrEmpty(pEffectiveDateEnd))
                pEffectiveDateEnd = DefaultValue.MaxDate.ToString();
            DateTime dEffectiveDateBegin = CustConverter.StringToDate(pEffectiveDateBegin, FormatParameters.FORMAT_DATE_INT);
            DateTime dEffectiveDateEnd = CustConverter.StringToDate(pEffectiveDateEnd, FormatParameters.FORMAT_DATE_INT);
            try
            {
                int iCountTMP = 0;
                List<ListOfTransPointHist> listOfTransPointTmp = new List<ListOfTransPointHist>();
                var listOfTransPointTmp01 = _dbContext.ListOfTransPointHists.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                                            && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (string.IsNullOrEmpty(pTxnStatus) || w.TxnStatus.Contains(pTxnStatus))
                                            && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
                                            && (w.EffectiveDate >= dEffectiveDateBegin.Date && w.EffectiveDate <= dEffectiveDateEnd.Date)
                                            && (pStatus == -1 || w.Status == pStatus)
                                        )
                                        .Where(delegate (ListOfTransPointHist c)
                                        {
                                            if (string.IsNullOrEmpty(pTxnPointName)
                                                || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                                                || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
                                                )
                                                return true;
                                            else
                                                return false;
                                        }
                                    ).ToList();
                if (string.IsNullOrEmpty(pTxnLocation))
                {
                    listOfTransPointTmp = listOfTransPointTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                }
                else
                {
                    if (listOfTransPointTmp01 != null && listOfTransPointTmp01.Count != 0)
                    {
                        listOfTransPointTmp = listOfTransPointTmp01.Where(w => w.TxnPointCode != "")
                                            .Where(delegate (ListOfTransPointHist c)
                                            {
                                                if (string.IsNullOrEmpty(pTxnLocation)
                                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
                                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
                                                    )
                                                    return true;
                                                else
                                                    return false;
                                            }
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                    }
                }

                if (listOfTransPointTmp != null && listOfTransPointTmp.Count != 0)
                {
                    foreach (var item in listOfTransPointTmp)
                    {
                        iCountTMP++;
                        ListOfTransPointHistViewModel objItem = new ListOfTransPointHistViewModel();
                        objItem = _mapper.Map<ListOfTransPointHistViewModel>(item);
                        objItem.OrderNo = iCountTMP;
                        objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EventName = EventBusinessCode.GetByCode(item.EventCode).Description;
                        objItem.EffectiveDateText = item.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.TxnStatusText = (item.TxnStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
                        objItem.BusinessDateText = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);

                        listTransPointHistAnswer.Add(objItem);
                    }
                }
                return listTransPointHistAnswer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pTransPointHistUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfTransPointHistViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật. Nếu truyền vào rỗng lấy theo người cập nhật từ pTransPointHistUpd</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Chỉ số Id bản ghi được Thêm/Chỉnh sửa</returns>
        public long UpdateListOfTransPointHist(ListOfTransPointHistViewModel pTransPointHistUpd, string pUserNameUpd, string pFlagCall)
        {
            int iSaveChanges = 0;
            long iResultId = 0;
            try
            {
                if (pTransPointHistUpd != null && !string.IsNullOrEmpty(pTransPointHistUpd.TxnPointCode))
                {
                    pTransPointHistUpd.EffectiveDate = pTransPointHistUpd.EffectiveDate.Date;
                    pTransPointHistUpd.BusinessDate = pTransPointHistUpd.BusinessDate.Date;
                }
                DateTime dCurrentDateVal = DateTime.Now;
                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                {
                    ListOfTransPointHist objTranspointAddNew = new ListOfTransPointHist();
                    objTranspointAddNew = _mapper.Map<ListOfTransPointHist>(pTransPointHistUpd);
                    objTranspointAddNew.Status = (pTransPointHistUpd.Status == -1) ? StatusTrans.Status_Created.Value : pTransPointHistUpd.Status;
                    objTranspointAddNew.CreatedBy = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.CreatedBy : pUserNameUpd;
                    objTranspointAddNew.CreatedDate = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.CreatedDate : dCurrentDateVal;
                    objTranspointAddNew.ModifiedBy = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.ModifiedBy : pUserNameUpd;
                    objTranspointAddNew.ModifiedDate = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.ModifiedDate : dCurrentDateVal;
                    objTranspointAddNew.ApproverBy = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.ApproverBy : pUserNameUpd;
                    objTranspointAddNew.ApprovalDate = string.IsNullOrEmpty(pUserNameUpd) ? pTransPointHistUpd.ApprovalDate : dCurrentDateVal;
                    _dbContext.ListOfTransPointHists.Add(objTranspointAddNew);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objTranspointAddNew.Id;
                }
                else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                {
                    var objTranspointHistUpdate = _dbContext.ListOfTransPointHists.Where(m => m.TxnPointCode == pTransPointHistUpd.TxnPointCode
                                && m.EventCode == pTransPointHistUpd.EventCode && m.Id == pTransPointHistUpd.Id
                                && m.ProvinceCode == pTransPointHistUpd.ProvinceCode && m.PosCode == pTransPointHistUpd.PosCode
                                && m.EffectiveDate == pTransPointHistUpd.EffectiveDate.Date && m.BusinessDate == pTransPointHistUpd.BusinessDate.Date).FirstOrDefault();
                    if (objTranspointHistUpdate != null && !string.IsNullOrEmpty(objTranspointHistUpdate.TxnPointCode))
                    {
                        #region --- Sửa bản ghi bình thường (Áp dụng cho trạng thái bản ghi (Status) tạo lập/chỉnh sửa) ---
                        //objTranspointHistUpdate.Id = pTransPointHistUpd.Id;
                        //objTranspointHistUpdate.ParentId = pTransPointHistUpd.ParentId;
                        //objTranspointHistUpdate.EventCode = pTransPointHistUpd.EventCode;
                        //objTranspointHistUpdate.DateSync = pTransPointHistUpd.DateSync; 
                        //objTranspointHistUpdate.TxnPointCode = pTransPointHistUpd.TxnPointCode; 
                        objTranspointHistUpdate.ProvinceCode = pTransPointHistUpd.ProvinceCode;
                        objTranspointHistUpdate.ProvinceName = pTransPointHistUpd.ProvinceName;
                        objTranspointHistUpdate.PosCode = pTransPointHistUpd.PosCode;
                        objTranspointHistUpdate.PosName = pTransPointHistUpd.PosName;

                        objTranspointHistUpdate.DistrictCode = pTransPointHistUpd.DistrictCode;
                        objTranspointHistUpdate.DistrictName = pTransPointHistUpd.DistrictName;
                        objTranspointHistUpdate.CommuneCode = pTransPointHistUpd.CommuneCode;
                        objTranspointHistUpdate.CommuneName = pTransPointHistUpd.CommuneName;
                        objTranspointHistUpdate.TxnPointName = pTransPointHistUpd.TxnPointName;

                        objTranspointHistUpdate.VisitDate = pTransPointHistUpd.VisitDate;
                        objTranspointHistUpdate.Times = pTransPointHistUpd.Times;
                        objTranspointHistUpdate.TimeBegin = pTransPointHistUpd.TimeBegin;
                        objTranspointHistUpdate.TimeEnd = pTransPointHistUpd.TimeEnd;
                        objTranspointHistUpdate.TimeBeginNum = pTransPointHistUpd.TimeBeginNum;
                        objTranspointHistUpdate.TimeEndNum = pTransPointHistUpd.TimeEndNum;
                        objTranspointHistUpdate.Hours = pTransPointHistUpd.Hours;
                        objTranspointHistUpdate.Minutes = pTransPointHistUpd.Minutes;
                        objTranspointHistUpdate.Longitude = pTransPointHistUpd.Longitude;
                        objTranspointHistUpdate.Latitude = pTransPointHistUpd.Latitude;
                        objTranspointHistUpdate.IsInCommune = pTransPointHistUpd.IsInCommune;
                        objTranspointHistUpdate.IsInPos = pTransPointHistUpd.IsInPos;
                        objTranspointHistUpdate.IsInterWard = pTransPointHistUpd.IsInterWard;
                        objTranspointHistUpdate.InterWardName = pTransPointHistUpd.InterWardName;
                        objTranspointHistUpdate.TxnLocation = pTransPointHistUpd.TxnLocation;
                        objTranspointHistUpdate.EffectiveDate = pTransPointHistUpd.EffectiveDate;
                        objTranspointHistUpdate.AddressDetail = pTransPointHistUpd.AddressDetail;
                        objTranspointHistUpdate.AddressCode = pTransPointHistUpd.AddressCode;
                        objTranspointHistUpdate.AddressFull = pTransPointHistUpd.AddressFull;
                        objTranspointHistUpdate.PhoneSupport = pTransPointHistUpd.PhoneSupport;
                        objTranspointHistUpdate.PhoneSupport01 = pTransPointHistUpd.PhoneSupport01;
                        objTranspointHistUpdate.PhoneSupport02 = pTransPointHistUpd.PhoneSupport02;
                        objTranspointHistUpdate.TxnStatus = pTransPointHistUpd.TxnStatus;
                        objTranspointHistUpdate.Status = pTransPointHistUpd.Status;
                        objTranspointHistUpdate.Remark = pTransPointHistUpd.Remark;

                        objTranspointHistUpdate.ModifiedBy = pUserNameUpd;
                        objTranspointHistUpdate.ModifiedDate = dCurrentDateVal;
                        objTranspointHistUpdate.BusinessDate = pTransPointHistUpd.BusinessDate;

                        objTranspointHistUpdate.DocumentId = pTransPointHistUpd.DocumentId;
                        objTranspointHistUpdate.StatusUpdateCore = pTransPointHistUpd.StatusUpdateCore;
                        objTranspointHistUpdate.CallApiTxnStatus = pTransPointHistUpd.CallApiTxnStatus;
                        objTranspointHistUpdate.CallApiResRecords = pTransPointHistUpd.CallApiResRecords;
                        objTranspointHistUpdate.CallApiResponseCode = pTransPointHistUpd.CallApiResponseCode;
                        objTranspointHistUpdate.CallApiResponseMsg = pTransPointHistUpd.CallApiResponseMsg;

                        _dbContext.Entry(objTranspointHistUpdate).State = EntityState.Modified;
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                            iResultId = objTranspointHistUpdate.Id;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultId;
        }

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch trong bảng lịch sử thay đổi (Bảng ListOfTransPointHist)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pId">Chỉ số khóa bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfTransPointHist)</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public bool DeleteListOfTransPointHist(string pEventCode, long pId, string pTxnPointCode, string pUserNameDelete, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objTranspointHistUpdate = _dbContext.ListOfTransPointHists.Where(w => w.Id == pId
                                && (string.IsNullOrEmpty(pEventCode) || w.EventCode == pEventCode)
                                && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode == pTxnPointCode)).OrderByDescending(o => o.ModifiedDate).FirstOrDefault();

                if (objTranspointHistUpdate != null && !string.IsNullOrEmpty(objTranspointHistUpdate.TxnPointCode) && objTranspointHistUpdate.Id > 0)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfTransPointHists.Remove(objTranspointHistUpdate);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objTranspointHistUpdate.Status = StatusTrans.Status_Closed.Value;
                        objTranspointHistUpdate.ModifiedBy = pUserNameDelete;
                        objTranspointHistUpdate.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objTranspointHistUpdate).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objTranspointHistUpdate).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objTranspointHistUpdate).Property(x => x.ModifiedDate).IsModified = true;
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





        #region --- Các hàm liên quan đến thực thi vào Corebanking iDC - Nghiệp vụ Quản lý điểm giao dịch ---
        /// <summary>
        /// Hàm thực hiện thêm mới bản ghi Điểm giao dịch vào bảng IDL_IDC.ADD_NEW_TXN_POINT_ITC
        /// Ex: var resultAddTransPoint = _serviceTransPoint.InsertTransactionPoint("002505", "TXN0234501", "17", "08h15-10h15", "Y", "Hà Giang 2", "228.605", "104.963", "TXN", "20251230", "", "N");
        /// </summary>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointId">Mã điểm giao dịch</param>
        /// <param name="pVisitDate">Ngày giao dịch cố định</param>
        /// <param name="pVisitTime">Thời gian giao dịch. Ex: 8h00-12h00</param>
        /// <param name="pTranpointFileGen">Cờ có xuất file không. Giá trị: Y/N</param>
        /// <param name="pTxnPointName">Tên điểm giao dịch</param>
        /// <param name="pLatitude">Tọa độ vĩ độ của điểm giao dịch</param>
        /// <param name="pLongitude">Tọa độ kinh độ của điểm giao dịch</param>
        /// <param name="pTypeCode">Mã ký tự đầu của điểm. TXN</param>
        /// <param name="pMakerDate">Ngày tạo điểm. Định dạng yyyyMMdd</param>
        /// <param name="pErrMsg">Mô tả lỗi. Khi thêm mới để trống</param>
        /// <param name="pSynStatus">Trọng thái đồng bộ để trống</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ExecuteResultModelModel> InsertTransPointIDC(string pPosCode, string pTxnPointId, string pVisitDate, string pVisitTime, string pTranpointFileGen,
                                           string pTxnPointName, string pLatitude, string pLongitude, string pTypeCode,
                                           string pMakerDate,string pErrMsg,string pSynStatus)
        {
            try
            {
                var paramsInsert = new[]
                {
                    new OracleParameter("P_POS_CD", OracleDbType.Varchar2) { Value = pPosCode ?? (object)DBNull.Value },
                    new OracleParameter("P_TXN_ID", OracleDbType.Varchar2) { Value = pTxnPointId ?? (object)DBNull.Value },
                    new OracleParameter("P_VISIT_DT", OracleDbType.Varchar2) { Value = pVisitDate ?? (object)DBNull.Value },
                    new OracleParameter("P_VISIT_TIME", OracleDbType.Varchar2) { Value = pVisitTime ?? (object)DBNull.Value },
                    new OracleParameter("P_TRANPOINT_FILE_GEN", OracleDbType.Varchar2) { Value = pTranpointFileGen ?? (object)DBNull.Value },
                    new OracleParameter("P_TXN_NAME", OracleDbType.Varchar2) { Value = pTxnPointName ?? (object)DBNull.Value },
                    new OracleParameter("P_TRANPOINT_LATITUDE", OracleDbType.Varchar2) { Value = pLatitude ?? (object)DBNull.Value },
                    new OracleParameter("P_TRANPOINT_LONGITUDE", OracleDbType.Varchar2) { Value = pLongitude ?? (object)DBNull.Value },
                    new OracleParameter("P_TYPE_CD", OracleDbType.Varchar2) { Value = pTypeCode ?? (object)DBNull.Value },
                    new OracleParameter("P_MKR_DT", OracleDbType.Varchar2) { Value = pMakerDate ?? (object)DBNull.Value },
                    new OracleParameter("P_ERR_MSG", OracleDbType.Varchar2) { Value = pErrMsg ?? (object)DBNull.Value },
                    new OracleParameter("P_SYN_STATUS", OracleDbType.Varchar2) { Value = pSynStatus ?? (object)DBNull.Value },

                    new OracleParameter("P_ROWS_ADD", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_SUCCESS", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output }
                };

                var sSQL = @"BEGIN VBSP_OSS_UPD.INSERT_ADD_NEW_TXN_POINT_ITC(:P_POS_CD, :P_TXN_ID, :P_VISIT_DT, :P_VISIT_TIME, :P_TRANPOINT_FILE_GEN, :P_TXN_NAME,
                                                :P_TRANPOINT_LATITUDE, :P_TRANPOINT_LONGITUDE, :P_TYPE_CD, :P_MKR_DT, :P_ERR_MSG, :P_SYN_STATUS, 
                                                :P_ROWS_ADD,:P_SUCCESS,:P_MESSAGE); END;";
                await _dbContextIDC.Database.ExecuteSqlRawAsync(sSQL, paramsInsert);
                // Lấy giá trị output
                decimal rowsAdd = Utilities.GetOracleDecimal(paramsInsert[12]);
                decimal successCode = Utilities.GetOracleDecimal(paramsInsert[13], -1);
                string message = paramsInsert[14].Value?.ToString() ?? "";

                var objExecuteResult = new ExecuteResultModelModel
                {
                    RowsAffected = (int)rowsAdd,
                    Success = (int)successCode,
                    Message = message,
                    TxnStatus = successCode switch
                    {
                        1 => ResultValueAPI.ResultValue_Status_Success,
                        0 => ResultValueAPI.ResultValue_Status_Failed,
                        -1 => ResultValueAPI.ResultValue_Status_Errored,
                        _ => ResultValueAPI.ResultValue_Status_Errored
                    }
                };

                return objExecuteResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"InsertTransactionPoint('{pTxnPointId}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm thêm mới điểm giao dịch " +
                                        $"InsertTransactionPoint('{pTxnPointId}') => Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hàm thực hiện tạo bảng ghi từ IDL_IDC.ADD_NEW_TXN_POINT_ITC vào 2 bảng IDL_IDC.TRANPOINT, IDL_IDC.TXIDMAP theo ngày SELECT PC_BUSINESS_DT FROM IDL_IDC.P_CTRL;
        /// Ex: var resultCreateTransPoint = _serviceTransPoint.CreateTransPointByBusinessDateIDC("20260331", "GIANGNT", "GIANGNT");
        /// </summary>
        /// <param name="pMakerDate">Không bắt buộc vì vào trong thủ tục CSDL chỉ sử dụng SELECT PC_BUSINESS_DT INTO LV_BUSINESS_DT FROM IDL_IDC.P_CTRL;</param>
        /// <param name="pCreatedBy">Người tạo lập</param>
        /// <param name="pApproverBy">Ngày tạo lập</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ExecuteResultModelModel> CreateTransPointByBusinessDateIDC(string pMakerDate, string pCreatedBy, string pApproverBy)
        {
            try
            {
                if (!string.IsNullOrEmpty(pCreatedBy))
                    pCreatedBy = (pCreatedBy.Trim().Length > 8) ? pCreatedBy.Substring(0, 8) : pCreatedBy;
                if (!string.IsNullOrEmpty(pApproverBy))
                    pApproverBy = (pApproverBy.Trim().Length > 8) ? pApproverBy.Substring(0, 8) : pApproverBy;

                var paramsInsert = new[]
                {
                    new OracleParameter("P_MKR_DT", OracleDbType.Varchar2) { Value = pMakerDate ?? (object)DBNull.Value },
                    new OracleParameter("P_CREATEDBY", OracleDbType.Varchar2) { Value = pCreatedBy ?? (object)DBNull.Value },
                    new OracleParameter("P_APPROVERBY", OracleDbType.Varchar2) { Value = pApproverBy ?? (object)DBNull.Value },
                    new OracleParameter("P_ROWS_ADD", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_SUCCESS", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output }
                };
                
                var sSQL = @"BEGIN VBSP_OSS_UPD.CREATE_NEW_TXN_POINT_BY_BUSINESSDATE(:P_MKR_DT, :P_CREATEDBY, :P_APPROVERBY, :P_ROWS_ADD,:P_SUCCESS,:P_MESSAGE); END;";
                await _dbContextIDC.Database.ExecuteSqlRawAsync(sSQL, paramsInsert);
                // Lấy giá trị Output
                decimal rowsAdd = Utilities.GetOracleDecimal(paramsInsert[3]);
                decimal successCode = Utilities.GetOracleDecimal(paramsInsert[4], -1);
                var message = paramsInsert[5].Value?.ToString() ?? "";

                var objExecuteResult = new ExecuteResultModelModel
                {
                    RowsAffected = (int)rowsAdd,
                    Success = (int)successCode,
                    Message = message,
                    TxnStatus = successCode switch
                    {
                        1 => ResultValueAPI.ResultValue_Status_Success,
                        0 => ResultValueAPI.ResultValue_Status_Failed,
                        -1 => ResultValueAPI.ResultValue_Status_Errored,
                        _ => ResultValueAPI.ResultValue_Status_Errored
                    }
                };

                return objExecuteResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"CreateTransPointByBusinessDateIDC failed. MakerDate: {pMakerDate} => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm tạo bản ghi từ ADD_NEW_TXN_POINT_ITC cho 2 bảng IDL_IDC.TRANPOINT,IDL_IDC.TXIDMAP " +
                                        $"CreateTransPointByBusinessDateIDC('{pMakerDate}') => Error: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Hàm lấy thông tin ngày giao dịch, EOD của hệ thống Core
        /// </summary>
        /// <param name="pFlagCall">Cờ xác định ngày muốn lấy. Giá trị: 
        ///             '1': Giá trị trả về là ngày BUSINESS_DT (định dạng yyyyMMdd)
        ///             '2': Giá trị trả về là ngày EOD_DT (định dạng yyyyMMdd)
        ///             '3': Giá trị trả về là ngày cuối tháng trước MON_END_DT
        ///             Còn lại là lấy ngày hiện thời hệ thống (máy chủ CSDL)
        /// </param>
        /// <returns></returns>
        public DateTime GetDateInCoreIDC(string pFlagCall)
        {
            try
            {
                string sSQL = @"SELECT VBSP_OSS_GET.FN_GETVALUE_CTRL(:P_FLAGCALL) AS Value FROM DUAL";

                var result = _dbContextIDC.Set<QueryResult>().FromSqlRaw(sSQL,
                                new OracleParameter(":P_FLAGCALL", OracleDbType.Varchar2) { Value = pFlagCall ?? "" }).FirstOrDefault();
                string sValTemp = result?.Value ?? DateTime.Now.ToString(FormatParameters.FORMAT_DATE_INT);

                return CustConverter.StringToDate(sValTemp, FormatParameters.FORMAT_DATE_INT).Date;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Có lỗi khi gọi hàm GetDateInCoreIDC('{pFlagCall}'). Chi tiết lỗi: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Hàm thực hiện thêm mới bản ghi Thay đổi ngày giao dịch vào bảng IDL_LMS.TXN_DATE_CHANGE_ITC
        /// Ex: var resultChangeVisitDate = _serviceTransPoint.InsertTxnDateChangeIDC("TXN0234501", "17", "20260401", "19", "", "", "20260423");
        /// </summary>
        /// <param name="pTxnPointId">Mã điểm giao dịch</param>
        /// <param name="pNewVisitDate">Ngày giao dịch cố định Mới</param>
        /// <param name="pEffDate">Ngày hiệu lực. Định dạng yyyyMMdđ</param>
        /// <param name="pOldVisitDate">Ngày giao dịch cố định Cũ</param>
        /// <param name="pMsgResult">Thông tin cập nhật</param>
        /// <param name="pChangeFlag">Cờ thay đổi</param>
        /// <param name="pMakerDate">Ngày tạo lập yêu cầu thay đổi. Định dạng yyyyMMdđ</param>
        /// <returns>1: Thành công; 0: Không thêm mới được; -1: Lỗi</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ExecuteResultModelModel> InsertTxnDateChangeIDC(string pTxnPointId, string pNewVisitDate, string pEffDate, string pOldVisitDate, string pMsgResult,
                                                                          string pChangeFlag, string pMakerDate)
        {
            try
            {
                var paramsInsert = new[]
                {
                    new OracleParameter("P_TXNPOINT_ID", OracleDbType.Varchar2) { Value = pTxnPointId ?? (object)DBNull.Value },
                    new OracleParameter("P_NEW_VISIT_DATE", OracleDbType.Varchar2) { Value = pNewVisitDate ?? (object)DBNull.Value },
                    new OracleParameter("P_EFF_DATE", OracleDbType.Varchar2) { Value = pEffDate ?? (object)DBNull.Value },
                    new OracleParameter("P_OLD_VISIT_DATE", OracleDbType.Varchar2) { Value = pOldVisitDate ?? (object)DBNull.Value },
                    new OracleParameter("P_MSG_RESULT", OracleDbType.Varchar2) { Value = pMsgResult ?? (object)DBNull.Value },
                    new OracleParameter("P_CHANGE_FLAG", OracleDbType.Varchar2) { Value = pChangeFlag ?? (object)DBNull.Value },
                    new OracleParameter("P_MKR_DT", OracleDbType.Varchar2) { Value = pMakerDate ?? (object)DBNull.Value },
                    new OracleParameter("P_ROWS_ADD", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_SUCCESS", OracleDbType.Decimal) { Direction = ParameterDirection.Output },
                    new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output }
                };

                var sSQL = @"BEGIN VBSP_OSS_UPD.INSERT_TXN_DATE_CHANGE_ITC(:P_TXNPOINT_ID, :P_NEW_VISIT_DATE, :P_EFF_DATE, :P_OLD_VISIT_DATE, :P_MSG_RESULT, :P_CHANGE_FLAG,
                                                :P_MKR_DT, :P_ROWS_ADD,:P_SUCCESS,:P_MESSAGE); END;";
                await _dbContextIDC.Database.ExecuteSqlRawAsync(sSQL, paramsInsert);
                // Lấy giá trị output
                decimal rowsAdd = Utilities.GetOracleDecimal(paramsInsert[7]);
                decimal successCode = Utilities.GetOracleDecimal(paramsInsert[8], -1);
                string message = paramsInsert[9].Value?.ToString() ?? "";

                var objExecuteResult = new ExecuteResultModelModel
                {
                    RowsAffected = (int)rowsAdd,
                    Success = (int)successCode,
                    Message = message,
                    TxnStatus = successCode switch
                    {
                        1 => ResultValueAPI.ResultValue_Status_Success,
                        0 => ResultValueAPI.ResultValue_Status_Failed,
                        -1 => ResultValueAPI.ResultValue_Status_Errored,
                        _ => ResultValueAPI.ResultValue_Status_Errored
                    }
                };

                return objExecuteResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"InsertTxnDateChangeIDC('{pTxnPointId}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm thay đổi ngày giao dịch của điểm giao dịch " +
                                        $"InsertTxnDateChangeIDC('{pTxnPointId}') => Error: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
