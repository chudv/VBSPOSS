using System;
using System.Linq;
using System.Reflection.Emit;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.ViewModel;
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
        private readonly ILogger<UserManagementIDCService> _logger;
        public UserManagementIDCService(ApplicationDbContext context, IMapper mapper, IApiInternalEsbService apiInternalEsbService, 
                        ILogger<UserManagementIDCService> logger)
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

        /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bảng dữ liệu người dùng trên Intellect iDC UserIDCMaster
        /// </summary>
        /// <param name="pUserIDCMasterUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> SaveUserIDCMaster(UserIDCMasterViewModel pUserIDCMasterUpd, string pUserNameUpd, string pFlagCall)
        {
            int iCountUpdate = 0;
            long iRetIdUpd = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pUserIDCMasterUpd != null && !string.IsNullOrEmpty(pUserIDCMasterUpd.UserId))
                {
                    if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                    {
                        #region --- Cập nhật chỉnh sửa thông tin ---
                        var objUserIDCMasterUpdNew = _dbContext.UserIDCMasters.Where(m => m.Id == pUserIDCMasterUpd.Id && m.UserId == pUserIDCMasterUpd.UserId).FirstOrDefault();
                        if (objUserIDCMasterUpdNew != null && !string.IsNullOrEmpty(objUserIDCMasterUpdNew.UserId))
                        {
                            objUserIDCMasterUpdNew.PosCode = string.IsNullOrEmpty(pUserIDCMasterUpd.PosCode) ? objUserIDCMasterUpdNew.PosCode : pUserIDCMasterUpd.PosCode;
                            objUserIDCMasterUpdNew.PosName = string.IsNullOrEmpty(pUserIDCMasterUpd.PosName) ? objUserIDCMasterUpdNew.PosName : pUserIDCMasterUpd.PosName;
                            objUserIDCMasterUpdNew.StaffId = string.IsNullOrEmpty(pUserIDCMasterUpd.StaffId) ? objUserIDCMasterUpdNew.StaffId : pUserIDCMasterUpd.StaffId;
                            objUserIDCMasterUpdNew.StaffCode = string.IsNullOrEmpty(pUserIDCMasterUpd.StaffCode) ? objUserIDCMasterUpdNew.StaffCode : pUserIDCMasterUpd.StaffCode;
                            objUserIDCMasterUpdNew.FirstName = string.IsNullOrEmpty(pUserIDCMasterUpd.FirstName) ? objUserIDCMasterUpdNew.FirstName : pUserIDCMasterUpd.FirstName;
                            objUserIDCMasterUpdNew.LastName = string.IsNullOrEmpty(pUserIDCMasterUpd.LastName) ? objUserIDCMasterUpdNew.LastName : pUserIDCMasterUpd.LastName;
                            objUserIDCMasterUpdNew.FullName = $"{objUserIDCMasterUpdNew.FirstName.Trim()} {objUserIDCMasterUpdNew.LastName.Trim()}";
                            objUserIDCMasterUpdNew.EmailAddress = string.IsNullOrEmpty(pUserIDCMasterUpd.EmailAddress) ? objUserIDCMasterUpdNew.EmailAddress : pUserIDCMasterUpd.EmailAddress;
                            objUserIDCMasterUpdNew.MobileNumber = string.IsNullOrEmpty(pUserIDCMasterUpd.MobileNumber) ? objUserIDCMasterUpdNew.MobileNumber : pUserIDCMasterUpd.MobileNumber;
                            objUserIDCMasterUpdNew.DateOfBirth = (pUserIDCMasterUpd.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_INT) == "19000101") ? objUserIDCMasterUpdNew.DateOfBirth : pUserIDCMasterUpd.DateOfBirth.Date;
                            objUserIDCMasterUpdNew.GroupName = string.IsNullOrEmpty(pUserIDCMasterUpd.GroupName) ? objUserIDCMasterUpdNew.GroupName : pUserIDCMasterUpd.GroupName;
                            objUserIDCMasterUpdNew.EntityList = string.IsNullOrEmpty(pUserIDCMasterUpd.EntityList) ? objUserIDCMasterUpdNew.EntityList : pUserIDCMasterUpd.EntityList;
                            objUserIDCMasterUpdNew.AuthType = string.IsNullOrEmpty(pUserIDCMasterUpd.AuthType) ? objUserIDCMasterUpdNew.AuthType : pUserIDCMasterUpd.AuthType;
                            objUserIDCMasterUpdNew.UserType = string.IsNullOrEmpty(pUserIDCMasterUpd.UserType) ? objUserIDCMasterUpdNew.UserType : pUserIDCMasterUpd.UserType;
                            objUserIDCMasterUpdNew.MailIdFlag = string.IsNullOrEmpty(pUserIDCMasterUpd.MailIdFlag) ? objUserIDCMasterUpdNew.MailIdFlag : pUserIDCMasterUpd.MailIdFlag;
                            objUserIDCMasterUpdNew.AuthsecType = string.IsNullOrEmpty(pUserIDCMasterUpd.AuthsecType) ? objUserIDCMasterUpdNew.AuthsecType : pUserIDCMasterUpd.AuthsecType;
                            objUserIDCMasterUpdNew.ExtraAttributeUserRole = string.IsNullOrEmpty(pUserIDCMasterUpd.ExtraAttributeUserRole) ? objUserIDCMasterUpdNew.ExtraAttributeUserRole : pUserIDCMasterUpd.ExtraAttributeUserRole;
                            objUserIDCMasterUpdNew.ExtraAttributeBranchCode = string.IsNullOrEmpty(pUserIDCMasterUpd.ExtraAttributeBranchCode) ? objUserIDCMasterUpdNew.ExtraAttributeBranchCode : pUserIDCMasterUpd.ExtraAttributeBranchCode;
                            objUserIDCMasterUpdNew.ExpiryDate = (pUserIDCMasterUpd.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_INT) == "19000101") ? objUserIDCMasterUpdNew.ExpiryDate : pUserIDCMasterUpd.ExpiryDate.Date;
                            objUserIDCMasterUpdNew.Remark = string.IsNullOrEmpty(pUserIDCMasterUpd.Remark) ? objUserIDCMasterUpdNew.Remark : pUserIDCMasterUpd.Remark;
                            objUserIDCMasterUpdNew.OrtherNotes = string.IsNullOrEmpty(pUserIDCMasterUpd.OrtherNotes) ? objUserIDCMasterUpdNew.OrtherNotes : pUserIDCMasterUpd.OrtherNotes;
                            objUserIDCMasterUpdNew.Status = pUserIDCMasterUpd.Status;
                            objUserIDCMasterUpdNew.ModifiedBy = pUserNameUpd; 
                            objUserIDCMasterUpdNew.ModifiedDate = dCurrentDateTmp;
                            objUserIDCMasterUpdNew.ApproverBy = pUserNameUpd; 
                            objUserIDCMasterUpdNew.ApprovalDate = dCurrentDateTmp;
                            _dbContext.UserIDCMasters.Update(objUserIDCMasterUpdNew);
                            int iSaveChanges = await _dbContext.SaveChangesAsync();
                            if (iSaveChanges > 0)
                            {
                                iCountUpdate++;
                                iRetIdUpd = objUserIDCMasterUpdNew.Id;
                            }
                        }
                        #endregion
                    }
                    else if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                    {
                        #region --- Cập nhật thêm mới thông tin (Bao gồm cả chỉnh sửa với bản ghi có Id != 0) ---
                        if (pUserIDCMasterUpd != null && !string.IsNullOrEmpty(pUserIDCMasterUpd.UserId))
                        {
                            UserIDCMaster objUserIDCMasterUpdNew = new UserIDCMaster();
                            objUserIDCMasterUpdNew.Id = 0;
                            objUserIDCMasterUpdNew.PosCode = pUserIDCMasterUpd.PosCode;
                            objUserIDCMasterUpdNew.PosName = pUserIDCMasterUpd.PosName;
                            objUserIDCMasterUpdNew.StaffId = pUserIDCMasterUpd.StaffId;
                            objUserIDCMasterUpdNew.StaffCode = pUserIDCMasterUpd.StaffCode;
                            objUserIDCMasterUpdNew.UserId = pUserIDCMasterUpd.UserId;
                            objUserIDCMasterUpdNew.NickName = pUserIDCMasterUpd.NickName;
                            objUserIDCMasterUpdNew.FirstName = pUserIDCMasterUpd.FirstName;
                            objUserIDCMasterUpdNew.LastName = pUserIDCMasterUpd.LastName;
                            objUserIDCMasterUpdNew.FullName = $"{objUserIDCMasterUpdNew.FirstName.Trim()} {objUserIDCMasterUpdNew.LastName.Trim()}";

                            objUserIDCMasterUpdNew.EmailAddress = pUserIDCMasterUpd.EmailAddress;
                            objUserIDCMasterUpdNew.MobileNumber = pUserIDCMasterUpd.MobileNumber;
                            objUserIDCMasterUpdNew.DateOfBirth = pUserIDCMasterUpd.DateOfBirth.Date;
                            objUserIDCMasterUpdNew.GroupName = pUserIDCMasterUpd.GroupName;
                            objUserIDCMasterUpdNew.EntityList = pUserIDCMasterUpd.EntityList;
                            objUserIDCMasterUpdNew.AuthType = pUserIDCMasterUpd.AuthType;

                            objUserIDCMasterUpdNew.UserType = pUserIDCMasterUpd.UserType;
                            objUserIDCMasterUpdNew.MailIdFlag = pUserIDCMasterUpd.MailIdFlag;
                            objUserIDCMasterUpdNew.AuthsecType = pUserIDCMasterUpd.AuthsecType;
                            objUserIDCMasterUpdNew.ExtraAttributeUserRole = pUserIDCMasterUpd.ExtraAttributeUserRole;
                            objUserIDCMasterUpdNew.ExtraAttributeBranchCode = pUserIDCMasterUpd.ExtraAttributeBranchCode;
                            objUserIDCMasterUpdNew.ExpiryDate = pUserIDCMasterUpd.ExpiryDate.Date;
                            objUserIDCMasterUpdNew.Remark = pUserIDCMasterUpd.Remark;
                            objUserIDCMasterUpdNew.OrtherNotes = pUserIDCMasterUpd.OrtherNotes;
                            objUserIDCMasterUpdNew.Status = pUserIDCMasterUpd.Status;
                            objUserIDCMasterUpdNew.CreatedBy = pUserNameUpd;
                            objUserIDCMasterUpdNew.CreatedDate = dCurrentDateTmp; 
                            objUserIDCMasterUpdNew.ModifiedBy = pUserNameUpd;
                            objUserIDCMasterUpdNew.ModifiedDate = dCurrentDateTmp;
                            objUserIDCMasterUpdNew.ApproverBy = pUserNameUpd;
                            objUserIDCMasterUpdNew.ApprovalDate = dCurrentDateTmp;

                            _dbContext.UserIDCMasters.Add(objUserIDCMasterUpdNew);
                            int iSaveChanges = _dbContext.SaveChanges();
                            if (iSaveChanges > 0)
                            {
                                iCountUpdate++;
                                iRetIdUpd = objUserIDCMasterUpdNew.Id;
                            }


                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                iRetIdUpd = -1;
                Console.WriteLine($"SaveUserIDCMaster('{pUserIDCMasterUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật thông tin cấu hình lãi suất " +
                                        $"SaveUserIDCMaster('{pUserIDCMasterUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return iRetIdUpd;
        }

        /// <summary>
        /// Hàm lấy thông tin người dùng trên iDC qua việc gọi đến API viewUser của ESB đến iDC
        /// Ex: var objUserInfo0 = await _userManagementIDCService.GetUserIDCInfoByApiViewUser("CHUV13");
        /// </summary>
        /// <param name="pUserId">Tên người dùng cần lấy. Ex 'CHUDV13'</param>
        /// <returns>Thông tin user ánh xạ vào Model ViewUserAPIReposeViewModel</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ViewUserAPIReposeViewModel> GetUserIDCInfoByApiViewUser(string pUserId)
        {
            try
            {
                ViewUserAPIReposeViewModel objUserIDCInfo = new ViewUserAPIReposeViewModel();
                if (string.IsNullOrEmpty(pUserId))
                    return objUserIDCInfo;
                pUserId = string.IsNullOrEmpty(pUserId) ? "" : pUserId;
                var _request = new ViewUserRequestViewModel();
                _request.Ticket = ConstValueAPI.Ticket;
                _request.UserId = pUserId;
                var responseAPIViewUser = await _apiInternalEsbService.GetUserIDCInfoByApiViewUser(_request);
                if (responseAPIViewUser == null || responseAPIViewUser.Result == null || !responseAPIViewUser.Result.Any())
                {
                    return new ViewUserAPIReposeViewModel();
                }
                if (responseAPIViewUser.ResponseCode == "00000")
                {
                    if (responseAPIViewUser.Result[0].ServiceStatusResponseViewModel != null)
                    {
                        objUserIDCInfo.ServiceStatusResponseSessionValReq = responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.SessionValReq;
                        objUserIDCInfo.ServiceStatusResponsePrevStatus = responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.PrevStatus;
                        objUserIDCInfo.ServiceStatusResponseResponseCode = responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.ResponseCode;
                        objUserIDCInfo.ServiceStatusResponseResponseMsg = responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.ResponseMsg;
                        if (!string.IsNullOrEmpty(responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.Status))
                            objUserIDCInfo.ServiceStatusResponseStatus = (responseAPIViewUser.Result[0].ServiceStatusResponseViewModel.Status.Trim() == "true") ? true : false;
                        else objUserIDCInfo.ServiceStatusResponseStatus = false;
                    }
                    objUserIDCInfo.LastPWDChanged = string.IsNullOrEmpty(responseAPIViewUser.Result[0].LastPWDChanged) ? "1900-01-01" : responseAPIViewUser.Result[0].LastPWDChanged;
                    objUserIDCInfo.PrimaryChoicebasedAuthType = responseAPIViewUser.Result[0].PrimaryChoicebasedAuthType;
                    objUserIDCInfo.MobileNumber = responseAPIViewUser.Result[0].MobileNumber;
                    objUserIDCInfo.TranAuthType = responseAPIViewUser.Result[0].TranAuthType;
                    objUserIDCInfo.ReqNo = responseAPIViewUser.Result[0].ReqNo;
                    objUserIDCInfo.SelfRegistration = responseAPIViewUser.Result[0].SelfRegistration;
                    objUserIDCInfo.FromRecord = responseAPIViewUser.Result[0].FromRecord;
                    objUserIDCInfo.Language = responseAPIViewUser.Result[0].Language;
                    objUserIDCInfo.UserCreatedDate = responseAPIViewUser.Result[0].UserCreatedDate;
                    objUserIDCInfo.CorporateName = responseAPIViewUser.Result[0].CorporateName;
                    objUserIDCInfo.EmailAddress = responseAPIViewUser.Result[0].EmailAddress;
                    objUserIDCInfo.AuthsecType = responseAPIViewUser.Result[0].AuthsecType;
                    objUserIDCInfo.DOB = responseAPIViewUser.Result[0].DOB;
                    objUserIDCInfo.InvalidAttempt = responseAPIViewUser.Result[0].InvalidAttempt;
                    objUserIDCInfo.UserFromService = responseAPIViewUser.Result[0].UserFromService;
                    if (responseAPIViewUser.Result[0].ExtraAttributeResponseViewModel != null)
                    {
                        objUserIDCInfo.UserRole = responseAPIViewUser.Result[0].ExtraAttributeResponseViewModel.UserRole;
                        objUserIDCInfo.BranchCode = responseAPIViewUser.Result[0].ExtraAttributeResponseViewModel.BranchCode;
                    }
                    else
                    {
                        objUserIDCInfo.UserRole = "";
                        objUserIDCInfo.BranchCode = "";
                    }
                    if (!string.IsNullOrEmpty(objUserIDCInfo.BranchCode))
                        objUserIDCInfo.BranchCode = Convert.ToInt32(objUserIDCInfo.BranchCode.Trim()).ToString("D6");       //string formatted3 = branch.ToString().PadLeft(6, '0');

                    objUserIDCInfo.NickName = responseAPIViewUser.Result[0].NickName;

                    objUserIDCInfo.DefaultBranch = responseAPIViewUser.Result[0].DefaultBranch;
                    objUserIDCInfo.HpinFlag = responseAPIViewUser.Result[0].HpinFlag;
                    objUserIDCInfo.ReqNumber = responseAPIViewUser.Result[0].ReqNumber;
                    objUserIDCInfo.ToRecord = responseAPIViewUser.Result[0].ToRecord;
                    objUserIDCInfo.AppendEntity = responseAPIViewUser.Result[0].AppendEntity;
                    objUserIDCInfo.FirstName = responseAPIViewUser.Result[0].FirstName;
                    objUserIDCInfo.GroupName = responseAPIViewUser.Result[0].GroupName;
                    objUserIDCInfo.IsWebSealUser = responseAPIViewUser.Result[0].IsWebSealUser;
                    objUserIDCInfo.EntityList = responseAPIViewUser.Result[0].EntityList;
                    objUserIDCInfo.UserIdentifierName = responseAPIViewUser.Result[0].UserIdentifierName;
                    objUserIDCInfo.OperationType = responseAPIViewUser.Result[0].OperationType;
                    objUserIDCInfo.UserType = responseAPIViewUser.Result[0].UserType;
                    objUserIDCInfo.EncryptExtraAttrib = responseAPIViewUser.Result[0].EncryptExtraAttrib;
                    objUserIDCInfo.LastName = responseAPIViewUser.Result[0].LastName;
                    objUserIDCInfo.UserIdentifierAlias = responseAPIViewUser.Result[0].UserIdentifierAlias;
                    objUserIDCInfo.UserStatus = responseAPIViewUser.Result[0].UserStatus;
                    objUserIDCInfo.SecondaryChoicebasedAuthType = responseAPIViewUser.Result[0].SecondaryChoicebasedAuthType;
                    objUserIDCInfo.PrevStatus = responseAPIViewUser.Result[0].PrevStatus;
                    objUserIDCInfo.AppendRole = responseAPIViewUser.Result[0].AppendRole;

                    objUserIDCInfo.LastLoginDate = string.IsNullOrEmpty(responseAPIViewUser.Result[0].LastLoginDate) ? "1900-01-01" : responseAPIViewUser.Result[0].LastLoginDate;
                    objUserIDCInfo.ExpiryDate = responseAPIViewUser.Result[0].ExpiryDate;
                    objUserIDCInfo.CheckerDate = responseAPIViewUser.Result[0].CheckerDate;
                    objUserIDCInfo.MailIdFlag = responseAPIViewUser.Result[0].MailIdFlag;
                    objUserIDCInfo.AuthType = responseAPIViewUser.Result[0].AuthType;
                    objUserIDCInfo.CredInfoEncryptType = responseAPIViewUser.Result[0].CredInfoEncryptType;
                    objUserIDCInfo.MakerId = responseAPIViewUser.Result[0].MakerId;
                    objUserIDCInfo.ReqActivity = responseAPIViewUser.Result[0].ReqActivity;
                    objUserIDCInfo.MakerDate = responseAPIViewUser.Result[0].MakerDate;
                    objUserIDCInfo.AppendEntityRoleMap = responseAPIViewUser.Result[0].AppendEntityRoleMap;
                    objUserIDCInfo.Salt = responseAPIViewUser.Result[0].Salt;
                    objUserIDCInfo.UserId = responseAPIViewUser.Result[0].UserId;
                    objUserIDCInfo.CheckerId = responseAPIViewUser.Result[0].CheckerId;
                    objUserIDCInfo.CurrLoginDate = string.IsNullOrEmpty(responseAPIViewUser.Result[0].CurrLoginDate) ? "1900-01-01" : responseAPIViewUser.Result[0].CurrLoginDate;
                }
                return objUserIDCInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy thông tin người dùng GetUserIDCInfoByApiViewUser('{pUserId}') từ API viewUser. Error: {ex.Message}", ex);
            }
        }

        /*
{
    "ticket": "",    
"userId": "CHUV13",
    "nickName": "CHUV13",
    "firstName": "Dương",
    "lastName": "Văn Chữ",
    "emailAddress": "chudv.cctt@gmail.com",
    "mobileNumber": "0908688212",
    "DOB": "1983-10-25",
    "groupName": "IDCROLE,GRPCLMSIT,GRPLMSIT",
    "entityList": "IDCPRODC",
    "authType": 1,
    "userType": 1,
    "mailIdFlag": 4,
    "expiryDate": "2060-12-31",
    "extraAttribute": {
        "BranchCode": "101",
        "UserRole": "POGD"
    }
}
{
    "sessionValReq": "true",
    "prevStatus": 0,
    "responseAttributes": {
        "USR_PASSWD": "7wvgD9PQ"
    },
    "responseCode": 0,
    "responseMsg": "User Successfully Registered",
    "status": "true"
}
         */



    }
}
