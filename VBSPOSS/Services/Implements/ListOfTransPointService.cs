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
        /// <param name="pPosCode">Mã Pos (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã (Không bắt buộc)</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pVisitDateBegin">Ngày giao dịch cố định bắt đầu (Không bắt buộc)</param>
        /// <param name="pVisitDateEnd">Ngày giao dịch cố định kết thúc (Không bắt buộc)</param>
        /// <param name="pTxnStatus">Trạng thái danh mục (Không bắt buộc). Nếu rỗng lấy tất; Nếu truyền A lấy danh mục mở</param>
        /// <returns>Danh sách bản ghi điểm giao dịch theo Model ListOfTransPointViewModel</returns>
        public List<ListOfTransPointViewModel> GetListOfTransPointSearch(string pProvinceCode, string pPosCode, string pCommuneCode, string pTxnPointCode, string pTxnPointName,
                                            int pVisitDateBegin, int pVisitDateEnd, string pTxnStatus)
        {
            var answer = new List<ListOfTransPointViewModel>();
            if (pVisitDateBegin <= 0 || pVisitDateBegin > 31)
                pVisitDateBegin = 0;
            if (pVisitDateEnd <= 0 || pVisitDateEnd > 31)
                pVisitDateEnd = 31;
            try
            {
                int iCountTMP = 0;
                var listOfTransPointTmp = _dbContext.ListOfTransPoints.Where(w => (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                                            && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
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
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.PosCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.TxnPointCode).ThenBy(o => o.EffectiveDate).ToList();

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
                
                var objTranspoint = _dbContext.ListOfTransPoints.Where(m => m.TxnPointCode == model.TxnPointCode).FirstOrDefault();
                DateTime dCurrentDateVal = DateTime.Now;
                if (objTranspoint != null)
                {
                    //objTranspoint.ProvinceCode = model.ProvinceCode;
                    objTranspoint.ProvinceName = model.ProvinceName;
                    //objTranspoint.PosCode = model.PosCode;
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
                    //objTranspoint.CreatedBy = model.CreatedBy;
                    //objTranspoint.CreatedDate = dCurrentDateVal;
                    objTranspoint.ModifiedBy = pUserName;
                    objTranspoint.ModifiedDate = dCurrentDateVal;
                    //objTranspoint.ApproverBy = model.ApproverBy;
                    //objTranspoint.ApprovalDate = dCurrentDateVal;
                    _dbContext.Entry(objTranspoint).State = EntityState.Modified;
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = iSaveChanges;
                }
                else
                {
                    ListOfTransPoint objModelTranspoint = new ListOfTransPoint();
                    objModelTranspoint = _mapper.Map<ListOfTransPoint>(model);

                    //objModelTranspoint.ProvinceCode = model.ProvinceCode;
                    //objModelTranspoint.ProvinceName = model.ProvinceName;
                    //objModelTranspoint.PosCode = model.PosCode;
                    //objModelTranspoint.PosName = model.PosName;
                    //objModelTranspoint.DistrictCode = model.DistrictCode;
                    //objModelTranspoint.DistrictName = model.DistrictName;
                    //objModelTranspoint.CommuneCode = model.CommuneCode;
                    //objModelTranspoint.CommuneName = model.CommuneName;
                    //objModelTranspoint.TxnPointCode = model.TxnPointCode;
                    //objModelTranspoint.TxnPointName = model.TxnPointName;
                    //objModelTranspoint.VisitDate = model.VisitDate;
                    //objModelTranspoint.Times = model.Times;
                    //objModelTranspoint.TimeBegin = model.TimeBegin;
                    //objModelTranspoint.TimeEnd = model.TimeEnd;
                    //objModelTranspoint.TimeBeginNum = model.TimeBeginNum;
                    //objModelTranspoint.TimeEndNum = model.TimeEndNum;
                    //objModelTranspoint.Hours = model.Hours;
                    //objModelTranspoint.Minutes = model.Minutes;
                    //objModelTranspoint.Longitude = model.Longitude;
                    //objModelTranspoint.Latitude = model.Latitude;
                    //objModelTranspoint.IsInCommune = model.IsInCommune;
                    //objModelTranspoint.IsInPos = model.IsInPos;
                    //objModelTranspoint.IsInterWard = model.IsInterWard;
                    //objModelTranspoint.InterWardName = model.InterWardName;
                    //objModelTranspoint.EffectiveDate = model.EffectiveDate;
                    //objModelTranspoint.TxnLocation = model.TxnLocation;
                    //objModelTranspoint.AddressDetail = model.AddressDetail;
                    //objModelTranspoint.AddressCode = model.AddressCode;
                    //objModelTranspoint.AddressFull = model.AddressFull;
                    //objModelTranspoint.PhoneSupport = model.PhoneSupport;
                    //objModelTranspoint.PhoneSupport01 = model.PhoneSupport01;
                    //objModelTranspoint.PhoneSupport02 = model.PhoneSupport02;
                    //objModelTranspoint.TxnStatus = model.TxnStatus;
                    //objModelTranspoint.Status = model.Status;
                    //objModelTranspoint.Remark = model.Remark;
                    objModelTranspoint.CreatedBy = pUserName;
                    objModelTranspoint.CreatedDate = dCurrentDateVal;
                    objModelTranspoint.ModifiedBy = pUserName;
                    objModelTranspoint.ModifiedDate = dCurrentDateVal;
                    objModelTranspoint.ApproverBy = model.ApproverBy;
                    objModelTranspoint.ApprovalDate = dCurrentDateVal;

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
