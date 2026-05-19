using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using System.Data;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.Models.IDC;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class TransferDataPosService : ITransferDataPosService
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

        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public TransferDataPosService(ApplicationDbContext dbContext, IConfiguration config, IMapper mapper, IntellectIDCDbContext dbContextIDC, ILogger<ListOfTransPointService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dbContextIDC = dbContextIDC;
            _logger = logger;
            _config = config;
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
        public List<TransferDataPosMasterViewModel> GetListOfTranferDataPosSearch(string pPosCode, string statuspStatus, string pUserGrade)
        {
            var transferDataPosMasterViews = new List<TransferDataPosMasterViewModel>();

            try
            {
                int orderNo = 0;
                var query = _dbContext.TransferDataPosMasters.AsQueryable();

                if (pUserGrade == "3")
                {
                    query = query.Where(w => w.IsDeleted == false &&
                        (w.Status == 2 || w.Status == 3 || w.Status == 4 || w.Status == 1) &&
                        w.MainPos.StartsWith(pPosCode));
                }
                else if (pUserGrade == "2")
                {
                    query = query.Where(w => w.IsDeleted == false && w.MainPos.StartsWith(pPosCode));
                }
                else
                {
                    query = query.Where(w => w.IsDeleted == false);
                }

                var listOfTransferDataPosMaster = query
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();

                transferDataPosMasterViews = listOfTransferDataPosMaster
                    .Select(x => new TransferDataPosMasterViewModel
                    {
                        OrderNo = ++orderNo,
                        Id = x.Id,
                        FromPosCode = x.FromPosCode,
                        FromPosName = GetPosName(x.FromPosCode),
                        ToPosCode = x.ToPosCode,
                        ToPosName = GetPosName(x.ToPosCode),
                        EffectiveDate = x.EffectiveDate,
                        Remark = x.Remark,
                        Status = x.Status,
                        TotalVillage = x.TotalVillage,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedDate = x.ModifiedDate,
                        ApproverBy = x.ApproverBy,
                        ApprovalDate = x.ApprovalDate,
                        RejectReason = x.RejectReason,
                        IsDeleted = x.IsDeleted
                    })
                    .ToList();

                return transferDataPosMasterViews;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetPosName(string posCode)
        {
            try
            {
                if (string.IsNullOrEmpty(posCode)) return string.Empty;

                var answer = _dbContext.ListOfPoss.FirstOrDefault(w => w.Code == posCode);
                if (answer == null) return string.Empty;

                return answer.Name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        public List<ValueConstModel> GetListPosOfBranch(string mainPos)
        {
            try
            {
                if (mainPos == "000000")
                {
                    var answer = _dbContext.ListOfPoss
                        .Where(w => w.Status == "O" && !new[] { "000100", "000196", "000197", "000199", "000101" }.Contains(w.Code))
                        .OrderBy(x => x.Code)
                        .Select(x => new ValueConstModel
                        {
                            Code = x.Code,
                            Description = x.MainPosCode.Substring(2, 2) + " - " + x.MainPosName.Replace("Chi nhánh ", "CN ") + " -> " + x.Code + " - " + x.Name
                        })
                        .ToList();

                    return answer;
                }
                else
                {
                    var answer = _dbContext.ListOfPoss
                        .Where(w => w.MainPosCode == mainPos && w.Status == "O" && !new[] { "000100", "000196", "000197", "000199", "000101" }.Contains(w.Code))
                        .OrderBy(x => x.Code)
                        .Select(x => new ValueConstModel
                        {
                            Code = x.Code,
                            Description = x.Code + " - " + x.Name
                        })
                        .ToList();

                    return answer;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<long> SaveTranferPosMaster(TransferDataPosMasterViewModel tranferMaster, string pUserNameUpd, string pFlagCall, string pButtonType, string mainPos)
        {
            int iSaveChanges = 0;
            long iRetIdUpd = 0;
            DateTime dCurrentDateTmp = DateTime.Now;

            try
            {
                if (tranferMaster == null) return 0;

                // INSERT
                if (tranferMaster.Id <= 0)
                {
                    var entity = new TransferDataPosMaster
                    {
                        MainPos = mainPos,
                        FromPosCode = tranferMaster.FromPosCode,
                        ToPosCode = tranferMaster.ToPosCode,
                        EffectiveDate = tranferMaster.EffectiveDate,
                        Remark = tranferMaster.Remark,
                        Status = TranferDataPosStatus.INIT.Value,
                        CreatedBy = pUserNameUpd,
                        CreatedDate = dCurrentDateTmp,
                        ModifiedBy = pUserNameUpd,
                        ModifiedDate = dCurrentDateTmp
                    };

                    await _dbContext.TransferDataPosMasters.AddAsync(entity);
                    iSaveChanges = await _dbContext.SaveChangesAsync();
                    if (iSaveChanges > 0) iRetIdUpd = entity.Id;
                }
                // UPDATE
                else
                {
                    var entityUpd = await _dbContext.TransferDataPosMasters.FirstOrDefaultAsync(x => x.Id == tranferMaster.Id);
                    if (entityUpd == null) return 0;

                    entityUpd.FromPosCode = tranferMaster.FromPosCode;
                    entityUpd.ToPosCode = tranferMaster.ToPosCode;
                    entityUpd.EffectiveDate = tranferMaster.EffectiveDate;
                    entityUpd.Remark = tranferMaster.Remark;
                    entityUpd.ModifiedBy = pUserNameUpd;
                    entityUpd.ModifiedDate = dCurrentDateTmp;

                    _dbContext.TransferDataPosMasters.Update(entityUpd);
                    iSaveChanges = await _dbContext.SaveChangesAsync();
                    if (iSaveChanges > 0) iRetIdUpd = entityUpd.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iRetIdUpd;
        }


        public List<ValueConstModel> GetListSubCommuneOfPos(string posCd)
        {
            try
            {
                var answer = _dbContext.ListOfCommunes
                .Where(w => w.PosCode == posCd && w.RecordStatus == "A" && w.SubCommuneCode != "00000000")
                .OrderBy(x => x.SubCommuneCode)
                .Select(x => new ValueConstModel
                {
                    Code = x.SubCommuneCode,
                    Description = x.CommuneCode + " - " + x.CommuneName + " -> " + x.SubCommuneCode + " - " + x.SubCommuneName
                })
                .ToList();

                return answer;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CheckExistTransferDataPosMaster(string fromPosCode, string toPosCode, DateTime? effectiveDate)
        {
            try
            {
                if (string.IsNullOrEmpty(fromPosCode) || string.IsNullOrEmpty(toPosCode) || effectiveDate == null) return false;

                bool isExist = _dbContext.TransferDataPosMasters.Any(x =>
                    x.FromPosCode == fromPosCode &&
                    x.ToPosCode == toPosCode &&
                    x.EffectiveDate.HasValue &&
                    x.EffectiveDate.Value.Date == effectiveDate.Value.Date);

                return isExist;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<long> SaveAttachedFile(long documentId, IFormFile file, string documentNumber, string userName)
        {
            long fileId = 0;
            try
            {
                if (file == null || file.Length <= 0) return 0;

                // ROOT FOLDER
                string uploadFolder = _config["AttachedFileSettings:FolderUpload"];
                string rootFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadFolder);

                // SUB FOLDER
                string subFolder = "TranferDataPos";
                string fullFolder = Path.Combine(rootFolder, subFolder);

                if (!Directory.Exists(fullFolder)) Directory.CreateDirectory(fullFolder);

                // FILE INFO
                string fileExtension = Path.GetExtension(file.FileName);
                string fileNameOrigin = Path.GetFileName(file.FileName);
                string fileNameNew = Guid.NewGuid().ToString("N") + fileExtension;
                string fullPath = Path.Combine(fullFolder, fileNameNew);

                // SAVE FILE PHYSICAL
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // SAVE DB
                var entity = new AttachedFileInfo
                {
                    DocumentId = documentId,
                    FileType = "8",
                    FileName = fileNameOrigin,
                    FileExtension = fileExtension,
                    PathFile = Path.Combine(uploadFolder, subFolder),
                    FileNameNew = fileNameNew,
                    DocumentNumber = documentNumber,
                    Status = 1,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now
                };

                await _dbContext.AttachedFileInfos.AddAsync(entity);
                await _dbContext.SaveChangesAsync();

                fileId = entity.FileId;
            }
            catch (Exception)
            {
                throw;
            }
            //return fileId;
            return fileId > 0 ? 0 : 1;
        }


        public async Task<AttachedFileInfo> DownloadTransferAttachFile(long documentId)
        {
            try
            {
                return await _dbContext.AttachedFileInfos
                    .Where(x => x.DocumentId == documentId && x.FileType == "8" && x.Status == 1)
                    .OrderByDescending(x => x.FileId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> DeleteTransferDataPos(long pId, string userName)
        {
            try
            {
                var entity = await _dbContext.TransferDataPosMasters.FirstOrDefaultAsync(x => x.Id == pId);
                if (entity == null) return false;

                entity.IsDeleted = true;
                entity.ModifiedBy = userName;
                entity.ModifiedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public TransferDataPosMaster GetTransferDataPosMasterById(long pId)
        {
            try
            {
                return _dbContext.TransferDataPosMasters
                    .FirstOrDefault(x => x.Id == pId && x.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> ApproveTransferDataPos(long pId, string pRemark, string pAction, string pUserName)
        {
            try
            {
                var entity = await _dbContext.TransferDataPosMasters.FirstOrDefaultAsync(x => x.Id == pId);
                if (entity == null) return 0;

                entity.Status = int.Parse(pAction);
                entity.Remark = pRemark;
                entity.ModifiedBy = pUserName;
                entity.ModifiedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveTransferDataPos");
                return 0;
            }
        }


        public List<TransferDataPosDetailViewModel> GetTransferDataPosDetailByMasterId(long pId)
        {
            try
            {
                var list = _dbContext.TransferDataPosDetails
                    .Where(x => x.MasterId == pId)
                    .OrderBy(x => x.Id)
                    .ToList();

                return list.Select(x => new TransferDataPosDetailViewModel
                {
                    Id = x.Id,
                    MasterId = x.MasterId,
                    FromVillageId = x.FromVillageId,
                    ToVillageId = x.ToVillageId,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTransferDataPosDetailByMasterId");
                return new List<TransferDataPosDetailViewModel>();
            }
        }


        public async Task<long> SaveTransferDataPosMaster(TransferDataPosMasterViewModel model, string userName, int totalVillage, string mainPos)
        {
            try
            {
                TransferDataPosMaster entity;

                // CREATE
                if (model.Id <= 0)
                {
                    entity = new TransferDataPosMaster
                    {
                        FromPosCode = model.FromPosCode,
                        ToPosCode = model.ToPosCode,
                        EffectiveDate = model.EffectiveDate,
                        Remark = model.Remark,
                        Status = 1,
                        TotalVillage = totalVillage,
                        MainPos = mainPos,
                        IsDeleted = false,
                        CreatedBy = userName,
                        CreatedDate = DateTime.Now
                    };

                    _dbContext.TransferDataPosMasters.Add(entity);
                    await _dbContext.SaveChangesAsync();
                }
                // UPDATE
                else
                {
                    entity = await _dbContext.TransferDataPosMasters.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (entity == null) return 0;

                    entity.FromPosCode = model.FromPosCode;
                    entity.ToPosCode = model.ToPosCode;
                    entity.EffectiveDate = model.EffectiveDate;
                    entity.Remark = model.Remark;
                    entity.ModifiedBy = userName;
                    entity.ModifiedDate = DateTime.Now;
                    entity.TotalVillage = totalVillage;

                    await _dbContext.SaveChangesAsync();
                }

                return entity.Id;
            }
            catch
            {
                return 0;
            }
        }


        public async Task<int> SaveTransferDataPosDetail(long masterId, List<TransferDataPosDetailViewModel> details, string userName)
        {
            try
            {
                // DELETE OLD
                var oldData = await _dbContext.TransferDataPosDetails
                    .Where(x => x.MasterId == masterId)
                    .ToListAsync();

                if (oldData.Any())
                {
                    _dbContext.TransferDataPosDetails.RemoveRange(oldData);
                }


                // INSERT NEW
                foreach (var item in details)
                {
                    var entity = new TransferDataPosDetail
                    {
                        MasterId = masterId,
                        FromVillageId = item.FromVillageId,
                        ToVillageId = item.ToVillageId,
                        // IsDeleted = false,
                        CreatedBy = userName,
                        CreatedDate = DateTime.Now
                    };

                    _dbContext.TransferDataPosDetails.Add(entity);
                }

                await _dbContext.SaveChangesAsync();
                return 1;
            }
            catch
            {
                return 0;
            }
        }


        public string GetVillageNameByCode(string villageCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(villageCode)) return "";

                var answer = _dbContext.ListOfCommunes
                    .Where(w => w.SubCommuneCode == villageCode && w.RecordStatus == "A")
                    .Select(x => x.CommuneCode + " - " + x.CommuneName + " -> " + x.SubCommuneCode + " - " + x.SubCommuneName)
                    .FirstOrDefault();

                return answer ?? "";
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<CommuneMaintenanceOracleView>> GetTransferDataForOracleAsync(long transferId)
        {
            try
            {
                if (transferId <= 0) return new List<CommuneMaintenanceOracleView>();

                var data = await _dbContext.CommuneMaintenanceOracleViews
                    .Where(x => x.JOB_ID == transferId)
                    .ToListAsync();

                if (data == null) return new List<CommuneMaintenanceOracleView>();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTransferDataForOracleAsync");
                return new List<CommuneMaintenanceOracleView>();
            }
        }


        public async Task<int> BulkInsertCommuneTransferAsync(List<CommuneMaintenanceOracleView> data)
        {
            try
            {
                if (data == null || data.Count <= 0) return 0;

                var arr = data.Select(x => new CommuneTransferOracleObject
                {
                    PROV_CD = x.PROV_CD,
                    DIST_CD = x.DIST_CD,
                    COMM_CD = x.COMM_CD,
                    SCOM_CD = x.SCOM_CD,
                    COMM_NAME = x.COMM_NAME,
                    TXN_ID = x.TXN_ID,
                    VISIT_DT = x.VISIT_DT,
                    POS_CD = x.POS_CD,
                    NEW_COMMUNE = x.NEW_COMMUNE,
                    FEE_LEN_AREA = x.FEE_LEN_AREA,
                    COM_LEN_AREA = x.COM_LEN_AREA,
                    CASA_AREA = x.CASA_AREA,
                    TYPE_CD = x.TYPE_CD,
                    JOB_ID = x.JOB_ID,
                    NEW_POS_CD = x.NEW_POS_CD,
                    DIFF_AREA = x.DIFF_AREA
                }).ToArray();

                using (var conn = new OracleConnection(_dbContextIDC.Database.GetConnectionString()))
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    using (var cmd = new OracleCommand("VBSPOSS.VBSP_OSS_UPD.SP_BULK_COMMUNE_TRANSFER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var param = new OracleParameter
                        {
                            ParameterName = "p_DATA",
                            OracleDbType = OracleDbType.Array,
                            UdtTypeName = "VBSPOSS.TAB_COMMUNE_TRANSFER",
                            Value = (Array)arr
                        };
                        cmd.Parameters.Add(param);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInsertCommuneTransferAsync");
                return 0;
            }
        }


        public List<ChangePosDataCheckingViewModel> GetListChangePosDataChecking(string pPosCode, string pTotrinh)
        {
            try
            {
                var query = _dbContextIDC.ChangePosDataCheckings.AsQueryable();

                if (!string.IsNullOrWhiteSpace(pPosCode))
                    query = query.Where(x => x.NEW_POS_CD == pPosCode);

                return query.Select(x => new ChangePosDataCheckingViewModel
                {
                    TYPE_CD = x.TYPE_CD,
                    POS_CD = x.POS_CD,
                    SCOM_CD = x.SCOM_CD,
                    CUST_ID = x.CUST_ID,
                    CUST_NAME = x.CUST_NAME,
                    AC_NO = x.AC_NO,
                    PROD_CD = x.PROD_CD,
                    PRIN_AMOUNT = x.PRIN_AMOUNT,
                    INT_AMOUNT = x.INT_AMOUNT,
                    GROUP_ID = x.GROUP_ID,
                    GROUP_NAME = x.GROUP_NAME,
                    NEW_POS_CD = x.NEW_POS_CD,
                    NEW_SCOM_CD = x.NEW_SCOM_CD,
                    MOV_STATUS = x.MOV_STATUS,
                    ACCEPT_MOV = x.ACCEPT_MOV == "1"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ChangePosDataChecking list");
                throw;
            }
        }


        public int SaveAcceptMove(List<ChangePosDataCheckingViewModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var acceptMov = item.ACCEPT_MOV ? "1" : "0";

                    _dbContextIDC.Database.ExecuteSqlRaw(
                        "UPDATE IDL_LMS.CHANGE_POS_DATA_CHECKING " +
                        "SET ACCEPT_MOV = {0} " +
                        "WHERE CUST_ID = {1} AND AC_NO = {2} AND PROD_CD = {3}",
                        acceptMov,
                        item.CUST_ID,
                        item.AC_NO,
                        item.PROD_CD
                    );
                }

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AcceptMove");
                return 0;
            }
        }

        public ArrayList GetToTrinh(string pUserPosCode)
        {
            try
            {
                ArrayList data = new ArrayList();

                var listData = _dbContext.TransferDataPosMasters
                    .Where(x => x.ToPosCode == pUserPosCode)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

                foreach (var item in listData)
                {
                    data.Add(new
                    {
                        Value = item.Id,
                        Text = "Tờ trình id: " + item.Id
                             + " | POS nguồn: " + item.FromPosCode
                             + " | POS đích: " + item.ToPosCode
                             + " | Số thôn điều chuyển: " + item.TotalVillage
                             + " | Ngày hiệu lực: "
                             + (item.EffectiveDate.HasValue
                                 ? item.EffectiveDate.Value.ToString("dd/MM/yyyy")
                                 : "")
                    });
                }

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}