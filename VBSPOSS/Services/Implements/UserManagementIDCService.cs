using System.Linq;
using AutoMapper;
using Kendo.Mvc.Extensions;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class UserManagementIDCService: IUserManagementIDCService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IApiInternalEsbService _apiInternalEsbService;
        private readonly ILogger<InterestRateConfigureService> _logger;
        public UserManagementIDCService(ApplicationDbContext context, IMapper mapper, IApiInternalEsbService apiInternalEsbService, 
                        ILogger<InterestRateConfigureService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _apiInternalEsbService = apiInternalEsbService;
            _logger = logger;
        }
        /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Không bắt buộc). Ex: 002721</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        public List<UserIDCMasterViewModel> GetListUserIDCMasters(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode)
        {
            try
            {
                List<string> listOfPosFind = new List<string>();
                listOfPosFind = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && w.Status == StatusLov.StatusOpenPOS
                                                            && (string.IsNullOrEmpty(pMainPosCode) || pMainPosCode == "000100" || (w.MainPosCode == pMainPosCode))
                                                            && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || (w.Code == pPosCode))
                                                            ).OrderBy(o => o.Code).Select(s => s.Code).ToList();
                List<UserIDCMasterViewModel> listUserIDCMasters = new List<UserIDCMasterViewModel>();
                List<UserIDCMasterViewModel> listUserIDCMasters01 = new List<UserIDCMasterViewModel>();

                var listUserIDCMasterTemp = _dbContext.UserIDCMasters.Where(w => w.UserId != ""
                        && (listOfPosFind==null|| listOfPosFind.Count<=0 || listOfPosFind.Contains(w.PosCode))
                        && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || (w.PosCode == pPosCode))
                        && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                        && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode)
                        && (pId == 0 || w.Id == pId))
                        .Where(delegate (UserIDCMaster c)
                        {
                            if (string.IsNullOrEmpty(pFullName)
                                || (c.FullName != null && c.FullName.ToLower().Contains(pFullName.ToLower()))
                                || (c.FullName != null && Utilities.ConvertToUnSign(c.FullName.ToLower()).IndexOf(pFullName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                )
                                return true;
                            else
                                return false;
                        }).OrderByDescending(o => o.PosCode).ThenBy(o => o.GroupName).ThenBy(o => o.UserId).ToList();
                    
                if (listUserIDCMasterTemp != null && listUserIDCMasterTemp.Count != 0)
                {
                    int iCountTemp = 0;
                    foreach (var item in listUserIDCMasterTemp)
                    {
                        iCountTemp++;
                        UserIDCMasterViewModel objItem = new UserIDCMasterViewModel();

                        objItem = _mapper.Map<UserIDCMasterViewModel>(item);
                        objItem.OrderNo = iCountTemp;
                        objItem.StatusText = ConfigStatus.GetByValue(item.Status).Description;
                        listUserIDCMasters.Add(objItem);
                    }
                }
                return listUserIDCMasters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //public async Task<long> SaveUserIDCMaster(UserIDCMasterViewModel objUserIDCMasterUpd, string pUserNameUpd, string pFlagCall)
        //{
        //    int iCountUpdate = 0, iCountTemp = 0;
        //    DateTime dCurrentDateTmp = DateTime.Now;
        //    try
        //    {
        //        if (objUserIDCMasterUpd != null && !string.IsNullOrEmpty(objUserIDCMasterUpd.UserId))
        //        {
        //            if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
        //            {
        //                #region --- Cập nhật chỉnh sửa thông tin ---
        //                List<InterestRateConfigMaster> listUpdateIntRateCofigDetail = new List<InterestRateConfigMaster>();
        //                foreach (var itemUpd in pListInterestRateConfigMasterUpds)
        //                {
        //                    var objIntRateCofigUpdNew = _dbContext.InterestRateConfigMasters.Where(m => m.Id == itemUpd.Id).FirstOrDefault();
        //                    if (objIntRateCofigUpdNew != null && !string.IsNullOrEmpty(objIntRateCofigUpdNew.ProductGroupCode)
        //                            && !string.IsNullOrEmpty(objIntRateCofigUpdNew.ProductCode) && objIntRateCofigUpdNew.Id != 0)
        //                    {
        //                        objIntRateCofigUpdNew.PosCode = itemUpd.PosCode;
        //                        objIntRateCofigUpdNew.PosName = itemUpd.PosName;
        //                        objIntRateCofigUpdNew.ProductGroupCode = string.IsNullOrEmpty(itemUpd.ProductGroupCode) ? "" : itemUpd.ProductGroupCode;//ProductGroupCode.ProductGroupCode_DepositPenal
        //                        objIntRateCofigUpdNew.UserId = string.IsNullOrEmpty(itemUpd.UserId) ? ConstValueAPI.UserId_Call_ApiIDC : itemUpd.UserId;
        //                        objIntRateCofigUpdNew.CircularDate = itemUpd.CircularDate.Value.Date;
        //                        objIntRateCofigUpdNew.CircularRefNum = itemUpd.CircularRefNum;
        //                        objIntRateCofigUpdNew.RecordSerialNo = itemUpd.RecordSerialNo;
        //                        objIntRateCofigUpdNew.ProductCode = itemUpd.ProductCode;
        //                        objIntRateCofigUpdNew.ProductName = itemUpd.ProductName;
        //                        objIntRateCofigUpdNew.AccountTypeCode = itemUpd.AccountTypeCode;
        //                        objIntRateCofigUpdNew.AccountTypeName = itemUpd.AccountTypeName;
        //                        objIntRateCofigUpdNew.AccountSubTypeCode = itemUpd.AccountSubTypeCode;
        //                        objIntRateCofigUpdNew.AccountSubTypeName = itemUpd.AccountSubTypeName;
        //                        objIntRateCofigUpdNew.CurrencyCode = itemUpd.CurrencyCode;
        //                        objIntRateCofigUpdNew.EffectiveDate = itemUpd.EffectiveDate.Date;
        //                        objIntRateCofigUpdNew.ExpiryDate = itemUpd.ExpiryDate.Value.Date;
        //                        objIntRateCofigUpdNew.DebitCreditFlag = itemUpd.DebitCreditFlag;

        //                        objIntRateCofigUpdNew.InterestRate = itemUpd.InterestRate;
        //                        objIntRateCofigUpdNew.PenalRate = itemUpd.PenalRate;
        //                        objIntRateCofigUpdNew.AmountSlab = itemUpd.AmountSlab;
        //                        objIntRateCofigUpdNew.TenorSerialNo = itemUpd.TenorSerialNo;
        //                        objIntRateCofigUpdNew.IntRateType = itemUpd.IntRateType;
        //                        objIntRateCofigUpdNew.SpreadRate = itemUpd.SpreadRate;
        //                        objIntRateCofigUpdNew.Remark = string.IsNullOrEmpty(objIntRateCofigUpdNew.Remark) ? itemUpd.Remark : objIntRateCofigUpdNew.Remark;
        //                        objIntRateCofigUpdNew.OrtherNotes = itemUpd.OrtherNotes;
        //                        objIntRateCofigUpdNew.Status = StatusTrans.Status_Modified.Value;
        //                        objIntRateCofigUpdNew.StatusUpdateCore = itemUpd.StatusUpdateCore;
        //                        objIntRateCofigUpdNew.CallApiTxnStatus = itemUpd.CallApiTxnStatus;
        //                        objIntRateCofigUpdNew.CallApiReqRecordSl = itemUpd.CallApiReqRecordSl;
        //                        objIntRateCofigUpdNew.CallApiResponseCode = itemUpd.CallApiResponseCode;
        //                        objIntRateCofigUpdNew.CallApiResponseMsg = itemUpd.CallApiResponseMsg;

        //                        objIntRateCofigUpdNew.ModifiedDate = dCurrentDateTmp;
        //                        objIntRateCofigUpdNew.ModifiedBy = pUserNameUpd;

        //                        listUpdateIntRateCofigDetail.Add(objIntRateCofigUpdNew);
        //                        iCountTemp++;
        //                    }
        //                }
        //                if (iCountTemp > 0)
        //                {
        //                    _dbContext.InterestRateConfigMasters.UpdateRange(listUpdateIntRateCofigDetail);
        //                    int iSaveChanges = await _dbContext.SaveChangesAsync();

        //                    if (iSaveChanges > 0)
        //                        iCountUpdate = iCountUpdate + iCountTemp;
        //                }
        //                #endregion
        //            }
        //            else if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
        //            {
        //                #region --- Cập nhật thêm mới thông tin (Bao gồm cả chỉnh sửa với bản ghi có Id != 0) ---

        //                List<InterestRateConfigMaster> listAddNewIntRateCofig = new List<InterestRateConfigMaster>();
        //                List<InterestRateConfigMaster> listUpdateIntRateCofig = new List<InterestRateConfigMaster>();

        //                listAddNewIntRateCofig = pListInterestRateConfigMasterUpds.Where(w => w.Id == 0).ToList();
        //                listUpdateIntRateCofig = pListInterestRateConfigMasterUpds.Where(w => w.Id != 0).ToList();
        //                iCountTemp = 0;
        //                if (listAddNewIntRateCofig != null && listAddNewIntRateCofig.Count != 0)
        //                {
        //                    foreach (var itemAdd in listAddNewIntRateCofig)
        //                    {
        //                        itemAdd.Status = StatusTrans.Status_Created.Value;
        //                        itemAdd.CreatedBy = pUserNameUpd;
        //                        itemAdd.CreatedDate = dCurrentDateTmp;
        //                        itemAdd.ModifiedBy = pUserNameUpd;
        //                        itemAdd.ModifiedDate = dCurrentDateTmp;
        //                        itemAdd.ApproverBy = pUserNameUpd;
        //                        itemAdd.ApprovalDate = dCurrentDateTmp;

        //                        itemAdd.StatusUpdateCore = StatusTrans.Status_CallApi_NotUpdated.Value;
        //                        itemAdd.CallApiTxnStatus = "";
        //                        itemAdd.CallApiReqRecordSl = 0;
        //                        itemAdd.CallApiResponseCode = "";
        //                        itemAdd.CallApiResponseMsg = "";
        //                        iCountTemp++;
        //                    }
        //                    if (iCountTemp > 0)
        //                    {
        //                        _dbContext.InterestRateConfigMasters.AddRange(listAddNewIntRateCofig);
        //                        int iSaveChanges = await _dbContext.SaveChangesAsync();
        //                        if (iSaveChanges > 0)
        //                            iCountUpdate = iCountUpdate + iCountTemp;
        //                    }
        //                }
        //                iCountTemp = 0;
        //                if (listUpdateIntRateCofig != null && listUpdateIntRateCofig.Count != 0)
        //                {
        //                    int iCountTempUpd = 0;
        //                    List<InterestRateConfigMaster> listUpdateIntRateCofigNew = new List<InterestRateConfigMaster>();
        //                    foreach (var itemUpd in listUpdateIntRateCofig)
        //                    {
        //                        var objIntRateUpdate = _dbContext.InterestRateConfigMasters.Where(m => m.Id == itemUpd.Id).FirstOrDefault();
        //                        if (objIntRateUpdate != null && !string.IsNullOrEmpty(objIntRateUpdate.ProductGroupCode) && !string.IsNullOrEmpty(objIntRateUpdate.ProductCode)
        //                                && objIntRateUpdate.Id != 0)
        //                        {
        //                            objIntRateUpdate.PosCode = itemUpd.PosCode;
        //                            objIntRateUpdate.PosName = itemUpd.PosName;
        //                            objIntRateUpdate.ProductGroupCode = string.IsNullOrEmpty(itemUpd.ProductGroupCode) ? "" : itemUpd.ProductGroupCode;//ProductGroupCode.ProductGroupCode_DepositPenal
        //                            objIntRateUpdate.UserId = string.IsNullOrEmpty(itemUpd.UserId) ? ConstValueAPI.UserId_Call_ApiIDC : itemUpd.UserId;
        //                            objIntRateUpdate.CircularDate = itemUpd.CircularDate.Value.Date;
        //                            objIntRateUpdate.CircularRefNum = itemUpd.CircularRefNum;
        //                            objIntRateUpdate.RecordSerialNo = itemUpd.RecordSerialNo;
        //                            objIntRateUpdate.ProductCode = itemUpd.ProductCode;
        //                            objIntRateUpdate.ProductName = itemUpd.ProductName;
        //                            objIntRateUpdate.AccountTypeCode = itemUpd.AccountTypeCode;
        //                            objIntRateUpdate.AccountTypeName = itemUpd.AccountTypeName;
        //                            objIntRateUpdate.AccountSubTypeCode = itemUpd.AccountSubTypeCode;
        //                            objIntRateUpdate.AccountSubTypeName = itemUpd.AccountSubTypeName;
        //                            objIntRateUpdate.CurrencyCode = itemUpd.CurrencyCode;
        //                            objIntRateUpdate.EffectiveDate = itemUpd.EffectiveDate.Date;
        //                            objIntRateUpdate.ExpiryDate = itemUpd.ExpiryDate.Value.Date;
        //                            objIntRateUpdate.DebitCreditFlag = itemUpd.DebitCreditFlag;

        //                            objIntRateUpdate.InterestRate = itemUpd.InterestRate;
        //                            objIntRateUpdate.PenalRate = itemUpd.PenalRate;
        //                            objIntRateUpdate.AmountSlab = itemUpd.AmountSlab;
        //                            objIntRateUpdate.TenorSerialNo = itemUpd.TenorSerialNo;
        //                            objIntRateUpdate.IntRateType = itemUpd.IntRateType;
        //                            objIntRateUpdate.SpreadRate = itemUpd.SpreadRate;
        //                            objIntRateUpdate.Remark = itemUpd.Remark;
        //                            objIntRateUpdate.OrtherNotes = itemUpd.OrtherNotes;
        //                            objIntRateUpdate.Status = StatusTrans.Status_Modified.Value;
        //                            objIntRateUpdate.StatusUpdateCore = itemUpd.StatusUpdateCore;
        //                            objIntRateUpdate.CallApiTxnStatus = itemUpd.CallApiTxnStatus;
        //                            objIntRateUpdate.CallApiReqRecordSl = itemUpd.CallApiReqRecordSl;
        //                            objIntRateUpdate.CallApiResponseCode = itemUpd.CallApiResponseCode;
        //                            objIntRateUpdate.CallApiResponseMsg = itemUpd.CallApiResponseMsg;

        //                            objIntRateUpdate.ModifiedDate = dCurrentDateTmp;
        //                            objIntRateUpdate.ModifiedBy = pUserNameUpd;
        //                            listUpdateIntRateCofigNew.Add(objIntRateUpdate);
        //                            iCountTempUpd++;
        //                        }
        //                    }
        //                    if (iCountTempUpd > 0)
        //                    {
        //                        _dbContext.InterestRateConfigMasters.UpdateRange(listUpdateIntRateCofigNew);
        //                        int iSaveChanges = await _dbContext.SaveChangesAsync();

        //                        if (iSaveChanges > 0)
        //                            iCountUpdate = iCountUpdate + iCountTempUpd;
        //                    }
        //                }
        //                #endregion
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        iCountUpdate = -1;
        //        Console.WriteLine($"SaveUserIDCMaster('{objUserIDCMasterUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
        //        throw new Exception($"Lỗi gọi hàm cập nhật thông tin cấu hình lãi suất " +
        //                                $"SaveUserIDCMaster('{objUserIDCMasterUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
        //    }
        //    return iCountUpdate;
        //}






    }
}
