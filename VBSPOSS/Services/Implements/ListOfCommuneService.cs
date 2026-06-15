// ListOfCommunesService.cs
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using VBSPOSS.Data;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;
using VBSPOSS.Constants;
using VBSPOSS.Utils;
using VBSPOSS.Data.OSS.Models;

namespace VBSPOSS.Services.Implements
{
    public class ListOfCommuneService : IListOfCommuneService
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
        private readonly ILogger<ListOfCommuneService> _logger;
        private readonly IApiInternalService _internalServiceAPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfCommuneService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        /// <param name="serviceLOV">The serviceLOV<see cref="IListOfValueService"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger{ListOfCommunesService}"/>.</param>
        /// <param name="internalServiceAPI">The internalServiceAPI<see cref="IApiInternalService"/>.</param>
        public ListOfCommuneService(
            ApplicationDbContext dbContext,
            IMapper mapper,
            IListOfValueService serviceLOV,
            ILogger<ListOfCommuneService> logger,
            IApiInternalService internalServiceAPI)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _serviceLOV = serviceLOV;
            _logger = logger;
            _internalServiceAPI = internalServiceAPI;
        }

        /// <summary>
        /// Lấy danh sách xã/phường theo các tiêu chí lọc.
        /// </summary>
        //public List<ListOfCommunesViewModel> GetListOfCommunesSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
        //                                    int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pTxnLocation)
        //{
        //    var answer = new List<ListOfCommunesViewModel>();
        //    if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
        //        pVisitDateBegin = 0;
        //    if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
        //        pVisitDateEnd = 31;
        //    try
        //    {
        //        int iCountTMP = 0;
        //        List<ListOfCommune> listOfTransPointTmp = new List<ListOfCommune>();
        //        var listOfCommuneTmp = _dbContext.ListOfCommunes.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
        //                                    //&& (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
        //                                    && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
        //                                    && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
        //                                    && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
        //                                    && (string.IsNullOrEmpty(pRecordStatus) || w.RecordStatus.Contains(pRecordStatus))
        //                             //   && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
        //                                )
        //                                .Where(delegate (ListOfCommune c)
        //                                {
        //                                    if (string.IsNullOrEmpty(pTxnPointName)
        //                                        || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
        //                                        || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
        //                                        || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
        //                                        )
        //                                        return true;
        //                                    else
        //                                        return false;
        //                                }
        //                                ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectDate).ToList();

        //        if (listOfCommuneTmp != null && listOfCommuneTmp.Count != 0)
        //        {
        //            foreach (var item in listOfCommuneTmp)
        //            {
        //                iCountTMP++;
        //                ListOfCommunesViewModel objItem = new ListOfCommunesViewModel();
        //                objItem = _mapper.Map<ListOfCommunesViewModel>(item);
        //                objItem.OrderNo = iCountTMP;
        //              //  objItem.VisitDateText = item.VisitDate.ToString("D2");
        //                objItem.EffectDateText = item.EffectDate.ToString(FormatParameters.FORMAT_DATE);
        //                objItem.RecordStatusText = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
        //                objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
        //                answer.Add(objItem);
        //            }
        //        }
        //        return answer;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}



        public List<ListOfCommuneViewModel> GetListOfCommuneSearch(
    string pPosCode = "", string pProvinceCode = "", string pDistrictCode = "",
    string pCommuneCode = "", string pSubCommuneCode = "", string pStatus = "",
    string pUserPosCode = "", int pUserGrade = 0)
        {
            var answer = new List<ListOfCommuneViewModel>();
            try
            {
                // Xử lý phân quyền
                string currentPosCode = pPosCode;
                if (string.IsNullOrEmpty(currentPosCode) || currentPosCode == "000100" || currentPosCode == "000199" || currentPosCode == "000196")
                {
                    currentPosCode = (pUserPosCode == "000100" || pUserPosCode == "000199" || pUserPosCode == "000196") ? "" : pUserPosCode;
                }

                if ((pUserGrade == PosGrade.MAIN_POS || pUserGrade == PosGrade.HEAD_POS)
                    && !string.IsNullOrEmpty(currentPosCode) && currentPosCode.Length >= 4)
                {
                    currentPosCode = currentPosCode.Substring(0, 4);
                }

                var listTmp = _dbContext.ListOfCommunes
                    .Where(w => (string.IsNullOrEmpty(currentPosCode) || w.PosCode.StartsWith(currentPosCode))
                             && (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                             && (string.IsNullOrEmpty(pDistrictCode) || w.DistrictCode == pDistrictCode)
                             && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode == pCommuneCode)
                             && (string.IsNullOrEmpty(pSubCommuneCode) || w.SubCommuneCode == pSubCommuneCode)
                             && (string.IsNullOrEmpty(pStatus) || w.Status.ToString() == pStatus || w.RecordStatus == pStatus))
                    .OrderBy(o => o.ProvinceCode)
                    .ThenBy(o => o.DistrictCode)
                    .ThenBy(o => o.CommuneCode)
                    .ThenBy(o => o.SubCommuneCode)
                    .ToList();

                int iCountTMP = 0;
                foreach (var item in listTmp)
                {
                    iCountTMP++;
                    var objItem = _mapper.Map<ListOfCommuneViewModel>(item);
                    // objItem.OrderNo = iCountTMP;   // Bỏ nếu ViewModel không có

                    objItem.ProvinceName = string.IsNullOrEmpty(objItem.ProvinceName) ? "" : objItem.ProvinceName.Trim();
                    objItem.CommuneName = string.IsNullOrEmpty(objItem.CommuneName) ? "" : objItem.CommuneName.Trim();
                    objItem.SubCommuneName = string.IsNullOrEmpty(objItem.SubCommuneName) ? "" : objItem.SubCommuneName.Trim();

                    answer.Add(objItem);
                }

                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
                List<ListOfCommune> listOfCommuneTmp = new List<ListOfCommune>();
                var listOfCommuneTmp01 = _dbContext.ListOfCommunes.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (pStatus == -1 || w.Status == pStatus)
                                        )
                                        .Where(delegate (ListOfCommune c)
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
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectDate).ToList();
                var listOfCommuneWorkTmp = _dbContext.ListOfCommuneWorks
                    .Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                        && (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                        && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode)))
                    .Where(delegate (ListOfCommuneWork c)
                    {
                        return string.IsNullOrEmpty(pTxnPointName)
                            || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
                            || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()));
                    }).ToList();
                if (listOfCommuneTmp01 != null && listOfCommuneTmp01.Count != 0)
                {
                    foreach (var item in listOfCommuneTmp01)
                    {
                        iCountTMP++;
                        ListOfCommuneWorksViewModel objItem = new ListOfCommuneWorksViewModel();
                        objItem = _mapper.Map<ListOfCommuneWorksViewModel>(item);
                        objItem.OrderNo = iCountTMP;
                        objItem.EventName = EventBusinessCode.GetByCode(objItem.EventCode).Description; ;
                        objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EffectDateText = item.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.RecordStatusText = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
                        //if (!string.IsNullOrEmpty(item.IsInCommune))
                        //    objItem.MaApDungList = "1";
                        //else if (!string.IsNullOrEmpty(item.IsInPos))
                        //    objItem.MaApDungList = "2";

                        if (listOfCommuneTmp != null && listOfCommuneTmp.Count != 0)
                        {
                            var objCommuneTmp = listOfCommuneTmp.Where(w => w.TxnPointCode == item.TxnPointCode).OrderByDescending(o => o.ModifiedDate).ThenByDescending(o => o.ApprovalDate).FirstOrDefault();
                            if (objCommuneTmp != null && !string.IsNullOrEmpty(objCommuneTmp.TxnPointCode))
                            {
                                objItem = _mapper.Map<ListOfCommuneWorksViewModel>(objCommuneTmp);
                                objItem.OrderNo = iCountTMP;
                                objItem.EventCode = "";
                                objItem.EventName = "";
                                objItem.VisitDateText = objCommuneTmp.VisitDate.ToString("D2");
                                objItem.EffectDateText = objCommuneTmp.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                                objItem.RecordStatusText = (objCommuneTmp.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                                objItem.StatusText = StatusTrans.GetByValue(objCommuneTmp.Status).Description;
                            }
                        }
                        if (!string.IsNullOrEmpty(pEventCode))
                        {
                            if (objItem.EventCode == pEventCode)
                                listCommuneWorks.Add(objItem);
                        }
                        else listCommuneWorks.Add(objItem);
                    }
                }
                else
                {
                    if (listOfCommuneTmp != null && listOfCommuneTmp.Count != 0)
                    {
                        foreach (var item in listOfCommuneTmp)
                        {
                            iCountTMP++;
                            ListOfCommuneWorksViewModel objItem = new ListOfCommuneWorksViewModel();
                            objItem = _mapper.Map<ListOfCommuneWorksViewModel>(item);
                            objItem.OrderNo = iCountTMP;
                            objItem.EventCode = "";
                            objItem.EventName = "";
                            objItem.VisitDateText = item.VisitDate.ToString("D2");
                            objItem.EffectDateText = item.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                            objItem.RecordStatusText = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                            objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
                            listCommuneWorks.Add(objItem);
                        }
                    }
                }
                return listCommuneWorks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục địa phương
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        public int UpdateListOfCommune(ListOfCommunesViewModel model, string pUserName)
        {
            int iResultId = 0, iSaveChanges = 0;
            try
            {

                var objCommune = _dbContext.ListOfCommunes.Where(m => m.TxnPointCode == model.TxnPointCode).FirstOrDefault();
                DateTime dCurrentDateVal = DateTime.Now;
                if (objCommune != null)
                {
                    //objCommune.ProvinceCode = model.ProvinceCode;
                    objCommune.ProvinceName = model.ProvinceName;
                    objCommune.PosCode = model.PosCode;
                    objCommune.PosName = model.PosName;
                    //objCommune.DistrictCode = model.DistrictCode;
                    objCommune.DistrictName = model.DistrictName;
                    //objCommune.CommuneCode = model.CommuneCode;
                    objCommune.CommuneName = model.CommuneName;
                    //objCommune.SubCommuneCode = model.SubCommuneCode;
                    objCommune.SubCommuneName = model.SubCommuneName;
                    objCommune.DistrictFlag30A = model.DistrictFlag30A;
                    objCommune.AreaEconomic = model.AreaEconomic;
                    objCommune.CommuneFlag135 = model.CommuneFlag135;
                    objCommune.IsNewCountryside = model.IsNewCountryside;
                    objCommune.TxnPointCode = model.TxnPointCode;
                    objCommune.TxnPointName = model.TxnPointName;
                  //  objCommune.VisitDate = model.VisitDate;
                    objCommune.Times = model.Times;
                    objCommune.TimeBegin = model.TimeBegin;
                    objCommune.TimeEnd = model.TimeEnd;
                    objCommune.TimeBeginNum = model.TimeBeginNum;
                    objCommune.TimeEndNum = model.TimeEndNum;
                    objCommune.Hours = model.Hours;
                    objCommune.Minutes = model.Minutes;
                    objCommune.Longitude = model.Longitude;
                    objCommune.Latitude = model.Latitude;
                    objCommune.IsInCommune = model.IsInCommune;
                    objCommune.IsInPos = model.IsInPos;
                    objCommune.IsInterWard = model.IsInterWard;
                    objCommune.InterWardName = model.InterWardName;
                    objCommune.EffectDate = model.EffectDate;
                    objCommune.BusinessDate = model.BusinessDate;
                    objCommune.Status = model.Status;
                    //objCommune.CreatedBy = model.CreatedBy;
                    //objCommune.CreatedDate = dCurrentDateVal;
                    objCommune.ModifiedBy = pUserName;
                    objCommune.ModifiedDate = dCurrentDateVal;
                    //objCommune.ApproverBy = model.ApproverBy;
                    //objCommune.ApprovalDate = dCurrentDateVal;
                    _dbContext.Entry(objCommune).State = EntityState.Modified;
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = iSaveChanges;
                }
                else
                {
                    ListOfCommune objModelCommune = new ListOfCommune();
                    objModelCommune = _mapper.Map<ListOfCommune>(model);

                    //objModelCommune.ProvinceCode = model.ProvinceCode;
                    //objModelCommune.ProvinceName = model.ProvinceName;
                    //objModelCommune.PosCode = model.PosCode;
                    //objModelCommune.PosName = model.PosName;
                    //objModelCommune.DistrictCode = model.DistrictCode;
                    //objModelCommune.DistrictName = model.DistrictName;
                    //objModelCommune.CommuneCode = model.CommuneCode;
                    //objModelCommune.CommuneName = model.CommuneName;
                    //objModelCommune.TxnPointCode = model.TxnPointCode;
                    //objModelCommune.TxnPointName = model.TxnPointName;
                    //objModelCommune.VisitDate = model.VisitDate;
                    //objModelCommune.Times = model.Times;
                    //objModelCommune.TimeBegin = model.TimeBegin;
                    //objModelCommune.TimeEnd = model.TimeEnd;
                    //objModelCommune.TimeBeginNum = model.TimeBeginNum;
                    //objModelCommune.TimeEndNum = model.TimeEndNum;
                    //objModelCommune.Hours = model.Hours;
                    //objModelCommune.Minutes = model.Minutes;
                    //objModelCommune.Longitude = model.Longitude;
                    //objModelCommune.Latitude = model.Latitude;
                    //objModelCommune.IsInCommune = model.IsInCommune;
                    //objModelCommune.IsInPos = model.IsInPos;
                    //objModelCommune.IsInterWard = model.IsInterWard;
                    //objModelCommune.InterWardName = model.InterWardName;
                    //objModelCommune.EffectiveDate = model.EffectiveDate;
                    //objModelCommune.TxnLocation = model.TxnLocation;
                    //objModelCommune.AddressDetail = model.AddressDetail;
                    //objModelCommune.AddressCode = model.AddressCode;
                    //objModelCommune.AddressFull = model.AddressFull;
                    //objModelCommune.PhoneSupport = model.PhoneSupport;
                    //objModelCommune.PhoneSupport01 = model.PhoneSupport01;
                    //objModelCommune.PhoneSupport02 = model.PhoneSupport02;
                    //objModelCommune.TxnStatus = model.TxnStatus;
                    //objModelCommune.Status = model.Status;
                    //objModelCommune.Remark = model.Remark;
                    objModelCommune.CreatedBy = pUserName;
                    objModelCommune.CreatedDate = dCurrentDateVal;
                    objModelCommune.ModifiedBy = pUserName;
                    objModelCommune.ModifiedDate = dCurrentDateVal;
                    objModelCommune.ApproverBy = model.ApproverBy;
                    objModelCommune.ApprovalDate = dCurrentDateVal;

                    _dbContext.ListOfCommunes.Add(objModelCommune);
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
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục địa phương
        /// </summary>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        public bool DeleteListOfCommune(string pTxnPointCode, string pUserName, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objListOfCommune = _dbContext.ListOfCommunes.Where(m => m.TxnPointCode == pTxnPointCode).FirstOrDefault();
                if (objListOfCommune != null)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfCommunes.Remove(objListOfCommune);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objListOfCommune.RecordStatus = StatusLov.StatusClosedPOS;
                        objListOfCommune.ModifiedBy = pUserName;
                        objListOfCommune.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objListOfCommune).Property(x => x.RecordStatus).IsModified = true;
                        _dbContext.Entry(objListOfCommune).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objListOfCommune).Property(x => x.ModifiedDate).IsModified = true;
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
        //public List<ListOfCommuneWorksViewModel> GetListOfCommuneWorkSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
        //                                    int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pEffectDateBegin, string pEffectDateEnd,
        //                                    int pStatus, string pTxnLocation)
        //{
        //    var listCommuneWorkAnswer = new List<ListOfCommuneWorksViewModel>();
        //    if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
        //        pVisitDateBegin = 0;
        //    if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
        //        pVisitDateEnd = 31;
        //    if (string.IsNullOrEmpty(pEffectDateBegin))
        //        pEffectDateBegin = DefaultValue.MinDate.ToString();
        //    if (string.IsNullOrEmpty(pEffectDateEnd))
        //        pEffectDateEnd = DefaultValue.MaxDate.ToString();
        //    DateTime dEffectDateBegin = CustConverter.StringToDate(pEffectDateBegin, FormatParameters.FORMAT_DATE_INT);
        //    DateTime dEffectDateEnd = CustConverter.StringToDate(pEffectDateEnd, FormatParameters.FORMAT_DATE_INT);
        //    try
        //    {
        //        int iCountTMP = 0;
        //        List<ListOfCommuneWork> listOfCommuneTmp = new List<ListOfCommuneWork>();
        //        var listOfCommuneTmp01 = _dbContext.ListOfCommuneWorks.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
        //                                    && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
        //                                    && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
        //                                    && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
        //                                    && (string.IsNullOrEmpty(pRecordStatus) || w.RecordStatus.Contains(pRecordStatus))
        //                                 //   && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
        //                                    && (w.EffectDate >= dEffectDateBegin.Date && w.EffectDate <= dEffectDateEnd.Date)
        //                                    && (pStatus == -1 || w.Status == pStatus)
        //                                )
        //                                .Where(delegate (ListOfCommuneWork c)
        //                                {
        //                                    if (string.IsNullOrEmpty(pTxnPointName)
        //                                        || (c.TxnPointName != null && c.TxnPointName.ToLower().Contains(pTxnPointName.ToLower()))
        //                                        || (c.TxnPointName != null && Utilities.ConvertToUnSign(c.TxnPointName.ToLower()).IndexOf(pTxnPointName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
        //                                        || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnPointName.ToLower()))
        //                                        )
        //                                        return true;
        //                                    else
        //                                        return false;
        //                                }
        //                            ).ToList();
        //        //if (string.IsNullOrEmpty(pTxnLocation))
        //        //{
        //        //    listOfCommuneTmp = listOfCommuneTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectDate).ToList();
        //        //}
        //        //else
        //        //{
        //        //    if (listOfCommuneTmp01 != null && listOfCommuneTmp01.Count != 0)
        //        //    {
        //        //        listOfCommuneTmp = listOfCommuneTmp01.Where(w => w.TxnPointCode != "")
        //        //                            .Where(delegate (ListOfCommuneWork c)
        //        //                            {
        //        //                                if (string.IsNullOrEmpty(pTxnLocation)
        //        //                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
        //        //                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
        //        //                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
        //        //                                    )
        //        //                                    return true;
        //        //                                else
        //        //                                    return false;
        //        //                            }
        //        //                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
        //        //    }
        //        //}

        //        if (listOfCommuneTmp != null && listOfCommuneTmp.Count != 0)
        //        {
        //            foreach (var item in listOfCommuneTmp)
        //            {
        //                iCountTMP++;
        //                ListOfCommuneWorksViewModel objItem = new ListOfCommuneWorksViewModel();
        //                objItem = _mapper.Map<ListOfCommuneWorksViewModel>(item);
        //                objItem.OrderNo = iCountTMP;
        //            //    objItem.VisitDateText = item.VisitDate.ToString("D2");
        //                objItem.EventName = EventBusinessCode.GetByCode(item.EventCode).Description;
        //                objItem.EffectDateText = item.EffectDate.ToString(FormatParameters.FORMAT_DATE);
        //                objItem.BusinessDateText = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
        //                objItem.RecordStatusText = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
        //                objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
        //                if (item.ParentId != 0)
        //                {
        //                    var listOfCommuneHistTmp = _dbContext.ListOfCommuneHists.Where(w => w.Id == item.ParentId).FirstOrDefault();
        //                    if (listOfCommuneHistTmp != null && listOfCommuneHistTmp.Id > 0)
        //                    {
        //                        objItem.ProvinceCodeOldInfo = listOfCommuneHistTmp.ProvinceCode;
        //                        objItem.ProvinceNameOldInfo = listOfCommuneHistTmp.ProvinceName;
        //                        objItem.PosCodeOldInfo = listOfCommuneHistTmp.PosCode;
        //                        objItem.PosNameOldInfo = listOfCommuneHistTmp.PosName;
        //                        objItem.DistrictCodeOldInfo = listOfCommuneHistTmp.DistrictCode;
        //                        objItem.DistrictNameOldInfo = listOfCommuneHistTmp.DistrictName;
        //                        objItem.CommuneCodeOldInfo = listOfCommuneHistTmp.CommuneCode;
        //                        objItem.CommuneNameOldInfo = listOfCommuneHistTmp.CommuneName;
        //                        objItem.SubCommuneCodeOldInfo = listOfCommuneHistTmp.SubCommuneCode;
        //                        objItem.SubCommuneNameOldInfo = listOfCommuneHistTmp.SubCommuneName;
        //                        objItem.DistrictFlag30AOldInfo = listOfCommuneHistTmp.DistrictFlag30A;
        //                        objItem.AreaEconomicOldInfo = listOfCommuneHistTmp.AreaEconomic;
        //                        objItem.CommuneFlag135OldInfo = listOfCommuneHistTmp.CommuneFlag135;
        //                        objItem.Region_01OldInfo = listOfCommuneHistTmp.Region_01;
        //                        objItem.Region_02OldInfo = listOfCommuneHistTmp.Region_02;
        //                        objItem.Region_03OldInfo = listOfCommuneHistTmp.Region_03;
        //                        objItem.Region_04OldInfo = listOfCommuneHistTmp.Region_04;
        //                        objItem.DiffAreaCodeOldInfo = listOfCommuneHistTmp.DiffAreaCode;
        //                        objItem.IsNewCountrysideOldInfo = listOfCommuneHistTmp.IsNewCountryside;
        //                        objItem.TxnPointCodeOldInfo = listOfCommuneHistTmp.TxnPointCode;
        //                        objItem.TxnPointNameOldInfo = listOfCommuneHistTmp.TxnPointName;
        //                      //  objItem.VisitDateOldInfo = listOfCommuneHistTmp.VisitDate;
        //                    //    objItem.VisitDateTextOldInfo = listOfCommuneHistTmp.VisitDate.ToString(FormatParameters.FORMAT_DATE);
        //                        objItem.TimesOldInfo = listOfCommuneHistTmp.Times;
        //                        objItem.TimeBeginOldInfo = listOfCommuneHistTmp.TimeBegin;
        //                        objItem.TimeEndOldInfo = listOfCommuneHistTmp.TimeEnd;
        //                        objItem.TimeBeginNumOldInfo = listOfCommuneHistTmp.TimeBeginNum;
        //                        objItem.TimeEndNumOldInfo = listOfCommuneHistTmp.TimeEndNum;
        //                        objItem.HoursOldInfo = listOfCommuneHistTmp.Hours;
        //                        objItem.MinutesOldInfo = listOfCommuneHistTmp.Minutes;
        //                        objItem.LongitudeOldInfo = listOfCommuneHistTmp.Longitude;
        //                        objItem.LatitudeOldInfo = listOfCommuneHistTmp.Latitude;
        //                        objItem.IsInCommuneOldInfo = listOfCommuneHistTmp.IsInCommune;
        //                        objItem.IsInPosOldInfo = listOfCommuneHistTmp.IsInPos;
        //                        objItem.IsInterWardOldInfo = listOfCommuneHistTmp.IsInterWard;
        //                        objItem.InterWardNameOldInfo = listOfCommuneHistTmp.InterWardName;
        //                        objItem.EffectDateOldInfo = listOfCommuneHistTmp.EffectDate;                                
        //                        //objItem.TxnLocationOldInfo = listOfCommuneHistTmp.TxnLocation;
        //                        //objItem.AddressDetailOldInfo = listOfCommuneHistTmp.AddressDetail;
        //                        //objItem.AddressCodeOldInfo = listOfCommuneHistTmp.AddressCode;
        //                        //objItem.AddressFullOldInfo = listOfCommuneHistTmp.AddressFull;
        //                        //objItem.PhoneSupportOldInfo = listOfCommuneHistTmp.PhoneSupport;
        //                        //objItem.PhoneSupport01OldInfo = listOfCommuneHistTmp.PhoneSupport01;
        //                        //objItem.PhoneSupport02OldInfo = listOfCommuneHistTmp.PhoneSupport02;
        //                        objItem.RecordStatusOldInfo = listOfCommuneHistTmp.RecordStatus;
        //                        objItem.RecordStatusTextOldInfo = (listOfCommuneHistTmp.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
        //                        objItem.StatusOldInfo = listOfCommuneHistTmp.Status;
        //                        objItem.StatusTextOldInfo = StatusTrans.GetByValue(listOfCommuneHistTmp.Status).Description;
        //                        //objItem.RemarkOldInfo = listOfCommuneHistTmp.Remark;
        //                        objItem.CreatedByOldInfo = listOfCommuneHistTmp.CreatedBy;
        //                        objItem.CreatedDateOldInfo = listOfCommuneHistTmp.CreatedDate;
        //                        objItem.ModifiedByOldInfo = listOfCommuneHistTmp.ModifiedBy;
        //                        objItem.ModifiedDateOldInfo = listOfCommuneHistTmp.ModifiedDate;
        //                        objItem.ApproverByOldInfo = listOfCommuneHistTmp.ApproverBy;
        //                        objItem.ApprovalDateOldInfo = listOfCommuneHistTmp.ApprovalDate;
        //                        objItem.BusinessDateOldInfo = listOfCommuneHistTmp.BusinessDate.Value;
        //                        objItem.BusinessDateTextOldInfo = listOfCommuneHistTmp.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
        //                        objItem.DocumentIdOldInfo = listOfCommuneHistTmp.DocumentId.Value;
        //                    }
        //                }
        //                else
        //                {
        //                    objItem.ProvinceCodeOldInfo = item.ProvinceCode;
        //                    objItem.ProvinceNameOldInfo = item.ProvinceName;
        //                    objItem.PosCodeOldInfo = item.PosCode;
        //                    objItem.PosNameOldInfo = item.PosName;
        //                    objItem.DistrictCodeOldInfo = item.DistrictCode;
        //                    objItem.DistrictNameOldInfo = item.DistrictName;
        //                    objItem.CommuneCodeOldInfo = item.CommuneCode;
        //                    objItem.CommuneNameOldInfo = item.CommuneName;
        //                    objItem.SubCommuneCodeOldInfo = item.SubCommuneCode;
        //                    objItem.SubCommuneNameOldInfo = item.SubCommuneName;
        //                    objItem.DistrictFlag30AOldInfo = item.DistrictFlag30A;
        //                    objItem.AreaEconomicOldInfo = item.AreaEconomic;
        //                    objItem.CommuneFlag135OldInfo = item.CommuneFlag135;
        //                    objItem.Region_01OldInfo = item.Region_01;
        //                    objItem.Region_02OldInfo = item.Region_02;
        //                    objItem.Region_03OldInfo = item.Region_03;
        //                    objItem.Region_04OldInfo = item.Region_04;
        //                    objItem.DiffAreaCodeOldInfo = item.DiffAreaCode;
        //                    objItem.IsNewCountrysideOldInfo = item.IsNewCountryside;
        //                    objItem.TxnPointCodeOldInfo = item.TxnPointCode;
        //                    objItem.TxnPointNameOldInfo = item.TxnPointName;
        //                  //  objItem.VisitDateOldInfo = item.VisitDate;
        //                 //   objItem.VisitDateTextOldInfo = item.VisitDate.ToString(FormatParameters.FORMAT_DATE);
        //                    objItem.TimesOldInfo = item.Times;
        //                    objItem.TimeBeginOldInfo = item.TimeBegin;
        //                    objItem.TimeEndOldInfo = item.TimeEnd;
        //                    objItem.TimeBeginNumOldInfo = item.TimeBeginNum;
        //                    objItem.TimeEndNumOldInfo = item.TimeEndNum;
        //                    objItem.HoursOldInfo = item.Hours;
        //                    objItem.MinutesOldInfo = item.Minutes;
        //                    objItem.LongitudeOldInfo = item.Longitude;
        //                    objItem.LatitudeOldInfo = item.Latitude;
        //                    objItem.IsInCommuneOldInfo = item.IsInCommune;
        //                    objItem.IsInPosOldInfo = item.IsInPos;
        //                    objItem.IsInterWardOldInfo = item.IsInterWard;
        //                    objItem.InterWardNameOldInfo = item.InterWardName;
        //                    objItem.EffectDateOldInfo = item.EffectDate;
        //                    //objItem.TxnLocationOldInfo = item.TxnLocation;
        //                    //objItem.AddressDetailOldInfo = item.AddressDetail;
        //                    //objItem.AddressCodeOldInfo = item.AddressCode;
        //                    //objItem.AddressFullOldInfo = item.AddressFull;
        //                    //objItem.PhoneSupportOldInfo = item.PhoneSupport;
        //                    //objItem.PhoneSupport01OldInfo = item.PhoneSupport01;
        //                    //objItem.PhoneSupport02OldInfo = item.PhoneSupport02;
        //                    objItem.RecordStatusOldInfo = item.RecordStatus;
        //                    objItem.RecordStatusTextOldInfo = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
        //                    objItem.StatusOldInfo = item.Status;
        //                    objItem.StatusTextOldInfo = StatusTrans.GetByValue(item.Status).Description;
        //                    //objItem.RemarkOldInfo = item.Remark;
        //                    objItem.CreatedByOldInfo = item.CreatedBy;
        //                    objItem.CreatedDateOldInfo = item.CreatedDate;
        //                    objItem.ModifiedByOldInfo = item.ModifiedBy;
        //                    objItem.ModifiedDateOldInfo = item.ModifiedDate;
        //                    objItem.ApproverByOldInfo = item.ApproverBy;
        //                    objItem.ApprovalDateOldInfo = item.ApprovalDate;
        //                    objItem.BusinessDateOldInfo = item.BusinessDate.Value;
        //                    objItem.BusinessDateTextOldInfo = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);
        //                    objItem.DocumentIdOldInfo = item.DocumentId.Value;
        //                }
        //                listCommuneWorkAnswer.Add(objItem);
        //            }
        //        }
        //        return listCommuneWorkAnswer;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        //add lại

        /// <summary>
        /// Danh sách bản ghi Yêu cầu Thêm mới / Chỉnh sửa Xã - Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        /// <summary>
        /// Danh sách bản ghi Yêu cầu Thêm mới / Chỉnh sửa Xã - Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        /// <summary>
        /// Danh sách bản ghi Yêu cầu Thêm mới / Chỉnh sửa Xã - Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        public List<ListOfCommuneWorksViewModel> GetListOfCommuneWorkSearch(
     string pPosCode = "", string pEventCode = "", string pProvinceCode = "",
     string pCommuneCode = "", string pSubCommuneCode = "", string pStatus = "",
     string pUserPosCode = "", int pUserGrade = 0)
        {
            var answer = new List<ListOfCommuneWorksViewModel>();
            try
            {
                string currentPosCode = pPosCode;
                if (string.IsNullOrEmpty(currentPosCode) || currentPosCode == "000100" || currentPosCode == "000199" || currentPosCode == "000196")
                {
                    currentPosCode = (pUserPosCode == "000100" || pUserPosCode == "000199" || pUserPosCode == "000196") ? "" : pUserPosCode;
                }

                if ((pUserGrade == PosGrade.MAIN_POS || pUserGrade == PosGrade.HEAD_POS)
                    && !string.IsNullOrEmpty(currentPosCode) && currentPosCode.Length >= 4)
                {
                    currentPosCode = currentPosCode.Substring(0, 4);
                }

                var listTmp = _dbContext.ListOfCommuneWorks
                    .Where(w => (string.IsNullOrEmpty(currentPosCode) || w.PosCode.StartsWith(currentPosCode))
                             && (string.IsNullOrEmpty(pEventCode) || w.EventCode == pEventCode)
                             && (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                             && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode == pCommuneCode)
                             && (string.IsNullOrEmpty(pSubCommuneCode) || w.SubCommuneCode == pSubCommuneCode)
                             && (string.IsNullOrEmpty(pStatus) || w.Status.ToString() == pStatus || w.RecordStatus == pStatus))
                    .Select(w => new ListOfCommuneWorksViewModel
                    {
                      //  Id = w.Id,
                        EventCode = w.EventCode,
                        ParentId = w.ParentId,
                        PosCode = w.PosCode,
                        PosName = w.PosName,
                        ProvinceCode = w.ProvinceCode,
                        ProvinceName = w.ProvinceName,
                        DistrictCode = w.DistrictCode,
                        DistrictName = w.DistrictName,
                        CommuneCode = w.CommuneCode,
                        CommuneName = w.CommuneName,
                        SubCommuneCode = w.SubCommuneCode,
                        SubCommuneName = w.SubCommuneName,
                        Status = w.Status,
                        RecordStatus = w.RecordStatus,
                        TxnPointCode = w.TxnPointCode,
                        TxnPointName = w.TxnPointName,
                        CreatedBy = w.CreatedBy,
                        CreatedDate = w.CreatedDate,
                        ModifiedBy = w.ModifiedBy,
                        ModifiedDate = w.ModifiedDate,
                        // Các cột mới
                        DiffAreaCode = w.DiffAreaCode,
                        DocumentId = w.DocumentId,
                        // Thêm các cột khác nếu cần
                    })
                    .OrderByDescending(o => o.CreatedDate)
                    .ThenBy(o => o.ProvinceCode)
                    .ThenBy(o => o.CommuneCode)
                    .ThenBy(o => o.SubCommuneCode)
                    .ToList();

                int iCount = 0;
                foreach (var item in listTmp)
                {
                    iCount++;
                    item.OrderNo = iCount;

                    if (string.IsNullOrEmpty(item.StatusText))
                    {
                        item.StatusText = item.Status switch
                        {
                            1 => "Tạo lập",
                            5 => "Chỉnh sửa",
                            2 => "Trình duyệt cấp chi nhánh",
                            6 => "Phê duyệt chi nhánh",
                            7 => "Từ chối chi nhánh",
                            3 => "Phê duyệt Ban CMNV",
                            4 => "Từ chối Ban CMNV",
                            9 => "Thực thi vào Corebanking",
                            _ => "Không xác định"
                        };
                    }

                    answer.Add(item);
                }

                return answer;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetListOfCommuneWorkSearch Error");
                throw;
            }
        }
        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục địa phương (Bảng ListOfCommuneWork)
        /// </summary>
        /// <param name="pCommuneWorkUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfCommuneWorkViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Số bản ghi được thêm/sửa</returns>
        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng ListOfCommuneWorks
        /// </summary>
        //public int UpdateListOfCommuneWork(ListOfCommuneWorksViewModel pCommuneWorkUpd, string pUserNameUpd, string pFlagCall)
        //{
        //    int iResultCountUpd = 0, iSaveChanges = 0;
        //    try
        //    {
        //        if (pCommuneWorkUpd == null)
        //            return 0;

        //        DateTime dCurrentDateVal = DateTime.Now;

        //        if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() || pFlagCall == "1")
        //        {
        //            ListOfCommuneWork objCommuneAddNew = new ListOfCommuneWork();
        //            objCommuneAddNew = _mapper.Map<ListOfCommuneWork>(pCommuneWorkUpd);

        //            // === BỔ SUNG GIÁ TRỊ BẮT BUỘC ===
        //            objCommuneAddNew.EventCode = "LOCALITY_ADDNEW";

        //            objCommuneAddNew.Status = StatusTrans.Status_Created.Value;
        //            objCommuneAddNew.RecordStatus = "A";
        //            objCommuneAddNew.ParentId = objCommuneAddNew.ParentId == 0 ? 0 : objCommuneAddNew.ParentId;

        //            objCommuneAddNew.DistrictCode = string.IsNullOrEmpty(objCommuneAddNew.DistrictCode) ? "00" : objCommuneAddNew.DistrictCode;
        //            objCommuneAddNew.DistrictName = string.IsNullOrEmpty(objCommuneAddNew.DistrictName) ? "-" : objCommuneAddNew.DistrictName;

        //            objCommuneAddNew.AreaEconomic = string.IsNullOrEmpty(objCommuneAddNew.AreaEconomic) ? "KTNT" : objCommuneAddNew.AreaEconomic;
        //            objCommuneAddNew.CommuneFlag135 = string.IsNullOrEmpty(objCommuneAddNew.CommuneFlag135) ? "1" : objCommuneAddNew.CommuneFlag135;
        //            objCommuneAddNew.IsNewCountryside = string.IsNullOrEmpty(objCommuneAddNew.IsNewCountryside) ? "N" : objCommuneAddNew.IsNewCountryside;
        //            objCommuneAddNew.DiffAreaCode = string.IsNullOrEmpty(objCommuneAddNew.DiffAreaCode) ? "0" : objCommuneAddNew.DiffAreaCode;

        //            // Region
        //            objCommuneAddNew.Region_01 = string.IsNullOrEmpty(objCommuneAddNew.Region_01) ? "0" : objCommuneAddNew.Region_01;
        //            objCommuneAddNew.Region_02 = string.IsNullOrEmpty(objCommuneAddNew.Region_02) ? "0" : objCommuneAddNew.Region_02;
        //            objCommuneAddNew.Region_03 = string.IsNullOrEmpty(objCommuneAddNew.Region_03) ? "0" : objCommuneAddNew.Region_03;
        //            objCommuneAddNew.Region_04 = string.IsNullOrEmpty(objCommuneAddNew.Region_04) ? "0" : objCommuneAddNew.Region_04;

        //            // Ngày tháng
        //            objCommuneAddNew.VisitDate = string.IsNullOrEmpty(objCommuneAddNew.VisitDate) ? "202606" : objCommuneAddNew.VisitDate;


        //            // Cách hiện đại và ngắn gọn nhất

        //            objCommuneAddNew.EffectDate = objCommuneAddNew.EffectDate == default(DateTime)? DateTime.Now.Date: objCommuneAddNew.EffectDate.Date;

        //            objCommuneAddNew.BusinessDate = (objCommuneAddNew.BusinessDate == null || objCommuneAddNew.BusinessDate == default(DateTime))
        //                ? DateTime.Now.Date : objCommuneAddNew.BusinessDate.Value.Date;

        //            // Thông tin người dùng
        //            objCommuneAddNew.CreatedBy = pUserNameUpd;
        //            objCommuneAddNew.CreatedDate = dCurrentDateVal;
        //            objCommuneAddNew.ModifiedBy = pUserNameUpd;
        //            objCommuneAddNew.ModifiedDate = dCurrentDateVal;
        //            objCommuneAddNew.ApproverBy = pUserNameUpd;
        //            objCommuneAddNew.ApprovalDate = dCurrentDateVal;

        //            _dbContext.ListOfCommuneWorks.Add(objCommuneAddNew);
        //            iSaveChanges = _dbContext.SaveChanges();

        //            if (iSaveChanges > 0)
        //                iResultCountUpd = 1;
        //        }

        //        return iResultCountUpd;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, "UpdateListOfCommuneWork Error");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Cập nhật (Thêm mới) yêu cầu Thêm mới Xã hoặc Thôn vào bảng ListOfCommuneWorks
        /// </summary>
        /// <summary>
        /// Hàm Thêm mới Yêu cầu Thêm Xã hoặc Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        /// <summary>
        /// Hàm Thêm mới Yêu cầu Thêm Xã hoặc Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        /// <summary>
        /// Hàm Thêm mới Yêu cầu Thêm Xã hoặc Thôn (bảng ListOfCommuneWorks)
        /// </summary>
        public int UpdateListOfCommuneWork(ListOfCommuneWorksViewModel model, string pUserNameUpd, string pFlagCall, string pPosCode = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrEmpty(pUserNameUpd))
                throw new ArgumentException("Tên người dùng không được để trống");

            if (pFlagCall != EventFlag.EventFlag_Add.Value.ToString() && pFlagCall != "1")
                throw new NotSupportedException("Hàm này hiện chỉ hỗ trợ thêm mới");

            try
            {
                DateTime currentDate = DateTime.Now;

                ValidateByAddType(model);

                if (IsDuplicateRequest(model))
                {
                    throw new InvalidOperationException("Xã/Thôn này đã tồn tại hoặc đang có yêu cầu chờ xử lý.");
                }

                ListOfCommuneWork entity = _mapper.Map<ListOfCommuneWork>(model);

                // ====================== GÁN COMPOSITE KEY ======================
                entity.EventCode = EventBusinessCode.EventCode_Locality_AddNew.Code;
                entity.ParentId = 0;

                entity.PosCode = string.IsNullOrWhiteSpace(model.PosCode)
                    ? (!string.IsNullOrWhiteSpace(pPosCode) ? pPosCode.Trim() : "000100")
                    : model.PosCode.Trim();

                entity.ProvinceCode = model.ProvinceCode?.Trim() ?? throw new ArgumentException("ProvinceCode không được để trống");
                entity.CommuneCode = model.CommuneCode?.Trim() ?? "";

                if (model.AddType == "Xa")
                {
                    entity.SubCommuneCode = "00000";
                    entity.SubCommuneName = "";
                }
                else if (model.AddType == "Thon")
                {
                    entity.SubCommuneCode = model.SubCommuneCode?.Trim() ?? "";
                }

                if (string.IsNullOrWhiteSpace(entity.TxnPointCode))
                {
                    entity.TxnPointCode = $"{entity.ProvinceCode}-{entity.CommuneCode}-{entity.SubCommuneCode}";
                }

                entity.TxnPointName = !string.IsNullOrWhiteSpace(model.TxnPointName)
                    ? model.TxnPointName.Trim()
                    : (model.AddType == "Thon" ? model.SubCommuneName?.Trim() : model.CommuneName?.Trim()) ?? "";

                // ====================== CÁC TRƯỜNG NOT NULL QUAN TRỌNG ======================
                entity.DistrictFlag30A = string.IsNullOrWhiteSpace(entity.DistrictFlag30A) ? "N" : entity.DistrictFlag30A.Trim();

                entity.DistrictCode = string.IsNullOrWhiteSpace(entity.DistrictCode) ? "00" : entity.DistrictCode.Trim();
                entity.DistrictName = string.IsNullOrWhiteSpace(entity.DistrictName) ? "-" : entity.DistrictName.Trim();

                entity.AreaEconomic = string.IsNullOrWhiteSpace(entity.AreaEconomic) ? "KTNT" : entity.AreaEconomic.Trim();
                entity.CommuneFlag135 = string.IsNullOrWhiteSpace(entity.CommuneFlag135) ? "1" : entity.CommuneFlag135.Trim();
                entity.IsNewCountryside = string.IsNullOrWhiteSpace(entity.IsNewCountryside) ? "N" : entity.IsNewCountryside.Trim();
                entity.DiffAreaCode = string.IsNullOrWhiteSpace(entity.DiffAreaCode) ? "0" : entity.DiffAreaCode.Trim();

                entity.Region_01 = string.IsNullOrWhiteSpace(entity.Region_01) ? "0" : entity.Region_01.Trim();
                entity.Region_02 = string.IsNullOrWhiteSpace(entity.Region_02) ? "0" : entity.Region_02.Trim();
                entity.Region_03 = string.IsNullOrWhiteSpace(entity.Region_03) ? "0" : entity.Region_03.Trim();
                entity.Region_04 = string.IsNullOrWhiteSpace(entity.Region_04) ? "0" : entity.Region_04.Trim();

                entity.IsInCommune = string.IsNullOrWhiteSpace(entity.IsInCommune) ? "1" : entity.IsInCommune.Trim();
                entity.IsInPos = string.IsNullOrWhiteSpace(entity.IsInPos) ? "1" : entity.IsInPos.Trim();
                entity.IsInterWard = string.IsNullOrWhiteSpace(entity.IsInterWard) ? "0" : entity.IsInterWard.Trim();

                entity.Status = StatusTrans.Status_Created.Value;
                entity.RecordStatus = "A";

                // Ngày tháng
                entity.EffectDate = (model.EffectDate == default || model.EffectDate == DateTime.MinValue)
                                    ? currentDate.Date : model.EffectDate.Date;

                entity.BusinessDate = (model.BusinessDate == default || model.BusinessDate == DateTime.MinValue)
                                      ? currentDate.Date : model.BusinessDate.Date;

                entity.VisitDate = string.IsNullOrWhiteSpace(model.VisitDate) ? currentDate.ToString("yyyyMM") : model.VisitDate;

                // Audit fields
                entity.CreatedBy = pUserNameUpd;
                entity.CreatedDate = currentDate;
                entity.ModifiedBy = pUserNameUpd;
                entity.ModifiedDate = currentDate;
                entity.ApproverBy = pUserNameUpd;
                entity.ApprovalDate = currentDate;
                // ProvinceName
                entity.ProvinceName = string.IsNullOrWhiteSpace(model.ProvinceName)
                    ? "Chưa có tên"   
                    : model.ProvinceName.Trim();

                //add
               

                entity.AreaEconomic = string.IsNullOrWhiteSpace(entity.AreaEconomic)
                                      ? "KTNT"
                                      : entity.AreaEconomic.Trim();

                entity.CommuneFlag135 = string.IsNullOrWhiteSpace(entity.CommuneFlag135)
                                        ? "1"
                                        : entity.CommuneFlag135.Trim();
                // ====================== LƯU ======================
                _dbContext.ListOfCommuneWorks.Add(entity);
                int result = _dbContext.SaveChanges();

                if (result > 0)
                {
                    _logger?.LogInformation("Thêm mới {AddType} thành công", model.AddType);
                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UpdateListOfCommuneWork Error - AddType: {AddType}", model?.AddType);
                throw;
            }
        }
        private void ValidateByAddType(ListOfCommuneWorksViewModel model)
        {
            if (model.AddType == "Xa")
            {
                if (string.IsNullOrWhiteSpace(model.ProvinceCode))
                    throw new ArgumentException("Vui lòng chọn Tỉnh/Thành phố");

                if (string.IsNullOrWhiteSpace(model.CommuneCode))
                    throw new ArgumentException("Vui lòng nhập Mã Phường/Xã");

                if (string.IsNullOrWhiteSpace(model.CommuneName))
                    throw new ArgumentException("Vui lòng nhập Tên Phường/Xã");
            }
            else if (model.AddType == "Thon")
            {
                if (string.IsNullOrWhiteSpace(model.ProvinceCode))
                    throw new ArgumentException("Vui lòng chọn Tỉnh/Thành phố");

                if (string.IsNullOrWhiteSpace(model.CommuneCode))
                    throw new ArgumentException("Vui lòng chọn Phường/Xã");

                if (string.IsNullOrWhiteSpace(model.SubCommuneCode))
                    throw new ArgumentException("Vui lòng nhập Mã Tổ/Thôn");

                if (string.IsNullOrWhiteSpace(model.SubCommuneName))
                    throw new ArgumentException("Vui lòng nhập Tên Thôn/Bản/Làng");
            }
            else
            {
                throw new ArgumentException("Loại thêm mới không hợp lệ (AddType phải là Xa hoặc Thon)");
            }
        }

        private bool IsDuplicateRequest(ListOfCommuneWorksViewModel model)
        {
            // Kiểm tra trong bảng danh mục chính (ListOfCommunes)
            bool existsInMain = _dbContext.ListOfCommunes.Any(x =>
                x.ProvinceCode == model.ProvinceCode &&
                x.CommuneCode == model.CommuneCode &&
                x.SubCommuneCode == (model.AddType == "Xa" ? "00000" : model.SubCommuneCode));

            if (existsInMain) return true;

            // Kiểm tra trong bảng yêu cầu đang chờ duyệt (không tính các yêu cầu đã bị từ chối)
            bool existsInWork = _dbContext.ListOfCommuneWorks.Any(x =>
                x.ProvinceCode == model.ProvinceCode &&
                x.CommuneCode == model.CommuneCode &&
                x.SubCommuneCode == (model.AddType == "Xa" ? "00000" : model.SubCommuneCode) &&
                x.Status != 4 && x.Status != 7); // 4=Từ chối Ban CMNV, 7=Từ chối Chi nhánh

            return existsInWork;
        }
        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục địa phương (Bảng ListOfCommuneWork)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pParentId">Chỉ số bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfCommuneHist) - Bản ghi trước khi được cập nhật vào bảng ListOfCommuneWork</param>
        /// <param name="pProvinceCode">Mã Tỉnh/TP</param>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pEffectDate">Ngày hiệu lực của danh mục địa phương </param>
        /// <param name="pBusinessDate">Ngày hệ thống Intellect iDC (Ngày hiệu lực thay đổi thông tin của danh mục địa phương của bản ghi)</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public bool DeleteListOfCommuneWork(string pEventCode, long pParentId, string pProvinceCode, string pPosCode, string pTxnPointCode,
                            DateTime pEffectDate, DateTime pBusinessDate, string pUserNameDelete, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objCommuneWorkUpdate = _dbContext.ListOfCommuneWorks.Where(m => m.TxnPointCode == pTxnPointCode
                               && m.EventCode == pEventCode && m.ParentId == pParentId
                               && m.ProvinceCode == pProvinceCode && m.PosCode == pPosCode
                               && m.EffectDate == pEffectDate.Date && m.BusinessDate == pBusinessDate.Date).OrderByDescending(o => o.ModifiedDate).FirstOrDefault();

                if (objCommuneWorkUpdate != null && !string.IsNullOrEmpty(objCommuneWorkUpdate.TxnPointCode))
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfCommuneWorks.Remove(objCommuneWorkUpdate);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objCommuneWorkUpdate.Status = StatusTrans.Status_Closed.Value;
                        objCommuneWorkUpdate.ModifiedBy = pUserNameDelete;
                        objCommuneWorkUpdate.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objCommuneWorkUpdate).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objCommuneWorkUpdate).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objCommuneWorkUpdate).Property(x => x.ModifiedDate).IsModified = true;
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
        /// Hàm lấy Danh sách địa phương ghi nhận thông tin Lịch sử Thêm mới/Thay đổi thông tin (Nguồn bảng ListOfCommuneHist)
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
        /// <returns>Danh sách bản ghi địa phương theo Model ListOfCommuneViewModel</returns>
        public List<ListOfCommuneHistsViewModel> GetListOfCommuneHistSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pRecordStatus, string pEffectDateBegin, string pEffectDateEnd,
                                            int pStatus)
        {
            var listCommuneHistAnswer = new List<ListOfCommuneHistsViewModel>();
            if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
                pVisitDateBegin = 0;
            if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
                pVisitDateEnd = 31;
            if (string.IsNullOrEmpty(pEffectDateBegin))
                pEffectDateBegin = DefaultValue.MinDate.ToString();
            if (string.IsNullOrEmpty(pEffectDateEnd))
                pEffectDateEnd = DefaultValue.MaxDate.ToString();
            DateTime dEffectDateBegin = CustConverter.StringToDate(pEffectDateBegin, FormatParameters.FORMAT_DATE_INT);
            DateTime dEffectDateEnd = CustConverter.StringToDate(pEffectDateEnd, FormatParameters.FORMAT_DATE_INT);
            try
            {
                int iCountTMP = 0;
                List<ListOfCommuneHist> listOfCommuneTmp = new List<ListOfCommuneHist>();
                var listOfCommuneTmp01 = _dbContext.ListOfCommuneHists.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                                            && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode.Contains(pCommuneCode))
                                            && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode.Contains(pTxnPointCode))
                                            && (string.IsNullOrEmpty(pRecordStatus) || w.RecordStatus.Contains(pRecordStatus))
                                         //   && (w.VisitDate >= pVisitDateBegin && w.VisitDate <= pVisitDateEnd)
                                            && (w.EffectDate >= dEffectDateBegin.Date && w.EffectDate <= dEffectDateEnd.Date)
                                            && (pStatus == -1 || w.Status == pStatus)
                                        )
                                        .Where(delegate (ListOfCommuneHist c)
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
                //if (string.IsNullOrEmpty(pTxnLocation))
                //{
                //    listOfCommuneTmp = listOfCommuneTmp01.OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                //}
                //else
                //{
                //    if (listOfCommuneTmp01 != null && listOfCommuneTmp01.Count != 0)
                //    {
                //        listOfCommuneTmp = listOfCommuneTmp01.Where(w => w.TxnPointCode != "")
                //                            .Where(delegate (ListOfCommuneHist c)
                //                            {
                //                                if (string.IsNullOrEmpty(pTxnLocation)
                //                                    || (c.TxnLocation != null && c.TxnLocation.ToLower().Contains(pTxnLocation.ToLower()))
                //                                    || (c.TxnLocation != null && Utilities.ConvertToUnSign(c.TxnLocation.ToLower()).IndexOf(pTxnLocation.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                //                                    || (c.TxnPointCode != null && c.TxnPointCode.ToLower().Contains(pTxnLocation.ToLower()))
                //                                    )
                //                                    return true;
                //                                else
                //                                    return false;
                //                            }
                //                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();
                //    }
                //}

                if (listOfCommuneTmp01 != null && listOfCommuneTmp01.Count != 0)
                {
                    foreach (var item in listOfCommuneTmp01)
                    {
                        iCountTMP++;
                        ListOfCommuneHistsViewModel objItem = new ListOfCommuneHistsViewModel();
                        objItem = _mapper.Map<ListOfCommuneHistsViewModel>(item);
                        objItem.OrderNo = iCountTMP;
                    //    objItem.VisitDateText = item.VisitDate.ToString("D2");
                        objItem.EventName = EventBusinessCode.GetByCode(item.EventCode).Description;
                        objItem.EffectDateText = item.EffectDate.ToString(FormatParameters.FORMAT_DATE);
                        objItem.RecordStatusText = (item.RecordStatus == DefaultValue.StatusOpenA) ? DefaultValue.StatusOpenText : DefaultValue.StatusClosedText;
                        objItem.StatusText = StatusTrans.GetByValue(item.Status).Description;
                        objItem.BusinessDateText = item.BusinessDate.Value.ToString(FormatParameters.FORMAT_DATE);

                        listCommuneHistAnswer.Add(objItem);
                    }
                }
                return listCommuneHistAnswer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng điểm giao dịch (Bảng ListOfCommuneHist)
        /// </summary>
        /// <param name="pCommuneHistUpd">Thông tin điểm giao dịch cập nhật theo Model ListOfCommuneHistViewModel</param>
        /// <param name="pUserNameUpd">Người cập nhật. Nếu truyền vào rỗng lấy theo người cập nhật từ pCommuneHistUpd</param>
        /// <param name="pFlagCall">Cờ xác định sự kiện: 1 - Thêm mới; 2 - Chỉnh sửa (EventFlag.EventFlag_Edit.Value)</param>
        /// <returns>Chỉ số Id bản ghi được Thêm/Chỉnh sửa</returns>
        public long UpdateListOfCommuneHist(ListOfCommuneHistsViewModel pCommuneHistUpd, string pUserNameUpd, string pFlagCall)
        {
            int iSaveChanges = 0;
            long iResultId = 0;
            try
            {
                if (pCommuneHistUpd != null && !string.IsNullOrEmpty(pCommuneHistUpd.TxnPointCode))
                {
                    pCommuneHistUpd.EffectDate = pCommuneHistUpd.EffectDate.Date;
                    pCommuneHistUpd.BusinessDate = pCommuneHistUpd.BusinessDate.Date;
                }
                DateTime dCurrentDateVal = DateTime.Now;
                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                {
                    ListOfCommuneHist objCommuneAddNew = new ListOfCommuneHist();
                    objCommuneAddNew = _mapper.Map<ListOfCommuneHist>(pCommuneHistUpd);
                    objCommuneAddNew.Status = (pCommuneHistUpd.Status == -1) ? StatusTrans.Status_Created.Value : pCommuneHistUpd.Status;
                    objCommuneAddNew.CreatedBy = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.CreatedBy : pUserNameUpd;
                    objCommuneAddNew.CreatedDate = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.CreatedDate : dCurrentDateVal;
                    objCommuneAddNew.ModifiedBy = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.ModifiedBy : pUserNameUpd;
                    objCommuneAddNew.ModifiedDate = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.ModifiedDate : dCurrentDateVal;
                    objCommuneAddNew.ApproverBy = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.ApproverBy : pUserNameUpd;
                    objCommuneAddNew.ApprovalDate = string.IsNullOrEmpty(pUserNameUpd) ? pCommuneHistUpd.ApprovalDate : dCurrentDateVal;
                    _dbContext.ListOfCommuneHists.Add(objCommuneAddNew);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objCommuneAddNew.Id;
                }
                else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                {
                    var objCommuneHistUpdate = _dbContext.ListOfCommuneHists.Where(m => m.TxnPointCode == pCommuneHistUpd.TxnPointCode
                                && m.EventCode == pCommuneHistUpd.EventCode && m.Id == pCommuneHistUpd.Id
                                && m.ProvinceCode == pCommuneHistUpd.ProvinceCode && m.PosCode == pCommuneHistUpd.PosCode
                                && m.EffectDate == pCommuneHistUpd.EffectDate.Date && m.BusinessDate == pCommuneHistUpd.BusinessDate.Date).FirstOrDefault();
                    if (objCommuneHistUpdate != null && !string.IsNullOrEmpty(objCommuneHistUpdate.TxnPointCode))
                    {
                        #region --- Sửa bản ghi bình thường (Áp dụng cho trạng thái bản ghi (Status) tạo lập/chỉnh sửa) ---
                        //objCommuneHistUpdate.Id = pCommuneHistUpd.Id;
                        //objCommuneHistUpdate.ParentId = pCommuneHistUpd.ParentId;
                        //objCommuneHistUpdate.EventCode = pCommuneHistUpd.EventCode;
                        //objCommuneHistUpdate.DateSync = pCommuneHistUpd.DateSync; 
                        //objCommuneHistUpdate.TxnPointCode = pCommuneHistUpd.TxnPointCode; 
                        objCommuneHistUpdate.ProvinceCode = pCommuneHistUpd.ProvinceCode;
                        objCommuneHistUpdate.ProvinceName = pCommuneHistUpd.ProvinceName;
                        objCommuneHistUpdate.PosCode = pCommuneHistUpd.PosCode;
                        objCommuneHistUpdate.PosName = pCommuneHistUpd.PosName;

                        objCommuneHistUpdate.DistrictCode = pCommuneHistUpd.DistrictCode;
                        objCommuneHistUpdate.DistrictName = pCommuneHistUpd.DistrictName;
                        objCommuneHistUpdate.CommuneCode = pCommuneHistUpd.CommuneCode;
                        objCommuneHistUpdate.CommuneName = pCommuneHistUpd.CommuneName;
                        objCommuneHistUpdate.SubCommuneCode = pCommuneHistUpd.SubCommuneCode;
                        objCommuneHistUpdate.SubCommuneName = pCommuneHistUpd.SubCommuneName;
                        objCommuneHistUpdate.DistrictFlag30A = pCommuneHistUpd.DistrictFlag30A;
                        objCommuneHistUpdate.AreaEconomic = pCommuneHistUpd.AreaEconomic;
                        objCommuneHistUpdate.CommuneFlag135 = pCommuneHistUpd.CommuneFlag135;
                        objCommuneHistUpdate.Region_01 = pCommuneHistUpd.Region_01;
                        objCommuneHistUpdate.Region_02 = pCommuneHistUpd.Region_02;
                        objCommuneHistUpdate.Region_03 = pCommuneHistUpd.Region_03;
                        objCommuneHistUpdate.Region_04 = pCommuneHistUpd.Region_04;
                        objCommuneHistUpdate.DiffAreaCode = pCommuneHistUpd.DiffAreaCode;
                        objCommuneHistUpdate.IsNewCountryside = pCommuneHistUpd.IsNewCountryside;
                        objCommuneHistUpdate.TxnPointCode = pCommuneHistUpd.TxnPointCode;
                        objCommuneHistUpdate.TxnPointName = pCommuneHistUpd.TxnPointName;

                    //    objCommuneHistUpdate.VisitDate = pCommuneHistUpd.VisitDate;
                        objCommuneHistUpdate.Times = pCommuneHistUpd.Times;
                        objCommuneHistUpdate.TimeBegin = pCommuneHistUpd.TimeBegin;
                        objCommuneHistUpdate.TimeEnd = pCommuneHistUpd.TimeEnd;
                        objCommuneHistUpdate.TimeBeginNum = pCommuneHistUpd.TimeBeginNum;
                        objCommuneHistUpdate.TimeEndNum = pCommuneHistUpd.TimeEndNum;
                        objCommuneHistUpdate.Hours = pCommuneHistUpd.Hours;
                        objCommuneHistUpdate.Minutes = pCommuneHistUpd.Minutes;
                        objCommuneHistUpdate.Longitude = pCommuneHistUpd.Longitude;
                        objCommuneHistUpdate.Latitude = pCommuneHistUpd.Latitude;
                        objCommuneHistUpdate.IsInCommune = pCommuneHistUpd.IsInCommune;
                        objCommuneHistUpdate.IsInPos = pCommuneHistUpd.IsInPos;
                        objCommuneHistUpdate.IsInterWard = pCommuneHistUpd.IsInterWard;
                        objCommuneHistUpdate.InterWardName = pCommuneHistUpd.InterWardName;
                        //objCommuneHistUpdate.TxnLocation = pCommuneHistUpd.TxnLocation;
                        objCommuneHistUpdate.EffectDate = pCommuneHistUpd.EffectDate;
                        //objCommuneHistUpdate.AddressDetail = pCommuneHistUpd.AddressDetail;
                        //objCommuneHistUpdate.AddressCode = pCommuneHistUpd.AddressCode;
                        //objCommuneHistUpdate.AddressFull = pCommuneHistUpd.AddressFull;
                        //objCommuneHistUpdate.PhoneSupport = pCommuneHistUpd.PhoneSupport;
                        //objCommuneHistUpdate.PhoneSupport01 = pCommuneHistUpd.PhoneSupport01;
                        //objCommuneHistUpdate.PhoneSupport02 = pCommuneHistUpd.PhoneSupport02;
                        objCommuneHistUpdate.RecordStatus = pCommuneHistUpd.RecordStatus;
                        objCommuneHistUpdate.Status = pCommuneHistUpd.Status;
                        //objCommuneHistUpdate.Remark = pCommuneHistUpd.Remark;

                        objCommuneHistUpdate.ModifiedBy = pUserNameUpd;
                        objCommuneHistUpdate.ModifiedDate = dCurrentDateVal;
                        objCommuneHistUpdate.BusinessDate = pCommuneHistUpd.BusinessDate;

                        objCommuneHistUpdate.DocumentId = pCommuneHistUpd.DocumentId;
                        objCommuneHistUpdate.StatusUpdateCore = pCommuneHistUpd.StatusUpdateCore;
                        objCommuneHistUpdate.CallApiTxnStatus = pCommuneHistUpd.CallApiTxnStatus;
                        objCommuneHistUpdate.CallApiResRecords = pCommuneHistUpd.CallApiResRecords;
                        objCommuneHistUpdate.CallApiResponseCode = pCommuneHistUpd.CallApiResponseCode;
                        objCommuneHistUpdate.CallApiResponseMsg = pCommuneHistUpd.CallApiResponseMsg;

                        _dbContext.Entry(objCommuneHistUpdate).State = EntityState.Modified;
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                            iResultId = objCommuneHistUpdate.Id;
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
        /// Hàm Xóa/Đánh dấu xóa bản ghi Điểm giao dịch trong bảng lịch sử thay đổi (Bảng ListOfCommuneHist)
        /// </summary>
        /// <param name="pEventCode">Nghiệp vụ của bản ghi cần xóa</param>
        /// <param name="pId">Chỉ số khóa bản ghi ở bảng HIST (Chỉ số Id ở bảng ListOfCommuneHist)</param>
        /// <param name="pTxnPointCode">Chỉ số xác định danh mục</param>
        /// <param name="pUserNameDelete">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public bool DeleteListOfCommuneHist(string pEventCode, long pId, string pTxnPointCode, string pUserNameDelete, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objCommuneHistUpdate = _dbContext.ListOfCommuneHists.Where(w => w.Id == pId
                                && (string.IsNullOrEmpty(pEventCode) || w.EventCode == pEventCode)
                                && (string.IsNullOrEmpty(pTxnPointCode) || w.TxnPointCode == pTxnPointCode)).OrderByDescending(o => o.ModifiedDate).FirstOrDefault();

                if (objCommuneHistUpdate != null && !string.IsNullOrEmpty(objCommuneHistUpdate.TxnPointCode) && objCommuneHistUpdate.Id > 0)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfCommuneHists.Remove(objCommuneHistUpdate);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objCommuneHistUpdate.Status = StatusTrans.Status_Closed.Value;
                        objCommuneHistUpdate.ModifiedBy = pUserNameDelete;
                        objCommuneHistUpdate.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objCommuneHistUpdate).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objCommuneHistUpdate).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objCommuneHistUpdate).Property(x => x.ModifiedDate).IsModified = true;
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


        /// Hàm trả về Danh sách Xã/Phường/Thôn theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfCommune
        /// </summary>
        /// <summary>
        /// Hàm trả về Danh sách Xã/Phường/Thôn theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfCommune
        /// </summary>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu POS Chi nhánh</param>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pDistrictCode">Mã quận/huyện (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã/phường (Không bắt buộc)</param>
        /// <param name="pSubCommuneCode">Mã tổ/thôn (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền "1" hoặc "A" lấy danh mục mở</param>
        /// <returns>Danh sách bản ghi theo Model ListOfCommuneViewModel</returns>
        /// <summary>
        /// Hàm trả về Danh sách Xã/Phường/Thôn theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfCommune
        /// </summary>
        /// <summary>
        /// Hàm trả về Danh sách Xã/Phường/Thôn theo những điều kiện truyền vào, lấy từ nguồn bảng ListOfCommune
        /// </summary>
        public List<ListOfCommuneViewModel> GetListOfCommuneSearch(string pPosCode = "", string pProvinceCode = "",
            string pDistrictCode = "", string pCommuneCode = "", string pSubCommuneCode = "", string pStatus = "")
        {
            var answer = new List<ListOfCommuneViewModel>();
            try
            {
                int iCountTMP = 0;

                var listOfCommuneTmp = _dbContext.ListOfCommunes
                    .Where(w => (string.IsNullOrEmpty(pPosCode) || w.PosCode.StartsWith(pPosCode))
                             && (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                             && (string.IsNullOrEmpty(pDistrictCode) || w.DistrictCode == pDistrictCode)
                             && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode == pCommuneCode)
                             && (string.IsNullOrEmpty(pSubCommuneCode) || w.SubCommuneCode == pSubCommuneCode)
                             && (string.IsNullOrEmpty(pStatus) || w.Status.ToString() == pStatus || w.RecordStatus == pStatus))
                    .OrderBy(o => o.ProvinceCode)
                    .ThenBy(o => o.DistrictCode)
                    .ThenBy(o => o.CommuneCode)
                    .ThenBy(o => o.SubCommuneCode)
                    .ToList();

                foreach (var item in listOfCommuneTmp)
                {
                    iCountTMP++;
                    ListOfCommuneViewModel objItem = _mapper.Map<ListOfCommuneViewModel>(item);

                    // Không gán OrderNo vì ViewModel không có property này
                    // objItem.OrderNo = iCountTMP;   // ← Bỏ dòng này

                    objItem.ProvinceName = string.IsNullOrEmpty(objItem.ProvinceName) ? "" : objItem.ProvinceName.Trim();
                    objItem.DistrictName = string.IsNullOrEmpty(objItem.DistrictName) ? "" : objItem.DistrictName.Trim();
                    objItem.CommuneName = string.IsNullOrEmpty(objItem.CommuneName) ? "" : objItem.CommuneName.Trim();
                    objItem.SubCommuneName = string.IsNullOrEmpty(objItem.SubCommuneName) ? "" : objItem.SubCommuneName.Trim();

                    // StatusText không có trong ViewModel nên bỏ hoặc gán vào property có sẵn nếu cần
                    // objItem.StatusText = ... 

                    answer.Add(objItem);
                }

                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}