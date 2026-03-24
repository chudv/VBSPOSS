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

                    /*
                      public string LastPWDChanged { get; set; }

        /// <summary>
        /// Không sử dụng, mặc định là 0
        /// </summary>
        public string PrimaryChoicebasedAuthType { get; set; }

        public string MobileNumber { get; set; }

        public string TranAuthType { get; set; }

        public string ReqNo { get; set; }

        public bool SelfRegistration { get; set; }

        public string FromRecord { get; set; }

        public string Language { get; set; }        //en_US | vi_VN

        public string UserCreatedDate { get; set; }     //Ngày tạo người dùng.Định dạng yyyy-MM-dd

        public string CorporateName { get; set; }       //Tên công ty dành cho người dùng là doanh nghiệp.Giá trị đang là '0'

        public string EmailAddress { get; set; }        //Địa chỉ email của người dùng

        public string AuthsecType { get; set; }         //Phương thức xác thực thứ 2. Giá trị mặc định '0'

        public string DOB { get; set; }                 //Ngày sinh của người dùng.Định dạng yyyy-MM-dd

        public string InvalidAttempt { get; set; }      //Số lần đăng nhập sai

        public bool UserFromService { get; set; }      //Không sử dụng, mặc định là false

        public string UserRole { get; set; }        //Nhóm quyền trên IDC(Không bao gồm Lending). Ex: POGD

        public string BranchCode { get; set; }      //Mã POS của người dùng. Ex: 101

        public string NickName { get; set; }      //Tên tài khoản người dùng cần lấy thông tin

        public string DefaultBranch { get; set; }      //Chỉ định chi nhánh mặc định cho người dùng sẽ được tạo. Ex: 'IDCPRODC'

        public int HpinFlag { get; set; }      //Có sử dụng hard PIN không, mặc định là 0

        public int ReqNumber { get; set; }      //Không sử dụng, mặc định là 0

        public int ToRecord { get; set; }      //Không sử dụng, mặc định là 0

        public bool AppendEntity { get; set; }      //Không sử dụng, mặc định là false

        public string FirstName { get; set; }

        public string GroupName { get; set; }               //Nhóm quyền của LMS/ FAMS, COLLATERAL (Trừ phần hệ Core). Ex: IDCROLE,GRPLMSIT,GRPCLMSIT

        public bool IsWebSealUser { get; set; }               //Mặc định là false

        public string EntityList { get; set; }               //Entity quản lý người dùng. Ex: UATVBSP hoặc Bank/IDCPRODC. Ex: 'IDCPRODC'

        public string UserIdentifierName { get; set; }               //Giá trị là mặc định 'All'. Tùy theo lựa chọn có thể là: All/Functional User/Administrator/Retail

        public int OperationType { get; set; }               //Giá trị là -1

        public int UserType { get; set; }               //Loại người dùng (IDL_ARX.TB_ARM_USER_TYPE@VBSPCBSLINK). Giá trị quy ước: 0: Bank; 1: Corporate; 2: Retail

        public bool EncryptExtraAttrib { get; set; }               //Giá trị mặc định false

        public string LastName { get; set; }

        public string UserIdentifierAlias { get; set; }         //Giá trị là 'All'

        public int UserStatus { get; set; }                     //Trạng thái người dùng. Giá trị: 1- Đóng/Khóa; 2 - Mở/Active

        public int SecondaryChoicebasedAuthType { get; set; }   //Giá trị là '0'

        public int PrevStatus { get; set; }             //Trạng thái trước đó của User. Giá trị là -7

        public bool AppendRole { get; set; }            //Giá trị là false

        public string LastLoginDate { get; set; }       //Lần cuối cùng login vào hệ thống (yyyyMMddHHmmss)

        public string ExpiryDate { get; set; }          //Ngày hết hiệu lực của người dùng, định dạng yyyy-MM-dd

        public string CheckerDate { get; set; }         //Ngày duyệt tạo người dùng, định dạng yyyy-MM-dd

        /// <summary>
        /// Cờ xác định cấp mật khẩu cho người dùng. Giá trị: 
        ///         '0': Mật khẩu mặc định là: 4 ký tự đầu của UserId và ngày sinh ddMMyyyy;
        ///         '1': Mật khẩu sinh ngẫu nhiên được gửi vào email của người dùng;
        ///         '2': Mật khẩu được gửi link vào email của người dùng
        ///         '4': Mật khẩu được sinh ngẫu nhiên và trả ra khi gọi API tạo người dùng
        /// Chú ý: Đối với các role có quyền tiền mặt gồm: POGD, POPGD, TKTTT, TKTTQ, TKTCB, CNGD, CNPGD, PKTTP, PKTPP, PKTTM, PKTTQ, SGDTQ, SGDTM, SGDPP, SGDTP, SGDPG, SGDGD, TTGD, TTKT, TTTQ, TTTKT, DTGD, DTKT, DTTQ, DTTKT, VPGD, VPKT, VPTQ thì bắt buộc Gía trị MailIdFlag = 4. Các role còn lại mặc định MailIdFlag = 0
        /// </summary>
        public string MailIdFlag { get; set; }

        /// <summary>
        /// Phương thức đăng nhập. Giá trị: -1: Super (Áp dụng cho user hệ thống không đăng nhập được);
        ///                                 1: Native (Bình thường Mật khẩu); 2: LDAP; 3: Safeword; 10: SMS OTP(Citi MFA)
        /// </summary>
        public int AuthType { get; set; }

        public int CredInfoEncryptType { get; set; }        //Giá trị là '0'

        public string MakerId { get; set; }                 //Người tạo tài khoản người dùng

        public int ReqActivity { get; set; }        //Giá trị là '0'

        public string MakerDate { get; set; }      //Ngày tạo tài khoản người dùng, định dạng yyyy-MM-dd

        public bool AppendEntityRoleMap { get; set; }       //Giá trị là false

        public string Salt { get; set; }                    //Giá trị là 'dummysalt'

        public string UserId { get; set; }                  //Tài khoản người dùng như trường nickName

        public string CheckerId { get; set; }               //Người duyệt tạo người dùng

        public string CurrLoginDate { get; set; }           //Ngày giờ login gần nhất (2026 03 24 031929)
                     */

                }
                //if(responseAPIViewUser.ResponseCode)
                var objUserInfoByAPI = responseAPIViewUser.Result[0];

                //  public async Task<GenericListRecordJava<ViewUserReposeViewModel>> GetUserIDCInfoByApiViewUser(ViewUserRequestViewModel requestInput)
                //var _lstProductList = _productService.GetAccountTypes("");
                //int _id = 1;
                //for (int i = 0; i < data.Count; i++)
                //{
                //    for (int j = 0; j < data[i].TermDetails.Count; j++)
                //    {
                //        TideTermViewModel item = new TideTermViewModel();
                //        item.Id = _id;
                //        item.TermProductCode = data[i].ProdCode;
                //        item.TermProductName = data[i].ProdName;
                //        item.TermAccountTypeCode = data[i].DepositType;
                //        var product = _lstProductList.FirstOrDefault(p => p.Value == data[i].DepositType);

                //        item.TermAccountTypeName = product == null ? "" : product.Text;
                //        item.TermAccountSubTypeCode = data[i].DepositSubType;
                //        item.TermCurrencyCode = data[i].Currency;
                //        item.TermEffectiveDate = string.IsNullOrEmpty(data[i].EffectDate) ? DateTime.Now : CustConverter.StringToDate(data[i].EffectDate);
                //        item.TermAmoutSlab = data[i].SlabRange == null ? 0 : Convert.ToDecimal(data[i].SlabRange);
                //        item.TermSerial = int.Parse(data[i].TermDetails[j].Serial);
                //        item.TermDesc = data[i].TermDetails[j].TermDesc;
                //        item.TermValue = int.Parse(data[i].TermDetails[j].TermValue);
                //        item.TermUnit = data[i].TermDetails[j].TermUnit;
                //        item.InclusionFlag = data[i].TermDetails[j].InclusionFlag;
                //        item.TermIntRate = data[i].TermDetails[j].IntRate == null ? 0 : Convert.ToDecimal(data[i].TermDetails[j].IntRate);
                //        item.TermIntRateNew = data[i].TermDetails[j].IntRate == null ? 0 : Convert.ToDecimal(data[i].TermDetails[j].IntRate);

                //        //Phan them bien do lai suat
                //        if (posCode == PosValue.HEAD_POS)
                //        {
                //            item.MinInterestRateSpread = 0;
                //            item.MaxInterestRateSpread = 0;
                //            item.MinTermIntRateNew = item.TermIntRate;
                //            item.MaxTermIntRateNew = item.TermIntRate;
                //        }
                //        else
                //        {
                //            var productParameter = _productService.GetProductParameter(ProductGroupCode.ProductGroupCode_Tide, data[i].ProdCode, null);
                //            item.MinInterestRateSpread = productParameter?.MinInterestRateSpread ?? 0;
                //            item.MaxInterestRateSpread = productParameter?.MaxInterestRateSpread ?? 0;
                //            item.MinTermIntRateNew = item.TermIntRate - (productParameter?.MinInterestRateSpread ?? 0);
                //            item.MaxTermIntRateNew = item.TermIntRate + (productParameter?.MaxInterestRateSpread ?? 0);
                //        }

                //        result.Add(item);
                //        _id++;
                //    }
                //}


                return objUserIDCInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy danh sách sản phẩm Tide: {ex.Message}", ex);
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
