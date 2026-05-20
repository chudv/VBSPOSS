using AutoMapper;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.DirectoryServices.Protocols;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class UserManagementIDCService : IUserManagementIDCService
    {
        private readonly IntellectIDCDbContext _dbContextIDC;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IApiInternalEsbService _apiInternalEsbService;
        private readonly IApiNotiGatewayService _apiNotiGatewayService;
        private readonly ILogger<UserManagementIDCService> _logger;
        private readonly IListOfValueService _serviceLOV;
        private readonly IApiInternalService _internalServiceAPI;


        public UserManagementIDCService(ApplicationDbContext context, IntellectIDCDbContext dbContextIDC, IMapper mapper, IApiInternalEsbService apiInternalEsbService,
                        IApiInternalService internalServiceAPI, IListOfValueService serviceLOV,
                        IApiNotiGatewayService apiNotiGatewayService,ILogger<UserManagementIDCService> logger)
        {
            _dbContext = context;
            _dbContextIDC = dbContextIDC;
            _mapper = mapper;
            _apiInternalEsbService = apiInternalEsbService;
            _logger = logger;
            _serviceLOV = serviceLOV;
            _apiNotiGatewayService = apiNotiGatewayService;
            _internalServiceAPI = internalServiceAPI;
        }
        
        /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <param name="pIsCallGetInfoCoreIDC">Cờ xác định: True: Sau khi lấy thông tin trong UserIDCMaster thì lấy tiếp trong iDC để ra thông tin cuối cùng của người dùng. False: Chỉ lấy thông tin trong UserIDCMaster</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        public async Task<List<UserIDCMasterViewModel>> GetListUserIDCMasters(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, 
                        string pStaffCode, int pStatus, bool pIsCallGetInfoCoreIDC)
        {
            try
            {
                List<string> listOfPosFind = new List<string>();
                listOfPosFind = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && w.Status == StatusLov.StatusOpenPOS
                                                            && (string.IsNullOrEmpty(pMainPosCode) || pMainPosCode == "000100" || (w.MainPosCode.StartsWith(pMainPosCode)))
                                                            && (string.IsNullOrEmpty(pPosCode) || w.Code.StartsWith(pPosCode))
                                                       ).OrderBy(o => o.Code).Select(s => s.Code).ToList();

                List<UserIDCMasterViewModel> listUserIDCMasters = new List<UserIDCMasterViewModel>();
                var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                var listUserIDCMasterTemp = _dbContext.UserIDCMasters.Where(w => w.Id == pId || (pId == 0
                        && (listOfPosFind == null || listOfPosFind.Count <= 0 || listOfPosFind.Contains(w.PosCode))
                        && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || (w.PosCode == pPosCode))
                        && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                        && (pStatus == -1 || w.Status == pStatus)
                        && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode)))
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
                    var listOfPosTmp = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && w.Status == StatusLov.StatusOpenPOS).OrderBy(o => o.Code).ToList();

                    int iCountTemp = 0;
                    foreach (var item in listUserIDCMasterTemp)
                    {
                        iCountTemp++;
                        UserIDCMasterViewModel objItem = new UserIDCMasterViewModel();
                        objItem = _mapper.Map<UserIDCMasterViewModel>(item);
                        objItem.OrderNo = iCountTemp;
                        objItem.StatusText = StatusBusinessFlow.GetByValue(item.Status).Description;
                        objItem.ExistsInCore = 0;
                        if (pIsCallGetInfoCoreIDC)
                        {
                            var objUserInfoIDC = await GetUserIDCInfoByApiViewUser(objItem.UserId);
                            if (objUserInfoIDC != null && !string.IsNullOrEmpty(objUserInfoIDC.NickName))
                            {
                                objItem.PosCode = string.IsNullOrEmpty(objUserInfoIDC.BranchCode) ? item.PosCode : objUserInfoIDC.BranchCode.Trim().PadLeft(6, '0');
                                if (!string.IsNullOrEmpty(objUserInfoIDC.BranchCode))
                                {
                                    if (objUserInfoIDC.BranchCode.Trim().PadLeft(6, '0') != item.PosCode)
                                    {
                                        if(listOfPosTmp!=null && listOfPosTmp.Count!=0)
                                        {
                                            objItem.PosName = listOfPosTmp.Where(w => w.Code == objItem.PosCode).Select(s => s.Name).FirstOrDefault();
                                        }    
                                    }
                                }
                                objItem.NickName = objUserInfoIDC.NickName;
                                objItem.FirstName = objUserInfoIDC.FirstName;
                                objItem.LastName = objUserInfoIDC.LastName;
                                objItem.FullName = $"{objItem.FirstName} {objItem.LastName}";
                                objItem.EmailAddress = objUserInfoIDC.EmailAddress;
                                objItem.MobileNumber = objUserInfoIDC.MobileNumber;
                                objItem.DateOfBirth = CustConverter.StringToDate(objUserInfoIDC.DOB.Trim().Replace("-", "").Replace("/", ""), FormatParameters.FORMAT_DATE_INT).Date;//yyyy-MM-dd
                                objItem.GroupName = objUserInfoIDC.GroupName;
                                objItem.EntityList = objUserInfoIDC.EntityList;
                                objItem.AuthType = objUserInfoIDC.AuthType.ToString();
                                objItem.UserType = objUserInfoIDC.UserType.ToString();
                                objItem.MailIdFlag = objUserInfoIDC.MailIdFlag;
                                objItem.AuthsecType = (string.IsNullOrEmpty(objUserInfoIDC.AuthsecType) || objUserInfoIDC.AuthsecType == "0") ? AuthSecType.AuthSecType_Native.Value.ToString() : objUserInfoIDC.AuthsecType;
                                objItem.ExtraAttributeUserRole = objUserInfoIDC.UserRole;
                                objItem.ExtraAttributeBranchCode = objUserInfoIDC.BranchCode;
                                objItem.ExpiryDate = CustConverter.StringToDate(objUserInfoIDC.ExpiryDate.Trim().Replace("-", "").Replace("/", ""), FormatParameters.FORMAT_DATE_INT).Date;//yyyy-MM-dd
                                objItem.UserStatus = objUserInfoIDC.UserStatus.ToString();
                                string sUserCreatedDate = objUserInfoIDC.UserCreatedDate.Trim();
                                if (!string.IsNullOrEmpty(sUserCreatedDate) && sUserCreatedDate.Length >= 10)
                                    sUserCreatedDate = sUserCreatedDate.Substring(0, 10);
                                sUserCreatedDate = sUserCreatedDate.Replace("-", "").Replace("/", "");
                                objItem.StartDate = CustConverter.StringToDate(sUserCreatedDate, FormatParameters.FORMAT_DATE_INT).Date;
                                objItem.ExistsInCore = 1;
                            }
                        }
                        if (listRoleUsers != null && listRoleUsers.Count != 0)
                        {
                            objItem.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objItem.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                            objItem.RoleToTransferCashName = (objItem.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                            objItem.RoleToTransferCashDescription = (objItem.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                            objItem.GroupNameDetail = $"{objItem.GroupName} - {listRoleUsers.Where(w => w.Code == objItem.GroupName).Select(s => s.Name).FirstOrDefault()}";
                        }
                        objItem.MailIdFlagName = int.TryParse(objItem.MailIdFlag, out var y) ? MailIdFlag.GetByValue(y)?.Description : "";
                        objItem.AuthsecTypeName = int.TryParse(objItem.AuthsecType, out var v) ? AuthSecType.GetByValue(v)?.Description : "";

                        if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                            objItem.UserStatusText = "Khóa (Đóng)";
                        else if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                            objItem.UserStatusText = "Mở (Bình thường)";
                        else if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                            objItem.UserStatusText = "Tạm khóa (Lock)";
                        else objItem.UserStatusText = "Không xác định";

                        //Lấy một số thông tin trong QLNS
                        objItem.GenderCode = "";
                        objItem.GenderText = "";
                        objItem.StaffPosCode = "";
                        objItem.StaffPosName = "";
                        objItem.StaffDepartmentCode = "";
                        objItem.StaffDepartmentName = "";
                        objItem.StaffPositionCode = "";
                        objItem.StaffPositionName = "";
                        var objStaffInfoTemp = await _internalServiceAPI.GetListStaffByStaffId(objItem.StaffId);
                        if (objStaffInfoTemp != null && objStaffInfoTemp.Success && objStaffInfoTemp.Result != null && objStaffInfoTemp.Result.Count > 0)
                        {
                            var objStaffInfo = objStaffInfoTemp.Result.FirstOrDefault();
                            if (objStaffInfo != null && !string.IsNullOrEmpty(objStaffInfo.StaffId))
                            {
                                objItem.GenderCode = objStaffInfo.GenderCode;
                                objItem.GenderText = objStaffInfo.GenderText;
                                objItem.StaffPosCode = objStaffInfo.StaffPosCode;
                                objItem.StaffPosName = objStaffInfo.StaffPosName;
                                objItem.StaffDepartmentCode = objStaffInfo.StaffDepartmentCode;
                                objItem.StaffDepartmentName = objStaffInfo.StaffDepartmentName;
                                objItem.StaffPositionCode = objStaffInfo.StaffPositionCode;
                                objItem.StaffPositionName = objStaffInfo.StaffPositionName;
                                objItem.StaffEmail = objStaffInfo.StaffEmail;
                                objItem.StaffMobileNo = objStaffInfo.StaffMobileNo;
                            }
                        }
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
            int iCountUpdate = 0, iCountManagerUpd = 0;
            long iRetIdUpd = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pUserIDCMasterUpd != null && !string.IsNullOrEmpty(pUserIDCMasterUpd.UserId))
                {
                    if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                    {
                        #region --- Cập nhật chỉnh sửa thông tin ---
                        var objUserIDCMasterUpdNew = _dbContext.UserIDCMasters.Where(m => m.UserId == pUserIDCMasterUpd.UserId).OrderByDescending(o => o.Id).FirstOrDefault();
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
                            objUserIDCMasterUpdNew.FullName = pUserIDCMasterUpd.FirstName + " " + pUserIDCMasterUpd.LastName;
                            //if (!string.IsNullOrWhiteSpace(pUserIDCMasterUpd.FullName))
                            //{
                            //    var partName= pUserIDCMasterUpd.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            //    if (partName.Length > 0)
                            //    {
                            //        objUserIDCMasterUpdNew.FirstName = partName[0];
                            //        objUserIDCMasterUpdNew.LastName = string.Join(" ", partName.Skip(1));
                            //    }
                            //}
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
                            objUserIDCMasterUpdNew.StartDate = pUserIDCMasterUpd.StartDate;
                            objUserIDCMasterUpdNew.IpSetCode = pUserIDCMasterUpd.IpSetCode; //Xử lý khi gọi API
                            objUserIDCMasterUpdNew.IpSetDetail = pUserIDCMasterUpd.IpSetDetail; //Xử lý khi gọi API
                            objUserIDCMasterUpdNew.RestrictionFlag = pUserIDCMasterUpd.RestrictionFlag; //Xử lý khi gọi API
                            objUserIDCMasterUpdNew.SubType = pUserIDCMasterUpd.SubType;
                            objUserIDCMasterUpdNew.EffectiveDate = dCurrentDateTmp;
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

       

        /*
         /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bảng dữ liệu quản lý người dùng trên Intellect iDC UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> SaveUserManagementIDC(UserManagementIDCViewModel pUserManagementUpd, string pUserNameUpd, string pFlagCall, string pButtonType)
        {
            int iCountUpdate = 0, iSaveChanges = 0, iCreateUserIDC = 0;
            long iRetIdUpd = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pUserManagementUpd != null && !string.IsNullOrEmpty(pUserManagementUpd.UserId))
                {
                    var objUserManagementIDCsUpdNew = _dbContext.UserManagementIDCs.Where(m => m.Id == pUserManagementUpd.Id && m.UserId == pUserManagementUpd.UserId).FirstOrDefault();
                    //Thêm mới vào bảng trong trường hợp thêm mới người dùng
                    if (objUserManagementIDCsUpdNew == null || pButtonType == FunctionTypeFlag.FunctionTypeFlag_EDIT.Value.ToString())
                    {
                        #region --- Cập nhật thêm mới thông tin ---
                        UserManagementIDC objUserManagementUpdNew = new UserManagementIDC();
                        objUserManagementUpdNew.Id = 0;
                        objUserManagementUpdNew.PosCode = pUserManagementUpd.PosCode;
                        objUserManagementUpdNew.PosName = pUserManagementUpd.PosName;
                        objUserManagementUpdNew.StaffId = pUserManagementUpd.StaffId;
                        objUserManagementUpdNew.StaffCode = pUserManagementUpd.StaffCode;
                        objUserManagementUpdNew.UserId = pUserManagementUpd.UserId;
                        if (!string.IsNullOrWhiteSpace(pUserManagementUpd.FullName))
                        {
                            var partName = pUserManagementUpd.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (partName.Length > 0)
                            {
                                objUserManagementUpdNew.LastName = partName[^1];
                                objUserManagementUpdNew.FirstName = string.Join(" ", partName.Take(partName.Length - 1));
                            }
                        }
                        objUserManagementUpdNew.NickName = pUserManagementUpd.NickName;
                        objUserManagementUpdNew.EmailAddress = pUserManagementUpd.EmailAddress;
                        objUserManagementUpdNew.MobileNumber = pUserManagementUpd.MobileNumber;
                        objUserManagementUpdNew.DateOfBirth = pUserManagementUpd.DateOfBirth.Date;
                        objUserManagementUpdNew.GroupName = pUserManagementUpd.GroupName;
                        objUserManagementUpdNew.EntityList = pUserManagementUpd.EntityList;
                        objUserManagementUpdNew.MailIdFlag = pUserManagementUpd.MailIdFlag;
                        if (!string.IsNullOrWhiteSpace(pUserManagementUpd.RoleToTransferCashValue))
                        {
                            objUserManagementUpdNew.AuthsecType = (pUserManagementUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? "17" : "0";
                        }
                        objUserManagementUpdNew.ExtraAttributeUserRole = pUserManagementUpd.ExtraAttributeUserRole;
                        objUserManagementUpdNew.ExtraAttributeBranchCode = pUserManagementUpd.ExtraAttributeBranchCode;
                        objUserManagementUpdNew.EffectiveDate = pUserManagementUpd.EffectiveDate;
                        objUserManagementUpdNew.ExpiryDate = pUserManagementUpd.ExpiryDate.Date;
                        objUserManagementUpdNew.Remark = pUserManagementUpd.Remark;
                        objUserManagementUpdNew.OrtherNotes = pUserManagementUpd.OrtherNotes;
                        objUserManagementUpdNew.Ticket = "";//Xử lý khi gọi API
                        objUserManagementUpdNew.Status = pUserManagementUpd.Status;
                        objUserManagementUpdNew.StatusUpdateCore = 0; //Xử lý khi gọi API
                        objUserManagementUpdNew.SessionValReq = true; //Xử lý khi gọi API
                        objUserManagementUpdNew.PrevStatus = 0; //Xử lý khi gọi API
                        objUserManagementUpdNew.ResponseAttributes = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.CallApiStatus = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.CallApiReqRecordSl = 0; //Xử lý khi gọi API
                        objUserManagementUpdNew.CallApiResponseCode = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.CallApiResponseMsg = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.IpSetCode = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.IpSetDetail = ""; //Xử lý khi gọi API
                        objUserManagementUpdNew.RestrictionFlag = 0; //Xử lý khi gọi API
                        objUserManagementUpdNew.SubType = "1";                     
                        // Thêm mới người dùng
                        if (pButtonType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Value.ToString())
                        {
                            objUserManagementUpdNew.FunctionType = FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code;
                            objUserManagementUpdNew.AuthType = pButtonType;
                            objUserManagementUpdNew.UserType = pButtonType;
                            objUserManagementUpdNew.CreatedBy = pUserNameUpd;
                            objUserManagementUpdNew.CreatedDate = dCurrentDateTmp;
                            objUserManagementUpdNew.ModifiedBy = "";
                            objUserManagementUpdNew.ModifiedDate = dCurrentDateTmp;
                            objUserManagementUpdNew.ApproverBy = "";
                            objUserManagementUpdNew.ApprovalDate = dCurrentDateTmp;
                            objUserManagementUpdNew.StartDate = pUserManagementUpd.StartDate;
                            objUserManagementUpdNew.CallApiAutoGeneratedPassword = "";
                        }
                        // Thêm mới theo chức năng yêu cầu chỉnh sửa
                        else
                        {
                            objUserManagementUpdNew.FunctionType = pUserManagementUpd.FunctionType;
                            objUserManagementUpdNew.AuthType = pUserManagementUpd.AuthType;
                            objUserManagementUpdNew.UserType = pUserManagementUpd.UserType;
                            objUserManagementUpdNew.CreatedBy = pUserNameUpd;
                            objUserManagementUpdNew.CreatedDate = dCurrentDateTmp;
                            objUserManagementUpdNew.ModifiedBy = pUserNameUpd;
                            objUserManagementUpdNew.ModifiedDate = dCurrentDateTmp;
                            objUserManagementUpdNew.ApproverBy = pUserManagementUpd.ApproverBy;
                            objUserManagementUpdNew.ApprovalDate = pUserManagementUpd.ApprovalDate;
                            objUserManagementUpdNew.StartDate = pUserManagementUpd.StartDate;
                            objUserManagementUpdNew.EffectiveDate = pUserManagementUpd.StartDate;
                            objUserManagementUpdNew.CallApiAutoGeneratedPassword = pUserManagementUpd.CallApiAutoGeneratedPassword;
                        }
                        _dbContext.UserManagementIDCs.Add(objUserManagementUpdNew);
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                        {
                            iCountUpdate++;
                            iRetIdUpd = objUserManagementUpdNew.Id;
                        }
                        #endregion
                    }
                    //Xử lý trường hợp chỉnh sửa người dùng (Chưa thực hiện phê duyệt)
                    else if (objUserManagementIDCsUpdNew != null && pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                    {
                        objUserManagementIDCsUpdNew.Id = pUserManagementUpd.Id;
                        objUserManagementIDCsUpdNew.FunctionType = pUserManagementUpd.FunctionType;
                        objUserManagementIDCsUpdNew.PosCode = pUserManagementUpd.PosCode;
                        objUserManagementIDCsUpdNew.PosName = pUserManagementUpd.PosName;
                        objUserManagementIDCsUpdNew.StaffId = pUserManagementUpd.StaffId;
                        objUserManagementIDCsUpdNew.StaffCode = pUserManagementUpd.StaffCode;
                        objUserManagementIDCsUpdNew.UserId = pUserManagementUpd.UserId;
                        objUserManagementIDCsUpdNew.FirstName = pUserManagementUpd.FirstName;
                        objUserManagementIDCsUpdNew.LastName = pUserManagementUpd.LastName;
                        objUserManagementIDCsUpdNew.NickName = pUserManagementUpd.NickName;
                        objUserManagementIDCsUpdNew.EmailAddress = pUserManagementUpd.EmailAddress;
                        objUserManagementIDCsUpdNew.MobileNumber = pUserManagementUpd.MobileNumber;
                        objUserManagementIDCsUpdNew.DateOfBirth = pUserManagementUpd.DateOfBirth.Date;
                        objUserManagementIDCsUpdNew.GroupName = pUserManagementUpd.GroupName;
                        objUserManagementIDCsUpdNew.EntityList = pUserManagementUpd.EntityList;
                        objUserManagementIDCsUpdNew.AuthType = pUserManagementUpd.AuthType;
                        objUserManagementIDCsUpdNew.UserType = pUserManagementUpd.UserType;
                        objUserManagementIDCsUpdNew.MailIdFlag = pUserManagementUpd.MailIdFlag;
                        objUserManagementIDCsUpdNew.AuthsecType = pUserManagementUpd.AuthsecType;
                        objUserManagementIDCsUpdNew.ExtraAttributeUserRole = pUserManagementUpd.ExtraAttributeUserRole;
                        objUserManagementIDCsUpdNew.ExtraAttributeBranchCode = pUserManagementUpd.ExtraAttributeBranchCode;
                        objUserManagementIDCsUpdNew.EffectiveDate = pUserManagementUpd.EffectiveDate;
                        objUserManagementIDCsUpdNew.ExpiryDate = pUserManagementUpd.ExpiryDate.Date;
                        objUserManagementIDCsUpdNew.Remark = pUserManagementUpd.Remark;
                        objUserManagementIDCsUpdNew.OrtherNotes = pUserManagementUpd.OrtherNotes;
                        objUserManagementIDCsUpdNew.Ticket = pUserManagementUpd.Ticket;
                        objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Modified.Value;
                        objUserManagementIDCsUpdNew.StatusUpdateCore = pUserManagementUpd.StatusUpdateCore; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.SessionValReq = pUserManagementUpd.SessionValReq; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.PrevStatus = pUserManagementUpd.PrevStatus; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.ResponseAttributes = pUserManagementUpd.ResponseAttributes; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiStatus = pUserManagementUpd.CallApiStatus; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiReqRecordSl = pUserManagementUpd.CallApiReqRecordSl; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiResponseCode = pUserManagementUpd.CallApiResponseCode; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiResponseMsg = pUserManagementUpd.CallApiResponseMsg; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CreatedBy = pUserManagementUpd.CreatedBy;
                        objUserManagementIDCsUpdNew.CreatedDate = pUserManagementUpd.CreatedDate;
                        objUserManagementIDCsUpdNew.ModifiedBy = pUserNameUpd;
                        objUserManagementIDCsUpdNew.ModifiedDate = dCurrentDateTmp;
                        objUserManagementIDCsUpdNew.ApproverBy = pUserManagementUpd.ApproverBy;
                        objUserManagementIDCsUpdNew.ApprovalDate = pUserManagementUpd.ApprovalDate;
                        objUserManagementIDCsUpdNew.IpSetCode = pUserManagementUpd.IpSetCode; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.IpSetDetail = pUserManagementUpd.IpSetDetail; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.RestrictionFlag = pUserManagementUpd.RestrictionFlag; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.SubType = pUserManagementUpd.SubType;
                        objUserManagementIDCsUpdNew.StartDate = pUserManagementUpd.StartDate;
                        objUserManagementIDCsUpdNew.CallApiAutoGeneratedPassword = pUserManagementUpd.CallApiAutoGeneratedPassword;
                        _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                        {
                            iCountUpdate++;
                            iRetIdUpd = objUserManagementIDCsUpdNew.Id;                      
                            var objNew = new UserManagementIDC();                        
                            _dbContext.Entry(objNew).CurrentValues.SetValues(objUserManagementIDCsUpdNew);
                            objNew.Id = 0;
                            objNew.Status = 1; 
                            _dbContext.UserManagementIDCs.Add(objNew);
                            _dbContext.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveUserManagementIDC('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật bảng UserManagementIDC " +
                                        $"SaveUserManagementIDC('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return iRetIdUpd;
        }

         */

        
        /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserManagementIDC Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã Chi nhánh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái bản ghi. Lấy tất cả truyền vào là -1 (Không bắt buộc)</param>
        /// <param name="pFunctionType">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với người dùng Intellect iDC (Không bắt buộc)</param>
        /// <param name="pIsJoinUserIDCMaster">Cờ xác định có Union Với UserIDCMaster không</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        public async Task<List<UserManagementIDCViewModel>> GetListUserIDCManagement(long pId,string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode, 
                                int pStatus, string pFunctionType, bool pIsJoinUserIDCMaster)
        {
            try
            {
                List<string> listOfPosFind = new List<string>();
                listOfPosFind = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && w.Status == StatusLov.StatusOpenPOS
                                                            && (string.IsNullOrEmpty(pPosCode) ||  w.Code.StartsWith(pPosCode))
                                                            && (string.IsNullOrEmpty(pMainPosCode) || w.MainPosCode.StartsWith(pMainPosCode))
                                                            ).OrderBy(o => o.Code).Select(s => s.Code).ToList();
                List<UserManagementIDCViewModel> listUserIDCManagement = new List<UserManagementIDCViewModel>();
                var listUserIDCManagementTemp = _dbContext.UserManagementIDCs.Where(w => w.Id != 0 && (pId == 0 || w.Id == pId)
                                                && (listOfPosFind == null || listOfPosFind.Count <= 0 || listOfPosFind.Contains(w.PosCode))
                                                && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                                                && (string.IsNullOrEmpty(pFunctionType) || w.FunctionType == pFunctionType)
                                                && (pStatus == -1 || w.Status == pStatus)
                                                && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode))
                        .Where(delegate (UserManagementIDC c)
                        {
                            if (string.IsNullOrEmpty(pFullName)
                                || (c.FullName != null && pFullName.ToLower().Contains(c.FullName.ToLower()))
                                || (c.FullName != null && Utilities.ConvertToUnSign(pFullName.ToLower()).IndexOf(c.FullName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                )
                                return true;
                            else
                                return false;
                        }).OrderBy(o => o.PosCode).ThenByDescending(o=>o.EffectiveDate).ThenByDescending(o => o.Status).ThenBy(o => o.UserId).ThenByDescending(o => o.Id).ToList();

                List<string> listUserIds = new List<string>();
                listUserIds = listUserIDCManagementTemp.Select(s=>s.UserId).Distinct().ToList();
                List<UserIDCMaster> listUserIDCMasterTemp = new List<UserIDCMaster>();
                if (pIsJoinUserIDCMaster)
                {
                    listUserIDCMasterTemp = _dbContext.UserIDCMasters.Where(w => w.Id != 0
                               && (listOfPosFind == null || listOfPosFind.Count <= 0 || listOfPosFind.Contains(w.PosCode))
                               && (listUserIds == null || listUserIds.Count <= 0 || !listUserIds.Contains(w.UserId))
                               && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                               && (pStatus == -1 || w.Status == pStatus)
                               && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode))
                       .Where(delegate (UserIDCMaster c)
                       {
                           if (string.IsNullOrEmpty(pFullName)
                               || (c.FullName != null && pFullName.ToLower().Contains(c.FullName.ToLower()))
                               || (c.FullName != null && Utilities.ConvertToUnSign(pFullName.ToLower()).IndexOf(c.FullName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                               )
                               return true;
                           else
                               return false;
                       }).OrderBy(o => o.PosCode).ThenBy(o => o.UserId).ThenByDescending(o => o.EffectiveDate).ToList();
                }

                if ((listUserIDCManagementTemp != null && listUserIDCManagementTemp.Count != 0) || (listUserIDCMasterTemp != null && listUserIDCMasterTemp.Count != 0))
                {
                    int iCountTemp = 0;
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                    if (listUserIDCManagementTemp != null && listUserIDCManagementTemp.Count != 0)
                    {
                        foreach (var item in listUserIDCManagementTemp)
                        {
                            iCountTemp++;
                            UserManagementIDCViewModel objItem = new UserManagementIDCViewModel();
                            objItem = _mapper.Map<UserManagementIDCViewModel>(item);
                            objItem.OrderNo = iCountTemp;
                            objItem.StatusText = StatusBusinessFlow.GetByValue(item.Status).Description;
                            if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                                objItem.UserStatusText = "Khóa (Đóng)";
                            else if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                                objItem.UserStatusText = "Mở (Bình thường)";
                            else if (item.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                                objItem.UserStatusText = "Tạm khóa (Lock)";
                            else objItem.UserStatusText = "Không xác định";
                            objItem.StartDateText = item.StartDate.Value.ToString(FormatParameters.FORMAT_DATE);
                            objItem.AuthsecType = (string.IsNullOrEmpty(objItem.AuthsecType) || objItem.AuthsecType == "0") ? AuthSecType.AuthSecType_Native.Value.ToString() : objItem.AuthsecType;
                            objItem.AuthsecTypeName = int.TryParse(objItem.AuthsecType, out var v) ? AuthSecType.GetByValue(v)?.Description : "";
                            objItem.MailIdFlagName = int.TryParse(objItem.MailIdFlag, out var y) ? MailIdFlag.GetByValue(y)?.Description : "";
                            objItem.GroupNameText = item.GroupName;
                            objItem.FunctionTypeName = string.IsNullOrEmpty(objItem.FunctionType) ? "" : FunctionTypeFlag.GetByCode(objItem.FunctionType).Description;
                            if (string.IsNullOrEmpty(objItem.GroupNameOld))
                            {
                                var objUserOldTemp = listUserIDCManagementTemp.Where(w => w.UserId == item.UserId && w.Id < item.Id).OrderByDescending(o => o.Id).FirstOrDefault();
                                if (objUserOldTemp != null && objUserOldTemp.Id > 0)
                                    objItem.GroupNameOld = objUserOldTemp.GroupName;
                                else
                                {
                                    var objUserOldTemp01 = listUserIDCMasterTemp.Where(w => w.UserId == item.UserId).OrderByDescending(o => o.Id).FirstOrDefault();
                                    if (objUserOldTemp01 != null && !string.IsNullOrEmpty(objUserOldTemp01.UserId))
                                    {
                                        objItem.GroupNameOld = objUserOldTemp01.GroupName;
                                    }
                                }
                            }
                            if (listRoleUsers != null && listRoleUsers.Count != 0)
                            {
                                objItem.GroupNameText = $"{item.GroupName} - {listRoleUsers.Where(w => w.Code == item.GroupName).Select(s => s.ShortName).FirstOrDefault()}";
                                objItem.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == item.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                                objItem.RoleToTransferCashName = (objItem.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                                objItem.RoleToTransferCashDescription = (objItem.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                                objItem.GroupNameOldText = $"{item.GroupNameOld} - {listRoleUsers.Where(w => w.Code == objItem.GroupNameOld).Select(s => s.ShortName).FirstOrDefault()}";
                            }

                            //Lấy một số thông tin trong QLNS
                            objItem.GenderCode = "";
                            objItem.GenderText = "";
                            objItem.StaffPosCode = "";
                            objItem.StaffPosName = "";
                            objItem.StaffDepartmentCode = "";
                            objItem.StaffDepartmentName = "";
                            objItem.StaffPositionCode = "";
                            objItem.StaffPositionName = "";
                            if (!string.IsNullOrEmpty(pUserId) || !string.IsNullOrEmpty(pPosCode))
                            {
                                var objStaffInfoTemp = await _internalServiceAPI.GetListStaffByStaffId(objItem.StaffId);
                                if (objStaffInfoTemp != null && objStaffInfoTemp.Success && objStaffInfoTemp.Result != null && objStaffInfoTemp.Result.Count > 0)
                                {
                                    var objStaffInfo = objStaffInfoTemp.Result.FirstOrDefault();
                                    if (objStaffInfo != null && !string.IsNullOrEmpty(objStaffInfo.StaffId))
                                    {
                                        objItem.GenderCode = objStaffInfo.GenderCode;
                                        objItem.GenderText = objStaffInfo.GenderText;
                                        objItem.StaffPosCode = objStaffInfo.StaffPosCode;
                                        objItem.StaffPosName = objStaffInfo.StaffPosName;
                                        objItem.StaffDepartmentCode = objStaffInfo.StaffDepartmentCode;
                                        objItem.StaffDepartmentName = objStaffInfo.StaffDepartmentName;
                                        objItem.StaffPositionCode = objStaffInfo.StaffPositionCode;
                                        objItem.StaffPositionName = objStaffInfo.StaffPositionName;
                                        objItem.StaffEmail = objStaffInfo.StaffEmail;
                                        objItem.StaffMobileNo = objStaffInfo.StaffMobileNo;
                                    }
                                }
                            }
                            objItem.ExistsInCore = 1;
                            listUserIDCManagement.Add(objItem);
                        }
                    }
                    //Add tiếp các bản ghi từ Master Vào
                    if (listUserIDCMasterTemp != null && listUserIDCMasterTemp.Count != 0)
                    {
                        foreach (var itemMT in listUserIDCMasterTemp)
                        {
                            iCountTemp++;
                            UserManagementIDCViewModel objItemMT = new UserManagementIDCViewModel();
                            objItemMT = _mapper.Map<UserManagementIDCViewModel>(itemMT);
                            objItemMT.FunctionType = "";
                            objItemMT.FunctionTypeName = "";
                            objItemMT.Id = 0;

                            objItemMT.OrderNo = iCountTemp;
                            objItemMT.StatusText = StatusBusinessFlow.GetByValue(itemMT.Status).Description;
                            if (itemMT.UserStatus == DefaultValue.UserIDC_UserStatus_Closed)
                                objItemMT.UserStatusText = "Khóa (Đóng)";
                            else if (itemMT.UserStatus == DefaultValue.UserIDC_UserStatus_Open)
                                objItemMT.UserStatusText = "Mở (Bình thường)";
                            else if (itemMT.UserStatus == DefaultValue.UserIDC_UserStatus_Lock)
                                objItemMT.UserStatusText = "Tạm khóa (Lock)";
                            else objItemMT.UserStatusText = "Không xác định";
                            objItemMT.StartDateText = itemMT.StartDate.Value.ToString(FormatParameters.FORMAT_DATE);
                            objItemMT.AuthsecType = (string.IsNullOrEmpty(objItemMT.AuthsecType) || objItemMT.AuthsecType == "0") ? AuthSecType.AuthSecType_Single.Value.ToString() : objItemMT.AuthsecType;
                            objItemMT.AuthsecTypeName = string.IsNullOrEmpty(objItemMT.AuthsecType) ? "" : AuthSecType.GetDescriptionByCode(objItemMT.AuthsecType).Description;
                            objItemMT.MailIdFlag = itemMT.MailIdFlag;
                            objItemMT.MailIdFlagName = string.IsNullOrEmpty(objItemMT.MailIdFlag) ? "0" : MailIdFlag.GetByValue(Convert.ToInt32(objItemMT.MailIdFlag)).Description;
                            objItemMT.FunctionTypeName = string.IsNullOrEmpty(objItemMT.FunctionType) ? "" : FunctionTypeFlag.GetByCode(objItemMT.FunctionType).Description;
                            objItemMT.SubType = string.IsNullOrEmpty(itemMT.SubType) ? DefaultValue.UserIDC_SubType : itemMT.SubType;
                            objItemMT.GroupNameText = itemMT.GroupName;
                            objItemMT.BusinessDate = itemMT.EffectiveDate.Value;
                            objItemMT.GroupNameOld = itemMT.GroupName;
                            objItemMT.GroupNameOldText = itemMT.GroupName;
                            if (listRoleUsers != null && listRoleUsers.Count != 0)
                            {
                                objItemMT.GroupNameText = $"{itemMT.GroupName} - {listRoleUsers.Where(w => w.Code == itemMT.GroupName).Select(s => s.ShortName).FirstOrDefault()}";
                                objItemMT.GroupNameOldText = $"{itemMT.GroupName} - {listRoleUsers.Where(w => w.Code == objItemMT.GroupNameOld).Select(s => s.ShortName).FirstOrDefault()}";
                                objItemMT.RoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == itemMT.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                                objItemMT.RoleToTransferCashName = (objItemMT.RoleToTransferCashValue == StatusLov.StatusYes) ? "X" : "";
                                objItemMT.RoleToTransferCashDescription = (objItemMT.RoleToTransferCashValue == StatusLov.StatusYes) ? "Có quyền tiền mặt" : "Không có quyền tiền mặt";
                            }
                            //Lấy một số thông tin trong QLNS
                            objItemMT.GenderCode = "";
                            objItemMT.GenderText = "";
                            objItemMT.StaffPosCode = "";
                            objItemMT.StaffPosName = "";
                            objItemMT.StaffDepartmentCode = "";
                            objItemMT.StaffDepartmentName = "";
                            objItemMT.StaffPositionCode = "";
                            objItemMT.StaffPositionName = "";
                            if (!string.IsNullOrEmpty(pUserId) || !string.IsNullOrEmpty(pPosCode))
                            {
                                var objStaffInfoTemp = await _internalServiceAPI.GetListStaffByStaffId(objItemMT.StaffId);
                                if (objStaffInfoTemp != null && objStaffInfoTemp.Success && objStaffInfoTemp.Result != null && objStaffInfoTemp.Result.Count > 0)
                                {
                                    var objStaffInfo = objStaffInfoTemp.Result.FirstOrDefault();
                                    if (objStaffInfo != null && !string.IsNullOrEmpty(objStaffInfo.StaffId))
                                    {
                                        objItemMT.GenderCode = objStaffInfo.GenderCode;
                                        objItemMT.GenderText = objStaffInfo.GenderText;
                                        objItemMT.StaffPosCode = objStaffInfo.StaffPosCode;
                                        objItemMT.StaffPosName = objStaffInfo.StaffPosName;
                                        objItemMT.StaffDepartmentCode = objStaffInfo.StaffDepartmentCode;
                                        objItemMT.StaffDepartmentName = objStaffInfo.StaffDepartmentName;
                                        objItemMT.StaffPositionCode = objStaffInfo.StaffPositionCode;
                                        objItemMT.StaffPositionName = objStaffInfo.StaffPositionName;
                                        objItemMT.StaffEmail = objStaffInfo.StaffEmail;
                                        objItemMT.StaffMobileNo = objStaffInfo.StaffMobileNo;
                                    }
                                }
                            }
                            objItemMT.ExistsInCore = 1;
                            objItemMT.FullNameOld = string.IsNullOrEmpty(objItemMT.FullNameOld) ? objItemMT.FullName : objItemMT.FullNameOld;
                            listUserIDCManagement.Add(objItemMT);
                        }
                    }
                }
                return listUserIDCManagement;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Hàm thực hiện thêm mới/chỉnh sửa thông tin bản ghi bảng dữ liệu quản lý người dùng trên Intellect iDC UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> SaveUserManagementIDC(UserManagementIDCViewModel pUserManagementUpd, string pUserNameUpd, string pFlagCall)
        {
            int iCountUpdate = 0, iSaveChanges = 0;
            long iRetIdUpd = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pUserManagementUpd == null || string.IsNullOrEmpty(pUserManagementUpd.UserId))
                    return 0;

                pUserManagementUpd.EffectiveDate = pUserManagementUpd.EffectiveDate.Date;
                pUserManagementUpd.ExpiryDate = pUserManagementUpd.ExpiryDate.Date;
                pUserManagementUpd.StartDate = pUserManagementUpd.StartDate.Date;
                if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString() || pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
                {
                    #region --- Cập nhật thêm mới bản ghi bảng UserManagementIDC ---
                    UserManagementIDC objUserManagementUpdNew = new UserManagementIDC();
                    objUserManagementUpdNew.Id = 0;
                    objUserManagementUpdNew.FunctionType = pUserManagementUpd.FunctionType;
                    objUserManagementUpdNew.PosCode = pUserManagementUpd.PosCode;
                    objUserManagementUpdNew.PosName = pUserManagementUpd.PosName;
                    objUserManagementUpdNew.StaffId = pUserManagementUpd.StaffId;
                    objUserManagementUpdNew.StaffCode = pUserManagementUpd.StaffCode;
                    objUserManagementUpdNew.UserId = pUserManagementUpd.UserId;
                    objUserManagementUpdNew.NickName = pUserManagementUpd.NickName;
                    objUserManagementUpdNew.FirstName = pUserManagementUpd.FirstName;
                    objUserManagementUpdNew.LastName = pUserManagementUpd.LastName;
                    objUserManagementUpdNew.FullName = pUserManagementUpd.FullName;
                    if (!string.IsNullOrWhiteSpace(pUserManagementUpd.FullName))
                    {
                        var partName = pUserManagementUpd.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (partName.Length > 0)
                        {
                            objUserManagementUpdNew.LastName = partName[^1];
                            objUserManagementUpdNew.FirstName = string.Join(" ", partName.Take(partName.Length - 1));
                        }
                    }
                    objUserManagementUpdNew.EmailAddress = pUserManagementUpd.EmailAddress;
                    objUserManagementUpdNew.MobileNumber = pUserManagementUpd.MobileNumber;
                    objUserManagementUpdNew.DateOfBirth = pUserManagementUpd.DateOfBirth.Date;
                    objUserManagementUpdNew.GroupName = pUserManagementUpd.GroupName;
                    objUserManagementUpdNew.EntityList = pUserManagementUpd.EntityList;
                    objUserManagementUpdNew.AuthType = string.IsNullOrEmpty(pUserManagementUpd.AuthType) ? DefaultValue.UserIDC_AuthType : pUserManagementUpd.AuthType;
                    objUserManagementUpdNew.UserType = pUserManagementUpd.UserType;
                    objUserManagementUpdNew.SubType = string.IsNullOrEmpty(pUserManagementUpd.SubType) ? DefaultValue.UserIDC_SubType : pUserManagementUpd.SubType;
                    objUserManagementUpdNew.MailIdFlag = pUserManagementUpd.MailIdFlag;
                    objUserManagementUpdNew.AuthsecType = pUserManagementUpd.AuthsecType;
                    if (!string.IsNullOrWhiteSpace(pUserManagementUpd.RoleToTransferCashValue))
                        objUserManagementUpdNew.AuthsecType = (pUserManagementUpd.RoleToTransferCashValue == StatusLov.StatusYes) ? AuthSecType.AuthSecType_ARXOTP.Code : AuthSecType.AuthSecType_Single.Code;
                    objUserManagementUpdNew.ExtraAttributeUserRole = pUserManagementUpd.ExtraAttributeUserRole;
                    objUserManagementUpdNew.ExtraAttributeBranchCode = pUserManagementUpd.ExtraAttributeBranchCode;
                    objUserManagementUpdNew.EffectiveDate = pUserManagementUpd.EffectiveDate;
                    objUserManagementUpdNew.ExpiryDate = pUserManagementUpd.ExpiryDate.Date;
                    objUserManagementUpdNew.Ticket = string.IsNullOrEmpty(pUserManagementUpd.Ticket) ? "" : pUserManagementUpd.Ticket;      //Xử lý khi gọi API
                    objUserManagementUpdNew.Remark = Utilities.Find_Replace(pUserManagementUpd.Remark);
                    objUserManagementUpdNew.OrtherNotes = Utilities.Find_Replace(pUserManagementUpd.OrtherNotes);
                    objUserManagementUpdNew.StartDate = pUserManagementUpd.StartDate;
                    objUserManagementUpdNew.IpSetCode = Utilities.Find_Replace(pUserManagementUpd.IpSetCode);
                    objUserManagementUpdNew.IpSetDetail = Utilities.Find_Replace(pUserManagementUpd.IpSetDetail);
                    objUserManagementUpdNew.RestrictionFlag = pUserManagementUpd.RestrictionFlag;
                    objUserManagementUpdNew.Status = pUserManagementUpd.Status;
                    objUserManagementUpdNew.UserStatus = string.IsNullOrEmpty(pUserManagementUpd.UserStatus) ? DefaultValue.UserIDC_UserStatus_Open : pUserManagementUpd.UserStatus;
                    objUserManagementUpdNew.StatusUpdateCore = 0; //Xử lý khi gọi API
                    objUserManagementUpdNew.SessionValReq = true; //Xử lý khi gọi API
                    objUserManagementUpdNew.PrevStatus = 0; //Xử lý khi gọi API
                    objUserManagementUpdNew.ResponseAttributes = ""; //Xử lý khi gọi API
                    objUserManagementUpdNew.CallApiStatus = ""; //Xử lý khi gọi API
                    objUserManagementUpdNew.CallApiReqRecordSl = 0; //Xử lý khi gọi API
                    objUserManagementUpdNew.CallApiResponseCode = ""; //Xử lý khi gọi API
                    objUserManagementUpdNew.CallApiResponseMsg = ""; //Xử lý khi gọi API
                    objUserManagementUpdNew.CallApiAutoGeneratedPassword = "";
                    if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                    {
                        objUserManagementUpdNew.PosCodeOld = pUserManagementUpd.PosCode;
                        objUserManagementUpdNew.PosNameOld = pUserManagementUpd.PosName;
                        objUserManagementUpdNew.GroupNameOld = pUserManagementUpd.GroupName;
                        objUserManagementUpdNew.FirstNameOld = pUserManagementUpd.FirstName;
                        objUserManagementUpdNew.LastNameOld = pUserManagementUpd.LastName;
                        objUserManagementUpdNew.FullNameOld = pUserManagementUpd.FullName;
                        objUserManagementUpdNew.EmailAddressOld = pUserManagementUpd.EmailAddress;
                        objUserManagementUpdNew.MobileNumberOld = pUserManagementUpd.MobileNumber;
                        objUserManagementUpdNew.DateOfBirthOld = pUserManagementUpd.DateOfBirth.Date;
                    }
                    else if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString())
                    {
                        objUserManagementUpdNew.PosCodeOld = pUserManagementUpd.PosCodeOld;
                        objUserManagementUpdNew.PosNameOld = pUserManagementUpd.PosNameOld;
                        objUserManagementUpdNew.GroupNameOld = pUserManagementUpd.GroupNameOld;
                        objUserManagementUpdNew.FirstNameOld = pUserManagementUpd.FirstNameOld;
                        objUserManagementUpdNew.LastNameOld = pUserManagementUpd.LastNameOld;
                        objUserManagementUpdNew.FullNameOld = pUserManagementUpd.FullNameOld;
                        objUserManagementUpdNew.EmailAddressOld = pUserManagementUpd.EmailAddressOld;
                        objUserManagementUpdNew.MobileNumberOld = pUserManagementUpd.MobileNumberOld;
                        objUserManagementUpdNew.DateOfBirthOld = pUserManagementUpd.DateOfBirthOld.Date;
                    }

                    objUserManagementUpdNew.BusinessDate = pUserManagementUpd.BusinessDate.Date;
                    objUserManagementUpdNew.ReasonReject = string.IsNullOrEmpty(pUserManagementUpd.ReasonReject) ? "" : pUserManagementUpd.ReasonReject;
                    objUserManagementUpdNew.ListFileId = string.IsNullOrEmpty(pUserManagementUpd.ListFileId) ? "" : pUserManagementUpd.ListFileId;
                    objUserManagementUpdNew.CreatedBy = pUserNameUpd;
                    objUserManagementUpdNew.CreatedDate = dCurrentDateTmp;
                    objUserManagementUpdNew.ModifiedBy = pUserNameUpd;
                    objUserManagementUpdNew.ModifiedDate = dCurrentDateTmp;
                    objUserManagementUpdNew.ApproverBy = pUserNameUpd;
                    objUserManagementUpdNew.ApprovalDate = dCurrentDateTmp;

                    if (pUserManagementUpd.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                    {
                        var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                        if (listRoleUsers != null && listRoleUsers.Count != 0)
                        {
                            var objRoleOfUser = listRoleUsers.Where(w => w.Code == objUserManagementUpdNew.GroupName).FirstOrDefault();
                            if (objRoleOfUser != null && !string.IsNullOrEmpty(objRoleOfUser.Code))
                            {
                                if (!string.IsNullOrEmpty(objRoleOfUser.LevelCode))
                                {
                                    objUserManagementUpdNew.AuthsecType = (objRoleOfUser.LevelCode == StatusLov.StatusYes) ? AuthSecType.AuthSecType_ARXOTP.Code : AuthSecType.AuthSecType_Single.Code;
                                }
                            }
                        }
                    }
                    _dbContext.UserManagementIDCs.Add(objUserManagementUpdNew);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                    {
                        iCountUpdate++;
                        iRetIdUpd = objUserManagementUpdNew.Id;
                        //Nếu có thay đổi quyền từ ngày đến ngày mà nhỏ hơn 90 ngày thì tạo thêm 1 bản ghi nữa để hết thời gian quay lại quyền cũ.
                        if (pFlagCall == EventFlag.EventFlag_EditIDC.Value.ToString()
                            && objUserManagementUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                        {
                            if (pUserManagementUpd.ChoiceEndDateChangeRole == 1)
                            {
                                int numberDays = (pUserManagementUpd.EndDateChangeRole - pUserManagementUpd.StartDate).Days;
                                if (numberDays <= 90)
                                {
                                    var objNewRow = new UserManagementIDC();
                                    _dbContext.Entry(objNewRow).CurrentValues.SetValues(objUserManagementUpdNew);
                                    objNewRow.Id = 0;
                                    objNewRow.Status = StatusBusinessFlow.Status_Created.Value;
                                    objNewRow.StartDate = pUserManagementUpd.EndDateChangeRole.AddDays(1).Date;
                                    objNewRow.GroupName = objUserManagementUpdNew.GroupNameOld;
                                    //Kiểm tra khi chuyển sang quyền mới có quyền tiền mặt không thì Cho phương thức xác thực thứ 2
                                    //objNewRow.GroupName
                                    var listRoleUsersNew = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                                    if (listRoleUsersNew != null && listRoleUsersNew.Count != 0)
                                    {
                                        var objRoleOfUserNew = listRoleUsersNew.Where(w => w.Code == objNewRow.GroupName).FirstOrDefault();
                                        if (objRoleOfUserNew != null && !string.IsNullOrEmpty(objRoleOfUserNew.Code))
                                        {
                                            if (!string.IsNullOrEmpty(objRoleOfUserNew.LevelCode))
                                            {
                                                objNewRow.AuthsecType = (objRoleOfUserNew.LevelCode == StatusLov.StatusYes) ? AuthSecType.AuthSecType_ARXOTP.Code : AuthSecType.AuthSecType_Single.Code;
                                            }
                                        }
                                    }
                                    objNewRow.ExtraAttributeUserRole = objUserManagementUpdNew.GroupNameOld;
                                    objNewRow.GroupNameOld = objUserManagementUpdNew.GroupName;
                                    objNewRow.ExpiryDate = CustConverter.StringToDate(DefaultValue.MaxDate.ToString(), FormatParameters.FORMAT_DATE_INT).Date;
                                    objNewRow.BusinessDate = objUserManagementUpdNew.ExpiryDate.AddDays(1).Date;
                                    objNewRow.OrtherNotes = $"Thêm mới tự động bản ghi {objNewRow.FunctionType} từ Id {iRetIdUpd}. Quyến đổi từ {objNewRow.GroupNameOld} sang {objNewRow.GroupName}";
                                    objNewRow.CreatedBy = pUserNameUpd;
                                    objNewRow.CreatedDate = dCurrentDateTmp;
                                    objNewRow.ModifiedBy = pUserNameUpd;
                                    objNewRow.ModifiedDate = dCurrentDateTmp;
                                    objNewRow.ApproverBy = pUserNameUpd;
                                    objNewRow.ApprovalDate = dCurrentDateTmp;
                                    _dbContext.UserManagementIDCs.Add(objNewRow);
                                    iSaveChanges = _dbContext.SaveChanges();
                                    if (iSaveChanges > 0)
                                        iCountUpdate++;
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                {
                    #region --- Cập nhật chỉnh sửa bản ghi bảng UserManagementIDC ---
                    var objUserManagementIDCsUpdNew = _dbContext.UserManagementIDCs.Where(m => m.Id == pUserManagementUpd.Id
                                    && m.UserId == pUserManagementUpd.UserId && m.FunctionType == pUserManagementUpd.FunctionType).FirstOrDefault();
                    if (objUserManagementIDCsUpdNew != null && !string.IsNullOrEmpty(objUserManagementIDCsUpdNew.UserId) && objUserManagementIDCsUpdNew.Id > 0)
                    {
                        objUserManagementIDCsUpdNew.FunctionType = pUserManagementUpd.FunctionType;
                        objUserManagementIDCsUpdNew.PosCode = pUserManagementUpd.PosCode;
                        objUserManagementIDCsUpdNew.PosName = pUserManagementUpd.PosName;
                        objUserManagementIDCsUpdNew.StaffId = pUserManagementUpd.StaffId;
                        objUserManagementIDCsUpdNew.StaffCode = pUserManagementUpd.StaffCode;
                        objUserManagementIDCsUpdNew.UserId = pUserManagementUpd.UserId;
                        objUserManagementIDCsUpdNew.NickName = pUserManagementUpd.NickName;
                        objUserManagementIDCsUpdNew.FirstName = pUserManagementUpd.FirstName;
                        objUserManagementIDCsUpdNew.LastName = pUserManagementUpd.LastName;
                        objUserManagementIDCsUpdNew.FullName = pUserManagementUpd.FullName;
                        objUserManagementIDCsUpdNew.EmailAddress = pUserManagementUpd.EmailAddress;
                        objUserManagementIDCsUpdNew.MobileNumber = pUserManagementUpd.MobileNumber;
                        objUserManagementIDCsUpdNew.DateOfBirth = pUserManagementUpd.DateOfBirth.Date;
                        objUserManagementIDCsUpdNew.GroupName = pUserManagementUpd.GroupName;
                        objUserManagementIDCsUpdNew.EntityList = pUserManagementUpd.EntityList;
                        objUserManagementIDCsUpdNew.AuthType = string.IsNullOrEmpty(pUserManagementUpd.AuthType) ? DefaultValue.UserIDC_AuthType : pUserManagementUpd.AuthType;
                        objUserManagementIDCsUpdNew.UserType = pUserManagementUpd.UserType;
                        objUserManagementIDCsUpdNew.SubType = string.IsNullOrEmpty(pUserManagementUpd.SubType) ? DefaultValue.UserIDC_SubType : pUserManagementUpd.SubType;
                        objUserManagementIDCsUpdNew.MailIdFlag = pUserManagementUpd.MailIdFlag;
                        objUserManagementIDCsUpdNew.AuthsecType = string.IsNullOrEmpty(pUserManagementUpd.AuthsecType) ? AuthSecType.AuthSecType_Native.Code : pUserManagementUpd.AuthsecType;

                        objUserManagementIDCsUpdNew.ExtraAttributeUserRole = pUserManagementUpd.ExtraAttributeUserRole;
                        objUserManagementIDCsUpdNew.ExtraAttributeBranchCode = pUserManagementUpd.ExtraAttributeBranchCode;
                        objUserManagementIDCsUpdNew.EffectiveDate = pUserManagementUpd.EffectiveDate;
                        objUserManagementIDCsUpdNew.ExpiryDate = pUserManagementUpd.ExpiryDate.Date;
                        objUserManagementIDCsUpdNew.Ticket = string.IsNullOrEmpty(pUserManagementUpd.Ticket) ? "" : pUserManagementUpd.Ticket;      //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.Remark = Utilities.Find_Replace(pUserManagementUpd.Remark);
                        objUserManagementIDCsUpdNew.OrtherNotes = Utilities.Find_Replace(pUserManagementUpd.OrtherNotes);
                        objUserManagementIDCsUpdNew.Status = pUserManagementUpd.Status;
                        objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Modified.Value;

                        objUserManagementIDCsUpdNew.StartDate = pUserManagementUpd.StartDate;
                        objUserManagementIDCsUpdNew.IpSetCode = Utilities.Find_Replace(pUserManagementUpd.IpSetCode);
                        objUserManagementIDCsUpdNew.IpSetDetail = Utilities.Find_Replace(pUserManagementUpd.IpSetDetail);
                        objUserManagementIDCsUpdNew.RestrictionFlag = pUserManagementUpd.RestrictionFlag;
                        objUserManagementIDCsUpdNew.UserStatus = string.IsNullOrEmpty(pUserManagementUpd.UserStatus) ? DefaultValue.UserIDC_UserStatus_Open : pUserManagementUpd.UserStatus;
                        objUserManagementIDCsUpdNew.StatusUpdateCore = pUserManagementUpd.StatusUpdateCore; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.SessionValReq = pUserManagementUpd.SessionValReq; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.PrevStatus = pUserManagementUpd.PrevStatus; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.ResponseAttributes = pUserManagementUpd.ResponseAttributes; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiStatus = pUserManagementUpd.CallApiStatus; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiReqRecordSl = pUserManagementUpd.CallApiReqRecordSl; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiResponseCode = pUserManagementUpd.CallApiResponseCode; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiResponseMsg = pUserManagementUpd.CallApiResponseMsg; //Xử lý khi gọi API
                        objUserManagementIDCsUpdNew.CallApiAutoGeneratedPassword = pUserManagementUpd.CallApiAutoGeneratedPassword;
                        objUserManagementIDCsUpdNew.PosCodeOld = pUserManagementUpd.PosCodeOld;
                        objUserManagementIDCsUpdNew.PosNameOld = pUserManagementUpd.PosNameOld;
                        objUserManagementIDCsUpdNew.GroupNameOld = pUserManagementUpd.GroupNameOld;
                        objUserManagementIDCsUpdNew.FirstNameOld = pUserManagementUpd.FirstNameOld;
                        objUserManagementIDCsUpdNew.LastNameOld = pUserManagementUpd.LastNameOld;
                        objUserManagementIDCsUpdNew.FullNameOld = pUserManagementUpd.FullNameOld;
                        objUserManagementIDCsUpdNew.EmailAddressOld = pUserManagementUpd.EmailAddressOld;
                        objUserManagementIDCsUpdNew.MobileNumberOld = pUserManagementUpd.MobileNumberOld;
                        objUserManagementIDCsUpdNew.DateOfBirthOld = pUserManagementUpd.DateOfBirthOld.Date;
                        objUserManagementIDCsUpdNew.BusinessDate = pUserManagementUpd.BusinessDate.Date;
                        objUserManagementIDCsUpdNew.ModifiedBy = pUserNameUpd;
                        objUserManagementIDCsUpdNew.ModifiedDate = dCurrentDateTmp;

                        _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                        {
                            iCountUpdate++;
                            iRetIdUpd = objUserManagementIDCsUpdNew.Id;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveUserManagementIDC('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật bản ghi bảng UserManagementIDC " +
                                        $"SaveUserManagementIDC('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return iRetIdUpd;
        }
        
        /// <summary>
        /// Hàm thực hiện cập nhật DocumentId (FileId) vào bảng UserManagementIDC - Cập nhật chỉ số xác định file tờ trình vào bảng UserManagementIDC
        /// </summary>
        /// <param name="pListIdUpdate">Danh sách Id bản ghi trong UserManagementIDC cần cập nhật DocumentId</param>
        /// <param name="pUserName">Người thực hiện</param>
        /// <param name="pFileId">Chuỗi Chỉ số xác định danh sách FileId (DocumentId) cần cập nhật vào bản ghi UserManagementIDC</param>
        /// <returns>Số lượng bản ghi được cập nhật </returns>
        public async Task<int> UpdateDocumentIdUserManagementIDC(List<long> pListIdUpdate, string pUserName, string pListFileId)
        {
            int iCountUpdateDocument = 0, iSaveChanges = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pListIdUpdate == null || pListIdUpdate.Count <= 0)
                    return 0;
                if (pListIdUpdate == null || !pListIdUpdate.Any())
                    return 0;
                var listUserManagementIDCUpdDocumentId = await _dbContext.UserManagementIDCs.Where(x => pListIdUpdate.Contains(x.Id)).ToListAsync();
                foreach (var itemIdUpdate in pListIdUpdate)
                {
                    var objUserManagementIDCUpdDocumentId = listUserManagementIDCUpdDocumentId.FirstOrDefault(x => x.Id == itemIdUpdate);
                    if (objUserManagementIDCUpdDocumentId != null && !string.IsNullOrEmpty(objUserManagementIDCUpdDocumentId.UserId))
                    {
                        objUserManagementIDCUpdDocumentId.ListFileId = !string.IsNullOrEmpty(pListFileId) ? pListFileId : objUserManagementIDCUpdDocumentId.ListFileId;
                        objUserManagementIDCUpdDocumentId.ModifiedDate = dCurrentDateTmp;
                        objUserManagementIDCUpdDocumentId.ModifiedBy = pUserName;
                        iSaveChanges = await _dbContext.SaveChangesAsync();
                        if (iSaveChanges > 0)
                        {
                            iCountUpdateDocument++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateDocumentIdUserManagementIDC('{pListIdUpdate.Count.ToString()}', '{pUserName}', '{pListFileId}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật DocumentId vào bảng UserManagementIDC " +
                                        $"UpdateDocumentIdUserManagementIDC('{pListIdUpdate.Count.ToString()}', '{pUserName}', '{pListFileId}') => Error: {ex.Message}", ex);
            }
            return iCountUpdateDocument;

        }

        /// <summary>
        /// Hàm thực hiện Xóa (Đóng) bản ghi nghiệp vụ thêm mới hoặc thay đổi thông tin tài khoản người dùng Intellect iDC (Bảng UserManagementIDC)
        /// </summary>
        /// <param name="pId">Chỉ số khóa bản ghi</param>
        /// <param name="pUserId">Tài khoản người dùng Intellect iDC</param>
        /// <param name="pStaffId">Id Cán bộ Có tài khoản</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào là -1</param>
        /// <param name="pFunctionType">Nghiệp vụ thêm mới hoặc thay đổi thông tin người dùng Intellect iDC (Bắt buộc)</param>
        /// <param name="pModifiedBy">Username thực hiện</param>
        /// <param name="pFlagDelete">Cờ xác định Xóa/Đánh dấu xóa: 1: Xóa hẳn; 2: Đánh dấu xóa;</param>
        /// <returns>True - Thành công; False - Không thành công</returns>
        public bool DeleteUserManagementIDC(long pId, string pUserId, string pStaffId, int pStatus, string pFunctionType, string pModifiedBy, int pFlagDelete)
        {
            bool bResult = false;
            int iCountDetele = 0;
            try
            {
                var objUserManagementIDCDelete = _dbContext.UserManagementIDCs.Where(w => w.Id != 0 && (pId == 0 || w.Id == pId)
                                                && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                                                && (string.IsNullOrEmpty(pStaffId) || w.StaffId == pStaffId)
                                                && (string.IsNullOrEmpty(pFunctionType) || w.FunctionType == pFunctionType)
                                                && (pStatus == -1 || w.Status == pStatus)).OrderByDescending(o => o.Id).FirstOrDefault();

                if (objUserManagementIDCDelete != null && objUserManagementIDCDelete.Id != 0)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.UserManagementIDCs.Remove(objUserManagementIDCDelete);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objUserManagementIDCDelete.Status = StatusLov.StatusClosed;
                        objUserManagementIDCDelete.ModifiedBy = pModifiedBy;
                        objUserManagementIDCDelete.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objUserManagementIDCDelete).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objUserManagementIDCDelete).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objUserManagementIDCDelete).Property(x => x.ModifiedDate).IsModified = true;
                        return (_dbContext.SaveChanges() > 0);
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
        /// Hàm tổng hợp số lượng yêu cầu của các chi nhánh về người dùng iDC để hiển thị hàng chờ phê duyệt
        /// </summary>
        /// <param name="pStartDateBegin">Ngày bắt đầu - Bắt đầu. Định dạng dd/MM/yyyy (Bắt buộc phải truyền)</param>
        /// <param name="pStartDateEnd">Ngày bắt đầu - Kết thúc. Định dạng dd/MM/yyyy (Bắt buộc phải truyền)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Bắt buộc phải truyền)</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc phải truyền)</param>
        /// <param name="pUserGrade">Cấp User cần thống kê: 1 - PGD; 2 - Chi nhánh; 3 - TQ</param>
        /// <param name="pListStatus">Danh sách trạng thái truyền vào cách nhau bởi dấu phẩy. Ex: 1,5,2</param>
        /// <param name="pFlagCall">Cờ xác định cách tổng hợp (Chưa sử dụng)</param>
        /// <returns></returns>
        public List<UserManagementIDCSumRequirementViewModel> UserManagementIDC_SumRequirement_GetSearch(string pStartDateBegin, string pStartDateEnd, string pMainPosCode,
            string pPosCode, int pUserGrade, string pListStatus, int pFlagCall)
        {
            var answer = new List<UserManagementIDCSumRequirementViewModel>();
            try
            {
                SqlParameter paramStartDateBegin = new SqlParameter("@pStartDateBegin", SqlDbType.VarChar);
                paramStartDateBegin.Value = pStartDateBegin;
                SqlParameter paramStartDateEnd = new SqlParameter("@pStartDateEnd", SqlDbType.VarChar);
                paramStartDateEnd.Value = pStartDateEnd;
                SqlParameter paramMainPosCode = new SqlParameter("@pMainPosCode", SqlDbType.VarChar);
                paramMainPosCode.Value = pMainPosCode;
                SqlParameter paramPosCode = new SqlParameter("@pPosCode", SqlDbType.VarChar);
                paramPosCode.Value = pPosCode;
                SqlParameter paramListStatus = new SqlParameter("@pListStatus", SqlDbType.VarChar);
                paramListStatus.Value = pListStatus;
                SqlParameter paramUserGrade = new SqlParameter("@pUserGrade", SqlDbType.Int);
                paramUserGrade.Value = pUserGrade;
                SqlParameter paramFlagCall = new SqlParameter("@pFlagCall", SqlDbType.Int);
                paramFlagCall.Value = pFlagCall;

                var listUserManagementIDCSumRequirement = _dbContext.UserManagementIDCSumRequirements.FromSqlRaw($"Exec [dbo].[UserManagementIDC_SumRequirement_GetSearch] @pStartDateBegin,@pStartDateEnd,@pMainPosCode,@pPosCode,@pUserGrade,@pListStatus,@pFlagCall", paramStartDateBegin, paramStartDateEnd, paramMainPosCode, paramPosCode, paramUserGrade, paramListStatus, paramFlagCall).ToList();
                if (listUserManagementIDCSumRequirement != null && listUserManagementIDCSumRequirement.Count != 0)
                {
                    if (pFlagCall == 1)
                        listUserManagementIDCSumRequirement = listUserManagementIDCSumRequirement.Where(w => w.MainPosCode != "").ToList();
                    foreach (var item in listUserManagementIDCSumRequirement)
                    {
                        UserManagementIDCSumRequirementViewModel objItem = new UserManagementIDCSumRequirementViewModel();
                        objItem = _mapper.Map<UserManagementIDCSumRequirementViewModel>(item);
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
        /// Hàm thực hiện Trình duyệt bản ghi Yêu cầu về tài khoản người dùng Intellect iDC
        /// Cập nhật trạng thái các bản ghi sang trình duyệt Status = StatusBusinessFlow.Status_Submitted.Value
        /// </summary>
        /// <param name="pListUserIdApprove">Danh sách người dùng cần trình duyệt. Ví dụ: [{"Id":"101","UserId":"20032","Status":"2"},{"Id":"102","UserId":"20004","Status":"5"}]</param>
        /// <param name="pFunctionType">Mã loại yêu cầu về người dùng</param>
        /// <param name="pSystemDateText">Ngày hiện thời của máy chủ hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pUserNameUpd">Người thực hiện trình duyệt</param>
        /// <param name="pFlagCall">Cờ Trình duyệt/Phê duyệt. Giá trị: EventFlag.EventFlag_Approval.Value; EventFlag.EventFlag_Authorize.Value</param>
        /// <returns>Danh sách Id bản ghi được Update thành công</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<long>> UpdateStatusApproveUserManagementIDC(List<UserManagementIDCViewModel> pListUserIdApprove, string pFunctionType, string pSystemDateText, 
                                    string pUserNameUpd, string pFlagCall)
        {
            List<long> listIdApprove = new List<long>();
            int iCountUpdate = 0, iSaveChanges = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pListUserIdApprove != null && pListUserIdApprove.Count != 0)
                {
                    foreach (var itemIdApprove in pListUserIdApprove)
                    {
                        var objUserApproveUpdateStatus = _dbContext.UserManagementIDCs.Where(m => m.Id == itemIdApprove.Id && m.UserId == itemIdApprove.UserId).OrderByDescending(o => o.Id).FirstOrDefault();
                        if (objUserApproveUpdateStatus != null && !string.IsNullOrEmpty(objUserApproveUpdateStatus.UserId))
                        {
                            objUserApproveUpdateStatus.Status = StatusBusinessFlow.Status_Submitted.Value;
                            objUserApproveUpdateStatus.ModifiedBy = pUserNameUpd;
                            objUserApproveUpdateStatus.ModifiedDate = dCurrentDateTmp;
                            iSaveChanges = await _dbContext.SaveChangesAsync();
                            if (iSaveChanges > 0)
                            {
                                iCountUpdate++;
                                listIdApprove.Add(objUserApproveUpdateStatus.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateStatusApproveUserManagementIDC('{pListUserIdApprove.Count.ToString()}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật trình duyệt danh sách yêu cầu về tài khoản người dùng Intellect iDC " +
                                        $"UpdateStatusApproveUserManagementIDC(' {pListUserIdApprove.Count.ToString()}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return listIdApprove;
        }

        /// <summary>
        /// Hàm thực hiện Phê duyệt bản ghi Yêu cầu về tài khoản người dùng Intellect iDC
        ///     - Cập nhật trạng thái các bản ghi sang trình duyệt Status = StatusBusinessFlow.Status_Submitted.Value;
        ///     - Thực hiện gọi API vào Intellect iDC để tạo các yêu cầu;
        /// </summary>
        /// <param name="pListUserIdAuthorize">Danh sách người dùng cần phê duyệt. Ví dụ: [{"Id":"101","UserId":"20032","Status":"2"},{"Id":"102","UserId":"20004","Status":"5"}]</param>
        /// <param name="pFunctionType">Mã loại yêu cầu về người dùng</param>
        /// <param name="pSystemDateText">Ngày hiện thời của máy chủ hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pBusinessDateText">Ngày mở sổ của hệ thống Intellect iDC. Định dạng dd/MM/yyyy</param>
        /// <param name="pUserNameUpd">Người thực hiện Phê duyệt</param>
        /// <param name="pUserGradeUpd">Cấp thực hiện: Phê duyệt. Giá trị: 
        ///                 1 - PGD (PosGrade.SUB_POS);
        ///                 2 - Chi nhánh (PosGrade.MAIN_POS);
        ///                 3 - TW (PosGrade.HEAD_POS)
        /// </param>
        /// <param name="pFlagCall">Cờ Trình duyệt/Phê duyệt. Giá trị: EventFlag.EventFlag_Approval.Value; EventFlag.EventFlag_Authorize.Value</param>
        /// <returns>Danh sách Id bản ghi được Update thành công</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<long>> SaveAuthorizeUserManagementIDC(List<UserManagementIDCViewModel> pListUserIdAuthorize, string pFunctionType, string pSystemDateText,
                                    string pBusinessDateText, string pUserNameUpd, int pUserGradeUpd, string pFlagCall)
        {
            List<long> listIdAuthorize = new List<long>();
            int iCountUpdate = 0, iSaveChanges = 0, iStatusUpdateCoreTmp = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            DateTime dBusinessDateOfIDC = CustConverter.StringToDate(pBusinessDateText, FormatParameters.FORMAT_DATE).Date;
            DateTime dSystemDateOfIDC = CustConverter.StringToDate(pSystemDateText, FormatParameters.FORMAT_DATE).Date;
            string sMainPosNew = "", sMainPosOld = "";
            try
            {
                var listPosTmp = _dbContext.ListOfPoss.Where(w => w.Status == StatusLov.StatusOpenPOS).ToList();
                var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);

                if (pListUserIdAuthorize != null && pListUserIdAuthorize.Count != 0)
                {
                    foreach (var itemIdAuth in pListUserIdAuthorize)
                    {
                        var objUserAuth = _dbContext.UserManagementIDCs.Where(m => m.Id == itemIdAuth.Id && m.UserId == itemIdAuth.UserId).OrderByDescending(o => o.Id).FirstOrDefault();
                        if (objUserAuth != null && !string.IsNullOrEmpty(objUserAuth.UserId))
                        {
                            sMainPosNew = ""; sMainPosOld = "";
                            sMainPosNew = listPosTmp.Where(w => w.Code == objUserAuth.PosCode).Select(s => s.MainPosCode).FirstOrDefault();
                            sMainPosOld = listPosTmp.Where(w => w.Code == objUserAuth.PosCodeOld).Select(s => s.MainPosCode).FirstOrDefault();
                            if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code && pUserGradeUpd == PosGrade.HEAD_POS)
                            {
                                #region --- 1. Trường hợp tạo mới tài khoản người dùng ---
                                //Gọi API để thêm mới người dùng vào IDC
                                AddUserRequestViewModel objAddUser = new AddUserRequestViewModel();
                                objAddUser.Ticket = objUserAuth.Ticket;
                                objAddUser.UserId = objUserAuth.UserId;
                                objAddUser.NickName = objUserAuth.NickName;
                                objAddUser.FirstName = objUserAuth.FirstName;
                                objAddUser.LastName = objUserAuth.LastName;
                                objAddUser.EmailAddress = objUserAuth.EmailAddress;
                                objAddUser.MobileNumber = objUserAuth.MobileNumber;
                                objAddUser.DateOfBirth = objUserAuth.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objAddUser.GroupName = objUserAuth.GroupName;
                                objAddUser.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='EntityList' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");
                                objAddUser.AuthType = Int32.Parse(objUserAuth.AuthType);
                                objAddUser.UserType = Int32.Parse(objUserAuth.UserType);
                                objAddUser.MailIdFlag = Int32.Parse(objUserAuth.MailIdFlag);
                                objAddUser.ExpiryDate = objUserAuth.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objAddUser.IpSet = objUserAuth.IpSetCode;
                                objAddUser.AuthsecType = objUserAuth.AuthsecType;
                                objAddUser.SubType = objUserAuth.SubType;
                                objAddUser.StartDate = objUserAuth.StartDate.Value.ToString(FormatParameters.FORMAT_DATE_INT);
                                objAddUser.RestrictSameTimeForAllDay = null;
                                objAddUser.AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest
                                {
                                    BranchCode = objUserAuth.PosCode.TrimStart('0'),
                                    UserRole = objUserAuth.GroupName
                                };
                                var objCreateUserIDCByApi = await CreateUserIDCByApiAddUser(objAddUser, pUserNameUpd);
                                iStatusUpdateCoreTmp++;
                                objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                objUserAuth.SessionValReq = true;
                                objUserAuth.PrevStatus = objCreateUserIDCByApi.PrevStatus;
                                objUserAuth.ResponseAttributes = objCreateUserIDCByApi.ResponseMsg;
                                objUserAuth.CallApiStatus = (objCreateUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                objUserAuth.CallApiResponseCode = objCreateUserIDCByApi.ResponseCode;
                                objUserAuth.CallApiResponseMsg = objCreateUserIDCByApi.ResponseMsg;
                                //
                                if (objCreateUserIDCByApi != null && objCreateUserIDCByApi.Status
                                    && (objCreateUserIDCByApi.ResponseCode == "0" || objCreateUserIDCByApi.ResponseCode == "00000"))
                                {
                                    //Thực hiện gán quyền tiền mặt đối với tập quyền mới có quyền tiền mặt
                                    if (objUserAuth.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                                    {
                                        TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                                        listTellerRoleAssign.TellerId = objUserAuth.UserId;
                                        listTellerRoleAssign.TellerRoleAllowed = StatusLov.StatusOpen;//1 - Gán quyền tiền mặt
                                        var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign, pUserNameUpd);
                                        if (objTellerRoleAssign == null || (objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000"))
                                        {
                                            objUserAuth.OrtherNotes = $"Người dùng có quyền tiền mặt nhưng gọi API thiết lập quyền tiền mặt bị lỗi {objTellerRoleAssign.ResponseCode}-{objTellerRoleAssign.ResponseMsg}";
                                        }
                                    }
                                    //Thực hiện Update vào bảng UserIDCManagement
                                    objUserAuth.CallApiAutoGeneratedPassword = objCreateUserIDCByApi.UserPassword;
                                    //Thực hiện Update vào bảng UserIDCMaster 
                                    UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                    objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserAuth);
                                    objUserIDCMaster.ModifiedBy = pUserNameUpd;
                                    objUserIDCMaster.ModifiedDate = dCurrentDateTmp;
                                    objUserIDCMaster.ApproverBy = pUserNameUpd;
                                    objUserIDCMaster.ApprovalDate = dCurrentDateTmp;
                                    var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, EventFlag.EventFlag_Add.Value.ToString());
                                    if (objCreateUserIDCMaster > 0)
                                    {
                                        DateTime dEffectiveDateTmp = objUserAuth.EffectiveDate.Value.Date;
                                        objUserAuth.EffectiveDate = dCurrentDateTmp;
                                        //Gọi Noti để thông báo đến Email người dùng
                                        var objNotiData = await InsertNotiData(objUserAuth, pUserNameUpd);

                                        objUserAuth.EffectiveDate = dEffectiveDateTmp;
                                    }
                                    objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                    objUserAuth.ApproverBy = pUserNameUpd;
                                    objUserAuth.ApprovalDate = dCurrentDateTmp;
                                }
                                else
                                {
                                    objUserAuth.StatusUpdateCore = 0;
                                    objUserAuth.SessionValReq = false;
                                    objUserAuth.PrevStatus = objCreateUserIDCByApi.PrevStatus;
                                    objUserAuth.ResponseAttributes = objCreateUserIDCByApi.ResponseMsg;
                                    objUserAuth.CallApiStatus = (objCreateUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = 0;
                                    objUserAuth.CallApiResponseCode = objCreateUserIDCByApi.ResponseCode;
                                    objUserAuth.CallApiResponseMsg = objCreateUserIDCByApi.ResponseMsg;
                                }
                                //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                iSaveChanges = await _dbContext.SaveChangesAsync();
                                if (iSaveChanges > 0)
                                {
                                    iCountUpdate++;
                                    listIdAuthorize.Add(objUserAuth.Id);
                                }
                                #endregion
                            }
                            if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code
                                        && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                            {
                                #region --- 2. Cấp lại mật khẩu cho tài khoản người dùng ---
                                ViewUserRequestViewModel objResetUserPw = new ViewUserRequestViewModel();
                                objResetUserPw.Ticket = objUserAuth.Ticket;
                                objResetUserPw.UserId = objUserAuth.UserId;
                                var objResetPWUserIDCByApi = await ResetUserPasswordByApiResetUserPw(objResetUserPw, pUserNameUpd);
                                if (objResetPWUserIDCByApi?.ResponseCode != "0" && objResetPWUserIDCByApi?.ResponseCode != "00000")
                                {
                                    objUserAuth.StatusUpdateCore = 0;
                                    objUserAuth.SessionValReq = false;
                                    objUserAuth.PrevStatus = objResetPWUserIDCByApi.PrevStatus;
                                    objUserAuth.CallApiStatus = (objResetPWUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = 0;
                                    objUserAuth.CallApiResponseCode = objResetPWUserIDCByApi.ResponseCode;
                                    objUserAuth.CallApiResponseMsg = objResetPWUserIDCByApi.ResponseMsg;
                                }
                                else
                                {
                                    objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                    objUserAuth.SessionValReq = objResetPWUserIDCByApi.SessionValReq;
                                    objUserAuth.PrevStatus = objResetPWUserIDCByApi.PrevStatus;
                                    objUserAuth.CallApiStatus = (objResetPWUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                    objUserAuth.CallApiResponseCode = objResetPWUserIDCByApi.ResponseCode;
                                    objUserAuth.CallApiResponseMsg = objResetPWUserIDCByApi.ResponseMsg;

                                    //Hà bổ sung hàm gọi thông báo đến người dùng sau khi Reset password qua Noti
                                    //Cấu trúc thông báo:       NHCSXH thông báo tài khoản iDC: <USER_LOGIN> đã được cấp lại mật khẩu với giá trị mặc định
                                    DateTime dEffectiveDateTmp = objUserAuth.EffectiveDate.Value.Date;
                                    objUserAuth.EffectiveDate = dCurrentDateTmp;
                                    //Gọi Noti để thông báo đến Email người dùng



                                    //Kết thúc gọi Noti thiết lập lại ngày hiệu lực
                                    objUserAuth.EffectiveDate = dEffectiveDateTmp;
                                    objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                    objUserAuth.ApproverBy = pUserNameUpd;
                                    objUserAuth.ApprovalDate = dCurrentDateTmp;
                                }
                                //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                iSaveChanges = await _dbContext.SaveChangesAsync();
                                if (iSaveChanges > 0)
                                {
                                    iCountUpdate++;
                                    listIdAuthorize.Add(objUserAuth.Id);
                                }
                                #endregion
                            }

                            if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code
                                       && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                            {
                                #region --- 3. Mở khóa cho tài khoản người dùng ---
                                ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                                objRsUser.Ticket = objUserAuth.Ticket;
                                objRsUser.UserId = objUserAuth.UserId;
                                var objEnableUserIDCByApiResult = await ChangeUserStatusByApiEnableUser(objRsUser, pUserNameUpd);
                                if (objEnableUserIDCByApiResult != null && (objEnableUserIDCByApiResult.ResponseCode == "0" || objEnableUserIDCByApiResult.ResponseCode == "00000"))
                                {
                                    objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                    objUserAuth.SessionValReq = objEnableUserIDCByApiResult.SessionValReq;
                                    objUserAuth.PrevStatus = objEnableUserIDCByApiResult.PrevStatus;
                                    objUserAuth.CallApiStatus = (objEnableUserIDCByApiResult.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                    objUserAuth.CallApiResponseCode = objEnableUserIDCByApiResult.ResponseCode;
                                    objUserAuth.CallApiResponseMsg = objEnableUserIDCByApiResult.ResponseMsg;

                                    //Hà bổ sung hàm gọi thông báo đến người dùng sau khi khóa hoặc mở khóa tài khoản đến APP QLTDCS
                                    //Cấu trúc thông báo:       NHCSXH thông báo tài khoản iDC: <USER_LOGIN> đã được <Mở khóa/ Khóa>.
                                    DateTime dEffectiveDateTmp = objUserAuth.EffectiveDate.Value.Date;
                                    objUserAuth.EffectiveDate = dCurrentDateTmp;
                                    //Gọi Noti để thông báo đến Email người dùng


                                    //Kết thúc gọi Noti thiết lập lại ngày hiệu lực
                                    objUserAuth.EffectiveDate = dEffectiveDateTmp;
                                    objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                    objUserAuth.ApproverBy = pUserNameUpd;
                                    objUserAuth.ApprovalDate = dCurrentDateTmp;
                                }
                                else
                                {
                                    objUserAuth.StatusUpdateCore = 0;
                                    objUserAuth.SessionValReq = false;
                                    objUserAuth.PrevStatus = objEnableUserIDCByApiResult.PrevStatus;
                                    objUserAuth.CallApiStatus = (objEnableUserIDCByApiResult.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = 0;
                                    objUserAuth.CallApiResponseCode = objEnableUserIDCByApiResult.ResponseCode;
                                    objUserAuth.CallApiResponseMsg = objEnableUserIDCByApiResult.ResponseMsg;
                                }
                                //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                iSaveChanges = await _dbContext.SaveChangesAsync();
                                if (iSaveChanges > 0)
                                {
                                    iCountUpdate++;
                                    listIdAuthorize.Add(objUserAuth.Id);
                                }
                                #endregion
                            }

                            if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code
                                       && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                            {
                                #region --- 4. Khóa tài khoản người dùng ---
                                //Kiểm tra thêm tại đây xem trước khi thực thi Khóa tài khoản Intellect iDC người dùng có mở sổ tiền mặt đầu ngày không
                                int iCheckOpenCash = CheckOpenCashByUserId(objUserAuth.UserId, dBusinessDateOfIDC.ToString(FormatParameters.FORMAT_DATE_ORA));
                                if (iCheckOpenCash != 0 && iCheckOpenCash != 3)
                                {
                                    //0 - Chưa mở sổ tiền mặt đầu ngày;     3 - Đã mở và đóng không còn tồn quỹ tiền mặt
                                    objUserAuth.StatusUpdateCore = 0;
                                    objUserAuth.SessionValReq = false;
                                    objUserAuth.PrevStatus = 0;
                                    objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = 0;
                                    objUserAuth.CallApiResponseCode = "";
                                    objUserAuth.CallApiResponseMsg = $"Đã mở sổ tiền mặt đầu ngày hoặc Mở sổ tiền mặt đầu ngày đã đóng nhưng vẫn còn tồn quỹ tiền mặt - Mã kiểm tra {iCheckOpenCash.ToString()}";
                                }
                                else
                                {
                                    //Gọi API khóa người dùng
                                    ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                                    objRsUser.Ticket = objUserAuth.Ticket;
                                    objRsUser.UserId = objUserAuth.UserId;
                                    var objDisableUserIDCByApi = await ChangeUserStatusByApiDisableUser(objRsUser, pUserNameUpd);
                                    if (objDisableUserIDCByApi != null && (objDisableUserIDCByApi.ResponseCode == "0" || objDisableUserIDCByApi.ResponseCode == "00000"))
                                    {
                                        objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                        objUserAuth.SessionValReq = objDisableUserIDCByApi.SessionValReq;
                                        objUserAuth.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                        objUserAuth.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;

                                        //Hà bổ sung hàm gọi thông báo đến người dùng sau khi khóa hoặc mở khóa tài khoản đến APP QLTDCS
                                        //Cấu trúc thông báo:       NHCSXH thông báo tài khoản iDC: <USER_LOGIN> đã được <Mở khóa/ Khóa>.
                                        DateTime dEffectiveDateTmp = objUserAuth.EffectiveDate.Value.Date;
                                        objUserAuth.EffectiveDate = dCurrentDateTmp;
                                        //Gọi Noti để thông báo đến Email người dùng


                                        //Kết thúc gọi Noti thiết lập lại ngày hiệu lực
                                        objUserAuth.EffectiveDate = dEffectiveDateTmp;
                                        objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                        objUserAuth.ApproverBy = pUserNameUpd;
                                        objUserAuth.ApprovalDate = dCurrentDateTmp;
                                    }
                                    else
                                    {
                                        objUserAuth.StatusUpdateCore = 0;
                                        objUserAuth.SessionValReq = false;
                                        objUserAuth.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = 0;
                                        objUserAuth.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;
                                    }
                                }
                                //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                iSaveChanges = await _dbContext.SaveChangesAsync();
                                if (iSaveChanges > 0)
                                {
                                    iCountUpdate++;
                                    listIdAuthorize.Add(objUserAuth.Id);
                                }
                                #endregion
                            }

                            if ((objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code
                                                || objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                        && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                            {
                                #region --- 5. Thay đổi quyền hoặc Thay đổi thông tin người dùng ---
                                bool bIsContinueTmp = true;
                                if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                {
                                    //Kiểm tra đã mở tiền mặt với User đổi quyền chưa?
                                    int iCheckOpenCash = CheckOpenCashByUserId(objUserAuth.UserId, dBusinessDateOfIDC.ToString(FormatParameters.FORMAT_DATE_ORA));
                                    if (iCheckOpenCash != 0 && iCheckOpenCash != 3)
                                    {
                                        //0 - Chưa mở sổ tiền mặt đầu ngày;     3 - Đã mở và đóng không còn tồn quỹ tiền mặt
                                        objUserAuth.StatusUpdateCore = 0;
                                        objUserAuth.SessionValReq = false;
                                        objUserAuth.PrevStatus = 0;
                                        objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = 0;
                                        objUserAuth.CallApiResponseCode = "";
                                        objUserAuth.CallApiResponseMsg = $"Đã mở sổ tiền mặt đầu ngày hoặc Mở sổ tiền mặt đầu ngày đã đóng nhưng vẫn còn tồn quỹ tiền mặt - Mã kiểm tra {iCheckOpenCash.ToString()}";
                                        bIsContinueTmp = false;
                                    }
                                }
                                if (bIsContinueTmp)
                                {
                                    if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                    {
                                        //Bỏ quyền tiền mặt của người dùng
                                        TellerRoleAssignRequestViewModel listTellerRoleRemove = new TellerRoleAssignRequestViewModel();
                                        listTellerRoleRemove.TellerId = objUserAuth.UserId;
                                        listTellerRoleRemove.TellerRoleAllowed = 0; //Bỏ quyền tiền mặt
                                        var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleRemove, pUserNameUpd);
                                        if (objTellerRoleAssign == null || (objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000"))
                                        {
                                            objUserAuth.StatusUpdateCore = 0;
                                            objUserAuth.SessionValReq = false;
                                            objUserAuth.PrevStatus = 0;
                                            objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                            objUserAuth.CallApiReqRecordSl = 0;
                                            objUserAuth.CallApiResponseCode = objTellerRoleAssign.ResponseCode;
                                            objUserAuth.CallApiResponseMsg = $"Lỗi khi gọi bỏ quyền tiền mặt của tài khoản người dùng (API TellerRoleAssign): {objTellerRoleAssign.ResponseMsg}";
                                            bIsContinueTmp = false;
                                        }
                                    }

                                    //Gọi vào API chỉnh sửa người dùng trên IDC
                                    var objModifyUser = new ModifyUserRequestViewModel
                                    {
                                        AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                                    };
                                    objModifyUser.Ticket = objUserAuth.Ticket;
                                    objModifyUser.UserId = objUserAuth.UserId;
                                    objModifyUser.NickName = objUserAuth.NickName;
                                    objModifyUser.FirstName = objUserAuth.FirstName;
                                    objModifyUser.LastName = objUserAuth.LastName;
                                    objModifyUser.GroupName = objUserAuth.GroupName;
                                    objModifyUser.EntityList = objUserAuth.EntityList;
                                    objModifyUser.MobileNumber = objUserAuth.MobileNumber;
                                    objModifyUser.EmailAddress = objUserAuth.EmailAddress;
                                    objModifyUser.ExpiryDate = objUserAuth.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                    objModifyUser.DateOfBirth = objUserAuth.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                    objModifyUser.Language = "vi_VN";
                                    objModifyUser.AddUserExtraAttributeRequestViewModel.BranchCode = objUserAuth.PosCode?.TrimStart('0');
                                    objModifyUser.AddUserExtraAttributeRequestViewModel.UserRole = objUserAuth.GroupName;
                                    objModifyUser.IpSet = objUserAuth.IpSetDetail;
                                    objModifyUser.AuthsecType = objUserAuth.AuthsecType;
                                    objModifyUser.SubType = objUserAuth.SubType;
                                    objModifyUser.StartDate = objUserAuth.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                                    var objResultModifyUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUser, pUserNameUpd);
                                    if (objResultModifyUserIDCByApi != null && (objResultModifyUserIDCByApi.ResponseCode == "0" || objResultModifyUserIDCByApi.ResponseCode == "00000"))
                                    {
                                        //Thực hiện gán quyền tiền mặt nếu người dùng có quyền tiền mặt
                                        if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                        {
                                            if (listRoleUsers != null && listRoleUsers.Count != 0)
                                            {
                                                string flagRoleToTransferCashValue = "";
                                                flagRoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserAuth.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                                                if (flagRoleToTransferCashValue == StatusLov.StatusYes)
                                                {
                                                    TellerRoleAssignRequestViewModel listTellerRoleAssignYes = new TellerRoleAssignRequestViewModel();
                                                    listTellerRoleAssignYes.TellerId = objUserAuth.UserId;
                                                    listTellerRoleAssignYes.TellerRoleAllowed = 1; //Gán quyền tiền mặt
                                                    var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssignYes, pUserNameUpd);
                                                    if (objTellerRoleAssign == null || (objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000"))
                                                    {
                                                        objUserAuth.StatusUpdateCore = 0;
                                                        objUserAuth.SessionValReq = false;
                                                        objUserAuth.PrevStatus = 0;
                                                        objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                                        objUserAuth.CallApiReqRecordSl = 0;
                                                        objUserAuth.CallApiResponseCode = objTellerRoleAssign.ResponseCode;
                                                        objUserAuth.CallApiResponseMsg = $"Lỗi khi gọi gán quyền tiền mặt của tài khoản người dùng (API TellerRoleAssign): {objTellerRoleAssign.ResponseMsg}";
                                                    }
                                                }
                                            }
                                            //Hà bổ sung hàm gọi thông báo đến người dùng sau khi khóa hoặc mở khóa tài khoản đến APP QLTDCS
                                            //Cấu trúc thông báo: NHCSXH thông báo tài khoản iDC: <USER_LOGIN> đã được đổi sang quyền <Quyền mới đổi>, có hiệu lực từ <Thời gian thực thi vào hệ thống>.
                                            DateTime dEffectiveDateTmp = objUserAuth.EffectiveDate.Value.Date;
                                            objUserAuth.EffectiveDate = dCurrentDateTmp;
                                            //Gọi Noti để thông báo đến Email người dùng


                                            //Kết thúc gọi Noti thiết lập lại ngày hiệu lực
                                            objUserAuth.EffectiveDate = dEffectiveDateTmp;
                                        }

                                        objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                        objUserAuth.SessionValReq = objResultModifyUserIDCByApi.SessionValReq;
                                        objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                        objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;

                                        objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                        objUserAuth.ApproverBy = pUserNameUpd;
                                        objUserAuth.ApprovalDate = dCurrentDateTmp;

                                        //Update thay đổi vào bảng UserIDCMaster
                                        UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                        objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserAuth);
                                        objUserIDCMaster.Id = 0;
                                        objUserIDCMaster.ModifiedBy = pUserNameUpd;
                                        objUserIDCMaster.ModifiedDate = dCurrentDateTmp;
                                        objUserIDCMaster.ApproverBy = pUserNameUpd;
                                        objUserIDCMaster.ApprovalDate = dCurrentDateTmp;
                                        var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, EventFlag.EventFlag_Edit.Value.ToString());
                                    }
                                    else
                                    {
                                        objUserAuth.StatusUpdateCore = 0;
                                        objUserAuth.SessionValReq = false;
                                        objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = 0;
                                        objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;
                                    }
                                    //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                    iSaveChanges = await _dbContext.SaveChangesAsync();
                                    if (iSaveChanges > 0)
                                    {
                                        iCountUpdate++;
                                        listIdAuthorize.Add(objUserAuth.Id);
                                    }
                                }
                                #endregion
                            }

                            if ((objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code
                                                || objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                        && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                            {
                                bool bValidAuth = false;
                                #region --- 6. Thay đổi POS người dùng ---
                                if (sMainPosNew == sMainPosOld && (pUserGradeUpd == PosGrade.HEAD_POS || pUserGradeUpd == PosGrade.MAIN_POS))
                                    bValidAuth = true;
                                if (sMainPosNew != sMainPosOld && (pUserGradeUpd == PosGrade.HEAD_POS))
                                    bValidAuth = true;
                                if (bValidAuth)
                                {
                                    //Kiểm tra còn giao dịch Pending hay không?

                                    //Kiểm tra đã mở tiền mặt với User đổi quyền chưa?
                                    int iCheckOpenCash = CheckOpenCashByUserId(objUserAuth.UserId, dBusinessDateOfIDC.ToString(FormatParameters.FORMAT_DATE_ORA));
                                    if (iCheckOpenCash != 0 && iCheckOpenCash != 3)
                                    {
                                        //0 - Chưa mở sổ tiền mặt đầu ngày;     3 - Đã mở và đóng không còn tồn quỹ tiền mặt
                                        objUserAuth.StatusUpdateCore = 0;
                                        objUserAuth.SessionValReq = false;
                                        objUserAuth.PrevStatus = 0;
                                        objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = 0;
                                        objUserAuth.CallApiResponseCode = "";
                                        objUserAuth.CallApiResponseMsg = $"Đã mở sổ tiền mặt đầu ngày hoặc Mở sổ tiền mặt đầu ngày đã đóng nhưng vẫn còn tồn quỹ tiền mặt - Mã kiểm tra {iCheckOpenCash.ToString()}";
                                        bValidAuth = false;
                                    }

                                    if (bValidAuth)
                                    {
                                        //Bỏ quyền tiền mặt của người dùng
                                        TellerRoleAssignRequestViewModel listTellerRoleRemove = new TellerRoleAssignRequestViewModel();
                                        listTellerRoleRemove.TellerId = objUserAuth.UserId;
                                        listTellerRoleRemove.TellerRoleAllowed = 0; //Bỏ quyền tiền mặt
                                        var objTellerRemoveRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleRemove, pUserNameUpd);
                                        if (objTellerRemoveRoleAssign == null || (objTellerRemoveRoleAssign.ResponseCode != "0" && objTellerRemoveRoleAssign.ResponseCode != "00000"))
                                        {
                                            objUserAuth.StatusUpdateCore = 0;
                                            objUserAuth.SessionValReq = false;
                                            objUserAuth.PrevStatus = 0;
                                            objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                            objUserAuth.CallApiReqRecordSl = 0;
                                            objUserAuth.CallApiResponseCode = objTellerRemoveRoleAssign.ResponseCode;
                                            objUserAuth.CallApiResponseMsg = $"Lỗi khi gọi bỏ quyền tiền mặt của tài khoản người dùng (API TellerRoleAssign): {objTellerRemoveRoleAssign.ResponseMsg}";
                                            bValidAuth = false;
                                        }

                                        //Bỏ xác thực 2 lớp với tài khoản người dùng này
                                        if (bValidAuth)
                                        {
                                            var objDeleteOTPRegister = await ChangeOTPRegisterByUserId(objUserAuth.UserId, 0);
                                            if (objDeleteOTPRegister == null || objDeleteOTPRegister.Success != 1)
                                            {
                                                objUserAuth.StatusUpdateCore = 0;
                                                objUserAuth.SessionValReq = false;
                                                objUserAuth.PrevStatus = 0;
                                                objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                                objUserAuth.CallApiReqRecordSl = 0;
                                                objUserAuth.CallApiResponseCode = objTellerRemoveRoleAssign.ResponseCode;
                                                objUserAuth.CallApiResponseMsg = $"Lỗi khi gọi bỏ xác thực 2 lớp với tài khoản người dùng: {objDeleteOTPRegister.Success.ToString()}";
                                                bValidAuth = false;
                                            }
                                        }
                                        if (bValidAuth)
                                        {
                                            //-------------------------------------------------------------------------------------------------------------
                                            //Gọi vào API chỉnh sửa người dùng trên IDC
                                            var objModifyUserChangePos = new ModifyUserRequestViewModel
                                            {
                                                AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                                            };
                                            objModifyUserChangePos.Ticket = objUserAuth.Ticket;
                                            objModifyUserChangePos.UserId = objUserAuth.UserId;
                                            objModifyUserChangePos.NickName = objUserAuth.NickName;
                                            objModifyUserChangePos.FirstName = objUserAuth.FirstName;
                                            objModifyUserChangePos.LastName = objUserAuth.LastName;
                                            objModifyUserChangePos.GroupName = objUserAuth.GroupName;
                                            objModifyUserChangePos.EntityList = objUserAuth.EntityList;
                                            objModifyUserChangePos.MobileNumber = objUserAuth.MobileNumber;
                                            objModifyUserChangePos.EmailAddress = objUserAuth.EmailAddress;
                                            objModifyUserChangePos.ExpiryDate = objUserAuth.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                            objModifyUserChangePos.DateOfBirth = objUserAuth.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                            objModifyUserChangePos.Language = "vi_VN";
                                            objModifyUserChangePos.AddUserExtraAttributeRequestViewModel.BranchCode = objUserAuth.PosCode?.TrimStart('0');
                                            objModifyUserChangePos.AddUserExtraAttributeRequestViewModel.UserRole = objUserAuth.GroupName;
                                            objModifyUserChangePos.IpSet = objUserAuth.IpSetDetail;
                                            objModifyUserChangePos.AuthsecType = objUserAuth.AuthsecType;
                                            objModifyUserChangePos.SubType = objUserAuth.SubType;
                                            objModifyUserChangePos.StartDate = objUserAuth.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                                            var objResultModifyUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUserChangePos, pUserNameUpd);
                                            if (objResultModifyUserIDCByApi != null && (objResultModifyUserIDCByApi.ResponseCode == "0" || objResultModifyUserIDCByApi.ResponseCode == "00000"))
                                            {
                                                //Thực hiện gán quyền tiền mặt nếu người dùng có quyền tiền mặt
                                                if (listRoleUsers != null && listRoleUsers.Count != 0)
                                                {
                                                    string flagRoleToTransferCashValue = "";
                                                    flagRoleToTransferCashValue = $"{listRoleUsers.Where(w => w.Code == objUserAuth.GroupName).Select(s => s.LevelCode).FirstOrDefault()}";
                                                    if (flagRoleToTransferCashValue == StatusLov.StatusYes)
                                                    {
                                                        TellerRoleAssignRequestViewModel listTellerRoleAssignYes = new TellerRoleAssignRequestViewModel();
                                                        listTellerRoleAssignYes.TellerId = objUserAuth.UserId;
                                                        listTellerRoleAssignYes.TellerRoleAllowed = 1; //Gán quyền tiền mặt
                                                        var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssignYes, pUserNameUpd);
                                                        if (objTellerRoleAssign == null || (objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000"))
                                                        {
                                                            objUserAuth.StatusUpdateCore = 0;
                                                            objUserAuth.SessionValReq = false;
                                                            objUserAuth.PrevStatus = 0;
                                                            objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                                            objUserAuth.CallApiReqRecordSl = 0;
                                                            objUserAuth.CallApiResponseCode = objTellerRoleAssign.ResponseCode;
                                                            objUserAuth.CallApiResponseMsg = $"Lỗi khi gọi gán quyền tiền mặt của tài khoản người dùng (API TellerRoleAssign): {objTellerRoleAssign.ResponseMsg}";
                                                        }
                                                    }
                                                }

                                                objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                                objUserAuth.SessionValReq = objResultModifyUserIDCByApi.SessionValReq;
                                                objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                                objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                                objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                                objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                                objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;

                                                objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                                objUserAuth.ApproverBy = pUserNameUpd;
                                                objUserAuth.ApprovalDate = dCurrentDateTmp;

                                                //Update thay đổi vào bảng UserIDCMaster
                                                UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                                objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserAuth);
                                                objUserIDCMaster.Id = 0;
                                                objUserIDCMaster.PosCode = objUserAuth.PosCode;
                                                objUserIDCMaster.ModifiedBy = pUserNameUpd;
                                                objUserIDCMaster.ModifiedDate = dCurrentDateTmp;
                                                objUserIDCMaster.ApproverBy = pUserNameUpd;
                                                objUserIDCMaster.ApprovalDate = dCurrentDateTmp;
                                                var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, EventFlag.EventFlag_Edit.Value.ToString());
                                                if (objCreateUserIDCMaster > 0)
                                                {
                                                    //Gửi thông báo Noti cho người dùng: NHCSXH thông báo tài khoản iDC: <USER_LOGIN> đã được đổi sang đơn vị mới <POS mới> với quyền <Quyền mới đổi>, có hiệu lực từ <Thời gian thực thi vào hệ thống>.
                                                    var objNotiData = await InsertNotiData(objUserAuth, pUserNameUpd);
                                                }
                                            }
                                            else
                                            {
                                                objUserAuth.StatusUpdateCore = 0;
                                                objUserAuth.SessionValReq = false;
                                                objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                                objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                                objUserAuth.CallApiReqRecordSl = 0;
                                                objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                                objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;
                                            }
                                            //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                            iSaveChanges = await _dbContext.SaveChangesAsync();
                                            if (iSaveChanges > 0)
                                            {
                                                iCountUpdate++;
                                                listIdAuthorize.Add(objUserAuth.Id);
                                            }
                                            //=============================================================================================================
                                        } 
                                    }
                                }
                                #endregion
                            }
                            if (objUserAuth.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code && pUserGradeUpd == PosGrade.MAIN_POS)
                            {
                                #region --- 7. Hủy người dùng ---
                                bool bIsContinueTmpCloseUserId = true;
                                //Kiểm tra đã mở tiền mặt với User đổi quyền chưa?
                                int iCheckOpenCash = CheckOpenCashByUserId(objUserAuth.UserId, dBusinessDateOfIDC.ToString(FormatParameters.FORMAT_DATE_ORA));
                                if (iCheckOpenCash != 0 && iCheckOpenCash != 3)
                                {
                                    //0 - Chưa mở sổ tiền mặt đầu ngày;     3 - Đã mở và đóng không còn tồn quỹ tiền mặt
                                    objUserAuth.StatusUpdateCore = 0;
                                    objUserAuth.SessionValReq = false;
                                    objUserAuth.PrevStatus = 0;
                                    objUserAuth.CallApiStatus = ResultValueAPI.ResultValue_Status_Failed;
                                    objUserAuth.CallApiReqRecordSl = 0;
                                    objUserAuth.CallApiResponseCode = "";
                                    objUserAuth.CallApiResponseMsg = $"Đã mở sổ tiền mặt đầu ngày hoặc Mở sổ tiền mặt đầu ngày đã đóng nhưng vẫn còn tồn quỹ tiền mặt - Mã kiểm tra {iCheckOpenCash.ToString()}";
                                    bIsContinueTmpCloseUserId = false;
                                }
                                if (bIsContinueTmpCloseUserId)
                                {
                                    var objModifyUserDelete = new ModifyUserRequestViewModel
                                    {
                                        AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                                    };
                                    objModifyUserDelete.Ticket = objUserAuth.Ticket;
                                    objModifyUserDelete.UserId = objUserAuth.UserId;
                                    objModifyUserDelete.NickName = objUserAuth.NickName;
                                    objModifyUserDelete.FirstName = objUserAuth.FirstName;
                                    objModifyUserDelete.LastName = objUserAuth.LastName;
                                    objModifyUserDelete.GroupName = objUserAuth.GroupName;
                                    objModifyUserDelete.EntityList = objUserAuth.EntityList;
                                    objModifyUserDelete.MobileNumber = objUserAuth.MobileNumber;
                                    objModifyUserDelete.EmailAddress = objUserAuth.EmailAddress;
                                    objModifyUserDelete.ExpiryDate = dSystemDateOfIDC.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                    objModifyUserDelete.DateOfBirth = objUserAuth.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                    objModifyUserDelete.Language = "vi_VN";
                                    objModifyUserDelete.AddUserExtraAttributeRequestViewModel.BranchCode = objUserAuth.PosCode?.TrimStart('0');
                                    objModifyUserDelete.AddUserExtraAttributeRequestViewModel.UserRole = objUserAuth.GroupName;
                                    objModifyUserDelete.IpSet = objUserAuth.IpSetDetail;
                                    objModifyUserDelete.AuthsecType = objUserAuth.AuthsecType;
                                    objModifyUserDelete.SubType = objUserAuth.SubType;
                                    objModifyUserDelete.StartDate = objUserAuth.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                                    var objResultModifyUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUserDelete, pUserNameUpd);
                                    if (objResultModifyUserIDCByApi != null && (objResultModifyUserIDCByApi.ResponseCode == "0" || objResultModifyUserIDCByApi.ResponseCode == "00000"))
                                    {
                                        //Gọi tiếp API khóa UserId
                                        ViewUserRequestViewModel objDisableUserId = new ViewUserRequestViewModel();
                                        objDisableUserId.Ticket = objUserAuth.Ticket;
                                        objDisableUserId.UserId = objUserAuth.UserId;
                                        var objDisableUserIDCByApi = await ChangeUserStatusByApiDisableUser(objDisableUserId, pUserNameUpd);
                                        if (objDisableUserIDCByApi != null && (objDisableUserIDCByApi.ResponseCode == "0" || objDisableUserIDCByApi.ResponseCode == "00000"))
                                        {
                                            objUserAuth.UserStatus = DefaultValue.UserIDC_UserStatus_Closed;
                                        } 
                                        objUserAuth.StatusUpdateCore = objUserAuth.StatusUpdateCore + 1;
                                        objUserAuth.SessionValReq = objResultModifyUserIDCByApi.SessionValReq;
                                        objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = objUserAuth.CallApiReqRecordSl + 1;
                                        objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;

                                        objUserAuth.ExpiryDate = dSystemDateOfIDC.Date;
                                        objUserAuth.Status = (pUserGradeUpd == PosGrade.HEAD_POS) ? StatusBusinessFlow.Status_HeadOffice_Approved.Value : StatusBusinessFlow.Status_Branch_Approved.Value;
                                        objUserAuth.ApproverBy = pUserNameUpd;
                                        objUserAuth.ApprovalDate = dCurrentDateTmp;

                                        //Update thay đổi vào bảng UserIDCMaster
                                        UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                        objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserAuth);
                                        objUserIDCMaster.Id = 0;
                                        objUserIDCMaster.PosCode = objUserAuth.PosCode;
                                        objUserIDCMaster.ModifiedBy = pUserNameUpd;
                                        objUserIDCMaster.ExpiryDate = dSystemDateOfIDC.Date;
                                        objUserIDCMaster.ModifiedDate = dCurrentDateTmp;
                                        objUserIDCMaster.ApproverBy = pUserNameUpd;
                                        objUserIDCMaster.ApprovalDate = dCurrentDateTmp;
                                        var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, EventFlag.EventFlag_Edit.Value.ToString());
                                    }
                                    else
                                    {
                                        objUserAuth.StatusUpdateCore = 0;
                                        objUserAuth.SessionValReq = false;
                                        objUserAuth.PrevStatus = objResultModifyUserIDCByApi.PrevStatus;
                                        objUserAuth.CallApiStatus = (objResultModifyUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                        objUserAuth.CallApiReqRecordSl = 0;
                                        objUserAuth.CallApiResponseCode = objResultModifyUserIDCByApi.ResponseCode;
                                        objUserAuth.CallApiResponseMsg = objResultModifyUserIDCByApi.ResponseMsg;
                                    }
                                    //Cập nhật trạng thái và các thông tin gọi API vào bảng UserManagementIDC
                                    iSaveChanges = await _dbContext.SaveChangesAsync();
                                    if (iSaveChanges > 0)
                                    {
                                        iCountUpdate++;
                                        listIdAuthorize.Add(objUserAuth.Id);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveAuthorizeUserManagementIDC('{pListUserIdAuthorize.Count.ToString()}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm Phê duyệt danh sách yêu cầu về tài khoản người dùng Intellect iDC " +
                                        $"SaveAuthorizeUserManagementIDC(' {pListUserIdAuthorize.Count.ToString()}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return listIdAuthorize;
        }













        /// <summary>
        /// Hàm thực hiện trình duyệt/Phê duyệt các chức năng người dùng trên Intellect iDC UserManagementIDC
        /// </summary>
        /// <param name="pUserManagementUpd">Thông tin người dùng cập nhật theo Model UserIDCMasterViewModel</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện</param>
        /// <param name="pFlagCall">Cờ thêm/sửa. Giá trị: Sửa - EventFlag.EventFlag_Edit.Value; Thêm - EventFlag.EventFlag_Add.Value</param>
        /// <returns>Chỉ số Id được cập nhật. -1: Lỗi; 0: Không tìm thấy bản ghi cập nhật chỉnh sửa hoặc thông tin truyền vào pUserIDCMasterUpd Null</returns>
        /// <exception cref="Exception"></exception>
        public async Task<long> SaveApproveUserManagementIDC(UserManagementIDCViewModel pUserManagementUpd, string pUserNameUpd, string pFlagCall, string pButtonType)
        {
            int iCountUpdate = 0, iSaveChanges = 0, iCreateUserIDC = 0;
            long iRetIdUpd = 0;
            var pMainPosOld = "";
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pUserManagementUpd != null && !string.IsNullOrEmpty(pUserManagementUpd.UserId))
                {
                    var objUserManagementIDCsUpdNew = _dbContext.UserManagementIDCs.Where(m => m.Id == pUserManagementUpd.Id && m.UserId == pUserManagementUpd.UserId).FirstOrDefault();
                    var objUserManagementIDCsOld = _dbContext.UserIDCMasters.Where(m => m.UserId == pUserManagementUpd.UserId).FirstOrDefault();
                    if (objUserManagementIDCsOld != null)
                    {
                        pMainPosOld = _dbContext.ListOfPoss.Where(w => w.Code == objUserManagementIDCsOld.PosCode).Select(s => s.MainPosCode).FirstOrDefault();
                    }

                    //Trường hợp trình duyệt ở cấp chi nhánh
                    if (objUserManagementIDCsUpdNew != null && pButtonType == FunctionTypeFlag.FunctionTypeFlag_APPROVAL.Value.ToString())
                    {
                        var pMainPos = _dbContext.ListOfPoss.Where(w => w.Code == objUserManagementIDCsUpdNew.PosCode).Select(s => s.MainPosCode).FirstOrDefault();
                        //Các trường hợp phải gửi lên CNTT để Update vào Core IDC
                        if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code
                            || objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code
                            || (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code && pMainPos != pMainPosOld))
                        {
                            objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Submitted.Value;
                        }
                        //Các trường hợp thực hiện tại chi nhánh Update vào Core IDC
                        else
                        {
                            //Xử lý trường hợp reset password
                            if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code)
                            {
                                iCreateUserIDC++;
                                ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                                objRsUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                                objRsUser.UserId = objUserManagementIDCsUpdNew.UserId;
                                var objResetPasswordUserIDCByApi = await ResetUserPasswordByApiResetUserPw(objRsUser, pUserNameUpd);
                                if (objResetPasswordUserIDCByApi?.ResponseCode != "0")
                                {
                                    iRetIdUpd = -2;
                                }
                                else
                                    objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objResetPasswordUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objResetPasswordUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objResetPasswordUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objResetPasswordUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objResetPasswordUserIDCByApi.ResponseMsg;
                                objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                            }

                            //Xử lý trường hợp mở lại người dùng
                            else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code)
                            {
                                iCreateUserIDC++;
                                ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                                objRsUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                                objRsUser.UserId = objUserManagementIDCsUpdNew.UserId;
                                var objEnableUserIDCByApi = await ChangeUserStatusByApiEnableUser(objRsUser, pUserNameUpd);
                                if (objEnableUserIDCByApi?.ResponseCode != "0")
                                {
                                    iRetIdUpd = -2;
                                }
                                else
                                    objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objEnableUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objEnableUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objEnableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objEnableUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objEnableUserIDCByApi.ResponseMsg;
                                objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                            }

                            //Xử lý trường hợp khóa người dùng
                            else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code)
                            {
                                iCreateUserIDC++;
                                //Check đảm bảo user đề nghị Khóa KHÔNG mở tiền mặt
                                int iCheckOpenCash = CheckOpenCashByUserId(objUserManagementIDCsUpdNew.UserId, objUserManagementIDCsUpdNew.StartDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper());
                                if (iCheckOpenCash > 0)
                                {
                                    iRetIdUpd = 6;
                                    return iRetIdUpd;
                                }
                                //Gọi API khóa người dùng
                                ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                                objRsUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                                objRsUser.UserId = objUserManagementIDCsUpdNew.UserId;
                                var objDisableUserIDCByApi = await ChangeUserStatusByApiDisableUser(objRsUser, pUserNameUpd);
                                if (objDisableUserIDCByApi?.ResponseCode != "0")
                                {
                                    iRetIdUpd = -2;
                                }
                                else
                                    objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objDisableUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;
                                objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                            }

                            //Xử lý trường hợp đổi thông tin người dùng
                            else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code || objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                            {
                                iCreateUserIDC++;
                                //Thực hiện bỏ quyền tiền mặt đối với tập quyền cũ của user
                                if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                {
                                    //TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                                    //listTellerRoleAssign.TellerId = objUserManagementIDCsUpdNew.UserId;
                                    //listTellerRoleAssign.TellerRoleAllowed = 0;//Bỏ quyền tiền mặt
                                    //var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign,pUserNameUpd);
                                    //if (objTellerRoleAssign == null || objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000")
                                    //{
                                    //    iRetIdUpd = 5;
                                    //    return iRetIdUpd;
                                    //}
                                    var objTellerRoleAssign = await ChangeOTPRegisterByUserId(objUserManagementIDCsUpdNew.UserId, 0);
                                    if (objTellerRoleAssign == null || objTellerRoleAssign.Success != 1)
                                    {
                                        iRetIdUpd = 5;
                                        return iRetIdUpd;
                                    }
                                }
                                //Gọi vào API chỉnh sửa người dùng trên IDC
                                var objModifyUser = new ModifyUserRequestViewModel
                                {
                                    AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                                };
                                objModifyUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                                objModifyUser.UserId = objUserManagementIDCsUpdNew.UserId;
                                objModifyUser.NickName = objUserManagementIDCsUpdNew.NickName;
                                objModifyUser.FirstName = objUserManagementIDCsUpdNew.FirstName;
                                objModifyUser.LastName = objUserManagementIDCsUpdNew.LastName;
                                objModifyUser.GroupName = objUserManagementIDCsUpdNew.GroupName;
                                objModifyUser.EntityList = objUserManagementIDCsUpdNew.EntityList;
                                objModifyUser.MobileNumber = objUserManagementIDCsUpdNew.MobileNumber;
                                objModifyUser.EmailAddress = objUserManagementIDCsUpdNew.EmailAddress;
                                objModifyUser.ExpiryDate = objUserManagementIDCsUpdNew.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objModifyUser.DateOfBirth = objUserManagementIDCsUpdNew.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objModifyUser.Language = "vi_VN";
                                objModifyUser.AddUserExtraAttributeRequestViewModel.BranchCode = objUserManagementIDCsUpdNew.PosCode?.TrimStart('0');
                                objModifyUser.AddUserExtraAttributeRequestViewModel.UserRole = objUserManagementIDCsUpdNew.GroupName;
                                objModifyUser.IpSet = objUserManagementIDCsUpdNew.IpSetDetail;
                                objModifyUser.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                                objModifyUser.SubType = objUserManagementIDCsUpdNew.SubType;
                                objModifyUser.StartDate = objUserManagementIDCsUpdNew.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                                var objDisableUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUser, pUserNameUpd);
                                if (objDisableUserIDCByApi?.ResponseCode != "0")
                                {
                                    iRetIdUpd = -2;
                                }
                                else
                                {
                                    objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                    //Lấy dữ liệu User ở IDC để cập nhật
                                    var objViewUserIDCByApi = await GetUserIDCInfoByApiViewUser(objModifyUser.UserId);
                                    if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                                    {
                                        //Trường hợp đổi quyền Thực hiện gán quyền tiền mặt đối với tập quyền mới có cash
                                        if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code)
                                        {
                                            if (objUserManagementIDCsUpdNew.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                                            {
                                                //TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                                                //listTellerRoleAssign.TellerId = objViewUserIDCByApi.UserId;
                                                //listTellerRoleAssign.TellerRoleAllowed = 1;//Gán quyền tiền mặt
                                                //var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign,pUserNameUpd);
                                                //if (objTellerRoleAssign == null || objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000")
                                                //{
                                                //    iRetIdUpd = 5;
                                                //    return iRetIdUpd;
                                                //}
                                                var objTellerRoleAssign = await ChangeOTPRegisterByUserId(objUserManagementIDCsUpdNew.UserId, 1);
                                                if (objTellerRoleAssign == null || objTellerRoleAssign.Success != 1)
                                                {
                                                    iRetIdUpd = 5;
                                                    return iRetIdUpd;
                                                }
                                            }
                                        }

                                        //Update thay đổi vào trường dữ liệu thêm mới tại bảng UserManagementIDC
                                        var objUserManagementIDCsUpdChange = _dbContext.UserManagementIDCs.Where(m => m.UserId == pUserManagementUpd.UserId && m.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code).FirstOrDefault();
                                        if (objUserManagementIDCsUpdChange != null)
                                        {
                                            objUserManagementIDCsUpdChange.MobileNumber = objViewUserIDCByApi.MobileNumber;
                                            objUserManagementIDCsUpdChange.EmailAddress = objViewUserIDCByApi.EmailAddress;
                                            objUserManagementIDCsUpdChange.GroupName = objViewUserIDCByApi.GroupName;
                                            objUserManagementIDCsUpdChange.ModifiedBy = pUserNameUpd;
                                            objUserManagementIDCsUpdChange.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                                            objUserManagementIDCsUpdChange.MailIdFlag = objUserManagementIDCsUpdNew.MailIdFlag;
                                            objUserManagementIDCsUpdChange.ModifiedDate = dCurrentDateTmp;
                                            _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdChange);
                                        }
                                        //Update thay đổi vào bảng UserIDCMaster
                                        UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                        objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserManagementIDCsUpdNew);
                                        objUserIDCMaster.Id = 0;
                                        objUserIDCMaster.MobileNumber = objViewUserIDCByApi.MobileNumber;
                                        objUserIDCMaster.EmailAddress = objViewUserIDCByApi.EmailAddress;
                                        objUserIDCMaster.GroupName = objViewUserIDCByApi.GroupName;
                                        objUserIDCMaster.Status = StatusBusinessFlow.Status_HeadOffice_Approved.Value;
                                        objUserIDCMaster.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                                        objUserIDCMaster.MailIdFlag = objUserManagementIDCsUpdNew.MailIdFlag;
                                        var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, "2");
                                    }
                                }
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objDisableUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;
                                objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                            }
                            //Xử lý trường hợp đổi POS người dùng sang POS cùng chi nhánh
                            else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code)
                            {
                                iCreateUserIDC++;
                                //Thực hiện bỏ quyền tiền mặt đối với tập quyền cũ của user
                                //TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                                //listTellerRoleAssign.TellerId = objUserManagementIDCsUpdNew.UserId;
                                //listTellerRoleAssign.TellerRoleAllowed = 0;//Bỏ quyền tiền mặt
                                //var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign,pUserNameUpd);
                                //if (objTellerRoleAssign == null || objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000")
                                //{
                                //    iRetIdUpd = 5;
                                //    return iRetIdUpd;
                                //}
                                var objTellerRoleAssign = await ChangeOTPRegisterByUserId(objUserManagementIDCsUpdNew.UserId, 0);
                                if (objTellerRoleAssign == null || objTellerRoleAssign.Success != 1)
                                {
                                    iRetIdUpd = 5;
                                    return iRetIdUpd;
                                }
                                //Thực hiện gọi API để đổi POS người dùng iDC
                                var objModifyUser = new ModifyUserRequestViewModel
                                {
                                    AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                                };
                                objModifyUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                                objModifyUser.UserId = objUserManagementIDCsUpdNew.UserId;
                                objModifyUser.NickName = objUserManagementIDCsUpdNew.NickName;
                                objModifyUser.FirstName = objUserManagementIDCsUpdNew.FirstName;
                                objModifyUser.LastName = objUserManagementIDCsUpdNew.LastName;
                                objModifyUser.GroupName = objUserManagementIDCsUpdNew.GroupName;
                                objModifyUser.EntityList = objUserManagementIDCsUpdNew.EntityList;
                                objModifyUser.MobileNumber = objUserManagementIDCsUpdNew.MobileNumber;
                                objModifyUser.EmailAddress = objUserManagementIDCsUpdNew.EmailAddress;
                                objModifyUser.ExpiryDate = objUserManagementIDCsUpdNew.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objModifyUser.DateOfBirth = objUserManagementIDCsUpdNew.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                                objModifyUser.Language = "vi_VN";
                                objModifyUser.AddUserExtraAttributeRequestViewModel.BranchCode = objUserManagementIDCsUpdNew.PosCode?.TrimStart('0');
                                objModifyUser.AddUserExtraAttributeRequestViewModel.UserRole = objUserManagementIDCsUpdNew.GroupName;
                                objModifyUser.IpSet = objUserManagementIDCsUpdNew.IpSetDetail;
                                objModifyUser.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                                objModifyUser.SubType = objUserManagementIDCsUpdNew.SubType;
                                objModifyUser.StartDate = objUserManagementIDCsUpdNew.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                                var objDisableUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUser, pUserNameUpd);
                                if (objDisableUserIDCByApi?.ResponseCode != "0")
                                    iRetIdUpd = -2;
                                else
                                {
                                    objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                    //Lấy thông tin người dùng từ IDC
                                    var objViewUserIDCByApi = await GetUserIDCInfoByApiViewUser(objModifyUser.UserId);
                                    if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                                    {
                                        //Update thay đổi vào trường dữ liệu thêm mới tại bảng UserManagementIDC
                                        var objUserManagementIDCsUpdChange = _dbContext.UserManagementIDCs.Where(m => m.UserId == pUserManagementUpd.UserId && m.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code).FirstOrDefault();
                                        if (objUserManagementIDCsUpdChange != null)
                                        {
                                            objUserManagementIDCsUpdChange.PosCode = objViewUserIDCByApi.BranchCode;
                                            objUserManagementIDCsUpdChange.PosName = objUserManagementIDCsUpdNew.PosName;
                                            objUserManagementIDCsUpdChange.ModifiedBy = pUserNameUpd;
                                            objUserManagementIDCsUpdChange.ModifiedDate = dCurrentDateTmp;
                                            objUserManagementIDCsUpdChange.ApproverBy = pUserNameUpd;
                                            objUserManagementIDCsUpdChange.ApprovalDate = dCurrentDateTmp;
                                            objUserManagementIDCsUpdChange.EffectiveDate = dCurrentDateTmp;
                                            _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdChange);
                                        }
                                        //Update thay đổi vào bảng UserIDCMaster
                                        UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                        objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserManagementIDCsUpdNew);
                                        objUserIDCMaster.Id = 0;
                                        objUserIDCMaster.PosCode = objViewUserIDCByApi.BranchCode;
                                        objUserIDCMaster.Status = StatusBusinessFlow.Status_HeadOffice_Approved.Value;
                                        var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, "2");
                                        if (objCreateUserIDCMaster > 0)
                                        {
                                            var objNotiData = await InsertNotiData(objUserManagementIDCsUpdNew, pUserNameUpd);
                                        }
                                    }
                                }

                            }
                        }
                    }
                    //Trường hợp phê duyệt ở TTCNTT
                    else if (objUserManagementIDCsUpdNew != null && pButtonType == FunctionTypeFlag.FunctionTypeFlag_AUTHORIZE.Value.ToString())
                    {
                        //Trường hợp thêm mới người dùng
                        if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                        {
                            //Gọi API để thêm mới người dùng vào IDC
                            AddUserRequestViewModel objAddUser = new AddUserRequestViewModel();
                            objAddUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                            var random = new Random();
                            objAddUser.UserId = objUserManagementIDCsUpdNew.UserId/* + random.Next(10, 99)*/;
                            objAddUser.NickName = objUserManagementIDCsUpdNew.NickName/* + random.Next(10, 99)*/;
                            objAddUser.FirstName = objUserManagementIDCsUpdNew.FirstName;
                            objAddUser.LastName = objUserManagementIDCsUpdNew.LastName;
                            objAddUser.EmailAddress = objUserManagementIDCsUpdNew.EmailAddress;
                            objAddUser.MobileNumber = objUserManagementIDCsUpdNew.MobileNumber;
                            objAddUser.DateOfBirth = objUserManagementIDCsUpdNew.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objAddUser.GroupName = objUserManagementIDCsUpdNew.GroupName;
                            objAddUser.EntityList = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='EntityList' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");//"IDCPROUAT";
                            objAddUser.AuthType = Int32.Parse(objUserManagementIDCsUpdNew.AuthType);
                            objAddUser.UserType = Int32.Parse(objUserManagementIDCsUpdNew.UserType);
                            objAddUser.MailIdFlag = Int32.Parse(objUserManagementIDCsUpdNew.MailIdFlag);
                            objAddUser.ExpiryDate = objUserManagementIDCsUpdNew.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objAddUser.IpSet = objUserManagementIDCsUpdNew.IpSetCode;
                            objAddUser.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                            objAddUser.SubType = objUserManagementIDCsUpdNew.SubType;
                            objAddUser.StartDate = (objUserManagementIDCsUpdNew.StartDate == null) ? DateTime.Now.ToString(FormatParameters.FORMAT_DATE_INT) : objUserManagementIDCsUpdNew.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                            objAddUser.RestrictSameTimeForAllDay = null;
                            objAddUser.AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest
                            {
                                BranchCode = objUserManagementIDCsUpdNew.PosCode?.TrimStart('0'),
                                UserRole = objUserManagementIDCsUpdNew.GroupName
                            };
                            //objAddUser.ListRestrictionRequest = objUserManagementIDCsUpdNew.ListRestrictionRequest;
                            var objCreateUserIDCByApi = await CreateUserIDCByApiAddUser(objAddUser, pUserNameUpd);
                            iCreateUserIDC++;
                            objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                            objUserManagementIDCsUpdNew.SessionValReq = true;
                            objUserManagementIDCsUpdNew.PrevStatus = objCreateUserIDCByApi.PrevStatus;
                            objUserManagementIDCsUpdNew.ResponseAttributes = objCreateUserIDCByApi.ResponseMsg;
                            objUserManagementIDCsUpdNew.CallApiStatus = (objCreateUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                            objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                            objUserManagementIDCsUpdNew.CallApiResponseCode = objCreateUserIDCByApi.ResponseCode;
                            objUserManagementIDCsUpdNew.CallApiResponseMsg = objCreateUserIDCByApi.ResponseMsg;
                            objUserManagementIDCsUpdNew.ModifiedBy = pUserNameUpd;
                            objUserManagementIDCsUpdNew.ModifiedDate = dCurrentDateTmp;

                            if (objCreateUserIDCByApi != null && objCreateUserIDCByApi.Status && objCreateUserIDCByApi.ResponseCode == "0")
                            {
                                //Thực hiện gán quyền tiền mặt đối với tập quyền mới có cash
                                if (objUserManagementIDCsUpdNew.AuthsecType == AuthSecType.AuthSecType_ARXOTP.Code)
                                {
                                    //TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                                    //listTellerRoleAssign.TellerId = objUserManagementIDCsUpdNew.UserId;
                                    //listTellerRoleAssign.TellerRoleAllowed = 1;//Gán quyền tiền mặt
                                    //var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign,pUserNameUpd);
                                    //if (objTellerRoleAssign == null || objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000")
                                    //{
                                    //    iRetIdUpd = 5;
                                    //    return iRetIdUpd;
                                    //}
                                    var objTellerRoleAssign = await ChangeOTPRegisterByUserId(objUserManagementIDCsUpdNew.UserId, 1);
                                    if (objTellerRoleAssign == null || objTellerRoleAssign.Success != 1)
                                    {
                                        iRetIdUpd = 5;
                                        return iRetIdUpd;
                                    }
                                }
                                //Thực hiện Update vào bảng UserIDCManagement
                                objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_HeadOffice_Approved.Value;
                                objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                                objUserManagementIDCsUpdNew.CallApiAutoGeneratedPassword = objCreateUserIDCByApi.UserPassword;
                                //Thực hiện Update vào bảng UserIDCMaster 
                                UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserManagementIDCsUpdNew);
                                var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, "1");
                                if (objCreateUserIDCMaster > 0)
                                {
                                    //Gọi Noti để thông báo đến Email người dùng
                                    var objNotiData = await InsertNotiData(objUserManagementIDCsUpdNew, pUserNameUpd);
                                }
                            }
                            else
                            {
                                iRetIdUpd = -2;
                            }
                        }
                        //Trường hợp đổi POS
                        else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code)
                        {
                            iCreateUserIDC++;
                            //Thực hiện bỏ quyền tiền mặt đối với tập quyền cũ của user
                            //TellerRoleAssignRequestViewModel listTellerRoleAssign = new TellerRoleAssignRequestViewModel();
                            //listTellerRoleAssign.TellerId = objUserManagementIDCsUpdNew.UserId;
                            //listTellerRoleAssign.TellerRoleAllowed = 0;//Bỏ quyền tiền mặt
                            //var objTellerRoleAssign = await ChangeRoleToTransferCashByApiTellerRoleAssign(listTellerRoleAssign,pUserNameUpd);
                            //if (objTellerRoleAssign == null || objTellerRoleAssign.ResponseCode != "0" && objTellerRoleAssign.ResponseCode != "00000")
                            //{
                            //    iRetIdUpd = 5;
                            //    return iRetIdUpd;
                            //}
                            var objTellerRoleAssign = await ChangeOTPRegisterByUserId(objUserManagementIDCsUpdNew.UserId, 0);
                            if (objTellerRoleAssign == null || objTellerRoleAssign.Success != 1)
                            {
                                iRetIdUpd = 5;
                                return iRetIdUpd;
                            }
                            //Thực hiện gọi API để đổi POS người dùng iDC
                            var objModifyUser = new ModifyUserRequestViewModel
                            {
                                AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                            };
                            objModifyUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                            objModifyUser.UserId = objUserManagementIDCsUpdNew.UserId;
                            objModifyUser.NickName = objUserManagementIDCsUpdNew.NickName;
                            objModifyUser.FirstName = objUserManagementIDCsUpdNew.FirstName;
                            objModifyUser.LastName = objUserManagementIDCsUpdNew.LastName;
                            objModifyUser.GroupName = objUserManagementIDCsUpdNew.GroupName;
                            objModifyUser.EntityList = objUserManagementIDCsUpdNew.EntityList;
                            objModifyUser.MobileNumber = objUserManagementIDCsUpdNew.MobileNumber;
                            objModifyUser.EmailAddress = objUserManagementIDCsUpdNew.EmailAddress;
                            objModifyUser.ExpiryDate = objUserManagementIDCsUpdNew.ExpiryDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objModifyUser.DateOfBirth = objUserManagementIDCsUpdNew.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objModifyUser.Language = "vi_VN";
                            objModifyUser.AddUserExtraAttributeRequestViewModel.BranchCode = objUserManagementIDCsUpdNew.PosCode?.TrimStart('0');
                            objModifyUser.AddUserExtraAttributeRequestViewModel.UserRole = objUserManagementIDCsUpdNew.GroupName;
                            objModifyUser.IpSet = objUserManagementIDCsUpdNew.IpSetDetail;
                            objModifyUser.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                            objModifyUser.SubType = objUserManagementIDCsUpdNew.SubType;
                            objModifyUser.StartDate = objUserManagementIDCsUpdNew.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                            var objDisableUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUser, pUserNameUpd);
                            if (objDisableUserIDCByApi?.ResponseCode != "0")
                                iRetIdUpd = -2;
                            else
                            {
                                objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                                //Kiểm tra user ở IDC xem đã thay đổi chưa?
                                var objViewUserIDCByApi = await GetUserIDCInfoByApiViewUser(objModifyUser.UserId);
                                if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                                {
                                    //Update thay đổi vào trường dữ liệu thêm mới tại bảng UserManagementIDC
                                    var objUserManagementIDCsUpdChange = _dbContext.UserManagementIDCs.Where(m => m.UserId == pUserManagementUpd.UserId && m.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code).FirstOrDefault();
                                    if (objUserManagementIDCsUpdChange != null)
                                    {
                                        //Thêm User tại Pos mới
                                        UserManagementIDC objUserManagementIDCPosNew = new UserManagementIDC();
                                        _dbContext.Entry(objUserManagementIDCPosNew).CurrentValues.SetValues(objUserManagementIDCsUpdChange);
                                        objUserManagementIDCPosNew.Id = 0;
                                        objUserManagementIDCPosNew.PosCode = objViewUserIDCByApi.BranchCode;
                                        objUserManagementIDCPosNew.PosName = objUserManagementIDCsUpdNew.PosName;
                                        objUserManagementIDCPosNew.ModifiedBy = pUserNameUpd;
                                        objUserManagementIDCPosNew.ModifiedDate = dCurrentDateTmp;
                                        objUserManagementIDCPosNew.ApproverBy = pUserNameUpd;
                                        objUserManagementIDCPosNew.ApprovalDate = dCurrentDateTmp;
                                        objUserManagementIDCPosNew.EffectiveDate = dCurrentDateTmp;
                                        _dbContext.UserManagementIDCs.Add(objUserManagementIDCPosNew);
                                        //Đóng User theo Pos cũ
                                        objUserManagementIDCsUpdChange.Status = StatusBusinessFlow.Status_Closed.Value;
                                        objUserManagementIDCsUpdChange.ExpiryDate = dCurrentDateTmp;
                                        objUserManagementIDCsUpdChange.ModifiedBy = pUserNameUpd;
                                        objUserManagementIDCsUpdChange.ModifiedDate = dCurrentDateTmp;
                                        objUserManagementIDCsUpdChange.ApproverBy = pUserNameUpd;
                                        objUserManagementIDCsUpdChange.ApprovalDate = dCurrentDateTmp;
                                        _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdChange);
                                    }
                                    //Update thay đổi vào bảng UserIDCMaster
                                    UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                    objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserManagementIDCsUpdNew);
                                    objUserIDCMaster.Id = 0;
                                    objUserIDCMaster.PosCode = objViewUserIDCByApi.BranchCode;
                                    objUserIDCMaster.Status = StatusBusinessFlow.Status_HeadOffice_Approved.Value;
                                    var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, "2");
                                    if (objCreateUserIDCMaster > 0)
                                    {
                                        var objNotiData = await InsertNotiData(objUserManagementIDCsUpdNew, pUserNameUpd);
                                    }
                                }
                            }

                        }
                        //Trường hợp delete user
                        else if (objUserManagementIDCsUpdNew.FunctionType == FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code)
                        {
                            iCreateUserIDC++;
                            //Check đảm bảo user đề nghị Hủy KHÔNG mở tiền mặt
                            int iCheckOpenCash = CheckOpenCashByUserId(objUserManagementIDCsUpdNew.UserId, objUserManagementIDCsUpdNew.StartDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)?.ToUpper());
                            if (iCheckOpenCash > 0)
                            {
                                iRetIdUpd = 6;
                                return iRetIdUpd;
                            }
                            //Đổi ngày hết hạn thành ngày hiện tại
                            var objModifyUser = new ModifyUserRequestViewModel
                            {
                                AddUserExtraAttributeRequestViewModel = new AddUserExtraAttributeRequest()
                            };
                            objModifyUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                            objModifyUser.UserId = objUserManagementIDCsUpdNew.UserId;
                            objModifyUser.NickName = objUserManagementIDCsUpdNew.NickName;
                            objModifyUser.FirstName = objUserManagementIDCsUpdNew.FirstName;
                            objModifyUser.LastName = objUserManagementIDCsUpdNew.LastName;
                            objModifyUser.GroupName = objUserManagementIDCsUpdNew.GroupName;
                            objModifyUser.EntityList = objUserManagementIDCsUpdNew.EntityList;
                            objModifyUser.MobileNumber = objUserManagementIDCsUpdNew.MobileNumber;
                            objModifyUser.EmailAddress = objUserManagementIDCsUpdNew.EmailAddress;
                            objModifyUser.ExpiryDate = dCurrentDateTmp.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objModifyUser.DateOfBirth = objUserManagementIDCsUpdNew.DateOfBirth.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD);
                            objModifyUser.Language = "vi_VN";
                            objModifyUser.AddUserExtraAttributeRequestViewModel.BranchCode = objUserManagementIDCsUpdNew.PosCode?.TrimStart('0');
                            objModifyUser.AddUserExtraAttributeRequestViewModel.UserRole = objUserManagementIDCsUpdNew.GroupName;
                            objModifyUser.IpSet = objUserManagementIDCsUpdNew.IpSetDetail;
                            objModifyUser.AuthsecType = objUserManagementIDCsUpdNew.AuthsecType;
                            objModifyUser.SubType = objUserManagementIDCsUpdNew.SubType;
                            objModifyUser.StartDate = objUserManagementIDCsUpdNew.StartDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                            var objModifyeUserIDCByApi = await ModifyUserByApiModifyUser(objModifyUser, pUserNameUpd);
                            if (objModifyeUserIDCByApi?.ResponseCode != "0")
                            {
                                iRetIdUpd = -2;
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objModifyeUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objModifyeUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objModifyeUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objModifyeUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objModifyeUserIDCByApi.ResponseMsg;
                                _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                                iSaveChanges = _dbContext.SaveChanges();
                                return iRetIdUpd;
                            }
                            //Hủy User
                            ViewUserRequestViewModel objRsUser = new ViewUserRequestViewModel();
                            objRsUser.Ticket = objUserManagementIDCsUpdNew.Ticket;
                            objRsUser.UserId = objUserManagementIDCsUpdNew.UserId;
                            var objDisableUserIDCByApi = await ChangeUserStatusByApiDisableUser(objRsUser, pUserNameUpd);
                            if (objDisableUserIDCByApi?.ResponseCode != "0")
                            {
                                iRetIdUpd = -2;
                                objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.SessionValReq = objDisableUserIDCByApi.SessionValReq;
                                objUserManagementIDCsUpdNew.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                                objUserManagementIDCsUpdNew.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                                objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                                objUserManagementIDCsUpdNew.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                                objUserManagementIDCsUpdNew.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;
                                _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                                iSaveChanges = _dbContext.SaveChanges();
                                return iRetIdUpd;
                            }
                            //Kiểm tra user ở IDC xem đã thay đổi chưa?
                            var objViewUserIDCByApi = await GetUserIDCInfoByApiViewUser(objModifyUser.UserId);
                            if (objViewUserIDCByApi?.ServiceStatusResponseResponseCode == "0")
                            {
                                //Update thay đổi vào trường dữ liệu thêm mới tại bảng UserManagementIDC
                                var objUserManagementIDCsUpdChange = _dbContext.UserManagementIDCs.Where(m => m.UserId == pUserManagementUpd.UserId && m.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code).FirstOrDefault();
                                if (objUserManagementIDCsUpdChange != null)
                                {
                                    objUserManagementIDCsUpdChange.Status = (objViewUserIDCByApi.UserStatus == 1) ? StatusBusinessFlow.Status_Closed.Value : objUserManagementIDCsUpdChange.Status;
                                    objUserManagementIDCsUpdChange.EffectiveDate = dCurrentDateTmp;
                                    objUserManagementIDCsUpdChange.ExpiryDate = dCurrentDateTmp;
                                    _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdChange);
                                }
                                //Update thay đổi vào bảng UserIDCMaster
                                UserIDCMasterViewModel objUserIDCMaster = new UserIDCMasterViewModel();
                                objUserIDCMaster = _mapper.Map<UserIDCMasterViewModel>(objUserManagementIDCsUpdNew);
                                objUserIDCMaster.Id = 0;
                                objUserIDCMaster.Status = StatusBusinessFlow.Status_Closed.Value;
                                objUserIDCMaster.ExpiryDate = dCurrentDateTmp;
                                var objCreateUserIDCMaster = await SaveUserIDCMaster(objUserIDCMaster, pUserNameUpd, "2");
                                if (objCreateUserIDCMaster > 0)
                                {
                                    var objNotiData = await InsertNotiData(objUserManagementIDCsUpdNew, pUserNameUpd);
                                }
                            }
                            objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Approved.Value;
                            objUserManagementIDCsUpdNew.StatusUpdateCore = iCreateUserIDC;
                            objUserManagementIDCsUpdNew.SessionValReq = objDisableUserIDCByApi.SessionValReq;
                            objUserManagementIDCsUpdNew.PrevStatus = objDisableUserIDCByApi.PrevStatus;
                            objUserManagementIDCsUpdNew.CallApiStatus = (objDisableUserIDCByApi.Status == true) ? ResultValueAPI.ResultValue_Status_Success : ResultValueAPI.ResultValue_Status_Failed;
                            objUserManagementIDCsUpdNew.CallApiReqRecordSl = iCreateUserIDC;
                            objUserManagementIDCsUpdNew.CallApiResponseCode = objDisableUserIDCByApi.ResponseCode;
                            objUserManagementIDCsUpdNew.CallApiResponseMsg = objDisableUserIDCByApi.ResponseMsg;
                            objUserManagementIDCsUpdNew.EffectiveDate = dCurrentDateTmp;
                        }
                        //_dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                        //iSaveChanges = _dbContext.SaveChanges();
                        //if (iSaveChanges > 0 && iRetIdUpd == 0)
                        //{
                        //    iCountUpdate++;
                        //    iRetIdUpd = objUserManagementIDCsUpdNew.Id;
                        //}
                    }
                    //Trường hợp từ chối
                    else if (objUserManagementIDCsUpdNew != null && (pButtonType == FunctionTypeFlag.FunctionTypeFlag_REJECT_BRANCH.Value.ToString() || pButtonType == FunctionTypeFlag.FunctionTypeFlag_REJECT_MAIN.Value.ToString()))
                    {
                        bool isRejectBranch = pButtonType == FunctionTypeFlag.FunctionTypeFlag_REJECT_BRANCH.Value.ToString();
                        bool isRejectMain = pButtonType == FunctionTypeFlag.FunctionTypeFlag_REJECT_MAIN.Value.ToString();

                        if (isRejectBranch || isRejectMain)
                        {
                            objUserManagementIDCsUpdNew.ApproverBy = pUserNameUpd;
                            objUserManagementIDCsUpdNew.ApprovalDate = dCurrentDateTmp;
                            if (isRejectBranch)
                            {
                                objUserManagementIDCsUpdNew.ModifiedBy = pUserNameUpd;
                                objUserManagementIDCsUpdNew.ModifiedDate = dCurrentDateTmp;
                                objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_Branch_Rejected.Value;
                            }
                            else
                            {
                                objUserManagementIDCsUpdNew.Status = StatusBusinessFlow.Status_HeadOffice_Rejected.Value;
                            }

                            //_dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                            //iSaveChanges = _dbContext.SaveChanges();

                            //if (iSaveChanges > 0 && iRetIdUpd == 0)
                            //{
                            //    iCountUpdate++;
                            //    iRetIdUpd = objUserManagementIDCsUpdNew.Id;
                            //}
                        }
                    }
                    objUserManagementIDCsUpdNew.ModifiedBy = pUserNameUpd;
                    objUserManagementIDCsUpdNew.ModifiedDate = dCurrentDateTmp;
                    objUserManagementIDCsUpdNew.ApproverBy = pUserNameUpd;
                    objUserManagementIDCsUpdNew.ApprovalDate = dCurrentDateTmp;
                    _dbContext.UserManagementIDCs.Update(objUserManagementIDCsUpdNew);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0 && iRetIdUpd == 0)
                    {
                        iCountUpdate++;
                        iRetIdUpd = objUserManagementIDCsUpdNew.Id;
                        if (objUserManagementIDCsUpdNew.Status == StatusBusinessFlow.Status_Branch_Approved.Value)
                        {
                            var objNotiData = await InsertNotiData(objUserManagementIDCsUpdNew, pUserNameUpd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveUserIDCMaster('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật thông tin cấu hình lãi suất " +
                                        $"SaveUserIDCMaster('{pUserManagementUpd.UserId}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return iRetIdUpd;
        }







        /*
         /// <summary>
        /// Hàm lấy danh sách bản ghi trong bảng UserManagementIDC Thông tin tài khoản người dùng Intellect iDC
        /// </summary>
        /// <param name="pId">Chỉ số khóa xác định bản ghi (Không bắt buộc)</param>
        /// <param name="pMainPosCode">Mã chi nhánh (Không bắt buộc). Ex: 002721</param>
        /// <param name="pPosCode">Mã đơn vị POS (Không bắt buộc)</param>
        /// <param name="pUserId">Tên đăng nhập người dùng</param>
        /// <param name="pFullName">Họ và tên (Không bắt buộc)</param>
        /// <param name="pStaffCode">Mã cán bộ của người dùng (Không bắt buộc)</param>
        /// <returns>Danh sách bản ghi trong bảng UserIDCMaster Thông tin tài khoản người dùng Intellect iDC</returns>
        public List<UserManagementIDCViewModel> GetListUserIDCManagement(long pId, string pMainPosCode, string pPosCode, string pUserId, string pFullName, string pStaffCode, string pFunctionType, int iStatus)
        {
            try
            {
                List<string> listOfPosFind = new List<string>();
                listOfPosFind = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && w.Status == StatusLov.StatusOpenPOS
                                                            && (string.IsNullOrEmpty(pMainPosCode) || pMainPosCode == "000100" || (w.MainPosCode == pMainPosCode))
                                                            ).OrderBy(o => o.Code).Select(s => s.Code).ToList();
                List<UserManagementIDCViewModel> listUserIDCManagement = new List<UserManagementIDCViewModel>();
                List<UserManagementIDC> listUserIDCManagementTemp = new List<UserManagementIDC>();
                if (pFunctionType == FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code)
                {
                    var addNewUsers = _dbContext.UserManagementIDCs.Where(x => x.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code);                
                    listUserIDCManagementTemp = (from w in _dbContext.UserManagementIDCs join add in addNewUsers
                                                    on w.UserId equals add.UserId into gj from add in gj.DefaultIfEmpty()                
                                                    where w.Id == pId || (pId == 0 && (listOfPosFind == null || listOfPosFind.Count == 0|| listOfPosFind.Contains(add.PosCode))               
                                                    && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || add.PosCode == pPosCode)              
                                                    && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                                                    && (w.FunctionType == pFunctionType)
                                                    && (iStatus == -1 || w.Status == iStatus)
                                                    && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode))
                                                    orderby add.PosCode descending, w.GroupName, w.UserId select w)
                                                .Where(c =>string.IsNullOrEmpty(pFullName) || (c.LastName != null && pFullName.ToLower().Contains(c.LastName.ToLower()))
                                                    || (c.LastName != null && Utilities.ConvertToUnSign(pFullName.ToLower()).IndexOf(c.LastName.ToLower(),StringComparison.CurrentCultureIgnoreCase) >= 0)).ToList();
                }    
                else
                {
                    listUserIDCManagementTemp = _dbContext.UserManagementIDCs.Where(w => w.Id == pId || (pId == 0
                            && (listOfPosFind == null || listOfPosFind.Count <= 0 || listOfPosFind.Contains(w.PosCode)) 
                            && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || (w.PosCode == pPosCode))
                            && (string.IsNullOrEmpty(pUserId) || w.UserId == pUserId)
                            && (string.IsNullOrEmpty(pFunctionType) || w.FunctionType == pFunctionType)
                            && (iStatus == -1 || w.Status == iStatus)
                            && (string.IsNullOrEmpty(pStaffCode) || w.StaffCode == pStaffCode)))
                            .Where(delegate (UserManagementIDC c)
                            {
                                if (string.IsNullOrEmpty(pFullName)
                                    || (c.LastName != null && pFullName.ToLower().Contains(c.LastName.ToLower()))
                                    || (c.LastName != null && Utilities.ConvertToUnSign(pFullName.ToLower()).IndexOf(c.LastName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    )
                                    return true;
                                else
                                    return false;
                            }).OrderByDescending(o => o.PosCode).ThenBy(o => o.GroupName).ThenBy(o => o.UserId).ToList();

                }    
                if (listUserIDCManagementTemp != null && listUserIDCManagementTemp.Count != 0)
                {
                    int iCountTemp = 0;
                    var listRoleUsers = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                    foreach (var item in listUserIDCManagementTemp)
                    {
                        iCountTemp++;
                        UserManagementIDCViewModel objItem = new UserManagementIDCViewModel();
                        objItem = _mapper.Map<UserManagementIDCViewModel>(item);
                        objItem.OrderNo = iCountTemp;
                        objItem.FullName = objItem.FirstName + " " + objItem.LastName;
                        objItem.StatusText = StatusBusinessFlow.GetByValue(item.Status).Description;
                        objItem.AuthsecTypeName = int.TryParse(objItem.AuthsecType, out var v) ? AuthSecType.GetByValue(v)?.Description : "";
                        objItem.MailIdFlagName = int.TryParse(objItem.MailIdFlag, out var y) ? MailIdFlag.GetByValue(y)?.Description : "";
                        var pFunctionTypeMap = new Dictionary<string, string>
                        {
                            { FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code, FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Code, FunctionTypeFlag.FunctionTypeFlag_ResetPassword.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Code, FunctionTypeFlag.FunctionTypeFlag_ENABLE_USER.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Code, FunctionTypeFlag.FunctionTypeFlag_DISABLE_USER.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Code, FunctionTypeFlag.FunctionTypeFlag_MODIFY_USER.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Code, FunctionTypeFlag.FunctionTypeFlag_CHANGE_POS.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Code, FunctionTypeFlag.FunctionTypeFlag_CHANGE_ROLE.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_APPROVAL.Code, FunctionTypeFlag.FunctionTypeFlag_APPROVAL.Description },
                            { FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Code, FunctionTypeFlag.FunctionTypeFlag_DELETE_USER.Description },
                        };
                        objItem.GroupNameText = item.GroupName;
                        objItem.FunctionTypeName = pFunctionTypeMap.ContainsKey(objItem.FunctionType) ? pFunctionTypeMap[objItem.FunctionType] : "";
                        if (listRoleUsers != null && listRoleUsers.Count != 0)
                        {
                            objItem.GroupNameText = $"{item.GroupName} - {listRoleUsers.Where(w => w.Code == item.GroupName).Select(s => s.ShortName).FirstOrDefault()}";
                        }
                        listUserIDCManagement.Add(objItem);
                    }
                }
                return listUserIDCManagement;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

         */


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

        /// <summary>
        /// Hàm thực hiện thêm mới tài khoản người dùng Intellect iDC => Gọi đến API addUser thêm mới thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// Ví dụ cách gọi Hàm:
        ///             AddUserRequestViewModel requestInputAddUser = new AddUserRequestViewModel();
        ///             requestInputAddUser.Ticket = "";
        ///             requestInputAddUser.UserId = "CHUDV004";
        ///             requestInputAddUser.NickName = "CHUDV004";
        ///             requestInputAddUser.FirstName= "Dương Quyết";
        ///             requestInputAddUser.LastName = "Văn Chữ";
        ///             requestInputAddUser.EmailAddress = "chudv2510@gmail.com";
        ///             requestInputAddUser.MobileNumber = "0908688212";
        ///             requestInputAddUser.DateOfBirth = "1983-10-25";
        ///             requestInputAddUser.GroupName = "POGD";
        ///             requestInputAddUser.EntityList = "IDCPRODC";
        ///             requestInputAddUser.AuthType = 1;
        ///             requestInputAddUser.UserType = 1;
        ///             requestInputAddUser.MailIdFlag = MailIdFlag.MailIdFlag_RandomSendAPI.Value;
        ///             requestInputAddUser.ExpiryDate = "2050-12-31";
        ///             AddUserExtraAttributeRequest objAddUserExtraAttribute = new AddUserExtraAttributeRequest();
        ///             objAddUserExtraAttribute.BranchCode = "2501";
        ///             objAddUserExtraAttribute.UserRole = "POGD";
        ///             requestInputAddUser.AddUserExtraAttributeRequestViewModel = objAddUserExtraAttribute;
        ///             var resultAddUser = _userManagementIDCService.CreateUserIDCByApiAddUser(requestInputAddUser, UserName);
        /// </summary>
        /// <param name="requestInput">Thông tin tài khoản người dùng Intellect iDC cần tạo</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về theo Model AddUserAPIResponseViewModel</returns>
        /// <exception cref="Exception"></exception>
        public async Task<AddUserAPIResponseViewModel> CreateUserIDCByApiAddUser(AddUserRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            AddUserAPIResponseViewModel objResultAddUser = new AddUserAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    var apiResponse = await _apiInternalEsbService.CreateUserIDCByAPIAddUser(requestInput);

                    if (apiResponse != null && (apiResponse.ResponseCode == "0" || apiResponse.ResponseCode == "00000"))
                    {
                        objResultAddUser.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultAddUser.PrevStatus = apiResponse.PrevStatus;
                        if (apiResponse.ResponseAttributes != null)
                            objResultAddUser.UserPassword = string.IsNullOrEmpty(apiResponse.ResponseAttributes.UsrPasswd) ? "" : apiResponse.ResponseAttributes.UsrPasswd;
                        else objResultAddUser.UserPassword = "";
                        objResultAddUser.ResponseCode = apiResponse.ResponseCode;
                        objResultAddUser.ResponseMsg = apiResponse.ResponseMsg;
                        objResultAddUser.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                    }
                    else
                    {
                        objResultAddUser.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultAddUser.PrevStatus = apiResponse.PrevStatus;
                        if (apiResponse.ResponseAttributes != null)
                            objResultAddUser.UserPassword = string.IsNullOrEmpty(apiResponse.ResponseAttributes.UsrPasswd) ? "" : apiResponse.ResponseAttributes.UsrPasswd;
                        else objResultAddUser.UserPassword = "";
                        objResultAddUser.ResponseCode = apiResponse.ResponseCode;
                        objResultAddUser.ResponseMsg = apiResponse.ResponseMsg;
                        objResultAddUser.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateUserIDCByApiAddUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm tạo tài khoản người dùng Intellect iDC " +
                                        $"CreateUserIDCByApiAddUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultAddUser;
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

        /// <summary>
        /// Hàm thực hiện gọi API tellerRoleAssign gán hoặc bỏ gán quyền tiền mặt cho người dùng đăng nhập Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/tellerRoleAssign
        /// Ví dụ cách gọi:
        ///             TellerRoleAssignRequestViewModel requestInput = new TellerRoleAssignRequestViewModel();
        ///             requestInput.TellerId = "CHUDV002";
        ///             requestInput.TellerRoleAllowed = 1;
        ///             requestInput.MkrId = ConstValueAPI.UserId_Call_ApiIDC;
        ///             var objTellerRoleAssignResult = _userManagementIDCService.ChangeRoleToTransferCashByApiTellerRoleAssign(requestInput, UserName);
        ///             if (objTellerRoleAssignResult != null && objTellerRoleAssignResult.Result != null)
        ///             {
        ///                 if (objTellerRoleAssignResult.Result.ResponseCode == "0" || objTellerRoleAssignResult.Result.ResponseCode == "00000")
        ///                 {
        ///                 }
        ///             } 
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "tellerId": "CHUDV002",
        ///         "tellerRoleAllowed": "1",
        ///         "mkrId": "IDCADMIN"
        ///     }
        /// </param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "txnStatus": "Success",
        ///         "responseMsg": "API Invocation Success",
        ///         "responseCode": "00000"
        ///     }
        ///     {
        ///         "txnStatus": "FAILED",
        ///         "responseMsg": "INVALID TELLER ID",
        ///         "responseCode": ""
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<TellerRoleAssignAPIResponseViewModel> ChangeRoleToTransferCashByApiTellerRoleAssign(TellerRoleAssignRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            TellerRoleAssignAPIResponseViewModel objResultTellerRoleAssign = new TellerRoleAssignAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.TellerId))
                {
                    if (string.IsNullOrEmpty(requestInput.MkrId))
                        requestInput.MkrId = _serviceLOV.GetCellValueForQuery($"Select IsNull(Notes,'') As Code From ListOfValue Where Code='UserIdCallAPIIDC' And ParentId={ListOfValueParentValue.ParentIdConfigIntellectIDC}");
                    var apiResponse = await _apiInternalEsbService.ChangeRoleToTransferCashByAPITellerRoleAssign(requestInput);

                    if (apiResponse == null)
                    {
                        objResultTellerRoleAssign.ResponseCode = "";
                        objResultTellerRoleAssign.ResponseMsg = "Error";
                        objResultTellerRoleAssign.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        objResultTellerRoleAssign.ResponseCode = apiResponse.ResponseCode;
                        objResultTellerRoleAssign.ResponseMsg = apiResponse.ResponseMsg;
                        objResultTellerRoleAssign.TxnStatus = apiResponse.TxnStatus.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                //iRetIdUpd = -1;
                Console.WriteLine($"ChangeRoleToTransferCashByApiTellerRoleAssign('{requestInput.TellerId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật thông tin cấu hình lãi suất " +
                                        $"ChangeRoleToTransferCashByApiTellerRoleAssign('{requestInput.TellerId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultTellerRoleAssign;
        }

        /// <summary>
        /// Hàm thực hiện Mở/Kích hoạt lại tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/enableUser
        /// Ví dụ cách sử dụng:
        ///     ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
        ///     requestInput.UserId = "CHUDV002";
        ///     requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
        ///     var objEnableUserResult = _userManagementIDCService.ChangeUserStatusByApiEnableUser(requestInput, UserName);
        ///     if (objEnableUserResult != null && objEnableUserResult.Result != null)
        ///     {
        ///         if (objEnableUserResult.Result.ResponseCode == "0" || objEnableUserResult.Result.ResponseCode == "00000")
        ///         {
        ///         }
        ///     }
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "enabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "enabled_at": "2026-03-27T10:06:40+00:00",
        ///         "responseCode": 0,
        ///         "responseMsg": "Enable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already enabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChangeInforUserIDCAPIResponseViewModel> ChangeUserStatusByApiEnableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            ChangeInforUserIDCAPIResponseViewModel objResultChangeUserStatus = new ChangeInforUserIDCAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    if (string.IsNullOrEmpty(requestInput.Ticket))
                        requestInput.Ticket = ConstValueAPI.Ticket;
                    var apiResponse = await _apiInternalEsbService.ChangeUserStatusByAPIEnableUser(requestInput);
                    if (apiResponse == null)
                    {
                        objResultChangeUserStatus.SessionValReq = false;
                        objResultChangeUserStatus.PrevStatus = 0;
                        objResultChangeUserStatus.ResponseCode = "-1";
                        objResultChangeUserStatus.ResponseMsg = "";
                        objResultChangeUserStatus.Status = false;
                        objResultChangeUserStatus.EmailAddress = "";
                        objResultChangeUserStatus.MobileNumber = "";
                        objResultChangeUserStatus.UserId = "";
                        objResultChangeUserStatus.EnabledAt = "";
                        objResultChangeUserStatus.EnabledBy = "";
                        objResultChangeUserStatus.DisabledAt = "";
                        objResultChangeUserStatus.DisabledBy = "";
                        objResultChangeUserStatus.ResetAt = "";
                        objResultChangeUserStatus.ResetBy = "";
                        objResultChangeUserStatus.MailFlag = "";
                        objResultChangeUserStatus.StatusCode = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        objResultChangeUserStatus.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultChangeUserStatus.PrevStatus = apiResponse.PrevStatus ?? 0;
                        objResultChangeUserStatus.ResponseCode = apiResponse.ResponseCode;
                        objResultChangeUserStatus.ResponseMsg = apiResponse.ResponseMsg;
                        objResultChangeUserStatus.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                        objResultChangeUserStatus.EmailAddress = apiResponse.EmailAddress ?? "";
                        objResultChangeUserStatus.MobileNumber = apiResponse.MobileNumber ?? "";
                        objResultChangeUserStatus.UserId = apiResponse.UserId ?? "";
                        objResultChangeUserStatus.EnabledAt = apiResponse.EnabledAt ?? "";
                        objResultChangeUserStatus.EnabledBy = apiResponse.EnabledBy ?? "";
                        objResultChangeUserStatus.DisabledAt = apiResponse.DisabledAt ?? "";
                        objResultChangeUserStatus.DisabledBy = apiResponse.DisabledBy ?? "";
                        objResultChangeUserStatus.ResetAt = "";
                        objResultChangeUserStatus.ResetBy = "";
                        objResultChangeUserStatus.MailFlag = "";
                        objResultChangeUserStatus.StatusCode = apiResponse.StatusCode ?? ResultValueAPI.ResultValue_Status_Success;
                    }
                }
            }
            catch (Exception ex)
            {
                objResultChangeUserStatus.SessionValReq = false;
                objResultChangeUserStatus.PrevStatus = 0;
                objResultChangeUserStatus.ResponseCode = "-1";
                objResultChangeUserStatus.ResponseMsg = ex.Message;
                objResultChangeUserStatus.Status = false;
                objResultChangeUserStatus.EmailAddress = "";
                objResultChangeUserStatus.MobileNumber = "";
                objResultChangeUserStatus.UserId = "";
                objResultChangeUserStatus.EnabledAt = "";
                objResultChangeUserStatus.EnabledBy = "";
                objResultChangeUserStatus.DisabledAt = "";
                objResultChangeUserStatus.DisabledBy = "";
                objResultChangeUserStatus.ResetAt = "";
                objResultChangeUserStatus.ResetBy = "";
                objResultChangeUserStatus.MailFlag = "";
                objResultChangeUserStatus.StatusCode = ResultValueAPI.ResultValue_Status_Errored;

                Console.WriteLine($"ChangeUserStatusByApiEnableUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm mở/kích hoạt tài khoản người dùng Intellect iDC " +
                                        $"ChangeUserStatusByApiEnableUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultChangeUserStatus;
        }

        /// <summary>
        /// Hàm thực hiện Đóng/Khóa tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/disableUser
        /// Ví dụ cách sử dụng:
        ///     ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
        ///     requestInput.UserId = "CHUDV002";
        ///     requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
        ///     var objDisableUserResult = _userManagementIDCService.ChangeUserStatusByApiDisableUser(requestInput, UserName);
        ///     if (objDisableUserResult != null && objDisableUserResult.Result != null)
        ///     {
        ///         if (objDisableUserResult.Result.ResponseCode == "0" || objDisableUserResult.Result.ResponseCode == "00000")
        ///         {
        ///         }
        ///     }
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về. Ex:
        ///     {
        ///         "emailAddress": "chudv2510@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "disabled_at": "2026-03-27T10:06:40+00:00",
        ///         "disabled_by": "MOBILE",
        ///         "userId": "CHUDV002",
        ///         "responseCode": 0,
        ///         "responseMsg": "Disable User Done Successfully"
        ///     }
        /// Kết quả không thành công:
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "responseCode": 735,
        ///         "responseMsg": "User is already disabled.",
        ///         "status": "true"
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChangeInforUserIDCAPIResponseViewModel> ChangeUserStatusByApiDisableUser(ViewUserRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            ChangeInforUserIDCAPIResponseViewModel objResultChangeUserStatus = new ChangeInforUserIDCAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    if (string.IsNullOrEmpty(requestInput.Ticket))
                        requestInput.Ticket = ConstValueAPI.Ticket;
                    var apiResponse = await _apiInternalEsbService.ChangeUserStatusByAPIDisableUser(requestInput);
                    if (apiResponse == null)
                    {
                        objResultChangeUserStatus.SessionValReq = false;
                        objResultChangeUserStatus.PrevStatus = 0;
                        objResultChangeUserStatus.ResponseCode = "-1";
                        objResultChangeUserStatus.ResponseMsg = "";
                        objResultChangeUserStatus.Status = false;
                        objResultChangeUserStatus.EmailAddress = "";
                        objResultChangeUserStatus.MobileNumber = "";
                        objResultChangeUserStatus.UserId = "";
                        objResultChangeUserStatus.EnabledAt = "";
                        objResultChangeUserStatus.EnabledBy = "";
                        objResultChangeUserStatus.DisabledAt = "";
                        objResultChangeUserStatus.DisabledBy = "";
                        objResultChangeUserStatus.ResetAt = "";
                        objResultChangeUserStatus.ResetBy = "";
                        objResultChangeUserStatus.MailFlag = "";
                        objResultChangeUserStatus.StatusCode = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        objResultChangeUserStatus.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultChangeUserStatus.PrevStatus = apiResponse.PrevStatus ?? 0;
                        objResultChangeUserStatus.ResponseCode = apiResponse.ResponseCode;
                        objResultChangeUserStatus.ResponseMsg = apiResponse.ResponseMsg;
                        objResultChangeUserStatus.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                        objResultChangeUserStatus.EmailAddress = apiResponse.EmailAddress ?? "";
                        objResultChangeUserStatus.MobileNumber = apiResponse.MobileNumber ?? "";
                        objResultChangeUserStatus.UserId = apiResponse.UserId ?? "";
                        objResultChangeUserStatus.EnabledAt = apiResponse.EnabledAt ?? "";
                        objResultChangeUserStatus.EnabledBy = apiResponse.EnabledBy ?? "";
                        objResultChangeUserStatus.DisabledAt = apiResponse.DisabledAt ?? "";
                        objResultChangeUserStatus.DisabledBy = apiResponse.DisabledBy ?? "";
                        objResultChangeUserStatus.ResetAt = "";
                        objResultChangeUserStatus.ResetBy = "";
                        objResultChangeUserStatus.MailFlag = "";
                        objResultChangeUserStatus.StatusCode = apiResponse.StatusCode ?? ResultValueAPI.ResultValue_Status_Success;
                    }
                }
            }
            catch (Exception ex)
            {
                objResultChangeUserStatus.SessionValReq = false;
                objResultChangeUserStatus.PrevStatus = 0;
                objResultChangeUserStatus.ResponseCode = "-1";
                objResultChangeUserStatus.ResponseMsg = ex.Message;
                objResultChangeUserStatus.Status = false;
                objResultChangeUserStatus.EmailAddress = "";
                objResultChangeUserStatus.MobileNumber = "";
                objResultChangeUserStatus.UserId = "";
                objResultChangeUserStatus.EnabledAt = "";
                objResultChangeUserStatus.EnabledBy = "";
                objResultChangeUserStatus.DisabledAt = "";
                objResultChangeUserStatus.DisabledBy = "";
                objResultChangeUserStatus.ResetAt = "";
                objResultChangeUserStatus.ResetBy = "";
                objResultChangeUserStatus.MailFlag = "";
                objResultChangeUserStatus.StatusCode = ResultValueAPI.ResultValue_Status_Errored;

                Console.WriteLine($"ChangeUserStatusByApiDisableUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm đóng/khóa tài khoản người dùng Intellect iDC " +
                                        $"ChangeUserStatusByApiDisableUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultChangeUserStatus;
        }

        /// <summary>
        /// Hàm thực hiện cấp lại mật khẩu tài khoản ngươi dùng Intellect iDC. Gọi đến API của ESB: http://10.63.54.51:7003/vbsp/internal/api/v1/resetUserPw
        /// Ví dụ cách sử dụng:
        ///     ViewUserRequestViewModel requestInput = new ViewUserRequestViewModel();
        ///     requestInput.UserId = "CHUDV002";
        ///     requestInput.Ticket = ConstValueAPI.UserId_Call_ApiIDC;
        ///     var objResetUserPwUserResult = _userManagementIDCService.ResetUserPasswordByApiResetUserPw(requestInput, UserName);
        ///     if (objResetUserPwUserResult != null && objResetUserPwUserResult.Result != null)
        ///     {
        ///         if (objResetUserPwUserResult.Result.ResponseCode == "0" || objResetUserPwUserResult.Result.ResponseCode == "00000")
        ///         {
        ///         }
        ///     }
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào có UserId và Ticket (Để trống)</param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu thành công
        ///     {
        ///         "emailAddress": "chudv.cctt@gmail.com",
        ///         "mobileNumber": "0908688212",
        ///         "reset_by": "SYSTEMADMIN2",
        ///         "userId": "CHUV12",
        ///         "reset_at": "2026-01-14T21:55:10+00:00",
        ///         "mail_flag": "0",
        ///         "responseCode": "0",
        ///         "responseMsg": "Password Reset Successful"
        ///     }
        /// Nếu không thành công
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": "0",
        ///         "responseAttributes": { },
        ///         "responseCode": "5317",
        ///         "responseMsg": "ARX-005317: User does not exist.",
        ///         "status": "true"
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChangeInforUserIDCAPIResponseViewModel> ResetUserPasswordByApiResetUserPw(ViewUserRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            ChangeInforUserIDCAPIResponseViewModel objResultResetUserPw = new ChangeInforUserIDCAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    if (string.IsNullOrEmpty(requestInput.Ticket))
                        requestInput.Ticket = ConstValueAPI.Ticket;
                    var apiResponse = await _apiInternalEsbService.ResetUserPasswordByAPIResetUserPw(requestInput);
                    if (apiResponse == null)
                    {
                        objResultResetUserPw.SessionValReq = false;
                        objResultResetUserPw.PrevStatus = 0;
                        objResultResetUserPw.ResponseCode = "-1";
                        objResultResetUserPw.ResponseMsg = "";
                        objResultResetUserPw.Status = false;
                        objResultResetUserPw.EmailAddress = "";
                        objResultResetUserPw.MobileNumber = "";
                        objResultResetUserPw.UserId = "";
                        objResultResetUserPw.EnabledAt = "";
                        objResultResetUserPw.EnabledBy = "";
                        objResultResetUserPw.DisabledAt = "";
                        objResultResetUserPw.DisabledBy = "";
                        objResultResetUserPw.ResetAt = "";
                        objResultResetUserPw.ResetBy = "";
                        objResultResetUserPw.MailFlag = "";
                        objResultResetUserPw.StatusCode = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        objResultResetUserPw.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultResetUserPw.PrevStatus = apiResponse.PrevStatus ?? 0;
                        objResultResetUserPw.ResponseCode = apiResponse.ResponseCode;
                        objResultResetUserPw.ResponseMsg = apiResponse.ResponseMsg;
                        objResultResetUserPw.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                        objResultResetUserPw.EmailAddress = apiResponse.EmailAddress ?? "";
                        objResultResetUserPw.MobileNumber = apiResponse.MobileNumber ?? "";
                        objResultResetUserPw.UserId = apiResponse.UserId ?? "";
                        objResultResetUserPw.EnabledAt = "";
                        objResultResetUserPw.EnabledBy = "";
                        objResultResetUserPw.DisabledAt = "";
                        objResultResetUserPw.DisabledBy = "";
                        objResultResetUserPw.ResetAt = apiResponse.ResetAt ?? "";
                        objResultResetUserPw.ResetBy = apiResponse.ResetBy ?? "";
                        objResultResetUserPw.MailFlag = apiResponse.MailFlag ?? "";
                        objResultResetUserPw.StatusCode = apiResponse.StatusCode ?? ResultValueAPI.ResultValue_Status_Success;
                    }
                }
            }
            catch (Exception ex)
            {
                objResultResetUserPw.SessionValReq = false;
                objResultResetUserPw.PrevStatus = 0;
                objResultResetUserPw.ResponseCode = "-1";
                objResultResetUserPw.ResponseMsg = ex.Message;
                objResultResetUserPw.Status = false;
                objResultResetUserPw.EmailAddress = "";
                objResultResetUserPw.MobileNumber = "";
                objResultResetUserPw.UserId = "";
                objResultResetUserPw.EnabledAt = "";
                objResultResetUserPw.EnabledBy = "";
                objResultResetUserPw.DisabledAt = "";
                objResultResetUserPw.DisabledBy = "";
                objResultResetUserPw.ResetAt = "";
                objResultResetUserPw.ResetBy = "";
                objResultResetUserPw.MailFlag = "";
                objResultResetUserPw.StatusCode = ResultValueAPI.ResultValue_Status_Errored;

                Console.WriteLine($"ResetUserPasswordByApiResetUserPw('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi khởi tạo/cấp lại mật khẩu tài khoản người dùng Intellect iDC " +
                                        $"ResetUserPasswordByApiResetUserPw('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultResetUserPw;
        }

        /// <summary>
        /// Hàm thực hiện gọi API modifyUser thay đổi thông tin người dùng vào Intellect iDC
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/addUser
        /// </summary>
        /// <param name="requestInput">Thông tin người dùng Intellect iDC cần thay đổi thông tin 
        ///     {
        ///         "ticket": "{{access_token}}",
        ///         "userId": "CHUDV99",
        ///         "firstName": "Dương Văn",
        ///         "lastName": "Chữ",
        ///         "groupName": "POPGD",
        ///         "entityList": "IDCPRODC",
        ///         "mobileNumber": "0908688212",
        ///         "emailAddress": "chudv.2510@gmail.com",
        ///         "expiryDate": "2045-10-25",
        ///         "DOB": "1983-10-25",
        ///         "mailIdFlag": 1,
        ///         "language": "vi_VN",
        ///         "extraAttribute": {
        ///             "BranchCode": "2505",
        ///             "UserRole": "POPGD"
        ///         }
        ///     }
        /// </param>
        /// <param name="pUserNameUpd">Người dùng thực hiện trên HTVH</param>
        /// <returns>Kết quả trả về. Ex: 
        ///     {
        ///         "sessionValReq": "true",
        ///         "prevStatus": 0,
        ///         "responseAttributes": {},
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2505",
        ///         "userRole": "POPGD",
        ///         "responseCode": 0,
        ///         "responseMsg": "Modify User Done Successfully",
        ///         "status": "true"
        ///     }
        /// --Hoặc nếu sửa tiếp POS thì trả ra như sau:
        ///     {
        ///         "mobileNumber": "0908688212",
        ///         "posCode": "2502",
        ///         "userRole": "POPGD",
        ///         "status": "true",
        ///         "responseMsg": " BranchCode Modify Done Successfully",
        ///         "responseCode": 0
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<ChangeInforUserIDCAPIResponseViewModel> ModifyUserByApiModifyUser(ModifyUserRequestViewModel requestInput, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            ChangeInforUserIDCAPIResponseViewModel objResultModifyUser = new ChangeInforUserIDCAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    if (string.IsNullOrEmpty(requestInput.Ticket))
                        requestInput.Ticket = ConstValueAPI.Ticket;
                    var apiResponse = await _apiInternalEsbService.ModifyUserIDCByAPIModifyUser(requestInput);
                    if (apiResponse == null)
                    {
                        objResultModifyUser.SessionValReq = false;
                        objResultModifyUser.PrevStatus = 0;
                        objResultModifyUser.ResponseCode = "-1";
                        objResultModifyUser.ResponseMsg = "";
                        objResultModifyUser.Status = false;
                        objResultModifyUser.EmailAddress = "";
                        objResultModifyUser.MobileNumber = "";
                        objResultModifyUser.UserId = "";
                        objResultModifyUser.EnabledAt = "";
                        objResultModifyUser.EnabledBy = "";
                        objResultModifyUser.DisabledAt = "";
                        objResultModifyUser.DisabledBy = "";
                        objResultModifyUser.ResetAt = "";
                        objResultModifyUser.ResetBy = "";
                        objResultModifyUser.MailFlag = "";

                        objResultModifyUser.PosCode = "";
                        objResultModifyUser.UserRole = "";
                        objResultModifyUser.StatusCode = ResultValueAPI.ResultValue_Status_Failed;
                    }
                    else
                    {
                        objResultModifyUser.SessionValReq = apiResponse.SessionValReq.Trim().ToLower().Equals("true") ? true : false;
                        objResultModifyUser.PrevStatus = apiResponse.PrevStatus ?? 0;
                        objResultModifyUser.ResponseCode = apiResponse.ResponseCode;
                        objResultModifyUser.ResponseMsg = apiResponse.ResponseMsg;
                        objResultModifyUser.Status = apiResponse.Status.Trim().ToLower().Equals("true") ? true : false;
                        objResultModifyUser.EmailAddress = apiResponse.EmailAddress ?? "";
                        objResultModifyUser.MobileNumber = apiResponse.MobileNumber ?? "";
                        objResultModifyUser.UserId = "";
                        objResultModifyUser.EnabledAt = "";
                        objResultModifyUser.EnabledBy = "";
                        objResultModifyUser.DisabledAt = "";
                        objResultModifyUser.DisabledBy = "";
                        objResultModifyUser.ResetAt = "";
                        objResultModifyUser.ResetBy = "";
                        objResultModifyUser.MailFlag = "";
                        objResultModifyUser.PosCode = apiResponse.PosCode ?? "";
                        objResultModifyUser.UserRole = apiResponse.UserRole ?? "";
                        objResultModifyUser.StatusCode = apiResponse.StatusCode ?? ResultValueAPI.ResultValue_Status_Success;
                    }
                }
            }
            catch (Exception ex)
            {
                objResultModifyUser.SessionValReq = false;
                objResultModifyUser.PrevStatus = 0;
                objResultModifyUser.ResponseCode = "-1";
                objResultModifyUser.ResponseMsg = ex.Message;
                objResultModifyUser.Status = false;
                objResultModifyUser.EmailAddress = "";
                objResultModifyUser.MobileNumber = "";
                objResultModifyUser.UserId = "";
                objResultModifyUser.EnabledAt = "";
                objResultModifyUser.EnabledBy = "";
                objResultModifyUser.DisabledAt = "";
                objResultModifyUser.DisabledBy = "";
                objResultModifyUser.ResetAt = "";
                objResultModifyUser.ResetBy = "";
                objResultModifyUser.MailFlag = "";
                objResultModifyUser.PosCode = "";
                objResultModifyUser.UserRole = "";
                objResultModifyUser.StatusCode = ResultValueAPI.ResultValue_Status_Errored;

                Console.WriteLine($"ModifyUserByApiModifyUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi thay đổi thông tin tài khoản người dùng Intellect iDC " +
                                        $"ModifyUserByApiModifyUser('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultModifyUser;
        }

        ///// <summary>
        ///// Hàm lấy danh sách Trình duyệt người dùng IDC
        ///// </summary>
        ///// <returns></returns>
        //public List<UserIDCApprovalViewModel> UserIDCApproval_GetSearch(string pNgayHLBatDau, string pNgayHLKetThuc, string pDonVi, int pFlagCall, string pTrangThai)
        //{
        //    var answer = new List<UserIDCApprovalViewModel>();
        //    try
        //    {
        //        SqlParameter paramNgayHLBatDau = new SqlParameter("@pNgayHLBatDau", SqlDbType.VarChar);
        //        paramNgayHLBatDau.Value = pNgayHLBatDau;
        //        SqlParameter paramNgayHLKetThuc = new SqlParameter("@pNgayHLKetThuc", SqlDbType.VarChar);
        //        paramNgayHLKetThuc.Value = pNgayHLKetThuc;
        //        SqlParameter paramDonViTrinhKT = new SqlParameter("@pDonVi", SqlDbType.VarChar);
        //        paramDonViTrinhKT.Value = pDonVi;
        //        SqlParameter paramFlagCall = new SqlParameter("@pFlagCall", SqlDbType.Int);
        //        paramFlagCall.Value = pFlagCall;
        //        SqlParameter paramTrangThai = new SqlParameter("@pTrangThai", SqlDbType.VarChar);
        //        paramTrangThai.Value = pTrangThai;
        //        var pApprovalTongHops = _dbContext.UserIDCApprovals.FromSqlRaw($"Exec [dbo].[UserIDCApproval_GetSearch] @pNgayHLBatDau,@pNgayHLKetThuc,@pDonVi,@pFlagCall,@pTrangThai", paramNgayHLBatDau, paramNgayHLKetThuc, paramDonViTrinhKT, paramFlagCall, paramTrangThai).ToList();
        //        if (pApprovalTongHops != null)
        //        {
        //            if (pFlagCall == 1)
        //                pApprovalTongHops = pApprovalTongHops.Where(w => w.MaDonVi != "").ToList();
        //            foreach (var item in pApprovalTongHops)
        //            {
        //                UserIDCApprovalViewModel objItem = new UserIDCApprovalViewModel();
        //                objItem = _mapper.Map<UserIDCApprovalViewModel>(item);
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

        
        /// <summary>
        /// Hàm thực hiện gọi API idcPendingTxn/lmsPendingTxn Lấy danh sách giao dịch Pending theo người dùng
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/idcPendingTxn
        /// http://10.63.54.51:7003/vbsp/internal/api/v1/lmsPendingTxn
        /// </summary>
        /// <param name="requestInput">Thông tin đầu vào. Ex:
        ///     {
        ///         "userId": "68510"
        ///     }
        ///     Hoặc
        ///     {
        ///         "userId": "20047"
        ///     }
        /// 
        /// </param>
        /// <param name="pApiName">Tên API truyền vào. Nếu trống sẽ lấy cả 2 API vào (EsbApiName.LMSPendingTxn)</param>
        /// <param name="pUserNameUpd">Người dùng của VBSPOSS thực hiện</param>
        /// <returns>Kết quả trả về. Ex:
        /// Nếu là idcPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "6",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:46:38"
        ///             },
        ///             {
        ///                 "txnDt": "20260302",
        ///                 "txnNarr": "Cash Deposit  ",
        ///                 "tranAmt": "600000",
        ///                 "batchNum": "7",
        ///                 "txnType": "Tạo lập, chỉnh sửa giao dịch Nộp/Rút tiền mặt",
        ///                 "branchCd": "002505",
        ///                 "tranEntTime": "20260403 16:47:17"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// Nếu là lmsPendingTxn
        ///     {
        ///         "txnStatus": "Success",
        ///         "record": [
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             },
        ///             {
        ///                 "txnRefNum": "6600000733118753",
        ///                 "mkrDt": "2026-02-26 16:57:51",
        ///                 "mkrId": "68510",
        ///                 "branchCd": "004301",
        ///                 "status": "Pending for Authorize"
        ///             }
        ///         ],
        ///         "responseCode": "00000",
        ///         "responseMsg": "Api Invocation Success"
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<PendingTransAPIResponseViewModel> GetPendingTransactionsByApiPendingTxn(PendingTransRequestViewModel requestInput, string pApiName, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            PendingTransAPIResponseViewModel objResultPendingTrans = new PendingTransAPIResponseViewModel();
            try
            {
                if (requestInput != null && !string.IsNullOrEmpty(requestInput.UserId))
                {
                    if (!string.IsNullOrEmpty(pApiName))
                    {
                        var apiResponse = await _apiInternalEsbService.GetPendingTransactionsByAPIPendingTxn(requestInput, pApiName);
                        if (apiResponse == null)
                        {
                            objResultPendingTrans.ResponseCode = "";
                            objResultPendingTrans.ResponseMsg = "Error";
                            objResultPendingTrans.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                            objResultPendingTrans.Records = null;
                        }
                        else
                        {
                            objResultPendingTrans.ResponseCode = apiResponse.ResponseCode;
                            objResultPendingTrans.ResponseMsg = apiResponse.ResponseMsg;
                            objResultPendingTrans.TxnStatus = apiResponse.TxnStatus.Trim();
                            List<PendingTransactionInforRecords> listTransPending = new List<PendingTransactionInforRecords>();
                            if (objResultPendingTrans.Records != null && objResultPendingTrans.Records.Count != 0)
                            {
                                foreach (var item in apiResponse.Records)
                                {
                                    PendingTransactionInforRecords itemResult = new PendingTransactionInforRecords();
                                    if (pApiName == EsbApiName.IDCPendingTxn.Code)
                                    {
                                        itemResult.TransDate = item.TransDate;
                                        itemResult.TxnNarr = item.TxnNarr;
                                        itemResult.TransAmount = item.TransAmount.Value;
                                        itemResult.BatchNum = item.BatchNum;
                                        itemResult.TransType = item.TransType;
                                        itemResult.BranchCd = item.BranchCd;
                                        itemResult.TranEntTime = item.TranEntTime.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                        itemResult.TxnRefNum = "";
                                        itemResult.MakerDate = itemResult.TranEntTime;
                                        itemResult.MakerId = requestInput.UserId;
                                        itemResult.Status = "Pending for Authorize";
                                    }
                                    else
                                    {
                                        itemResult.TransDate = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                        if (item.MakerDate.Length > 8)
                                            itemResult.TransDate = itemResult.TransDate.Substring(0, 8);
                                        itemResult.TxnNarr = "Lending";
                                        itemResult.TransAmount = 0;
                                        itemResult.BatchNum = 0;
                                        itemResult.TransType = "Giao dịch về Lending";
                                        itemResult.TranEntTime = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");

                                        itemResult.TxnRefNum = item.TxnRefNum;
                                        itemResult.MakerDate = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                        itemResult.MakerId = item.MakerId;
                                        itemResult.BranchCd = item.BranchCd;
                                        itemResult.Status = item.Status;
                                    }
                                    listTransPending.Add(itemResult);
                                }

                            }
                            objResultPendingTrans.Records.AddRange(listTransPending);
                        }
                    }
                    else
                    {
                        //Trường hợp không truyền tên API thì gọi lấy cả 2 Pending bên IDC và LMS
                        var apiResponseIDC = await _apiInternalEsbService.GetPendingTransactionsByAPIPendingTxn(requestInput, EsbApiName.IDCPendingTxn.Code);
                        if (apiResponseIDC == null)
                        {
                            objResultPendingTrans.ResponseCode = "";
                            objResultPendingTrans.ResponseMsg = "Error";
                            objResultPendingTrans.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                            objResultPendingTrans.Records = null;
                        }
                        else
                        {
                            objResultPendingTrans.ResponseCode = apiResponseIDC.ResponseCode;
                            objResultPendingTrans.ResponseMsg = apiResponseIDC.ResponseMsg;
                            objResultPendingTrans.TxnStatus = apiResponseIDC.TxnStatus.Trim();
                            List<PendingTransactionInforRecords> listTransPending = new List<PendingTransactionInforRecords>();
                            if (objResultPendingTrans.Records != null && objResultPendingTrans.Records.Count != 0)
                            {
                                foreach (var item in apiResponseIDC.Records)
                                {
                                    PendingTransactionInforRecords itemResult = new PendingTransactionInforRecords();
                                    itemResult.TransDate = item.TransDate;
                                    itemResult.TxnNarr = item.TxnNarr;
                                    itemResult.TransAmount = item.TransAmount.Value;
                                    itemResult.BatchNum = item.BatchNum;
                                    itemResult.TransType = item.TransType;
                                    itemResult.BranchCd = item.BranchCd;
                                    itemResult.TranEntTime = item.TranEntTime.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                    itemResult.TxnRefNum = "";
                                    itemResult.MakerDate = itemResult.TranEntTime;
                                    itemResult.MakerId = requestInput.UserId;
                                    itemResult.Status = "Pending for Authorize";
                                    listTransPending.Add(itemResult);
                                }
                            }
                            objResultPendingTrans.Records.AddRange(listTransPending);
                        }

                        var apiResponseLMS = await _apiInternalEsbService.GetPendingTransactionsByAPIPendingTxn(requestInput, EsbApiName.LMSPendingTxn.Code);
                        if (apiResponseLMS == null && objResultPendingTrans == null)
                        {
                            objResultPendingTrans.ResponseCode = "";
                            objResultPendingTrans.ResponseMsg = "Error";
                            objResultPendingTrans.TxnStatus = ResultValueAPI.ResultValue_Status_Failed;
                            objResultPendingTrans.Records = null;
                        }
                        else
                        {
                            if (objResultPendingTrans == null)
                            {
                                objResultPendingTrans.ResponseCode = apiResponseLMS.ResponseCode;
                                objResultPendingTrans.ResponseMsg = apiResponseLMS.ResponseMsg;
                                objResultPendingTrans.TxnStatus = apiResponseLMS.TxnStatus.Trim();
                            }
                            List<PendingTransactionInforRecords> listTransPending = new List<PendingTransactionInforRecords>();
                            if (objResultPendingTrans.Records != null && objResultPendingTrans.Records.Count != 0)
                            {
                                foreach (var item in apiResponseLMS.Records)
                                {
                                    PendingTransactionInforRecords itemResult = new PendingTransactionInforRecords();

                                    itemResult.TransDate = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                    if (item.MakerDate.Length > 8)
                                        itemResult.TransDate = itemResult.TransDate.Substring(0, 8);
                                    itemResult.TxnNarr = "Lending";
                                    itemResult.TransAmount = 0;
                                    itemResult.BatchNum = 0;
                                    itemResult.TransType = "Giao dịch về Lending";
                                    itemResult.TranEntTime = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");

                                    itemResult.TxnRefNum = item.TxnRefNum;
                                    itemResult.MakerDate = item.MakerDate.Replace(" ", "").Replace(":", "").Replace("-", "").Replace("/", "");
                                    itemResult.MakerId = item.MakerId;
                                    itemResult.BranchCd = item.BranchCd;
                                    itemResult.Status = item.Status;

                                    listTransPending.Add(itemResult);
                                }
                            }
                            objResultPendingTrans.Records.AddRange(listTransPending);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //iRetIdUpd = -1;
                Console.WriteLine($"GetPendingTransactionsByApiPendingTxn('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm lấy danh sách giao dịch Pending của người dùng " +
                                        $"GetPendingTransactionsByApiPendingTxn('{requestInput.UserId}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objResultPendingTrans;
        }


        /// <summary>
        /// Hàm kiểm tra xem người dùng có mở sổ tiền mặt đầu ngày không
        /// Ex: SELECT VBSP_OSS_GET.FN_CHECK_OPENCASH_BY_USERID('44573', '03-SEP-2025') FROM DUAL
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng trên iDC</param>
        /// <param name="pReportDate">Ngày kiểm tra định dạng dd-MON-yyyy</param>
        /// <returns>Kết quả trả về:
        ///                 0 - Chưa mở sổ tiền mặt đầu ngày;
        ///                 1 - Đã mở chưa đóng;
        ///                 2 - Đã mở và đóng nhưng còn tồn quỹ tiền mặt chưa chuyển về quỹ chính
        ///                 3 - Đã mở và đóng không còn tồn quỹ tiền mặt
        /// </returns>
        public int CheckOpenCashByUserId(string pUserId, string pReportDate)
        {
            string sReportDate = string.IsNullOrEmpty(pReportDate) ? DateTime.Now.ToString(FormatParameters.FORMAT_DATE_ORA) : pReportDate;
            try
            {
                string sSQL = @"SELECT TO_CHAR(VBSP_OSS_GET.FN_CHECK_OPENCASH_BY_USERID(:P_USERID, :P_REPORTDATE)) AS Value FROM DUAL";

                var result = _dbContextIDC.Set<QueryResult>().FromSqlRaw(sSQL,
                                new OracleParameter(":P_USERID", OracleDbType.Varchar2) { Value = pUserId ?? "" },
                                new OracleParameter(":P_REPORTDATE", OracleDbType.Varchar2) { Value = sReportDate }).FirstOrDefault();
                string sValTemp = result?.Value ?? "0";

                return Convert.ToInt32(sValTemp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Có lỗi khi gọi hàm kiểm tra mở quỹ tiền mặt của người dùng {pUserId} và ngày {sReportDate}. Chi tiết lỗi: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Hàm xóa thông tin phân quyền chức năng của người dùng trên iDC khi người dùng bị khóa tài khoản hoặc xóa tài khoản trên iDC. Thực hiện xóa bản ghi trong bảng AuthSecType theo UserId
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng cần hủy xác thực 2 bước (Xóa xác thực bằng OTP)</param>
        /// <returns>Kết quả</returns>
        public async Task<ExecuteResultModelModel> DeleteAuthSecTypeByUserIdAsync(string pUserId)
        {
            try
            {
                var sUserIdInput = new OracleParameter("P_USERID", OracleDbType.Varchar2) { Direction = ParameterDirection.Input, Value = pUserId };
                var iRowsDeletedOut = new OracleParameter("P_ROWS_DELETED", OracleDbType.Decimal) { Direction = ParameterDirection.Output };
                var iSuccessOut = new OracleParameter("P_SUCCESS", OracleDbType.Decimal) { Direction = ParameterDirection.Output };
                var sMessageOut = new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };

                var sSQL = @"BEGIN VBSP_OSS_UPD.SP_DELETE_AUTHSECTYPE_BY_USERID(:P_USERID, :P_ROWS_DELETED, :P_SUCCESS, :P_MESSAGE); END;";
                await _dbContextIDC.Database.ExecuteSqlRawAsync(sSQL, pUserId, iRowsDeletedOut, iSuccessOut, sMessageOut);

                // Mapping kết quả
                var objExecuteResult = new ExecuteResultModelModel
                        {
                            RowsAffected = iRowsDeletedOut.Value == DBNull.Value ? 0 : Convert.ToInt32(iRowsDeletedOut.Value),
                            Success = iSuccessOut.Value == DBNull.Value ? -1 : Convert.ToInt32(iSuccessOut.Value),
                            Message = sMessageOut.Value?.ToString()
                        };

                // Map TxnStatus chuẩn hoá
                objExecuteResult.TxnStatus = objExecuteResult.Success switch
                {
                    1 => ResultValueAPI.ResultValue_Status_Success,
                    0 => ResultValueAPI.ResultValue_Status_Failed,
                    -1 => ResultValueAPI.ResultValue_Status_Errored,
                    _ => ResultValueAPI.ResultValue_Status_Errored
                };
                return objExecuteResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteAuthSecTypeByUserIdAsync('{pUserId}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm xóa AuthSecType theo UserId " +
                                        $"DeleteAuthSecTypeByUserIdAsync('{pUserId}') => Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hàm thực hiện đăng ký hoặc hủy đăng ký xác thực 2 lớp (Lớp user/password và OTP) cho người dùng (Tương đương hàm IDL_ARX.PRC_OTP_REG của Intellect)
        ///     Nếu đăng ký: Thêm bản ghi vào bảng IDL_ARX.TB_ARM_USER_AUTH_TYPE với AUTH_TYPE_ID = 17
        ///     Nếu hủy đăng ký: Xóa bản ghi bảng IDL_ARX.TB_ARM_USER_AUTH_TYPE với AUTH_TYPE_ID = 17
        /// </summary>
        /// <param name="pUserId">Tài khoản người dùng cần đăng ký/hủy đăng ký xác thực 2 bước (Xác thực lớp 2 bằng OTP)</param>
        /// <param name="pRegisterFlag">Cờ xác định: 1-Đăng ký; 0 - Hủy đăng ký</param>
        /// <returns>Kết quả</returns>
        public async Task<ExecuteResultModelModel> ChangeOTPRegisterByUserId(string pUserId, int pRegisterFlag)
        {
            try
            {
                var sUserIdInput = new OracleParameter("P_USERID", OracleDbType.Varchar2) { Direction = ParameterDirection.Input, Value = pUserId };
                var iRegisterFlagInput = new OracleParameter("P_REG_FLAG", OracleDbType.Int32) { Direction = ParameterDirection.Input, Value = pRegisterFlag };
                var iRowsChangeOut = new OracleParameter("P_ROWS_CHANGE", OracleDbType.Decimal) { Direction = ParameterDirection.Output };
                var iSuccessOut = new OracleParameter("P_SUCCESS", OracleDbType.Decimal) { Direction = ParameterDirection.Output };
                var sMessageOut = new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, 4000) { Direction = ParameterDirection.Output };

                var sSQL = @"BEGIN VBSP_OSS_UPD.PRC_CHANGE_OTP_REGISTER_BY_USERID(:P_USERID, :P_REG_FLAG, :P_ROWS_CHANGE, :P_SUCCESS, :P_MESSAGE); END;";
                await _dbContextIDC.Database.ExecuteSqlRawAsync(sSQL, pUserId, iRegisterFlagInput, iRowsChangeOut, iSuccessOut, sMessageOut);

                // Mapping kết quả
                var objExecuteResult = new ExecuteResultModelModel
                {
                    RowsAffected = iRowsChangeOut.Value == DBNull.Value ? 0 : Convert.ToInt32(iRowsChangeOut.Value),
                    Success = iSuccessOut.Value == DBNull.Value ? -1 : Convert.ToInt32(iSuccessOut.Value),
                    Message = sMessageOut.Value?.ToString()
                };

                // Map TxnStatus chuẩn hoá
                objExecuteResult.TxnStatus = objExecuteResult.Success switch
                {
                    1 => ResultValueAPI.ResultValue_Status_Success,
                    0 => ResultValueAPI.ResultValue_Status_Failed,
                    -1 => ResultValueAPI.ResultValue_Status_Errored,
                    _ => ResultValueAPI.ResultValue_Status_Errored
                };
                return objExecuteResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangeOTPRegisterByUserId('{pUserId}',{pRegisterFlag.ToString()}) => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm xóa AuthSecType theo UserId " +
                                        $"ChangeOTPRegisterByUserId('{pUserId}',{pRegisterFlag.ToString()}) => Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hàm lưu file đính kèm
        /// </summary>
        public async Task<List<long>> Xoa_SaveAttachedFiles(long configureId, List<AttachedFileInfo> attachedFiles, string userId)
        {
            if (attachedFiles == null || !attachedFiles.Any())
            {
                return null;
            }
            try
            {
                List<long> result = new List<long>();
                List<AttachedFileInfo> attachedFilesAdd = new();
                List<AttachedFileInfo> attachedFilesUpdate = new();
                foreach (var attachedFile in attachedFiles)
                {
                    if (attachedFile.FileId == 0)
                    {
                        if (string.IsNullOrEmpty(attachedFile.CreatedBy))
                            attachedFile.CreatedBy = userId ?? "UnknownUser";
                        if (attachedFile.CreatedDate == default)
                            attachedFile.CreatedDate = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(attachedFile.FileName) || string.IsNullOrEmpty(attachedFile.FileNameNew) || string.IsNullOrEmpty(attachedFile.PathFile))
                        {
                            throw new Exception("FileName, FileNameNew, hoặc PathFile không được để trống.");
                        }
                        if (string.IsNullOrEmpty(attachedFile.DocumentNumber))
                        {
                            throw new Exception("DocumentNumber không được để trống.");
                        }
                        attachedFilesAdd.Add(attachedFile);
                    }
                    else
                    {
                        var existingAttachedFile = await _dbContext.AttachedFileInfos.FindAsync(attachedFile.FileId);
                        if (existingAttachedFile != null)
                        {
                            _mapper.Map(attachedFile, existingAttachedFile);
                            existingAttachedFile.ModifiedBy = userId ?? "UnknownUser";
                            existingAttachedFile.ModifiedDate = DateTime.UtcNow;
                            attachedFilesUpdate.Add(existingAttachedFile);
                        }
                    }
                }
                if (attachedFilesUpdate.Any())
                {
                    _dbContext.AttachedFileInfos.UpdateRange(attachedFilesUpdate);
                }
                if (attachedFilesAdd.Any())
                {
                    _dbContext.AttachedFileInfos.AddRange(attachedFilesAdd);
                }

                var changes = await _dbContext.SaveChangesAsync();

                if (attachedFilesAdd.Any())
                {
                    result.AddRange(attachedFilesAdd.Select(s => s.FileId).ToList());
                }

                if (attachedFilesUpdate.Any())
                {
                    result.AddRange(attachedFilesUpdate.Select(s => s.FileId).ToList());
                }

                return result;
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"DB Update Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Hàm lấy danh sách thông tin tệp tin đính kèm theo các điều kiện truyền vào (nếu có)
        /// </summary>
        /// <param name="pFileId">Chỉ số khóa bản ghi File đính kèm</param>
        /// <param name="pThamChieuId">Chỉ số xác định Tham chiếu có file đính kèm (Id của phân hệ có file đính kèm)</param>
        /// <param name="pThamChieuHT">Tiêu đề/Số hiệu/Nội dung của thông tinTham chiếu có file đính kèm (Ví dụ QĐ Khen thưởng thì đây là tiêu đề quyết định cần tìm)</param>
        /// <param name="pPhanLoai">Mã hiệu phan loại file đính kèm</param>
        /// <param name="pTenFile">Tên file lúc đính kèm (Tên nguyên bản)</param>
        /// <param name="pTenFileMoi">Tên file mới của file đính kèm theo quy ước</param>
        /// <param name="pMoTa">Mô tả file đính kèm</param>
        /// <param name="pTrangThai">Trạng thái bản ghi.Nếu truyền '0' lấy tất; Nếu truyền '1' lấy bản ghi đang mở</param>
        /// <returns>Danh sách thông tin file đính kèm</returns>
        public List<AttachedFileInfo> GetAttachFileSearch(int pFileId, long pDocumentId, string pTenFile, string pTenFileMoi, string pMoTa, int pTrangThai)
        {
            var answer = new List<AttachedFileInfo>();
            try
            {
                int iCount = 0;
                var profileAttachFileTMP = _dbContext.AttachedFileInfos.Where(w => w.FileId != 0 && (pFileId == 0 || w.FileId == pFileId)
                                            && (pDocumentId == -1 || w.DocumentId == pDocumentId)
                                            && (string.IsNullOrEmpty(pTenFile) || w.FileName == pTenFile)
                                            && (string.IsNullOrEmpty(pTenFileMoi) || w.FileNameNew == pTenFileMoi)
                                            && (string.IsNullOrEmpty(pMoTa) || w.DocumentNumber.Contains(pMoTa))
                                            && (w.CreatedDate >= DateTime.Now.AddDays(-1) && w.CreatedDate <= DateTime.Now.AddDays(1))
                                            && (pTrangThai == -1 || w.Status == pTrangThai)
                                        ).OrderBy(o => o.DocumentNumber).ThenBy(o => o.FileNameNew).ToList();
                List<AttachedFileInfo> profileAttachFiles = new List<AttachedFileInfo>();
                profileAttachFiles = profileAttachFileTMP;

                foreach (var item in profileAttachFiles)
                {
                    iCount++;
                    AttachedFileInfo objItem = new AttachedFileInfo();
                    objItem = _mapper.Map<AttachedFileInfo>(item);
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
        /// Hàm thực hiện gọi API Insert dữ liệu noti vào bảng VBSP_NOTIFICATION_DATA
        /// Gọi đến API ESB: http://10.63.54.52:8085/api/v1/insert-notification-data
        /// </summary>
        /// <exception cref="Exception"></exception>
        public async Task<NotificationDataResponse> InsertNotiData(UserManagementIDC pUserManagementUpd, string pUserNameUpd)
        {
            DateTime dCurrentDateTmp = DateTime.Now;
            NotificationDataResponse objNotiData = new NotificationDataResponse();
            try
            {
                var pMainPosName = _dbContext.ListOfPoss.Where(w => w.Code == pUserManagementUpd.PosCode).Select(s=>s.MainPosName).FirstOrDefault();
                var listGroupName = _serviceLOV.GetListOfValueSearch(ListOfValueParentValue.ParentId_UserRoleIDC, "", 0, "", "", -1, 2);
                if (pUserManagementUpd != null && !string.IsNullOrEmpty(pUserManagementUpd.UserId))
                {                  
                    var pGroupNameDetail = listGroupName.Where(w=>w.Code == pUserManagementUpd.GroupName).Select(s=>s.Name).FirstOrDefault();
                    objNotiData.notiType = pUserManagementUpd.FunctionType;
                    objNotiData.sourceId = "02";
                    objNotiData.businessDate = pUserManagementUpd.EffectiveDate;
                    objNotiData.posCode = pUserManagementUpd.PosCode;
                    objNotiData.posName = pUserManagementUpd.PosName;
                    objNotiData.customerId = pUserManagementUpd.StaffCode;
                    objNotiData.customerName = pUserManagementUpd.FullName;
                    objNotiData.mobileNo = pUserManagementUpd.MobileNumber;
                    objNotiData.email = pUserManagementUpd.EmailAddress;
                    objNotiData.d1 = pUserManagementUpd.FirstName + " " + pUserManagementUpd.LastName;
                    objNotiData.d2 = pMainPosName;
                    objNotiData.d3 = pUserManagementUpd.UserId;
                    objNotiData.d4 = pUserManagementUpd.GroupName + " - " + pGroupNameDetail;
                    objNotiData.d5 = pUserManagementUpd.PosName;
                    objNotiData.d6 = pUserManagementUpd.EffectiveDate?.ToString(FormatParameters.FORMAT_DATE_INT);
                    objNotiData.d7 = "OTT_TDCS";
                    objNotiData.d8 = pUserManagementUpd.PosCode + " - " + pUserManagementUpd.PosName;
                    objNotiData.status = "0";
                    objNotiData.errorCode = "00";
                    if (pUserManagementUpd.FunctionType == FunctionTypeFlag.FunctionTypeFlag_ADDNEW_USER.Code)
                    {
                        objNotiData.errorCode3 = "00";
                        objNotiData.sendType = "/3";
                        objNotiData.status3 = "1";
                        objNotiData.d9 = "3";
                    }
                    else
                    {
                        objNotiData.errorCode2 = "00";
                        objNotiData.sendType = "/1//2/";
                        objNotiData.status2 = "1";
                        objNotiData.d9 = "2";
                    }
                    objNotiData.createdBy = pUserNameUpd;
                    objNotiData.createdTime = dCurrentDateTmp;
                    var apiResponse = await _apiNotiGatewayService.InsertNotiDataList(new List<NotificationDataResponse> { objNotiData });
                    if (apiResponse != null && apiResponse.Code == "00")
                    {
                        var apiSendNoti = await _apiNotiGatewayService.GetNotiByTypeAsync(objNotiData.notiType, objNotiData.d9, objNotiData.d7);
                        if (string.IsNullOrEmpty(apiSendNoti))
                        {
                            throw new Exception("Không parse được response từ Noti API");
                        }
                        else
                        {
                            var result = JsonSerializer.Deserialize<UpdateNotiResult>(apiSendNoti,new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (result?.Code == "00")
                                Console.WriteLine("Thành công");
                            else
                                throw new Exception("Không parse được response từ Noti API");
                        }
                    }
                    else
                    {
                        throw new Exception("Không parse được response từ Noti API");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InsertNotiData('{pUserManagementUpd.UserId}', '{pUserNameUpd}') => Error: {ex.Message}");
                throw new Exception($"Lỗi khi insert dữ liệu vào Noti " +
                                        $"InsertNotiData('{objNotiData.d3}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
            }
            return objNotiData;
        }


    }
}
